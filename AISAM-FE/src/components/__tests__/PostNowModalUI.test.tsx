import React from "react";
import { afterEach, beforeEach, expect, it, vi } from "vitest";
import { cleanup, render, screen, fireEvent } from "@testing-library/react";
import PostNowModal from "../content/PostNowModal";

const mocks = vi.hoisted(() => ({ preview: vi.fn(), start: vi.fn(), read: vi.fn() }));
vi.mock("@/services/composerService", () => ({ previewPublish: mocks.preview, startPublish: mocks.start, readPublish: mocks.read }));
vi.mock("@/lib/auth", () => ({ getUserIdFromToken: () => "user-test" }));
vi.mock("@/stores/workspace-store", () => ({ getStoredActiveWorkspace: () => ({ id: "workspace-test" }) }));

const props = { contentId: "content-123", onClose: vi.fn(), onSuccess: vi.fn() };

beforeEach(() => {
  localStorage.clear();
  vi.clearAllMocks();
  mocks.preview.mockResolvedValue({
    version: "v1",
    approved: true,
    media: [
      { url: "https://example.com/img1.png", mimeType: "image/png", sortOrder: 0, isCover: true }
    ],
    destinations: [
      { id: "fb-1", name: "Facebook Fanpage", platform: "facebook", error: null, caption: "FB Caption Content", capability: { maxItems: 10, mixedMedia: true, multiVideo: false } },
      { id: "ig-1", name: "Instagram Business", platform: "instagram", error: null, caption: "IG Caption with #hashtags", capability: { maxItems: 10, mixedMedia: false, multiVideo: false } },
      { id: "tt-1", name: "TikTok Channel", platform: "tiktok", error: null, caption: "Short TikTok text", capability: { maxItems: 1, mixedMedia: false, multiVideo: false } },
    ]
  });
  mocks.read.mockResolvedValue([]);
});

afterEach(cleanup);

it("renders 3-tier structure: fixed header, scrollable body, fixed footer", async () => {
  render(<PostNowModal {...props} />);
  expect(await screen.findByRole("heading", { name: "Post Now" })).toBeDefined();
  expect(screen.getByRole("button", { name: "Đóng" })).toBeDefined();
  expect(screen.getByRole("button", { name: "Close" })).toBeDefined();
  expect(screen.getByRole("button", { name: /Đăng lên 0 kênh/ })).toBeDefined();
});

it("supports 'Chọn tất cả kênh hợp lệ' and 'Bỏ chọn tất cả'", async () => {
  render(<PostNowModal {...props} />);
  const selectAllBtn = await screen.findByRole("button", { name: "Chọn tất cả kênh hợp lệ" });
  fireEvent.click(selectAllBtn);

  expect(await screen.findByRole("button", { name: /Đăng lên 3 kênh/ })).toBeDefined();
  expect(screen.getByRole("button", { name: "Bỏ chọn tất cả" })).toBeDefined();

  fireEvent.click(screen.getByRole("button", { name: "Bỏ chọn tất cả" }));
  expect(await screen.findByRole("button", { name: /Đăng lên 0 kênh/ })).toBeDefined();
});

it("switches channel preview tabs and displays respective channel caption", async () => {
  render(<PostNowModal {...props} />);
  // Initial preview tab defaults to first destination (Facebook Fanpage)
  expect(await screen.findByText("FB Caption Content")).toBeDefined();

  // Click Instagram tab in preview header
  const igTab = screen.getByRole("button", { name: "Instagram Business" });
  fireEvent.click(igTab);

  expect(await screen.findByText("IG Caption with #hashtags")).toBeDefined();
});
