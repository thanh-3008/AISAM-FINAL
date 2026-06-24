"use client";

export interface ActiveProfile {
  id: string;
  name: string;
  profileType: number;
}

const STORAGE_KEY = "aisam_active_profile";

export function getStoredActiveProfile(): ActiveProfile | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const parsed = JSON.parse(raw) as Partial<ActiveProfile>;
    if (!parsed.id || !parsed.name || typeof parsed.profileType !== "number") {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }

    return {
      id: parsed.id,
      name: parsed.name,
      profileType: parsed.profileType,
    };
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

export function storeActiveProfile(profile: ActiveProfile): void {
  if (typeof window === "undefined") return;
  localStorage.setItem(STORAGE_KEY, JSON.stringify(profile));
}

export function clearActiveProfile(): void {
  if (typeof window === "undefined") return;
  localStorage.removeItem(STORAGE_KEY);
}
