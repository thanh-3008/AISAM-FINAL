"use client";
import { createContext, useContext } from "react";

export type WorkspaceRoleV2 = "Owner" | "WorkspaceManager" | "Member";
export type TeamRoleV2 = "Manager" | "ContentCreator" | "Viewer";
export interface RbacContextValue {
  contractVersion: 2;
  revision: string;
  workspaceRole: WorkspaceRoleV2;
  actions: string[];
  teams: { teamId: string; role: TeamRoleV2 | null }[];
  scopes: { teamId: string; brandId: string; role: TeamRoleV2 | null; channelIds: string[] }[];
}
export const RbacContext = createContext<RbacContextValue | null>(null);
export const useRbac = () => useContext(RbacContext);
export function parseRbacContext(value: unknown): RbacContextValue {
  if (!value || typeof value !== "object") throw new Error("Không đọc được quyền truy cập.");
  const v = value as RbacContextValue;
  // RBAC v2 is now the active application contract. Silently returning null
  // here used to make consumers fall back to the legacy role model. That
  // mixed workspace roles with Team roles and made a v2 Member appear as a
  // legacy Viewer.
  if (v.contractVersion === undefined && typeof v.revision === "string" && !("workspaceRole" in v)) {
    throw new Error("Backend chưa bật RBAC v2. Vui lòng khởi động lại API với Rbac:UseV2=true.");
  }
  if (v.contractVersion !== 2 || !["Owner", "WorkspaceManager", "Member"].includes(v.workspaceRole) ||
      typeof v.revision !== "string" || !Array.isArray(v.actions) || !Array.isArray(v.teams) || !Array.isArray(v.scopes))
    throw new Error("Phiên bản phân quyền chưa được hỗ trợ.");
  // ASP.NET omits nullable properties when its JSON null-ignore option is
  // active. For an Owner/WorkspaceManager, a workspace-wide scope therefore
  // arrives without `role`; that has the same meaning as `role: null`.
  const validRole = (role: unknown) => role == null || ["Manager", "ContentCreator", "Viewer"].includes(role as string);
  if (!v.actions.every(a => typeof a === "string") ||
      !v.teams.every(t => t && typeof t.teamId === "string" && validRole(t.role)) ||
      !v.scopes.every(s => s && typeof s.teamId === "string" && typeof s.brandId === "string" && validRole(s.role) && Array.isArray(s.channelIds) && s.channelIds.every(c => typeof c === "string")))
    throw new Error("Dữ liệu quyền truy cập không hợp lệ.");
  return {
    ...v,
    teams: v.teams.map(t => ({ ...t, role: t.role ?? null })),
    scopes: v.scopes.map(s => ({ ...s, role: s.role ?? null })),
  };
}
