using System.Text.Json;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class AutomationGenerationService : IAutomationGenerationService
{
    private readonly AisamContext _context;
    private readonly IGeminiTextClient _textClient;
    private readonly IAIImageProvider _imageProvider;
    private readonly IAIVideoProvider _videoProvider;
    private readonly IMediaStorageService _mediaStorage;
    private readonly IAutomationCreditService _automationCredits;
    private readonly ILogger<AutomationGenerationService> _logger;

    public AutomationGenerationService(
        AisamContext context,
        IGeminiTextClient textClient,
        IAIImageProvider imageProvider,
        IAIVideoProvider videoProvider,
        IMediaStorageService mediaStorage,
        IAutomationCreditService automationCredits,
        ILogger<AutomationGenerationService> logger)
    {
        _context = context;
        _textClient = textClient;
        _imageProvider = imageProvider;
        _videoProvider = videoProvider;
        _mediaStorage = mediaStorage;
        _automationCredits = automationCredits;
        _logger = logger;
    }

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default)
    {
        var item = await _context.AutomationItems
            .Include(value => value.AutomationPlan)
            .Include(value => value.Brand)
            .Include(value => value.Product)
            .Where(value => value.AutomationPlan.Status == AutomationPlanStatusEnum.Generating &&
                            (value.Status == AutomationItemStatusEnum.Pending ||
                             (value.Status == AutomationItemStatusEnum.GeneratingMedia && value.VideoJobId != null)))
            .OrderBy(value => value.Status == AutomationItemStatusEnum.Pending ? 0 : 1)
            .ThenBy(value => value.AutomationPlan.CreatedAt)
            .ThenBy(value => value.RowIndex)
            .ThenBy(value => value.Platform)
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null) return false;

        var profile = await _context.Profiles.AsNoTracking()
            .FirstOrDefaultAsync(value => value.Id == item.AutomationPlan.ProfileId, cancellationToken);
        if (profile is null)
        {
            await FailAsync(item, "The profile that created this plan no longer exists.", cancellationToken);
            return true;
        }

        var resumingVideo = item.Status == AutomationItemStatusEnum.GeneratingMedia && !string.IsNullOrWhiteSpace(item.VideoJobId);
        if (!resumingVideo)
        {
            item.Status = AutomationItemStatusEnum.GeneratingText;
            item.GenerationAttemptCount++;
            item.LastError = null;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        try
        {
            var requiresImage = item.RequestedContentType is AutomationContentTypeEnum.Image or AutomationContentTypeEnum.Auto &&
                                !item.Platform.Equals("tiktok", StringComparison.OrdinalIgnoreCase);
            var requiresVideo = item.RequestedContentType == AutomationContentTypeEnum.Video ||
                                item.Platform.Equals("tiktok", StringComparison.OrdinalIgnoreCase);

            var content = item.ContentId.HasValue
                ? await _context.Contents.FirstOrDefaultAsync(value => value.Id == item.ContentId.Value, cancellationToken)
                : null;
            content ??= new Content
            {
                Id = Guid.NewGuid(),
                ProfileId = item.AutomationPlan.ProfileId,
                WorkspaceId = item.AutomationPlan.WorkspaceId,
                BrandId = item.BrandId,
                ProductId = item.ProductId,
                Title = item.Topic,
                Status = ContentStatusEnum.Draft,
                IsAiGenerated = true
            };
            content.AdType = requiresVideo ? AdTypeEnum.VideoText : requiresImage ? AdTypeEnum.ImageText : AdTypeEnum.TextOnly;
            content.ContextDescription = $"Automation plan: {item.AutomationPlan.Name}; platform: {item.Platform}";
            content.UpdatedAt = DateTime.UtcNow;
            if (item.ContentId is null) _context.Contents.Add(content);
            item.ContentId = content.Id;

            if (item.UsedCredits < 1 || string.IsNullOrWhiteSpace(content.TextContent))
            {
                var textPrompt = BuildTextPrompt(item);
                var aiGen = new AiGeneration
                {
                    Id = Guid.NewGuid(),
                    ContentId = content.Id,
                    AiPrompt = textPrompt,
                    ProviderName = "Gemini",
                    Status = AiStatusEnum.Processing,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.AiGenerations.Add(aiGen);
                await _context.SaveChangesAsync(cancellationToken);

                try
                {
                    var generatedText = await _textClient.GenerateAsync(textPrompt, cancellationToken);
                    if (string.IsNullOrWhiteSpace(generatedText)) throw new InvalidOperationException("AI returned empty content.");
                    content.TextContent = EnsureProductLandingUrlInCaption(generatedText.Trim(), item.Product, $"{item.Notes}\n{item.Cta}");
                    aiGen.GeneratedText = content.TextContent;
                    aiGen.Status = AiStatusEnum.Completed;
                    aiGen.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    aiGen.Status = AiStatusEnum.Failed;
                    aiGen.ErrorMessage = ex.Message;
                    aiGen.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                    throw;
                }

                if (item.UsedCredits < 1)
                {
                    var textCharge = await _automationCredits.SettleAsync(item.Id, profile.UserId, CreditActionEnum.GenerateText, 1, 1, cancellationToken);
                    if (!textCharge.Success) throw new InvalidOperationException(textCharge.Message ?? "Unable to charge text generation credits.");
                }
            }

            if (requiresVideo)
            {
                var video = string.IsNullOrWhiteSpace(item.VideoJobId)
                    ? await _videoProvider.StartVideoGenerationAsync(BuildVideoPrompt(item),
                        new VideoGenerationOptions { DurationSeconds = 4, AspectRatio = "9:16" }, cancellationToken)
                    : await _videoProvider.CheckStatusAsync(item.VideoJobId, cancellationToken);
                item.VideoProvider = video.ProviderName;
                if (!video.Success || video.Status == VideoGenerationStatus.Failed)
                    throw new InvalidOperationException(video.ErrorMessage ?? "Video generation failed.");
                if (video.Status is VideoGenerationStatus.Queued or VideoGenerationStatus.Processing)
                {
                    item.VideoJobId = video.JobId ?? item.VideoJobId;
                    item.Status = AutomationItemStatusEnum.GeneratingMedia;
                    item.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                    return false;
                }
                if (string.IsNullOrWhiteSpace(video.MediaUrl))
                    throw new InvalidOperationException("Video provider completed without a media URL.");
                using (var httpClient = new HttpClient())
                {
                    var videoBytes = await httpClient.GetByteArrayAsync(video.MediaUrl, cancellationToken);
                    content.VideoUrl = await _mediaStorage.UploadBytesAsync(videoBytes, "automation-videos", $"{item.Id:N}.mp4", cancellationToken);
                }
                if (item.UsedCredits < 21)
                {
                    var videoCharge = await _automationCredits.SettleAsync(item.Id, profile.UserId, CreditActionEnum.GenerateVideo, 20, 21, cancellationToken);
                    if (!videoCharge.Success) throw new InvalidOperationException(videoCharge.Message ?? "Unable to charge video generation credits.");
                }
                item.Status = AutomationItemStatusEnum.AwaitingApproval;
            }
            else if (requiresImage)
            {
                if (item.UsedCredits < 6 || string.IsNullOrWhiteSpace(content.ImageUrl))
                {
                    item.Status = AutomationItemStatusEnum.GeneratingMedia;
                    await _context.SaveChangesAsync(cancellationToken);
                    var media = await _imageProvider.GenerateImageAsync(
                        BuildImagePrompt(item),
                        new ImageGenerationOptions
                        {
                            ReferenceImageUrls = SelectProductReferenceImages(item.Product).Select(reference => reference.Url).ToList()
                        },
                        cancellationToken);
                    if (!media.Success || media.MediaBytes is null)
                        throw new InvalidOperationException(media.ErrorMessage ?? "Image generation failed.");

                    var url = await _mediaStorage.UploadBytesAsync(media.MediaBytes, "automation",
                        $"{item.Id:N}.png", cancellationToken);
                    content.ImageUrl = JsonSerializer.Serialize(new[] { url });
                    await _context.SaveChangesAsync(cancellationToken);
                    if (item.UsedCredits < 6)
                    {
                        var imageCharge = await _automationCredits.SettleAsync(item.Id, profile.UserId, CreditActionEnum.GenerateImage, 5, 6, cancellationToken);
                        if (!imageCharge.Success) throw new InvalidOperationException(imageCharge.Message ?? "Unable to charge image generation credits.");
                    }
                }
                item.Status = AutomationItemStatusEnum.AwaitingApproval;
            }
            else
            {
                item.Status = AutomationItemStatusEnum.AwaitingApproval;
            }

            item.UpdatedAt = DateTime.UtcNow;
            await RecalculatePlanAsync(item.AutomationPlan, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Automation generation failed for item {AutomationItemId}", item.Id);
            await FailAsync(item, exception.Message, cancellationToken);
        }

        return true;
    }

    private async Task FailAsync(AutomationItem item, string message, CancellationToken cancellationToken)
    {
        item.Status = AutomationItemStatusEnum.GenerationFailed;
        item.LastError = message.Length > 2000 ? message[..2000] : message;
        item.UpdatedAt = DateTime.UtcNow;
        await RecalculatePlanAsync(item.AutomationPlan, cancellationToken);
    }

    private async Task RecalculatePlanAsync(AutomationPlan plan, CancellationToken cancellationToken)
    {
        await _context.Entry(plan).ReloadAsync(cancellationToken);
        await _context.Entry(plan).Collection(value => value.Items).LoadAsync(cancellationToken);
        plan.FailedItems = plan.Items.Count(value => value.Status is AutomationItemStatusEnum.GenerationFailed or AutomationItemStatusEnum.NeedsAttention);
        var unfinished = plan.Items.Any(value => value.Status is AutomationItemStatusEnum.Pending or AutomationItemStatusEnum.GeneratingText or AutomationItemStatusEnum.GeneratingMedia or AutomationItemStatusEnum.QualityCheck);
        if (!unfinished && plan.Status != AutomationPlanStatusEnum.Cancelled)
        {
            var ready = plan.Items.Any(value => value.Status == AutomationItemStatusEnum.AwaitingApproval);
            plan.Status = ready ? AutomationPlanStatusEnum.AwaitingApproval : AutomationPlanStatusEnum.Failed;
        }
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        if (!unfinished && plan.Status != AutomationPlanStatusEnum.Cancelled) await _automationCredits.ReleaseAsync(plan.Id, cancellationToken);
    }

    private static string BuildTextPrompt(AutomationItem item) => $"""
        Create a ready-to-publish social media caption in Vietnamese.
        Platform: {item.Platform}
        Brand: {item.Brand.Name}
        Product: {item.Product?.Name ?? "Not specified"}
        Product knowledge profile:
        {BuildProductKnowledgeContext(item.Product)}
        Product landing URL: {ExtractProductSourceUrl(item.Product) ?? "Not specified"}
        Topic: {item.Topic}
        Objective: {item.Objective ?? "Not specified"}
        Tone: {item.Tone ?? "Natural and professional"}
        CTA: {item.Cta ?? "Choose a suitable call to action"}
        Notes: {item.Notes ?? "None"}
        Return only the final caption, including a concise CTA and relevant hashtags. Write the post naturally. DO NOT include structural labels like "Title:", "Caption:", or "Nội dung:". Just output the final text ready to be posted. The backend will append the exact Product landing URL after generation when one is available, so do not invent, shorten, rewrite, or add fake URLs yourself. Tuyệt đối không đưa ra các tuyên bố y tế, tài chính chưa được kiểm chứng, không sử dụng các từ ngữ cam kết tuyệt đối như '100% hiệu quả', 'chữa khỏi hoàn toàn', và không vi phạm chính sách nội dung quảng cáo. Do not explain your answer. Do not include assistant-talk such as "Được thôi", "Tôi sẽ", or "Dưới đây là".
        """;

    private static string BuildImagePrompt(AutomationItem item) => $"""
        {BuildProductImageReferenceInstruction(item.Product)}
        Create a polished social media advertising image for {item.Brand.Name}.
        Topic: {item.Topic}. Product: {item.Product?.Name ?? "brand offering"}.
        Product knowledge profile:
        {BuildProductKnowledgeContext(item.Product)}
        Tone: {item.Tone ?? "professional"}. Platform: {item.Platform}.
        If product reference image URLs are present, use all provided reference images collectively to reconstruct the same physical product. The first reference image is the primary reference and has the highest priority; supporting reference images provide complementary views and details.
        Treat all reference images as different views of one product. Do not redesign it. Do not replace it with a generic product, a different model, or a product inferred from the brand name.
        Preserve the exact silhouette, proportions, color scheme, visible parts, materials, accessories, logo or marking placement, and distinctive details from the reference product images, maintaining the exact product design, high fidelity, commercial advertising photography.
        Act as an expert commercial product photographer.
        Strict rules: NO HUMANS, NO FACES, NO HANDS, NO BODY PARTS. Do not show people, hands holding the product, faces, silhouettes, or human skin.
        Use a minimalist clean commercial background related to the product with intentional negative space for future text placement.
        Use studio lighting, 4k resolution, hyper-realistic, polished 3D render style, premium commercial product photography. Put the product as the central hero subject.
        Add only subtle decor accents such as soft blurred leaves, window light, water reflections, geometric shapes, or material textures when relevant.
        no text, no typos, clean background, no watermark, no logo text, no gibberish typography, no broken-font characters.
        Do not render readable text, fake words, broken-font characters, watermarks, logos, UI elements, or brand-name text inside the image.
        """;

    private static string BuildVideoPrompt(AutomationItem item) => $"""
        A premium advertising video showcasing {item.Product?.Name ?? item.Brand.Name} as the central hero subject.
        {BuildProductKnowledgeContext(item.Product)}
        Topic: {item.Topic}. Objective: {item.Objective ?? "engagement"}. Platform: {item.Platform}.
        The product performs a subtle and elegant motion (slowly rotates, gleams under light, or is gently handled in context).
        Camera: smooth slow dolly-in from establishing shot to close-up product detail.
        Lighting: professional studio soft-box lighting with warm highlight accent, clean minimalist background.
        Tone: {item.Tone ?? "professional, premium, social-media-ready"}.
        No text overlay, no watermark, no readable letters on screen, no people, no hands, no faces.
        Short-form vertical video optimized for {item.Platform} advertising.
        """;


    private static string BuildProductKnowledgeContext(Product? product)
    {
        if (product == null) return "Not specified";

        var parts = new List<string>();
        AddPart(parts, "Name", product.Name);
        AddPart(parts, "Category", product.Category);
        AddPart(parts, "Description", product.Description);
        AddPart(parts, "Primary use", product.PrimaryUse);
        AddPart(parts, "USP", product.Usp);
        AddPart(parts, "Target audience", product.TargetAudience);
        AddPart(parts, "Visual identity", product.VisualIdentity);
        AddPart(parts, "Knowledge profile", product.KnowledgeProfile);
        AddPart(parts, "Product landing URL", ExtractProductSourceUrl(product));
        AddPart(parts, "Reference image URLs", FormatProductImages(product.Images));

        return parts.Count == 0 ? "Not specified" : string.Join("\n", parts);
    }

    private static void AddPart(ICollection<string> parts, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add($"- {label}: {value.Trim()}");
        }
    }

    private static string? FormatProductImages(string? rawImages)
    {
        var images = ParseProductImages(rawImages);
        return images.Count == 0 ? null : string.Join(", ", images.Take(5));
    }

    private static string? ExtractProductSourceUrl(Product? product)
    {
        if (IsValidHttpUrl(product?.ProductUrl)) return product!.ProductUrl!.Trim();
        if (string.IsNullOrWhiteSpace(product?.KnowledgeProfile)) return null;

        try
        {
            using var doc = JsonDocument.Parse(product.KnowledgeProfile);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("sourceUrl", out var sourceUrl) &&
                sourceUrl.ValueKind == JsonValueKind.String)
            {
                var value = sourceUrl.GetString();
                return IsValidHttpUrl(value) ? value : null;
            }
        }
        catch (JsonException)
        {
            var match = System.Text.RegularExpressions.Regex.Match(product.KnowledgeProfile, @"https?://[^\s""'<>]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return match.Success && IsValidHttpUrl(match.Value) ? match.Value : null;
        }

        return null;
    }

    private static string EnsureProductLandingUrlInCaption(string caption, Product? product, string? instructionText = null)
    {
        if (string.IsNullOrWhiteSpace(caption)) return caption;
        var cleanedCaption = StripStructuralPostLabels(caption);
        if (ShouldSuppressProductLink(instructionText)) return cleanedCaption;

        var sourceUrl = ExtractProductSourceUrl(product);
        if (string.IsNullOrWhiteSpace(sourceUrl)) return cleanedCaption;
        if (cleanedCaption.Contains(sourceUrl, StringComparison.OrdinalIgnoreCase)) return cleanedCaption;

        return $"{cleanedCaption.Trim()}\n\n👉 Xem chi tiết và mua ngay tại: {sourceUrl}";
    }

    private static string StripStructuralPostLabels(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var cleaned = value.Trim();
        cleaned = System.Text.RegularExpressions.Regex.Replace(
            cleaned,
            @"(?im)^\s*(?:#+\s*)?(?:\*\*)?\s*(?:title|tiêu đề|tieu de)\s*:?\s*(?:\*\*)?\s*.*(?:\r?\n)?",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        cleaned = System.Text.RegularExpressions.Regex.Replace(
            cleaned,
            @"(?im)^\s*(?:#+\s*)?(?:\*\*)?\s*(?:caption|nội dung|noi dung|content)\s*:?\s*(?:\*\*)?\s*",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        cleaned = System.Text.RegularExpressions.Regex.Replace(
            cleaned,
            @"\n{3,}",
            "\n\n",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);

        return cleaned.Trim();
    }

    private static bool ShouldSuppressProductLink(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        var normalized = RemoveVietnameseDiacritics(value).ToLowerInvariant();
        var signals = new[]
        {
            "no link",
            "without link",
            "do not include link",
            "dont include link",
            "khong can chen link",
            "khong chen link",
            "khong gan link",
            "khong them link",
            "bo link"
        };

        return signals.Any(signal => normalized.Contains(signal));
    }

    private static string RemoveVietnameseDiacritics(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString()
            .Replace('đ', 'd')
            .Replace('Đ', 'd')
            .Normalize(System.Text.NormalizationForm.FormC);
    }

    private static bool IsValidHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string BuildProductImageReferenceInstruction(Product? product)
    {
        var references = SelectProductReferenceImages(product);
        if (references.Count == 0) return string.Empty;

        var referenceList = string.Join(" ", references.Select((reference, index) =>
            $"Reference image {index + 1} ({reference.Role}): {reference.Url}."));

        return $"""
        {referenceList}
        Use the reference images above as the exact product subject. Do not invent, modify, or guess image URLs.
        """;
    }

    private static List<ProductReferenceImage> SelectProductReferenceImages(Product? product)
    {
        return ParseProductImages(product?.Images)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((url, index) => new ProductReferenceImage(url, index == 0 ? "primary" : "supporting"))
            .ToList();
    }

    private sealed record ProductReferenceImage(string Url, string Role);

    private static List<string> ParseProductImages(string? rawImages)
    {
        if (string.IsNullOrWhiteSpace(rawImages)) return new List<string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(rawImages)?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToList() ?? new List<string>();
        }
        catch (System.Text.Json.JsonException)
        {
            return new List<string>();
        }
    }
}
