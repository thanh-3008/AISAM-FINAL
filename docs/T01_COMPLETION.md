# T01 — Nghiệm thu schema, attribution và migration

Ngày 08/09/2026. **T01 hoàn thành trong phạm vi schema/data của kế hoạch.** T02/T03/T08 tiếp tục resolver, API cấp quyền, lọc dữ liệu và re-check quyền của worker; chưa phát hành permission mới lên môi trường đang dùng.

## Schema và quyết định dữ liệu

- Team.WorkspaceId chuẩn; ProfileId nullable. Unique Team–Brand, Team–User, TeamBrand–Integration; index Content theo workspace/brand/creator/ngày, Post theo publisher/ngày và AuditLog theo workspace/ngày.
- TeamChannelAccess tái sử dụng bảng cũ, thêm CanView/CanPublish/CanManage. Cả ba mặc định false. Publish/Manage yêu cầu View. Không suy diễn số ChannelAccessMode hay JSON permission cũ thành grant mới. Owner cấp lại các grant đã xác minh qua API T03; giữ mode cũ để đối soát, không phá dữ liệu.
- Không thêm BrandMemberAccess: D05 chọn cấp qua Team, chưa có yêu cầu override trực tiếp.
- Creator dùng Content.PrimaryCreatorId; Updater dùng UpdatedByUserId; Post.PublishedByUserId tách biệt Creator. ContentCalendar.ScheduledByUserId giữ người lên lịch; ExecutedBySystem tách hành động worker khỏi hành động trực tiếp.
- API middleware thiết lập actor từ JWT trên DbContext scoped; không nhận actor từ body/header tự do. SaveChanges ghi attribution cho đường ghi qua EF, bao gồm API legacy đi qua middleware. Tạo/clone Content, AI và automation còn truyền actor rõ ở service. Clone không giữ Creator của bản nguồn.
- Scheduler khôi phục actor từ lịch cho mỗi lần thực thi, không dùng Creator của Content làm Publisher. Automation generation dùng người tạo plan cho Content và credit. Context actor được phục hồi sau mỗi phạm vi xử lý.
- Import mạng xã hội, lịch cũ và tác vụ không có bằng chứng người thực hiện giữ actor null. Không suy ra tác giả từ chủ profile. Actor null không cấp quyền; T02/T08 phải đánh giá danh tính và quyền trước thực thi. Lịch cũ cần tiếp quản có audit ở T08, không tự backfill.

## Ràng buộc và audit

Guard EF bảo vệ quan hệ Team–Brand–Channel và attribution; trigger PostgreSQL bổ sung bảo vệ khi dùng raw SQL/attach entity rời. Trigger chặn chuyển workspace, Brand/Creator, assignment sai Brand và grant mutation thiếu View; Team member đang hoạt động phải thuộc workspace. Quan hệ cha được khóa FOR SHARE khi kiểm tra. Đây không thay thế kiểm tra role/action ở service.

AuditLog tái sử dụng ActorId, ActionType, TargetTable, TargetId, OldValues/NewValues và map các cột sẵn có WorkspaceId, AffectedUserId, ApprovedBy, RequestedBy, ReferenceId, TeamId, ExecutedBySystem; bổ sung Result nullable cho lịch sử cũ.

Contract ghi audit ở T03: ActionType là `permission.grant`/`permission.revoke` hoặc tên action nghiệp vụ; Result là `allowed`, `denied`, `failed`; TargetTable/TargetId là resource; ReferenceId liên kết request/operation. Old/New chỉ whitelist ID, role, grant boolean, revision; không lưu token, secret hoặc payload provider. Grant/revoke và audit thành công phải chung transaction. T01 hoàn thành schema/contract, không khẳng định đã nối audit mọi endpoint.

## Migration và bằng chứng

Ba migration mới:

1. `20260907075759_ReconcilePermissionFoundation`: hòa giải schema đã có; chỉ backfill workspace từ profile có căn cứ; dừng nếu mâu thuẫn.
2. `20260907162333_AddAutomationCreatorAttribution`: thêm Creator nullable, giữ plan cũ chưa xác định.
3. `20260908003735_CompletePermissionSchema`: grant rõ, attribution/audit/index/trigger và bảng vấn đề dữ liệu `permission_migration_issues`.

Công cụ `tools/PermissionBackupCheck` đã dump schema public và dữ liệu thật bằng pg_dump chỉ đọc; restore thành công vào database mới trên cụm PostgreSQL 18 localhost:55439, không dùng database nguồn để ghi. Đã xác nhận:

- Chạy SQL của cả ba migration; số bản ghi và checksum toàn bộ cột cũ không đổi ở contents, posts, teams, team_brands, team_channel_access, automation_plans.
- EF đọc Content, AutomationPlan, AuditLog sau migration.
- Raw SQL đổi workspace của năm loại resource, đổi Creator và grant Publish thiếu View đều bị từ chối.
- Rollback transaction migration thành công trên bản restore.
- Sau rollback, EF Database.MigrateAsync áp dụng lịch sử migration còn thiếu thành công trên chính bản restore.
- EF không có pending model changes. Fixture migration giả lập trước đó cũng đạt (xem báo cáo migration).
- Toàn bộ backend cuối: **446/449 đạt**, chỉ 3 lỗi PromptEnhancerTests đã tái hiện trước thay đổi. Không có lỗi ở test schema, attribution, controller, workspace, billing, scheduler hoặc automation. `git diff --check` đạt; còn cảnh báo nullable CS8601 hiện hữu ở PayOSPaymentService.

Backup và database thử nghiệm nằm trong `.artifacts`, bị loại khỏi Git. Không đưa bản dump vào báo cáo hoặc gửi ra dịch vụ khác. Công cụ chỉ hỗ trợ cụm localhost chuyên dụng, không nhận địa chỉ đích production.

## Dữ liệu chưa xác định và triển khai

Migration ghi **579 vấn đề** trên bản backup khảo sát, gồm Creator chưa xác định và Post lệch Brand/kênh; số này là theo bản backup, không phải số cố định. Bảng vấn đề chỉ chứa loại tài nguyên/ID/lý do. Post lịch sử không bị xóa hay chuyển Brand; mọi dữ liệu cũ giữ checksum. Các resource có vấn đề không được dùng làm bằng chứng cấp quyền. T03 phải loại Post lệch scope trước list/aggregate; chỉ sửa lịch sử khi có dữ liệu nguồn chứng minh, qua migration đối soát riêng và audit.

**Chưa áp dụng migration lên database nguồn.** Trước chạy bản ứng dụng mới: tạo backup mới, restore kiểm chứng, kiểm tra danh sách pending migration rồi áp dụng migration; sau đó smoke test auth/billing/content/automation. Không chạy model mới trên schema chưa cập nhật. Grant mới mặc định tắt; không bật resolver trước khi Owner xác nhận phạm vi.

Rollback ưu tiên application version cũ, giữ additive schema và attribution. Migration foundation/completion không hỗ trợ Down phá hủy. Không chạy Down automation sau khi đã ghi attribution. Khi cần quay lại schema cũ, restore backup vào database khác đã kiểm chứng rồi chuyển connection; không ghi đè nguồn trong công cụ kiểm thử.

Giới hạn nghiệm thu: PostgreSQL 18 local từ backup public, chưa chứng nhận PostgreSQL 16 hay môi trường production; không test publish thật/PayOS. Ba test prompt video đã lỗi từ trước và nằm ngoài T01, cần xử lý trong task AI/provider tương ứng. Permission enforcement toàn API, takeover lịch cũ, chống race thu hồi quyền/publish và audit runtime là T02/T03/T08, không được suy ra từ nghiệm thu schema.
