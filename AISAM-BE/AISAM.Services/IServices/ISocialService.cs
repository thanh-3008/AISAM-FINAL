using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface ISocialService
{
    Task<AuthUrlResponse> GetAuthUrlAsync(string provider, Guid profileId, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> LinkAccountAsync(string provider, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialAccountDto>> GetProfileAccountsAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsForAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> LinkSelectedTargetsForAccountAsync(Guid profileId, Guid socialAccountId, LinkSelectedTargetsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialTargetDto>> GetLinkedTargetsAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<bool> UnlinkAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<bool> UnlinkTargetAsync(Guid profileId, Guid socialIntegrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialIntegrationDto>> GetIntegrationsByBrandAsync(Guid profileId, Guid brandId, CancellationToken cancellationToken = default);
    Task<SocialAccountDto?> GetSocialAccountByIdAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> LinkAccountInWorkspaceAsync(string provider, Guid workspaceId, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialAccountDto>> GetWorkspaceAccountsAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> LinkSelectedTargetsInWorkspaceAsync(Guid workspaceId, Guid profileId, Guid socialAccountId, LinkSelectedTargetsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialTargetDto>> GetLinkedTargetsInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<bool> UnlinkAccountInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<bool> UnlinkTargetInWorkspaceAsync(Guid workspaceId, Guid socialIntegrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialIntegrationDto>> GetIntegrationsByBrandInWorkspaceAsync(Guid workspaceId, Guid brandId, CancellationToken cancellationToken = default);
}
