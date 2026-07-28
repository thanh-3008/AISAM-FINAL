using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class FallbackVideoProvider : IAIVideoProvider
{
    private readonly DeApiVideoClient _deapi;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<FallbackVideoProvider> _logger;

    public string ProviderName => "DeAPI";

    public FallbackVideoProvider(
        DeApiVideoClient deapi,
        IOptions<VideoProviderSettings> options,
        ILogger<FallbackVideoProvider> logger)
    {
        _deapi = deapi;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("========= VIDEO GENERATION START =========");
        _logger.LogInformation("Prompt: {Prompt}", prompt.Length > 100 ? prompt[..100] + "..." : prompt);
        _logger.LogInformation("Options: AspectRatio={AR}, Duration={Dur}", options?.AspectRatio ?? "9:16", options?.DurationSeconds > 0 ? $"{options.DurationSeconds}s" : "N/A (Default/Segmented)");
        _logger.LogInformation("Provider: DeAPI");
        _logger.LogInformation("DeAPI Key: {HasKey}, BaseUrl: {Url}, Model: {Model}", !string.IsNullOrWhiteSpace(_settings.DeApiApiKey) ? "SET" : "MISSING", _settings.DeApiBaseUrl ?? "(default)", _settings.DeApiModel ?? "(default)");

        // === Provider 1: DeAPI ===
        _logger.LogInformation("[1/1] Attempting DeAPI video start...");
        var deapiResult = await _deapi.StartAsync(prompt, options, cancellationToken);
        if (deapiResult.Success)
        {
            _logger.LogInformation("[1/1] ✅ DeAPI SUCCESS. JobId={JobId}", deapiResult.JobId);
            return deapiResult;
        }
        _logger.LogWarning("[1/1] ❌ DeAPI FAILED: {Error}", deapiResult.ErrorMessage);

        var error = $"DeAPI failed: [{deapiResult.ErrorMessage}]";
        _logger.LogError("========= VIDEO GENERATION FAILED ========= {Error}", error);
        return VideoGenerationResult.Fail(error, ProviderName);
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

        return VideoGenerationResult.Fail($"Unknown JobId format: {jobId}", ProviderName);
    }
}
