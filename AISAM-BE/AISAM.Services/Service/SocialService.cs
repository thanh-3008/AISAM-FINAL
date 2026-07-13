using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class SocialService : ISocialService
{
    private readonly ISocialAccountRepository _socialAccountRepository;
    private readonly ISocialIntegrationRepository _socialIntegrationRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IOAuthStateStore _oauthStateStore;
    private readonly ISocialTokenProtector _tokenProtector;
    private readonly FacebookSettings _facebookSettings;
    private readonly InstagramSettings _instagramSettings;
    private readonly TikTokSettings _tikTokSettings;
    private readonly Dictionary<string, IProviderService> _providers;

    public SocialService(
        ISocialAccountRepository socialAccountRepository,
        ISocialIntegrationRepository socialIntegrationRepository,
        IBrandRepository brandRepository,
        IOAuthStateStore oauthStateStore,
        ISocialTokenProtector tokenProtector,
        IOptions<FacebookSettings> facebookSettings,
        IOptions<InstagramSettings> instagramSettings,
        IOptions<TikTokSettings> tikTokSettings,
        IEnumerable<IProviderService> providers)
    {
        _socialAccountRepository = socialAccountRepository;
        _socialIntegrationRepository = socialIntegrationRepository;
        _brandRepository = brandRepository;
        _oauthStateStore = oauthStateStore;
        _tokenProtector = tokenProtector;
        _facebookSettings = facebookSettings.Value;
        _instagramSettings = instagramSettings.Value;
        _tikTokSettings = tikTokSettings.Value;
        _providers = providers.ToDictionary(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<AuthUrlResponse> GetAuthUrlAsync(string provider, Guid profileId, CancellationToken cancellationToken = default)
    {
        var providerService = GetProvider(provider);
        var state = await _oauthStateStore.CreateAsync(profileId, provider, cancellationToken);
        var authUrl = await providerService.GetAuthUrlAsync(state, GetRedirectUri(provider), cancellationToken);
        return new AuthUrlResponse
        {
            AuthUrl = authUrl,
            State = state
        };
    }

    public async Task<SocialAccountDto> LinkAccountAsync(string provider, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default)
        => await LinkAccountInternalAsync(provider, profileId, null, request, cancellationToken);

    private async Task<SocialAccountDto> LinkAccountInternalAsync(string provider, Guid profileId, Guid? workspaceId, SocialCallbackRequest request, CancellationToken cancellationToken)
    {
        var providerService = GetProvider(provider);
        var platform = GetPlatform(provider);
        var statePayload = await _oauthStateStore.ConsumeAsync(request.State, profileId, provider, cancellationToken);
        if (statePayload == null)
        {
            throw new InvalidOperationException("OAuth state is invalid or expired.");
        }

        var providerAccount = await providerService.ExchangeCodeAsync(request.Code, GetRedirectUri(provider), cancellationToken);
        var existing = await _socialAccountRepository.GetByProfileIdPlatformAndAccountIdAsync(
            profileId,
            platform,
            providerAccount.ProviderUserId,
            cancellationToken);

        if (existing != null)
        {
            if (workspaceId.HasValue && existing.WorkspaceId != Guid.Empty && existing.WorkspaceId != workspaceId)
            {
                throw new InvalidOperationException("Social account is already linked to another workspace.");
            }

            existing.WorkspaceId = workspaceId ?? existing.WorkspaceId;
            existing.UserAccessToken = _tokenProtector.Protect(providerAccount.AccessToken);
            existing.RefreshToken = string.IsNullOrWhiteSpace(providerAccount.RefreshToken)
                ? null
                : _tokenProtector.Protect(providerAccount.RefreshToken);
            existing.ExpiresAt = providerAccount.ExpiresAt;
            existing.IsDeleted = false;
            existing.IsActive = true;
            await _socialAccountRepository.UpdateAsync(existing, cancellationToken);
            return MapAccount(existing);
        }

        var account = new SocialAccount
        {
            ProfileId = profileId,
            WorkspaceId = workspaceId ?? throw new InvalidOperationException("Workspace context is required."),
            Platform = platform,
            AccountId = providerAccount.ProviderUserId,
            UserAccessToken = _tokenProtector.Protect(providerAccount.AccessToken),
            RefreshToken = string.IsNullOrWhiteSpace(providerAccount.RefreshToken)
                ? null
                : _tokenProtector.Protect(providerAccount.RefreshToken),
            ExpiresAt = providerAccount.ExpiresAt,
            IsActive = true,
            IsDeleted = false
        };

        await _socialAccountRepository.AddAsync(account, cancellationToken);
        return MapAccount(account);
    }

    public async Task<IReadOnlyList<SocialAccountDto>> GetProfileAccountsAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var accounts = await _socialAccountRepository.GetByProfileIdAsync(profileId, cancellationToken);
        return accounts.Select(account => MapAccount(account)).ToList();
    }

    public async Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsForAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await RequireOwnedAccountAsync(profileId, socialAccountId, cancellationToken);
        var provider = GetProvider(account.Platform.ToString().ToLowerInvariant());
        var userAccessToken = _tokenProtector.Unprotect(account.UserAccessToken);
        var targets = (await provider.GetTargetsAsync(userAccessToken, cancellationToken)).ToList();
        return await EnrichAvailableTargetsAsync(account.WorkspaceId, account.Platform, targets, cancellationToken);
    }

    public async Task<SocialAccountDto> LinkSelectedTargetsForAccountAsync(Guid profileId, Guid socialAccountId, LinkSelectedTargetsRequest request, CancellationToken cancellationToken = default)
    {
        var account = await RequireOwnedAccountAsync(profileId, socialAccountId, cancellationToken);
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
        if (brand == null || brand.ProfileId != profileId)
        {
            throw new ArgumentException("Brand not found.");
        }

        return await LinkSelectedTargetsInternalAsync(profileId, account, brand, request, cancellationToken);
    }

    private async Task<SocialAccountDto> LinkSelectedTargetsInternalAsync(
        Guid profileId,
        SocialAccount account,
        Brand brand,
        LinkSelectedTargetsRequest request,
        CancellationToken cancellationToken)
    {
        var accountProvider = account.Platform.ToString().ToLowerInvariant();
        if (!string.Equals(request.Provider, accountProvider, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Provider does not match the selected social account.");
        }

        var provider = GetProvider(accountProvider);
        var userAccessToken = _tokenProtector.Unprotect(account.UserAccessToken);
        var availableTargets = (await provider.GetTargetsAsync(userAccessToken, cancellationToken)).ToList();
        var availableById = availableTargets.ToDictionary(target => target.ProviderTargetId, StringComparer.Ordinal);
        var selectedIds = request.ProviderTargetIds.Distinct(StringComparer.Ordinal).ToList();

        foreach (var providerTargetId in selectedIds)
        {
            if (!availableById.ContainsKey(providerTargetId))
            {
                throw new ArgumentException("Selected target is not available for this account.");
            }
        }

        var targetTokens = await provider.GetTargetAccessTokensAsync(userAccessToken, selectedIds, cancellationToken);
        foreach (var providerTargetId in selectedIds)
        {
            var target = availableById[providerTargetId];
            var linkedInWorkspace = await _socialIntegrationRepository.GetByWorkspacePlatformExternalIdAsync(
                account.WorkspaceId,
                account.Platform,
                providerTargetId,
                cancellationToken);

            if (linkedInWorkspace != null && linkedInWorkspace.BrandId != brand.Id)
            {
                var linkedBrandName = linkedInWorkspace.Brand?.Name ?? "another brand";
                throw new InvalidOperationException($"Target \"{target.Name}\" is already linked to brand \"{linkedBrandName}\".");
            }

            if (!targetTokens.TryGetValue(providerTargetId, out var targetAccessToken))
            {
                throw new InvalidOperationException($"Missing access token for target {providerTargetId}.");
            }

            var existing = linkedInWorkspace ?? await _socialIntegrationRepository.GetByExternalIdAsync(account.Id, providerTargetId, cancellationToken);
            if (existing != null)
            {
                existing.BrandId = brand.Id;
                existing.WorkspaceId = account.WorkspaceId;
                existing.SocialAccountId = account.Id;
                existing.AccessToken = _tokenProtector.Protect(targetAccessToken);
                existing.TargetName = target.Name;
                existing.TargetType = target.Type;
                existing.TargetCategory = target.Category;
                existing.ProfilePictureUrl = target.ProfilePictureUrl;
                existing.IsDeleted = false;
                existing.IsActive = true;
                await _socialIntegrationRepository.UpdateAsync(existing, cancellationToken);

                if (account.SocialIntegrations.All(integration => integration.Id != existing.Id))
                {
                    account.SocialIntegrations.Add(existing);
                }

                continue;
            }

            var integration = new SocialIntegration
            {
                ProfileId = profileId,
                WorkspaceId = account.WorkspaceId,
                BrandId = brand.Id,
                SocialAccountId = account.Id,
                Platform = account.Platform,
                ExternalId = providerTargetId,
                TargetName = target.Name,
                TargetType = target.Type,
                TargetCategory = target.Category,
                ProfilePictureUrl = target.ProfilePictureUrl,
                AccessToken = _tokenProtector.Protect(targetAccessToken),
                IsActive = true,
                IsDeleted = false
            };

            await _socialIntegrationRepository.AddAsync(integration, cancellationToken);
            account.SocialIntegrations.Add(integration);
        }

        var reloaded = await _socialAccountRepository.GetByIdWithIntegrationsAsync(account.Id, cancellationToken)
            ?? throw new ArgumentException("Social account not found.");
        return MapAccount(reloaded);
    }

    public async Task<IReadOnlyList<SocialTargetDto>> GetLinkedTargetsAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await RequireOwnedAccountAsync(profileId, socialAccountId, cancellationToken);
        return account.SocialIntegrations
            .Where(integration => !integration.IsDeleted)
            .Select(MapTarget)
            .ToList();
    }

    public async Task<bool> UnlinkAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await _socialAccountRepository.GetByIdWithIntegrationsAsync(socialAccountId, cancellationToken);
        if (account == null || account.ProfileId != profileId)
        {
            return false;
        }

        account.IsDeleted = true;
        account.IsActive = false;
        foreach (var integration in account.SocialIntegrations.Where(integration => !integration.IsDeleted))
        {
            integration.IsDeleted = true;
            integration.IsActive = false;
            await _socialIntegrationRepository.UpdateAsync(integration, cancellationToken);
        }

        await _socialAccountRepository.UpdateAsync(account, cancellationToken);
        return true;
    }

    public async Task<bool> UnlinkTargetAsync(Guid profileId, Guid socialIntegrationId, CancellationToken cancellationToken = default)
    {
        var integration = await _socialIntegrationRepository.GetByIdAsync(socialIntegrationId, cancellationToken);
        if (integration == null || integration.ProfileId != profileId)
        {
            return false;
        }

        integration.IsDeleted = true;
        integration.IsActive = false;
        await _socialIntegrationRepository.UpdateAsync(integration, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<SocialIntegrationDto>> GetIntegrationsByBrandAsync(Guid profileId, Guid brandId, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
        if (brand == null || brand.ProfileId != profileId)
        {
            throw new ArgumentException("Brand not found.");
        }

        var integrations = await _socialIntegrationRepository.GetByBrandIdAsync(brandId, cancellationToken);
        return integrations.Select(MapIntegration).ToList();
    }

    public async Task<SocialAccountDto?> GetSocialAccountByIdAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await _socialAccountRepository.GetByIdWithIntegrationsAsync(socialAccountId, cancellationToken);
        return account == null || account.ProfileId != profileId ? null : MapAccount(account, includeCredentials: true);
    }

    public async Task<SocialAccountDto> LinkAccountInWorkspaceAsync(string provider, Guid workspaceId, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default)
        => await LinkAccountInternalAsync(provider, profileId, workspaceId, request, cancellationToken);

    public async Task<IReadOnlyList<SocialAccountDto>> GetWorkspaceAccountsAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => (await _socialAccountRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken))
            .Select(account => MapAccount(account))
            .ToList();

    public async Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await RequireWorkspaceAccountAsync(workspaceId, socialAccountId, cancellationToken);
        return await ListAvailableTargetsForAccountAsync(account.ProfileId, socialAccountId, cancellationToken);
    }

    public async Task<SocialAccountDto> LinkSelectedTargetsInWorkspaceAsync(Guid workspaceId, Guid profileId, Guid socialAccountId, LinkSelectedTargetsRequest request, CancellationToken cancellationToken = default)
    {
        var account = await RequireWorkspaceAccountAsync(workspaceId, socialAccountId, cancellationToken);
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
        if (brand == null || brand.WorkspaceId != workspaceId) throw new ArgumentException("Brand not found.");
        return await LinkSelectedTargetsInternalAsync(profileId, account, brand, request, cancellationToken);
    }

    public async Task<IReadOnlyList<SocialTargetDto>> GetLinkedTargetsInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await RequireWorkspaceAccountAsync(workspaceId, socialAccountId, cancellationToken);
        return await GetLinkedTargetsAsync(account.ProfileId, socialAccountId, cancellationToken);
    }

    public async Task<bool> UnlinkAccountInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await _socialAccountRepository.GetByIdWithIntegrationsAsync(socialAccountId, cancellationToken);
        return account != null && account.WorkspaceId == workspaceId && await UnlinkAccountAsync(account.ProfileId, socialAccountId, cancellationToken);
    }

    public async Task<bool> UnlinkTargetInWorkspaceAsync(Guid workspaceId, Guid socialIntegrationId, CancellationToken cancellationToken = default)
    {
        var integration = await _socialIntegrationRepository.GetByIdAsync(socialIntegrationId, cancellationToken);
        return integration != null && integration.WorkspaceId == workspaceId && await UnlinkTargetAsync(integration.ProfileId, socialIntegrationId, cancellationToken);
    }

    public async Task<IReadOnlyList<SocialIntegrationDto>> GetIntegrationsByBrandInWorkspaceAsync(Guid workspaceId, Guid brandId, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
        if (brand == null || brand.WorkspaceId != workspaceId) throw new ArgumentException("Brand not found.");
        return (await _socialIntegrationRepository.GetByBrandIdAsync(brandId, cancellationToken)).Where(i => i.WorkspaceId == workspaceId).Select(MapIntegration).ToList();
    }

    private async Task<SocialAccount> RequireWorkspaceAccountAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken)
    {
        var account = await _socialAccountRepository.GetByIdWithIntegrationsAsync(socialAccountId, cancellationToken);
        if (account == null || account.WorkspaceId != workspaceId || account.IsDeleted) throw new ArgumentException("Social account not found.");
        return account;
    }

    private async Task<IReadOnlyList<AvailableTargetDto>> EnrichAvailableTargetsAsync(
        Guid workspaceId,
        SocialPlatformEnum platform,
        IReadOnlyList<AvailableTargetDto> targets,
        CancellationToken cancellationToken)
    {
        var integrations = await _socialIntegrationRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        var linkedByExternalId = integrations
            .Where(integration => integration.Platform == platform && !string.IsNullOrWhiteSpace(integration.ExternalId))
            .GroupBy(integration => integration.ExternalId!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var target in targets)
        {
            if (linkedByExternalId.TryGetValue(target.ProviderTargetId, out var linked))
            {
                target.LinkedBrandId = linked.BrandId;
                target.LinkedBrandName = linked.Brand?.Name;
                target.LinkedIntegrationId = linked.Id;
            }
        }

        return targets;
    }

    private async Task<SocialAccount> RequireOwnedAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken)
    {
        var account = await _socialAccountRepository.GetByIdWithIntegrationsAsync(socialAccountId, cancellationToken);
        if (account == null || account.ProfileId != profileId || account.IsDeleted)
        {
            throw new ArgumentException("Social account not found.");
        }

        return account;
    }

    public async Task<IReadOnlyList<FacebookAdAccountData>> GetAdAccountsForSocialAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await RequireOwnedAccountAsync(profileId, socialAccountId, cancellationToken);
        var provider = GetProvider(account.Platform.ToString().ToLowerInvariant());
        var userAccessToken = _tokenProtector.Unprotect(account.UserAccessToken);
        return (await provider.GetAdAccountsAsync(userAccessToken, cancellationToken)).ToList();
    }

    public async Task<string?> GetFacebookUserAccessTokenAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var accounts = await _socialAccountRepository.GetByProfileIdAsync(profileId, cancellationToken);
        var fbAccount = accounts.FirstOrDefault(a => a.Platform == SocialPlatformEnum.Facebook && a.IsActive);
        if (fbAccount == null || string.IsNullOrWhiteSpace(fbAccount.UserAccessToken)) return null;
        return _tokenProtector.Unprotect(fbAccount.UserAccessToken);
    }

    public async Task<IReadOnlyList<FacebookAdAccountData>> GetAdAccountsForSocialAccountInWorkspaceAsync(Guid workspaceId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await RequireWorkspaceAccountAsync(workspaceId, socialAccountId, cancellationToken);
        var provider = GetProvider(account.Platform.ToString().ToLowerInvariant());
        var userAccessToken = _tokenProtector.Unprotect(account.UserAccessToken);
        return (await provider.GetAdAccountsAsync(userAccessToken, cancellationToken)).ToList();
    }

    private IProviderService GetProvider(string provider)
    {
        if (!string.Equals(provider, "facebook", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(provider, "instagram", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(provider, "tiktok", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only Facebook, Instagram and TikTok are supported.");
        }

        if (!_providers.TryGetValue(provider, out var providerService))
        {
            throw new InvalidOperationException($"{provider} provider is not registered.");
        }

        return providerService;
    }

    private string GetRedirectUri(string provider) => GetPlatform(provider) switch
    {
        SocialPlatformEnum.Facebook => _facebookSettings.RedirectUri,
        SocialPlatformEnum.Instagram => _instagramSettings.RedirectUri,
        SocialPlatformEnum.TikTok => _tikTokSettings.RedirectUri,
        _ => throw new ArgumentException("Unsupported social provider.")
    };

    private static SocialPlatformEnum GetPlatform(string provider)
    {
        if (string.Equals(provider, "facebook", StringComparison.OrdinalIgnoreCase)) return SocialPlatformEnum.Facebook;
        if (string.Equals(provider, "instagram", StringComparison.OrdinalIgnoreCase)) return SocialPlatformEnum.Instagram;
        if (string.Equals(provider, "tiktok", StringComparison.OrdinalIgnoreCase)) return SocialPlatformEnum.TikTok;
        throw new ArgumentException("Unsupported social provider.");
    }

    private SocialAccountDto MapAccount(SocialAccount account, bool includeCredentials = false)
    {
        return new SocialAccountDto
        {
            Id = account.Id,
            ProfileId = account.ProfileId,
            Provider = account.Platform.ToString().ToLowerInvariant(),
            ProviderUserId = account.AccountId ?? string.Empty,
            AccessToken = includeCredentials && !string.IsNullOrWhiteSpace(account.UserAccessToken)
                ? _tokenProtector.Unprotect(account.UserAccessToken)
                : string.Empty,
            RefreshToken = includeCredentials && !string.IsNullOrWhiteSpace(account.RefreshToken)
                ? _tokenProtector.Unprotect(account.RefreshToken)
                : null,
            IsActive = account.IsActive,
            ExpiresAt = account.ExpiresAt,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            Targets = account.SocialIntegrations
                .Where(integration => !integration.IsDeleted)
                .Select(MapTarget)
                .ToList()
        };
    }

    private static SocialTargetDto MapTarget(SocialIntegration integration)
    {
        return new SocialTargetDto
        {
            Id = integration.Id,
            BrandId = integration.BrandId,
            BrandName = integration.Brand?.Name,
            ProviderTargetId = integration.ExternalId ?? string.Empty,
            Name = string.IsNullOrWhiteSpace(integration.TargetName) ? integration.ExternalId ?? string.Empty : integration.TargetName,
            Type = string.IsNullOrWhiteSpace(integration.TargetType) ? integration.Platform switch
            {
                SocialPlatformEnum.TikTok => "tiktok_account",
                SocialPlatformEnum.Instagram => "instagram_business_account",
                _ => "page"
            } : integration.TargetType,
            Category = integration.TargetCategory,
            ProfilePictureUrl = integration.ProfilePictureUrl,
            IsActive = integration.IsActive
        };
    }

    private static SocialIntegrationDto MapIntegration(SocialIntegration integration)
    {
        return new SocialIntegrationDto
        {
            Id = integration.Id,
            SocialAccountId = integration.SocialAccountId,
            ProfileId = integration.ProfileId,
            BrandId = integration.BrandId,
            ExternalId = integration.ExternalId ?? string.Empty,
            Name = string.IsNullOrWhiteSpace(integration.TargetName) ? integration.ExternalId ?? string.Empty : integration.TargetName,
            Type = string.IsNullOrWhiteSpace(integration.TargetType) ? integration.Platform switch
            {
                SocialPlatformEnum.TikTok => "tiktok_account",
                SocialPlatformEnum.Instagram => "instagram_business_account",
                _ => "page"
            } : integration.TargetType,
            Category = integration.TargetCategory,
            ProfilePictureUrl = integration.ProfilePictureUrl,
            Platform = integration.Platform.ToString().ToLowerInvariant(),
            IsActive = integration.IsActive,
            CreatedAt = integration.CreatedAt,
            UpdatedAt = integration.UpdatedAt,
            BrandName = integration.Brand?.Name
        };
    }
}
