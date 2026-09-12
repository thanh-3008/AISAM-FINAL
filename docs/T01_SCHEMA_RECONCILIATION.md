# T01 — Tiến độ hòa giải schema

Lịch sử triển khai từ 07/09/2026. **T01 đã hoàn thành ngày 08/09/2026**; xem [báo cáo nghiệm thu cuối](T01_COMPLETION.md). Các mục "còn lại" bên dưới giữ lịch sử ở thời điểm ghi, đã được thay thế bởi báo cáo cuối.

## Đã triển khai

- Team dùng WorkspaceId và ProfileId nullable, đúng dữ liệu thực tế.
- Content map PrimaryCreatorId và TeamId hiện có; không suy diễn hoặc backfill người tạo.
- TeamBrand map ChannelAccessMode; tái sử dụng bảng TeamChannelAccess hiện có, không tạo TeamSocialChannel song song.
- Mapping unique Team–Brand, Team–User và TeamBrand–Integration.
- Migration `20260907075759_ReconcilePermissionFoundation` và model snapshot đồng bộ.
- Migration dùng ADD/CREATE IF NOT EXISTS cho schema đã tồn tại; backfill workspace chỉ từ profile hợp lệ và không mâu thuẫn Brand. Nếu không xác định được, dừng và rollback transaction thay vì gán Guid.Empty.
- Không suy ra ý nghĩa channel_access_mode cũ bằng số đoán; resolver T02 cần đối chiếu grant/migration nguồn trước khi dùng để cấp quyền.

## Kiểm tra

- 16/16 test policy và WorkspaceDomainFoundation đạt ở lần kiểm tra nền tảng.
- Kiểm tra mở rộng policy, workspace foundation/service và ContentService: **42/42 test đạt**. Có cảnh báo nullable CS8601 hiện hữu ở PayOSPaymentService.cs:66.
- Công cụ preflight đã đọc thành công Team, TeamBrand, TeamChannelAccess, Content bằng EF mới trong giao dịch chỉ đọc; xem [báo cáo](T01_DATABASE_SCHEMA.md).
- EF `has-pending-model-changes`: không có thay đổi model chưa đưa vào snapshot.
- Chưa chạy migration Up lên database đang dùng. Migration SQL chưa được chứng nhận trên bản sao staging.
- Đã chạy hai migration mới trên PostgreSQL 18 cục bộ riêng: 6 fixture và kiểm tra rollback đạt; xem [báo cáo migration](T01_MIGRATION_VALIDATION.md). Đây chưa phải bản sao staging từ backup thật.

## Bổ sung kiểm tra dữ liệu ghi — 07/09/2026

- `AisamContext.PermissionIntegrity.cs` kiểm tra cả SaveChanges đồng bộ và bất đồng bộ trước khi ghi. Team–Brand phải cùng workspace; TeamChannelAccess phải trỏ Integration thuộc đúng Brand và workspace.
- Không cho đổi workspace/ownership qua cập nhật entity được theo dõi. Creator mới, nếu được cung cấp, phải là user đang hoạt động và có membership hoạt động trong workspace. Membership vừa bị thu hồi trong ChangeTracker không được thay thế bằng bản ghi cũ còn hoạt động trong database.
- Giữ Creator null của dữ liệu cũ; không suy diễn người tạo. Chưa hoàn tất việc gán Creator từ danh tính xác thực cho tất cả luồng tạo nội dung.
- 6 test mới trong `PermissionIntegrityTests` đạt; bộ chọn permission policy, integrity, WorkspaceService và ContentService đạt **42/42**.
- Chạy toàn bộ backend: **437/440 đạt**, 3 lỗi tại `PromptEnhancerTests`: `EnhanceVideoPromptAsync_ReturnsEnhancedEnglishPrompt_WhenGeminiSucceeds`, `EnhanceVideoPromptAsync_FallsBackToDefaultSafeEnglish_WhenT2vaReturnsVietnamese`, `EnhanceVideoPromptAsync_FallsBackToDefaultSafeEnglish_WhenRawPromptIsVietnamese`. Chạy riêng nhóm này vẫn tái hiện; lỗi so sánh prompt và kỳ vọng ASCII, chưa sửa trong T01.
- Giới hạn: guard ứng dụng chưa thay thế constraint database hoặc kiểm tra quyền actor ở service. So sánh ownership dựa vào OriginalValue của entity được theo dõi; chưa bảo vệ mọi thao tác attach entity rời, ExecuteUpdate/raw SQL hoặc thay đổi đồng thời từ tiến trình khác. Các phần này vẫn chưa hoàn thành.

## Rollback dữ liệu

### Automation attribution

Đã xác nhận bằng preflight chỉ đọc rằng automation_plans chưa có created_by_user_id. Migration `20260907162333_AddAutomationCreatorAttribution` thêm cột uuid nullable, không backfill từ Profile. API tạo, CSV, Google Sheet và clone truyền actor JWT; clone ghi người nhân bản. Worker ghi actor vào Content.PrimaryCreatorId và credit settlement, kiểm tra user/membership trước khi gọi provider. Creator null hoặc membership mất hiệu lực làm item GenerationFailed; cơ chế hiện hữu hoàn lại credit dự trữ khi plan kết thúc. Chưa có luồng tiếp quản plan cũ.

12/12 test automation/integrity đạt, bao gồm import giữ actor, clone đổi actor và worker chặn Creator thiếu/membership bị thu hồi trước khi tạo Content. Chưa kiểm thử provider thực hoặc migration staging. Guard actor chưa thay thế kiểm tra quyền Brand/Channel của T02/T08.

**Điều kiện triển khai:** cần kiểm thử và áp dụng migration thêm cột trước khi chạy API/worker bản này. Database hiện tại chưa được cập nhật; truy vấn AutomationPlan bằng model mới sẽ lỗi nếu chạy trước migration. Down của migration attribution sẽ xóa cột và mất attribution mới; ưu tiên rollback ứng dụng giữ schema, không chạy Down sau khi đã có dữ liệu cần giữ.

Không hỗ trợ Down phá hủy tự động: các cột/bảng có thể tồn tại từ trước migration này. Down ném lỗi rõ ràng để tránh xóa attribution/assignment đang sử dụng. Chọn rollback application giữ additive schema hoặc restore backup đã kiểm chứng. Kế hoạch restore/staging còn phải thực hiện trước khi đóng T01.

## Còn lại để hoàn thành T01

Đã nối attribution cho hai API tạo/clone Content trong workspace: controller lấy actor bằng UserClaimsHelper, service yêu cầu actor không rỗng và ghi PrimaryCreatorId. Không lấy Creator từ request DTO hoặc chủ profile. Clone giữ attribution bản gốc và ghi actor mới cho bản sao. Interface không fallback âm thầm sang phương thức cũ bỏ mất actor. Bộ ContentServiceTests + PermissionIntegrityTests đạt 24/24, bao gồm 3 test mới về attribution. Build có cảnh báo CS8601 hiện hữu ở PayOSPaymentService.

AIService đã gán PrimaryCreatorId tại cả 5 chỗ tạo Content: draft, nhánh ảnh từ chat, video từ chat và nội dung dùng ảnh sản phẩm gốc. Actor lấy từ tham số server đã được controller truyền theo membership; không thêm trường Creator vào request DTO. Draft/chat workspace từ chối actor rỗng trước khi gọi provider. Bộ AIServiceTests và PermissionIntegrityTests đạt 39/39, gồm test Creator của draft và từ chối thiếu actor. Chưa có test riêng bao phủ attribution từng nhánh chat media.

Phạm vi còn lại đã tìm thấy: CreateAsync/CloneAsync kiểu profile cũ; AutomationGenerationService và PostInsightsSyncService có đường tạo riêng. AutomationPlan đã có attribution trong bản thay đổi mới (xem mục Automation attribution); database cần migration trước khi sử dụng. Nội dung nhập từ mạng xã hội cũng không tự coi người đồng bộ là tác giả. Các đường này cần contract/schema attribution bổ sung. Kiểm tra quyền theo Brand/Creator ở service vẫn thuộc T02/T03, không được coi là hoàn thành chỉ vì đã ghi attribution.

- Hòa giải ý nghĩa grant kênh và bổ sung CanView/CanPublish/CanManage với defaults không mở quyền.
- Attribution updater/publisher và mọi đường tạo mới, gồm AI/automation/import/clone.
- Ràng buộc cùng workspace cho dữ liệu ghi mới (service và database), index attribution/audit còn thiếu.
- Đối soát Post lệch Brand/kênh; không tự sửa hoặc xóa bài lịch sử.
- Kiểm thử migration trên PostgreSQL staging: schema cũ, schema đã nâng cấp, dữ liệu không xác định, quan hệ chéo và duplicate; kiểm chứng backup/restore.

Không bật scope enforcement trong API chỉ dựa trên việc mapping đọc được dữ liệu.
