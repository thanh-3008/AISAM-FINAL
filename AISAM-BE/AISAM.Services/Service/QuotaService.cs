using System.Net;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class QuotaService : IQuotaService
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public QuotaService(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
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
}
