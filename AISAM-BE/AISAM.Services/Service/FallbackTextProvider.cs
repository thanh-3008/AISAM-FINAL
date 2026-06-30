using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class FallbackTextProvider : IGeminiTextClient
{
    private readonly GeminiTextClient _geminiClient;
    private readonly FallbackGeminiTextClient _fallbackGeminiClient;
    private readonly OpenRouterTextClient _openRouterClient;
    private readonly ILogger<FallbackTextProvider> _logger;

    public FallbackTextProvider(
        GeminiTextClient geminiClient,
        FallbackGeminiTextClient fallbackGeminiClient,
        OpenRouterTextClient openRouterClient,
        ILogger<FallbackTextProvider> logger)
    {
        _geminiClient = geminiClient;
        _fallbackGeminiClient = fallbackGeminiClient;
        _openRouterClient = openRouterClient;
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
            _logger.LogWarning(ex, "Gemini text generation failed. Trying OpenRouter...");

            try
            {
                return await _openRouterClient.GenerateAsync(prompt, cancellationToken);
            }
            catch (Exception openRouterEx)
            {
                _logger.LogWarning(openRouterEx, "OpenRouter text generation failed. Falling back to secondary Gemini API...");

                try
                {
                    return await _fallbackGeminiClient.GenerateAsync(prompt, cancellationToken);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "All AI text providers failed.");
                    throw new Exception($"All AI text providers failed. Primary Gemini: {ex.Message} | OpenRouter: {openRouterEx.Message} | Secondary Gemini: {fallbackEx.Message}", fallbackEx);
                }
            }
        }
    }
}
