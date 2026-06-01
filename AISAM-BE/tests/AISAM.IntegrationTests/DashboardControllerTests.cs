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
    public async Task GetSummary_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeDashboardService
        {
            Result = GenericResponse<DashboardSummaryDto>.CreateSuccess(new DashboardSummaryDto())
        };
        var controller = CreateController(service, profileId);

        await controller.GetSummary();

        Assert.Equal(profileId, service.LastProfileId);
    }

    private static DashboardController CreateController(IDashboardService service, Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;

        return new DashboardController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeDashboardService : IDashboardService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<DashboardSummaryDto> Result { get; set; } = GenericResponse<DashboardSummaryDto>.CreateSuccess(new DashboardSummaryDto());

        public Task<GenericResponse<DashboardSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(Result);
        }
    }
}
