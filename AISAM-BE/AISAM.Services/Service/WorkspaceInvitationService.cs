using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;

namespace AISAM.Services.Service;

public sealed class WorkspaceInvitationService : IWorkspaceInvitationService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IWorkspaceInvitationRepository _workspaceInvitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly string _frontendBaseUrl;

    public WorkspaceInvitationService(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IWorkspaceInvitationRepository workspaceInvitationRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IOptions<FrontendSettings> frontendSettings)
    {
        _workspaceRepository = workspaceRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _workspaceInvitationRepository = workspaceInvitationRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _frontendBaseUrl = frontendSettings.Value.BaseUrl.TrimEnd('/');
    }

    public async Task<GenericResponse<WorkspaceInvitationResponseDto>> InviteAsync(
        Guid workspaceId,
        Guid inviterUserId,
        CreateWorkspaceInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<WorkspaceInvitationResponseDto>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var inviterMembership = workspace.Members.FirstOrDefault(member =>
            member.UserId == inviterUserId && member.IsActive);
        if (inviterMembership?.Role != WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<WorkspaceInvitationResponseDto>.CreateError(
                "Only the workspace owner can invite members.",
                HttpStatusCode.Forbidden);
        }

        if (workspace.WorkspaceType != WorkspaceTypeEnum.Business)
        {
            return GenericResponse<WorkspaceInvitationResponseDto>.CreateError(
                "Personal workspace cannot invite members.",
                HttpStatusCode.Forbidden);
        }

        if (workspace.Status != WorkspaceStatusEnum.Active)
        {
            return GenericResponse<WorkspaceInvitationResponseDto>.CreateError(
                "Workspace must be active to invite members.",
                HttpStatusCode.Forbidden);
        }

        if (!Enum.IsDefined(request.Role) || request.Role == WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<WorkspaceInvitationResponseDto>.CreateError("Invalid invitation role.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var invitedUser = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (invitedUser != null &&
            await _workspaceMemberRepository.ExistsAsync(workspaceId, invitedUser.Id, cancellationToken))
        {
            return GenericResponse<WorkspaceInvitationResponseDto>.CreateError(
                "User is already a member of this workspace.",
                HttpStatusCode.Conflict);
        }

        if (await _workspaceInvitationRepository.GetPendingByWorkspaceAndEmailAsync(
                workspaceId,
                normalizedEmail,
                cancellationToken) != null)
        {
            return GenericResponse<WorkspaceInvitationResponseDto>.CreateError(
                "A pending invitation already exists for this email.",
                HttpStatusCode.Conflict);
        }

        var activeMemberCount = workspace.Members.Count(member => member.IsActive);
        var pendingInvitationCount = await _workspaceInvitationRepository.CountPendingByWorkspaceIdAsync(
            workspaceId,
            cancellationToken);
        if (activeMemberCount + pendingInvitationCount >= workspace.MemberLimit)
        {
            return GenericResponse<WorkspaceInvitationResponseDto>.CreateError(
                $"Workspace member limit of {workspace.MemberLimit} has been reached.",
                HttpStatusCode.Conflict);
        }

        var invitation = await _workspaceInvitationRepository.AddAsync(new WorkspaceInvitation
        {
            WorkspaceId = workspaceId,
            InvitedByUserId = inviterUserId,
            Email = normalizedEmail,
            Role = request.Role,
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }, cancellationToken);

        var inviter = await _userRepository.GetByIdAsync(inviterUserId);
        var inviterName = inviter?.FullName ?? inviter?.Email ?? "Workspace owner";
        var invitationLink = $"{_frontendBaseUrl}/workspace/invitations/accept?token={Uri.EscapeDataString(invitation.Token)}";
        await _emailService.SendTeamInvitationAsync(normalizedEmail, workspace.Name, inviterName, invitationLink);

        invitation.Workspace = workspace;
        return GenericResponse<WorkspaceInvitationResponseDto>.CreateSuccess(
            Map(invitation),
            "Workspace invitation created successfully.");
    }

    public async Task<GenericResponse<AcceptWorkspaceInvitationResponseDto>> AcceptAsync(
        Guid userId,
        AcceptWorkspaceInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return GenericResponse<AcceptWorkspaceInvitationResponseDto>.CreateError("User not found.", HttpStatusCode.NotFound);
        }

        var invitation = await _workspaceInvitationRepository.GetByTokenAsync(request.Token.Trim(), cancellationToken);
        if (invitation == null)
        {
            return GenericResponse<AcceptWorkspaceInvitationResponseDto>.CreateError("Invitation not found.", HttpStatusCode.NotFound);
        }

        if (invitation.AcceptedAt.HasValue || invitation.RevokedAt.HasValue || invitation.ExpiresAt <= DateTime.UtcNow)
        {
            return GenericResponse<AcceptWorkspaceInvitationResponseDto>.CreateError("Invitation is no longer valid.");
        }

        if (invitation.Workspace.WorkspaceType != WorkspaceTypeEnum.Business ||
            invitation.Workspace.Status != WorkspaceStatusEnum.Active)
        {
            return GenericResponse<AcceptWorkspaceInvitationResponseDto>.CreateError(
                "Workspace must be an active business workspace to accept invitations.",
                HttpStatusCode.Forbidden);
        }

        if (!string.Equals(invitation.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            return GenericResponse<AcceptWorkspaceInvitationResponseDto>.CreateError(
                "Invitation email does not match the authenticated user.",
                HttpStatusCode.Forbidden);
        }

        var activeMembers = await _workspaceMemberRepository.GetByWorkspaceIdAsync(
            invitation.WorkspaceId,
            cancellationToken);
        if (activeMembers.Count >= invitation.Workspace.MemberLimit)
        {
            return GenericResponse<AcceptWorkspaceInvitationResponseDto>.CreateError(
                $"Workspace member limit of {invitation.Workspace.MemberLimit} has been reached.",
                HttpStatusCode.Conflict);
        }

        var membership = await _workspaceInvitationRepository.AcceptAsync(invitation, userId, cancellationToken);
        return GenericResponse<AcceptWorkspaceInvitationResponseDto>.CreateSuccess(
            new AcceptWorkspaceInvitationResponseDto
            {
                WorkspaceId = membership.WorkspaceId,
                WorkspaceName = invitation.Workspace.Name,
                Role = membership.Role
            },
            "Workspace invitation accepted successfully.");
    }

    private static WorkspaceInvitationResponseDto Map(WorkspaceInvitation invitation)
    {
        return new WorkspaceInvitationResponseDto
        {
            Id = invitation.Id,
            WorkspaceId = invitation.WorkspaceId,
            WorkspaceName = invitation.Workspace.Name,
            Email = invitation.Email,
            Role = invitation.Role,
            InvitedByUserId = invitation.InvitedByUserId,
            ExpiresAt = invitation.ExpiresAt,
            CreatedAt = invitation.CreatedAt
        };
    }
}
