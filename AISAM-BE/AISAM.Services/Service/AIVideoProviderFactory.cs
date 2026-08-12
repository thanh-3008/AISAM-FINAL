using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class AIVideoProviderFactory
{
    private readonly IServiceProvider _sp;
    private readonly VideoProviderSettings _videoSettings;
    private readonly ILogger<AIVideoProviderFactory> _logger;

    public AIVideoProviderFactory(
        IServiceProvider sp,
        IOptions<VideoProviderSettings> videoOptions,
        ILogger<AIVideoProviderFactory> logger)
    {
        _sp = sp;
        _videoSettings = videoOptions.Value;
        _logger = logger;
    }

    public IAIVideoProvider Create()
    {
        // Video bị tắt hoàn toàn
        if (!_videoSettings.Enabled)
        {
            _logger.LogInformation("[VideoFactory] VIDEO_ENABLED=false — using NullVideoProvider.");
            return _sp.GetRequiredService<NullVideoProvider>();
        }

        // Beeknoee là primary khi API key được cấu hình
        var beeknoeeSettings = _sp.GetRequiredService<IOptions<BeeknoeeSettings>>().Value;
        if (!string.IsNullOrWhiteSpace(beeknoeeSettings.ApiKey))
        {
            _logger.LogInformation(
                "[VideoFactory] BeeknoeeVideoProvider selected (primary). Model={Model}",
                beeknoeeSettings.DefaultVideoModel);
            return _sp.GetRequiredService<BeeknoeeVideoProvider>();
        }

        // Fallback: DeAPI (provider cũ — không xoá)
        _logger.LogWarning(
            "[VideoFactory] BEEKNOEE_API_KEY not configured — falling back to FallbackVideoProvider (DeAPI).");
        return _sp.GetRequiredService<FallbackVideoProvider>();
    }
}

