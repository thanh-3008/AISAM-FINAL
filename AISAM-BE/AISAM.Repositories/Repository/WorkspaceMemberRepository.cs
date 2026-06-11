using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class WorkspaceMemberRepository : IWorkspaceMemberRepository
{
    private readonly AisamContext _context;

    public WorkspaceMemberRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(member => member.Id == id && member.IsActive, cancellationToken);
    }

    public async Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(member =>
                member.WorkspaceId == workspaceId &&
                member.UserId == userId &&
                member.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<WorkspaceMember>> GetByWorkspaceIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(member => member.WorkspaceId == workspaceId && member.IsActive)
            .OrderBy(member => member.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkspaceMember>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(member => member.UserId == userId && member.IsActive)
            .OrderByDescending(member => member.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceMember> AddAsync(WorkspaceMember member, CancellationToken cancellationToken = default)
    {
        if (member.Role == WorkspaceMemberRoleEnum.Owner)
        {
            throw new InvalidOperationException("Use workspace creation or ownership transfer to assign the owner.");
        }

        var existingMembership = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(existing =>
                existing.WorkspaceId == member.WorkspaceId &&
                existing.UserId == member.UserId,
                cancellationToken);

        if (existingMembership?.IsActive == true)
        {
            throw new InvalidOperationException("User is already a member of this workspace.");
        }

        if (existingMembership != null)
        {
            existingMembership.Role = member.Role;
            existingMembership.QuotaMode = member.QuotaMode;
            existingMembership.CreditLimit = member.CreditLimit;
            existingMembership.CreditUsed = member.CreditUsed;
            existingMembership.CreditPeriodStart = member.CreditPeriodStart;
            existingMembership.JoinedAt = DateTime.UtcNow;
            existingMembership.IsActive = true;

            await _context.SaveChangesAsync(cancellationToken);
            return existingMembership;
        }

        member.JoinedAt = DateTime.UtcNow;
        _context.WorkspaceMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
        return member;
    }

    public async Task UpdateAsync(WorkspaceMember member, CancellationToken cancellationToken = default)
    {
        var persistedRole = await _context.WorkspaceMembers
            .AsNoTracking()
            .Where(existing => existing.Id == member.Id)
            .Select(existing => (WorkspaceMemberRoleEnum?)existing.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (persistedRole == WorkspaceMemberRoleEnum.Owner && member.Role != WorkspaceMemberRoleEnum.Owner)
        {
            throw new InvalidOperationException("Workspace owner role cannot be changed. Transfer ownership first.");
        }

        if (persistedRole != WorkspaceMemberRoleEnum.Owner && member.Role == WorkspaceMemberRoleEnum.Owner)
        {
            throw new InvalidOperationException("Use ownership transfer to change the workspace owner.");
        }

        _context.WorkspaceMembers.Update(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(existing => existing.Id == id && existing.IsActive, cancellationToken);

        if (member == null)
        {
            return false;
        }

        if (member.Role == WorkspaceMemberRoleEnum.Owner)
        {
            throw new InvalidOperationException("Workspace owner cannot be removed. Transfer ownership first.");
        }

        member.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .AnyAsync(member =>
                member.WorkspaceId == workspaceId &&
                member.UserId == userId &&
                member.IsActive,
                cancellationToken);
    }

    private IQueryable<WorkspaceMember> Query()
    {
        return _context.WorkspaceMembers
            .Include(member => member.Workspace)
            .Include(member => member.User);
    }
}
