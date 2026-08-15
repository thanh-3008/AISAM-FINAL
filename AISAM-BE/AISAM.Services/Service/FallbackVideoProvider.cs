using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class FallbackVideoProvider : IAIVideoProvider
{
    private readonly DeApiVideoClient _deapi;
    private readonly ColabVideoStrategy _colab;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<FallbackVideoProvider> _logger;

    public string ProviderName => "DeAPI→Colab";

    public FallbackVideoProvider(
        DeApiVideoClient deapi,
        ColabVideoStrategy colab,
        IOptions<VideoProviderSettings> options,
        ILogger<FallbackVideoProvider> logger)
    {
        _deapi = deapi;
        _colab = colab;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("========= VIDEO GENERATION START =========");
        _logger.LogInformation("Prompt: {Prompt}", prompt.Length > 100 ? prompt[..100] + "..." : prompt);
        _logger.LogInformation("Options: AspectRatio={AR}, Duration={Dur}", options?.AspectRatio ?? "9:16", options?.DurationSeconds > 0 ? $"{options.DurationSeconds}s" : "N/A");
        _logger.LogInformation("Provider: DeAPI → Colab (OpenAI Sora disabled for video)");

        // === Provider 1: DeAPI ===
        _logger.LogInformation("[1/2] Attempting DeAPI video start...");
        var deapiResult = await _deapi.StartAsync(prompt, options, cancellationToken);
        if (deapiResult.Success)
        {
            _logger.LogInformation("[1/2] ✅ DeAPI SUCCESS. JobId={JobId}", deapiResult.JobId);
            return deapiResult;
        }
        _logger.LogWarning("[1/2] ❌ DeAPI FAILED: {Error}", deapiResult.ErrorMessage);

        // === Provider 2: Colab ===
        if (_settings.EnableColabFallback && !string.IsNullOrWhiteSpace(_settings.ColabBaseUrl))
        {
            _logger.LogInformation("[2/2] Attempting Colab video start...");
            var colabResult = await _colab.StartVideoGenerationAsync(prompt, options, cancellationToken);
            if (colabResult.Success)
            {
                _logger.LogInformation("[2/2] ✅ Colab SUCCESS. JobId={JobId}", colabResult.JobId);
                return colabResult;
            }
            _logger.LogWarning("[2/2] ❌ Colab FAILED: {Error}", colabResult.ErrorMessage);
            var error = $"DeAPI: [{deapiResult.ErrorMessage}] | Colab: [{colabResult.ErrorMessage}]";
            _logger.LogError("========= VIDEO GENERATION FAILED ========= {Error}", error);
            return VideoGenerationResult.Fail(error, ProviderName);
        }

        _logger.LogError("========= VIDEO GENERATION FAILED ========= {Error}", deapiResult.ErrorMessage);
        return VideoGenerationResult.Fail(deapiResult.ErrorMessage ?? "DeAPI video generation failed.", ProviderName);
    }

    public async Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return VideoGenerationResult.Fail("JobId is null or empty.", ProviderName);

        jobId = jobId.Trim();

        if (jobId.StartsWith("openai-video:", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[FallbackVideoProvider] Legacy openai-video: JobId received. OpenAI video is disabled. Marking as failed.");
            return VideoGenerationResult.Fail("OpenAI video generation is disabled. This job cannot be polled.", ProviderName);
        }

        if (jobId.StartsWith("deapi:", StringComparison.OrdinalIgnoreCase))
        {
            var opName = jobId["deapi:".Length..].Trim();
            return await _deapi.PollAsync(opName, cancellationToken);
        }

        // Colab job IDs don't have a prefix
        return await _colab.CheckStatusAsync(jobId, cancellationToken);
    }
}

