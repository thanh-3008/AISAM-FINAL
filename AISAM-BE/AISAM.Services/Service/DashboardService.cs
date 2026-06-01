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

        var draftCountTask = _contentRepository.GetPagedByProfileIdAsync(profileId, countRequest, status: ContentStatusEnum.Draft, cancellationToken: cancellationToken);
        var pendingApprovalCountTask = _contentRepository.GetPagedByProfileIdAsync(profileId, countRequest, status: ContentStatusEnum.PendingApproval, cancellationToken: cancellationToken);
        var publishedCountTask = _contentRepository.GetPagedByProfileIdAsync(profileId, countRequest, status: ContentStatusEnum.Published, cancellationToken: cancellationToken);
        var postsCountTask = _postRepository.GetPagedByProfileIdAsync(profileId, countRequest, cancellationToken: cancellationToken);
        var unreadCountTask = _notificationRepository.GetUnreadCountAsync(profileId, cancellationToken);
        var socialAccountsTask = _socialAccountRepository.GetByProfileIdAsync(profileId, cancellationToken);
        var upcomingSchedulesTask = _contentCalendarRepository.CountUpcomingByProfileIdAsync(profileId, DateTime.UtcNow, cancellationToken);
        var failedSchedulesTask = _contentCalendarRepository.CountFailedByProfileIdAsync(profileId, cancellationToken);

        await Task.WhenAll(
            draftCountTask,
            pendingApprovalCountTask,
            publishedCountTask,
            postsCountTask,
            unreadCountTask,
            socialAccountsTask,
            upcomingSchedulesTask,
            failedSchedulesTask);

        var activeIntegrationCount = socialAccountsTask.Result
            .Where(account => !account.IsDeleted)
            .SelectMany(account => account.SocialIntegrations)
            .Count(integration => !integration.IsDeleted && integration.IsActive);

        return GenericResponse<DashboardSummaryDto>.CreateSuccess(new DashboardSummaryDto
        {
            DraftContentCount = draftCountTask.Result.TotalCount,
            PublishedContentCount = publishedCountTask.Result.TotalCount,
            PendingApprovalContentCount = pendingApprovalCountTask.Result.TotalCount,
            UpcomingScheduleCount = upcomingSchedulesTask.Result,
            FailedScheduleCount = failedSchedulesTask.Result,
            ActiveSocialIntegrationCount = activeIntegrationCount,
            PublishedPostCount = postsCountTask.Result.TotalCount,
            UnreadNotificationCount = unreadCountTask.Result
        }, "Dashboard summary retrieved successfully.");
    }
}
