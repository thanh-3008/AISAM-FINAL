using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class WorkspaceDashboardService : IWorkspaceDashboardService
{
    private readonly ICreditUsageRecordRepository _creditUsageRecordRepository;
    private readonly IPostRepository _postRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IQuotaService _quotaService;
    private readonly ICreditService _creditService;

    public WorkspaceDashboardService(
        ICreditUsageRecordRepository creditUsageRecordRepository,
        IPostRepository postRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IQuotaService quotaService,
        ICreditService creditService)
    {
        _creditUsageRecordRepository = creditUsageRecordRepository;
        _postRepository = postRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _quotaService = quotaService;
        _creditService = creditService;
    }

    public async Task<GenericResponse<WorkspaceDashboardSummaryDto>> GetSummaryAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _creditService.EnsureCurrentFreeCreditsAsync(workspaceId, cancellationToken: cancellationToken);
        var usage = await _creditUsageRecordRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        var members = await _workspaceMemberRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        var posts = await _postRepository.GetPagedByWorkspaceIdAsync(
            workspaceId,
            new PaginationRequest { Page = 1, PageSize = 1 },
            status: ContentStatusEnum.Published,
            cancellationToken: cancellationToken);
        var quota = await _quotaService.GetWorkspaceSummaryAsync(workspaceId, cancellationToken);
        if (!quota.Success)
        {
            return GenericResponse<WorkspaceDashboardSummaryDto>.CreateError(
                quota.Message ?? "Unable to resolve workspace post quota.",
                (HttpStatusCode)quota.StatusCode,
                quota.Error?.ErrorCode);
        }

        var successfulUsage = usage.Where(record => record.Status == CreditUsageStatusEnum.Success).ToList();
        var consumptionUsage = successfulUsage.Where(record => IsConsumption(record.Action)).ToList();
        var usageByUser = consumptionUsage
            .GroupBy(record => record.UserId)
            .ToDictionary(
                group => group.Key,
                group => new { Credits = group.Sum(record => record.Credits), Count = group.Count() });

        var topMembers = members
            .Select(member =>
            {
                usageByUser.TryGetValue(member.UserId, out var memberUsage);
                return new WorkspaceTopMemberDto
                {
                    UserId = member.UserId,
                    Name = member.User.FullName ?? member.User.Email,
                    Email = member.User.Email,
                    CreditsUsed = memberUsage?.Credits ?? 0,
                    AiUsageCount = memberUsage?.Count ?? 0
                };
            })
            .OrderByDescending(member => member.CreditsUsed)
            .ThenByDescending(member => member.AiUsageCount)
            .ThenBy(member => member.Email)
            .Take(5)
            .ToList();

        return GenericResponse<WorkspaceDashboardSummaryDto>.CreateSuccess(
            new WorkspaceDashboardSummaryDto
            {
                WorkspaceId = workspaceId,
                CreditBalance = wallet?.Balance ?? 0,
                CreditsUsed = consumptionUsage.Sum(record => record.Credits),
                PublishedPostCount = posts.TotalCount,
                PostQuotaLimit = quota.Data!.PostQuotaLimit,
                PostsRemaining = quota.Data.PostRemaining,
                AiUsageCount = consumptionUsage.Count,
                ActiveMemberCount = members.Count,
                TopMembers = topMembers
            },
            "Workspace dashboard summary retrieved successfully.");
    }

    private static bool IsConsumption(CreditActionEnum action)
        => action is not CreditActionEnum.SubscriptionGrant and not CreditActionEnum.CreditPackGrant;
}
