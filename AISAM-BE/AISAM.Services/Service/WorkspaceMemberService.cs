using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AISAM.Services.Access;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class WorkspaceMemberService : IWorkspaceMemberService
{
    private readonly bool _v2;
    private readonly AisamContext? _db;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public WorkspaceMemberService(
        IWorkspaceMemberRepository workspaceMemberRepository,
        IWorkspaceRepository workspaceRepository,
        ISubscriptionRepository subscriptionRepository, IConfiguration? configuration=null, AisamContext? db=null)
    {
        _workspaceMemberRepository = workspaceMemberRepository;
        _workspaceRepository = workspaceRepository;
        _subscriptionRepository = subscriptionRepository;
        _db=db;
        _v2=configuration?.GetValue<bool>("Rbac:UseV2")==true;
    }

    public async Task<GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>> GetMembersAsync(
        Guid workspaceId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (!await _workspaceMemberRepository.ExistsAsync(workspaceId, actorUserId, cancellationToken))
        {
            return GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>.CreateError(
                "You are not a member of this workspace.",
                HttpStatusCode.Forbidden);
        }

        if(_v2)
        {
            var actor=await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId,actorUserId,cancellationToken);
            if(actor?.WorkspaceRoleV2 is not (WorkspaceRoleV2.Owner or WorkspaceRoleV2.WorkspaceManager))
            {
                if(_db is null || actor?.IsActive!=true || actor.WorkspaceRoleV2!=WorkspaceRoleV2.Member)
                    return GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>.CreateError("Directory not available.",HttpStatusCode.Forbidden);
                // Every active member can discover the workspace directory so a
                // Team Manager can select people who are not in their Team yet.
                // Financial quota fields remain omitted for non-HR members.
                var directory=await _db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking()
                    .Where(m=>m.WorkspaceId==workspaceId && m.IsActive && m.User.IsActive)
                    .OrderBy(m=>m.User.FullName).ThenBy(m=>m.User.Email)
                    .Select(m=>new WorkspaceMemberResponseDto
                    {
                        Id=m.Id,UserId=m.UserId,FullName=m.User.FullName,Email=m.User.Email,
                        WorkspaceRole=m.WorkspaceRoleV2.ToString(),JoinedAt=m.JoinedAt
                    }).ToListAsync(cancellationToken);
                return GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>.CreateSuccess(directory,"Workspace directory retrieved.");
            }
        }
        var members = await _workspaceMemberRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        return GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>.CreateSuccess(
            members.Select(Map).ToList(),
            "Workspace members retrieved successfully.");
    }

    public async Task<GenericResponse<WorkspaceMemberResponseDto>> UpdateRoleAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid memberId,
        UpdateWorkspaceMemberRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var authorizationError = await RequireOwnerAsync(workspaceId, actorUserId, cancellationToken);
        if (authorizationError != null)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                authorizationError.Value.Message,
                authorizationError.Value.Status);
        }

        if (_v2 ? request.WorkspaceRole is not (WorkspaceRoleV2.Member or WorkspaceRoleV2.WorkspaceManager) : !Enum.IsDefined(request.Role) || request.Role == WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Use ownership transfer to assign the workspace owner.");
        }

        var member = await _workspaceMemberRepository.GetByIdAsync(memberId, cancellationToken);
        if (member == null || member.WorkspaceId != workspaceId)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError("Workspace member not found.", HttpStatusCode.NotFound);
        }

        if (_v2 ? member.WorkspaceRoleV2 is not (WorkspaceRoleV2.Member or WorkspaceRoleV2.WorkspaceManager) : member.Role == WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Workspace owner role cannot be changed. Transfer ownership first.");
        }

        if(_v2) member.WorkspaceRoleV2=request.WorkspaceRole; else member.Role = request.Role;
        await _workspaceMemberRepository.UpdateAsync(member, cancellationToken);
        return GenericResponse<WorkspaceMemberResponseDto>.CreateSuccess(
            Map(member),
            "Workspace member role updated successfully.");
    }

    public async Task<GenericResponse<WorkspaceMemberResponseDto>> UpdateQuotaAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid memberId,
        UpdateWorkspaceMemberQuotaRequest request,
        CancellationToken cancellationToken = default)
    {
        var authorizationError = await RequireOwnerAsync(workspaceId, actorUserId, cancellationToken);
        if (authorizationError != null)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                authorizationError.Value.Message,
                authorizationError.Value.Status);
        }

        var member = await _workspaceMemberRepository.GetByIdAsync(memberId, cancellationToken);
        if (member == null || member.WorkspaceId != workspaceId)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError("Workspace member not found.", HttpStatusCode.NotFound);
        }

        if (_v2 ? member.WorkspaceRoleV2 is not (WorkspaceRoleV2.Member or WorkspaceRoleV2.WorkspaceManager) : member.Role == WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Workspace owner quota mode cannot be changed.",
                HttpStatusCode.BadRequest);
        }

        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError("Workspace not found.", HttpStatusCode.NotFound);
        }

        var quotaValidationError = await ValidateQuotaRequestAsync(workspace, request, cancellationToken);
        if (quotaValidationError != null)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                quotaValidationError.Value.Message,
                quotaValidationError.Value.Status,
                quotaValidationError.Value.ErrorCode);
        }

        ApplyQuota(member, request.QuotaMode, request.CreditLimit, DateTime.UtcNow);
        await _workspaceMemberRepository.UpdateAsync(member, cancellationToken);
        return GenericResponse<WorkspaceMemberResponseDto>.CreateSuccess(
            Map(member),
            "Workspace member quota updated successfully.");
    }

    public async Task<GenericResponse<object>> RemoveAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var authorizationError = await RequireOwnerAsync(workspaceId, actorUserId, cancellationToken);
        if (!_v2 && authorizationError != null)
        {
            return GenericResponse<object>.CreateError(
                authorizationError.Value.Message,
                authorizationError.Value.Status);
        }

        var member = await _workspaceMemberRepository.GetByIdAsync(memberId, cancellationToken);
        if (member == null || member.WorkspaceId != workspaceId)
        {
            return GenericResponse<object>.CreateError("Workspace member not found.", HttpStatusCode.NotFound);
        }

        if (_v2 ? member.WorkspaceRoleV2 is not (WorkspaceRoleV2.Member or WorkspaceRoleV2.WorkspaceManager) : member.Role == WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<object>.CreateError(
                "Workspace owner cannot be removed. Transfer ownership first.");
        }

        if(_v2)
        {
            var actor=await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId,actorUserId,cancellationToken);
            if(actor?.Workspace.Status!=WorkspaceStatusEnum.Active || !WorkspaceHrV2Policy.CanRemove(actor,member))
                return GenericResponse<object>.CreateError("Member removal not allowed.",HttpStatusCode.Forbidden);
        }
        await _workspaceMemberRepository.RemoveAsync(memberId, cancellationToken);
        return GenericResponse<object>.CreateSuccess(null, "Workspace member removed successfully.");
    }

    public async Task<GenericResponse<WorkspaceMemberResponseDto>> TransferOwnershipAsync(
        Guid workspaceId,
        Guid actorUserId,
        TransferWorkspaceOwnershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var authorizationError = await RequireOwnerAsync(workspaceId, actorUserId, cancellationToken);
        if (authorizationError != null)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                authorizationError.Value.Message,
                authorizationError.Value.Status);
        }

        var target = await _workspaceMemberRepository.GetByIdAsync(request.TargetMemberId, cancellationToken);
        if (target == null || target.WorkspaceId != workspaceId)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Workspace member not found.",
                HttpStatusCode.NotFound);
        }

        if (_v2 ? target.WorkspaceRoleV2!=WorkspaceRoleV2.WorkspaceManager : target.Role != WorkspaceMemberRoleEnum.Manager)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Ownership can only be transferred to an active workspace manager.");
        }

        var newOwner = await _workspaceMemberRepository.TransferOwnershipAsync(
            workspaceId,
            actorUserId,
            request.TargetMemberId,
            cancellationToken);

        return GenericResponse<WorkspaceMemberResponseDto>.CreateSuccess(
            Map(newOwner),
            "Workspace ownership transferred successfully.");
    }

    private async Task<(string Message, HttpStatusCode Status)?> RequireOwnerAsync(
        Guid workspaceId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var actor = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(
            workspaceId,
            actorUserId,
            cancellationToken);

        if (_v2 ? actor?.IsActive!=true || actor.WorkspaceRoleV2!=WorkspaceRoleV2.Owner : actor?.Role != WorkspaceMemberRoleEnum.Owner)
        {
            return ("Only the workspace owner can manage members.", HttpStatusCode.Forbidden);
        }

        return actor.Workspace.Status == WorkspaceStatusEnum.Active
            ? null
            : ("Workspace must be active to manage members.", HttpStatusCode.Forbidden);
    }

    private static WorkspaceMemberResponseDto Map(WorkspaceMember member)
    {
        return new WorkspaceMemberResponseDto
        {
            Id = member.Id,
            UserId = member.UserId,
            Email = member.User.Email,
            FullName = member.User.FullName,
            Role = member.Role,
            WorkspaceRole=member.WorkspaceRoleV2?.ToString(),
            QuotaMode = member.QuotaMode,
            CreditLimit = member.CreditLimit,
            CreditUsed = member.CreditUsed,
            CreditPeriodStart = member.CreditPeriodStart,
            JoinedAt = member.JoinedAt
        };
    }

    private async Task<(string Message, HttpStatusCode Status, string? ErrorCode)?> ValidateQuotaRequestAsync(
        Workspace workspace,
        UpdateWorkspaceMemberQuotaRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.QuotaMode))
        {
            return ("Invalid member quota mode.", HttpStatusCode.BadRequest, "INVALID_MEMBER_QUOTA_MODE");
        }

        if (request.QuotaMode == MemberQuotaModeEnum.SharedPool)
        {
            return request.CreditLimit.HasValue
                ? ("Shared pool members cannot have an assigned credit limit.", HttpStatusCode.BadRequest, "INVALID_MEMBER_CREDIT_LIMIT")
                : null;
        }

        if (request.CreditLimit is null or <= 0)
        {
            return ("Assigned member quota requires a positive credit limit.", HttpStatusCode.BadRequest, "INVALID_MEMBER_CREDIT_LIMIT");
        }

        var activeSubscription = await _subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(workspace.Id, cancellationToken);
        if (workspace.WorkspaceType != WorkspaceTypeEnum.Business || activeSubscription?.Plan != SubscriptionPlanEnum.Premium)
        {
            return ("Assigned member quotas are only available for Business Pro workspaces.", HttpStatusCode.BadRequest, "PLAN_DOES_NOT_SUPPORT_MEMBER_QUOTA_MODE");
        }

        return null;
    }

    private static void ApplyQuota(
        WorkspaceMember member,
        MemberQuotaModeEnum quotaMode,
        long? creditLimit,
        DateTime utcNow)
    {
        if (quotaMode == MemberQuotaModeEnum.SharedPool)
        {
            member.QuotaMode = MemberQuotaModeEnum.SharedPool;
            member.CreditLimit = null;
            member.CreditUsed = 0;
            member.CreditPeriodStart = null;
            return;
        }

        var isModeChanged = member.QuotaMode != quotaMode;
        member.QuotaMode = quotaMode;
        member.CreditLimit = creditLimit;

        if (quotaMode == MemberQuotaModeEnum.MonthlyAssignedLimit)
        {
            var currentMonthStart = new DateTime(utcNow.Year, utcNow.Month, 1);
            if (isModeChanged || member.CreditPeriodStart != currentMonthStart)
            {
                member.CreditUsed = 0;
                member.CreditPeriodStart = currentMonthStart;
            }
        }
        else
        {
            if (isModeChanged)
            {
                member.CreditUsed = 0;
            }

            member.CreditPeriodStart = null;
        }
    }
}
