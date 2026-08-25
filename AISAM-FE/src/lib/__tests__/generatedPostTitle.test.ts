import { describe, expect, it } from "vitest";
import { deriveTitleFromCaption } from "@/lib/generatedPostTitle";

describe("deriveTitleFromCaption", () => {
  it("uses the leading uppercase caption headline instead of the AI title", () => {
    const caption = `New product introduction
🔥 TẠO DẤU ẤN ĐƯỜNG PHỐ CỰC CHẤT
CÙNG NIKE AIR MAX 95 BIG BUBBLE!

Các sneakerhead đã sẵn sàng đón nhận sự trở lại đầy bứt phá?`;

    expect(deriveTitleFromCaption(caption)).toBe(
      "🔥 TẠO DẤU ẤN ĐƯỜNG PHỐ CỰC CHẤT CÙNG NIKE AIR MAX 95 BIG BUBBLE!",
    );
  });

  it("uses the caption first sentence when there is no uppercase headline", () => {
    expect(deriveTitleFromCaption("Caption: Khám phá sản phẩm mới hôm nay. Đừng bỏ lỡ!")).toBe(
      "Khám phá sản phẩm mới hôm nay.",
    );
  });

  it("ignores an explicit AI title label", () => {
    expect(deriveTitleFromCaption("Title: New product introduction\nCaption: Nội dung caption thực tế."))
      .toBe("Nội dung caption thực tế.");
  });
});
