using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class ConversationControllerTests
{
    [Fact]
    public async Task GetPaged_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeConversationService();
        var controller = CreateController(service, profileId);

        await controller.GetPaged();

        Assert.Equal(profileId, service.LastProfileId);
    }

    [Fact]
    public async Task GetById_ReturnsServiceStatusCode_WhenConversationBelongsToAnotherProfile()
    {
        var service = new FakeConversationService
        {
            DetailResult = GenericResponse<ConversationDetailDto>.CreateError("Conversation not found.", HttpStatusCode.NotFound)
        };
        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.GetById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    private static ConversationController CreateController(IConversationService service, Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = profileId;

        return new ConversationController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeConversationService : IConversationService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<ConversationDetailDto> DetailResult { get; set; } = GenericResponse<ConversationDetailDto>.CreateSuccess(new ConversationDetailDto());

        public Task<GenericResponse<PagedResult<ConversationResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(GenericResponse<PagedResult<ConversationResponseDto>>.CreateSuccess(new PagedResult<ConversationResponseDto>()));
        }

        public Task<GenericResponse<ConversationDetailDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(DetailResult);
        }

        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}




