namespace AISAM.Services.IServices;

public interface IOAuthStateStore
{
    Task<string> CreateAsync(Guid profileId, string provider, string? origin = null, string? redirectUri = null, CancellationToken cancellationToken = default);
    Task<OAuthStatePayload?> ConsumeAsync(string state, Guid profileId, string provider, CancellationToken cancellationToken = default);
    string? TryPeekOrigin(string state);
}

public sealed class OAuthStatePayload
{
    public string State { get; init; } = string.Empty;
    public Guid ProfileId { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string? Origin { get; init; }
    public string? RedirectUri { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
}
