using Microsoft.Extensions.Configuration;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class WorkspaceInvitationRepository : IWorkspaceInvitationRepository
{
    private readonly AisamContext _context;
    private readonly bool _v2;

    public WorkspaceInvitationRepository(AisamContext context, IConfiguration? configuration=null)
    {
        _context = context;
        _v2=string.Equals(configuration?["Rbac:UseV2"],"true",StringComparison.OrdinalIgnoreCase);
    }

    public async Task<WorkspaceInvitation?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(invitation => invitation.Token == token, cancellationToken);
    }

    public async Task<WorkspaceInvitation?> GetByWorkspaceAndIdAsync(
        Guid workspaceId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(invitation => invitation.WorkspaceId == workspaceId && invitation.Id == id, cancellationToken);
    }

    public async Task<WorkspaceInvitation?> GetPendingByWorkspaceAndEmailAsync(
        Guid workspaceId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var utcNow = DateTime.UtcNow;

        return await Query()
            .FirstOrDefaultAsync(invitation =>
                invitation.WorkspaceId == workspaceId &&
                invitation.Email == normalizedEmail &&
                invitation.AcceptedAt == null &&
                invitation.RevokedAt == null &&
                invitation.ExpiresAt > utcNow,
                cancellationToken);
    }

    public async Task<IReadOnlyList<WorkspaceInvitation>> GetPendingByWorkspaceIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        return await Query()
            .Where(invitation =>
                invitation.WorkspaceId == workspaceId &&
                invitation.AcceptedAt == null &&
                invitation.RevokedAt == null &&
                invitation.ExpiresAt > utcNow)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPendingByWorkspaceIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        return await _context.WorkspaceInvitations.CountAsync(invitation =>
            invitation.WorkspaceId == workspaceId &&
            invitation.AcceptedAt == null &&
            invitation.RevokedAt == null &&
            invitation.ExpiresAt > utcNow,
            cancellationToken);
    }

    public async Task<WorkspaceInvitation> AddAsync(
        WorkspaceInvitation invitation,
        CancellationToken cancellationToken = default)
    {
        invitation.Email = invitation.Email.Trim().ToLowerInvariant();
        invitation.CreatedAt = DateTime.UtcNow;

        _context.WorkspaceInvitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);
        return invitation;
    }

    public async Task<WorkspaceMember> AcceptAsync(
        WorkspaceInvitation invitation,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            () => AcceptInTransactionAsync(invitation, userId, cancellationToken));
    }

    private async Task<WorkspaceMember> AcceptInTransactionAsync(
        WorkspaceInvitation invitation,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await using var transaction=_v2 && _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,cancellationToken) : null;
        if(_v2)
        {
            await _context.Entry(invitation).ReloadAsync(cancellationToken);
            var inviter=await _context.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(m=>m.WorkspaceId==invitation.WorkspaceId && m.UserId==invitation.InvitedByUserId && m.IsActive,cancellationToken);
            var user=await _context.Users.AsNoTracking().SingleOrDefaultAsync(u=>u.Id==userId && u.IsActive,cancellationToken);
            var workspace=await _context.Workspaces.IgnoreQueryFilters().AsNoTracking().SingleAsync(w=>w.Id==invitation.WorkspaceId,cancellationToken);
            var canInvite=invitation.WorkspaceRoleV2==WorkspaceRoleV2.Member && inviter?.WorkspaceRoleV2 is WorkspaceRoleV2.Owner or WorkspaceRoleV2.WorkspaceManager ||
                invitation.WorkspaceRoleV2==WorkspaceRoleV2.WorkspaceManager && inviter?.WorkspaceRoleV2==WorkspaceRoleV2.Owner;
            if(!canInvite || user is null || !string.Equals(user.Email,invitation.Email,StringComparison.OrdinalIgnoreCase) ||
                invitation.AcceptedAt.HasValue || invitation.RevokedAt.HasValue || invitation.ExpiresAt<=DateTime.UtcNow ||
                workspace.Status!=WorkspaceStatusEnum.Active || workspace.WorkspaceType!=WorkspaceTypeEnum.Business)
                throw new InvalidOperationException("Invitation is no longer authorized.");
            if(await _context.WorkspaceMembers.IgnoreQueryFilters().CountAsync(m=>m.WorkspaceId==workspace.Id && m.IsActive,cancellationToken)>=workspace.MemberLimit)
                throw new InvalidOperationException("Workspace member limit reached.");
        }
        if (invitation.Role == AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Owner)
        {
            throw new InvalidOperationException("Use ownership transfer to assign the workspace owner.");
        }

        var membership = await _context.WorkspaceMembers.FirstOrDefaultAsync(existing =>
            existing.WorkspaceId == invitation.WorkspaceId &&
            existing.UserId == userId,
            cancellationToken);

        if (membership?.IsActive == true)
        {
            throw new InvalidOperationException("User is already a member of this workspace.");
        }

        if (membership == null)
        {
            membership = new WorkspaceMember
            {
                WorkspaceId = invitation.WorkspaceId,
                UserId = userId,
                Role = invitation.Role,
                WorkspaceRoleV2 = invitation.WorkspaceRoleV2 ?? (invitation.Role == WorkspaceMemberRoleEnum.Owner ? WorkspaceRoleV2.Owner : WorkspaceRoleV2.Member),
                QuotaMode = invitation.QuotaMode,
                CreditLimit = invitation.CreditLimit,
                CreditPeriodStart = invitation.QuotaMode == AISAM.Data.Enumeration.MemberQuotaModeEnum.MonthlyAssignedLimit
                    ? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                    : null,
                JoinedAt = DateTime.UtcNow
            };
            _context.WorkspaceMembers.Add(membership);
        }
        else
        {
            membership.Role = invitation.Role;
            membership.WorkspaceRoleV2 = invitation.WorkspaceRoleV2 ?? (invitation.Role == WorkspaceMemberRoleEnum.Owner ? WorkspaceRoleV2.Owner : WorkspaceRoleV2.Member);
            membership.QuotaMode = invitation.QuotaMode;
            membership.CreditLimit = invitation.CreditLimit;
            membership.CreditUsed = 0;
            membership.CreditPeriodStart = invitation.QuotaMode == AISAM.Data.Enumeration.MemberQuotaModeEnum.MonthlyAssignedLimit
                ? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                : null;
            membership.JoinedAt = DateTime.UtcNow;
            membership.IsActive = true;
        }

        invitation.AcceptedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        if(transaction is not null) await transaction.CommitAsync(cancellationToken);
        return membership;
    }

    public async Task UpdateAsync(
        WorkspaceInvitation invitation,
        CancellationToken cancellationToken = default)
    {
        _context.WorkspaceInvitations.Update(invitation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<WorkspaceInvitation> Query()
    {
        return _context.WorkspaceInvitations
            .Include(invitation => invitation.Workspace)
            .Include(invitation => invitation.InvitedByUser);
    }
}
