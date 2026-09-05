using System.Text.RegularExpressions;

namespace AISAM.IntegrationTests;

public sealed class CredentialLoggingSecurityTests
{
    private static readonly Regex OutputStatement = new(
        @"(?:Log(?:Trace|Debug|Information|Warning|Error|Critical)\s*\(|Console\.Write(?:Line)?\s*\(|File\.(?:AppendAllText|WriteAllText)\s*\()[\s\S]*?;",
        RegexOptions.Compiled);

    [Fact]
    public void Cloudinary_OutputStatements_DoNotReferenceCredentialValues()
    {
        var source = ReadServiceSource("CloudinaryMediaStorageService.cs");

        AssertOutputStatementsExclude(source, ", apiKey", ", apiSecret", ", cloudName", "uploadResult.Error.Message");
        Assert.DoesNotContain("fallback: '{Value}'", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Facebook_OutputStatements_DoNotReferenceCredentialsOrRawProviderPayloads()
    {
        var source = ReadServiceSource("FacebookProvider.cs") + ReadServiceSource("PostInsightsSyncService.cs");

        AssertOutputStatementsExclude(
            source,
            "userAccessToken",
            "accessToken",
            "debugContent",
            "bodyStr",
            "ResponseBody",
            "RawResponse",
            "TokenPrefix",
            "Signature={Signature}",
            "Body={Body}",
            "{Content}");
        Assert.DoesNotContain("TOKEN DEBUG", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.AppendAllText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Response: {truncated}", source, StringComparison.Ordinal);
    }

    [Fact]
    public void PayOS_OutputStatements_DoNotReferenceCredentialsSignaturesOrRawProviderPayloads()
    {
        var source = ReadServiceSource("PayOSPaymentService.cs");

        AssertOutputStatementsExclude(
            source,
            "_settings.ApiKey",
            "_settings.ChecksumKey",
            "responseBody",
            "errorBody",
            "signature",
            "returnUrl",
            "cancelUrl",
            "Reference={Reference}",
            "Message={Message}");
        Assert.DoesNotContain("ResponseBody", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RawResponse", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Signature={Signature}", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Gemini_OutputStatements_DoNotReferenceCredentialsPromptsOrGeneratedText()
    {
        var serviceDirectory = GetServiceDirectory();
        var files = Directory.EnumerateFiles(serviceDirectory, "*Gemini*.cs")
            .Concat(Directory.EnumerateFiles(serviceDirectory, "OpenRouterTextClient.cs"));
        var source = string.Join(Environment.NewLine, files.Select(File.ReadAllText));

        AssertOutputStatementsExclude(
            source,
            "_settings.ApiKey",
            "_settings.FallbackApiKey",
            "_settings.FallbackApiKey2",
            "_settings.FallbackApiKey3",
            "_settings.FallbackApiKey4",
            ", prompt",
            ", textPrompt",
            "errorBody",
            "responseBody",
            "generatedText");
    }

    [Fact]
    public void Supabase_OutputStatements_DoNotReferenceCredentialOrTokenValues()
    {
        var backendDirectory = Directory.GetParent(GetServiceDirectory())!.Parent!.FullName;
        var files = Directory.EnumerateFiles(backendDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
        var supabaseSource = string.Join(
            Environment.NewLine,
            files.Select(File.ReadAllText).Where(text => text.Contains("Supabase", StringComparison.OrdinalIgnoreCase)));

        AssertOutputStatementsExclude(supabaseSource, "ApiKey", "Secret", "AccessToken", "RefreshToken", "Credential");
    }

    [Fact]
    public void AIService_UsesStructuredLoggingWithoutRawConsoleOrProviderDetails()
    {
        var source = ReadServiceSource("AIService.cs");

        Assert.DoesNotContain("Console.Write", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Credit deduction result:", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Task canceled (Timeout or Client Disconnect):", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception: {errorMessage}", source, StringComparison.Ordinal);
        Assert.Contains(
            "AI chat operation. Operation={Operation}",
            source,
            StringComparison.Ordinal);
        AssertOutputStatementsExclude(source, ".ErrorMessage", "imageUrl");
        Assert.DoesNotContain("_logger.LogError(ex,", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_logger.LogWarning(ex,", source, StringComparison.Ordinal);
    }

    private static void AssertOutputStatementsExclude(string source, params string[] forbiddenValues)
    {
        var outputStatements = string.Join(Environment.NewLine, OutputStatement.Matches(source).Select(match => match.Value));
        foreach (var forbiddenValue in forbiddenValues)
        {
            Assert.DoesNotContain(forbiddenValue, outputStatements, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string ReadServiceSource(string fileName)
        => File.ReadAllText(Path.Combine(GetServiceDirectory(), fileName));

    private static string GetServiceDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "AISAM.Services", "Service");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate AISAM.Services/Service for source security checks.");
    }
}
