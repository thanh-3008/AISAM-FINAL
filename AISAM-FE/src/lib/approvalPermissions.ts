import type { RbacContextValue } from "@/contexts/RbacContext";

export function canReviewContentWithRbac(
  rbac: RbacContextValue,
  teamId?: string | null,
): boolean {
  if (rbac.workspaceRole === "Owner" || rbac.workspaceRole === "WorkspaceManager") return true;
  if (!teamId) return false;
  return rbac.teams.some((team) => team.teamId === teamId && team.role === "Manager");
}
