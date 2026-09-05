import { afterEach, describe, expect, it, vi } from "vitest";
import { act, cleanup, render, screen } from "@testing-library/react";
import { AccessProvider } from "../AccessContext";
import { notifyAccessChanged } from "@/lib/accessEvents";

const state = vi.hoisted(() => ({ path: "/analytics", role: "Viewer", workspaceId: "workspace-a" }));
vi.mock("next/navigation", () => ({ usePathname: () => state.path }));
vi.mock("next/link", () => ({ default: ({ children }: { children: React.ReactNode }) => <span>{children}</span> }));
vi.mock("@/hooks/useWorkspaces", () => ({ useWorkspaces: () => ({ activeWorkspace: { id: state.workspaceId } }) }));
vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn(async () => ({ success: true, data: {
  workspaceId: state.workspaceId, userId: "user-a", role: state.role, version: state.role,
  canViewAnalytics: state.role === "Owner" || state.role === "Manager",
  canViewOwnAnalytics: state.role !== "Viewer", teamIds: [],
} })) }));

afterEach(() => { cleanup(); state.path = "/analytics"; state.role = "Viewer"; state.workspaceId = "workspace-a"; });

describe("current backend access context", () => {
  it("rechecks another device's downgrade when the window regains focus", async () => {
    state.role = "Manager";
    render(<AccessProvider><div>protected analytics</div></AccessProvider>);
    await screen.findByText("protected analytics");
    state.role = "Viewer";
    await act(async () => window.dispatchEvent(new Event("focus")));
    await screen.findByRole("alert");
    expect(screen.queryByText("protected analytics")).toBeNull();
  });

  it("does not carry Owner analytics into a Viewer workspace", async () => {
    state.role = "Owner";
    const view = render(<AccessProvider><div>protected analytics</div></AccessProvider>);
    await screen.findByText("protected analytics");
    state.workspaceId = "workspace-b"; state.role = "Viewer";
    view.rerender(<AccessProvider><div>protected analytics</div></AccessProvider>);
    await screen.findByRole("alert");
    expect(screen.queryByText("protected analytics")).toBeNull();
  });
  it("does not mount protected analytics for Viewer", async () => {
    render(<AccessProvider><div>protected analytics</div></AccessProvider>);
    await screen.findByRole("alert");
    expect(screen.queryByText("protected analytics")).toBeNull();
  });

  it("allows Creator personal analytics", async () => {
    state.role = "ContentCreator"; state.path = "/own-analytics";
    render(<AccessProvider><div>personal analytics</div></AccessProvider>);
    expect(await screen.findByText("personal analytics")).toBeTruthy();
  });

  it("clears mounted protected data on 403 without navigating to login", async () => {
    state.role = "Manager";
    render(<AccessProvider><div>protected analytics</div></AccessProvider>);
    await screen.findByText("protected analytics");
    await act(async () => notifyAccessChanged("denied"));
    expect(screen.queryByText("protected analytics")).toBeNull();
    expect(screen.getByRole("alert")).toBeTruthy();
  });

  it("uses downgraded server role after access invalidation", async () => {
    state.role = "Owner";
    render(<AccessProvider><div>protected analytics</div></AccessProvider>);
    await screen.findByText("protected analytics");
    state.role = "Viewer";
    await act(async () => notifyAccessChanged());
    await screen.findByRole("alert");
    expect(screen.queryByText("protected analytics")).toBeNull();
  });
});
