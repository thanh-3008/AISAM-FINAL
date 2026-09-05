import { afterEach, describe, expect, it, vi } from "vitest";
import { act, cleanup, render, screen } from "@testing-library/react";
import { AccessProvider } from "../AccessContext";
import { notifyAccessChanged } from "@/lib/accessEvents";
import { apiClient } from "@/lib/apiClient";

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

  it("allows Creator to access /analytics for personal history", async () => {
    state.role = "ContentCreator"; state.path = "/analytics";
    render(<AccessProvider><div>personal analytics on analytics route</div></AccessProvider>);
    expect(await screen.findByText("personal analytics on analytics route")).toBeTruthy();
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

  it("displays connection error screen with retry button when /access/context fails with 500", async () => {
    state.role = "Manager";
    const err500 = new Error("Internal Server Error") as Error & { status?: number };
    err500.status = 500;
    vi.mocked(apiClient).mockRejectedValueOnce(err500);

    render(<AccessProvider><div>protected analytics</div></AccessProvider>);

    const errorMsg = await screen.findByText("Không thể kết nối đến máy chủ xác minh quyền truy cập.");
    expect(errorMsg).toBeTruthy();
    const retryBtn = screen.getByRole("button", { name: "Thử lại" });
    expect(retryBtn).toBeTruthy();
    expect(screen.queryByText("Đang xác minh quyền truy cập…")).toBeNull();
    expect(screen.queryByText("protected analytics")).toBeNull();

    await act(async () => {
      retryBtn.click();
    });
    expect(await screen.findByText("protected analytics")).toBeTruthy();
    expect(screen.queryByText("Không thể kết nối đến máy chủ xác minh quyền truy cập.")).toBeNull();
  });

  it("displays connection error screen with retry button when network fails", async () => {
    state.role = "Manager";
    vi.mocked(apiClient).mockRejectedValueOnce(new TypeError("Failed to fetch"));

    render(<AccessProvider><div>protected analytics</div></AccessProvider>);

    expect(await screen.findByText("Không thể kết nối đến máy chủ xác minh quyền truy cập.")).toBeTruthy();
    expect(screen.getByRole("button", { name: "Thử lại" })).toBeTruthy();
    expect(screen.queryByText("Đang xác minh quyền truy cập…")).toBeNull();
    expect(screen.queryByText("protected analytics")).toBeNull();
  });

  it("displays access denied message without retry button when /access/context returns 403", async () => {
    state.role = "Manager";
    const err403 = new Error("Forbidden") as Error & { status?: number };
    err403.status = 403;
    vi.mocked(apiClient).mockRejectedValueOnce(err403);

    render(<AccessProvider><div>protected analytics</div></AccessProvider>);

    expect(await screen.findByText("Quyền truy cập đã thay đổi. Vui lòng chọn trang khác.")).toBeTruthy();
    expect(screen.queryByText("Không thể kết nối đến máy chủ xác minh quyền truy cập.")).toBeNull();
    expect(screen.queryByRole("button", { name: "Thử lại" })).toBeNull();
    expect(screen.queryByText("protected analytics")).toBeNull();
  });

  it("still renders public routes when /access/context fails with 500", async () => {
    state.path = "/overview";
    const err500 = new Error("Server Error") as Error & { status?: number };
    err500.status = 500;
    vi.mocked(apiClient).mockRejectedValueOnce(err500);

    render(<AccessProvider><div>public overview content</div></AccessProvider>);

    expect(await screen.findByText("public overview content")).toBeTruthy();
    expect(screen.queryByText("Không thể kết nối đến máy chủ xác minh quyền truy cập.")).toBeNull();
  });
});
