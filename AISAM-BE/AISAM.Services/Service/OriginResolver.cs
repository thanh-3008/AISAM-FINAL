using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AISAM.Services.Service;

public sealed class OriginResolver : IOriginResolver
{
    private readonly HashSet<string> _allowedOrigins;
    private readonly string _defaultOrigin;

    public OriginResolver(IConfiguration configuration)
    {
        _allowedOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddOrigin(string? origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                return;
            }

            var cleaned = Normalize(origin);
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                _allowedOrigins.Add(cleaned);
            }
        }

        foreach (var origin in configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
        {
            AddOrigin(origin);
        }

        AddOrigin(configuration["FrontendSettings:BaseUrl"]);

        var envOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
            ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");

        foreach (var origin in (envOrigins ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            AddOrigin(origin);
        }

        var configuredBase = Normalize(configuration["FrontendSettings:BaseUrl"] ?? Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? string.Empty);
        _defaultOrigin = (configuredBase != null && _allowedOrigins.Contains(configuredBase))
            ? configuredBase
            : _allowedOrigins.FirstOrDefault(o => o.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                ?? _allowedOrigins.FirstOrDefault()
                ?? "https://aisam.io.vn";
    }

    public string ResolveOrigin(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Origin header
        if (request.Headers.TryGetValue("Origin", out var originValues))
        {
            var originHeader = originValues.ToString();
            if (!string.IsNullOrWhiteSpace(originHeader))
            {
                return ResolveOrigin(originHeader);
            }
        }

        // 2. Referer header
        if (request.Headers.TryGetValue("Referer", out var refererValues))
        {
            var refererHeader = refererValues.ToString();
            if (!string.IsNullOrWhiteSpace(refererHeader) &&
                Uri.TryCreate(refererHeader, UriKind.Absolute, out var refererUri))
            {
                var candidate = $"{refererUri.Scheme}://{refererUri.Authority}";
                return ResolveOrigin(candidate);
            }
        }

        // 3. X-Forwarded-Host or Host
        string? host = null;
        if (request.Headers.TryGetValue("X-Forwarded-Host", out var fwdHostValues) && !string.IsNullOrWhiteSpace(fwdHostValues.ToString()))
        {
            host = fwdHostValues.ToString().Split(',')[0].Trim();
        }
        else if (request.Host.HasValue)
        {
            host = request.Host.Value;
        }

        if (!string.IsNullOrWhiteSpace(host))
        {
            var scheme = request.Headers.TryGetValue("X-Forwarded-Proto", out var fwdProtoValues) && !string.IsNullOrWhiteSpace(fwdProtoValues.ToString())
                ? fwdProtoValues.ToString().Split(',')[0].Trim()
                : (string.IsNullOrWhiteSpace(request.Scheme) ? "https" : request.Scheme);

            var candidate = $"{scheme}://{host}";
            var normalized = Normalize(candidate);
            if (normalized != null && _allowedOrigins.Contains(normalized))
            {
                return normalized;
            }
        }

        return _defaultOrigin;
    }

    public string ResolveOrigin(string? candidateOrigin)
    {
        if (string.IsNullOrWhiteSpace(candidateOrigin))
        {
            return _defaultOrigin;
        }

        var normalized = Normalize(candidateOrigin);
        if (normalized != null && _allowedOrigins.Contains(normalized))
        {
            return normalized;
        }

        throw new InvalidOperationException($"Origin '{candidateOrigin}' is not allowed.");
    }

    public bool IsAllowedOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return false;
        }

        var normalized = Normalize(origin);
        return normalized != null && _allowedOrigins.Contains(normalized);
    }

    private static string? Normalize(string origin)
    {
        var cleaned = origin.Trim().TrimEnd('/');
        if (Uri.TryCreate(cleaned, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return $"{uri.Scheme}://{uri.Authority}".ToLowerInvariant();
        }

        return null;
    }
}
