using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using Microsoft.Extensions.Options;

namespace AISAM.IntegrationTests;

public class PaymentServiceTests
{
    [Fact]
    public async Task CreateCheckoutAsync_ReturnsSafeError_WhenPayOsConfigMissing()
    {
        var service = CreateService();

        var result = await service.CreateCheckoutAsync(Guid.NewGuid(), new CreateCheckoutRequest
        {
            PlanCode = "Plus"
        });

        Assert.False(result.Success);
        Assert.Equal(503, result.StatusCode);
        Assert.Equal("PAYOS_NOT_CONFIGURED", result.Error?.ErrorCode);
    }

    [Fact]
    public async Task GetCurrentSubscriptionAsync_DoesNotRequirePayOsOutboundConfig()
    {
        var profileId = Guid.NewGuid();
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Plan = SubscriptionPlanEnum.Premium,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 30),
            IsActive = true
        };
        var service = CreateService(subscriptionRepository: new FakeSubscriptionRepository(subscription));

        var result = await service.GetCurrentSubscriptionAsync(profileId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(subscription.Id, result.Data!.SubscriptionId);
        Assert.Equal("Premium", result.Data.PlanName);
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_ReturnsProfilesPaymentsWithoutPayOsConfig()
    {
        var profileId = Guid.NewGuid();
        var service = CreateService(paymentRepository: new FakePaymentRepository(
            new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SubscriptionId = Guid.NewGuid(),
                Amount = 150_000m,
                PaymentMethod = "PayOS",
                Status = PaymentStatusEnum.Success,
                CreatedAt = new DateTime(2026, 6, 2, 8, 0, 0, DateTimeKind.Utc)
            }));

        var result = await service.GetPaymentHistoryAsync(profileId, new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.True(result.Success);
        Assert.Single(result.Data!.Data);
        Assert.Equal("PayOS", result.Data.Data[0].PaymentMethod);
    }

    private static PayOSPaymentService CreateService(
        FakePaymentRepository? paymentRepository = null,
        FakeSubscriptionRepository? subscriptionRepository = null,
        PayOSSettings? settings = null)
    {
        return new PayOSPaymentService(
            paymentRepository ?? new FakePaymentRepository(),
            subscriptionRepository ?? new FakeSubscriptionRepository(),
            Options.Create(settings ?? new PayOSSettings()),
            new HttpClient());
    }

    private sealed class FakePaymentRepository : IPaymentRepository
    {
        private readonly Dictionary<Guid, Payment> _payments;

        public FakePaymentRepository(params Payment[] payments)
        {
            _payments = payments.ToDictionary(payment => payment.Id);
        }

        public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _payments.TryGetValue(id, out var payment);
            return Task.FromResult(payment);
        }

        public Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            var payment = _payments.Values.FirstOrDefault(item => item.TransactionId == reference);
            return Task.FromResult(payment);
        }

        public Task<PagedResult<Payment>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<Payment>
            {
                Data = _payments.Values.OrderByDescending(payment => payment.CreatedAt).ToList(),
                TotalCount = _payments.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _payments[payment.Id] = payment;
            return Task.FromResult(payment);
        }

        public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _payments[payment.Id] = payment;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSubscriptionRepository : ISubscriptionRepository
    {
        private readonly Dictionary<Guid, Subscription> _subscriptions;

        public FakeSubscriptionRepository(params Subscription[] subscriptions)
        {
            _subscriptions = subscriptions.ToDictionary(subscription => subscription.Id);
        }

        public Task<Subscription?> GetCurrentActiveByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            var subscription = _subscriptions.Values
                .Where(item => item.ProfileId == profileId && item.IsActive && !item.IsDeleted)
                .OrderByDescending(item => item.StartDate)
                .FirstOrDefault();
            return Task.FromResult(subscription);
        }

        public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _subscriptions.TryGetValue(id, out var subscription);
            return Task.FromResult(subscription);
        }

        public Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            _subscriptions[subscription.Id] = subscription;
            return Task.FromResult(subscription);
        }

        public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            _subscriptions[subscription.Id] = subscription;
            return Task.CompletedTask;
        }

        public Task<int> CountSuccessfulPromptUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}
