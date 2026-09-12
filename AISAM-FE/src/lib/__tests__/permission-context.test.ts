import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiClient } from "../apiClient";
import { storeActiveWorkspace } from "@/stores/workspace-store";
import { checkPermissions, changeAssignment } from "@/services/permissionService";
import { refreshAccessToken, removeToken } from "../auth";
vi.mock("../auth", () => ({ getToken: () => "token", ensureValidToken: vi.fn(), refreshAccessToken: vi.fn(), removeToken: vi.fn(), removeRefreshToken: vi.fn() }));
const a = { id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", name: "A", workspaceType: 2 };
const b = { ...a, id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", name: "B" };
const response = (status: number, data: unknown) => new Response(JSON.stringify(data), { status });
beforeEach(() => { localStorage.clear(); storeActiveWorkspace(a); vi.clearAllMocks(); global.fetch = vi.fn(); });
describe("permission context", () => {
  it("drops a response even if workspace changes while reading its body", async () => {
    vi.mocked(fetch).mockResolvedValue({ ok: true, status: 200, text: async () => { storeActiveWorkspace(b); return '{"data":"private A"}'; } } as Response);
    await expect(apiClient("/content")).rejects.toMatchObject({ name: "AbortError" });
  });
  it("never retries an old mutation in the newly selected workspace", async () => {
    vi.mocked(fetch).mockResolvedValue(response(401, {}));
    vi.mocked(refreshAccessToken).mockImplementation(async () => { storeActiveWorkspace(b); return "new-token"; });
    await expect(apiClient("/content/one", { method: "DELETE" })).rejects.toMatchObject({ name: "AbortError" });
    expect(fetch).toHaveBeenCalledTimes(1);
  });
  it("returns a settled 403 error without logging out", async () => {
    vi.mocked(fetch).mockResolvedValue(response(403, { errorCode: "ACCESS_DENIED" }));
    await expect(apiClient("/content")).rejects.toMatchObject({ status: 403 });
    expect(removeToken).not.toHaveBeenCalled();
  });
  it("does not replay an assignment after revision conflict", async () => {
    vi.mocked(fetch).mockResolvedValue(response(409, { errorCode: "ACCESS_REVISION_CONFLICT" }));
    await expect(changeAssignment("brand", "team", "old-revision", false)).rejects.toMatchObject({ status: 409 });
    expect(fetch).toHaveBeenCalledTimes(1);
    expect(fetch).toHaveBeenCalledWith(expect.any(String), expect.objectContaining({ method: "DELETE", headers: expect.objectContaining({ "If-Match": "old-revision" }) }));
  });
  it("fails closed on malformed permission responses", async () => {
    vi.mocked(fetch).mockResolvedValue(response(200, { data: [] }));
    await expect(checkPermissions([{ kind: 2, resourceId: "content", permission: 4 }])).rejects.toThrow();
  });
});
