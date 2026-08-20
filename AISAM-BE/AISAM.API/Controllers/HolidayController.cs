using AISAM.API.Utils;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/workspace-context/{workspaceId}/holidays")]
[Authorize]
public class HolidayController : ControllerBase
{
    private readonly IHolidayService _holidayService;
    private readonly IProfileRepository _profileRepository;

    public HolidayController(IHolidayService holidayService, IProfileRepository profileRepository)
    {
        _holidayService = holidayService;
        _profileRepository = profileRepository;
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingHolidays(Guid workspaceId, [FromQuery] int days = 30)
    {
        var result = await _holidayService.GetUpcomingAsync(days);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{holidayId}/suggest-caption")]
    public async Task<IActionResult> SuggestCaption(Guid workspaceId, Guid holidayId, [FromBody] SuggestHolidayCaptionRequest request)
    {
        var profileId = await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository);
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);

        var result = await _holidayService.GetSuggestionAsync(workspaceId, profileId, userId, request.BrandId, holidayId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{holidayId}/generate-video")]
    public async Task<IActionResult> GenerateVideo(Guid workspaceId, Guid holidayId, [FromBody] SuggestHolidayCaptionRequest request)
    {
        var profileId = await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository);
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);

        var result = await _holidayService.GenerateHolidayVideoAsync(workspaceId, profileId, userId, request.BrandId, holidayId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("custom-event")]
    public async Task<IActionResult> GenerateCustomEvent(Guid workspaceId, [FromBody] AISAM.Common.Dtos.Request.GenerateCustomEventContentRequest request)
    {
        var profileId = await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository);
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);

        var result = await _holidayService.GetCustomEventSuggestionAsync(workspaceId, profileId, userId, request);
        return StatusCode(result.StatusCode, result);
    }
}

public class SuggestHolidayCaptionRequest
{
    public Guid BrandId { get; set; }
}
