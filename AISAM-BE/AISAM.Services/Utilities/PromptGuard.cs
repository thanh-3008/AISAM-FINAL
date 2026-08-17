using System.Text.RegularExpressions;

namespace AISAM.Services.Utilities;

public static class PromptGuard
{
    private static readonly string[] InjectionKeywords =
    [
        "ignore previous instructions",
        "ignore all previous",
        "disregard all previous",
        "bỏ qua hướng dẫn",
        "bỏ qua tất cả hướng dẫn",
        "bỏ qua chỉ dẫn",
        "system prompt",
        "developer message",
        "tiết lộ prompt",
        "show system prompt",
        "print system prompt",
        "override instructions",
        "you are now in developer mode",
        "jailbreak"
    ];

    public static bool ContainsInjectionPattern(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalized = input.ToLowerInvariant();
        return InjectionKeywords.Any(keyword => normalized.Contains(keyword));
    }

    public static string SanitizePromptInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        
        // Strip control characters while keeping valid multiline text
        var cleaned = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);
        return cleaned.Trim();
    }
}
