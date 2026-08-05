using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class FallbackTextProvider : IGeminiTextClient
{
    private readonly GeminiTextClient _geminiClient;
    private readonly FallbackGeminiTextClient _fallbackGeminiClient;
    private readonly FallbackGeminiTextClient2 _fallbackGeminiClient2;
    private readonly ILogger<FallbackTextProvider> _logger;

    public FallbackTextProvider(
        GeminiTextClient geminiClient,
        FallbackGeminiTextClient fallbackGeminiClient,
        FallbackGeminiTextClient2 fallbackGeminiClient2,
        ILogger<FallbackTextProvider> logger)
    {
        _geminiClient = geminiClient;
        _fallbackGeminiClient = fallbackGeminiClient;
        _fallbackGeminiClient2 = fallbackGeminiClient2;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to generate text with Gemini...");
            return await _geminiClient.GenerateAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini text generation failed. Trying first fallback Gemini API...");

            try
            {
                return await _fallbackGeminiClient.GenerateAsync(prompt, cancellationToken);
            }
            catch (Exception fallback1Ex)
            {
                _logger.LogWarning(fallback1Ex, "Fallback Gemini text generation failed. Trying secondary Fallback Gemini API...");

                try
                {
                    return await _fallbackGeminiClient2.GenerateAsync(prompt, cancellationToken);
                }
                catch (Exception fallback2Ex)
                {
                    _logger.LogError(fallback2Ex, "All AI text providers failed.");
                    throw new Exception($"All AI text providers failed. Primary Gemini: {ex.Message} | Fallback 1: {fallback1Ex.Message} | Fallback 2: {fallback2Ex.Message}", fallback2Ex);
                }
            }
        }
    }

    public async Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to generate text with Vision using primary Gemini...");
            return await _geminiClient.GenerateWithVisionAsync(textPrompt, imageBytes, mimeType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini Vision generation failed. Trying first fallback Gemini API...");

            try
            {
                return await _fallbackGeminiClient.GenerateWithVisionAsync(textPrompt, imageBytes, mimeType, cancellationToken);
            }
            catch (Exception fallback1Ex)
            {
                _logger.LogWarning(fallback1Ex, "Fallback 1 Vision generation failed. Trying secondary Fallback Gemini API...");

                try
                {
                    return await _fallbackGeminiClient2.GenerateWithVisionAsync(textPrompt, imageBytes, mimeType, cancellationToken);
                }
                catch (Exception fallback2Ex)
                {
                    _logger.LogError(fallback2Ex, "All Vision providers failed. Falling back to text-only generation.");
                    // Final fallback: call text-only (ignoring image bytes, generating script from text context)
                    return await GenerateAsync(textPrompt, cancellationToken);
                }
            }
        }
    }
}
