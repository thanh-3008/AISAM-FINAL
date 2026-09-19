/**
 * Standard Role Labels for AISAM (RBAC v2 & Workspace / Team layers)
 * Single source of truth for role display labels across the frontend.
 */

export const WORKSPACE_MANAGER_LABEL = "Workspace Manager";

export const WORKSPACE_ROLE_LABELS: Record<string, string> = {
  Owner: "Owner",
  WorkspaceManager: WORKSPACE_MANAGER_LABEL,
  Member: "Member",
};

export const TEAM_ROLE_LABELS: Record<string, string> = {
  Manager: "Team Manager",
  ContentCreator: "Content Creator",
  Viewer: "Viewer",
};

/**
 * Returns human-readable label for a workspace role.
 * Unknown or undefined roles return the raw value (or empty string) with a dev warning,
 * preventing silent fallback to arbitrary roles.
 */
export function getWorkspaceRoleLabel(role?: string | null): string {
  if (!role) return "";
  const label = WORKSPACE_ROLE_LABELS[role];
  if (label) return label;
  if (process.env.NODE_ENV !== "production") {
    console.warn(`[roleLabels] Unknown workspace role encountered: "${role}"`);
  }
  return role;
}

/**
 * Returns human-readable label for a team role.
 */
export function getTeamRoleLabel(role?: string | null): string {
  if (!role) return "";
  const label = TEAM_ROLE_LABELS[role];
  if (label) return label;
  if (process.env.NODE_ENV !== "production") {
    console.warn(`[roleLabels] Unknown team role encountered: "${role}"`);
  }
  return role;
}

/**
 * Returns human-readable label for either a workspace role or a team role.
 */
export function getRoleLabel(role?: string | null): string {
  if (!role) return "";
  if (role in WORKSPACE_ROLE_LABELS) return WORKSPACE_ROLE_LABELS[role];
  if (role in TEAM_ROLE_LABELS) return TEAM_ROLE_LABELS[role];
  if (process.env.NODE_ENV !== "production") {
    console.warn(`[roleLabels] Unknown role encountered: "${role}"`);
  }
  return role;
}

