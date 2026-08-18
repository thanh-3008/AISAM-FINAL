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
    public async Task GetCurrentWorkspaceQuota_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakeQuotaService
        {
            SummaryResult = GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto())
        };
        var controller = CreateController(service, workspaceId);

        await controller.GetCurrentWorkspaceQuota();

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    private static QuotaController CreateController(IQuotaService service, Guid workspaceId)
    {
        var context = new DefaultHttpContext();
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;

        return new QuotaController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeQuotaService : IQuotaService
    {
        public Guid LastProfileId { get; private set; }
        public Guid LastWorkspaceId { get; private set; }
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

        public Task<GenericResponse<QuotaSummaryDto>> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(SummaryResult);
        }

        public Task<GenericResponse<bool>> EnsureWorkspacePostQuotaAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(PostResult);
        }
    }
}




