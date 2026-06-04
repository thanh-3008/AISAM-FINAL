using AISAM.Common.Models;
using AISAM.Data.Model;

namespace AISAM.Services.IServices;

public interface IProviderService
{
    string ProviderName { get; }
    Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default);
    Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default);
}
