import { describe, expect, it, vi } from "vitest";
import {
  WORKSPACE_MANAGER_LABEL,
  WORKSPACE_ROLE_LABELS,
  TEAM_ROLE_LABELS,
  getWorkspaceRoleLabel,
  getTeamRoleLabel,
  getRoleLabel,
} from "@/lib/roleLabels";

describe("roleLabels", () => {
  describe("constants", () => {
    it("defines WORKSPACE_MANAGER_LABEL as 'Workspace Manager'", () => {
      expect(WORKSPACE_MANAGER_LABEL).toBe("Workspace Manager");
    });

    it("defines correct workspace role labels per D2", () => {
      expect(WORKSPACE_ROLE_LABELS).toEqual({
        Owner: "Owner",
        WorkspaceManager: "Workspace Manager",
        Member: "Member",
      });
    });

    it("defines correct team role labels per D2", () => {
      expect(TEAM_ROLE_LABELS).toEqual({
        Manager: "Team Manager",
        ContentCreator: "Content Creator",
        Viewer: "Viewer",
      });
    });
  });

  describe("getWorkspaceRoleLabel", () => {
    it("maps Owner to 'Owner'", () => {
      expect(getWorkspaceRoleLabel("Owner")).toBe("Owner");
    });

    it("maps WorkspaceManager to 'Workspace Manager'", () => {
      expect(getWorkspaceRoleLabel("WorkspaceManager")).toBe("Workspace Manager");
    });

    it("maps Member to 'Member'", () => {
      expect(getWorkspaceRoleLabel("Member")).toBe("Member");
    });

    it("returns empty string for undefined and null without crashing", () => {
      expect(getWorkspaceRoleLabel(undefined)).toBe("");
      expect(getWorkspaceRoleLabel(null)).toBe("");
      expect(getWorkspaceRoleLabel("")).toBe("");
    });

    it("returns raw value and logs a warning in dev for unknown role", () => {
      const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});
      const result = getWorkspaceRoleLabel("SuperAdmin");
      expect(result).toBe("SuperAdmin");
      expect(warnSpy).toHaveBeenCalledWith(
        expect.stringContaining('[roleLabels] Unknown workspace role encountered: "SuperAdmin"')
      );
      warnSpy.mockRestore();
    });
  });

  describe("getTeamRoleLabel", () => {
    it("maps Manager to 'Team Manager'", () => {
      expect(getTeamRoleLabel("Manager")).toBe("Team Manager");
    });

    it("maps ContentCreator to 'Content Creator'", () => {
      expect(getTeamRoleLabel("ContentCreator")).toBe("Content Creator");
    });

    it("maps Viewer to 'Viewer'", () => {
      expect(getTeamRoleLabel("Viewer")).toBe("Viewer");
    });

    it("returns empty string for undefined and null without crashing", () => {
      expect(getTeamRoleLabel(undefined)).toBe("");
      expect(getTeamRoleLabel(null)).toBe("");
      expect(getTeamRoleLabel("")).toBe("");
    });

    it("returns raw value and logs a warning in dev for unknown role", () => {
      const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});
      const result = getTeamRoleLabel("Guest");
      expect(result).toBe("Guest");
      expect(warnSpy).toHaveBeenCalledWith(
        expect.stringContaining('[roleLabels] Unknown team role encountered: "Guest"')
      );
      warnSpy.mockRestore();
    });
  });

  describe("getRoleLabel", () => {
    it("maps both workspace and team roles correctly", () => {
      expect(getRoleLabel("Owner")).toBe("Owner");
      expect(getRoleLabel("WorkspaceManager")).toBe("Workspace Manager");
      expect(getRoleLabel("Member")).toBe("Member");
      expect(getRoleLabel("Manager")).toBe("Team Manager");
      expect(getRoleLabel("ContentCreator")).toBe("Content Creator");
      expect(getRoleLabel("Viewer")).toBe("Viewer");
    });

    it("returns empty string for undefined and null", () => {
      expect(getRoleLabel(undefined)).toBe("");
      expect(getRoleLabel(null)).toBe("");
      expect(getRoleLabel("")).toBe("");
    });
  });

  describe("Display priority: workspaceRole vs legacy fallback", () => {
    it("resolves to Workspace Manager when workspaceRole is 'WorkspaceManager' regardless of legacy currentUserRole", () => {
      const payload = { currentUserRole: 2, workspaceRole: "WorkspaceManager" };
      const displayRole = payload.workspaceRole
        ? getWorkspaceRoleLabel(payload.workspaceRole)
        : "Legacy Fallback";
      expect(displayRole).toBe("Workspace Manager");
    });

    it("preserves legacy behavior when workspaceRole is null/undefined", () => {
      const payloadNull = { currentUserRole: 2, workspaceRole: null };
      const displayRoleNull = payloadNull.workspaceRole
        ? getWorkspaceRoleLabel(payloadNull.workspaceRole)
        : (payloadNull.currentUserRole === 2 ? "Manager" : "Member");
      expect(displayRoleNull).toBe("Manager");

      const payloadUndefined = { currentUserRole: 0, workspaceRole: undefined };
      const displayRoleUndefined = payloadUndefined.workspaceRole
        ? getWorkspaceRoleLabel(payloadUndefined.workspaceRole)
        : (payloadUndefined.currentUserRole === 0 ? "Owner" : "Member");
      expect(displayRoleUndefined).toBe("Owner");
    });
  });
});
