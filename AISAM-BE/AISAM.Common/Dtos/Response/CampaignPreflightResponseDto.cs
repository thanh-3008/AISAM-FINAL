namespace AISAM.Common.Dtos.Response;

public sealed class CampaignPreflightResponseDto
{
    public bool Ready { get; set; }
    public IReadOnlyList<CampaignPreflightCheckDto> Checks { get; set; } = [];
    public int Errors { get; set; }
    public int Warnings { get; set; }
}

public sealed class CampaignPreflightCheckDto
{
    public string Key { get; set; } = string.Empty;
    public string Status { get; set; } = "passed";
    public string Message { get; set; } = string.Empty;
}
