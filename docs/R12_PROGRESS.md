# R12 — Triển khai và nghiệm thu

Ngày cập nhật: 15/09/2026. **Đang thực hiện; chưa đánh dấu hoàn thành.**

Chuẩn bị production: xem [checklist phát hành](PRODUCTION_RELEASE_CHECKLIST.md). Công cụ AcceptancePreflight đã có tùy chọn `--rbac-v2` để kiểm tra ngoại lệ quyền cùng schema/migration; chỉ đọc dữ liệu và trả mã lỗi khi cần rà soát. Workflow tự deploy trên main/master nhưng script VPS chưa được đối chiếu; chưa xác nhận sẵn sàng phát hành.

## Đã thực hiện

- Người dùng cung cấp URL môi trường đích `https://aisam.ddns.net/`. Kiểm tra công khai ngày 15/09/2026: trang gốc trả 200; `/api/health` trả JSON AISAM Backend Healthy (200); `/api/permissions/context` và `/api/workspaces` trả 401 khi không đăng nhập. API cùng domain có base URL `https://aisam.ddns.net/api`. Chỉ kiểm tra GET công khai; chưa xác nhận commit triển khai, trạng thái migration hoặc RBAC v2 trên server, chưa thực hiện giao dịch.

- Thêm `scripts/Test-R12LocalBackup.ps1`: dump read-only từ `127.0.0.1:5432/aisam_local`, khôi phục sang database mới có tiền tố `aisam_r12_restore_`, so sánh số lượng sáu bảng nghiệp vụ. Không ghi đè database ứng dụng; mật khẩu lấy từ biến môi trường.
- Đã chạy thành công; bản sao nghiệm thu: `aisam_r12_restore_20260915184311`. Dump, SHA256 và kết quả nằm tại `.artifacts/r12/20260915184311/backup-report.json`. Dump chứa dữ liệu riêng, chỉ giữ trong artifacts local.
- Trước migration: bản sao có 6 migration chờ và thiếu 6 cột model. Có 43 content, 30 post, 3 automation plan, 40 social integration.
- Đã chạy **EF database update thực tế** toàn bộ 6 migration còn thiếu trên bản sao: BackfillTeamChannelAccessCanView, MigrateRbacSchemaAndEnums, AddWorkspaceRoleV2, PrepareTeamRbacV2, AddAutomationTeamScope, AddConversationTeamScope. Thành công, log `.artifacts/r12/migrate-clone.log`.
- Kiểm tra sau migration: database truy cập được, **0 pending migration, 0 missing model column**. Báo cáo `.artifacts/r12/post-migration.json`.
- Preflight bản sao mới có **6 `channel_scope_needs_review`**. Con số 2 ở R01/R11 thuộc bản sao cũ, không thay thế kiểm tra mới. Giữ các scope chưa duyệt ở trạng thái đóng.

## Còn thiếu để nghiệm thu R12

### Cập nhật lượt kiểm tra toàn bộ

- Đã thêm và chạy `scripts/Test-R12AuthenticatedSmoke.ps1`: **14 kiểm tra HTTP thật đạt** trên bản sao (5 Owner, 2 Member; mỗi người nhận context v2 đúng role và bị từ chối workspace không thuộc quyền). API dùng JWT ký bằng khóa tạm riêng, worker tắt, không sửa dữ liệu nghiệp vụ; tiến trình dừng sau test. Báo cáo `.artifacts/r12/authenticated-smoke.json`. Chưa thay thế đăng nhập Google hoặc kiểm tra WorkspaceManager/TeamRole đầy đủ trên toàn tuyến.
- Lượt E2E gặp timeout do chờ tải tài nguyên ngoài. Fixture đã tách font CDN Google khỏi kiểm thử nghiệp vụ; kiểm tra chuyển hướng Admin chờ DOM và vẫn xác nhận URL đích. Không tăng timeout hoặc bỏ assertion quyền.

- Backup mới đã restore và so sánh thành công: `.artifacts/r12/20260915185705/backup-report.json`.
- **Đã áp dụng 6 migration vào `aisam_local`**, sau backup; không có API local trên cổng 5027 lúc thực hiện. Kiểm tra sau migration: 0 pending migrations, 0 missing model columns; `.artifacts/r12/local-preflight.json`. Các ghi chú “chưa áp dụng local” ở lượt trước là lịch sử, được thay thế bởi kết quả này.
- Chạy lại: backend **600/600**, Web unit **133/133**, Mobile **59/59**. Log lần lượt `backend-final.log`, `web-final.log`, `mobile-final.log` trong `.artifacts/r12`.
- Thêm `BackgroundJobs:Enabled` (biến môi trường `BackgroundJobs__Enabled`), mặc định true. Đặt false để chạy smoke không khởi động 8 worker; không thay đổi mặc định vận hành.
- Build API sau thay đổi: 0 lỗi. Khởi động API thật trên `127.0.0.1:5127`, môi trường Testing, RBAC v2 bật chỉ trong tiến trình này, database bản sao và worker tắt. Health trả 200; permissions/context, workspaces và content không token trả 401. Log `.artifacts/r12/http-smoke.json`. Tiến trình kiểm thử đã dừng.
- Smoke này chưa kiểm chứng luồng đăng nhập và thao tác theo từng role trên toàn tuyến; không dùng kết quả 200/401 để kết luận các luồng nghiệp vụ đã nghiệm thu.
- Chưa bật v2 lâu dài hoặc đổi `.env`. Giữ scope kênh chưa duyệt đóng; không tự cấp lại quyền legacy.

- Xác định môi trường đích: local hoặc staging, URL truy cập và scope tài khoản/kênh sandbox.
- Owner rà soát 6 scope legacy trên bản sao mới trước bật v2; không suy ra cấp kênh từ các quyền lẻ cũ.
- Kiểm tra Web → API → database thật: Owner, WorkspaceManager, Manager A/Viewer B, Creator, Viewer và thành viên bị thu hồi; kiểm tra job đang chờ.
- OAuth, đăng bài sandbox, checkout và Mobile thiết bị thật theo phạm vi được phép.
- Rollout đầy đủ và smoke nghiệp vụ: schema local đã cập nhật như mục trên; chưa sửa `.env`, chưa bật v2 lâu dài, chưa chạy provider thật.
- Workflow VPS gọi `deploy-aisam.sh` nhưng file không có trong checkout; cần đối chiếu script trên VPS và tên service trước dùng workflow để triển khai.

## Runbook triển khai

1. Ghi commit/bản build, URL FE/API, database đích, tên service API/worker và người thực hiện. Không ghi secret vào tài liệu.
2. Build/test theo báo cáo R11. Mobile dùng phạm vi hỗ trợ đã ghi tại R10.
3. Dừng tiến trình ghi và worker trên môi trường đích; tạo dump mới và kiểm tra restore vào database riêng. So sánh số lượng chỉ là kiểm tra cơ bản, không phải checksum mọi row.
4. Trỏ công cụ EF tới database đích bằng biến môi trường trong terminal; kiểm tra đích đã chọn trước lệnh `dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API`. Không dùng chuỗi kết nối trong command log.
5. Kiểm tra pending migrations, cột model, `rbac_v2_preflight`, `rbac_v2_channel_diff`. Ngoại lệ còn mở phải được xử lý hoặc ghi rõ chặn bật v2.
6. Cấu hình `Rbac__UseV2=true` trên tiến trình API khi dữ liệu và client đã sẵn sàng. Không tin role cũ trong token; thành viên không Team không được cấp toàn Brand.
7. Khởi động API/FE, kiểm tra health và các endpoint permission context bằng tài khoản nghiệm thu. Sau đó kiểm tra tạo bài → duyệt → lịch/đăng sandbox và thu hồi quyền.
8. Chỉ mở lại worker khi đã xác minh dữ liệu lịch cũ, actor/Team/kênh và snapshot; theo dõi lỗi 403/409/500 và job bị từ chối.
9. Ghi kết quả, thời gian, request/job ID và bằng chứng không chứa token. Chỉ đánh dấu R12 sau nghiệm thu môi trường đích.

## Rollback

- Nếu chưa phát sinh ghi v2: dừng API/worker, phục hồi bản build và cấu hình trước đó theo bản phát hành đã ghi; đối chiếu schema tương thích. Không chỉ tắt cờ v2 nếu policy legacy có thể mở lại quyền rộng.
- Nếu đã phát sinh dữ liệu v2: không tự chạy Down để xóa attribution/Team. Dừng ghi, giữ bản sao sự cố, restore dump đã kiểm chứng vào database mới và kiểm tra trước đổi đích kết nối. Xác định dữ liệu phát sinh sau backup cần phục hồi; không ghi đè nguồn hiện tại mà chưa có phương án bảo toàn.
- Script backup này không tự deploy, không bật v2, không xóa database và không tự thực hiện rollback.
