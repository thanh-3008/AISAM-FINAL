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
  it("sends v2 contract and uses the HR revision returned for this workspace", async () => {
    vi.mocked(fetch).mockResolvedValueOnce(new Response('{"data":[]}', { status: 200, headers: { "X-HR-Revision": "hr-a" } }))
      .mockResolvedValueOnce(response(200, {}));
    await apiClient("/workspace-members");
    await apiClient("/teams", { method: "POST", data: { name: "Team" } });
    expect(fetch).toHaveBeenLastCalledWith(expect.any(String), expect.objectContaining({ headers: expect.objectContaining({ "X-RBAC-Contract-Version": "2", "If-Match": "hr-a" }) }));
    storeActiveWorkspace(b);
    vi.mocked(fetch).mockResolvedValueOnce(response(428, {}));
    await expect(apiClient("/teams", { method: "POST", data: { name: "B" } })).rejects.toMatchObject({ status: 428 });
    const headers = (vi.mocked(fetch).mock.calls.at(-1)?.[1]?.headers as Record<string, string>);
    expect(headers["If-Match"]).toBeUndefined();
  });
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
  it("refreshes a stale automatic HR revision and retries the rejected mutation once", async () => {
    vi.mocked(fetch)
      .mockResolvedValueOnce(new Response('{"data":[]}', { status: 200, headers: { "X-HR-Revision": "hr-old" } }))
      .mockResolvedValueOnce(response(409, { errorCode: "ACCESS_REVISION_CONFLICT" }))
      .mockResolvedValueOnce(new Response('{"data":[]}', { status: 200, headers: { "X-HR-Revision": "hr-new" } }))
      .mockResolvedValueOnce(response(200, { success: true }));

    await apiClient("/workspace-members");
    await expect(apiClient("/workspace-invitations", { method: "POST", data: { email: "member@example.com" } }))
      .resolves.toMatchObject({ success: true });

    expect(fetch).toHaveBeenCalledTimes(4);
    expect(fetch).toHaveBeenNthCalledWith(3, expect.any(String), expect.objectContaining({ method: "GET", headers: expect.not.objectContaining({ "If-Match": expect.anything() }) }));
    expect(fetch).toHaveBeenNthCalledWith(4, expect.any(String), expect.objectContaining({ method: "POST", headers: expect.objectContaining({ "If-Match": "hr-new" }) }));
  });
  it("preserves a business conflict message and does not retry the mutation", async () => {
    vi.mocked(fetch)
      .mockResolvedValueOnce(new Response('{"data":[]}', { status: 200, headers: { "X-HR-Revision": "hr-current" } }))
      .mockResolvedValueOnce(response(409, {
        message: "Workspace member limit of 3 has been reached.",
        statusCode: 409,
      }));

    await apiClient("/workspace-members");
    await expect(apiClient("/workspace-invitations", { method: "POST", data: { email: "fourth@example.com" } }))
      .rejects.toThrow("Workspace member limit of 3 has been reached.");

    expect(fetch).toHaveBeenCalledTimes(2);
  });
  it("fails closed on malformed permission responses", async () => {
    vi.mocked(fetch).mockResolvedValue(response(200, { data: [] }));
    await expect(checkPermissions([{ kind: 2, resourceId: "content", permission: 4 }])).rejects.toThrow();
  });
});
