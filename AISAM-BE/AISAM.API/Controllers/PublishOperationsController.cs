using AISAM.API.Utils;
using AISAM.Repositories;
using AISAM.Services.Access;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AISAM.API.Controllers;
[ApiController,Authorize]
public sealed class PublishOperationsController(PublishOperationService service,AisamContext db,IAccessControlService access):ControllerBase
{
    public sealed record Start(Guid ExpectedVersion,List<Guid> IntegrationIds,string IdempotencyKey);
    private Guid Actor=>UserClaimsHelper.GetUserIdOrThrow(User);
    private Guid Workspace=>WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
    private static object View(AISAM.Data.Model.PublishOperation o)=>new{o.Id,o.ContentId,o.SnapshotId,o.IntegrationId,o.Status,o.Attempts,o.ProviderId,o.ErrorCode,o.CreatedAt,o.UpdatedAt,media=System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(o.MediaResults)};
    [HttpPost("api/content/{contentId:guid}/publish-operations")]
    public async Task<IActionResult> Create(Guid contentId,Start request,CancellationToken ct)
    {
        try
        {
            var rows=await service.StartAsync(Actor,Workspace,contentId,request.ExpectedVersion,request.IntegrationIds,request.IdempotencyKey,ct);
            var status=rows.All(o=>o.Status=="Published")?"Published":rows.Any(o=>o.Status=="Published")?"PartiallyPublished":
                rows.Any(o=>o.Status=="NeedsAttention")?"NeedsAttention":rows.Any(o=>o.Status=="Publishing")?"Publishing":
                rows.Any(o=>o.Status=="UploadingMedia")?"UploadingMedia":rows.Any(o=>o.Status=="Queued")?"Queued":
                rows.All(o=>o.Status=="Cancelled")?"Cancelled":"Failed";
            return Ok(new{success=true,data=new{status,operations=rows.Select(View)}});
        }
        catch(MediaConflictException){return Conflict(new{success=false,errorCode="CONTENT_VERSION_CONFLICT"});}
        catch(DbUpdateException e)when(e.InnerException is Npgsql.PostgresException{SqlState:"23505"})
        {return Conflict(new{success=false,errorCode="IDEMPOTENCY_CONFLICT"});}
    }
    [HttpGet("api/publish-operations/{operationId:guid}")]
    public async Task<IActionResult> Get(Guid operationId,CancellationToken ct)
    {
        var operation=await db.PublishOperations.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(o=>o.Id==operationId&&o.WorkspaceId==Workspace,ct);
        if(operation is null)return NotFound();
        var decision=await access.CheckAsync(new(Actor,Workspace,AccessResourceKind.Content,operation.ContentId,ResourcePermission.ContentView),ct);
        if(!decision.Allowed)return NotFound();
        return Ok(new{success=true,data=View(operation)});
    }
    [HttpPost("api/publish-operations/{operationId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid operationId,CancellationToken ct)
    {
        var operation=await db.PublishOperations.IgnoreQueryFilters().SingleOrDefaultAsync(o=>o.Id==operationId&&o.WorkspaceId==Workspace,ct);
        if(operation is null)return NotFound();
        var decision=await access.CheckAsync(new(Actor,Workspace,AccessResourceKind.Content,operation.ContentId,ResourcePermission.PostPublish,operation.IntegrationId),ct);
        if(!decision.Allowed)return NotFound();
        if(operation.Status!="Queued")return Conflict(new{success=false,errorCode="PUBLISH_ALREADY_STARTED"});
        operation.Status="Cancelled";operation.UpdatedAt=DateTime.UtcNow;await db.SaveChangesAsync(ct);
        return Ok(new{success=true,data=View(operation)});
    }
}
