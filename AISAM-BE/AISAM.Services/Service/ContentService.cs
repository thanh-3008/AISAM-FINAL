using AISAM.Common;
using AISAM.Common.Messages;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Service;

public sealed class ContentService : IContentService
{
    private enum ApprovalNotificationEvent
    {
        None,
        Submitted,
        Approved,
        Rejected
    }

    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _publishLocks = new();

    private readonly IContentRepository _contentRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISocialIntegrationRepository _socialIntegrationRepository;
    private readonly ISocialAccountRepository _socialAccountRepository;
    private readonly IPostRepository _postRepository;
    private readonly Dictionary<string, IProviderService> _providers;
    private readonly ISocialTokenProtector _tokenProtector;
    private readonly IQuotaService _quotaService;
    private readonly IContentCalendarRepository _contentCalendarRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly INotificationRepository? _notificationRepository;
    private readonly AISAM.Services.Access.IAccessControlService? _access;
    private readonly AISAM.Repositories.AisamContext? _context;
    private readonly InstagramSettings _instagram;
    private readonly PublishProgressContext? _progress;

    public ContentService(
        IContentRepository contentRepository,
        IBrandRepository brandRepository,
        IProductRepository productRepository,
        ISocialIntegrationRepository socialIntegrationRepository,
        ISocialAccountRepository socialAccountRepository,
        IPostRepository postRepository,
        IEnumerable<IProviderService> providers,
        ISocialTokenProtector tokenProtector,
        IQuotaService quotaService,
        IContentCalendarRepository contentCalendarRepository,
        IWorkspaceRepository workspaceRepository,
        INotificationRepository? notificationRepository = null,
        AISAM.Services.Access.IAccessControlService? access = null,
        AISAM.Repositories.AisamContext? context = null,
        Microsoft.Extensions.Options.IOptions<InstagramSettings>? instagram=null,
        PublishProgressContext? progress=null)
    {
        _contentRepository = contentRepository;
        _brandRepository = brandRepository;
        _productRepository = productRepository;
        _socialIntegrationRepository = socialIntegrationRepository;
        _socialAccountRepository = socialAccountRepository;
        _postRepository = postRepository;
        _providers = providers.ToDictionary(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase);
        _tokenProtector = tokenProtector;
        _quotaService = quotaService;
        _contentCalendarRepository = contentCalendarRepository;
        _workspaceRepository = workspaceRepository;
        _notificationRepository = notificationRepository;
        _access=access;
        _context=context;
        _instagram=instagram?.Value??new();
        _progress=progress;
    }

    public async Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default)
    {
        var statusValidation = ValidateCreateStatus(request.Status);
        if (!statusValidation.Success)
        {
            return GenericResponse<ContentResponseDto>.CreateError(statusValidation.Message!, (HttpStatusCode)statusValidation.StatusCode);
        }

        var validation = await ValidateBrandAndProductAsync(profileId, request.BrandId, request.ProductId, cancellationToken);
        if (!validation.Success)
        {
            return GenericResponse<ContentResponseDto>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
        }

        var content = new Content
        {
            ProfileId = profileId,
            BrandId = request.BrandId,
            ProductId = request.ProductId,
            TeamId = request.TeamId,
            AdType = request.AdType,
            Title = request.Title,
            TextContent = request.RichTextJson is null ? request.TextContent : AISAM.Data.RichTextDocument.PlainText(request.RichTextJson, request.RichTextVersion), RichTextJson = request.RichTextJson, RichTextVersion = request.RichTextVersion,
            ImageUrl = FormatImageUrlForJsonb(request.ImageUrl),
            VideoUrl = request.VideoUrl,
            StyleDescription = request.StyleDescription,
            ContextDescription = request.ContextDescription,
            RepresentativeCharacter = request.RepresentativeCharacter,
            Status = request.Status ?? ContentStatusEnum.Draft,
            IsAiGenerated = request.IsAiGenerated,
            Tags = request.Tags is { Count: > 0 } ? JsonSerializer.Serialize(request.Tags) : null
        };

        await _contentRepository.AddAsync(content, cancellationToken);
        if (content.Status == ContentStatusEnum.PendingApproval)
        {
            await CreateApprovalNotificationAsync(content, ApprovalNotificationEvent.Submitted, null, cancellationToken);
        }
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), MessageConstants.Content.CreatedSuccess);
    }

    public async Task<GenericResponse<ContentResponseDto>> CreateInWorkspaceAsync(Guid workspaceId, Guid profileId, Guid actorUserId, CreateContentRequest request, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty) return GenericResponse<ContentResponseDto>.CreateError("Authenticated creator is required.", HttpStatusCode.Unauthorized);
        var statusValidation = ValidateCreateStatus(request.Status);
        if (!statusValidation.Success) return GenericResponse<ContentResponseDto>.CreateError(statusValidation.Message!, (HttpStatusCode)statusValidation.StatusCode);

        // Validate image count
        if (request.ImageUrls is { Count: > 5 })
            return GenericResponse<ContentResponseDto>.CreateError("Maximum 5 images allowed per post.", HttpStatusCode.BadRequest);

        var validation = await ValidateBrandAndProductInWorkspaceAsync(workspaceId, request.BrandId, request.ProductId, cancellationToken);
        if (!validation.Success) return GenericResponse<ContentResponseDto>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
        var content = new Content
        {
            PrimaryCreatorId = actorUserId,
            WorkspaceId = workspaceId, ProfileId = profileId, BrandId = request.BrandId, ProductId = request.ProductId,
            TeamId = request.TeamId,
            AdType = request.AdType, Title = request.Title, TextContent = request.RichTextJson is null ? request.TextContent : AISAM.Data.RichTextDocument.PlainText(request.RichTextJson, request.RichTextVersion), RichTextJson = request.RichTextJson, RichTextVersion = request.RichTextVersion,
            ImageUrl = ResolveImageUrlForStorage(request.ImageUrls, request.ImageUrl), VideoUrl = request.VideoUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            StyleDescription = request.StyleDescription, ContextDescription = request.ContextDescription,
            RepresentativeCharacter = request.RepresentativeCharacter, Status = request.Status ?? ContentStatusEnum.Draft,
            IsAiGenerated = request.IsAiGenerated,
            Tags = request.Tags is { Count: > 0 } ? JsonSerializer.Serialize(request.Tags) : null
        };
        await _contentRepository.AddAsync(content, cancellationToken);
        if (content.Status == ContentStatusEnum.PendingApproval)
        {
            await CreateApprovalNotificationAsync(content, ApprovalNotificationEvent.Submitted, null, cancellationToken);
        }
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), MessageConstants.Content.CreatedSuccess);
    }

    public async Task<GenericResponse<List<string>>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var tags = await _contentRepository.GetDistinctTagsByWorkspaceAsync(workspaceId, cancellationToken);
        return GenericResponse<List<string>>.CreateSuccess(tags);
    }

    public async Task<GenericResponse<PagedResult<ContentListDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        if (brandId.HasValue)
        {
            var validation = await ValidateBrandAndProductInWorkspaceAsync(workspaceId, brandId.Value, null, cancellationToken);
            if (!validation.Success) return GenericResponse<PagedResult<ContentListDto>>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
        }
        var result = await _contentRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, brandId, adType, includeDeleted, status, cancellationToken);
        return GenericResponse<PagedResult<ContentListDto>>.CreateSuccess(result, MessageConstants.Content.ListRetrievedSuccess);
    }

    public async Task<GenericResponse<ContentResponseDto>> GetByIdInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        return content == null || content.WorkspaceId != workspaceId ? NotFound() : GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), MessageConstants.Content.RetrievedSuccess);
    }

    public async Task<GenericResponse<ContentResponseDto>> UpdateInWorkspaceAsync(Guid id, Guid workspaceId, UpdateContentRequest request, WorkspaceMemberRoleEnum role, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.WorkspaceId != workspaceId) return NotFound();
        var validation = await ValidateBrandAndProductInWorkspaceAsync(workspaceId, content.BrandId, request.ProductId, cancellationToken);
        if (!validation.Success) return GenericResponse<ContentResponseDto>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);

        // Validate image count
        if (request.ImageUrls is { Count: > 5 })
            return GenericResponse<ContentResponseDto>.CreateError("Maximum 5 images allowed per post.", HttpStatusCode.BadRequest);

        if (request.ProductId.HasValue) content.ProductId = request.ProductId;
        if (request.AdType.HasValue) content.AdType = request.AdType.Value;
        if (request.Title != null) content.Title = request.Title;
        if (request.RichTextJson != null) { content.RichTextJson = request.RichTextJson; content.RichTextVersion = request.RichTextVersion; content.TextContent = AISAM.Data.RichTextDocument.PlainText(request.RichTextJson, request.RichTextVersion); } else if (request.TextContent != null) { content.TextContent = request.TextContent; content.RichTextJson = null; content.RichTextVersion = null; }
        // Multi-image update: prefer ImageUrls over legacy ImageUrl
        if (request.ImageUrls != null || request.ImageUrl != null)
            content.ImageUrl = ResolveImageUrlForStorage(request.ImageUrls, request.ImageUrl);
        if (request.VideoUrl != null) content.VideoUrl = request.VideoUrl;
        if (request.StyleDescription != null) content.StyleDescription = request.StyleDescription;
        if (request.ContextDescription != null) content.ContextDescription = request.ContextDescription;
        if (request.RepresentativeCharacter != null) content.RepresentativeCharacter = request.RepresentativeCharacter;
        if (request.Tags != null) content.Tags = request.Tags.Count > 0 ? JsonSerializer.Serialize(request.Tags) : null;
        if (request.Status.HasValue)
        {
            var previousStatus = content.Status;
            if (request.Status.Value != content.Status &&
                role is not WorkspaceMemberRoleEnum.Owner and not WorkspaceMemberRoleEnum.Manager)
            {
                return GenericResponse<ContentResponseDto>.CreateError(
                    "Only workspace owners and managers can change content status.",
                    HttpStatusCode.Forbidden);
            }
            var statusValidation = ValidateStatusTransition(content.Status, request.Status.Value);
            if (!statusValidation.Success)
                return GenericResponse<ContentResponseDto>.CreateError(statusValidation.Message!, (HttpStatusCode)statusValidation.StatusCode);
            content.Status = request.Status.Value;
            await _contentRepository.UpdateAsync(content, cancellationToken);
            if (previousStatus != content.Status)
            {
                await CreateApprovalNotificationAsync(content, MapStatusToNotificationEvent(content.Status), null, cancellationToken);
            }
            return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), MessageConstants.Content.UpdatedSuccess);
        }
        else if (content.Status == ContentStatusEnum.Approved || content.Status == ContentStatusEnum.Rejected)
        {
            content.Status = ContentStatusEnum.Draft;
        }
        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), MessageConstants.Content.UpdatedSuccess);
    }

    public async Task<GenericResponse<ContentResponseDto>> CloneInWorkspaceAsync(Guid id, Guid workspaceId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (actorUserId == Guid.Empty) return GenericResponse<ContentResponseDto>.CreateError("Authenticated creator is required.", HttpStatusCode.Unauthorized);
        var existing = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null || existing.WorkspaceId != workspaceId) return NotFound();
        var clone = new Content { WorkspaceId = workspaceId, ProfileId = existing.ProfileId, BrandId = existing.BrandId, Brand = existing.Brand, ProductId = existing.ProductId, Product = existing.Product, AdType = existing.AdType, Title = existing.Title, TextContent = existing.TextContent, RichTextJson = existing.RichTextJson, RichTextVersion = existing.RichTextVersion, ImageUrl = existing.ImageUrl, VideoUrl = existing.VideoUrl, Tags = existing.Tags, Status = ContentStatusEnum.Draft };
        clone.PrimaryCreatorId = actorUserId;
        await _contentRepository.AddAsync(clone, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(clone), MessageConstants.Content.ClonedSuccess);
    }

    public Task<GenericResponse<bool>> SoftDeleteInWorkspaceAsync(Guid id, Guid workspaceId, WorkspaceMemberRoleEnum role, CancellationToken cancellationToken = default)
        => ChangeDeletedInWorkspaceAsync(id, workspaceId, true, role, cancellationToken);

    public Task<GenericResponse<bool>> RestoreInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => ChangeDeletedInWorkspaceAsync(id, workspaceId, false, null, cancellationToken);

    private async Task<GenericResponse<bool>> ChangeDeletedInWorkspaceAsync(Guid id, Guid workspaceId, bool deleted, WorkspaceMemberRoleEnum? role, CancellationToken cancellationToken)
    {
        var content = deleted ? await _contentRepository.GetByIdAsync(id, cancellationToken) : await _contentRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
        if (content == null || content.WorkspaceId != workspaceId) return GenericResponse<bool>.CreateError(MessageConstants.Content.NotFound, HttpStatusCode.NotFound);

        // Role-based deletion logic for Workspaces
        if (deleted && role.HasValue)
        {
            if (role.Value == WorkspaceMemberRoleEnum.ContentCreator)
            {
                if (content.Status == ContentStatusEnum.Approved || content.Status == ContentStatusEnum.Published)
                {
                    return GenericResponse<bool>.CreateError("Content creators cannot delete accepted or published content. Only owners can.", HttpStatusCode.Forbidden);
                }
            }
        }

        content.IsDeleted = deleted;
        if (!deleted) 
        {
            content.Status = ContentStatusEnum.Draft;
        }
        else
        {
            // Cascade soft-delete to approvals
            if (content.Approvals != null)
            {
                foreach (var approval in content.Approvals)
                {
                    approval.IsDeleted = true;
                }
            }
        }

        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, deleted ? MessageConstants.Content.DeletedSuccess : MessageConstants.Content.RestoredSuccess);
    }

    public async Task<GenericResponse<bool>> SubmitForApprovalAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.WorkspaceId != workspaceId)
        {
            return GenericResponse<bool>.CreateError(MessageConstants.Content.NotFound, HttpStatusCode.NotFound);
        }

        if (content.Status != ContentStatusEnum.Draft && content.Status != ContentStatusEnum.Rejected)
        {
            return GenericResponse<bool>.CreateError("Only draft or rejected content can be submitted for approval.", HttpStatusCode.BadRequest);
        }

        content.Status = ContentStatusEnum.PendingApproval;
        content.Approvals.Add(new Approval { ContentId=content.Id, Status=ContentStatusEnum.PendingApproval, SubmittedAt=DateTime.UtcNow });
        await _contentRepository.UpdateAsync(content, cancellationToken);
        await CreateApprovalNotificationAsync(content, ApprovalNotificationEvent.Submitted, null, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Content submitted for approval successfully.");
    }

    public async Task<GenericResponse<ContentResponseDto>> ApproveAsync(Guid id, Guid workspaceId, Guid approverUserId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.WorkspaceId != workspaceId)
        {
            return GenericResponse<ContentResponseDto>.CreateError(MessageConstants.Content.NotFound, HttpStatusCode.NotFound);
        }

        if (content.Status != ContentStatusEnum.PendingApproval)
        {
            return GenericResponse<ContentResponseDto>.CreateError("Only pending approval content can be approved.", HttpStatusCode.BadRequest);
        }

        content.Status = ContentStatusEnum.Approved;
        content.Approvals.Add(new Approval
        {
            ContentId = content.Id,
            ApproverUserId = approverUserId,
            Status = ContentStatusEnum.Approved,
            SubmittedAt = content.Approvals.Where(a=>!a.IsDeleted && a.Status==ContentStatusEnum.PendingApproval).OrderByDescending(a=>a.SubmittedAt).Select(a=>a.SubmittedAt).FirstOrDefault(),
            ApprovedAt = DateTime.UtcNow
        });

        await _contentRepository.UpdateAsync(content, cancellationToken);
        await CreateApprovalNotificationAsync(content, ApprovalNotificationEvent.Approved, null, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), "Content approved successfully.");
    }

    public async Task<GenericResponse<ContentResponseDto>> RejectAsync(Guid id, Guid workspaceId, Guid approverUserId, string? notes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notes) || notes.Trim().Length < 5)
        {
            return GenericResponse<ContentResponseDto>.CreateError("Rejection notes are required and must be at least 5 characters.", HttpStatusCode.BadRequest);
        }

        if (notes.Trim().Length > 1000)
        {
            return GenericResponse<ContentResponseDto>.CreateError("Rejection notes must not exceed 1000 characters.", HttpStatusCode.BadRequest);
        }

        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.WorkspaceId != workspaceId)
        {
            return GenericResponse<ContentResponseDto>.CreateError(MessageConstants.Content.NotFound, HttpStatusCode.NotFound);
        }

        if (content.Status != ContentStatusEnum.PendingApproval)
        {
            return GenericResponse<ContentResponseDto>.CreateError("Only pending approval content can be rejected.", HttpStatusCode.BadRequest);
        }

        content.Status = ContentStatusEnum.Rejected;
        content.Approvals.Add(new Approval
        {
            ContentId = content.Id,
            ApproverUserId = approverUserId,
            Status = ContentStatusEnum.Rejected,
            SubmittedAt = content.Approvals.Where(a=>!a.IsDeleted && a.Status==ContentStatusEnum.PendingApproval).OrderByDescending(a=>a.SubmittedAt).Select(a=>a.SubmittedAt).FirstOrDefault(),
            Notes = notes
        });

        await _contentRepository.UpdateAsync(content, cancellationToken);
        await CreateApprovalNotificationAsync(content, ApprovalNotificationEvent.Rejected, notes, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), "Content rejected successfully.");
    }

    public async Task<GenericResponse<PagedResult<ContentListDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        if (brandId.HasValue)
        {
            var validation = await ValidateBrandAndProductAsync(profileId, brandId.Value, null, cancellationToken);
            if (!validation.Success)
            {
                return GenericResponse<PagedResult<ContentListDto>>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
            }
        }

        var result = await _contentRepository.GetPagedByProfileIdAsync(profileId, request, brandId, adType, includeDeleted, status, cancellationToken);
        return GenericResponse<PagedResult<ContentListDto>>.CreateSuccess(result, MessageConstants.Content.ListRetrievedSuccess);
    }

    public async Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return NotFound();
        }

        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), MessageConstants.Content.RetrievedSuccess);
    }

    public async Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, UpdateContentRequest request, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return NotFound();
        }

        var validation = await ValidateBrandAndProductAsync(profileId, content.BrandId, request.ProductId, cancellationToken);
        if (!validation.Success)
        {
            return GenericResponse<ContentResponseDto>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
        }

        if (request.ProductId.HasValue) content.ProductId = request.ProductId;
        if (request.AdType.HasValue) content.AdType = request.AdType.Value;
        if (request.Title != null) content.Title = request.Title;
        if (request.RichTextJson != null) { content.RichTextJson = request.RichTextJson; content.RichTextVersion = request.RichTextVersion; content.TextContent = AISAM.Data.RichTextDocument.PlainText(request.RichTextJson, request.RichTextVersion); } else if (request.TextContent != null) { content.TextContent = request.TextContent; content.RichTextJson = null; content.RichTextVersion = null; }
        if (request.ImageUrl != null) content.ImageUrl = FormatImageUrlForJsonb(request.ImageUrl);
        if (request.VideoUrl != null) content.VideoUrl = request.VideoUrl;
        if (request.StyleDescription != null) content.StyleDescription = request.StyleDescription;
        if (request.ContextDescription != null) content.ContextDescription = request.ContextDescription;
        if (request.RepresentativeCharacter != null) content.RepresentativeCharacter = request.RepresentativeCharacter;
        if (request.Tags != null) content.Tags = request.Tags.Count > 0 ? JsonSerializer.Serialize(request.Tags) : null;

        if (content.Status == ContentStatusEnum.Approved || content.Status == ContentStatusEnum.Rejected)
        {
            content.Status = ContentStatusEnum.Draft;
        }

        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), MessageConstants.Content.UpdatedSuccess);
    }

    public async Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var existing = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null || existing.ProfileId != profileId)
        {
            return NotFound();
        }

        var clone = new Content
        {
            ProfileId = existing.ProfileId,
            WorkspaceId = existing.WorkspaceId,
            BrandId = existing.BrandId,
            Brand = existing.Brand,
            ProductId = existing.ProductId,
            Product = existing.Product,
            AdType = existing.AdType,
            Title = existing.Title,
            TextContent = existing.TextContent, RichTextJson = existing.RichTextJson, RichTextVersion = existing.RichTextVersion,
            ImageUrl = existing.ImageUrl,
            VideoUrl = existing.VideoUrl,
            StyleDescription = existing.StyleDescription,
            ContextDescription = existing.ContextDescription,
            RepresentativeCharacter = existing.RepresentativeCharacter,
            Tags = existing.Tags,
            Status = ContentStatusEnum.Draft
        };

        await _contentRepository.AddAsync(clone, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(clone), MessageConstants.Content.ClonedSuccess);
    }

    public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return GenericResponse<bool>.CreateError(MessageConstants.Content.NotFound, HttpStatusCode.NotFound);
        }

        content.IsDeleted = true;
        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, MessageConstants.Content.DeletedSuccess);
    }

    public async Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return GenericResponse<bool>.CreateError(MessageConstants.Content.NotFound, HttpStatusCode.NotFound);
        }

        if (!content.IsDeleted)
        {
            return GenericResponse<bool>.CreateError(MessageConstants.Content.NotDeleted, HttpStatusCode.BadRequest);
        }

        content.IsDeleted = false;
        content.Status = ContentStatusEnum.Draft;
        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, MessageConstants.Content.RestoredSuccess);
    }

    public async Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default)
    {
        return await PublishInternalAsync(contentId, integrationId, profileId, null, true, cancellationToken);
    }

    public async Task<GenericResponse<PublishResultDto>> PublishAsync(
        Guid contentId,
        Guid integrationId,
        Guid profileId,
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await PublishInternalAsync(contentId, integrationId, profileId, workspaceId, true, cancellationToken);
    }

    public async Task<GenericResponse<PublishResultDto>> PublishScheduledAsync(
        Guid contentId, Guid integrationId, Guid profileId, Guid workspaceId,
        CancellationToken cancellationToken = default)
        => await PublishInternalAsync(contentId, integrationId, profileId, workspaceId, false, cancellationToken);

    private async Task<GenericResponse<PublishResultDto>> PublishInternalAsync(
        Guid contentId,
        Guid integrationId,
        Guid profileId,
        Guid? workspaceId,
        bool cancelActiveSchedules,
        CancellationToken cancellationToken)
    {
        if(_access is not null)
        {
            if(_context?.ExecutionActorId is not { } actor || workspaceId is not { } workspace)
                return GenericResponse<PublishResultDto>.CreateError("Publishing requires a known actor and workspace.",HttpStatusCode.Forbidden);
            var decision=await _access.CheckAsync(new(actor,workspace,AISAM.Services.Access.AccessResourceKind.Content,contentId,AISAM.Services.Access.ResourcePermission.PostPublish,integrationId),cancellationToken);
            if(!decision.Allowed) return GenericResponse<PublishResultDto>.CreateError(decision.ErrorCode!, (HttpStatusCode)decision.StatusCode);
        }
        var semaphore = _publishLocks.GetOrAdd(contentId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken);
            if (content == null || content.IsDeleted ||
                (workspaceId.HasValue ? content.WorkspaceId != workspaceId : content.ProfileId != profileId))
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.NotFound, HttpStatusCode.NotFound);
            }

            // A content item may be published to more than one social integration.
            // The first successful post marks it Published; later integrations must
            // still be allowed to publish the same content.
            if (_context?.ExecutionSnapshotId is null && content.Status != ContentStatusEnum.Approved && content.Status != ContentStatusEnum.Published)
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.MustBeApproved, HttpStatusCode.BadRequest);
            }

            if (workspaceId.HasValue)
            {
                var workspace = await _workspaceRepository.GetByIdAsync(workspaceId.Value, cancellationToken);
                if (workspace != null)
                {
                    WorkspaceLifecyclePolicy.SynchronizeStatus(workspace, DateTime.UtcNow);
                    if (WorkspaceLifecyclePolicy.IsReadOnly(workspace.Status))
                    {
                        return GenericResponse<PublishResultDto>.CreateError(
                            MessageConstants.Content.WorkspaceExpiredOrInactive, HttpStatusCode.Forbidden);
                    }
                }
            }

            var integration = await _socialIntegrationRepository.GetByIdAsync(integrationId, cancellationToken);
            if (integration == null || integration.IsDeleted || !integration.IsActive || integration.BrandId != content.BrandId ||
                (workspaceId.HasValue ? integration.WorkspaceId != workspaceId : integration.ProfileId != profileId))
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.SocialIntegrationNotFoundOrInactive, HttpStatusCode.NotFound);
            }

            var quotaCheck = workspaceId.HasValue
                ? await _quotaService.EnsureWorkspacePostQuotaAsync(workspaceId.Value, cancellationToken)
                : await _quotaService.EnsurePostQuotaAsync(profileId, cancellationToken);
            if (!quotaCheck.Success)
            {
                return GenericResponse<PublishResultDto>.CreateError(
                    quotaCheck.Message!,
                    (HttpStatusCode)quotaCheck.StatusCode,
                    quotaCheck.Error?.ErrorCode);
            }

            if (!_providers.TryGetValue(integration.Platform.ToString().ToLowerInvariant(), out var provider))
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.PublishingProviderNotSupported, HttpStatusCode.BadRequest);
            }

            var socialAccount = integration.SocialAccount
                ?? await _socialAccountRepository.GetByIdAsync(integration.SocialAccountId, cancellationToken);
            if (socialAccount == null || socialAccount.IsDeleted ||
                (workspaceId.HasValue ? socialAccount.WorkspaceId != workspaceId : socialAccount.ProfileId != profileId))
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.SocialAccountNotFound, HttpStatusCode.NotFound);
            }

            if (!socialAccount.IsActive)
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.SocialAccountInactive, HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(socialAccount.UserAccessToken))
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.SocialAccountTokenMissing, HttpStatusCode.Conflict,"SOCIAL_REAUTH_REQUIRED");
            }

            if (socialAccount.ExpiresAt.HasValue && socialAccount.ExpiresAt.Value <= DateTime.UtcNow)
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.SocialAccountTokenExpired, HttpStatusCode.Conflict,"SOCIAL_REAUTH_REQUIRED");
            }

            if (string.IsNullOrWhiteSpace(integration.AccessToken))
            {
                return GenericResponse<PublishResultDto>.CreateError(MessageConstants.Content.IntegrationTokenMissing, HttpStatusCode.Conflict,"SOCIAL_REAUTH_REQUIRED");
            }

            if (_context is not null && !cancelActiveSchedules && !_context.ExecutionSnapshotId.HasValue)
                return GenericResponse<PublishResultDto>.CreateError("Legacy schedule has no reviewed snapshot. Resubmit, approve and schedule again.",HttpStatusCode.Conflict);
            var payloadContent=content;
            PublishSnapshot? frozen=null;
            if(_context is not null)
            {
                var snapshotId=_context.ExecutionSnapshotId??content.ApprovedSnapshotId;
                frozen=await _context.PublishSnapshots.IgnoreQueryFilters().AsNoTracking().Include(s=>s.Media)
                    .SingleOrDefaultAsync(s=>s.Id==snapshotId && s.ContentId==content.Id && s.WorkspaceId==content.WorkspaceId,cancellationToken);
                if(frozen is null)return GenericResponse<PublishResultDto>.CreateError("Content needs a reviewed snapshot. Submit and approve it again.",HttpStatusCode.Conflict);
                var capabilityError=PublishingCapabilities.Validate(PublishingCapabilities.For(integration,socialAccount,DateTime.UtcNow,_instagram.VerifiedCarouselIntegrationIds.Contains(integration.Id)),frozen.Media);
                if(capabilityError is not null)return GenericResponse<PublishResultDto>.CreateError(capabilityError,
                    capabilityError=="SOCIAL_REAUTH_REQUIRED"?HttpStatusCode.Conflict:HttpStatusCode.BadRequest,capabilityError);
                payloadContent=JsonSerializer.Deserialize<Content>(frozen.Payload)!;
                var images=frozen.Media.Where(m=>m.MimeType?.StartsWith("image/")==true).OrderBy(m=>m.SortOrder).ToList();
                var videos=frozen.Media.Where(m=>m.MimeType?.StartsWith("video/")==true).OrderBy(m=>m.SortOrder).ToList();
                if(images.Count>0){payloadContent.AdType=AdTypeEnum.ImageText;payloadContent.ImageUrl=JsonSerializer.Serialize(images.Select(m=>m.Url));}
                if(videos.Count>0){payloadContent.AdType=AdTypeEnum.VideoText;payloadContent.VideoUrl=videos[0].Url;}
            }
            var postDto = BuildPostDto(payloadContent);
            if (payloadContent.FormattedCaptions?.TryGetValue(integration.Platform.ToString().ToLowerInvariant(), out var formattedCaption) == true) postDto.Message = formattedCaption;
            postDto.Progress=async(stage,media,token)=>
            {
                if(stage=="Publishing"&&_access is not null)
                {
                    var permission=await _access.CheckAsync(new(_context!.ExecutionActorId!.Value,workspaceId!.Value,AISAM.Services.Access.AccessResourceKind.Content,contentId,AISAM.Services.Access.ResourcePermission.PostPublish,integrationId),token);
                    if(!permission.Allowed)throw new AISAM.Repositories.ResourceMutationDeniedException();
                }
                if(_progress?.Report is {} report)await report(stage,media,token);
            };
            if(frozen is not null)postDto.Media=frozen.Media.OrderBy(m=>m.SortOrder).Select(m=>new PublishMediaDto(m.Id,m.Url,m.MimeType??"unknown",m.DurationSeconds)).ToList();
            var decryptedAccount = CloneAccountForPublish(socialAccount);
            var decryptedIntegration = CloneIntegrationForPublish(integration);

            try
            {
                decryptedAccount.UserAccessToken = _tokenProtector.Unprotect(socialAccount.UserAccessToken);
                decryptedIntegration.AccessToken = _tokenProtector.Unprotect(integration.AccessToken);
            }
            catch (Exception)
            {
                return GenericResponse<PublishResultDto>.CreateError(
                    "Stored social credentials can no longer be decrypted. Disconnect and reconnect the account.",
                    HttpStatusCode.Conflict,
                    "SOCIAL_REAUTH_REQUIRED");
            }

            if(_access is not null)
            {
                var latest=await _access.CheckAsync(new(_context!.ExecutionActorId!.Value,workspaceId!.Value,AISAM.Services.Access.AccessResourceKind.Content,contentId,AISAM.Services.Access.ResourcePermission.PostPublish,integrationId),cancellationToken);
                if(!latest.Allowed) return GenericResponse<PublishResultDto>.CreateError(latest.ErrorCode!, (HttpStatusCode)latest.StatusCode);
            }
            PublishResultDto publishResult;
            try{publishResult=await provider.PublishAsync(decryptedAccount,decryptedIntegration,postDto,cancellationToken);}
            catch(AISAM.Repositories.ResourceMutationDeniedException)
            {
                return GenericResponse<PublishResultDto>.CreateError("Publishing permission was revoked.",HttpStatusCode.Forbidden,"ACCESS_DENIED_CHANNEL");
            }
            catch(Exception)
            {
                // A timeout may happen after the platform committed. All entry points,
                // including legacy schedules, must stop and reconcile rather than retry.
                return GenericResponse<PublishResultDto>.CreateSuccess(new(){RequiresReconciliation=true,ErrorMessage="PUBLISH_OUTCOME_UNKNOWN"},"Provider outcome is unknown; reconcile before retrying.");
            }
            if(publishResult.Success&&string.IsNullOrWhiteSpace(publishResult.ProviderPostId))publishResult.RequiresReconciliation=true;
            if(publishResult.RequiresReconciliation)
                return GenericResponse<PublishResultDto>.CreateSuccess(publishResult,"Provider accepted the request; publication requires status reconciliation.");
            if (!publishResult.Success)
            {
                var failure=GenericResponse<PublishResultDto>.CreateError("Provider rejected publication.",HttpStatusCode.BadGateway,"PUBLISH_PROVIDER_REJECTED");
                publishResult.ErrorMessage="Provider rejected publication.";
                failure.Data=publishResult;
                return failure;
            }

            try
            {
            if (cancelActiveSchedules)
            {
                await _contentCalendarRepository.CancelActiveSchedulesForContentAsync(contentId, cancellationToken);
            }

            var publishedPost=new Post
            {
                ContentId = content.Id,
                IntegrationId = integration.Id,
                SnapshotId = frozen?.Id,
                ExternalPostId = publishResult.ProviderPostId,
                PublishedAt = publishResult.PostedAt ?? DateTime.UtcNow,
                Status = ContentStatusEnum.Published
            };
            await _postRepository.AddAsync(publishedPost,cancellationToken);
            if(_context is not null&&publishResult.Media.Count>0)
            {
                var mediaRows=await _context.PostMedia.Where(m=>m.PostId==publishedPost.Id).ToListAsync(cancellationToken);
                foreach(var row in mediaRows)
                {
                    var item=publishResult.Media.SingleOrDefault(m=>m.Id==row.SnapshotMediaId);
                    if(item is not null){row.ProviderMediaId=item.ProviderMediaId;row.Status=item.Status;row.ErrorCode=item.ErrorCode;}
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(publishResult.RefreshedTargetAccessToken))
            {
                integration.AccessToken = _tokenProtector.Protect(publishResult.RefreshedTargetAccessToken);
                await _socialIntegrationRepository.UpdateAsync(integration, cancellationToken);
            }

            if(frozen is null || content.ApprovedSnapshotId==frozen.Id)
            {
                content.Status = ContentStatusEnum.Published;
                await _contentRepository.UpdateAsync(content, cancellationToken);
            }

            return GenericResponse<PublishResultDto>.CreateSuccess(publishResult, MessageConstants.Content.PublishedSuccess);
            }
            catch(Exception)
            {
                _context?.ChangeTracker.Clear();
                publishResult.RequiresReconciliation=true;
                return GenericResponse<PublishResultDto>.CreateSuccess(publishResult,"Provider accepted publication; local persistence needs reconciliation.");
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<GenericResponse<bool>> ValidateBrandAndProductAsync(Guid profileId, Guid brandId, Guid? productId, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
        if (brand == null || brand.ProfileId != profileId)
        {
            return GenericResponse<bool>.CreateError(MessageConstants.Content.BrandNotFound, HttpStatusCode.NotFound);
        }

        if (productId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(productId.Value, cancellationToken);
            if (product == null)
            {
                return GenericResponse<bool>.CreateError(MessageConstants.Content.ProductNotFound, HttpStatusCode.NotFound);
            }

            if (product.BrandId != brandId)
            {
                return GenericResponse<bool>.CreateError(MessageConstants.Content.ProductNotInBrand, HttpStatusCode.BadRequest);
            }
        }

        return GenericResponse<bool>.CreateSuccess(true);
    }

    private async Task<GenericResponse<bool>> ValidateBrandAndProductInWorkspaceAsync(Guid workspaceId, Guid brandId, Guid? productId, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
        if (brand == null || brand.WorkspaceId != workspaceId) return GenericResponse<bool>.CreateError(MessageConstants.Content.BrandNotFound, HttpStatusCode.NotFound);
        if (productId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(productId.Value, cancellationToken);
            if (product == null) return GenericResponse<bool>.CreateError(MessageConstants.Content.ProductNotFound, HttpStatusCode.NotFound);
            if (product.BrandId != brandId) return GenericResponse<bool>.CreateError(MessageConstants.Content.ProductNotInBrand, HttpStatusCode.BadRequest);
        }
        return GenericResponse<bool>.CreateSuccess(true);
    }

    private static GenericResponse<bool> ValidateStatusTransition(ContentStatusEnum current, ContentStatusEnum next)
    {
        if (current == next) return GenericResponse<bool>.CreateSuccess(true);

        if (current == ContentStatusEnum.Published)
        {
            return GenericResponse<bool>.CreateError(MessageConstants.Content.CannotChangePublished, HttpStatusCode.BadRequest);
        }

        if (next == ContentStatusEnum.Published)
        {
            return GenericResponse<bool>.CreateError(MessageConstants.Content.UsePublishEndpoint, HttpStatusCode.BadRequest);
        }

        var allowed = current switch
        {
            ContentStatusEnum.Draft => new[] { ContentStatusEnum.PendingApproval },
            ContentStatusEnum.PendingApproval => new[] { ContentStatusEnum.Draft, ContentStatusEnum.Approved, ContentStatusEnum.Rejected },
            ContentStatusEnum.Approved => new[] { ContentStatusEnum.Draft },
            ContentStatusEnum.Rejected => new[] { ContentStatusEnum.Draft, ContentStatusEnum.PendingApproval },
            _ => Array.Empty<ContentStatusEnum>()
        };

        if (!allowed.Contains(next))
        {
            return GenericResponse<bool>.CreateError(
                string.Format(MessageConstants.Content.InvalidStatusTransition, current, next), HttpStatusCode.BadRequest);
        }

        return GenericResponse<bool>.CreateSuccess(true);
    }

    private static GenericResponse<bool> ValidateCreateStatus(ContentStatusEnum? status)
    {
        if (status is null or ContentStatusEnum.Draft)
        {
            return GenericResponse<bool>.CreateSuccess(true);
        }

        return GenericResponse<bool>.CreateError(
            "Only Draft status can be selected when creating content.",
            HttpStatusCode.BadRequest);
    }

    private static GenericResponse<ContentResponseDto> NotFound()
    {
        return GenericResponse<ContentResponseDto>.CreateError(MessageConstants.Content.NotFound, HttpStatusCode.NotFound);
    }

    private async Task CreateApprovalNotificationAsync(
        Content content,
        ApprovalNotificationEvent notificationEvent,
        string? notes,
        CancellationToken cancellationToken)
    {
        if (_notificationRepository == null ||
            content.WorkspaceId == Guid.Empty ||
            notificationEvent == ApprovalNotificationEvent.None)
        {
            return;
        }

        var contentTitle = GetContentDisplayTitle(content);
        var (title, message) = notificationEvent switch
        {
            ApprovalNotificationEvent.Submitted => (
                "Content pending approval",
                $"\"{contentTitle}\" is ready for Owner or Manager review."),
            ApprovalNotificationEvent.Approved => (
                "Content approved",
                $"\"{contentTitle}\" has been approved and is ready to publish."),
            ApprovalNotificationEvent.Rejected => (
                "Content needs changes",
                string.IsNullOrWhiteSpace(notes)
                    ? $"\"{contentTitle}\" was rejected and needs changes."
                    : $"\"{contentTitle}\" was rejected: {notes.Trim()}"),
            _ => (string.Empty, string.Empty)
        };

        if (string.IsNullOrWhiteSpace(title)) return;

        await _notificationRepository.AddAsync(new Notification
        {
            ProfileId = content.ProfileId,
            WorkspaceId = content.WorkspaceId,
            Title = title,
            Message = message,
            Type = NotificationTypeEnum.ApprovalNeeded,
            TargetId = content.Id,
            TargetType = "content"
        }, cancellationToken);
    }

    private static ApprovalNotificationEvent MapStatusToNotificationEvent(ContentStatusEnum status) => status switch
    {
        ContentStatusEnum.PendingApproval => ApprovalNotificationEvent.Submitted,
        ContentStatusEnum.Approved => ApprovalNotificationEvent.Approved,
        ContentStatusEnum.Rejected => ApprovalNotificationEvent.Rejected,
        _ => ApprovalNotificationEvent.None
    };

    private static string GetContentDisplayTitle(Content content)
    {
        if (!string.IsNullOrWhiteSpace(content.Title)) return content.Title.Trim();
        var text = content.TextContent?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return "Untitled content";
        text = string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return text.Length <= 80 ? text : $"{text[..77]}...";
    }

    private static PostDto BuildPostDto(Content content)
    {
        var cleanMessage = content.TextContent;
        if (!string.IsNullOrWhiteSpace(cleanMessage))
        {
            cleanMessage = System.Text.RegularExpressions.Regex.Replace(
                cleanMessage, 
                @"\[VIDEO_SCRIPT\].*?\[/VIDEO_SCRIPT\]", 
                string.Empty, 
                System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
        }

        var postDto = new PostDto
        {
            Message = cleanMessage
        };

        if (content.AdType == AdTypeEnum.ImageText && !string.IsNullOrWhiteSpace(content.ImageUrl))
        {
            var raw = content.ImageUrl.Trim();
            if (raw.StartsWith("[", StringComparison.Ordinal))
            {
                var urls = JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
                var validUrls = urls.Where(url => !string.IsNullOrWhiteSpace(url)).ToList();
                if (validUrls.Count == 1)
                {
                    postDto.ImageUrl = validUrls[0];
                }
                else if (validUrls.Count > 1)
                {
                    postDto.ImageUrls = validUrls;
                }
            }
            else
            {
                postDto.ImageUrl = content.ImageUrl;
            }
        }
        else if (content.AdType == AdTypeEnum.VideoText)
        {
            postDto.VideoUrl = content.VideoUrl;
        }

        return postDto;
    }

    private static SocialAccount CloneAccountForPublish(SocialAccount account)
    {
        return new SocialAccount
        {
            Id = account.Id,
            ProfileId = account.ProfileId,
            Platform = account.Platform,
            AccountId = account.AccountId,
            UserAccessToken = account.UserAccessToken,
            RefreshToken = account.RefreshToken,
            ExpiresAt = account.ExpiresAt,
            IsActive = account.IsActive,
            IsDeleted = account.IsDeleted,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }

    private static SocialIntegration CloneIntegrationForPublish(SocialIntegration integration)
    {
        return new SocialIntegration
        {
            Id = integration.Id,
            ProfileId = integration.ProfileId,
            BrandId = integration.BrandId,
            SocialAccountId = integration.SocialAccountId,
            Platform = integration.Platform,
            AccessToken = integration.AccessToken,
            RefreshToken = integration.RefreshToken,
            ExpiresAt = integration.ExpiresAt,
            ExternalId = integration.ExternalId,
            AdAccountId = integration.AdAccountId,
            IsActive = integration.IsActive,
            IsDeleted = integration.IsDeleted,
            CreatedAt = integration.CreatedAt,
            UpdatedAt = integration.UpdatedAt
        };
    }

    private static string? FormatImageUrlForJsonb(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var trimmed = imageUrl.Trim();
        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
        {
            return trimmed;
        }

        return System.Text.Json.JsonSerializer.Serialize(new[] { trimmed });
    }

    private static ContentResponseDto MapToDto(Content content)
    {
        // Parse image URL JSON array for the response
        List<string>? imageUrls = null;
        if (!string.IsNullOrWhiteSpace(content.ImageUrl))
        {
            var raw = content.ImageUrl.Trim();
            if (raw.StartsWith("[", StringComparison.Ordinal))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<string>>(raw);
                    if (parsed is { Count: > 0 })
                        imageUrls = parsed.Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
                }
                catch { /* ignore malformed */ }
            }
            else
            {
                imageUrls = new List<string> { raw };
            }
        }

        return new ContentResponseDto
        {
            Id = content.Id,
            ProfileId = content.ProfileId,
            BrandId = content.BrandId,
            BrandName = content.Brand?.Name,
            ProductId = content.ProductId,
            AdType = content.AdType,
            Title = content.Title,
            TextContent = content.TextContent, RichTextJson = content.RichTextJson, RichTextVersion = content.RichTextVersion,
            ImageUrl = content.ImageUrl,
            ImageUrls = imageUrls,
            VideoUrl = content.VideoUrl,
            ThumbnailUrl = content.ThumbnailUrl,
            StyleDescription = content.StyleDescription,
            ContextDescription = content.ContextDescription,
            RepresentativeCharacter = content.RepresentativeCharacter,
            IsAiGenerated = content.IsAiGenerated,
            Tags = content.Tags,
            Status = content.Status,
            RejectionReason = content.Status == ContentStatusEnum.Rejected 
                ? content.Approvals?.Where(a => a.Status == ContentStatusEnum.Rejected && !a.IsDeleted)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefault()?.Notes 
                : null,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }

    /// <summary>
    /// Resolve the image_url JSONB value from multi-image list (preferred) or legacy single URL.
    /// </summary>
    private static string? ResolveImageUrlForStorage(List<string>? imageUrls, string? legacyImageUrl)
    {
        if (imageUrls is { Count: > 0 })
        {
            var valid = imageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
            if (valid.Count > 0)
                return JsonSerializer.Serialize(valid);
        }
        return FormatImageUrlForJsonb(legacyImageUrl);
    }
}
