using AISAM.Data.Model;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISAM.IntegrationTests;

public class PromptEnhancerTests
{
    // ─── Image prompt enhancement ─────────────────────────────────────────────

    [Fact]
    public async Task EnhanceImagePromptAsync_ReturnsEnhancedEnglishPrompt_WhenGeminiSucceeds()
    {
        const string geminiResponse = "A professional studio shot of a sleek aluminum water bottle on a marble surface, soft diffused lighting, no text, no watermark.";
        var enhancer = CreateEnhancer(new StubGeminiClient(geminiResponse));

        var result = await enhancer.EnhanceImagePromptAsync(
            "Tạo ảnh quảng cáo bình nước", product: null, hasReferenceImages: false);

        Assert.Equal(geminiResponse, result);
    }

    [Fact]
    public async Task EnhanceImagePromptAsync_FallsBackToRawPrompt_WhenGeminiThrows()
    {
        const string raw = "Vẽ ảnh sản phẩm";
        var enhancer = CreateEnhancer(new StubGeminiClient(new Exception("API key missing")));

        var result = await enhancer.EnhanceImagePromptAsync(raw, product: null, hasReferenceImages: false);

        Assert.Equal(raw, result);
    }

    [Fact]
    public async Task EnhanceImagePromptAsync_FallsBackToRawPrompt_WhenGeminiReturnsEmpty()
    {
        const string raw = "Create a product image";
        var enhancer = CreateEnhancer(new StubGeminiClient(string.Empty));

        var result = await enhancer.EnhanceImagePromptAsync(raw, product: null, hasReferenceImages: false);

        Assert.Equal(raw, result);
    }

    [Fact]
    public async Task EnhanceImagePromptAsync_IncludesProductContextInMetaPrompt()
    {
        var capture = new CapturingGeminiClient("Enhanced prompt result.");
        var enhancer = CreateEnhancer(capture);

        var product = new Product
        {
            Name = "Sữa rửa mặt Hana",
            Category = "Skincare",
            Usp = "Không cồn, dịu nhẹ cho da nhạy cảm",
            TargetAudience = "Phụ nữ 18-35 tuổi",
            VisualIdentity = "Màu trắng pastel, bao bì tối giản"
        };

        await enhancer.EnhanceImagePromptAsync("Tạo ảnh quảng cáo", product, hasReferenceImages: false);

        Assert.Contains("Sữa rửa mặt Hana", capture.LastPrompt);
        Assert.Contains("Skincare", capture.LastPrompt);
        Assert.Contains("Không cồn", capture.LastPrompt);
    }

    [Fact]
    public async Task EnhanceImagePromptAsync_IncludesReferenceImageInstructions_WhenHasReferenceImages()
    {
        var capture = new CapturingGeminiClient("Enhanced.");
        var enhancer = CreateEnhancer(capture);

        await enhancer.EnhanceImagePromptAsync("Tạo ảnh sản phẩm", product: null, hasReferenceImages: true);

        Assert.Contains("reference image", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Preserve", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnhanceImagePromptAsync_MetaPrompt_ContainsSafetyInstructions()
    {
        var capture = new CapturingGeminiClient("Enhanced.");
        var enhancer = CreateEnhancer(capture);

        await enhancer.EnhanceImagePromptAsync("Draw product", product: null, hasReferenceImages: false);

        Assert.Contains("no humans", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no watermark", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FLUX.2", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
    }

    // ─── Video prompt enhancement ─────────────────────────────────────────────

    [Fact]
    public async Task EnhanceVideoPromptAsync_ReturnsEnhancedEnglishPrompt_WhenGeminiSucceeds()
    {
        const string visualText = "A premium skincare bottle slowly rotates on a marble surface, camera dolly-in, soft studio lighting.";
        const string geminiResponse = "{\"pattern_id\": \"test_pattern\", \"integrated_multimodal_description\": \"" + visualText + "\"}";
        var enhancer = CreateEnhancer(new StubGeminiClient(geminiResponse));

        var result = await enhancer.EnhanceVideoPromptAsync(
            "Tạo video quảng cáo mỹ phẩm", product: null, durationSeconds: 8);

        Assert.Equal(visualText.TrimEnd('.') + ", no faces, no hands", result.Prompt);
        Assert.Equal("test_pattern", result.PatternId);
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_FallsBackToRawPrompt_WhenGeminiThrows()
    {
        const string raw = "Create a product video"; // Must be English for fallback safety
        var enhancer = CreateEnhancer(new StubGeminiClient(new Exception("timeout")));

        var result = await enhancer.EnhanceVideoPromptAsync(raw, product: null, durationSeconds: 8);

        Assert.Contains(raw, result.Prompt);
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_FallsBackToDefaultSafeEnglish_WhenRawPromptIsVietnamese()
    {
        // Arrange: Gemini T2VA throws (simulate failure), and rawPrompt is Vietnamese
        var vietnameseRawPrompt = "Quảng cáo bình giữ nhiệt Rạng Đông cao cấp";
        var enhancer = CreateEnhancer(new StubGeminiClient(new Exception("Gemini unavailable")));

        // Act
        var result = await enhancer.EnhanceVideoPromptAsync(
            vietnameseRawPrompt, product: null, durationSeconds: 8);

        // Assert: phải fallback về DefaultSafeEnglish, không được trả tiếng Việt
        Assert.DoesNotContain("ă", result.Prompt);
        Assert.DoesNotContain("ơ", result.Prompt);
        Assert.DoesNotContain("ệ", result.Prompt);
        Assert.Null(result.PatternId);
        // Verify prompt là ASCII thuần
        Assert.True(result.Prompt.All(c => c <= 127), "Fallback prompt must be pure ASCII.");
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_FallsBackToDefaultSafeEnglish_WhenT2vaReturnsVietnamese()
    {
        // Arrange: Gemini T2VA trả JSON với integrated_multimodal_description chứa tiếng Việt
        var t2vaOutput = """{"pattern_id":"macro_detail","integrated_multimodal_description":"Bình giữ nhiệt được quay cận cảnh, ánh sáng mềm, nền trắng"}""";
        var enhancer = CreateEnhancer(new StubGeminiClient(t2vaOutput));

        // Act
        var result = await enhancer.EnhanceVideoPromptAsync(
            "Quảng cáo bình giữ nhiệt", product: null, durationSeconds: 8);

        // Assert: EnforceVideoSafety reject T2VA output → fallback
        Assert.True(result.Prompt.All(c => c <= 127), "Final prompt must be ASCII even when T2VA returns Vietnamese.");
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_IncludesProductContextInMetaPrompt()
    {
        var capture = new CapturingGeminiClient("Result.");
        var enhancer = CreateEnhancer(capture);

        var product = new Product
        {
            Name = "Nước hoa Versace",
            Usp = "Hương thơm sang trọng, lâu phai",
            TargetAudience = "Nam giới 25-45 tuổi"
        };

        await enhancer.EnhanceVideoPromptAsync("Quảng cáo nước hoa", product, durationSeconds: 10, aspectRatio: "9:16");

        Assert.Contains("Nước hoa Versace", capture.LastPrompt);
        Assert.Contains("10", capture.LastPrompt);
        Assert.Contains("9:16", capture.LastPrompt);
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_PreservesRequestedTypography()
    {
        const string visual = "A bottle rotates with bold text reading 'SALE'";
        var response = System.Text.Json.JsonSerializer.Serialize(new
        {
            pattern_id = "typography",
            integrated_multimodal_description = visual
        });
        var enhancer = CreateEnhancer(new StubGeminiClient(response));

        var result = await enhancer.EnhanceVideoPromptAsync("Add bold SALE typography", null);

        Assert.Contains("text reading 'SALE'", result.Prompt);
        Assert.DoesNotContain("no text overlay", result.Prompt);
        Assert.Equal("typography", result.PatternId);
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_PreservesKnownProductNameAndUnicodePunctuation()
    {
        const string visual = "The Rạng Đông bottle rotates — soft studio lighting";
        var response = System.Text.Json.JsonSerializer.Serialize(new
        {
            pattern_id = "product",
            integrated_multimodal_description = visual
        });
        var enhancer = CreateEnhancer(new StubGeminiClient(response));

        var result = await enhancer.EnhanceVideoPromptAsync("Advertise the bottle", new Product { Name = "Rạng Đông" });

        Assert.StartsWith(visual, result.Prompt);
        Assert.Equal("product", result.PatternId);
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_DefaultFallbackOmitsUntranslatedDescription()
    {
        var enhancer = CreateEnhancer(new StubGeminiClient(new Exception("Gemini unavailable")));
        var product = new Product { Name = "Rạng Đông", Description = "Bình giữ nhiệt cao cấp" };

        var result = await enhancer.EnhanceVideoPromptAsync("Quảng cáo bình giữ nhiệt", product);

        Assert.StartsWith("A high-quality commercial advertising video", result.Prompt);
        Assert.Contains(product.Name, result.Prompt);
        Assert.DoesNotContain(product.Description, result.Prompt);
        Assert.Null(result.PatternId);
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_MetaPrompt_ContainsLtxGuidelines()
    {
        var capture = new CapturingGeminiClient("Result.");
        var enhancer = CreateEnhancer(capture);

        await enhancer.EnhanceVideoPromptAsync("Create video", product: null, durationSeconds: 8);

        Assert.True(
            capture.LastPrompt.Contains("LTX-2.3", StringComparison.OrdinalIgnoreCase) ||
            capture.LastPrompt.Contains("Pattern Library", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("camera", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnhanceVideoPromptWithScriptAsync_Includes6LayerFormulaAndScript()
    {
        var capture = new CapturingGeminiClient("Enhanced 6-layer prompt.");
        var enhancer = CreateEnhancer(capture);

        var script = "[{\"scene\":1,\"time\":\"00:00-00:03\",\"action\":\"Close-up macro shot\"}]";
        var res = await enhancer.EnhanceVideoPromptWithScriptAsync("Quảng cáo giày thể thao", script, product: null, durationSeconds: 9, aspectRatio: "9:16");

        Assert.Equal("Enhanced 6-layer prompt.", res);
        Assert.Contains("6-Layer Advertising Prompt Formula", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Subject (Chủ thể)", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Action/Motion (Hành động/Chuyển động)", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Close-up macro shot", capture.LastPrompt, StringComparison.OrdinalIgnoreCase);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static PromptEnhancerService CreateEnhancer(IGeminiTextClient gemini)
        => new(gemini, NullLogger<PromptEnhancerService>.Instance);

    private sealed class StubGeminiClient : IGeminiTextClient
    {
        private readonly string? _response;
        private readonly Exception? _ex;
        public StubGeminiClient(string response) => _response = response;
        public StubGeminiClient(Exception ex) => _ex = ex;
        public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
            => _ex != null ? Task.FromException<string>(_ex) : Task.FromResult(_response!);
        public Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
            => GenerateAsync(textPrompt, cancellationToken);
    }

    private sealed class CapturingGeminiClient : IGeminiTextClient
    {
        private readonly string _response;
        public string LastPrompt { get; private set; } = string.Empty;
        public CapturingGeminiClient(string response) => _response = response;
        public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            LastPrompt = prompt;
            return Task.FromResult(_response);
        }
        public Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
            => GenerateAsync(textPrompt, cancellationToken);
    }
}




