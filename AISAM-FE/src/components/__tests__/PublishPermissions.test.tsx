import React from "react";
import { afterEach, beforeEach, expect, it, vi } from "vitest";
import { cleanup, render, screen, fireEvent, waitFor } from "@testing-library/react";
import PostNowModal from "../content/PostNowModal";
const mocks = vi.hoisted(() => ({ preview: vi.fn(), start: vi.fn(), read: vi.fn() }));
vi.mock("@/services/composerService", () => ({ previewPublish: mocks.preview, startPublish: mocks.start, readPublish: mocks.read }));
vi.mock("@/lib/auth", () => ({ getUserIdFromToken: () => "actor" }));
vi.mock("@/stores/workspace-store", () => ({ getStoredActiveWorkspace: () => ({ id: "workspace" }) }));
const props = { contentId: "content", onClose: () => {}, onSuccess: () => {} };
beforeEach(() => {
  localStorage.clear(); vi.clearAllMocks();
  mocks.preview.mockResolvedValue({ version: "v1", approved: true, media: [], destinations: [
    { id: "allowed", name: "Allowed channel", platform: "facebook", error: null, caption: "#AISAM", capability: { maxItems: 10 } },
    { id: "denied", name: "View only channel", platform: "facebook", error: "ACCESS_DENIED_CHANNEL", caption: "#AISAM", capability: { maxItems: 10 } },
  ] });
  mocks.read.mockResolvedValue([]);
});
afterEach(cleanup);
it("keeps incompatible channels visible but cannot select them", async () => {
  render(<PostNowModal {...props} />);
  const denied = await screen.findByRole("checkbox", { name: /View only/ });
  expect((denied as HTMLInputElement).disabled).toBe(true);
  expect(screen.getByRole("checkbox", { name: /Allowed/ }).hasAttribute("disabled")).toBe(false);
});
it("persists an attempt before timeout and reopens without submitting again", async () => {
  mocks.start.mockRejectedValue(new Error("timeout"));
  const view = render(<PostNowModal {...props} />);
  fireEvent.click(await screen.findByRole("checkbox", { name: /Allowed/ }));
  fireEvent.click(screen.getByRole("button", { name: /Đăng lên 1/ }));
  await screen.findByText(/timeout/);
  expect(mocks.start).toHaveBeenCalledTimes(1);
  expect(localStorage.getItem("aisam-publish:actor:workspace:content")).toContain("allowed");
  view.unmount(); render(<PostNowModal {...props} />);
  await screen.findByRole("button", { name: "Kiểm tra kết quả" });
  expect(screen.queryByRole("button", { name: /Đăng lên/ })).toBeNull();
  fireEvent.click(screen.getByRole("button", { name: "Kiểm tra kết quả" }));
  await waitFor(() => expect(mocks.read).toHaveBeenCalled());
  expect(mocks.start).toHaveBeenCalledTimes(1);
});
it("unknown outcome cannot prepare a fresh retry", async () => {
  mocks.start.mockResolvedValue([{ id: "op", integrationId: "allowed", status: "NeedsAttention", errorCode: "PUBLISH_OUTCOME_UNKNOWN" }]);
  render(<PostNowModal {...props} />);
  fireEvent.click(await screen.findByRole("checkbox", { name: /Allowed/ }));
  fireEvent.click(screen.getByRole("button", { name: /Đăng lên 1/ }));
  await screen.findByText("NeedsAttention");
  expect(screen.queryByRole("button", { name: /Chuẩn bị/ })).toBeNull();
});
it("prepares only the failed destination after partial publication", async () => {
  mocks.preview.mockResolvedValue({ version: "v1", approved: true, media: [], destinations: ["one", "two"].map(id =>
    ({ id, name: id, platform: "facebook", error: null, caption: "caption", capability: { maxItems: 10 } })) });
  mocks.start.mockResolvedValueOnce([
    { id: "op1", integrationId: "one", status: "Published" },
    { id: "op2", integrationId: "two", status: "Failed", errorCode: "MEDIA_TYPE_UNSUPPORTED" },
  ]).mockResolvedValueOnce([{ id: "op3", integrationId: "two", status: "Published" }]);
  render(<PostNowModal {...props} />);
  fireEvent.click(await screen.findByRole("checkbox", { name: /one/ }));
  fireEvent.click(screen.getByRole("checkbox", { name: /two/ }));
  fireEvent.click(screen.getByRole("button", { name: /Đăng lên 2/ }));
  fireEvent.click(await screen.findByRole("button", { name: /Chuẩn bị/ }));
  const retry = await screen.findByRole("button", { name: /Đăng lên 1/ });
  expect((screen.getByRole("checkbox", { name: /one/ }) as HTMLInputElement).disabled).toBe(true);
  fireEvent.click(retry);
  await screen.findByRole("button", { name: "Hoàn tất" });
  expect(mocks.start.mock.calls[1][2]).toEqual(["two"]);
  expect(mocks.start.mock.calls[1][3]).not.toBe(mocks.start.mock.calls[0][3]);
});
