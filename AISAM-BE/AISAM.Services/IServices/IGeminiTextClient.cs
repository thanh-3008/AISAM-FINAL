namespace AISAM.Services.IServices;

public interface IGeminiTextClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
