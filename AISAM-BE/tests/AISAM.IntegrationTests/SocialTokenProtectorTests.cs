using AISAM.Services.Service;
using Microsoft.AspNetCore.DataProtection;

namespace AISAM.IntegrationTests;

public class SocialTokenProtectorTests
{
    [Fact]
    public void Protect_RoundTripsOriginalToken()
    {
        var protector = CreateProtector();

        var ciphertext = protector.Protect("social-secret");
        var plaintext = protector.Unprotect(ciphertext);

        Assert.Equal("social-secret", plaintext);
    }

    [Fact]
    public void Protect_ReturnsCiphertextDifferentFromPlaintext()
    {
        var protector = CreateProtector();

        var ciphertext = protector.Protect("social-secret");

        Assert.NotEqual("social-secret", ciphertext);
    }

    private static SocialTokenProtector CreateProtector()
    {
        var keyDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        keyDirectory.Create();
        var provider = DataProtectionProvider.Create(keyDirectory);
        return new SocialTokenProtector(provider);
    }
}
