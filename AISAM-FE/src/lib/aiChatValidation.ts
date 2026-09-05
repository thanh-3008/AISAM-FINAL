export const MISSING_AI_CHAT_BRAND_MESSAGE = "Bạn chưa chọn Brand.";

export function validateAiChatBrand(brandId: string): string | null {
  return brandId.trim() ? null : MISSING_AI_CHAT_BRAND_MESSAGE;
}
