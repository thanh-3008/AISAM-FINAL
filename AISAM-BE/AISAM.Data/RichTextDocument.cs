using System.Text;
using System.Text.Json;

namespace AISAM.Data;

public static class RichTextDocument
{
    public const int Version = 1;
    public static string PlainText(string json, int? version)
    {
        if (version != Version || json.Length > 200_000) throw new ArgumentException("RICH_TEXT_INVALID_VERSION_OR_SIZE");
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 32 });
            var count = 0;
            return Render(document.RootElement, "root", ref count, false);
        }
        catch (JsonException) { throw new ArgumentException("RICH_TEXT_INVALID_JSON"); }
        catch (InvalidOperationException) { throw new ArgumentException("RICH_TEXT_INVALID_SHAPE"); }
    }

    private static string Render(JsonElement node, string parent, ref int count, bool formatForFacebook)
    {
        if (++count > 5000 || node.ValueKind != JsonValueKind.Object ||
            !node.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
            throw new ArgumentException("RICH_TEXT_INVALID_NODE");
        var type = typeElement.GetString()!;
        var valid = parent switch
        {
            "root" => type == "doc",
            "doc" or "listItem" => type is "paragraph" or "heading" or "bulletList" or "orderedList" or "blockquote",
            "blockquote" => type is "paragraph" or "heading" or "bulletList" or "orderedList",
            "bulletList" or "orderedList" => type == "listItem",
            "paragraph" or "heading" => type is "text" or "hardBreak",
            _ => false
        };
        if (!valid) throw new ArgumentException("RICH_TEXT_UNSUPPORTED_NODE");
        foreach (var property in node.EnumerateObject())
            if (property.Name is not ("type" or "content" or "text" or "marks" or "attrs")) throw new ArgumentException("RICH_TEXT_UNSUPPORTED_PROPERTY");
        if (node.TryGetProperty("attrs", out var attrs))
        {
            if (attrs.ValueKind != JsonValueKind.Object) throw new ArgumentException("RICH_TEXT_INVALID_ATTRIBUTES");
            foreach (var attribute in attrs.EnumerateObject())
                if (!(type == "heading" && attribute.Name == "level" && attribute.Value.TryGetInt32(out var level) && level is >= 1 and <= 3) &&
                    !(type == "orderedList" && attribute.Name == "start" && attribute.Value.TryGetInt32(out var start) && start is >= 1 and <= 9999) &&
                    !(type == "orderedList" && attribute.Name == "type" && (attribute.Value.ValueKind == JsonValueKind.Null || attribute.Value.GetString() == "1")))
                    throw new ArgumentException("RICH_TEXT_INVALID_ATTRIBUTES");
        }
        if (type == "text")
        {
            if (!node.TryGetProperty("text", out var text) || text.ValueKind != JsonValueKind.String || node.TryGetProperty("content", out _))
                throw new ArgumentException("RICH_TEXT_INVALID_TEXT");
            var result = text.GetString()!;
            string? linkSuffix = null;
            var bold = false;
            var italic = false;
            var underline = false;
            if (node.TryGetProperty("marks", out var marks))
            {
                if (marks.ValueKind != JsonValueKind.Array || marks.GetArrayLength() > 6) throw new ArgumentException("RICH_TEXT_INVALID_MARKS");
                foreach (var mark in marks.EnumerateArray())
                {
                    if (!mark.TryGetProperty("type", out var markType) || markType.ValueKind != JsonValueKind.String ||
                        markType.GetString() is not ("bold" or "italic" or "underline" or "strike" or "highlight" or "link"))
                        throw new ArgumentException("RICH_TEXT_UNSUPPORTED_MARK");
                    foreach (var property in mark.EnumerateObject())
                        if (property.Name != "type" && !(property.Name == "attrs" && markType.GetString() == "link"))
                            throw new ArgumentException("RICH_TEXT_INVALID_MARK_ATTRIBUTES");
                    if (markType.GetString() == "link")
                    {
                        if (!mark.TryGetProperty("attrs", out var link) || link.ValueKind != JsonValueKind.Object ||
                            !link.TryGetProperty("href", out var href) || href.ValueKind != JsonValueKind.String || !SafeLink(href.GetString()))
                            throw new ArgumentException("RICH_TEXT_UNSAFE_LINK");
                        foreach (var attribute in link.EnumerateObject())
                            if (attribute.Name is not ("href" or "target" or "rel" or "class" or "title")) throw new ArgumentException("RICH_TEXT_INVALID_LINK_ATTRIBUTES");
                        if (result != href.GetString()) linkSuffix = $" ({href.GetString()})";
                    }
                    bold |= markType.GetString() == "bold";
                    italic |= markType.GetString() == "italic";
                    underline |= markType.GetString() == "underline";
                }
            }
            var formatted = formatForFacebook ? FormatFacebookText(result, bold, italic, underline) : result;
            return formatted + linkSuffix;
        }
        if (node.TryGetProperty("text", out _) || node.TryGetProperty("marks", out _)) throw new ArgumentException("RICH_TEXT_INVALID_BLOCK");
        if (type == "hardBreak")
        {
            if (node.TryGetProperty("content", out _)) throw new ArgumentException("RICH_TEXT_INVALID_BREAK");
            return "\n";
        }
        var children = new List<string>();
        if (node.TryGetProperty("content", out var content))
        {
            if (content.ValueKind != JsonValueKind.Array) throw new ArgumentException("RICH_TEXT_INVALID_CONTENT");
            foreach (var child in content.EnumerateArray()) children.Add(Render(child, type, ref count, formatForFacebook));
        }
        var first = type == "orderedList" && attrs.ValueKind == JsonValueKind.Object && attrs.TryGetProperty("start", out var ordinal) ? ordinal.GetInt32() : 1;
        return type switch
        {
            "bulletList" => string.Join("\n", children.Select(c => "• " + c.Replace("\n", "\n  "))),
            "orderedList" => string.Join("\n", children.Select((c, i) => $"{first + i}. " + c.Replace("\n", "\n   "))),
            "doc" or "blockquote" => string.Join("\n\n", children),
            "listItem" => string.Join("\n", children),
            _ => string.Concat(children)
        };
    }

    public static bool SafeLink(string? value) => value is not null && value.Length <= 2048 &&
        !value.Any(char.IsControl) && Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme is "http" or "https" or "mailto");

    public static Dictionary<string, string> FormatCaptions(string plainText, string? richTextJson = null, int? version = null) => new()
    {
        ["facebook"] = richTextJson is null ? plainText : FacebookText(richTextJson, version),
        ["instagram"] = plainText,
        ["tiktok"] = plainText,
        ["google"] = plainText
    };

    private static string FacebookText(string json, int? version)
    {
        if (version != Version || json.Length > 200_000) throw new ArgumentException("RICH_TEXT_INVALID_VERSION_OR_SIZE");
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 32 });
            var count = 0;
            return Render(document.RootElement, "root", ref count, true);
        }
        catch (JsonException) { throw new ArgumentException("RICH_TEXT_INVALID_JSON"); }
        catch (InvalidOperationException) { throw new ArgumentException("RICH_TEXT_INVALID_SHAPE"); }
    }

    private static string FormatFacebookText(string value, bool bold, bool italic, bool underline)
    {
        if (!bold && !italic && !underline) return value;
        var result = new StringBuilder(value.Length * (underline ? 2 : 1));
        foreach (var rune in value.EnumerateRunes())
        {
            result.Append(StyleAsciiRune(rune.Value, bold, italic));
            if (underline && !Rune.IsWhiteSpace(rune)) result.Append('\u0332');
        }
        return result.ToString();
    }

    private static string StyleAsciiRune(int value, bool bold, bool italic)
    {
        if (value is >= 'A' and <= 'Z')
        {
            var start = bold && italic ? 0x1D63C : bold ? 0x1D5D4 : italic ? 0x1D608 : 0;
            if (start != 0) return char.ConvertFromUtf32(start + value - 'A');
        }
        if (value is >= 'a' and <= 'z')
        {
            var start = bold && italic ? 0x1D656 : bold ? 0x1D5EE : italic ? 0x1D622 : 0;
            if (start != 0) return char.ConvertFromUtf32(start + value - 'a');
        }
        if (bold && value is >= '0' and <= '9')
            return char.ConvertFromUtf32(0x1D7EC + value - '0');
        return char.ConvertFromUtf32(value);
    }
}
