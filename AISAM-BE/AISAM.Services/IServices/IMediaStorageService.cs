using Microsoft.AspNetCore.Http;

namespace AISAM.Services.IServices;

public interface IMediaStorageService
{
    Task<string> UploadAsync(
        IFormFile file,
        string folder,
        string fileName,
        CancellationToken cancellationToken = default);
}
