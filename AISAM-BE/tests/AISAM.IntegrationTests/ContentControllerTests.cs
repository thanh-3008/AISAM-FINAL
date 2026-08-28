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
using System.Reflection;

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
            PagedResult = GenericResponse<PagedResult<ContentListDto>>.CreateSuccess(new PagedResult<ContentListDto>())
        };
        var controller = CreateController(service, profileId);

        await controller.GetPaged();

        Assert.Equal(profileId, service.LastProfileId);
    }

    [Fact]
    public void UploadMedia_UsesEndpointScopedMultipartAndTransportLimits()
    {
        var method = typeof(ContentController).GetMethod(nameof(ContentController.UploadMedia));
        var requestSizeLimit = method?.GetCustomAttribute<RequestSizeLimitAttribute>();
        var requestSizeLimitMetadata = method?.CustomAttributes.Single(attribute => attribute.AttributeType == typeof(RequestSizeLimitAttribute));
        var formLimit = method?.GetCustomAttribute<RequestFormLimitsAttribute>();

        Assert.NotNull(requestSizeLimit);
        Assert.NotNull(requestSizeLimitMetadata);
        Assert.NotNull(formLimit);
        Assert.Equal(55L * 1024 * 1024, (long)requestSizeLimitMetadata!.ConstructorArguments.Single().Value!);
        Assert.Equal(55L * 1024 * 1024, formLimit!.MultipartBodyLengthLimit);
    }

    [Fact]
    public async Task UploadMedia_RejectsFileLargerThan50Mb_WithApplicationError()
    {
        var controller = CreateController(new FakeContentService(), Guid.NewGuid(), new FakeMediaStorageService());
        var oversizedFile = CreateFormFile("oversized.mp4", "video/mp4", 50L * 1024 * 1024 + 1);

        var result = await controller.UploadMedia(new ContentMediaUploadRequest { File = oversizedFile });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<GenericResponse<ContentMediaUploadResponse>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal("Media file must be 50MB or smaller.", response.Error?.ErrorMessage);
    }

    [Fact]
    public async Task UploadMedia_AcceptsSupportedVideo_AndPreservesOriginalFileNameInResponse()
    {
        var storage = new FakeMediaStorageService();
        var controller = CreateController(new FakeContentService(), Guid.NewGuid(), storage);
        var file = CreateFormFile("product-demo.mp4", "video/mp4", 1024);

        var result = await controller.UploadMedia(new ContentMediaUploadRequest { File = file });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GenericResponse<ContentMediaUploadResponse>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal("product-demo.mp4", response.Data?.FileName);
        Assert.Equal("https://media.test/uploaded.mp4", response.Data?.Url);
        Assert.Equal("video/mp4", storage.UploadedContentType);
    }

    private static FormFile CreateFormFile(string fileName, string contentType, long length)
    {
        var file = new FormFile(Stream.Null, 0, length, "File", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
        return file;
    }

    private static ContentController CreateController(IContentService service, Guid profileId, IMediaStorageService? mediaStorage = null)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = profileId;

        return new ContentController(service, new FakeProfileRepository(), mediaStorage)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeMediaStorageService : IMediaStorageService
    {
        public string? UploadedContentType { get; private set; }

        public Task<string> UploadAsync(IFormFile file, string folder, string fileName, CancellationToken cancellationToken = default)
        {
            UploadedContentType = file.ContentType;
            return Task.FromResult("https://media.test/uploaded.mp4");
        }

        public Task<string> UploadBytesAsync(byte[] data, string folder, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult("https://media.test/uploaded.mp4");
    }

    private sealed class FakeContentService : IContentService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<ContentResponseDto> CreateResult { get; set; } = GenericResponse<ContentResponseDto>.CreateSuccess(new ContentResponseDto());
        public GenericResponse<PagedResult<ContentListDto>> PagedResult { get; set; } = GenericResponse<PagedResult<ContentListDto>>.CreateSuccess(new PagedResult<ContentListDto>());

        public Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(CreateResult);
        }

        public Task<GenericResponse<PagedResult<ContentListDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
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
        public Task<GenericResponse<bool>> SoftDeleteInWorkspaceAsync(Guid id, Guid workspaceId, WorkspaceMemberRoleEnum memberRole, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<List<string>>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SubmitForApprovalAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> ApproveAsync(Guid id, Guid workspaceId, Guid approverUserId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> RejectAsync(Guid id, Guid workspaceId, Guid approverUserId, string? notes, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        public Task<Profile?> GetBasicByIdAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<IEnumerable<Profile>> GetBasicByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => GetByUserIdAsync(userId, cancellationToken);
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





