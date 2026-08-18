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
        const string geminiResponse = "A premium skincare bottle slowly rotates on a marble surface, camera dolly-in, soft studio lighting.";
        var enhancer = CreateEnhancer(new StubGeminiClient(geminiResponse));

        var result = await enhancer.EnhanceVideoPromptAsync(
            "Tạo video quảng cáo mỹ phẩm", product: null, durationSeconds: 8);

        Assert.Equal(geminiResponse, result);
    }

    [Fact]
    public async Task EnhanceVideoPromptAsync_FallsBackToRawPrompt_WhenGeminiThrows()
    {
        const string raw = "Tạo video sản phẩm";
        var enhancer = CreateEnhancer(new StubGeminiClient(new Exception("timeout")));

        var result = await enhancer.EnhanceVideoPromptAsync(raw, product: null, durationSeconds: 8);

        Assert.Equal(raw, result);
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




