using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

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

    [Fact]
    public async Task CreateCheckoutAsync_CreatesPendingPaymentAndReturnsPayOsCheckoutUrl()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var paymentRepository = new FakePaymentRepository();
        var subscriptionRepository = new FakeSubscriptionRepository();
        var service = CreateService(
            paymentRepository,
            subscriptionRepository,
            new FakeProfileRepository(new Profile
            {
                Id = profileId,
                UserId = userId,
                Name = "Owner",
                ProfileType = ProfileTypeEnum.Basic,
                Status = ProfileStatusEnum.Active
            }),
            CreateConfiguredSettings(),
            new HttpClient(new StubHttpMessageHandler("""
            {
              "code": "00",
              "desc": "success",
              "data": {
                "checkoutUrl": "https://pay.payos.vn/web/mock",
                "paymentLinkId": "plink_123",
                "orderCode": "123456"
              }
            }
            """)));

        var result = await service.CreateCheckoutAsync(profileId, new CreateCheckoutRequest { PlanCode = "Plus" });

        Assert.True(result.Success);
        Assert.Equal("https://pay.payos.vn/web/mock", result.Data!.CheckoutUrl);
        Assert.Single(paymentRepository.Payments);
        Assert.Single(subscriptionRepository.Subscriptions);
        Assert.Equal(PaymentStatusEnum.Pending, paymentRepository.Payments[0].Status);
        Assert.False(subscriptionRepository.Subscriptions[0].IsActive);
        Assert.Equal("plink_123", subscriptionRepository.Subscriptions[0].PayOSPaymentLinkId);
    }

    [Fact]
    public async Task HandleWebhookAsync_MarksPaymentSuccessAndActivatesSubscription()
    {
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Plan = SubscriptionPlanEnum.Plus,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 30),
            IsActive = false,
            PayOSOrderCode = "987654",
            PayOSPaymentLinkId = "plink_987"
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubscriptionId = subscription.Id,
            Amount = 99_000m,
            Status = PaymentStatusEnum.Pending,
            PaymentMethod = "PayOS",
            TransactionId = "987654",
            Subscription = subscription
        };
        var profile = new Profile
        {
            Id = profileId,
            UserId = userId,
            Name = "Owner",
            ProfileType = ProfileTypeEnum.Basic,
            Status = ProfileStatusEnum.Active
        };
        var service = CreateService(
            new FakePaymentRepository(payment),
            new FakeSubscriptionRepository(subscription),
            new FakeProfileRepository(profile),
            CreateConfiguredSettings());

        var signature = CreateSignature(new Dictionary<string, string>
        {
            ["orderCode"] = "987654",
            ["paymentLinkId"] = "plink_987",
            ["status"] = "PAID",
            ["reference"] = "txn_987"
        });
        var result = await service.HandleWebhookAsync($$"""
        {
          "code": "00",
          "desc": "success",
          "signature": "{{signature}}",
          "data": {
            "orderCode": "987654",
            "paymentLinkId": "plink_987",
            "status": "PAID",
            "reference": "txn_987"
          }
        }
        """);

        Assert.True(result.Success);
        Assert.Equal(PaymentStatusEnum.Success, payment.Status);
        Assert.True(subscription.IsActive);
        Assert.Equal(subscription.Id, profile.SubscriptionId);
    }

    [Fact]
    public async Task HandleWebhookAsync_RejectsPayloadWithoutSignature()
    {
        var service = CreateService(
            settings: CreateConfiguredSettings(),
            httpClient: new HttpClient());

        var result = await service.HandleWebhookAsync("""
        {
          "code": "00",
          "desc": "success",
          "success": true,
          "data": {
            "orderCode": 123,
            "status": "PAID",
            "reference": "verification"
          }
        }
        """);

        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("PAYOS_SIGNATURE_REQUIRED", result.Error?.ErrorCode);
    }

    [Fact]
    public async Task HandleCallbackAsync_RejectsQueryWithoutSignature()
    {
        var service = CreateService(settings: CreateConfiguredSettings());
        var query = new Microsoft.AspNetCore.Http.QueryCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["orderCode"] = "123",
                ["status"] = "PAID"
            });

        var result = await service.HandleCallbackAsync(query);

        Assert.False(result.Success);
        Assert.Equal("PAYOS_SIGNATURE_REQUIRED", result.Error?.ErrorCode);
    }

    private static PayOSPaymentService CreateService(
        FakePaymentRepository? paymentRepository = null,
        FakeSubscriptionRepository? subscriptionRepository = null,
        FakeProfileRepository? profileRepository = null,
        PayOSSettings? settings = null,
        HttpClient? httpClient = null)
    {
        return new PayOSPaymentService(
            paymentRepository ?? new FakePaymentRepository(),
            subscriptionRepository ?? new FakeSubscriptionRepository(),
            profileRepository ?? new FakeProfileRepository(),
            Options.Create(settings ?? new PayOSSettings()),
            httpClient ?? new HttpClient());
    }

    private static PayOSSettings CreateConfiguredSettings()
    {
        return new PayOSSettings
        {
            ClientId = "client-id",
            ApiKey = "api-key",
            ChecksumKey = "checksum-key",
            BaseUrl = "https://payos.test",
            ReturnUrl = "https://app.test/payment/success",
            CancelUrl = "https://app.test/payment/cancel"
        };
    }

    private static string CreateSignature(IReadOnlyDictionary<string, string> values)
    {
        var data = string.Join("&", values.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => $"{item.Key}={item.Value}"));
        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("checksum-key"), Encoding.UTF8.GetBytes(data)))
            .ToLowerInvariant();
    }

    private sealed class FakePaymentRepository : IPaymentRepository
    {
        private readonly Dictionary<Guid, Payment> _payments;
        public List<Payment> Payments => _payments.Values.ToList();

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
            var payment = _payments.Values.FirstOrDefault(item =>
                item.TransactionId == reference ||
                item.Subscription?.PayOSOrderCode == reference ||
                item.Subscription?.PayOSPaymentLinkId == reference);
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
        public List<Subscription> Subscriptions => _subscriptions.Values.ToList();

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

    private sealed class FakeProfileRepository : IProfileRepository
    {
        private readonly Dictionary<Guid, Profile> _profiles;

        public FakeProfileRepository(params Profile[] profiles)
        {
            _profiles = profiles.ToDictionary(profile => profile.Id);
        }

        public Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _profiles.TryGetValue(id, out var profile);
            return Task.FromResult(profile);
        }

        public Task<Profile?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<IEnumerable<Profile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(_profiles.Values.Where(profile => profile.UserId == userId).AsEnumerable());
        public Task<IEnumerable<Profile>> GetByUserIdIncludingDeletedAsync(Guid userId, bool isDeleted, CancellationToken cancellationToken = default) => GetByUserIdAsync(userId, cancellationToken);
        public Task<IEnumerable<Profile>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default) => GetByUserIdAsync(userId, cancellationToken);
        public Task<Profile> CreateAsync(Profile profile, CancellationToken cancellationToken = default)
        {
            _profiles[profile.Id] = profile;
            return Task.FromResult(profile);
        }

        public Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default)
        {
            _profiles[profile.Id] = profile;
            return Task.FromResult(profile);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_profiles.Remove(id));
        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_profiles.ContainsKey(id));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseBody;

        public StubHttpMessageHandler(string responseBody)
        {
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            });
        }
    }
}
