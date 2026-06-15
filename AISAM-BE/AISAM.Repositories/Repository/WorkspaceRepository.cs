using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class WorkspaceRepository : IWorkspaceRepository
{
    private readonly AisamContext _context;

    public WorkspaceRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(workspace =>
                workspace.Id == id &&
                workspace.Status != WorkspaceStatusEnum.Deleted,
                cancellationToken);
    }

    public async Task<Workspace?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(workspace => workspace.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(workspace =>
                workspace.Status != WorkspaceStatusEnum.Deleted &&
                workspace.Members.Any(member => member.UserId == userId && member.IsActive))
            .OrderByDescending(workspace => workspace.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        workspace.CreatedAt = utcNow;
        workspace.UpdatedAt = utcNow;

        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync(cancellationToken);
        return workspace;
    }

    public async Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        workspace.UpdatedAt = DateTime.UtcNow;
        _context.Workspaces.Update(workspace);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .AnyAsync(workspace =>
                workspace.Id == id &&
                workspace.Status != WorkspaceStatusEnum.Deleted,
                cancellationToken);
    }

    private IQueryable<Workspace> Query()
    {
        return _context.Workspaces
            .Include(workspace => workspace.Members)
            .ThenInclude(member => member.User);
    }
}
