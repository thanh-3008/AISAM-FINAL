using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class NotificationsControllerTests
{
    [Fact]
    public async Task GetPaged_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeNotificationService
        {
            PagedResult = GenericResponse<PagedResult<NotificationListItemDto>>.CreateSuccess(new PagedResult<NotificationListItemDto>())
        };
        var controller = CreateController(service, profileId);

        await controller.GetPaged();

        Assert.Equal(profileId, service.LastProfileId);
    }

    [Fact]
    public async Task MarkRead_ReturnsServiceStatusCode_WhenNotificationBelongsToAnotherProfile()
    {
        var service = new FakeNotificationService
        {
            MarkReadResult = GenericResponse<bool>.CreateError("Notification not found.", HttpStatusCode.NotFound)
        };
        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.MarkRead(Guid.NewGuid());

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    private static NotificationsController CreateController(INotificationService service, Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = profileId;

        return new NotificationsController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<PagedResult<NotificationListItemDto>> PagedResult { get; set; } = GenericResponse<PagedResult<NotificationListItemDto>>.CreateSuccess(new PagedResult<NotificationListItemDto>());
        public GenericResponse<NotificationDetailDto> DetailResult { get; set; } = GenericResponse<NotificationDetailDto>.CreateSuccess(new NotificationDetailDto());
        public GenericResponse<bool> MarkReadResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<bool> MarkAllReadResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<UnreadNotificationCountDto> CountResult { get; set; } = GenericResponse<UnreadNotificationCountDto>.CreateSuccess(new UnreadNotificationCountDto());

        public Task<GenericResponse<PagedResult<NotificationListItemDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PagedResult);
        }

        public Task<GenericResponse<NotificationDetailDto>> GetByIdAsync(Guid profileId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(DetailResult);
        }

        public Task<GenericResponse<bool>> MarkReadAsync(Guid profileId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(MarkReadResult);
        }

        public Task<GenericResponse<bool>> MarkAllReadAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(MarkAllReadResult);
        }

        public Task<GenericResponse<UnreadNotificationCountDto>> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(CountResult);
        }

        public Task<GenericResponse<PagedResult<NotificationListItemDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PagedResult);
        }

        public Task<GenericResponse<NotificationDetailDto>> GetByIdInWorkspaceAsync(Guid workspaceId, Guid profileId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(DetailResult);
        }

        public Task<GenericResponse<bool>> MarkReadInWorkspaceAsync(Guid workspaceId, Guid profileId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(MarkReadResult);
        }

        public Task<GenericResponse<bool>> MarkAllReadInWorkspaceAsync(Guid workspaceId, Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(MarkAllReadResult);
        }

        public Task<GenericResponse<UnreadNotificationCountDto>> GetUnreadCountByWorkspaceAsync(Guid workspaceId, Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(CountResult);
        }

        public Task<GenericResponse<bool>> DeleteInWorkspaceAsync(Guid workspaceId, Guid profileId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<bool>.CreateSuccess(true, "Deleted."));
        }
    }
}




