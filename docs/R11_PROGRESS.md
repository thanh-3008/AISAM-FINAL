# R11 — Hồi quy và kiểm tra migration

Ngày kiểm tra: 15/09/2026. Phạm vi: mã nguồn hiện tại và môi trường kiểm thử cục bộ; không bật RBAC v2 trên database ứng dụng.

Trạng thái: **hoàn thành hồi quy cục bộ R11**, với giới hạn nghiệm thu và dữ liệu cần rà soát được ghi bên dưới.

## Lỗi phát hiện và đã sửa

1. **Tác giả nội dung từ request tương tác:** `PrimaryCreatorId` truyền sẵn trước đây có thể được giữ lại. `AisamContext.PermissionIntegrity` nay luôn lấy `ExecutionActorId` khi `ExecutionIsSystem=false`. Worker giữ attribution đã xác minh từ job. Ca hồi quy `ServerActorOverridesInputAndAttributesUpdatePublishAndSchedule` đạt.
2. **Xung đột HR trả sai 500:** Npgsql có thể bọc lỗi PostgreSQL `40001` trong `InvalidOperationException`/`DbUpdateException`. Filter hiện nhận diện chuỗi inner exception, trả 409, không retry mutation. Đồng thời xử lý trường hợp MVC trả exception trong `ActionExecutedContext`, đánh dấu đã xử lý và giữ rollback.
3. **Fixture E2E chưa theo contract:** bổ sung context legacy có revision; fixture v2 expose header HR như CORS backend thật. Assertion 403 dùng thông báo tiếng Việt thực tế của ứng dụng.

## Bằng chứng

| Kiểm tra | Kết quả / nguồn |
|---|---|
| Backend toàn bộ | 600/600 đạt, không skip; `.artifacts/r11/r11-backend.trx` |
| Web unit/component | 133/133 đạt, 37 file; `.artifacts/r11-web-unit.log` |
| Web E2E trên Edge | 24/24 đạt; gồm 3 ca v2 Owner/WorkspaceManager/Member, header revision, payload role mới và 403 không logout/retry; `.artifacts/r11-e2e.log` |
| PostgreSQL migration R01 | Backup và fresh schema đều đạt; `.artifacts/r11-migration.log` |
| PostgreSQL migration bổ sung | Down/Up cho `AddAutomationTeamScope`, `AddConversationTeamScope` trên schema tạo từ model hiện tại đạt |
| PostgreSQL HR đồng thời | Hai request cùng revision: một commit, một 409; chỉ một Team tồn tại; request stale sau commit tiếp tục bị chặn |
| MVC wrapped exception | Hai test bổ sung: exception ném trực tiếp và exception trả trong action result đều chuyển thành 409 |
| Mobile | Kế thừa bằng chứng R10: 59 test, bundle đạt; R11 không thay đổi Mobile |

Log PostgreSQL mới: `.artifacts/r11-postgres.log`. Công cụ `AISAM-BE/tools/RbacV2RegressionCheck` dùng hai connection thật, database cố định `aisam_r01_verification` và schema ngẫu nhiên riêng; `finally` xóa đúng schema kiểm thử. Không đọc `.env`, không ghi `aisam_local`.

## Đối chiếu tình huống quyền

| Nhóm yêu cầu | Bằng chứng hồi quy |
|---|---|
| Member không Team; Creator/Viewer không được nâng quyền | `RbacV2Tests`, `WorkspaceHrV2PolicyTests`, `ContentServiceTests`, `AIServiceTests` |
| Manager A/Viewer B; hai Team cùng Brand | `MixedTeamsChannelsAndRevocation`, `ContentMediaTests`, `PermissionQueryScopeTests` |
| Creator chỉ sửa đúng tác giả/trạng thái; Viewer không đọc draft | `ContentServiceTests`, `RbacV2Tests.ReviewAndPublishRequireRoleAndState`, `PermissionIntegrityTests` |
| TeamManager không quản trị workspace/Brand/social | `TeamV2Tests`, `BrandV2Tests`, `AssignmentV2Tests`, các test social v2 và HR policy |
| WorkspaceManager đọc billing nhưng không thanh toán | `RbacV2Tests.BillingAndBoundary`, test middleware/payment |
| Thu hồi Team/member/Brand/kênh và job chờ | `TeamV2Tests`, `AssignmentV2Tests`, `AutomationCreatorTests`, `RbacV2Tests`, test publishing/schedule |
| Nội dung/asset/conversation và thống kê không vượt scope | `ContentMediaTests`, `ConversationHistoryRequiresCreatorAndCurrentWriteTeamScope`, các test query scope/KPI của R08 |
| Đổi workspace khi request còn chạy; 403 không logout | Web apiClient/WorkspaceBoundary unit tests, Mobile interceptor R10, Web E2E v2 |
| Backfill không tự cấp membership, không đổi TeamId đã có | Công cụ migration trên bản sao PostgreSQL; chạy lại và rollback đạt |
| Legacy role/grant không thành quyền v2 | `RbacV2Tests`, `WorkspaceHrV2PolicyTests`, parser Web/Mobile, báo cáo migration |

Các test API/controller/service dùng database InMemory/SQLite hoặc mock theo từng fixture. E2E chạy trình duyệt thật nhưng **mock API**, không phải bằng chứng toàn tuyến Web → API → PostgreSQL. PostgreSQL thật được kiểm tra riêng cho migration và concurrency HR; không gọi email, OAuth, AI provider, đăng bài hoặc checkout thật.

## Giới hạn và việc tiếp theo

- Bản sao migration còn **2 `channel_scope_needs_review`**. Giữ scope đóng; Owner phải rà soát trước bật v2. Không tự phê duyệt để làm báo cáo trống.
- Fresh schema được tạo từ EF model, không chứng nhận toàn bộ lịch sử migration legacy chạy từ database trống. Rollback schema mới rỗng không chứng nhận rollback an toàn sau khi đã phát sinh dữ liệu v2; rollout cần backup/restore theo R12.
- Chưa thay connection string, chưa áp dụng migration vào database ứng dụng, chưa bật `Rbac:UseV2`.
- R12 còn nghiệm thu môi trường đích, dữ liệu thật, thiết bị Mobile và provider sandbox. Không gọi tiến độ này là hoàn thành triển khai 100%.

## Chạy lại

Từ gốc repository, dùng SDK .NET đã cấu hình trên máy:

```powershell
dotnet test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
# PGPASSWORD được cấu hình trong terminal; chỉ database kiểm thử cố định được sử dụng.
dotnet run --project AISAM-BE/tools/RbacV2MigrationCheck/RbacV2MigrationCheck.csproj
dotnet run --project AISAM-BE/tools/RbacV2RegressionCheck/RbacV2RegressionCheck.csproj
```

Trong `AISAM-FE`: `npm test -- --run`; sau khi có production build, đặt `$env:E2E_BROWSER_CHANNEL='msedge'` rồi `npx playwright test`. Dùng Edge có sẵn vì Chromium do Playwright quản lý chưa được cài trên máy.
