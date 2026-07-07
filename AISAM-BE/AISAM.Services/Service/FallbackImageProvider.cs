using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class FallbackImageProvider : IAIImageProvider
{
    private readonly OpenRouterImageClient _openRouter;
    private readonly HuggingFaceImageClient _huggingFace;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FallbackImageProvider> _logger;

    public string ProviderName => "OpenRouter→HuggingFace";

    public FallbackImageProvider(
        OpenRouterImageClient openRouter,
        HuggingFaceImageClient huggingFace,
        HttpClient httpClient,
        ILogger<FallbackImageProvider> logger)
    {
        _openRouter = openRouter;
        _huggingFace = huggingFace;
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

        _logger.LogWarning("OpenRouter image failed: {Error}. Falling back to Hugging Face...", orResult.Error);

        var huggingFaceResult = await _huggingFace.GenerateAsync(prompt, cancellationToken);
        if (huggingFaceResult.Url != null)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(huggingFaceResult.Url, cancellationToken);
                return AIMediaResult.OkBytes(bytes, "Hugging Face");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to download image from Hugging Face URL: {Error}.", ex.Message);
            }
        }
        else if (huggingFaceResult.Bytes != null)
        {
             return AIMediaResult.OkBytes(huggingFaceResult.Bytes, "Hugging Face");
        }

        var hfError = huggingFaceResult.Error ?? "Download failed or no result";

        var errorMessage = $"All providers failed. OpenRouter: [{orResult.Error}] | Hugging Face: [{hfError}]";
        _logger.LogError(errorMessage);
        return AIMediaResult.Fail(errorMessage, ProviderName);
    }
}
