# R02 — Bộ kiểm tra quyền tập trung

Ngày cập nhật: 14/09/2026. Hoàn thành nền tảng resolver/policy, adapter và tích hợp chế độ v2; chưa bật trên môi trường ứng dụng.

## Đầu ra đã kiểm tra

- [x] RbacV2Policy: phân biệt WorkspaceRole/TeamRole, billing read/manage, trạng thái workspace/nội dung, tác giả, kênh và hiệu suất cá nhân.
- [x] RbacV2AccessResolver đọc membership và scope hiện hành từ DB, không fallback role hoặc permission JSON legacy.
- [x] Content lấy Team từ bản ghi, không cho client thay Team để lấy quyền cao hơn. Publish cần kênh cấp cho chính Team sở hữu Content.
- [x] VisibleContentsAsync và query filter v2 lọc tại database; Viewer chỉ Approved/Published, Manager Team A không mở bản nháp B.
- [x] Context có contractVersion, revision, workspaceRole, teams, scopes và capabilities cấp workspace. Các action tài nguyên vẫn kiểm tra theo đối tượng/trạng thái, không cấp từ capabilities hiển thị.
- [x] Adapter IAccessControlService ánh xạ lời gọi sang v2; thiếu TeamId khi tạo nội dung thì từ chối thay vì đoán.
- [x] Middleware chọn nhánh v2, không gán cờ Manager toàn request; chặn thay Team/Brand/workspace/tác giả qua sửa Content.
- [x] Nhánh v2 yêu cầu header contract 2, chặn role không hợp lệ và không OR với quyết định legacy.
- [x] Kiểm thử resolver, adapter, middleware, PostgreSQL query translation và hồi quy liên quan đạt.

## Kiểm thử

`dotnet test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj --filter "FullyQualifiedName~RbacV2Tests|FullyQualifiedName~PermissionScopeMiddlewareTests|FullyQualifiedName~ActiveWorkspaceMiddlewareTests|FullyQualifiedName~ResourcePermissionsControllerTests|FullyQualifiedName~PermissionQueryScopeTests" --verbosity quiet`

Kết quả: **56 passed, 0 failed**. Có warning sẵn có CS9113 trong TeamService (tham số access chưa dùng). git diff --check đạt, chỉ cảnh báo đổi kiểu xuống dòng Windows.

Ca quan trọng: legacy Owner nhưng v2 Member không thành admin; Manager A/Viewer B cùng Brand; grant B không dùng đăng bài A; permission Publish legacy không nâng Viewer; thu hồi grant được đọc lại qua context DB khác; membership inactive đóng scope; revision thay đổi; scope reset sau middleware. Kiểm tra SQL là kiểm tra dịch query PostgreSQL; các ca resolver/middleware dùng EF InMemory, không chứng nhận race giao dịch thực tế.

## Điều kiện sử dụng và công việc kế tiếp

- Server chọn v2 bằng cấu hình `Rbac:UseV2=true` (`Rbac__UseV2` qua environment); mặc định vẫn legacy. Không bật theo header client. Khi chọn v2 chỉ dùng adapter v2, không hợp quyền cũ/mới.
- Chưa thay .env, chưa bật v2 hoặc áp dụng migration lên database ứng dụng.
- R03–R08 phải cập nhật từng API nghiệp vụ: nhân sự, Team, Brand/kênh, workflow Content, jobs và analytics. Các kiểm tra role legacy bên trong những service này vẫn cần thay ở task tương ứng; chưa thể coi toàn ứng dụng sẵn sàng bật v2.
- R09/R10 phải gửi contract header, TeamId và sử dụng context mới. Client cũ trong chế độ v2 nhận RBAC_CLIENT_UPDATE_REQUIRED.
- Các truy vấn bypass query filters phải gọi resolver trước trả dữ liệu; R06–R08 rà soát các đường export/assets/analytics/jobs. Không coi bộ query filter là bảo đảm mọi truy vấn trực tiếp đã an toàn.
- Member quản trị Team hiện bị chặn ở cổng route rộng; R04 thay bằng kiểm tra action cụ thể cho TeamManager sửa C/V. Đây chưa phải luồng quản lý Team hoàn chỉnh.

**Phạm vi hoàn thành là nền tảng R02.** Rollout và chứng nhận toàn bộ đường truy cập thuộc R11/R12 sau các task nghiệp vụ; cấu hình v2 hiện dành kiểm thử phát triển.
