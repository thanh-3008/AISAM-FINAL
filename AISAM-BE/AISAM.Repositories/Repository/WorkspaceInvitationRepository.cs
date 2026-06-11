using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class WorkspaceInvitationRepository : IWorkspaceInvitationRepository
{
    private readonly AisamContext _context;

    public WorkspaceInvitationRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<WorkspaceInvitation?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(invitation => invitation.Token == token, cancellationToken);
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
