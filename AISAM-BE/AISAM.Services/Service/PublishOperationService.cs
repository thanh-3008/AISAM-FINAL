using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Service;

public sealed class PublishOperationService(AisamContext db,IAccessControlService access,IContentService contentService,Microsoft.Extensions.Options.IOptions<AISAM.Common.Models.InstagramSettings> settings,PublishProgressContext? progress=null)
{
    public Task<IReadOnlyList<PublishOperation>> StartScheduledAsync(ContentCalendar schedule, CancellationToken ct)
    {
        if (schedule.ScheduledByUserId is not Guid actor || actor == Guid.Empty ||
            schedule.SnapshotId is not Guid snapshot || schedule.IntegrationId is not Guid integration)
            throw new ResourceMutationDeniedException();
        return StartCoreAsync(actor, schedule.WorkspaceId, schedule.ContentId, Guid.Empty,
            [integration], $"schedule:{schedule.Id}", ct, snapshot);
    }

    public async Task<IReadOnlyList<PublishOperation>> StartAsync(Guid actor,Guid workspace,Guid contentId,Guid version,
        IReadOnlyList<Guid> integrations,string key,CancellationToken ct)
        => await StartCoreAsync(actor, workspace, contentId, version, integrations, key, ct);

    private async Task<IReadOnlyList<PublishOperation>> StartCoreAsync(Guid actor,Guid workspace,Guid contentId,Guid version,
        IReadOnlyList<Guid> integrations,string key,CancellationToken ct,Guid? scheduledSnapshot=null)
    {
        if(string.IsNullOrWhiteSpace(key)||key.Length>128||integrations is null||integrations.Count is <1 or >10||integrations.Distinct().Count()!=integrations.Count)
            throw new ArgumentException("Provide a key and 1-10 distinct destinations.");
        try
        {
            foreach(var id in integrations)
                if(!(await access.CheckAsync(new(actor,workspace,AccessResourceKind.Content,contentId,ResourcePermission.PostPublish,id),ct)).Allowed)
                    throw new ResourceMutationDeniedException();
        }
        catch (Npgsql.NpgsqlException ex) when (ex.IsTransient)
        { throw new PublishPreparationUnavailableException(ex); }
        catch (TimeoutException ex)
        { throw new PublishPreparationUnavailableException(ex); }
        var existing=await db.PublishOperations.IgnoreQueryFilters().Include(o=>o.Snapshot).Where(o=>o.WorkspaceId==workspace&&o.ActorId==actor&&o.IdempotencyKey==key).ToListAsync(ct);
        if(existing.Count>0)
        {
            if(existing.Any(o=>o.ContentId!=contentId||(scheduledSnapshot.HasValue?o.SnapshotId!=scheduledSnapshot:o.Snapshot.Version!=version))||!existing.Select(o=>o.IntegrationId).Order().SequenceEqual(integrations.Order()))throw new MediaConflictException();
            // Never replay an operation whose external outcome may already exist.
            return existing;
        }
        var content=await db.Contents.SingleAsync(c=>c.Id==contentId,ct);
        if(!scheduledSnapshot.HasValue && (content.MediaVersion!=version||content.ApprovedSnapshotId is null))throw new MediaConflictException();
        var snapshot=await db.PublishSnapshots.Include(s=>s.Media).SingleAsync(s=>s.Id==(scheduledSnapshot ?? content.ApprovedSnapshotId),ct);
        if(snapshot.ContentId!=contentId || snapshot.WorkspaceId!=workspace || content.WorkspaceId!=workspace)throw new ResourceMutationDeniedException();
        var operations=new List<PublishOperation>();
        foreach(var id in integrations)
        {
            var integration=await db.SocialIntegrations.IgnoreQueryFilters().Include(i=>i.SocialAccount).SingleAsync(i=>i.Id==id,ct);
            var error=PublishingCapabilities.Validate(PublishingCapabilities.For(integration,integration.SocialAccount,DateTime.UtcNow,settings.Value.VerifiedCarouselIntegrationIds.Contains(id)),snapshot.Media);
            operations.Add(new(){WorkspaceId=workspace,ActorId=actor,ContentId=contentId,SnapshotId=snapshot.Id,IntegrationId=id,IdempotencyKey=key,
                Status=error is null?"Queued":error=="SOCIAL_REAUTH_REQUIRED"?"NeedsAttention":"Failed",ErrorCode=error});
        }
        db.PublishRequests.Add(new(){WorkspaceId=workspace,ActorId=actor,IdempotencyKey=key});
        db.PublishOperations.AddRange(operations);await db.SaveChangesAsync(ct);
        foreach(var operation in operations.Where(o=>o.Status=="Queued"))
        {
            if(ct.IsCancellationRequested)break;
            if(db.Entry(operation).State==EntityState.Detached)db.Attach(operation);
            operation.Status="UploadingMedia";operation.Attempts++;operation.UpdatedAt=DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            // Provider reports Publishing immediately before committing the post.
            var previousActor=db.ExecutionActorId;var previousSnapshot=db.ExecutionSnapshotId;
            var previousProgress=progress?.Report;
            try
            {
                if(progress is not null)progress.Report=async(status,media,token)=>
                {
                    if(status is not ("UploadingMedia" or "Publishing"))throw new InvalidOperationException("Invalid provider stage.");
                    if(status=="Publishing"&&!(await access.CheckAsync(new(actor,workspace,AccessResourceKind.Content,contentId,ResourcePermission.PostPublish,operation.IntegrationId),token)).Allowed)
                        throw new ResourceMutationDeniedException();
                    operation.Status=status;
                    operation.MediaResults=System.Text.Json.JsonSerializer.Serialize(media);
                    operation.UpdatedAt=DateTime.UtcNow;
                    await db.SaveChangesAsync(token);
                };
                db.ExecutionActorId=actor;db.ExecutionSnapshotId=snapshot.Id;
                var response=await contentService.PublishScheduledAsync(contentId,operation.IntegrationId,content.ProfileId,workspace,ct);
                if(db.Entry(operation).State==EntityState.Detached)db.Attach(operation);
                operation.ProviderId=response.Data?.ProviderPostId;
                if(response.Data?.Media is {Count:>0} media)operation.MediaResults=System.Text.Json.JsonSerializer.Serialize(media);
                var confirmed=response.Success&&response.Data?.Success==true&&!string.IsNullOrWhiteSpace(response.Data.ProviderPostId)&&!response.Data.RequiresReconciliation;
                operation.Status=response.Success?(confirmed?"Published":"NeedsAttention"):
                    response.StatusCode is 401 or 403 or 409?"NeedsAttention":"Failed";
                operation.ErrorCode=response.Success?(confirmed?null:"PUBLISH_OUTCOME_PENDING"):
                    response.Error?.ErrorCode??"PUBLISH_PROVIDER_REJECTED";
            }
            catch(Exception)
            {
                // Includes cancellation/timeout: sending twice is unsafe without reconciliation.
                db.ChangeTracker.Clear();db.Attach(operation);
                operation.Status="NeedsAttention";operation.ErrorCode="PUBLISH_OUTCOME_UNKNOWN";
            }
            finally{db.ExecutionActorId=previousActor;db.ExecutionSnapshotId=previousSnapshot;if(progress is not null)progress.Report=previousProgress;}
            operation.UpdatedAt=DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
            if(ct.IsCancellationRequested)break;
        }
        return operations;
    }
}

// Only raised before creating/claiming an operation or invoking a provider.
public sealed class PublishPreparationUnavailableException(Exception inner)
    : Exception("PUBLISH_RETRY_SAFE", inner);
