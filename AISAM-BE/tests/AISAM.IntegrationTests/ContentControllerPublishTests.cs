using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class ContentControllerPublishTests
{
    [Fact]
    public async Task Publish_ReturnsServiceStatusCode_WhenIntegrationBelongsToAnotherProfile()
    {
        var service = new FakeContentService
        {
            PublishResult = GenericResponse<PublishResultDto>.CreateError("Social integration not found.", HttpStatusCode.NotFound)
        };
        var controller = CreateController(service, Guid.NewGuid(), Guid.NewGuid());

        var result = await controller.Publish(Guid.NewGuid(), Guid.NewGuid());

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    [Fact]
    public async Task Publish_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeContentService
        {
            PublishResult = GenericResponse<PublishResultDto>.CreateSuccess(new PublishResultDto
            {
                Success = true,
                ProviderPostId = "facebook-post-1"
            })
        };
        var controller = CreateController(service, profileId, Guid.NewGuid());
        var contentId = Guid.NewGuid();
        var integrationId = Guid.NewGuid();

        await controller.Publish(contentId, integrationId);

        Assert.Equal(profileId, service.LastProfileId);
        Assert.Equal(contentId, service.LastPublishedContentId);
        Assert.Equal(integrationId, service.LastPublishedIntegrationId);
    }

    private static ContentController CreateController(IContentService service, Guid profileId, Guid workspaceId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;

        return new ContentController(service, new FakeProfileRepository())
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeContentService : IContentService
    {
        public Guid LastProfileId { get; private set; }
        public Guid LastPublishedContentId { get; private set; }
        public Guid LastPublishedIntegrationId { get; private set; }
        public GenericResponse<PublishResultDto> PublishResult { get; set; } = GenericResponse<PublishResultDto>.CreateSuccess(new PublishResultDto());

        public Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, UpdateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default)
        {
            LastPublishedContentId = contentId;
            LastPublishedIntegrationId = integrationId;
            LastProfileId = profileId;
            return Task.FromResult(PublishResult);
        }

        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastPublishedContentId = contentId;
            LastPublishedIntegrationId = integrationId;
            LastProfileId = profileId;
            return Task.FromResult(PublishResult);
        }

        public Task<GenericResponse<List<string>>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        public Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Profile?>(null);
        public Task<Profile?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Profile?>(null);
        public Task<IEnumerable<Profile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Profile>());
        public Task<IEnumerable<Profile>> GetByUserIdIncludingDeletedAsync(Guid userId, bool isDeleted, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Profile>());
        public Task<IEnumerable<Profile>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Profile>());
        public Task<Profile> CreateAsync(Profile profile, CancellationToken cancellationToken = default) => Task.FromResult(profile);
        public Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default) => Task.FromResult(profile);
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
