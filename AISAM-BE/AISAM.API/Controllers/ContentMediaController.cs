using AISAM.API.Utils;
using AISAM.Repositories;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AISAM.API.Controllers;
[ApiController,Authorize,Route("api/content/{contentId:guid}/media")]
public sealed class ContentMediaController(ContentMediaService service,AisamContext db):ControllerBase
{
    public sealed record Change(Guid ExpectedVersion,List<MediaItemRequest> Items);
    public sealed record Import(Guid ExpectedVersion);
    private Guid Actor=>UserClaimsHelper.GetUserIdOrThrow(User);
    private Guid Workspace=>WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
    private async Task<IActionResult> Execute(Func<Task<object>> action)
    {
        try{return Ok(new {success=true,data=await action()});}
        catch(ArgumentException e){return BadRequest(new {success=false,message=e.Message});}
        catch(ResourceMutationDeniedException){return StatusCode(403,new {success=false,errorCode="RESOURCE_ACCESS_DENIED"});}
        catch(Exception e) when(e is MediaConflictException or DbUpdateConcurrencyException || (e as PostgresException ?? (e as DbUpdateException)?.InnerException as PostgresException)?.SqlState=="40001")
        {return Conflict(new {success=false,errorCode="MEDIA_VERSION_CONFLICT",message="Content changed. Reload before saving media."});}
    }
    [HttpGet]public Task<IActionResult> Get(Guid contentId,CancellationToken ct)=>Execute(async()=>await service.ReadAsync(Actor,Workspace,contentId,ct));
    [HttpPut]public Task<IActionResult> Replace(Guid contentId,Change request,CancellationToken ct)=>Execute(async()=>await service.ReplaceAsync(Actor,Workspace,contentId,request.ExpectedVersion,request.Items,ct));
    [HttpPost("import-legacy")]public Task<IActionResult> ImportLegacy(Guid contentId,Import request,CancellationToken ct)=>Execute(async()=>await service.ImportLegacyAsync(Actor,Workspace,contentId,request.ExpectedVersion,ct));
    [HttpPost("upload"),RequestSizeLimit(210L*1024*1024),RequestFormLimits(MultipartBodyLengthLimit=210L*1024*1024)]
    public Task<IActionResult> Upload(Guid contentId,[FromForm]List<IFormFile> files,CancellationToken ct)=>Execute(async()=>await service.UploadAsync(Actor,Workspace,contentId,files,ct));
    [HttpGet("snapshots")]
    public Task<IActionResult> Snapshots(Guid contentId,CancellationToken ct)=>Execute(async()=>await db.PublishSnapshots.AsNoTracking().Where(s=>s.ContentId==contentId)
        .OrderByDescending(s=>s.CreatedAt).Select(s=>new {s.Id,s.Version,s.CreatedAt,s.Checksum,s.Payload,media=s.Media.OrderBy(m=>m.SortOrder).Select(m=>new{m.AssetId,m.SortOrder,m.Url,m.MimeType,m.IsCover,m.AltText,m.Caption,m.SizeBytes,m.DurationSeconds,m.Width,m.Height})}).ToListAsync(ct));
}
