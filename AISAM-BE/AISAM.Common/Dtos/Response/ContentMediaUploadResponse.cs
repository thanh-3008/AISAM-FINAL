namespace AISAM.Common.Dtos.Response;

public sealed class ContentMediaUploadResponse
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}
