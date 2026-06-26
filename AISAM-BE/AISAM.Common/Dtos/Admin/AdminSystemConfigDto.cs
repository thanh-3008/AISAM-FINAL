namespace AISAM.Common.Dtos.Admin;

public class AdminSystemConfigDto
{
    public Dictionary<string, object> Config { get; set; } = new();
}

public class AdminUpdateSystemConfigRequest
{
    public Dictionary<string, object> Config { get; set; } = new();
}
