# 🗺️ AISAM - Chi tiết Luồng Người Dùng (User Flow) mapping 68 User Stories

Tài liệu này mô tả chi tiết hành trình của người dùng (User Flow), trong đó chỉ rõ từng bước tương ứng với **68 User Stories** của hệ thống AISAM.

---

## 1. Luồng Xác Thực & Thiết Lập Tài Khoản (Authentication & Onboarding)
**🎯 Mục tiêu:** Đăng ký, đăng nhập, khôi phục mật khẩu và tạo không gian làm việc (Business Profile).

1. **Đăng nhập / Đăng ký:**
   - **(US-01)** User bấm `Sign Up` ➡️ Điền thông tin ➡️ Đăng ký tài khoản.
   - **(US-11)** Bấm `Login with Google` ➡️ Đăng nhập nhanh bằng Google.
   - **(US-02)** Nhập Email/Password ➡️ Đăng nhập vào hệ thống.
   - **(US-03)** Hệ thống ngầm tự động làm mới phiên (refresh token) để User không bị đăng xuất giữa chừng.
2. **Xác thực & Khôi phục:**
   - **(US-07)** Đăng ký xong ➡️ Chuyển sang màn hình xác minh Email.
   - **(US-08)** Nếu chưa nhận được mail, bấm `Resend verification email`.
   - **(US-09, US-10)** Quên mật khẩu ➡️ Bấm `Forgot Password` ở màn Login ➡️ Nhận mail ➡️ Bấm link kèm token để Đặt lại mật khẩu.
3. **Onboarding & Quản lý Profile:**
   - **(US-12)** Đăng nhập lần đầu ➡️ Bắt buộc vào Onboarding để tạo Business Profile đầu tiên.
   - **(US-13)** Bấm vào `Profile Switcher` trên Navbar ➡️ Xem danh sách Profile của tôi.
   - **(US-14)** Chọn `Edit Profile` ➡️ Cập nhật thông tin doanh nghiệp.
   - **(US-16)** (Hệ thống) Khi đổi Profile trên Navbar, mọi dữ liệu Brand/Content bên dưới tự động load theo Profile đang được chọn.
4. **Tài khoản cá nhân:**
   - **(US-04)** Bấm Avatar ➡️ Chọn `My Account` ➡️ Xem thông tin tài khoản hiện tại.
   - **(US-05)** Bấm Avatar ➡️ Chọn `Logout` ➡️ Đăng xuất thiết bị hiện tại.
   - **(US-06)** Trong `My Account` ➡️ Chọn `Logout All Devices` ➡️ Đăng xuất mọi nơi.

---

## 2. Luồng Thiết lập Thương Hiệu & Sản Phẩm (Brands & Products)
**🎯 Mục tiêu:** Tạo Brand Kit và danh mục sản phẩm làm ngữ cảnh cho AI.

1. **Quản lý Brand:**
   - **(US-15)** Ở Dashboard, bấm `Brands & Products` ➡️ Xem, tạo mới, cập nhật, hoặc xóa Brand Kit. (Có thể khôi phục nếu xóa nhầm).
2. **Quản lý Sản phẩm theo Brand:**
   - **(US-17)** Bấm vào chi tiết 1 Brand ➡️ Chuyển sang tab `Products` ➡️ Xem, tạo mới, sửa, xóa, khôi phục Sản phẩm thuộc Brand đó.
   - **(US-18)** Tại danh sách Products ➡️ Dùng thanh tìm kiếm và bộ lọc để lọc sản phẩm theo từ khóa.

---

## 3. Luồng Kết nối Mạng Xã Hội (Social Integrations)
**🎯 Mục tiêu:** Cấp quyền cho AISAM đăng bài lên các nền tảng MXH.

1. **Kết nối Facebook:**
   - **(US-29)** Bấm `Social Integrations` ➡️ Bấm `Connect Facebook` (OAuth).
   - **(US-30)** Kết nối xong ➡️ Xem danh sách các tài khoản Social đã liên kết.
   - **(US-31)** Hệ thống hiển thị danh sách các Fanpage khả dụng từ tài khoản Facebook vừa nối.
2. **Liên kết với Brand:**
   - **(US-32)** User chọn 1 Fanpage ➡️ Bấm `Link to Brand` ➡️ Fanpage này giờ đây dùng để đăng bài cho Brand đã chọn.
3. **Ngắt kết nối:**
   - **(US-33)** Khi không dùng nữa, bấm `Unlink` để ngắt kết nối tài khoản hoặc Fanpage.
4. **[Chưa Migrate] Các kênh mở rộng:**
   - **(US-62, US-63)** Tương tự như trên, dùng để kết nối thêm Instagram Business và TikTok Business.

---

## 4. Luồng Sáng tạo Nội dung & AI (Content Hub)
**🎯 Mục tiêu:** Viết bài quảng cáo tự động và duyệt bài.

1. **Khởi tạo Nội dung:**
   - **(US-20)** Bấm `Content Hub` ➡️ Xem danh sách bản nháp (Drafts) hiện có.
   - **(US-19)** Bấm `New Content` ➡️ Tạo nội dung thủ công, chọn Brand và Product.
   - **(US-21)** Bấm vào nội dung cũ ➡️ Chọn `Clone` để nhân bản nhanh cho chiến dịch mới.
2. **Sử dụng AI:**
   - **(US-22)** Tại màn tạo mới, nhập Prompt ➡️ Bấm `Generate with AI` để sinh bản nháp văn bản.
   - **(US-26)** Trong quá trình sửa, chat với AI ngay tại ngữ cảnh bài viết để brainstorm thêm ý tưởng.
   - **(US-23)** Chọn đoạn văn bản ➡️ Yêu cầu AI cải thiện (`Improve with AI`).
3. **Phê duyệt & Lịch sử:**
   - **(US-25)** Bấm nút `History` ➡️ Xem lại toàn bộ các phiên bản AI đã sinh ra trước đó.
   - **(US-24)** Chọn 1 phiên bản ưng ý ➡️ Bấm `Approve` để cập nhật vào nội dung chính thức.
   - **(US-27, US-28)** Bấm tab `AI Chat` ➡️ Xem lại lịch sử chat với AI, có thể xóa cuộc hội thoại rác.
4. **[Chưa Migrate] Tính năng nâng cao:**
   - **(US-58)** Gửi bài viết cho sếp duyệt (Pending -> Approve/Reject/Feedback).
   - **(US-61)** Upload media (ảnh/video) lên Storage riêng.
   - **(US-64, US-65)** Dùng AI sinh hẳn ảnh hoặc video quảng cáo đính kèm vào bài.

---

## 5. Luồng Xuất bản & Lên lịch (Publishing & Calendar)
**🎯 Mục tiêu:** Đăng bài lên Facebook hoặc lên lịch tự động.

1. **Đăng bài ngay lập tức:**
   - **(US-34)** Tại Content đã duyệt, bấm `Publish Now` ➡️ Chọn Fanpage ➡️ Bài đăng ngay lập tức lên Facebook.
   - **(US-35)** Bấm menu `Posts History` ➡️ Xem danh sách và chi tiết các bài đã đăng thành công/thất bại.
2. **Lên lịch đăng:**
   - **(US-40)** Bấm `Schedule` ➡️ Chọn Fanpage, Ngày, Giờ.
   - **(US-41)** Bấm menu `Calendar` ➡️ Xem lịch, cập nhật ngày giờ, hoặc hủy (xóa) lịch đăng sắp tới.
   - **(US-42)** (Hệ thống) Background worker âm thầm check lịch và tự động publish khi đến hạn.

---

## 6. Luồng Theo dõi, Quota & Thanh toán (Dashboard & Billing)
**🎯 Mục tiêu:** Xem báo cáo, nhận thông báo, và gia hạn Subscription.

1. **Dashboard & Quota:**
   - **(US-43)** Bấm menu `Dashboard` ➡️ Xem tổng quan số Content, số bài Post, lịch đang chờ (MVP).
   - **(US-48)** Xem thanh tiến trình Quota ➡️ Biết còn bao nhiêu lượt Prompt AI và lượt Publish.
   - **(US-49, US-50)** (Hệ thống) Khi User vượt Quota, hệ thống chặn tính năng `Generate AI` hoặc `Publish/Schedule`, hiện cảnh báo.
2. **Thông báo (Notifications):**
   - **(US-36, US-39)** Hệ thống gửi thông báo nội bộ (VD: Đăng bài thành công/lỗi). Có hiển thị số thông báo chưa đọc (Unread Count) ở góc Navbar.
   - **(US-37)** Bấm vào hình quả chuông ➡️ Mở xem danh sách thông báo.
   - **(US-38)** Bấm `Mark as read` để đánh dấu đã đọc.
3. **Thanh toán & Gói cước:**
   - **(US-46)** Bấm Avatar ➡️ Chọn `Subscription` ➡️ Xem gói hiện tại.
   - **(US-44)** Bấm `Upgrade` ➡️ Chuyển sang thanh toán qua PayOS bằng mã QR.
   - **(US-47)** (Hệ thống) Nhận webhook từ PayOS để tự động kích hoạt gói.
   - **(US-45)** User xem danh sách Lịch sử thanh toán.
4. **[Chưa Migrate] Nâng cao:**
   - **(US-67, US-68)** Xem Analytics chi tiết hơn và nhận gợi ý tối ưu từ AI.

---

## 7. Luồng Quản trị & Kỹ thuật (Admin & System - Backend/Admin App)
**🎯 Mục tiêu:** Vận hành hệ thống (Dành cho Admin).

1. **Admin Portal:**
   - **(US-51)** Login bằng tài khoản Admin ➡️ Chuyển sang giao diện `/admin`.
   - **(US-55)** (Hệ thống) Chặn User thường không được vào route `/admin` hoặc gọi API Admin.
   - **(US-52, US-53)** Quản trị viên xem danh sách Users, Profiles, xem Subscription/Payment của khách để hỗ trợ khách hàng.
   - **(US-54)** Bấm `Seed Demo Data` ➡️ Dựng data mẫu để demo (Chỉ Admin mới có quyền).
2. **[Chưa Migrate] Mở rộng Team & Ads:**
   - **(US-59)** Admin tạo Team, thêm Member, phân quyền (Manager/Editor/Viewer).
   - **(US-60)** Mở tính năng chạy Facebook Ads trực tiếp từ AISAM.
   - **(US-66)** Admin tự động thêm/bớt các Gói cước (Plans) động mà không cần sửa code.
3. **System Testing:**
   - **(US-56, US-57)** Backend developers chạy Unit Test và rà soát Setup Guide để đảm bảo API hoạt động ổn định.
