# T09 — Rich Text và formatter

Hoàn thành phần phát triển ngày 09/09/2026. Đã kiểm tra code, editor, build web và PostgreSQL restore riêng; chưa migrate database nguồn hoặc nghiệm thu provider sandbox.

## Contract và hành vi

- `RichTextJson` là chuỗi JSON Tiptap, `RichTextVersion = 1`. Hai trường nullable cho dữ liệu cũ. Cột PostgreSQL: `rich_text_json` JSONB và `rich_text_version` integer.
- Khi nhận JSON, backend tự sinh `TextContent`; không tin văn bản do client gửi kèm. Response cung cấp cả `plainText` (alias của TextContent), `richTextJson`, `richTextVersion`. Giữ TextContent để không phá client mobile/AI cũ.
- Request update chỉ gửi TextContent là thao tác thay thế bằng văn bản thuần và xóa document cũ. Update không gửi cả hai trường nội dung giữ nguyên document. Document rỗng hợp lệ dùng doc/paragraph rỗng với version 1.
- Legacy không có JSON được editor đọc thành text literal với hardBreak. Không diễn giải HTML hoặc đoán Markdown. Migration không đổi text_content cũ. Nội dung Markdown đã lưu từ editor cũ vẫn giữ nguyên ký tự; muốn có định dạng mới, người dùng chỉnh lại trong editor rồi lưu. Không tự đoán dấu `*`, `<u>` hay `#` là markup.
- Editor hỗ trợ bold, italic, underline, strike, highlight, heading H2, bullet/numbered list, link, emoji, hashtag dạng text và undo/redo. Giữ số bắt đầu của ordered list. Tắt trailing-node tự động để undo không tự thêm đoạn trống.
- Tạo nội dung và sửa chi tiết/caption gửi JSON cùng version. Các lượt tạo bằng AI hoặc client cũ tiếp tục dùng text cho tới khi được chỉnh bằng editor.

## Schema và giới hạn

Root `doc`; block gồm paragraph, heading (level 1–3), blockquote, bulletList, orderedList và listItem theo cấu trúc cho phép. Inline gồm text/hardBreak. Marks: bold, italic, underline, strike, highlight, link.

Backend từ chối JSON lỗi, version khác 1, node/mark/property không hỗ trợ, cấu trúc cha/con sai, document trên 200.000 ký tự hoặc 5.000 node và độ sâu JSON vượt 32. Ordered list bắt đầu từ 1–9999; kiểu decimal mặc định. Chấp nhận các thuộc tính link mặc định của Tiptap để round-trip, nhưng preview không sử dụng class/style tùy ý từ document.

Link chỉ cho `http`, `https`, `mailto`, tối đa 2.048 ký tự, không control character. Javascript/data/URL tương đối bị từ chối. Preview dựng React elements từ allow-list và để React escape text; không dùng `dangerouslySetInnerHTML` hoặc đưa HTML legacy vào DOM. Link mở tab mới dùng noopener/noreferrer.

## Formatter và snapshot

Formatter v1 khai báo đầu ra cho Facebook, Instagram, TikTok và Google. Các caption hiện dùng fallback plain text: giữ Unicode, hashtag, đoạn, xuống dòng; giữ bullet và số thứ tự; link có nhãn được đổi thành `nhãn (URL)`. Bold/italic/highlight chỉ giữ ở editor, không chuyển thành HTML hoặc bộ ký tự Unicode giả định dạng.

Frontend đếm Unicode code point từ caption đã format, bao gồm URL và ký hiệu list. Đây là số ký tự theo contract AISAM, không khẳng định là cách tính quota của mọi provider. Giới hạn/capability provider và preview media đầy đủ vẫn thuộc T07/T10/T11.

Snapshot mới lưu RichTextJson, RichTextVersion, PlainText, FormatterVersion và FormattedCaptions theo platform trong payload được checksum. Worker dùng caption trong snapshot, không format lại draft hiện tại. Snapshot cũ không có caption map tiếp tục đọc bằng fallback cũ. Đổi riêng mark cũng đổi MediaVersion, bỏ approval hiện hành và đưa nội dung về Draft; snapshot của lịch đã tạo vẫn bất biến.

## Kiểm thử

- **9/9 test backend tập trung**: schema/link xấu, list/link/Unicode, text do server sinh, đổi định dạng làm mất approval, snapshot bất biến và provider nhận caption đã khóa.
- **505/508 backend toàn bộ đạt**; ba lỗi PromptEnhancer cũ còn tồn tại ngoài T09.
- **87/87 test web đạt**: có round-trip bằng Editor Tiptap thật, undo/redo, định dạng caption và preview không tạo element từ chuỗi XSS.
- **Build production Next.js đạt**; TypeScript đạt. Cảnh báo quy ước middleware hiện hữu không chặn build.
- **PostgreSQL restore đạt**: JSONB/version đọc lại, TextContent được sinh, caption đóng băng, sửa mark invalidation; downgrade/reapply toàn chuỗi migration đạt. Các cột dữ liệu cũ không đổi bởi migration. EF không còn pending model changes.

Migration: `20260909115446_AddRichTextDocument`. Chỉ thêm hai cột nullable; Down xóa document/version, giữ text_content nên mất phần định dạng mới nếu rollback. Không dùng rollback làm cách sửa dữ liệu nội dung.

## Kiểm tra thủ công khi triển khai

1. Áp dụng migration vào database được chọn, khởi động lại BE và FE.
2. Tạo nội dung có bold/highlight, numbered list, link và tiếng Việt/emoji; lưu rồi mở lại, kiểm tra định dạng còn nguyên.
3. Đổi caption ảnh/video; kiểm tra cùng document được lưu, không chỉ thay text.
4. Submit/approve, tạo lịch, sau đó chỉ sửa mark trong draft. Draft phải cần duyệt lại; lịch cũ vẫn trỏ snapshot ban đầu.
5. Kiểm tra platform preview: caption plain text có URL/số list, không có thẻ HTML định dạng. Nghiệm thu đăng thật trên sandbox tại T11.

Task tiếp theo: **T10 — Composer nhiều media và preview**.
