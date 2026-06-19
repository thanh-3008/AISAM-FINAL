import { beforeEach, describe, expect, it, vi } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { invalidateWorkspaceCache, useWorkspaces } from "@/hooks/useWorkspaces";
import { apiClient } from "@/lib/apiClient";

vi.mock("@/lib/apiClient", () => ({
  apiClient: vi.fn(async (endpoint: string) => {
    if (endpoint === "/workspaces") {
      return {
        success: true,
        data: [
          { id: "w1", name: "Workspace A", workspaceType: 2, status: 1, currentUserRole: 0, createdAt: "", updatedAt: "" },
        ],
      };
    }
    throw new Error(`unexpected endpoint: ${endpoint}`);
  }),
}));

vi.mock("@/lib/auth", () => ({
  getUserIdFromToken: vi.fn(() => "user1"),
}));

describe("useWorkspaces", () => {
  beforeEach(() => {
    localStorage.clear();
    invalidateWorkspaceCache();
    vi.mocked(apiClient).mockReset();
    vi.mocked(apiClient).mockImplementation(async (endpoint: string) => {
      if (endpoint === "/workspaces") {
        return {
          success: true,
          data: [
            { id: "w1", name: "Workspace A", workspaceType: 2, status: 1, currentUserRole: 0, createdAt: "", updatedAt: "" },
          ],
        };
      }
      throw new Error(`unexpected endpoint: ${endpoint}`);
    });
  });

  it("loads workspaces only from workspace endpoint", async () => {
    const { result } = renderHook(() => useWorkspaces());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.workspaces).toHaveLength(1);
    expect(result.current.activeWorkspace?.id).toBe("w1");
  });

  it("keeps stored active workspace when workspace fetch fails after reload", async () => {
    localStorage.setItem("aisam_active_workspace", JSON.stringify({
      id: "stored-personal",
      name: "Personal Workspace",
      workspaceType: 1,
    }));
    vi.mocked(apiClient).mockRejectedValueOnce(new Error("Forbidden"));

    const { result } = renderHook(() => useWorkspaces());

    expect(result.current.activeWorkspace?.id).toBe("stored-personal");
    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current.workspaces).toHaveLength(1);
    expect(result.current.activeWorkspace?.name).toBe("Personal Workspace");
  });
});
