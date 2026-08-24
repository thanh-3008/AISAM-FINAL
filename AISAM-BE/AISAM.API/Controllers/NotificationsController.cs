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
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IProfileRepository? _profileRepository;

    public NotificationsController(INotificationService notificationService, IProfileRepository? profileRepository = null)
    {
        _notificationService = notificationService;
        _profileRepository = profileRepository;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<NotificationListItemDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetPagedByWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), await GetProfileIdAsync(cancellationToken), new PaginationRequest
        {
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{notificationId:guid}")]
    public async Task<ActionResult<GenericResponse<NotificationDetailDto>>> GetById(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetByIdInWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), await GetProfileIdAsync(cancellationToken), notificationId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{notificationId:guid}/mark-read")]
    public async Task<ActionResult<GenericResponse<bool>>> MarkRead(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.MarkReadInWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), await GetProfileIdAsync(cancellationToken), notificationId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult<GenericResponse<bool>>> MarkAllRead(CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.MarkAllReadInWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), await GetProfileIdAsync(cancellationToken), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<GenericResponse<UnreadNotificationCountDto>>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetUnreadCountByWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), await GetProfileIdAsync(cancellationToken), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{notificationId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> Delete(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.DeleteInWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), await GetProfileIdAsync(cancellationToken), notificationId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private Task<Guid> GetProfileIdAsync(CancellationToken cancellationToken)
        => _profileRepository == null
            ? Task.FromResult(ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext))
            : WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
}
