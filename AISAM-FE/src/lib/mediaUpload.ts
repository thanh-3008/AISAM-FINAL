export const MAX_MEDIA_FILE_SIZE_MB = 50;
export const MAX_MEDIA_FILE_SIZE_BYTES = MAX_MEDIA_FILE_SIZE_MB * 1024 * 1024;

export type MediaKind = "image" | "video";

const ALLOWED_IMAGE_TYPES = new Set([
  "image/jpeg",
  "image/png",
  "image/webp",
  "image/gif",
]);

const ALLOWED_VIDEO_TYPES = new Set([
  "video/mp4",
  "video/webm",
  "video/quicktime",
]);

export function validateMediaFile(file: File, expectedKind?: MediaKind): string | null {
  const mediaLabel = expectedKind ?? (file.type.startsWith("video/") ? "video" : "image");

  if (file.size > MAX_MEDIA_FILE_SIZE_BYTES) {
    return `The selected ${mediaLabel} is too large. Maximum allowed size is ${MAX_MEDIA_FILE_SIZE_MB} MB.`;
  }

  if (expectedKind === "image" && !ALLOWED_IMAGE_TYPES.has(file.type)) {
    return "Unsupported image format. Please upload JPEG, PNG, WebP, or GIF.";
  }

  if (expectedKind === "video" && !ALLOWED_VIDEO_TYPES.has(file.type)) {
    return "Unsupported video format. Please upload MP4, WebM, or MOV.";
  }

  if (!expectedKind && !ALLOWED_IMAGE_TYPES.has(file.type) && !ALLOWED_VIDEO_TYPES.has(file.type)) {
    return "Unsupported media format. Please upload a supported image or video file.";
  }

  return null;
}

export function assertValidMediaFile(file: File, expectedKind?: MediaKind): void {
  const validationError = validateMediaFile(file, expectedKind);
  if (validationError) throw new Error(validationError);
}
