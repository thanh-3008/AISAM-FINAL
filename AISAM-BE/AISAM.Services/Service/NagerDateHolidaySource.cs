using System.Net.Http.Json;
using AISAM.Data.Model;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public class NagerDateHolidaySource : IHolidaySource
{
    private readonly HttpClient _httpClient;

    public NagerDateHolidaySource(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://date.nager.at/");
    }

    public async Task<IEnumerable<HolidayEvent>> GetHolidaysAsync(int year, string countryCode, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v3/PublicHolidays/{year}/{countryCode}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var holidays = await response.Content.ReadFromJsonAsync<List<NagerHolidayDto>>(cancellationToken: cancellationToken);
        if (holidays == null) return Enumerable.Empty<HolidayEvent>();

        var fetchedHolidays = holidays.Select(h => new HolidayEvent
        {
            Name = h.Name,
            LocalName = h.LocalName,
            ExactDate = DateTime.SpecifyKind(h.Date, DateTimeKind.Utc),
            Year = year,
            CountryCode = countryCode,
            IsActive = true,
            IsManuallyOverridden = false
        }).ToList();

        // Inject Lunar Marketing Holidays (2026-2030) for Vietnam
        if (countryCode == "VN")
        {
            var customHolidays = GetLunarMarketingHolidays(year);
            foreach (var ch in customHolidays)
            {
                if (!fetchedHolidays.Any(h => h.ExactDate.Date == ch.ExactDate.Date))
                {
                    fetchedHolidays.Add(ch);
                }
            }
        }

        return fetchedHolidays;
    }

    private IEnumerable<HolidayEvent> GetLunarMarketingHolidays(int year)
    {
        var dict = new Dictionary<int, List<(DateTime Date, string LocalName, string Name)>>
        {
            { 2026, new List<(DateTime, string, string)> { (new DateTime(2026, 9, 25), "Tết Trung Thu", "Mid-Autumn Festival"), (new DateTime(2026, 2, 26), "Ngày vía Thần Tài", "God of Wealth Day") } },
            { 2027, new List<(DateTime, string, string)> { (new DateTime(2027, 9, 15), "Tết Trung Thu", "Mid-Autumn Festival"), (new DateTime(2027, 2, 15), "Ngày vía Thần Tài", "God of Wealth Day") } },
            { 2028, new List<(DateTime, string, string)> { (new DateTime(2028, 10, 3), "Tết Trung Thu", "Mid-Autumn Festival"), (new DateTime(2028, 2, 4), "Ngày vía Thần Tài", "God of Wealth Day") } },
            { 2029, new List<(DateTime, string, string)> { (new DateTime(2029, 9, 22), "Tết Trung Thu", "Mid-Autumn Festival"), (new DateTime(2029, 2, 22), "Ngày vía Thần Tài", "God of Wealth Day") } },
            { 2030, new List<(DateTime, string, string)> { (new DateTime(2030, 9, 12), "Tết Trung Thu", "Mid-Autumn Festival"), (new DateTime(2030, 2, 12), "Ngày vía Thần Tài", "God of Wealth Day") } }
        };

        if (dict.TryGetValue(year, out var holidays))
        {
            return holidays.Select(h => new HolidayEvent
            {
                Name = h.Name,
                LocalName = h.LocalName,
                ExactDate = DateTime.SpecifyKind(h.Date, DateTimeKind.Utc),
                Year = year,
                CountryCode = "VN",
                IsActive = true,
                IsManuallyOverridden = true // Mark as manually overridden since it's injected
            });
        }

        return Enumerable.Empty<HolidayEvent>();
    }

    private class NagerHolidayDto
    {
        public DateTime Date { get; set; }
        public string LocalName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
    }
}
