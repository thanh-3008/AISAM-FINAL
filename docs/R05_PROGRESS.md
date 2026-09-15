# R05 — Brand và kênh theo Team

Hoàn thành phần backend v2 và kiểm thử cục bộ ngày 14/09/2026; chưa bật v2 trên ứng dụng.

- [x] BrandService dùng WorkspaceRoleV2 cho quản lý Brand: Owner/WorkspaceManager, không dùng quyền TeamCreate/BrandCreate legacy ở nhánh v2.
- [x] Xem chi tiết Brand tách khỏi quản lý: Member được xem Brand liên kết qua Team hợp lệ; Brand ngoài scope bị từ chối.
- [x] AssignmentService v2 cho O/W gán Team/kênh mà không cần membership của chính mình trong Team. TeamManager không được quản lý assignments.
- [x] V2 chỉ cấp scope kênh bằng ScopeEnabledV2; từ chối CanView/CanPublish/CanManage từ request legacy. Quyền hành động do role quyết định.
- [x] Scope kênh tham gia revision; mutation kiểm tra revision và ghi audit trong transaction Serializable trên PostgreSQL.
- [x] Gỡ TeamBrand tắt scope kênh; gán lại không khôi phục grant cũ. Kênh cấp mới phải hoạt động, đúng Brand/workspace.
- [x] Xóa mềm Brand vô hiệu assignments và scope v2; giữ dữ liệu lịch sử, restore Brand không tự bật lại quyền.
- [x] Response assignments có ScopeEnabledV2 để UI R09 đọc được scope mới.
- [x] Middleware v2 hiện có chặn TeamManager gọi social-auth và mutation social; resolver không cấp SocialManage cho TeamManager dù còn grant legacy.
- [x] Bổ sung expose X-HR-Revision qua CORS để client R09 đọc được revision HR/Team đã có từ R03/R04.

## Request và luồng vận hành

GET /api/brands/{brandId}/access lấy revision. PUT /api/brands/{brandId}/teams/{teamId} gán Team; PUT /api/brands/{brandId}/channels/{integrationId}/teams/{teamId} gán kênh với ExpectedRevision, không gửi các cờ quyền cũ. DELETE các route tương ứng dùng If-Match theo contract hiện có.

Gán Brand hiện yêu cầu Team có Manager hoạt động theo quy tắc AssignmentService đang có. Team không có Manager vẫn tạo được ở R04 nhưng phải bổ nhiệm Manager trước khi gán Brand qua API.

## Bằng chứng

56 passed, 0 failed với filter AssignmentV2Tests, AssignmentServiceTests, RbacV2Tests, TeamV2Tests, BrandV2Tests, ActiveWorkspaceMiddlewareTests.

Ca mới: W có legacy Viewer không thuộc Team vẫn cấp Brand/kênh; TeamManager bị chặn; revision đổi theo scope, stale bị từ chối; revoke/regrant không hồi quyền; request legacy flags bị từ chối; Member đọc Brand được cấp nhưng không đọc Brand khác hoặc xóa Brand.

Kiểm thử dùng EF InMemory; chưa nghiệm thu cạnh tranh transaction PostgreSQL hoặc kết nối/ngắt tài khoản provider thật. OAuth thực tế, token và webhook không được gọi trong lượt này. Không sửa .env, không áp dụng migration hoặc bật v2. UI chọn kênh và nghiệm thu tích hợp thuộc R09/R11/R12.
