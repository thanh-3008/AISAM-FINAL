using AISAM.API.Infrastructure;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/dev")]
[Authorize]
[DevelopmentOnly]
public sealed class DevSchedulerController : ControllerBase
{
    private readonly IScheduledPostingService _scheduledPostingService;
    private readonly IWebHostEnvironment _environment;

    public Guid LastValidatedProfileId { get; private set; }

    public DevSchedulerController(
        IScheduledPostingService scheduledPostingService,
        IWebHostEnvironment environment)
    {
        _scheduledPostingService = scheduledPostingService;
        _environment = environment;
    }

    [HttpPost("scheduler/run-now")]
    public async Task<ActionResult<GenericResponse<SchedulerRunResultDto>>> RunNow(CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment())
        {
            return StatusCode(StatusCodes.Status404NotFound, GenericResponse<SchedulerRunResultDto>.CreateError("Not found.", System.Net.HttpStatusCode.NotFound));
        }

        LastValidatedProfileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
        var result = await _scheduledPostingService.RunDueSchedulesAsync(20, cancellationToken);
        return Ok(GenericResponse<SchedulerRunResultDto>.CreateSuccess(result, "Scheduler run completed successfully."));
    }
}
