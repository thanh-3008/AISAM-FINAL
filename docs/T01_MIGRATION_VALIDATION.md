# T01 — Kiểm thử migration PostgreSQL

Ngày 07/09/2026. Công cụ: `AISAM-BE/tools/PermissionMigrationCheck`.

Cập nhật 08/09/2026: đã bổ sung kiểm thử backup/restore dữ liệu thật và EF migration history bằng PermissionBackupCheck; kết quả cuối tại [nghiệm thu T01](T01_COMPLETION.md). Các giới hạn bên dưới mô tả riêng bộ fixture ban đầu.

Chạy thành công trên cụm PostgreSQL 18 riêng tại `.artifacts/permission-pg-test`, bind `127.0.0.1:55439`. Không đọc `.env`, không kết nối database dự án. Cụm đã dừng sau kiểm tra.

Công cụ dùng EF IMigrationsSqlGenerator để lấy SQL từ UpOperations của `ReconcilePermissionFoundation` và `AddAutomationCreatorAttribution`. Mỗi fixture có schema tối thiểu trong transaction riêng và rollback cả schema.

| Tình huống | Kết quả |
|---|---|
| Schema cũ: backfill Team từ Profile hợp lệ, Creator cũ giữ null | Đạt |
| Schema đã hòa giải: chạy lại foundation không trùng cột/bảng | Đạt |
| Kênh thuộc Brand khác | Từ chối, SQLSTATE P0001 |
| Không xác định được workspace của Team | Từ chối, SQLSTATE P0001 |
| Team–Brand chéo workspace | Từ chối, SQLSTATE P0001 |
| Assignment Team–Brand trùng | Từ chối, SQLSTATE 23505 |
| Sau rollback không còn bảng fixture | Đạt |

## Chạy lại

Cần cụm thử nghiệm riêng ở địa chỉ cố định trên; không trỏ công cụ sang production. Từ gốc repository:

```powershell
& 'C:\Program Files\PostgreSQL\18\bin\pg_ctl.exe' -D "$PWD\.artifacts\permission-pg-test" -l "$PWD\.artifacts\permission-pg-test.log" -o '-h 127.0.0.1 -p 55439' -w start
try {
    & 'C:\Users\thanh\.dotnet\dotnet.exe' run --project AISAM-BE/tools/PermissionMigrationCheck/PermissionMigrationCheck.csproj
} finally {
    & 'C:\Program Files\PostgreSQL\18\bin\pg_ctl.exe' -D "$PWD\.artifacts\permission-pg-test" -m fast -w stop
}
```

Đường dẫn executable là riêng máy hiện tại. Cụm fixture dùng trust authentication; chỉ khởi động khi kiểm thử trên máy phát triển.

## Giới hạn

Đây là kiểm thử SQL migration trên dữ liệu giả lập, không phải restore backup thật, kiểm thử toàn bộ lịch sử migration, hoặc thử Down. Chưa chứng nhận PostgreSQL 16 của Docker Compose. Chưa kiểm tra tải lớn, race condition, index/constraint trùng tên nhưng khác định nghĩa. Các yêu cầu này còn mở trong T01; chưa áp dụng migration lên database dự án.
