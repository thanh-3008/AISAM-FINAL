# RBAC COMPLIANCE VERIFICATION REPORT
**Dự án:** AISAM — Nền tảng Phân quyền Đa tầng (Decoupled Team-Based Scoped RBAC)  
**Tài liệu tham chiếu gốc (Ground Truth):** `TÀI LIỆU THIẾT KẾ KIẾN TRÚC - Decoupled Team-Based Scoped RBAC.docx`  
**Vai trò kiểm định:** Compliance Auditor (Độc lập — Read-Only — Không chỉnh sửa mã nguồn)  
**Ngày lập:** 12/09/2026  
**Trạng thái kiểm tra:** HOÀN THÀNH — PHÁT HIỆN LỖ HỔNG BẢO MẬT & SAI LỆCH CHỨC NĂNG

---

## 1. EXECUTIVE SUMMARY

> [!CAUTION]
> ### 🚨 CẢNH BÁO BẢO MẬT NGHIÊM TRỌNG (FAIL NGHIÊM TRỌNG)
> Phát hiện **1 LỖ HỔNG BẢO MẬT LEO THANG ĐẶC QUYỀN**:
> - **Hành động:** Nối / Hủy nối Team với Brand (`PUT/DELETE /api/brands/{brandId}/teams/{teamId}`).
> - **Tài liệu thiết kế quy định:** Cấm tuyệt đối đối với `Member + Team Manager` (Ký hiệu: **❌**). Hành vi này độc quyền thuộc về `Owner` và `Workspace Manager`.
> - **Code thực tế:** Tại [`AssignmentService.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/AssignmentService.cs#L20-L73), hệ thống kiểm tra `ResourcePermission.BrandManage` (vốn được cấp cho Team Manager tại [`ResourcePermissionPolicy.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L137)) và chỉ yêu cầu người dùng thuộc `teamId` mục tiêu (L71). Do đó, **bất kỳ `Member + Team Manager` nào cũng có thể tự ý gán hoặc gỡ Team của mình vào/khỏi Brand** mà không cần sự đồng ý của `Owner` hay `Workspace Manager`.

### Thống kê tổng hợp tuân thủ ma trận phân quyền (90 ô kiểm tra = 18 hành động × 5 vai trò):
- **Tổng số ô kiểm tra:** 90 ô (18 hành động × 5 vai trò)
- **PASS (Tuân thủ hoàn toàn):** 78 ô (86.67%)
- **FAIL Chức năng (Tài liệu ghi ✓ nhưng code chặn):** 11 ô (12.22%)
- **FAIL NGHIÊM TRỌNG (Tài liệu ghi ❌ nhưng code cho phép — Lỗ hổng bảo mật):** 1 ô (1.11%)
- **PARTIAL / KHÔNG XÁC ĐỊNH:** 0 ô (0%)
- **TỶ LỆ TUÂN THỦ TOÀN DIỆN:** **86.67%**

### Tóm tắt các nhóm Non-Compliance chính được phát hiện:
1. **Lỗ hổng leo thang quyền Team-Brand (Lỗi bảo mật):** `Member + Team Manager` có thể can thiệp liên kết Brand-Team.
2. **Liệt quyền Quản trị của `Workspace Manager` (HR Admin):**
   - Không thể Mời thành viên mới (`WorkspaceInvitationService.cs` L57 chỉ cho phép `Owner`).
   - Không thể Xóa thành viên (`WorkspaceMemberService.cs` L215 chỉ cho phép `Owner`).
   - Bị chặn Tạo Team trừ khi có delegated permission `TeamCreate` (`TeamService.cs` L156).
   - Bị chặn Sửa/Xóa Team và Thêm/Đổi vai trò thành viên Team nếu bản thân không gia nhập Team đó (`TeamService.cs` L51).
   - Bị chặn Tạo Brand nếu không có delegated permission `BrandCreate` (`BrandService.cs` L100).
   - Bị giới hạn Báo cáo Hiệu suất trong phạm vi Team mình tham gia thay vì toàn Workspace (`MemberPerformanceService.cs` L79).
3. **Liệt quyền Điều hành của `Member + Team Manager`:**
   - Không thể Lên lịch bài viết (`/api/content-schedules`) do `ActiveWorkspaceMiddleware.cs` L249+L403 kiểm tra cứng enum Workspace role cũ (`Owner/Manager/WorkspaceManager`), chặn toàn bộ người dùng có role `Member` (int 3).
   - Không thể Hủy bài chờ đăng (`DELETE /api/content-schedules/{id}`) do cùng lý do trên.
   - Không thể Quản lý thành viên trong chính Team của mình (`TeamService.cs` L48 kiểm tra `member.Role != WorkspaceMemberRoleEnum.Manager`, chặn role `Member`).
4. **Viewer & Creator bị chặn xem Báo cáo Analytics của Brand:**
   - EF Global Query Filter trên `PerformanceReport` ([`AisamContext.PermissionScope.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L70)) và [`ResourcePermissionPolicy.cs`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L147) yêu cầu cờ Manager, lọc sạch 100% dữ liệu đối với `ContentCreator` và `Viewer`.

---

## 2. MA TRẬN ĐỐI CHIẾU CHI TIẾT (FULL 90 DÒNG — TỪNG DÒNG, TỪNG CỘT)

| # | Nhóm chức năng | Hành động chi tiết | Vai trò | Tài liệu yêu cầu | Code thực tế | Bằng chứng thô (File + Dòng + Trích dẫn) | Trạng thái |
|---|---|---|---|---|---|---|---|
| 1 | Workspace & Billing | Thay đổi gói cước / Thanh toán PayOS | Owner | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L396`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L396): `WorkspacePermissionEnum.ManageBilling => role == WorkspaceMemberRoleEnum.Owner` | **PASS** |
| 2 | Workspace & Billing | Thay đổi gói cước / Thanh toán PayOS | Workspace Manager | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L396`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L396): Role `WorkspaceManager` != `Owner` trả về `WORKSPACE_PERMISSION_DENIED` | **PASS** |
| 3 | Workspace & Billing | Thay đổi gói cước / Thanh toán PayOS | Member + Team Manager | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L396`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L396): Role `Member` != `Owner` trả về 403 | **PASS** |
| 4 | Workspace & Billing | Thay đổi gói cước / Thanh toán PayOS | Member + Content Creator | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L396`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L396): Role `Member` != `Owner` trả về 403 | **PASS** |
| 5 | Workspace & Billing | Thay đổi gói cước / Thanh toán PayOS | Member + Viewer | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L396`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L396): Role `Member` != `Owner` trả về 403 | **PASS** |
| 6 | Workspace & Billing | Xem lịch sử hóa đơn & thông tin Subscription | Owner | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L158`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L158): `if (membership.Role is not (WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.WorkspaceManager))` -> Cho phép Owner | **PASS** |
| 7 | Workspace & Billing | Xem lịch sử hóa đơn & thông tin Subscription | Workspace Manager | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L158`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L158): `membership.Role is ... WorkspaceManager` -> Cho phép WorkspaceManager | **PASS** |
| 8 | Workspace & Billing | Xem lịch sử hóa đơn & thông tin Subscription | Member + Team Manager | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L160`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L160): Trả về 403 `You do not have permission to view billing in the active workspace.` | **PASS** |
| 9 | Workspace & Billing | Xem lịch sử hóa đơn & thông tin Subscription | Member + Content Creator | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L160`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L160): Trả về 403 | **PASS** |
| 10 | Workspace & Billing | Xem lịch sử hóa đơn & thông tin Subscription | Member + Viewer | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L160`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L160): Trả về 403 | **PASS** |
| 11 | Workspace & Billing | Đổi tên hoặc Xóa Workspace | Owner | ✓ | Cho phép đổi tên; xóa qua Admin | [`WorkspaceService.cs:L150`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceService.cs#L150): Cho phép Owner đổi tên; [`WorkspaceController.cs:L72`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/WorkspaceController.cs#L72): Delete yêu cầu role Admin hệ thống | **PASS** |
| 12 | Workspace & Billing | Đổi tên hoặc Xóa Workspace | Workspace Manager | ❌ | Chặn 403 | [`WorkspaceService.cs:L150`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceService.cs#L150): `if (membership.Role != WorkspaceMemberRoleEnum.Owner)` trả về 403 | **PASS** |
| 13 | Workspace & Billing | Đổi tên hoặc Xóa Workspace | Member + Team Manager | ❌ | Chặn 403 | [`WorkspaceService.cs:L150`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceService.cs#L150): Trả về 403 | **PASS** |
| 14 | Workspace & Billing | Đổi tên hoặc Xóa Workspace | Member + Content Creator | ❌ | Chặn 403 | [`WorkspaceService.cs:L150`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceService.cs#L150): Trả về 403 | **PASS** |
| 15 | Workspace & Billing | Đổi tên hoặc Xóa Workspace | Member + Viewer | ❌ | Chặn 403 | [`WorkspaceService.cs:L150`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceService.cs#L150): Trả về 403 | **PASS** |
| 16 | Nhân sự (HR Management) | Mời / Xóa thành viên khỏi Workspace | Owner | ✓ | Cho phép | [`WorkspaceInvitationService.cs:L57`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs#L57) & [`WorkspaceMemberService.cs:L215`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceMemberService.cs#L215): Cho phép Owner | **PASS** |
| 17 | Nhân sự (HR Management) | Mời / Xóa thành viên khỏi Workspace | Workspace Manager | ✓ | **Bị chặn 403** | [`WorkspaceInvitationService.cs:L57`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs#L57): `if (inviterMembership?.Role != WorkspaceMemberRoleEnum.Owner) return Error("Only the workspace owner can invite members.", Forbidden)` & [`WorkspaceMemberService.cs:L215`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceMemberService.cs#L215): `if (actor?.Role != WorkspaceMemberRoleEnum.Owner) return ("Only the workspace owner can manage members.", Forbidden)` | **FAIL** |
| 18 | Nhân sự (HR Management) | Mời / Xóa thành viên khỏi Workspace | Member + Team Manager | ❌ | Chặn 403 | [`WorkspaceInvitationService.cs:L57`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs#L57): Chặn 403 | **PASS** |
| 19 | Nhân sự (HR Management) | Mời / Xóa thành viên khỏi Workspace | Member + Content Creator | ❌ | Chặn 403 | [`WorkspaceInvitationService.cs:L57`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs#L57): Chặn 403 | **PASS** |
| 20 | Nhân sự (HR Management) | Mời / Xóa thành viên khỏi Workspace | Member + Viewer | ❌ | Chặn 403 | [`WorkspaceInvitationService.cs:L57`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs#L57): Chặn 403 | **PASS** |
| 21 | Nhân sự (HR Management) | Tạo / Sửa / Xóa Team | Owner | ✓ | Cho phép | [`TeamService.cs:L47, L150`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L47): `if (member.Role == WorkspaceMemberRoleEnum.Owner) return;` | **PASS** |
| 22 | Nhân sự (HR Management) | Tạo / Sửa / Xóa Team | Workspace Manager | ✓ | **Bị hạn chế / Chặn** | [`TeamService.cs:L156`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L156): Tạo Team đòi hỏi `HasDelegatedPermission(TeamCreate)`; [`TeamService.cs:L51-54`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L51): Sửa/Xóa Team đòi hỏi phải là thành viên của Team đó (`isTeamMember`) | **FAIL** |
| 23 | Nhân sự (HR Management) | Tạo / Sửa / Xóa Team | Member + Team Manager | ❌ | Chặn 403 | [`TeamService.cs:L161`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L161): Chặn 403 | **PASS** |
| 24 | Nhân sự (HR Management) | Tạo / Sửa / Xóa Team | Member + Content Creator | ❌ | Chặn 403 | [`TeamService.cs:L161`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L161): Chặn 403 | **PASS** |
| 25 | Nhân sự (HR Management) | Tạo / Sửa / Xóa Team | Member + Viewer | ❌ | Chặn 403 | [`TeamService.cs:L161`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L161): Chặn 403 | **PASS** |
| 26 | Nhân sự (HR Management) | Thêm / Đổi Role của Member trong Team | Owner | ✓ | Cho phép | [`TeamService.cs:L47`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L47): `if (member.Role == WorkspaceMemberRoleEnum.Owner) return;` | **PASS** |
| 27 | Nhân sự (HR Management) | Thêm / Đổi Role của Member trong Team | Workspace Manager | ✓ | **Bị chặn nếu không thuộc team** | [`TeamService.cs:L53`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L53): `if (!isTeamMember) throw new UnauthorizedAccessException("Manager can only manage teams they belong to.");` | **FAIL** |
| 28 | Nhân sự (HR Management) | Thêm / Đổi Role của Member trong Team | Member + Team Manager | ✓ (Chỉ trong Team mình) | **Bị chặn 403** | [`TeamService.cs:L48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L48): `if (member.Role != WorkspaceMemberRoleEnum.Manager) throw new UnauthorizedAccessException("Only Owner or Manager can manage teams.");` Do user có role Workspace là `Member` (int 3), code ném 403 ngay lập tức, không xét `TeamMember.Role == Manager`! | **FAIL** |
| 29 | Nhân sự (HR Management) | Thêm / Đổi Role của Member trong Team | Member + Content Creator | ❌ | Chặn 403 | [`TeamService.cs:L48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L48): Chặn 403 | **PASS** |
| 30 | Nhân sự (HR Management) | Thêm / Đổi Role của Member trong Team | Member + Viewer | ❌ | Chặn 403 | [`TeamService.cs:L48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L48): Chặn 403 | **PASS** |
| 31 | Quản lý Brand & Kênh | Tạo / Sửa / Xóa Brand | Owner | ✓ | Cho phép | [`BrandService.cs:L78, L330`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/BrandService.cs#L78): Cho phép Owner | **PASS** |
| 32 | Quản lý Brand & Kênh | Tạo / Sửa / Xóa Brand | Workspace Manager | ✓ | **Bị hạn chế khi tạo** | [`BrandService.cs:L84-104`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/BrandService.cs#L84): `if (membership.Role == WorkspaceMemberRoleEnum.Manager)` (WM có value 2 trùng alias Manager) -> Đòi hỏi `BrandCreate` trong `TeamMember.Permissions`, nếu không có sẽ bị chặn | **FAIL** |
| 33 | Quản lý Brand & Kênh | Tạo / Sửa / Xóa Brand | Member + Team Manager | ❌ | Chặn 403 | [`BrandService.cs:L330`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/BrandService.cs#L330): `if (membership.Role != Owner && != Manager && != WorkspaceManager) return Error("Only workspace Owner and Manager can manage brands")` | **PASS** |
| 34 | Quản lý Brand & Kênh | Tạo / Sửa / Xóa Brand | Member + Content Creator | ❌ | Chặn 403 | [`BrandService.cs:L330`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/BrandService.cs#L330): Chặn 403 | **PASS** |
| 35 | Quản lý Brand & Kênh | Tạo / Sửa / Xóa Brand | Member + Viewer | ❌ | Chặn 403 | [`BrandService.cs:L330`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/BrandService.cs#L330): Chặn 403 | **PASS** |
| 36 | Quản lý Brand & Kênh | Nối / Hủy nối Team với Brand | Owner | ✓ | Cho phép | [`AssignmentService.cs:L20, L71`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/AssignmentService.cs#L20): Cho phép Owner toàn quyền | **PASS** |
| 37 | Quản lý Brand & Kênh | Nối / Hủy nối Team với Brand | Workspace Manager | ✓ | **Bị chặn nếu không thuộc team** | [`AssignmentService.cs:L71`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/AssignmentService.cs#L71): `if(member.Role!=Owner && !await db.TeamMembers.AnyAsync(m=>m.TeamId==team.Id && m.UserId==actor))` -> Chặn WorkspaceManager nếu không tham gia team | **FAIL** |
| 38 | Quản lý Brand & Kênh | Nối / Hủy nối Team với Brand | Member + Team Manager | ❌ | **CHO PHÉP THỰC HIỆN** | [`AssignmentService.cs:L20, L71, L108-110`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/AssignmentService.cs#L20): Team Manager có `BrandManage`, qua được L71 nếu thuộc team -> Thực hiện `assignment.IsActive = request.Active` thành công! | **FAIL NGHIÊM TRỌNG (SECURITY HOLE)** |
| 39 | Quản lý Brand & Kênh | Nối / Hủy nối Team với Brand | Member + Content Creator | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L137`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L137): Creator không có `BrandManage` -> Chặn tại `AssignmentService.ReadAsync` | **PASS** |
| 40 | Quản lý Brand & Kênh | Nối / Hủy nối Team với Brand | Member + Viewer | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L137`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L137): Viewer không có `BrandManage` -> Chặn tại `AssignmentService.ReadAsync` | **PASS** |
| 41 | Quản lý Brand & Kênh | Kết nối / Ngắt kết nối Kênh Social | Owner | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L146`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L146): `ResourcePermission.SocialManage => teamManager && facts.ChannelAccessible && facts.ChannelCanManage` (Owner luôn true) | **PASS** |
| 42 | Quản lý Brand & Kênh | Kết nối / Ngắt kết nối Kênh Social | Workspace Manager | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L113, L146`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L113): `teamManager` bao gồm `workspaceManager` -> Cho phép | **PASS** |
| 43 | Quản lý Brand & Kênh | Kết nối / Ngắt kết nối Kênh Social | Member + Team Manager | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L113, L146`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L113): `TeamRole == TeamRoleEnum.Manager` -> Cho phép trên các kênh được cấp | **PASS** |
| 44 | Quản lý Brand & Kênh | Kết nối / Ngắt kết nối Kênh Social | Member + Content Creator | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L146`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L146): Creator không phải teamManager -> Chặn 403 | **PASS** |
| 45 | Quản lý Brand & Kênh | Kết nối / Ngắt kết nối Kênh Social | Member + Viewer | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L146`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L146): Chặn 403 | **PASS** |
| 46 | Quản lý Nội dung | Xem danh sách Content | Owner | ✓ | Cho phép | [`AisamContext.PermissionScope.cs:L40-48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L40): `PermissionOwner` bypass toàn bộ filter | **PASS** |
| 47 | Quản lý Nội dung | Xem danh sách Content | Workspace Manager | ✓ (Toàn bộ) | Cho phép toàn bộ | [`AisamContext.PermissionScope.cs:L40-48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L40): `PermissionWorkspaceManager` bypass toàn bộ filter | **PASS** |
| 48 | Quản lý Nội dung | Xem danh sách Content | Member + Team Manager | ✓ (Chỉ bài Team mình) | Cho phép bài Team mình | [`AisamContext.PermissionScope.cs:L41`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L41): `PermissionTeamIds.Contains(c.TeamId.Value)` lọc đúng bài của Team mình | **PASS** |
| 49 | Quản lý Nội dung | Xem danh sách Content | Member + Content Creator | ✓ (Chỉ bài Team mình) | Cho phép bài Team mình | [`AisamContext.PermissionScope.cs:L41, L45, L48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L41): Lọc theo TeamId, thấy bài nháp của mình và bài Approved của đồng đội | **PASS** |
| 50 | Quản lý Nội dung | Xem danh sách Content | Member + Viewer | ✓ (Chỉ bài Team mình) | Cho phép bài Team mình | [`AisamContext.PermissionScope.cs:L41, L48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L41): Lọc theo TeamId, chỉ thấy bài trạng thái Approved/Published | **PASS** |
| 51 | Quản lý Nội dung | Tạo bài viết mới / Upload Media | Owner | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L138`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L138): `ResourcePermission.ContentCreate => teamManager || teamCreator` -> Cho phép Owner | **PASS** |
| 52 | Quản lý Nội dung | Tạo bài viết mới / Upload Media | Workspace Manager | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L113, L138`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L113): Cho phép WorkspaceManager | **PASS** |
| 53 | Quản lý Nội dung | Tạo bài viết mới / Upload Media | Member + Team Manager | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L113, L138`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L113): Cho phép Team Manager | **PASS** |
| 54 | Quản lý Nội dung | Tạo bài viết mới / Upload Media | Member + Content Creator | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L114, L138`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L114): Cho phép Content Creator | **PASS** |
| 55 | Quản lý Nội dung | Tạo bài viết mới / Upload Media | Member + Viewer | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L138`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L138): Viewer không có ContentCreate -> Bị chặn tại mutation hook [`PermissionScopeMiddleware.cs:L126`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/PermissionScopeMiddleware.cs#L126) | **PASS** |
| 56 | Quản lý Nội dung | Chỉnh sửa / Xóa bài viết | Owner | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L141`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L141): Cho phép Owner | **PASS** |
| 57 | Quản lý Nội dung | Chỉnh sửa / Xóa bài viết | Workspace Manager | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L141`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L141): Cho phép WorkspaceManager | **PASS** |
| 58 | Quản lý Nội dung | Chỉnh sửa / Xóa bài viết | Member + Team Manager | ✓ (Bài trong Team) | Cho phép bài trong Team | [`ResourcePermissionPolicy.cs:L141`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L141): `(teamManager && (facts.SameTeamContent || facts.OwnContent))` -> Cho phép bài cùng Team | **PASS** |
| 59 | Quản lý Nội dung | Chỉnh sửa / Xóa bài viết | Member + Content Creator | ✓ (Bài do chính mình tạo) | Cho phép bài do mình tạo | [`ResourcePermissionPolicy.cs:L141`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L141): `(teamCreator && facts.OwnContent)` -> Chỉ cho phép sửa/xóa bài do chính mình tạo | **PASS** |
| 60 | Quản lý Nội dung | Chỉnh sửa / Xóa bài viết | Member + Viewer | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L141`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L141): Chặn Viewer | **PASS** |
| 61 | Quản lý Nội dung | Duyệt / Từ chối bài viết (Approval) | Owner | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L142`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L142): `ResourcePermission.ApprovalReview => teamManager` -> Cho phép Owner | **PASS** |
| 62 | Quản lý Nội dung | Duyệt / Từ chối bài viết (Approval) | Workspace Manager | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L142`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L142): Cho phép WorkspaceManager | **PASS** |
| 63 | Quản lý Nội dung | Duyệt / Từ chối bài viết (Approval) | Member + Team Manager | ✓ | Cho phép | [`ResourcePermissionPolicy.cs:L142`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L142) kết hợp [`ActiveWorkspaceMiddleware.cs:L123-127`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L123): Cho phép Team Manager duyệt bài | **PASS** |
| 64 | Quản lý Nội dung | Duyệt / Từ chối bài viết (Approval) | Member + Content Creator | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L142`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L142): Không có delegated Review -> Chặn 403 | **PASS** |
| 65 | Quản lý Nội dung | Duyệt / Từ chối bài viết (Approval) | Member + Viewer | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L142`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L142): Chặn 403 | **PASS** |
| 66 | Đăng bài (Publishing) | Đăng ngay | Owner | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L400`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L400) & [`ResourcePermissionPolicy.cs:L143`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L143): Cho phép Owner | **PASS** |
| 67 | Đăng bài (Publishing) | Đăng ngay | Workspace Manager | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L400`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L400) & [`ResourcePermissionPolicy.cs:L143`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L143): Cho phép WorkspaceManager | **PASS** |
| 68 | Đăng bài (Publishing) | Đăng ngay | Member + Team Manager | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L123-127`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L123) & [`ResourcePermissionPolicy.cs:L143`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L143): Cho phép Team Manager | **PASS** |
| 69 | Đăng bài (Publishing) | Đăng ngay | Member + Content Creator | ❌ (Cần gửi duyệt) | Chặn 403 | [`ResourcePermissionPolicy.cs:L143`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L143): Chặn đăng trực tiếp nếu không có `CanPublish` | **PASS** |
| 70 | Đăng bài (Publishing) | Đăng ngay | Member + Viewer | ❌ | Chặn 403 | [`ResourcePermissionPolicy.cs:L143`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L143): Chặn Viewer | **PASS** |
| 71 | Đăng bài (Publishing) | Lên lịch (Schedule) | Owner | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L403): Cho phép Owner | **PASS** |
| 72 | Đăng bài (Publishing) | Lên lịch (Schedule) | Workspace Manager | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L403): Cho phép WorkspaceManager | **PASS** |
| 73 | Đăng bài (Publishing) | Lên lịch (Schedule) | Member + Team Manager | ✓ | **Bị chặn 403** | [`ActiveWorkspaceMiddleware.cs:L249, L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L249): `EnsurePermission(membership.Role, ManageSchedules)` kiểm tra cứng `role is Owner or Manager or WorkspaceManager`. Người dùng có workspace role `Member` (int 3) bị chặn 403 tại `/api/content-schedules` (không được rescue qua L123 vì path khác `/api/content`) | **FAIL** |
| 74 | Đăng bài (Publishing) | Lên lịch (Schedule) | Member + Content Creator | ❌ (Cần gửi duyệt) | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L403): Chặn 403 | **PASS** |
| 75 | Đăng bài (Publishing) | Lên lịch (Schedule) | Member + Viewer | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L403): Chặn 403 | **PASS** |
| 76 | Đăng bài (Publishing) | Hủy bài chờ đăng | Owner | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L403): Cho phép Owner | **PASS** |
| 77 | Đăng bài (Publishing) | Hủy bài chờ đăng | Workspace Manager | ✓ | Cho phép | [`ActiveWorkspaceMiddleware.cs:L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L403): Cho phép WorkspaceManager | **PASS** |
| 78 | Đăng bài (Publishing) | Hủy bài chờ đăng | Member + Team Manager | ✓ | **Bị chặn 403** | [`ActiveWorkspaceMiddleware.cs:L249, L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L249): `DELETE /api/content-schedules/{id}` bị chặn 403 do role Workspace là `Member` | **FAIL** |
| 79 | Đăng bài (Publishing) | Hủy bài chờ đăng | Member + Content Creator | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L403): Chặn 403 | **PASS** |
| 80 | Đăng bài (Publishing) | Hủy bài chờ đăng | Member + Viewer | ❌ | Chặn 403 | [`ActiveWorkspaceMiddleware.cs:L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L403): Chặn 403 | **PASS** |
| 81 | Báo cáo & Analytics | Xem Báo cáo Analytics của Brand | Owner | ✓ | Cho phép | [`AisamContext.PermissionScope.cs:L70`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L70): `PermissionOwner` cho phép xem `PerformanceReport` | **PASS** |
| 82 | Báo cáo & Analytics | Xem Báo cáo Analytics của Brand | Workspace Manager | ✓ | Cho phép | [`AisamContext.PermissionScope.cs:L70`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L70): `PermissionWorkspaceManager` cho phép | **PASS** |
| 83 | Báo cáo & Analytics | Xem Báo cáo Analytics của Brand | Member + Team Manager | ✓ | Cho phép | [`AisamContext.PermissionScope.cs:L70`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L70): `PermissionManager` cho phép xem báo cáo các bài thuộc Brand | **PASS** |
| 84 | Báo cáo & Analytics | Xem Báo cáo Analytics của Brand | Member + Content Creator | ✓ | **Bị Query Filter chặn 100% dữ liệu** | [`AisamContext.PermissionScope.cs:L70`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L70): `(PermissionOwner || PermissionWorkspaceManager || PermissionManager)` -> Với Creator, cả 3 cờ đều `false`, EF Query Filter lọc rỗng toàn bộ `PerformanceReport`! Đồng thời [`ResourcePermissionPolicy.cs:L147`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L147): `AnalyticsView => teamManager` chặn Creator | **FAIL** |
| 85 | Báo cáo & Analytics | Xem Báo cáo Analytics của Brand | Member + Viewer | ✓ | **Bị Query Filter chặn 100% dữ liệu** | [`AisamContext.PermissionScope.cs:L70`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L70) & [`ResourcePermissionPolicy.cs:L147`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L147): Viewer bị lọc rỗng 100% `PerformanceReport` và bị chặn quyền `AnalyticsView` | **FAIL** |
| 86 | Báo cáo & Analytics | Xem Báo cáo Hiệu suất Nhân viên | Owner | ✓ | Cho phép toàn Workspace | [`MemberPerformanceService.cs:L55, L74`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/MemberPerformanceService.cs#L55): Cho phép Owner xem toàn Workspace | **PASS** |
| 87 | Báo cáo & Analytics | Xem Báo cáo Hiệu suất Nhân viên | Workspace Manager | ✓ (Toàn Workspace) | **Bị thu hẹp chỉ trong Team tham gia** | [`MemberPerformanceService.cs:L74-80`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/MemberPerformanceService.cs#L74): `if (!owner) ... teamsQuery = teamsQuery.Where(t => actorTeamIds.Contains(t.Id));` -> WorkspaceManager bị ép lọc theo `actorTeamIds`, không được xem toàn Workspace như Owner | **FAIL** |
| 88 | Báo cáo & Analytics | Xem Báo cáo Hiệu suất Nhân viên | Member + Team Manager | ✓ (Chỉ trong Team) | Cho phép trong Team | [`MemberPerformanceService.cs:L76`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/MemberPerformanceService.cs#L76): Ép lọc theo `managedTeamIds` đúng Team mình quản lý | **PASS** |
| 89 | Báo cáo & Analytics | Xem Báo cáo Hiệu suất Nhân viên | Member + Content Creator | ❌ (Chỉ xem cá nhân) | Chỉ xem cá nhân | [`MemberPerformanceService.cs:L68, L87`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/MemberPerformanceService.cs#L68): `if (creator && memberId.HasValue && memberId != actor) throw 404; if (creator && member.UserId != actor) continue;` | **PASS** |
| 90 | Báo cáo & Analytics | Xem Báo cáo Hiệu suất Nhân viên | Member + Viewer | ❌ | Chặn 403 | [`MemberPerformanceService.cs:L67`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/MemberPerformanceService.cs#L67): `if (!owner && !isTeamManager && !creator) throw new PerformanceAccessException(403);` | **PASS** |

---

## 3. CORE PRINCIPLES VERIFICATION (MỤC B)

### 3.1. Decoupled Architecture (Kiến trúc Tách rời Độc lập)
- **Yêu cầu:** Team và Brand là 2 đối tượng độc lập, liên kết duy nhất qua bảng trung gian `team_brands`. Không còn code gán cứng quyền Brand trực tiếp từ `WorkspaceMember`.
- **Kiểm tra thực tế:**
  - Bảng trung gian `TeamBrand` ([`AISAMContext.cs:L401-416`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AISAMContext.cs#L401)) và `TeamChannelAccess` ([`AISAMContext.cs:L357-364`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AISAMContext.cs#L357)) được thiết lập chuẩn hóa.
  - Tầng dịch vụ tính toán quyền thông qua `IEffectivePermissionContext` truy vấn `TeamBrands` + `TeamMembers` ([`PermissionScopeMiddleware.cs:L34-38`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/PermissionScopeMiddleware.cs#L34)), không còn bất kỳ cột `brand_id` nào trong `workspace_members`.
  - **Trạng thái:** **PASS**.

---

### 3.2. Max Privilege Rule (Quy tắc Cộng dồn Quyền cao nhất)
- **Kịch bản Bắt buộc Kiểm tra:** User X thuộc Team A (`Role = Viewer`) VÀ Team B (`Role = Manager`), cả hai Team cùng nối với Brand Y (`team_brands`). Xác nhận quyền thực tế của X trên Brand Y có tính đúng là `Manager` hay không.
- **Dấu vết Thực thi Thực tế (Trace code thật):**
  1. **Bước 1: Nạp phân công tại Middleware:**  
     Tại [`PermissionScopeMiddleware.cs:L34-39`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/PermissionScopeMiddleware.cs#L34), câu lệnh LINQ join `TeamBrands` và `TeamMembers`:
     ```csharp
     var assignments = from b in db.TeamBrands.AsNoTracking()
         join t in db.Teams.AsNoTracking() on b.TeamId equals t.Id
         join member in db.TeamMembers.AsNoTracking() on t.Id equals member.TeamId
         where b.IsActive && t.WorkspaceId == workspace && !t.IsDeleted && t.Status == TeamStatusEnum.Active && member.UserId == actor && member.IsActive
         select new { b.Id, b.BrandId, b.TeamId, member.Permissions, member.Role };
     ```
     Với User X trên Brand Y, kết quả trả về 2 bản ghi:
     - Record 1: `{ BrandId = Brand Y, TeamId = Team A, Role = TeamRoleEnum.Viewer }`
     - Record 2: `{ BrandId = Brand Y, TeamId = Team B, Role = TeamRoleEnum.Manager }`
  2. **Bước 2: Gom nhóm & Tính quyền cao nhất:**  
     Tại [`PermissionScopeMiddleware.cs:L59-64`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/PermissionScopeMiddleware.cs#L59):
     ```csharp
     brandMaxRole = rows
         .GroupBy(r => r.BrandId)
         .ToDictionary(
             g => g.Key,
             g => EffectivePermissionContext.ResolveMaxRole(g.Select(x => x.Role))
         );
     ```
  3. **Bước 3: Thuật toán xếp hạng đặc quyền:**  
     Tại [`EffectivePermissionContext.cs:L76-87`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/EffectivePermissionContext.cs#L76):
     ```csharp
     public static int GetPrivilegeRank(TeamRoleEnum role) => role switch
     {
         TeamRoleEnum.Manager => 3,
         TeamRoleEnum.ContentCreator => 2,
         TeamRoleEnum.Viewer => 1,
         _ => 0
     };
     public static TeamRoleEnum ResolveMaxRole(IEnumerable<TeamRoleEnum> roles) =>
         roles.OrderByDescending(GetPrivilegeRank).FirstOrDefault();
     ```
     Hàm sắp xếp: `Manager (Rank 3) > Viewer (Rank 1)` -> `ResolveMaxRole` trả về chính xác `TeamRoleEnum.Manager`.
  4. **Bước 4: Xác thực qua Test tự động đã chạy:**  
     Test case thực tế [`Phase6TestSuiteAndCiTests.cs:L21-73`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/tests/AISAM.IntegrationTests/Phase6TestSuiteAndCiTests.cs#L21) (`MaxPrivilege_UserInTwoTeamsOnSameBrand_ResolvesToHigherRoleManager`) thiết lập đúng User X thuộc Team 1 (Viewer) và Team 2 (Manager) trên cùng Brand 1. Kết quả kiểm tra:
     - `EffectivePermissionContext.ResolveMaxRole` = `TeamRoleEnum.Manager`.
     - `accessControl.CheckAsync(ApprovalReview)` = `Allowed: true`.
     - `accessControl.CheckAsync(ContentEdit)` = `Allowed: true`.
- **Trạng thái:** **PASS**.

---

### 3.3. Team Data Isolation (Cô lập Dữ liệu Bài viết theo Team)
- **Yêu cầu:** Thành viên Team A không được thấy Content do Team B tạo, dù cả hai Team cùng quản lý chung 1 Brand. Phải truy vết qua EF Core Global Query Filter.
- **Dấu vết Thực thi Thực tế (Trace EF Core):**
  Tại [`AisamContext.PermissionScope.cs:L39-48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L39):
  ```csharp
  m.Entity<Content>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId &&
      (PermissionOwner || PermissionWorkspaceManager || PermissionBrandIds.Contains(c.BrandId)) &&
      (PermissionOwner || PermissionWorkspaceManager || !c.TeamId.HasValue || PermissionTeamIds.Contains(c.TeamId.Value)) &&
      (!PermissionOnlyMyContent || c.PrimaryCreatorId==PermissionActorId) &&
      (!PermissionReviewQueue || PermissionOwner || PermissionWorkspaceManager || PermissionManager || (PermissionCreator && PermissionReviewBrandIds.Contains(c.BrandId))) &&
      (PermissionOwner || PermissionWorkspaceManager || PermissionManager || PermissionManagerBrandIds.Contains(c.BrandId) ||
       (PermissionCreator && (c.PrimaryCreatorId==PermissionActorId || PermissionViewAllBrandIds.Contains(c.BrandId))) ||
       c.Id==PermissionReviewContentId ||
       (PermissionReviewQueue && PermissionCreator && PermissionReviewBrandIds.Contains(c.BrandId) && c.Status==AISAM.Data.Enumeration.ContentStatusEnum.PendingApproval) ||
       (c.Status==AISAM.Data.Enumeration.ContentStatusEnum.Approved || c.Status==AISAM.Data.Enumeration.ContentStatusEnum.Published)));
  ```
  - Khi User thuộc Team A truy vấn, `PermissionTeamIds = [TeamA.Id]`.
  - Mọi bài viết do Team B tạo có `c.TeamId = TeamB.Id`.
  - Điều kiện `PermissionTeamIds.Contains(c.TeamId.Value)` đánh giá thành `false`.
  - EF Core dịch sang mệnh đề SQL: `c.team_id IN ('<TeamA_Guid>')`, loại bỏ hoàn toàn các bản ghi của Team B ngay ở tầng Database query.
  - Test case [`QueryFilterVisibilityTests.cs:L30-81`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/tests/AISAM.IntegrationTests/QueryFilterVisibilityTests.cs#L30) (`QueryFilter_Content_IsolatedByTeam_OnSameBrand`) đã kiểm chứng: Manager của Team A chỉ thấy nội dung Team A, nhận 0 kết quả từ Team B trên cùng Brand.
- **Trạng thái:** **PASS**.

---

## 4. REGRESSION CHECK (ĐỐI CHIẾU LỖI ĐÃ BIẾT — MỤC D)

| # | Lỗi cũ cần kiểm tra | Trạng thái thực tế | Bằng chứng kiểm tra |
|---|---|---|---|
| 1 | **Leo thang đặc quyền xuyên Brand (cờ PermissionManager toàn cục)** | **ĐÃ FIX THẬT** | Tại [`PermissionScopeMiddleware.cs:L74-93`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/PermissionScopeMiddleware.cs#L74), cờ `db.PermissionManager` chỉ được gán `true` khi `requestedBrandId.HasValue` và người dùng có vai trò `Manager` trên chính Brand đó. Các social integration cũng được phân vùng nghiêm ngặt qua `PermissionManagerBrandIds` (L101-105). Test `HF-01` pass. |
| 2 | **Viewer từng bị Query Filter chặn 100% Content** | **ĐÃ FIX THẬT** | Tại [`AisamContext.PermissionScope.cs:L48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L48), đã bổ sung nhánh `(c.Status==ContentStatusEnum.Approved \|\| c.Status==ContentStatusEnum.Published)`. Viewer xem được các bài đã duyệt/xuất bản của Team mình. Test [`QueryFilterVisibilityTests.cs:L13-28`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/tests/AISAM.IntegrationTests/QueryFilterVisibilityTests.cs#L13) pass. |
| 3 | **Content leak giữa các Team cùng 1 Brand** | **ĐÃ FIX THẬT** | Tại [`AisamContext.PermissionScope.cs:L41`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L41), query filter bắt buộc `PermissionTeamIds.Contains(c.TeamId.Value)`. Test [`QueryFilterVisibilityTests.cs:L30-81`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/tests/AISAM.IntegrationTests/QueryFilterVisibilityTests.cs#L30) pass. |
| 4 | **542 content legacy thiếu team_id** | **ĐÃ FIX THẬT** | Query trực tiếp trên database PostgreSQL thực tế: `Total Contents: 549, With team_id: 549, Missing team_id: 0`. **Hiện tại còn 0 bản ghi thiếu `team_id`**. |
| 5 | **Đối chiếu enum WorkspaceMemberRole cũ→mới trong DB** | **CẢNH BÁO: CHƯA MIGRATION TRÊN DB THỰC TẾ** | Query trực tiếp bảng `workspace_members` trên PostgreSQL: `Role 1: 59`, `Role 2: 4`, `Role 3: 4`, `Role 4: 2`. Bảng `_rbac_migration_wm_backup` **chưa tồn tại**. Migration `20260912140000` và script `Phase2LegacyDataMigration.sql` **chưa chạy trên Database Production**. 4 tài khoản role 2 cũ (Manager cũ) vẫn đang ở role 2 và có nguy cơ bị nhận diện thành `WorkspaceManager` nếu khởi chạy backend mới trước khi chạy Phase 8. |

---

## 5. TEST COVERAGE GAP (MỤC E)

Qua rà soát toàn bộ bộ test tích hợp (592 tests backend + 122 tests frontend):

### Các kịch bản ma trận ĐÃ CÓ test bảo vệ:
1. `MaxPrivilege_UserInTwoTeamsOnSameBrand_ResolvesToHigherRoleManager` (`Phase6TestSuiteAndCiTests.cs`)
2. `QueryFilter_Viewer_CanSeeApprovedAndPublishedForOwnTeamOnly` (`QueryFilterVisibilityTests.cs`)
3. `QueryFilter_Content_IsolatedByTeam_OnSameBrand` (`QueryFilterVisibilityTests.cs`)
4. `ApprovalWorkflow_TeamManagerWithWorkspaceMemberRoleMember_CanApproveContentViaMiddleware` (`Phase5DependentModulesTests.cs`)
5. `ActiveWorkspaceMiddleware_ManageBilling_OwnerAllowed_NonOwnerDenied` (`ActiveWorkspaceMiddlewareTests.cs`)

### Các kịch bản ma trận ĐANG THIẾU TEST BẢO VỆ (COVERAGE GAPS):
1. **Thiếu test chặn `Member + Team Manager` gọi `PUT /api/brands/{id}/teams/{id}`** (Chính vì thiếu test này nên lỗ hổng bảo mật số 1 không bị phát hiện lúc build).
2. **Thiếu test xác nhận `Member + Team Manager` có thể Lên lịch bài viết (`POST /api/content-schedules`)** (Nếu có test gọi qua HTTP middleware, bug L249+L403 đã bị phát hiện).
3. **Thiếu test xác nhận `Member + Team Manager` có thể Hủy bài chờ đăng (`DELETE /api/content-schedules/{id}`)**.
4. **Thiếu test xác nhận `Member + Team Manager` có thể Thêm/Sửa Role thành viên trong chính Team mình**.
5. **Thiếu test kiểm tra quyền của `Workspace Manager` khi Mời và Xóa thành viên khỏi Workspace**.
6. **Thiếu test kiểm tra `Workspace Manager` Tạo/Sửa/Xóa Team khi chưa tham gia bất kỳ Team nào**.
7. **Thiếu test kiểm tra `ContentCreator` và `Viewer` xem Báo cáo Analytics của Brand**.
8. **Thiếu test kiểm tra `Workspace Manager` xem Báo cáo Hiệu suất toàn Workspace**.

---

## 6. DANH SÁCH NON-COMPLIANCE CẦN XỬ LÝ (CHỈ LIỆT KÊ)

1. **NC-01 (Lỗ hổng bảo mật — CRITICAL):** [`AssignmentService.cs:L20, L71`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/AssignmentService.cs#L20) cho phép `Member + Team Manager` thực hiện gán/hủy nối Team với Brand (`Active = true/false`). Cần giới hạn hành động này chỉ dành riêng cho `Owner` và `WorkspaceManager`.
2. **NC-02 (Lỗi chức năng — HIGH):** [`ActiveWorkspaceMiddleware.cs:L249, L403`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs#L249) chặn người dùng có Workspace Role `Member` (int 3) thực hiện Lên lịch bài viết và Hủy lịch bài viết (`POST/DELETE /api/content-schedules`), khiến `Member + Team Manager` không thể lên lịch hoặc hủy bài.
3. **NC-03 (Lỗi chức năng — HIGH):** [`WorkspaceInvitationService.cs:L57`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs#L57) và [`WorkspaceMemberService.cs:L215`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/WorkspaceMemberService.cs#L215) kiểm tra cứng `Role != WorkspaceMemberRoleEnum.Owner`, chặn hoàn toàn `Workspace Manager` mời hoặc xóa thành viên khỏi Workspace.
4. **NC-04 (Lỗi chức năng — HIGH):** [`TeamService.cs:L48`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L48) kiểm tra `member.Role != WorkspaceMemberRoleEnum.Manager`, chặn `Member + Team Manager` thêm/sửa vai trò thành viên trong chính Team của mình.
5. **NC-05 (Lỗi chức năng — MEDIUM):** [`TeamService.cs:L51-54, L156`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/TeamService.cs#L51) và [`BrandService.cs:L84-104`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Service/BrandService.cs#L84) yêu cầu `Workspace Manager` phải có delegated permission (`TeamCreate`, `BrandCreate`) hoặc phải là thành viên trong Team mới được quản lý Team/Brand.
6. **NC-06 (Lỗi chức năng — MEDIUM):** [`AisamContext.PermissionScope.cs:L70`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Repositories/AisamContext.PermissionScope.cs#L70) và [`ResourcePermissionPolicy.cs:L147`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/ResourcePermissionPolicy.cs#L147) chặn `ContentCreator` và `Viewer` xem dữ liệu báo cáo `PerformanceReport` của Brand.
7. **NC-07 (Lỗi chức năng — MEDIUM):** [`MemberPerformanceService.cs:L74-80`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Services/Access/MemberPerformanceService.cs#L74) thu hẹp phạm vi xem Báo cáo Hiệu suất của `Workspace Manager` theo các team tham gia (`actorTeamIds`), thay vì cho phép xem toàn Workspace như tài liệu quy định.
8. **NC-08 (Tiêu chuẩn dữ liệu — LOW):** [`WorkspaceMemberRoleEnum.cs:L13-17`](file:///c:/tai%20lieu/AISAM-FINAL/AISAM-BE/AISAM.Data/Enumeration/WorkspaceMemberRoleEnum.cs#L13) vẫn giữ 3 alias `Manager`, `ContentCreator`, `Viewer`. Trên DB Production, các bản ghi `workspace_members` vẫn còn role 2, 3, 4 và cột `team_members.role` vẫn là kiểu chuỗi (chưa apply migration Phase 1 và Phase 2).

---

## 7. OPEN QUESTIONS (ĐIỂM TÀI LIỆU MƠ HỒ)

1. **Q-01: Content Creator và Viewer xem Báo cáo Analytics của Brand ở mức độ nào?**  
   Tài liệu thiết kế đánh dấu **✓** cho cả Content Creator và Viewer ở mục *"Xem Báo cáo Analytics của Brand"*. Tuy nhiên, trong mã nguồn hiện tại, `PerformanceReport` gắn liền với số liệu chi phí quảng cáo (`AdId`), doanh thu, lượt click và bài đăng tổng thể. Tài liệu chưa làm rõ liệu Viewer/Creator có được xem toàn bộ số liệu tài chính/chi phí ads của Brand hay chỉ được xem các chỉ số tương tác bài viết thông thường (reach, engagement, impressions).
2. **Q-02: Cơ chế Delegated Permission (`jsonb permissions` trong `TeamMember`) có còn tồn tại trong kiến trúc mới hay không?**  
   Tài liệu thiết kế kiến trúc chỉ định nghĩa 3 vai trò cố định cấp Team (`MANAGER`, `CONTENT_CREATOR`, `VIEWER`), nhưng trong code vẫn còn hệ thống cờ phân quyền ủy quyền (`TeamCreate`, `BrandCreate`, `ViewAllCreators`, `Review`, `Publish`). Cần xác nhận kiến trúc mới có tiếp tục hỗ trợ các cờ này hay bãi bỏ hoàn toàn để tuân thủ ma trận 3 vai trò thuần túy.

---

## 8. KẾT LUẬN & DỪNG GATE

Báo cáo kiểm định tuân thủ đã hoàn tất.  
**TUYỆT ĐỐI KHÔNG TỰ Ý SỬA CODE HOẶC CHẠY MIGRATION.**  
Đang dừng lại chờ ý kiến chỉ đạo tiếp theo từ Kiet về các Non-Compliance (đặc biệt là lỗi bảo mật **NC-01** và lỗi luồng đăng/lên lịch **NC-02**).
