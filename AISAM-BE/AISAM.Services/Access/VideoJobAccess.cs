using AISAM.Repositories;
using AISAM.Data.Enumeration;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Access;

public static class VideoJobAccess
{
    // Standalone jobs have no Content/Team. Members must use the content-scoped flow.
    public static async Task<bool> CanRunAsync(AisamContext db, IAccessControlService? access, Guid actor, Guid workspace, CancellationToken ct)
    {
        if (access is not RbacV2AccessAdapter) return true;
        var context=await new RbacV2AccessResolver(db).ContextAsync(actor,workspace,ct);
        if (context?.WorkspaceRole is not ("Owner" or "WorkspaceManager")) return false;
        var state=await db.Workspaces.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(w=>w.Id==workspace,ct);
        if (state is null) return false;
        WorkspaceLifecyclePolicy.SynchronizeStatus(state,DateTime.UtcNow);
        return !WorkspaceLifecyclePolicy.IsReadOnly(state.Status);
    }
}
