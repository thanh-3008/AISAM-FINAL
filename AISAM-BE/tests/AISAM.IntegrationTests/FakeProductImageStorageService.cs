using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;

namespace AISAM.IntegrationTests;

internal sealed class FakeProductImageStorageService : IProductImageStorageService
{
    public Task<IReadOnlyList<string>> UploadAsync(
        IReadOnlyCollection<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> urls = files
            .Select(file => $"/uploads/products/{file.FileName}")
            .ToList();
        return Task.FromResult(urls);
    }
}
