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
    private readonly IWorkspaceLifecycleService _workspaceLifecycleService;
    private readonly ICreditService _creditService;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        IWorkspaceLifecycleService workspaceLifecycleService,
        ICreditService creditService)
    {
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
        _workspaceLifecycleService = workspaceLifecycleService;
        _creditService = creditService;
    }

    public async Task<GenericResponse<IReadOnlyList<WorkspaceResponseDto>>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var workspaces = await _workspaceRepository.GetByUserIdAsync(userId, cancellationToken);
        await SynchronizeWorkspacesAsync(workspaces, cancellationToken);
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

        await SynchronizeWorkspaceAsync(workspace, cancellationToken);

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
            Members =
            [
                new WorkspaceMember
                {
                    UserId = userId,
                    Role = WorkspaceMemberRoleEnum.Owner
                }
            ]
        };

        var createdWorkspace = await _workspaceRepository.AddAsync(workspace, cancellationToken);
        await _creditService.EnsureWalletAsync(createdWorkspace.Id, cancellationToken);
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

        workspace.Name = request.Name.Trim();
        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

        return GenericResponse<WorkspaceResponseDto>.CreateSuccess(
            MapToDto(workspace, userId),
            "Workspace updated successfully.");
    }

    public Task<GenericResponse<bool>> AdminSoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return AdminSoftDeleteInternalAsync(id, cancellationToken);
    }

    private async Task<GenericResponse<bool>> AdminSoftDeleteInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<bool>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var lifecycleState = _workspaceLifecycleService.ResolveState(workspace);
        if (lifecycleState != WorkspaceLifecycleState.EligibleForAdminDeletion)
        {
            return GenericResponse<bool>.CreateError(
                "Workspace is not eligible for admin soft delete.",
                HttpStatusCode.BadRequest);
        }

        workspace.Status = WorkspaceStatusEnum.Deleted;
        workspace.DeletedAt ??= DateTime.UtcNow;
        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

        return GenericResponse<bool>.CreateSuccess(true, "Workspace soft deleted successfully.");
    }

    private static WorkspaceMember? GetActiveMembership(Workspace workspace, Guid userId)
    {
        return workspace.Members.FirstOrDefault(member => member.UserId == userId && member.IsActive);
    }

    private async Task SynchronizeWorkspacesAsync(IEnumerable<Workspace> workspaces, CancellationToken cancellationToken)
    {
        foreach (var workspace in workspaces)
        {
            await SynchronizeWorkspaceAsync(workspace, cancellationToken);
        }
    }

    private async Task SynchronizeWorkspaceAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        if (_workspaceLifecycleService.TrySynchronizePersistenceState(workspace))
        {
            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
        }
    }

    private static WorkspaceResponseDto MapToDto(Workspace workspace, Guid userId)
    {
        var membership = GetActiveMembership(workspace, userId)
            ?? throw new InvalidOperationException("Active workspace membership is required.");

        return new WorkspaceResponseDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
            WorkspaceType = workspace.WorkspaceType,
            Status = workspace.Status,
            CurrentUserRole = membership.Role,
            ActiveMemberCount = workspace.Members.Count(member => member.IsActive),
            MemberLimit = workspace.MemberLimit,
            SubscriptionExpiredAt = workspace.SubscriptionExpiredAt,
            CreatedAt = workspace.CreatedAt,
            UpdatedAt = workspace.UpdatedAt
        };
    }
}
