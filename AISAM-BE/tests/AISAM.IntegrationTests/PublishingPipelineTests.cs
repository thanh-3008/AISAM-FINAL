using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace AISAM.IntegrationTests;
public class PublishingPipelineTests
{
    [Fact]
    public async Task ScheduledOperationUsesFrozenSnapshotAndNeverReplaysAfterTimeout()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var actor = Guid.NewGuid();
        var content = new Content { WorkspaceId = Guid.NewGuid(), TextContent = "approved original" };
        db.Add(content); await db.SaveChangesAsync();
        content.Status = ContentStatusEnum.Approved; await db.SaveChangesAsync();
        var snapshot = content.ApprovedSnapshotId;
        var integration = new SocialIntegration { WorkspaceId = content.WorkspaceId, Platform = SocialPlatformEnum.Facebook, AccessToken = "test", SocialAccount = new() { UserAccessToken = "test", IsActive = true } };
        db.Add(integration); await db.SaveChangesAsync();
        var schedule = new ContentCalendar { ContentId = content.Id, WorkspaceId = content.WorkspaceId, ScheduledByUserId = actor, IntegrationId = integration.Id, SnapshotId = snapshot };
        content.TextContent = "later draft"; content.Status = ContentStatusEnum.Draft; await db.SaveChangesAsync();
        var proxy = DispatchProxy.Create<IContentService, ContentProxy>();
        var fake = (ContentProxy)(object)proxy;
        fake.Timeout = true;
        fake.BeforeCommit = () => { Assert.Equal(actor, db.ExecutionActorId); Assert.Equal(snapshot, db.ExecutionSnapshotId); return Task.CompletedTask; };
        var access = new Access { Timeout = true };
        var service = new PublishOperationService(db, access, proxy, Options.Create(new InstagramSettings()));
        await Assert.ThrowsAsync<PublishPreparationUnavailableException>(() => service.StartScheduledAsync(schedule, default));
        Assert.Equal(0, fake.Calls);
        Assert.Empty(await db.PublishRequests.ToListAsync());
        access.Timeout = false;
        var first = (await service.StartScheduledAsync(schedule, default)).Single();
        Assert.Equal(snapshot, first.SnapshotId);
        Assert.Equal("NeedsAttention", first.Status);
        var second = (await service.StartScheduledAsync(schedule, default)).Single();
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, fake.Calls);
        Assert.Null(db.ExecutionActorId); Assert.Null(db.ExecutionSnapshotId);
    }

    private sealed class Access:IAccessControlService
    {
        public bool Denied;
        public bool Timeout;
        public Task<AccessDecision> CheckAsync(AccessRequest r,CancellationToken ct=default)=>Timeout?throw new TimeoutException():Task.FromResult(Denied?new AccessDecision(false,403,"ACCESS_DENIED_CHANNEL"):AccessDecision.Permit);
        public Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid a,Guid w,CancellationToken ct=default)=>throw new NotSupportedException();
    }
    public class ContentProxy:DispatchProxy
    {
        public int Calls;
        public bool Timeout;
        public Func<Task>? BeforeCommit;
        public int Commits;
        protected override object? Invoke(MethodInfo? method,object?[]? args)
        {
            if(method?.Name!="PublishScheduledAsync")throw new NotSupportedException();
            Calls++;
            return CompleteAsync();
        }
        private async Task<GenericResponse<PublishResultDto>> CompleteAsync()
        {
            if(BeforeCommit is not null)await BeforeCommit();
            if(Timeout)throw new TimeoutException("private provider detail");
            Commits++;return GenericResponse<PublishResultDto>.CreateSuccess(new(){Success=true,ProviderPostId="published"});
        }
    }
    [Theory]
    [InlineData(SocialPlatformEnum.Facebook,false)]
    [InlineData(SocialPlatformEnum.Instagram,true)]
    [InlineData(SocialPlatformEnum.TikTok,false)]
    public void MixedMediaRequiresVerifiedInstagram(SocialPlatformEnum platform,bool allowed)
    {
        var integration=new SocialIntegration{Platform=platform,AccessToken="test"};var account=new SocialAccount{UserAccessToken="test",IsActive=true};
        SnapshotMedia[] media=[new(){MimeType="image/jpeg",Url="https://cdn.test/1"},new(){MimeType="video/mp4",Url="https://cdn.test/2",DurationSeconds=30}];
        Assert.Equal(allowed,PublishingCapabilities.Validate(PublishingCapabilities.For(integration,account,DateTime.UtcNow,true),media) is null);
        Assert.NotNull(PublishingCapabilities.Validate(PublishingCapabilities.For(integration,account,DateTime.UtcNow),media));
    }
    [Fact]
    public void VideoRequiresTrustedDurationAndUsesConservativeLimit()
    {
        var capability=PublishingCapabilities.For(new(){Platform=SocialPlatformEnum.Instagram,AccessToken="test"},new(){UserAccessToken="test",IsActive=true},DateTime.UtcNow);
        var media=new SnapshotMedia{MimeType="video/mp4",Url="https://cdn.test/video"};
        Assert.Equal("MEDIA_METADATA_REQUIRED",PublishingCapabilities.Validate(capability,[media]));
        media.DurationSeconds=61;Assert.Equal("MEDIA_LIMIT_EXCEEDED",PublishingCapabilities.Validate(capability,[media]));
        media.DurationSeconds=30;Assert.Null(PublishingCapabilities.Validate(capability,[media]));
        media.MimeType="video/webm";Assert.Equal("MEDIA_TYPE_UNSUPPORTED",PublishingCapabilities.Validate(capability,[media]));
    }
    [Fact]
    public void ReconnectAndLimitsAreExplicitAndTokensNeverSerialized()
    {
        var integration=new SocialIntegration{Platform=SocialPlatformEnum.Instagram,AccessToken="secret",ExpiresAt=DateTime.UtcNow.AddMinutes(-1)};
        var account=new SocialAccount{UserAccessToken="secret",IsActive=true};
        var capability=PublishingCapabilities.For(integration,account,DateTime.UtcNow);
        Assert.False(capability.Available);Assert.Equal("SOCIAL_REAUTH_REQUIRED",capability.UnavailableReason);
        integration.ExpiresAt=null;capability=PublishingCapabilities.For(integration,account,DateTime.UtcNow);
        Assert.Equal("MEDIA_LIMIT_EXCEEDED",PublishingCapabilities.Validate(capability,[new(){Url="https://cdn.test/1",MimeType="image/jpeg",SizeBytes=51L*1024*1024}]));
        var json=System.Text.Json.JsonSerializer.Serialize(new PublishResultDto{RefreshedTargetAccessToken="private-token"});
        Assert.DoesNotContain("private-token",json);Assert.DoesNotContain("RefreshedTargetAccessToken",json);
    }
    [Fact]
    public async Task DestinationsAreIndependentAndSameKeyNeverReplays()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var actor=Guid.NewGuid();var workspace=Guid.NewGuid();var content=new Content{WorkspaceId=workspace,TextContent="approved"};
        db.Add(content);await db.SaveChangesAsync();content.Status=ContentStatusEnum.Approved;await db.SaveChangesAsync();
        var account=new SocialAccount{UserAccessToken="test",IsActive=true};
        var good=new SocialIntegration{WorkspaceId=workspace,Platform=SocialPlatformEnum.Facebook,AccessToken="test",SocialAccount=account};
        var unsupported=new SocialIntegration{WorkspaceId=workspace,Platform=SocialPlatformEnum.Google,AccessToken="test",SocialAccount=account};
        db.AddRange(good,unsupported);await db.SaveChangesAsync();
        var proxy=DispatchProxy.Create<IContentService,ContentProxy>();var fake=(ContentProxy)(object)proxy;
        var access=new Access();var progress=new PublishProgressContext();var service=new PublishOperationService(db,access,proxy,Options.Create(new InstagramSettings()),progress);
        var rows=await service.StartAsync(actor,workspace,content.Id,content.MediaVersion,[good.Id,unsupported.Id],"request-1",default);
        Assert.Equal("Published",rows.Single(o=>o.IntegrationId==good.Id).Status);Assert.Equal("Failed",rows.Single(o=>o.IntegrationId==unsupported.Id).Status);Assert.Equal(1,fake.Calls);
        await service.StartAsync(actor,workspace,content.Id,content.MediaVersion,[good.Id,unsupported.Id],"request-1",default);Assert.Equal(1,fake.Calls);
        await Assert.ThrowsAsync<MediaConflictException>(()=>service.StartAsync(actor,workspace,content.Id,Guid.NewGuid(),[good.Id,unsupported.Id],"request-1",default));
        fake.Timeout=true;
        rows=await service.StartAsync(actor,workspace,content.Id,content.MediaVersion,[good.Id],"request-2",default);
        Assert.Equal("NeedsAttention",rows[0].Status);Assert.Equal("PUBLISH_OUTCOME_UNKNOWN",rows[0].ErrorCode);
        await service.StartAsync(actor,workspace,content.Id,content.MediaVersion,[good.Id],"request-2",default);Assert.Equal(2,fake.Calls);
        access.Denied=true;
        await Assert.ThrowsAsync<ResourceMutationDeniedException>(()=>service.StartAsync(actor,workspace,content.Id,content.MediaVersion,[good.Id],"request-3",default));
        Assert.Equal(2,fake.Calls);Assert.False(await db.PublishRequests.AnyAsync(r=>r.IdempotencyKey=="request-3"));
        access.Denied=false;fake.Timeout=false;
        fake.BeforeCommit=async()=>{access.Denied=true;await progress.Report!("Publishing",[],default);};
        rows=await service.StartAsync(actor,workspace,content.Id,content.MediaVersion,[good.Id],"revoke-during-upload",default);
        Assert.Equal("NeedsAttention",rows[0].Status);Assert.Equal(1,fake.Commits);
        Assert.Null(progress.Report);
    }
}
