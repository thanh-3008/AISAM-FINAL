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
    Task<SocialAccountDto> LinkAccountInWorkspaceAsync(string provider, Guid workspaceId, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default)
        => LinkAccountAsync(provider, profileId, request, cancellationToken);
    Task<IReadOnlyList<SocialAccountDto>> GetWorkspaceAccountsAsync(Guid workspaceId, CancellationToken cancellationToken = default) => GetProfileAccountsAsync(workspaceId, cancellationToken);
    Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default) => ListAvailableTargetsForAccountAsync(workspaceId, socialAccountId, cancellationToken);
    Task<SocialAccountDto> LinkSelectedTargetsInWorkspaceAsync(Guid workspaceId, Guid profileId, Guid socialAccountId, LinkSelectedTargetsRequest request, CancellationToken cancellationToken = default) => LinkSelectedTargetsForAccountAsync(profileId, socialAccountId, request, cancellationToken);
    Task<IReadOnlyList<SocialTargetDto>> GetLinkedTargetsInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default) => GetLinkedTargetsAsync(workspaceId, socialAccountId, cancellationToken);
    Task<bool> UnlinkAccountInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default) => UnlinkAccountAsync(workspaceId, socialAccountId, cancellationToken);
    Task<bool> UnlinkTargetInWorkspaceAsync(Guid workspaceId, Guid socialIntegrationId, CancellationToken cancellationToken = default) => UnlinkTargetAsync(workspaceId, socialIntegrationId, cancellationToken);
    Task<IReadOnlyList<SocialIntegrationDto>> GetIntegrationsByBrandInWorkspaceAsync(Guid workspaceId, Guid brandId, CancellationToken cancellationToken = default) => GetIntegrationsByBrandAsync(workspaceId, brandId, cancellationToken);
    Task<IReadOnlyList<FacebookAdAccountData>> GetAdAccountsForSocialAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FacebookAdAccountData>> GetAdAccountsForSocialAccountInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default) => GetAdAccountsForSocialAccountAsync(workspaceId, socialAccountId, cancellationToken);
    Task<string?> GetFacebookUserAccessTokenAsync(Guid profileId, CancellationToken cancellationToken = default);
}
