using AISAM.Services.IServices;
using Microsoft.Extensions.DependencyInjection;

namespace AISAM.Services.Service;

public sealed class AIImageProviderFactory
{
    private readonly IServiceProvider _sp;

    public AIImageProviderFactory(IServiceProvider sp)
    {
        _sp = sp;
    }

    public IAIImageProvider Create()
    {
        return _sp.GetRequiredService<FallbackImageProvider>();
    }
}
