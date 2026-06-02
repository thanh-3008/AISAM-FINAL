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

public class PaymentControllerTests
{
    [Fact]
    public async Task GetCurrentSubscription_ReturnsOnlyActiveProfilesSubscription()
    {
        var profileId = Guid.NewGuid();
        var service = new FakePaymentService
        {
            CurrentSubscriptionResult = GenericResponse<CurrentSubscriptionDto>.CreateSuccess(new CurrentSubscriptionDto())
        };
        var controller = CreateController(service, profileId);

        await controller.GetCurrentSubscription();

        Assert.Equal(profileId, service.LastProfileId);
    }

    [Fact]
    public async Task GetPaymentHistory_ReturnsProfilesPayments()
    {
        var profileId = Guid.NewGuid();
        var service = new FakePaymentService
        {
            PaymentHistoryResult = GenericResponse<PagedResult<PaymentHistoryItemDto>>.CreateSuccess(new PagedResult<PaymentHistoryItemDto>())
        };
        var controller = CreateController(service, profileId);

        await controller.GetHistory(new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.Equal(profileId, service.LastProfileId);
    }

    [Fact]
    public async Task CreateCheckout_ReturnsServiceUnavailable_WhenPayOsConfigMissing()
    {
        var service = new FakePaymentService
        {
            CheckoutResult = GenericResponse<PayOSCheckoutResponse>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED")
        };
        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.CreateCheckout(new CreateCheckoutRequest { PlanCode = "Plus" });

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, objectResult.StatusCode);
    }

    private static PaymentController CreateController(IPaymentService service, Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;

        return new PaymentController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakePaymentService : IPaymentService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<PayOSCheckoutResponse> CheckoutResult { get; set; } = GenericResponse<PayOSCheckoutResponse>.CreateSuccess(new PayOSCheckoutResponse());
        public GenericResponse<bool> CallbackResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<bool> WebhookResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<PagedResult<PaymentHistoryItemDto>> PaymentHistoryResult { get; set; } = GenericResponse<PagedResult<PaymentHistoryItemDto>>.CreateSuccess(new PagedResult<PaymentHistoryItemDto>());
        public GenericResponse<CurrentSubscriptionDto> CurrentSubscriptionResult { get; set; } = GenericResponse<CurrentSubscriptionDto>.CreateSuccess(new CurrentSubscriptionDto());

        public Task<GenericResponse<PayOSCheckoutResponse>> CreateCheckoutAsync(Guid profileId, CreateCheckoutRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(CheckoutResult);
        }

        public Task<GenericResponse<bool>> HandleCallbackAsync(IQueryCollection query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CallbackResult);
        }

        public Task<GenericResponse<bool>> HandleWebhookAsync(string rawPayload, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(WebhookResult);
        }

        public Task<GenericResponse<PagedResult<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PaymentHistoryResult);
        }

        public Task<GenericResponse<CurrentSubscriptionDto>> GetCurrentSubscriptionAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(CurrentSubscriptionResult);
        }
    }
}
