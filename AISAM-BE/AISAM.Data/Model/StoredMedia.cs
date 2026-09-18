namespace AISAM.Data.Model;

public sealed record StoredMedia(string Url, int? Width = null, int? Height = null, decimal? DurationSeconds = null, string? PublicId = null, long? SizeBytes = null);
