using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.IntegrationTests;

public class DashboardControllerTests
{
    [Fact]
    public async Task GetSummary_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakeDashboardService
        {
            Result = GenericResponse<DashboardSummaryDto>.CreateSuccess(new DashboardSummaryDto())
        };
        var controller = CreateController(service, workspaceId);

        await controller.GetSummary();

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    private static DashboardController CreateController(IDashboardService service, Guid workspaceId)
    {
        var context = new DefaultHttpContext();
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;

        return new DashboardController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeDashboardService : IDashboardService
    {
        public Guid LastProfileId { get; private set; }
        public Guid LastWorkspaceId { get; private set; }
        public GenericResponse<DashboardSummaryDto> Result { get; set; } = GenericResponse<DashboardSummaryDto>.CreateSuccess(new DashboardSummaryDto());

        public Task<GenericResponse<DashboardSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(Result);
        }

        public Task<GenericResponse<DashboardSummaryDto>> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(Result);
        }
    }
}
