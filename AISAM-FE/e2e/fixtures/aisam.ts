import { expect, test as base, type Page, type Route } from "@playwright/test";
import { Buffer } from "buffer";
export const IDS = {
  user: "10000000-0000-4000-8000-000000000001",
  workspace: "20000000-0000-4000-8000-000000000002",
  profile: "30000000-0000-4000-8000-000000000003",
  content: "40000000-0000-4000-8000-000000000004",
  brand: "50000000-0000-4000-8000-000000000005",
};

const b64 = (value: object) => Buffer.from(JSON.stringify(value)).toString("base64url");
export const tokenFor = (role: "User" | "Admin" = "User") =>
  `${b64({ alg: "none", typ: "JWT" })}.${b64({
    sub: IDS.user,
    name: role === "Admin" ? "AISAM Admin" : "E2E User",
    email: role === "Admin" ? "admin@aisam.test" : "user@aisam.test",
    role,
    exp: Math.floor(Date.now() / 1000) + 3600,
  })}.e2e`;

const ok = (data: unknown = null) => ({ success: true, data, message: "Success" });

async function fulfill(route: Route, data: unknown = null, status = 200) {
  await route.fulfill({ status, contentType: "application/json", body: JSON.stringify(status < 400 ? ok(data) : data) });
}

export async function installApiMock(page: Page) {
  // Match both the dedicated E2E host and the app's localhost fallback. This
  // also makes E2E_BASE_URL useful with builds that were compiled elsewhere.
  await page.route("**/api/**", async (route) => {
    const request = route.request();
    const url = new URL(request.url());
    const path = url.pathname.replace(/^\/api/, "");
    const method = request.method();

    if (path === "/auth/login" && method === "POST") {
      const body = request.postDataJSON() as { email: string };
      const role = body.email.startsWith("admin") ? "Admin" : "User";
      return fulfill(route, {
        accessToken: tokenFor(role), refreshToken: "e2e-refresh-token",
        user: { id: IDS.user, fullName: role === "Admin" ? "AISAM Admin" : "E2E User", email: body.email, role },
      });
    }
    if (path === "/auth/me") return fulfill(route, { id: IDS.user, fullName: "E2E User", email: "user@aisam.test" });
    if (path === "/auth/register" || path === "/auth/forgot-password" || path === "/auth/reset-password" || path.includes("verify-email")) return fulfill(route);
    if (path === "/workspaces") return fulfill(route, [{ id: IDS.workspace, name: "E2E Workspace", workspaceType: 0 }]);
    if (path.startsWith("/profiles/user/")) return fulfill(route, [{ id: IDS.profile, name: "E2E Profile", profileType: 0, workspaceId: IDS.workspace }]);
    if (path.startsWith("/brands") && method === "GET") return fulfill(route, { data: [{ id: IDS.brand, name: "E2E Brand", description: "Deterministic test brand", status: 1 }], totalCount: 1, page: 1, pageSize: 100 });
    if (path.startsWith("/products")) return fulfill(route, { data: [{ id: "60000000-0000-4000-8000-000000000006", name: "E2E Product", brandId: IDS.brand }], totalCount: 1, page: 1, pageSize: 100 });
    if (path.startsWith("/content") && method === "GET") return fulfill(route, { data: [{ id: IDS.content, title: "E2E Launch Post", contentType: 0, status: 0, platforms: ["Facebook"], createdAt: new Date().toISOString() }], totalCount: 1, page: 1, pageSize: 12, totalPages: 1 });
    if (path === "/content" && method === "POST") return fulfill(route, { id: IDS.content, title: "E2E Created Content", contentType: 0, status: 0, platforms: ["Facebook"], createdAt: new Date().toISOString() });
    if (path.startsWith("/tags")) return fulfill(route, ["launch", "e2e"]);
    if (path.startsWith("/notifications/unread-count")) return fulfill(route, { count: 1 });
    if (path.startsWith("/notifications")) return fulfill(route, { data: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });
    if (path.startsWith("/posts")) return fulfill(route, { data: [], totalCount: 0, page: 1, pageSize: 12, totalPages: 0 });
    if (path.startsWith("/content-schedules")) return fulfill(route, { data: [], totalCount: 0, page: 1, pageSize: 50, totalPages: 0 });
    if (path.startsWith("/social/accounts/me") || path.startsWith("/social/integrations")) return fulfill(route, []);
    if (path.startsWith("/campaigns")) return fulfill(route, { data: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });
    if (path.startsWith("/workspace-members")) return fulfill(route, []);
    if (path.startsWith("/workspace-invitations")) return fulfill(route, []);
    if (path.startsWith("/automation")) return fulfill(route, []);
    if (path.startsWith("/pricing/plans") || path.startsWith("/pricing/credit-packs")) return fulfill(route, []);
    if (path.startsWith("/payment/") || path.startsWith("/quota/") || path.startsWith("/credit-usage")) return fulfill(route, null);
    if (path === "/feature-flags") return fulfill(route, { features: {} });
    if (path.startsWith("/dashboard/") || path.startsWith("/workspace-dashboard/") || path.startsWith("/analytics/")) return fulfill(route, {});
    if (path.startsWith("/admin/users")) return fulfill(route, { items: [], totalCount: 0, page: 1, pageSize: 20 });
    if (path.startsWith("/admin/workspaces") || path.startsWith("/admin/payments") || path.startsWith("/admin/content") || path.startsWith("/admin/audit-logs")) return fulfill(route, { items: [], totalCount: 0, page: 1, pageSize: 20 });
    if (path === "/admin/settings") return fulfill(route, []);
    if (path === "/admin/plans") return fulfill(route, { plans: [] });
    if (path === "/admin/plans/credit-packs") return fulfill(route, { creditPacks: [] });
    if (path.startsWith("/admin/")) return fulfill(route, {});
    return fulfill(route, method === "GET" ? [] : null);
  });
}

export async function authenticate(page: Page, role: "User" | "Admin" = "User") {
  const token = tokenFor(role);
  await page.context().addCookies([{ name: "aisam_role", value: role, domain: "127.0.0.1", path: "/" }]);
  await page.addInitScript(({ token, role, ids }) => {
    localStorage.setItem("aisam_token", token);
    localStorage.setItem("aisam_refresh_token", "e2e-refresh-token");
    localStorage.setItem("aisam_user", JSON.stringify({ id: ids.user, fullName: role === "Admin" ? "AISAM Admin" : "E2E User", email: `${role.toLowerCase()}@aisam.test` }));
    if (role === "User") {
      localStorage.setItem("aisam_active_workspace", JSON.stringify({ id: ids.workspace, name: "E2E Workspace", workspaceType: 0 }));
      localStorage.setItem("aisam_active_profile", JSON.stringify({ id: ids.profile, name: "E2E Profile", profileType: 0 }));
    }
  }, { token, role, ids: IDS });
}

type Fixtures = { userPage: Page; adminPage: Page };
export const test = base.extend<Fixtures>({
  userPage: async ({ page }, runTest) => { await installApiMock(page); await authenticate(page, "User"); await runTest(page); },
  adminPage: async ({ page }, runTest) => { await installApiMock(page); await authenticate(page, "Admin"); await runTest(page); },
});
export { expect };
