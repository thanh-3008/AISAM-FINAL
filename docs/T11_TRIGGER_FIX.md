# T11 — Sửa trigger phân quyền khi tạo Brand/Team

Ngày 10/09/2026, seed dữ liệu tải trên PostgreSQL restore phát hiện lỗi `record "new" has no field "is_active"` khi INSERT Brand.

Hàm trigger `aisam_permission_integrity()` dùng chung cho nhiều bảng. Điều kiện `TG_TABLE_NAME='team_members' AND NEW.is_active` vẫn khiến PostgreSQL phân giải field của record không có cột đó. Vì vậy kiểm tra UPDATE tài nguyên cũ chưa đủ để phát hiện lỗi INSERT Brand mới.

Migration bổ sung `20260910010000_FixPermissionTriggerRecordAccess` đổi truy cập field ở điều kiện chung sang JSONB `n->>'is_active'`, mặc định false khi không tồn tại. Các kiểm tra workspace, Brand, creator, channel grant và membership vẫn giữ nguyên.

Migration kiểm tra định nghĩa hàm trước khi thay thế và chấp nhận hàm đã sửa để có thể reapply. Down giữ bản sửa tương thích; không khôi phục lỗi INSERT. Không sửa migration cũ đã có thể được triển khai. Không thay đổi entity/schema nên không có thay đổi model snapshot.

Kiểm chứng qua `tools/PermissionBackupCheck`: restore cô lập, migrate, downgrade/reapply, tạo Brand/Team mới và seed dữ liệu. TeamMember được tạo sau WorkspaceMember để đáp ứng trigger, không tắt ràng buộc. Migration chưa được áp dụng lên database nguồn hay production.

Trước triển khai, áp dụng migration trên staging theo runbook T11 rồi thử tạo Brand, Team, thêm thành viên hợp lệ và thử thêm thành viên ngoài workspace. Không coi việc backend unit tests đạt là bằng chứng migration đã chạy trên môi trường người dùng.
