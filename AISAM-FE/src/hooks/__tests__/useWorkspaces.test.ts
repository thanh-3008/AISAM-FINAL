import { describe, expect, it, vi } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { useWorkspaces } from "@/hooks/useWorkspaces";

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
  it("loads workspaces only from workspace endpoint", async () => {
    const { result } = renderHook(() => useWorkspaces());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.workspaces).toHaveLength(1);
    expect(result.current.activeWorkspace?.id).toBe("w1");
  });
});
