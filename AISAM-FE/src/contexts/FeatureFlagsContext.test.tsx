import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, render, screen, waitFor } from "@testing-library/react";
import { FeatureFlagsProvider } from "./FeatureFlagsContext";
const mocks = vi.hoisted(() => ({ path: "/reset-password", token: null as string | null, api: vi.fn() }));
vi.mock("next/navigation", () => ({ usePathname: () => mocks.path }));
vi.mock("@/lib/auth", () => ({ getToken: () => mocks.token }));
vi.mock("@/lib/apiClient", () => ({ apiClient: mocks.api }));
afterEach(() => { cleanup(); mocks.api.mockReset(); mocks.token = null; });
it.each(["/forgot-password", "/reset-password", "/login", "/register"])("does not request authenticated flags on %s even with a stale token", (path) => {
  mocks.path = path;
  mocks.token = "expired-token";
  render(<FeatureFlagsProvider><div>Recovery form</div></FeatureFlagsProvider>);
  expect(screen.getByText("Recovery form")).toBeTruthy();
  expect(mocks.api).not.toHaveBeenCalled();
});
it("loads flags after navigation to an authenticated page", async () => {
  mocks.path = "/reset-password";
  const view = render(<FeatureFlagsProvider><div>Page</div></FeatureFlagsProvider>);
  mocks.path = "/dashboard";
  mocks.token = "session-token";
  mocks.api.mockResolvedValue({ success: true, data: { enabledFeatures: [], maintenanceMode: false } });
  view.rerender(<FeatureFlagsProvider><div>Page</div></FeatureFlagsProvider>);
  await waitFor(() => expect(mocks.api).toHaveBeenCalledWith("/feature-flags"));
});
