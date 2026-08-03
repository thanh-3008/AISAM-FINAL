using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

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
"{rawPrompt}"

Output rules:
- Output ONLY the final English prompt. No explanation, no markdown, no quotes, no extra text.
- The prompt must be a single cohesive paragraph or structured sentence block.
- Always include at the end: "No text, no watermark, no logo text, no readable letters, no humans, no faces, no hands, no body parts, no gibberish typography, no broken-font characters, commercial product photography, 4K ultra-realistic."
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

    /// <inheritdoc />
    public async Task<string> EnhanceVideoPromptAsync(
        string rawPrompt,
        Product? product,
        int durationSeconds = 8,
        string? aspectRatio = null,
        CancellationToken cancellationToken = default)
    {
        var productContext = BuildProductContext(product);
        var aspectHint = string.IsNullOrWhiteSpace(aspectRatio) ? "9:16 vertical (social media short-form)" : aspectRatio;

        var metaPrompt = $"""
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
"{rawPrompt}"

Output rules:
- Output ONLY the final English prompt. No explanation, no markdown, no quotes, no extra text.
- The prompt must describe: subject + motion + camera + lighting + mood.
- The product must be the central hero subject. Do not replace or redesign the product.
- Always include at the end: "No text overlay, no watermark, no readable letters, no watermark on screen, no people, no hands, no faces, professional social media advertising video."
""";

        try
        {
            var enhanced = await _gemini.GenerateAsync(metaPrompt, cancellationToken);
            if (!string.IsNullOrWhiteSpace(enhanced))
            {
                _logger.LogInformation("[PromptEnhancer] Video prompt enhanced. Original length={OrigLen}, Enhanced length={EhLen}",
                    rawPrompt.Length, enhanced.Trim().Length);
                return enhanced.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[PromptEnhancer] Gemini failed during video prompt enhancement. Falling back to raw prompt.");
        }

        // Fallback: return raw prompt so video generation is not blocked
        return rawPrompt;
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
}
