using AISAM.Common.Config;
using AISAM.Services.IServices;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public class CloudinaryMediaStorageService : IMediaStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryMediaStorageService(IOptions<CloudinarySettings> config, ILogger<CloudinaryMediaStorageService> logger)
    {
        var settings = config.Value;
        var cloudName = settings.CloudName;
        var apiKey = settings.ApiKey;
        var apiSecret = settings.ApiSecret;

        if (string.IsNullOrWhiteSpace(cloudName))
        {
            cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
            logger.LogWarning("Cloudinary CloudName not found in configuration, trying env var fallback: '{Value}'", cloudName ?? "");
        }
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
            logger.LogWarning("Cloudinary ApiKey not found in configuration, trying env var fallback: '{Value}'", apiKey ?? "");
        }
        if (string.IsNullOrWhiteSpace(apiSecret))
        {
            apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
            logger.LogWarning("Cloudinary ApiSecret not found in configuration, trying env var fallback: '{Value}'", apiSecret ?? "");
        }

        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(cloudName)) missing.Add("CloudName");
        if (string.IsNullOrWhiteSpace(apiKey)) missing.Add("ApiKey");
        if (string.IsNullOrWhiteSpace(apiSecret)) missing.Add("ApiSecret");
        if (missing.Count > 0)
        {
            var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
            logger.LogError("Cloudinary settings are not fully configured. Missing: {Missing}. .env file path: {EnvPath}, exists: {EnvExists}",
                string.Join(", ", missing), envPath, File.Exists(envPath));
            throw new InvalidOperationException($"Cloudinary settings are not fully configured. Missing: {string.Join(", ", missing)}.");
        }

        var account = new Account(cloudName!, apiKey!, apiSecret!);

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

        Stream stream;
        try
        {
            stream = file.OpenReadStream();
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Cannot read uploaded file: {ex.Message}", nameof(file), ex);
        }

        await using (stream.ConfigureAwait(false))
        {
            var isVideo = file.ContentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) == true;
            var publicId = Path.GetFileNameWithoutExtension(fileName);

            RawUploadParams uploadParams;
            if (isVideo)
            {
                uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    PublicId = publicId,
                    Overwrite = true,
                };
            }
            else
            {
                uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    PublicId = publicId,
                    Overwrite = true,
                };
            }

            RawUploadResult uploadResult;
            try
            {
                if (isVideo)
                {
                    uploadResult = await _cloudinary.UploadLargeAsync((VideoUploadParams)uploadParams, cancellationToken: cancellationToken);
                }
                else
                {
                    uploadResult = await _cloudinary.UploadAsync((ImageUploadParams)uploadParams, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {ex.Message}", ex);
            }

            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            if (uploadResult.SecureUrl == null)
            {
                throw new InvalidOperationException("Cloudinary upload returned no URL.");
            }

            return uploadResult.SecureUrl.ToString();
        }
    }
}
