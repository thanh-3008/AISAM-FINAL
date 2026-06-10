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
        var membershipExists = await _context.WorkspaceMembers
            .AnyAsync(existing =>
                existing.WorkspaceId == member.WorkspaceId &&
                existing.UserId == member.UserId,
                cancellationToken);

        if (membershipExists)
        {
            throw new InvalidOperationException("User is already a member of this workspace.");
        }

        member.JoinedAt = DateTime.UtcNow;
        _context.WorkspaceMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
        return member;
    }

    public async Task UpdateAsync(WorkspaceMember member, CancellationToken cancellationToken = default)
    {
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
