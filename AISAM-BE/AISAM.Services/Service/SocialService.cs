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
    private readonly TikTokSettings _tikTokSettings;
    private readonly Dictionary<string, IProviderService> _providers;

    public SocialService(
        ISocialAccountRepository socialAccountRepository,
        ISocialIntegrationRepository socialIntegrationRepository,
        IBrandRepository brandRepository,
        IOAuthStateStore oauthStateStore,
        ISocialTokenProtector tokenProtector,
        IOptions<FacebookSettings> facebookSettings,
        IOptions<TikTokSettings> tikTokSettings,
        IEnumerable<IProviderService> providers)
    {
        _socialAccountRepository = socialAccountRepository;
        _socialIntegrationRepository = socialIntegrationRepository;
        _brandRepository = brandRepository;
        _oauthStateStore = oauthStateStore;
        _tokenProtector = tokenProtector;
        _facebookSettings = facebookSettings.Value;
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
        return accounts.Select(MapAccount).ToList();
    }

    public async Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsForAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        var account = await RequireOwnedAccountAsync(profileId, socialAccountId, cancellationToken);
        var provider = GetProvider(account.Platform.ToString().ToLowerInvariant());
        var userAccessToken = _tokenProtector.Unprotect(account.UserAccessToken);
        return (await provider.GetTargetsAsync(userAccessToken, cancellationToken)).ToList();
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
            if (!targetTokens.TryGetValue(providerTargetId, out var targetAccessToken))
            {
                throw new InvalidOperationException($"Missing access token for target {providerTargetId}.");
            }

            var existing = await _socialIntegrationRepository.GetByExternalIdAsync(account.Id, providerTargetId, cancellationToken);
            if (existing != null)
            {
                existing.BrandId = brand.Id;
                existing.WorkspaceId = account.WorkspaceId;
                existing.AccessToken = _tokenProtector.Protect(targetAccessToken);
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
        return account == null || account.ProfileId != profileId ? null : MapAccount(account);
    }

    public async Task<SocialAccountDto> LinkAccountInWorkspaceAsync(string provider, Guid workspaceId, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default)
        => await LinkAccountInternalAsync(provider, profileId, workspaceId, request, cancellationToken);

    public async Task<IReadOnlyList<SocialAccountDto>> GetWorkspaceAccountsAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => (await _socialAccountRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken)).Select(MapAccount).ToList();

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
            !string.Equals(provider, "tiktok", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only Facebook and TikTok are supported.");
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
        SocialPlatformEnum.TikTok => _tikTokSettings.RedirectUri,
        _ => throw new ArgumentException("Unsupported social provider.")
    };

    private static SocialPlatformEnum GetPlatform(string provider)
    {
        if (string.Equals(provider, "facebook", StringComparison.OrdinalIgnoreCase)) return SocialPlatformEnum.Facebook;
        if (string.Equals(provider, "tiktok", StringComparison.OrdinalIgnoreCase)) return SocialPlatformEnum.TikTok;
        throw new ArgumentException("Unsupported social provider.");
    }

    private SocialAccountDto MapAccount(SocialAccount account)
    {
        return new SocialAccountDto
        {
            Id = account.Id,
            ProfileId = account.ProfileId,
            Provider = account.Platform.ToString().ToLowerInvariant(),
            ProviderUserId = account.AccountId ?? string.Empty,
            AccessToken = account.UserAccessToken != null ? _tokenProtector.Unprotect(account.UserAccessToken) : string.Empty,
            RefreshToken = string.IsNullOrWhiteSpace(account.RefreshToken) ? null : _tokenProtector.Unprotect(account.RefreshToken),
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
            ProviderTargetId = integration.ExternalId ?? string.Empty,
            Name = integration.ExternalId ?? string.Empty,
            Type = integration.Platform == SocialPlatformEnum.TikTok ? "tiktok_account" : "page",
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
            Name = integration.ExternalId ?? string.Empty,
            Platform = integration.Platform.ToString().ToLowerInvariant(),
            IsActive = integration.IsActive,
            CreatedAt = integration.CreatedAt,
            UpdatedAt = integration.UpdatedAt,
            BrandName = integration.Brand?.Name
        };
    }
}
