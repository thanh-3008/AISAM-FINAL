using System.Net;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class PayOSPaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly PayOSSettings _settings;
    private readonly HttpClient _httpClient;

    public PayOSPaymentService(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IOptions<PayOSSettings> settings,
        HttpClient httpClient)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _settings = settings.Value;
        _httpClient = httpClient;
    }

    public Task<GenericResponse<PayOSCheckoutResponse>> CreateCheckoutAsync(Guid profileId, CreateCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasPayOsConfig())
        {
            return Task.FromResult(GenericResponse<PayOSCheckoutResponse>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED"));
        }

        var response = new PayOSCheckoutResponse
        {
            CheckoutUrl = _settings.ReturnUrl,
            PaymentLinkId = null,
            OrderCode = null
        };

        return Task.FromResult(GenericResponse<PayOSCheckoutResponse>.CreateSuccess(response, "Checkout request created."));
    }

    public Task<GenericResponse<bool>> HandleCallbackAsync(IQueryCollection query, CancellationToken cancellationToken = default)
    {
        if (!HasPayOsConfig())
        {
            return Task.FromResult(GenericResponse<bool>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED"));
        }

        return Task.FromResult(GenericResponse<bool>.CreateSuccess(true, "PayOS callback accepted."));
    }

    public Task<GenericResponse<bool>> HandleWebhookAsync(string rawPayload, CancellationToken cancellationToken = default)
    {
        if (!HasPayOsConfig())
        {
            return Task.FromResult(GenericResponse<bool>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED"));
        }

        return Task.FromResult(GenericResponse<bool>.CreateSuccess(true, "PayOS webhook accepted."));
    }

    public async Task<GenericResponse<PagedResult<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetPagedByProfileIdAsync(profileId, request, cancellationToken);

        var mapped = new PagedResult<PaymentHistoryItemDto>
        {
            Data = payments.Data.Select(MapPaymentHistoryItem).ToList(),
            TotalCount = payments.TotalCount,
            Page = payments.Page,
            PageSize = payments.PageSize
        };

        return GenericResponse<PagedResult<PaymentHistoryItemDto>>.CreateSuccess(mapped);
    }

    public async Task<GenericResponse<CurrentSubscriptionDto>> GetCurrentSubscriptionAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetCurrentActiveByProfileIdAsync(profileId, cancellationToken);
        if (subscription == null)
        {
            return GenericResponse<CurrentSubscriptionDto>.CreateError("Active subscription not found.", HttpStatusCode.NotFound);
        }

        return GenericResponse<CurrentSubscriptionDto>.CreateSuccess(new CurrentSubscriptionDto
        {
            SubscriptionId = subscription.Id,
            PlanName = subscription.Plan.ToString(),
            Status = subscription.IsActive ? "Active" : "Inactive",
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate
        });
    }

    private bool HasPayOsConfig()
    {
        return !string.IsNullOrWhiteSpace(_settings.ClientId)
            && !string.IsNullOrWhiteSpace(_settings.ApiKey)
            && !string.IsNullOrWhiteSpace(_settings.ChecksumKey);
    }

    private static PaymentHistoryItemDto MapPaymentHistoryItem(Payment payment)
    {
        return new PaymentHistoryItemDto
        {
            Id = payment.Id,
            PaymentMethod = payment.PaymentMethod ?? string.Empty,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt
        };
    }
}
