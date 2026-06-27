using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/payment")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<GenericResponse<PayOSCheckoutResponse>>> CreateCheckout(
        [FromBody] CreateCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);
        var result = await _paymentService.CreateCheckoutAsync(membership.WorkspaceId, membership.UserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("business-workspace-checkout")]
    [Authorize]
    public async Task<ActionResult<GenericResponse<PayOSCheckoutResponse>>> CreateBusinessWorkspaceCheckout(
        [FromBody] CreateBusinessWorkspaceCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _paymentService.CreateBusinessWorkspaceCheckoutAsync(userId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("business-workspace-checkout/sync")]
    [Authorize]
    public async Task<ActionResult<GenericResponse<bool>>> SynchronizeBusinessWorkspaceCheckout(
        [FromBody] SynchronizeBusinessWorkspaceCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _paymentService.SynchronizeBusinessWorkspaceCheckoutAsync(userId, request.Reference, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<ActionResult<GenericResponse<bool>>> HandleCallback(CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.HandleCallbackAsync(Request.Query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult<GenericResponse<bool>>> HandleWebhook([FromBody] object payload, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.HandleWebhookAsync(payload.ToString() ?? string.Empty, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<GenericResponse<PagedResult<PaymentHistoryItemDto>>>> GetHistory(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetPaymentHistoryAsync(GetWorkspaceId(), request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("subscription/current")]
    [Authorize]
    public async Task<ActionResult<GenericResponse<CurrentSubscriptionDto>>> GetCurrentSubscription(CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetCurrentSubscriptionAsync(GetWorkspaceId(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetWorkspaceId()
    {
        return WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
    }
}
