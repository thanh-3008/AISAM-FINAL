using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class QuotaControllerTests
{
    [Fact]
    public async Task GetProfileQuota_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeQuotaService
        {
            SummaryResult = GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto())
        };
        var controller = CreateController(service, profileId);

        await controller.GetProfileQuota(profileId);

        Assert.Equal(profileId, service.LastProfileId);
    }

    [Fact]
    public async Task GetProfileQuota_ReturnsNotFound_WhenRouteProfileDiffersFromActiveProfile()
    {
        var controller = CreateController(new FakeQuotaService(), Guid.NewGuid());

        var result = await controller.GetProfileQuota(Guid.NewGuid());

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    private static QuotaController CreateController(IQuotaService service, Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;

        return new QuotaController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeQuotaService : IQuotaService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<QuotaSummaryDto> SummaryResult { get; set; } = GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto());
        public GenericResponse<bool> PromptResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<bool> PostResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);

        public Task<GenericResponse<QuotaSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(SummaryResult);
        }

        public Task<GenericResponse<bool>> EnsurePromptQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PromptResult);
        }

        public Task<GenericResponse<bool>> EnsurePostQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PostResult);
        }
    }
}
