using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class AIControllerTests
{
    [Fact]
    public async Task GenerateDraft_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var service = new FakeAIService();
        var controller = CreateController(service, profileId, workspaceId, userId);

        await controller.GenerateDraft(new CreateDraftRequest());

        Assert.Equal(profileId, service.LastProfileId);
        Assert.Equal(workspaceId, service.LastWorkspaceId);
        Assert.Equal(userId, service.LastUserId);
    }

    [Fact]
    public async Task Chat_ReturnsServiceStatusCode_WhenGeminiIsUnavailable()
    {
        var service = new FakeAIService
        {
            ChatResult = GenericResponse<ChatResponse>.CreateError("AI chat is temporarily unavailable.", HttpStatusCode.ServiceUnavailable)
        };
        var controller = CreateController(service, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await controller.Chat(new ChatRequest { Message = "Hello" });

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, objectResult.StatusCode);
    }

    private static GeminiController CreateController(IAIService service, Guid profileId, Guid workspaceId, Guid userId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = new AISAM.Data.Model.WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = userId
        };

        return new GeminiController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeAIService : IAIService
    {
        public Guid LastProfileId { get; private set; }
        public Guid LastWorkspaceId { get; private set; }
        public Guid LastUserId { get; private set; }
        public GenericResponse<ChatResponse> ChatResult { get; set; } = GenericResponse<ChatResponse>.CreateSuccess(new ChatResponse());

        public Task<GenericResponse<AiGenerationResponse>> GenerateDraftAsync(Guid profileId, Guid workspaceId, Guid userId, CreateDraftRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            LastWorkspaceId = workspaceId;
            LastUserId = userId;
            return Task.FromResult(GenericResponse<AiGenerationResponse>.CreateSuccess(new AiGenerationResponse()));
        }

        public Task<GenericResponse<AiGenerationResponse>> ImproveAsync(Guid contentId, Guid profileId, Guid workspaceId, Guid userId, ImproveContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> ApproveAsync(Guid generationId, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<IEnumerable<AiGenerationResponse>>> GetGenerationsAsync(Guid contentId, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<GenericResponse<ChatResponse>> ChatAsync(Guid profileId, ChatRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(ChatResult);
        }

        public Task<GenericResponse<ChatResponse>> ChatInWorkspaceAsync(Guid profileId, Guid workspaceId, ChatRequest request, CancellationToken cancellationToken = default) => ChatAsync(profileId, request, cancellationToken);
        public Task<GenericResponse<ContentResponseDto>> ApproveInWorkspaceAsync(Guid generationId, Guid workspaceId, CancellationToken cancellationToken = default) => ApproveAsync(generationId, workspaceId, cancellationToken);
        public Task<GenericResponse<IEnumerable<AiGenerationResponse>>> GetGenerationsInWorkspaceAsync(Guid contentId, Guid workspaceId, CancellationToken cancellationToken = default) => GetGenerationsAsync(contentId, workspaceId, cancellationToken);
    }
}
