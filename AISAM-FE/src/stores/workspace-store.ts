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
    if (!raw) return null;

    const parsed = JSON.parse(raw) as Partial<ActiveWorkspace>;
    if (!parsed.id || !parsed.name || typeof parsed.workspaceType !== "number") {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }

    return {
      id: parsed.id,
      name: parsed.name,
      workspaceType: parsed.workspaceType,
    };
  } catch {
    localStorage.removeItem(STORAGE_KEY);
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
