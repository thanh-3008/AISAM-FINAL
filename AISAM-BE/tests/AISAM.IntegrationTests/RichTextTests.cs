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
}
