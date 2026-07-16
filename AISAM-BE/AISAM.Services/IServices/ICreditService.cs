using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;

namespace AISAM.Services.IServices;

public interface ICreditService
{
    Task<CreditWallet> EnsureWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<CreditWallet> EnsureCurrentFreeCreditsAsync(Guid workspaceId, DateTime? now = null, CancellationToken cancellationToken = default)
        => EnsureWalletAsync(workspaceId, cancellationToken);
    Task<GenericResponse<CreditWallet>> GrantSubscriptionCreditsAsync(
        Guid workspaceId,
        Guid userId,
        WorkspaceTypeEnum workspaceType,
        SubscriptionPlanEnum plan,
        CancellationToken cancellationToken = default);
    Task<GenericResponse<CreditWallet>> GrantCreditPackCreditsAsync(
        Guid workspaceId,
        Guid userId,
        WorkspaceTypeEnum workspaceType,
        long credits,
        CancellationToken cancellationToken = default);
    Task<GenericResponse<CreditWallet>> AdminAdjustCreditsAsync(
        Guid workspaceId,
        Guid adminUserId,
        long amount,
        string reason,
        CancellationToken cancellationToken = default);
    Task<GenericResponse<CreditUsageRecord>> ConsumeCreditsAsync(
        Guid workspaceId,
        Guid userId,
        CreditActionEnum action,
        long credits,
        Guid? aiGenerationId = null,
        DateTime? now = null,
        CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> EnsureCreditsAvailableAsync(
        Guid workspaceId,
        Guid userId,
        long credits,
        DateTime? now = null,
        CancellationToken cancellationToken = default);
    Task<GenericResponse<CreditUsageRecord>> RecordUsageAsync(
        Guid workspaceId,
        Guid userId,
        CreditActionEnum action,
        long credits,
        CreditUsageStatusEnum status,
        Guid? aiGenerationId = null,
        CancellationToken cancellationToken = default);

    Task<CreditWallet?> GetWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyCreditUsageDto>> GetDailyUsageAsync(Guid workspaceId, int days, CancellationToken cancellationToken = default);

    Task<PagedResult<CreditUsageRecordDto>> GetPagedUsageAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default);
}
