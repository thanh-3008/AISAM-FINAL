using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Repositories.IRepositories;
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
    private readonly IProfileRepository _profileRepository;

    public ContentSchedulesController(IContentScheduleService contentScheduleService, IProfileRepository profileRepository)
    {
        _contentScheduleService = contentScheduleService;
        _profileRepository = profileRepository;
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<ContentScheduleDto>>> Create(
        [FromBody] CreateContentScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.CreateInWorkspaceAsync(GetWorkspaceId(), await GetProfileIdAsync(cancellationToken), request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<ContentScheduleDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.GetPagedByWorkspaceAsync(GetWorkspaceId(), new PaginationRequest
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
        var result = await _contentScheduleService.GetUpcomingByWorkspaceAsync(GetWorkspaceId(), limit, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{scheduleId:guid}")]
    public async Task<ActionResult<GenericResponse<ContentScheduleDto>>> GetById(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.GetByIdInWorkspaceAsync(GetWorkspaceId(), scheduleId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{scheduleId:guid}")]
    public async Task<ActionResult<GenericResponse<ContentScheduleDto>>> Update(
        Guid scheduleId,
        [FromBody] UpdateContentScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.UpdateInWorkspaceAsync(GetWorkspaceId(), scheduleId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<GenericResponse<BulkCreateResultDto>>> BulkCreate(
        [FromBody] BulkCreateContentScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.BulkCreateInWorkspaceAsync(GetWorkspaceId(), await GetProfileIdAsync(cancellationToken), request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{scheduleId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> Delete(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentScheduleService.DeleteInWorkspaceAsync(GetWorkspaceId(), scheduleId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private async Task<Guid> GetProfileIdAsync(CancellationToken cancellationToken)
    {
        return await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
    }
    private Guid GetWorkspaceId() => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
}
