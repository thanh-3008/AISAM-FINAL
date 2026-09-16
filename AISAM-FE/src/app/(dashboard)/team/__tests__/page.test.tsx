import React from "react";
import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import TeamPage from "../page";
import { useRbac } from "@/contexts/RbacContext";

vi.mock("@/contexts/RbacContext", () => ({ useRbac: vi.fn() }));
vi.mock("@/components/team/RbacTeamManagement", () => ({
  default: () => <div>RBAC v2 team management</div>,
}));

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});

describe("TeamPage RBAC contract gate", () => {
  it("waits for the v2 permission context instead of rendering legacy roles", () => {
    vi.mocked(useRbac).mockReturnValue(null);

    render(<TeamPage />);

    expect(screen.getByText("Đang tải phân quyền hai tầng")).toBeTruthy();
    expect(screen.queryByText("Role Distribution")).toBeNull();
  });

  it("renders the two-layer management flow when the v2 context is ready", () => {
    vi.mocked(useRbac).mockReturnValue({
      contractVersion: 2,
      revision: "r1",
      workspaceRole: "Owner",
      actions: ["team.manage"],
      teams: [],
      scopes: [],
    });

    render(<TeamPage />);

    expect(screen.getByText("RBAC v2 team management")).toBeTruthy();
  });
});
