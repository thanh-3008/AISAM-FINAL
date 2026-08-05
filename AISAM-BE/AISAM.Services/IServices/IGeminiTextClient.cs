namespace AISAM.Services.IServices;

public interface IGeminiTextClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

    Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default);
}
