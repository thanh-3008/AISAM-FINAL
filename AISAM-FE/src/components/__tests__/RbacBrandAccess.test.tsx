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
beforeEach(() => { vi.mocked(apiClient).mockReset(); vi.mocked(apiClient).mockImplementation(async path => ({ data: path.includes("/access") ? snapshot : { items: [{ id: "team", name: "Team A" }] } })); });
afterEach(cleanup);
it("sends only the v2 channel scope revision and does not report success on conflict", async () => {
  render(<RbacContext.Provider value={context}><RbacBrandAccess brandId="brand" /></RbacContext.Provider>);
  const checkbox = await screen.findByRole("checkbox", { name: "Kênh A" });
  vi.mocked(apiClient).mockRejectedValueOnce(new Error("Revision conflict"));
  fireEvent.click(checkbox);
  expect((await screen.findByRole("alert")).textContent).toContain("Revision conflict");
  await waitFor(() => expect(apiClient).toHaveBeenLastCalledWith("/brands/brand/channels/channel/teams/team", { method: "PUT", data: { expectedRevision: "r1" } }));
  expect((checkbox as HTMLInputElement).checked).toBe(false);
  expect(screen.queryByText("Channel permissions updated.")).toBeNull();
});
it("does not fetch assignments for a workspace Member", () => {
  render(<RbacContext.Provider value={{ ...context, workspaceRole: "Member", actions: [] }}><RbacBrandAccess brandId="brand" /></RbacContext.Provider>);
  expect(apiClient).not.toHaveBeenCalled();
  expect(screen.queryByRole("checkbox")).toBeNull();
});

