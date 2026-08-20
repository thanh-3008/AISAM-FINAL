using AISAM.Data.Model;

namespace AISAM.Services.IServices;

public interface IHolidaySource
{
    Task<IEnumerable<HolidayEvent>> GetHolidaysAsync(int year, string countryCode, CancellationToken cancellationToken = default);
}
