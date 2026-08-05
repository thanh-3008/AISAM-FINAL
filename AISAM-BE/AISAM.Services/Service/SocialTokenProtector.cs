using AISAM.Services.IServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class SocialTokenProtector : ISocialTokenProtector
{
    private readonly IDataProtector _protector;
    private readonly ILogger<SocialTokenProtector> _logger;

    public SocialTokenProtector(IDataProtectionProvider provider, ILogger<SocialTokenProtector> logger)
    {
        _protector = provider.CreateProtector("AISAM.SocialTokens");
        _logger = logger;
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);

    public string? TryUnprotect(string ciphertext)
    {
        if (string.IsNullOrWhiteSpace(ciphertext))
            return null;

        try
        {
            return _protector.Unprotect(ciphertext);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stored social token could not be decrypted. The data protection key may have changed.");
            return null;
        }
    }
}
