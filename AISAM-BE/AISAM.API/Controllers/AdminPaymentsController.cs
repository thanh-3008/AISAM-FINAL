using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminPaymentsController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAdminDashboardService _dashboardService;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminPaymentsController> _logger;

    public AdminPaymentsController(
        IAdminService adminService,
        IAdminDashboardService dashboardService,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        ILogger<AdminPaymentsController> logger)
    {
        _adminService = adminService;
        _dashboardService = dashboardService;
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

        [HttpGet]
        public async Task<ActionResult<GenericResponse<object>>> GetPayments(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] int? status = null,
            CancellationToken cancellationToken = default)
        {
            var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
            var request = new PaginationRequest { Page = page, PageSize = pageSize };
            var result = await _adminService.GetPaymentsAsync(adminUserId, request, status, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

    [AllowAnonymous]
    [HttpGet("test")]
    public async Task<IActionResult> TestGetPayments([FromServices] IPaymentRepository paymentRepo)
    {
        var request = new PaginationRequest { Page = 1, PageSize = 20 };
        var result = await paymentRepo.GetPagedAllAsync(request, null, default);
        return Ok(GenericResponse<object>.CreateSuccess(result));
    }

    [HttpGet("revenue/stats")]
    public async Task<ActionResult<GenericResponse<object>>> GetRevenueStats(
        [FromQuery] string period = "month", CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _dashboardService.GetRevenueStatsAsync(adminUserId, period, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("subscriptions")]
    public async Task<ActionResult<GenericResponse<object>>> GetSubscriptions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<object>.CreateError("Only administrators can access this resource.", System.Net.HttpStatusCode.Forbidden));

        var request = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await _subscriptionRepository.GetPagedAllAsync(request, cancellationToken);
        var items = result.Data.Select(s => new
        {
            s.Id,
            s.Plan,
            s.StartDate,
            s.EndDate,
            s.IsActive,
            s.CreatedAt,
            WorkspaceName = s.Workspace?.Name ?? "N/A",
            WorkspaceId = s.WorkspaceId
        }).ToList();

        return Ok(GenericResponse<object>.CreateSuccess(new { Items = items, Total = result.TotalCount }));
    }

    [HttpPatch("subscriptions/{id:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> UpdateSubscription(
        Guid id, [FromBody] UpdateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<bool>.CreateError("Only administrators can access this resource.", System.Net.HttpStatusCode.Forbidden));

        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound(GenericResponse<bool>.CreateError("Subscription not found.", System.Net.HttpStatusCode.NotFound));

        if (request.Plan.HasValue)
            subscription.Plan = (SubscriptionPlanEnum)request.Plan.Value;
        if (request.EndDate.HasValue)
            subscription.EndDate = request.EndDate.Value;
        if (request.IsActive.HasValue)
            subscription.IsActive = request.IsActive.Value;

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        return Ok(GenericResponse<bool>.CreateSuccess(true, "Subscription updated."));
    }
}

public class UpdateSubscriptionRequest
{
    public int? Plan { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}
