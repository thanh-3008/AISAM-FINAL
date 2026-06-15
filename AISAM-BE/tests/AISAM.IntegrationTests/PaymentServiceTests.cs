using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
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

        var result = await service.CreateCheckoutAsync(Guid.NewGuid(), Guid.NewGuid(), new CreateCheckoutRequest
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
        var workspaceId = Guid.NewGuid();
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Plan = SubscriptionPlanEnum.Premium,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 30),
            IsActive = true
        };
        var service = CreateService(subscriptionRepository: new FakeSubscriptionRepository(subscription));

        var result = await service.GetCurrentSubscriptionAsync(workspaceId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(subscription.Id, result.Data!.SubscriptionId);
        Assert.Equal("Premium", result.Data.PlanName);
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_ReturnsWorkspacesPaymentsWithoutPayOsConfig()
    {
        var workspaceId = Guid.NewGuid();
        var service = CreateService(paymentRepository: new FakePaymentRepository(
            new Payment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                SubscriptionId = Guid.NewGuid(),
                Amount = 150_000m,
                PaymentMethod = "PayOS",
                Status = PaymentStatusEnum.Success,
                CreatedAt = new DateTime(2026, 6, 2, 8, 0, 0, DateTimeKind.Utc)
            }));

        var result = await service.GetPaymentHistoryAsync(workspaceId, new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.True(result.Success);
        Assert.Single(result.Data!.Data);
        Assert.Equal("PayOS", result.Data.Data[0].PaymentMethod);
    }

    [Fact]
    public async Task CreateCheckoutAsync_CreatesPendingPaymentAndReturnsPayOsCheckoutUrl()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var paymentRepository = new FakePaymentRepository();
        var subscriptionRepository = new FakeSubscriptionRepository();
        var service = CreateService(
            paymentRepository,
            subscriptionRepository,
            workspaceRepository: new FakeWorkspaceRepository(new Workspace
            {
                Id = workspaceId,
                Name = "Business Workspace",
                WorkspaceType = WorkspaceTypeEnum.Business
            }),
            settings: CreateConfiguredSettings(),
            httpClient: new HttpClient(new StubHttpMessageHandler("""
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

        var result = await service.CreateCheckoutAsync(workspaceId, userId, new CreateCheckoutRequest { PlanCode = "Plus" });

        Assert.True(result.Success);
        Assert.Equal("https://pay.payos.vn/web/mock", result.Data!.CheckoutUrl);
        Assert.Single(paymentRepository.Payments);
        Assert.Single(subscriptionRepository.Subscriptions);
        Assert.Equal(PaymentStatusEnum.Pending, paymentRepository.Payments[0].Status);
        Assert.Equal(workspaceId, paymentRepository.Payments[0].WorkspaceId);
        Assert.Equal(userId, paymentRepository.Payments[0].UserId);
        Assert.False(subscriptionRepository.Subscriptions[0].IsActive);
        Assert.Equal(workspaceId, subscriptionRepository.Subscriptions[0].WorkspaceId);
        Assert.Equal("plink_123", subscriptionRepository.Subscriptions[0].PayOSPaymentLinkId);
    }

    [Fact]
    public async Task CreateCheckoutAsync_CreatesCreditPackPaymentWithoutSubscription()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var paymentRepository = new FakePaymentRepository();
        var subscriptionRepository = new FakeSubscriptionRepository();
        var service = CreateService(
            paymentRepository,
            subscriptionRepository,
            workspaceRepository: new FakeWorkspaceRepository(new Workspace
            {
                Id = workspaceId,
                Name = "Business Workspace",
                WorkspaceType = WorkspaceTypeEnum.Business
            }),
            settings: CreateConfiguredSettings(),
            httpClient: new HttpClient(new StubHttpMessageHandler("""
            {
              "code": "00",
              "desc": "success",
              "data": {
                "checkoutUrl": "https://pay.payos.vn/web/credit-pack",
                "paymentLinkId": "plink_pack",
                "orderCode": "654321"
              }
            }
            """)));

        var result = await service.CreateCheckoutAsync(workspaceId, userId, new CreateCheckoutRequest
        {
            PaymentType = PaymentTypeEnum.CreditPack,
            CreditPackCode = CreditPackCodeEnum.Growth
        });

        Assert.True(result.Success);
        Assert.Single(paymentRepository.Payments);
        Assert.Empty(subscriptionRepository.Subscriptions);
        Assert.Equal(PaymentTypeEnum.CreditPack, paymentRepository.Payments[0].PaymentType);
        Assert.Equal(CreditPackCodeEnum.Growth, paymentRepository.Payments[0].CreditPackCode);
        Assert.Equal(1_500, paymentRepository.Payments[0].CreditAmount);
    }

    [Fact]
    public async Task HandleWebhookAsync_MarksPaymentSuccessAndActivatesSubscription()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workspace = new Workspace
        {
            Id = workspaceId,
            Name = "Business Workspace",
            WorkspaceType = WorkspaceTypeEnum.Business,
            MemberLimit = 10
        };
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Plan = SubscriptionPlanEnum.Premium,
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
            WorkspaceId = workspaceId,
            SubscriptionId = subscription.Id,
            Amount = 99_000m,
            Status = PaymentStatusEnum.Pending,
            PaymentMethod = "PayOS",
            TransactionId = "987654",
            Subscription = subscription
        };
        var creditService = new FakeCreditService();
        var service = CreateService(
            new FakePaymentRepository(payment),
            new FakeSubscriptionRepository(subscription),
            settings: CreateConfiguredSettings(),
            workspaceRepository: new FakeWorkspaceRepository(workspace),
            creditService: creditService);

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
        Assert.Equal(subscription.EndDate, workspace.SubscriptionExpiredAt);
        Assert.Equal(50, workspace.MemberLimit);
        Assert.Single(creditService.Wallets);
        Assert.Equal(50_000, creditService.Wallets[workspaceId].Balance);
    }

    [Fact]
    public async Task HandleWebhookAsync_RenewsFromCurrentWorkspaceExpiryAndDeactivatesPreviousSubscription()
    {
        var workspaceId = Guid.NewGuid();
        var currentEndDate = DateTime.UtcNow.Date.AddDays(12);
        var workspace = new Workspace
        {
            Id = workspaceId,
            Name = "Business Workspace",
            WorkspaceType = WorkspaceTypeEnum.Business,
            MemberLimit = 50,
            SubscriptionExpiredAt = currentEndDate,
            Status = WorkspaceStatusEnum.Archived,
            ArchivedAt = DateTime.UtcNow.Date.AddDays(-1)
        };
        var currentSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Plan = SubscriptionPlanEnum.Premium,
            StartDate = DateTime.UtcNow.Date.AddDays(-18),
            EndDate = currentEndDate,
            IsActive = true
        };
        var renewalSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Plan = SubscriptionPlanEnum.Plus,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(30),
            IsActive = false,
            PayOSOrderCode = "renewal-123"
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            SubscriptionId = renewalSubscription.Id,
            Amount = 99_000m,
            Status = PaymentStatusEnum.Pending,
            PaymentMethod = "PayOS",
            TransactionId = "renewal-123",
            Subscription = renewalSubscription
        };
        var creditService = new FakeCreditService();
        var service = CreateService(
            new FakePaymentRepository(payment),
            new FakeSubscriptionRepository(currentSubscription, renewalSubscription),
            workspaceRepository: new FakeWorkspaceRepository(workspace),
            settings: CreateConfiguredSettings(),
            creditService: creditService);

        var signature = CreateSignature(new Dictionary<string, string>
        {
            ["orderCode"] = "renewal-123",
            ["status"] = "PAID"
        });
        var result = await service.HandleWebhookAsync($$"""
        {
          "signature": "{{signature}}",
          "data": {
            "orderCode": "renewal-123",
            "status": "PAID"
          }
        }
        """);

        Assert.True(result.Success);
        Assert.False(currentSubscription.IsActive);
        Assert.True(renewalSubscription.IsActive);
        Assert.Equal(currentEndDate.AddDays(30), renewalSubscription.EndDate);
        Assert.Equal(renewalSubscription.EndDate, workspace.SubscriptionExpiredAt);
        Assert.Equal(10, workspace.MemberLimit);
        Assert.Equal(WorkspaceStatusEnum.Active, workspace.Status);
        Assert.Null(workspace.ArchivedAt);
        Assert.Equal(15_000, creditService.Wallets[workspaceId].Balance);
    }

    [Fact]
    public async Task HandleWebhookAsync_CreditPackAddsCreditsWithoutChangingSubscription()
    {
        var workspaceId = Guid.NewGuid();
        var currentEndDate = DateTime.UtcNow.Date.AddDays(20);
        var workspace = new Workspace
        {
            Id = workspaceId,
            Name = "Business Workspace",
            WorkspaceType = WorkspaceTypeEnum.Business,
            MemberLimit = 10,
            SubscriptionExpiredAt = currentEndDate
        };
        var activeSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Plan = SubscriptionPlanEnum.Plus,
            StartDate = DateTime.UtcNow.Date.AddDays(-10),
            EndDate = currentEndDate,
            IsActive = true
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Amount = 249_000m,
            Status = PaymentStatusEnum.Pending,
            PaymentMethod = "PayOS",
            PaymentType = PaymentTypeEnum.CreditPack,
            CreditPackCode = CreditPackCodeEnum.Growth,
            CreditAmount = 1_500,
            TransactionId = "pack-123"
        };
        var creditService = new FakeCreditService();

        var service = CreateService(
            new FakePaymentRepository(payment),
            new FakeSubscriptionRepository(activeSubscription),
            workspaceRepository: new FakeWorkspaceRepository(workspace),
            settings: CreateConfiguredSettings(),
            creditService: creditService);

        var signature = CreateSignature(new Dictionary<string, string>
        {
            ["orderCode"] = "pack-123",
            ["status"] = "PAID"
        });
        var result = await service.HandleWebhookAsync($$"""
        {
          "signature": "{{signature}}",
          "data": {
            "orderCode": "pack-123",
            "status": "PAID"
          }
        }
        """);

        Assert.True(result.Success);
        Assert.Equal(PaymentStatusEnum.Success, payment.Status);
        Assert.True(activeSubscription.IsActive);
        Assert.Equal(currentEndDate, activeSubscription.EndDate);
        Assert.Equal(currentEndDate, workspace.SubscriptionExpiredAt);
        Assert.Equal(1_500, creditService.Wallets[workspaceId].Balance);
    }

    [Fact]
    public async Task HandleWebhookAsync_RejectsCreditGrantThatWouldExceedWorkspaceMaximumBalance()
    {
        var workspaceId = Guid.NewGuid();
        var workspace = new Workspace
        {
            Id = workspaceId,
            Name = "Personal Workspace",
            WorkspaceType = WorkspaceTypeEnum.Personal,
            MemberLimit = 1
        };
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Plan = SubscriptionPlanEnum.Premium,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(30),
            IsActive = false,
            PayOSOrderCode = "overflow-123"
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            SubscriptionId = subscription.Id,
            Amount = 199_000m,
            Status = PaymentStatusEnum.Pending,
            PaymentMethod = "PayOS",
            TransactionId = "overflow-123",
            Subscription = subscription
        };
        var creditService = new FakeCreditService();
        creditService.Wallets[workspaceId] = new CreditWallet
        {
            WorkspaceId = workspaceId,
            Balance = 14_000
        };

        var service = CreateService(
            new FakePaymentRepository(payment),
            new FakeSubscriptionRepository(subscription),
            workspaceRepository: new FakeWorkspaceRepository(workspace),
            settings: CreateConfiguredSettings(),
            creditService: creditService);

        var signature = CreateSignature(new Dictionary<string, string>
        {
            ["orderCode"] = "overflow-123",
            ["status"] = "PAID"
        });
        var result = await service.HandleWebhookAsync($$"""
        {
          "signature": "{{signature}}",
          "data": {
            "orderCode": "overflow-123",
            "status": "PAID"
          }
        }
        """);

        Assert.False(result.Success);
        Assert.Equal(PaymentStatusEnum.Failed, payment.Status);
        Assert.False(subscription.IsActive);
        Assert.Equal(14_000, creditService.Wallets[workspaceId].Balance);
    }

    [Fact]
    public async Task HandleWebhookAsync_RejectsCreditPackThatWouldExceedWorkspaceMaximumBalance()
    {
        var workspaceId = Guid.NewGuid();
        var workspace = new Workspace
        {
            Id = workspaceId,
            Name = "Personal Workspace",
            WorkspaceType = WorkspaceTypeEnum.Personal,
            MemberLimit = 1
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Amount = 699_000m,
            Status = PaymentStatusEnum.Pending,
            PaymentMethod = "PayOS",
            PaymentType = PaymentTypeEnum.CreditPack,
            CreditPackCode = CreditPackCodeEnum.Business,
            CreditAmount = 5_000,
            TransactionId = "pack-overflow"
        };
        var creditService = new FakeCreditService();
        creditService.Wallets[workspaceId] = new CreditWallet
        {
            WorkspaceId = workspaceId,
            Balance = 14_000
        };

        var service = CreateService(
            new FakePaymentRepository(payment),
            new FakeSubscriptionRepository(),
            workspaceRepository: new FakeWorkspaceRepository(workspace),
            settings: CreateConfiguredSettings(),
            creditService: creditService);

        var signature = CreateSignature(new Dictionary<string, string>
        {
            ["orderCode"] = "pack-overflow",
            ["status"] = "PAID"
        });
        var result = await service.HandleWebhookAsync($$"""
        {
          "signature": "{{signature}}",
          "data": {
            "orderCode": "pack-overflow",
            "status": "PAID"
          }
        }
        """);

        Assert.False(result.Success);
        Assert.Equal(PaymentStatusEnum.Failed, payment.Status);
        Assert.Equal(14_000, creditService.Wallets[workspaceId].Balance);
    }

    [Fact]
    public async Task HandleWebhookAsync_IsIdempotent_WhenSubscriptionIsAlreadyActive()
    {
        var workspaceId = Guid.NewGuid();
        var existingEndDate = DateTime.UtcNow.Date.AddDays(30);
        var workspace = new Workspace
        {
            Id = workspaceId,
            Name = "Business Workspace",
            WorkspaceType = WorkspaceTypeEnum.Business,
            MemberLimit = 10,
            SubscriptionExpiredAt = existingEndDate
        };
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Plan = SubscriptionPlanEnum.Plus,
            StartDate = DateTime.UtcNow.Date,
            EndDate = existingEndDate,
            IsActive = true,
            PayOSOrderCode = "paid-123"
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            SubscriptionId = subscription.Id,
            Amount = 99_000m,
            Status = PaymentStatusEnum.Success,
            PaymentMethod = "PayOS",
            TransactionId = "paid-123",
            Subscription = subscription
        };
        var service = CreateService(
            new FakePaymentRepository(payment),
            new FakeSubscriptionRepository(subscription),
            workspaceRepository: new FakeWorkspaceRepository(workspace),
            settings: CreateConfiguredSettings());

        var signature = CreateSignature(new Dictionary<string, string>
        {
            ["orderCode"] = "paid-123",
            ["status"] = "PAID"
        });
        var result = await service.HandleWebhookAsync($$"""
        {
          "signature": "{{signature}}",
          "data": {
            "orderCode": "paid-123",
            "status": "PAID"
          }
        }
        """);

        Assert.True(result.Success);
        Assert.Equal(existingEndDate, subscription.EndDate);
        Assert.Equal(existingEndDate, workspace.SubscriptionExpiredAt);
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
    public async Task HandleWebhookAsync_AcceptsPayOsSignature_WhenSignedDataContainsNullValue()
    {
        var signature = CreateSignature(new Dictionary<string, string>
        {
            ["code"] = "00",
            ["counterAccountName"] = string.Empty,
            ["orderCode"] = "987654",
            ["paymentLinkId"] = "plink_987",
            ["reference"] = "txn_987"
        });
        var service = CreateService(settings: CreateConfiguredSettings());

        var result = await service.HandleWebhookAsync($$"""
        {
          "code": "00",
          "desc": "success",
          "success": true,
          "data": {
            "code": "00",
            "counterAccountName": null,
            "orderCode": 987654,
            "paymentLinkId": "plink_987",
            "reference": "txn_987"
          },
          "signature": "{{signature}}"
        }
        """);

        Assert.True(result.Success);
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
        FakeWorkspaceRepository? workspaceRepository = null,
        FakeCreditService? creditService = null,
        PayOSSettings? settings = null,
        HttpClient? httpClient = null)
    {
        return new PayOSPaymentService(
            paymentRepository ?? new FakePaymentRepository(),
            subscriptionRepository ?? new FakeSubscriptionRepository(),
            profileRepository ?? new FakeProfileRepository(),
            workspaceRepository ?? new FakeWorkspaceRepository(),
            creditService ?? new FakeCreditService(),
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

        public Task<PagedResult<Payment>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var payments = _payments.Values
                .Where(payment => payment.WorkspaceId == workspaceId)
                .OrderByDescending(payment => payment.CreatedAt)
                .ToList();
            return Task.FromResult(new PagedResult<Payment>
            {
                Data = payments,
                TotalCount = payments.Count,
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

        public Task<Subscription?> GetCurrentActiveByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            var subscription = _subscriptions.Values
                .Where(item => item.WorkspaceId == workspaceId && item.IsActive && !item.IsDeleted)
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

        public Task<int> CountSuccessfulPromptUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<int> CountSuccessfulPostUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
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

    private sealed class FakeWorkspaceRepository : IWorkspaceRepository
    {
        private readonly Dictionary<Guid, Workspace> _workspaces;

        public FakeWorkspaceRepository(params Workspace[] workspaces)
        {
            _workspaces = workspaces.ToDictionary(workspace => workspace.Id);
        }

        public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _workspaces.TryGetValue(id, out var workspace);
            return Task.FromResult(workspace);
        }

        public Task<Workspace?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) =>
            GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Workspace>>(_workspaces.Values.ToList());

        public Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
        {
            _workspaces[workspace.Id] = workspace;
            return Task.FromResult(workspace);
        }

        public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
        {
            _workspaces[workspace.Id] = workspace;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_workspaces.ContainsKey(id));
    }

    private sealed class FakeCreditService : ICreditService
    {
        public Dictionary<Guid, CreditWallet> Wallets { get; } = [];

        public Task<CreditWallet> EnsureWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            if (!Wallets.TryGetValue(workspaceId, out var wallet))
            {
                wallet = new CreditWallet
                {
                    WorkspaceId = workspaceId,
                    Balance = 0
                };
                Wallets[workspaceId] = wallet;
            }

            return Task.FromResult(wallet);
        }

        public Task<GenericResponse<CreditWallet>> GrantSubscriptionCreditsAsync(
            Guid workspaceId,
            Guid userId,
            WorkspaceTypeEnum workspaceType,
            SubscriptionPlanEnum plan,
            CancellationToken cancellationToken = default)
        {
            var wallet = Wallets.TryGetValue(workspaceId, out var existing)
                ? existing
                : new CreditWallet
                {
                    WorkspaceId = workspaceId,
                    Balance = 0
                };

            var increment = (workspaceType, plan) switch
            {
                (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Free) => 50L,
                (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Plus) => 500L,
                (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Premium) => 2_000L,
                (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Plus) => 15_000L,
                (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Premium) => 50_000L,
                _ => 0L
            };

            var maximum = workspaceType == WorkspaceTypeEnum.Business ? 500_000L : 15_000L;
            if (wallet.Balance + increment > maximum)
            {
                return Task.FromResult(GenericResponse<CreditWallet>.CreateError("Wallet balance exceeds workspace maximum balance.", HttpStatusCode.BadRequest, "CREDIT_BALANCE_LIMIT_EXCEEDED"));
            }

            wallet.Balance += increment;
            Wallets[workspaceId] = wallet;
            return Task.FromResult(GenericResponse<CreditWallet>.CreateSuccess(wallet));
        }

        public Task<GenericResponse<CreditWallet>> GrantCreditPackCreditsAsync(
            Guid workspaceId,
            Guid userId,
            WorkspaceTypeEnum workspaceType,
            long credits,
            CancellationToken cancellationToken = default)
        {
            var wallet = Wallets.TryGetValue(workspaceId, out var existing)
                ? existing
                : new CreditWallet
                {
                    WorkspaceId = workspaceId,
                    Balance = 0
                };

            var maximum = workspaceType == WorkspaceTypeEnum.Business ? 500_000L : 15_000L;
            if (wallet.Balance + credits > maximum)
            {
                return Task.FromResult(GenericResponse<CreditWallet>.CreateError("Wallet balance exceeds workspace maximum balance.", HttpStatusCode.BadRequest, "CREDIT_BALANCE_LIMIT_EXCEEDED"));
            }

            wallet.Balance += credits;
            Wallets[workspaceId] = wallet;
            return Task.FromResult(GenericResponse<CreditWallet>.CreateSuccess(wallet));
        }

        public Task<GenericResponse<CreditUsageRecord>> RecordUsageAsync(
            Guid workspaceId,
            Guid userId,
            CreditActionEnum action,
            long credits,
            CreditUsageStatusEnum status,
            Guid? aiGenerationId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<CreditUsageRecord>.CreateSuccess(new CreditUsageRecord
            {
                WorkspaceId = workspaceId,
                UserId = userId,
                Action = action,
                Credits = credits,
                Status = status,
                AiGenerationId = aiGenerationId
            }));
        }

        public Task<GenericResponse<CreditUsageRecord>> ConsumeCreditsAsync(
            Guid workspaceId,
            Guid userId,
            CreditActionEnum action,
            long credits,
            Guid? aiGenerationId = null,
            DateTime? now = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<CreditUsageRecord>.CreateSuccess(new CreditUsageRecord
            {
                WorkspaceId = workspaceId,
                UserId = userId,
                Action = action,
                Credits = credits,
                Status = CreditUsageStatusEnum.Success,
                AiGenerationId = aiGenerationId
            }));
        }

        public Task<GenericResponse<bool>> EnsureCreditsAvailableAsync(
            Guid workspaceId,
            Guid userId,
            long credits,
            DateTime? now = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
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
