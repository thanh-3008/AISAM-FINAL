"use client";

import { useState, useEffect } from "react";

const STORAGE_KEY = "aisam_autosave_enabled";

export function getStoredAutosave(): boolean {
  if (typeof window === "undefined") return true; // Default to true
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw !== null ? JSON.parse(raw) : true;
  } catch {
    return true;
  }
}

export function setStoredAutosave(enabled: boolean): void {
  if (typeof window === "undefined") return;
  localStorage.setItem(STORAGE_KEY, JSON.stringify(enabled));
  window.dispatchEvent(new Event("autosave_changed"));
}

export function useSettings() {
  const [autosaveEnabled, setAutosaveEnabled] = useState<boolean>(true);

  useEffect(() => {
    setAutosaveEnabled(getStoredAutosave());

    const handleStorageChange = () => {
      setAutosaveEnabled(getStoredAutosave());
    };

    window.addEventListener("autosave_changed", handleStorageChange);
    window.addEventListener("storage", handleStorageChange); // Cross-tab support

    return () => {
      window.removeEventListener("autosave_changed", handleStorageChange);
      window.removeEventListener("storage", handleStorageChange);
    };
  }, []);

  const toggleAutosave = (enabled: boolean) => {
    setAutosaveEnabled(enabled);
    setStoredAutosave(enabled);
  };

  return { autosaveEnabled, toggleAutosave };
}
