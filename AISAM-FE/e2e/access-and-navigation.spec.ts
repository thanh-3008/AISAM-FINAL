import { expect, test } from "./fixtures/aisam";

test("public landing and legal pages are reachable", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
  await page.goto("/privacy");
  await expect(page.getByRole("heading", { name: "Privacy Policy" })).toBeVisible();
  await page.goto("/terms");
  await expect(page.getByRole("heading", { name: "Terms of Service" })).toBeVisible();
});

test("non-admin is redirected away from admin routes", async ({ userPage }) => {
  await userPage.goto("/admin/users");
  await expect(userPage).toHaveURL(/\/dashboard$/);
});

test("admin is redirected away from user workspace routes", async ({ adminPage }) => {
  await adminPage.goto("/content");
  await expect(adminPage).toHaveURL(/\/admin\/dashboard$/);
});

const userRoutes = [
  ["/content", "Content Library"], ["/approvals", "Content Approvals"],
  ["/posts", "Posts"], ["/workspace-dashboard", "Workspace Dashboard"],
] as const;

for (const [path, heading] of userRoutes) {
  test(`authenticated workspace smoke: ${path}`, async ({ userPage }) => {
    await userPage.goto(path);
    await expect(userPage.getByRole("heading", { name: heading }).first()).toBeVisible();
  });
}

const adminRoutes = [
  ["/admin/users", "Users"], ["/admin/content", "Content Moderation Queue"],
  ["/admin/plans", "Pricing Management"], ["/admin/broadcast", "Broadcast Notification"],
  ["/admin/tools", "Developer Tools"], ["/admin/settings/system", "System Settings"],
] as const;

for (const [path, heading] of adminRoutes) {
  test(`admin smoke: ${path}`, async ({ adminPage }) => {
    await adminPage.goto(path);
    await expect(adminPage.getByRole("heading", { name: heading }).first()).toBeVisible();
  });
}
