using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class FallbackImageProvider : IAIImageProvider
{
    private readonly GeminiImageClient _gemini;
    private readonly OpenRouterImageClient _openRouter;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FallbackImageProvider> _logger;

    public string ProviderName => "OpenRouter→Gemini";

    public FallbackImageProvider(
        GeminiImageClient gemini,
        OpenRouterImageClient openRouter,
        HttpClient httpClient,
        ILogger<FallbackImageProvider> logger)
    {
        _gemini = gemini;
        _openRouter = openRouter;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AIMediaResult> GenerateImageAsync(string prompt, ImageGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting OpenRouter image generation...");
        var orResult = await _openRouter.GenerateAsync(prompt, cancellationToken);
        if (orResult.Bytes != null)
        {
            return AIMediaResult.OkBytes(orResult.Bytes, "OpenRouter");
        }
        if (!string.IsNullOrEmpty(orResult.Url))
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(orResult.Url, cancellationToken);
                return AIMediaResult.OkBytes(bytes, "OpenRouter");
            }
            catch (Exception ex)
            {
                return AIMediaResult.Fail($"Failed to download image from OpenRouter URL: {ex.Message}", "OpenRouter");
            }
        }

        _logger.LogWarning("OpenRouter image failed: {Error}. Falling back to Gemini...", orResult.Error);

        var geminiResult = await _gemini.GenerateAsync(prompt, cancellationToken);
        if (geminiResult.Bytes != null)
        {
            return AIMediaResult.OkBytes(geminiResult.Bytes, "Gemini");
        }

        var errorMessage = $"Both providers failed. OpenRouter: [{orResult.Error}] | Gemini: [{geminiResult.Error}]";
        _logger.LogError(errorMessage);
        return AIMediaResult.Fail(errorMessage, ProviderName);
    }
}
