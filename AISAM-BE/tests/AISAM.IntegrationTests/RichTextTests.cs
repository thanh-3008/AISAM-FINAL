using AISAM.Data;
using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AISAM.IntegrationTests;

public class RichTextTests
{
    private const string Document = """
        {"type":"doc","content":[{"type":"heading","attrs":{"level":2},"content":[{"type":"text","text":"Xin chào 👩‍💻 #AISAM","marks":[{"type":"bold"},{"type":"highlight"}]}]},{"type":"orderedList","attrs":{"start":3},"content":[{"type":"listItem","content":[{"type":"paragraph","content":[{"type":"text","text":"Mua","marks":[{"type":"link","attrs":{"href":"https://example.test/p"}}]}]}]}]}]}
        """;

    [Fact]
    public void FormatterPreservesLinksListsAndUnicodeWithoutHtml()
    {
        var plain = RichTextDocument.PlainText(Document, 1);
        Assert.Equal("Xin chào 👩‍💻 #AISAM\n\n3. Mua (https://example.test/p)", plain);
        Assert.All(RichTextDocument.FormatCaptions(plain).Values, caption => Assert.Equal(plain, caption));
    }

    [Fact]
    public void FacebookCaptionUsesUnicodeForSupportedRichTextMarks()
    {
        const string document = """
            {"type":"doc","content":[{"type":"paragraph","content":[{"type":"text","text":"Bold 12","marks":[{"type":"bold"}]},{"type":"text","text":" Việt","marks":[{"type":"italic"}]},{"type":"text","text":" Both","marks":[{"type":"bold"},{"type":"italic"}]},{"type":"text","text":" Ấn","marks":[{"type":"underline"}]}]}]}
            """;
        var plain = RichTextDocument.PlainText(document, 1);
        var captions = RichTextDocument.FormatCaptions(plain, document, 1);

        Assert.Equal("𝗕𝗼𝗹𝗱 𝟭𝟮 𝘝𝘪ệ𝘵 𝘽𝙤𝙩𝙝 Ấ̲n̲", captions["facebook"]);
        Assert.Equal(plain, captions["instagram"]);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,test")]
    [InlineData("//evil.test")]
    public void UnsafeLinksAreRejected(string url) => Assert.Throws<ArgumentException>(() =>
        RichTextDocument.PlainText(Document.Replace("https://example.test/p", url), 1));

    [Theory]
    [InlineData("{\"type\":\"script\"}")]
    [InlineData("{\"type\":\"doc\",\"content\":42}")]
    [InlineData("{\"type\":\"doc\",\"attrs\":{\"onclick\":\"bad\"}}")]
    public void UnknownSchemaFailsClosed(string json) => Assert.Throws<ArgumentException>(() => RichTextDocument.PlainText(json, 1));

    [Fact]
    public async Task SaveDerivesTextAndFormattingEditInvalidatesApprovalButNotFrozenSnapshot()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var content = new Content { TextContent = "untrusted client text", RichTextJson = Document, RichTextVersion = 1 };
        db.Add(content); await db.SaveChangesAsync();
        Assert.Equal(RichTextDocument.PlainText(Document, 1), content.TextContent);
        content.Status = ContentStatusEnum.Approved; await db.SaveChangesAsync();
        var snapshot = await db.PublishSnapshots.SingleAsync(s => s.Id == content.ApprovedSnapshotId);
        var payload = JsonSerializer.Deserialize<Content>(snapshot.Payload)!;
        Assert.Equal(Document, payload.RichTextJson);
        Assert.Equal(content.TextContent, payload.FormattedCaptions!["instagram"]);
        var oldVersion = content.MediaVersion;
        content.RichTextJson = Document.Replace("bold", "italic"); await db.SaveChangesAsync();
        Assert.Equal(ContentStatusEnum.Draft, content.Status);
        Assert.NotEqual(oldVersion, content.MediaVersion);
        Assert.Null(content.ApprovedSnapshotId);
        Assert.Equal(Document, JsonSerializer.Deserialize<Content>(snapshot.Payload)!.RichTextJson);
        content.TextContent = "legacy writer replacement <literal> **text**"; await db.SaveChangesAsync();
        Assert.Null(content.RichTextJson);
        Assert.Equal("legacy writer replacement <literal> **text**", content.TextContent);
    }

    [Fact]
    public void MultiMarksOnTextPassValidationAndRenderCorrectly()
    {
        const string multiMarkDoc = """
            {"type":"doc","content":[{"type":"paragraph","content":[{"type":"text","text":"Multi-mark test","marks":[{"type":"bold"},{"type":"italic"},{"type":"underline"},{"type":"highlight"}]}]}]}
            """;
        var plain = RichTextDocument.PlainText(multiMarkDoc, 1);
        Assert.Equal("Multi-mark test", plain);
    }

    [Fact]
    public void HardBreakWithMarksPassesValidationAndRendersAsNewline()
    {
        const string docWithMarkedBreak = """
            {"type":"doc","content":[{"type":"paragraph","content":[{"type":"text","text":"Line 1","marks":[{"type":"bold"},{"type":"italic"},{"type":"underline"}]},{"type":"hardBreak","marks":[{"type":"bold"},{"type":"italic"},{"type":"underline"}]},{"type":"text","text":"Line 2"}]}]}
            """;
        var plain = RichTextDocument.PlainText(docWithMarkedBreak, 1);
        Assert.Equal("Line 1\nLine 2", plain);
    }

    [Fact]
    public void TrueBlockNodesWithMarksStillFailValidation()
    {
        const string invalidBlockDoc = """
            {"type":"doc","content":[{"type":"paragraph","marks":[{"type":"bold"}],"content":[{"type":"text","text":"Bad block"}]}]}
            """;
        var ex = Assert.Throws<ArgumentException>(() => RichTextDocument.PlainText(invalidBlockDoc, 1));
        Assert.Equal("RICH_TEXT_INVALID_BLOCK", ex.Message);
    }

    [Fact]
    public async Task SaveWithMultiMarkAndMarkedHardBreakSucceeds()
    {
        const string fullDoc = """
            {"type":"doc","content":[{"type":"paragraph","content":[{"type":"text","text":"• **ĐỈNH CAO MIZUNO**","marks":[{"type":"bold"},{"type":"italic"},{"type":"underline"}]},{"type":"hardBreak","marks":[{"type":"bold"},{"type":"italic"},{"type":"underline"}]},{"type":"text","text":"👉 Xem chi tiết","marks":[{"type":"link","attrs":{"href":"https://example.com"}}]}]}]}
            """;
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var content = new Content { TextContent = "untrusted", RichTextJson = fullDoc, RichTextVersion = 1 };
        db.Add(content);
        await db.SaveChangesAsync();

        Assert.Equal("• **ĐỈNH CAO MIZUNO**\n👉 Xem chi tiết (https://example.com)", content.TextContent);
        Assert.Equal(fullDoc, content.RichTextJson);
    }
}

