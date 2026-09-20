import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import MixedMediaGallery from "../content/MixedMediaGallery";

afterEach(cleanup);

it("moves from the video to an image with the next arrow", () => {
  const { container } = render(<MixedMediaGallery videoUrl="https://cdn.test/video.mp4" images={["https://cdn.test/photo.jpg"]} />);
  expect(container.querySelector("video")?.getAttribute("src")).toBe("https://cdn.test/video.mp4");
  fireEvent.click(screen.getByRole("button", { name: "Next media" }));
  expect(container.querySelector("img")?.getAttribute("src")).toBe("https://cdn.test/photo.jpg");
  expect(screen.getByText("2 / 2")).toBeTruthy();
});

it("keeps carousel controls and a thumbnail visible for one video", () => {
  const { container } = render(<MixedMediaGallery videoUrl="https://cdn.test/only.mp4" images={[]} />);
  expect(screen.getByText("1 / 1")).toBeTruthy();
  expect((screen.getByRole("button", { name: "Previous media" }) as HTMLButtonElement).disabled).toBe(true);
  expect((screen.getByRole("button", { name: "Next media" }) as HTMLButtonElement).disabled).toBe(true);
  expect(container.querySelectorAll("video")).toHaveLength(2);
});

it("provides fullscreen and remove actions on every image thumbnail", () => {
  const onRemove = vi.fn();
  const onReplace = vi.fn();
  render(<MixedMediaGallery images={[]} media={[
    { url: "https://cdn.test/one.jpg", mimeType: "image/jpeg" },
    { url: "https://cdn.test/two.jpg", mimeType: "image/jpeg" },
  ]} onRemove={onRemove} onReplace={onReplace} />);
  fireEvent.click(screen.getByRole("button", { name: "Enlarge image 1" }));
  expect(screen.getByTitle("Close (Esc)")).toBeTruthy();
  fireEvent.click(screen.getByTitle("Close (Esc)"));
  fireEvent.click(screen.getByRole("button", { name: "Replace image 1" }));
  expect(onReplace).toHaveBeenCalledWith(0);
  fireEvent.click(screen.getByRole("button", { name: "Remove image 2" }));
  expect(onRemove).toHaveBeenCalledWith(1);
});
