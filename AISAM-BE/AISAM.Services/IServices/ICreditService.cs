using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Services.IServices;

public interface ICreditService
{
    Task<CreditWallet> EnsureWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default);
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
}
