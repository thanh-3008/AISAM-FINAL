# T08 — Scheduler và automation an toàn

Ngày kiểm tra: 09/09/2026. Hoàn thành phần phát triển và kiểm thử cục bộ. Kiểm chứng provider thật và triển khai thuộc cổng nghiệm thu T11, cùng các điều kiện sandbox còn lại của T07.

## Hành vi đã triển khai

- Scheduler thực thi với `ScheduledByUserId` và `SnapshotId` của lịch; automation dùng `CreatedByUserId`. Không lấy chủ profile thay cho người tạo automation. Actor không xác định hoặc mất quyền khiến tác vụ dừng và cần xem xét.
- `PublishOperationService.StartScheduledAsync` dùng khóa `schedule:{scheduleId}` và destination của lịch, lưu journal trước khi gọi provider. Đọc lại cùng lịch trả operation đã có, kể cả timeout; không gọi provider lần hai. Snapshot của lịch được kiểm tra cùng content/workspace và không bị thay bởi draft sửa sau đó.
- Quyền được kiểm tra trước khi nhận publish operation, trước khi gọi provider và ở callback `Publishing`. Pipeline dùng chung kiểm tra workspace, quota, snapshot, capability và social credential.
- PostgreSQL nhận lịch bằng `UPDATE` cùng `FOR UPDATE SKIP LOCKED`. Chỉ lịch Pending đến hạn được nhận; Failed và Processing không được tự nhận lại. Lỗi tạm thời tại kiểm tra quyền trước khi tạo operation/gọi provider được phân loại `PUBLISH_RETRY_SAFE`, tối đa ba lần, chờ tăng dần 2 rồi 4 phút. Không thay đổi thời điểm/payload đã duyệt để tính backoff.
- Timeout sau khi có thể gửi request, provider ID đang chờ xác nhận và kết quả không chắc chắn đều cần đối soát. Lịch Processing quá 24 giờ chuyển Failed với `PUBLISH_OUTCOME_UNKNOWN`, kèm thông báo, không tự đưa về Pending.
- Operation lưu attempts, error code, provider ID và tiến độ media. Lỗi gửi notification được log riêng, không đổi kết quả publish hoặc làm mất provider ID đã lưu.
- Generation được tuần tự hóa giữa các instance bằng advisory transaction lock PostgreSQL trên kết nối riêng; duyệt/schedule automation khóa theo plan để tránh tạo nhiều lịch cùng lúc. Khóa generation hiện là toàn worker, ưu tiên tính đúng đắn hơn throughput. Đóng kết nối giải phóng khóa; không tự chạy lại lượt tạo media có kết quả chưa rõ.
- Thu hồi quyền Brand trước hoặc giữa các bước tạo AI chuyển item sang NeedsAttention, plan PartiallyFailed và tắt AutoApprove. Mất quyền review/publish ngăn tạo approval/lịch và có notification. Reserved credit của plan bị dừng được giữ để xử lý theo luồng quản lý/cancel plan hiện có, không tự phát sinh lượt charge mới.
- Tổng hợp nhiều đích: item vẫn Scheduled khi còn destination Pending/Processing; chỉ Published khi mọi đích hoàn tất thành công. Một đích Failed khiến kết quả cuối là PublishFailed, giữ lỗi và tắt AutoApprove.

## Bằng chứng kiểm thử

Toàn bộ backend: **496/499 đạt**, ba lỗi PromptEnhancer đã tồn tại ngoài phạm vi T08. Các test scheduler/publishing/automation đều đạt trong lần chạy toàn bộ này.

| Điều kiện | Bằng chứng |
|---|---|
| P05: mất quyền chặn thực thi | AutomationCreatorTests: creator/membership/Brand bị thu hồi, không gọi provider; approval bị chặn, không tạo lịch |
| Thu hồi sát lúc publish | PublishingPipelineTests: callback Publishing bị thu hồi, không commit provider |
| PUB06: timeout không đăng trùng | Gọi lại operation cùng khóa và cùng lịch sau timeout chỉ có một lần gọi provider |
| PUB08: payload đã khóa | ScheduledOperationUsesFrozenSnapshotAndNeverReplaysAfterTimeout; kiểm tra actor/snapshot khi draft đã thay đổi; PostgreSQL snapshot bất biến và thứ tự media |
| PUB09: reconnect | Capability trả SOCIAL_REAUTH_REQUIRED; scheduler đánh dấu terminal và gửi notification; lỗi delivery không làm đổi trạng thái |
| PUB10: nhiều đích | Destination độc lập trong PublishingPipelineTests; tổng hợp Pending/Failed trong AutomationCreatorTests |
| Hai worker | PermissionBackupCheck chạy hai context PostgreSQL đồng thời: tổng cộng chỉ một lịch được nhận |
| Crash | PostgreSQL: claim quá 24 giờ bị cách ly; khóa automation không cấp cho worker thứ hai và được giải phóng khi đóng kết nối |

Lệnh kiểm thử từ gốc repository:

```powershell
dotnet test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
```

`AISAM-BE/tools/PermissionBackupCheck` đã chạy trên database restore riêng: kiểm tra claim/backoff/recovery, advisory lock, unique idempotency, concurrency, snapshot và migration. Không migrate hoặc sửa database nguồn; không đăng bài lên tài khoản thật.

## Điều kiện vận hành và kiểm thử T11

- Áp dụng các migration T01–T07 theo quy trình triển khai trước khi chạy worker mới. T08 không thêm migration.
- Đối soát `PUBLISH_OUTCOME_UNKNOWN/PENDING` bằng journal và provider trước khi tạo yêu cầu đăng mới. Không reset Failed về Pending bằng SQL để thử lại.
- Không tự suy ra TikTok publish ID là bài đã public. Polling/reconciliation với provider thật vẫn phải kiểm chứng ở sandbox T11.
- Khóa automation giữ một kết nối/transaction riêng trong suốt bước xử lý. Cần cấu hình timeout database/pool phù hợp thời gian provider và đo throughput khi triển khai; thử nghiệm hiện tại dùng PostgreSQL trực tiếp, chưa chứng nhận cấu hình pooler của môi trường triển khai.
- Chưa thử kill tiến trình trong lúc provider thật đang xử lý. Kiểm thử cục bộ xác nhận journal không replay, claim bỏ dở bị cách ly và khóa được giải phóng khi disconnect; sandbox/staging phải xác minh hành vi end-to-end này.
