# T11 — Điều kiện đóng task

> Cập nhật phạm vi 10/09/2026: T11 đã hoàn thành phát triển theo yêu cầu người dùng làm xong code trước, nghiệm thu tổng thể sau T12. Các trạng thái chưa đóng dưới đây phản ánh tiêu chí nghiệm thu ban đầu và vẫn được giữ để theo dõi đợt cuối. Xem [T11_COMPLETION.md](T11_COMPLETION.md).

Ngày cập nhật: 10/09/2026. **Chưa đủ điều kiện đóng T11 theo kế hoạch gốc.**

## Bằng chứng đã có

- Backend 516/516 test đạt; `.artifacts/t11/t11-backend-20260910.trx`.
- Web E2E 21/21 đạt trên production build với API giả lập (lượt trước).
- PostgreSQL restore: checksum bảy bảng, migration/rollback/reapply, trigger, scope, snapshot, journal và worker cạnh tranh đạt.
- Seed 10.000 Content, hai Brand/Team và đủ năm actor; phân trang và Creator isolation đạt.
- 200 lượt list đồng thời; 100 lượt analytics có 200 Post/400 reports, xác minh snapshot mới nhất và các chỉ số theo Creator đạt.
- Upload vượt số file/tổng dung lượng bị từ chối trước storage. Lô đúng 200 MiB qua service đọc đủ byte, SHA256 chính xác và tạo bốn Asset độc lập. Test dùng dữ liệu tổng hợp có signature và storage giả lập đọc toàn bộ stream, không phải file media đã được decoder xác minh hay upload HTTP/CDN thật.

Chi tiết, lịch sử số đo và giới hạn: [T11_VALIDATION.md](T11_VALIDATION.md).

## Các điều kiện chưa đạt

| Điều kiện | Cần thêm để đóng |
|---|---|
| Upload qua HTTP/storage thật | Môi trường staging và storage kiểm thử; thực hiện multipart hợp lệ tới giới hạn, đo thời gian/RAM, lỗi từng file và cleanup |
| Migration/đối soát staging | Database staging xác định; backup/restore/rollback, migration history và count/checksum; dữ liệu restore cục bộ không thay thế bước này |
| Legacy | Phân loại 542 Content, 36 automation thiếu creator và một Post lệch Brand/kênh; xác nhận giữ legacy bị hạn chế hoặc sửa bằng bằng chứng attribution, không gán Owner tự động |
| Social/Google/checkout demo | Kênh và tài khoản sandbox được phép thao tác; thực hiện demo xuyên suốt, ghi operation/external post/payment ID và kết quả revoke/timeout |
| Hồ sơ release | Đóng băng commit được triển khai; đối chiếu Swagger/ERD từ chính bản đó với database staging; ghi giới hạn capability và kết quả nghiệm thu |

Không có staging/sandbox URL hoặc thông tin truy cập được cung cấp trong phiên này. Không đổi định nghĩa hoàn thành thành “chỉ kiểm thử cục bộ” khi chưa có quyết định phạm vi. Không đánh dấu task hoàn thành chỉ vì các test tự động đạt.

Migration bổ sung cần đưa vào release: `20260910010000_FixPermissionTriggerRecordAccess`; [chi tiết](T11_TRIGGER_FIX.md). Migration chưa được áp dụng lên database nguồn. Toàn bộ thao tác restore/seed trong đợt kiểm thử chạy trên PostgreSQL cô lập, không đăng bài hoặc thanh toán thật.
