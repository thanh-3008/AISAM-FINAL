using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    private void RequireAccountingWorkspace(Guid workspaceId)
    {
        if (workspaceId == Guid.Empty || AccessScope.Enforced && workspaceId != AccessScope.WorkspaceId)
            throw new UnauthorizedAccessException("Accounting workspace is not authorized.");
    }

    // Accounting counts the whole workspace, independent of the actor's resource visibility.
    // Every filter bypass immediately restores the explicit workspace boundary.
    public IQueryable<Content> WorkspaceUsageContents(Guid workspaceId)
    {
        RequireAccountingWorkspace(workspaceId);
        return Contents.IgnoreQueryFilters().Where(c => c.WorkspaceId == workspaceId);
    }

    public IQueryable<AiGeneration> WorkspaceUsageGenerations(Guid workspaceId)
    {
        RequireAccountingWorkspace(workspaceId);
        return AiGenerations.IgnoreQueryFilters().Where(g => g.Content.WorkspaceId == workspaceId);
    }

    public IQueryable<Post> WorkspaceUsagePosts(Guid workspaceId)
    {
        RequireAccountingWorkspace(workspaceId);
        return Posts.IgnoreQueryFilters().Where(p => p.Content.WorkspaceId == workspaceId);
    }
}
