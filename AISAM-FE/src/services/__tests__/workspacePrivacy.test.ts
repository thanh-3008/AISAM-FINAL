import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/apiClient";
import { fetchCreditWallet, fetchWorkspaceDashboard } from "@/services/workspaceService";

vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn() }));

describe("workspace financial field privacy", () => {
  beforeEach(() => vi.resetAllMocks());
  it("keeps unauthorized dashboard fields absent instead of zero-filling", async () => {
    vi.mocked(apiClient).mockResolvedValue({ success: true, data: { creditsUsed: 3, publishedPostCount: 2 } });
    const dashboard = await fetchWorkspaceDashboard();
    expect(dashboard?.creditsUsed).toBe(3);
    expect(dashboard?.creditBalance).toBeUndefined();
    expect(dashboard?.postQuotaLimit).toBeUndefined();
    expect(dashboard?.postsRemaining).toBeUndefined();
  });
  it("does not reconstruct a wallet from a dashboard without financial permission", async () => {
    vi.mocked(apiClient).mockImplementation(async (path) => {
      if (path === "/workspace-dashboard/summary") return { success: true, data: { workspaceId: "w", creditsUsed: 3 } };
      throw new Error("Forbidden");
    });
    expect(await fetchCreditWallet()).toBeNull();
  });
  it("preserves an authorized zero balance", async () => {
    vi.mocked(apiClient).mockResolvedValue({ success: true, data: { balance: 0, workspaceId: "w" } });
    expect((await fetchCreditWallet())?.balance).toBe(0);
  });
});
