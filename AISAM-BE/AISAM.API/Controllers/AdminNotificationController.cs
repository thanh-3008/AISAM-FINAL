using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminNotificationController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<AdminNotificationController> _logger;

    public AdminNotificationController(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        INotificationRepository notificationRepository,
        ILogger<AdminNotificationController> logger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    [HttpPost("broadcast")]
    public async Task<ActionResult<GenericResponse<bool>>> BroadcastNotification(
        [FromBody] BroadcastNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<bool>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        var sentCount = 0;

        foreach (var user in users)
        {
            if (request.ExcludeAdmins && user.Role == UserRoleEnum.Admin) continue;

            var profiles = await _profileRepository.GetByUserIdAsync(user.Id, cancellationToken);
            var profile = profiles.FirstOrDefault();
            if (profile == null) continue;

            var notification = new Notification
            {
                ProfileId = profile.Id,
                WorkspaceId = Guid.Empty,
                Title = request.Title,
                Message = request.Message,
                Type = NotificationTypeEnum.SystemUpdate,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification, cancellationToken);
            sentCount++;
        }

        return Ok(GenericResponse<bool>.CreateSuccess(true, $"Notification sent to {sentCount} users."));
    }
}

public class BroadcastNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool ExcludeAdmins { get; set; }
}
