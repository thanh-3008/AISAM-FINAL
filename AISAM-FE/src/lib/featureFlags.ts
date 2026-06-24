export const FEATURE_FLAGS = {
  INSTAGRAM_CONNECT: false,
  TIKTOK_CONNECT: false,
  AI_IMAGE_GENERATION: false,
  AI_VIDEO_GENERATION: false,
  MEDIA_UPLOAD: false,
  AI_RECOMMENDATIONS: false,
} as const;

export function isFeatureEnabled(flag: keyof typeof FEATURE_FLAGS): boolean {
  return FEATURE_FLAGS[flag];
}
