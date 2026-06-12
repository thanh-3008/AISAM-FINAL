using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/content-schedules")]
[Authorize]
public sealed class ContentSchedulesController : ControllerBase
{
    private readonly IContentScheduleService _contentScheduleService;

    public ContentSchedulesController(IContentScheduleService contentScheduleService)
    {
        _contentScheduleService = contentScheduleService;
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<ContentScheduleDto>>> Create(
        [FromBody] CreateContentScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.CreateAsync(
            GetProfileId(),
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            request,
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<ContentScheduleDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.GetPagedAsync(GetProfileId(), new PaginationRequest
        {
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<ContentScheduleDto>>>> GetUpcoming(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.GetUpcomingAsync(GetProfileId(), limit, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{scheduleId:guid}")]
    public async Task<ActionResult<GenericResponse<ContentScheduleDto>>> GetById(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.GetByIdAsync(GetProfileId(), scheduleId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{scheduleId:guid}")]
    public async Task<ActionResult<GenericResponse<ContentScheduleDto>>> Update(
        Guid scheduleId,
        [FromBody] UpdateContentScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.UpdateAsync(
            GetProfileId(),
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            scheduleId,
            request,
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{scheduleId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> Delete(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.DeleteAsync(
            GetProfileId(),
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            scheduleId,
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetProfileId()
    {
        return ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
    }
}
