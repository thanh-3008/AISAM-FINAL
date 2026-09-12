# T03 — Scope dữ liệu và API assignment

**Hoàn thành — 08/09/2026.** Phạm vi backend: query scope, kiểm tra resource/action, API assignment và audit. UI permission là T04; publish pipeline, snapshot, takeover và xử lý race với provider tiếp tục ở T07/T08.

## Cơ chế thực thi

Sau ActiveWorkspaceMiddleware, PermissionScopeMiddleware dựng phạm vi từ membership, TeamBrand/TeamMember và TeamChannelAccess thật. Không nhận danh sách Brand/kênh hoặc role từ client. User inactive bị chặn. Query filter EF áp trước list/search/count/pagination/aggregate cho Brand, Product, Content, Post, Social, lịch, approval, AI, automation, campaign/insight, thành viên, credit usage, hội thoại và notification. Query admin/worker không có workspace HTTP scope vẫn có nghiệp vụ riêng; không giả định query filter là authorization cho background service.

ContentCreator chỉ có lịch sử của mình hoặc Brand được cấp view-all. Post phải có Content/Integration cùng Brand và workspace. Viewer không thấy Content/Post. Scope Team/member không trả toàn bộ directory workspace cho người ngoài Team. Unknown notification target không được phát cho non-owner vì chưa chứng minh được scope. Asset cũ thiếu workspace chỉ hiện theo uploader; collection liên kết media là T06.

Automation plan chứa item Brand ngoài phạm vi bị ẩn cả plan với non-owner, tránh lộ tên và tổng item; không trả một plan bị cắt mất item mà vẫn có tổng toàn workspace. Hội thoại mới ghi CreatedByUserId, hội thoại legacy không đoán Creator từ profile; Owner thấy trong workspace, user khác chỉ thấy của mình. Migration `20260908064221_ConversationCreatorScope` thêm cột nullable, không backfill.

ResourcePermissionFilter kiểm tra ID trong route/request trước thao tác, chặn body chứa ID mâu thuẫn route; view không cấp edit/delete. Trước SaveChanges có kiểm tra lại cho Content, lịch, Product, SocialIntegration và Campaign, bao gồm đường bulk. Mutation bị từ chối trả 403 RESOURCE_ACCESS_DENIED. Publish service kiểm tra actor và quyền kênh trước xử lý, rồi re-check ngay trước gọi provider; lịch không biết người thực thi bị từ chối. Đây không tuyên bố hủy được tác động đã commit bên provider.

Creator có delegation review/publish được đánh giá qua resolver thay vì bị chặn chỉ vì role; Manager có thể tạo AI trong Brand được giao. GET `/api/content/review-queue` chỉ mở phạm vi pending review trong request đó, không mở lịch sử chung. Billing Owner khi workspace hết hạn giữ nhánh hiện hữu, scope mới không làm mất quyền billing.

OAuth account credential/discovery không mang Brand đáng tin cậy: chỉ Owner được thao tác account-wide. Manager quản lý Integration được cấp CanManage, không được ngắt credential cấp account làm ảnh hưởng kênh khác. GET callback relay chỉ chuyển hướng, không dùng làm đường cấp quyền resource. UI diễn giải các giới hạn này ở T04.

## Assignment API

Actor từ JWT; workspace từ middleware. Body không nhận actor/workspace. Snapshot trả revision là SHA-256 của trạng thái canonical; cần gửi lại nguyên token khi thay đổi.

| Method | Route | Nội dung |
|---|---|---|
| GET | `/api/teams?page=1&pageSize=50` | Danh sách Team đúng scope, tối đa 100/trang |
| GET | `/api/brands/{brandId}/access` | Snapshot Team/Channel cùng revision |
| PUT | `/api/brands/{brandId}/teams/{teamId}` | Body `{ expectedRevision }` |
| DELETE | cùng route Team | Header `If-Match` là revision |
| PUT | `/api/brands/{brandId}/channels/{integrationId}/teams/{teamId}` | Body `{ expectedRevision, canView, canPublish, canManage }` |
| DELETE | cùng route Channel | Header `If-Match` là revision |

Route Channel bổ sung brandId so với đề xuất T00 để diễn đạt rõ quan hệ; backend vẫn đối chiếu tất cả ID từ database. Snapshot Manager chỉ trả Team mình thuộc và Channel có View; revision vẫn tính toàn Brand để phát hiện xung đột. Không đưa entity navigation/tokens vào response.

Authorization + revision + mutation + audit chạy trong transaction Serializable. Revision cũ/trùng key/xung đột serialization trả 409 ACCESS_REVISION_CONFLICT; không retry mù grant với entity tracked. Manager không cấp ngoài Brand/Team mình quản lý, không cấp Publish khi chính mình không có Publish; quản lý kênh phải có CanManage. Team khác workspace bị từ chối. Thu hồi TeamBrand xóa các cờ quyền kênh, cấp lại không khôi phục quyền cũ.

Các versioned delegation key của T02 vẫn phải được quản lý bằng công cụ tin cậy; API này quản lý Team–Brand/Channel, không nhận JSON permissions tự do và không cung cấp endpoint sửa role Owner.

## Audit

Grant/revoke ghi audit cùng transaction; conflict/denied không ghi một audit `allowed`. SaveChanges ghi actor/resource/action/result cho Content create/edit/delete, Post publish/delete, approval và social connection thay đổi; không ghi token, prompt hay provider payload. Lịch sử workflow cụ thể và thông báo lỗi provider còn được hoàn thiện ở T07/T08.

## Kiểm thử và triển khai

- Scope tests: Creator count/pagination/lịch, view-all, Viewer, Owner không xuyên workspace, Post lệch kênh bị ẩn.
- Action filter tests: Update/Approve/Publish bị từ chối trước khi controller chạy; actor/resource/channel lấy đúng phía server.
- 57/57 test tập trung permission/controller/scheduler đạt. Toàn bộ backend: **468/471 đạt**, chỉ ba lỗi PromptEnhancerTests đã có trước T03. Cảnh báo CS8601 hiện hữu ở PayOSPaymentService.
- PostgreSQL 18 từ backup public: filter Owner/non-owner dịch SQL và chạy thành công cho resource và dữ liệu phụ; assignment + audit commit; revision cũ và Team khác workspace bị từ chối, không ghi một phần. Các thao tác thử nghiệm chỉ ở bản restore localhost.
- EF snapshot không còn pending model changes sau migration conversation. Chưa dùng tài khoản mạng xã hội/PayOS thật để thử side effect, chưa chạy browser E2E; các bước đó thuộc T11.

Database nguồn **chưa migrate**. Phải áp dụng các migration T01 và ConversationCreatorScope trước khi chạy bản API này. Grant mới mặc định đóng, cần cấp phạm vi đã xác minh. Không tự chuyển Post sai Brand hoặc gán Creator cho dữ liệu cũ. T04 cần cập nhật UI cho 403/404, revision conflict, review queue và danh sách Team đúng scope.
