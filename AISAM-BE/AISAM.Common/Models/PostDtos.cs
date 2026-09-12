namespace AISAM.Common.Models;

public sealed class PostDto
{
    [System.Text.Json.Serialization.JsonIgnore]
    public Func<string,IReadOnlyList<PublishMediaResult>,CancellationToken,Task>? Progress {get;set;}
    public Task ReportAsync(string status,IReadOnlyList<PublishMediaResult> media,CancellationToken ct)
        =>Progress?.Invoke(status,media,ct)??Task.CompletedTask;
    public List<PublishMediaDto>? Media {get;set;}
    public string Message { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<string>? ImageUrls { get; set; }
    public string? VideoUrl { get; set; }
    public string? LinkUrl { get; set; }
}

public sealed class PublishResultDto
{
    public List<PublishMediaResult> Media {get;set;}=[];
    public bool RequiresReconciliation {get;set;}
    public bool Success { get; set; }
    public string? ProviderPostId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? PostedAt { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? RefreshedTargetAccessToken { get; set; }
}

public sealed record PublishMediaDto(Guid Id,string Url,string MimeType,decimal? DurationSeconds=null);
public sealed record PublishMediaResult(Guid Id,string Status,string? ProviderMediaId=null,string? ErrorCode=null);
