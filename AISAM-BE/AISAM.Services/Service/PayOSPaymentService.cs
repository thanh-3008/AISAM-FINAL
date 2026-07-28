using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

namespace AISAM.Services.Service;

public sealed class PayOSPaymentService : IPaymentService
{
    private const string PaymentMethod = "PayOS";

    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICreditWalletRepository _creditWalletRepository;
    private readonly ICreditService _creditService;
    private readonly PayOSSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly AisamContext? _context;

    public PayOSPaymentService(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IProfileRepository profileRepository,
        IWorkspaceRepository workspaceRepository,
        ICreditWalletRepository creditWalletRepository,
        ICreditService creditService,
        IOptions<PayOSSettings> settings,
        HttpClient httpClient,
        AisamContext? context = null)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _profileRepository = profileRepository;
        _workspaceRepository = workspaceRepository;
        _creditWalletRepository = creditWalletRepository;
        _creditService = creditService;
        _settings = settings.Value;
        _httpClient = httpClient;
        _context = context;
    }

    public async Task<GenericResponse<PayOSCheckoutResponse>> CreateCheckoutAsync(
        Guid workspaceId,
        Guid userId,
        CreateCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!HasPayOsConfig())
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        return request.PaymentType switch
        {
            PaymentTypeEnum.CreditPack => await CreateCreditPackCheckoutAsync(workspaceId, userId, request, cancellationToken),
            _ => await CreateSubscriptionCheckoutAsync(workspaceId, userId, request, cancellationToken)
        };
    }

    public async Task<GenericResponse<PayOSCheckoutResponse>> CreateBusinessWorkspaceCheckoutAsync(
        Guid userId,
        CreateBusinessWorkspaceCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!HasPayOsConfig())
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED");
        }

        var workspaceName = request.WorkspaceName?.Trim();
        if (string.IsNullOrWhiteSpace(workspaceName) || workspaceName.Length > 255)
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                "Workspace name is required and must not exceed 255 characters.",
                HttpStatusCode.BadRequest,
                "INVALID_WORKSPACE_NAME");
        }

        var plan = ResolvePlan(request.PlanCode);
        if (plan is not (SubscriptionPlanEnum.Plus or SubscriptionPlanEnum.Premium))
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                "Business workspaces require Business Plus or Business Pro.",
                HttpStatusCode.BadRequest,
                "BUSINESS_PLAN_REQUIRED");
        }

        var planDefinition = GetPlanDefinition(WorkspaceTypeEnum.Business, plan.Value);
        var returnUrl = FirstNonEmpty(request.ReturnUrl, _settings.ReturnUrl);
        var cancelUrl = FirstNonEmpty(request.CancelUrl, _settings.CancelUrl);
        if (string.IsNullOrWhiteSpace(returnUrl) || string.IsNullOrWhiteSpace(cancelUrl))
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                "PayOS return/cancel URL is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_URL_NOT_CONFIGURED");
        }

        var orderCode = GenerateOrderCode();
        var description = plan == SubscriptionPlanEnum.Premium ? "AISAM Business Pro" : "AISAM Business Plus";
        var payment = await _paymentRepository.AddAsync(new Payment
        {
            UserId = userId,
            WorkspaceId = null,
            PendingWorkspaceName = workspaceName,
            RequestedPlan = plan,
            Amount = planDefinition.Amount,
            Currency = "VND",
            Status = PaymentStatusEnum.Pending,
            PaymentType = PaymentTypeEnum.Subscription,
            PaymentMethod = PaymentMethod,
            TransactionId = orderCode.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

        var checkoutPayload = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["amount"] = ((long)planDefinition.Amount).ToString(CultureInfo.InvariantCulture),
            ["cancelUrl"] = cancelUrl,
            ["description"] = description,
            ["orderCode"] = orderCode.ToString(CultureInfo.InvariantCulture),
            ["returnUrl"] = returnUrl
        };

        var payOsRequest = new
        {
            orderCode,
            amount = (long)planDefinition.Amount,
            description,
            returnUrl,
            cancelUrl,
            signature = CreateSignature(checkoutPayload)
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildPayOsUri("/v2/payment-requests"))
        {
            Content = JsonContent.Create(payOsRequest)
        };
        httpRequest.Headers.Add("x-client-id", _settings.ClientId);
        httpRequest.Headers.Add("x-api-key", _settings.ApiKey);

        using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            payment.Status = PaymentStatusEnum.Failed;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                $"PayOS checkout creation failed with status {(int)httpResponse.StatusCode}.",
                HttpStatusCode.BadGateway,
                "PAYOS_CHECKOUT_FAILED");
        }

        var payOsResponse = ParseCreatePaymentResponse(responseBody);
        if (string.IsNullOrWhiteSpace(payOsResponse.CheckoutUrl))
        {
            payment.Status = PaymentStatusEnum.Failed;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                "PayOS response did not contain a checkout URL.",
                HttpStatusCode.BadGateway,
                "PAYOS_CHECKOUT_URL_MISSING");
        }

        payment.InvoiceUrl = payOsResponse.CheckoutUrl;
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        return GenericResponse<PayOSCheckoutResponse>.CreateSuccess(payOsResponse, "Business workspace checkout created.");
    }

    public async Task<GenericResponse<bool>> SynchronizeBusinessWorkspaceCheckoutAsync(
        Guid userId,
        string reference,
        CancellationToken cancellationToken = default)
    {
        if (!HasPayOsConfig())
        {
            return GenericResponse<bool>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED");
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            return GenericResponse<bool>.CreateError(
                "Payment reference is required.",
                HttpStatusCode.BadRequest,
                "PAYOS_REFERENCE_MISSING");
        }

        var payment = await _paymentRepository.GetByReferenceAsync(reference.Trim(), cancellationToken);
        if (payment == null || payment.UserId != userId)
        {
            return GenericResponse<bool>.CreateError("Payment not found.", HttpStatusCode.NotFound);
        }

        // The webhook often completes the purchase before the browser returns from PayOS.
        // Treat repeated browser synchronization as successful without creating a second workspace.
        if (payment.Status == PaymentStatusEnum.Success && payment.WorkspaceId.HasValue)
        {
            return GenericResponse<bool>.CreateSuccess(true, "Payment is already synchronized.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            BuildPayOsUri($"/v2/payment-requests/{Uri.EscapeDataString(reference.Trim())}"));
        request.Headers.Add("x-client-id", _settings.ClientId);
        request.Headers.Add("x-api-key", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return GenericResponse<bool>.CreateError(
                "Unable to verify the payment with PayOS.",
                HttpStatusCode.BadGateway,
                "PAYOS_STATUS_CHECK_FAILED");
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var data = root.TryGetProperty("data", out var dataElement) ? dataElement : root;
        var status = FirstNonEmpty(TryGetString(data, "status"), TryGetString(root, "status"), TryGetString(root, "code"));
        var transactionId = FirstNonEmpty(TryGetString(data, "id"), TryGetString(data, "reference"));

        return await ApplyPaymentStatusAsync(
            reference.Trim(),
            status,
            transactionId,
            acknowledgeMissingPayment: false,
            cancellationToken);
    }

    private async Task<GenericResponse<PayOSCheckoutResponse>> CreateSubscriptionCheckoutAsync(
        Guid workspaceId,
        Guid userId,
        CreateCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var plan = ResolvePlan(request.PlanCode);
        if (plan == null)
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError("Invalid subscription plan.", HttpStatusCode.BadRequest, "INVALID_PLAN");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var planDefinition = GetPlanDefinition(workspace.WorkspaceType, plan.Value);
        if (planDefinition.Amount <= 0)
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError("Selected plan does not require PayOS checkout.", HttpStatusCode.BadRequest, "PLAN_DOES_NOT_REQUIRE_PAYMENT");
        }

        var returnUrl = FirstNonEmpty(request.ReturnUrl, _settings.ReturnUrl);
        var cancelUrl = FirstNonEmpty(request.CancelUrl, _settings.CancelUrl);
        if (string.IsNullOrWhiteSpace(returnUrl) || string.IsNullOrWhiteSpace(cancelUrl))
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError("PayOS return/cancel URL is not configured.", HttpStatusCode.ServiceUnavailable, "PAYOS_URL_NOT_CONFIGURED");
        }

        var orderCode = GenerateOrderCode();
        var description = $"AISAM {plan.Value}";
        var subscription = await _subscriptionRepository.AddAsync(new Subscription
        {
            WorkspaceId = workspaceId,
            Plan = plan.Value,
            QuotaPostsPerMonth = planDefinition.PostQuota,
            QuotaAIContentPerDay = planDefinition.PromptQuota,
            QuotaAIImagesPerDay = planDefinition.ImageQuota,
            QuotaPlatforms = planDefinition.PlatformQuota,
            QuotaAccounts = planDefinition.AccountQuota,
            AnalysisLevel = planDefinition.AnalysisLevel,
            QuotaAdBudgetMonthly = planDefinition.AdBudgetMonthly,
            QuotaAdCampaigns = planDefinition.AdCampaignQuota,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(30),
            IsActive = false,
            PayOSOrderCode = orderCode.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

        var payment = await _paymentRepository.AddAsync(new Payment
        {
            UserId = userId,
            SubscriptionId = subscription.Id,
            WorkspaceId = workspaceId,
            Amount = planDefinition.Amount,
            Currency = "VND",
            Status = PaymentStatusEnum.Pending,
            PaymentType = PaymentTypeEnum.Subscription,
            PaymentMethod = PaymentMethod,
            TransactionId = orderCode.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

        var checkoutPayload = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["amount"] = ((long)planDefinition.Amount).ToString(CultureInfo.InvariantCulture),
            ["cancelUrl"] = cancelUrl,
            ["description"] = description,
            ["orderCode"] = orderCode.ToString(CultureInfo.InvariantCulture),
            ["returnUrl"] = returnUrl
        };

        var payOsRequest = new
        {
            orderCode,
            amount = (long)planDefinition.Amount,
            description,
            returnUrl,
            cancelUrl,
            signature = CreateSignature(checkoutPayload)
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildPayOsUri("/v2/payment-requests"))
        {
            Content = JsonContent.Create(payOsRequest)
        };
        httpRequest.Headers.Add("x-client-id", _settings.ClientId);
        httpRequest.Headers.Add("x-api-key", _settings.ApiKey);

        using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            payment.Status = PaymentStatusEnum.Failed;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                $"PayOS checkout creation failed with status {(int)httpResponse.StatusCode}.",
                HttpStatusCode.BadGateway,
                "PAYOS_CHECKOUT_FAILED");
        }

        var payOsResponse = ParseCreatePaymentResponse(responseBody);
        if (string.IsNullOrWhiteSpace(payOsResponse.CheckoutUrl))
        {
            payment.Status = PaymentStatusEnum.Failed;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            return GenericResponse<PayOSCheckoutResponse>.CreateError("PayOS response did not contain a checkout URL.", HttpStatusCode.BadGateway, "PAYOS_CHECKOUT_URL_MISSING");
        }

        subscription.PayOSPaymentLinkId = payOsResponse.PaymentLinkId;
        payment.InvoiceUrl = payOsResponse.CheckoutUrl;
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return GenericResponse<PayOSCheckoutResponse>.CreateSuccess(payOsResponse, "Checkout request created.");
    }

    private async Task<GenericResponse<PayOSCheckoutResponse>> CreateCreditPackCheckoutAsync(
        Guid workspaceId,
        Guid userId,
        CreateCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.CreditPackCode.HasValue)
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError("Credit pack code is required.", HttpStatusCode.BadRequest, "CREDIT_PACK_REQUIRED");
        }

        var pack = GetCreditPackDefinition(request.CreditPackCode.Value);
        var returnUrl = FirstNonEmpty(request.ReturnUrl, _settings.ReturnUrl);
        var cancelUrl = FirstNonEmpty(request.CancelUrl, _settings.CancelUrl);
        if (string.IsNullOrWhiteSpace(returnUrl) || string.IsNullOrWhiteSpace(cancelUrl))
        {
            return GenericResponse<PayOSCheckoutResponse>.CreateError("PayOS return/cancel URL is not configured.", HttpStatusCode.ServiceUnavailable, "PAYOS_URL_NOT_CONFIGURED");
        }

        var orderCode = GenerateOrderCode();
        var description = $"AISAM Credit Pack {request.CreditPackCode.Value}";
        var payment = await _paymentRepository.AddAsync(new Payment
        {
            UserId = userId,
            WorkspaceId = workspaceId,
            Amount = pack.Amount,
            Currency = "VND",
            Status = PaymentStatusEnum.Pending,
            PaymentType = PaymentTypeEnum.CreditPack,
            CreditPackCode = request.CreditPackCode.Value,
            CreditAmount = pack.Credits,
            PaymentMethod = PaymentMethod,
            TransactionId = orderCode.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);

        var checkoutPayload = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["amount"] = ((long)pack.Amount).ToString(CultureInfo.InvariantCulture),
            ["cancelUrl"] = cancelUrl,
            ["description"] = description,
            ["orderCode"] = orderCode.ToString(CultureInfo.InvariantCulture),
            ["returnUrl"] = returnUrl
        };

        var payOsRequest = new
        {
            orderCode,
            amount = (long)pack.Amount,
            description,
            returnUrl,
            cancelUrl,
            signature = CreateSignature(checkoutPayload)
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildPayOsUri("/v2/payment-requests"))
        {
            Content = JsonContent.Create(payOsRequest)
        };
        httpRequest.Headers.Add("x-client-id", _settings.ClientId);
        httpRequest.Headers.Add("x-api-key", _settings.ApiKey);

        using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            payment.Status = PaymentStatusEnum.Failed;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            return GenericResponse<PayOSCheckoutResponse>.CreateError(
                $"PayOS checkout creation failed with status {(int)httpResponse.StatusCode}.",
                HttpStatusCode.BadGateway,
                "PAYOS_CHECKOUT_FAILED");
        }

        var payOsResponse = ParseCreatePaymentResponse(responseBody);
        if (string.IsNullOrWhiteSpace(payOsResponse.CheckoutUrl))
        {
            payment.Status = PaymentStatusEnum.Failed;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            return GenericResponse<PayOSCheckoutResponse>.CreateError("PayOS response did not contain a checkout URL.", HttpStatusCode.BadGateway, "PAYOS_CHECKOUT_URL_MISSING");
        }

        payment.InvoiceUrl = payOsResponse.CheckoutUrl;
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return GenericResponse<PayOSCheckoutResponse>.CreateSuccess(payOsResponse, "Checkout request created.");
    }

    public async Task<GenericResponse<bool>> HandleCallbackAsync(IQueryCollection query, CancellationToken cancellationToken = default)
    {
        if (!HasPayOsConfig())
        {
            return GenericResponse<bool>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED");
        }

        var reference = FirstNonEmpty(query["orderCode"].FirstOrDefault(), query["id"].FirstOrDefault(), query["paymentLinkId"].FirstOrDefault());
        if (string.IsNullOrWhiteSpace(reference))
        {
            return GenericResponse<bool>.CreateError("PayOS callback is missing payment reference.", HttpStatusCode.BadRequest, "PAYOS_REFERENCE_MISSING");
        }

        if (!query.TryGetValue("signature", out var signature) || string.IsNullOrWhiteSpace(signature.FirstOrDefault()))
        {
            return GenericResponse<bool>.CreateError(
                "PayOS callback signature is required.",
                HttpStatusCode.BadRequest,
                "PAYOS_SIGNATURE_REQUIRED");
        }

        if (!string.IsNullOrWhiteSpace(signature.FirstOrDefault()))
        {
            var signedValues = query
                .Where(item => !string.Equals(item.Key, "signature", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(item => item.Key, item => item.Value.FirstOrDefault() ?? string.Empty, StringComparer.Ordinal);

            if (!VerifySignature(signedValues, signature.FirstOrDefault()!))
            {
                return GenericResponse<bool>.CreateError("Invalid PayOS callback signature.", HttpStatusCode.BadRequest, "PAYOS_SIGNATURE_INVALID");
            }
        }

        var status = FirstNonEmpty(query["status"].FirstOrDefault(), query["code"].FirstOrDefault());
        return await ApplyPaymentStatusAsync(reference, status, query["id"].FirstOrDefault(), acknowledgeMissingPayment: false, cancellationToken);
    }

    public async Task<GenericResponse<bool>> HandleWebhookAsync(string rawPayload, CancellationToken cancellationToken = default)
    {
        if (!HasPayOsConfig())
        {
            return GenericResponse<bool>.CreateError(
                "PayOS is not configured.",
                HttpStatusCode.ServiceUnavailable,
                "PAYOS_NOT_CONFIGURED");
        }

        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            return GenericResponse<bool>.CreateError("PayOS webhook body is empty.", HttpStatusCode.BadRequest, "PAYOS_WEBHOOK_EMPTY");
        }

        using var document = JsonDocument.Parse(rawPayload);
        var root = document.RootElement;
        var signature = TryGetString(root, "signature");
        var data = root.TryGetProperty("data", out var dataElement) ? dataElement : root;

        if (string.IsNullOrWhiteSpace(signature))
        {
            return GenericResponse<bool>.CreateError(
                "PayOS webhook signature is required.",
                HttpStatusCode.BadRequest,
                "PAYOS_SIGNATURE_REQUIRED");
        }

        if (!VerifySignature(ExtractPrimitiveValues(data), signature))
        {
            return GenericResponse<bool>.CreateError("Invalid PayOS webhook signature.", HttpStatusCode.BadRequest, "PAYOS_SIGNATURE_INVALID");
        }

        var reference = FirstNonEmpty(
            TryGetString(data, "orderCode"),
            TryGetString(data, "paymentLinkId"),
            TryGetString(root, "orderCode"),
            TryGetString(root, "paymentLinkId"));

        if (string.IsNullOrWhiteSpace(reference))
        {
            return GenericResponse<bool>.CreateError("PayOS webhook is missing payment reference.", HttpStatusCode.BadRequest, "PAYOS_REFERENCE_MISSING");
        }

        var status = FirstNonEmpty(TryGetString(data, "status"), TryGetString(data, "code"), TryGetString(root, "status"), TryGetString(root, "code"));
        var transactionId = FirstNonEmpty(TryGetString(data, "reference"), TryGetString(data, "id"), TryGetString(root, "id"));
        return await ApplyPaymentStatusAsync(reference, status, transactionId, acknowledgeMissingPayment: true, cancellationToken);
    }

    public async Task<GenericResponse<PagedResult<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, cancellationToken);

        var mapped = new PagedResult<PaymentHistoryItemDto>
        {
            Data = payments.Data.Select(MapPaymentHistoryItem).ToList(),
            TotalCount = payments.TotalCount,
            Page = payments.Page,
            PageSize = payments.PageSize
        };

        return GenericResponse<PagedResult<PaymentHistoryItemDto>>.CreateSuccess(mapped);
    }

    public async Task<GenericResponse<CurrentSubscriptionDto>> GetCurrentSubscriptionAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(workspaceId, cancellationToken);

        var dto = subscription != null
            ? new CurrentSubscriptionDto
            {
                SubscriptionId = subscription.Id,
                PlanName = subscription.Plan.ToString(),
                Status = subscription.IsActive ? "Active" : "Inactive",
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                PromptQuota = subscription.QuotaAIContentPerDay,
                ImageQuota = subscription.QuotaAIImagesPerDay,
                PostQuota = subscription.QuotaPostsPerMonth,
                PlatformQuota = subscription.QuotaPlatforms,
                AccountQuota = subscription.QuotaAccounts,
                AnalysisLevel = subscription.AnalysisLevel,
                AdBudgetMonthly = subscription.QuotaAdBudgetMonthly,
                AdCampaignQuota = subscription.QuotaAdCampaigns
            }
            : new CurrentSubscriptionDto
            {
                PlanName = SubscriptionPlanEnum.Free.ToString(),
                Status = "Active",
                StartDate = DateTime.UtcNow.Date,
                PostQuota = 20,
                PlatformQuota = 1,
                AccountQuota = 1
            };

        return GenericResponse<CurrentSubscriptionDto>.CreateSuccess(dto);
    }

    private async Task<GenericResponse<bool>> ApplyPaymentStatusAsync(
        string reference,
        string? status,
        string? transactionId,
        bool acknowledgeMissingPayment,
        CancellationToken cancellationToken)
    {
        if (_context == null || !_context.Database.IsRelational() || _context.Database.CurrentTransaction != null)
        {
            return await ApplyPaymentStatusCoreAsync(reference, status, transactionId, acknowledgeMissingPayment, cancellationToken);
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                var result = await ApplyPaymentStatusCoreAsync(reference, status, transactionId, acknowledgeMissingPayment, cancellationToken);
                if (!result.Success)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return result;
                }
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private async Task<GenericResponse<bool>> ApplyPaymentStatusCoreAsync(
        string reference,
        string? status,
        string? transactionId,
        bool acknowledgeMissingPayment,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByReferenceAsync(reference, cancellationToken);
        if (payment == null)
        {
            if (acknowledgeMissingPayment)
            {
                return GenericResponse<bool>.CreateSuccess(true, "PayOS webhook acknowledged; no matching payment was found.");
            }

            return GenericResponse<bool>.CreateError("Payment not found.", HttpStatusCode.NotFound);
        }

        if (!string.IsNullOrWhiteSpace(transactionId) && string.IsNullOrWhiteSpace(payment.TransactionId))
        {
            payment.TransactionId = transactionId;
        }

        if (IsPaidStatus(status))
        {
            if (payment.Status == PaymentStatusEnum.Success)
            {
                await _paymentRepository.UpdateAsync(payment, cancellationToken);
                return GenericResponse<bool>.CreateSuccess(true, "PayOS payment status synchronized.");
            }

            payment.Status = PaymentStatusEnum.Success;
            if (!string.IsNullOrWhiteSpace(payment.PendingWorkspaceName) && !payment.WorkspaceId.HasValue)
            {
                var creationResult = await CompleteBusinessWorkspacePurchaseAsync(
                    payment,
                    reference,
                    cancellationToken);
                if (!creationResult.Success)
                {
                    return creationResult;
                }
            }
            else if (payment.PaymentType == PaymentTypeEnum.CreditPack)
            {
                if (!payment.WorkspaceId.HasValue)
                {
                    return GenericResponse<bool>.CreateError(
                        "Credit pack payment is missing its workspace.",
                        HttpStatusCode.Conflict,
                        "PAYMENT_WORKSPACE_REQUIRED");
                }

                var workspace = await _workspaceRepository.GetByIdAsync(payment.WorkspaceId.Value, cancellationToken);
                if (workspace == null)
                {
                    payment.Status = PaymentStatusEnum.Failed;
                    await _paymentRepository.UpdateAsync(payment, cancellationToken);
                    return GenericResponse<bool>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
                }

                var creditGrant = await _creditService.GrantCreditPackCreditsAsync(
                    workspace.Id,
                    payment.UserId,
                    workspace.WorkspaceType,
                    payment.CreditAmount ?? 0,
                    cancellationToken);
                if (!creditGrant.Success)
                {
                    payment.Status = PaymentStatusEnum.Failed;
                    await _paymentRepository.UpdateAsync(payment, cancellationToken);
                    return GenericResponse<bool>.CreateError(
                        creditGrant.Message ?? "Unable to grant credit pack credits.",
                        (HttpStatusCode)creditGrant.StatusCode,
                        creditGrant.Error?.ErrorCode);
                }
            }
            else if (payment.SubscriptionId.HasValue)
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(payment.SubscriptionId.Value, cancellationToken);
                if (subscription != null)
                {
                    var today = DateTime.UtcNow.Date;
                    var workspace = await _workspaceRepository.GetByIdAsync(subscription.WorkspaceId, cancellationToken);

                    if (subscription.IsActive)
                    {
                        if (workspace != null)
                        {
                            var wallet = await _creditWalletRepository.GetByWorkspaceIdAsync(subscription.WorkspaceId, cancellationToken);
                            var planCredits = ResolvePlanCreditAmount(workspace.WorkspaceType, subscription.Plan);
                            if (wallet == null || wallet.Balance < planCredits)
                            {
                                await _creditService.GrantSubscriptionCreditsAsync(
                                    workspace.Id,
                                    payment.UserId,
                                    workspace.WorkspaceType,
                                    subscription.Plan,
                                    cancellationToken);
                            }
                        }
                        await _paymentRepository.UpdateAsync(payment, cancellationToken);
                        return GenericResponse<bool>.CreateSuccess(true, "PayOS payment status synchronized.");
                    }

                    var currentSubscription = await _subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(subscription.WorkspaceId, cancellationToken);
                    var renewalBaseDate = currentSubscription?.EndDate is { } currentEndDate && currentEndDate > today
                        ? currentEndDate
                        : today;

                    if (workspace != null)
                    {
                        var creditGrant = await _creditService.GrantSubscriptionCreditsAsync(
                            workspace.Id,
                            payment.UserId,
                            workspace.WorkspaceType,
                            subscription.Plan,
                            cancellationToken);
                        if (!creditGrant.Success)
                        {
                            payment.Status = PaymentStatusEnum.Failed;
                            await _paymentRepository.UpdateAsync(payment, cancellationToken);
                                return GenericResponse<bool>.CreateError(
                                    creditGrant.Message ?? "Unable to grant subscription credits.",
                                    (HttpStatusCode)creditGrant.StatusCode,
                                    creditGrant.Error?.ErrorCode);
                        }
                    }

                    if (currentSubscription != null && currentSubscription.Id != subscription.Id)
                    {
                        currentSubscription.IsActive = false;
                        await _subscriptionRepository.UpdateAsync(currentSubscription, cancellationToken);
                    }

                    subscription.IsActive = true;
                    subscription.StartDate = today;
                    subscription.EndDate = renewalBaseDate.AddDays(30);
                    await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

                    if (workspace != null)
                    {
                        workspace.SubscriptionExpiredAt = subscription.EndDate;
                        workspace.Status = WorkspaceStatusEnum.Active;
                        workspace.ArchivedAt = null;
                        workspace.DeletedAt = null;
                        workspace.MemberLimit = ResolveMemberLimit(workspace.WorkspaceType, subscription.Plan);
                        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
                    }

                    var profile = subscription.ProfileId.HasValue
                        ? await _profileRepository.GetByIdAsync(subscription.ProfileId.Value, cancellationToken)
                        : null;
                    if (profile != null)
                    {
                        profile.SubscriptionId = subscription.Id;
                        await _profileRepository.UpdateAsync(profile, cancellationToken);
                    }
                }
            }
        }
        else if (IsFailedStatus(status))
        {
            payment.Status = PaymentStatusEnum.Failed;
        }

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "PayOS payment status synchronized.");
    }

    private async Task<GenericResponse<bool>> CompleteBusinessWorkspacePurchaseAsync(
        Payment payment,
        string paymentReference,
        CancellationToken cancellationToken)
    {
        if (payment.RequestedPlan is not (SubscriptionPlanEnum.Plus or SubscriptionPlanEnum.Premium))
        {
            return GenericResponse<bool>.CreateError(
                "Pending Business workspace payment has an invalid plan.",
                HttpStatusCode.Conflict,
                "BUSINESS_PLAN_REQUIRED");
        }

        var plan = payment.RequestedPlan.Value;
        var today = DateTime.UtcNow.Date;
        var planDefinition = GetPlanDefinition(WorkspaceTypeEnum.Business, plan);
        var workspace = await _workspaceRepository.AddAsync(new Workspace
        {
            Name = payment.PendingWorkspaceName!.Trim(),
            WorkspaceType = WorkspaceTypeEnum.Business,
            Status = WorkspaceStatusEnum.Active,
            MemberLimit = ResolveMemberLimit(WorkspaceTypeEnum.Business, plan),
            SubscriptionExpiredAt = today.AddDays(30),
            CreditWallet = new CreditWallet { Balance = 0 },
            Members =
            [
                new WorkspaceMember
                {
                    UserId = payment.UserId,
                    Role = WorkspaceMemberRoleEnum.Owner
                }
            ]
        }, cancellationToken);

        var subscription = await _subscriptionRepository.AddAsync(new Subscription
        {
            WorkspaceId = workspace.Id,
            Plan = plan,
            QuotaPostsPerMonth = planDefinition.PostQuota,
            QuotaAIContentPerDay = planDefinition.PromptQuota,
            QuotaAIImagesPerDay = planDefinition.ImageQuota,
            QuotaPlatforms = planDefinition.PlatformQuota,
            QuotaAccounts = planDefinition.AccountQuota,
            AnalysisLevel = planDefinition.AnalysisLevel,
            QuotaAdBudgetMonthly = planDefinition.AdBudgetMonthly,
            QuotaAdCampaigns = planDefinition.AdCampaignQuota,
            StartDate = today,
            EndDate = today.AddDays(30),
            IsActive = true,
            PayOSOrderCode = paymentReference
        }, cancellationToken);

        payment.WorkspaceId = workspace.Id;
        payment.SubscriptionId = subscription.Id;
        payment.PendingWorkspaceName = null;

        var creditGrant = await _creditService.GrantSubscriptionCreditsAsync(
            workspace.Id,
            payment.UserId,
            WorkspaceTypeEnum.Business,
            plan,
            cancellationToken);
        if (!creditGrant.Success)
        {
            return GenericResponse<bool>.CreateError(
                creditGrant.Message ?? "Unable to grant subscription credits.",
                (HttpStatusCode)creditGrant.StatusCode,
                creditGrant.Error?.ErrorCode);
        }

        return GenericResponse<bool>.CreateSuccess(true, "Business workspace created after successful payment.");
    }

    private bool HasPayOsConfig()
    {
        return !string.IsNullOrWhiteSpace(_settings.ClientId)
            && !string.IsNullOrWhiteSpace(_settings.ApiKey)
            && !string.IsNullOrWhiteSpace(_settings.ChecksumKey);
    }

    private Uri BuildPayOsUri(string path)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl)
            ? "https://api-merchant.payos.vn"
            : _settings.BaseUrl.TrimEnd('/');

        return new Uri($"{baseUrl}{path}");
    }

    private string CreateSignature(IReadOnlyDictionary<string, string> values)
    {
        var data = string.Join("&", values.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => $"{item.Key}={item.Value}"));
        var key = Encoding.UTF8.GetBytes(_settings.ChecksumKey);
        var bytes = Encoding.UTF8.GetBytes(data);
        return Convert.ToHexString(HMACSHA256.HashData(key, bytes)).ToLowerInvariant();
    }

    private bool VerifySignature(IReadOnlyDictionary<string, string> values, string signature)
    {
        var expected = CreateSignature(values);
        return string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase);
    }

    private static PayOSCheckoutResponse ParseCreatePaymentResponse(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var data = root.TryGetProperty("data", out var dataElement) ? dataElement : root;

        return new PayOSCheckoutResponse
        {
            CheckoutUrl = FirstNonEmpty(TryGetString(data, "checkoutUrl"), TryGetString(data, "checkoutUrl")),
            PaymentLinkId = TryGetString(data, "paymentLinkId"),
            OrderCode = TryGetString(data, "orderCode")
        };
    }

    private static SubscriptionPlanEnum? ResolvePlan(string planCode)
    {
        return Enum.TryParse<SubscriptionPlanEnum>(planCode, ignoreCase: true, out var plan)
            ? plan
            : null;
    }

    private static PlanDefinition GetPlanDefinition(WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan)
    {
        return (workspaceType, plan) switch
        {
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Plus) => new PlanDefinition(2_000m, 300, 50, 10, 2, 2, 1, 3_000_000m, 3),
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Premium) => new PlanDefinition(3_000m, 1_000, 200, 30, 3, 5, 2, 10_000_000m, 10),
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.PlusTrial) => new PlanDefinition(0m, 300, 10, 3, 1, 1, 1, 0m, 1),
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Free) => new PlanDefinition(0m, 20, 0, 0, 1, 1, 0, 0m, 0),
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Plus) => new PlanDefinition(4_000m, 5_000, 50, 10, 2, 2, 1, 3_000_000m, 3),
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Premium) => new PlanDefinition(5_000m, 20_000, 200, 30, 3, 5, 2, 10_000_000m, 10),
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.PlusTrial) => new PlanDefinition(0m, 1_000, 10, 3, 1, 1, 1, 0m, 1),
            _ => new PlanDefinition(0m, 20, 0, 0, 1, 1, 0, 0m, 0)
        };
    }

    private static CreditPackDefinition GetCreditPackDefinition(CreditPackCodeEnum packCode)
    {
        return packCode switch
        {
            CreditPackCodeEnum.Starter => new CreditPackDefinition(2_000m, 100),
            CreditPackCodeEnum.Standard => new CreditPackDefinition(3_000m, 500),
            CreditPackCodeEnum.Growth => new CreditPackDefinition(4_000m, 1_500),
            CreditPackCodeEnum.Business => new CreditPackDefinition(5_000m, 5_000),
            _ => throw new InvalidOperationException("Unsupported credit pack.")
        };
    }

    private static int ResolveMemberLimit(WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan)
    {
        if (workspaceType == WorkspaceTypeEnum.Personal)
        {
            return 1;
        }

        return plan switch
        {
            SubscriptionPlanEnum.Premium => 50,
            SubscriptionPlanEnum.Plus => 10,
            _ => 1
        };
    }

    private static long ResolvePlanCreditAmount(WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan)
    {
        return (workspaceType, plan) switch
        {
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Free) => 50,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Plus) => 500,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Premium) => 2_000,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.PlusTrial) => 100,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Plus) => 15_000,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.PlusTrial) => 1_000,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Premium) => 50_000,
            _ => 0
        };
    }

    private static long GenerateOrderCode()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = RandomNumberGenerator.GetInt32(100, 999);
        return long.Parse($"{timestamp % 1000000000000}{random}", CultureInfo.InvariantCulture);
    }

    private static Dictionary<string, string> ExtractPrimitiveValues(JsonElement element)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array or JsonValueKind.Undefined)
            {
                continue;
            }

            values[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                JsonValueKind.Null => string.Empty,
                _ => property.Value.GetRawText()
            };
        }

        return values;
    }

    private static bool IsPaidStatus(string? status)
    {
        return string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "00", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFailedStatus(string? status)
    {
        return string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "CANCELED", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "EXPIRED", StringComparison.OrdinalIgnoreCase);
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    public async Task<GenericResponse<bool>> CancelSubscriptionAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(workspaceId, cancellationToken);
        if (subscription == null)
        {
            return GenericResponse<bool>.CreateError("No active subscription found for this workspace.", HttpStatusCode.NotFound);
        }

        if (subscription.Plan == SubscriptionPlanEnum.Free)
        {
            return GenericResponse<bool>.CreateError("Free plan cannot be cancelled.", HttpStatusCode.BadRequest);
        }

        subscription.IsActive = false;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        return GenericResponse<bool>.CreateSuccess(true, "Subscription cancelled successfully. You will retain access until the end of your current billing period.");
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

    private sealed record PlanDefinition(
        decimal Amount,
        int PostQuota,
        int PromptQuota,
        int ImageQuota,
        int PlatformQuota,
        int AccountQuota,
        int AnalysisLevel,
        decimal AdBudgetMonthly,
        int AdCampaignQuota);

    private sealed record CreditPackDefinition(
        decimal Amount,
        long Credits);
}
