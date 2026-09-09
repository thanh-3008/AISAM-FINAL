import React from "react";
import { beforeEach, afterEach, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import BrandAccessPage from "@/app/(dashboard)/brands/[id]/access/page";
import { changeAssignment, readAssignments } from "@/services/permissionService";
vi.mock("next/navigation", () => ({ useParams: () => ({ id: "brand" }) }));
vi.mock("next/link", () => ({ default: ({ children }: { children: React.ReactNode }) => <span>{children}</span> }));
vi.mock("@/hooks/useResourcePermissions", () => ({ useResourcePermissions: () => () => true }));
vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn(async () => ({ data: { items: [{ id: "team", name: "Content team" }], totalCount: 1 } })) }));
vi.mock("@/services/socialAccountService", () => ({ fetchSocialIntegrations: vi.fn(async () => [{ id: "channel", accountName: "Page", provider: "facebook" }]) }));
vi.mock("@/services/permissionService", () => ({ Kind: { Brand: 1 }, Permission: { BrandManage: 1 }, readAssignments: vi.fn(), changeAssignment: vi.fn() }));
beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(readAssignments).mockResolvedValue({ revision: "r1", teams: [{ id: "assignment", teamId: "team", isActive: true }], channels: [] });
});
afterEach(cleanup);
it("requires view before publish and sends the current revision", async () => {
  vi.mocked(changeAssignment).mockResolvedValue({ revision: "r2", teams: [{ id: "assignment", teamId: "team", isActive: true }], channels: [{ teamBrandId: "assignment", integrationId: "channel", canView: true, canPublish: false, canManage: false }] });
  render(<BrandAccessPage />);
  const view = await screen.findByRole("checkbox", { name: "Xem" });
  expect((screen.getByRole("checkbox", { name: "Đăng bài" }) as HTMLInputElement).disabled).toBe(true);
  fireEvent.click(view);
  await waitFor(() => expect(changeAssignment).toHaveBeenCalledWith("brand", "team", "r1", true, { id: "channel", canView: true, canPublish: false, canManage: false }));
  await waitFor(() => expect((screen.getByRole("checkbox", { name: "Đăng bài" }) as HTMLInputElement).disabled).toBe(false));
});
it("shows a conflict and removes stale controls until explicit reload", async () => {
  vi.mocked(changeAssignment).mockRejectedValue(Object.assign(new Error("Quyền đã thay đổi. Tải lại."), { status: 409 }));
  render(<BrandAccessPage />);
  fireEvent.click(await screen.findByRole("checkbox", { name: "Xem" }));
  expect((await screen.findByRole("alert")).textContent).toContain("Tải lại");
  expect(screen.queryByRole("checkbox", { name: "Xem" })).toBeNull();
  expect(changeAssignment).toHaveBeenCalledTimes(1);
  fireEvent.click(screen.getByRole("button", { name: "Tải lại quyền" }));
  await screen.findByRole("checkbox", { name: "Xem" });
  expect(readAssignments).toHaveBeenCalledTimes(2);
});
