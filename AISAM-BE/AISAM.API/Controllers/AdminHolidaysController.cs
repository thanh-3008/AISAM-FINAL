using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/holidays")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public class AdminHolidaysController : ControllerBase
{
    private readonly AisamContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;

    public AdminHolidaysController(AisamContext context, IUserRepository userRepository, IMemoryCache cache)
    {
        _context = context;
        _userRepository = userRepository;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<IEnumerable<object>>>> GetAllHolidays(
        [FromQuery] int? year = null, 
        [FromQuery] string countryCode = "VN", 
        CancellationToken cancellationToken = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        var holidays = await _context.HolidayEvents
            .AsNoTracking()
            .Where(h => h.Year == targetYear && h.CountryCode == countryCode)
            .OrderBy(h => h.ExactDate)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.LocalName,
                h.ExactDate,
                h.Year,
                h.CountryCode,
                h.IsActive,
                h.IsManuallyOverridden
            })
            .ToListAsync(cancellationToken);

        return Ok(GenericResponse<IEnumerable<object>>.CreateSuccess(holidays));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GenericResponse<object>>> UpdateHoliday(
        Guid id, 
        [FromBody] UpdateHolidayRequest request, 
        CancellationToken cancellationToken = default)
    {
        var holiday = await _context.HolidayEvents.FindAsync(new object[] { id }, cancellationToken);
        if (holiday == null)
        {
            return NotFound(GenericResponse<object>.CreateError("Holiday not found.", HttpStatusCode.NotFound));
        }

        holiday.Name = request.Name ?? holiday.Name;
        holiday.LocalName = request.LocalName ?? holiday.LocalName;
        holiday.IsActive = request.IsActive;
        holiday.IsManuallyOverridden = true; // Mark as manually overridden if admin edits it

        await _context.SaveChangesAsync(cancellationToken);

        _cache.Remove("UpcomingHolidays_14");
        _cache.Remove("UpcomingHolidays_30");

        return Ok(GenericResponse<object>.CreateSuccess(new
        {
            holiday.Id,
            holiday.Name,
            holiday.LocalName,
            holiday.IsActive,
            holiday.IsManuallyOverridden
        }, "Holiday updated successfully."));
    }
}

public class UpdateHolidayRequest
{
    public string? Name { get; set; }
    public string? LocalName { get; set; }
    public bool IsActive { get; set; }
}
