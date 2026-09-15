# Kế hoạch triển khai phân quyền hai tầng Workspace – Team

Ngày lập: **14/09/2026**.

Nguồn yêu cầu ban đầu: `TÀI LIỆU THIẾT KẾ KIẾN TRÚC.docx` do người dùng cung cấp, đối chiếu với mã nguồn và backlog `docs/TEAM_BRAND_NEXT_TASKS.md`.

Bổ sung: `Untitled document (1).docx` cung cấp quyết định D01–D09 và phạm vi kênh theo Team. Nội dung được cập nhật vào kế hoạch; câu yêu cầu triển khai trong tài liệu không được coi là yêu cầu chạy migration/code trong lượt đọc này.

**Trạng thái: lập kế hoạch, chưa triển khai các thay đổi trong tài liệu này.** Checkbox chỉ được đánh dấu khi hoàn thành phạm vi và kiểm tra tương ứng. Các phần có mã nguồn từ trước không đồng nghĩa đã đạt thiết kế mới.

## 1. Mục tiêu và thay đổi chính

| Thành phần | Hiện trạng đã đối chiếu | Thiết kế cần triển khai |
|---|---|---|
| Vai trò workspace | `WorkspaceMemberRoleEnum`: Owner, Manager, ContentCreator, Viewer | Owner, WorkspaceManager, Member; tách quyền tổ chức khỏi quyền nghiệp vụ |
| Vai trò Team | Đã có `TeamMember.Role` và `TeamRoleEnum`: Manager, ContentCreator, Viewer | Kế thừa, đưa vai trò Team vào toàn bộ quyết định quyền theo tài nguyên |
| Team – Brand | Có API xem Team và gán/gỡ Team, kênh theo Brand trong `ResourceAssignmentsController` | Hoàn thiện quản lý Team thật, thành viên và liên kết; Team và Brand độc lập |
| Nội dung | Đã có `Content.TeamId` nullable và `PrimaryCreatorId` | Cách ly theo Team, kiểm tra tác giả khi sửa/xóa, xử lý dữ liệu cũ thiếu Team |
| Manager | Backlog cũ đề xuất Owner cấp riêng quyền tạo Team/Brand cho Manager | Thiết kế mới dành tạo Team/Brand cho Owner hoặc WorkspaceManager; TeamManager không tự có quyền này |
| Nghiệm thu | Đã có kế hoạch Permission/Publishing và tiến độ T12 | Bổ sung nghiệm thu thiết kế hai tầng; không dùng kết quả cũ để chứng nhận thiết kế mới |

Các điểm vào cần rà soát: entity/enum và migrations; `AccessControlService`; middleware workspace/permission; controller và service; query filter; worker đăng bài; `teamService.ts`, màn Team/Brand và permission context; module workspace/team trên mobile nếu có.

## 2. Quy tắc nghiệp vụ đích

- **Owner:** toàn workspace, quản lý sở hữu, billing và vòng đời workspace.
- **WorkspaceManager:** quản lý nhân sự, Team, Brand và nghiệp vụ toàn workspace; không nâng gói/thanh toán, không đổi Owner hoặc xóa/đổi tên workspace.
- **Member:** quyền nghiệp vụ lấy từ membership Team đang hoạt động; không tự có quyền quản trị workspace.
- **TeamManager:** vận hành trong Team được quản lý: duyệt, đăng, lên lịch, sử dụng kênh được cấp (không kết nối/ngắt tài khoản), hiệu suất Team.
- **ContentCreator:** tạo/upload/gửi duyệt trong Team; sửa/xóa nội dung mình tạo theo trạng thái cho phép.
- **Viewer:** xem Brand được cấp, lịch và nội dung đã duyệt; analytics theo phạm vi được phép; không chỉnh sửa.
- Mời vào workspace không tự cấp quyền vào mọi Team/Brand. Không công khai danh mục toàn bộ Brand cho mọi Member trong phạm vi kế hoạch này.
- Thao tác tạo Brand của TeamManager trong backlog cũ được thay bằng quy tắc mới: cần vai trò WorkspaceManager hoặc Owner.

## 3. Quyết định D01–D09 từ tài liệu bổ sung

Các quyết định dưới đây thay thế đề xuất tương ứng của bản kế hoạch đầu. R00 đã hoàn thành thiết kế: xem [hợp đồng R00](docs/R00_RBAC_CONTRACT.md) cho ma trận action/resource, ánh xạ dữ liệu cũ, API/DTO và mã lỗi. Các đề xuất kỹ thuật dưới đây được chốt chi tiết trong hợp đồng đó. Ghi chú kỹ thuật là đề xuất triển khai, không phải trích nguyên văn tài liệu.

| ID | Quyết định trong tài liệu mới | Ảnh hưởng triển khai |
|---|---|---|
| D01 | WorkspaceManager có `billing.read`, không có `billing.manage` | Cho xem hóa đơn/gói; chặn thanh toán/nâng gói cả UI và API |
| D02 | Max privilege ở Brand; quyền Content chỉ theo `Content.TeamId` | Không lan quyền Manager từ Team A sang bài Team B. “Quản lý chung Brand” không ghi đè quy tắc Brand CRUD dành Owner/WorkspaceManager |
| D03 | Viewer chỉ thấy APPROVED/PUBLISHED qua lọc query backend | Áp dụng theo role của từng Team, không dùng một cờ Viewer toàn user; đối chiếu enum trạng thái thực tế khi triển khai |
| D04 | TeamManager chỉ đổi Creator ↔ Viewer trong Team mình | Owner/WorkspaceManager mới bổ nhiệm hoặc gỡ TeamManager; không được tự nâng vai trò workspace |
| D05 | Bài mới bắt buộc TeamId hợp lệ; bài cũ thiếu TeamId backfill vào Default Team của Brand | Backfill theo từng Brand, chạy lại không tạo Team trùng; giữ tác giả/lịch sử. Không tự đưa mọi thành viên vào Default Team |
| D06 | Hiệu suất bài lọc theo TeamId; tổng chỉ số provider cấp kênh dành Owner/WorkspaceManager hoặc TeamManager có Brand liên kết | Tách chỉ số bài và chỉ số kênh; kết hợp scope kênh bổ sung bên dưới |
| D07 | Chỉ Owner/WorkspaceManager kết nối/ngắt tài khoản Social | TeamManager chỉ dùng kênh đã nối và được cấp; chặn cả endpoint OAuth và disconnect |
| D08 | Bỏ hệ thống permission lẻ cũ, chuyển sang cặp WorkspaceRole/TeamRole | Không cộng grant cũ để vượt role mới; scope tài nguyên Team–Brand–Channel vẫn bắt buộc |
| D09 | Soft delete Team bằng trạng thái DEACTIVATED, bảo toàn Content, thu hồi TeamBrand ngay | Vô hiệu hóa liên kết để giữ audit; job chờ kiểm tra lại, không đăng sau thu hồi |

### 3.1. Bổ sung phạm vi kênh theo Team

Tài liệu yêu cầu quan hệ `TeamBrandChannel (TeamId, BrandId, SocialChannelId)`: cùng Brand có 5 kênh, Team A được dùng 2, Team B dùng 3 còn lại.

- Mã nguồn đã có `TeamChannelAccess(TeamBrandId, IntegrationId, CanView, CanPublish, CanManage)` và `TeamBrand(TeamId, BrandId)`. R01 cần đối chiếu để kế thừa/chuyển đổi, không tạo bảng trùng chức năng chỉ vì tên khác.
- R00 phải xác định `IntegrationId` có đúng đơn vị kênh đăng (Page/account) mà tài liệu gọi SocialChannelId hay cần tham chiếu target khác.
- Quyền hành động lấy từ role; quan hệ kênh giới hạn tài nguyên. Bỏ permission lẻ không đồng nghĩa bỏ phạm vi kênh.
- Đề xuất mặc định: không có kênh được gán thì không được đăng; thêm kênh mới không tự cấp cho mọi Team. Owner/WorkspaceManager quản lý danh sách cấp kênh.
- Đề xuất D06 kết hợp phạm vi mới: TeamManager chỉ xem tổng provider của kênh được cấp trong Brand liên kết, không xem mọi kênh của Brand.
- Kiểm tra Team, Brand, kênh cùng workspace và đúng liên kết; quyền đăng cần role phù hợp trong Team sở hữu bài và kênh cấp cho chính Team đó.

### 3.2. Điều kiện backfill Default Team

Đề xuất kỹ thuật: mỗi Brand có một Default Team được xác định ổn định, không tìm chỉ theo tên. Không tự sao chép membership của các Team đang phụ trách Brand vào Team mặc định vì sẽ trộn quyền đọc lịch sử. Owner/WorkspaceManager phân công người sau khi xem báo cáo. Content thiếu/sai Brand phải vào báo cáo ngoại lệ để xử lý, không gán ngẫu nhiên. Đây là phần cần đặc tả thêm trong R00/R01.

## 4. Bảng task lớn

P0: nền tảng và an toàn dữ liệu; P1: luồng người dùng; P2: đồng bộ và bàn giao. Thứ tự không phải cam kết thời gian.

| Xong | Task | Ưu tiên | Phạm vi và đầu ra | Phụ thuộc | Tiêu chí hoàn thành |
|---|---|---|---|---|---|
| [x] | **R00 — Chốt hợp đồng phân quyền** | P0 | Ma trận action/resource/scope, quyết định D01–D09, bản đồ quyền cũ → mới, API/DTO và mã lỗi | — | Không còn mâu thuẫn nghiệp vụ cho các task sau; có ví dụ nhiều Team cùng Brand |
| [x] | **R01 — Mô hình và migration** | P0 | Workspace roles mới, kế thừa TeamRole, ràng buộc membership/liên kết/kênh, Default Team theo Brand, migration và báo cáo backfill | R00 | Chạy được trên DB mới và bản sao DB cũ; không tự nâng Manager cũ thành WorkspaceManager; giữ dữ liệu lịch sử |
| [x] | **R02 — Bộ kiểm tra quyền tập trung** | P0 | Resolver hai tầng, permission context, scope query, đồng bộ middleware/controller/service | R01 | Backend kiểm tra đúng action và Team của tài nguyên; chặn truy cập chéo workspace/Team và quyền cũ còn cache |
| [x] | **R03 — Nhân sự workspace** | P1 | Mời, chấp nhận, xem thành viên, gỡ thành viên, đổi workspace role | R02 | Owner/WorkspaceManager quản lý đúng phạm vi; Member không tự nâng quyền; bảo vệ Owner cuối cùng và thu hồi quyền Team khi rời workspace |
| [x] | **R04 — Team CRUD và thành viên** | P1 | API lưu Team thật, sửa/lưu trữ, thêm/gỡ người và đổi TeamRole | R03 | Tạo → reload còn dữ liệu; kiểm tra cùng workspace; TeamManager không quản lý Team khác hoặc cấp quyền vượt trần |
| [x] | **R05 — Brand và kênh theo Team** | P1 | Brand CRUD, liên kết nhiều Team, chọn tập kênh cho từng Team, connect/disconnect chỉ Owner/WManager, thu hồi liên kết | R04 | Owner/WorkspaceManager tạo Brand; Member chỉ thấy Brand được cấp; tháo liên kết thu hồi quyền và không làm mất lịch sử |
| [x] | **R06 — Cách ly nội dung và duyệt** | P0 | List/detail/create/update/delete/assets/search/export/approval theo TeamId và tác giả | R05 | Hai Team cùng Brand không đọc chéo nội dung; Creator chỉ sửa/xóa bài mình theo trạng thái; Viewer không xem bản nháp |
| [x] | **R07 — Publishing, lịch và automation** | P0 | Đăng ngay, lên lịch/hủy, job/video/automation, kiểm tra actor và quyền tại thời điểm thực thi | R06 | Job không đăng khi quyền/kênh/Team bị thu hồi; không đăng trùng khi retry; không có đường bỏ qua quyền qua worker |
| [x] | **R08 — Analytics và hiệu suất** | P1 | Dashboard, lịch, thống kê Brand, hiệu suất thành viên và dữ liệu xuất | R06 | Owner/WManager toàn workspace; TeamManager đúng Team; Creator bản thân; Viewer không truy cập KPI thành viên; không lộ số liệu qua tổng đếm |
| [x] | **R09 — Giao diện Web trọn luồng** | P1 | Team Management, form thành viên/Brand và chọn kênh từng Team, chọn Team tạo bài, hiển thị workspace role khác TeamRole, thông báo quyền | R03–R08 | Tạo Team → thêm người → gán Brand → thành viên thao tác đúng quyền; bỏ hàm giả lập; lỗi 403 hiện thông báo rõ trong giao diện, không lặp request vô hạn |
| [x] | **R10 — Đồng bộ Mobile** | P2 | DTO, giải mã role, permission context, màn/luồng tương ứng hiện có và xử lý từ chối | R02–R08 | Client không suy diễn enum cũ thành quyền mới; chọn workspace và thao tác nội dung đúng scope; phần chưa hỗ trợ được ghi rõ |
| [x] | **R11 — Hồi quy và kiểm tra migration** | P0 | Test quyền, integration/API, Web E2E, kiểm tra dữ liệu và tính tương thích | R01–R10 | Bộ test trọng yếu ở mục 6 đạt; có báo cáo lỗi còn lại, không đánh dấu hoàn thành nếu còn lỗi vượt quyền |
| [ ] | **R12 — Triển khai và nghiệm thu** | P2 | Runbook, backup/rollback, cập nhật tài liệu, smoke test staging và sandbox | R11 | Migration/health/smoke đạt trên môi trường đích; luồng tích hợp có bằng chứng hoặc được ghi rõ chưa nghiệm thu, không ghi 100% khi chưa chạy |

## 5. Checklist triển khai quan trọng

### R01 — Dữ liệu và tương thích

- [ ] Không đổi ý nghĩa các số enum cũ bằng cách thay tên trực tiếp.
- [ ] Giữ Owner hiện có; lập báo cáo từng membership còn lại trước chuyển đổi, không tự cấp quyền quản trị toàn workspace.
- [ ] Vai trò nghiệp vụ cũ được đối chiếu Team thực tế; người chưa có Team không được cấp đại vào mọi Brand.
- [ ] Tận dụng `TeamMember.Role`, `Content.TeamId`, `PrimaryCreatorId`; chỉ thêm schema còn thiếu sau khảo sát.
- [ ] Phát hiện membership trùng, khác workspace, TeamBrand mồ côi, Content không xác định Team; xuất báo cáo để xử lý.
- [ ] Backfill có thể chạy lại an toàn; có bước kiểm tra trước/sau và bản sao lưu phục hồi.
- [ ] Định nghĩa tương thích JWT/client cũ: đọc quyền từ dữ liệu hiện hành, refresh/invalidate khi cần; không tin role cũ trong token để cấp quyền mới.

### R02–R08 — Thực thi quyền

- [ ] Owner/WorkspaceManager vẫn bị ràng buộc workspace hiện hành; không vượt tenant boundary.
- [ ] Không dùng một cờ “Manager” toàn request để cấp quyền cho mọi Team.
- [ ] Kiểm tra đồng thời membership hoạt động, liên kết Team–Brand, action, trạng thái nội dung và quyền kênh.
- [ ] TeamId/BrandId từ client phải được backend xác minh, không coi header/DTO là bằng chứng được cấp quyền.
- [ ] Quyền hiển thị, quyền sửa và quyền phân công là các quyền riêng.
- [ ] Sửa/gỡ quyền có audit; request và job sau đó dùng quyền mới; xử lý cache/revision nhất quán.
- [ ] Tác vụ nền có ngữ cảnh quyền rõ ràng, không vô tình chạy bằng scope toàn hệ thống để bỏ qua thu hồi quyền.
- [ ] Kiểm tra các đường truy cập gián tiếp: asset, nội dung hội thoại liên quan, tải file, lịch, thông báo, tổng số, export và thống kê.

### R09 — Luồng Web cần giao

1. Owner/WorkspaceManager mời người vào workspace bằng vai trò phù hợp.
2. Người dùng chấp nhận lời mời và trở thành thành viên hoạt động.
3. Owner/WorkspaceManager tạo Team, chọn thành viên và vai trò Team.
4. Owner/WorkspaceManager tạo hoặc chọn Brand, gán cho một/nhiều Team.
5. Owner/WorkspaceManager nối tài khoản và chọn kênh cấp cho từng Team; TeamManager sử dụng các kênh được cấp.
6. Creator chọn Team/Brand hợp lệ → tạo bài → gửi duyệt.
7. TeamManager duyệt → đăng hoặc lên lịch; Viewer chỉ thấy nội dung được phép.
8. Khi bị thu hồi quyền, giao diện cập nhật và thông báo; không tiếp tục giữ dữ liệu nhạy cảm trong màn đang mở.

Nút và menu dựa vào permission context của backend. Không báo thành công trước khi lưu API thành công; lỗi lưu nhiều bước phải có cách tiếp tục/sửa và không để UI hiểu nhầm đã cấu hình đầy đủ.

## 6. Bộ tình huống nghiệm thu bắt buộc

| Tình huống | Kết quả mong đợi |
|---|---|
| Member mới được mời, chưa vào Team | Không thấy dữ liệu Brand/Content ngoài quyền được cấp |
| Manager Team A, Viewer Team B, cả hai cùng Brand | Duyệt bài A được; duyệt bài B bị từ chối; xem B chỉ theo quyền Viewer |
| Hai Team cùng Brand, user chỉ thuộc A | Không đọc bài B bằng URL, API, search, asset hoặc export |
| Creator cùng Team | Xem theo policy Team; chỉ sửa/xóa bài bản thân và đúng trạng thái |
| Viewer gọi thẳng API bản nháp | Bị từ chối hoặc không xuất hiện trong kết quả; không lộ chi tiết nội dung |
| TeamManager tạo Team/Brand hoặc đổi workspace role | Bị từ chối |
| WorkspaceManager thanh toán, chuyển Owner, xóa workspace | Bị từ chối; vẫn xem được hóa đơn/gói theo D01 |
| Gỡ thành viên/gỡ TeamBrand khi có lịch chờ | Quyền bị thu hồi; job không xuất bản trái quyền, có trạng thái/lý do rõ |
| Đổi workspace trong khi request cũ đang chạy | Không hiển thị kết quả workspace cũ hoặc dùng nhầm scope |
| Content cũ không TeamId | Được backfill vào Default Team đúng Brand; không tự cấp membership; trường hợp thiếu Brand có báo cáo ngoại lệ |
| Client/token chứa enum cũ hoặc không nhận biết role mới | Không được tự nâng quyền; yêu cầu cập nhật/refresh phù hợp |
| API bị từ chối quyền | Thông báo phù hợp trong giao diện, không vòng lặp request, không tự đăng xuất vì 403 |
| Analytics/hiệu suất của Team khác | Không truy cập được cả bảng chi tiết, tổng hợp và file xuất |
| Migration chạy lại hoặc rollback | Không nhân bản membership, mất nội dung hoặc cấp quyền rộng hơn |

Test cục bộ được viết/chạy cùng từng task. Có thể gom nghiệm thu thủ công toàn hệ thống vào cuối theo mong muốn người dùng; không hoãn kiểm tra chống vượt quyền tới sau triển khai.

### Kiểm tra bổ sung theo tài liệu mới

- [ ] Brand có 5 kênh: A chỉ đăng lên 2 kênh được cấp, B chỉ đăng lên 3 kênh được cấp; sửa payload không vượt scope.
- [ ] Manager A đồng thời Viewer B không dùng vai trò A để đăng bài B hoặc xem bản nháp B.
- [ ] TeamManager bị chặn connect/disconnect kể cả còn grant CanManage cũ.
- [ ] Thu hồi một kênh sau khi đặt lịch: worker từ chối đăng trên kênh đó.
- [ ] Backfill chạy hai lần: không tạo trùng Default Team, không thêm membership hoặc thay TeamId đã có.
- [ ] Team DEACTIVATED: liên kết hết hiệu lực ngay; lịch sử còn, cache và job không giữ quyền cũ.
- [ ] Grant lẻ cũ cho Creator duyệt/đăng không còn có hiệu lực sau chuyển đổi.
- [ ] Tổng provider cấp kênh không xuất hiện cho Creator/Viewer hoặc TeamManager ngoài scope kênh.

## 7. Thứ tự và cách đánh dấu tiến độ

Thứ tự chính: **R00 → R01 → R02 → R03 → R04 → R05 → R06 → R07/R08 → R09/R10 → R11 → R12**.

- Hoàn thành task lớn khi đủ đầu ra và tiêu chí, ghi ngày, file thay đổi và lệnh/kết quả kiểm tra dưới task tương ứng.
- Phân biệt ba trạng thái: hoàn thành mã nguồn, kiểm thử cục bộ đạt, nghiệm thu môi trường thật đạt.
- Không tự động sửa checkbox T00–T12 cũ để biểu thị hoàn thành thiết kế này.
- R12 cần môi trường được phép thử OAuth/đăng bài/thanh toán. Việc lập kế hoạch không tự thực hiện các giao dịch bên ngoài.
- Backlog cũ được giữ để tham chiếu. Các đề xuất trái thiết kế mới, đặc biệt Manager tạo Team/Brand, được thay thế sau khi R00 chốt contract.

**Tiến độ kế hoạch mới: 12/13 task lớn hoàn thành (R00–R11; phạm vi và bằng chứng theo từng báo cáo).** R10 có các chức năng Mobile chưa hỗ trợ; R11 là hồi quy cục bộ, E2E mock API. R12 chưa hoàn thành. Các cấu trúc đã có là đầu vào để kế thừa, không phải 13 task phải viết lại từ đầu.

## 8. Nhật ký hoàn thành

- R12 đang thực hiện: [báo cáo và runbook R12](docs/R12_PROGRESS.md). Backup/restore local và áp dụng 6 migration trên bản sao mới đạt; không còn pending migration/cột thiếu trên bản sao. Có 6 scope kênh legacy cần rà soát. Chưa triển khai database ứng dụng hoặc nghiệm thu provider thật, nên giữ R12 chưa hoàn thành.

- Cập nhật R12: đã backup lại và áp dụng migration vào `aisam_local`; 0 migration chờ/cột thiếu. Kiểm tra lại 600 backend, 133 Web, 59 Mobile đạt; API thật trên bản sao trả health 200 và chặn truy cập không token 401. Chưa bật v2 lâu dài hoặc nghiệm thu các role/provider thật; checkbox R12 vẫn giữ mở đúng tiêu chí.

- R00: hoàn thành ngày 14/09/2026; đầu ra [R00_RBAC_CONTRACT.md](docs/R00_RBAC_CONTRACT.md). Đã đối chiếu enum, model kênh và routes hiện có; rà soát ma trận, mapping, API và các tình huống nhiều Team. Chưa chạy migration hoặc thay đổi nghiệp vụ. R01–R12 giữ chưa hoàn thành vì chưa có bằng chứng đáp ứng thiết kế mới.

- R01 hoàn thành: xem [báo cáo R01](docs/R01_PROGRESS.md). Đã kiểm thử schema, backfill lặp lại và rollback trên bản sao local/schema mới; 2 scope kênh legacy giữ chờ rà soát trước bật v2. Chưa triển khai lên database ứng dụng.

- R02: hoàn thành nền tảng resolver/policy/context và tích hợp middleware v2; 56 kiểm thử liên quan đạt. Xem [báo cáo R02](docs/R02_PROGRESS.md) cho giới hạn và các API nghiệp vụ tiếp tục ở R03–R08. Chế độ v2 chưa bật trên ứng dụng.

- R03 hoàn thành backend và kiểm thử cục bộ: [báo cáo R03](docs/R03_PROGRESS.md). 52 kiểm thử liên quan đạt; chưa bật v2, chưa gửi email thật hoặc nghiệm thu concurrency PostgreSQL (R11).

- R04 hoàn thành backend v2: [báo cáo R04](docs/R04_PROGRESS.md). 61 kiểm thử liên quan đạt; chưa nghiệm thu UI hoặc bật chế độ v2.

- R05 hoàn thành backend v2: [báo cáo R05](docs/R05_PROGRESS.md). 56 kiểm thử liên quan đạt; chưa gọi OAuth/provider thật hoặc bật v2.

- R06 hoàn thành backend và kiểm thử cục bộ: [báo cáo R06](docs/R06_PROGRESS.md). 43 kiểm thử đạt; cách ly nội dung/media/snapshot theo Team và trạng thái, chặn bỏ qua duyệt, thêm thu hồi duyệt. Chưa bật v2; kiểm tra worker/concurrency tiếp tục ở R07, UI ở R09.

- R07 hoàn thành backend và kiểm thử cục bộ ngày 15/09/2026: [báo cáo R07](docs/R07_PROGRESS.md). 132 kiểm thử đạt; kiểm tra actor/Team/kênh/snapshot khi thực thi, TeamId cho automation và bảo vệ worker video. Chưa bật v2 hoặc áp dụng migration; nghiệm thu PostgreSQL đồng thời và provider thật ở R11/R12.

- R08 hoàn thành backend và kiểm thử cục bộ ngày 15/09/2026: [báo cáo R08](docs/R08_PROGRESS.md). 39 kiểm thử đạt; KPI theo Team/vai trò, query Post theo grant đúng Team, export JSON cùng scope và chặn dashboard tài chính cho Member. Chưa bật v2; UI ở R09 và nghiệm thu PostgreSQL ở R11.

- R09 hoàn thành mã nguồn và kiểm thử cục bộ ngày 15/09/2026: [báo cáo R09](docs/R09_PROGRESS.md). Đã nối Web hai tầng, AI/automation theo Team và sửa/xóa/tạo lại lịch qua API; 133 test Web, 70 test backend liên quan và build Web đạt. Thêm migration Team cho hội thoại, đồng bộ snapshot enum. Chưa cập nhật database ứng dụng hoặc bật v2; nghiệm thu tích hợp ở R11/R12.

- R10 hoàn thành phạm vi contract/nội dung Mobile và kiểm thử cục bộ ngày 15/09/2026: [báo cáo R10](docs/R10_PROGRESS.md). 59 test đạt, build bundle đạt, phần mã mới không có lỗi lint. Thêm Team khi tạo bài/AI, kiểm tra quyền từ API và tương thích revision. Quản trị Team/thành viên và hội thoại AI v2 trên Mobile hiện hướng dẫn dùng Web; không dựng form legacy sai contract. Chưa bật v2 hoặc nghiệm thu thiết bị thật; R11 là bước tiếp theo.

- R11 hoàn thành hồi quy cục bộ ngày 15/09/2026: [báo cáo R11](docs/R11_PROGRESS.md). 600 backend test, 133 Web test và 24 E2E đạt. Migration/backfill/rollback và HR concurrency được kiểm tra PostgreSQL riêng. Sửa gán tác giả từ request và chuyển lỗi serialization bọc thành 409. Chưa triển khai database ứng dụng; 2 scope kênh legacy còn cần rà soát; nghiệm thu toàn tuyến/provider thật thuộc R12.

