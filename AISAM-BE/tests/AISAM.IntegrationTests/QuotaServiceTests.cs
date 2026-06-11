using AISAM.Common;
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
            postUsage: 2));

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
            postUsage: 0));

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
            postUsage: 1));

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
        var service = new QuotaService(repository);

        await service.GetSummaryAsync(profileId);

        Assert.Equal(DateTime.UtcNow.Date, repository.LastPromptWindowStart);
        Assert.Equal(DateTime.UtcNow.Date, repository.LastPromptWindowEnd);
    }

    private sealed class FakeSubscriptionRepository : ISubscriptionRepository
    {
        private readonly Subscription? _subscription;
        private readonly int _promptUsage;
        private readonly int _postUsage;
        public DateTime? LastPromptWindowStart { get; private set; }
        public DateTime? LastPromptWindowEnd { get; private set; }

        public FakeSubscriptionRepository(Subscription? subscription, int promptUsage, int postUsage)
        {
            _subscription = subscription;
            _promptUsage = promptUsage;
            _postUsage = postUsage;
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
            return Task.FromResult(_promptUsage);
        }

        public Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_postUsage);
        }
    }
}
