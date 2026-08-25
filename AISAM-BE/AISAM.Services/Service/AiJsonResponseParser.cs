using System.Text.Json;

namespace AISAM.Services.Service;

internal static class AiJsonResponseParser
{
    private const int PreviewLimit = 240;

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Disallow
    };

    internal static AiJsonParseResult Parse(string? response)
    {
        var input = response?.Trim() ?? string.Empty;
        if (input.Length == 0)
        {
            return AiJsonParseResult.Failure(
                "AI response is empty.",
                response?.Length ?? 0,
                CreatePreview(input));
        }

        var searchIndex = 0;
        AiJsonParseResult? lastParseFailure = null;

        while (searchIndex < input.Length)
        {
            var startIndex = FindJsonStart(input, searchIndex);
            if (startIndex < 0)
            {
                break;
            }

            if (!TryFindMatchingEnd(input, startIndex, out var endIndex, out var extractionError))
            {
                return AiJsonParseResult.Failure(
                    extractionError,
                    response?.Length ?? 0,
                    CreatePreview(input[startIndex..]));
            }

            var json = input[startIndex..(endIndex + 1)];
            try
            {
                using var _ = JsonDocument.Parse(json, DocumentOptions);
                return AiJsonParseResult.Success(json, response?.Length ?? 0);
            }
            catch (JsonException exception)
            {
                lastParseFailure = AiJsonParseResult.Failure(
                    exception.Message,
                    response?.Length ?? 0,
                    CreatePreview(json),
                    exception.GetType().Name,
                    exception.LineNumber,
                    exception.BytePositionInLine);
                searchIndex = endIndex + 1;
            }
        }

        return lastParseFailure ?? AiJsonParseResult.Failure(
            "AI response does not contain a JSON object or array.",
            response?.Length ?? 0,
            CreatePreview(input));
    }

    private static int FindJsonStart(string input, int searchIndex)
    {
        for (var index = searchIndex; index < input.Length; index++)
        {
            if (input[index] is '{' or '[')
            {
                return index;
            }
        }

        return -1;
    }

    private static bool TryFindMatchingEnd(
        string input,
        int startIndex,
        out int endIndex,
        out string error)
    {
        var delimiters = new Stack<char>();
        var inString = false;
        var escaped = false;

        for (var index = startIndex; index < input.Length; index++)
        {
            var current = input[index];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (current == '\\')
                {
                    escaped = true;
                }
                else if (current == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current is '{' or '[')
            {
                delimiters.Push(current);
                continue;
            }

            if (current is not ('}' or ']'))
            {
                continue;
            }

            if (delimiters.Count == 0 || !IsMatchingPair(delimiters.Peek(), current))
            {
                endIndex = -1;
                error = "AI response contains mismatched JSON delimiters.";
                return false;
            }

            delimiters.Pop();
            if (delimiters.Count == 0)
            {
                endIndex = index;
                error = string.Empty;
                return true;
            }
        }

        endIndex = -1;
        error = inString
            ? "AI response contains an unterminated JSON string."
            : "AI response contains truncated or unbalanced JSON.";
        return false;
    }

    private static bool IsMatchingPair(char opening, char closing) =>
        (opening == '{' && closing == '}') || (opening == '[' && closing == ']');

    private static string CreatePreview(string value)
    {
        var singleLine = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\t", " ", StringComparison.Ordinal);

        return singleLine.Length <= PreviewLimit
            ? singleLine
            : singleLine[..PreviewLimit] + "...";
    }
}

internal sealed record AiJsonParseResult(
    bool IsSuccess,
    string? Json,
    string? ExceptionType,
    string? ErrorMessage,
    long? LineNumber,
    long? BytePositionInLine,
    int ResponseLength,
    string Preview)
{
    internal static AiJsonParseResult Success(string json, int responseLength) =>
        new(true, json, null, null, null, null, responseLength, string.Empty);

    internal static AiJsonParseResult Failure(
        string errorMessage,
        int responseLength,
        string preview,
        string? exceptionType = null,
        long? lineNumber = null,
        long? bytePositionInLine = null) =>
        new(false, null, exceptionType, errorMessage, lineNumber, bytePositionInLine, responseLength, preview);
}
