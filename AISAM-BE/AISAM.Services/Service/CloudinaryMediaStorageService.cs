using AISAM.Common.Config;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public class CloudinaryMediaStorageService : IMediaStorageService
{
    public async Task<bool> DeleteAsync(string publicId,bool video,CancellationToken cancellationToken=default)
    {
        var result=await _cloudinary.DestroyAsync(new DeletionParams(publicId){ResourceType=video?ResourceType.Video:ResourceType.Image,Invalidate=true});
        return result.Result is "ok" or "not found";
    }
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryMediaStorageService> _logger;

    public CloudinaryMediaStorageService(IOptions<CloudinarySettings> config, ILogger<CloudinaryMediaStorageService> logger)
    {
        _logger = logger;
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
            logger.LogWarning("Cloudinary ApiKey not found in configuration, trying env var fallback.");
        }
        if (string.IsNullOrWhiteSpace(apiSecret))
        {
            apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
            logger.LogWarning("Cloudinary ApiSecret not found in configuration, trying env var fallback.");
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
        IFormFile file,string folder,string fileName,CancellationToken cancellationToken=default)
        =>(await UploadDetailedAsync(file,folder,fileName,cancellationToken)).Url;

    public async Task<StoredMedia> UploadDetailedAsync(
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

            return uploadResult switch
            {
                VideoUploadResult video=>new(uploadResult.SecureUrl.ToString(),video.Width,video.Height,(decimal)video.Duration,uploadResult.PublicId,video.Bytes),
                ImageUploadResult image=>new(uploadResult.SecureUrl.ToString(),image.Width,image.Height,null,uploadResult.PublicId,image.Bytes>0?image.Bytes:file.Length),
                _=>new(uploadResult.SecureUrl.ToString(),PublicId:uploadResult.PublicId,SizeBytes:file.Length)
            };
        }
    }

    public async Task<string> UploadBytesAsync(
        byte[] data,
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
        => (await UploadBytesDetailedAsync(data, folder, fileName, cancellationToken)).Url;

    public async Task<StoredMedia> UploadBytesDetailedAsync(
        byte[] data,
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (data == null || data.Length == 0)
        {
            throw new ArgumentException("Data is empty", nameof(data));
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var isVideo = extension == ".mp4" || extension == ".mov" || extension == ".avi" || extension == ".mkv";
        
        using (var stream = new MemoryStream(data))
        {
            RawUploadParams uploadParams;

            if (isVideo)
            {
                uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    PublicId = Path.GetFileNameWithoutExtension(fileName),
                    Overwrite = true,
                };
            }
            else
            {
                uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    PublicId = Path.GetFileNameWithoutExtension(fileName),
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
                throw new InvalidOperationException($"Cloudinary byte upload failed: {ex.Message}", ex);
            }

            if (uploadResult.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary byte upload failed: {uploadResult.Error.Message}");
            }

            if (uploadResult.SecureUrl == null)
            {
                throw new InvalidOperationException("Cloudinary byte upload returned no URL.");
            }

            return uploadResult switch
            {
                VideoUploadResult video => new(uploadResult.SecureUrl.ToString(), video.Width, video.Height, (decimal)video.Duration, uploadResult.PublicId, video.Bytes),
                ImageUploadResult image => new(uploadResult.SecureUrl.ToString(), image.Width, image.Height, null, uploadResult.PublicId, image.Bytes > 0 ? image.Bytes : (long)data.Length),
                _ => new(uploadResult.SecureUrl.ToString(), PublicId: uploadResult.PublicId, SizeBytes: data.Length)
            };
        }
    }

    public async Task<StoredMedia?> GetMediaMetadataAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
            var path = uri.AbsolutePath;
            var marker = "/upload/";
            var idx = path.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            var afterUpload = path[(idx + marker.Length)..];
            var segments = afterUpload.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0) return null;

            var publicIdWithExt = string.Join("/", segments.Length > 1 && segments[0].StartsWith("v", StringComparison.OrdinalIgnoreCase) && long.TryParse(segments[0][1..], out _) ? segments.Skip(1) : segments);
            var publicId = Path.Combine(Path.GetDirectoryName(publicIdWithExt) ?? "", Path.GetFileNameWithoutExtension(publicIdWithExt)).Replace('\\', '/');

            var res = await _cloudinary.GetResourceAsync(new GetResourceParams(publicId)
            {
                ResourceType = ResourceType.Video,
                ImageMetadata = true
            }, cancellationToken);

            if (res.Error != null || res.JsonObj == null) return null;

            decimal? duration = null;
            var durStr = res.JsonObj["duration"]?.ToString();
            if (!string.IsNullOrWhiteSpace(durStr) && decimal.TryParse(durStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedDur))
            {
                duration = parsedDur;
            }

            return new StoredMedia(url, res.Width, res.Height, duration, publicId, res.Bytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve Cloudinary metadata for URL: {Url}", url);
            return null;
        }
    }
}
