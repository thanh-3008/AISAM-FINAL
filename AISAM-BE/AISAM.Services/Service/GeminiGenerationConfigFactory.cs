using AISAM.Common.Models;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

internal sealed record GeminiGenerationConfig(
    int MaxOutputTokens,
    double Temperature,
    string ResponseMimeType,
    string? ThinkingLevel,
    IReadOnlyDictionary<string, object> RequestPayload);

internal static class GeminiGenerationConfigFactory
{
    public static GeminiGenerationConfig Create(
        GeminiSettings settings,
        GeminiGenerationOptions? options = null)
    {
        var maxOutputTokens = options?.MaxOutputTokens ?? settings.MaxTokens;
        var responseMimeType = options?.ResponseMimeType ?? "text/plain";
        var thinkingLevel = string.IsNullOrWhiteSpace(options?.ThinkingLevel)
            ? null
            : options.ThinkingLevel;

        var requestPayload = new Dictionary<string, object>
        {
            ["maxOutputTokens"] = maxOutputTokens,
            ["temperature"] = settings.Temperature,
            ["responseMimeType"] = responseMimeType
        };

        if (thinkingLevel is not null)
        {
            requestPayload["thinkingConfig"] = new { thinkingLevel };
        }

        return new GeminiGenerationConfig(
            maxOutputTokens,
            settings.Temperature,
            responseMimeType,
            thinkingLevel,
            requestPayload);
    }
}
