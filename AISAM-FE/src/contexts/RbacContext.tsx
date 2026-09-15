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
export function parseRbacContext(value: unknown): RbacContextValue | null {
  if (!value || typeof value !== "object") throw new Error("Không đọc được quyền truy cập.");
  const v = value as RbacContextValue;
  if (v.contractVersion === undefined && typeof v.revision === "string" && !('workspaceRole' in v)) return null; // legacy backend
  if (v.contractVersion !== 2 || !["Owner", "WorkspaceManager", "Member"].includes(v.workspaceRole) ||
      typeof v.revision !== "string" || !Array.isArray(v.actions) || !Array.isArray(v.teams) || !Array.isArray(v.scopes))
    throw new Error("Phiên bản phân quyền chưa được hỗ trợ.");
  const validRole = (role: unknown) => role === null || ["Manager", "ContentCreator", "Viewer"].includes(role as string);
  if (!v.actions.every(a => typeof a === "string") ||
      !v.teams.every(t => t && typeof t.teamId === "string" && validRole(t.role)) ||
      !v.scopes.every(s => s && typeof s.teamId === "string" && typeof s.brandId === "string" && validRole(s.role) && Array.isArray(s.channelIds) && s.channelIds.every(c => typeof c === "string")))
    throw new Error("Dữ liệu quyền truy cập không hợp lệ.");
  return v;
}
