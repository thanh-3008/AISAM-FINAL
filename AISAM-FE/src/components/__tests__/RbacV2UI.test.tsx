import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { RbacContext, parseRbacContext, type RbacContextValue } from "@/contexts/RbacContext";
import TeamScopeSelect from "@/components/content/TeamScopeSelect";
import RbacTeamManagement from "@/components/team/RbacTeamManagement";
vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn(async (path: string) => ({ data: path === "/workspace-members" ? [] : { items: [{ id: "a", name: "Team A" }, { id: "b", name: "Team B" }] } })) }));
vi.mock("next/link", () => ({ default: ({ children }: { children: React.ReactNode }) => <span>{children}</span> }));
afterEach(cleanup);
const context: RbacContextValue = { contractVersion: 2, revision: "r1", workspaceRole: "Member", actions: [], teams: [{ teamId: "a", role: "Manager" }, { teamId: "b", role: "Viewer" }], scopes: [
  { teamId: "a", brandId: "brand", role: "Manager", channelIds: [] },
  { teamId: "b", brandId: "brand", role: "Viewer", channelIds: [] },
] };
it("does not offer Viewer Team B for creation when the same user manages A", async () => {
  render(<RbacContext.Provider value={context}><TeamScopeSelect brandId="brand" value="" onChange={() => {}} /></RbacContext.Provider>);
  expect(await screen.findByRole("option", { name: "Team A" })).toBeTruthy();
  expect(screen.queryByRole("option", { name: "Team B" })).toBeNull();
});
it("does not show workspace HR or Team creation to a Team manager", async () => {
  render(<RbacContext.Provider value={context}><RbacTeamManagement /></RbacContext.Provider>);
  fireEvent.click(await screen.findByRole("button", { name: "Teams" }));
  await screen.findByRole("button", { name: "Team A" });
  expect(screen.queryByRole("button", { name: "Tạo Team" })).toBeNull();
  expect(screen.queryByRole("button", { name: "Mời vào workspace" })).toBeNull();
});
it("rejects unsupported roles and incomplete v2 context instead of using legacy access", () => {
  expect(() => parseRbacContext({ ...context, workspaceRole: "Manager" })).toThrow();
  expect(() => parseRbacContext({ ...context, contractVersion: undefined })).toThrow();
  expect(() => parseRbacContext({ ...context, scopes: [null] })).toThrow();
  expect(() => parseRbacContext({ revision: "legacy" })).toThrow(/RBAC v2/);
});

it("normalizes omitted nullable Team roles in an Owner context", () => {
  const parsed = parseRbacContext({
    ...context,
    workspaceRole: "Owner",
    teams: [{ teamId: "a" }],
    scopes: [{ teamId: "00000000-0000-0000-0000-000000000000", brandId: "brand-a", channelIds: [] }],
  });

  expect(parsed.teams[0].role).toBeNull();
  expect(parsed.scopes[0].role).toBeNull();
});
