# Phát hành RBAC hai tầng

## Trạng thái hiện tại

Mã nguồn và kiểm thử cục bộ đã có bằng chứng trong R11/R12. **Chưa xác nhận sẵn sàng phát hành production**: chưa có bằng chứng nghiệm thu môi trường đích và provider sandbox, chưa đối chiếu script/service triển khai, còn scope kênh legacy cần quyết định.

Workflow `.github/workflows/deploy-aisam.yml` tự deploy khi push thay đổi liên quan vào `main/master`. Không coi push lên các nhánh này là chỉ lưu mã nguồn. Chuẩn bị/review trên nhánh phát hành riêng trước khi merge theo quy trình triển khai thực tế.

## Kiểm tra database đích

Trong thư mục `AISAM-BE`, cấu hình `CONNECTION_STRING` trong môi trường terminal (không đặt secret trong tài liệu/log), chạy:

```powershell
dotnet run --project tools/AcceptancePreflight/AcceptancePreflight.csproj -- --rbac-v2
```

Công cụ chỉ đọc database, không chạy API/worker. Exit 1 nếu còn migration, cột model thiếu, không có view RBAC hoặc có ngoại lệ cần rà soát. Báo cáo: `.artifacts/final-acceptance/database-preflight.json`.

`channel_scope_needs_review` là scope đang đóng. Không sửa `scope_enabled_v2=true` hàng loạt để vượt kiểm tra. Owner phải đối chiếu Team/Brand/kênh, xác định scope được cấp và scope phải tiếp tục đóng; lưu quyết định và kiểm tra tác động nghiệp vụ. View hiện đánh dấu mọi scope đóng nên kết quả này cần đọc cùng biên bản rà soát, không đồng nghĩa tất cả phải được bật.

## Thứ tự phát hành

1. Chốt commit phát hành và giữ bản build trước đó; xác nhận URL API/FE, database, tên service, đường dẫn triển khai và script VPS thực tế.
2. Tạo backup mới trong cửa sổ dừng ghi; restore thử sang database riêng. Không dùng dump cũ trên máy phát triển thay backup production.
3. Áp dụng migration bằng EF đúng database đã kiểm tra; chạy lại preflight và rà soát scope. Không suy ra trạng thái production từ database local.
4. Triển khai API và FE cùng contract. Khi smoke ban đầu, dùng `BackgroundJobs__Enabled=false`; cờ này mặc định true nếu không cấu hình.
5. Chỉ bật `Rbac__UseV2=true` khi dữ liệu và client sẵn sàng. Test tài khoản Owner, WorkspaceManager, Manager A/Viewer B, Creator, Viewer, người không Team và người bị thu hồi.
6. Nghiệm thu tạo bài → gửi duyệt → duyệt → lịch/đăng sandbox; thu hồi kênh trước giờ chạy và xác nhận worker từ chối. Kiểm tra OAuth và checkout sandbox được phép.
7. Mở worker, theo dõi lỗi và job; ghi thời gian, commit, request/job ID và kết quả. Không ghi token vào bằng chứng.
8. Nếu thất bại: dừng ghi/worker và thực hiện rollback theo `docs/R12_PROGRESS.md`. Không chỉ tắt v2 để quay về quyền legacy rộng hơn.

## Bằng chứng phải bổ sung để đóng R12

- URL/môi trường đích, commit triển khai và cách quản lý tiến trình thực tế.
- Backup production đã restore thử; preflight và quyết định cho scope legacy.
- Smoke toàn tuyến theo role và kênh sandbox, OAuth/checkout, kết quả worker sau thu hồi quyền.
- Phạm vi Mobile theo R10; phần chưa hỗ trợ vẫn dùng Web và chưa được coi ngang bằng Web.

R12 giữ chưa hoàn thành cho đến khi các bằng chứng bắt buộc được bổ sung. Không thay checkbox bằng việc chỉ viết hướng dẫn triển khai.
