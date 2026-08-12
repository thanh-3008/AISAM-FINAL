import { expect, installApiMock, test } from "./fixtures/aisam";

test.beforeEach(async ({ page }) => installApiMock(page));

test("login validates required fields and invalid email", async ({ page }) => {
  await page.goto("/login");
  await page.getByRole("button", { name: "Sign In" }).click();
  await expect(page.getByText("Email is required")).toBeVisible();
  await expect(page.getByText("Password is required")).toBeVisible();
  await page.getByLabel("Email Address").fill("not-an-email");
  await page.getByLabel("Password").fill("secret");
  await page.getByRole("button", { name: "Sign In" }).click();
  await expect(page.getByText("Please enter a valid email address")).toBeVisible();
});

test("user login stores session and honors safe redirect", async ({ page }) => {
  await page.goto("/login?redirect=/content");
  await expect(page.getByRole("button", { name: "Sign In" })).toBeEnabled();
  await page.getByLabel("Email Address").fill("user@aisam.test");
  await page.getByLabel("Password").fill("Password123!");
  await page.getByRole("button", { name: "Sign In" }).click();
  await expect(page).toHaveURL(/\/content$/);
  await expect.poll(() => page.evaluate(() => localStorage.getItem("aisam_token"))).toBeTruthy();
});

test("admin login routes to the admin dashboard", async ({ page }) => {
  await page.goto("/login");
  await expect(page.getByRole("button", { name: "Sign In" })).toBeEnabled();
  await page.getByLabel("Email Address").fill("admin@aisam.test");
  await page.getByLabel("Password").fill("Password123!");
  await page.getByRole("button", { name: "Sign In" }).click();
  await expect(page).toHaveURL(/\/admin\/dashboard$/);
});

test("forgot-password submits without leaking account existence", async ({ page }) => {
  await page.goto("/forgot-password");
  await page.getByLabel(/email/i).fill("person@example.com");
  await page.getByRole("button", { name: /send reset link/i }).click();
  await expect(page.getByRole("heading", { name: "Check your inbox" })).toBeVisible();
});

test("register performs client-side password validation", async ({ page }) => {
  await page.goto("/register");
  await page.getByPlaceholder("John Doe").fill("E2E User");
  await page.getByPlaceholder("name@company.com").fill("new@aisam.test");
  await page.getByPlaceholder("Min. 8 characters").fill("short");
  await page.getByPlaceholder("Re-enter your password").fill("different");
  await page.getByRole("button", { name: /create account/i }).click();
  await expect(page.getByText("Password must be at least 8 characters")).toBeVisible();
  await expect(page.getByText("Confirm password does not match")).toBeVisible();
});
