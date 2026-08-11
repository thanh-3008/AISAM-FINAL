using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace AISAM.IntegrationTests;

public class DevSchedulerControllerTests
{
    [Fact]
    public async Task RunNow_UsesValidatedActiveProfileAndReturnsOk_InDevelopment()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeScheduledPostingService
        {
            Result = new SchedulerRunResultDto
            {
                ScannedCount = 1,
                SuccessCount = 1,
                FailedCount = 0
            }
        };
        var controller = CreateController(service, profileId, true);

        var result = await controller.RunNow();

        Assert.Equal(profileId, controller.LastValidatedProfileId);
        var ok = Assert.IsAssignableFrom<OkObjectResult>(result.Result);
        var response = Assert.IsType<GenericResponse<SchedulerRunResultDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data!.SuccessCount);
    }

    [Fact]
    public async Task RunNow_ReturnsNotFound_WhenEnvironmentIsNotDevelopment()
    {
        var controller = CreateController(new FakeScheduledPostingService(), Guid.NewGuid(), false);

        var result = await controller.RunNow();

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    private static DevSchedulerController CreateController(IScheduledPostingService service, Guid profileId, bool isDevelopment)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;

        return new DevSchedulerController(service, new FakeWebHostEnvironment(isDevelopment))
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeScheduledPostingService : IScheduledPostingService
    {
        public SchedulerRunResultDto Result { get; set; } = new();

        public Task<SchedulerRunResultDto> RunDueSchedulesAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(bool isDevelopment)
        {
            EnvironmentName = isDevelopment ? "Development" : "Production";
        }

        public string ApplicationName { get; set; } = "AISAM.API";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; }
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}




