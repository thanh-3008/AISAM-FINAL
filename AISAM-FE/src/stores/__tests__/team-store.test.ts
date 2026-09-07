import { describe, expect, it, beforeEach } from "vitest";
import { clearActiveTeam, getStoredActiveTeam, storeActiveTeam } from "@/stores/team-store";

describe("team-store", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("returns null when no team stored", () => {
    expect(getStoredActiveTeam()).toBeNull();
  });

  it("stores and retrieves active team", () => {
    storeActiveTeam({
      id: "11111111-1111-1111-1111-111111111111",
      name: "Engineering Team",
      workspaceId: "22222222-2222-2222-2222-222222222222",
    });

    const team = getStoredActiveTeam();
    expect(team).toEqual({
      id: "11111111-1111-1111-1111-111111111111",
      name: "Engineering Team",
      workspaceId: "22222222-2222-2222-2222-222222222222",
    });
  });

  it("filters by expectedWorkspaceId and returns null if mismatched", () => {
    storeActiveTeam({
      id: "11111111-1111-1111-1111-111111111111",
      name: "Engineering Team",
      workspaceId: "22222222-2222-2222-2222-222222222222",
    });

    expect(getStoredActiveTeam("22222222-2222-2222-2222-222222222222")).not.toBeNull();
    expect(getStoredActiveTeam("33333333-3333-3333-3333-333333333333")).toBeNull();
  });

  it("clears stored active team", () => {
    storeActiveTeam({
      id: "11111111-1111-1111-1111-111111111111",
      name: "Engineering Team",
    });
    clearActiveTeam();
    expect(getStoredActiveTeam()).toBeNull();
  });
});
