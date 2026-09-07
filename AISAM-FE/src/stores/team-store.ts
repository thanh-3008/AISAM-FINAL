"use client";

export interface ActiveTeam {
  id: string;
  name: string;
  workspaceId?: string;
}

const STORAGE_KEY = "aisam_active_team";

export function getStoredActiveTeam(expectedWorkspaceId?: string): ActiveTeam | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const parsed = JSON.parse(raw) as Partial<ActiveTeam>;
    if (!parsed.id || !parsed.name) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }

    if (expectedWorkspaceId && parsed.workspaceId && parsed.workspaceId !== expectedWorkspaceId) {
      return null;
    }

    return {
      id: parsed.id,
      name: parsed.name,
      workspaceId: parsed.workspaceId,
    };
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

export function storeActiveTeam(team: ActiveTeam): void {
  if (typeof window === "undefined") return;
  localStorage.setItem(STORAGE_KEY, JSON.stringify(team));
  try {
    window.dispatchEvent(new CustomEvent("aisam_active_team_changed", { detail: team }));
  } catch {
    // ignore in environments without CustomEvent
  }
}

export function clearActiveTeam(): void {
  if (typeof window === "undefined") return;
  localStorage.removeItem(STORAGE_KEY);
  try {
    window.dispatchEvent(new CustomEvent("aisam_active_team_changed", { detail: null }));
  } catch {
    // ignore in environments without CustomEvent
  }
}
