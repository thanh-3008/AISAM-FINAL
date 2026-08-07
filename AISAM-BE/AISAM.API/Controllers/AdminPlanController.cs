using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/plans")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminPlanController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ISystemSettingRepository _settingRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public AdminPlanController(IUserRepository userRepository, ISystemSettingRepository settingRepository, IAuditLogRepository auditLogRepository)
    {
        _userRepository = userRepository;
        _settingRepository = settingRepository;
        _auditLogRepository = auditLogRepository;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetPlans(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<object>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        var setting = await _settingRepository.GetByKeyAsync("subscription.plans");
        if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
        {
            var defaults = GetDefaultPlans();
            return Ok(GenericResponse<object>.CreateSuccess(new { Plans = defaults }));
        }

        try
        {
            var plans = JsonSerializer.Deserialize<List<AISAM.Common.Dtos.SubscriptionPlanDto>>(setting.Value);
            return Ok(GenericResponse<object>.CreateSuccess(new { Plans = plans ?? GetDefaultPlans() }));
        }
        catch
        {
            return Ok(GenericResponse<object>.CreateSuccess(new { Plans = GetDefaultPlans() }));
        }
    }

    [HttpPut]
    public async Task<ActionResult<GenericResponse<bool>>> SavePlans(
        [FromBody] SavePlansRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<bool>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        if (request.Plans.Count == 0 || request.Plans.Count > 20)
            return BadRequest(GenericResponse<bool>.CreateError("Between 1 and 20 plans are required."));
        if (request.Plans.Any(p => string.IsNullOrWhiteSpace(p.Id) || string.IsNullOrWhiteSpace(p.Name)
            || p.Price < 0 || p.Credits < 0 || p.PostsPerMonth < 0 || p.Members < 1))
            return BadRequest(GenericResponse<bool>.CreateError("Plan values are invalid."));
        if (request.Plans.Select(p => p.Id.Trim().ToLowerInvariant()).Distinct().Count() != request.Plans.Count)
            return BadRequest(GenericResponse<bool>.CreateError("Plan identifiers must be unique."));

        var previous = await _settingRepository.GetByKeyAsync("subscription.plans");

        var json = JsonSerializer.Serialize(request.Plans);
        var setting = new Data.Model.SystemSetting
        {
            Key = "subscription.plans",
            Value = json,
            Description = "Subscription plan definitions",
            UpdatedBy = adminUserId
        };
        await _settingRepository.UpsertAsync(setting);
        await _auditLogRepository.AddAsync(new Data.Model.AuditLog
        {
            ActorId = adminUserId,
            ActionType = "UPDATE_SUBSCRIPTION_PLANS",
            TargetTable = "system_settings",
            TargetId = previous?.Id ?? setting.Id,
            OldValues = previous?.Value,
            NewValues = json,
            Notes = $"Updated {request.Plans.Count} subscription plans"
        }, cancellationToken);
        return Ok(GenericResponse<bool>.CreateSuccess(true, "Plans saved."));
    }

    [HttpGet("credit-packs")]
    public async Task<ActionResult<GenericResponse<object>>> GetCreditPacks(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<object>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        var setting = await _settingRepository.GetByKeyAsync("credit.packs");
        if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
        {
            var defaults = GetDefaultCreditPacks();
            return Ok(GenericResponse<object>.CreateSuccess(new { CreditPacks = defaults }));
        }

        try
        {
            var packs = JsonSerializer.Deserialize<List<AISAM.Common.Dtos.CreditPackDto>>(setting.Value);
            return Ok(GenericResponse<object>.CreateSuccess(new { CreditPacks = packs ?? GetDefaultCreditPacks() }));
        }
        catch
        {
            return Ok(GenericResponse<object>.CreateSuccess(new { CreditPacks = GetDefaultCreditPacks() }));
        }
    }

    [HttpPut("credit-packs")]
    public async Task<ActionResult<GenericResponse<bool>>> SaveCreditPacks(
        [FromBody] SaveCreditPacksRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<bool>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        var json = JsonSerializer.Serialize(request.CreditPacks);
        var setting = new Data.Model.SystemSetting
        {
            Key = "credit.packs",
            Value = json,
            Description = "Credit pack definitions",
            UpdatedBy = adminUserId
        };
        await _settingRepository.UpsertAsync(setting);
        return Ok(GenericResponse<bool>.CreateSuccess(true, "Credit packs saved."));
    }

    private List<SubscriptionPlanDto> GetDefaultPlans()
    {
        return new List<SubscriptionPlanDto>
        {
            new() { Id = "free", Name = "Free", Price = 0, Credits = 50, PostsPerMonth = 20, Members = 1, Features = new List<string> { "basicAnalytics", "generateText" }, IsActive = true },
            new() { Id = "plus", Name = "Plus", Price = 2000, Credits = 500, PostsPerMonth = 300, Members = 1, Features = new List<string> { "basicAnalytics", "generateText", "multiPlatformPublish", "schedulePost", "aiImage" }, IsActive = true },
            new() { Id = "premium", Name = "Premium", Price = 3000, Credits = 2000, PostsPerMonth = 1000, Members = 1, Features = new List<string> { "basicAnalytics", "advancedAnalytics", "generateText", "multiPlatformPublish", "schedulePost", "aiImage", "aiVideo", "trendAnalysis", "holidaySuggestion", "campaignRecommendation" }, IsActive = true },
            new() { Id = "business-plus", Name = "Business Plus", Price = 4000, Credits = 15000, PostsPerMonth = 5000, Members = 10, Features = new List<string> { "basicAnalytics", "generateText", "multiPlatformPublish", "schedulePost", "aiImage", "workspaceDashboard" }, IsActive = true },
            new() { Id = "business-pro", Name = "Business Pro", Price = 5000, Credits = 50000, PostsPerMonth = 20000, Members = 50, Features = new List<string> { "basicAnalytics", "advancedAnalytics", "generateText", "multiPlatformPublish", "schedulePost", "aiImage", "aiVideo", "trendAnalysis", "holidaySuggestion", "campaignRecommendation", "workspaceDashboard" }, IsActive = true }
        };
    }

    private List<CreditPackDto> GetDefaultCreditPacks()
    {
        return new List<CreditPackDto>
        {
            new() { Id = "starter", Name = "Starter", Credits = 100, Price = 2000, IsActive = true },
            new() { Id = "standard", Name = "Standard", Credits = 500, Price = 3000, IsActive = true },
            new() { Id = "growth", Name = "Growth", Credits = 1500, Price = 4000, IsActive = true },
            new() { Id = "business", Name = "Business", Credits = 5000, Price = 5000, IsActive = true }
        };
    }
}

public class SavePlansRequest
{
    public List<AISAM.Common.Dtos.SubscriptionPlanDto> Plans { get; set; } = new();
}

public class SaveCreditPacksRequest
{
    public List<AISAM.Common.Dtos.CreditPackDto> CreditPacks { get; set; } = new();
}

