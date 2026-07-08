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

    public AdminPlanController(IUserRepository userRepository, ISystemSettingRepository settingRepository)
    {
        _userRepository = userRepository;
        _settingRepository = settingRepository;
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
            var plans = JsonSerializer.Deserialize<List<SubscriptionPlanDto>>(setting.Value);
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

        var json = JsonSerializer.Serialize(request.Plans);
        var setting = new Data.Model.SystemSetting
        {
            Key = "subscription.plans",
            Value = json,
            Description = "Subscription plan definitions",
            UpdatedBy = adminUserId
        };
        await _settingRepository.UpsertAsync(setting);
        return Ok(GenericResponse<bool>.CreateSuccess(true, "Plans saved."));
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
}

public class SubscriptionPlanDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Credits { get; set; }
    public int PostsPerMonth { get; set; }
    public int Members { get; set; }
    public List<string> Features { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class SavePlansRequest
{
    public List<SubscriptionPlanDto> Plans { get; set; } = new();
}
