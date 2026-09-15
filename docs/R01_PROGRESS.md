# R01 — Mô hình và migration RBAC hai tầng

Trạng thái: **hoàn thành phần mô hình/migration và kiểm tra R01**, ngày 14/09/2026. Chưa bật policy v2; enforcement và giao diện thuộc R02 trở đi.

## Đầu ra

- [x] WorkspaceRoleV2 và cột riêng cho membership; Owner giữ Owner, role cũ khác Owner chuyển Member; unknown giữ null, không tự nâng quyền.
- [x] WorkspaceInvitation có role v2; lời mời legacy hợp lệ chuyển Member, không tạo lời mời Owner/WorkspaceManager tự động.
- [x] Team.DefaultForBrandId có FK và unique index; backfill bài thiếu TeamId vào Default Team đúng Brand/workspace, không thêm thành viên và không đổi tác giả.
- [x] ScopeEnabledV2 trên TeamChannelAccess mặc định false. Không tái tạo bảng TeamBrandChannel trùng chức năng; snapshot quyền lẻ cũ và view rbac_v2_channel_diff phục vụ rà soát.
- [x] Check constraint role và trigger chặn bật scope kênh sai Brand/workspace/trạng thái. View rbac_v2_preflight báo dữ liệu thiếu Team, membership không hợp lệ, liên kết khác workspace và scope chờ duyệt.
- [x] Rollback chỉ gỡ attribution do migration tạo và còn nguyên; từ chối tự xóa Team đã có thành viên/nội dung mới/kênh được phân công.
- [x] Kiểm tra thực thi SQL migration trên bản sao aisam_local và schema mới rỗng; backfill chạy lại và rollback đạt.

## Bằng chứng kiểm tra

Lệnh: `dotnet run --project AISAM-BE/tools/RbacV2MigrationCheck/RbacV2MigrationCheck.csproj` với PGPASSWORD cấu hình cục bộ.

Công cụ khóa đích vào `127.0.0.1:5432/aisam_r01_verification`, không đọc .env. Bản sao được tạo bằng pg_dump/pg_restore; dữ liệu nguồn có 43 Content. Mọi ca kiểm tra rollback transaction.

Kết quả: không tự nâng Manager hoặc lời mời; scope kênh mặc định đóng; backfill bài hợp lệ; giữ TeamId đã có và PrimaryCreatorId; từ chối role v2 ngoài tập; chạy lại không nhân bản Team/không thêm thành viên; rollback khôi phục attribution. Cả hai chế độ backup/fresh đều đạt.

Bản sao có **2 channel_scope_needs_review**, không có nhóm ngoại lệ khác trong báo cáo. Đây là hai scope legacy cần rà soát trước bật v2, không tự phê duyệt để làm báo cáo trống. Truy vấn `SELECT * FROM rbac_v2_channel_diff` sau migration để xem IDs và quyền trước/sau; không chứa token.

Giới hạn: ca fresh tạo schema từ EF model rồi đưa về cấu trúc trước hai migration v2, không chứng nhận toàn bộ lịch sử migration cũ từ database trống. Bản sao local cần migration tiền nhiệm MigrateRbacSchemaAndEnums trước v2; harness cũng kiểm tra bước đó. Chưa chạy giao dịch social hoặc nghiệm thu UI.

## Quy tắc triển khai tiếp

1. Backup database đích và dừng tiến trình ghi trong cửa sổ migration.
2. Áp dụng migration tiền nhiệm còn thiếu, AddWorkspaceRoleV2 và PrepareTeamRbacV2 qua công cụ EF của dự án.
3. Đọc rbac_v2_preflight và rbac_v2_channel_diff. Không bật scope theo phép OR quyền cũ; Owner đối chiếu tác động role mới trước khi cấp.
4. R02 phải dùng role/scope v2 và kiểm tra Team/TeamBrand hoạt động ở mỗi request/job; trigger cấp scope không thay thế kiểm tra quyền lúc sử dụng.
5. Trạng thái thiết kế DEACTIVATED ánh xạ TeamStatusEnum.Inactive=1 hiện có; không đổi giá trị Archived=2. R04 thực thi thu hồi liên kết nguyên tử khi deactivate.
6. Schema đã chuẩn bị nhưng việc cấp scope, đổi WorkspaceManager và cập nhật invitation accept thuộc policy/API tiếp theo. Null role không được cấp quyền v2.

**Chưa áp dụng lên aisam_local.** Khi chạy binary mới có các cột này phải áp dụng migration trước; chưa thể dùng bản model mới với database cũ. Đây là hoàn thành R01 trên môi trường kiểm thử, không phải hoàn thành rollout R12.

Bản dump kiểm tra nằm trong thư mục TEMP của máy; database aisam_r01_verification giữ bản sao trước thay đổi, không dùng làm môi trường ứng dụng. Khi rollback sau khi Team đã được sử dụng, dùng backup đã kiểm chứng thay vì xóa lịch sử.