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
    private readonly IWorkspaceRepository _workspaceRepository;

    public CreditService(
        ICreditWalletRepository creditWalletRepository,
        ICreditUsageRecordRepository creditUsageRecordRepository,
        IWorkspaceRepository workspaceRepository)
    {
        _creditWalletRepository = creditWalletRepository;
        _creditUsageRecordRepository = creditUsageRecordRepository;
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

        var walletToUpdate = await EnsureWalletAsync(workspaceId, cancellationToken);
        var maximumBalance = ResolveMaximumBalance(workspaceType);
        if (walletToUpdate.Balance + creditsToGrant > maximumBalance)
        {
            return GenericResponse<CreditWallet>.CreateError(
                "Wallet balance exceeds workspace maximum balance.",
                HttpStatusCode.BadRequest,
                "CREDIT_BALANCE_LIMIT_EXCEEDED");
        }

        walletToUpdate.Balance += creditsToGrant;
        await _creditWalletRepository.UpdateAsync(walletToUpdate, cancellationToken);

        await _creditUsageRecordRepository.AddAsync(new CreditUsageRecord
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Action = CreditActionEnum.SubscriptionGrant,
            Credits = creditsToGrant,
            Status = CreditUsageStatusEnum.Success
        }, cancellationToken);

        return GenericResponse<CreditWallet>.CreateSuccess(walletToUpdate, "Subscription credits granted successfully.");
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
}
