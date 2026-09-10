import React, { useState } from "react";
import { afterEach, expect, it, vi } from "vitest";
import { act, cleanup, fireEvent, render, screen } from "@testing-library/react";
import WorkspaceBoundary from "../WorkspaceBoundary";
vi.mock("@/hooks/useWorkspaces", () => ({ invalidateWorkspaceCache: vi.fn() }));
vi.mock("@/hooks/useProfiles", () => ({ invalidateProfileCache: vi.fn() }));
vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn(async () => ({ data: { revision: "r1" } })) }));
vi.mock("next/link", () => ({ default: ({ children }: { children: React.ReactNode }) => <span>{children}</span> }));
afterEach(cleanup);
function Draft() { const [value, setValue] = useState(""); return <input aria-label="draft" value={value} onChange={e => setValue(e.target.value)} />; }
it("does not hide the workspace for a single forbidden feature", async () => {
  render(<WorkspaceBoundary><Draft /></WorkspaceBoundary>);
  await act(async () => window.dispatchEvent(new CustomEvent("aisam-access-denied", { detail: { path: "/analytics", status: 403 } })));
  expect(screen.queryByLabelText("draft")).not.toBeNull();
  expect(screen.queryByRole("alert")).toBeNull();
});
it("clears page state on workspace change and hides it on denied access", () => {
  render(<WorkspaceBoundary><Draft /></WorkspaceBoundary>);
  fireEvent.change(screen.getByLabelText("draft"), { target: { value: "private draft A" } });
  act(() => window.dispatchEvent(new Event("aisam-workspace-changed")));
  expect((screen.getByLabelText("draft") as HTMLInputElement).value).toBe("");
  act(() => window.dispatchEvent(new Event("aisam-access-denied")));
  expect(screen.queryByLabelText("draft")).toBeNull();
  expect(screen.getByRole("alert").textContent).toContain("Bạn vẫn đang đăng nhập");
  fireEvent.click(screen.getByRole("button", { name: "Kiểm tra lại quyền" }));
  expect((screen.getByLabelText("draft") as HTMLInputElement).value).toBe("");
});
