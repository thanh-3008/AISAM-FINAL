import { describe, expect, it, beforeEach } from "vitest";
import { clearActiveWorkspace, getStoredActiveWorkspace } from "@/stores/workspace-store";

describe("workspace-store migration", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("does not blindly promote legacy profile id into workspace state", () => {
    localStorage.setItem("aisam_active_profile", JSON.stringify({
      id: "11111111-1111-1111-1111-111111111111",
      name: "Legacy Profile",
      profileType: 2,
    }));

    const workspace = getStoredActiveWorkspace();

    expect(workspace).toBeNull();
  });

  it("returns normalized workspace when workspace storage is already valid", () => {
    localStorage.setItem("aisam_active_workspace", JSON.stringify({
      id: "22222222-2222-2222-2222-222222222222",
      name: "Main Workspace",
      workspaceType: 2,
    }));

    expect(getStoredActiveWorkspace()).toEqual({
      id: "22222222-2222-2222-2222-222222222222",
      name: "Main Workspace",
      workspaceType: 2,
    });
  });
});
