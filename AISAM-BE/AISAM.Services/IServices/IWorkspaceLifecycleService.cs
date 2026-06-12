using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Services.IServices;

public interface IWorkspaceLifecycleService
{
    WorkspaceLifecycleState ResolveState(Workspace workspace, DateTime? utcNow = null);
    bool TrySynchronizePersistenceState(Workspace workspace, DateTime? utcNow = null);
}
