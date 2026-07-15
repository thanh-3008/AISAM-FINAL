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
        var referenceUrls = options?.ReferenceImageUrls?
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Take(3)
            .ToList() ?? new List<string>();
        var requiresReferenceImage = referenceUrls.Count > 0;

        _logger.LogInformation("Attempting OpenRouter image generation...");
        var orResult = await _openRouter.GenerateAsync(prompt, options, cancellationToken);
        if (orResult.Bytes != null)
        {
            return AIMediaResult.OkBytes(orResult.Bytes, requiresReferenceImage ? "OpenRouter ImageEdit" : "OpenRouter");
        }
        if (!string.IsNullOrEmpty(orResult.Url))
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(orResult.Url, cancellationToken);
                return AIMediaResult.OkBytes(bytes, requiresReferenceImage ? "OpenRouter ImageEdit" : "OpenRouter");
            }
            catch (Exception ex)
            {
                return AIMediaResult.Fail($"Failed to download image from OpenRouter URL: {ex.Message}", requiresReferenceImage ? "OpenRouter ImageEdit" : "OpenRouter");
            }
        }

        if (requiresReferenceImage)
        {
            var error = $"Reference image generation failed, so fallback text-to-image was blocked to avoid creating a wrong product image. OpenRouter/deAPI error: [{orResult.Error}]";
            _logger.LogError(error);
            return AIMediaResult.Fail(error, "OpenRouter ImageEdit");
        }

        _logger.LogWarning("OpenRouter image failed: {Error}. Falling back to Hugging Face...", orResult.Error);

        var huggingFacePrompt = AppendReferenceUrlsToPrompt(prompt, options);
        var huggingFaceResult = await _huggingFace.GenerateAsync(huggingFacePrompt, cancellationToken);
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

    private static string AppendReferenceUrlsToPrompt(string prompt, ImageGenerationOptions? options)
    {
        var references = options?.ReferenceImageUrls?
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Take(3)
            .ToList();

        if (references is not { Count: > 0 })
        {
            return prompt;
        }

        return $"{string.Join(" ", references)} {prompt}";
    }
}
