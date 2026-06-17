import { describe, expect, it, vi } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { useProfiles } from "@/hooks/useProfiles";

vi.mock("@/lib/apiClient", () => ({
  apiClient: vi.fn(async () => ({
    success: true,
    data: [
      { id: "p1", userId: "u1", name: "Only Profile", profileType: 2, status: 1 },
    ],
  })),
}));

vi.mock("@/lib/auth", () => ({
  getUserIdFromToken: vi.fn(() => "user1"),
}));

vi.mock("@/hooks/useWorkspaces", () => ({
  useWorkspaces: vi.fn(() => ({
    activeWorkspace: { id: "w1", name: "W1", workspaceType: 2 },
  })),
}));

describe("useProfiles", () => {
  it("auto-selects the only valid profile", async () => {
    const { result } = renderHook(() => useProfiles());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.activeProfile?.id).toBe("p1");
  });
});
