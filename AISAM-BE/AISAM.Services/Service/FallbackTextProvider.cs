using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AISAM.Services.Service;

public sealed class FallbackTextProvider : IGeminiTextClient
{
    private readonly GeminiTextClient _geminiClient;
    private readonly FallbackGeminiTextClient _fallbackGeminiClient;
    private readonly FallbackGeminiTextClient2 _fallbackGeminiClient2;
    private readonly FallbackGeminiTextClient3 _fallbackGeminiClient3;
    private readonly FallbackGeminiTextClient4 _fallbackGeminiClient4;
    private readonly ILogger<FallbackTextProvider> _logger;

    public FallbackTextProvider(
        GeminiTextClient geminiClient,
        FallbackGeminiTextClient fallbackGeminiClient,
        FallbackGeminiTextClient2 fallbackGeminiClient2,
        FallbackGeminiTextClient3 fallbackGeminiClient3,
        FallbackGeminiTextClient4 fallbackGeminiClient4,
        ILogger<FallbackTextProvider> logger)
    {
        _geminiClient = geminiClient;
        _fallbackGeminiClient = fallbackGeminiClient;
        _fallbackGeminiClient2 = fallbackGeminiClient2;
        _fallbackGeminiClient3 = fallbackGeminiClient3;
        _fallbackGeminiClient4 = fallbackGeminiClient4;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var providers = new (string Name, Func<Task<string>> Generate)[]
        {
            ("Gemini", () => _geminiClient.GenerateAsync(prompt, cancellationToken)),
            ("FallbackGemini1", () => _fallbackGeminiClient.GenerateAsync(prompt, cancellationToken)),
            ("FallbackGemini2", () => _fallbackGeminiClient2.GenerateAsync(prompt, cancellationToken)),
            ("FallbackGemini3", () => _fallbackGeminiClient3.GenerateAsync(prompt, cancellationToken)),
            ("FallbackGemini4", () => _fallbackGeminiClient4.GenerateAsync(prompt, cancellationToken))
        };

        Exception? lastException = null;
        for (var index = 0; index < providers.Length; index++)
        {
            var provider = providers[index];
            var timer = Stopwatch.StartNew();
            try
            {
                var result = await provider.Generate();
                _logger.LogInformation(
                    "AskAI.LLM.ProviderAttempt ProviderName={ProviderName} AttemptOrder={AttemptOrder} DurationMs={DurationMs} Success={Success} FailureCategory={FailureCategory} Cancelled={Cancelled}",
                    provider.Name, index + 1, timer.ElapsedMilliseconds, true, null, false);
                return result;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(
                    "AskAI.LLM.ProviderAttempt ProviderName={ProviderName} AttemptOrder={AttemptOrder} DurationMs={DurationMs} Success={Success} FailureCategory={FailureCategory} Cancelled={Cancelled} ExceptionType={ExceptionType}",
                    provider.Name, index + 1, timer.ElapsedMilliseconds, false, "CLIENT_CANCELLED", true, ex.GetType().Name);
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(
                    ex,
                    "AskAI.LLM.ProviderAttempt ProviderName={ProviderName} AttemptOrder={AttemptOrder} DurationMs={DurationMs} Success={Success} FailureCategory={FailureCategory} Cancelled={Cancelled}",
                    provider.Name, index + 1, timer.ElapsedMilliseconds, false, ClassifyProviderFailure(ex), false);
            }
        }

        _logger.LogError(lastException, "All AI text providers failed.");
        throw new InvalidOperationException("All AI text providers failed.", lastException);
    }

    private static string ClassifyProviderFailure(Exception exception)
        => exception is TaskCanceledException ? "LLM_TIMEOUT" : exception is HttpRequestException ? "LLM_PROVIDER_FAILURE" : exception.GetType().Name;

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
                    _logger.LogWarning(fallback2Ex, "Fallback 2 Vision generation failed. Trying tertiary Fallback Gemini API...");

                    try
                    {
                        return await _fallbackGeminiClient3.GenerateWithVisionAsync(textPrompt, imageBytes, mimeType, cancellationToken);
                    }
                    catch (Exception fallback3Ex)
                    {
                        _logger.LogWarning(fallback3Ex, "Fallback 3 Vision generation failed. Trying quaternary Fallback Gemini API...");

                        try
                        {
                            return await _fallbackGeminiClient4.GenerateWithVisionAsync(textPrompt, imageBytes, mimeType, cancellationToken);
                        }
                        catch (Exception fallback4Ex)
                        {
                            _logger.LogError(fallback4Ex, "All Vision providers failed. Falling back to text-only generation.");
                            // Final fallback: call text-only (ignoring image bytes, generating script from text context)
                            return await GenerateAsync(textPrompt, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
