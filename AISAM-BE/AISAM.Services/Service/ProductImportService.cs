using AISAM.Common;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AISAM.Services.Service;

public sealed class ProductImportService : IProductImportService
{
    private const int MaxHtmlBytes = 2 * 1024 * 1024;
    private readonly HttpClient _httpClient;
    private readonly IGeminiTextClient _textClient;

    public ProductImportService(HttpClient httpClient, IGeminiTextClient textClient)
    {
        _httpClient = httpClient;
        _textClient = textClient;
    }

    public async Task<GenericResponse<ProductUrlExtractResponseDto>> ExtractFromUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return GenericResponse<ProductUrlExtractResponseDto>.CreateError("URL sản phẩm không hợp lệ.");
        }

        string html;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 AISAMBot/1.0 (+https://aisam.local)");
            request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                return GenericResponse<ProductUrlExtractResponseDto>.CreateError("Không thể đọc dữ liệu từ trang web này. Vui lòng nhập thủ công.", HttpStatusCode.Forbidden);
            }

            if (!response.IsSuccessStatusCode)
            {
                return GenericResponse<ProductUrlExtractResponseDto>.CreateError($"Không thể đọc URL sản phẩm ({(int)response.StatusCode}). Vui lòng nhập thủ công.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var buffer = new char[8192];
            var builder = new StringBuilder();
            while (builder.Length < MaxHtmlBytes)
            {
                var read = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0) break;
                builder.Append(buffer, 0, read);
            }
            html = builder.ToString();
        }
        catch (TaskCanceledException)
        {
            return GenericResponse<ProductUrlExtractResponseDto>.CreateError("Quá thời gian đọc dữ liệu sản phẩm. Vui lòng thử lại hoặc nhập thủ công.", HttpStatusCode.GatewayTimeout);
        }
        catch (HttpRequestException)
        {
            return GenericResponse<ProductUrlExtractResponseDto>.CreateError("Không thể đọc dữ liệu từ trang web này. Vui lòng nhập thủ công.");
        }

        var raw = ExtractRawProductData(html, uri);
        if (string.IsNullOrWhiteSpace(raw.RawTitle) && string.IsNullOrWhiteSpace(raw.RawDescription))
        {
            return GenericResponse<ProductUrlExtractResponseDto>.CreateError("Không tìm thấy dữ liệu sản phẩm rõ ràng trong trang này. Vui lòng nhập thủ công.");
        }

        var fallback = BuildFallback(raw, uri.ToString());
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(35));
            var aiText = await _textClient.GenerateAsync(BuildAiPrompt(raw), timeoutCts.Token);
            var parsed = ParseAiJson(aiText, fallback);
            parsed.SourceUrl = uri.ToString();
            parsed.Images = MergeImages(parsed.Images, raw.Images);
            parsed.ImportStatus = "Draft";
            return GenericResponse<ProductUrlExtractResponseDto>.CreateSuccess(parsed, "Product extracted successfully");
        }
        catch (TaskCanceledException)
        {
            return GenericResponse<ProductUrlExtractResponseDto>.CreateError("AI xử lý quá lâu. Vui lòng thử lại hoặc nhập thủ công.", HttpStatusCode.GatewayTimeout);
        }
        catch
        {
            return GenericResponse<ProductUrlExtractResponseDto>.CreateSuccess(fallback, "Product extracted with crawler fallback");
        }
    }

    private static RawProductData ExtractRawProductData(string html, Uri sourceUri)
    {
        var metas = ExtractMetaTags(html);
        var jsonLd = ExtractJsonLdProduct(html);

        var title = FirstNonEmpty(
            GetMeta(metas, "og:title"),
            jsonLd.Title,
            ExtractTagText(html, "h1"),
            ExtractTagText(html, "title"));

        var description = FirstNonEmpty(
            GetMeta(metas, "og:description"),
            GetMeta(metas, "description"),
            jsonLd.Description,
            ExtractTagText(html, "p"));

        var images = new List<string>();
        AddIfImage(images, ResolveUrl(GetMeta(metas, "og:image"), sourceUri));
        foreach (var image in jsonLd.Images) AddIfImage(images, ResolveUrl(image, sourceUri));
        foreach (Match match in Regex.Matches(html, "<img\\b[^>]*?src=[\"'](?<src>[^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            AddIfImage(images, ResolveUrl(match.Groups["src"].Value, sourceUri));
            if (images.Count >= 12) break;
        }

        return new RawProductData
        {
            RawTitle = CleanText(title),
            RawDescription = CleanText(description),
            Price = jsonLd.Price ?? ParsePrice(GetMeta(metas, "product:price:amount") ?? GetMeta(metas, "og:price:amount")),
            Url = sourceUri.ToString(),
            Images = images.Distinct(StringComparer.OrdinalIgnoreCase).Take(12).ToList()
        };
    }

    private static Dictionary<string, string> ExtractMetaTags(string html)
    {
        var metas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in Regex.Matches(html, "<meta\\b(?<attrs>[^>]+)>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            var attrs = match.Groups["attrs"].Value;
            var key = ExtractAttribute(attrs, "property") ?? ExtractAttribute(attrs, "name");
            var content = ExtractAttribute(attrs, "content");
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(content))
            {
                metas[key.Trim()] = WebUtility.HtmlDecode(content.Trim());
            }
        }
        return metas;
    }

    private static JsonLdProduct ExtractJsonLdProduct(string html)
    {
        foreach (Match match in Regex.Matches(html, "<script\\b[^>]*type=[\"']application/ld\\+json[\"'][^>]*>(?<json>.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            var json = WebUtility.HtmlDecode(match.Groups["json"].Value.Trim());
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (TryReadProductNode(doc.RootElement, out var product))
                {
                    return product;
                }
            }
            catch (JsonException)
            {
                continue;
            }
        }

        return new JsonLdProduct();
    }

    private static bool TryReadProductNode(JsonElement element, out JsonLdProduct product)
    {
        product = new JsonLdProduct();
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryReadProductNode(item, out product)) return true;
            }
            return false;
        }

        if (element.ValueKind != JsonValueKind.Object) return false;

        if (IsProductType(element))
        {
            product.Title = ReadString(element, "name");
            product.Description = ReadString(element, "description");
            product.Price = ReadOfferPrice(element);
            product.Images = ReadImages(element);
            return true;
        }

        if (element.TryGetProperty("@graph", out var graph))
        {
            return TryReadProductNode(graph, out product);
        }

        return false;
    }

    private static bool IsProductType(JsonElement element)
    {
        if (!element.TryGetProperty("@type", out var type)) return false;
        if (type.ValueKind == JsonValueKind.String) return type.GetString()?.Contains("Product", StringComparison.OrdinalIgnoreCase) == true;
        if (type.ValueKind == JsonValueKind.Array) return type.EnumerateArray().Any(t => t.ValueKind == JsonValueKind.String && t.GetString()?.Contains("Product", StringComparison.OrdinalIgnoreCase) == true);
        return false;
    }

    private static decimal? ReadOfferPrice(JsonElement product)
    {
        // "offers" can be a single Offer object or an array of Offers
        if (!product.TryGetProperty("offers", out var offers)) return null;
        if (offers.ValueKind == JsonValueKind.Array) offers = offers.EnumerateArray().FirstOrDefault();
        if (offers.ValueKind != JsonValueKind.Object) return null;

        // price may be a string, a number, or nested inside priceSpecification
        if (offers.TryGetProperty("price", out var priceEl))
        {
            if (priceEl.ValueKind == JsonValueKind.String) return ParsePrice(priceEl.GetString());
            if (priceEl.ValueKind == JsonValueKind.Number) return priceEl.GetDecimal();
        }

        if (offers.TryGetProperty("lowPrice", out var lowEl))
        {
            if (lowEl.ValueKind == JsonValueKind.String) return ParsePrice(lowEl.GetString());
            if (lowEl.ValueKind == JsonValueKind.Number) return lowEl.GetDecimal();
        }

        // priceSpecification (used by some automotive/real-estate sites)
        if (offers.TryGetProperty("priceSpecification", out var spec))
        {
            if (spec.ValueKind == JsonValueKind.Array) spec = spec.EnumerateArray().FirstOrDefault();
            if (spec.ValueKind == JsonValueKind.Object)
                return ReadOfferPrice(spec); // recurse to handle price / lowPrice inside spec
        }

        return null;
    }

    private static List<string> ReadImages(JsonElement product)
    {
        if (!product.TryGetProperty("image", out var image)) return new List<string>();
        if (image.ValueKind == JsonValueKind.String) return [image.GetString() ?? string.Empty];
        if (image.ValueKind == JsonValueKind.Object) { var u = ReadImageUrl(image); return u != null ? new List<string> { u } : new List<string>(); }
        if (image.ValueKind == JsonValueKind.Array)
            return image.EnumerateArray()
                .Select(ReadImageUrl)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
        return new List<string>();
    }

    /// <summary>
    /// Safely reads an image URL from a JSON element that may be a plain string
    /// or an ImageObject with a "url" / "contentUrl" property.
    /// </summary>
    private static string? ReadImageUrl(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String) return element.GetString();
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in new[] { "url", "contentUrl", "src" })
            {
                if (element.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String)
                    return v.GetString();
            }
        }
        return null;
    }


    private static ProductUrlExtractResponseDto BuildFallback(RawProductData raw, string sourceUrl)
    {
        return new ProductUrlExtractResponseDto
        {
            ProductName = raw.RawTitle ?? "Sản phẩm mới",
            Description = raw.RawDescription,
            Price = raw.Price,
            Images = raw.Images,
            SourceUrl = sourceUrl,
            Benefits = new List<string>(),
            Features = new List<string>(),
            Keywords = new List<string>(),
            ImportStatus = "Draft"
        };
    }

    private static string BuildAiPrompt(RawProductData raw)
    {
        var rawJson = JsonSerializer.Serialize(new
        {
            rawTitle = raw.RawTitle,
            rawDescription = raw.RawDescription,
            price = raw.Price,
            url = raw.Url
        });

        return """
        You are an AI Marketing Strategist. Extract product data from the raw input and output ONLY a valid JSON object with the following keys:
        productName (string), description (string), benefits (array of max 4 strings), features (array of strings), targetAudience (string), tone (string), keywords (array of strings), recommendedCTA (string).
        If missing, infer logically or return null. Output in Vietnamese. Do not include markdown fences, comments, or extra text.

        Raw input:
        """ + rawJson;
    }

    private static ProductUrlExtractResponseDto ParseAiJson(string text, ProductUrlExtractResponseDto fallback)
    {
        var json = ExtractJsonObject(text);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        return new ProductUrlExtractResponseDto
        {
            ProductName = ReadString(root, "productName") ?? fallback.ProductName,
            Description = ReadString(root, "description") ?? fallback.Description,
            Price = fallback.Price,
            Images = fallback.Images,
            SourceUrl = fallback.SourceUrl,
            Benefits = ReadStringArray(root, "benefits").Take(4).ToList(),
            Features = ReadStringArray(root, "features"),
            TargetAudience = ReadString(root, "targetAudience"),
            Tone = ReadString(root, "tone"),
            Keywords = ReadStringArray(root, "keywords"),
            RecommendedCTA = ReadString(root, "recommendedCTA"),
            ImportStatus = "Draft"
        };
    }

    private static string ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) throw new JsonException("AI response did not contain a JSON object.");
        return text[start..(end + 1)];
    }

    private static List<string> MergeImages(IEnumerable<string>? aiImages, IEnumerable<string> crawlerImages)
    {
        return (aiImages ?? Enumerable.Empty<string>())
            .Concat(crawlerImages)
            .Where(IsHttpUrl)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList();
    }

    private static string? ExtractAttribute(string attrs, string name)
    {
        var match = Regex.Match(attrs, $"{name}\\s*=\\s*[\"'](?<value>[^\"']*)[\"']", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["value"].Value : null;
    }

    private static string? ExtractTagText(string html, string tag)
    {
        var match = Regex.Match(html, $"<{tag}\\b[^>]*>(?<text>.*?)</{tag}>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? StripTags(match.Groups["text"].Value) : null;
    }

    private static string? GetMeta(IReadOnlyDictionary<string, string> metas, string key)
    {
        return metas.TryGetValue(key, out var value) ? value : null;
    }

    private static string? ReadString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value)) return null;

        // Plain string — most common case
        if (value.ValueKind == JsonValueKind.String) return CleanText(value.GetString());

        // Some sites put description / name as an array of strings — join them
        if (value.ValueKind == JsonValueKind.Array)
        {
            var parts = value.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            return parts.Count > 0 ? CleanText(string.Join(" ", parts)) : null;
        }

        return null;
    }

    private static List<string> ReadStringArray(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value)) return new List<string>();
        if (value.ValueKind == JsonValueKind.Array)
        {
            return value.EnumerateArray()
                .Select(x =>
                {
                    // Plain string item
                    if (x.ValueKind == JsonValueKind.String) return CleanText(x.GetString());
                    // Object item — try common string-value keys (e.g. {"name":"..."})
                    if (x.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var key in new[] { "name", "value", "text", "description", "label" })
                        {
                            if (x.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String)
                                return CleanText(v.GetString());
                        }
                    }
                    return null;
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var single = CleanText(value.GetString());
            return string.IsNullOrWhiteSpace(single) ? new List<string>() : [single];
        }

        return new List<string>();
    }

    private static decimal? ParsePrice(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var normalized = Regex.Replace(raw, "[^0-9,.]", "");
        if (string.IsNullOrWhiteSpace(normalized)) return null;
        normalized = normalized.Replace(",", "");
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ? price : null;
    }

    private static string? ResolveUrl(string? value, Uri baseUri)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var absolute)) return absolute.ToString();
        if (Uri.TryCreate(baseUri, value.Trim(), out var relative)) return relative.ToString();
        return null;
    }

    private static void AddIfImage(ICollection<string> images, string? value)
    {
        if (IsHttpUrl(value) && !images.Contains(value!, StringComparer.OrdinalIgnoreCase)) images.Add(value!);
    }

    private static bool IsHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string? CleanText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var decoded = WebUtility.HtmlDecode(StripTags(value));
        return Regex.Replace(decoded, "\\s+", " ").Trim();
    }

    private static string StripTags(string value)
    {
        return Regex.Replace(value, "<.*?>", " ");
    }

    private sealed class RawProductData
    {
        public string? RawTitle { get; set; }
        public string? RawDescription { get; set; }
        public decimal? Price { get; set; }
        public string Url { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new();
    }

    private sealed class JsonLdProduct
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public List<string> Images { get; set; } = new();
    }
}
