import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import TeamPage from "@/app/(dashboard)/team/page";
import { apiClient } from "@/lib/apiClient";

vi.mock("@/contexts/RbacContext", () => ({
  useRbac: () => ({
    contractVersion: 2,
    revision: "r1",
    workspaceRole: "Owner",
    actions: ["team.manage"],
    teams: [],
    scopes: [],
  }),
}));

vi.mock("@/lib/apiClient", () => ({
  apiClient: vi.fn((endpoint: string) => {
    if (endpoint === "/teams/manage") {
      return Promise.resolve({ data: { items: [
        { id: "t1", name: "Marketing", description: "Social campaigns", status: "Active", memberCount: 1, brandCount: 2, createdAt: "2026-01-01" },
        { id: "t2", name: "Archive Team", description: "Old campaigns", status: "Inactive", memberCount: 0, brandCount: 0, createdAt: "2026-01-02" },
      ], totalCount: 2 } });
    }
    if (endpoint === "/workspace-members") {
      return Promise.resolve({ data: [
        { id: "m1", userId: "u1", fullName: "Alice Smith", email: "alice@example.com", workspaceRole: "Member" },
        { id: "m2", userId: "u2", fullName: "Bob Manager", email: "bob@example.com", workspaceRole: "WorkspaceManager" },
      ] });
    }
    if (endpoint === "/workspace-invitations") {
      return Promise.resolve({ data: [
        { id: "i1", email: "pending@example.com", workspaceRole: "Member", invitedByName: "Workspace Owner", createdAt: "2026-09-15T08:00:00Z", expiresAt: "2026-09-22T08:00:00Z" },
      ] });
    }
    if (endpoint === "/workspace-invitations/i1") return Promise.resolve({ success: true });
    if (endpoint === "/teams/t1") return Promise.resolve({ data: { id: "t1", name: "Marketing", description: "Social campaigns", status: "Active", members: [], brands: [] } });
    if (endpoint === "/teams/t2") return Promise.resolve({ data: { id: "t2", name: "Archive Team", description: "Old campaigns", status: "Inactive", members: [], brands: [] } });
    return Promise.reject(new Error(`Unexpected endpoint: ${endpoint}`));
  }),
}));

vi.mock("next/link", () => ({
  default: ({ href, children, ...props }: { href: string; children: React.ReactNode }) => (
    <a href={href} {...props}>{children}</a>
  ),
}));

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});

it("renders workspace and Team roles as separate RBAC v2 concepts", async () => {
  render(<TeamPage />);

  expect(await screen.findByText("Alice Smith")).toBeTruthy();
  expect(screen.getAllByText("Thành viên workspace").length).toBeGreaterThan(0);
  expect(screen.getAllByText("Thành viên").length).toBeGreaterThan(0);
  expect(screen.queryByText("Viewer")).toBeNull();

  const performanceLink = screen.getByRole("link", { name: /Hiệu suất thành viên/i });
  expect(performanceLink.getAttribute("href")).toBe("/team/performance");
  expect(screen.getByRole("button", { name: /Tạo Team/i })).toBeTruthy();
  expect(apiClient).toHaveBeenCalledWith("/teams/manage");
  expect(apiClient).toHaveBeenCalledWith("/workspace-members");
  expect(apiClient).toHaveBeenCalledWith("/workspace-invitations");
});

it("filters workspace members and Teams independently", async () => {
  render(<TeamPage />);
  expect(await screen.findByText("Alice Smith")).toBeTruthy();
  expect(await screen.findByRole("button", { name: "Marketing" })).toBeTruthy();

  fireEvent.change(screen.getByLabelText("Tìm thành viên workspace"), { target: { value: "bob" } });
  expect(screen.queryByText("alice@example.com")).toBeNull();
  expect(screen.getByText("bob@example.com")).toBeTruthy();

  fireEvent.change(screen.getByLabelText("Tìm thành viên workspace"), { target: { value: "" } });
  fireEvent.change(screen.getByLabelText("Lọc vai trò workspace"), { target: { value: "Member" } });
  expect(screen.getByText("alice@example.com")).toBeTruthy();
  expect(screen.queryByText("bob@example.com")).toBeNull();

  fireEvent.change(screen.getByLabelText("Tìm Team"), { target: { value: "archive" } });
  expect(screen.queryByRole("button", { name: "Marketing" })).toBeNull();
  expect(screen.getByRole("button", { name: "Archive Team" })).toBeTruthy();

  fireEvent.change(screen.getByLabelText("Tìm Team"), { target: { value: "" } });
  fireEvent.change(screen.getByLabelText("Lọc trạng thái Team"), { target: { value: "Active" } });
  expect(screen.getByRole("button", { name: "Marketing" })).toBeTruthy();
  expect(screen.queryByRole("button", { name: "Archive Team" })).toBeNull();
});

it("shows pending invitations and allows the workspace owner to cancel one", async () => {
  vi.spyOn(window, "confirm").mockReturnValue(true);
  render(<TeamPage />);

  expect(await screen.findByText("pending@example.com")).toBeTruthy();
  fireEvent.click(screen.getByRole("button", { name: "Hủy lời mời pending@example.com" }));

  await waitFor(() => expect(apiClient).toHaveBeenCalledWith(
    "/workspace-invitations/i1",
    { method: "DELETE" },
  ));
});
