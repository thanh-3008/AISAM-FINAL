using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;

namespace AISAM.IntegrationTests;

public class QuotaServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ReturnsDerivedUsageInsideCurrentSubscriptionWindow()
    {
        var profileId = Guid.NewGuid();
        var service = new QuotaService(new FakeSubscriptionRepository(
            new Subscription
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                Plan = SubscriptionPlanEnum.Plus,
                QuotaAIContentPerDay = 3,
                QuotaPostsPerMonth = 5,
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 6, 30),
                IsActive = true
            },
            promptUsage: 1,
            postUsage: 2),
            new FakeWorkspaceRepository(),
            new FakeProfileRepository(), new FakeContentRepository());

        var result = await service.GetSummaryAsync(profileId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.PromptUsage);
        Assert.Equal(2, result.Data.PostUsage);
        Assert.Equal(2, result.Data.PromptRemaining);
        Assert.Equal(3, result.Data.PostRemaining);
    }

    [Fact]
    public async Task EnsurePromptQuotaAsync_ReturnsForbiddenWithPromptErrorCode_WhenQuotaExceeded()
    {
        var profileId = Guid.NewGuid();
        var service = new QuotaService(new FakeSubscriptionRepository(
            new Subscription
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                Plan = SubscriptionPlanEnum.Free,
                QuotaAIContentPerDay = 1,
                QuotaPostsPerMonth = 5,
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 6, 30),
                IsActive = true
            },
            promptUsage: 1,
            postUsage: 0),
            new FakeWorkspaceRepository(),
            new FakeProfileRepository(), new FakeContentRepository());

        var result = await service.EnsurePromptQuotaAsync(profileId);

        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("PROMPT_QUOTA_EXCEEDED", result.Error?.ErrorCode);
    }

    [Fact]
    public async Task EnsurePostQuotaAsync_ReturnsForbiddenWithPostErrorCode_WhenQuotaExceeded()
    {
        var profileId = Guid.NewGuid();
        var service = new QuotaService(new FakeSubscriptionRepository(
            new Subscription
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                Plan = SubscriptionPlanEnum.Free,
                QuotaAIContentPerDay = 2,
                QuotaPostsPerMonth = 1,
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 6, 30),
                IsActive = true
            },
            promptUsage: 0,
            postUsage: 1),
            new FakeWorkspaceRepository(),
            new FakeProfileRepository(), new FakeContentRepository());

        var result = await service.EnsurePostQuotaAsync(profileId);

        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("POST_QUOTA_EXCEEDED", result.Error?.ErrorCode);
    }

    [Fact]
    public async Task GetSummaryAsync_CountsPromptUsageForCurrentUtcDay()
    {
        var profileId = Guid.NewGuid();
        var repository = new FakeSubscriptionRepository(
            new Subscription
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                Plan = SubscriptionPlanEnum.Plus,
                QuotaAIContentPerDay = 3,
                StartDate = DateTime.UtcNow.Date.AddDays(-10),
                EndDate = DateTime.UtcNow.Date.AddDays(20),
                IsActive = true
            },
            promptUsage: 0,
            postUsage: 0);
        var service = new QuotaService(repository, new FakeWorkspaceRepository(), new FakeProfileRepository(), new FakeContentRepository());

        await service.GetSummaryAsync(profileId);

        Assert.Equal(DateTime.UtcNow.Date, repository.LastPromptWindowStart);
        Assert.Equal(DateTime.UtcNow.Date, repository.LastPromptWindowEnd);
    }

    [Fact]
    public async Task GetWorkspaceSummaryAsync_UsesOnlyActiveWorkspaceUsage()
    {
        var workspaceId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var memberUserId = Guid.NewGuid();
        var ownerProfileId = Guid.NewGuid();
        var memberProfileId = Guid.NewGuid();
        var service = new QuotaService(
            new FakeSubscriptionRepository(
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    Plan = SubscriptionPlanEnum.Premium,
                    QuotaAIContentPerDay = 200,
                    StartDate = new DateTime(2026, 6, 1),
                    EndDate = new DateTime(2026, 6, 30),
                    IsActive = true
                },
                workspacePromptUsage: 5,
                workspacePostUsage: 15,
                useWorkspaceUsage: true),
            new FakeWorkspaceRepository(new Workspace
            {
                Id = workspaceId,
                WorkspaceType = WorkspaceTypeEnum.Business,
                Members =
                [
                    new WorkspaceMember { WorkspaceId = workspaceId, UserId = ownerUserId, IsActive = true },
                    new WorkspaceMember { WorkspaceId = workspaceId, UserId = memberUserId, IsActive = true }
                ]
            }),
            new FakeProfileRepository(
                new Profile { Id = ownerProfileId, UserId = ownerUserId, Name = "Owner", ProfileType = ProfileTypeEnum.Basic },
                new Profile { Id = memberProfileId, UserId = memberUserId, Name = "Member", ProfileType = ProfileTypeEnum.Basic }),
            new FakeContentRepository());

        var result = await service.GetWorkspaceSummaryAsync(workspaceId);

        Assert.True(result.Success);
        Assert.Equal(15, result.Data!.PostUsage);
        Assert.Equal(20_000, result.Data.PostQuotaLimit);
        Assert.Equal(19_985, result.Data.PostRemaining);
        Assert.Equal(5, result.Data.PromptUsage);
    }

    [Fact]
    public async Task EnsureWorkspacePostQuotaAsync_ReturnsForbidden_WhenWorkspacePostQuotaExceeded()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var service = new QuotaService(
            new FakeSubscriptionRepository(
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    Plan = SubscriptionPlanEnum.Free,
                    QuotaAIContentPerDay = 1,
                    StartDate = new DateTime(2026, 6, 1),
                    EndDate = new DateTime(2026, 6, 7),
                    IsActive = true
                },
                workspacePromptUsage: 0,
                workspacePostUsage: 20,
                useWorkspaceUsage: true),
            new FakeWorkspaceRepository(new Workspace
            {
                Id = workspaceId,
                WorkspaceType = WorkspaceTypeEnum.Personal,
                Members = [new WorkspaceMember { WorkspaceId = workspaceId, UserId = userId, IsActive = true }]
            }),
            new FakeProfileRepository(
                new Profile { Id = profileId, UserId = userId, Name = "Owner", ProfileType = ProfileTypeEnum.Basic }),
            new FakeContentRepository());

        var result = await service.EnsureWorkspacePostQuotaAsync(workspaceId);

        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("POST_QUOTA_EXCEEDED", result.Error?.ErrorCode);
    }

    private sealed class FakeSubscriptionRepository : ISubscriptionRepository
    {
        private readonly Subscription? _subscription;
        private readonly int _promptUsage;
        private readonly int _postUsage;
        private readonly IReadOnlyDictionary<Guid, int> _promptUsageByProfileId;
        private readonly IReadOnlyDictionary<Guid, int> _postUsageByProfileId;
        private readonly int _workspacePromptUsage;
        private readonly int _workspacePostUsage;
        public DateTime? LastPromptWindowStart { get; private set; }
        public DateTime? LastPromptWindowEnd { get; private set; }

        public FakeSubscriptionRepository(Subscription? subscription, int promptUsage, int postUsage)
        {
            _subscription = subscription;
            _promptUsage = promptUsage;
            _postUsage = postUsage;
            _promptUsageByProfileId = new Dictionary<Guid, int>();
            _postUsageByProfileId = new Dictionary<Guid, int>();
            _workspacePromptUsage = promptUsage;
            _workspacePostUsage = postUsage;
        }

        public FakeSubscriptionRepository(
            Subscription? subscription,
            int workspacePromptUsage,
            int workspacePostUsage,
            bool useWorkspaceUsage)
        {
            _subscription = subscription;
            _promptUsage = 0;
            _postUsage = 0;
            _promptUsageByProfileId = new Dictionary<Guid, int>();
            _postUsageByProfileId = new Dictionary<Guid, int>();
            _workspacePromptUsage = workspacePromptUsage;
            _workspacePostUsage = workspacePostUsage;
        }

        public Task<Subscription?> GetCurrentActiveByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_subscription?.ProfileId == profileId ? _subscription : null);
        }

        public Task<Subscription?> GetCurrentActiveByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_subscription?.WorkspaceId == workspaceId ? _subscription : null);
        }

        public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_subscription?.Id == id ? _subscription : null);
        }

        public Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountSuccessfulPromptUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
        {
            LastPromptWindowStart = windowStart;
            LastPromptWindowEnd = windowEnd;
            return Task.FromResult(_promptUsageByProfileId.TryGetValue(profileId, out var usage) ? usage : _promptUsage);
        }

        public Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_postUsageByProfileId.TryGetValue(profileId, out var usage) ? usage : _postUsage);
        }

        public Task<int> CountSuccessfulPromptUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
            => Task.FromResult(_workspacePromptUsage);

        public Task<int> CountSuccessfulPostUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
            => Task.FromResult(_workspacePostUsage);
    }

    private sealed class FakeWorkspaceRepository : IWorkspaceRepository
    {
        private readonly Dictionary<Guid, Workspace> _workspaces;

        public FakeWorkspaceRepository(params Workspace[] workspaces)
        {
            _workspaces = workspaces.ToDictionary(workspace => workspace.Id);
        }

        public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_workspaces.GetValueOrDefault(id));

        public Task<Workspace?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Workspace>>(_workspaces.Values.ToList());

        public Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_workspaces.ContainsKey(id));
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        private readonly List<Profile> _profiles;

        public FakeProfileRepository(params Profile[] profiles)
        {
            _profiles = profiles.ToList();
        }

        public Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_profiles.FirstOrDefault(profile => profile.Id == id));

        public Task<Profile?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<IEnumerable<Profile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_profiles.Where(profile => profile.UserId == userId).AsEnumerable());

        public Task<IEnumerable<Profile>> GetByUserIdIncludingDeletedAsync(Guid userId, bool isDeleted, CancellationToken cancellationToken = default)
            => GetByUserIdAsync(userId, cancellationToken);

        public Task<IEnumerable<Profile>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default)
            => GetByUserIdAsync(userId, cancellationToken);

        public Task<Profile> CreateAsync(Profile profile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Content>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Content content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
