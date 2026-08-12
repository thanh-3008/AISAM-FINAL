using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class AIImageProviderFactory
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<AIImageProviderFactory> _logger;

    public AIImageProviderFactory(IServiceProvider sp, ILogger<AIImageProviderFactory> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    public IAIImageProvider Create()
    {
        var beeknoeeSettings = _sp.GetRequiredService<IOptions<BeeknoeeSettings>>().Value;

        // Dùng Beeknoee nếu API key đã được cấu hình (BEEKNOEE_API_KEY != empty)
        if (!string.IsNullOrWhiteSpace(beeknoeeSettings.ApiKey))
        {
            _logger.LogInformation(
                "[ImageFactory] BeeknoeeImageProvider selected (primary). Model={Model}",
                beeknoeeSettings.DefaultImageModel);
            return _sp.GetRequiredService<BeeknoeeImageProvider>();
        }

        // Fallback: DeAPI + OpenRouter + HuggingFace (provider cũ — không xoá)
        _logger.LogWarning(
            "[ImageFactory] BEEKNOEE_API_KEY not configured — falling back to FallbackImageProvider (DeAPI/OpenRouter/HuggingFace).");
        return _sp.GetRequiredService<FallbackImageProvider>();
    }
}

