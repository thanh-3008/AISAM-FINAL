# R07 — Publishing, lịch và automation

Ngày cập nhật: 15/09/2026. Hoàn thành backend và bộ kiểm thử cục bộ của R07. Chưa bật RBAC v2 trên ứng dụng, chưa áp dụng migration mới hoặc nghiệm thu provider thật.

## Đã thực hiện

### Lịch và xuất bản

- Filter kiểm tra lịch theo Content và integration cụ thể, không suy ra quyền đăng từ Brand.
- Create/bulk create/update/delete lịch ở v2 kiểm tra actor hiện hành, Team và kênh bằng resolver. Role Owner legacy không cấp quyền trong v2. Đổi kênh phải có quyền với kênh mới.
- Lịch Processing không được sửa/xóa. Status của ContentCalendar là concurrency token, để thay đổi dựa trên trạng thái cũ không ghi đè một lần nhận job đồng thời.
- Dùng lại cơ chế claim nguyên tử và publish operation/idempotency hiện có; không tự gửi lại operation có kết quả bên ngoài chưa xác định. Quyền được đọc lại trước khi publish và tại callback Publishing.
- Snapshot của lịch phải trùng snapshot được duyệt hiện hành. Adapter kiểm tra lại snapshot thực thi với dữ liệu database, kể cả khi DbContext đã giữ một Content cũ.
- Cho phép đăng tiếp nội dung Published sang kênh khác khi snapshot vẫn thuộc phiên bản được duyệt hiện hành. Đây là phần làm rõ contract: Published không có nghĩa phải duyệt lại riêng cho từng kênh; Draft hoặc snapshot cũ vẫn bị từ chối.

### Automation

- Thêm `AutomationItem.TeamId`, DTO import/update/response, CSV TeamId, clone và truyền Team sang Content được sinh.
- Khi v2 bật, mỗi dòng tạo plan phải có Team/Brand được phép. Không suy ra Team từ Brand. Không cho sửa Team của item đã có Content.
- Scope plan yêu cầu người dùng có quyền trên tất cả Team/Brand của các item: TeamManager trong phạm vi quản lý hoặc Creator với plan mình tạo. Owner/WorkspaceManager nhìn toàn workspace. Scope này tránh lộ các item khác qua chi tiết/tổng hợp plan.
- Worker dùng người tạo plan làm actor, kiểm tra quyền tạo/sửa trước khi sinh và sau khi provider trả dữ liệu. Không có actor, bị thu hồi scope hoặc bài đã ra khỏi trạng thái có thể sửa thì dừng để xử lý quyền.
- Sinh xong chuyển Content sang PendingApproval, lưu payload trước khi tạo snapshot; không chỉ đổi trạng thái của AutomationItem.
- Review kiểm tra quyền duyệt trước, lưu Approved rồi mới kiểm tra quyền schedule. Không còn yêu cầu bài PendingApproval đồng thời thỏa điều kiện Publish. Retry phần schedule của bài đã Approved vẫn kiểm tra quyền kênh.
- Auto-approve chỉ bật nếu actor mà worker sử dụng có quyền quản lý toàn bộ Team của plan. Creator không được dùng auto-approve để tự duyệt.
- Ghi Approval bằng Add rõ ràng trong DbContext; áp dụng cả submit/approve/reject/withdraw thường để tránh EF coi lịch sử mới là bản ghi cập nhật.

### Video

- Polling video gắn Content dùng PrimaryCreatorId, không mượn Owner từ Profile/workspace trong v2. Kiểm tra quyền sửa trước khi polling/đồng bộ; AIService kiểm tra lại trước khi ghi video vào bài.
- Job video độc lập hiện không có Content/Team: v2 chỉ cho Owner/WorkspaceManager thực hiện khi workspace còn cho phép ghi. Member dùng luồng video gắn với Content có Team. Worker dừng job độc lập khi quyền bị thu hồi.

## Migration

`20260914120000_AddAutomationTeamScope` thêm cột nullable `automation_items.team_id`. Chỉ backfill từ Content hiện có nếu cùng workspace và Brand. Item chưa có nguồn Team đáng tin giữ null và không được worker v2 tự nhận vào Team bất kỳ.

Migration chưa áp dụng lên database ứng dụng. Cần áp dụng theo thứ tự R01 rồi migration này trước khi chạy binary mới trên database đó. Snapshot EF đã cập nhật. R11 kiểm tra thực thi migration/rollback trên bản sao PostgreSQL; không coi việc build thành công là bằng chứng đã chạy migration.

## Kiểm thử

132 kiểm thử đạt, 0 lỗi, 0 bỏ qua, với bộ lọc:

```powershell
& C:\Users\thanh\.dotnet\dotnet.exe test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj --filter 'FullyQualifiedName~Automation|FullyQualifiedName~RbacV2Tests|FullyQualifiedName~ContentScheduleServiceTests|FullyQualifiedName~ScheduledPostingServiceTests|FullyQualifiedName~PublishOperation|FullyQualifiedName~ContentServiceTests|FullyQualifiedName~ResourcePermissionFilterTests|FullyQualifiedName~AIService|FullyQualifiedName~Video' --verbosity quiet
```

Kiểm thử mới bao gồm Viewer mang Owner legacy bị chặn sửa lịch, thu hồi grant sau khi đã thao tác thành công, automation yêu cầu/lưu TeamId, Creator không tự duyệt, generator chuyển bài sang PendingApproval và có snapshot, Published dùng cùng approval, execution snapshot sai bị từ chối và quyền job video độc lập.

`git diff --check` đạt. Kiểm thử nghiệp vụ chủ yếu dùng InMemory/fake provider; không gọi OAuth, đăng bài, thanh toán hoặc phát sinh phí AI thật.

## Phần nghiệm thu sau

- R09 bổ sung giao diện Team cho automation/import và thể hiện lỗi quyền rõ ràng.
- R11 kiểm tra PostgreSQL cạnh tranh thực tế: claim lịch với sửa/hủy, thu hồi duyệt với worker, retry nhiều tiến trình, migration/backfill/rollback. Concurrency token và kiểm tra snapshot đã có mã, chưa phải chứng nhận chạy đồng thời trên PostgreSQL.
- R12 nghiệm thu sandbox provider. Việc thu hồi quyền sau khi request đã được provider nhận không tự thu hồi bài đã đăng; kết quả không rõ phải đối soát, không retry mù.
