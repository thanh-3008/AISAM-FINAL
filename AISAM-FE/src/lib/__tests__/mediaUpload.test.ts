import { describe, expect, it } from "vitest";
import {
  MAX_MEDIA_FILE_SIZE_BYTES,
  MAX_MEDIA_FILE_SIZE_MB,
  validateMediaFile,
} from "@/lib/mediaUpload";

function fileWithSize(name: string, type: string, size: number) {
  const file = new File(["content"], name, { type });
  Object.defineProperty(file, "size", { value: size });
  return file;
}

describe("media upload validation", () => {
  it("accepts supported images and videos at the 50 MB boundary", () => {
    expect(validateMediaFile(fileWithSize("image.webp", "image/webp", MAX_MEDIA_FILE_SIZE_BYTES), "image")).toBeNull();
    expect(validateMediaFile(fileWithSize("demo.mp4", "video/mp4", MAX_MEDIA_FILE_SIZE_BYTES), "video")).toBeNull();
  });

  it("rejects files larger than the shared application limit", () => {
    expect(validateMediaFile(fileWithSize("demo.mp4", "video/mp4", MAX_MEDIA_FILE_SIZE_BYTES + 1), "video"))
      .toBe(`The selected video is too large. Maximum allowed size is ${MAX_MEDIA_FILE_SIZE_MB} MB.`);
    expect(validateMediaFile(fileWithSize("image.png", "image/png", MAX_MEDIA_FILE_SIZE_BYTES + 1), "image"))
      .toBe(`The selected image is too large. Maximum allowed size is ${MAX_MEDIA_FILE_SIZE_MB} MB.`);
  });

  it("rejects formats that the backend does not accept", () => {
    expect(validateMediaFile(fileWithSize("demo.avi", "video/x-msvideo", 1024), "video"))
      .toContain("MP4, WebM, or MOV");
    expect(validateMediaFile(fileWithSize("image.bmp", "image/bmp", 1024), "image"))
      .toContain("JPEG, PNG, WebP, or GIF");
  });
});
