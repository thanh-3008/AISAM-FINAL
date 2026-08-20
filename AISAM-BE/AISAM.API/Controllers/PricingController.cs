using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/pricing")]
[Authorize]
public sealed class PricingController : ControllerBase
{
    private readonly ISystemSettingRepository _settingRepository;

    public PricingController(ISystemSettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<GenericResponse<object>>> GetPlans(CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByKeyAsync("subscription.plans");
        if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return Ok(GenericResponse<object>.CreateSuccess(new { Plans = GetDefaultPlans() }));
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

    [HttpGet("credit-packs")]
    public async Task<ActionResult<GenericResponse<object>>> GetCreditPacks(CancellationToken cancellationToken = default)
    {
        var setting = await _settingRepository.GetByKeyAsync("credit.packs");
        if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return Ok(GenericResponse<object>.CreateSuccess(new { CreditPacks = GetDefaultCreditPacks() }));
        }

        try
        {
            var packs = JsonSerializer.Deserialize<List<CreditPackDto>>(setting.Value);
            return Ok(GenericResponse<object>.CreateSuccess(new { CreditPacks = packs ?? GetDefaultCreditPacks() }));
        }
        catch
        {
            return Ok(GenericResponse<object>.CreateSuccess(new { CreditPacks = GetDefaultCreditPacks() }));
        }
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
