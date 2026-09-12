# BÁO CÁO ĐÁNH GIÁ TÍNH KHẢ THI (TECHNICAL FEASIBILITY AUDIT REPORT)
## Chuyển đổi kiến trúc từ Flat RBAC sang Decoupled Team-Based Scoped RBAC (2 tầng)
**Dự án:** AISAM (.NET 8 + EF Core 9 + PostgreSQL)  
**Tài liệu đối chiếu:** TÀI LIỆU THIẾT KẾ KIẾN TRÚC - Decoupled Team-Based Scoped RBAC (`TÀI LIỆU THIẾT KẾ KIẾN TRÚC.docx`)  
**Vai trò thẩm định:** Technical Auditor  
**Trạng thái phiên làm việc:** Phase Read-Only (Không sửa code, không tạo migration, không can thiệp schema)  
**Ngày lập báo cáo:** 12/09/2026  

---

## 1. EXECUTIVE SUMMARY (TỔNG QUAN ĐÁNH GIÁ)

- **Kết luận tính khả thi tổng thể:** **CÓ KHẢ THI VỚI ĐIỀU KIỆN (FEASIBLE WITH CONDITIONS)**.
- **Lý do ngắn gọn:**
  1. **Nền tảng Schema đã hoàn thành 70%:** Các bảng cốt lõi của mô hình tách rời gồm `teams`, `team_members`, `team_brands`, `team_channel_access` và cột `contents.team_id` **đã tồn tại thực tế** trong database và EF Core model (áp dụng từ migration `20260908003735_CompletePermissionSchema` và `20260911120000_BackfillTeamChannelAccessCanView`). Kiến trúc decoupled trung gian qua `TeamBrand` đã có sẵn, không phải dựng mới từ con số 0.
  2. **Rào cản lớn nhất nằm ở Authorization Pipeline:** Hiện tại logic phân quyền đang bị phân mảnh và mâu thuẫn giữa 3 tầng:
     - `ActiveWorkspaceMiddleware.cs` đang kiểm tra quyền phẳng dựa trên `WorkspaceMember.Role` đơn lẻ ở đầu pipeline (chặn `403` ngay trước khi request kịp đến Controller/Filter).
     - `PermissionScopeMiddleware.cs` và `AisamContext.PermissionScope.cs` (EF Core Query Filters) sử dụng các cờ tĩnh cấp request (`PermissionManager`, `PermissionCreator`) áp dụng trên toàn DbContext thay vì tính theo từng cặp (Team, Brand).
     - `AccessControlService.cs` vẫn truyền `member.Role` phẳng vào `ResourcePermissionFacts` thay vì tính quyền cộng dồn cao nhất (Max Privilege Rule) của User trên Brand cụ thể.
  3. **Vấn đề dữ liệu cũ (Legacy Content Attribution):** 542 / 544 bản ghi `contents` hiện tại chưa có thông tin `primary_creator_id` rõ ràng và nhiều bài viết chưa có `team_id`. Trigger bảo vệ PostgreSQL `aisam_permission_integrity()` đang kích hoạt chặn cập nhật `team_id`/`brand_id` nếu không có quy trình migration rõ ràng.

---

## 2. CURRENT STATE INVENTORY (BẰNG CHỨNG THÔ TỪ MÃ NGUỒN)

### A. Schema & Entity hiện tại

#### A.1. Thực thể `WorkspaceMember` và Enum `Role`
- **File định nghĩa Entity:** [`WorkspaceMember.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/WorkspaceMember.cs#L8-L50)
- **Raw Evidence:**
  ```csharp
  [Table("workspace_members")]
  public class WorkspaceMember
  {
      [Key]
      [Column("id")]
      public Guid Id { get; set; } = Guid.NewGuid();

      [Required]
      [Column("workspace_id")]
      public Guid WorkspaceId { get; set; }

      [Required]
      [Column("user_id")]
      public Guid UserId { get; set; }

      [Required]
      [Column("role")]
      public WorkspaceMemberRoleEnum Role { get; set; }
      ...
  ```
- **File định nghĩa Enum:** [`WorkspaceMemberRoleEnum.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Enumeration/WorkspaceMemberRoleEnum.cs#L3-L9)
- **Raw Evidence:**
  ```csharp
  namespace AISAM.Data.Enumeration
  {
      public enum WorkspaceMemberRoleEnum
      {
          Owner = 1,
          Manager = 2,
          ContentCreator = 3,
          Viewer = 4
      }
  }
  ```
- **Nhận định Auditor:** Hiện tại `WorkspaceMemberRoleEnum` lưu 4 role phẳng (`Owner`, `Manager`, `ContentCreator`, `Viewer`) trực tiếp trên quan hệ thành viên Workspace. Để đạt mục tiêu kiến trúc mới, tầng Workspace Role chỉ được giữ 3 vai trò: `OWNER`, `WORKSPACE_MANAGER`, `MEMBER`. Các vai trò `Manager`, `ContentCreator`, `Viewer` phải chuyển dịch hoàn toàn xuống cấp Team Role.

---

#### A.2. Thực thể `Team`, `TeamMember`, `TeamBrand` và `TeamChannelAccess`
- **File định nghĩa `Team`:** [`Team.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/Team.cs#L7-L48)
  - Đã có bảng `teams` với các cột: `id`, `profile_id`, `workspace_id`, `name`, `description`, `is_deleted`, `status`, `created_at`, `updated_at`.
- **File định nghĩa `TeamMember`:** [`TeamMember.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/TeamMember.cs#L7-L41)
  - **Raw Evidence:**
    ```csharp
    [Table("team_members")]
    public class TeamMember
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("team_id")]
        public Guid TeamId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("role")]
        public string Role { get; set; } = string.Empty;

        [Column("permissions", TypeName = "jsonb")]
        public List<string> Permissions { get; set; } = new(); // JSON permissions
    ```
  - **Nhận định Auditor:** Bảng `team_members` đã tồn tại, tuy nhiên cột `role` đang là `string Role` dạng text tự do (chưa chuẩn hóa thành Enum `TeamRoleEnum` gồm `MANAGER`, `CONTENT_CREATOR`, `VIEWER`), đi kèm cột `permissions` dạng `jsonb` lưu các quyền ủy quyền riêng lẻ (`view_all_creators`, `review`, `publish`).
- **File định nghĩa `TeamBrand`:** [`TeamBrand.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/TeamBrand.cs#L7-L40)
  - **Raw Evidence:**
    ```csharp
    [Table("team_brands")]
    public class TeamBrand
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("team_id")]
        public Guid TeamId { get; set; }

        [Required]
        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("channel_access_mode")]
        public int ChannelAccessMode { get; set; }

        public virtual ICollection<TeamChannelAccess> Channels { get; set; } = new List<TeamChannelAccess>();
    ```
  - **Nhận định Auditor:** Mô hình quan hệ `TeamBrand` tách rời hoàn toàn độc lập Team và Brand đã được thiết lập sẵn trong schema hiện tại, đi kèm quan hệ 1-N với `TeamChannelAccess`.
- **File định nghĩa `TeamChannelAccess`:** [`TeamChannelAccess.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/TeamChannelAccess.cs#L6-L20)
  - **Raw Evidence:**
    ```csharp
    [Table("team_channel_access")]
    public class TeamChannelAccess
    {
        [Key, Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Column("team_brand_id")]
        public Guid TeamBrandId { get; set; }
        [Column("integration_id")]
        public Guid IntegrationId { get; set; }
        [Column("can_view")] public bool CanView { get; set; }
        [Column("can_publish")] public bool CanPublish { get; set; }
        [Column("can_manage")] public bool CanManage { get; set; }
        public virtual TeamBrand TeamBrand { get; set; } = null!;
        public virtual SocialIntegration Integration { get; set; } = null!;
    }
    ```
  - **Đánh giá tính tương thích của `TEAM_CHANNEL_ACCESS`:** Bảng này **hoàn toàn tương thích** với kiến trúc mới. Nó liên kết trực tiếp qua `team_brand_id` đến `TeamBrand` và gán quyền granular (`can_view`, `can_publish`, `can_manage`) trên từng kênh (`SocialIntegration`). Đã có migration backfill idempotency [`20260911120000_BackfillTeamChannelAccessCanView.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/Migrations/20260911120000_BackfillTeamChannelAccessCanView.cs#L12-L25). Không cần xóa hay tạo lại, chỉ cần giữ nguyên cấu trúc.

---

#### A.3. Cột `TeamId` trong thực thể `Content`
- **File định nghĩa Entity:** [`Content.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/Content.cs#L33-L34)
- **Raw Evidence:**
  ```csharp
  [Column("team_id")]
  public Guid? TeamId { get; set; }
  ```
- **Inventory dữ liệu thực tế từ Preflight Checklist:** ([`docs/T01_DATABASE_SCHEMA.md#L117-L149`](file:///c:/tai%20lieu/AISAM-FINAL/docs/T01_DATABASE_SCHEMA.md#L117-L149))
  - Tổng số bản ghi `contents`: **544**
  - Số bản ghi `contents_without_primary_creator`: **542**
  - Số bản ghi `teams`: **66**
  - Số bản ghi `team_brands`: **46**
  - Số bản ghi `team_members`: **72**
- **Cơ chế tự gán `TeamId` hiện có:** [`AisamContext.PermissionIntegrity.cs#L127-L186`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionIntegrity.cs#L127-L186)
  - Khi lưu `Content`, hàm `EnsureContentTeamAsync` tự động tìm Team của creator gán cho Brand, hoặc fallback về Team bất kỳ của Workspace, hoặc tự sinh "Default Team".
- **Chính sách nội dung mơ hồ cũ (Legacy-ambiguous Content Policy):** Trích quyết định đã chốt tại [`docs/PERMISSION_PUBLISHING_DECISIONS.md#L26`](file:///c:/tai%20lieu/AISAM-FINAL/docs/PERMISSION_PUBLISHING_DECISIONS.md#L26):
  > *"Attribution cũ không xác định được giữ null và báo cáo. Không dùng Profile.UserId để cấp quyền Creator giả."*
  Và bảng ghi nhận lỗi migration [`20260908003735_CompletePermissionSchema.cs#L21-L24`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/Migrations/20260908003735_CompletePermissionSchema.cs#L21-L24):
  ```sql
  CREATE TABLE IF NOT EXISTS permission_migration_issues(resource_table text NOT NULL,resource_id uuid NOT NULL,issue text NOT NULL,PRIMARY KEY(resource_table,resource_id,issue));
  INSERT INTO permission_migration_issues SELECT 'contents',id,'creator_unknown' FROM contents WHERE primary_creator_id IS NULL ON CONFLICT DO NOTHING;
  ```
- **Nhận định Auditor:** Cột `team_id` đã có sẵn trong bảng `contents`, KHÔNG cần chạy DDL để tạo cột. Tuy nhiên, 542 bài viết cũ không có creator rõ ràng. Nếu áp dụng luật Team Data Isolation nghiêm ngặt ("chỉ thấy bài do Team mình tạo"), toàn bộ nội dung legacy này sẽ bị ẩn đối với tất cả Member/Creator/Team Manager, và chỉ có `Owner` / `WorkspaceManager` thấy được trừ khi có kịch bản backfill gán vào Team mặc định.

---

#### A.4. Cột `AccessExpiresAt` (Team Transfer Phase)
- **Kết quả rà soát mã nguồn:**
  - Tìm kiếm toàn bộ repo AISAM-BE: **0 kết quả** cho `AccessExpiresAt`.
  - Cột này **chưa hề tồn tại** trong bất kỳ entity (`TeamBrand`, `TeamMember`, `WorkspaceMember`) hay migration nào của EF Core.
  - Tuy nhiên, trong trigger PostgreSQL [`20260908003735_CompletePermissionSchema.cs#L34-L35`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/Migrations/20260908003735_CompletePermissionSchema.cs#L34-L35), đã có ràng buộc chặn chuyển nhượng Team:
    ```sql
    IF TG_TABLE_NAME='team_brands' AND n->'team_id' IS DISTINCT FROM o->'team_id' THEN
      RAISE EXCEPTION 'Team transfer requires explicit migration' USING ERRCODE='23514'; END IF;
    ```
- **Nhận định Auditor:** `AccessExpiresAt` là khái niệm nằm trong tài liệu thảo luận/thiết kế cho phase tương lai, chưa đụng độ vật lý với bảng `team_brands` hiện tại. Nếu sau này thêm cột này vào `team_brands` hoặc `team_members`, logic truy vấn quyền chỉ cần bổ sung điều kiện `&& (tb.AccessExpiresAt == null || tb.AccessExpiresAt > DateTime.UtcNow)`.

---

### B. Authorization Logic hiện tại

#### B.1. Toàn bộ các vị trí kiểm tra Role trong Backend
Đã kiểm kê chi tiết tất cả các điểm check role trong hệ thống:

1. **Tầng Middleware cấp cao (Pipeline Gateway):**
   - [`ActiveWorkspaceMiddleware.cs#L386-L407`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L386-L407):
     ```csharp
     private static (HttpStatusCode Status, string Message, string? ErrorCode)? EnsurePermission(
         WorkspaceMemberRoleEnum role,
         WorkspacePermissionEnum permission)
     {
         var allowed = permission switch
         {
             WorkspacePermissionEnum.ManageBilling => role == WorkspaceMemberRoleEnum.Owner,
             WorkspacePermissionEnum.ManageBrands => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
             WorkspacePermissionEnum.ManageProducts => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
             WorkspacePermissionEnum.ManageContent => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager or WorkspaceMemberRoleEnum.ContentCreator,
             WorkspacePermissionEnum.PublishContent => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
             WorkspacePermissionEnum.ReviewContent => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
             WorkspacePermissionEnum.GenerateAiContent => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager or WorkspaceMemberRoleEnum.ContentCreator,
             WorkspacePermissionEnum.ManageSchedules => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
             WorkspacePermissionEnum.ManageCampaigns => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
             _ => false
         };
         ...
     ```
   - [`ActiveWorkspaceMiddleware.cs#L154-L162`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L154-L162): Kiểm tra route `/api/payment` write endpoints yêu cầu `ManageBilling`.
   - [`ActiveWorkspaceMiddleware.cs#L267-L274`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L267-L274): Kiểm tra route `/publish/`, `/approve`, `/reject` yêu cầu `PublishContent` hoặc `ReviewContent`.

2. **Tầng Permission Scope & Context Flagging:**
   - [`PermissionScopeMiddleware.cs#L21-L34`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/PermissionScopeMiddleware.cs#L21-L34):
     ```csharp
     db.PermissionOwner=membership.Role==WorkspaceMemberRoleEnum.Owner;
     db.PermissionManager=membership.Role==WorkspaceMemberRoleEnum.Manager;
     db.PermissionCreator=membership.Role==WorkspaceMemberRoleEnum.ContentCreator;
     ...
     db.PermissionManager=membership.Role==WorkspaceMemberRoleEnum.Manager || rows.Any(r=>r.Role=="Manager");
     ```

3. **Tầng Global EF Core Query Filters (Data Isolation):**
   - [`AisamContext.PermissionScope.cs#L26-L73`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L26-L73):
     - L32-33: `WorkspaceMember` filter:
       ```csharp
       m.Entity<WorkspaceMember>().HasQueryFilter(m=>!PermissionScopeEnabled || m.WorkspaceId==PermissionWorkspaceId &&
           (PermissionOwner || m.UserId==PermissionActorId || PermissionManager && TeamMembers.Any(t=>t.UserId==m.UserId && t.IsActive)));
       ```
     - L35-39: `Content` filter:
       ```csharp
       m.Entity<Content>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionBrandIds.Contains(c.BrandId)) &&
           (!PermissionOnlyMyContent || c.PrimaryCreatorId==PermissionActorId) &&
           (!PermissionReviewQueue || PermissionOwner || PermissionManager || PermissionCreator && PermissionReviewBrandIds.Contains(c.BrandId)) &&
           (PermissionOwner || PermissionManager || PermissionCreator && (c.PrimaryCreatorId==PermissionActorId || PermissionViewAllBrandIds.Contains(c.BrandId)) || c.Id==PermissionReviewContentId ||
            PermissionReviewQueue && PermissionCreator && PermissionReviewBrandIds.Contains(c.BrandId) && c.Status==AISAM.Data.Enumeration.ContentStatusEnum.PendingApproval));
       ```
       *(Chú ý: Hoàn toàn KHÔNG lọc theo `c.TeamId`!)*

4. **Tầng Resource Permission Policy & Access Control Service:**
   - [`ResourcePermissionPolicy.cs#L32-L63`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L32-L63):
     ```csharp
     var owner = facts.Role == WorkspaceMemberRoleEnum.Owner;
     var manager = facts.Role == WorkspaceMemberRoleEnum.Manager;
     var creator = facts.Role == WorkspaceMemberRoleEnum.ContentCreator;
     ...
     ResourcePermission.ApprovalReview => manager || creator && facts.CanReview,
     ```
   - [`AccessControlService.cs#L35, L61, L108, L135`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/AccessControlService.cs#L35):
     ```csharp
     var facts = new ResourcePermissionFacts(member.Role, workspace.Status, true, true, owner || assignments.Count > 0, ...);
     ```

5. **Tầng Service nghiệp vụ trực tiếp:**
   - [`WorkspaceMemberService.cs#L61, L73, L107, L156, L188, L215`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceMemberService.cs#L61): Ràng buộc chỉ Owner mới được đổi role, xóa member, chuyển quyền.
   - [`ContentService.cs#L180, L207, L243, L257`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/ContentService.cs#L207):
     ```csharp
     if (role.Value == WorkspaceMemberRoleEnum.ContentCreator && content.PrimaryCreatorId != ... ) // BOLA check
     ```
   - [`BrandService.cs#L84, L151, L330`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/BrandService.cs#L84): Check `membership.Role == WorkspaceMemberRoleEnum.Manager`.
   - [`TeamService.cs#L47, L48, L150, L154, L359`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L47): Check actor có phải Owner hoặc Manager không.
   - [`MemberPerformanceService.cs#L55-L57`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/MemberPerformanceService.cs#L55-L57):
     ```csharp
     var owner=membership.Role==WorkspaceMemberRoleEnum.Owner;
     var creator=membership.Role==WorkspaceMemberRoleEnum.ContentCreator;
     if(!owner && !creator && membership.Role!=WorkspaceMemberRoleEnum.Manager) throw new PerformanceAccessException(403);
     ```

---

#### B.2. Đánh giá chuyển đổi sang "Max Privilege Rule" (Cộng dồn quyền qua nhiều Team)
- **Bản chất Max Privilege Rule trong tài liệu thiết kế:**
  Nếu User U thuộc Team 1 (Role = `VIEWER`) nối với Brand B, và đồng thời thuộc Team 2 (Role = `MANAGER`) nối với cùng Brand B: trên Brand B, quyền thực tế của U là `MANAGER` (`Manager > ContentCreator > Viewer`).
- **Khuyết tật của mã nguồn hiện tại:**
  1. `WorkspaceMember.Role` là thuộc tính đơn lẻ toàn cục.
  2. `PermissionScopeMiddleware.cs` L31 đặt cờ `db.PermissionManager = ... || rows.Any(r => r.Role == "Manager")`. Nếu User làm Manager ở Team X quản lý Brand X, cờ `db.PermissionManager` trở thành `true` cho toàn bộ request, khiến User đó được đối xử như Manager trên cả Brand Y (nơi User chỉ là Viewer hoặc không thuộc Team nào)! Đây chính là lỗ hổng **Privilege Escalation** xuyên Brand.
  3. `AccessControlService.cs` L108 truyền trực tiếp `member.Role` (vai trò Workspace) vào `ResourcePermissionFacts`, không tính `effectiveRole` cho Brand được yêu cầu.
- **Số điểm chạm bắt buộc sửa:** **Tối thiểu 38 điểm chạm** (xem mục 6).

---

#### B.3. Tác động tới các Fix BOLA trước đó
Kiểm tra đối chiếu các fix BOLA (được ghi nhận trong [`AISAM_ChangePlan.md`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM_ChangePlan.md#L92-L170) và [`AISAM_PatternSweep_Report.md`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM_PatternSweep_Report.md#L93-L160)):

| Fix BOLA / An ninh | Mã gốc | Cơ chế fix hiện tại | Nguy cơ bị phá vỡ bởi kiến trúc mới | Mức độ ảnh hưởng | Giải pháp bảo toàn |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **Social Targets Takeover** | Bug B gốc | [`SocialService.cs#L368-L376`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/SocialService.cs#L368-L376): Actor phải có `ResourcePermission.BrandManage` trên Brand qua `_accessControl.CheckAsync`. | **Có nguy cơ.** Nếu `AccessControlService` không resolve đúng `TeamRole.MANAGER` của Team nối với Brand đó, request của Team Manager hợp lệ sẽ bị từ chối hoặc Viewer bị lọt. | **Cao (High)** | `AccessControlService` phải tính Max Privilege trên BrandId từ bảng trung gian `team_brands`. |
| **Schedule BOLA Delete/Update** | B1-01 | [`ContentScheduleService.cs#L260-L310`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/ContentScheduleService.cs#L260-L310): Check actor là Owner hoặc thuộc `TeamMembers` của Team nối với Brand của Schedule. | **Có nguy cơ.** Hiện tại code join `TeamBrands` nhưng chưa check Team Role (chỉ check `tm.IsActive`). ContentCreator của Team khác vẫn có thể xóa lịch nếu cùng Brand. | **Rất cao (Critical)** | Phải siết thêm điều kiện `Content.TeamId` trùng với Team của Creator hoặc actor là Team Manager của Team tạo bài. |
| **Product Brand BOLA** | B1-02 | [`ProductService.cs#L345-L364`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/ProductService.cs#L345-L364): `IsUserAssignedToBrandAsync` kiểm tra membership qua TeamBrand. | **Ít nguy cơ.** Bản thân fix này đã dùng `TeamBrand`. Tuy nhiên cần nâng cấp: thao tác ghi Product đòi hỏi Team Role `MANAGER`, không cho phép `VIEWER` sửa. | **Trung bình (Medium)** | Bổ sung kiểm tra Team Role vào phương thức kiểm tra Brand. |
| **Ad Campaign BOLA** | B1-03 | [`AdCampaignService.cs#L55-L65`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/AdCampaignService.cs#L55-L65): Kiểm tra actor thuộc Team gán Brand trước khi tạo Campaign. | **Ít nguy cơ.** Đã tương thích bảng `TeamBrand`. Cần đảm bảo chỉ `TeamRole == Manager` mới được Deploy Campaign. | **Trung bình (Medium)** | Giữ nguyên check TeamBrand, thêm check Max Privilege Role >= Manager. |
| **Content Ownership BOLA** | Cửu vạn | [`ContentService.cs#L207, L257`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/ContentService.cs#L207): Creator chỉ được sửa/xóa bài do chính mình tạo (`PrimaryCreatorId == actor`). | **Không phá vỡ.** Logic này là bất biến theo Design Doc ("Creator: Chỉnh sửa/Xóa bài viết do chính mình tạo"). | **Thấp (Low)** | Bảo lưu nguyên vẹn điều kiện `OwnContent`. |

*(Ghi chú đặc biệt: Mã định danh dạng `C-01–C-04` và `H-01–H-09` không xuất hiện dạng text thô trong codebase hiện tại; các phát hiện an ninh trong repo được định danh là Bug B, B1-01, B1-02, B1-03. Chi tiết xem mục 7 - Open Questions).*

---

#### B.4. Phân tích Viewer Scope và Team Data Isolation
- **Tầng đang check Viewer hiện tại:**
  1. `ActiveWorkspaceMiddleware.cs` L395-L401: Viewer không có bất kỳ quyền nào trong số `ManageContent`, `PublishContent`, `ReviewContent`, `ManageSchedules`. Khi gọi các API này, middleware trả về `403 WORKSPACE_PERMISSION_DENIED`.
  2. `AisamContext.PermissionScope.cs` L38:
     ```csharp
     (PermissionOwner || PermissionManager || PermissionCreator && ...)
     ```
     Vì Viewer có cả 3 cờ `PermissionOwner`, `PermissionManager`, `PermissionCreator` đều bằng `false`, Query Filter EF Core trả về rỗng cho mọi câu truy vấn `Contents`!
- **Sự không tương thích với thiết kế mới:**
  - Tài liệu thiết kế quy định:
    > *"Viewer (VIEWER): Người xem/Dự thính. Chỉ xem được thông tin Brand, Lịch đăng bài (Calendar), Bài viết đã được duyệt và Báo cáo Analytics. Không được tạo/sửa/xóa bài."*
    > *"Xem danh sách Content: Member + Viewer: ✓ (Chỉ bài Team mình)"*
  - **Mâu thuẫn:** Hiện tại Viewer bị triệt tiêu 100% tầm nhìn bài viết trong database query filter.
  - **Sự thiếu vắng của Team Data Isolation:** Trong `AisamContext.PermissionScope.cs` L35-39, `Content` filter chỉ lọc theo `PermissionBrandIds.Contains(c.BrandId)`. Nó **hoàn toàn không lọc theo `c.TeamId`**. Do đó, hiện tại Team A và Team B cùng quản lý 1 Brand thì Creator của Team A có thể nhìn thấy bài viết nháp của Team B (nếu được bật cờ `PermissionViewAllBrandIds`), gây rò rỉ dữ liệu giữa các Team (Data Leakage) đúng như cảnh báo ở Mục I.2 tài liệu thiết kế.

---

### C. Các module phụ thuộc (Dependency Impact)

#### C.1. Approval Workflow
- **Role hiện tại gắn quyền duyệt:**
  - [`ActiveWorkspaceMiddleware.cs#L269-L274, L397`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L269-L274): Tuyến đường `/approve` và `/reject` yêu cầu `WorkspacePermissionEnum.ReviewContent`. Enum này chỉ cho phép `role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager`.
  - [`ResourcePermissionPolicy.cs#L55`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L55):
    ```csharp
    ResourcePermission.ApprovalReview => manager || creator && facts.CanReview,
    ```
- **Điểm nghẽn cần sửa để khớp Team Manager:**
  - Nếu một nhân sự có Workspace Role là `MEMBER` và Team Role là `MANAGER` (Team Manager): request duyệt bài của họ sẽ bị `ActiveWorkspaceMiddleware` chặn `403` ngay tại cửa vào vì `membership.Role == Member` không có `ReviewContent`!
  - **Khắc phục:** Phải gỡ bỏ kiểm tra `membership.Role` cứng cho `ReviewContent` tại `ActiveWorkspaceMiddleware`, chuyển toàn bộ thẩm quyền kiểm tra cho `ResourcePermissionFilter` và `AccessControlService` để xác nhận actor có quyền `TeamRole == Manager` trên Brand của bài viết đó.

---

#### C.2. Kết nối Channel Social (Social Integration)
- **Gán theo Brand hay Team?**
  - Thực thể `SocialIntegration` ([`SocialIntegration.cs#L22`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/SocialIntegration.cs#L22)) lưu trực tiếp `brand_id` và `workspace_id`. Kênh mạng xã hội thuộc sở hữu của Brand.
  - Sự liên kết với Team được thực hiện gián tiếp thông qua bảng `team_channel_access` (`team_brand_id` <-> `integration_id`).
- **Đối chiếu Ma trận quyền mới:**
  - Ma trận yêu cầu: Kết nối / Ngắt kết nối Kênh Social: `Owner: ✓`, `Workspace Manager: ✓`, `Member + Team Manager: ✓`, `Content Creator: ❌`, `Viewer: ❌`.
  - Thực tế trong code: [`SocialService.cs#L371-L373`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/SocialService.cs#L371-L373) yêu cầu `ResourcePermission.BrandManage` khi Link Targets. Điều này khớp hoàn toàn với kiến trúc mới: Team Manager được cấp quyền `BrandManage` trên Brand mình quản lý sẽ có quyền nối kênh.

---

#### C.3. Analytics / Báo cáo hiệu suất nhân viên
- **File:** [`MemberPerformanceService.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/MemberPerformanceService.cs)
- **Query hiện tại lọc theo gì?**
  - Kiểm tra dòng 55-57: Lọc theo `membership.Role` phẳng. Nếu chuyển user sang `WorkspaceRole = Member`, họ sẽ bị văng lỗi 403 `PerformanceAccessException`.
  - Kiểm tra dòng 88:
    ```csharp
    var contents=db.Contents.IgnoreQueryFilters().AsNoTracking().Where(c=>c.WorkspaceId==workspace && !c.IsDeleted && memberBrands.Contains(c.BrandId) && (!teamId.HasValue || c.TeamId==teamId));
    ```
- **Cần join thêm bảng nào?**
  - Hiện tại tham số `teamId` là optional từ client (`Guid? teamId = null`). Nếu Team Manager không truyền `teamId`, hệ thống đang lấy toàn bộ `contents` thuộc các `memberBrands`.
  - Để đảm bảo nguyên tắc phân quyền ("Member + Team Manager: Chỉ trong Team; Content Creator: Chỉ xem cá nhân"): Backend bắt buộc phải tự động join `team_members` để lấy danh sách các Team mà actor làm Manager, ép mệnh đề `c.TeamId IN (managedTeamIds)` và không cho phép client xem dữ liệu của thành viên thuộc Team khác dù cùng Brand.

---

#### C.4. Billing / PayOS
- **Xác nhận tính tách biệt của Logic Billing khỏi Role:**
  - File: [`ActiveWorkspaceMiddleware.cs#L154-L162, L392`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L154-L162)
    ```csharp
    WorkspacePermissionEnum.ManageBilling => role == WorkspaceMemberRoleEnum.Owner,
    ```
  - File: [`PaymentController.cs#L28-L29`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/PaymentController.cs#L28-L29):
    Các API thay đổi gói cước (`/api/payment/checkout`, `/cancel`) được bảo vệ nghiêm ngặt ở middleware chỉ cho phép duy nhất `Owner`.
  - **Tác động khi thêm `WORKSPACE_MANAGER`:**
    - Tuyệt đối an toàn đối với các thao tác thay đổi gói cước / thanh toán (vẫn khóa chặt cho `Owner`).
    - **Điểm cần điều chỉnh:** Ma trận mới quy định `Workspace Manager` được phép *"Xem lịch sử hóa đơn & thông tin Subscription"* (`✓`), trong khi `Member` thông thường thì `❌`. Hiện tại các route `GET /api/payment/history` và `GET /api/payment/subscription/current` đang thả nổi cho mọi member active (do dòng 156 bỏ qua kiểm tra cho HTTP GET). Cần siết lại các endpoint GET này chỉ cho phép `Owner` và `WorkspaceManager`.

---

## 3. GAP ANALYSIS (BẢNG ĐỐI CHIẾU GAP HIỆN TRẠNG VÀ MỤC TIÊU)

| STT | Hạng mục | Hiện trạng mã nguồn (Current State) | Mục tiêu thiết kế mới (Target State) | Mức độ thay đổi (Impact) |
| :---: | :--- | :--- | :--- | :---: |
| **1** | **Workspace Roles** | 4 vai trò phẳng trong `WorkspaceMemberRoleEnum`: `Owner(1)`, `Manager(2)`, `ContentCreator(3)`, `Viewer(4)`. | 3 vai trò cấp Workspace: `OWNER`, `WORKSPACE_MANAGER`, `MEMBER`. | 🟠 Vừa (Medium) |
| **2** | **Team Roles** | Cột `role` trong `TeamMember.cs` là `string` tự do; quyền thao tác lưu trong mảng `jsonb permissions`. | Chuẩn hóa Enum `TeamRoleEnum` gồm 3 bậc: `MANAGER`, `CONTENT_CREATOR`, `VIEWER`. | 🟠 Vừa (Medium) |
| **3** | **Quy tắc giải quyết quyền (Resolution Rule)** | Kiểm tra 1 role duy nhất từ WorkspaceMember hoặc cờ tĩnh `PermissionManager` toàn cục trong DbContext. | Quy tắc **Max Privilege Rule**: Lấy quyền cao nhất của User trên từng Brand thông qua các Team được nối bởi `TeamBrand`. | 🔴 Lớn (High) |
| **4** | **Cô lập dữ liệu Content (Team Data Isolation)** | EF Core Query Filter trên `Content` chỉ lọc theo `BrandId`, không lọc theo `TeamId`. Creator/Manager thấy bài của nhau. | EF Core Query Filter trên `Content` lọc theo `TeamId` của các Team mà User tham gia. User chỉ thấy bài của Team mình. | 🔴 Lớn (High) |
| **5** | **Viewer Content Scope** | Viewer bị chặn 100% ở `ActiveWorkspaceMiddleware` và bị EF Filter loại bỏ toàn bộ danh sách `Content`. | Viewer được xem danh sách bài viết đã duyệt của Team mình, xem Lịch đăng bài, xem Analytics. | 🟠 Vừa (Medium) |
| **6** | **Quyền Duyệt bài (Approval Workflow)** | Cố định cho `Owner` hoặc `Manager` cấp Workspace tại `ActiveWorkspaceMiddleware`. | Cho phép `Team Manager` duyệt bài của các Team mình quản lý trên Brand tương ứng. | 🟠 Vừa (Medium) |
| **7** | **Báo cáo hiệu suất (Member Performance)** | Cho phép chọn xem toàn Brand nếu không truyền `teamId`; chặn cứng role `Member` bằng lỗi 403. | Ép phạm vi chỉ trong Team của Manager; mở quyền cho Workspace Role `Member` nếu có Team Role tương ứng. | 🟠 Vừa (Medium) |
| **8** | **Xem thông tin hóa đơn (Billing View)** | Bỏ trống kiểm tra cho method GET; mọi role đều xem được lịch sử thanh toán qua API. | Chỉ cho phép `Owner` và `WorkspaceManager` xem; chặn toàn bộ `Member`. | 🟢 Nhỏ (Low) |

---

## 4. FEASIBILITY VERDICT THEO TỪNG HẠNG MỤC

| Hạng mục | Đánh giá | Lý do kỹ thuật cụ thể |
| :--- | :---: | :--- |
| **1. Database Schema DDL** | **EASY** | Các bảng `teams`, `team_members`, `team_brands`, `team_channel_access` và cột `contents.team_id` đã có sẵn trong database. Không cần tạo bảng mới, chỉ cần chỉnh sửa enum mapping và kiểu dữ liệu `team_members.role`. |
| **2. Module Billing & PayOS** | **EASY** | Logic ghi gói cước đã tách biệt hoàn toàn và khóa cho `Owner`. Chỉ cần thêm 1 nhánh kiểm tra cho `WorkspaceManager` ở các endpoint GET xem lịch sử hóa đơn. |
| **3. Approval & Publishing Workflow** | **MEDIUM** | Cần gỡ bỏ role check thô ở `ActiveWorkspaceMiddleware`, chuyển sang kiểm tra `ResourcePermission.ApprovalReview` dựa trên Max Privilege Team Role trong `ResourcePermissionFilter`. |
| **4. Social Channel Connection** | **MEDIUM** | Đã liên kết qua `TeamChannelAccess` và `TeamBrand`. Chỉ cần đảm bảo quyền `BrandManage` được kích hoạt cho `TeamRole.MANAGER`. |
| **5. Max Privilege Evaluation Engine** | **HARD** | Phải viết lại phương thức resolve quyền trong `AccessControlService` và xóa bỏ việc dùng cờ DbContext tĩnh (`db.PermissionManager`) trong `PermissionScopeMiddleware`. Engine phải query quan hệ `User -> TeamMember -> TeamBrand -> Brand` để tính toán Max Role cho từng request. |
| **6. Team Data Isolation Query Filter** | **HARD** | Cập nhật `HasQueryFilter` cho `Content` trong `AisamContext.PermissionScope.cs` để lọc `PermissionTeamIds.Contains(c.TeamId)`. Phải cẩn trọng xử lý 542 bài viết legacy đang có `team_id = null` để không làm sập dashboard của khách hàng. |
| **7. Viewer Scope Synchronization** | **MEDIUM** | Sửa `ActiveWorkspaceMiddleware` và nới lỏng EF Filter cho phép Viewer đọc các `Content` có trạng thái `Approved` / `Published` thuộc Team của họ. |

---

## 5. RISK REGISTER (DANH MỤC RỦI RO KỸ THUẬT)

| Mã Rủi ro | Tên Rủi ro | Phân tích Nguy cơ & Hậu quả | Biện pháp Giảm thiểu Bắt buộc |
| :---: | :--- | :--- | :--- |
| **R-01** | **Data Leakage giữa các Team trong lúc chuyển đổi** | Nếu chuyển role của User sang mô hình mới nhưng Query Filter chưa kịp siết `c.TeamId`, Team Manager của Team A có thể can thiệp bài viết của Team B trên cùng Brand. | Phải cập nhật EF Query Filter và `ResourcePermissionPolicy` đồng bộ trong cùng một transaction/release. |
| **R-02** | **Trigger PostgreSQL chặn Migration (`23514`)** | Trigger `aisam_permission_integrity()` trong database hiện tại đang bắt lỗi `Team transfer requires explicit migration` nếu có câu lệnh UPDATE làm đổi `team_id` trên `team_brands` hoặc `contents`. | Mọi script data migration phải tuân thủ nghiêm ngặt: INSERT bản ghi mới trước, chuyển đổi liên kết, sau đó xóa bản ghi cũ, hoặc tạm disable trigger trong transaction migration có kiểm soát. |
| **R-03** | **Biến mất 542 bài viết Legacy đối với người dùng** | 542 bài viết cũ không có `primary_creator_id` và có thể thiếu `team_id`. Nếu filter theo `TeamId` lập tức kích hoạt, toàn bộ Creator và Team Manager sẽ thấy màn hình trắng trơn (chỉ Owner thấy). | Phải có migration backfill gán các bài viết legacy này vào "Default Team" của Workspace tương ứng trước khi bật Query Filter mới. |
| **R-04** | **CI Test "Green-False" (Báo xanh giả lập)** | Một số bài test dùng SQLite hoặc In-Memory Database không kích hoạt được trigger `aisam_permission_integrity()` của PostgreSQL, dẫn đến việc pass test trên máy dev nhưng nổ lỗi trigger trên Production. | Bắt buộc chạy toàn bộ test suite trên container Docker PostgreSQL thực tế có nạp migration `20260908003735_CompletePermissionSchema`. |
| **R-05** | **Tê liệt UI do Frontend lệch Enum** | Nếu backend đổi giá trị enum role của `WorkspaceMember` mà frontend chưa đồng bộ, các hàm kiểm tra `isOwner`, `isManager` trên Next.js/React sẽ bị fail, dẫn đến ẩn thanh điều hướng và menu chức năng. | Định nghĩa rõ Backward-Compatible DTO layer trong giai đoạn chuyển tiếp. |

---

## 6. EFFORT ESTIMATE (ƯỚC TÍNH ĐIỂM CHẠM MÃ NGUỒN)

Bảng kiểm kê số lượng điểm chạm (touchpoints) trong mã nguồn backend:

1. **Entity & Enum Layer (4 điểm chạm):**
   - [`WorkspaceMemberRoleEnum.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Enumeration/WorkspaceMemberRoleEnum.cs): Cập nhật 3 roles mới.
   - [NEW] `TeamRoleEnum.cs`: Tạo enum mới gồm `Manager(1)`, `ContentCreator(2)`, `Viewer(3)`.
   - [`TeamMember.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/TeamMember.cs#L24): Đổi `string Role` thành `TeamRoleEnum Role`.
   - [`Content.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Model/Content.cs): Bổ sung Navigation Property `public virtual Team? Team { get; set; }`.

2. **Middleware Layer (6 điểm chạm):**
   - [`ActiveWorkspaceMiddleware.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs):
     - Dòng 154-162: Siết quyền GET billing cho Owner/WorkspaceManager.
     - Dòng 267-274: Gỡ bỏ chặn role cứng cho `/approve` và `/reject`.
     - Dòng 386-407: Sửa hàm `EnsurePermission` cho phù hợp với 3 Workspace Roles.
   - [`PermissionScopeMiddleware.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/PermissionScopeMiddleware.cs):
     - Dòng 21-34: Viết lại việc nạp quyền theo cấu trúc Team-Brand, loại bỏ biến cờ toàn cục gây leak quyền.

3. **EF Core Context & Query Filters (5 điểm chạm):**
   - [`AisamContext.PermissionScope.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs):
     - Dòng 35-39: Thêm điều kiện cô lập theo `TeamId` vào Query Filter của `Content`.
     - Dòng 32-33: Sửa query filter cho `WorkspaceMember`.
     - Dòng 43-46: Rà soát lại query filter cho `Post`.
   - [`AisamContext.PermissionIntegrity.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionIntegrity.cs):
     - Rà soát các hàm đảm bảo toàn vẹn dữ liệu tự động.

4. **Access Control & Policy Engine (8 điểm chạm):**
   - [`ResourcePermissionPolicy.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs): Sửa `ResourcePermissionFacts` và bảng logic `Allows` nhận Team Role.
   - [`AccessControlService.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/AccessControlService.cs): Viết thuật toán Max Privilege Rule trên tập `TeamBrands`.
   - [`ResourcePermissionFilter.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ResourcePermissionFilter.cs): Bổ sung kiểm tra Team Role cho các hành động đặc thù.

5. **Services & Repositories (15 điểm chạm):**
   - `ContentService.cs`: Sửa các hàm CRUD và Approve/Reject để tương thích Team Role.
   - `ContentScheduleService.cs`: Đảm bảo lịch đăng bài cô lập theo Team.
   - `BrandService.cs` & `ProductService.cs`: Cập nhật hàm check quyền quản trị Brand.
   - `TeamService.cs`: Cập nhật logic quản lý Team theo quyền `WorkspaceManager` và `TeamManager`.
   - `MemberPerformanceService.cs`: Cập nhật query báo cáo nhân viên lọc theo Team.
   - `WorkspaceMemberService.cs` & `WorkspaceInvitationService.cs`: Cập nhật luồng mời và gán quyền thành viên Workspace.

**Tổng cộng:** Ước tính **khoảng 38 điểm chạm trực tiếp** trong codebase .NET.

---

## 7. DATA MIGRATION STRATEGY (ĐỀ XUẤT CHIẾN LƯỢC DỮ LIỆU)

### Các bước di chuyển dữ liệu (Data Migration Steps):
1. **Bước 1 (Schema Prep):** Bổ sung kiểu dữ liệu/cột mới mà không xóa cột cũ. Thêm Enum `TeamRoleEnum`.
2. **Bước 2 (Data Backfill cho Legacy Content):** Chạy script idempotent kiểm tra 542 bài viết chưa có `team_id`. Nếu Brand của bài viết đã nối với Team nào thì gán vào Team đó; nếu chưa có Team thì gán vào "Default Team" của Workspace.
3. **Bước 3 (WorkspaceMember Role Migration):**
   - Các bản ghi `Role == Owner(1)` giữ nguyên `Owner`.
   - Các bản ghi `Role == Manager(2)`: Cần quyết định chuyển thành `WorkspaceManager` hay `Member` (Xem Open Question 1).
   - Các bản ghi `Role == ContentCreator(3)` hoặc `Viewer(4)`: Chuyển thành `Member` cấp Workspace, đồng thời kiểm tra tạo bản ghi tương ứng trong `team_members` nếu chưa có.
4. **Bước 4 (TeamMember Role Standardization):** Chuẩn hóa cột `role` trong `team_members` từ string sang enum tương ứng.
5. **Bước 5 (Switch Logic & Verification):** Bật logic phân quyền mới, chạy bộ kiểm thử toàn diện.

### So sánh chiến lược triển khai:
- **Big-Bang Migration:**
  - *Đặc điểm:* Dừng hệ thống trong 1 khoảng bảo trì, chạy toàn bộ migration script, deploy đồng loạt BE và FE.
  - *Đánh giá:* Dữ liệu hiện tại của hệ thống tương đối nhỏ (544 contents, 72 team_members, 46 team_brands), thời gian chạy script chỉ tính bằng giây. Big-bang là phương án ít tốn công overhead nhất.
- **Dual-Write / Blue-Green:**
  - *Đặc điểm:* Hỗ trợ đồng thời 2 bảng phân quyền, đồng bộ qua lại.
  - *Đánh giá:* Quá phức tạp và thừa thãi đối với quy mô dữ liệu hiện tại, dễ gây lỗi conflict trigger.
- **Feature-Flag theo Workspace (Khuyến nghị nếu muốn an toàn tối đa):**
  - *Đặc điểm:* Cho phép bật cờ `UseDecoupledRbac` theo từng Workspace. Các Workspace thử nghiệm sẽ chạy RBAC mới, Workspace cũ vẫn chạy Flat RBAC cho đến khi nghiệm thu hoàn tất.

---

## 8. OPEN QUESTIONS (CẦN KIET QUYẾT ĐỊNH TRƯỚC KHI LẬP CHANGE PLAN)

> [!IMPORTANT]
> **Câu hỏi 1: Xử lý các tài khoản đang có vai trò `Manager` ở cấp WorkspaceMember cũ thế nào khi migrate?**
> - Trong Flat RBAC, `Manager` là người quản lý chung.
> - Trong mô hình mới, `WORKSPACE_MANAGER` là vai trò HR Admin tối cao (mời/xóa thành viên, tạo/xóa Team toàn Workspace).
> - Nếu tự động chuyển tất cả `Manager` cũ thành `WORKSPACE_MANAGER`, họ sẽ nhận được quyền quản trị nhân sự toàn workspace (vượt quá quyền quản lý content/brand mà họ từng có).
> - **Lựa chọn:** Chuyển `Manager` cũ thành `Member` (và cấp `TeamRole = Manager` trong các Team họ đang phụ trách), CHỈ chuyển thành `WORKSPACE_MANAGER` nếu Owner chủ động chỉ định?

> [!IMPORTANT]
> **Câu hỏi 2: Chính sách xử lý 542 bài viết Legacy thiếu `primary_creator_id` và `team_id`?**
> - Quyết định D03/D04 trước đó ghi: *"Attribution cũ không xác định được giữ null và báo cáo"*.
> - Nếu giữ `team_id = null`, theo luật Team Data Isolation, **chỉ có Owner và WorkspaceManager nhìn thấy các bài này**, toàn bộ Team Member khác sẽ không thấy trên Calendar/Content List.
> - **Lựa chọn:** Chấp nhận chỉ Owner thấy các bài viết legacy này, HAY chạy script gán toàn bộ bài cũ vào "Default Team" của Workspace để các thành viên trong Default Team cùng thấy?

> [!WARNING]
> **Câu hỏi 3: Làm rõ danh mục mã lỗi `C-01–C-04` và `H-01–H-09`?**
> - Trong yêu cầu audit có nhắc đến *"Các fix BOLA đã làm trước đó (C-01–C-04, H-01–H-09)"*.
> - Auditor đã rà soát toàn bộ git commits, documentation và codebase: các lỗi BOLA được định danh trong hệ thống hiện tại là `Bug B`, `B1-01`, `B1-02`, `B1-03`, `A2`, `A3`, `A4` (trong `AISAM_ChangePlan.md`). Mã `C-01–C-04` và `H-01–H-09` không có trong repo (có thể từ báo cáo pentest bên ngoài hoặc danh mục test bảo mật riêng của Kiet).
> - Kiet vui lòng xác nhận danh sách chi tiết các mã này (nếu có tài liệu đính kèm riêng) để đối chiếu 1-1.

> [!NOTE]
> **Câu hỏi 4: Kế hoạch đồng bộ giao diện người dùng (Frontend - Next.js)?**
> - Việc sửa Workspace Role thành 3 role mới và đưa quyền thao tác về Team Role đòi hỏi cập nhật các hook phân quyền phía Frontend (`usePermission`, `Sidebar.tsx`, `CreateTeamWizard.tsx`, trang quản lý Team và Brands).
> - Giai đoạn 1 sẽ ưu tiên hoàn thiện backend hay triển khai song song cả FE?

---
*Báo cáo được lập theo đúng ràng buộc Read-Only của Phase Thẩm định Kỹ thuật. Dừng lại chờ người dùng xem xét và phê duyệt.*
