using System.Net;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class CreditService : ICreditService
{
    private const long PersonalMaximumBalance = 15_000;
    private const long BusinessMaximumBalance = 500_000;

    private readonly ICreditWalletRepository _creditWalletRepository;
    private readonly ICreditUsageRecordRepository _creditUsageRecordRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IWorkspaceRepository _workspaceRepository;

    public CreditService(
        ICreditWalletRepository creditWalletRepository,
        ICreditUsageRecordRepository creditUsageRecordRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IWorkspaceRepository workspaceRepository)
    {
        _creditWalletRepository = creditWalletRepository;
        _creditUsageRecordRepository = creditUsageRecordRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _workspaceRepository = workspaceRepository;
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
            cancellationToken);
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

        var wallet = await EnsureWalletAsync(workspaceId, cancellationToken);
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

        if (wallet.Balance < credits)
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

    private static long ResolvePlanCredits(WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan)
    {
        return (workspaceType, plan) switch
        {
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Free) => 50,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Plus) => 500,
            (WorkspaceTypeEnum.Personal, SubscriptionPlanEnum.Premium) => 2_000,
            (WorkspaceTypeEnum.Business, SubscriptionPlanEnum.Plus) => 15_000,
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

    private async Task<GenericResponse<CreditWallet>> GrantCreditsAsync(
        Guid workspaceId,
        Guid userId,
        WorkspaceTypeEnum workspaceType,
        long credits,
        CreditActionEnum action,
        string successMessage,
        CancellationToken cancellationToken)
    {
        var walletToUpdate = await EnsureWalletAsync(workspaceId, cancellationToken);
        var maximumBalance = ResolveMaximumBalance(workspaceType);
        if (walletToUpdate.Balance + credits > maximumBalance)
        {
            return GenericResponse<CreditWallet>.CreateError(
                "Wallet balance exceeds workspace maximum balance.",
                HttpStatusCode.BadRequest,
                "CREDIT_BALANCE_LIMIT_EXCEEDED");
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
}
