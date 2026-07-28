using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class PaymentControllerTests
{
    [Fact]
    public async Task GetCurrentSubscription_ReturnsOnlyActiveWorkspacesSubscriptionAsync()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakePaymentService
        {
            CurrentSubscriptionResult = GenericResponse<CurrentSubscriptionDto>.CreateSuccess(new CurrentSubscriptionDto())
        };
        var controller = CreateController(service, workspaceId);

        await controller.GetCurrentSubscription();

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    [Fact]
    public async Task GetPaymentHistory_ReturnsWorkspacesPaymentsAsync()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakePaymentService
        {
            PaymentHistoryResult = GenericResponse<PagedResult<PaymentHistoryItemDto>>.CreateSuccess(new PagedResult<PaymentHistoryItemDto>())
        };
        var controller = CreateController(service, workspaceId);

        await controller.GetHistory(new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    [Fact]
    public async Task CreateCheckout_ReturnsServiceUnavailable_WhenPayOsConfigMissingAsync()
    {
        var service = new FakePaymentService
        {
            CheckoutResult = GenericResponse<PayOSCheckoutResponse>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED")
        };
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var controller = CreateController(service, workspaceId, userId);

        var result = await controller.CreateCheckout(new CreateCheckoutRequest { PlanCode = "Plus" });

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, objectResult.StatusCode);
        Assert.Equal(workspaceId, service.LastWorkspaceId);
        Assert.Equal(userId, service.LastUserId);
    }

    [Fact]
    public async Task CreateCheckout_ForCreditPack_ForwardsPaymentTypeAndPackCodeAsync()
    {
        var service = new FakePaymentService();
        var workspaceId = Guid.NewGuid();
        var controller = CreateController(service, workspaceId, Guid.NewGuid());

        await controller.CreateCheckout(new CreateCheckoutRequest
        {
            PaymentType = PaymentTypeEnum.CreditPack,
            CreditPackCode = CreditPackCodeEnum.Growth
        });

        Assert.Equal(PaymentTypeEnum.CreditPack, service.LastRequest!.PaymentType);
        Assert.Equal(CreditPackCodeEnum.Growth, service.LastRequest.CreditPackCode);
    }

    private static PaymentController CreateController(IPaymentService service, Guid workspaceId, Guid? userId = null)
    {
        var context = new DefaultHttpContext();
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = userId ?? Guid.NewGuid(),
            Role = WorkspaceMemberRoleEnum.Owner
        };

        return new PaymentController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakePaymentService : IPaymentService
    {
        public Guid LastWorkspaceId { get; private set; }
        public Guid LastUserId { get; private set; }
        public CreateCheckoutRequest? LastRequest { get; private set; }
        public GenericResponse<PayOSCheckoutResponse> CheckoutResult { get; set; } = GenericResponse<PayOSCheckoutResponse>.CreateSuccess(new PayOSCheckoutResponse());
        public GenericResponse<bool> CallbackResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<bool> WebhookResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<PagedResult<PaymentHistoryItemDto>> PaymentHistoryResult { get; set; } = GenericResponse<PagedResult<PaymentHistoryItemDto>>.CreateSuccess(new PagedResult<PaymentHistoryItemDto>());
        public GenericResponse<CurrentSubscriptionDto> CurrentSubscriptionResult { get; set; } = GenericResponse<CurrentSubscriptionDto>.CreateSuccess(new CurrentSubscriptionDto());

        public Task<GenericResponse<PayOSCheckoutResponse>> CreateCheckoutAsync(Guid workspaceId, Guid userId, CreateCheckoutRequest request, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            LastUserId = userId;
            LastRequest = request;
            return Task.FromResult(CheckoutResult);
        }

        public Task<GenericResponse<PayOSCheckoutResponse>> CreateBusinessWorkspaceCheckoutAsync(Guid userId, CreateBusinessWorkspaceCheckoutRequest request, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult(CheckoutResult);
        }

        public Task<GenericResponse<bool>> SynchronizeBusinessWorkspaceCheckoutAsync(Guid userId, string reference, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult(CallbackResult);
        }

        public Task<GenericResponse<bool>> HandleCallbackAsync(IQueryCollection query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CallbackResult);
        }

        public Task<GenericResponse<bool>> HandleWebhookAsync(string rawPayload, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(WebhookResult);
        }

        public Task<GenericResponse<PagedResult<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(PaymentHistoryResult);
        }

        public Task<GenericResponse<CurrentSubscriptionDto>> GetCurrentSubscriptionAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(CurrentSubscriptionResult);
        }
    }
}
