using AISAM.Data.Model;
using AISAM.Services.IServices;
using AISAM.Services.Utilities;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.RegularExpressions;

namespace AISAM.Services.Service;

/// <summary>
/// Rewrites raw user prompts into optimized, product-accurate English prompts
/// for FLUX.2 [klein] (image) and LTX-2.3 (video) generation models.
/// </summary>
public sealed class PromptEnhancerService : IPromptEnhancerService
{
    private readonly IGeminiTextClient _gemini;
    private readonly ILogger<PromptEnhancerService> _logger;

    public PromptEnhancerService(IGeminiTextClient gemini, ILogger<PromptEnhancerService> logger)
    {
        _gemini = gemini;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> EnhanceImagePromptAsync(
        string rawPrompt,
        Product? product,
        bool hasReferenceImages,
        CancellationToken cancellationToken = default)
    {
        var cleanPrompt = PromptGuard.SanitizePromptInput(rawPrompt);
        if (PromptGuard.ContainsInjectionPattern(cleanPrompt))
        {
            _logger.LogWarning("[PromptEnhancer] Potential prompt injection detected in EnhanceImagePromptAsync. Sanitizing.");
            cleanPrompt = "High quality commercial product advertising photography";
        }

        var productContext = BuildProductContext(product);
        var referenceMode = hasReferenceImages
            ? "The user will supply one or more reference images of the actual product. The product identity (shape, silhouette, color palette, material, label/packaging layout, and distinctive visible details) MUST be preserved exactly. Only the background, lighting, camera angle, and scene composition may change."
            : "No reference images. Infer the product appearance from the product context provided.";

        var metaPrompt = $"""
You are an expert commercial advertising photographer and AI image-generation prompt engineer.
Your task: rewrite the user's request into a single, highly detailed English image generation prompt optimized for FLUX.2 [klein] (a rectified flow transformer model by Black Forest Labs).

FLUX.2 [klein] best practices:
- Write in descriptive English. Be specific about materials, textures, colors, surface finishes.
- Specify lighting type (e.g. soft studio diffused light, natural window light, dramatic rim highlight).
- Specify camera angle and composition (e.g. eye-level close-up, overhead flat-lay, 3/4 product angle).
- Specify background mood and context (e.g. minimalist white marble, soft cream textile, dark lifestyle scene).
- Always end with safety rules to prevent AI artifacts.

Product context (use as the SOURCE OF TRUTH — do not invent details not present here):
{productContext}

Reference image handling:
{referenceMode}

User's original request (may be in any language):
"{cleanPrompt}"

Output rules:
- Output ONLY the final English prompt. No explanation, no markdown, no quotes, no extra text.
- The prompt must be a single cohesive paragraph or structured sentence block.
- CRITICAL RULE: NEVER include any text, typography, letters, branding, names, prices, or watermarks in the generated image. The final image MUST BE COMPLETELY TEXT-FREE, even if the product name or brand is provided in the context.
- Always include at the end of the prompt: "commercial advertising photography, high fidelity, 4k ultra-realistic, completely text-free, no readable text, no typography, no watermark, no letters, no words, no numbers, no humans, no faces, no hands."
- If reference images exist: include "Preserve exact product identity: shape, silhouette, proportions, color scheme, material, and label layout from the reference image. Do not redesign or replace the product."
""";

        try
        {
            var enhanced = await _gemini.GenerateAsync(metaPrompt, cancellationToken);
            if (!string.IsNullOrWhiteSpace(enhanced))
            {
                _logger.LogInformation("[PromptEnhancer] Image prompt enhanced. Original length={OrigLen}, Enhanced length={EhLen}",
                    rawPrompt.Length, enhanced.Trim().Length);
                return enhanced.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[PromptEnhancer] Gemini failed during image prompt enhancement. Falling back to raw prompt.");
        }

        // Fallback: raw prompt continues, other safety suffixes are added downstream by CleanPromptForImageEdit
        return rawPrompt;
    }

    private static string? _videoContextCache;
    private static readonly object _contextLock = new object();

    private static string GetVideoContext()
    {
        if (_videoContextCache != null) return _videoContextCache;
        lock (_contextLock)
        {
            if (_videoContextCache != null) return _videoContextCache;
            
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "VideoContext.md");
            if (File.Exists(path))
            {
                _videoContextCache = File.ReadAllText(path);
            }
            else
            {
                _videoContextCache = string.Empty;
            }
            return _videoContextCache;
        }
    }

    /// <inheritdoc />
    public async Task<(string Prompt, string? PatternId)> EnhanceVideoPromptAsync(
        string rawPrompt,
        Product? product,
        int durationSeconds = 8,
        string? aspectRatio = null,
        List<string>? recentlyUsedPrompts = null,
        string? referenceImageUrl = null,
        CancellationToken cancellationToken = default)
    {
        var videoContext = GetVideoContext();
        var cleanPrompt = PromptGuard.SanitizePromptInput(rawPrompt);
        if (PromptGuard.ContainsInjectionPattern(cleanPrompt))
        {
            _logger.LogWarning("[PromptEnhancer] Potential prompt injection detected in EnhanceVideoPromptAsync. Sanitizing.");
            cleanPrompt = "Cinematic social media product advertisement video";
        }

        string metaPrompt;
        
        if (!string.IsNullOrWhiteSpace(videoContext))
        {
            var usedPatternsStr = recentlyUsedPrompts != null && recentlyUsedPrompts.Any() 
                ? string.Join(", ", recentlyUsedPrompts) 
                : "none";

            metaPrompt = $@"{videoContext}

Product name: {product?.Name ?? "No product name"}
Product description: {product?.Description ?? "No description available"}
Target audience: {product?.TargetAudience ?? "General audience"}
Recently used patterns to avoid repeating: {usedPatternsStr}

User's original request (incorporate into the script if possible):
""{rawPrompt}""
";
        }
        else
        {
            var productContext = BuildProductContext(product);
            var aspectHint = string.IsNullOrWhiteSpace(aspectRatio) ? "9:16 vertical (social media short-form)" : aspectRatio;

            metaPrompt = $"""
You are an expert advertising creative director and AI video generation prompt engineer.
Your task: rewrite the user's request into a single, highly detailed English video generation prompt optimized for LTX-2.3 (a DiT-based audio-video foundation model by Lightricks).

LTX-2.3 best practices (from official Lightricks Prompting Guide):
- Structure: "[Subject] [action], camera [movement], [lighting], [mood/aesthetic], [technical specs]"
- Be explicit about camera movement (e.g. "slow dolly-in", "gentle arc around product", "static close-up with subtle zoom").
- Describe the visual motion of the subject (e.g. "product slowly rotates", "liquid pours in slow motion", "steam rises gently").
- Specify lighting dynamism (e.g. "soft diffused studio light", "warm golden-hour backlight", "cool product photography lighting").
- Specify mood and aesthetic fitting for social media advertising.
- Keep pacing calm and premium for product ads unless the brand calls for energetic.
- Do NOT include text, logos, or human faces unless explicitly required.

Product context (use as the SOURCE OF TRUTH — do not invent details not present here):
{productContext}

Video parameters:
- Duration: {durationSeconds} seconds
- Aspect ratio: {aspectHint}

User's original request (may be in any language):
"{cleanPrompt}"

Output rules:
- Output ONLY the final English prompt. MANDATORY RULE: The prompt text given to the video model MUST be written completely in English. Do not output Vietnamese or any other language except inside the `<d>` dialogue tags if absolutely necessary.
- The prompt must describe: subject + motion + camera + lighting + mood.
- The pattern selected is only a reference/inspiration. The video script MUST revolve primarily around the product itself as the central hero subject. Do not let the pattern overshadow the product. Do not replace or redesign the product.
- If the user explicitly requests text or typography, you MAY include text rendering instructions (e.g. "bold text reading 'SALE'"). Otherwise, ALWAYS include at the end: "No text overlay, no watermark, no readable letters, no watermark on screen, no people, no hands, no faces, professional social media advertising video."
""";
        }

        byte[]? imageBytes = await DownloadImageBytesAsync(referenceImageUrl, cancellationToken);
        if (imageBytes != null)
        {
            metaPrompt = "[SYSTEM_NOTE: The user has attached a reference image. Preserving the exact visual identity of the product in the reference image is a HARD RULE. Ground your visual descriptions entirely on this image.]\n\n" + metaPrompt;
        }

        try
        {
            string enhanced;
            if (imageBytes != null)
            {
                enhanced = await _gemini.GenerateWithVisionAsync(metaPrompt, imageBytes, "image/jpeg", "application/json", cancellationToken);
            }
            else
            {
                enhanced = await _gemini.GenerateAsync(metaPrompt, "application/json", cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(enhanced))
            {
                var (parsedPrompt, patternId) = ExtractVisualPromptFromT2va(enhanced);
                var safePrompt = EnforceVideoSafety(parsedPrompt, product?.Name);
                _logger.LogInformation("[PromptEnhancer] Video prompt enhanced. Original length={OrigLen}, Enhanced length={EhLen}, Final length={FinalLen}, Pattern={PatternId}, Img2Video={HasImg}",
                    rawPrompt.Length, enhanced.Trim().Length, safePrompt.Length, patternId, imageBytes != null);
                return (safePrompt, patternId);
            }
        }
        catch (InvalidOperationException ioe) when (ioe.Message.Contains("non-ASCII"))
        {
            _logger.LogWarning("[PromptEnhancer][NonAscii] PromptEnhancer output contained non-ASCII. Falling back. Source=T2VA");
            // Fall through to rawPrompt fallback
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PromptEnhancer] Gemini failed during video prompt enhancement. Falling back to safe prompt.");
        }

        // Fallback: Check if the raw prompt is safe.
        try
        {
            var fallbackSafe = EnforceVideoSafety(cleanPrompt, product?.Name);
            return (fallbackSafe, null);
        }
        catch (InvalidOperationException ioe) when (ioe.Message.Contains("non-ASCII"))
        {
            _logger.LogWarning("[PromptEnhancer][NonAscii] rawPrompt from Chat Orchestrator contained non-ASCII. Using DefaultSafeEnglish. RawPromptLength={Len}", rawPrompt?.Length ?? 0);
            var productName = string.IsNullOrWhiteSpace(product?.Name) ? "the product" : product.Name;
            // Untranslated descriptions must not reintroduce the rejected language into the fallback.
            var productDesc = string.IsNullOrWhiteSpace(product?.Description) || HasNonAsciiLetters(product.Description)
                ? "" : $" ({product.Description})";
            var defaultSafeEnglish = $"A high-quality commercial advertising video showcasing {productName}{productDesc} in a professional setting, cinematic lighting, 8k resolution, ultra-realistic, no text overlay, no watermark, no hands, no faces.";
            return (defaultSafeEnglish, null);
        }
    }

    public async Task<string> EnhanceVideoPromptWithScriptAsync(
        string rawPrompt,
        string? videoScript,
        Product? product,
        int durationSeconds = 9,
        string? aspectRatio = "9:16",
        string? referenceImageUrl = null,
        CancellationToken cancellationToken = default)
    {
        var cleanPrompt = PromptGuard.SanitizePromptInput(rawPrompt);
        if (PromptGuard.ContainsInjectionPattern(cleanPrompt))
        {
            _logger.LogWarning("[PromptEnhancer] Potential prompt injection detected in EnhanceVideoPromptWithScriptAsync. Sanitizing.");
            cleanPrompt = "Cinematic commercial product advertisement video";
        }

        if (string.IsNullOrWhiteSpace(cleanPrompt) && string.IsNullOrWhiteSpace(videoScript))
        {
            return cleanPrompt;
        }

        var productContext = BuildProductContext(product);
        var aspectHint = string.IsNullOrWhiteSpace(aspectRatio) ? "9:16 (vertical mobile short-form video)" : aspectRatio;

        var metaPrompt = $"""
You are an expert commercial advertising director and prompt engineer specializing in AI video generation models (LTX-2.3, Veo, Kling, Runway Gen-3).
Your task is to transform the provided user request and video script into a single, high-performing video generation prompt in English using the strict 6-Layer Advertising Formula:

### 6-Layer Advertising Prompt Formula:
1. **Subject (Chủ thể)**: What is the central hero object? (Product appearance, colors, texture, packaging). Must be ultra-detailed.
2. **Action/Motion (Hành động/Chuyển động)**: What happens? Describe liquid pouring, light reflections shifting, product rotating slowly, lid opening, or subtle background movement. Avoid static scenes.
3. **Context/Setting (Bối cảnh)**: Where is the product? (Minimalist studio, wet marble surface, luxury gradient backdrop, nature setting).
4. **Lighting (Ánh sáng)**: Soft studio rim light, cinematic chiaroscuro, golden hour sidelight, neon glow, or clean softbox lighting.
5. **Camera & Lens (Góc máy/Lens)**: Macro lens, 85mm portrait lens, shallow depth of field, slow dolly-in, orbit shot, tracking camera.
6. **Style & Quality (Phong cách & Chất lượng)**: 4K cinematic advertising commercial, photorealistic, Unreal Engine 5 render aesthetic, elegant color grading, aspirational tone.

Product context (use as SOURCE OF TRUTH — preserve exact product visual details and brand identity):
{productContext}

Storyboard/Video Script (if available):
{(string.IsNullOrWhiteSpace(videoScript) ? "None provided — synthesize a cohesive 3-scene advertising flow (Hook -> Feature Demonstration -> Hero CTA shot)." : videoScript)}

Video Parameters:
- Duration: {durationSeconds} seconds
- Aspect Ratio: {aspectHint}

Original User Prompt / Direction:
"{cleanPrompt}"

Output Rules:
- Output ONLY the final, continuous English prompt paragraph combining all 6 layers. No explanation, no markdown fences, no layer labels.
- MANDATORY RULE: The final generated prompt MUST be strictly in English.
- The video script and storyboard MUST revolve primarily around the product itself. Treat any pattern or script purely as an inspirational reference.
- If a multi-scene script is provided, weave the transitions naturally: "[Scene 1 description], seamlessly transitioning to [Scene 2 description], and concluding with [Scene 3 hero composition]".
- If the user explicitly requests text or typography, include it clearly. Otherwise, always append at the very end: "commercial advertising photography, high fidelity, 4k cinematic, no text overlay, no watermark, no readable letters, no ugly morphing, no extra hands, professional social media video ad."
""";

        byte[]? imageBytes = await DownloadImageBytesAsync(referenceImageUrl, cancellationToken);
        if (imageBytes != null)
        {
            metaPrompt = "[SYSTEM_NOTE: The user has attached a reference image. Preserving the exact visual identity of the product in the reference image is a HARD RULE. Ground your visual descriptions entirely on this image.]\n\n" + metaPrompt;
        }

        try
        {
            string enhanced;
            if (imageBytes != null)
            {
                enhanced = await _gemini.GenerateWithVisionAsync(metaPrompt, imageBytes, "image/jpeg", "text/plain", cancellationToken);
            }
            else
            {
                enhanced = await _gemini.GenerateAsync(metaPrompt, "text/plain", cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(enhanced))
            {
                _logger.LogInformation("[PromptEnhancer] Video script prompt enhanced with 6-layer formula. Original length={OrigLen}, Enhanced length={EhLen}, Img2Video={HasImg}",
                    (rawPrompt + videoScript).Length, enhanced.Trim().Length, imageBytes != null);
                return enhanced.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[PromptEnhancer] Gemini failed during 6-layer video prompt enhancement. Falling back to raw prompt.");
        }

        return !string.IsNullOrWhiteSpace(rawPrompt) ? rawPrompt : videoScript ?? string.Empty;
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────

    private static string BuildProductContext(Product? product)
    {
        if (product == null) return "No product selected.";

        var parts = new List<string>();
        AddPart(parts, "Name", product.Name);
        AddPart(parts, "Category", product.Category);
        AddPart(parts, "Description", product.Description);
        AddPart(parts, "Primary use", product.PrimaryUse);
        AddPart(parts, "Unique selling proposition (USP)", product.Usp);
        AddPart(parts, "Target audience", product.TargetAudience);
        AddPart(parts, "Visual identity & style notes", product.VisualIdentity);
        AddPart(parts, "Knowledge profile", product.KnowledgeProfile);

        return parts.Count > 0 ? string.Join("\n", parts) : "No product details available.";
    }

    private static void AddPart(List<string> parts, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            parts.Add($"- {label}: {value}");
    }

    private static (string VisualText, string PatternId) ExtractVisualPromptFromT2va(string enhanced)
    {
        if (string.IsNullOrWhiteSpace(enhanced)) throw new FormatException("Gemini output is empty.");

        // 1. Parse JSON from Gemini (it might be wrapped in ```json ... ```)
        var jsonSpan = enhanced;
        var startIndex = jsonSpan.IndexOf("{");
        var endIndex = jsonSpan.LastIndexOf("}");
        if (startIndex >= 0 && endIndex > startIndex)
        {
            jsonSpan = jsonSpan.Substring(startIndex, endIndex - startIndex + 1);
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(jsonSpan);
            var root = doc.RootElement;
            if (root.TryGetProperty("integrated_multimodal_description", out var descEl) &&
                root.TryGetProperty("pattern_id", out var patternEl))
            {
                var visualText = descEl.GetString() ?? string.Empty;
                var patternId = patternEl.GetString() ?? string.Empty;
                
                // Strip <d>...</d> (dialogue) just in case
                visualText = Regex.Replace(visualText, @"<d>.*?</d>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                // Strip text in quotes to avoid LTX-2.3 hallucinating on-screen text
                visualText = Regex.Replace(visualText, @"""[^""]*""", ""); 
                
                return (visualText.Trim(), patternId);
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Parse JSON failed
        }
        
        throw new FormatException("Gemini did not return the expected JSON format or missing integrated_multimodal_description/pattern_id.");
    }

    private static bool HasNonAsciiLetters(string text)
        => text.Any(c => c > 127 && char.IsLetter(c));

    private static string EnforceVideoSafety(string prompt, string? productName = null)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return string.Empty;

        // Preserve the known product name and Unicode punctuation, but reject untranslated
        // letters elsewhere even in short prompts. This is a conservative fallback guard,
        // not a general-purpose language detector.
        var textToValidate = string.IsNullOrWhiteSpace(productName)
            ? prompt : prompt.Replace(productName, "", StringComparison.OrdinalIgnoreCase);
        if (HasNonAsciiLetters(textToValidate))
        {
            throw new InvalidOperationException("Prompt contains non-ASCII letters outside the product name. Rejecting to fallback.");
        }

        var safePrompt = prompt;

        // Optionally, we could still force "no faces, no hands" here if needed, 
        // but since we allow text now, we should not blindly append "no text overlay".
        string[] requiredClauses = { "no faces", "no hands" };
        var missing = requiredClauses.Where(c => !safePrompt.Contains(c, StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (missing.Any())
        {
            safePrompt = safePrompt.TrimEnd('.', ',', ' ') + ", " + string.Join(", ", missing);
        }

        return safePrompt.Trim();
    }

    private async Task<byte[]?> DownloadImageBytesAsync(string? url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            return await client.GetByteArrayAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[PromptEnhancer] Failed to download reference image bytes from {Url}. Will silently fallback to text-only generation.", url);
            return null;
        }
    }
}
