namespace AISAM.Services.IServices;

public interface ISocialTokenProtector
{
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}
