import React, { useEffect, useState } from "react";
import { afterEach, expect, it, vi } from "vitest";
import { act, cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import WorkspaceBoundary from "../WorkspaceBoundary";
import { useRbac } from "@/contexts/RbacContext";
import { invalidateWorkspaceCache } from "@/hooks/useWorkspaces";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { invalidateProfileCache } from "@/hooks/useProfiles";
import { apiClient } from "@/lib/apiClient";
const permissionContext = (revision: string) => ({
  data: {
    contractVersion: 2,
    revision,
    workspaceRole: "Owner",
    actions: ["team.manage"],
    teams: [],
    scopes: [],
  },
});
vi.mock("@/hooks/useWorkspaces", () => ({ invalidateWorkspaceCache: vi.fn() }));
vi.mock("@/stores/workspace-store", () => ({ getStoredActiveWorkspace: vi.fn(() => ({ id: "w1" })) }));
vi.mock("@/hooks/useProfiles", () => ({ invalidateProfileCache: vi.fn() }));
vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn(async () => permissionContext("r1")) }));
vi.mock("next/link", () => ({ default: ({ children }: { children: React.ReactNode }) => <span>{children}</span> }));
afterEach(() => {
  cleanup();
  vi.useRealTimers();
  vi.clearAllMocks();
  vi.mocked(apiClient).mockResolvedValue(permissionContext("r1"));
  vi.mocked(getStoredActiveWorkspace).mockReturnValue({ id: "w1" } as never);
});
function Draft() { const [value, setValue] = useState(""); return <input aria-label="draft" value={value} onChange={e => setValue(e.target.value)} />; }
function PermissionConsumer({ changed }: { changed: () => void }) { const context = useRbac(); useEffect(changed, [context, changed]); return <span>ready</span>; }
it("does not hide the workspace for a single forbidden feature", async () => {
  render(<WorkspaceBoundary><Draft /></WorkspaceBoundary>);
  await act(async () => window.dispatchEvent(new CustomEvent("aisam-access-denied", { detail: { path: "/analytics", status: 403 } })));
  expect(screen.queryByLabelText("draft")).not.toBeNull();
  expect(screen.queryByRole("alert")).toBeNull();
});
it("ignores a feature 403 without replacing the permission context", async () => {
  const changed = vi.fn();
  render(<WorkspaceBoundary><PermissionConsumer changed={changed} /></WorkspaceBoundary>);
  await screen.findByText("ready");
  await waitFor(() => expect(changed.mock.calls.length).toBeGreaterThanOrEqual(2));
  const callsBeforeFeatureDenial = changed.mock.calls.length;
  await act(async () => window.dispatchEvent(new CustomEvent("aisam-access-denied", { detail: { path: "/workspace-dashboard/summary", status: 403 } })));
  expect(changed).toHaveBeenCalledTimes(callsBeforeFeatureDenial);
});
it("accepts a changed permission revision without invalidating the active workspace", async () => {
  vi.mocked(apiClient)
    .mockResolvedValueOnce(permissionContext("r1"))
    .mockResolvedValueOnce(permissionContext("r2"));
  render(<WorkspaceBoundary><Draft /></WorkspaceBoundary>);
  await screen.findByLabelText("draft");
  fireEvent.change(screen.getByLabelText("draft"), { target: { value: "old brand state" } });

  await act(async () => window.dispatchEvent(new Event("focus")));

  expect(screen.queryByRole("alert")).toBeNull();
  expect((screen.getByLabelText("draft") as HTMLInputElement).value).toBe("");
  expect(invalidateWorkspaceCache).not.toHaveBeenCalled();
  expect(invalidateProfileCache).not.toHaveBeenCalled();
});
it("coalesces duplicate events for the same selected workspace", async () => {
  let resolveContext!: (value: unknown) => void;
  vi.mocked(apiClient).mockImplementationOnce(() => new Promise(resolve => { resolveContext = resolve; }));
  render(<WorkspaceBoundary><Draft /></WorkspaceBoundary>);

  window.dispatchEvent(new Event("aisam-workspace-changed"));
  window.dispatchEvent(new Event("aisam-permissions-changed"));
  window.dispatchEvent(new Event("focus"));
  expect(apiClient).toHaveBeenCalledTimes(1);

  await act(async () => resolveContext(permissionContext("r1")));
  expect(await screen.findByLabelText("draft")).not.toBeNull();
  expect(invalidateWorkspaceCache).not.toHaveBeenCalled();
});
it("does not report a server failure as revoked workspace access", async () => {
  const failure = Object.assign(new Error("Permission service unavailable"), { status: 500 });
  vi.mocked(apiClient).mockRejectedValueOnce(failure);

  render(<WorkspaceBoundary><Draft /></WorkspaceBoundary>);

  expect(await screen.findByText("Permission service unavailable")).not.toBeNull();
  expect(screen.getByLabelText("draft")).not.toBeNull();
  expect(screen.queryByText("Unable to access workspace")).toBeNull();
});
it("recovers when the first contract is invalid and the retry has the same revision", async () => {
  vi.mocked(apiClient)
    .mockResolvedValueOnce({ data: { revision: "r1" } })
    .mockResolvedValueOnce(permissionContext("r1"));
  const changed = vi.fn();

  render(<WorkspaceBoundary><PermissionConsumer changed={changed} /></WorkspaceBoundary>);

  expect(await screen.findByText(/Backend chưa bật RBAC v2/)).toBeTruthy();
  await act(async () => window.dispatchEvent(new Event("aisam-permissions-changed")));
  await waitFor(() => expect(changed.mock.calls.length).toBeGreaterThanOrEqual(2));
  expect(screen.queryByText(/Backend chưa bật RBAC v2/)).toBeNull();
});
it("escapes the loading screen when permission loading never settles", async () => {
  vi.useFakeTimers();
  vi.mocked(apiClient).mockImplementationOnce(() => new Promise(() => {}));
  render(<WorkspaceBoundary><Draft /></WorkspaceBoundary>);

  await act(async () => { await vi.advanceTimersByTimeAsync(10000); });

  expect(screen.getByText("Permission verification timed out. Please try again.")).not.toBeNull();
  expect(screen.getByLabelText("draft")).not.toBeNull();
});
it("clears page state on workspace change and hides it on denied access", async () => {
  render(<WorkspaceBoundary><Draft /></WorkspaceBoundary>);
  await screen.findByLabelText("draft");
  fireEvent.change(screen.getByLabelText("draft"), { target: { value: "private draft A" } });
  vi.mocked(getStoredActiveWorkspace).mockReturnValue({ id: "w2" } as never);
  await act(async () => window.dispatchEvent(new Event("aisam-workspace-changed")));
  expect((screen.getByLabelText("draft") as HTMLInputElement).value).toBe("");
  act(() => window.dispatchEvent(new Event("aisam-access-denied")));
  expect(screen.queryByLabelText("draft")).toBeNull();
  expect(screen.getByRole("alert").textContent).toContain("You are still signed in");
  await act(async () => fireEvent.click(screen.getByRole("button", { name: "Re-verify permissions" })));
  expect((screen.getByLabelText("draft") as HTMLInputElement).value).toBe("");
});
