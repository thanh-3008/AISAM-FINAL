using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class FallbackVideoProvider : IAIVideoProvider
{
    private readonly GeminiVideoClient _gemini;
    private readonly OpenRouterVideoClient _openRouter;
    private readonly ILogger<FallbackVideoProvider> _logger;

    public string ProviderName => "OpenRouter→Gemini";

    public FallbackVideoProvider(
        GeminiVideoClient gemini,
        OpenRouterVideoClient openRouter,
        ILogger<FallbackVideoProvider> logger)
    {
        _gemini = gemini;
        _openRouter = openRouter;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting OpenRouter video start...");
        var orResult = await _openRouter.StartAsync(prompt, cancellationToken);
        if (orResult.Success)
        {
            return orResult;
        }

        _logger.LogWarning("OpenRouter video failed: {Error}. Falling back to Gemini...", orResult.ErrorMessage);

        var geminiResult = await _gemini.StartAsync(prompt, cancellationToken);
        if (geminiResult.Success)
        {
            return geminiResult;
        }

        var errorMessage = $"Both providers failed. OpenRouter: [{orResult.ErrorMessage}] | Gemini: [{geminiResult.ErrorMessage}]";
        _logger.LogError(errorMessage);
        return VideoGenerationResult.Fail(errorMessage, ProviderName);
    }

    public async Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return VideoGenerationResult.Fail("JobId is null or empty.", ProviderName);
        }

        if (jobId.StartsWith("gemini:"))
        {
            var opName = jobId["gemini:".Length..];
            return await _gemini.PollAsync(opName, cancellationToken);
        }

        if (jobId.StartsWith("openrouter:"))
        {
            var id = jobId["openrouter:".Length..];
            return await _openRouter.PollAsync(id, cancellationToken);
        }

        return VideoGenerationResult.Fail($"Unknown JobId format: {jobId}", ProviderName);
    }
}
