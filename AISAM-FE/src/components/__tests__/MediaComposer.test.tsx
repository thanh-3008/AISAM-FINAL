import React from "react";
import { beforeEach, afterEach, expect, it, vi } from "vitest";
import { render, screen, fireEvent, waitFor, cleanup } from "@testing-library/react";
import MediaComposer from "../content/MediaComposer";
const mocks = vi.hoisted(() => ({ read: vi.fn(), save: vi.fn(), upload: vi.fn(), importLegacy: vi.fn() }));
vi.mock("@/services/composerService", () => ({ readMedia: mocks.read, saveMedia: mocks.save, uploadMediaItem: mocks.upload, importLegacyMedia: mocks.importLegacy }));
vi.mock("@/lib/auth", () => ({ getUserIdFromToken: () => "actor" }));
vi.mock("@/stores/workspace-store", () => ({ getStoredActiveWorkspace: () => ({ id: "workspace" }) }));
vi.mock("@/lib/composerFiles", () => ({ loadComposerFiles: async () => [], storeComposerFiles: async () => {} }));
const image = (id: string) => ({ assetId: id, url: `https://cdn.test/${id}`, mimeType: "image/png", isCover: false, sortOrder: 0 });
beforeEach(() => {
  sessionStorage.clear();
  vi.clearAllMocks();
  mocks.read.mockResolvedValue({ version: "v1", items: [] });
  mocks.importLegacy.mockResolvedValue({ version: "v2", items: [] });
});
afterEach(() => { cleanup(); vi.unstubAllGlobals(); });
it("retries only failed files and keeps successful uploads in draft", async () => {
  mocks.upload.mockResolvedValueOnce(image("a")).mockRejectedValueOnce(new Error("storage failed")).mockResolvedValueOnce(image("b"));
  render(<MediaComposer contentId="content" canEdit />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [new File(["a"], "a.png", { type: "image/png" }), new File(["b"], "b.png", { type: "image/png" })] } });
  await screen.findByText(/storage failed/);
  expect(mocks.upload).toHaveBeenCalledTimes(2);
  fireEvent.click(screen.getByText("Retry failed files"));
  await waitFor(() => expect(mocks.upload).toHaveBeenCalledTimes(3));
  expect(mocks.upload.mock.calls[2][1].name).toBe("b.png");
  await waitFor(() => expect(JSON.parse(sessionStorage.getItem("aisam-media-draft:actor:workspace:content")!).items).toHaveLength(2));
});
it("uploads selected files immediately", async () => {
  mocks.upload.mockResolvedValue(image("a"));
  render(<MediaComposer contentId="content" canEdit />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [new File(["a"], "a.png", { type: "image/png" })] } });
  await waitFor(() => expect(mocks.upload).toHaveBeenCalledTimes(1));
  await screen.findByRole("img");
});
it("keeps the current video when adding an image", async () => {
  const video = { ...image("video"), mimeType: "video/mp4", url: "https://cdn.test/video.mp4" };
  mocks.importLegacy.mockResolvedValue({ version: "v2", items: [video] });
  mocks.upload.mockResolvedValue(image("photo"));
  const { container } = render(<MediaComposer contentId="content" canEdit />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [new File(["a"], "photo.png", { type: "image/png" })] } });
  await waitFor(() => expect(container.querySelector("video")?.getAttribute("src")).toBe("https://cdn.test/video.mp4"));
  await waitFor(() => expect(container.querySelector("img")?.getAttribute("src")).toBe("https://cdn.test/photo"));
});
it("auto-saves a mixed collection with multiple images and videos", async () => {
  const currentVideo = { ...image("video-1"), mimeType: "video/mp4", url: "https://cdn.test/video-1.mp4" };
  const secondVideo = { ...image("video-2"), mimeType: "video/mp4", url: "https://cdn.test/video-2.mp4" };
  mocks.importLegacy.mockResolvedValue({ version: "v2", items: [currentVideo] });
  mocks.upload.mockResolvedValueOnce(image("photo")).mockResolvedValueOnce(secondVideo);
  mocks.save.mockImplementation(async (_id, collection) => ({ ...collection, version: "v3" }));
  const { container } = render(<MediaComposer contentId="content" canEdit compact autoSave fallbackVideoUrl={currentVideo.url} />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [
    new File(["a"], "photo.png", { type: "image/png" }),
    new File(["video"], "second.mp4", { type: "video/mp4" }),
  ] } });
  await waitFor(() => expect(mocks.save).toHaveBeenCalledTimes(1));
  expect(mocks.save.mock.calls[0][1].items.map((item: { assetId: string }) => item.assetId)).toEqual(["video-1", "photo", "video-2"]);
  await screen.findByText("2 / 3");
  expect(container.querySelector('.aspect-video > img[src="https://cdn.test/photo"]')).toBeTruthy();
  fireEvent.click(screen.getByRole("button", { name: "Next media" }));
  expect(screen.getByText("3 / 3")).toBeTruthy();
  expect(screen.queryByText("Lưu thay đổi")).toBeNull();
});
it("shows a selected image in the carousel before upload finishes", async () => {
  let finishUpload!: (value: ReturnType<typeof image>) => void;
  mocks.upload.mockReturnValue(new Promise(resolve => { finishUpload = resolve; }));
  const originalCreateObjectUrl = URL.createObjectURL;
  const originalRevokeObjectUrl = URL.revokeObjectURL;
  URL.createObjectURL = vi.fn(() => "blob:selected-image");
  URL.revokeObjectURL = vi.fn();
  const { container } = render(<MediaComposer contentId="content" canEdit compact autoSave fallbackVideoUrl="https://cdn.test/video.mp4" />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [new File(["a"], "selected.png", { type: "image/png" })] } });
  await screen.findByText("2 / 2");
  expect(screen.getByText("2/10")).toBeTruthy();
  await waitFor(() => expect(container.querySelector('.aspect-video > img[src="blob:selected-image"]')).toBeTruthy());
  finishUpload(image("selected"));
  await waitFor(() => expect(mocks.save).toHaveBeenCalledTimes(1));
  URL.createObjectURL = originalCreateObjectUrl;
  URL.revokeObjectURL = originalRevokeObjectUrl;
});
it("opens the stable hidden input used by the working Add Image control", async () => {
  let finishUpload!: (value: ReturnType<typeof image>) => void;
  mocks.upload.mockReturnValue(new Promise(resolve => { finishUpload = resolve; }));
  const originalCreateObjectUrl = URL.createObjectURL;
  const originalRevokeObjectUrl = URL.revokeObjectURL;
  URL.createObjectURL = vi.fn(() => "blob:windows-selected-image");
  URL.revokeObjectURL = vi.fn();
  const { container } = render(<MediaComposer contentId="content" canEdit compact autoSave fallbackVideoUrl="https://cdn.test/video.mp4" />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  const click = vi.spyOn(input as HTMLInputElement, "click").mockImplementation(() => {});
  fireEvent.click(screen.getByRole("button", { name: /Add Image or Video/i }));
  expect(click).toHaveBeenCalledTimes(1);
  fireEvent.change(input, { target: { files: [new File(["a"], "windows-photo.png", { type: "image/png" })] } });
  await screen.findByText("2/10");
  expect(container.querySelector('img[src="blob:windows-selected-image"]')).toBeTruthy();
  expect(mocks.upload).toHaveBeenCalledTimes(1);
  finishUpload(image("windows-photo"));
  await waitFor(() => expect(mocks.save).toHaveBeenCalledTimes(1));
  URL.createObjectURL = originalCreateObjectUrl;
  URL.revokeObjectURL = originalRevokeObjectUrl;
});
it("does not upload twice when native and React change handlers both run", async () => {
  mocks.upload.mockResolvedValue(image("single-upload"));
  mocks.save.mockImplementation(async (_id, collection) => collection);
  render(<MediaComposer contentId="content" canEdit compact autoSave />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [new File(["a"], "single.png", { type: "image/png" })] } });
  await waitFor(() => expect(mocks.upload).toHaveBeenCalledTimes(1));
});
it("shows remove for legacy images and deletes the matching image after import", async () => {
  const legacyImage = { ...image("legacy-photo"), url: "https://cdn.test/legacy-photo.jpg" };
  const legacyVideo = { ...image("legacy-video"), mimeType: "video/mp4", url: "https://cdn.test/legacy-video.mp4" };
  mocks.importLegacy.mockResolvedValue({ version: "v2", items: [legacyImage, legacyVideo] });
  mocks.save.mockImplementation(async (_id, collection) => ({ ...collection, version: "v3" }));
  render(<MediaComposer
    contentId="content"
    canEdit
    compact
    autoSave
    fallbackVideoUrl={legacyVideo.url}
    fallbackImages={[legacyImage.url]}
  />);
  const removeButton = await screen.findByRole("button", { name: "Remove image 2" });
  fireEvent.click(removeButton);
  await waitFor(() => expect(mocks.save).toHaveBeenCalledTimes(1));
  expect(mocks.save.mock.calls[0][1].items.map((item: { assetId: string }) => item.assetId)).toEqual(["legacy-video"]);
});
it("replaces a legacy image from its thumbnail action", async () => {
  const legacyImage = { ...image("legacy-photo"), url: "https://cdn.test/legacy-photo.jpg" };
  const legacyVideo = { ...image("legacy-video"), mimeType: "video/mp4", url: "https://cdn.test/legacy-video.mp4" };
  mocks.importLegacy.mockResolvedValue({ version: "v2", items: [legacyImage, legacyVideo] });
  mocks.upload.mockResolvedValue(image("replacement"));
  mocks.save.mockImplementation(async (_id, collection) => ({ ...collection, version: "v3" }));
  render(<MediaComposer contentId="content" canEdit compact autoSave fallbackVideoUrl={legacyVideo.url} fallbackImages={[legacyImage.url]} />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  const click = vi.spyOn(input as HTMLInputElement, "click").mockImplementation(() => {});
  fireEvent.click(screen.getByRole("button", { name: "Replace image 2" }));
  expect(click).toHaveBeenCalledTimes(1);
  fireEvent.change(input, { target: { files: [new File(["a"], "replacement.png", { type: "image/png" })] } });
  await waitFor(() => expect(mocks.save).toHaveBeenCalledTimes(1));
  expect(mocks.save.mock.calls[0][1].items.map((item: { assetId: string }) => item.assetId)).toEqual(["replacement", "legacy-video"]);
});
it("keeps the selected preview visible when legacy import fails", async () => {
  mocks.importLegacy.mockRejectedValue(new Error("import unavailable"));
  const originalCreateObjectUrl = URL.createObjectURL;
  URL.createObjectURL = vi.fn(() => "blob:kept-image");
  const { container } = render(<MediaComposer contentId="content" canEdit compact autoSave fallbackVideoUrl="https://cdn.test/video.mp4" />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [new File(["a"], "kept.png", { type: "image/png" })] } });
  await screen.findByText("Could not prepare the current media. Please try again.");
  expect(container.querySelector('img[src="blob:kept-image"]')).toBeTruthy();
  expect(screen.getByText("2/10")).toBeTruthy();
  URL.createObjectURL = originalCreateObjectUrl;
});
it("uploads files when randomUUID is unavailable", async () => {
  vi.stubGlobal("crypto", {});
  mocks.upload.mockResolvedValue(image("fallback"));
  render(<MediaComposer contentId="content" canEdit />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [new File(["a"], "fallback.png", { type: "image/png" })] } });
  await waitFor(() => expect(mocks.upload).toHaveBeenCalledTimes(1));
});
it("infers the image MIME type when the browser leaves it empty", async () => {
  mocks.upload.mockResolvedValue(image("mime-fallback"));
  render(<MediaComposer contentId="content" canEdit />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: [new File(["a"], "photo.jpg")] } });
  await waitFor(() => expect(mocks.upload).toHaveBeenCalledTimes(1));
  expect(mocks.upload.mock.calls[0][1].type).toBe("image/jpeg");
});
it("reorders media and retains the draft on version conflict", async () => {
  mocks.read.mockResolvedValue({ version: "v1", items: [image("a"), image("b")] });
  mocks.save.mockRejectedValue(new Error("MEDIA_VERSION_CONFLICT"));
  render(<MediaComposer contentId="content" canEdit />);
  const buttons = await screen.findAllByRole("button", { name: "Xuống" }); fireEvent.click(buttons[0]);
  fireEvent.click(screen.getByText("Lưu thay đổi"));
  await screen.findByRole("alert");
  expect(mocks.save.mock.calls[0][1].items.map((m: { assetId: string }) => m.assetId)).toEqual(["b", "a"]);
  expect(sessionStorage.getItem("aisam-media-draft:actor:workspace:content")).toContain("v1");
});
it("restores saved draft media only after explicit review", async () => {
  sessionStorage.setItem("aisam-media-draft:actor:workspace:content", JSON.stringify({ version: "old", items: [image("saved")] }));
  render(<MediaComposer contentId="content" canEdit />);
  await screen.findByText("Khôi phục");
  expect(screen.queryByRole("img")).toBeNull();
  fireEvent.click(screen.getByText("Khôi phục"));
  await screen.findByRole("img");
  expect(JSON.parse(sessionStorage.getItem("aisam-media-draft:actor:workspace:content")!).version).toBe("v1");
});
it("rejects too many media without silently truncating the selection", async () => {
  render(<MediaComposer contentId="content" canEdit />);
  const input = await screen.findByLabelText("Media files");
  await waitFor(() => expect((input as HTMLInputElement).disabled).toBe(false));
  fireEvent.change(input, { target: { files: Array.from({ length: 11 }, (_, i) => new File(["a"], `${i}.png`, { type: "image/png" })) } });
  await screen.findByText(/A maximum of 10 media files is allowed/);
  expect(mocks.upload).not.toHaveBeenCalled();
});
