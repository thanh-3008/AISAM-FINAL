# R03 — Nhân sự workspace v2

Hoàn thành triển khai backend và kiểm thử cục bộ ngày 14/09/2026. Nhánh v2 vẫn tắt mặc định; UI ở R09, rollout ở R12.

## Đã thực hiện

- [x] WorkspaceHrV2Policy tách mời Member và WorkspaceManager: W chỉ mời Member, O được mời W, không mời Owner.
- [x] Request có WorkspaceRole riêng; response có tên WorkspaceRole, không diễn giải enum legacy thành W. Null/unknown không tự được cấp quyền.
- [x] Service mời/revoke kiểm tra role v2; W không thu hồi lời mời cấp W. Danh sách lời mời chỉ O/W.
- [x] Accept kiểm tra lại quyền người mời, người nhận/email, trạng thái và hạn lời mời, giới hạn thành viên. Repository dùng transaction Serializable trên PostgreSQL, lưu membership v2 và AcceptedAt cùng lần ghi.
- [x] Chỉ O đổi workspace role; không đổi Owner qua endpoint đổi role thường. W chỉ gỡ Member, không gỡ O/W.
- [x] Gỡ thành viên vô hiệu cả membership Team cùng workspace trong cùng SaveChanges, tránh mời lại khôi phục Team cũ.
- [x] Chuyển Owner chỉ tới W hoạt động; cập nhật cả role legacy và role v2 của hai người trong transaction hiện có. Không tạo Owner thứ hai qua invite.
- [x] O/W xem danh bạ đầy đủ; Member chỉ lấy projection đồng đội trong Team hoạt động và bản thân, không trả email/quota thực tế của đồng đội.
- [x] HR concurrency filter: GET trả X-HR-Revision; write yêu cầu If-Match, thiếu trả 428, stale trả 409; transaction/lock workspace và reload tracked membership trước authorization.
- [x] 52 kiểm thử liên quan đạt, gồm policy mới, luồng invite/accept v2, gỡ thành viên/thu hồi Team, chuyển Owner v2 và revision thiếu/cũ.

## Cách tích hợp client sau này

Chế độ server Rbac:UseV2 chọn toàn bộ nhánh v2. Client gửi X-RBAC-Contract-Version: 2; lấy X-HR-Revision từ GET danh sách nhân sự/lời mời và gửi If-Match cho mutation HR. Revision HR khác revision permission context (có tính cả lời mời và thành viên khác); không dùng lẫn.

Invite/đổi role gửi WorkspaceRole mới; trường Role legacy được giữ để tương thích chế độ cũ. Sau accept phải reload permission context; không tự thêm Team. Directory Member dùng DTO hiện có nhưng không điền email/quota, client R09 cần dùng các trường id/userId/fullName/workspaceRole cho chế độ này.

## Kiểm thử và giới hạn

Chạy dotnet test với filter WorkspaceInvitationServiceTests, WorkspaceMemberServiceTests, WorkspaceHrV2, WorkspaceInvitationControllerTests, WorkspaceMemberRepositoryTests, WorkspaceInvitationRepositoryTests, RbacV2Tests: **52 passed, 0 failed**.

Các ca mới dùng EF InMemory; transaction Serializable và khóa PostgreSQL chưa được thử tải đồng thời thực tế, cần đưa vào R11. Không gửi email thật trong kiểm thử (FakeEmailService), không thay cấu hình hoặc database local. Việc gửi email hiện đồng bộ trong request như luồng cũ; nếu giao dịch thất bại sau khi gửi, link có thể không hợp lệ, không được coi email đã gửi là bằng chứng lời mời đã commit.

R09 cần hiển thị lỗi 409/428 và tải lại danh sách, không retry mutation vô hạn. Đây là hoàn thành backend R03, chưa chứng nhận toàn luồng UI hoặc triển khai v2.
