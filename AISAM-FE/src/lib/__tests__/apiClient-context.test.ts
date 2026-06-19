import { describe, expect, it, vi, beforeEach } from "vitest";
import { apiClient } from "@/lib/apiClient";

describe("apiClient context headers", () => {
  beforeEach(() => {
    localStorage.clear();
    global.fetch = vi.fn(async () => ({
      ok: true,
      status: 200,
      json: async () => ({ success: true }),
    })) as unknown as typeof fetch;
  });

  it("always sends workspace header from workspace storage", async () => {
    localStorage.setItem("aisam_active_workspace", JSON.stringify({
      id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      name: "Workspace",
      workspaceType: 2,
    }));

    await apiClient("/brands");

    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/brands"),
      expect.objectContaining({
        headers: expect.objectContaining({
          "X-Workspace-Id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        }),
      }),
    );
  });

  it("does not send legacy profile header", async () => {
    const legacyHeader = ["X", "Profile", "Id"].join("-");
    localStorage.setItem("aisam_active_workspace", JSON.stringify({
      id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      name: "Workspace",
      workspaceType: 2,
    }));
    localStorage.setItem("aisam_active_profile", JSON.stringify({
      id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      name: "Legacy Profile",
      profileType: 0,
    }));

    await apiClient("/content");

    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/content"),
      expect.objectContaining({
        headers: expect.not.objectContaining({
          [legacyHeader]: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        }),
      }),
    );
  });
});
