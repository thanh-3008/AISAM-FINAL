using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class FallbackVideoProvider : IAIVideoProvider
{
    private readonly DeApiVideoClient _deapi;
    private readonly OpenRouterVideoClient _openRouter;
    private readonly ColabVideoStrategy _colab;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<FallbackVideoProvider> _logger;

    public string ProviderName => "OpenRouter→DeAPI→Colab";

    public FallbackVideoProvider(
        DeApiVideoClient deapi,
        OpenRouterVideoClient openRouter,
        ColabVideoStrategy colab,
        IOptions<VideoProviderSettings> options,
        ILogger<FallbackVideoProvider> logger)
    {
        _deapi = deapi;
        _openRouter = openRouter;
        _colab = colab;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("========= VIDEO GENERATION START =========");
        _logger.LogInformation("Prompt: {Prompt}", prompt.Length > 100 ? prompt[..100] + "..." : prompt);
        _logger.LogInformation("Options: AspectRatio={AR}, Duration={Dur}", options?.AspectRatio ?? "9:16", options?.DurationSeconds > 0 ? $"{options.DurationSeconds}s" : "N/A (Default/Segmented)");
        _logger.LogInformation("Provider chain: OpenRouter → DeAPI → Colab");
        _logger.LogInformation("OpenRouter Key: {HasKey}, BaseUrl: {Url}", !string.IsNullOrWhiteSpace(_settings.OpenRouterApiKey) ? "SET" : "MISSING", _settings.OpenRouterBaseUrl ?? "(default)");
        _logger.LogInformation("DeAPI Key: {HasKey}, BaseUrl: {Url}, Model: {Model}", !string.IsNullOrWhiteSpace(_settings.DeApiApiKey) ? "SET" : "MISSING", _settings.DeApiBaseUrl ?? "(default)", _settings.DeApiModel ?? "(default)");
        _logger.LogInformation("Colab Enabled: {Enabled}, BaseUrl: {Url}", _settings.EnableColabFallback, _settings.ColabBaseUrl ?? "(not set)");

        // === Provider 1: OpenRouter ===
        _logger.LogInformation("[1/3] Attempting OpenRouter video start...");
        var orResult = await _openRouter.StartAsync(prompt, options, cancellationToken);
        if (orResult.Success)
        {
            _logger.LogInformation("[1/3] ✅ OpenRouter SUCCESS. JobId={JobId}", orResult.JobId);
            return orResult;
        }
        _logger.LogWarning("[1/3] ❌ OpenRouter FAILED: {Error}", orResult.ErrorMessage);

        // === Provider 2: DeAPI ===
        _logger.LogInformation("[2/3] Attempting DeAPI video start...");
        var deapiResult = await _deapi.StartAsync(prompt, options, cancellationToken);
        if (deapiResult.Success)
        {
            _logger.LogInformation("[2/3] ✅ DeAPI SUCCESS. JobId={JobId}", deapiResult.JobId);
            return deapiResult;
        }
        _logger.LogWarning("[2/3] ❌ DeAPI FAILED: {Error}", deapiResult.ErrorMessage);

        // === Provider 3: Colab ===
        if (_settings.EnableColabFallback)
        {
            _logger.LogInformation("[3/3] Attempting Colab video start...");
            var colabResult = await _colab.StartVideoGenerationAsync(prompt, options, cancellationToken);
            if (colabResult.Success)
            {
                _logger.LogInformation("[3/3] ✅ Colab SUCCESS. JobId={JobId}", colabResult.JobId);
                return VideoGenerationResult.Queued($"colab:{colabResult.JobId}", ProviderName);
            }
            _logger.LogWarning("[3/3] ❌ Colab FAILED: {Error}", colabResult.ErrorMessage);

            var error = $"All providers failed. OpenRouter: [{orResult.ErrorMessage}] | DeAPI: [{deapiResult.ErrorMessage}] | Colab: [{colabResult.ErrorMessage}]";
            _logger.LogError("========= VIDEO GENERATION FAILED ========= {Error}", error);
            return VideoGenerationResult.Fail(error, ProviderName);
        }

        _logger.LogInformation("[3/3] Colab SKIPPED (disabled).");
        var errorMessage = $"Both providers failed. OpenRouter: [{orResult.ErrorMessage}] | DeAPI: [{deapiResult.ErrorMessage}] (Colab Disabled)";
        _logger.LogError("========= VIDEO GENERATION FAILED ========= {Error}", errorMessage);
        return VideoGenerationResult.Fail(errorMessage, ProviderName);
    }

    public async Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return VideoGenerationResult.Fail("JobId is null or empty.", ProviderName);
        }

        if (jobId.StartsWith("deapi:"))
        {
            var opName = jobId["deapi:".Length..];
            return await _deapi.PollAsync(opName, cancellationToken);
        }

        if (jobId.StartsWith("openrouter:"))
        {
            var id = jobId["openrouter:".Length..];
            return await _openRouter.PollAsync(id, cancellationToken);
        }

        if (jobId.StartsWith("colab:"))
        {
            var id = jobId["colab:".Length..];
            return await _colab.CheckStatusAsync(id, cancellationToken);
        }

        return VideoGenerationResult.Fail($"Unknown JobId format: {jobId}", ProviderName);
    }
}
