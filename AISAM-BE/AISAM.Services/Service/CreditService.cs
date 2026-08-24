using System.Net;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories;
using AISAM.Services.IServices;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Service;

public sealed class CreditService : ICreditService
{
    private const long PersonalMaximumBalance = 15_000;
    private const long BusinessMaximumBalance = 500_000;

    private readonly ICreditWalletRepository _creditWalletRepository;
    private readonly ICreditUsageRecordRepository _creditUsageRecordRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly AisamContext _context;

    public CreditService(
        ICreditWalletRepository creditWalletRepository,
        ICreditUsageRecordRepository creditUsageRecordRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IWorkspaceRepository workspaceRepository,
        AisamContext context)
    {
        _creditWalletRepository = creditWalletRepository;
        _creditUsageRecordRepository = creditUsageRecordRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _workspaceRepository = workspaceRepository;
        _context = context;
    }

    public async Task<CreditWallet> EnsureWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var existingWallet = await _creditWalletRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        if (existingWallet != null)
        {
            return existingWallet;
        }

        return await _creditWalletRepository.AddAsync(new CreditWallet
        {
            WorkspaceId = workspaceId,
            Balance = 0
        }, cancellationToken);
    }

    public async Task<CreditWallet> EnsureCurrentFreeCreditsAsync(
        Guid workspaceId,
        DateTime? now = null,
        CancellationToken cancellationToken = default)
        => await ExecuteInTransactionAsync(
            () => EnsureCurrentFreeCreditsCoreAsync(workspaceId, now, cancellationToken),
            cancellationToken);

    public async Task<GenericResponse<CreditWallet>> GrantSubscriptionCreditsAsync(
        Guid workspaceId,
        Guid userId,
        WorkspaceTypeEnum workspaceType,
        SubscriptionPlanEnum plan,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<CreditWallet>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var creditsToGrant = ResolvePlanCredits(workspaceType, plan);
        if (creditsToGrant <= 0)
        {
            var wallet = await EnsureWalletAsync(workspaceId, cancellationToken);
            return GenericResponse<CreditWallet>.CreateSuccess(wallet, "No credits granted for the selected plan.");
        }

        return await GrantCreditsAsync(
            workspaceId,
            userId,
            workspaceType,
            creditsToGrant,
            CreditActionEnum.SubscriptionGrant,
            "Subscription credits granted successfully.",
            cancellationToken);
    }

    public async Task<GenericResponse<CreditWallet>> GrantCreditPackCreditsAsync(
        Guid workspaceId,
        Guid userId,
        WorkspaceTypeEnum workspaceType,
        long credits,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<CreditWallet>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        if (credits <= 0)
        {
            return GenericResponse<CreditWallet>.CreateError("Credit pack amount is invalid.", HttpStatusCode.BadRequest, "INVALID_CREDIT_PACK");
        }

        return await GrantCreditsAsync(
            workspaceId,
            userId,
            workspaceType,
            credits,
            CreditActionEnum.CreditPackGrant,
            "Credit pack applied successfully.",
            cancellationToken,
            bypassCap: true);
    }

    public async Task<GenericResponse<CreditWallet>> AdminAdjustCreditsAsync(
        Guid workspaceId,
        Guid adminUserId,
        long amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
            if (workspace == null)
            {
                return GenericResponse<CreditWallet>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
            }

            var wallet = await EnsureWalletAsync(workspaceId, cancellationToken);
            
            if (amount < 0 && wallet.Balance < Math.Abs(amount))
            {
                return GenericResponse<CreditWallet>.CreateError("Insufficient credits to deduct.", HttpStatusCode.BadRequest);
            }

            var maxBalance = workspace.WorkspaceType == WorkspaceTypeEnum.Personal ? PersonalMaximumBalance : BusinessMaximumBalance;
            if (amount > 0 && wallet.Balance + amount > maxBalance)
            {
                return GenericResponse<CreditWallet>.CreateError($"Cannot exceed maximum balance of {maxBalance}.", HttpStatusCode.BadRequest);
            }

            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;
            await _creditWalletRepository.UpdateAsync(wallet, cancellationToken);

            await _creditUsageRecordRepository.AddAsync(new CreditUsageRecord
            {
                WorkspaceId = workspaceId,
                UserId = adminUserId,
                Action = CreditActionEnum.AdminAdjust,
                Credits = amount,
                Status = CreditUsageStatusEnum.Success,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            return GenericResponse<CreditWallet>.CreateSuccess(wallet, $"Credit adjusted successfully: {reason}");
        }, cancellationToken);
    }

    public async Task<GenericResponse<CreditUsageRecord>> RecordUsageAsync(
        Guid workspaceId,
        Guid userId,
        CreditActionEnum action,
        long credits,
        CreditUsageStatusEnum status,
        Guid? aiGenerationId = null,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<CreditUsageRecord>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var record = await _creditUsageRecordRepository.AddAsync(new CreditUsageRecord
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            AiGenerationId = aiGenerationId,
            Action = action,
            Credits = credits,
            Status = status
        }, cancellationToken);

        return GenericResponse<CreditUsageRecord>.CreateSuccess(record, "Credit usage metadata recorded successfully.");
    }

    public async Task<GenericResponse<CreditUsageRecord>> ConsumeCreditsAsync(
        Guid workspaceId,
        Guid userId,
        CreditActionEnum action,
        long credits,
        Guid? aiGenerationId = null,
        DateTime? now = null,
        CancellationToken cancellationToken = default)
        => await ExecuteInTransactionAsync(
            () => ConsumeCreditsCoreAsync(workspaceId, userId, action, credits, aiGenerationId, now, cancellationToken),
            cancellationToken);

    private async Task<GenericResponse<CreditUsageRecord>> ConsumeCreditsCoreAsync(
        Guid workspaceId,
        Guid userId,
        CreditActionEnum action,
        long credits,
        Guid? aiGenerationId,
        DateTime? now,
        CancellationToken cancellationToken)
    {
        if (credits <= 0)
        {
            return GenericResponse<CreditUsageRecord>.CreateError(
                "Credits to consume must be greater than zero.",
                HttpStatusCode.BadRequest,
                "INVALID_CREDIT_AMOUNT");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<CreditUsageRecord>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var member = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);
        if (member == null || !member.IsActive)
        {
            return GenericResponse<CreditUsageRecord>.CreateError(
                "Active workspace member not found.",
                HttpStatusCode.Forbidden,
                "WORKSPACE_MEMBER_NOT_FOUND");
        }

        var wallet = await EnsureCurrentFreeCreditsCoreAsync(workspaceId, now, cancellationToken);
        var utcNow = (now ?? DateTime.UtcNow).Date;

        ResetMonthlyUsageIfNeeded(member, utcNow);

        if (member.QuotaMode != MemberQuotaModeEnum.SharedPool)
        {
            if (!member.CreditLimit.HasValue)
            {
                return GenericResponse<CreditUsageRecord>.CreateError(
                    "Assigned member quota is not configured correctly.",
                    HttpStatusCode.BadRequest,
                    "INVALID_MEMBER_CREDIT_LIMIT");
            }

            if (member.CreditUsed + credits > member.CreditLimit.Value)
            {
                await _creditUsageRecordRepository.AddAsync(new CreditUsageRecord
                {
                    WorkspaceId = workspaceId,
                    UserId = userId,
                    AiGenerationId = aiGenerationId,
                    Action = action,
                    Credits = credits,
                    Status = CreditUsageStatusEnum.Failed
                }, cancellationToken);

                return GenericResponse<CreditUsageRecord>.CreateError(
                    "Member credit limit exceeded.",
                    HttpStatusCode.BadRequest,
                    "MEMBER_CREDIT_LIMIT_EXCEEDED");
            }
        }

        if (wallet.Balance - wallet.ReservedBalance < credits)
        {
            await _creditUsageRecordRepository.AddAsync(new CreditUsageRecord
            {
                WorkspaceId = workspaceId,
                UserId = userId,
                AiGenerationId = aiGenerationId,
                Action = action,
                Credits = credits,
                Status = CreditUsageStatusEnum.Failed
            }, cancellationToken);

            return GenericResponse<CreditUsageRecord>.CreateError(
                "Workspace does not have enough credits.",
                HttpStatusCode.BadRequest,
                "INSUFFICIENT_WORKSPACE_CREDITS");
        }

        wallet.Balance -= credits;
        await _creditWalletRepository.UpdateAsync(wallet, cancellationToken);

        if (member.QuotaMode != MemberQuotaModeEnum.SharedPool)
        {
            member.CreditUsed += credits;
            await _workspaceMemberRepository.UpdateAsync(member, cancellationToken);
        }

        var record = await _creditUsageRecordRepository.AddAsync(new CreditUsageRecord
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            AiGenerationId = aiGenerationId,
            Action = action,
            Credits = credits,
            Status = CreditUsageStatusEnum.Success
        }, cancellationToken);

        return GenericResponse<CreditUsageRecord>.CreateSuccess(record, "Credits consumed successfully.");
    }

    public async Task<GenericResponse<bool>> EnsureCreditsAvailableAsync(
        Guid workspaceId,
        Guid userId,
        long credits,
        DateTime? now = null,
        CancellationToken cancellationToken = default)
    {
        if (credits <= 0)
        {
            return GenericResponse<bool>.CreateError(
                "Credits to consume must be greater than zero.",
                HttpStatusCode.BadRequest,
                "INVALID_CREDIT_AMOUNT");
        }

        if (await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken) == null)
        {
            return GenericResponse<bool>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var member = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);
        if (member == null || !member.IsActive)
        {
            return GenericResponse<bool>.CreateError(
                "Active workspace member not found.",
                HttpStatusCode.Forbidden,
                "WORKSPACE_MEMBER_NOT_FOUND");
        }

        ResetMonthlyUsageIfNeeded(member, (now ?? DateTime.UtcNow).Date);

        if (member.QuotaMode != MemberQuotaModeEnum.SharedPool)
        {
            if (!member.CreditLimit.HasValue)
            {
                return GenericResponse<bool>.CreateError(
                    "Assigned member quota is not configured correctly.",
                    HttpStatusCode.BadRequest,
                    "INVALID_MEMBER_CREDIT_LIMIT");
            }

            if (member.CreditUsed + credits > member.CreditLimit.Value)
            {
                return GenericResponse<bool>.CreateError(
                    "Member credit limit exceeded.",
                    HttpStatusCode.BadRequest,
                    "MEMBER_CREDIT_LIMIT_EXCEEDED");
            }
        }

        var wallet = await EnsureCurrentFreeCreditsAsync(workspaceId, now, cancellationToken);
        return wallet.Balance - wallet.ReservedBalance < credits
            ? GenericResponse<bool>.CreateError(
                "Workspace does not have enough credits.",
                HttpStatusCode.BadRequest,
                "INSUFFICIENT_WORKSPACE_CREDITS")
            : GenericResponse<bool>.CreateSuccess(true);
    }

    private static long ResolvePlanCredits(WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan)
    {
        return (workspaceType, plan) switch
        {
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Free) => 50,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Plus) => 500,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Premium) => 2_000,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.PlusTrial) => 100,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Plus) => 15_000,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.PlusTrial) => 1_000,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Premium) => 50_000,
            _ => 0
        };
    }

    private static long ResolveMaximumBalance(WorkspaceTypeEnum workspaceType)
    {
        return workspaceType == WorkspaceTypeEnum.Business
            ? BusinessMaximumBalance
            : PersonalMaximumBalance;
    }

    public async Task<long> GetMaximumBalanceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        return ResolveMaximumBalance(workspace?.WorkspaceType ?? WorkspaceTypeEnum.Personal);
    }

    private async Task<GenericResponse<CreditWallet>> GrantCreditsAsync(
        Guid workspaceId,
        Guid userId,
        WorkspaceTypeEnum workspaceType,
        long credits,
        CreditActionEnum action,
        string successMessage,
        CancellationToken cancellationToken,
        bool bypassCap = false)
        => await ExecuteInTransactionAsync(
            () => GrantCreditsCoreAsync(workspaceId, userId, workspaceType, credits, action, successMessage, cancellationToken, bypassCap),
            cancellationToken);

    private async Task<GenericResponse<CreditWallet>> GrantCreditsCoreAsync(
        Guid workspaceId,
        Guid userId,
        WorkspaceTypeEnum workspaceType,
        long credits,
        CreditActionEnum action,
        string successMessage,
        CancellationToken cancellationToken,
        bool bypassCap = false)
    {
        var walletToUpdate = await EnsureWalletAsync(workspaceId, cancellationToken);
        if (!bypassCap)
        {
            var maximumBalance = ResolveMaximumBalance(workspaceType);
            if (walletToUpdate.Balance + credits > maximumBalance)
            {
                return GenericResponse<CreditWallet>.CreateError(
                    "Wallet balance exceeds workspace maximum balance.",
                    HttpStatusCode.BadRequest,
                    "CREDIT_BALANCE_LIMIT_EXCEEDED");
            }
        }

        walletToUpdate.Balance += credits;
        await _creditWalletRepository.UpdateAsync(walletToUpdate, cancellationToken);

        await _creditUsageRecordRepository.AddAsync(new CreditUsageRecord
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Action = action,
            Credits = credits,
            Status = CreditUsageStatusEnum.Success
        }, cancellationToken);

        return GenericResponse<CreditWallet>.CreateSuccess(walletToUpdate, successMessage);
    }

    private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        if (!_context.Database.IsRelational() || _context.Database.CurrentTransaction != null)
        {
            return await action();
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                var result = await action();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private async Task<CreditWallet> EnsureCurrentFreeCreditsCoreAsync(
        Guid workspaceId,
        DateTime? now,
        CancellationToken cancellationToken)
    {
        var wallet = await EnsureWalletAsync(workspaceId, cancellationToken);
        var utcDate = (now ?? DateTime.UtcNow).Date;
        var subscription = await _context.Subscriptions
            .Where(item =>
                item.WorkspaceId == workspaceId &&
                item.Plan == SubscriptionPlanEnum.Free &&
                item.IsActive &&
                !item.IsDeleted &&
                item.StartDate <= utcDate &&
                (!item.EndDate.HasValue || item.EndDate.Value >= utcDate))
            .OrderByDescending(item => item.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
        if (subscription == null)
        {
            return wallet;
        }

        var start = subscription.StartDate.Date;
        var cycleStart = start.AddDays(((utcDate - start).Days / 7) * 7);
        var latestGrant = await _context.CreditUsageRecords
            .Where(record =>
                record.WorkspaceId == workspaceId &&
                record.Action == CreditActionEnum.SubscriptionGrant &&
                record.Status == CreditUsageStatusEnum.Success)
            .OrderByDescending(record => record.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (latestGrant?.CreatedAt.Date >= cycleStart)
        {
            return wallet;
        }

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken)
            ?? throw new InvalidOperationException("Workspace not found while refreshing free credits.");
        var owner = workspace.Members.FirstOrDefault(member =>
            member.IsActive && member.Role == WorkspaceMemberRoleEnum.Owner)
            ?? throw new InvalidOperationException("Active workspace owner not found while refreshing free credits.");

        long creditsGranted = 0;
        if (wallet.Balance < 50)
        {
            creditsGranted = 50 - wallet.Balance;
            wallet.Balance = 50;
            await _creditWalletRepository.UpdateAsync(wallet, cancellationToken);
        }

        await _creditUsageRecordRepository.AddAsync(new CreditUsageRecord
        {
            WorkspaceId = workspaceId,
            UserId = owner.UserId,
            Action = CreditActionEnum.SubscriptionGrant,
            Credits = creditsGranted,
            Status = CreditUsageStatusEnum.Success,
            CreatedAt = utcDate
        }, cancellationToken);
        return wallet;
    }

    private static void ResetMonthlyUsageIfNeeded(WorkspaceMember member, DateTime utcDate)
    {
        if (member.QuotaMode != MemberQuotaModeEnum.MonthlyAssignedLimit)
        {
            return;
        }

        var currentMonthStart = new DateTime(utcDate.Year, utcDate.Month, 1);
        if (member.CreditPeriodStart == currentMonthStart)
        {
            return;
        }

        member.CreditUsed = 0;
        member.CreditPeriodStart = currentMonthStart;
    }

    public async Task<CreditWallet?> GetWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _creditWalletRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyCreditUsageDto>> GetDailyUsageAsync(Guid workspaceId, int days, CancellationToken cancellationToken = default)
    {
        return await _creditUsageRecordRepository.GetDailyUsageAsync(workspaceId, days, cancellationToken);
    }

    public async Task<PagedResult<CreditUsageRecordDto>> GetPagedUsageAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _creditUsageRecordRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, cancellationToken);
        return new PagedResult<CreditUsageRecordDto>
        {
            Data = result.Data.Select(MapRecord).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<PagedResult<CreditUsageRecordDto>> GetPagedUsageByUserAsync(Guid workspaceId, Guid userId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _creditUsageRecordRepository.GetPagedByWorkspaceAndUserIdAsync(workspaceId, userId, request, cancellationToken);
        return new PagedResult<CreditUsageRecordDto>
        {
            Data = result.Data.Select(MapRecord).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    private static CreditUsageRecordDto MapRecord(Data.Model.CreditUsageRecord record)
    {
        return new CreditUsageRecordDto
        {
            Id = record.Id,
            UserId = record.UserId,
            UserName = record.User?.FullName ?? record.User?.Email ?? "Unknown",
            Action = MapActionName(record.Action),
            FeatureUsed = MapFeatureUsed(record.Action),
            Credits = record.Credits,
            Status = record.Status == CreditUsageStatusEnum.Success ? "Success" : "Failed",
            CreatedAt = record.CreatedAt
        };
    }

    private static string MapActionName(CreditActionEnum action) => action switch
    {
        CreditActionEnum.SubscriptionGrant => "Subscription Grant",
        CreditActionEnum.CreditPackGrant => "Credit Pack Purchase",
        CreditActionEnum.GenerateText => "Generate Text",
        CreditActionEnum.RegenerateText => "Regenerate Text",
        CreditActionEnum.GenerateImage => "Generate Image",
        CreditActionEnum.GenerateVideo => "Generate Video",
        CreditActionEnum.TrendAnalysis => "Trend Analysis",
        CreditActionEnum.CampaignRecommendation => "Campaign Recommendation",
        _ => action.ToString()
    };

    private static string MapFeatureUsed(CreditActionEnum action) => action switch
    {
        CreditActionEnum.SubscriptionGrant => "Subscription",
        CreditActionEnum.CreditPackGrant => "Credit Pack",
        CreditActionEnum.GenerateText => "AI Content",
        CreditActionEnum.RegenerateText => "AI Content",
        CreditActionEnum.GenerateImage => "AI Image",
        CreditActionEnum.GenerateVideo => "AI Video",
        CreditActionEnum.TrendAnalysis => "Analytics",
        CreditActionEnum.CampaignRecommendation => "Campaign",
        _ => "Other"
    };
}
