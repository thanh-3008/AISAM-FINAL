namespace AISAM.Data.Enumeration;

// Separate storage from the legacy enum: legacy Manager=2 must never become WorkspaceManager.
public enum WorkspaceRoleV2
{
    Owner = 1,
    WorkspaceManager = 2,
    Member = 3
}
