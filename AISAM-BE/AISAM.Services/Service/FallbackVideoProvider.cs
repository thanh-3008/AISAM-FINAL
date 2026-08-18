using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class FallbackVideoProvider : IAIVideoProvider
{
    private readonly OpenAIVideoClient _openAI;
    private readonly DeApiVideoClient _deapi;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<FallbackVideoProvider> _logger;

    public string ProviderName => "OpenAI→DeAPI";

    public FallbackVideoProvider(
        OpenAIVideoClient openAI,
        DeApiVideoClient deapi,
        IOptions<VideoProviderSettings> options,
        ILogger<FallbackVideoProvider> logger)
    {
        _openAI = openAI;
        _deapi = deapi;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("========= VIDEO GENERATION START =========");
        _logger.LogInformation("Prompt: {Prompt}", prompt.Length > 100 ? prompt[..100] + "..." : prompt);
        _logger.LogInformation("Options: AspectRatio={AR}, Duration={Dur}", options?.AspectRatio ?? "9:16", options?.DurationSeconds > 0 ? $"{options.DurationSeconds}s" : "N/A (Default/Segmented)");
        _logger.LogInformation("Provider: OpenAI -> DeAPI");

        // === Tạm thời khóa OpenAI và nhảy thẳng tới DeAPI theo yêu cầu ===
        _logger.LogInformation("Attempting DeAPI video start (OpenAI bypassed)...");
        var deapiResult = await _deapi.StartAsync(prompt, options, cancellationToken);
        if (deapiResult.Success)
        {
            _logger.LogInformation("✅ DeAPI SUCCESS. JobId={JobId}", deapiResult.JobId);
            return deapiResult;
        }
        
        _logger.LogWarning("❌ DeAPI FAILED: {Error}", deapiResult.ErrorMessage);
        _logger.LogError("========= VIDEO GENERATION FAILED ========= DeAPI Error: {Error}", deapiResult.ErrorMessage);
        
        return VideoGenerationResult.Fail(deapiResult.ErrorMessage ?? "Unknown error", "DeAPI");
    }

    public async Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return VideoGenerationResult.Fail("JobId is null or empty.", ProviderName);
        }

        jobId = jobId.Trim();

        if (jobId.StartsWith("openai-video:", StringComparison.OrdinalIgnoreCase))
        {
            var opName = jobId["openai-video:".Length..].Trim();
            return await _openAI.PollAsync(opName, cancellationToken);
        }
        if (jobId.StartsWith("deapi:", StringComparison.OrdinalIgnoreCase))
        {
            var opName = jobId["deapi:".Length..].Trim();
            return await _deapi.PollAsync(opName, cancellationToken);
        }

        return VideoGenerationResult.Fail($"Unknown JobId format: {jobId}", ProviderName);
    }
}
