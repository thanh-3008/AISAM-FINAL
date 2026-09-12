using AISAM.API.Utils;
using AISAM.Repositories;
using AISAM.Services.Access;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AISAM.API.Controllers;
[ApiController,Authorize]
public sealed class PublishingCapabilitiesController(AisamContext db,IAccessControlService access,Microsoft.Extensions.Options.IOptions<AISAM.Common.Models.InstagramSettings> settings):ControllerBase
{
    [HttpGet("api/content/{contentId:guid}/publish-preview")]
    public async Task<IActionResult> Preview(Guid contentId, CancellationToken ct)
    {
        var actor=UserClaimsHelper.GetUserIdOrThrow(User);
        var workspace=WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        if (!(await access.CheckAsync(new(actor,workspace,AccessResourceKind.Content,contentId,ResourcePermission.ContentView),ct)).Allowed) return NotFound();
        var content=await db.Contents.AsNoTracking().SingleAsync(c=>c.Id==contentId && c.WorkspaceId==workspace,ct);
        var snapshot=await db.PublishSnapshots.AsNoTracking().Include(s=>s.Media).SingleOrDefaultAsync(s=>s.Id==content.ApprovedSnapshotId && s.ContentId==contentId,ct);
        var media=snapshot?.Media.ToList() ?? await db.ContentMedia.AsNoTracking().Where(m=>m.ContentId==contentId)
            .OrderBy(m=>m.SortOrder).Select(m=>new AISAM.Data.Model.SnapshotMedia {Url=m.Asset.StoragePath,MimeType=m.Asset.MimeType,SizeBytes=m.Asset.SizeBytes,DurationSeconds=m.Asset.DurationSeconds,SortOrder=m.SortOrder,IsCover=m.IsCover}).ToListAsync(ct);
        var frozen=snapshot is null?null:System.Text.Json.JsonSerializer.Deserialize<AISAM.Data.Model.Content>(snapshot.Payload);
        var captions=frozen?.FormattedCaptions ?? AISAM.Data.RichTextDocument.FormatCaptions(frozen?.TextContent ?? content.TextContent);
        var integrations=await db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking().Include(i=>i.SocialAccount)
            .Where(i=>i.WorkspaceId==workspace && i.BrandId==content.BrandId && !i.IsDeleted).ToListAsync(ct);
        var destinations=new List<object>();
        foreach(var integration in integrations)
        {
            if (!(await access.CheckAsync(new(actor,workspace,AccessResourceKind.Channel,integration.Id,ResourcePermission.SocialView),ct)).Allowed) continue;
            var permission=await access.CheckAsync(new(actor,workspace,AccessResourceKind.Content,contentId,ResourcePermission.PostPublish,integration.Id),ct);
            var capability=PublishingCapabilities.For(integration,integration.SocialAccount,DateTime.UtcNow,settings.Value.VerifiedCarouselIntegrationIds.Contains(integration.Id));
            var platform=integration.Platform.ToString().ToLowerInvariant();
            destinations.Add(new {id=integration.Id,name=integration.TargetName??platform,platform,capability,
                error=permission.Allowed?PublishingCapabilities.Validate(capability,media):permission.ErrorCode,
                caption=captions.GetValueOrDefault(platform,content.TextContent)});
        }
        return Ok(new {success=true,data=new {version=content.MediaVersion,approved=snapshot is not null,media=media.OrderBy(m=>m.SortOrder).Select(m=>new{m.Url,m.MimeType,m.SortOrder,m.IsCover}),destinations}});
    }
    [HttpGet("api/social/integrations/{integrationId:guid}/publishing-capabilities")]
    public async Task<IActionResult> Get(Guid integrationId,CancellationToken ct)
    {
        var decision=await access.CheckAsync(new(UserClaimsHelper.GetUserIdOrThrow(User),WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            AccessResourceKind.Channel,integrationId,ResourcePermission.SocialView),ct);
        if(!decision.Allowed)return StatusCode(decision.StatusCode,new{success=false,errorCode=decision.ErrorCode});
        var integration=await db.SocialIntegrations.IgnoreQueryFilters().Include(i=>i.SocialAccount).SingleAsync(i=>i.Id==integrationId,ct);
        return Ok(new{success=true,data=PublishingCapabilities.For(integration,integration.SocialAccount,DateTime.UtcNow,settings.Value.VerifiedCarouselIntegrationIds.Contains(integration.Id))});
    }
}
