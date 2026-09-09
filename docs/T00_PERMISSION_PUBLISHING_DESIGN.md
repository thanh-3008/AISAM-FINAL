# T00 — Khảo sát và thiết kế Permission / Publishing

Ngày: 07/09/2026. Baseline commit: `8544e89b04a4a242eb71dc2b1c9fe741e6d1b895`.

Trạng thái: **T00 hoàn thành — 07/09/2026.** Đây là ghi chú khảo sát ban đầu. [Contract cuối](T00_IMPLEMENTATION_CONTRACT.md) và [preflight database](T00_DATABASE_PREFLIGHT.md) thay thế các giả định ban đầu bên dưới, đặc biệt đề xuất backfill Team từ Profile: database đã có WorkspaceId hợp lệ và ProfileId trống. Không áp dụng migration hoặc thay đổi quyền trên database trong bước này.

## 1. Kết quả khảo sát mã nguồn

| Nguồn | Hiện trạng xác nhận | Thay đổi cần thiết |
|---|---|---|
| `AISAM.Data/Model/Team.cs` | Có ProfileId, không có WorkspaceId trực tiếp | Thêm WorkspaceId nullable ở migration mở rộng; backfill từ Profile.WorkspaceId có kiểm chứng |
| `AISAM.Data/Model/Profile.cs` | Có UserId và WorkspaceId nullable | Không dùng UserId của Profile như bằng chứng Creator của mọi Content |
| `AISAM.Data/Model/TeamBrand.cs` | Có TeamId, BrandId, AssignedAt, IsActive | Tận dụng mapping, đối chiếu dữ liệu/index trước khi thay đổi |
| `AISAM.Data/Model/SocialIntegration.cs` | Có WorkspaceId, BrandId, SocialAccountId, ExternalId, trạng thái | Chọn IntegrationId làm định danh kênh cho lớp permission mới |
| `AISAM.Data/Model/Content.cs` | Có WorkspaceId, BrandId, ProfileId; chưa có Creator riêng | Thêm CreatedByUserId nullable, UpdatedByUserId nullable; xác định từ user xác thực khi ghi mới |
| `AISAM.Data/Model/Content.cs` | ImageUrl là jsonb, VideoUrl đơn; TextContent là chuỗi | Giữ tương thích cũ; bổ sung collection và RichTextJson/version theo giai đoạn |
| `AISAM.Data/Model/Asset.cs` | Có uploader, MIME, size, width/height, duration, metadata | Tái sử dụng; bổ sung ràng buộc phạm vi và thông tin collection/snapshot |
| `AISAM.Data/Model/Post.cs` | Có ContentId, IntegrationId, ExternalPostId; PublishedAt bắt buộc, status dùng ContentStatusEnum | Không đưa job queued vào Post với ngày đăng giả; chốt job/snapshot riêng trước khi đổi semantics |
| `AISAM.Data/Model/ContentCalendar.cs` | Có IntegrationId, ProfileId, WorkspaceId, AttemptCount, ScheduledAt, ExecutedAt | Thêm actor/snapshot tham chiếu; giữ recurring và timezone |
| `AISAM.Services/Service/ContentService.cs` | Đã có ImageUrls, validate tối đa 5 ảnh trong một số flow | Không mô tả multi-image là chưa có; chuyển giới hạn sang capability sau khi giữ compatibility |
| `AISAM.Services/Service/ScheduledPostingService.cs` | Có ClaimDueSchedulesAtomicallyAsync, retry và lifecycle check; gọi PublishScheduledAsync bằng ContentId | Mở rộng re-check resource và snapshot, không bỏ cơ chế claim hiện hữu |
| `AISAM.Services/IServices/IProviderService.cs` | PublishAsync nhận SocialAccount, SocialIntegration, PostDto | Tạo contract mới qua adapter để tránh phá đồng loạt provider/test |
| `AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs` | Billing Owner được xét trước workspace read-only | Giữ nguyên nhánh billing khi thêm resource scope |

Các đường dẫn trên tương đối từ `AISAM-BE/`. Chưa kiểm kê dữ liệu database thật; không coi enum/entity là bằng chứng rằng schema production đã đồng bộ.

## 2. Quyết định kỹ thuật đề xuất

### 2.1 Định danh resource

- Workspace là tenant boundary.
- Brand là scope nội dung; Integration là kênh publish thực vì Post đã dùng IntegrationId và Integration đã chứa WorkspaceId/BrandId.
- Team hiện phụ thuộc Profile: phải bổ sung quan hệ workspace rõ ràng trước khi mở API assignment.
- Không thêm BrandMemberAccess trong migration đầu; chờ xác nhận nhu cầu override trực tiếp.
- Không cho client đặt CreatedByUserId/PublishedByUserId tùy ý.

### 2.2 Thiết kế permission

Service trung tâm tính quyền từ membership đang hoạt động, role, Team–Brand, Team–Integration, ownership và action. Controller không tự viết phiên bản policy riêng.

Query scope phải áp dụng trước count/pagination/aggregation. Kiểm tra detail theo metadata tối thiểu; không trả resource rồi mới kiểm tra. Thiếu Creator ở dữ liệu cũ không có nghĩa mọi Creator được xem.

Giai đoạn đầu ưu tiên không cache quyền để tránh cửa sổ thu hồi chưa rõ. Chỉ thêm cache sau khi có số đo, key đầy đủ và invalidation test. Background job luôn đánh giá lại trước thao tác bên ngoài.

### 2.3 Schema mở rộng dự kiến

| Entity | Đề xuất | Quy tắc |
|---|---|---|
| Team | WorkspaceId nullable trong bước đầu | Backfill chỉ khi Profile.WorkspaceId hợp lệ và mọi TeamBrand đồng workspace |
| TeamSocialChannel | WorkspaceId, TeamId, IntegrationId, CanView/CanPublish/CanManage, IsActive, GrantedByUserId, timestamps | Unique TeamId+IntegrationId; cả Team và Integration phải cùng workspace |
| Content | CreatedByUserId, UpdatedByUserId nullable | Không backfill mặc định từ Profile.UserId |
| Post | CreatedByUserId, PublishedByUserId nullable | Không suy ra người đăng từ chủ social account |
| Audit | Actor/resource/action/result + workspace/brand khi phù hợp | Không lưu token hoặc nội dung bí mật |

Collection media và publish job/snapshot thuộc T06, không ghép thành migration T01 quá lớn.

## 3. Migration/backfill an toàn

1. Xuất báo cáo read-only về team không tìm được workspace, TeamBrand chéo workspace, duplicate mapping và attribution chưa rõ.
2. Backup và thử trên bản sao staging.
3. Thêm field nullable/bảng mới; vẫn giữ field media/profile cũ.
4. Backfill Team chỉ khi xác định duy nhất và không mâu thuẫn. Ghi unresolved vào báo cáo, không gán workspace ngẫu nhiên.
5. Attribution chỉ backfill từ nguồn lịch sử chứng minh actor (nếu tồn tại và đáng tin); nguồn suy đoán không được dùng để cấp quyền.
6. Bắt đầu ghi actor cho mọi đường tạo mới: API, clone, AI, automation, import.
7. Kiểm tra tính đầy đủ rồi mới quyết định NOT NULL/index và chuyển read path.
8. Khi rollback sau khi có dữ liệu mới, không drop bảng/cột ngay; ưu tiên rollback ứng dụng tương thích và giữ dữ liệu phục hồi.

Điều kiện chặn migration NOT NULL: còn Team chưa xác định workspace hoặc Content/Post chưa xác định attribution mà chưa có policy xử lý dữ liệu cũ.

## 4. Contract API dự kiến

| API | Quyền/phạm vi |
|---|---|
| GET /api/brands/{brandId}/access | Chỉ chủ thể quản lý access trong Brand được phép |
| PUT/DELETE /api/brands/{brandId}/teams/{teamId} | Kiểm tra quyền grant và quan hệ cùng workspace |
| GET /api/social/integrations/{integrationId}/teams | Quản lý access kênh trong Brand hợp lệ |
| PUT/DELETE /api/social/integrations/{integrationId}/teams/{teamId} | DTO chỉ chứa quyền action; actor lấy từ phiên xác thực |
| GET /api/team/member-performance | Scope trước aggregate; filter không mở rộng phạm vi người gọi |

Tên route Integration là điều chỉnh kỹ thuật so với ví dụ SocialAccount trong tài liệu nguồn. Chưa đăng ký endpoint cho tới khi T01/T02 sẵn sàng.

Error contract đề xuất: ACCESS_DENIED_BRAND, ACCESS_DENIED_CHANNEL, CONTENT_NOT_OWNED, MEDIA_LIMIT_EXCEEDED, MEDIA_TYPE_UNSUPPORTED, MEDIA_UPLOAD_FAILED, PUBLISH_PROVIDER_REJECTED, SOCIAL_REAUTH_REQUIRED. Convention 403/404 và việc tránh lộ ownership ngoài scope phải được thống nhất trước T03.

## 5. Publishing: thiết kế cần khóa

- Snapshot lưu cả nội dung đã format/version và media theo thứ tự, không chỉ AssetId trỏ tới dữ liệu có thể thay đổi.
- Có publish operation/job riêng theo đích Integration, liên kết lịch và snapshot; tránh đổi ý nghĩa PublishedAt của Post hiện tại.
- Recurring schedule cần snapshot rõ ràng cho từng lần thực thi và idempotency theo occurrence+integration; không dùng duy nhất ContentId để khử trùng.
- Timeout sau khi provider đã nhận bài là kết quả chưa xác định: cần reconcile bằng provider ID/cơ chế khả dụng, không retry mù rồi khẳng định exactly-once.
- Giữ provider cũ qua adapter; media collection mới chỉ bật cho adapter đã có validation và test.
- Không ghi số giới hạn provider như sự thật đã xác minh: capability matrix thực tế còn cần kiểm chứng ở T00/T07 theo tài khoản demo.

| Provider trong code | Capability mới | Tình trạng xác minh |
|---|---|---|
| Facebook | Multiple images/video/mixed, giới hạn media | Chưa kiểm chứng cho tài khoản demo |
| Instagram | Carousel/video/mixed, giới hạn media | Chưa kiểm chứng cho tài khoản demo |
| TikTok | Photo/video và quyền tài khoản | Chưa kiểm chứng cho tài khoản demo |
| Google provider | Phạm vi publishing thực được triển khai | Cần đọc adapter và xác định có nằm trong demo |

## 6. Test data và trình tự test

Fixture tối thiểu: 2 workspace để test cross-tenant; workspace chính có 2 Brand, 2 Team, 2 Integration; Owner, Manager A/B, Creator A/B, Viewer. Có membership/assignment active và revoked.

Data legacy: Content thiếu Creator; Team thiếu workspace; mapping trùng/chéo; lịch recurring; draft nhiều ảnh cũ; bài đã đăng có external ID.

Thứ tự: migration/constraint → policy thuần → repository list/detail/count/export → API grant/revoke → worker revoke/race → UI → E2E/staging. Bắt buộc regression Owner billing trong workspace Limited.

## 7. Quyết định nghiệp vụ đang chờ

Người dùng đã ủy quyền quyết định nghiệp vụ; hai nhóm policy được chốt:

1. Owner toàn workspace; Manager chỉ Brand/Team được gán; Creator lịch sử cá nhân trong Brand được cấp; Viewer không xem lịch sử Creator khác — **đã chốt theo ủy quyền**.
2. Thu hồi publish dừng job chưa đăng, thông báo; Manager tiếp quản chủ động và duyệt/lên lịch lại — **đã chốt theo ủy quyền**.

Các quyết định review, override, KPI/timezone, snapshot, 403/404 và self-performance đã được ghi trong PERMISSION_PUBLISHING_DECISIONS.md. Capability/fallback và kiểm kê dữ liệu đã hoàn tất trong contract cuối và preflight.

## 8. Bước tiếp theo

- Sau xác nhận policy: tạo permission matrix cuối và test T02 trước khi nối vào runtime.
- T01: thiết kế migration mở rộng Team workspace/channel/attribution và báo cáo legacy; không chạy database update tự động.
- Tiếp tục khảo sát mọi đường tạo Content và publish để bảo đảm actor được ghi đồng nhất.

Trong bước khảo sát này chỉ tạo tài liệu và cập nhật tiến độ; chưa chạy test runtime vì chưa thay đổi code.
