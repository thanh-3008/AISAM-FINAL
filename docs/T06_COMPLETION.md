# T06 — Media collection, upload và snapshot

Hoàn thành phạm vi dữ liệu/API ngày 09/09/2026. Database nguồn chưa áp dụng migration; chưa gọi Cloudinary hoặc đăng bài thật để nghiệm thu provider.

## Phần đã triển khai

- `ContentMedia`: danh sách AssetId có thứ tự, cover, alt text và caption. Giới hạn 10 asset khác nhau; thứ tự liên tục từ 0; tối đa một cover.
- `Asset`: workspace/brand/uploader do server xác định, MIME, kích thước, SHA-256 và storage public ID; metadata kích thước ảnh/thời lượng có thể chưa biết.
- Batch upload: tối đa 10 file, 200 MiB tổng, 50 MiB/file; kiểm tra quyền sửa Content, MIME và magic bytes. Kết quả độc lập từng item. Tên storage được tạo bằng UUID. Endpoint upload cũ cũng kiểm tra signature và không trả lỗi storage nội bộ.
- Lưu/reorder kiểm tra version, tenant/brand và quyền dùng asset. Giao dịch serializable trên PostgreSQL; version cũ nhận 409. Không thay ownership theo request.
- `PublishSnapshot` và `SnapshotMedia`: chụp nội dung text và media khi gửi duyệt; duyệt giữ snapshot đó; lịch gắn snapshot đã duyệt. Sửa draft làm mất trạng thái duyệt của draft nhưng giữ nguyên snapshot cũ.
- Publish dùng snapshot đã duyệt, worker dùng snapshot gắn lịch. Nội dung/lịch cũ chưa có snapshot phải gửi duyệt và tạo lịch lại. Không tự lấy draft hiện tại để thay payload của lịch cũ.
- `PostMedia`: lưu liên kết snapshot media, trạng thái, provider media ID và mã lỗi. Luồng publish hiện tại ghi Published sau khi provider báo thành công; provider ID từng media và partial failure orchestration sẽ được nối trong T07.
- Import legacy từ URL đã có trên Content được phép sửa; không nhận URL import tùy ý và không tải URL từ server. Luồng legacy cập nhật ảnh/video thay thế collection draft, không sửa snapshot.
- Cleanup mỗi giờ, TTL 24 giờ: chỉ xử lý asset có storage public ID, không còn được draft hoặc snapshot tham chiếu; đánh dấu expired trước khi xóa storage, giữ tombstone và thử lại khi chưa xóa thành công. Không xóa asset legacy chưa biết storage ID.
- Ràng buộc SQL chặn sửa/xóa snapshot, đổi danh tính asset, gắn asset đã expired và expire asset còn tham chiếu.

## API để test

Cần JWT AISAM, `X-Workspace-Id` và quyền với Content. Base route: `/api/content/{contentId}/media`.

| Method | Path bổ sung | Request/kết quả |
|---|---|---|
| GET | Không | Version và collection đã sắp thứ tự |
| POST | `/upload` | Multipart `files`; kết quả index, assetId/url hoặc error từng file |
| PUT | Không | `expectedVersion`, `items` gồm assetId, sortOrder, isCover, altText, caption |
| POST | `/import-legacy` | `expectedVersion`; chuyển URL đang có thành collection |
| GET | `/snapshots` | Danh sách snapshot và media được phép xem |

Ví dụ body PUT, thay GUID bằng giá trị thật từ GET/upload:

```json
{
  "expectedVersion": "VERSION_FROM_GET",
  "items": [
    { "assetId": "ASSET_ID", "sortOrder": 0, "isCover": true, "altText": "Ảnh sản phẩm", "caption": "" }
  ]
}
```

Trình tự test: tạo draft → upload → GET lấy version → PUT chọn/reorder → GET xác nhận → submit/approve bằng API hiện hữu → xem snapshots → sửa draft → kiểm tra snapshot vẫn giữ nội dung trước đó. Upload không tự gắn file vào draft; PUT mới chọn các asset cần dùng.

## Bằng chứng kiểm tra

- 19/19 test tập trung: ContentMediaTests, ContentControllerTests, PermissionIntegrityTests, ResourcePermissionFilterTests.
- Bộ BE tổng: 473/476 đạt. Ba lỗi PromptEnhancerTests đã có từ trước: enhanced English và hai trường hợp fallback Vietnamese. Chưa coi toàn bộ regression xanh.
- Build thành công; `dotnet ef migrations has-pending-model-changes` không phát hiện lệch model.
- `tools/PermissionBackupCheck`: restore backup vào PostgreSQL localhost:55439; áp dụng toàn bộ pending migrations thành công. Kiểm tra 5 ảnh reorder/reload, snapshot không đổi theo draft, cleanup giữ asset snapshot và SQL từ chối sửa snapshot/expire asset còn tham chiếu.
- Downgrade T06 về T05 rồi reapply thành công trên bản sao cô lập. Downgrade xóa dữ liệu collection/snapshot mới; không dùng như phương án rollback không mất dữ liệu sau khi đã đưa vào sử dụng.
- Không thay đổi database nguồn. Không chạy lại test FE vì task này không sửa FE.

## Migration và giới hạn

Migration mới: `20260908222838_AddMediaCollectionsAndSnapshots`. Cần triển khai cùng các migration T01–T05 trước khi dùng API mới. Backup và kiểm tra database đích trước khi migrate; worker cũng cần schema này.

Snapshot legacy cố định URL, không bảo đảm bên lưu trữ ngoài hệ thống không thay byte tại URL đó. Kiểm tra magic bytes không thay thế giải mã/transcode hay kiểm tra capability của nền tảng. Width/height/duration chưa được suy đoán từ tên file.

Nếu storage upload thành công nhưng ghi database thất bại, file ngoài storage có thể chưa được ghi nhận để cleanup tự động; cần đối soát storage theo folder khi gặp lỗi đó. Cleanup hiện áp dụng cho asset đã được ghi nhận; không tuyên bố giao dịch phân tán với Cloudinary.

Nhiều video/mixed media lưu được ở collection nhưng đang bị chặn publish rõ ràng cho đến T07. Chưa có UI composer mới (T10), chưa nghiệm thu sandbox provider (T07/T11), chưa phải bản bàn giao toàn hệ thống.
