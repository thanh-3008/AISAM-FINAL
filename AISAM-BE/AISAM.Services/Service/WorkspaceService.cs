using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository)
    {
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
    }

    public async Task<GenericResponse<IReadOnlyList<WorkspaceResponseDto>>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var workspaces = await _workspaceRepository.GetByUserIdAsync(userId, cancellationToken);
        foreach (var workspace in workspaces)
        {
            await SynchronizeLifecycleAsync(workspace, cancellationToken);
        }

        var response = workspaces.Select(workspace => MapToDto(workspace, userId)).ToList();

        return GenericResponse<IReadOnlyList<WorkspaceResponseDto>>.CreateSuccess(
            response,
            "Workspaces retrieved successfully.");
    }

    public async Task<GenericResponse<WorkspaceResponseDto>> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id, cancellationToken);
        if (workspace == null || GetActiveMembership(workspace, userId) == null)
        {
            return GenericResponse<WorkspaceResponseDto>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        await SynchronizeLifecycleAsync(workspace, cancellationToken);
        return GenericResponse<WorkspaceResponseDto>.CreateSuccess(
            MapToDto(workspace, userId),
            "Workspace retrieved successfully.");
    }

    public async Task<GenericResponse<WorkspaceResponseDto>> CreateAsync(
        Guid userId,
        CreateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return GenericResponse<WorkspaceResponseDto>.CreateError("User not found.", HttpStatusCode.NotFound);
        }

        if (!Enum.IsDefined(request.WorkspaceType))
        {
            return GenericResponse<WorkspaceResponseDto>.CreateError("Invalid workspace type.");
        }

        var workspace = new Workspace
        {
            Name = request.Name.Trim(),
            WorkspaceType = request.WorkspaceType,
            MemberLimit = request.WorkspaceType == WorkspaceTypeEnum.Business ? 10 : 1,
            CreditWallet = new CreditWallet { Balance = request.WorkspaceType == WorkspaceTypeEnum.Personal ? 50 : 0 },
            Members =
            [
                new WorkspaceMember
                {
                    UserId = userId,
                    Role = WorkspaceMemberRoleEnum.Owner
                }
            ]
        };
        if (request.WorkspaceType == WorkspaceTypeEnum.Personal)
        {
            workspace.Subscriptions.Add(new Subscription
            {
                WorkspaceId = workspace.Id,
                Plan = SubscriptionPlanEnum.Free,
                QuotaPostsPerMonth = 20,
                StartDate = DateTime.UtcNow.Date,
                IsActive = true
            });
            workspace.CreditUsageRecords.Add(new CreditUsageRecord
            {
                WorkspaceId = workspace.Id,
                UserId = userId,
                Action = CreditActionEnum.SubscriptionGrant,
                Credits = 50,
                Status = CreditUsageStatusEnum.Success
            });
        }

        var createdWorkspace = await _workspaceRepository.AddAsync(workspace, cancellationToken);
        return GenericResponse<WorkspaceResponseDto>.CreateSuccess(
            MapToDto(createdWorkspace, userId),
            "Workspace created successfully.");
    }

    public async Task<GenericResponse<WorkspaceResponseDto>> UpdateAsync(
        Guid id,
        Guid userId,
        UpdateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id, cancellationToken);
        var membership = workspace == null ? null : GetActiveMembership(workspace, userId);

        if (workspace == null || membership == null)
        {
            return GenericResponse<WorkspaceResponseDto>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        if (membership.Role != WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<WorkspaceResponseDto>.CreateError(
                "Only the workspace owner can update the workspace.",
                HttpStatusCode.Forbidden);
        }

        await SynchronizeLifecycleAsync(workspace, cancellationToken);
        if (WorkspaceLifecyclePolicy.IsReadOnly(workspace.Status))
        {
            return GenericResponse<WorkspaceResponseDto>.CreateError(
                "Workspace is read-only while its subscription is expired.",
                HttpStatusCode.Forbidden,
                "WORKSPACE_READ_ONLY");
        }

        workspace.Name = request.Name.Trim();
        workspace.CompanyName = request.CompanyName?.Trim();
        workspace.Bio = request.Bio?.Trim();
        workspace.AvatarUrl = request.AvatarUrl?.Trim();
        workspace.UpdatedAt = DateTime.UtcNow;
        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

        return GenericResponse<WorkspaceResponseDto>.CreateSuccess(
            MapToDto(workspace, userId),
            "Workspace updated successfully.");
    }

    public async Task<GenericResponse<bool>> AdminSoftDeleteAsync(
        Guid id,
        Guid adminUserId,
        CancellationToken cancellationToken = default)
    {
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
        {
            return GenericResponse<bool>.CreateError(
                "Only an administrator can delete a workspace.",
                HttpStatusCode.Forbidden);
        }

        var workspace = await _workspaceRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
        if (workspace == null || workspace.Status == WorkspaceStatusEnum.Deleted)
        {
            return GenericResponse<bool>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        await SynchronizeLifecycleAsync(workspace, cancellationToken);
        if (workspace.Status != WorkspaceStatusEnum.EligibleForDeletion)
        {
            return GenericResponse<bool>.CreateError(
                "Workspace is not eligible for deletion.",
                HttpStatusCode.Conflict,
                "WORKSPACE_NOT_ELIGIBLE_FOR_DELETION");
        }

        workspace.Status = WorkspaceStatusEnum.Deleted;
        workspace.DeletedAt = DateTime.UtcNow;
        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

        return GenericResponse<bool>.CreateSuccess(true, "Workspace soft deleted successfully.");
    }

    private async Task SynchronizeLifecycleAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        if (WorkspaceLifecyclePolicy.SynchronizeStatus(workspace, DateTime.UtcNow))
        {
            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
        }
    }

    private static WorkspaceMember? GetActiveMembership(Workspace workspace, Guid userId)
    {
        return workspace.Members.FirstOrDefault(member => member.UserId == userId && member.IsActive);
    }

    private static WorkspaceResponseDto MapToDto(Workspace workspace, Guid userId)
    {
        var membership = GetActiveMembership(workspace, userId)
            ?? throw new InvalidOperationException("Active workspace membership is required.");

        return new WorkspaceResponseDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
            CompanyName = workspace.CompanyName,
            Bio = workspace.Bio,
            AvatarUrl = workspace.AvatarUrl,
            WorkspaceType = workspace.WorkspaceType,
            Status = workspace.Status,
            CurrentUserRole = membership.Role,
            ActiveMemberCount = workspace.Members.Count(member => member.IsActive),
            MemberLimit = workspace.MemberLimit,
            SubscriptionExpiredAt = workspace.SubscriptionExpiredAt,
            ArchivedAt = workspace.ArchivedAt,
            CreatedAt = workspace.CreatedAt,
            UpdatedAt = workspace.UpdatedAt
        };
    }
}
