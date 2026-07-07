# Đề xuất tính năng AI Content Automation cho AISAM

> Trạng thái triển khai: Phase 1 đã bắt đầu. Đã có schema `AutomationPlan`/`AutomationItem`, import CSV, validation, idempotency key, API danh sách/chi tiết/xác nhận và trang `/automation`. Background generation, credit reservation/settlement, approval và tự động tạo lịch thuộc các phase tiếp theo.

## 1. Mục tiêu

Người dùng tải lên hoặc nhập một bảng lịch trình nội dung. AISAM phân tích lịch, tự tạo nội dung, ảnh/video, đề xuất nền tảng và thời gian đăng, sau đó đưa toàn bộ kết quả vào hàng chờ duyệt. Chỉ nội dung được người dùng duyệt mới được đưa vào lịch đăng thật.

Tính năng nên được định vị là **AI Campaign Autopilot có kiểm soát**, không phải cơ chế tự đăng hoàn toàn. Điều này giúp giảm rủi ro nội dung sai, chi phí AI ngoài ý muốn và vi phạm chính sách nền tảng.

## 2. Luồng người dùng đề xuất

```text
Tải bảng lịch trình
        ↓
Kiểm tra và chuẩn hóa dữ liệu
        ↓
Hiển thị bản kế hoạch nháp để người dùng xác nhận
        ↓
AI tạo caption, hashtag, CTA và nội dung theo từng nền tảng
        ↓
AI tạo ảnh/video khi lịch yêu cầu
        ↓
Kiểm tra tự động chất lượng và khả năng đăng
        ↓
Đưa vào Approval Queue
        ↓
Người dùng duyệt / sửa / yêu cầu tạo lại
        ↓
Tạo lịch Facebook, Instagram, TikTok
        ↓
Scheduler đăng bài và ghi nhận kết quả
```

## 3. Định dạng bảng lịch trình

Giai đoạn đầu nên hỗ trợ CSV và Excel. Sau này có thể thêm Google Sheets.

Các cột đề xuất:

| Cột | Bắt buộc | Ý nghĩa |
|---|---:|---|
| Date | Có | Ngày dự kiến đăng |
| Time | Có | Giờ đăng theo múi giờ workspace |
| Brand | Có | Thương hiệu trong AISAM |
| Product | Không | Sản phẩm liên quan |
| Topic | Có | Chủ đề hoặc ý tưởng bài viết |
| Objective | Không | Awareness, Engagement, Traffic, Conversion |
| Platforms | Có | Facebook, Instagram, TikTok |
| ContentType | Có | Text, Image, Video, Auto |
| Tone | Không | Trẻ trung, chuyên nghiệp, hài hước... |
| CTA | Không | Hành động mong muốn |
| Notes | Không | Yêu cầu bổ sung cho AI |

Trong CSV mẫu, người dùng nhập riêng `Date` và `Time`:

- `Date`: ưu tiên `dd/MM/yyyy`, ví dụ `10/08/2026`; cũng chấp nhận `yyyy-MM-dd`.
- `Time`: định dạng 24 giờ `HH:mm`, ví dụ `09:00` hoặc `19:30`.
- Hệ thống ghép hai cột theo múi giờ được chọn khi import. Cột `ScheduledAt` cũ vẫn được hỗ trợ để tương thích file đã tạo trước đây.

Nếu `ContentType = Auto`, AI đề xuất định dạng dựa trên chủ đề và nền tảng. TikTok chỉ được chọn khi đầu ra có video.

## 4. Các giai đoạn xử lý

### Giai đoạn A — Import và kiểm tra

- Đọc CSV/Excel và hiển thị preview.
- Ghép tên Brand/Product trong file với dữ liệu AISAM.
- Phát hiện dòng thiếu ngày, sai nền tảng, thời gian quá khứ hoặc thương hiệu không tồn tại.
- Phát hiện trùng lịch hoặc cùng một integration đã có lịch hoạt động.
- Ước tính số credit cần dùng cho text, image và video.
- Không gọi AI trước khi người dùng xác nhận kế hoạch và chi phí.
- Trước khi xác nhận, mỗi item có thể mở form **Sửa yêu cầu** để chọn lại Brand/Product, sửa chủ đề, nền tảng, loại nội dung, ngày giờ, tone, CTA và ghi chú. Sau khi lưu, backend chạy lại toàn bộ validation và cập nhật số item hợp lệ.

### Giai đoạn B — Lập kế hoạch AI

AI tạo một `Automation Job`, sau đó chia thành nhiều `Automation Item`, mỗi item tương ứng với một dòng lịch trình.

Với mỗi item, AI xác định:

- Thông điệp chính và góc nội dung.
- Caption gốc.
- Phiên bản caption riêng cho từng nền tảng.
- Hashtag, CTA và prompt tạo media.
- Image hay Video nếu người dùng chọn Auto.
- Social integrations phù hợp của Brand.

Nội dung cần sử dụng thông tin từ Brand Kit, Product và ghi chú của người dùng để tránh caption chung chung.

### Giai đoạn C — Sinh nội dung và media

- Text được sinh trước vì rẻ và nhanh.
- Ảnh/video chỉ được tạo khi text đã hợp lệ.
- Mỗi asset phải lưu provider, model, prompt, credit, trạng thái và URL Cloudinary.
- Video nên chạy bằng background worker vì thời gian xử lý dài.
- Mỗi item có retry độc lập; một video lỗi không làm hỏng cả bảng lịch trình.

### Giai đoạn D — Kiểm tra tự động

Trước khi chuyển sang duyệt, hệ thống kiểm tra:

- Nội dung rỗng, quá dài hoặc chứa từ khóa cấm.
- URL ảnh/video có thể truy cập công khai.
- TikTok bắt buộc có video.
- Instagram bắt buộc có ảnh, carousel hoặc video.
- Integration còn hoạt động và token chưa hết hạn.
- Ngày đăng vẫn nằm trong tương lai.
- Content chưa có lịch trùng trên cùng integration.

Item không đạt được gắn trạng thái `NeedsAttention`, kèm lý do cụ thể.

### Giai đoạn E — Duyệt

Approval Queue nên hỗ trợ:

- Duyệt từng item.
- Chọn nhiều item và duyệt hàng loạt.
- Sửa caption, hashtag, thời gian và nền tảng.
- Tạo lại riêng text, ảnh hoặc video.
- So sánh phiên bản cũ và mới.
- Từ chối kèm ghi chú để AI tạo lại.
- Preview bài theo Facebook, Instagram và TikTok.
- Nếu Brand chỉ có một Page/target đang hoạt động trên nền tảng, hệ thống tự chọn. Nếu có nhiều Page, màn hình duyệt bắt buộc hiển thị checkbox để người dùng chọn một hoặc nhiều Page; mỗi Page được chọn tạo một ContentCalendar riêng và retry không tạo trùng.

Khi duyệt, hệ thống tạo hoặc cập nhật `Content`, sau đó tạo `ContentCalendar` cho từng integration được chọn.

### Giai đoạn F — Đăng và theo dõi

- Scheduler sử dụng luồng đăng hiện có.
- Mỗi nền tảng có lịch và kết quả riêng.
- Một nền tảng lỗi không chặn các nền tảng khác.
- Retry theo loại lỗi; lỗi token phải yêu cầu kết nối lại thay vì retry vô hạn.
- Thông báo cho người dùng khi đăng thành công, thất bại hoặc cần xử lý.

## 5. Trạng thái đề xuất

### Automation Job

`Uploaded → Validating → AwaitingConfirmation → Generating → AwaitingApproval → Scheduling → Completed`

Trạng thái lỗi: `PartiallyFailed`, `Failed`, `Cancelled`.

### Automation Item

`Pending → GeneratingText → GeneratingMedia → QualityCheck → AwaitingApproval → Approved → Scheduled → Published`

Trạng thái phụ: `NeedsAttention`, `Rejected`, `GenerationFailed`, `PublishFailed`.

## 6. Dữ liệu cần bổ sung

### AutomationJob

- Id, WorkspaceId, ProfileId
- FileName, SourceType, Timezone
- Status, TotalItems, SuccessItems, FailedItems
- EstimatedCredits, ReservedCredits, UsedCredits, ReleasedCredits
- CreatedAt, StartedAt, CompletedAt

### AutomationItem

- AutomationJobId, ContentId, BrandId, ProductId
- RowIndex, Platform, IdempotencyKey
- Topic, Objective, Platforms, RequestedContentType
- GeneratedCaption, Hashtags, CTA
- ImageUrl, VideoUrl, MediaPrompt
- ScheduledAt, Status, LastError
- GenerationAttemptCount, Version

### AutomationApproval

- AutomationItemId, ReviewerProfileId
- Decision, Comment, ReviewedAt

Nên lưu JSON gốc của từng dòng import để có thể audit và tái xử lý.

## 7. Kiến trúc phù hợp với dự án hiện tại

- `AutomationController`: upload, preview, confirm, progress, cancel.
- `AutomationService`: điều phối job và item.
- `ScheduleImportService`: đọc CSV/Excel và chuẩn hóa.
- `AutomationGenerationWorker`: sinh text/media nền.
- `AutomationValidationService`: kiểm tra quy tắc nền tảng.
- Tái sử dụng `ContentService`, AI providers, Cloudinary, Approval, `ContentScheduleService` và `ScheduledPostingService` hiện có.
- Frontend thêm trang `/automation` gồm wizard import và dashboard tiến độ.

Không nên xử lý toàn bộ file trong một HTTP request. API chỉ tạo job; background worker xử lý từng item để tránh timeout và cho phép resume.

## 8. Quy tắc an toàn và chi phí

- Mặc định bắt buộc duyệt trước khi tạo lịch.
- Tùy chọn auto-approve chỉ nên dành cho workspace có quyền cao và template đã được phê duyệt.
- Hiển thị ước tính credit trước khi generate.
- Khi người dùng xác nhận kế hoạch, tạm giữ `EstimatedCredits` trong Credit Wallet thay vì trừ ngay.
- Khi từng item hoàn thành, chỉ quyết toán `UsedCredits` thực tế của item đó.
- Khi item lỗi do hệ thống, job bị hủy hoặc không sử dụng hết dự toán, giải phóng phần credit chưa dùng.
- Đặt giới hạn số video/job và số job chạy đồng thời.
- Không trừ credit lần nữa khi retry do lỗi hệ thống.
- Lưu lịch sử prompt, model, người duyệt và phiên bản nội dung.
- Cho phép dừng job; item đã hoàn thành vẫn được giữ lại.

### Cơ chế giữ và quyết toán Credit

Credit của Automation Job nên đi qua ba bước:

```text
Available Credit
      ↓ reserve khi xác nhận kế hoạch
Reserved Credit
      ↓ settle theo chi phí generate thực tế
Used Credit + Released Credit
```

Các giá trị cần theo dõi:

- `EstimatedCredits`: tổng chi phí dự kiến được hiển thị trước khi xác nhận.
- `ReservedCredits`: số credit đang bị khóa cho job và chưa thể dùng cho tác vụ khác.
- `UsedCredits`: chi phí thực tế đã quyết toán thành công.
- `ReleasedCredits`: phần dự toán không sử dụng được trả lại số dư khả dụng.

Quy tắc quyết toán:

1. Khi xác nhận kế hoạch, thực hiện transaction nguyên tử để kiểm tra số dư và tạo reservation.
2. Mỗi item chỉ được settle một lần sau khi provider trả kết quả thành công.
3. Retry do timeout hoặc lỗi hệ thống sử dụng lại reservation hiện có và không tạo giao dịch trừ credit mới.
4. Lỗi do dữ liệu người dùng cần được phân loại riêng; chỉ charge nếu provider thực sự đã hoàn thành tác vụ có tính phí.
5. Khi cancel job, item đang chạy được yêu cầu dừng; phần chưa settle được release sau khi worker xác nhận trạng thái cuối.
6. Khi job hoàn tất, hệ thống release toàn bộ số reserved còn dư.

Nên bổ sung loại giao dịch Credit Wallet như `AutomationReserve`, `AutomationSettle` và `AutomationRelease`. Mỗi giao dịch phải có `AutomationJobId`, `AutomationItemId` nếu có, `IdempotencyKey` và số dư trước/sau để audit.

Không nên cập nhật số dư bằng nhiều thao tác rời. Reserve, settle và release phải dùng database transaction hoặc cơ chế atomic update để hai job chạy đồng thời không tiêu vượt số dư.

### Idempotency và khả năng resume

Mỗi đơn vị công việc theo nền tảng cần một idempotency key ổn định:

```text
SHA256(AutomationPlanId + ":" + RowIndex + ":" + Platform)
```

Trong đó:

- `AutomationPlanId` xác định lần import/kế hoạch.
- `RowIndex` xác định dòng gốc trong bảng lịch trình.
- `Platform` phân biệt Facebook, Instagram và TikTok.

Không đưa attempt number hoặc timestamp vào key vì retry phải tái sử dụng đúng key cũ.

Các ràng buộc đề xuất:

- Unique index trên `AutomationItem.IdempotencyKey`.
- Unique index trên `(AutomationItemId, IntegrationId)` của lịch đăng hoặc bảng liên kết tương ứng.
- Credit transaction có unique index trên `(IdempotencyKey, TransactionType)`.
- Provider generation request lưu `ProviderRequestId` để truy vấn lại kết quả nếu worker mất kết nối.

Khi retry hoặc resume:

1. Tìm `AutomationItem` bằng idempotency key trước khi tạo mới.
2. Nếu text/media đã hoàn thành, bỏ qua bước generate tương ứng.
3. Nếu Content đã tồn tại, cập nhật hoặc tái sử dụng thay vì tạo Content thứ hai.
4. Trước khi tạo `ContentCalendar`, tìm theo `AutomationItemId + IntegrationId`.
5. Nếu lịch Pending/Processing/Scheduled đã tồn tại, trả lại chính bản ghi đó như một kết quả thành công idempotent.
6. Nếu lịch đã Published, không đăng lại trừ khi người dùng chủ động chọn thao tác Republish tạo idempotency key mới.
7. Nếu worker chết giữa chừng, job có thể resume từ trạng thái bền vững gần nhất mà không sinh nội dung, trừ credit hoặc tạo lịch trùng.

API confirm, retry, approve và schedule nên nhận header `Idempotency-Key`. Backend vẫn phải tự tính và kiểm tra key chuẩn từ dữ liệu nội bộ; không tin hoàn toàn giá trị do client gửi lên.

## 9. MVP nên làm trước

### Sprint 1 — Import và text automation

- CSV/Excel template.
- Preview, validate và map Brand/Product.
- Tạo caption/hashtag/CTA.
- Approval Queue và tạo lịch đa nền tảng.

### Sprint 2 — Image automation

- Sinh ảnh, upload Cloudinary, preview và regenerate.
- Theo dõi credit và lỗi theo item.

### Sprint 3 — Video automation

- Background video generation và polling.
- Quy tắc TikTok/video.
- Retry, timeout và notification.

### Sprint 4 — Vận hành nâng cao

- Google Sheets sync.
- Template chiến dịch lặp lại.
- Auto-approve theo rule.
- Báo cáo hiệu quả để AI cải thiện lịch tiếp theo.

## 10. Tiêu chí hoàn thành MVP

- Import được ít nhất 100 dòng mà request không timeout.
- Dòng lỗi không chặn các dòng hợp lệ.
- Người dùng thấy trước chi phí và tiến độ.
- Có thể duyệt hàng loạt hoặc tạo lại từng item.
- Một item tạo được nhiều lịch cho Facebook, Instagram và TikTok.
- TikTok không được lập lịch nếu không có video.
- Không có nội dung nào được đăng khi chưa được duyệt.
- Mọi lần sinh, duyệt, lập lịch và đăng đều có audit log.

## 11. Khuyến nghị sản phẩm

MVP nên bắt đầu bằng **import → sinh text/ảnh → duyệt → lập lịch**. Video nên là bước sau vì chậm, đắt và dễ thất bại hơn. Giá trị lớn nhất với người dùng không nằm ở việc “AI tự làm tất cả”, mà ở việc biến một bảng kế hoạch thành hàng chục bản nháp có cấu trúc trong vài phút, trong khi người dùng vẫn kiểm soát thương hiệu và quyết định đăng.

## 12. Nơi quản lý toàn bộ bài thuộc một lịch trình

Mỗi lần người dùng nhập bảng không nên chỉ tạo ra các Content rời rạc. Hệ thống cần tạo một **Automation Plan** (hoặc Content Plan) làm hồ sơ cha của toàn bộ lịch trình.

Ví dụ:

```text
Content Plan: Chiến dịch ra mắt sản phẩm tháng 8
├── 05/08 09:00 — Bài giới thiệu — Facebook + Instagram
├── 07/08 19:30 — Video hướng dẫn — Facebook + Instagram + TikTok
├── 10/08 10:00 — Feedback khách hàng — Facebook
└── 12/08 20:00 — Video ưu đãi — Instagram + TikTok
```

### Trang danh sách `/automation`

Hiển thị mỗi lịch trình dưới dạng một card hoặc một dòng:

- Tên lịch trình và thương hiệu.
- Khoảng thời gian bắt đầu–kết thúc.
- Tổng số bài.
- Số bài đang tạo, chờ duyệt, đã duyệt, đã lên lịch, đã đăng và lỗi.
- Tiến độ phần trăm.
- Credit đã dùng.
- Người tạo và ngày nhập file.

### Trang chi tiết `/automation/{planId}`

Đây là màn hình chính để người dùng quản lý toàn bộ kế hoạch, gồm các chế độ xem:

1. **Timeline:** sắp xếp bài theo ngày giờ.
2. **Calendar:** hiển thị toàn bộ bài của riêng lịch trình trên lịch tháng/tuần.
3. **Table:** phù hợp chỉnh sửa và duyệt hàng loạt.
4. **Board:** nhóm theo trạng thái Generated, Awaiting Approval, Scheduled, Published, Failed.

Mỗi item hiển thị:

- Ngày giờ đăng và múi giờ.
- Tiêu đề, caption rút gọn và thumbnail.
- Brand/Product.
- Các nền tảng sẽ đăng.
- Trạng thái tạo text, ảnh/video, duyệt, lịch và đăng.
- Cảnh báo như thiếu video TikTok, token hết hạn hoặc lịch trùng.
- Nút Preview, Edit, Approve, Regenerate, Reschedule và View Result.

### Quan hệ dữ liệu

Không nên lưu một bản nội dung hoàn toàn tách biệt và trùng lặp. `AutomationItem` là lớp liên kết giúp truy ngược toàn bộ dữ liệu:

```text
AutomationPlan
    └── AutomationItem
          ├── Content
          ├── ContentCalendar (một bản ghi cho mỗi nền tảng)
          ├── Post (được tạo sau khi đăng thành công)
          └── AutomationApproval
```

Các trường liên kết quan trọng:

- `AutomationItem.AutomationPlanId`
- `AutomationItem.ContentId`
- `ContentCalendar.AutomationItemId` (khuyến nghị bổ sung)
- `Post` truy ra từ `ContentId` và `IntegrationId`

Nhờ vậy, trang Automation Plan có thể tổng hợp đầy đủ mà không nhân bản caption, media hoặc kết quả đăng.

### Trạng thái tổng hợp của một item

Một bài có thể đăng lên ba nền tảng, vì vậy không nên chỉ có một trạng thái Published chung. Trang lịch trình cần hiển thị trạng thái theo từng nền tảng:

| Nền tảng | Duyệt | Lịch đăng | Kết quả |
|---|---|---|---|
| Facebook | Approved | 07/08 19:30 | Published |
| Instagram | Approved | 07/08 19:30 | Scheduled |
| TikTok | Approved | 07/08 19:30 | Failed: token expired |

Trạng thái tổng của item được suy ra:

- `Draft`: AI chưa tạo xong.
- `AwaitingApproval`: có kết quả chờ người dùng.
- `Scheduled`: tất cả nền tảng đã có lịch.
- `PartiallyPublished`: một số nền tảng đã đăng, một số chưa hoặc lỗi.
- `Published`: tất cả nền tảng đã đăng thành công.
- `NeedsAttention`: có lỗi cần người dùng xử lý.

### Liên kết với các trang hiện tại

- Content vẫn chứa bản nội dung chuẩn và có link “Thuộc lịch trình: …”.
- Calendar có filter theo Automation Plan.
- Posts có filter theo Automation Plan.
- Automation Plan là nơi quản lý tổng thể và là màn hình mặc định sau khi import.

Sau khi người dùng xác nhận file, hệ thống chuyển thẳng đến `/automation/{planId}` và cập nhật tiến độ theo thời gian thực. Đây sẽ là nơi họ làm việc chính; Content, Calendar và Posts chỉ còn là các màn hình chuyên sâu khi cần.

## 13. Tiến độ triển khai

### Phase 1 — Hoàn thành

- Import/validate CSV, tạo Automation Plan và Automation Item.
- Idempotency key theo plan, dòng và nền tảng.
- API danh sách/chi tiết/xác nhận cùng màn hình `/automation`.

### Phase 2 — Hoàn thành phần sinh text và ảnh

- Background worker xử lý từng item độc lập.
- Sinh caption riêng theo nền tảng và lưu thành Content nháp.
- Sinh ảnh, upload Cloudinary và ghi nhận credit thực dùng.
- Retry không sinh/trừ lại phần đã hoàn thành; hỗ trợ hủy plan.
- Giao diện tự cập nhật tiến độ, hiển thị caption, ảnh và lỗi từng item.
- Video được đánh dấu `NeedsAttention` và sẽ triển khai trong Phase 3.

### Phase 3 — Hoàn thành video, duyệt và lên lịch

- Video chạy bất đồng bộ theo cơ chế start/poll và tiếp tục được sau khi restart BE.
- Video hoàn tất được tải về, lưu trên Cloudinary rồi mới ghi vào Content.
- Chỉ trừ credit video sau khi provider trả kết quả thành công.
- Hỗ trợ duyệt từng item hoặc duyệt hàng loạt toàn bộ plan.
- Khi duyệt, hệ thống tự tìm social integration đúng Brand và nền tảng rồi tạo lịch đăng.
- `AutomationItem.ContentCalendarId` liên kết trực tiếp item với lịch đăng và ngăn tạo lịch trùng khi retry.
- Lưu audit Approval; hỗ trợ từ chối từng item kèm lý do.
- Trang Automation có preview video, nút duyệt/lên lịch, từ chối và trạng thái lịch.

### Phase 4 — Hoàn thành vận hành nâng cao

- Import Google Sheets công khai qua CSV export, chỉ chấp nhận HTTPS từ `docs.google.com`.
- Clone một plan làm template và dịch toàn bộ lịch theo số ngày được chọn.
- Auto-approve chỉ cho Owner/Manager; worker tự duyệt và tạo lịch sau khi generate xong.
- Worker đồng bộ `Scheduled → Published/PublishFailed` từ kết quả ContentCalendar.
- Báo cáo theo plan gồm số bài, impressions, engagement, CTR và doanh thu ước tính từ PerformanceReport.
- Credit được giữ thật trong `CreditWallet.ReservedBalance`, quyết toán theo text/image/video và giải phóng phần chưa dùng khi hoàn tất, lỗi hoặc hủy.
- Các bước retry dùng trạng thái credit đã quyết toán để không trừ lại phần đã thành công.
