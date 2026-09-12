import { expect, IDS, test } from "./fixtures/aisam";
import { readFileSync } from "node:fs";

test("content library sends workspace/profile context and opens create flow", async ({ userPage }) => {
  const apiRequests: import("@playwright/test").Request[] = [];
  userPage.on("request", (request) => {
    if (request.url().includes("/api/content") && request.method() === "GET") apiRequests.push(request);
  });
  await userPage.goto("/content");
  await expect(userPage.getByRole("heading", { name: "Content Library" })).toBeVisible();
  await expect.poll(() => apiRequests.length).toBeGreaterThan(0);
  const request = apiRequests[0];
  expect(request, "content list API request").toBeTruthy();
  if (!request) return;
  expect(request.headers()["x-workspace-id"]).toBe(IDS.workspace);
  expect(request.headers()["x-profile-id"]).toBe(IDS.profile);
  await userPage.getByRole("button", { name: /create new content/i }).click();
  await userPage.getByRole("button", { name: /manual creation/i }).click();
  await expect(userPage).toHaveURL(/\/content\/create$/);
});

test("manual content creation posts the form with workspace context", async ({ userPage }) => {
  await userPage.goto("/content/create");
  await userPage.getByPlaceholder(/compelling title/i).fill("E2E Created Content");
  await userPage.getByRole("combobox").nth(1).selectOption({ label: "E2E Product" });
  const requestPromise = userPage.waitForRequest((request) => new URL(request.url()).pathname === "/api/content" && request.method() === "POST");
  await userPage.getByRole("button", { name: /Lưu draft và mở composer/i }).click();
  const request = await requestPromise;
  expect(request.headers()["x-workspace-id"]).toBe(IDS.workspace);
  expect(request.postDataJSON()).toMatchObject({ title: "E2E Created Content", brandId: IDS.brand });
  await expect(userPage.getByText(/content created successfully/i).first()).toBeVisible();
});

test("selected video keeps its filename and remains playable across post preview tabs", async ({ userPage }) => {
  await userPage.goto("/content/create");
  await userPage.getByRole("button", { name: /^play_circle Video$/i }).click();

  const videoBytes = readFileSync("e2e/fixtures/product-demo.webm");

  await userPage.locator('input[type="file"][accept*=".webm"]').setInputFiles({
    name: "product-demo.webm",
    mimeType: "video/webm",
    buffer: videoBytes,
  });

  await expect(userPage.getByText("product-demo.webm")).toBeVisible();
  await expect(userPage.getByLabel("Selected video preview")).toHaveCount(2);
  await expect.poll(() => userPage.getByLabel("Selected video preview").last().evaluate((element) => (element as HTMLVideoElement).readyState))
    .toBeGreaterThan(0);

  await userPage.getByRole("button", { name: /Instagram/i }).click();
  await expect(userPage.getByLabel("Selected video preview")).toHaveCount(2);
  await userPage.getByRole("button", { name: /TikTok/i }).click();
  await expect(userPage.getByLabel("Selected video preview")).toHaveCount(2);
  await userPage.getByRole("button", { name: /Facebook/i }).click();

  await expect(userPage.getByText("product-demo.webm")).toBeVisible();
  await expect(userPage.getByLabel("Selected video preview")).toHaveCount(2);
});
