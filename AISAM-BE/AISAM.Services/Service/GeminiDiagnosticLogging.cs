using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AISAM.Services.Service;

/// <summary>
/// Temporary diagnostic-only logging for Gemini generation metadata.
/// Never logs prompts, API keys, or generated text.
/// </summary>
internal static class GeminiDiagnosticLogging
{
    private static readonly AsyncLocal<GenerationContext?> CurrentGeneration = new();

    internal static IDisposable BeginGeneration(string correlationId, string attempt)
    {
        var previous = CurrentGeneration.Value;
        CurrentGeneration.Value = new GenerationContext(correlationId, attempt);
        return new GenerationContextScope(previous);
    }

    internal static void LogRequestConfiguration(
        ILogger logger,
        string provider,
        string model,
        int maxOutputTokens,
        double temperature,
        string responseMimeType)
    {
        logger.LogInformation(
            "DIAGNOSTIC Gemini.RequestConfiguration CorrelationId={CorrelationId} Attempt={Attempt} Provider={Provider} Model={Model} MaxOutputTokens={MaxOutputTokens} Temperature={Temperature} ResponseMimeType={ResponseMimeType}",
            CurrentGeneration.Value?.CorrelationId,
            CurrentGeneration.Value?.Attempt,
            provider,
            model,
            maxOutputTokens,
            temperature,
            responseMimeType);
    }

    internal static void LogResponseMetadata(
        ILogger logger,
        string provider,
        string model,
        JsonElement root)
    {
        var hasCandidates = root.TryGetProperty("candidates", out var candidates)
            && candidates.ValueKind == JsonValueKind.Array;
        var candidateCount = hasCandidates ? candidates.GetArrayLength() : 0;

        var hasUsageMetadata = root.TryGetProperty("usageMetadata", out var usageMetadata)
            && usageMetadata.ValueKind == JsonValueKind.Object;

        logger.LogInformation(
            "DIAGNOSTIC Gemini.ResponseMetadata CorrelationId={CorrelationId} Attempt={Attempt} Provider={Provider} Model={Model} CandidateCount={CandidateCount} PromptTokenCount={PromptTokenCount} CandidatesTokenCount={CandidatesTokenCount} TotalTokenCount={TotalTokenCount} ThoughtsTokenCount={ThoughtsTokenCount} CachedContentTokenCount={CachedContentTokenCount}",
            CurrentGeneration.Value?.CorrelationId,
            CurrentGeneration.Value?.Attempt,
            provider,
            model,
            candidateCount,
            hasUsageMetadata ? ReadInt64(usageMetadata, "promptTokenCount") : null,
            hasUsageMetadata ? ReadInt64(usageMetadata, "candidatesTokenCount") : null,
            hasUsageMetadata ? ReadInt64(usageMetadata, "totalTokenCount") : null,
            hasUsageMetadata ? ReadInt64(usageMetadata, "thoughtsTokenCount") : null,
            hasUsageMetadata ? ReadInt64(usageMetadata, "cachedContentTokenCount") : null);

        if (!hasCandidates)
        {
            return;
        }

        for (var candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
        {
            var candidate = candidates[candidateIndex];
            var finishReason = ReadString(candidate, "finishReason") ?? "UNKNOWN";
            JsonElement parts = default;
            var hasParts = candidate.TryGetProperty("content", out var content)
                && content.ValueKind == JsonValueKind.Object
                && content.TryGetProperty("parts", out parts)
                && parts.ValueKind == JsonValueKind.Array;
            var partCount = hasParts ? parts.GetArrayLength() : 0;

            logger.LogInformation(
                "DIAGNOSTIC Gemini.CandidateMetadata CorrelationId={CorrelationId} Attempt={Attempt} Provider={Provider} Model={Model} CandidateIndex={CandidateIndex} FinishReason={FinishReason} FinishReasonClass={FinishReasonClass} PartCount={PartCount}",
                CurrentGeneration.Value?.CorrelationId,
                CurrentGeneration.Value?.Attempt,
                provider,
                model,
                candidateIndex,
                finishReason,
                ClassifyFinishReason(finishReason),
                partCount);

            if (!hasParts)
            {
                continue;
            }

            for (var partIndex = 0; partIndex < partCount; partIndex++)
            {
                var part = parts[partIndex];
                var textLength = ReadString(part, "text")?.Length;

                logger.LogInformation(
                    "DIAGNOSTIC Gemini.PartMetadata CorrelationId={CorrelationId} Attempt={Attempt} Provider={Provider} Model={Model} CandidateIndex={CandidateIndex} PartIndex={PartIndex} PartType={PartType} IsThought={IsThought} TextLength={TextLength}",
                    CurrentGeneration.Value?.CorrelationId,
                    CurrentGeneration.Value?.Attempt,
                    provider,
                    model,
                    candidateIndex,
                    partIndex,
                    DeterminePartType(part),
                    ReadBoolean(part, "thought"),
                    textLength);
            }
        }
    }

    private static string ClassifyFinishReason(string finishReason) => finishReason.ToUpperInvariant() switch
    {
        "STOP" => "STOP",
        "MAX_TOKENS" => "MAX_TOKENS",
        "SAFETY" => "SAFETY",
        "RECITATION" => "RECITATION",
        "UNKNOWN" => "UNKNOWN",
        _ => "OTHER"
    };

    private static string DeterminePartType(JsonElement part)
    {
        if (part.ValueKind != JsonValueKind.Object) return "UNKNOWN";
        if (part.TryGetProperty("text", out _)) return "text";
        if (part.TryGetProperty("inlineData", out _)) return "inlineData";
        if (part.TryGetProperty("functionCall", out _)) return "functionCall";
        if (part.TryGetProperty("functionResponse", out _)) return "functionResponse";
        if (part.TryGetProperty("executableCode", out _)) return "executableCode";
        if (part.TryGetProperty("codeExecutionResult", out _)) return "codeExecutionResult";
        if (part.TryGetProperty("fileData", out _)) return "fileData";
        return "UNKNOWN";
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static bool? ReadBoolean(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : null;

    private static long? ReadInt64(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt64(out var value)
            ? value
            : null;

    private sealed record GenerationContext(string CorrelationId, string Attempt);

    private sealed class GenerationContextScope(GenerationContext? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            CurrentGeneration.Value = previous;
            _disposed = true;
        }
    }
}
