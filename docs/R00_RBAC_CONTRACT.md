# R00 — Hợp đồng phân quyền Workspace / Team

Ngày hoàn thành thiết kế: 14/09/2026. Trạng thái: **contract cho R01–R12, chưa phải chức năng đang chạy**.

Nguồn: hai tài liệu Word được cung cấp và `KE_HOACH_TASK_RBAC_HAI_TANG.md`. Quyết định bổ sung dưới đây giải quyết các chi tiết tài liệu chưa quy định; dùng làm chuẩn triển khai và kiểm thử.

## 1. Vai trò và nguyên tắc

WorkspaceRole: `Owner`, `WorkspaceManager`, `Member`. TeamRole: `Manager`, `ContentCreator`, `Viewer`. API mới trả tên role dạng chuỗi, `contractVersion: 2`; không suy diễn role không nhận biết.

Mọi quyền yêu cầu tài khoản, workspace membership và tài nguyên cùng workspace hợp lệ. Owner/WorkspaceManager được toàn phạm vi workspace nhưng vẫn chịu trạng thái hệ thống, quota và chính sách nội dung. Không bypass các điều kiện đó bằng role.

Member phải có Team hoạt động và TeamBrand hoạt động. Với tài nguyên thuộc Team, chỉ lấy role của chính Team đó. Max privilege trên Brand chỉ tổng hợp khả năng Brand; không cho phép dùng role Team A đối với bài Team B và không cấp quyền CRUD Brand cho TeamManager.

Các tên action dưới đây là kết quả tính từ role/scope, không phải permission lẻ có thể cấp tùy ý.

## 2. Ma trận hành động

O = Owner; W = WorkspaceManager; M/C/V = role trong **Team sở hữu tài nguyên**. Dấu — nghĩa là từ chối. Tất cả phạm vi chỉ trong workspace hiện hành.

| Action | O | W | M | C | V |
|---|---|---|---|---|---|
| billing.read | Toàn bộ | Toàn bộ | — | — | — |
| billing.manage / đổi tên,xóa workspace / chuyển Owner | Có | — | — | — | — |
| Mời/gỡ Member workspace | Có | Member thường | — | — | — |
| Bổ nhiệm/gỡ WorkspaceManager | Có | — | — | — | — |
| Xem danh bạ workspace | Toàn bộ | Toàn bộ | Đồng đội | Đồng đội | Đồng đội |
| Team create/update/deactivate | Có | Có | — | — | — |
| Team list/detail | Toàn bộ | Toàn bộ | Team tham gia | Team tham gia | Team tham gia |
| Thêm/gỡ/đổi role Team | Có | Có | Chỉ C/V trong Team quản lý | — | — |
| Bổ nhiệm/gỡ TeamManager | Có | Có | — | — | — |
| Brand create/update/delete; TeamBrand assign/revoke | Có | Có | — | — | — |
| Brand read | Toàn bộ | Toàn bộ | Brand liên kết | Brand liên kết | Brand liên kết |
| Social connect/disconnect; cấp kênh cho Team | Có | Có | — | — | — |
| Content read | Toàn bộ | Toàn bộ | Team | Team | Team: Approved/Published |
| Content create/upload/submit | Có | Có | Team | Team | — |
| Content update/delete | Theo trạng thái | Theo trạng thái | Team, theo trạng thái | Bài mình, theo trạng thái | — |
| Content approve/reject | Có | Có | Team | — | — |
| Publish/schedule/cancel | Có | Có | Team + kênh được cấp | — | — |
| Analytics bài | Toàn bộ | Toàn bộ | Team | Team | Chỉ bài được xem |
| Tổng provider cấp kênh | Toàn bộ | Toàn bộ | Kênh được cấp, Brand liên kết | — | — |
| Hiệu suất thành viên | Toàn bộ | Toàn bộ | Team | Bản thân trong Team được cấp | — |

Danh bạ đồng đội chỉ gồm id, tên hiển thị, avatar và vai trò Team; không lộ email riêng, billing, session hay KPI. W không gỡ/sửa Owner hoặc W khác; Owner quản lý cấp W. Không có đường tự nâng role qua invite, bulk update hoặc accept invitation. Chuyển Owner là thao tác riêng, nguyên tử, giữ ít nhất một Owner hoạt động.

### Trạng thái nội dung

Kế thừa enum thực tế: Draft=0, PendingApproval=1, Approved=2, Rejected=3, Published=4, Flagged=5, RejectedByPlatform=6, Failed=7.

- Sửa/xóa bản nháp: Draft/Rejected; giữ lịch sử bằng xóa mềm. Published không sửa/xóa bằng endpoint nháp.
- Submit: Draft/Rejected → PendingApproval; approve/reject chỉ từ PendingApproval. M được duyệt bài trong Team kể cả bài mình theo ma trận hiện hành; audit ghi rõ actor.
- Approved muốn sửa phải thu hồi về Draft bằng O/W/M sau khi hủy lịch chờ; Creator không sửa lén để giữ approval cũ.
- Publish/schedule chỉ Approved; retry lỗi phải xác minh approval của phiên bản nội dung hiện hành, kênh và quyền hiện tại. Các trạng thái lỗi/Flagged không được tự bỏ qua duyệt.
- Viewer query chỉ Approved/Published; list/detail/search/calendar/assets/export dùng cùng điều kiện. User M ở A và V ở B dùng điều kiện theo từng Team, không dùng cờ Viewer toàn request.

## 3. Kênh, Default Team và thu hồi

`SocialIntegration.Id` là channelId nội bộ: entity có BrandId, WorkspaceId, ExternalId và TargetType của target đăng. Không dùng SocialAccountId cấp tài khoản đăng nhập làm channelId. Các tích hợp thiếu target phải bị chặn đăng và xuất báo cáo; không tự ánh xạ sang mọi Page.

Kế thừa `TeamChannelAccess → TeamBrand → (TeamId, BrandId)` cho quan hệ TeamBrandChannel; không tạo bảng thứ hai chứa cùng dữ liệu. Sau chuyển đổi, sự có mặt của liên kết kênh cấp scope, action do role quyết định; CanManage cũ không cấp connect/disconnect. Không có grant kênh = không dùng kênh; kênh mới không tự cấp. O/W cấp/thu hồi, M không tự mở scope.

Đăng bài cần role M trên Content.TeamId **và** grant kênh của chính Team đó. Tổng provider cho M cũng giới hạn kênh được cấp. Không hợp nhất quyền Team A và kênh Team B để cho đăng.

Default Team: mỗi Brand có một Team mặc định xác định bằng khóa liên kết duy nhất, không bằng tên. Backfill chỉ Content.TeamId đang null với Brand/workspace hợp lệ; giữ nguyên tác giả. Không tự thêm membership. Content sai/thiếu Brand đưa vào báo cáo ngoại lệ, chỉ O/W xem và sửa; không tự đoán. Bài mới bắt buộc TeamId ngay cả O/W; Team phải hoạt động và liên kết Brand.

DEACTIVATED ánh xạ trạng thái Team hiện có bằng migration tường minh; vô hiệu TeamBrand và scope kênh trong cùng giao dịch, giữ Content/audit. Không tự phục hồi grant khi kích hoạt lại. Lịch chờ bị chặn với lý do thu hồi, không xóa lịch sử. Job kiểm tra quyền ngay trước gọi provider; request đã gửi provider có thể đã đăng, phải đối soát kết quả, không cam kết thu hồi có thể hủy hành động đã xảy ra.

## 4. Hợp đồng API v2 cần triển khai

Giữ route hiện có khi tương thích; client gửi `X-RBAC-Contract-Version: 2` cho API phân quyền thay đổi. Server trả 409 `RBAC_CLIENT_UPDATE_REQUIRED` nếu client cũ gọi contract đã đổi, không diễn giải số role cũ. JWT xác định user; database quyết định quyền. `X-Workspace-Id` chỉ chọn ngữ cảnh.

| Route/method | Request chính | Response data / kiểm tra |
|---|---|---|
| GET /api/permissions/context | Workspace header | contractVersion, revision, workspaceRole, teams[{teamId,role}], scopes[{teamId,brandId,channelIds}], actions theo scope; không token provider |
| POST /api/workspace-invitations | email, workspaceRole: Member/WorkspaceManager | invitationId,status; chỉ O mời W; accept kiểm tra lại người mời còn quyền |
| PUT /api/workspace-members/{memberId}/role | workspaceRole, expectedRevision | Membership mới; chỉ O đổi cấp workspace, không truyền Owner ở đây |
| GET /api/teams | page,pageSize | items,totalCount; lọc Team được xem |
| POST /api/teams | name,description?, members:[{userId,role}], brandIds:[] | TeamDetail,revision; O/W; toàn bộ ghi nguyên tử |
| GET /api/teams/{teamId} | — | id,name,description,status,members,brandIds,revision theo scope |
| PATCH /api/teams/{teamId} | name?,description?,expectedRevision | TeamDetail; không cho đổi workspaceId |
| POST /api/teams/{teamId}/deactivate | expectedRevision | trạng thái và revision mới; O/W |
| PUT /api/teams/{teamId}/members/{userId} | role,expectedRevision | Membership; user thuộc workspace hoạt động; M chỉ thêm/đổi C/V, không hạ M |
| DELETE /api/teams/{teamId}/members/{userId} | If-Match | revision mới; M không gỡ M |
| GET /api/brands/{brandId}/access | — | revision,teams,channels; O/W quản trị assignments |
| PUT/DELETE /api/brands/{brandId}/teams/{teamId} | expectedRevision / If-Match | Liên kết mới; O/W; thu hồi kéo theo scope kênh |
| PUT/DELETE /api/brands/{brandId}/channels/{integrationId}/teams/{teamId} | expectedRevision / If-Match | Gán/gỡ scope kênh, không nhận CanView/CanPublish/CanManage v2 |
| POST endpoint tạo Content hiện có | teamId,brandId và trường nội dung hiện có | Backend xác minh liên kết; tác giả lấy từ user, không tin PrimaryCreatorId client |

Các API khác giữ payload nghiệp vụ nhưng thay kiểm tra quyền bằng contract này. Không thêm alias routes trùng controller; R04 hợp nhất đăng ký `/api/teams` hiện có. Mọi mutation quyền tăng revision nguyên tử; thiếu revision trả 428, cũ trả 409, không ghi một phần. Tên Team trim 1–255 ký tự, description tối đa 1000; page≥1, pageSize 1–100; role ngoài tập trả 400. Thao tác thêm liên kết đã tồn tại có cùng trạng thái là idempotent sau kiểm tra revision/quyền.

Envelope chuẩn: `{success,statusCode,message,data,error:{errorCode,errorMessage},traceId}`; lỗi không gửi stack trace hoặc thông tin tài nguyên ngoài scope. 200 cho đọc/sửa, 201 tạo, mutation xóa mềm trả 200 với revision mới.

| HTTP | errorCode | Ý nghĩa |
|---|---|---|
| 400 | VALIDATION_ERROR | Payload/role không hợp lệ |
| 401 | UNAUTHENTICATED | Thiếu/hết hạn phiên |
| 403 | ACTION_NOT_ALLOWED | Có scope đọc nhưng action không được phép |
| 404 | RESOURCE_NOT_FOUND | Không có hoặc ngoài scope đọc; tránh dò ID |
| 409 | ACCESS_REVISION_CONFLICT | Quyền đã đổi, tải lại context |
| 409 | CONTENT_STATE_CONFLICT | Trạng thái không cho thao tác |
| 409 | RBAC_CLIENT_UPDATE_REQUIRED | Client contract cũ |
| 428 | ACCESS_REVISION_REQUIRED | Thiếu điều kiện ghi đồng thời |

Job nội bộ ghi `ACCESS_REVOKED`/`TEAM_DEACTIVATED`/`CHANNEL_ACCESS_REVOKED` để vận hành, không cần lộ các mã này cho người ngoài scope. FE không logout vì 403/404; dừng retry vô hạn và xóa dữ liệu scope bị thu hồi.

## 5. Ánh xạ và cắt chuyển dữ liệu

| Nguồn hiện tại | Đích | Quy tắc |
|---|---|---|
| Workspace Owner=1 | Owner | Giữ người sở hữu và trạng thái hoạt động |
| Workspace Manager=2, Creator=3, Viewer=4 | Member | Không tự nâng thành W; Owner cấp W tường minh sau migration |
| TeamRole Manager=1, Creator=2, Viewer=3 hợp lệ | Giữ TeamRole | Membership cùng workspace và hoạt động mới cấp quyền |
| User không có Team | Không tự tạo membership | O/W phân công; workspace role cũ không đủ chứng minh scope Team |
| Permission JSON/grant/delegation cũ | Lưu bản sao audit, không xét quyền v2 | Không chuyển thành ngoại lệ vượt vai trò |
| Grant kênh cũ có CanView hoặc CanPublish | Chuyển scope kênh có kiểm soát | Vì scope mới có thể mở action cho M: xuất diff quyền trước/sau, Owner duyệt ánh xạ trước bật; không tự chuyển CanManage-only |
| Mode all-channel cũ | Danh sách kênh tường minh đã đối chiếu | Không bao gồm kênh tạo sau snapshot; cùng bước duyệt diff |
| Content.TeamId null | Default Team của Brand | Không thêm thành viên, ngoại lệ có báo cáo |

Migration schema dùng cột role v2 hoặc chuyển đổi SQL tường minh, không tái sử dụng số 2 thành W. Chạy dry-run và backup trước backfill. Dữ liệu role/link không hợp lệ không cấp quyền; báo cáo phải đủ id và lý do nhưng không chứa secret.

Cắt chuyển: chuẩn bị schema → backfill/diff → kiểm tra → triển khai client và server hỗ trợ v2 → bật enforcement v2 đồng bộ, vô hiệu cache/context cũ. Không dùng phép OR policy cũ/mới. Pending invitation dùng role cũ phải đổi về Member hoặc hủy và mời lại; không accept thành W ngoài ý định Owner. Rollback cần phục hồi mapping và phiên bản tương thích trong cửa sổ bảo trì; không chỉ downgrade binary trên schema/role mới. R01/R12 hiện thực và kiểm chứng các bước này.

## 6. Ví dụ chuẩn cho kiểm thử

1. Nam M ở A, V ở B; cùng Brand X: duyệt A được, B không; chỉ Approved/Published B được đọc.
2. A cấp kênh 1/2, B cấp 3/4/5: bài A không được đăng kênh 3 dù Nam thuộc cả hai Team.
3. W xem hóa đơn được, checkout không; M còn CanManage cũ vẫn không disconnect.
4. Backfill hai lần tạo đúng một Default Team/Brand; không thay TeamId có sẵn, không thêm thành viên.
5. Deactivate A thu hồi liên kết và chặn job chờ; O/W vẫn đọc lịch sử trong workspace.
6. M không đổi bản thân thành W, không gỡ M khác, không mời user ngoài workspace vào Team.

## 7. Bằng chứng hoàn thành R00

- [x] Đối chiếu enum Workspace/Team, ContentStatus và SocialIntegration với mã nguồn.
- [x] Chốt D01–D09 và scope kênh, không còn lựa chọn mở cho task phụ thuộc.
- [x] Ma trận hành động, trạng thái nội dung và chống lan quyền giữa Team.
- [x] Ánh xạ role/grant/Default Team, điều kiện cắt chuyển và rollback.
- [x] Route/DTO/revision, phiên bản client và mã lỗi.
- [x] Ví dụ nghiệm thu làm chuẩn cho R11.

R00 hoàn thành **thiết kế**. Test runtime, migration thực tế và code thuộc R01–R12, chưa được chứng nhận bởi tài liệu này.

## Làm rõ khi triển khai R07 (15/09/2026)

Published được đăng tiếp tới kênh khác nếu vẫn dùng đúng snapshot/MediaVersion đã duyệt hiện hành và actor còn quyền Team/kênh. Không cho lịch cũ dùng snapshot đã bị thay thế. Job video độc lập chưa có Team chỉ dành cho Owner/WorkspaceManager; Member dùng video gắn Content có Team.
