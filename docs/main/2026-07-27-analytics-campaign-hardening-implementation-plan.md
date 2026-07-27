# Analytics & Campaign Hardening - Implementation Plan

> Ngày lập: 2026-07-27  
> Trạng thái: Ready for implementation  
> Phạm vi: AISAM Backend + AISAM Frontend  
> Ưu tiên: Hoàn thiện nhanh phần dễ bị phản biện khi demo/bảo vệ  
> Nguyên tắc: Mỗi task phải build/test xong trước khi chuyển task tiếp theo. Không tự ý commit.

## 1. Mục tiêu

Kế hoạch này tập trung xử lý bốn vấn đề có rủi ro cao nhất:

1. Analytics đang có dữ liệu hard-code hoặc được nội suy từ tổng campaign.
2. Một số KPI đang sai định nghĩa hoặc gây hiểu nhầm.
3. Campaign còn thiếu validation chặt cho dữ liệu tham chiếu và dữ liệu deploy.
4. Hệ thống chưa lưu lịch sử insight theo ngày nên không thể dựng time-series đáng tin cậy.

Kết quả cuối cần đạt:

- Không hiển thị dữ liệu giả như dữ liệu thật.
- KPI được tính thống nhất và có thể giải thích.
- Biểu đồ sử dụng snapshot theo ngày từ provider.
- Campaign không thể deploy khi dữ liệu chưa hợp lệ.
- FE phân biệt rõ loading, empty, unavailable, stale, partial và failed.
- Các thao tác campaign tiếp tục được bảo vệ bởi `ManageCampaigns`.
- Có automated tests cho công thức, workspace isolation, validation và sync.

## 2. Phạm vi ưu tiên

### 2.1. Bắt buộc thực hiện

- Sửa công thức KPI.
- Xóa audience fallback hard-code.
- Xóa time-series và sparkline giả.
- Bổ sung availability/data freshness contract.
- Tạo `CampaignInsightSnapshot`.
- Đồng bộ insight theo ngày và upsert idempotent.
- Chuyển Analytics repository sang đọc snapshot.
- Validate campaign reference, platform, objective, URL, targeting và date range.
- Thêm campaign preflight trước deploy.
- Sửa FE để không nuốt lỗi rồi hiển thị số 0.
- Bổ sung backend integration tests và frontend service/component tests trọng yếu.

### 2.2. Chỉ làm nếu còn thời gian

- Manual sync button và sync history UI.
- Weekly/monthly granularity.
- AI recommendation có cấu trúc.
- Reconciliation job đầy đủ giữa local state và provider.
- Audience breakdown thật từ provider.

### 2.3. Ngoài phạm vi plan này

- Auto optimization ngân sách.
- Tự động apply AI recommendation.
- A/B testing.
- Google Ads/TikTok Ads analytics.
- Data warehouse hoặc event streaming.
- Thiết kế lại toàn bộ Campaign UI.

## 3. Hiện trạng đã xác minh

### Analytics

- `AnalyticsService` luôn trả `LastSyncedAt = null`, `IsPartial = true`.
- `GetDailyTimeSeriesAsync` phân bổ tổng campaign thành dữ liệu theo ngày; đây không phải lịch sử thật.
- Sparkline chia đều tổng campaign và dùng số bài đăng làm CTR.
- Audience có fallback quốc gia, độ tuổi và thiết bị hard-code.
- FE tự tạo hai “AI insights” từ chuỗi template.
- FE đang tính ROAS bằng `conversions / spend`.
- Channel breakdown trả phần lớn metric bằng 0.
- Endpoint nhận `granularity`, nhưng repository vẫn chỉ tạo dữ liệu theo ngày.

### Campaign

- Luồng deploy bốn bước đã có checkpoint:
  - campaign,
  - ad set,
  - creative,
  - ad.
- Middleware đã áp dụng `ManageCampaigns`; Owner/Manager được phép quản lý campaign.
- Service vẫn nên kiểm tra quyền quan trọng theo defense-in-depth.
- Insight sync hiện chỉ lưu impressions, clicks, spend; conversions được ghi bằng 0.
- `ProductId` và `ContentId` chưa được validate đầy đủ với workspace/brand.
- `Platform`, `Objective`, `Targeting`, `LandingUrl` còn validation yếu.
- Khi thiếu targeting, service tự dùng Việt Nam.
- Khi thiếu content, service có thể tự lấy content đầu tiên.

## 4. Kiến trúc đích tối thiểu

```text
Meta/provider API
        |
        v
Campaign insight sync service
        |
        v
campaign_insight_snapshots (daily, idempotent)
        |
        v
PerformanceReportRepository / AnalyticsService
        |
        v
/api/analytics/*
        |
        v
Analytics FE
```

Nguồn dữ liệu:

- `CampaignInsightSnapshot`: nguồn chuẩn cho campaign performance theo thời gian.
- `PerformanceReport`: tiếp tục dùng cho post/content performance trong phạm vi hiện có.
- `AdCampaign`: metadata, deployment state và totals mới nhất để load danh sách nhanh; không dùng để dựng lịch sử giả.

## 5. Định nghĩa KPI canonical

| KPI | Công thức | Khi mẫu số bằng 0 |
| --- | --- | --- |
| CTR | `clicks / impressions * 100` | `null` |
| CPC | `spend / clicks` | `null` |
| CPM | `spend / impressions * 1000` | `null` |
| CVR | `conversions / clicks * 100` | `null` |
| CPA | `spend / conversions` | `null` |
| ROAS | `attributedRevenue / spend` | `null` |
| Engagement rate | `engagement / reach * 100` | `null` |
| Frequency | `impressions / reach` | `null` |

Quy tắc:

- Backend tính KPI; FE chỉ format.
- Không thay `null` bằng `0`.
- Tiền phải có currency.
- Chưa có revenue thì ROAS là `null`.
- Không cộng tiền khác currency trong cùng một total.
- Tỷ lệ trả về theo đơn vị phần trăm, ví dụ `2.5` nghĩa là `2.5%`.

## 6. Data model mới

### 6.1. Entity `CampaignInsightSnapshot`

File tạo:

```text
AISAM-BE/AISAM.Data/Model/CampaignInsightSnapshot.cs
```

Trường tối thiểu:

```text
Id                    Guid
WorkspaceId           Guid
CampaignId            Guid
Platform              string(20)
SnapshotDate          date
Currency              string(3)
Impressions           long
Reach                 long?
Clicks                long
Engagement            long?
Spend                 decimal(18,2)
Conversions           decimal(18,4)?
AttributedRevenue     decimal(18,2)?
AttributionWindow     string(50)?
Source                string(50)
IsPartial             bool
SyncedAt              DateTime
RawData               jsonb?
CreatedAt             DateTime
UpdatedAt             DateTime?
```

Ràng buộc:

```text
FK WorkspaceId -> workspaces
FK CampaignId -> ad_campaigns
UNIQUE (CampaignId, Platform, SnapshotDate, AttributionWindow)
INDEX (WorkspaceId, SnapshotDate)
INDEX (CampaignId, SnapshotDate)
```

### 6.2. Sync state tối giản

Không cần tạo bảng sync-run ở vòng đầu nếu thời gian quá gấp. Lấy:

- `LastSyncedAt = MAX(CampaignInsightSnapshot.SyncedAt)`.
- `IsPartial = snapshots.Any(IsPartial)`.
- Lỗi lần sync gần nhất trả trực tiếp từ manual sync response và log server.

Nếu còn thời gian, bổ sung `AnalyticsSyncRun` ở task mở rộng.

## 7. API contract cần chuẩn hóa

Mọi Analytics response chính phải có:

```json
{
  "data": {},
  "freshness": {
    "status": "fresh",
    "lastSyncedAt": "2026-07-27T03:00:00Z",
    "isPartial": false,
    "sources": ["facebook"],
    "warnings": []
  }
}
```

`freshness.status`:

```text
fresh
stale
partial
syncing
failed
not_configured
no_data
```

Endpoint giữ lại:

```text
GET /api/analytics/overview
GET /api/analytics/time-series
GET /api/analytics/channel-breakdown
GET /api/analytics/campaign-breakdown
GET /api/analytics/top-posts
GET /api/analytics/sync-status
GET /api/analytics/audience
```

Endpoint bổ sung:

```text
POST /api/analytics/sync
POST /api/campaigns/{id}/preflight
```

Không đổi route hiện có nếu không cần thiết để giảm ảnh hưởng FE.

## 8. Thứ tự implementation bắt buộc

---

### Task 1 - Khóa metric contract và sửa lỗi KPI

#### Mục tiêu

Sửa các công thức sai trước khi thêm nguồn dữ liệu mới.

#### File sửa

```text
AISAM-BE/AISAM.Common/Models/AnalyticsDtos.cs
AISAM-BE/AISAM.Services/Service/AnalyticsService.cs
AISAM-BE/AISAM.Repositories/Repository/PerformanceReportRepository.cs
AISAM-FE/src/services/analyticsService.ts
AISAM-FE/src/components/analytics/AnalyticsKpiCards.tsx
AISAM-FE/src/components/analytics/AnalyticsEfficiencyCard.tsx
```

#### Việc cần làm

- [ ] Chuyển KPI không tính được sang nullable.
- [ ] Sửa CPA thành `spend / conversions`.
- [ ] Thêm CPC riêng nếu UI cần.
- [ ] Sửa CVR thành `conversions / clicks * 100`.
- [ ] Sửa ROAS thành `attributedRevenue / spend`.
- [ ] Sửa Engagement Rate thành `engagement / reach * 100`.
- [ ] Xóa phép tính ROAS ở FE.
- [ ] Thêm tooltip công thức cho KPI.
- [ ] Thống nhất rounding: tỷ lệ 2 chữ số, tiền 2 chữ số.

#### Test tạo/sửa

```text
AISAM-BE/tests/AISAM.IntegrationTests/AnalyticsMetricTests.cs
AISAM-FE/src/components/analytics/__tests__/analyticsUtils.test.ts
```

Test cases:

- [ ] Mẫu số bằng 0 trả `null`.
- [ ] CTR, CPC, CPM, CVR, CPA, ROAS đúng.
- [ ] Revenue null làm ROAS null.
- [ ] FE không hiển thị `0%` cho metric unavailable.

#### Verify

```powershell
dotnet build AISAM-BE\AISAM.API\AISAM.API.csproj --no-restore
dotnet test AISAM-BE\tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --no-restore
npm.cmd run test -- --run
```

---

### Task 2 - Xóa dữ liệu Analytics giả và thêm availability

#### File sửa

```text
AISAM-BE/AISAM.Services/Service/AnalyticsService.cs
AISAM-BE/AISAM.Repositories/Repository/PerformanceReportRepository.cs
AISAM-BE/AISAM.Common/Models/AnalyticsDtos.cs
AISAM-FE/src/services/analyticsService.ts
AISAM-FE/src/app/(dashboard)/analytics/page.tsx
AISAM-FE/src/components/analytics/AnalyticsChart.tsx
AISAM-FE/src/components/analytics/AnalyticsAiInsights.tsx
```

#### Việc cần làm

- [ ] Xóa default geographic/demographic/device hard-code.
- [ ] Khi provider không có audience data, trả empty + reason.
- [ ] Xóa daily time-series nội suy.
- [ ] Xóa sparkline chia đều.
- [ ] Xóa CTR lấy từ post count.
- [ ] Không gắn nhãn AI cho insight template.
- [ ] Không catch rỗng ở FE.
- [ ] Thêm các UI state: unavailable, no-data, partial, failed.
- [ ] Đổi “Real-time” thành “Latest synced”.

#### Quy tắc tương thích

- Giữ field hiện có nếu FE khác còn sử dụng, nhưng trả collection rỗng.
- Không trả sample data trong production response.
- Có thể giữ demo data trong test fixture riêng.

#### Acceptance criteria

- [ ] Không kết nối Facebook: audience hiển thị “Not available”, không hiện US/UK giả.
- [ ] Không có snapshot: chart hiển thị empty state.
- [ ] API lỗi: FE có retry, không âm thầm hiển thị 0.

---

### Task 3 - Tạo snapshot entity và migration

#### File tạo

```text
AISAM-BE/AISAM.Data/Model/CampaignInsightSnapshot.cs
AISAM-BE/AISAM.Repositories/Migrations/<timestamp>_AddCampaignInsightSnapshots.cs
AISAM-BE/AISAM.Repositories/Migrations/<timestamp>_AddCampaignInsightSnapshots.Designer.cs
```

#### File sửa

```text
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Data/Model/AdCampaign.cs
AISAM-BE/AISAM.Data/Model/Workspace.cs
```

#### Việc cần làm

- [ ] Thêm entity và navigation tối thiểu.
- [ ] Cấu hình precision, date type, jsonb.
- [ ] Thêm unique index idempotency.
- [ ] Thêm workspace/date và campaign/date index.
- [ ] Tạo migration.
- [ ] Review migration: không drop/rename bảng cũ.
- [ ] Không backfill dữ liệu cumulative thành daily snapshot giả.

#### Migration command

Chạy từ `AISAM-BE` sau khi xác nhận startup/data project:

```powershell
dotnet ef migrations add AddCampaignInsightSnapshots --project AISAM.Repositories\AISAM.Repositories.csproj --startup-project AISAM.API\AISAM.API.csproj
dotnet ef database update --project AISAM.Repositories\AISAM.Repositories.csproj --startup-project AISAM.API\AISAM.API.csproj
```

#### Rollback

```powershell
dotnet ef database update <PreviousMigration> --project AISAM.Repositories\AISAM.Repositories.csproj --startup-project AISAM.API\AISAM.API.csproj
```

Không xóa migration đã apply trên shared database nếu chưa thống nhất với nhóm.

---

### Task 4 - Snapshot repository và idempotent upsert

#### File tạo

```text
AISAM-BE/AISAM.Repositories/IRepositories/ICampaignInsightSnapshotRepository.cs
AISAM-BE/AISAM.Repositories/Repository/CampaignInsightSnapshotRepository.cs
```

#### File sửa

```text
AISAM-BE/AISAM.API/Program.cs
```

#### Interface tối thiểu

```csharp
Task UpsertRangeAsync(
    IReadOnlyCollection<CampaignInsightSnapshot> snapshots,
    CancellationToken cancellationToken);

Task<IReadOnlyList<CampaignInsightSnapshot>> GetRangeAsync(
    Guid workspaceId,
    DateTime from,
    DateTime to,
    Guid? brandId,
    string? platform,
    Guid? campaignId,
    CancellationToken cancellationToken);

Task<DateTime?> GetLastSyncedAtAsync(
    Guid workspaceId,
    CancellationToken cancellationToken);
```

#### Quy tắc

- [ ] Upsert theo unique key.
- [ ] Sync lại cùng ngày cập nhật record, không cộng dồn.
- [ ] Mọi query phải scope `WorkspaceId`.
- [ ] CancellationToken phải truyền xuống EF.
- [ ] Không load `RawData` trong query aggregation.

#### Tests

- [ ] Upsert hai lần không duplicate.
- [ ] Update spend cùng ngày thay giá trị cũ.
- [ ] Workspace A không đọc được snapshot workspace B.
- [ ] Filter campaign/platform/date hoạt động.

---

### Task 5 - Mở rộng provider contract cho daily insight

#### File cần kiểm tra/sửa

```text
AISAM-BE/AISAM.Services/IServices/IProviderService.cs
AISAM-BE/AISAM.Services/Service/FacebookProvider.cs
AISAM-BE/AISAM.Common/Models/<provider insight DTO files>
```

#### DTO đề xuất

```csharp
public sealed record CampaignDailyInsightDto(
    DateOnly Date,
    long Impressions,
    long? Reach,
    long Clicks,
    decimal Spend,
    decimal? Conversions,
    decimal? AttributedRevenue,
    string Currency,
    string? AttributionWindow,
    bool IsPartial,
    string? RawData);
```

#### Việc cần làm

- [ ] Thêm method lấy insight theo range và `time_increment=1`.
- [ ] Parse số dùng `InvariantCulture`.
- [ ] Map Meta actions sang conversion theo objective đã hỗ trợ.
- [ ] Nếu chưa map được conversion/revenue, trả `null`, không trả 0.
- [ ] Không log access token hoặc raw payload nhạy cảm.
- [ ] Phân loại lỗi: token, permission, rate limit, timeout, provider error.

#### Giới hạn MVP

- Chỉ Facebook/Instagram qua Meta.
- Chỉ sync campaign đã deploy và có external campaign ID.
- Range manual sync tối đa 90 ngày.

---

### Task 6 - Tạo Campaign Insight Sync Service

#### File tạo

```text
AISAM-BE/AISAM.Services/IServices/ICampaignInsightSyncService.cs
AISAM-BE/AISAM.Services/Service/CampaignInsightSyncService.cs
```

#### File sửa

```text
AISAM-BE/AISAM.API/Program.cs
AISAM-BE/AISAM.API/Controllers/AnalyticsController.cs
AISAM-BE/AISAM.Services/Service/AdCampaignService.cs
```

#### Luồng

1. Xác nhận active workspace.
2. Lấy campaign đã deploy thuộc workspace.
3. Resolve đúng social account/token.
4. Gọi provider theo range.
5. Map sang snapshot.
6. Upsert transactionally theo từng campaign.
7. Cập nhật totals mới nhất trên `AdCampaign` từ snapshot mới nhất hoặc tổng range phù hợp.
8. Trả kết quả partial nếu một campaign lỗi.

#### Request

```json
{
  "campaignId": null,
  "from": "2026-07-20",
  "to": "2026-07-27"
}
```

#### Response

```json
{
  "status": "partial",
  "campaignsRequested": 4,
  "campaignsSucceeded": 3,
  "snapshotsUpserted": 21,
  "lastSyncedAt": "2026-07-27T04:00:00Z",
  "warnings": []
}
```

#### Permission

- Endpoint sync phải yêu cầu `ManageCampaigns` hoặc Owner/Manager.
- Analytics GET vẫn cho authenticated workspace member nếu policy hiện tại cho phép xem.

#### Tests

- [ ] Sync success.
- [ ] Provider trả empty.
- [ ] Một campaign lỗi, toàn batch trả partial.
- [ ] Token revoked trả lỗi có thể hành động.
- [ ] Sync lại không duplicate.
- [ ] Campaign ngoài workspace bị chặn.

---

### Task 7 - Chuyển Analytics aggregation sang snapshot

#### File sửa

```text
AISAM-BE/AISAM.Repositories/Repository/PerformanceReportRepository.cs
AISAM-BE/AISAM.Repositories/IRepositories/IPerformanceReportRepository.cs
AISAM-BE/AISAM.Services/Service/AnalyticsService.cs
AISAM-BE/AISAM.Common/Models/AnalyticsDtos.cs
```

#### Quy tắc query

Date overlap cho campaign metadata:

```text
StartDate <= to
AND (EndDate IS NULL OR EndDate >= from)
```

Snapshot:

```text
SnapshotDate >= from.Date
AND SnapshotDate <= to.Date
AND WorkspaceId = activeWorkspace
```

#### Việc cần làm

- [ ] Overview campaign metrics lấy từ snapshot.
- [ ] Time-series group theo `SnapshotDate`.
- [ ] Campaign breakdown group theo campaign.
- [ ] Platform filter áp dụng cho mọi campaign query.
- [ ] Brand/campaign filter áp dụng nhất quán.
- [ ] Channel breakdown lấy campaign metric từ snapshot và post metric từ PerformanceReport.
- [ ] `granularity=day` hoạt động thật.
- [ ] Reject granularity khác bằng 400 trong MVP, hoặc implement group tuần/tháng nếu đủ thời gian.
- [ ] Freshness lấy từ snapshot.
- [ ] Không cộng currency khác nhau; trả warning nếu có mixed currency.

#### Invariant tests

- [ ] Tổng time-series bằng overview trong cùng filter.
- [ ] Campaign breakdown totals bằng overview campaign totals.
- [ ] Filter platform không làm lẫn dữ liệu.
- [ ] Previous period có độ dài bằng current period.
- [ ] Date range giao nhau được tính đúng.

---

### Task 8 - Campaign validation hardening

#### File tạo

```text
AISAM-BE/AISAM.Services/Validation/AdCampaignValidationService.cs
AISAM-BE/AISAM.Services/IServices/IAdCampaignValidationService.cs
```

Nếu logic ngắn, có thể giữ private methods trong `AdCampaignService`; không tạo abstraction chỉ để tăng số file.

#### File sửa

```text
AISAM-BE/AISAM.Common/Dtos/Request/CreateAdCampaignRequest.cs
AISAM-BE/AISAM.Common/Dtos/Request/UpdateAdCampaignRequest.cs
AISAM-BE/AISAM.Services/Service/AdCampaignService.cs
AISAM-BE/AISAM.API/Program.cs
```

#### Validation bắt buộc

- [ ] Platform allowlist: `facebook`, `instagram`.
- [ ] Objective allowlist theo provider support.
- [ ] Budget > 0 và đáp ứng minimum nếu biết.
- [ ] StartDate <= EndDate.
- [ ] LandingUrl là absolute HTTPS URL.
- [ ] Targeting parse được thành JSON.
- [ ] Targeting có location hợp lệ; không tự default VN.
- [ ] Brand thuộc workspace.
- [ ] Product thuộc workspace và đúng brand.
- [ ] Content thuộc workspace và đúng brand.
- [ ] Content có trạng thái hợp lệ cho deploy.
- [ ] Ad account thuộc profile/workspace liên quan.
- [ ] Update không cho thay immutable fields sau deploy.

#### Defense-in-depth

- Giữ middleware `ManageCampaigns`.
- Service mutation methods tiếp tục xác minh membership.
- Với deploy, activate, pause, delete và budget change, bổ sung helper xác minh Owner/Manager để service không phụ thuộc hoàn toàn vào middleware.

#### Error contract

Trả mã ổn định:

```text
CAMPAIGN_PLATFORM_UNSUPPORTED
CAMPAIGN_OBJECTIVE_INVALID
CAMPAIGN_TARGETING_INVALID
CAMPAIGN_LANDING_URL_INVALID
CAMPAIGN_REFERENCE_OUTSIDE_WORKSPACE
CAMPAIGN_CONTENT_NOT_DEPLOYABLE
CAMPAIGN_PERMISSION_DENIED
```

---

### Task 9 - Campaign preflight

#### File tạo

```text
AISAM-BE/AISAM.Common/Dtos/Response/CampaignPreflightResponseDto.cs
```

#### File sửa

```text
AISAM-BE/AISAM.Services/IServices/IAdCampaignService.cs
AISAM-BE/AISAM.Services/Service/AdCampaignService.cs
AISAM-BE/AISAM.API/Controllers/AdCampaignController.cs
AISAM-FE/src/services/campaignService.ts
AISAM-FE/src/app/(dashboard)/campaigns/page.tsx
```

#### Endpoint

```text
POST /api/campaigns/{id}/preflight
```

#### Checks

```text
workspace
permission
platform
objective
budget
dates
targeting
landing_url
content
media
social_account
ad_account
provider_token
```

#### Response

```json
{
  "ready": false,
  "checks": [
    {
      "key": "targeting",
      "status": "failed",
      "message": "Targeting location is required."
    }
  ],
  "errors": 1,
  "warnings": 0
}
```

#### Deploy change

- [ ] `DeployAsync` chạy lại server-side preflight.
- [ ] Có failed check thì không gọi provider.
- [ ] Không tự lấy content đầu tiên.
- [ ] Không tự thêm targeting VN.
- [ ] Không tự giả định duration 30 ngày.
- [ ] FE hiển thị preflight modal trước deploy.

---

### Task 10 - Frontend Analytics reliability

#### File sửa chính

```text
AISAM-FE/src/services/analyticsService.ts
AISAM-FE/src/app/(dashboard)/analytics/page.tsx
AISAM-FE/src/components/analytics/AnalyticsFilterBar.tsx
AISAM-FE/src/components/analytics/AnalyticsKpiCards.tsx
AISAM-FE/src/components/analytics/AnalyticsChart.tsx
AISAM-FE/src/components/analytics/AnalyticsPerformanceTable.tsx
AISAM-FE/src/components/analytics/AnalyticsAiInsights.tsx
```

#### Lưu ý Next.js bắt buộc

Trước khi sửa code Next.js, đọc guide phù hợp trong:

```text
AISAM-FE/node_modules/next/dist/docs/
```

#### Việc cần làm

- [ ] Dùng một request model chung cho tất cả filter.
- [ ] Không gọi API trùng do hai `useEffect` song song.
- [ ] Có AbortController hoặc cơ chế tránh response cũ ghi đè response mới.
- [ ] Không catch rỗng.
- [ ] Giữ data cũ khi refresh lỗi, nhưng hiển thị stale/error banner.
- [ ] Hiển thị `—` cho metric unavailable.
- [ ] Hiển thị last synced.
- [ ] Disable Export khi không có data.
- [ ] CSV ghi date range, currency và last synced.
- [ ] AI section chỉ hiển thị dữ liệu từ endpoint AI thật; nếu không dùng thì đổi tên thành Summary.

#### UI states phải test

```text
loading
success
empty
not_configured
partial
stale
failed
```

---

### Task 11 - Frontend Campaign preflight và error handling

#### File tạo

```text
AISAM-FE/src/components/campaigns/CampaignPreflightModal.tsx
```

#### File sửa

```text
AISAM-FE/src/services/campaignService.ts
AISAM-FE/src/app/(dashboard)/campaigns/page.tsx
AISAM-FE/src/components/campaigns/CreateCampaignModal.tsx
AISAM-FE/src/components/campaigns/EditCampaignModal.tsx
AISAM-FE/src/components/campaigns/StartConfirmModal.tsx
```

#### Việc cần làm

- [ ] Form chỉ cho chọn platform/objective được hỗ trợ.
- [ ] Targeting sử dụng structured fields hoặc JSON editor có validation.
- [ ] Không cho submit URL không hợp lệ.
- [ ] Nút deploy gọi preflight.
- [ ] Hiển thị failed/warning checks.
- [ ] Chỉ xác nhận deploy khi `ready=true`.
- [ ] Hiển thị message/code thật từ backend.
- [ ] Không thay error cụ thể bằng “Failed to deploy” chung chung.
- [ ] Disable mutation controls nếu user không có `ManageCampaigns`.

---

### Task 12 - Regression, documentation và demo proof

#### Automated verification

```powershell
dotnet build AISAM-BE\AISAM.API\AISAM.API.csproj --no-restore
dotnet test AISAM-BE\tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --no-restore
npm.cmd run test -- --run
npm.cmd run lint
npm.cmd run build
```

Chạy npm trong:

```text
AISAM-FE
```

#### API smoke test

| API | Test | Expected |
| --- | --- | --- |
| `GET /api/analytics/overview` | Workspace có snapshot | KPI đúng, freshness có giá trị |
| `GET /api/analytics/time-series` | Cùng filter overview | Tổng khớp overview |
| `GET /api/analytics/audience` | Provider unavailable | Empty + unavailable, không fake data |
| `POST /api/analytics/sync` | Owner/Manager | Snapshot được upsert |
| `POST /api/analytics/sync` | Member không quyền | 403 |
| `POST /api/campaigns/{id}/preflight` | Campaign thiếu targeting | `ready=false` |
| `POST /api/campaigns/{id}/deploy` | Preflight fail | Không gọi provider |
| `POST /api/campaigns/{id}/deploy` | Valid campaign | Resume deployment đúng step |

#### UI smoke test

- [ ] Analytics không provider.
- [ ] Analytics có snapshot.
- [ ] Analytics sync lỗi.
- [ ] Đổi date/platform/brand/campaign filter.
- [ ] Export.
- [ ] Campaign create invalid.
- [ ] Campaign preflight fail.
- [ ] Campaign deploy success.
- [ ] User không có permission.

#### Tài liệu cập nhật

```text
AISAM-FE/README.md
docs/reference/backend/codebase-update.md
docs/main/setup-guide.md
```

Ghi rõ:

- Analytics source.
- Công thức KPI.
- Sync flow.
- Provider limitation.
- Dữ liệu unavailable khác dữ liệu zero.
- Campaign deploy prerequisites.

## 9. Kế hoạch thời gian rút gọn

### Phương án 7 ngày làm việc

| Ngày | Task |
| --- | --- |
| 1 | Task 1-2: KPI + bỏ dữ liệu giả |
| 2 | Task 3-4: entity, migration, repository |
| 3 | Task 5-6: provider daily insight + sync service |
| 4 | Task 7: snapshot aggregation |
| 5 | Task 8-9: Campaign validation + preflight |
| 6 | Task 10-11: FE Analytics + Campaign |
| 7 | Task 12: test, regression, docs, demo |

### Nếu chỉ còn 3-4 ngày

Thực hiện theo thứ tự:

1. Task 1.
2. Task 2.
3. Task 8.
4. Task 9.
5. Task 10-11 ở mức UI state/preflight.
6. Task 3-7 chỉ làm nếu Meta daily insight có thể xác minh.

Trong phương án này:

- Không tạo time-series nếu chưa có snapshot.
- Chỉ hiển thị campaign totals thật.
- Không giả dữ liệu để lấp biểu đồ.
- Ghi rõ “Historical analytics is not available yet”.

## 10. Risk register

| Rủi ro | Mức | Giảm thiểu |
| --- | --- | --- |
| Meta token thiếu insight permission | Cao | Preflight + actionable error + demo account chuẩn bị trước |
| Meta chưa trả conversion/revenue | Cao | Nullable KPI, không giả 0/ROAS |
| Migration ảnh hưởng shared DB | Cao | Migration additive, review SQL, rollback target rõ |
| Currency trộn lẫn | Cao | Group theo currency hoặc warning, không cộng trực tiếp |
| Sync API rate limit | Trung bình | Range tối đa 90 ngày, incremental sync, retry có giới hạn |
| FE/BE contract lệch | Trung bình | DTO contract tests và build sau từng task |
| Deployment provider/local lệch | Trung bình | Không đổi local active trước provider success; log reconciliation required |
| Scope phình sang AI optimization | Cao | AI nâng cao ngoài critical path |

## 11. Quy tắc rollback

- Task 1-2: khôi phục DTO/mapping cũ nhưng không phục hồi fake audience data.
- Task 3-4: migration additive; rollback về migration trước nếu chưa có production data.
- Task 5-7: có thể tắt manual sync endpoint và để Analytics trả unavailable.
- Task 8-9: nếu preflight provider check không ổn định, giữ validation local và trả warning cho provider connectivity; không bỏ validation workspace.
- Task 10-11: giữ component mới sau feature flag nếu UI cần rollback nhanh.

Không rollback bằng cách đưa dữ liệu giả trở lại production.

## 12. Definition of Done toàn plan

- [ ] Không còn hard-coded Analytics data trong production path.
- [ ] Không còn fabricated daily time-series/sparkline.
- [ ] KPI theo đúng bảng canonical.
- [ ] Snapshot migration additive và apply thành công.
- [ ] Sync idempotent.
- [ ] Overview/time-series cùng filter khớp tổng.
- [ ] Workspace isolation được test.
- [ ] Campaign reference validation đầy đủ.
- [ ] Campaign deploy bắt buộc preflight.
- [ ] Middleware `ManageCampaigns` không bị suy yếu.
- [ ] FE phân biệt unavailable và zero.
- [ ] Backend build/test pass.
- [ ] Frontend test/lint/build pass.
- [ ] API/UI smoke test pass.
- [ ] Tài liệu nguồn dữ liệu và giới hạn provider đã cập nhật.
- [ ] Không tự ý commit; chỉ commit khi người dùng yêu cầu.

## 13. Điểm dừng bắt buộc

Dừng implementation và báo lại nếu gặp một trong các trường hợp:

- Không xác định được Meta insight permission cần thiết.
- Provider không thể trả daily data với token hiện tại.
- Migration dự kiến drop/rename dữ liệu hiện có.
- Không xác định được conversion action canonical.
- Một workspace có nhiều currency trong cùng report mà API chưa có cách biểu diễn.
- Cần thay đổi scope sang auto optimization hoặc AI tự apply.

Không giải quyết blocker bằng dữ liệu hard-code hoặc số liệu suy đoán.
