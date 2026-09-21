import React from "react";
import { afterEach, beforeEach, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import RbacBrandAccess from "@/components/brands/RbacBrandAccess";
import { RbacContext, type RbacContextValue } from "@/contexts/RbacContext";
import { apiClient } from "@/lib/apiClient";
vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn() }));
vi.mock("@/services/socialAccountService", () => ({ fetchSocialIntegrations: vi.fn(async () => [{ id: "channel", accountName: "Kênh A", isActive: true }]) }));
const snapshot = { revision: "r1", teams: [{ id: "link", teamId: "team", isActive: true }], channels: [] };
const context: RbacContextValue = { contractVersion: 2, revision: "r1", workspaceRole: "WorkspaceManager", actions: ["brand.manage"], teams: [], scopes: [] };
beforeEach(() => { vi.mocked(apiClient).mockReset(); vi.mocked(apiClient).mockImplementation(async path => ({ data: path.includes("/access") ? snapshot : { items: [
  { id: "team", name: "Team A", status: "Active" },
  { id: "inactive-team", name: "Stopped Team", status: "Inactive" }
] } })); });
afterEach(cleanup);
it("stages channel changes and sends one batch only after Save", async () => {
  render(<RbacContext.Provider value={context}><RbacBrandAccess brandId="brand" /></RbacContext.Provider>);
  const checkbox = await screen.findByRole("checkbox", { name: "Kênh A" });
  expect(screen.queryByText("Stopped Team")).toBeNull();
  const initialCalls = vi.mocked(apiClient).mock.calls.length;
  fireEvent.click(checkbox);
  expect((checkbox as HTMLInputElement).checked).toBe(true);
  expect(apiClient).toHaveBeenCalledTimes(initialCalls);
  const saveButton = screen.getByRole("button", { name: "Save changes" });
  expect(saveButton).toBeTruthy();
  fireEvent.click(saveButton);
  await waitFor(() => expect(apiClient).toHaveBeenLastCalledWith("/brands/brand/access", { method: "PUT", data: {
    expectedRevision: "r1", teams: [{ teamId: "team", active: true, channelIds: ["channel"] }]
  } }));
});
it("does not fetch assignments for a workspace Member", () => {
  render(<RbacContext.Provider value={{ ...context, workspaceRole: "Member", actions: [] }}><RbacBrandAccess brandId="brand" /></RbacContext.Provider>);
  expect(apiClient).not.toHaveBeenCalled();
  expect(screen.queryByRole("checkbox")).toBeNull();
});

