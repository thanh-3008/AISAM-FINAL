import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import ContentMediaGallery from "../content/ContentMediaGallery";

const readMedia = vi.hoisted(() => vi.fn());
vi.mock("@/services/composerService", () => ({ readMedia }));
afterEach(cleanup);

it("shows image and video media from the same review collection", async () => {
  readMedia.mockResolvedValue({ version: "v1", items: [
    { assetId: "video", url: "https://cdn.test/video.mp4", mimeType: "video/mp4", sortOrder: 0, isCover: false },
    { assetId: "image", url: "https://cdn.test/image.jpg", mimeType: "image/jpeg", sortOrder: 1, isCover: true },
  ] });
  const { container } = render(<ContentMediaGallery contentId="content" />);
  await screen.findByText("1 / 2");
  expect(container.querySelector("video")?.getAttribute("src")).toBe("https://cdn.test/video.mp4");
  fireEvent.click(screen.getByRole("button", { name: "Next media" }));
  expect(container.querySelector("img")?.getAttribute("src")).toBe("https://cdn.test/image.jpg");
});
