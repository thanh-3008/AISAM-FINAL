using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ISystemSettingRepository _settingRepository;

    public AdminPaymentsController(
        IAdminService adminService,
        IAdminDashboardService dashboardService,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        ILogger<AdminPaymentsController> logger,
        IAuditLogRepository auditLogRepository,
        ISystemSettingRepository settingRepository)
    {
        _adminService = adminService;
        _dashboardService = dashboardService;
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _logger = logger;
        _auditLogRepository = auditLogRepository;
        _settingRepository = settingRepository;
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

    [HttpGet("test")]
    public async Task<IActionResult> TestGetPayments([FromServices] IPaymentRepository paymentRepo)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            return NotFound();
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
        var planPrices = await GetCurrentPlanPricesAsync();
        var items = result.Data.Select(s => new
        {
            s.Id,
            s.Plan,
            s.StartDate,
            s.EndDate,
            s.IsActive,
            s.CreatedAt,
            WorkspaceName = s.Workspace?.Name ?? "N/A",
            WorkspaceId = s.WorkspaceId,
            WorkspaceType = s.Workspace?.WorkspaceType,
            CurrentPrice = planPrices.GetValueOrDefault(GetPlanSettingId(s.Workspace?.WorkspaceType ?? WorkspaceTypeEnum.Personal, s.Plan), 0m)
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

        if (!request.Plan.HasValue && !request.EndDate.HasValue && !request.IsActive.HasValue)
            return BadRequest(GenericResponse<bool>.CreateError("At least one subscription change is required."));

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            Plan = (int)subscription.Plan,
            subscription.EndDate,
            subscription.IsActive
        });

        if (request.Plan.HasValue)
        {
            if (!Enum.IsDefined(typeof(SubscriptionPlanEnum), request.Plan.Value))
                return BadRequest(GenericResponse<bool>.CreateError("Invalid subscription plan."));
            subscription.Plan = (SubscriptionPlanEnum)request.Plan.Value;
        }
        if (request.EndDate.HasValue && request.EndDate.Value <= subscription.StartDate)
            return BadRequest(GenericResponse<bool>.CreateError("Subscription end date must be after its start date."));
        if (request.EndDate.HasValue)
            subscription.EndDate = request.EndDate.Value;
        if (request.IsActive.HasValue)
            subscription.IsActive = request.IsActive.Value;

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _auditLogRepository.AddAsync(new AISAM.Data.Model.AuditLog
        {
            ActorId = adminUserId,
            ActionType = "UPDATE_SUBSCRIPTION",
            TargetTable = "subscriptions",
            TargetId = subscription.Id,
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                Plan = (int)subscription.Plan,
                subscription.EndDate,
                subscription.IsActive
            }),
            Notes = string.IsNullOrWhiteSpace(request.Reason) ? "Administrative subscription update" : request.Reason.Trim()
        }, cancellationToken);
        return Ok(GenericResponse<bool>.CreateSuccess(true, "Subscription updated."));
    }

    private async Task<Dictionary<string, decimal>> GetCurrentPlanPricesAsync()
    {
        var setting = await _settingRepository.GetByKeyAsync("subscription.plans");
        if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return GetDefaultPlanPrices();
        }

        try
        {
            var plans = JsonSerializer.Deserialize<List<SubscriptionPlanDto>>(setting.Value);
            return plans?
                .Where(plan => !string.IsNullOrWhiteSpace(plan.Id))
                .ToDictionary(plan => plan.Id.Trim().ToLowerInvariant(), plan => plan.Price)
                ?? GetDefaultPlanPrices();
        }
        catch
        {
            return GetDefaultPlanPrices();
        }
    }

    private static Dictionary<string, decimal> GetDefaultPlanPrices() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["free"] = 0m,
        ["plus"] = 2_000m,
        ["premium"] = 3_000m,
        ["business-plus"] = 4_000m,
        ["business-pro"] = 5_000m
    };

    private static string GetPlanSettingId(WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan)
    {
        if (workspaceType == WorkspaceTypeEnum.Business)
        {
            return plan switch
            {
                SubscriptionPlanEnum.Plus => "business-plus",
                SubscriptionPlanEnum.Premium => "business-pro",
                _ => "free"
            };
        }

        return plan switch
        {
            SubscriptionPlanEnum.Plus => "plus",
            SubscriptionPlanEnum.Premium => "premium",
            _ => "free"
        };
    }

    [HttpPatch("{id:guid}/refund")]
    public async Task<ActionResult<GenericResponse<bool>>> RefundPayment(
        Guid id, [FromBody] RefundPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.RefundPaymentAsync(adminUserId, id, request.Reason, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public class RefundPaymentRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class UpdateSubscriptionRequest
{
    public int? Plan { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
    public string? Reason { get; set; }
}
