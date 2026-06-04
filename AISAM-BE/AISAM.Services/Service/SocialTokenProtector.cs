using AISAM.Services.IServices;
using Microsoft.AspNetCore.DataProtection;

namespace AISAM.Services.Service;

public sealed class SocialTokenProtector : ISocialTokenProtector
{
    private readonly IDataProtector _protector;

    public SocialTokenProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("AISAM.SocialTokens");
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}
