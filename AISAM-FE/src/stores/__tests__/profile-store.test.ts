import { describe, expect, it, beforeEach, afterEach, vi } from "vitest";
import { getStoredActiveProfile, storeActiveProfile, clearActiveProfile, type ActiveProfile } from "../profile-store";

describe("profile-store", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it("getStoredActiveProfile returns null if nothing is stored", () => {
    expect(getStoredActiveProfile()).toBeNull();
  });

  it("getStoredActiveProfile returns parsed profile if stored", () => {
    const mockProfile: ActiveProfile = { id: "1", name: "Test Profile", profileType: 1 };
    localStorage.setItem("aisam_active_profile", JSON.stringify(mockProfile));

    const result = getStoredActiveProfile();
    expect(result).toEqual(mockProfile);
  });

  it("getStoredActiveProfile returns null on invalid JSON", () => {
    localStorage.setItem("aisam_active_profile", "invalid-json");
    expect(getStoredActiveProfile()).toBeNull();
  });

  it("storeActiveProfile saves profile to localStorage", () => {
    const mockProfile: ActiveProfile = { id: "2", name: "New Profile", profileType: 2 };
    storeActiveProfile(mockProfile);

    const storedStr = localStorage.getItem("aisam_active_profile");
    expect(storedStr).not.toBeNull();
    expect(JSON.parse(storedStr!)).toEqual(mockProfile);
  });

  it("clearActiveProfile removes profile from localStorage", () => {
    localStorage.setItem("aisam_active_profile", JSON.stringify({ id: "3", name: "To Delete", profileType: 1 }));
    
    clearActiveProfile();
    
    expect(localStorage.getItem("aisam_active_profile")).toBeNull();
  });
});
