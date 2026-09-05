using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Service;

public sealed class WorkspaceDashboardService : IWorkspaceDashboardService
{
    private readonly ICreditUsageRecordRepository _creditUsageRecordRepository;
    private readonly IPostRepository _postRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IQuotaService _quotaService;
    private readonly ICreditService _creditService;
    private readonly AisamContext? _db;

    public WorkspaceDashboardService(
        ICreditUsageRecordRepository creditUsageRecordRepository,
        IPostRepository postRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IQuotaService quotaService,
        ICreditService creditService,
        AisamContext? db = null)
    {
        _creditUsageRecordRepository = creditUsageRecordRepository;
        _postRepository = postRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _quotaService = quotaService;
        _creditService = creditService;
        _db = db;
    }

    public async Task<GenericResponse<WorkspaceDashboardSummaryDto>> GetSummaryAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        if (_db?.AccessScope.Enforced == true &&
            (_db.AccessScope.WorkspaceId != workspaceId || !_db.AccessScope.CanViewAggregate))
            return GenericResponse<WorkspaceDashboardSummaryDto>.CreateError("Access denied.", HttpStatusCode.Forbidden);
        // OQ-007 does not yet authorize Workspace wallet/billing fields for other roles.
        // Do not fetch or materialize those fields for a non-Owner dashboard.
        var canViewBilling = _db?.AccessScope.Enforced != true || _db.AccessScope.IsOwner;
        var wallet = canViewBilling ? await _creditService.EnsureCurrentFreeCreditsAsync(workspaceId, cancellationToken: cancellationToken) : null;
        var usage = await _creditUsageRecordRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        var members = _db?.AccessScope.Enforced == true
            ? await _db.WorkspaceMembers.AsNoTracking()
                .Where(m => m.WorkspaceId == workspaceId && m.IsActive &&
                    (_db.AccessScope.IsOwner || _db.AccessScope.MemberIds.Contains(m.UserId)))
                .Select(m => new WorkspaceTopMemberDto { UserId = m.UserId, Name = m.User.FullName ?? m.User.Email, Email = m.User.Email })
                .ToListAsync(cancellationToken)
            : (await _workspaceMemberRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken))
                .Select(m => new WorkspaceTopMemberDto { UserId = m.UserId, Name = m.User.FullName ?? m.User.Email, Email = m.User.Email }).ToList();
        var posts = await _postRepository.GetPagedByWorkspaceIdAsync(
            workspaceId,
            new PaginationRequest { Page = 1, PageSize = 1 },
            status: ContentStatusEnum.Published,
            cancellationToken: cancellationToken);
        var quota = canViewBilling ? await _quotaService.GetWorkspaceSummaryAsync(workspaceId, cancellationToken) : null;
        if (quota != null && !quota.Success)
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
                    Name = member.Name,
                    Email = member.Email,
                    CreditsUsed = memberUsage?.Credits ?? 0,
                    AiUsageCount = memberUsage?.Count ?? 0
                };
            })
            .OrderByDescending(member => member.CreditsUsed)
            .ThenByDescending(member => member.AiUsageCount)
            .ThenBy(member => member.Email)
            .Take(5)
            .ToList();

        long? maxBalanceCap = canViewBilling ? await _creditService.GetMaximumBalanceAsync(workspaceId, cancellationToken) : null;

        return GenericResponse<WorkspaceDashboardSummaryDto>.CreateSuccess(
            new WorkspaceDashboardSummaryDto
            {
                WorkspaceId = workspaceId,
                CreditBalance = canViewBilling ? wallet?.Balance ?? 0 : null,
                CreditsUsed = consumptionUsage.Sum(record => record.Credits),
                MaxBalanceCap = maxBalanceCap,
                PublishedPostCount = posts.TotalCount,
                PostQuotaLimit = quota?.Data?.PostQuotaLimit,
                PostsRemaining = quota?.Data?.PostRemaining,
                AiUsageCount = consumptionUsage.Count,
                ActiveMemberCount = members.Count,
                TopMembers = topMembers
            },
            "Workspace dashboard summary retrieved successfully.");
    }

    private static bool IsConsumption(CreditActionEnum action)
        => action is not CreditActionEnum.SubscriptionGrant and not CreditActionEnum.CreditPackGrant;
}
