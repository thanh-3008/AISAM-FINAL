using AISAM.Common.Dtos.Response;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;

namespace AISAM.IntegrationTests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ReturnsCountsScopedToActiveProfile()
    {
        var profileId = Guid.NewGuid();
        var otherProfileId = Guid.NewGuid();
        var service = CreateService(
            contentRepository: new FakeContentRepository(
                new Content { Id = Guid.NewGuid(), ProfileId = profileId, BrandId = Guid.NewGuid(), AdType = AdTypeEnum.TextOnly, TextContent = "draft-1", Status = ContentStatusEnum.Draft },
                new Content { Id = Guid.NewGuid(), ProfileId = profileId, BrandId = Guid.NewGuid(), AdType = AdTypeEnum.TextOnly, TextContent = "draft-2", Status = ContentStatusEnum.Draft },
                new Content { Id = Guid.NewGuid(), ProfileId = profileId, BrandId = Guid.NewGuid(), AdType = AdTypeEnum.TextOnly, TextContent = "pending", Status = ContentStatusEnum.PendingApproval },
                new Content { Id = Guid.NewGuid(), ProfileId = profileId, BrandId = Guid.NewGuid(), AdType = AdTypeEnum.TextOnly, TextContent = "published", Status = ContentStatusEnum.Published },
                new Content { Id = Guid.NewGuid(), ProfileId = otherProfileId, BrandId = Guid.NewGuid(), AdType = AdTypeEnum.TextOnly, TextContent = "other", Status = ContentStatusEnum.Published }),
            socialAccountRepository: new FakeSocialAccountRepository(
                CreateAccount(profileId, 2, 1),
                CreateAccount(otherProfileId, 3, 0)),
            postRepository: new FakePostRepository(profileId, 3, otherProfileId, 2),
            notificationRepository: new FakeNotificationRepository(profileId, 4, otherProfileId, 6),
            contentCalendarRepository: new FakeContentCalendarRepository(upcomingCount: 5, failedCount: 2));

        var result = await service.GetSummaryAsync(profileId);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.DraftContentCount);
        Assert.Equal(1, result.Data.PublishedContentCount);
        Assert.Equal(1, result.Data.PendingApprovalContentCount);
        Assert.Equal(5, result.Data.UpcomingScheduleCount);
        Assert.Equal(2, result.Data.FailedScheduleCount);
        Assert.Equal(2, result.Data.ActiveSocialIntegrationCount);
        Assert.Equal(3, result.Data.PublishedPostCount);
        Assert.Equal(4, result.Data.UnreadNotificationCount);
    }

    [Fact]
    public async Task GetSummaryAsync_CountsUpcomingSchedulesAndUnreadNotifications()
    {
        var profileId = Guid.NewGuid();
        var service = CreateService(
            contentRepository: new FakeContentRepository(),
            socialAccountRepository: new FakeSocialAccountRepository(),
            postRepository: new FakePostRepository(profileId, 0),
            notificationRepository: new FakeNotificationRepository(profileId, 1),
            contentCalendarRepository: new FakeContentCalendarRepository(upcomingCount: 2, failedCount: 1));

        var result = await service.GetSummaryAsync(profileId);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.UpcomingScheduleCount);
        Assert.Equal(1, result.Data.FailedScheduleCount);
        Assert.Equal(1, result.Data.UnreadNotificationCount);
    }

    [Fact]
    public async Task GetWorkspaceSummaryAsync_ReturnsOnlyRequestedWorkspaceCounts()
    {
        var workspaceId = Guid.NewGuid();
        var otherWorkspaceId = Guid.NewGuid();
        var service = CreateService(
            contentRepository: new FakeContentRepository(
                new Content { WorkspaceId = workspaceId, ProfileId = Guid.NewGuid(), BrandId = Guid.NewGuid(), AdType = AdTypeEnum.TextOnly, TextContent = "own", Status = ContentStatusEnum.Draft },
                new Content { WorkspaceId = otherWorkspaceId, ProfileId = Guid.NewGuid(), BrandId = Guid.NewGuid(), AdType = AdTypeEnum.TextOnly, TextContent = "other", Status = ContentStatusEnum.Draft }),
            socialAccountRepository: new FakeSocialAccountRepository(),
            postRepository: new FakePostRepository(workspaceId, 2, otherWorkspaceId, 9),
            notificationRepository: new FakeNotificationRepository(workspaceId, 3, otherWorkspaceId, 8),
            contentCalendarRepository: new FakeContentCalendarRepository(upcomingCount: 4, failedCount: 1));

        var result = await service.GetWorkspaceSummaryAsync(workspaceId);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.DraftContentCount);
        Assert.Equal(2, result.Data.PublishedPostCount);
        Assert.Equal(3, result.Data.UnreadNotificationCount);
    }

    private static DashboardService CreateService(
        IContentRepository? contentRepository = null,
        ISocialAccountRepository? socialAccountRepository = null,
        IPostRepository? postRepository = null,
        INotificationRepository? notificationRepository = null,
        IContentCalendarRepository? contentCalendarRepository = null)
    {
        return new DashboardService(
            contentRepository ?? new FakeContentRepository(),
            socialAccountRepository ?? new FakeSocialAccountRepository(),
            postRepository ?? new FakePostRepository(Guid.NewGuid(), 0),
            notificationRepository ?? new FakeNotificationRepository(Guid.NewGuid(), 0),
            contentCalendarRepository ?? new FakeContentCalendarRepository());
    }

    private static SocialAccount CreateAccount(Guid profileId, int activeIntegrations, int deletedIntegrations)
    {
        var account = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "token"
        };

        for (var index = 0; index < activeIntegrations; index++)
        {
            account.SocialIntegrations.Add(new SocialIntegration
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                BrandId = Guid.NewGuid(),
                SocialAccountId = account.Id,
                Platform = SocialPlatformEnum.Facebook,
                AccessToken = "token",
                IsDeleted = false,
                IsActive = true
            });
        }

        for (var index = 0; index < deletedIntegrations; index++)
        {
            account.SocialIntegrations.Add(new SocialIntegration
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                BrandId = Guid.NewGuid(),
                SocialAccountId = account.Id,
                Platform = SocialPlatformEnum.Facebook,
                AccessToken = "token",
                IsDeleted = true,
                IsActive = false
            });
        }

        return account;
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        private readonly List<Content> _contents;

        public FakeContentRepository(params Content[] contents)
        {
            _contents = contents.ToList();
        }

        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<PagedResult<ContentListDto>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            var query = _contents.Where(content => content.ProfileId == profileId);
            if (!includeDeleted)
            {
                query = query.Where(content => !content.IsDeleted);
            }

            if (status.HasValue)
            {
                query = query.Where(content => content.Status == status.Value);
            }

            var data = query.ToList();
            return Task.FromResult(new PagedResult<ContentListDto>
            {
                Data = data.Select(c => new ContentListDto { Id = c.Id, ProfileId = c.ProfileId, WorkspaceId = c.WorkspaceId, Status = c.Status }).ToList(),
                TotalCount = data.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Content content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<PagedResult<ContentListDto>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            var query = _contents.Where(content => content.WorkspaceId == workspaceId);
            if (!includeDeleted) query = query.Where(content => !content.IsDeleted);
            if (status.HasValue) query = query.Where(content => content.Status == status.Value);
            var data = query.ToList();
            return Task.FromResult(new PagedResult<ContentListDto> { Data = data.Select(c => new ContentListDto { Id = c.Id, ProfileId = c.ProfileId, WorkspaceId = c.WorkspaceId, Status = c.Status }).ToList(), TotalCount = data.Count, Page = request.Page, PageSize = request.PageSize });
        }

        public Task<PagedResult<ContentListDto>> GetPagedAllAsync(PaginationRequest request, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Dictionary<DateTime, int>> GetDailyCreatedAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
    }

    private sealed class FakeSocialAccountRepository : ISocialAccountRepository
    {
        private readonly List<SocialAccount> _accounts;

        public FakeSocialAccountRepository(params SocialAccount[] accounts)
        {
            _accounts = accounts.ToList();
        }

        public Task<SocialAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialAccount?> GetByIdWithIntegrationsAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialAccount?> GetByProfileIdPlatformAndAccountIdAsync(Guid profileId, SocialPlatformEnum platform, string accountId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialAccount> AddAsync(SocialAccount account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(SocialAccount account, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IReadOnlyList<SocialAccount>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SocialAccount> data = _accounts
                .Where(account => account.ProfileId == profileId && !account.IsDeleted)
                .ToList();
            return Task.FromResult(data);
        }

        public Task<IReadOnlyList<SocialAccount>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SocialAccount> data = _accounts.Where(account => account.WorkspaceId == workspaceId && !account.IsDeleted).ToList();
            return Task.FromResult(data);
        }

        public Task<IReadOnlyList<SocialAccount>> GetByProfileIdsAsync(IEnumerable<Guid> profileIds, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SocialAccount> data = _accounts.Where(account => profileIds.Contains(account.ProfileId)).ToList();
            return Task.FromResult(data);
        }

        
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly Dictionary<Guid, int> _counts = new();

        public FakePostRepository(Guid profileId, int count, Guid? secondProfileId = null, int secondCount = 0)
        {
            _counts[profileId] = count;
            if (secondProfileId.HasValue)
            {
                _counts[secondProfileId.Value] = secondCount;
            }
        }

        public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<PagedResult<Post>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            _counts.TryGetValue(profileId, out var count);
            return Task.FromResult(new PagedResult<Post>
            {
                Data = new List<Post>(),
                TotalCount = count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<PagedResult<Post>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
            => GetPagedByProfileIdAsync(workspaceId, request, brandId, status, cancellationToken);

        public Task<List<Post>> GetPublishedByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Post post, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        private readonly Dictionary<Guid, int> _unreadCounts = new();

        public FakeNotificationRepository(Guid profileId, int unreadCount, Guid? secondProfileId = null, int secondUnreadCount = 0)
        {
            _unreadCounts[profileId] = unreadCount;
            if (secondProfileId.HasValue)
            {
                _unreadCounts[secondProfileId.Value] = secondUnreadCount;
            }
        }

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Notification>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            _unreadCounts.TryGetValue(profileId, out var count);
            return Task.FromResult(count);
        }

        public Task<int> GetUnreadCountByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => GetUnreadCountAsync(workspaceId, cancellationToken);

        public Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeContentCalendarRepository : IContentCalendarRepository
    {
        private readonly int _upcomingCount;
        private readonly int _failedCount;

        public FakeContentCalendarRepository(int upcomingCount = 0, int failedCount = 0)
        {
            _upcomingCount = upcomingCount;
            _failedCount = failedCount;
        }

        public Task<ContentCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<ContentCalendar>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ContentCalendar>> GetUpcomingByProfileIdAsync(Guid profileId, int limit, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ContentCalendar>> GetDueSchedulesAsync(DateTime utcNow, int limit, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ContentCalendar>> ClaimDueSchedulesAtomicallyAsync(DateTime utcNow, int limit, int maxAttemptCount, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> HasActiveScheduleAsync(Guid contentId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task CancelActiveSchedulesForContentAsync(Guid contentId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<ContentCalendar> AddAsync(ContentCalendar schedule, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<int> CountUpcomingByProfileIdAsync(Guid profileId, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(_upcomingCount);

        public Task<int> CountFailedByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(_failedCount);

        public Task<int> CountUpcomingByWorkspaceIdAsync(Guid workspaceId, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(_upcomingCount);

        public Task<int> CountFailedByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(_failedCount);
    }
}






