using System.Net;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class QuotaService : IQuotaService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IProfileRepository _profileRepository;

    public QuotaService(
        ISubscriptionRepository subscriptionRepository,
        IWorkspaceRepository workspaceRepository,
        IProfileRepository profileRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _workspaceRepository = workspaceRepository;
        _profileRepository = profileRepository;
    }

    public async Task<GenericResponse<QuotaSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetCurrentActiveByProfileIdAsync(profileId, cancellationToken);
        if (subscription == null)
        {
            return GenericResponse<QuotaSummaryDto>.CreateError("Active subscription not found.", HttpStatusCode.NotFound);
        }

        var windowStart = subscription.StartDate;
        var windowEnd = subscription.EndDate;
        var promptDay = DateTime.UtcNow.Date;
        var promptUsage = await _subscriptionRepository.CountSuccessfulPromptUsageAsync(profileId, promptDay, promptDay, cancellationToken);
        var postUsage = await _subscriptionRepository.CountSuccessfulPostUsageAsync(profileId, windowStart, windowEnd, cancellationToken);

        var promptLimit = subscription.QuotaAIContentPerDay;
        var postLimit = subscription.QuotaPostsPerMonth;

        return GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto
        {
            PlanName = subscription.Plan.ToString(),
            SubscriptionStatus = subscription.IsActive ? "Active" : "Inactive",
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            PromptQuotaLimit = promptLimit,
            PromptUsage = promptUsage,
            PromptRemaining = Math.Max(0, promptLimit - promptUsage),
            PostQuotaLimit = postLimit,
            PostUsage = postUsage,
            PostRemaining = Math.Max(0, postLimit - postUsage)
        });
    }

    public async Task<GenericResponse<QuotaSummaryDto>> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<QuotaSummaryDto>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var subscription = await _subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(workspaceId, cancellationToken);
        if (subscription == null)
        {
            return GenericResponse<QuotaSummaryDto>.CreateError("Active workspace subscription not found.", HttpStatusCode.NotFound);
        }

        var memberProfiles = await GetActiveWorkspaceProfilesAsync(workspace, cancellationToken);
        var (windowStart, windowEnd) = ResolvePostQuotaWindow(subscription, DateTime.UtcNow.Date);
        var promptDay = DateTime.UtcNow.Date;

        var promptUsage = 0;
        var postUsage = 0;
        foreach (var profile in memberProfiles)
        {
            promptUsage += await _subscriptionRepository.CountSuccessfulPromptUsageAsync(profile.Id, promptDay, promptDay, cancellationToken);
            postUsage += await _subscriptionRepository.CountSuccessfulPostUsageAsync(profile.Id, windowStart, windowEnd, cancellationToken);
        }

        var postLimit = ResolveWorkspacePostQuota(workspace.WorkspaceType, subscription.Plan);
        var promptLimit = subscription.QuotaAIContentPerDay;

        return GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto
        {
            PlanName = BuildWorkspacePlanName(workspace.WorkspaceType, subscription.Plan),
            SubscriptionStatus = subscription.IsActive ? "Active" : "Inactive",
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            PromptQuotaLimit = promptLimit,
            PromptUsage = promptUsage,
            PromptRemaining = Math.Max(0, promptLimit - promptUsage),
            PostQuotaLimit = postLimit,
            PostUsage = postUsage,
            PostRemaining = Math.Max(0, postLimit - postUsage)
        });
    }

    public async Task<GenericResponse<bool>> EnsurePromptQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(profileId, cancellationToken);
        if (!summary.Success)
        {
            return GenericResponse<bool>.CreateError(summary.Message ?? "Unable to resolve prompt quota.", (HttpStatusCode)summary.StatusCode, summary.Error?.ErrorCode);
        }

        if (summary.Data!.PromptRemaining <= 0)
        {
            return GenericResponse<bool>.CreateError(
                "Prompt quota has been exceeded for the current subscription.",
                HttpStatusCode.Forbidden,
                "PROMPT_QUOTA_EXCEEDED");
        }

        return GenericResponse<bool>.CreateSuccess(true);
    }

    public async Task<GenericResponse<bool>> EnsurePostQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(profileId, cancellationToken);
        if (!summary.Success)
        {
            return GenericResponse<bool>.CreateError(summary.Message ?? "Unable to resolve post quota.", (HttpStatusCode)summary.StatusCode, summary.Error?.ErrorCode);
        }

        if (summary.Data!.PostRemaining <= 0)
        {
            return GenericResponse<bool>.CreateError(
                "Post quota has been exceeded for the current subscription.",
                HttpStatusCode.Forbidden,
                "POST_QUOTA_EXCEEDED");
        }

        return GenericResponse<bool>.CreateSuccess(true);
    }

    public async Task<GenericResponse<bool>> EnsureWorkspacePostQuotaAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var summary = await GetWorkspaceSummaryAsync(workspaceId, cancellationToken);
        if (!summary.Success)
        {
            return GenericResponse<bool>.CreateError(summary.Message ?? "Unable to resolve workspace post quota.", (HttpStatusCode)summary.StatusCode, summary.Error?.ErrorCode);
        }

        if (summary.Data!.PostRemaining <= 0)
        {
            return GenericResponse<bool>.CreateError(
                "Post quota has been exceeded for the current subscription.",
                HttpStatusCode.Forbidden,
                "POST_QUOTA_EXCEEDED");
        }

        return GenericResponse<bool>.CreateSuccess(true);
    }

    private async Task<IReadOnlyList<Profile>> GetActiveWorkspaceProfilesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        var profiles = new List<Profile>();
        foreach (var userId in workspace.Members.Where(member => member.IsActive).Select(member => member.UserId).Distinct())
        {
            var userProfiles = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
            profiles.AddRange(userProfiles);
        }

        return profiles
            .GroupBy(profile => profile.Id)
            .Select(group => group.First())
            .ToList();
    }

    private static (DateTime WindowStart, DateTime? WindowEnd) ResolvePostQuotaWindow(Subscription subscription, DateTime utcToday)
    {
        if (subscription.Plan != SubscriptionPlanEnum.Free)
        {
            return (subscription.StartDate.Date, subscription.EndDate?.Date);
        }

        var start = subscription.StartDate.Date;
        if (utcToday < start)
        {
            return (start, start.AddDays(6));
        }

        var elapsedDays = (utcToday - start).Days;
        var cycleOffset = (elapsedDays / 7) * 7;
        var windowStart = start.AddDays(cycleOffset);
        return (windowStart, windowStart.AddDays(6));
    }

    private static int ResolveWorkspacePostQuota(WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan)
    {
        return (workspaceType, plan) switch
        {
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Free) => 20,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Plus) => 300,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Premium) => 1_000,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.PlusTrial) => 300,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Plus) => 5_000,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Premium) => 20_000,
            _ => subscriptionFallback(plan)
        };

        static int subscriptionFallback(SubscriptionPlanEnum subscriptionPlan)
        {
            return subscriptionPlan switch
            {
                SubscriptionPlanEnum.Free => 20,
                SubscriptionPlanEnum.Plus or SubscriptionPlanEnum.PlusTrial => 300,
                SubscriptionPlanEnum.Premium => 1_000,
                _ => 0
            };
        }
    }

    private static string BuildWorkspacePlanName(WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan)
    {
        return (workspaceType, plan) switch
        {
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Free) => "Free",
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Plus) => "Personal Plus",
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Premium) => "Personal Pro",
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.PlusTrial) => "Personal Plus Trial",
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Plus) => "Business Plus",
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Premium) => "Business Pro",
            _ => plan.ToString()
        };
    }
}
