# T07 — Nghiệm thu phần phát triển capability và publish pipeline

Cập nhật 09/09/2026. **Hoàn thành phần phát triển và kiểm thử tự động. Chưa nghiệm thu sandbox.** Database nguồn chưa migrate; chưa đăng bài thật. Điều kiện sandbox trước khi bật/công bố hỗ trợ thực tế được giữ ở T11; không coi kết quả dưới đây là chứng nhận production.

## Đã triển khai và kiểm tra

- API capability theo integration: Facebook ảnh/nhiều ảnh/video đơn; Instagram ảnh/nhiều ảnh/video đơn; TikTok video đơn; Google publishing không hỗ trợ. Kiểm tra trạng thái account/integration và hạn token lưu tại AISAM. Rich text hiện khai báo PlainText. Policy AISAM: 10 item (TikTok 1), ảnh tối đa 8 MiB, video MP4 tối đa 50 MiB/60 giây; Instagram ảnh JPEG, Facebook ảnh JPEG/PNG. Đây là giới hạn bảo thủ của ứng dụng, không phải giới hạn tối đa của nền tảng. Video thiếu duration trả MEDIA_METADATA_REQUIRED; cần upload lại và duyệt snapshot mới.
- Instagram video/mixed carousel chỉ mở cho integration có trong `InstagramSettings:VerifiedCarouselIntegrationIds`. Danh sách mặc định rỗng. Adapter tạo child theo đúng thứ tự, chờ video FINISHED, dừng nếu một child lỗi, sau đó mới tạo parent/publish. Test giả lập không tự thêm kênh thật vào danh sách.
- `PublishOperation` lưu actor, workspace, snapshot, integration, status, attempts, provider ID, error code và kết quả media JSON. `PublishRequest` khóa unique workspace/actor/idempotencyKey; operations khóa thêm integration. Request cùng key nhưng đổi content/version/đích bị 409; request đã tồn tại chỉ trả trạng thái, không gửi lại provider.
- Các đích được xử lý độc lập; tổng hợp PartiallyPublished nếu có đích thành công và đích khác chưa thành công. Timeout/ngoại lệ sau khi bắt đầu gọi publish chuyển NeedsAttention, không tự replay. Cancel chỉ cho Queued và kiểm tra quyền publish hiện tại; concurrency token ngăn ghi đè khi operation đã bắt đầu.
- Tạo/sửa lịch và publish kiểm tra snapshot/capability. Worker dùng snapshot gắn lịch và kiểm tra lại khi publish. Luồng đổi lịch giữ snapshot cũ, không lấy draft mới làm payload.
- Facebook nhiều ảnh và Instagram carousel trả provider media ID/trạng thái từng item; PostMedia nhận kết quả trên bài đã đăng. Operation giữ kết quả các item khi publish thất bại. Không coi carousel thiếu item là Published.
- TikTok upload trả publish_id được coi là chờ đối soát, chưa tạo Post Published. Lịch cũ ghi lỗi PUBLISH_OUTCOME_PENDING và dừng retry; không cho đổi giờ để xóa lỗi rồi retry mù. Đối soát tự động sẽ nối ở phần worker.
- Lỗi social cần reconnect trả 409/SOCIAL_REAUTH_REQUIRED, không dùng 401 của phiên AISAM. RefreshedTargetAccessToken không được serialize ra response. Lỗi operation không lưu nội dung exception/provider chứa token.

## API

Các route yêu cầu JWT, X-Workspace-Id và quyền tài nguyên:

| Route | Mục đích |
|---|---|
| GET `/api/social/integrations/{integrationId}/publishing-capabilities` | Capability của adapter và trạng thái kết nối đã lưu |
| POST `/api/content/{contentId}/publish-operations` | Body: expectedVersion, integrationIds, idempotencyKey; chạy từng đích và trả operations |
| GET `/api/publish-operations/{operationId}` | Đọc operation và media results, không trả token |
| POST `/api/publish-operations/{operationId}/cancel` | Chỉ hủy khi chưa bắt đầu |

Endpoint create hiện chạy trong request, chưa phải hàng đợi nền có lease. `scheduledAt` chưa nằm trong request mới; dùng API schedule hiện hữu. Callback provider cập nhật UploadingMedia sau từng child và Publishing ngay trước lệnh đăng. Upload ảnh đơn/video Facebook là thao tác kết hợp nên dùng Publishing. Callback lưu dấu vết trước khi tiếp tục gọi provider. Nếu tiến trình dừng giữa chừng, cần đối soát operation thay vì tự gửi lại.

## Migration và bằng chứng

Migration mới:

- `20260908232337_AddPublishOperations`
- `20260908232810_AddPublishOperationMediaResults`
- `20260908233207_AddPublishRequestIdempotencyBoundary`
- `20260909034403_AddSnapshotMediaDimensions`

54/54 test tập trung đạt (bao gồm HTTP qua TestServer, ContentMediaTests và các nhóm publish/provider/schedule). Bộ BE mới nhất: 485/488 đạt; ba lỗi PromptEnhancer có từ trước. Build thành công; EF không phát hiện lệch model/migration.

PermissionBackupCheck restore vào PostgreSQL localhost:55439 đã áp dụng toàn bộ migration; ghi/đọc operation media JSON và từ chối request trùng bằng unique constraint. Downgrade/reapply trên bản sao đạt. Không áp dụng migration vào database nguồn. Không chạy giao dịch thanh toán, gọi storage hoặc đăng bài thật.

## Bổ sung đã hoàn thành trong lần tiếp tục

- Cloudinary trả width/height/duration/public ID qua UploadDetailedAsync; Asset nhận metadata từ storage, snapshot sao chép bất biến, checksum bao gồm metadata. Không lấy duration do trình duyệt tự khai báo. Snapshot cũ vẫn null, không bịa dữ liệu hoặc sửa snapshot cũ.
- TikTok đối chiếu duration với max_video_post_duration_sec trả về từ creator_info ngay trước download/init. Test video vượt giới hạn tài khoản dừng sau đúng một request creator-info, chưa upload.
- Instagram ảnh đơn/Reel có kết quả media/container ID; Facebook single-media ghi trạng thái nhưng để media ID null nếu response legacy chỉ xác nhận post ID.
- Callback persist trạng thái/media trước bước tiếp theo. Khi provider timeout hoặc local persistence lỗi sau provider thành công, luồng legacy và operation đều trả RequiresReconciliation; scheduler ngừng retry. Provider ID đã biết được giữ lại.

## Điều kiện phát hành và công việc phụ thuộc

1. Kiểm chứng OAuth/capability trực tiếp trên tài khoản demo; dữ liệu token/active trong database không chứng minh đủ scope thực tế ở provider. Chưa có xác nhận kênh sandbox được phép đăng nên chưa bật verified carousel.
2. Nghiệm thu sandbox multi-video/mixed carousel, single media và TikTok pending/complete, lưu provider post/container ID làm bằng chứng. Stub không thay thế kiểm tra thật.
3. HTTP TestServer đã kiểm tra 401, 404 khi ngoài scope, 403 khi không có quyền create, 400 payload không hợp lệ, 409 version cũ và hủy sau khi bắt đầu; gửi lại cùng key không gọi provider lần hai. Hai request cancel đồng thời chỉ một thành công. PostgreSQL kiểm tra claim/cancel trên hai DbContext độc lập ở cả hai thứ tự. Test resolver quyền bị thu hồi trước Publishing không commit provider. Full E2E với JWT/Google/kênh thật vẫn thuộc T11.
4. Phần nối hàng đợi, lease, retry budget và đối soát tự động worker thuộc T08; hiện operation chạy đồng bộ theo request, kết quả bất định dừng ở NeedsAttention. Luồng lịch legacy dùng Failed kèm PUBLISH_OUTCOME_PENDING để ngăn retry, chưa có UI tiếp quản.

Nguồn chính thức đã đối chiếu: [Meta Instagram publishing](https://www.postman.com/meta/instagram/documentation/6yqw8pt/instagram-api?entity=request-23987686-ab559ffb-8e2c-4b0a-b43a-5737b6d2f672) mô tả carousel với child image/video; [TikTok Get Post Status](https://developers.tiktok.com/docs/en/content-posting-api-reference-get-video-status) phân biệt upload và kết quả publish. Khả năng được tài liệu mô tả không đồng nghĩa kênh AISAM hiện tại đã được nghiệm thu.

Nguồn cho giới hạn tài khoản TikTok: [Query Creator Info](https://developers.tiktok.com/docs/en/content-posting-api-reference-query-creator-info).

## Chốt mốc ngày 09/09/2026

Mốc phát triển T07 đóng với provider stub, HTTP và PostgreSQL. Tiêu chí gốc “có kiểm thử sandbox cho provider demo trước khi công bố hỗ trợ thực tế” vẫn bắt buộc trước phát hành, không được đánh dấu đã đạt. Không thêm integration vào VerifiedCarouselIntegrationIds trong lần này. T08 tiếp tục hàng đợi/lease/reconciliation; T11 nghiệm thu sandbox và E2E triển khai.

Callback Publishing kiểm tra lại quyền sau upload và trước lệnh đăng. Nếu quyền bị thu hồi, ContentService trả ACCESS_DENIED_CHANNEL; operation dừng ở NeedsAttention. Không khẳng định thu hồi có thể hoàn tác bài mà provider đã nhận.