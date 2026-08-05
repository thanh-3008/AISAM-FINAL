using AISAM.Data.Model;

namespace AISAM.Services.IServices;

/// <summary>
/// Rewrites raw user prompts (any language) into optimized English prompts
/// tailored to the target model's requirements and the product's identity.
/// </summary>
public interface IPromptEnhancerService
{
    /// <summary>
    /// Rewrites a raw image prompt into an optimized English prompt for FLUX.2 [klein].
    /// Incorporates product context, photography style, and safety rules.
    /// Falls back to the raw prompt if Gemini is unavailable.
    /// </summary>
    Task<string> EnhanceImagePromptAsync(
        string rawPrompt,
        Product? product,
        bool hasReferenceImages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rewrites a raw video prompt into an optimized English prompt for LTX-2.3.
    /// Incorporates product context, motion/camera direction, and LTX-2.3 prompting style.
    /// Falls back to the raw prompt if Gemini is unavailable.
    /// </summary>
    Task<string> EnhanceVideoPromptAsync(
        string rawPrompt,
        Product? product,
        int durationSeconds = 8,
        string? aspectRatio = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enhances a video generation prompt utilizing a 6-layer advertising formula (Subject -> Action -> Context -> Style -> Technical -> Quality)
    /// and incorporates multi-scene storyboard directions when available.
    /// </summary>
    Task<string> EnhanceVideoPromptWithScriptAsync(
        string rawPrompt,
        string? videoScript,
        Product? product,
        int durationSeconds = 9,
        string? aspectRatio = "9:16",
        CancellationToken cancellationToken = default);
}
