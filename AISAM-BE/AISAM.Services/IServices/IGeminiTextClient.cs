namespace AISAM.Services.IServices;

public sealed record GeminiGenerationOptions(
    string? ResponseMimeType = null,
    int? MaxOutputTokens = null,
    string? ThinkingLevel = null);

public interface IGeminiTextClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> GenerateAsync(string prompt, string? responseMimeType, CancellationToken cancellationToken = default) => GenerateAsync(prompt, cancellationToken);
    Task<string> GenerateWithOptionsAsync(string prompt, GeminiGenerationOptions options, CancellationToken cancellationToken = default)
        => GenerateAsync(prompt, options.ResponseMimeType, cancellationToken);

    Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default);
    Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType, string? responseMimeType, CancellationToken cancellationToken = default) => GenerateWithVisionAsync(textPrompt, imageBytes, mimeType, cancellationToken);
}
