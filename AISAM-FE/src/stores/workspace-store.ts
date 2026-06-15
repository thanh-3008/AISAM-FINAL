"use client";

export interface ActiveWorkspace {
  id: string;
  name: string;
  workspaceType: number;
}

const STORAGE_KEY = "aisam_active_workspace";

export function getStoredActiveWorkspace(): ActiveWorkspace | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) return JSON.parse(raw);
    const legacy = localStorage.getItem("aisam_active_profile");
    if (legacy) {
      const legacyProfile = JSON.parse(legacy);
      const ws: ActiveWorkspace = { id: legacyProfile.id, name: legacyProfile.name, workspaceType: legacyProfile.profileType };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(ws));
      return ws;
    }
    return null;
  } catch {
    return null;
  }
}

export function storeActiveWorkspace(workspace: ActiveWorkspace): void {
  if (typeof window === "undefined") return;
  localStorage.setItem(STORAGE_KEY, JSON.stringify(workspace));
}

export function clearActiveWorkspace(): void {
  if (typeof window === "undefined") return;
  localStorage.removeItem(STORAGE_KEY);
}
