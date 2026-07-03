using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class NullVideoProvider : IAIVideoProvider
{
    public string ProviderName => "None";

    public Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VideoGenerationResult.Fail("Video generation is not enabled. Please contact support.", ProviderName));
    }

    public Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VideoGenerationResult.Fail("Video generation is not enabled.", ProviderName));
    }
}
