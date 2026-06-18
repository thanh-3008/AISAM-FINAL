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

public class ContentControllerTests
{
    [Fact]
    public async Task Create_ReturnsServiceStatusCode_WhenValidationFails()
    {
        var service = new FakeContentService
        {
            CreateResult = GenericResponse<ContentResponseDto>.CreateError("Brand not found.", HttpStatusCode.NotFound)
        };
        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.Create(new CreateContentRequest());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetPaged_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeContentService
        {
            PagedResult = GenericResponse<PagedResult<ContentResponseDto>>.CreateSuccess(new PagedResult<ContentResponseDto>())
        };
        var controller = CreateController(service, profileId);

        await controller.GetPaged();

        Assert.Equal(profileId, service.LastProfileId);
    }

    private static ContentController CreateController(IContentService service, Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = profileId;

        return new ContentController(service, new FakeProfileRepository())
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeContentService : IContentService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<ContentResponseDto> CreateResult { get; set; } = GenericResponse<ContentResponseDto>.CreateSuccess(new ContentResponseDto());
        public GenericResponse<PagedResult<ContentResponseDto>> PagedResult { get; set; } = GenericResponse<PagedResult<ContentResponseDto>>.CreateSuccess(new PagedResult<ContentResponseDto>());

        public Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(CreateResult);
        }

        public Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PagedResult);
        }

        public Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, UpdateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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
