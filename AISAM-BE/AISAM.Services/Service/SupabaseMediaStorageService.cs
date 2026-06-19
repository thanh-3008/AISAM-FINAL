using System.Net;
using System.Net.Http.Headers;
using AISAM.Common.Config;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class SupabaseMediaStorageService : IMediaStorageService
{
    private readonly HttpClient _httpClient;
    private readonly MediaStorageSettings _settings;

    public SupabaseMediaStorageService(HttpClient httpClient, IOptions<MediaStorageSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> UploadAsync(
        IFormFile file,
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SupabaseUrl) || string.IsNullOrWhiteSpace(_settings.SupabaseKey))
        {
            throw new InvalidOperationException("Supabase storage is not configured.");
        }

        var bucket = string.IsNullOrWhiteSpace(_settings.SupabaseBucket)
            ? "aisam-media"
            : _settings.SupabaseBucket.Trim();
        var objectPath = BuildObjectPath(folder, fileName);
        var uploadUrl = $"{_settings.SupabaseUrl.TrimEnd('/')}/storage/v1/object/{Uri.EscapeDataString(bucket)}/{EncodePath(objectPath)}";

        await using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType);

        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SupabaseKey);
        request.Headers.TryAddWithoutValidation("apikey", _settings.SupabaseKey);
        request.Headers.TryAddWithoutValidation("x-upsert", "false");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("A media file with the generated storage path already exists.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Supabase upload failed ({(int)response.StatusCode}): {body}");
        }

        return $"{_settings.SupabaseUrl.TrimEnd('/')}/storage/v1/object/public/{Uri.EscapeDataString(bucket)}/{EncodePath(objectPath)}";
    }

    private static string BuildObjectPath(string folder, string fileName)
    {
        var safeFolder = folder.Replace('\\', '/').Trim('/');
        return $"{safeFolder}/{fileName}";
    }

    private static string EncodePath(string path)
    {
        return string.Join("/", path
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));
    }
}
