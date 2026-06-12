using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class ContentSchedulesControllerTests
{
    [Fact]
    public async Task Create_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeContentScheduleService
        {
            CreateResult = GenericResponse<ContentScheduleDto>.CreateSuccess(new ContentScheduleDto())
        };
        var controller = CreateController(service, profileId, Guid.NewGuid());

        await controller.Create(new CreateContentScheduleRequest
        {
            ContentId = Guid.NewGuid(),
            IntegrationId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow.AddHours(1)
        });

        Assert.Equal(profileId, service.LastProfileId);
    }

    [Fact]
    public async Task Create_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var service = new FakeContentScheduleService();
        var controller = CreateController(service, profileId, workspaceId);

        await controller.Create(new CreateContentScheduleRequest
        {
            ContentId = Guid.NewGuid(),
            IntegrationId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow.AddHours(1)
        });

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    [Fact]
    public async Task Update_ReturnsServiceStatusCode_WhenScheduleCannotBeUpdated()
    {
        var service = new FakeContentScheduleService
        {
            UpdateResult = GenericResponse<ContentScheduleDto>.CreateError(
                "Completed schedules cannot be updated.",
                HttpStatusCode.BadRequest)
        };
        var controller = CreateController(service, Guid.NewGuid(), Guid.NewGuid());

        var result = await controller.Update(Guid.NewGuid(), new UpdateContentScheduleRequest
        {
            ScheduledAt = DateTime.UtcNow.AddHours(2)
        });

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
    }

    private static ContentSchedulesController CreateController(IContentScheduleService service, Guid profileId, Guid workspaceId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;

        return new ContentSchedulesController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeContentScheduleService : IContentScheduleService
    {
        public Guid LastProfileId { get; private set; }
        public Guid LastWorkspaceId { get; private set; }
        public GenericResponse<ContentScheduleDto> CreateResult { get; set; } = GenericResponse<ContentScheduleDto>.CreateSuccess(new ContentScheduleDto());
        public GenericResponse<PagedResult<ContentScheduleDto>> PagedResult { get; set; } = GenericResponse<PagedResult<ContentScheduleDto>>.CreateSuccess(new PagedResult<ContentScheduleDto>());
        public GenericResponse<ContentScheduleDto> DetailResult { get; set; } = GenericResponse<ContentScheduleDto>.CreateSuccess(new ContentScheduleDto());
        public GenericResponse<ContentScheduleDto> UpdateResult { get; set; } = GenericResponse<ContentScheduleDto>.CreateSuccess(new ContentScheduleDto());
        public GenericResponse<bool> DeleteResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<IReadOnlyList<ContentScheduleDto>> UpcomingResult { get; set; } = GenericResponse<IReadOnlyList<ContentScheduleDto>>.CreateSuccess(Array.Empty<ContentScheduleDto>());

        public Task<GenericResponse<ContentScheduleDto>> CreateAsync(Guid profileId, Guid workspaceId, CreateContentScheduleRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            LastWorkspaceId = workspaceId;
            return Task.FromResult(CreateResult);
        }

        public Task<GenericResponse<PagedResult<ContentScheduleDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PagedResult);
        }

        public Task<GenericResponse<ContentScheduleDto>> GetByIdAsync(Guid profileId, Guid scheduleId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(DetailResult);
        }

        public Task<GenericResponse<ContentScheduleDto>> UpdateAsync(Guid profileId, Guid workspaceId, Guid scheduleId, UpdateContentScheduleRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            LastWorkspaceId = workspaceId;
            return Task.FromResult(UpdateResult);
        }

        public Task<GenericResponse<bool>> DeleteAsync(Guid profileId, Guid workspaceId, Guid scheduleId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            LastWorkspaceId = workspaceId;
            return Task.FromResult(DeleteResult);
        }

        public Task<GenericResponse<IReadOnlyList<ContentScheduleDto>>> GetUpcomingAsync(Guid profileId, int limit, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(UpcomingResult);
        }
    }
}
