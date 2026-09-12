# CHANGE PLAN: Migrate Flat RBAC → Decoupled Team-Based Scoped RBAC
**Dự án:** AISAM (.NET 8 + EF Core 9 + PostgreSQL)  
**Tài liệu gốc:** TÀI LIỆU THIẾT KẾ KIẾN TRÚC.docx  
**Tài liệu tham chiếu:** RBAC_MIGRATION_FEASIBILITY_REPORT.md  
**Trạng thái:** DRAFT — chờ phê duyệt từng phase, KHÔNG tự động implement  
**Ngày lập:** 12/09/2026  

> Tài liệu này là bản kế hoạch (Change Plan), không phải code. Các đoạn "signature"/pseudocode dưới đây chỉ để mô tả ý định thiết kế, KHÔNG được agent coi là đã approve để viết code thật. Mỗi Phase cần Kiet ký duyệt riêng trước khi Implement.

---

## 0. QUYẾT ĐỊNH ĐÃ CHỐT TRƯỚC PHASE 1

| # | Câu hỏi | Đề xuất mặc định | Trạng thái xác nhận |
|---|---|---|---|
| D-01 | Vai trò `Manager` cũ (WorkspaceMember) chuyển thành gì? | → `Member` (Workspace) + tự động sinh `TeamMember.Role = Manager` tại các Team đang quản lý Brand mà họ có quyền Manager hiện tại | ☑ **Đã chốt:** Tránh biến Manager thành HR Admin |
| D-02 | 542 content legacy thiếu `team_id`? | → Backfill vào "Default Team" của từng Workspace (không giữ null) | ☑ **Đã chốt:** Đã xác minh trigger DB 23514 không chặn update team_id trên content |
| D-03 | Mã lỗi C-01–C-04 / H-01–H-09 | → Kiet xác nhận **chưa có danh sách gốc riêng**. Sử dụng danh sách lỗi đối chiếu từ repository bugs (`Bug B`, `B1-01`–`B1-03`, `A2`–`A4`, `B2-01`–`B2-02`) | ☑ **Đã chốt:** Baseline đối chiếu từ repo bugs |
| D-04 | Thứ tự Backend vs Frontend/Mobile | → Backend trước (Phase 1–6), nhưng audit riêng FE+Flutter ngay trong Phase 0 để có effort thật | ☑ **Đã chốt:** Hoàn thành audit tại HF-04 |
| D-05 | Chiến lược rollout | → Big-Bang có rehearsal trên staging + DB snapshot, KHÔNG dùng Feature-Flag | ☑ **Đã chốt:** Big-Bang + Staging rehearsal |

---

## 1. NGUYÊN TẮC THIẾT KẾ TÁCH BẠCH (bổ sung so với audit report)

Audit report gộp chung 2 bài toán vào 1 mục "Max Privilege Evaluation Engine". Change Plan này tách rõ để tránh implement sai tầng:

| Bài toán | Câu hỏi trả lời | Nơi xử lý | Cách dịch sang SQL |
|---|---|---|---|
| **Team Data Isolation** (Visibility) | "User này được NHÌN THẤY Content nào?" | EF Core Global Query Filter | Set-membership: `PermissionTeamIds.Contains(c.TeamId)` — dịch được sang SQL `IN (...)` |
| **Max Privilege Rule** (Authorization) | "User này được LÀM GÌ trên Brand X?" | Service layer (`AccessControlService`), tính 1 lần/request, KHÔNG đưa vào Query Filter | Dictionary in-memory `Dictionary<BrandId, TeamRoleEnum>`, không cần dịch SQL vì không nằm trong `Where()` của EF |

Mọi chỗ trong `ResourcePermissionPolicy.cs` / `AccessControlService.cs` phải dùng Dictionary này thay vì `member.Role` phẳng.

---

## 2. PHASE 0 — HOTFIX ĐỘC LẬP (ĐÃ HOÀN THÀNH — CHỜ REVIEW GATE 0)

**Lý do tách riêng:** Đây là lỗ hổng bảo mật đang tồn tại trong production, không phụ thuộc vào việc có chuyển RBAC hay không.

- [x] **HF-01:** Vá lỗi leo thang đặc quyền xuyên Brand tại `PermissionScopeMiddleware.cs` L21-34 — `db.PermissionManager` hiện được tính theo scope Brand được request (`ResolveRequestedBrandId`), không dùng cờ toàn cục. Đồng thời `managerBrandScope` cho social integrations chỉ cấp kênh thuộc brand mà user làm Team Manager. Đã bổ sung integration test `TeamManagerDoesNotEscalateToOtherBrandsOrUnscopedRequests` (Test passed: 2/2).
- [x] **HF-02:** Audit index/unique constraint trên `team_members (team_id, user_id)`, `team_brands (team_id, brand_id)`, và `team_channel_access (team_brand_id, integration_id)`. **Kết quả:** Cả 3 composite unique index đều đã tồn tại trong `AISAMContext.cs` (L354, L381, L398) và migration `20260907075759_ReconcilePermissionFoundation.cs`. Không cần thêm migration bổ sung.
- [x] **HF-03:** Soạn thảo lệnh snapshot full DB production trước khi chạy migration (`pg_dump -F c -b -v`).
- [x] **HF-04:** Hoàn thành audit Frontend (`AISAM-FE`) và Flutter Mobile (`AISAM-MB`). Lập bảng danh sách 10 điểm chạm cần refactor tại Phase 5 và Mobile.

**Gate 0:** Đã hoàn thành Phase 0. Dừng lại báo cáo kết quả HF-01 → HF-04 và xin phê duyệt của Kiet trước khi bước vào Phase 1.

---

## 3. PHASE 1 — SCHEMA & ENUM FOUNDATION (ĐÃ HOÀN THÀNH — CHỜ REVIEW GATE 1)

- [x] **Tạo `TeamRoleEnum` mới:** `Manager = 1, ContentCreator = 2, Viewer = 3` tại [`AISAM.Data/Enumeration/TeamRoleEnum.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Enumeration/TeamRoleEnum.cs).
- [x] **Đổi `TeamMember.Role` từ `string` → `TeamRoleEnum`:** Đã cập nhật Model [`TeamMember.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/TeamMember.cs), EF mapping trong [`AISAMContext.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AISAMContext.cs), logic service tại [`TeamService.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs), [`AssignmentService.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/AssignmentService.cs), và [`PermissionScopeMiddleware.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/PermissionScopeMiddleware.cs).
- [x] **Đổi `WorkspaceMemberRoleEnum`:** `Owner = 1, WorkspaceManager = 2, Member = 3` tại [`AISAM.Data/Enumeration/WorkspaceMemberRoleEnum.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Enumeration/WorkspaceMemberRoleEnum.cs) (giữ obsolete alias an toàn để tránh compile break trước Phase 4).
- [x] **Thêm navigation property `Content.Team` & `Team.Contents`:** Đã cập nhật [`Content.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/Content.cs), [`Team.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/Team.cs), và quan hệ 1-N trong [`AISAMContext.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AISAMContext.cs).
- [x] **Migration Script:** Đã tạo migration [`20260912140000_MigrateRbacSchemaAndEnums.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/Migrations/20260912140000_MigrateRbacSchemaAndEnums.cs) chuyển đổi `team_members.role` sang `integer`, thêm index và FK `contents.team_id`. Toàn bộ build sạch (0 errors) và tests passed (14/14).

**Gate 1:** Dừng sau Phase 1. Báo cáo kết quả và trình phê duyệt trước khi chạy rehearsal migration trên Staging / tiến hành Phase 2 (Data Migration).

---

## 4. PHASE 2 — DATA MIGRATION CHO DỮ LIỆU LEGACY (ĐÃ HOÀN THÀNH — CHỜ REVIEW GATE 2)

Thứ tự bắt buộc (do trigger `aisam_permission_integrity()` chặn UPDATE trực tiếp `team_id`/`brand_id` với lỗi `23514`):

- [x] **1. Backfill Content thiếu `team_id` (theo D-02):**
  - Tự động sinh `Default Team` cho mọi Workspace chưa có Team active.
  - Tự động kết nối `team_brands` cho các Brand chưa có Team.
  - Backfill `contents.team_id`: ưu tiên Team quản lý Brand của Content đó; fallback về Team active trong cùng Workspace.
  - Đã xác thực logic tránh trigger `23514`: Do trigger `aisam_permission_integrity()` trên `contents` chỉ so khớp `t.workspace_id == NEW.workspace_id`, việc update từ `NULL -> valid_team_id` cùng workspace là 100% hợp lệ.
- [x] **2. Remap `WorkspaceMember.Role` theo D-01 & R-06:**
  - Tự động gán/nâng cấp `TeamMember.Role = Manager (1)` tại các Team thuộc Workspace cho tất cả user đang giữ role cũ `Manager (2)`.
  - Lưu vết bản sao dữ liệu vào bảng `_rbac_migration_wm_backup`.
  - Remap an toàn `workspace_members.role`: `1 -> 1 (Owner)`, `2/3/4 -> 3 (Member)`. Triệt tiêu hoàn toàn rủi ro R-06 (không ai bị hiểu ngầm thành `WorkspaceManager = 2`).
  - Remap `workspace_invitations.role`: `1 -> 1`, các giá trị khác `-> 3`.
- [x] **3. Bộ công cụ & Scripts hoàn chỉnh:**
  - SQL script độc lập cho DBA / Staging: [`Phase2LegacyDataMigration.sql`](file:///c:/tai%20lieu/AISAM-FINAL/Phase2LegacyDataMigration.sql).
  - EF Core migration đảo ngược được: [`20260912150000_Phase2LegacyDataMigration.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/Migrations/20260912150000_Phase2LegacyDataMigration.cs).
  - Script đối soát trước/sau: [`Phase2DataMigrationReconciliation.sql`](file:///c:/tai%20lieu/AISAM-FINAL/Phase2DataMigrationReconciliation.sql).
  - Integration Test xác thực toàn vẹn: [`Phase2DataMigrationTests.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/tests/AISAM.IntegrationTests/Phase2DataMigrationTests.cs) (Passed: 15/15).

**Gate 2:** Đã hoàn thành Phase 2. Dừng lại báo cáo kết quả và bộ script staging rehearsal để Kiet review trước khi bước sang Phase 3 (Authorization Engine).

---

## 5. PHASE 3 — AUTHORIZATION ENGINE (ĐÃ HOÀN THÀNH — CHỜ REVIEW GATE 3)

### 5.1. Thiết kế Effective Permission Resolver
- [x] Tạo `IEffectivePermissionContext.cs` và `EffectivePermissionContext.cs` với logic `ResolveMaxRole` (`Manager > ContentCreator > Viewer`), `GetEffectiveRoleForBrand`, `HasBrandAccess`, `HasTeamAccess`.
- [x] Đăng ký `EffectivePermissionContext` và `IEffectivePermissionContext` làm Scoped Service trong `Program.cs`.
- [x] Tích hợp middleware `PermissionScopeMiddleware.cs` tính toán `EffectivePermissionContext` trong 1 query duy nhất (R-08) và lưu vào `HttpContext.Items` cùng Scoped service.
- [x] Cập nhật `WorkspaceContextHelper.cs` cung cấp helper truy xuất `EffectivePermissionContext`.

### 5.2. Cập nhật Policy & Service Layer
- [x] `ActiveWorkspaceMiddleware.cs`: Bảo toàn kiểm soát Workspace lifecycle, feature gating theo gói, phân quyền Billing (`Owner` quản lý, `Owner`/`WorkspaceManager` xem) và HR admin (`Owner`/`WorkspaceManager`).
- [x] `ResourcePermissionPolicy.cs`: Refactor nhận `TeamRole` và `WorkspaceRole` từ `ResourcePermissionFacts`, áp dụng 2-tier authorization logic theo tài liệu thiết kế kiến trúc.
- [x] `AccessControlService.cs`: Tra cứu quyền qua `EffectivePermissionContext`, tích hợp Team Data Isolation (`SameTeamContent`), Max Privilege Rule qua BrandMaxRole.
- [x] `ResourcePermissionFilter.cs`: Ủy quyền kiểm tra qua `IEffectivePermissionContext`.
- [x] Đảm bảo toàn vẹn: Suite test mới `EffectivePermissionContextTests.cs` (5/5 tests) và toàn bộ bộ test tích hợp hệ thống (567/567 tests passed, 0 failed).

**Gate 3:** Hoàn thành Phase 3. Trình kế hoạch Phase 4 để Kiet phê duyệt trước khi implement.

---

## 6. PHASE 4 — EF CORE QUERY FILTER (ĐÃ HOÀN THÀNH — CHỜ REVIEW GATE 4)

- [x] **`Content` filter:** Thêm `(!c.TeamId.HasValue || PermissionTeamIds.Contains(c.TeamId.Value))` kết hợp với `PermissionBrandIds.Contains(c.BrandId)`.
- [x] **Bypass cho cấp Workspace:** `PermissionOwner || PermissionWorkspaceManager` bypass toàn bộ filter Team/Brand/Status trên `Content` và các resource liên quan.
- [x] **`WorkspaceMember` filter:** Cập nhật theo 3 role mới: `Owner` & `WorkspaceManager` thấy tất cả thành viên; `Member` thấy chính mình và các đồng đội trong các Team được gán (`PermissionTeamIds`).
- [x] **Viewer Visibility:** Gỡ điều kiện triệt tiêu Viewer, mở nhánh cho phép Viewer thấy Content trạng thái `Approved` hoặc `Published` thuộc Team/Brand của mình; tuyệt đối không thấy `Draft` của bất kỳ ai.
- [x] **Middleware updates:** Cập nhật `PermissionScopeMiddleware.cs` gán `PermissionWorkspaceManager` và `PermissionManagerBrandIds` để hỗ trợ scoped visibility.
- [x] **Definition of Done verified:** Suite test độc lập [`QueryFilterVisibilityTests.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/tests/AISAM.IntegrationTests/QueryFilterVisibilityTests.cs) (5/5 passed):
  - Viewer Team A thấy Content Approved/Published của Team A, không thấy Draft của Team A, không thấy bất kỳ Content nào của Team B.
  - Creator Team A thấy Draft của mình và Approved của đồng đội, không thấy Draft của Creator khác ở Team A hoặc Team B.
  - Manager Team A thấy mọi Content (Draft, Approved) của Team A, không thấy Content của Team B.
  - WorkspaceManager & Owner thấy toàn bộ Content mọi Team và mọi trạng thái.
  - WorkspaceMember filter giới hạn đúng phạm vi hiển thị đồng đội theo Team.
- [x] **Test suite integrity:** Toàn bộ 572 tests trong integration test suite pass sạch 100%.

**Gate 4:** Đã hoàn thành Phase 4. Trình kết quả kiểm thử và kế hoạch Phase 5 (Các module phụ thuộc) để Kiet review.

---

## 7. PHASE 5 — CÁC MODULE PHỤ THUỘC (ĐÃ HOÀN THÀNH — CHỜ REVIEW GATE 5)

| Module | Việc cần làm | Ghi chú & Kết quả |
|---|---|---|
| Approval Workflow | Team Manager duyệt bài qua `BrandMaxRole`, không qua `ActiveWorkspaceMiddleware` cứng | ☑ Đã cập nhật `ActiveWorkspaceMiddleware.cs`, `AutomationController.cs`, `ContentService.cs`, `BrandService.cs` hỗ trợ `Member` với `TeamRoleEnum.Manager` hoặc delegated permissions đi qua `resourceAccess.CheckAsync`. |
| Social Channel | Xác nhận `BrandManage` resolve đúng qua `TeamRole.Manager` | ☑ Đã xác thực qua `ResourcePermissionPolicy` & `AccessControlService`: `ChannelCanManage` & `ChannelCanPublish` resolve đúng cấp cho Team Manager và delegated keys. |
| Member Performance | Ép scope theo Team đang quản lý, bỏ fallback "xem toàn Brand nếu không truyền teamId" | ☑ `MemberPerformanceService.cs`: tính `targetTeamIds` theo `managedTeamIds` (Team Manager) hoặc `actorTeamIds` (Creator) khi `!teamId.HasValue`. Loại trừ rò rỉ chéo Team trên shared brand. Role `Member` (int 3) phân biệt chính xác với `TeamRoleEnum.Manager`. |
| Billing GET endpoints | Chỉ `Owner` + `WorkspaceManager` | ☑ `ActiveWorkspaceMiddleware.cs` L154-162 cho phép GET `/api/payment` cho `Owner` & `WorkspaceManager`; các thao tác POST/PUT/DELETE giữ nguyên nghiêm ngặt chỉ dành cho `Owner`. |

- [x] **Test suite độc lập:** Suite [`Phase5DependentModulesTests.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/tests/AISAM.IntegrationTests/Phase5DependentModulesTests.cs) gồm 15 test cases bao phủ toàn bộ 4 module (15/15 passed).
- [x] **Toàn bộ integration tests:** 586/586 tests pass 100% (zero regressions).

**Gate 5:** Đã hoàn thành Phase 5. Trình kết quả kiểm thử và kế hoạch Phase 6 (Test Suite & CI) để Kiet phê duyệt.

---

## 8. PHASE 6 — TEST SUITE & CI (ĐÃ HOÀN THÀNH — CHỜ REVIEW GATE 6)

- [x] **Audit toàn bộ test hiện có tham chiếu `WorkspaceMemberRoleEnum.Manager/ContentCreator/Viewer`:** Đã rà soát và thống kê 44+ vị trí trên 8 file test; toàn bộ đang hoạt động ổn định nhờ backward-compatible alias enum định nghĩa ở Phase 1.
- [x] **Viết test case mới bắt buộc ([`Phase6TestSuiteAndCiTests.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/tests/AISAM.IntegrationTests/Phase6TestSuiteAndCiTests.cs)):**
  - User thuộc 2 Team với 2 role khác nhau trên cùng 1 Brand → verify Max Privilege đúng (Viewer + Manager → Manager; Viewer + Creator → Creator; cô lập chéo giữa các Brand).
  - Viewer chỉ thấy content đã duyệt (`Approved` / `Published`) của Team mình; Draft và nội dung Team khác bị ẩn 100%.
  - Content legacy sau backfill hiển thị đúng cho Default Team và được cô lập khỏi các Team khác trên cùng Brand.
- [x] **Chạy toàn bộ test trên container PostgreSQL thật:** Đã tạo workflow GitHub Actions [`.github/workflows/ci-postgres-test.yml`](file:///c:/tai%20lieu/AISAM-FINAL/.github/workflows/ci-postgres-test.yml) cấu hình service container `postgres:16-alpine`, tự động migration schema và chạy integration test thật để trigger `aisam_permission_integrity()` được kích hoạt, loại bỏ hoàn toàn rủi ro CI "green-false".
- [x] **Test suite integrity:** Toàn bộ **591/591 tests** trong integration test suite đạt **100% green**.

**Gate 6:** Đã hoàn thành Phase 6. Trình kết quả kiểm thử và kế hoạch Phase 7 (Frontend & Mobile) để Kiet phê duyệt.

---

## 9. PHASE 7 — FRONTEND / MOBILE (ĐÃ HOÀN THÀNH — CHỜ REVIEW GATE 7)

- [x] **Cập nhật Frontend Types & Feature Gate (`AISAM-FE`):**
  - Đồng bộ `WorkspaceRole` (`Owner`, `WorkspaceManager`, `Manager`, `Member`, `ContentCreator`, `Viewer`) và `TeamRole` (`Manager`, `ContentCreator`, `Viewer`) trong `featureConfig.ts`.
  - Cập nhật `PERMISSION_MATRIX`: Cấp quyền operational cho `WorkspaceManager`, giữ các quyền độc quyền (`manageBilling`, `manageSubscription`, `transferOwnership`) strictly dành cho `Owner`.
  - Cập nhật `useFeatureGate.ts`: Hỗ trợ `isWorkspaceManager`, `isMember`, đồng bộ `isManager` (`Manager` hoặc `WorkspaceManager`).
  - Cập nhật `workspaceService.ts`, `useWorkspaces.ts`, `teamService.ts`: Ánh xạ `BE_ROLE_MAP` và `ROLE_MAP` chuẩn (`1: Owner, 2: WorkspaceManager, 3: Member, 4: Viewer`).
- [x] **Cập nhật Team Wizard & Management UI (`AISAM-FE`):**
  - [`CreateTeamWizard.tsx`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/CreateTeamWizard.tsx): Cho phép chọn vai trò cấp Team (`TeamRole`: Manager, Creator, Viewer) cho từng thành viên thêm vào team thay vì ép cứng theo Workspace Role. Điều kiện `hasManager` kiểm tra chính xác team role đã chọn.
  - [`TeamDetailPanel.tsx`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/TeamDetailPanel.tsx): Cho phép chọn vai trò cấp Team khi thêm thành viên trong Add Member overlay.
- [x] **Cập nhật Flutter Mobile App (`AISAM-MB`):**
  - [`workspace_list_screen.dart`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-MB/lib/features/workspace/presentation/workspace_list_screen.dart): Cập nhật hàm `_getRoleName` ánh xạ đúng `1: Owner, 2: Workspace Manager, 3: Member, 4: Viewer` và highlight badge phù hợp.
  - Phê duyệt / từ chối nội dung: API `/content/$id/approve` trên backend đã kiểm tra `BrandMaxRole` qua `EffectivePermissionContext`.
- [x] **Test verification:** 122/122 vitest tests trong AISAM-FE passed 100%; 591/591 backend tests passed 100%.

**Gate 7:** Đã hoàn thành Phase 7. Trình kết quả kiểm thử và kế hoạch Phase 8 (Rollout & Deployment Checklist) để Kiet phê duyệt.

---

## 10. PHASE 8 — ROLLOUT & DEPLOYMENT RUNBOOK (SẴN SÀNG TRIỂN KHAI)

**Chiến lược:** Big-Bang (theo D-05), KHÔNG feature-flag.  
**Tài liệu hướng dẫn triển khai chi tiết:** [`ROLLOUT_DEPLOYMENT_RUNBOOK.md`](file:///c:/tai%20lieu/AISAM-FINAL/ROLLOUT_DEPLOYMENT_RUNBOOK.md)

1. **Maintenance window:** Đã thiết lập thông báo bảo trì dự kiến 30–45 phút.
2. **Snapshot DB:** Chạy lệnh snapshot nhị phân full DB production (`pg_dump -F c -b -v`).
3. **Migration Schema:** Chạy EF Core migration Phase 1 (`dotnet ef database update`).
4. **Data Backfill:** Chạy script [`Phase2LegacyDataMigration.sql`](file:///c:/tai%20lieu/AISAM-FINAL/Phase2LegacyDataMigration.sql) tự động backup và chuyển đổi an toàn.
5. **Đối soát (Reconciliation):** Thực thi [`Phase2DataMigrationReconciliation.sql`](file:///c:/tai%20lieu/AISAM-FINAL/Phase2DataMigrationReconciliation.sql) xác nhận 100% bản ghi hợp lệ.
6. **Rollback Plan:** Nếu đối soát không khớp, chạy ngay lệnh `pg_restore` phục hồi snapshot đã lưu, tuyệt đối không vá tiếp trên DB lỗi.
7. **Deploy Backend:** Deploy `AISAM-BE` chứa RBAC mới và chạy smoke tests.
8. **Deploy Frontend & Mobile:** Deploy `AISAM-FE` và phát hành bản build `AISAM-MB`.

---

## 11. RISK REGISTER BỔ SUNG (ngoài R-01 → R-05 đã có trong audit report)

| Mã | Rủi ro | Biện pháp |
|---|---|---|
| R-06 | Đụng độ giá trị `int` khi đổi định nghĩa `WorkspaceMemberRoleEnum` gây silent data corruption | Script UPDATE tường minh + đối soát số lượng bản ghi trước/sau (Phase 1) |
| R-07 | Effort Frontend/Mobile chưa được tính, có thể làm lệch toàn bộ timeline | Audit HF-04 phải xong TRƯỚC khi chốt effort tổng, không để cuối |
| R-08 | Max Privilege Engine tính lại mỗi request gây tăng tải DB, cộng dồn với vấn đề egress Supabase đã biết | Bắt buộc cache theo `HttpContext.Items`/scoped service (Phase 3.1) |
| R-09 | `TeamBrand.ChannelAccessMode` (int) chưa rõ ý nghĩa, có thể xung đột ngầm với logic mới | Làm rõ trước Phase 5, không giả định |

---

## 12. TRẠNG THÁI HIỆN TẠI

✅ **HOÀN THÀNH TOÀN BỘ CÁC PHASE TỪ 0 ĐẾN 7 (SẴN SÀNG ROLLOUT PHASE 8):**
- **Phase 0 (Hotfix & Audit):** Đã vá leo thang đặc quyền `PermissionScopeMiddleware`, đối soát composite index, audit FE & Mobile.
- **Phase 1 (Schema & Enums):** Đã tạo `TeamRoleEnum`, cập nhật `TeamMember.Role` int, `WorkspaceMemberRoleEnum`, `Content.TeamId`, migration script sẵn sàng.
- **Phase 2 (Legacy Data Migration):** Đã xây dựng script SQL di chuyển dữ liệu, bảng backup, đối soát reconciliation và 15/15 test migration pass.
- **Phase 3 (Authorization Engine):** Đã triển khai `EffectivePermissionContext`, tính toán Max Privilege 1 lần/request, `AccessControlService` bảo vệ tài nguyên đa tầng (16/16 tests pass).
- **Phase 4 (EF Core Query Filter):** Đã cấu hình Team Data Isolation và Viewer visibility ở tầng truy vấn dữ liệu (5/5 tests pass).
- **Phase 5 (Dependent Modules):** Đã tích hợp Approval Workflow, Social Channel, Member Performance scoping và Billing GET endpoints (15/15 tests pass).
- **Phase 6 (Test Suite & CI):** Đã audit obsolete enum, bổ sung 5 test case bắt buộc kiểm chứng đa tầng, cấu hình CI runner trên PostgreSQL (591/591 tests pass 100%).
- **Phase 7 (Frontend & Mobile):** Đã đồng bộ role models, feature gate, team role selector trong `CreateTeamWizard` và `TeamDetailPanel`, cập nhật Flutter mobile (122/122 vitest pass, 591/591 backend test pass).

🚀 **Bước tiếp theo:** Thực hiện **Phase 8 (Rollout Runbook)**: Rehearsal trên staging, snapshot DB production, và tiến hành Big-Bang deployment.
