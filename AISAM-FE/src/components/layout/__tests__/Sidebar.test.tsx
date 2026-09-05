import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Sidebar from "../Sidebar";

vi.mock("next/navigation", () => ({
  usePathname: () => "/dashboard",
}));

vi.mock("@/hooks/useWorkspaces", () => ({
  useWorkspaces: () => ({
    workspaces: [{ id: "ws-1", name: "Test Workspace", workspaceType: 1 }],
    loading: false,
    activeWorkspace: { id: "ws-1", name: "Test Workspace", workspaceType: 1 },
    selectWorkspace: vi.fn(),
  }),
  getWorkspaceTypeLabel: () => "Pro",
}));

vi.mock("@/hooks/useFeatureGate", () => ({
  useFeatureGate: () => ({
    can: () => true,
    canAccess: () => true,
  }),
}));

vi.mock("@/contexts/SidebarContext", () => ({
  useSidebar: () => ({
    open: true,
    toggle: vi.fn(),
  }),
}));

vi.mock("@/contexts/AccessContext", () => ({
  useAccessContext: () => ({
    workspaceId: "ws-1",
    userId: "user-1",
    role: "ContentCreator",
    canViewAnalytics: false,
    canViewOwnAnalytics: true,
    canReviewContent: false,
    canPublish: false,
  }),
}));

describe("Sidebar Navigation", () => {
  it("does not render My Analytics & History button", () => {
    render(<Sidebar />);
    expect(screen.queryByText("My Analytics & History")).toBeNull();
  });

  it("renders Analysis menu item", () => {
    render(<Sidebar />);
    expect(screen.getByText("Analysis")).toBeTruthy();
  });
});
