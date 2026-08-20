namespace AISAM.Common.Dtos;

public class HolidayEventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LocalName { get; set; }
    public DateTime ExactDate { get; set; }
    public int Year { get; set; }
    public string CountryCode { get; set; } = string.Empty;
}
