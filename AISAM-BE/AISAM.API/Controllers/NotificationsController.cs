using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
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

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<NotificationListItemDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetPagedByWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), new PaginationRequest
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
        var result = await _notificationService.GetByIdInWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), notificationId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{notificationId:guid}/mark-read")]
    public async Task<ActionResult<GenericResponse<bool>>> MarkRead(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.MarkReadInWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), notificationId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult<GenericResponse<bool>>> MarkAllRead(CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.MarkAllReadInWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<GenericResponse<UnreadNotificationCountDto>>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetUnreadCountByWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{notificationId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> Delete(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.DeleteInWorkspaceAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), notificationId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
