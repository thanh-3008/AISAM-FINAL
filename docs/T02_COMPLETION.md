# T02 — AccessControlService và resolver phân quyền

Ngày 08/09/2026. **Hoàn thành T02**: service đọc quyền từ database, policy tập trung, đăng ký DI và kiểm thử. Việc thay các kiểm tra cũ trong toàn bộ endpoint/list/aggregate là T03; đăng ký service không tự bật enforcement ở mọi API.

## Contract sử dụng

`IAccessControlService.CheckAsync(AccessRequest)` nhận ActorId từ JWT/server, WorkspaceId đang chọn, loại resource, ID và action. PostPublish bắt buộc Content + ChannelId; AnalyticsMember bắt buộc MemberId. Không model-bind AccessRequest trực tiếp từ request body. `GetAccessibleBrandIdsAsync` trả tập Brand đang hoạt động để T03 áp scope trước pagination/count; không phải tập Content đã lọc Creator.

Các loại resource hỗ trợ: Workspace (billing), Brand, Content, Channel (SocialIntegration), Post. Action không phù hợp loại resource, ID thiếu hoặc tài nguyên sai tenant/Brand trả 404 RESOURCE_NOT_FOUND, kể cả Owner. Resource nhìn thấy nhưng action bị cấm trả 403; mã CONTENT_NOT_OWNED, ACCESS_DENIED_CHANNEL/BRAND hoặc ACCESS_DENIED cho từ chối chung. Không trả tên/metadata resource trong quyết định lỗi.

Resolver kiểm tra user hoạt động → membership hoạt động và role hợp lệ → workspace lifecycle → resource thật → Team/Brand đang hoạt động → channel grant → ownership → action. Workspace được đọc detached; tính lifecycle không ghi database. Deleted luôn chặn. Owner được billing khi Limited/Archived/EligibleForDeletion; các mutation khác vẫn theo policy read-only. SocialManage/Publish yêu cầu Integration active; kiểm tra token OAuth/provider còn thuộc luồng thực thi hiện hữu và T03/T08.

Post lịch sử có Content/Integration lệch workspace hoặc Brand bị ẩn kể cả Owner. Viewer xem Brand/kênh được giao nhưng không thấy lịch sử Content/Post. Creator chỉ xem của mình mặc định; quyền view-all không cho sửa nội dung người khác. Review có thể đánh giá Content trong Brand được giao mà không mở API lịch sử; T03 phải giới hạn review queue tương ứng. Analytics thành viên của Manager phải cùng Team đang được giao, không chỉ cùng workspace.

## Grant và thu hồi

CanView/CanPublish/CanManage lấy từ TeamChannelAccess của TeamBrand hợp lệ; không gộp grant khác Brand/workspace. ChannelAccessMode cũ không mở tất cả kênh. TeamMember.Role, wildcard và chuỗi permission legacy không nâng quyền workspace.

Ba key delegation chuẩn, chỉ đối chiếu exact/case-sensitive:

- `aisam.permission.v1.content.view_all_creators`
- `aisam.permission.v1.approval.review`
- `aisam.permission.v1.post.publish`

Đây là namespace phiên bản mới trong Permissions hiện có, không diễn giải lại chuỗi legacy. T03 phải whitelist key, kiểm tra người cấp không vượt quyền và ghi audit khi lưu. Không có endpoint cấp các key mới trong T02. Billing không bao giờ lấy từ JSON grant. Các tên billing.manage/analytics.member được tập trung trong constants để dùng thống nhất khi nối API.

Không cache; mọi lần đánh giá dùng AsNoTracking và query database, không dùng entity stale trong ChangeTracker. Thu hồi có hiệu lực ở lần đánh giá sau khi transaction thu hồi commit. Kết quả không phải giấy phép giữ lâu: worker/API phải re-check ở điểm thực thi trong T03/T08. Chưa tuyên bố ngăn được revoke xảy ra sau khi provider đã nhận publish.

## Bằng chứng

- 16 test resolver + 10 test policy: role matrix, scope bắt buộc cả Owner, Creator/view-all/review, inactive/deleted scope, thu hồi trong DbContext khác, Creator publish cần delegation, Owner billing khi hết hạn, analytics thành viên cùng Team, Post lịch sử lệch Brand.
- Build API đạt. Toàn bộ backend: **462/465 đạt**, chỉ ba lỗi PromptEnhancerTests đã có trước T02; cảnh báo CS8601 hiện hữu ở PayOSPaymentService.
- PermissionBackupCheck đã restore backup public vào PostgreSQL 18 riêng, chạy EF migration và chạy resolver Brand scope/decision với membership hoạt động thành công. Không ghi database nguồn. Kiểm thử PostgreSQL này xác nhận query/mapping, không thay thế toàn bộ test role matrix chạy bằng InMemory.
- Không thêm migration ở T02; cần schema T01 trước khi sử dụng resolver. Không thay route auth/OAuth/billing hiện tại; áp dụng service vào chúng cần review T03.

Task tiếp theo: T03 — áp scope trong query, nối resource checks vào endpoint, triển khai assignment API và audit transaction. FE chưa thể coi nút hiển thị là bằng chứng được cấp quyền.
