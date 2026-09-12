# AISAM — Pattern Sweep Audit Report
**Mục tiêu:** Rà soát toàn bộ codebase để tìm các instance mắc cùng loại pattern lỗi với Bug A–E trước khi chốt triển khai (Implement).  
**Nguyên tắc thực hiện:** Read-only 100%, không sửa code sản phẩm, kèm bằng chứng (file, line number, code trích xuất trực tiếp) và phân tích nguyên nhân gốc rễ.

---

## TỔNG KẾT NHANH CÁC PHÁT HIỆN MỚI

| Nhóm Pattern | Trạng thái rà soát | Số instance mới phát hiện | Mã định danh các phát hiện mới |
| :--- | :--- | :---: | :--- |
| **Pattern A** (Filter AND chồng / Thiếu Fallback) | **Phát hiện thêm** | **3** | `A2`, `A3`, `A4` |
| **Pattern B1** (BOLA: Chỉ check WorkspaceId, thiếu check Actor trên Resource) | **Phát hiện thêm** | **3** | `B1-01`, `B1-02`, `B1-03` |
| **Pattern B2** (Hardcode quyền theo role Owner thay vì theo Resource) | **Phát hiện thêm** | **2** | `B2-01`, `B2-02` |
| **Pattern C** (Validate step return true // optional vô điều kiện) | **Không phát hiện thêm** | **0** | Đã rà soát toàn bộ 5 modal/wizard |
| **Pattern D** (Nuốt Exception / URL "ma" không tồn tại) | **Không phát hiện thêm** | **0** | Đã rà soát toàn bộ 57 controller routes |
| **Pattern E** (Mảng status frontend lệch/thiếu so với Enum backend) | **Phát hiện thêm** | **2** | `E2`, `E3` |

---

## 1. PATTERN A — Query Filter / Điều kiện xem dữ liệu bị "AND chồng" quá mức & Thiếu Fallback

### Danh sách các Entity đã rà soát:
Đã kiểm tra toàn bộ 26 khai báo `HasQueryFilter` trong [AisamContext.PermissionScope.cs](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Infrastructure/Context/AisamContext.PermissionScope.cs) và [AisamContext.Media.cs](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Infrastructure/Context/AisamContext.Media.cs):
`Brand`, `Product`, `Content`, `SocialIntegration`, `SocialAccount`, `AutomationPlan`, `AutomationItem`, `Asset`, `Post`, `AudienceInsight`, `ContentCalendar`, `Approval`, `WorkflowInstance`, `WorkflowStepInstance`, `AdCampaign`, `AdGroup`, `AdCreative`, `Notification`, `AuditLog`, `WorkspaceMember`, `Team`, `TeamMember`, `TeamBrand`, `TeamChannelPermission`, `MediaAsset`, `MediaFolder`.

### Kết luận: **Phát hiện thêm 3 instance mới** (`A2`, `A3`, `A4`)

---

### Instance A2 — Entity `AutomationItem`: Bắt buộc BrandId phải có giá trị và nằm trong PermissionBrandIds (Thiếu bypass Owner và bỏ rơi item chưa gắn Brand)
- **Vị trí:** [AisamContext.PermissionScope.cs#L50](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Infrastructure/Context/AisamContext.PermissionScope.cs#L50)
- **Raw Code:**
  ```csharp
  m.Entity<AutomationItem>().HasQueryFilter(i =>
      !PermissionScopeEnabled ||
      AutomationPlans.Any(p => p.Id == i.AutomationPlanId) &&
      i.BrandId.HasValue &&
      PermissionBrandIds.Contains(i.BrandId.Value));
  ```
- **Vì sao khớp Pattern A:**
  1. Điều kiện yêu cầu `i.BrandId.HasValue && PermissionBrandIds.Contains(i.BrandId.Value)` mà **hoàn toàn không có nhánh `PermissionOwner`**.
  2. Bất kỳ `AutomationItem` nào được tạo mà không gán cụ thể cho 1 Brand (`BrandId == null`) đều bị filter EF Core loại bỏ 100%, kể cả khi người truy vấn là Owner.
  3. Hệ lụy dây chuyền tại [PermissionScopeMiddleware.cs#L47-L49](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middlewares/PermissionScopeMiddleware.cs#L47-L49):
     ```csharp
     var planIds = await db.AutomationPlans.AsNoTracking()
         .Where(p => !p.Items.Any() || p.Items.All(i => i.BrandId.HasValue && allowedBrandIds.Contains(i.BrandId.Value)))
         .Select(p => p.Id).ToListAsync();
     ```
     Nếu 1 `AutomationPlan` chứa dù chỉ 1 item có `BrandId == null`, điều kiện `.All(...)` trả về `false`, khiến toàn bộ `AutomationPlan` đó bị loại khỏi `PermissionPlanIds` và biến mất hoàn toàn khỏi màn hình người dùng.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Cao**. Gây lỗi ẩn kế hoạch tự động hóa và các item trong kịch bản automation.

---

### Instance A3 — Entity `Asset`: Fallback về `Guid.Empty` khiến mọi Asset dùng chung cấp Workspace bị ẩn đối với non-owner
- **Vị trí:** [AisamContext.PermissionScope.cs#L63-L64](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Infrastructure/Context/AisamContext.PermissionScope.cs#L63-L64)
- **Raw Code:**
  ```csharp
  m.Entity<Asset>().HasQueryFilter(a =>
      !PermissionScopeEnabled ||
      (PermissionOwner ||
       PermissionBrandIds.Contains(a.BrandId ?? Guid.Empty) &&
       (PermissionManager || a.UploadedBy == PermissionActorId)));
  ```
- **Vì sao khớp Pattern A:**
  1. Trong hệ thống, một tài sản số (Asset: logo, template, media mẫu) có thể thuộc phạm vi toàn Workspace (`BrandId == null`).
  2. Đoạn code fallback: `a.BrandId ?? Guid.Empty`. Tuy nhiên, danh sách `PermissionBrandIds` chỉ chứa các ID Brand thực tế được gán cho user/team, **không bao giờ chứa `Guid.Empty`**.
  3. Hậu quả: Nếu `a.BrandId` là `null`, biểu thức `PermissionBrandIds.Contains(Guid.Empty)` luôn trả về `false`. Do đó, Manager và Content Creator (kể cả chính người upload asset đó) đều không thể nhìn thấy Asset dùng chung của Workspace.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Cao**. Thư viện Asset dùng chung cấp Workspace bị vô hiệu hóa đối với mọi thành viên không phải Owner.

---

### Instance A4 — Entity `Post`: AND chồng giữa `Contents` và `SocialIntegrations` khiến Post bị ẩn nếu kênh chưa được cấp quyền
- **Vị trí:** [AisamContext.PermissionScope.cs#L43](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Infrastructure/Context/AisamContext.PermissionScope.cs#L43)
- **Raw Code:**
  ```csharp
  m.Entity<Post>().HasQueryFilter(p =>
      !PermissionScopeEnabled ||
      Contents.Any(c => c.Id == p.ContentId &&
          SocialIntegrations.Any(i => i.Id == p.IntegrationId &&
              i.BrandId == c.BrandId &&
              i.WorkspaceId == c.WorkspaceId)));
  ```
- **Vì sao khớp Pattern A:**
  1. Để xem được một bài đăng (`Post`), filter bắt buộc bài đăng đó phải đồng thời thỏa mãn: thuộc tập `Contents` hợp lệ VÀ thuộc tập `SocialIntegrations` hợp lệ.
  2. `SocialIntegrations` lại bị ràng buộc bởi `PermissionChannelIds` (kênh xã hội). Nếu một Content Creator đã tạo nội dung và nội dung đó đã được duyệt/đăng lên mạng xã hội, nhưng Creator đó chưa được tick phân quyền kênh (`CanView == false` trên kênh đó), thì bài `Post` lịch sử đăng bài bị ẩn mất khỏi dashboard của Creator dù họ chính là tác giả của `Content`.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Trung bình**. Làm mất tính toàn vẹn khi tra cứu lịch sử xuất bản của Content Creator.

---

## 2. PATTERN B1 — BOLA: Nhận ID tài nguyên từ Client mà không xác minh Actor có quyền trên tài nguyên đó

### Danh sách Controller & Service đã rà soát:
Đã rà soát toàn bộ 57 Controller trong `AISAM.API/Controllers` và các Service nghiệp vụ cốt lõi:
`BrandService`, `ProductService`, `AdCampaignService`, `ContentScheduleService`, `SocialService`, `TeamService`, `AssignmentService`, `ContentService`, `AutomationPlanService`, `MediaService`.

### Kết luận: **Phát hiện thêm 3 instance mới** (`B1-01`, `B1-02`, `B1-03`)

---

### Instance B1-01 — `ContentSchedulesController` & `ContentScheduleService`: Thiếu xác thực quyền Brand/Channel trên Lịch đăng bài (Bỏ lọt ResourcePermissionFilter)
- **Vị trí:**
  - Controller: [ContentSchedulesController.cs#L68-L94](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/ContentSchedulesController.cs#L68-L94)
  - Service: [ContentScheduleService.cs#L250-L277](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-Services/Calendar/ContentScheduleService.cs#L250-L277)
- **Raw Code:**
  ```csharp
  // ContentScheduleService.cs
  public async Task<GenericResponse<bool>> DeleteInWorkspaceAsync(Guid workspaceId, Guid scheduleId, CancellationToken cancellationToken = default)
  {
      var schedule = await _contentCalendarRepository.GetByIdAsync(scheduleId, cancellationToken);
      if (schedule == null || schedule.WorkspaceId != workspaceId || schedule.IsDeleted)
          return GenericResponse<bool>.NotFound("Schedule not found");

      schedule.IsDeleted = true;
      schedule.IsActive = false;
      await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
      return GenericResponse<bool>.Success(true, "Schedule deleted successfully");
  }
  ```
- **Vì sao khớp Pattern B1:**
  1. `ResourcePermissionFilter.cs` chỉ quét các tham số route có tên: `contentId`, `postId`, `brandId`, `productId`, `integrationId`. Tuyệt đối **không có case nào cho `scheduleId`**.
  2. Tại Service: Hàm `DeleteInWorkspaceAsync` và `UpdateInWorkspaceAsync` chỉ kiểm tra `schedule.WorkspaceId != workspaceId`. Hàm thậm chí **không nhận `actorUserId`**, không kiểm tra xem actor có quyền đối với Brand hoặc Kênh phát sóng của lịch đó hay không.
  3. Hậu quả: Bất kỳ thành viên nào (kể cả Viewer hay ContentCreator của Team Brand A) nếu có `scheduleId` đều có thể gửi request xóa hoặc thay đổi lịch đăng bài của Brand B trong cùng Workspace.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Nghiêm trọng (Critical)**. Cho phép thao tác trái quyền lên lịch xuất bản bài đăng giữa các team.

---

### Instance B1-02 — `ProductService`: Tham số `userId` bị bỏ xó trong hàm kiểm tra quyền Brand
- **Vị trí:** [ProductService.cs#L345-L364](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-Services/Product/ProductService.cs#L345-L364)
- **Raw Code:**
  ```csharp
  private static bool IsBrandVisibleInWorkspace(Brand brand, Guid workspaceId, Guid userId)
  {
      return brand.WorkspaceId == workspaceId;
  }
  ```
- **Vì sao khớp Pattern B1:**
  1. Trong các hàm `CreateAsync`, `UpdateAsync`, `SoftDeleteAsync`, `RestoreAsync` của `ProductService`, phương thức `IsBrandVisibleInWorkspace` được gọi để validate quyền. Tham số `userId` được truyền vào nhưng hoàn toàn không được sử dụng.
  2. Hệ thống chỉ kiểm tra `brand.WorkspaceId == workspaceId`.
  3. Tại Controller [ProductController.cs#L106](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/ProductController.cs#L106): Tham số route là `[HttpPut("{id}")]` (tên là `id` chứ không phải `productId`), khiến `ResourcePermissionFilter` không thể tự động nhận diện để chặn từ xa.
  4. Hậu quả: Member thuộc Team không quản lý Brand vẫn có thể thêm/sửa/xóa sản phẩm thuộc Brand đó nếu biết `brandId`.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Cao (High)**. Vi phạm phân quyền tài nguyên Brand-Product.

---

### Instance B1-03 — `AdCampaignService`: Chỉ kiểm tra thành viên Workspace (`EnsureWorkspaceMemberAsync`), bỏ qua phân quyền Team-Brand
- **Vị trí:** [AdCampaignService.cs#L117-L157](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-Services/Ads/AdCampaignService.cs#L117-L157), [L215-L245](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-Services/Ads/AdCampaignService.cs#L215-L245), [L393-L410](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-Services/Ads/AdCampaignService.cs#L393-L410)
- **Raw Code:**
  ```csharp
  // CreateCampaignAsync
  await EnsureWorkspaceMemberAsync(workspaceId, actorUserId, cancellationToken);
  if (request.BrandId.HasValue && request.BrandId.Value != Guid.Empty)
  {
      var brand = await _brandRepository.GetByIdAsync(request.BrandId.Value, cancellationToken);
      if (brand == null || brand.WorkspaceId != workspaceId || brand.IsDeleted)
          return GenericResponse<CampaignResponse>.NotFound("Brand not found in workspace");
  }
  ```
- **Vì sao khớp Pattern B1:**
  1. `EnsureWorkspaceMemberAsync` chỉ kiểm tra user có tồn tại trong bảng `WorkspaceMember` của Workspace hay không.
  2. Khi nhận `request.BrandId`, service chỉ kiểm tra `brand.WorkspaceId != workspaceId`. Không hề có bước kiểm tra: nếu user không phải Owner thì user có thuộc Team được giao quản lý `BrandId` này hay không.
  3. Hậu quả: Bất kỳ user nào trong Workspace cũng có thể tạo Campaign, điều chỉnh ngân sách quảng cáo của một Brand mà mình không được phân quyền quản lý.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Cao (High)**. Rủi ro tác động tài chính và chiến dịch quảng cáo chéo giữa các Brand.

---

## 3. PATTERN B2 — Check quyền Hardcode theo Role Owner thay vì theo Resource cụ thể

### Danh sách vị trí đã rà soát:
- Backend: Tìm kiếm toàn bộ các điểm tham chiếu `PermissionOwner` trong `AISAM.Infrastructure` và `AISAM.Services`.
- Frontend: Tìm kiếm toàn bộ `isOwner` trong `AISAM-FE/src`.
  - Phân loại hợp lý: Quản lý nạp tiền, nâng cấp gói, cấu hình thanh toán (`billing`, `payments`, `subscription`, `workspace-settings`) — Đúng bản chất chỉ Owner mới được làm.
  - Phân loại bất hợp lý (khớp Pattern B2): Các màn hình quản lý Team, phân công thành viên.

### Kết luận: **Phát hiện thêm 2 instance mới** (`B2-01`, `B2-02`)

---

### Instance B2-01 — Frontend Team Page: Hardcode `isOwner` làm tê liệt UI của Manager
- **Vị trí:** [AISAM-FE/src/app/(dashboard)/team/page.tsx#L69](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/app/(dashboard)/team/page.tsx#L69), [L353](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/app/(dashboard)/team/page.tsx#L353), [L577](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/app/(dashboard)/team/page.tsx#L577), [L700](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/app/(dashboard)/team/page.tsx#L700)
- **Raw Code:**
  ```tsx
  // L69
  const isOwner = activeWorkspace?.isOwner === true;

  // L353: Ẩn nút tạo team đối với Manager
  {isOwner && (
    <button onClick={() => setShowCreateModal(true)} ...>+ Create Team</button>
  )}

  // L577: Ẩn toàn bộ cột hành động chỉnh sửa/xóa team
  {isOwner && <th className="...">Actions</th>}
  ```
- **Vì sao khớp Pattern B2:**
  1. Tại backend [TeamService.cs#L44-L55](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L44-L55), hệ thống cho phép cả `Owner` lẫn `Manager` được tạo Team và quản lý các Team mà mình là Manager.
  2. Tuy nhiên trên giao diện `team/page.tsx`, frontend kiểm tra trực tiếp `activeWorkspace?.isOwner === true`.
  3. Hậu quả: Một user có role Manager trong Workspace bị ẩn hoàn toàn nút `Create Team`, không thể quản lý các thành viên trong Team mà mình phụ trách từ bảng điều khiển chính.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Cao (High)**. Gây tắc nghẽn trải nghiệm người dùng, Manager không thể vận hành dù backend có hỗ trợ.

---

### Instance B2-02 — Component `TeamDetailPanel`: Chặn thêm/xóa thành viên dựa trên `isOwner` thay vì kiểm tra quyền Manager của Team
- **Vị trí:** [AISAM-FE/src/components/team/TeamDetailPanel.tsx#L218](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/TeamDetailPanel.tsx#L218), [L237](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/TeamDetailPanel.tsx#L237), [L263](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/TeamDetailPanel.tsx#L263), [L305](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/TeamDetailPanel.tsx#L305)
- **Raw Code:**
  ```tsx
  // L218
  {isOwner && (
    <button onClick={() => setEditing(true)} ...>Edit</button>
  )}

  // L237
  {isOwner && (
    <button onClick={() => setShowAddMember(true)} ...>+ Add Member</button>
  )}

  // L305
  {isOwner && (
    <button onClick={() => handleRemoveMember(m.userId)} ...>Remove</button>
  )}
  ```
- **Vì sao khớp Pattern B2:**
  1. Component nhận prop `isOwner: boolean` từ trang cha.
  2. Các thao tác thêm thành viên (`Add Member`), xóa thành viên (`Remove`), và chỉnh sửa tên Team (`Edit`) đều bị khóa cứng bởi điều kiện `isOwner`.
  3. Lẽ ra quyền này phải cho phép nếu: `isOwner || isTeamManager` (người dùng hiện tại là Manager của chính Team đang mở xem chi tiết).
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Cao (High)**.

---

## 4. PATTERN C — Bước Validate trong luồng nhiều bước trả `true` vô điều kiện (Bỏ sót Business Rule)

### Danh sách Wizard / Modal Form đã rà soát:
1. [CreateTeamWizard.tsx](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/CreateTeamWizard.tsx): Nơi đã phát hiện Bug C cũ.
2. [CreateTeamModal.tsx](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/CreateTeamModal.tsx): Xác nhận là file rác (dead code), không được import ở bất kỳ đâu trong dự án.
3. [CreateCampaignModal.tsx](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/campaign/CreateCampaignModal.tsx): Kiểm tra các bước validate form chiến dịch.
4. [CreateContentModal.tsx](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/content/CreateContentModal.tsx): Kiểm tra validate form tạo nội dung.
5. [BulkScheduleModal.tsx](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/calendar/BulkScheduleModal.tsx): Kiểm tra validate lịch hàng loạt.

### Bằng chứng rà soát chi tiết:
- Tại [CreateCampaignModal.tsx](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/campaign/CreateCampaignModal.tsx): Kiểm tra bắt buộc `name.trim().length > 0` và `brandId` hợp lệ trước khi submit.
- Tại [CreateContentModal.tsx](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/content/CreateContentModal.tsx): Kiểm tra chặt chẽ `title`, `brandId`, nội dung text trước khi cho phép gọi API.
- Không có bước wizard nào khác sử dụng pattern `case X: return true; // optional` mà thiếu validate.

### Kết luận: **Không phát hiện thêm instance nào của Pattern C** ngoài Bug C cũ tại `CreateTeamWizard.tsx`.

---

## 5. PATTERN D — Lỗi im lặng nuốt Exception che giấu API call sai (URL "ma")

### Danh sách các điểm bắt catch đã đối chiếu với Route Backend:
Đã grep toàn bộ các lệnh gọi API tự gõ tay kèm `.catch(...)` hoặc `catch { }` trong frontend và so khớp với danh sách route của 57 controller backend:

| File Frontend | Đường dẫn URL tự gõ | Route Backend đối ứng | Kết quả kiểm tra |
| :--- | :--- | :--- | :--- |
| `teamService.ts#L79` | `/workspace-members` | `[HttpGet("workspace-members")]` tại `WorkspacesController.cs#L85` | **Hợp lệ (Route thật)** |
| `content/page.tsx#L97` | `/dashboard/summary` | `[HttpGet("summary")]` tại `DashboardController.cs#L36` | **Hợp lệ (Route thật)** |
| `admin/payments/page.tsx#L53` | `/admin/dashboard/charts` | `[HttpGet("charts")]` tại `AdminDashboardController.cs#L45` | **Hợp lệ (Route thật)** |
| `BrandTeamAccess.tsx#L85` | `/brands/${brandId}/teams` | `[HttpGet("{brandId}/teams")]` tại `BrandAssignmentsController.cs#L32` | **Hợp lệ (Route thật)** |
| `tiktok/route.ts#L48` | `/social/oauth/tiktok/callback`| `[HttpPost("oauth/tiktok/callback")]` tại `SocialOAuthController.cs#L98` | **Hợp lệ (Route thật)** |

### Kết luận: **Không phát hiện thêm instance nào của Pattern D**.
Chỉ duy nhất Bug D cũ tại [CreateTeamWizard.tsx#L125](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/components/team/CreateTeamWizard.tsx#L125) gọi sai endpoint `/social/integrations?pageSize=200` (thực tế backend là `/integrations/channel-permissions` hoặc `/social/accounts`). Tất cả các vị trí catch rỗng khác đều gọi đúng route backend đang hoạt động.

---

## 6. PATTERN E — Mảng render Tab/Status ở FE bị lệch (thiếu) so với Enum Backend

### Danh sách các Enum Backend đã đối chiếu:
1. `ContentStatusEnum` ([ContentStatusEnum.cs](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Domain/Enums/ContentStatusEnum.cs)):
   - `0: Draft`
   - `1: PendingApproval`
   - `2: Approved`
   - `3: Rejected`
   - `4: Published`
   - `5: Flagged`
   - `6: RejectedByPlatform`
   - `7: Failed`
2. `CampaignStatusEnum` ([CampaignStatus.cs](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Domain/Enums/CampaignStatus.cs)): `Draft (0)`, `Active (1)`, `Paused (2)`, `Completed (3)`, `Archived (4)`.
3. `ScheduleStatusEnum` ([ScheduleStatusEnum.cs](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Domain/Enums/ScheduleStatusEnum.cs)): `Draft (0)`, `Scheduled (1)`, `Publishing (2)`, `Published (3)`, `Failed (4)`, `Cancelled (5)`.

### Kết luận: **Phát hiện thêm 2 instance mới** (`E2`, `E3`)

---

### Instance E2 — `contentConstants.tsx` & Content Page: Mảng lọc trạng thái thiếu `Flagged` và `RejectedByPlatform`
- **Vị trí:** [AISAM-FE/src/lib/contentConstants.tsx#L4](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/lib/contentConstants.tsx#L4), [L42-L50](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/lib/contentConstants.tsx#L42-L50)
- **Raw Code:**
  ```typescript
  export type ContentStatus =
    | "Draft"
    | "Awaiting Approval"
    | "Approved"
    | "Rejected"
    | "Published"
    | "Scheduled"
    | "Failed";

  export const STATUS_OPTIONS: { label: string; value: ContentStatus }[] = [
    { label: "Draft", value: "Draft" },
    { label: "Awaiting Approval", value: "Awaiting Approval" },
    { label: "Approved", value: "Approved" },
    { label: "Rejected", value: "Rejected" },
    { label: "Published", value: "Published" },
    { label: "Scheduled", value: "Scheduled" },
    { label: "Failed", value: "Failed" },
  ];
  ```
- **Vì sao khớp Pattern E:**
  1. Backend `ContentStatusEnum` hỗ trợ trạng thái kiểm duyệt nội dung: `Flagged (5)` và trạng thái lỗi từ nền tảng: `RejectedByPlatform (6)`.
  2. Mảng hằng số `STATUS_OPTIONS` và định nghĩa type `ContentStatus` ở frontend thiếu hẳn 2 giá trị này.
  3. Hậu quả: Khi một bài viết bị mạng xã hội từ chối đăng (ví dụ TikTok vi phạm chính sách) hoặc bị đánh cờ kiểm duyệt, người dùng vào màn hình Content Management sẽ không có cách nào lọc tìm các bài này để xử lý.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Trung bình (Medium)**.

---

### Instance E3 — Admin Content Moderation Page: Map thiếu status và filter dropdown thiếu giá trị `Failed`
- **Vị trí:** [AISAM-FE/src/app/(admin)/admin/content/page.tsx#L9](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/app/(admin)/admin/content/page.tsx#L9), [L120-L128](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-FE/src/app/(admin)/admin/content/page.tsx#L120-L128)
- **Raw Code:**
  ```typescript
  // L9
  const contentStatusLabels: Record<number, string> = {
    0: "Draft",
    1: "Pending",
    2: "Approved",
    3: "Rejected",
    4: "Published",
    5: "Flagged",
  };

  // L120-L128 (Filter dropdown)
  <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
    <option value="">All Statuses</option>
    <option value="0">Draft</option>
    <option value="1">Pending</option>
    <option value="2">Approved</option>
    <option value="3">Rejected</option>
    <option value="4">Published</option>
    <option value="5">Flagged</option>
    {/* Thiếu hoàn toàn option 6 (RejectedByPlatform) và 7 (Failed) */}
  </select>
  ```
- **Vì sao khớp Pattern E:**
  1. `contentStatusLabels` chỉ định nghĩa đến key 5. Nếu backend trả về status 6 (`RejectedByPlatform`) hoặc 7 (`Failed`), trên bảng hiển thị sẽ bị fallback về chuỗi `"Unknown"`.
  2. Dropdown lọc của Admin không có option `6` và `7`, khiến Admin không thể lọc danh sách các nội dung bị lỗi đăng bài hoặc bị nền tảng reject để hỗ trợ người dùng.
- **Mức độ tin cậy:** 100%.
- **Mức độ ảnh hưởng:** **Trung bình (Medium)**.

---

## 7. ĐỀ XUẤT CẬP NHẬT CHANGE PLAN (`AISAM_ChangePlan.md`)

Để đảm bảo giải quyết triệt để các phát hiện mới mà không làm xáo trộn cấu trúc triển khai, đề xuất tích hợp các phát hiện vào các Phase hiện có như sau:

### Tích hợp vào Phase 1 (Data Layer & Query Filters):
- **Bổ sung `A2`:** Sửa filter `AutomationItem` trong `AisamContext.PermissionScope.cs` thêm bypass `PermissionOwner` và cho phép `BrandId == null`. Đồng thời cập nhật `PermissionScopeMiddleware.cs` để không làm ẩn `AutomationPlan`.
- **Bổ sung `A3`:** Sửa filter `Asset` trong `AisamContext.PermissionScope.cs` để khi `a.BrandId == null` thì cho phép toàn bộ thành viên trong Workspace truy cập.
- **Bổ sung `A4`:** Tinh chỉnh filter `Post` để tác giả tạo Content (`c.CreatedBy == PermissionActorId`) luôn được xem post của mình bất kể quyền kênh.

### Tích hợp vào Phase 2 (Authorization & BOLA):
- **Bổ sung `B1-01`:** Thêm `scheduleId` vào `ResourcePermissionFilter.cs` và truyền `actorUserId` vào `ContentScheduleService.DeleteInWorkspaceAsync` / `UpdateInWorkspaceAsync` để kiểm tra quyền Brand của schedule.
- **Bổ sung `B1-02`:** Cập nhật `ProductService.IsBrandVisibleInWorkspace` để kiểm tra `PermissionBrandIds` của actor khi actor không phải là Owner.
- **Bổ sung `B1-03`:** Bổ sung check quyền Brand trong `AdCampaignService` trước khi tạo hoặc chỉnh sửa Campaign.

### Tích hợp vào Phase 4 (Frontend UI/UX & Flow):
- **Bổ sung `B2-01` & `B2-02`:** Cập nhật `team/page.tsx` và `TeamDetailPanel.tsx` thay vì chỉ check `isOwner`, cho phép cả `isTeamManager` được thao tác trên Team tương ứng.
- **Bổ sung `E2` & `E3`:** Bổ sung các giá trị enum còn thiếu (`RejectedByPlatform`, `Failed`, `Flagged`) vào `contentConstants.tsx` và trang Admin Content Moderation.
