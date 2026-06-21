using AISAM.Common.Config;
using AISAM.Services.IServices;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public class CloudinaryMediaStorageService : IMediaStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryMediaStorageService(IOptions<CloudinarySettings> config)
    {
        var settings = config.Value;
        if (string.IsNullOrWhiteSpace(settings.CloudName) ||
            string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            throw new InvalidOperationException("Cloudinary settings are not fully configured.");
        }

        var account = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret);

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadAsync(
        IFormFile file,
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty", nameof(file));
        }

        using var stream = file.OpenReadStream();
        
        var isVideo = file.ContentType?.StartsWith("video/") == true;
        
        var uploadParams = isVideo 
            ? new VideoUploadParams()
            {
                File = new FileDescription(fileName, stream),
                Folder = folder,
                PublicId = fileName,
                Overwrite = true
            } as RawUploadParams
            : new ImageUploadParams()
            {
                File = new FileDescription(fileName, stream),
                Folder = folder,
                PublicId = fileName,
                Overwrite = true
            } as RawUploadParams;

        RawUploadResult uploadResult;
        if (isVideo)
        {
            uploadResult = await _cloudinary.UploadLargeAsync((VideoUploadParams)uploadParams);
        }
        else
        {
            uploadResult = await _cloudinary.UploadAsync((ImageUploadParams)uploadParams);
        }

        if (uploadResult.Error != null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }
}
