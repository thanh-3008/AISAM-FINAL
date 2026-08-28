import { beforeEach, describe, expect, it, vi } from "vitest";
import { MAX_MEDIA_FILE_SIZE_BYTES } from "@/lib/mediaUpload";

const { apiFetchMock } = vi.hoisted(() => ({ apiFetchMock: vi.fn() }));

vi.mock("@/lib/apiClient", () => ({
  apiClient: vi.fn(),
  apiFetch: apiFetchMock,
}));

import { uploadContentMedia } from "@/services/contentService";

function fileWithSize(size: number) {
  const file = new File(["video"], "product-demo.mp4", { type: "video/mp4" });
  Object.defineProperty(file, "size", { value: size });
  return file;
}

describe("uploadContentMedia", () => {
  beforeEach(() => apiFetchMock.mockReset());

  it("does not send an oversized file to the API", async () => {
    await expect(uploadContentMedia(fileWithSize(MAX_MEDIA_FILE_SIZE_BYTES + 1), "video"))
      .rejects.toThrow("Maximum allowed size is 50 MB");
    expect(apiFetchMock).not.toHaveBeenCalled();
  });

  it("maps a readable infrastructure 413 to a user-facing size error", async () => {
    apiFetchMock.mockRejectedValueOnce(Object.assign(new Error("413 Request Entity Too Large"), { status: 413 }));

    await expect(uploadContentMedia(fileWithSize(1024), "video"))
      .rejects.toThrow("Maximum allowed size is 50 MB");
  });
});
