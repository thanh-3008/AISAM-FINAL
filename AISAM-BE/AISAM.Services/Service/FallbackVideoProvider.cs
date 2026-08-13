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

        // === Provider 1: OpenAI ===
        _logger.LogInformation("[1/2] Attempting OpenAI video start...");
        var openAiResult = await _openAI.StartAsync(prompt, options, cancellationToken);
        if (openAiResult.Success)
        {
            _logger.LogInformation("[1/2] ✅ OpenAI SUCCESS. JobId={JobId}", openAiResult.JobId);
            return openAiResult;
        }
        _logger.LogWarning("[1/2] ❌ OpenAI FAILED: {Error}", openAiResult.ErrorMessage);

        // === Provider 2: DeAPI ===
        _logger.LogInformation("[2/2] Attempting DeAPI video start...");
        var deapiResult = await _deapi.StartAsync(prompt, options, cancellationToken);
        if (deapiResult.Success)
        {
            _logger.LogInformation("[2/2] ✅ DeAPI SUCCESS. JobId={JobId}", deapiResult.JobId);
            return deapiResult;
        }
        _logger.LogWarning("[2/2] ❌ DeAPI FAILED: {Error}", deapiResult.ErrorMessage);

        var error = $"OpenAI failed: [{openAiResult.ErrorMessage}] | DeAPI failed: [{deapiResult.ErrorMessage}]";
        _logger.LogError("========= VIDEO GENERATION FAILED ========= {Error}", error);
        return VideoGenerationResult.Fail(error, ProviderName);
    }

    public async Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return VideoGenerationResult.Fail("JobId is null or empty.", ProviderName);
        }

        if (jobId.StartsWith("openai-video:"))
        {
            var opName = jobId["openai-video:".Length..];
            return await _openAI.PollAsync(opName, cancellationToken);
        }
        if (jobId.StartsWith("deapi:"))
        {
            var opName = jobId["deapi:".Length..];
            return await _deapi.PollAsync(opName, cancellationToken);
        }

        return VideoGenerationResult.Fail($"Unknown JobId format: {jobId}", ProviderName);
    }
}
