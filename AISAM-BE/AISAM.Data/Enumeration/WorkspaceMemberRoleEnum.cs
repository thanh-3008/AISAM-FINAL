using System;

namespace AISAM.Data.Enumeration
{
    public enum WorkspaceMemberRoleEnum
    {
        Owner = 1,
        WorkspaceManager = 2,
        Member = 3,

        // Legacy compatibility aliases for transition phase (R-06 safe mapping)
        [Obsolete("Use WorkspaceManager. Scheduled for removal in Phase 4.")]
        Manager = 2,
        [Obsolete("Use Member for workspace scope and TeamRoleEnum.ContentCreator for team scope.")]
        ContentCreator = 3,
        [Obsolete("Use Member for workspace scope and TeamRoleEnum.Viewer for team scope.")]
        Viewer = 4
    }
}
