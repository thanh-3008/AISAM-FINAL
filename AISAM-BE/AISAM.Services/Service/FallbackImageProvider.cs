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

        _logger.LogInformation("Attempting primary image generation (Image-to-Image when references exist, otherwise Text-to-Image)...");
        var orResult = await _openRouter.GenerateAsync(prompt, options, cancellationToken);
        if (orResult.Bytes != null)
        {
            return AIMediaResult.OkBytes(orResult.Bytes, requiresReferenceImage ? "OpenRouter Image-to-Image (FLUX.2 Klein)" : "OpenRouter Text-to-Image");
        }
        if (!string.IsNullOrEmpty(orResult.Url))
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(orResult.Url, cancellationToken);
                return AIMediaResult.OkBytes(bytes, requiresReferenceImage ? "OpenRouter Image-to-Image (FLUX.2 Klein)" : "OpenRouter Text-to-Image");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download image from OpenRouter URL: {Url}. Proceeding to fallback...", orResult.Url);
                orResult.Error = $"Failed to download image from URL: {ex.Message}";
            }
        }

        string? textToImageError = null;
        if (requiresReferenceImage)
        {
            _logger.LogWarning("Primary image-to-image generation (FLUX.2 Klein) failed: [{Error}]. Falling back to OpenRouter text-to-image as backup...", orResult.Error);
            var fallbackOptions = new ImageGenerationOptions
            {
                Width = options?.Width ?? 1024,
                Height = options?.Height ?? 1024,
                ReferenceImageUrls = Array.Empty<string>()
            };
            var t2iResult = await _openRouter.GenerateAsync(prompt, fallbackOptions, cancellationToken);
            if (t2iResult.Bytes != null)
            {
                return AIMediaResult.OkBytes(t2iResult.Bytes, "OpenRouter Text-to-Image (Fallback)");
            }
            if (!string.IsNullOrEmpty(t2iResult.Url))
            {
                try
                {
                    var bytes = await _httpClient.GetByteArrayAsync(t2iResult.Url, cancellationToken);
                    return AIMediaResult.OkBytes(bytes, "OpenRouter Text-to-Image (Fallback)");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to download fallback image from OpenRouter URL: {Message}", ex.Message);
                    t2iResult.Error = $"Failed to download image: {ex.Message}";
                }
            }
            textToImageError = t2iResult.Error ?? "Unknown Text-to-Image error";
            _logger.LogWarning("OpenRouter text-to-image fallback failed: {Error}. Falling back to Hugging Face text-to-image...", textToImageError);
        }
        else
        {
            _logger.LogWarning("OpenRouter image generation failed: {Error}. Falling back to Hugging Face...", orResult.Error);
        }

        var huggingFacePrompt = AppendReferenceUrlsToPrompt(prompt, options);
        var huggingFaceResult = await _huggingFace.GenerateAsync(huggingFacePrompt, cancellationToken);
        if (huggingFaceResult.Url != null)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(huggingFaceResult.Url, cancellationToken);
                return AIMediaResult.OkBytes(bytes, "Hugging Face Text-to-Image (Fallback)");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to download image from Hugging Face URL: {Error}.", ex.Message);
            }
        }
        else if (huggingFaceResult.Bytes != null)
        {
             return AIMediaResult.OkBytes(huggingFaceResult.Bytes, "Hugging Face Text-to-Image (Fallback)");
        }

        var hfError = huggingFaceResult.Error ?? "Download failed or no result";

        var errorMessage = requiresReferenceImage && textToImageError != null
            ? $"All providers failed. Primary I2I (FLUX.2 Klein): [{orResult.Error}] | Fallback T2I: [{textToImageError}] | Hugging Face T2I: [{hfError}]"
            : $"All providers failed. OpenRouter: [{orResult.Error}] | Hugging Face: [{hfError}]";
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
