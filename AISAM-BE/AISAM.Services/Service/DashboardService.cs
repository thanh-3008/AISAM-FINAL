using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class DashboardService : IDashboardService
{
    private readonly IContentRepository _contentRepository;
    private readonly ISocialAccountRepository _socialAccountRepository;
    private readonly IPostRepository _postRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IContentCalendarRepository _contentCalendarRepository;

    public DashboardService(
        IContentRepository contentRepository,
        ISocialAccountRepository socialAccountRepository,
        IPostRepository postRepository,
        INotificationRepository notificationRepository,
        IContentCalendarRepository contentCalendarRepository)
    {
        _contentRepository = contentRepository;
        _socialAccountRepository = socialAccountRepository;
        _postRepository = postRepository;
        _notificationRepository = notificationRepository;
        _contentCalendarRepository = contentCalendarRepository;
    }

    public async Task<GenericResponse<DashboardSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var countRequest = new PaginationRequest
        {
            Page = 1,
            PageSize = 1
        };

        var draftCount = await _contentRepository.GetPagedByProfileIdAsync(profileId, countRequest, status: ContentStatusEnum.Draft, cancellationToken: cancellationToken);
        var pendingApprovalCount = await _contentRepository.GetPagedByProfileIdAsync(profileId, countRequest, status: ContentStatusEnum.PendingApproval, cancellationToken: cancellationToken);
        var publishedCount = await _contentRepository.GetPagedByProfileIdAsync(profileId, countRequest, status: ContentStatusEnum.Published, cancellationToken: cancellationToken);
        var postsCount = await _postRepository.GetPagedByProfileIdAsync(profileId, countRequest, cancellationToken: cancellationToken);
        var unreadCount = await _notificationRepository.GetUnreadCountAsync(profileId, cancellationToken);
        var socialAccounts = await _socialAccountRepository.GetByProfileIdAsync(profileId, cancellationToken);
        var upcomingSchedulesCount = await _contentCalendarRepository.CountUpcomingByProfileIdAsync(profileId, DateTime.UtcNow, cancellationToken);
        var failedSchedulesCount = await _contentCalendarRepository.CountFailedByProfileIdAsync(profileId, cancellationToken);

        var activeIntegrationCount = socialAccounts
            .Where(account => !account.IsDeleted)
            .SelectMany(account => account.SocialIntegrations)
            .Count(integration => !integration.IsDeleted && integration.IsActive);

        return GenericResponse<DashboardSummaryDto>.CreateSuccess(new DashboardSummaryDto
        {
            DraftContentCount = draftCount.TotalCount,
            PublishedContentCount = publishedCount.TotalCount,
            PendingApprovalContentCount = pendingApprovalCount.TotalCount,
            UpcomingScheduleCount = upcomingSchedulesCount,
            FailedScheduleCount = failedSchedulesCount,
            ActiveSocialIntegrationCount = activeIntegrationCount,
            PublishedPostCount = postsCount.TotalCount,
            UnreadNotificationCount = unreadCount
        }, "Dashboard summary retrieved successfully.");
    }

    public async Task<GenericResponse<DashboardSummaryDto>> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var countRequest = new PaginationRequest { Page = 1, PageSize = 1 };
        var draftCount = await _contentRepository.GetPagedByWorkspaceIdAsync(workspaceId, countRequest, status: ContentStatusEnum.Draft, cancellationToken: cancellationToken);
        var pendingApprovalCount = await _contentRepository.GetPagedByWorkspaceIdAsync(workspaceId, countRequest, status: ContentStatusEnum.PendingApproval, cancellationToken: cancellationToken);
        var publishedCount = await _contentRepository.GetPagedByWorkspaceIdAsync(workspaceId, countRequest, status: ContentStatusEnum.Published, cancellationToken: cancellationToken);
        var postsCount = await _postRepository.GetPagedByWorkspaceIdAsync(workspaceId, countRequest, cancellationToken: cancellationToken);
        var unreadCount = await _notificationRepository.GetUnreadCountByWorkspaceIdAsync(workspaceId, cancellationToken);
        var socialAccounts = await _socialAccountRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        var upcomingSchedulesCount = await _contentCalendarRepository.CountUpcomingByWorkspaceIdAsync(workspaceId, DateTime.UtcNow, cancellationToken);
        var failedSchedulesCount = await _contentCalendarRepository.CountFailedByWorkspaceIdAsync(workspaceId, cancellationToken);
        var activeIntegrationCount = socialAccounts
            .Where(account => !account.IsDeleted)
            .SelectMany(account => account.SocialIntegrations)
            .Count(integration => !integration.IsDeleted && integration.IsActive);

        return GenericResponse<DashboardSummaryDto>.CreateSuccess(new DashboardSummaryDto
        {
            DraftContentCount = draftCount.TotalCount,
            PublishedContentCount = publishedCount.TotalCount,
            PendingApprovalContentCount = pendingApprovalCount.TotalCount,
            UpcomingScheduleCount = upcomingSchedulesCount,
            FailedScheduleCount = failedSchedulesCount,
            ActiveSocialIntegrationCount = activeIntegrationCount,
            PublishedPostCount = postsCount.TotalCount,
            UnreadNotificationCount = unreadCount
        }, "Workspace dashboard summary retrieved successfully.");
    }
}
