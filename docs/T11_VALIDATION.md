# T11 — Kiểm thử và điều kiện bàn giao

> Cập nhật phạm vi 10/09/2026: T11 đã hoàn thành phát triển theo yêu cầu người dùng làm xong code trước, nghiệm thu tổng thể sau T12. Các trạng thái chưa đóng dưới đây phản ánh tiêu chí nghiệm thu ban đầu và vẫn được giữ để theo dõi đợt cuối. Xem [T11_COMPLETION.md](T11_COMPLETION.md).

Ngày: 09/09/2026. **Đang thực hiện, chưa nghiệm thu phát hành.** Baseline Git: `e01171531e443e9cf771ff14ee9d5e51b125b842`; kiểm thử trên working tree có thay đổi T09–T11, không phải một release commit đã đóng băng.

## Kết quả cục bộ

Cập nhật cuối lượt upload: **516/516 backend test đạt**. `ContentMediaTests.Exact200MiBBatchHashesAndStreamsEveryByteWithIndependentAssets` đưa bốn stream 50 MiB qua service thật, đối chiếu tổng byte storage đọc, SHA256 và bốn Asset độc lập. Payload tổng hợp dùng signature PNG, storage nhận stream giả lập; không chứng nhận decoder, multipart HTTP hay CDN. Điều kiện đóng task được tổng hợp tại [T11_RELEASE_GATES.md](T11_RELEASE_GATES.md).

Lượt mở rộng insights ngày 10/09 đã đạt: 10.000 Content tổng hợp, 200 Post, 400 PerformanceReport (hai thời điểm mỗi Post). 100 lượt MemberPerformance/20 worker đạt, p95 **256,7 ms**; mỗi Creator đúng 100 bài có insights, engagement 700, impressions 100.000, reach 80.000, engagement rate 0,7%. Kiểm tra xác nhận lấy báo cáo mới nhất, không cộng dồn hai snapshot và không lẫn Brand/Creator. Query list cùng lượt p95 **31,7 ms**, 200 lượt đạt. Đây là dữ liệu tổng hợp, không gọi provider hoặc HTTP. PostgreSQL cô lập đã dừng sau kiểm tra. Báo cáo JSON mới nhất ghi 200 Post/400 report và `latestInsightAggregationPassed=true`.

Cập nhật lượt kiểm tra analytics/upload ngày 10/09: backend **515/515 đạt**, TRX `t11-backend-20260910.trx` đã cập nhật. Hai test mới xác minh lô 11 file hoặc tổng dung lượng khai báo vượt 200 MB bị từ chối trước storage và không tạo Asset. Dùng độ dài khai báo, không cấp phát/truyền payload 200 MB; chưa phải load upload thực.

Analytics qua `MemberPerformanceService` thật trên PostgreSQL restore: **20 worker, 100 lượt**, p95 **149,1 ms**. Mỗi lượt trả đúng một Creator và 5.000 Content của người đó. Dataset có 10.000 Content tổng hợp; chưa có posts/insights tổng hợp tương ứng nên chưa kết luận tải KPI published-post/engagement. Không bao gồm HTTP, auth middleware hay provider.

Lượt query list chạy cùng đợt: **200 lượt đạt**, p95 **53,7 ms**, 10.009 Content hiển thị. Tổng hiển thị có thể khác giữa các lượt vì harness chọn Owner từ backup và cộng fixture; luôn kiểm tra ít nhất 10.000 Content. Báo cáo máy mới nhất ở `.artifacts/permission-backup/query-load.json`; số liệu bên dưới là các lượt trước để truy vết. Không so p95 giữa các lượt như một phép benchmark môi trường cố định.

Toàn bộ harness migration/rollback, seed, pagination và Creator isolation chạy đạt; PostgreSQL kiểm thử đã dừng. Release gate vẫn còn upload thực, published-post analytics, HTTP và staging/sandbox.

- Backend cập nhật 10/09: **513/513 đạt**, không skip. Báo cáo máy: `.artifacts/t11/t11-backend-20260910.trx`. Kết quả 09/09 (506/509) được giữ trong TRX cũ để truy vết.
- Web E2E: **21/21 đạt** trên Edge, Next production build với API giả lập; báo cáo `AISAM-FE/playwright-report/index.html`. Bao gồm auth/navigation, approvals, tạo draft, workspace/profile headers và phát video qua các tab preview. Chưa phải demo end-to-end với backend/provider thật.
- Fixture mới `PermissionReleaseMatrixTests`: 5 actor (Owner, Manager, Viewer, hai Creator), hai Team/Brand/kênh; billing chỉ Owner, Creator cần delegation và channel grant, không đọc chéo Brand, Manager không xem analytics Brand khác, Viewer không publish, revoke có hiệu lực ngay. Dùng EF InMemory, chưa phải seed staging.
- PostgreSQL: chạy `tools/PermissionBackupCheck` thành công trên bản restore cô lập, port 55439. Dùng backup cục bộ `source.dump` ngày 08/09; không khẳng định dữ liệu nguồn mới nhất. Kiểm tra checksum cột cũ và số lượng sáu bảng, migration/down/up, ràng buộc SQL, query translation, revision/audit, reorder năm ảnh, snapshot, journal/idempotency, claim/cancel cạnh tranh, scheduler và automation lock, Rich Text JSONB. Tiến trình PostgreSQL kiểm thử đã dừng sau khi hoàn tất.
- **579 reconciliation issues** được harness ghi nhận trên dữ liệu khôi phục: đây là tồn tại dữ liệu cần phân loại/xử lý trước phát hành, không phải 579 test thất bại. Không tự gán quyền để làm sạch số liệu.
- Downgrade T06 loại bỏ dữ liệu media mới. Không dùng downgrade schema như một phương án bảo toàn dữ liệu mới trong production; cần backup và kế hoạch khôi phục.

Ba test từng thất bại, đã sửa ngày 10/09, thuộc `PromptEnhancerTests`:

1. `EnhanceVideoPromptAsync_ReturnsEnhancedEnglishPrompt_WhenGeminiSucceeds`
2. `EnhanceVideoPromptAsync_FallsBackToDefaultSafeEnglish_WhenT2vaReturnsVietnamese`
3. `EnhanceVideoPromptAsync_FallsBackToDefaultSafeEnglish_WhenRawPromptIsVietnamese`

Đã sửa bước chuẩn hóa: loại văn bản chưa dịch chứa chữ ngoài ASCII ngay cả câu ngắn, cho phép nhãn/dialogue trong dấu nháy giữ ngôn ngữ gốc; mặc định thêm ràng buộc không chèn chữ. Yêu cầu thêm chữ rõ ràng được nhận diện bằng các mẫu động từ/text/chữ; không phải bộ phân tích ngôn ngữ toàn diện. Fallback khi không dịch được dùng mô tả sản phẩm chung, không đưa lại tên/mô tả tiếng Việt; có thể mất chi tiết yêu cầu. Không đổi kỳ vọng ba test cũ. Bổ sung bốn trường hợp kiểm tra yêu cầu text, phủ định và fallback với sản phẩm tiếng Việt; nhóm PromptEnhancer đạt 17/17.

### Đối soát bổ sung 10/09

Harness đã thêm `assets` vào bộ đếm/checksum, nâng phạm vi lên bảy bảng và chạy đạt trên restore cô lập. Count/checksum là kiểm tra cột có trước migration nền tảng, không khẳng định downgrade bảo toàn media mới. Log lần chạy vẫn ghi “six” do nhãn cũ; nhãn đã sửa theo độ dài danh sách bảng.

| Nhóm | Số bản ghi trong backup | Cách xử lý trước staging acceptance |
|---|---:|---|
| Content `creator_unknown` | 542 | Đối chiếu audit/nguồn tạo để xác định creator; không suy từ Owner workspace. Nếu không có bằng chứng, giữ legacy null và kiểm tra quyền fail-closed. |
| Automation `creator_unknown` | 36 | Xác định người chịu trách nhiệm có membership/quyền hợp lệ; không chạy tự động dưới danh tính suy đoán. |
| Post `channel_brand_mismatch` | 1 | Đối chiếu Content, integration và external post; không tự đổi Brand/kênh để mở quyền xem. |

Các số trên là backup 08/09, chưa phải kiểm kê database đang chạy ngày 10/09. Không sửa dữ liệu nguồn trong đợt kiểm tra này.

## Truy vết ma trận

Các file backend bên dưới nằm trong `AISAM-BE/tests/AISAM.IntegrationTests/`. Test provider dùng mock, không chứng minh OAuth/API provider đang hoạt động. “Coverage cục bộ” không đồng nghĩa toàn bộ tiêu chí đã nghiệm thu trên staging.

| Mã | Bằng chứng cục bộ | Phần cần nghiệm thu bổ sung |
|---|---|---|
| P01 | `PermissionQueryScopeTests.CreatorScopeAppliesBeforeCountAndPaginationAndPropagatesToSchedules` | Browser với actor thật |
| P02 | `AccessControlServiceTests`, `PermissionQueryScopeTests`, `PublishOperationsHttpTests` | IDOR toàn bộ endpoint staging |
| P03 | `AccessControlServiceTests.ManagerMemberAnalyticsRequiresSharedAssignedTeam`, fixture release; PostgreSQL analytics query translation | Đối soát KPI Brand A/B bằng dữ liệu mục tiêu |
| P04 | `PermissionIntegrityTests.CrossWorkspaceTeamBrandIsRejectedBeforeAnySave`; PostgreSQL mutation rejection | Lưu bằng API staging |
| P05 | `AutomationCreatorTests.RevokedPublishPermissionPreventsAutoApprovalAndScheduling`; scheduled pipeline revoke; fixture release | Worker thật sau revoke |
| P06 | `ResourcePermissionPolicyTests`, `ResourcePermissionFilterTests`, `ActiveWorkspaceMiddlewareTests.InvokeAsync_ReturnsForbidden_WhenViewerPublishesContent` | Gọi trực tiếp API staging với Viewer |
| P07 | `PermissionQueryScopeTests` và query filters PostgreSQL | Đối soát search/export thật, không chỉ list/count |
| P08 | `AccessControlServiceTests.ExpiredWorkspaceAllowsOwnerBillingButNotContentWrites`, `ActiveWorkspaceMiddlewareTests`, `PaymentServiceTests`, fixture release | Checkout sandbox Owner ở workspace hết hạn |
| PUB01 | PostgreSQL năm ảnh reorder/reload; `ContentMediaTests`, FE `MediaComposer.test.tsx` | Upload/CDN thật, reload browser với năm ảnh |
| PUB02 | Instagram carousel provider mocks | Nhiều video trên kênh đủ capability; chưa bật verified carousel để thay thế nghiệm thu |
| PUB03 | `PublishingPipelineTests`, capability giới hạn media | Kiểm tra payload nhiều video và không gọi provider trên staging |
| PUB04 | `PublishingPipelineTests.MixedMediaRequiresVerifiedInstagram` | Thông báo UI và request thật |
| PUB05 | `InstagramProviderTests.FailedChildDoesNotPublishIncompleteCarousel`, FE `MediaComposer.test.tsx` | Lỗi upload/provider thật có kiểm soát |
| PUB06 | `PublishingPipelineTests.ScheduledOperationUsesFrozenSnapshotAndNeverReplaysAfterTimeout`, journal/claim PostgreSQL | Timeout mạng sandbox, đối soát post ID |
| PUB07 | `RichTextTests`, FE `richTextDocument.test.tsx` | Kiểm tra render dữ liệu cũ trên staging |
| PUB08 | `RichTextTests.SaveDerivesTextAndFormattingEditInvalidatesApprovalButNotFrozenSnapshot`, PostgreSQL snapshot | Schedule/draft edit với worker thật |
| PUB09 | `PublishingPipelineTests.ReconnectAndLimitsAreExplicitAndTokensNeverSerialized`, `ContentServicePublishTests`, scheduler notification tests | Token hết hạn/reconnect sandbox |
| PUB10 | `PublishingPipelineTests.DestinationsAreIndependentAndSameKeyNeverReplays`, FE `PublishPermissions.test.tsx` | Hai platform sandbox, đối soát partial success |

## Security và tải

### Kết quả mở rộng: 10.000 Content

Đã chạy lại harness với 10.000 Content tổng hợp, hai Brand, hai Team, hai Creator, Manager, Viewer và Owner hiện có. Đây là seed **database restore cô lập**, không phải tài khoản staging có thể đăng nhập. Dữ liệu nguồn không thay đổi.

- Tổng Content hiển thị trong workspace kiểm thử: **10.016**, gồm dữ liệu restore và fixture khác của harness.
- 20 worker × 10 lượt count + page = **200 lượt đạt**, p95 **22,1 ms**. Mỗi DbContext giữ một kết nối mở; không so trực tiếp với phép đo 9 Content mở lại kết nối từng truy vấn trước đó.
- Duyệt toàn bộ các trang, kiểm tra đủ tổng bản ghi và không lặp ID xuyên trang: đạt.
- 20 kiểm tra Creator đồng thời: mỗi Creator thấy đúng 5.000 Content, không thấy Content của người còn lại.
- Báo cáo JSON đã cập nhật tại `.artifacts/permission-backup/query-load.json`. Các số đo là query PostgreSQL cục bộ, chưa gồm HTTP/5xx, analytics hay upload.
- Seed phát hiện và đã sửa lỗi trigger INSERT Brand/Team bằng migration `20260910010000_FixPermissionTriggerRecordAccess`; xem [bằng chứng và cách triển khai](T11_TRIGGER_FIX.md). Migration đã qua apply/downgrade/reapply trên restore; chưa áp dụng lên nguồn.

Backend chạy lại vẫn **513/513 đạt**. Chưa đóng T11: nghiệm thu staging/sandbox, tải analytics/upload, xử lý legacy và hồ sơ release còn mở. Đã yêu cầu thông tin staging/sandbox hoặc xác nhận đổi phạm vi; chưa nhận được câu trả lời, nên giữ tiêu chí ban đầu.

### Phép đo sơ bộ PostgreSQL ngày 10/09

Đã bổ sung `AISAM-BE/tools/PermissionBackupCheck/QueryLoadCheck.cs`, chạy sau migration/reapply trên database restore cô lập. Công cụ từ chối host/port/tên database ngoài `127.0.0.1:55439/permission_restore_*` và không ghi dữ liệu trong bước đo tải.

- 20 worker, mỗi worker có DbContext riêng, 10 lượt count + page: **200 lượt thành công**.
- p95 **497,3 ms** trong lần chạy này; bộ dữ liệu của Owner được chọn chỉ có **9 Content hiển thị**.
- Kiểm tra mỗi dòng đúng workspace/Brand, không lặp ID trong trang; tổng count ổn định giữa các lượt.
- Báo cáo máy: `.artifacts/permission-backup/query-load.json`, chứa p50/p95/max, số worker, kích thước dữ liệu và giới hạn phép đo; không chứa token hay connection string.
- Đây là phép đo sơ bộ trên dữ liệu nhỏ, **chưa đạt tiêu chí tải 10.000 Content**, chưa kiểm tra phân trang nhiều trang, HTTP/5xx, analytics, upload hoặc scope Creator dưới tải. p95 dưới 2 giây trong phép đo này không đóng release gate tải staging.

Toàn bộ harness migration/query/worker chạy lại đạt, gồm checksum bảy bảng và phân nhóm reconciliation; PostgreSQL cô lập đã dừng sau kiểm tra.

Coverage backend có tenant/ownership boundary, actor attribution chống mass assignment, revoke, MIME validation (`ContentMediaTests.UploadHasPerItemValidationAndDoesNotTrustExtension`), Rich Text allowlist, webhook thiếu chữ ký và idempotency trong `PaymentServiceTests`. Không thay thế pentest toàn bộ ứng dụng.

Chưa chạy load test theo dữ liệu mục tiêu. Ngưỡng đề xuất cho staging: 20 người dùng đồng thời, 10.000 Content chia ít nhất hai Brand; list và analytics p95 dưới 2 giây, tỷ lệ 5xx dưới 1%, không rò scope, pagination không trùng/thiếu với bộ dữ liệu cố định. Upload thử tối đa giới hạn công bố 10 file/200 MB mỗi lựa chọn, 50 MB/file; ghi thời gian, RAM và lỗi từng file, không áp ngưỡng 2 giây cho upload. Đây là **ngưỡng dự kiến, chưa có kết quả đạt**.

## Chạy lại

Từ repository root:

```powershell
& 'C:\Users\thanh\.dotnet\dotnet.exe' test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj --logger 'trx;LogFileName=t11-backend.trx' --results-directory .artifacts/t11
```

Trong `AISAM-FE`:

```powershell
npm run build
npx playwright install ffmpeg
$env:E2E_BROWSER_CHANNEL = 'msedge'
npm run test:e2e
```

Edge phải được cài trên máy; không đặt channel nếu dùng Chromium do Playwright quản lý. E2E chạy Next production build và **API giả lập**. Token giả chỉ phục vụ mock browser, không gửi tới backend thật. Fixture đã cập nhật resource permission check theo resource/action xác định, không cấp mọi quyền mặc định; route `/content-schedules` không còn bị mock `/content` bắt nhầm. Video fixture cố định một giây VP8 thay cho MediaRecorder không ổn định trong headless; xem `AISAM-FE/e2e/fixtures/README.md`.

## Hồ sơ bàn giao và staging

Đọc cùng [thiết kế](T00_PERMISSION_PUBLISHING_DESIGN.md), [contract](T00_IMPLEMENTATION_CONTRACT.md), [schema](T01_DATABASE_SCHEMA.md), [migration](T01_MIGRATION_VALIDATION.md), [media](T06_COMPLETION.md), [publishing](T07_PROGRESS.md), [worker](T08_COMPLETION.md), [Rich Text](T09_COMPLETION.md), [composer](T10_COMPLETION.md).

1. Đóng băng release commit sau khi xử lý regression; lưu phiên bản SDK, migration history và cấu hình không chứa secret. Xuất Swagger từ API của chính bản build đó, đối chiếu schema/ERD với database staging.
2. Xác định URL/database staging và kênh sandbox được phép thử. Tắt worker trên staging trước restore/migrate để tránh đăng từ dữ liệu khôi phục. Không dùng credential social production trong bản restore.
3. Backup staging; thử restore vào database riêng trước. Ghi count/checksum Content/Post/Asset và migration history trước/sau; phân loại reconciliation issues, đối chiếu owner/team/brand, không tự mở rộng quyền legacy.
4. Thực hành rollback trên bản sao, kiểm tra dữ liệu mới sau T06 có thể mất khi downgrade. Nếu đã có ghi dữ liệu mới, chọn phục hồi backup hoặc forward fix có đối soát; không chạy down migration mù.
5. Seed qua nghiệp vụ: Owner tạo hai Brand/Team, Manager/Viewer/Creator A vào Alpha, Creator B vào Beta; cấp channel view/publish và delegation Creator A có chủ đích. Xác minh 5 tài khoản hoạt động trước demo.
6. Chạy chín bước demo ở mục 8 kế hoạch. Lưu request/status đã che token, snapshot ID, operation ID, external post ID và notification; đối soát từng platform. Chỉ bật worker/kênh sandbox đã xác định.
7. Chạy tải theo dataset/ngưỡng trên; ghi p50/p95, 5xx, truy vấn/index và RAM. Không đạt thì giữ release gate đóng.
8. Bàn giao link evidence, giới hạn capability, kết quả migration/rollback và commit thực nghiệm thu. Production là bước riêng.

**Còn mở:** load test, xử lý reconciliation, staging seed/migration/demo social/checkout, Swagger của release build và release commit. Regression tự động backend đã đạt; không đánh dấu T11 hoàn thành chỉ dựa trên kiểm thử cục bộ.
