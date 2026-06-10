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

    public WorkspaceService(IWorkspaceRepository workspaceRepository, IUserRepository userRepository)
    {
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
    }

    public async Task<GenericResponse<IReadOnlyList<WorkspaceResponseDto>>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var workspaces = await _workspaceRepository.GetByUserIdAsync(userId, cancellationToken);
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
            WorkspaceType = workspace.WorkspaceType,
            Status = workspace.Status,
            CurrentUserRole = membership.Role,
            ActiveMemberCount = workspace.Members.Count(member => member.IsActive),
            SubscriptionExpiredAt = workspace.SubscriptionExpiredAt,
            CreatedAt = workspace.CreatedAt,
            UpdatedAt = workspace.UpdatedAt
        };
    }
}
