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
