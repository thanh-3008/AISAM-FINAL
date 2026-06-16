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
    return raw ? JSON.parse(raw) : null;
  } catch {
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
