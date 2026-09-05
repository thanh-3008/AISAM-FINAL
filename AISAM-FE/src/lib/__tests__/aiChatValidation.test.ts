import { describe, expect, it } from "vitest";
import { MISSING_AI_CHAT_BRAND_MESSAGE, validateAiChatBrand } from "@/lib/aiChatValidation";

describe("AI chat Brand validation", () => {
  it.each(["", "   "])("blocks submission when Brand is missing (%j)", (brandId) => {
    expect(validateAiChatBrand(brandId)).toBe(MISSING_AI_CHAT_BRAND_MESSAGE);
    expect(validateAiChatBrand(brandId)).toContain("chưa chọn Brand");
  });

  it("allows submission when a Brand is selected", () => {
    expect(validateAiChatBrand("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")).toBeNull();
  });
});
