using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class GoogleProvider : IProviderService
{
    private readonly ILogger<GoogleProvider> _logger;

    public GoogleProvider(ILogger<GoogleProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "google";

    public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Google OAuth is not available in Phase C.");
        throw new NotSupportedException("Google OAuth is not available in Phase C.");
    }

    public Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google OAuth is not available in Phase C.");
    }

    public Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<AvailableTargetDto>>(Array.Empty<AvailableTargetDto>());
    }

    public Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Dictionary<string, string>());
    }

    public Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google publishing is not available in Phase C.");
    }
}
