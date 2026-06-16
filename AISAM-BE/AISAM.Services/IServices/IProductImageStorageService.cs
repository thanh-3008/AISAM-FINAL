using Microsoft.AspNetCore.Http;

namespace AISAM.Services.IServices;

public interface IProductImageStorageService
{
    Task<IReadOnlyList<string>> UploadAsync(
        IReadOnlyCollection<IFormFile> files,
        CancellationToken cancellationToken = default);
}
