using System;
using System.Threading;
using System.Threading.Tasks;
using AISAM.Common;
using AISAM.Data.Model;

namespace AISAM.Services.IServices;

public interface IVideoGenerationOrchestrator
{
    Task<GenericResponse<VideoGenerationJob>> StartVideoGenerationAsync(
        Guid workspaceId, 
        Guid userId, 
        string prompt, 
        VideoGenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GenericResponse<VideoGenerationJob>> CheckVideoStatusAsync(
        Guid jobId, 
        Guid workspaceId, 
        CancellationToken cancellationToken = default);
}
