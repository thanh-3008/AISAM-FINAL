using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class AIVideoProviderFactory
{
    private readonly IServiceProvider _sp;
    private readonly VideoProviderSettings _settings;

    public AIVideoProviderFactory(IServiceProvider sp, IOptions<VideoProviderSettings> options)
    {
        _sp = sp;
        _settings = options.Value;
    }

    public IAIVideoProvider Create()
    {
        return _settings.Enabled
            ? _sp.GetRequiredService<FallbackVideoProvider>()
            : _sp.GetRequiredService<NullVideoProvider>();
    }
}
