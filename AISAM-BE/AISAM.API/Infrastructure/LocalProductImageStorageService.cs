using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;

namespace AISAM.API.Infrastructure;

public sealed class LocalProductImageStorageService : IProductImageStorageService
{
    private const int MaxFiles = 5;
    private const long MaxFileSize = 5 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif"
        };

    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalProductImageStorageService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<string>> UploadAsync(
        IReadOnlyCollection<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        if (files.Count > MaxFiles)
        {
            throw new InvalidOperationException($"A product can have at most {MaxFiles} images.");
        }

        var uploadDirectory = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "products");
        Directory.CreateDirectory(uploadDirectory);

        var urls = new List<string>(files.Count);
        foreach (var file in files)
        {
            if (file.Length <= 0 || file.Length > MaxFileSize)
            {
                throw new InvalidOperationException("Each product image must be between 1 byte and 5 MB.");
            }

            if (!AllowedTypes.TryGetValue(file.ContentType, out var extension))
            {
                throw new InvalidOperationException("Product images must be JPEG, PNG, WebP, or GIF files.");
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);
            await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream, cancellationToken);
            urls.Add(BuildPublicUrl(fileName));
        }

        return urls;
    }

    private string BuildPublicUrl(string fileName)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var relativePath = $"/uploads/products/{fileName}";
        return request == null ? relativePath : $"{request.Scheme}://{request.Host}{relativePath}";
    }
}
