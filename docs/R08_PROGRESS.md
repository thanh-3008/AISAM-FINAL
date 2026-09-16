# R08 — Analytics và hiệu suất thành viên

Ngày cập nhật: 15/09/2026. Hoàn thành backend và kiểm thử cục bộ; RBAC v2 chưa bật trên ứng dụng. UI được đồng bộ ở R09.

## Quy tắc và thay đổi

- Hiệu suất dùng WorkspaceRoleV2 và TeamRole hiện hành. Owner/WorkspaceManager xem toàn workspace, kể cả Brand chưa gán Team. Không dùng Owner legacy để mở quyền v2.
- TeamManager xem thành viên trong Team được quản lý. Creator xem KPI bản thân trong Team có vai trò Creator. Viewer không được vào báo cáo KPI thành viên.
- Truy vấn nội dung cho từng dòng báo cáo kiểm tra TeamId, TeamBrand còn hiệu lực, membership của actor và thành viên đích trước khi tính số lượng, tỷ lệ, lịch và thời gian duyệt. Một người Manager ở A, Viewer ở B hoặc Creator ở C không được cộng KPI của người khác ở B/C dù cùng Brand.
- Bộ lọc Brand/Team/thành viên và Total dựa trên phạm vi được phép. Truy vấn chỉ định Team hoặc thành viên ngoài phạm vi trả lỗi, không trả dữ liệu tổng workspace.
- Bài đăng và insights kiểm tra grant kênh của đúng Team chứa Content. Grant của Team A không mở bài thuộc Team B. Query filter Post được các truy vấn analytics/dashboard dùng lại; PerformanceReport tiếp tục phụ thuộc Post/Ad được phép.
- Analytics bài theo contract R00: Creator có thể xem analytics các bài trong Team được cấp; quy tắc chỉ bản thân áp dụng cho KPI thành viên. Viewer chỉ đọc analytics bài Approved/Published được phép xem.
- Dashboard tài chính `/api/workspace-dashboard/summary` chứa ví credit, quota và xếp hạng chi tiêu toàn workspace: v2 chỉ cho Owner/WorkspaceManager, kiểm tra trước khi đọc ví hoặc danh sách thành viên. Dashboard nội dung `/api/dashboard` vẫn dùng phạm vi nội dung của người dùng; không trả số 0 giả để thay dữ liệu tài chính bị cấm.
- Dữ liệu quảng cáo không có TeamId vẫn giữ phạm vi quản trị workspace; không tự chia số liệu campaign theo Brand khi nhiều Team dùng chung Brand.

## Xuất dữ liệu

Thêm `GET /api/team/member-performance/export`, nhận `from`, `to`, `brandId`, `teamId`, `memberId` giống báo cáo. Trả file JSON gồm số liệu và định nghĩa metric, sử dụng cùng service/phân quyền cho từng trang dữ liệu; giới hạn 10.000 thành viên mỗi lần xuất. Không tạo truy vấn bỏ qua scope riêng cho export. Các endpoint export quản trị hiện có vẫn thuộc phân quyền admin.

## Kiểm chứng

39 kiểm thử đạt, không lỗi, không bỏ qua:

```powershell
& C:\Users\thanh\.dotnet\dotnet.exe test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj --filter 'FullyQualifiedName~MemberPerformanceTests|FullyQualifiedName~AnalyticsServiceTests|FullyQualifiedName~PerformanceReportRepositoryTests|FullyQualifiedName~Dashboard|FullyQualifiedName~RbacV2Tests|FullyQualifiedName~ResourcePermissionFilterTests|FullyQualifiedName~ContentMediaTests' --verbosity quiet
```

Các kiểm thử mới kiểm tra:

- Hai Team dùng chung Brand, actor có vai trò khác nhau, không cộng nhầm KPI.
- Creator chỉ nhận dòng của mình, Viewer bị từ chối.
- WorkspaceManager xem được Brand chưa có Team; membership bị thu hồi bị chặn.
- Export trả đúng số liệu đã lọc; dashboard tài chính chặn Member trước khi gọi dependency tài chính.
- Grant kênh đúng Team ở query Post, không dùng union kênh giữa các Team; sinh SQL PostgreSQL thành công.
- Bộ fixture legacy chọn đúng TeamBrand cần thu hồi thay vì giả định database chỉ có một assignment.

## Giới hạn nghiệm thu

Kiểm thử dùng InMemory/fake và kiểm tra sinh SQL; chưa chứng nhận hiệu năng, snapshot dữ liệu nhất quán qua nhiều trang export hoặc chạy đồng thời trên PostgreSQL. R11 kiểm tra các điểm này trên bản sao database. Không gọi provider lấy insights thật trong task. Không có migration mới ở R08; các migration R01/R07 vẫn cần được áp dụng trước khi chạy phiên bản mã nguồn mới trên database ứng dụng.
