# AISAM -- Manual Test Cases

**Ngày tạo:** 2026-07-20 | **Loại:** Manual Testing (UI/Browser)

---

## SHEET 1/19: AUTH -- Authentication (74 cases)

| **Feature** | Authentication |
|---|---|
| **Test requirement** | Đăng ký (validate Full Name, Email, Password, Confirm Password, email đã tồn tại, sai định dạng, khoảng trắng, loading, double click, toggle password, verify workspace tự động tạo, verify email); Đăng nhập Email/Password (validate, sai email/mật khẩu, session hết hạn, logout all, multi-device); Quên mật khẩu (gửi reset link, email tồn tại/không tồn tại, validate form); Đặt lại mật khẩu (token hợp lệ, hết hạn, không tồn tại, dùng lại, validate MK mới); Đổi mật khẩu (current password sai, MK mới trùng, validate); Verify Email & Resend (token hợp lệ, hết hạn, resend, link cũ vô hiệu); Google OAuth (login lần đầu, đã liên kết, trùng email, hủy OAuth, từ chối quyền, mất mạng)

### 1.1 SIGN UP -- Đăng ký (SU-01 -> SU-22)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SU-01 | Đăng ký thành công với đầy đủ thông tin hợp lệ | 1. Truy cập https://[domain]/register 2. Nhập Full Name: Nguyen Van A 3. Nhập Email: test@example.com (chưa dùng) 4. Nhập Password: Pass1234 5. Nhập Confirm Password: Pass1234 6. Click Create Account | Chuyển hướng sang /overview. Sidebar hiển thị workspace tên "Nguyen Van A's Workspace" | Email chưa tồn tại |
| SU-02 | Đăng ký thành công khi bỏ trống Full Name | 1. Bỏ trống Full Name 2. Nhập Email, Password, Confirm Password hợp lệ 3. Click Create Account | Chuyển hướng sang /overview. Sidebar hiển thị workspace tên "Personal Workspace" | Email chưa tồn tại |
| SU-03 | Đăng ký với email đã tồn tại | 1. Nhập Email đã đăng ký trước đó 2. Nhập các trường còn lại hợp lệ 3. Click Create Account | Hiển thị "User with this email already exists". Vẫn ở trang Register | Có tài khoản với email đó |
| SU-04 | Đăng ký với email sai định dạng (thiếu @) | 1. Nhập Email: testexample.com 2. Nhập các trường còn lại 3. Click Create Account | Hiển thị "Please enter a valid email address" dưới ô Email. Nút không gửi được form | Không |
| SU-05 | Đăng ký khi bỏ trống Email | 1. Bỏ trống Email 2. Nhập các trường khác 3. Click Create Account | Hiển thị "Email is required" dưới ô Email. Nút không gửi được form | Không |
| SU-06 | Đăng ký với Password dưới 8 ký tự | 1. Nhập Password: Pass123 (7 ký tự) 2. Nhập Confirm Password giống hệt 3. Click Create Account | Hiển thị "Password must be at least 8 characters". Vẫn ở trang Register | Email chưa tồn tại |
| SU-07 | Đăng ký khi bỏ trống Password | 1. Bỏ trống Password 2. Nhập các trường khác 3. Click Create Account | Hiển thị "Password is required" dưới ô Password. Nút không gửi được form | Không |
| SU-08 | Confirm Password không khớp | 1. Nhập Password: SecurePass1 2. Nhập Confirm Password: SecurePass2 3. Click Create Account | Hiển thị "Passwords do not match" dưới ô Confirm Password. Nút không gửi được form | Email chưa tồn tại |
| SU-09 | Bỏ trống Confirm Password | 1. Nhập Password hợp lệ 2. Bỏ trống Confirm Password 3. Click Create Account | Hiển thị "Confirm password is required" dưới ô Confirm Password. Nút không gửi được form | Email chưa tồn tại |
| SU-10 | Bỏ trống tất cả các trường | 1. Không nhập gì 2. Click Create Account | Hiển thị đồng thời "Email is required", "Password is required", "Confirm password is required". Nút không gửi được form | Không |
| SU-11 | Toggle hiển thị/ẩn password | 1. Nhập Password 2. Click icon con mắt 3. Click icon con mắt lần nữa | Lần 1: password hiện chữ thường. Lần 2: ẩn thành dấu chấm. 2 ô hoạt động độc lập | Đang ở trang Register |
| SU-12 | Loading state khi đang submit | 1. Nhập đầy đủ thông tin hợp lệ 2. Click Create Account | Nút chuyển thành "Creating Account..." kèm spinner xoay, bị disable. Sau khi xong tự chuyển trang | Email chưa tồn tại |
| SU-13 | Link Sign In điều hướng đúng | 1. Click "Already have an account? Sign In" | Chuyển đến trang /login, hiển thị form đăng nhập | Đang ở trang Register |
| SU-14 | User đã đăng nhập truy cập trang Register | 1. Đăng nhập trước đó 2. Truy cập /register | Tự động chuyển hướng về /overview, không hiển thị form đăng ký | Đã đăng nhập |
| SU-15 | Verify Workspace tự động tạo sau đăng ký | 1. Đăng ký user mới FullName: Nguyen Van A 2. Vào dashboard, kiểm tra sidebar và các trang liên quan | Sidebar hiển thị workspace "Nguyen Van A's Workspace". Role hiển thị Owner. Trang Credit hiển thị 50 credits. Trang Pricing hiển thị plan Free, post quota 20/tháng | Đăng ký thành công |
| SU-16 | Verify email xác minh được gửi sau đăng ký | 1. Đăng ký user với email thật 2. Mở hộp thư, kiểm tra email nhận được | Nhận email từ AISAM, tiêu đề "Verify your email". Nội dung chứa tên user đã đăng ký. Có nút/link "Verify Email". Click link -> hiển thị "Email verified successfully" | Email service hoạt động |
| SU-17 | Đăng ký với email có ký tự đặc biệt hợp lệ | 1. Nhập Email: test.user+tag@domain.co.vn 2. Nhập các trường khác hợp lệ 3. Click Create Account 4. Logout, login lại với email đó | Đăng ký thành công. Login lại bằng đúng email đó -> vào được dashboard | Email chưa tồn tại |
| SU-18 | Đăng ký với FullName tiếng Việt có dấu | 1. Nhập FullName: Nguyễn Văn An 2. Nhập Email, Password hợp lệ 3. Click Create Account | Đăng ký thành công. Sidebar hiển thị "Nguyễn Văn An's Workspace" (đúng dấu, không lỗi font). Header hiển thị "Nguyễn Văn An" | Email chưa tồn tại |
| SU-19 | Đăng ký với password toàn khoảng trắng | 1. Nhập Password: 8 dấu cách 2. Nhập Confirm Password giống hệt 3. Email, FullName hợp lệ 4. Click Create Account 5. Logout, login với 8 dấu cách | Đăng ký thành công. Login với 8 dấu cách -> vào được. Login với password rỗng -> "Invalid email or password" | Email chưa tồn tại |
| SU-20 | Double-click submit (click nhanh 2 lần) | 1. Nhập đầy đủ thông tin hợp lệ 2. Click Create Account 2 lần liên tục | Lần 1: nút loading, đăng ký thành công. Nút đã disable nên lần 2 không tác dụng. Chỉ nhận 1 email verify, chỉ có 1 workspace trong sidebar | Email chưa tồn tại |
| SU-21 | Đăng ký với email có khoảng trắng đầu/cuối | 1. Nhập Email: "  test@example.com  " 2. Nhập các trường khác hợp lệ 3. Click Create Account 4. Logout, login với "test@example.com" (đã trim) | Ghi nhận thực tế: nếu login được -> FE đã trim. Nếu báo "Invalid email or password" -> FE chưa trim, báo bug | Email chưa tồn tại |
| SU-22 | Spam đăng ký liên tục nhiều lần | 1. Điền form hợp lệ 2. Click Create Account 10+ lần trong vài giây | Các lần đầu báo lỗi email đã tồn tại hoặc thành công. Sau N lần hiển thị "Too Many Requests" hoặc "Please try again later" | Email chưa tồn tại |

### 1.2 SIGN IN -- Đăng nhập Email/Password (SI-01 -> SI-44)

**A. Login cơ bản (SI-01 -> SI-12)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SI-01 | Đăng nhập thành công | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com 3. Nhập Password: SecurePass1 4. Click Sign In | Chuyển hướng sang /overview. Header hiển thị tên user. Sidebar hiển thị workspace của user | Tài khoản đã đăng ký |
| SI-02 | Email không tồn tại | 1. Nhập Email: notexist@example.com 2. Nhập Password bất kỳ 3. Click Sign In | Hiển thị "Invalid email or password". Vẫn ở trang Login, form không bị reset | Không |
| SI-03 | Password sai | 1. Nhập Email đúng 2. Nhập Password sai 3. Click Sign In | Hiển thị "Invalid email or password" (giống SI-02, không phân biệt sai gì) | Tài khoản đã đăng ký |
| SI-04 | Đăng nhập khi chưa xác minh email | 1. Dùng tài khoản vừa đăng ký, chưa click link verify 2. Đăng nhập bình thường | Đăng nhập thành công, vào được dashboard. Có thể thấy badge/cảnh báo "Email not verified" (nếu FE có) | TK vừa đăng ký |
| SI-05 | Bỏ trống Email | 1. Bỏ trống ô Email 2. Nhập Password 3. Click Sign In | Hiển thị "Email is required" dưới ô Email. Nút không gửi được form | Không |
| SI-06 | Bỏ trống Password | 1. Nhập Email hợp lệ 2. Bỏ trống ô Password 3. Click Sign In | Hiển thị "Password is required" dưới ô Password. Nút không gửi được form | Không |
| SI-07 | Email sai định dạng | 1. Nhập Email: notanemail 2. Nhập Password 3. Click Sign In | Hiển thị "Please enter a valid email address" dưới ô Email. Nút không gửi được form | Không |
| SI-08 | Email có khoảng trắng đầu/cuối | 1. Nhập Email: "  test@example.com  " 2. Nhập Password đúng 3. Click Sign In | Ghi nhận thực tế: nếu vào được -> FE có trim. Nếu báo "Invalid email or password" -> bug | TK đã đăng ký |
| SI-09 | Password chứa tiếng Việt có dấu | 1. Đăng ký với password "MậtKhẩu123" 2. Logout 3. Login với "MậtKhẩu123" | Đăng nhập thành công, vào được dashboard | TK đã đăng ký |
| SI-10 | Loading state khi submit | 1. Nhập email + password hợp lệ 2. Click Sign In | Nút chuyển thành "Signing In..." kèm spinner, bị disable. Sau khi xong tự chuyển trang | TK đã đăng ký |
| SI-11 | Toggle hiển thị/ẩn password | 1. Nhập Password 2. Click icon con mắt 3. Click icon con mắt lần nữa | Lần 1: password hiện chữ thường. Lần 2: ẩn thành dấu chấm | Đang ở trang Login |
| SI-12 | Đăng nhập sai nhiều lần liên tiếp | 1. Nhập email đúng, password sai 10+ lần 2. Quan sát phản hồi | Mỗi lần hiển thị "Invalid email or password". Sau nhiều lần có thể hiển thị "Too many attempts. Please try again later". Tài khoản không bị khóa vĩnh viễn | TK đã đăng ký |

**B. Navigation & Session (SI-13 -> SI-18)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SI-13 | Link Sign Up | 1. Click "Don't have an account? Sign Up" | Chuyển đến /register, hiển thị form đăng ký | Đang ở trang Login |
| SI-14 | Link Forgot Password | 1. Click "Forgot Password?" | Chuyển đến /forgot-password, hiển thị form nhập email reset | Đang ở trang Login |
| SI-15 | Đã đăng nhập, truy cập /login | 1. Đăng nhập trước đó 2. Mở tab mới, truy cập /login | Tự động chuyển hướng về /overview, không hiển thị form login | Đã đăng nhập |
| SI-16 | Đăng nhập trên 2 trình duyệt cùng lúc | 1. Đăng nhập trên Chrome 2. Mở Firefox, đăng nhập cùng tài khoản | Cả 2 vào được dashboard, hoạt động độc lập, không bị logout lẫn nhau | TK đã đăng ký |
| SI-17 | Login lại sau khi Logout All | 1. Đăng nhập -> Settings -> Logout All Devices 2. Đăng nhập lại | Sau logout all: bị đá ra trang login. Login lại thành công. Các thiết bị khác (nếu có) bị logout | TK đã đăng ký |
| SI-18 | Phiên đăng nhập hết hạn | 1. Đăng nhập, để yên 2. Chờ token hết hạn 3. Click menu bất kỳ | Tự động refresh token -> không gián đoạn. HOẶC bị đá về login kèm "Session expired. Please login again" | TK đã đăng ký |

**C. Forgot Password (SI-19 -> SI-22)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SI-19 | Gửi yêu cầu quên mật khẩu - email tồn tại | 1. Vào /forgot-password 2. Nhập email đã đăng ký 3. Click Send Reset Link 4. Kiểm tra hộp thư | Hiển thị "If the email exists, a password reset link has been sent". Nhận email chứa link /reset-password?token=... | TK đã đăng ký |
| SI-20 | Gửi yêu cầu quên mật khẩu - email không tồn tại | 1. Nhập Email: notexist@example.com 2. Click Send Reset Link | Hiển thị "If the email exists, a password reset link has been sent" (giống SI-19). Không nhận email. Không bị lộ email không tồn tại | Không |
| SI-21 | Bỏ trống Email | 1. Bỏ trống ô Email 2. Click Send Reset Link | Hiển thị "Email is required" dưới ô Email. Nút không gửi được form | Không |
| SI-22 | Email sai định dạng | 1. Nhập Email: notanemail 2. Click Send Reset Link | Hiển thị "Please enter a valid email address" dưới ô Email. Nút không gửi được form | Không |

**D. Reset Password (SI-23 -> SI-30)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SI-23 | Đặt lại mật khẩu thành công | 1. Click link trong email reset 2. Nhập Password mới: NewPass@456 3. Nhập Confirm: NewPass@456 4. Click Reset Password | Hiển thị "Password reset successfully. Please login with your new password". Login MK mới -> thành công. Login MK cũ -> "Invalid email or password" | Có token hợp lệ |
| SI-24 | Token hết hạn (>1 giờ) | 1. Dùng link reset quá 1 tiếng 2. Nhập mật khẩu mới 3. Click Reset Password | Hiển thị "Invalid or expired reset token". Có link "Request a new reset link" về /forgot-password | Token quá hạn |
| SI-25 | Token không tồn tại | 1. Truy cập /reset-password?token=abc123xyz 2. Nhập mật khẩu mới 3. Click Reset Password | Hiển thị "Invalid or expired reset token" | Token bịa đặt |
| SI-26 | Dùng lại link đã reset thành công | 1. Reset MK thành công 2. Mở lại link cũ 3. Nhập MK mới lần nữa | Hiển thị "Invalid or expired reset token". Mật khẩu không bị đổi thêm | Đã reset 1 lần |
| SI-27 | Mật khẩu mới quá ngắn | 1. Nhập Password mới: Abc123 (6 ký tự) 2. Click Reset Password | Hiển thị "Password must be at least 8 characters". Form không bị reset | Có token hợp lệ |
| SI-28 | Confirm Password không khớp | 1. Nhập MK mới: SecurePass1 2. Nhập Confirm: SecurePass2 3. Click Reset Password | Hiển thị "Passwords do not match" dưới ô Confirm. Nút không gửi được form | Có token hợp lệ |
| SI-29 | Link reset dẫn đúng trang | 1. Click link trong email reset | Mở trang /reset-password?token=... Hiển thị form nhập mật khẩu mới (2 ô) kèm tiêu đề "Reset Your Password" | Nhận email reset |
| SI-30 | Truy cập /reset-password không có token | 1. Truy cập trực tiếp /reset-password | Hiển thị "Invalid or missing reset token". Có link về /forgot-password | Không |

**E. Change Password (SI-31 -> SI-35)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SI-31 | Đổi mật khẩu thành công | 1. Đăng nhập -> Settings -> Change Password 2. Nhập Current Password đúng 3. Nhập New Password: NewStr@ng1 + Confirm 4. Click Change Password | Hiển thị "Password changed successfully. Please login again.". Tự chuyển về Login. Login MK mới -> thành công. Login MK cũ -> "Invalid email or password" | Đã đăng nhập |
| SI-32 | Current Password sai | 1. Nhập Current Password: sai 2. New Password & Confirm hợp lệ 3. Click Change Password | Hiển thị "Current password is incorrect". Vẫn ở trang Change Password, không bị logout | Đã đăng nhập |
| SI-33 | Đổi MK khi chưa đăng nhập | 1. Logout 2. Truy cập trang Change Password | Tự động chuyển hướng về Login, không hiển thị form | Chưa đăng nhập |
| SI-34 | New Password trùng Current Password | 1. Current Password: OldPass1 2. New Password: OldPass1 3. Click Change Password | Nếu FE chặn -> "New password must be different from current password". Nếu không chặn -> đổi thành công nhưng phải login lại | Đã đăng nhập |
| SI-35 | New Password quá ngắn | 1. Current Password đúng 2. New Password: Abc1 (4 ký tự) 3. Click Change Password | Hiển thị "Password must be at least 8 characters". Form không bị reset | Đã đăng nhập |

**F. Verify Email & Resend (SI-36 -> SI-44)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SI-36 | Xác minh email thành công | 1. Mở email verify, click "Verify Email" | Hiển thị "Email verified successfully. You can now login.". Có link về trang Login | Token hợp lệ |
| SI-37 | Link verify hết hạn (>7 ngày) | 1. Dùng link verify quá 7 ngày 2. Click link | Hiển thị "Invalid or expired verification token". Có nút "Resend verification email" | Token quá hạn |
| SI-38 | Link verify không hợp lệ | 1. Truy cập /verify-email?token=abc123xyz | Hiển thị "Invalid or expired verification token" | Token bịa đặt |
| SI-39 | Click lại link đã verify thành công | 1. Verify thành công 2. Mở lại link cũ | Hiển thị "Invalid or expired verification token". Email vẫn ở trạng thái đã verify | Đã verify |
| SI-40 | Gửi lại email xác minh - thành công | 1. Nhập email chưa verify 2. Click Resend Verification Email | Hiển thị "If the email exists and is not verified, a verification email has been sent". Nhận email verify mới | Email chưa verify |
| SI-41 | Gửi lại email - email đã verify | 1. Nhập email đã verify 2. Click Resend Verification Email | Hiển thị giống SI-40. Không nhận thêm email. Không bị lộ đã verify | Email đã verify |
| SI-42 | Gửi lại email - email không tồn tại | 1. Nhập email không có trong hệ thống 2. Click Resend Verification Email | Hiển thị giống SI-40. Không nhận email. Không bị lộ email không tồn tại | Email không tồn tại |
| SI-43 | Resend tạo link mới, link cũ vô hiệu | 1. Nhận email verify đầu (chưa click) 2. Resend email mới 3. Click link trong email mới -> thành công 4. Click link email cũ | Link mới: verify thành công. Link cũ: "Invalid or expired verification token" | Email chưa verify |
| SI-44 | Bỏ trống Email khi resend | 1. Bỏ trống ô Email 2. Click Resend Verification Email | Hiển thị "Email is required" dưới ô Email. Nút không gửi được form | Không |

### 1.3 SIGN IN WITH GOOGLE (GL-01 -> GL-08)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| GL-01 | Đăng nhập Google lần đầu | 1. Click Sign in with Google 2. Chọn Google account chưa từng dùng 3. Cấp quyền (nếu hỏi) | Chuyển hướng sang /overview. Sidebar hiển thị workspace theo Google display name. Trang Credit hiển thị 50 credits. Login bằng email/password với email đó -> không được (chưa đặt MK) | GOOGLE_CLIENT_ID đã config |
| GL-02 | Đăng nhập Google với tài khoản đã liên kết | 1. Click Sign in with Google 2. Chọn tài khoản đã dùng ở GL-01 | Vào /overview, hiển thị đúng workspace + data cũ. Không tạo workspace trùng | TK đã login Google |
| GL-03 | Google login với email đã đăng ký bằng Email/Password | 1. Đăng ký same@example.com qua form thường 2. Logout 3. Click Sign in with Google với đúng email đó | Vào đúng tài khoản cũ, dữ liệu workspace/brand cũ còn nguyên. Không tạo tài khoản trùng | User đã tồn tại |
| GL-04 | Google OAuth chưa cấu hình | 1. Server chưa set GOOGLE_CLIENT_ID 2. Click Sign in with Google | Nút Google bị ẩn hoặc không hoạt động. Nếu click được: hiển thị "Google login is not available at this time". Không crash | Chưa config |
| GL-05 | Google token không hợp lệ | 1. Giả lập token hết hạn (hoặc chờ quá lâu ở popup) | Hiển thị "Google login failed. Please try again". Quay về Login. Nút Google vẫn dùng được để thử lại | Token không hợp lệ |
| GL-06 | Hủy OAuth giữa chừng | 1. Click Sign in with Google 2. Tại popup, click Cancel hoặc đóng popup | Popup đóng. Vẫn ở trang Login. Có thể bấm Google login lại bình thường | GOOGLE_CLIENT_ID đã config |
| GL-07 | Mất mạng khi đang OAuth | 1. Click Sign in with Google 2. Ngắt mạng 3. Chọn tài khoản Google | Popup hiển thị lỗi kết nối của Google/Browser. Khi có mạng lại: bấm Google login -> thành công | GOOGLE_CLIENT_ID đã config |
| GL-08 | Từ chối quyền trên Google | 1. Click Sign in with Google 2. Google hỏi cấp quyền 3. Click Deny / Từ chối | Popup đóng. Vẫn ở trang Login. Có thể thử lại và cấp quyền | GOOGLE_CLIENT_ID đã config |

### 1.4 LOGOUT -- Đăng xuất (LO-01 -> LO-12)

**A. Logout cơ bản (LO-01 -> LO-05)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| LO-01 | Đăng xuất thành công (BE khả dụng) | 1. Đăng nhập vào dashboard với tài khoản test@example.com / SecurePass1 2. Tại góc phải Header, click avatar user 3. Trong dropdown menu, click nút "Logout" | 1. Gửi POST /api/auth/logout với Authorization: Bearer {token}, body: {refreshToken} 2. BE revoke session hiện tại 3. FE xóa toàn bộ localStorage: aisam_token, aisam_refresh_token, aisam_user 4. FE xóa cookie aisam_role (max-age=0, path=/) 5. FE invalidate workspace cache (cachedWorkspaces = null) 6. Chuyển hướng sang /login 7. Gõ thủ công /dashboard -> tự redirect về /login, không vào được dashboard | Đã đăng nhập, BE đang chạy |
| LO-02 | Đăng xuất khi BE không khả dụng (mất mạng) | 1. Đăng nhập vào dashboard 2. Mở DevTools -> Network -> chọn Offline hoặc chặn domain API 3. Click avatar Header -> Logout | 1. Gửi POST /api/auth/logout -> thất bại (Network Error) 2. FE bắt lỗi trong catch, KHÔNG hiển thị toast lỗi cho user (silent fail) 3. FE vẫn thực thi finally block: xóa toàn bộ localStorage (aisam_token, aisam_refresh_token, aisam_user) + cookie aisam_role + invalidate workspace cache 4. Chuyển hướng sang /login 5. Gõ /dashboard -> redirect /login | BE không khả dụng, Đã đăng nhập |
| LO-03 | Đăng xuất khi token đã hết hạn | 1. Đăng nhập, chờ token access hết hạn (hoặc chỉnh expire trong BE) 2. Không refresh token 3. Click avatar Header -> Logout | 1. Gửi POST /api/auth/logout với token hết hạn -> BE trả 401 2. FE vẫn xóa toàn bộ localStorage + cookie + cache (finally block luôn chạy) 3. Chuyển hướng sang /login 4. Không crash, không loop redirect | Token hết hạn, Đã đăng nhập |
| LO-04 | Sau logout, truy cập trực tiếp URL được bảo vệ | 1. Đăng xuất thành công (đang ở /login) 2. Gõ lần lượt các URL trên thanh địa chỉ: /dashboard, /content, /brands, /campaigns, /analytics, /workspace-dashboard, /social, /automation, /posts, /calendar, /notifications, /team, /credit-pack, /credit-history, /approvals, /profiles/[id], /admin/dashboard 3. Với mỗi URL, quan sát kết quả | Tất cả URL được bảo vệ đều redirect về /login. Không URL nào hiển thị flash nội dung dashboard/sidebar trước khi redirect. Không trang nào hiển thị lỗi trắng hoặc crash | Vừa logout |
| LO-05 | Browser Back sau logout không quay lại dashboard | 1. Đăng nhập -> vào /dashboard 2. Click Logout -> đang ở /login 3. Click nút Back của trình duyệt 4. Nếu quay lại được /dashboard, click menu bất kỳ trong sidebar | Kịch bản A: Back -> vẫn ở /login (trang dashboard không được cache, middleware chặn) Kịch bản B: Back -> hiển thị cache cũ của /dashboard nhưng click menu -> gọi API không có token -> 401 -> redirect /login. Cả 2 kịch bản đều không cho phép thao tác tiếp với dữ liệu | Vừa logout |

**B. Xác minh dữ liệu bị xóa (LO-06 -> LO-08)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| LO-06 | Logout xóa toàn bộ key aisam_* trong localStorage | 1. Đăng nhập 2. Mở DevTools (F12) -> tab Application -> Storage -> Local Storage -> https://[domain] 3. Ghi nhận tất cả key hiện có: aisam_token, aisam_refresh_token, aisam_user, aisam_sidebar_open (nếu có), theme... 4. Click Logout 5. Ngay sau khi redirect về /login, kiểm tra lại Local Storage | Các key bị xóa: aisam_token, aisam_refresh_token, aisam_user. Các key không liên quan đến auth giữ nguyên: theme, aisam_sidebar_open (nếu có). Cookie aisam_role bị xóa (tab Application -> Cookies -> không còn aisam_role) | Đã đăng nhập |
| LO-07 | Logout xóa workspace cache, không ảnh hưởng auth key khác | 1. Đăng nhập, vào /overview để load workspace cache 2. Mở Console, gõ lệnh kiểm tra cache (nếu exposed) 3. Click Logout 4. Đăng nhập lại, vào /overview | Sau logout: workspace cache bị invalidate (cachedWorkspaces = null). Sau login lại: fetch lại danh sách workspace từ API, không dùng cache cũ. Không hiển thị dữ liệu workspace của phiên trước | Đã đăng nhập |
| LO-08 | Logout trên tab A, tab B mất quyền truy cập | 1. Mở tab A: đăng nhập, vào /dashboard 2. Mở tab B: truy cập cùng domain, vào /dashboard (dùng chung localStorage) 3. Tab A: click Logout -> redirect /login 4. Quay lại tab B, click menu "Content" hoặc "Brands" trong sidebar | Tab B: localStorage đã bị tab A xóa (aisam_token = null). Khi click menu -> apiClient gọi API không kèm Authorization header -> BE trả 401 -> handleResponse trong apiClient xóa token lần nữa + clear cookie + redirect window.location.href = "/login". Tab B chuyển sang /login | Cùng tài khoản, 2 tab |

**C. UI & Edge Cases (LO-09 -> LO-10)**

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| LO-09 | Nút Logout hiển thị đúng vị trí trên Header | 1. Đăng nhập user thường (role User) 2. Quan sát góc phải Header: click avatar/icon user 3. Đăng nhập Admin 4. Quan sát góc phải AdminHeader | User thường: Header góc phải có avatar/initials. Click -> dropdown hiển thị: tên user, email, divider, nút "Logout" (icon logout + text). Admin: tương tự nhưng trong layout Admin. Nút Logout luôn hiển thị ở cuối dropdown, phân biệt rõ với các mục khác | Đã đăng nhập |
| LO-10 | Double-click nút Logout | 1. Đăng nhập vào dashboard 2. Click nút Logout 2 lần liên tục thật nhanh (double-click) | Lần click 1: hàm logout() chạy -> gọi API -> xóa storage -> redirect /login. Lần click 2: trang đã redirect sang /login, hàm logout() không chạy lại vì component đã unmount. Chỉ có 1 request POST /api/auth/logout được gửi. Không crash, không lỗi console | Đã đăng nhập |

**Module:** AUTH | **Total:** 84 cases (74 + 10 LOGOUT) | API: /api/auth/*

---

## SHEET 2/19: WORKSPACE -- Workspace Management (22 cases)

| **Feature** | Workspace Management |
|---|---|
| **Test requirement** | Tạo Personal Workspace lần đầu (click card "Personal Workspace" tại /overview → auto-create với tên từ Full Name, redirect /dashboard); Tạo Business Workspace (click card "Business Workspace" → redirect /pricing?create=business → flow 3 bước có thanh toán); Đã có workspace → /overview hiển thị danh sách card workspace + nút Create Workspace (→ /pricing?create=business); Loading, double click, API lỗi khi tạo; Sidebar workspace switcher (xem danh sách, empty state, chuyển đổi); Workspace Settings (xem chi tiết, sửa tên - Owner, validate tên rỗng/quá dài, Member không sửa được); Dashboard summary; Access control (chưa đăng nhập, token hết hạn, không thuộc workspace, workspace bị admin xóa) |

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WS-01 | Tạo Personal Workspace lần đầu (0 workspace) | 1. Đăng nhập user mới, chưa có workspace 2. Vào /overview 3. Click card "Personal Workspace" (Free) | Tự động tạo workspace với tên "{FullName}'s Workspace" qua POST /profiles. Hiển thị "Creating workspace" + spinner. Redirect /dashboard sau 2s. Sidebar hiển thị workspace vừa tạo, role Owner | User mới, 0 workspace |
| WS-02 | Tạo Personal Workspace khi Full Name rỗng | 1. Đăng nhập user không có Full Name 2. Vào /overview 3. Click card "Personal Workspace" | Tự động tạo với tên "{email_prefix}'s Workspace". Redirect /dashboard. Sidebar hiển thị đúng tên | User mới, Full Name rỗng |
| WS-03 | Tạo Business Workspace từ /overview | 1. Đăng nhập user mới 2. Vào /overview 3. Click card "Business Workspace" (Pro) | Redirect sang /pricing?create=business. Hiển thị flow 3 bước: Overview → Name+Plan → Payment | User mới, 0 workspace |
| WS-04 | Đã có workspace → /overview hiển thị danh sách | 1. Đăng nhập user có 1+ workspace 2. Vào /overview | Hiển thị tiêu đề "Choose your workspace". Danh sách card workspace hiện có (tên, type badge Personal/Business, role). Nút "Go to Dashboard". Nút "Create Workspace" → /pricing?create=business | Có 1+ workspace |
| WS-05 | Click workspace card đã có → chọn workspace | 1. Đăng nhập user có 2 workspace 2. Vào /overview 3. Click card workspace thứ 2 | Toast hiển thị tên workspace. Redirect /dashboard sau 2s. Sidebar + dashboard hiển thị workspace vừa chọn | Có 2+ workspace |
| WS-06 | Nút Create Workspace khi đã có workspace | 1. Đăng nhập user có workspace 2. Vào /overview 3. Click nút "Create Workspace" | Redirect sang /pricing?create=business. Flow tạo Business Workspace có thanh toán | Có workspace |
| WS-07 | Loading state khi tạo Personal workspace | 1. User mới → /overview → click Personal Workspace 2. DevTools Slow 3G | Hiển thị spinner "Creating workspace" + "Setting up your environment...". Sau khi API OK → toast + redirect | Network chậm |
| WS-08 | Double click card Personal Workspace | 1. User mới → /overview → click Personal Workspace 2 lần nhanh | Lần 1: setCreating(true) → card disabled. Lần 2: không trigger do creating=true. Chỉ tạo 1 workspace | User mới, 0 workspace |
| WS-09 | Tạo Personal workspace thất bại (API lỗi) | 1. User mới → /overview → click Personal Workspace 2. API POST /profiles trả lỗi | Hiển thị message lỗi trong banner đỏ. Nút card không bị disable vĩnh viễn, có thể thử lại | API lỗi |
| WS-10 | Chưa đăng nhập truy cập /overview | 1. Logout 2. Truy cập /overview | Redirect về /login | Chưa đăng nhập |
| WS-11 | Token hết hạn khi load /overview | 1. Chờ token hết hạn 2. F5 tại /overview | Tự refresh token HOẶC redirect /login + Session expired | Token hết hạn |
| WS-12 | Xem danh sách workspace qua sidebar switcher | 1. Đăng nhập, có 2+ workspace 2. Click workspace switcher ở góc dưới sidebar | Dropdown hiển thị tất cả workspace: tên, initials, type badge (Personal/Business), plan. Active workspace được highlight (nền primary/8) | Có 2+ workspace |
| WS-13 | Empty state sidebar switcher | 1. User mới vào dashboard, chưa có workspace trong cache? | Dropdown hiển thị "No workspaces yet" + link "Manage workspaces" → /overview | 0 workspace |
| WS-14 | Chuyển đổi workspace qua sidebar switcher | 1. Đang ở workspace A 2. Mở switcher → chọn workspace B | Sidebar + toàn bộ dashboard cập nhật sang workspace B. URL giữ nguyên hoặc đổi context. Workspace B được highlight trong switcher | Có 2+ workspace |
| WS-15 | Xem chi tiết workspace trong Settings | 1. Đăng nhập → vào workspace bất kỳ 2. Sidebar → Settings (hoặc /profiles/[id]) 3. Tab "Workspace Info" | Hiển thị: tên workspace, loại (Personal/Business), ngày tạo, subscription, member role. Có nút Edit tên (nếu là Owner) | Đang trong workspace |
| WS-16 | Sửa tên workspace (Owner) | 1. Owner → Settings → Workspace Info 2. Click Edit tên → nhập "Updated WS" 3. Save | PUT /profiles/{id}. Toast "Workspace updated". Tên trong sidebar + Workspace Settings header cập nhật | User là Owner |
| WS-17 | Validate tên workspace khi sửa (rỗng) | 1. Owner → Settings → Workspace Info → Edit 2. Xóa trắng tên 3. Save | Hiển thị "Name is required". Không gửi được form | User là Owner |
| WS-18 | Validate tên workspace khi sửa (quá dài) | 1. Owner → Edit tên → nhập 256 ký tự 2. Save | Ghi nhận: nếu BE chặn → "Name must not exceed X characters". Nếu không → lưu thành công | User là Owner |
| WS-19 | Member thường không sửa được tên workspace | 1. Đăng nhập Member 2. Settings → Workspace Info | Không hiển thị nút Edit, hoặc nút bị disable. Chỉ xem được thông tin | User là Member |
| WS-20 | Dashboard summary tại /overview | 1. Đăng nhập, vào /overview (có workspace) | Hiển thị workspace cards với: tên, type badge, role, nút chọn. Nếu đang active → nút primary "Current". Thống kê brands, campaigns | Có workspace |
| WS-21 | User không thuộc workspace truy cập | 1. Copy URL workspace của user khác 2. Truy cập | "You don't have access" hoặc redirect về dashboard workspace của mình | Không thuộc workspace |
| WS-22 | Workspace bị admin xóa | 1. Admin soft-delete workspace 2. User đang trong workspace đó → F5 hoặc chuyển trang | Workspace mất khỏi sidebar switcher. Tự động chuyển sang workspace khác (nếu có). Nếu không còn workspace nào → redirect /overview | Workspace bị xóa |

**Module:** WORKSPACE | **Total:** 22 cases | API: /api/workspaces, /api/workspace-dashboard/summary

---

## SHEET 3/19: TEAM-MANAGEMENT -- Members & Invitations (34 cases)

| **Feature** | Team Management |
|---|---|
| **Test requirement** | Mời thành viên qua email (hợp lệ, không tồn tại, đã là thành viên, sai định dạng, rỗng, khoảng trắng); quản lý lời mời (gửi, xem, thu hồi, chấp nhận, token không hợp lệ, hết hạn, đã accept, bị thu hồi, accept sai email); phân quyền (Owner/Manager/Member, đổi role, member không có quyền); quota thành viên; chuyển quyền sở hữu (hủy transfer, Manager không transfer); xóa thành viên; loading, double click, chưa đăng nhập, token hết hạn |

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WM-01 | Mời thành viên bằng email hợp lệ | 1. Đăng nhập Owner 2. Vào Team Management 3. Click Invite Member 4. Nhập Email: member@example.com 5. Chọn Role: Member, Quota: Shared 6. Click Send Invitation | Hiển thị Invitation sent. Lời mời xuất hiện ở Pending. Người nhận có email | Owner, email chưa là thành viên |
| WM-02 | Mời email chưa đăng ký | 1. Nhập Email: newuser@example.com 2. Click Send | Invitation sent. User đăng ký sau vẫn accept được | Email chưa tồn tại |
| WM-03 | Mời email đã là thành viên | 1. Nhập Email của thành viên hiện tại 2. Click Send | User is already a member of this workspace | Đã là thành viên |
| WM-04 | Mời lại email đang có lời mời pending | 1. Email đã có lời mời 2. Mời lại | Ghi nhận: nếu chặn -> An invitation has already been sent. Nếu không -> gửi lại email mới | Có lời mời pending |
| WM-05 | Mời email rỗng | 1. Để trống Email 2. Click Send | Email is required dưới ô Email | -- |
| WM-06 | Mời email sai định dạng | 1. Nhập Email: notanemail 2. Click Send | Please enter a valid email address | -- |
| WM-07 | Mời email có khoảng trắng đầu/cuối | 1. Nhập "  member@example.com  " 2. Click Send | Ghi nhận: FE trim -> gửi thành công. Không trim -> có thể lỗi | -- |
| WM-08 | Mời email tiếng Việt có dấu | 1. Nhập người.dùng@thương-hiệu.vn 2. Click Send | Ghi nhận: hỗ trợ unicode email -> gửi thành công | -- |
| WM-09 | Member thường cố gửi lời mời | 1. Đăng nhập Member 2. Vào Team Management | Không hiển thị nút Invite | User là Member |
| WM-10 | Loading state khi gửi lời mời | 1. Nhập email 2. Click Send | Nút Sending... + spinner + disable. Xong -> thông báo | -- |
| WM-11 | Double click Send Invitation | 1. Click Send 2 lần | Chỉ gửi 1 lời mời. 1 email | -- |
| WM-12 | Xem danh sách lời mời pending | 1. Tab Pending Invitations | Danh sách: email, role, quota, ngày gửi, nút Revoke | Có lời mời |
| WM-13 | Thu hồi lời mời | 1. Click Revoke 2. Xác nhận | Invitation revoked. Lời mời biến mất. Link cũ -> không hợp lệ | Lời mời pending |
| WM-14 | Chấp nhận lời mời thành công | 1. Click link email 2. Đăng nhập (nếu cần) 3. Click Join Workspace | You have joined [WS]. Workspace vào sidebar | Token hợp lệ |
| WM-15 | Token không hợp lệ | 1. Truy cập link token bịa đặt | Invalid or expired invitation token | Token sai |
| WM-16 | Lời mời đã bị thu hồi | 1. Owner revoke 2. Người nhận click link cũ | This invitation is no longer valid | Đã revoke |
| WM-17 | Accept khi đăng nhập email khác | 1. A nhận lời mời 2. Đang login B 3. Click link | Ghi nhận: nếu chặn -> This invitation is for a different email | Login email khác |
| WM-18 | Xem danh sách thành viên | 1. Tab Members | Danh sách: avatar, tên, email, role badge, quota, ngày join | Có 2+ thành viên |
| WM-19 | Owner đổi role Member -> Manager | 1. Action menu -> Change Role -> Manager | Role updated. Badge đổi. Quyền quản lý thêm | Owner |
| WM-20 | Member cố đổi role | 1. Member vào Team Management | Không có nút Change Role | User là Member |
| WM-21 | Owner thay đổi quota thành viên | 1. Edit quota -> Limited, Credit Limit: 100 | Member quota updated. Hiển thị Limited (100) | Owner |
| WM-22 | Xóa thành viên | 1. Owner click Remove 2. Xác nhận | Member removed. WS mất khỏi sidebar người bị xóa | Owner |
| WM-23 | Owner tự xóa mình | 1. Owner tìm nút Remove | Không hiển thị. API: Cannot remove owner | Owner duy nhất |
| WM-24 | Chuyển quyền sở hữu | 1. Owner -> Manager -> Transfer Ownership -> Confirm | Ownership transferred. Cũ -> Manager, Mới -> Owner | Có Manager |
| WM-25 | Chuyển cho Member thường | 1. Transfer cho Member | Ghi nhận: nếu chặn -> chỉ Manager+. Nếu không -> Member lên Owner | Có Member |
| WM-26 | Không có Manager để transfer | 1. WS chỉ có Owner | Danh sách trống hoặc "Need at least one Manager" | Chỉ có Owner |
| WM-27 | Chưa đăng nhập click link invitation | 1. Logout 2. Click link | Redirect /login, sau login -> accept | Chưa đăng nhập |
| WM-28 | Token hết hạn khi quản lý members | 1. Chờ token hết hạn 2. Thực hiện thao tác | Refresh token hoặc redirect /login + Session expired | Token hết hạn |
| WM-29 | Accept lại lời mời đã dùng | 1. Đã accept 2. Click link lần nữa | You have already joined hoặc invitation already accepted | Đã accept |
| WM-30 | Lời mời hết hạn | 1. Chờ quá expiry 2. Click link | This invitation has expired. Link Request new invitation | Lời mời quá hạn |
| WM-31 | Hủy chuyển quyền sở hữu | 1. Owner bắt đầu transfer 2. Click Cancel trước confirm cuối | Transfer bị hủy. Owner vẫn Owner. Không thay đổi | Đang transfer |
| WM-32 | Owner không thể tự đổi role | 1. Owner chọn mình 2. Tìm Change Role | Không hiển thị. API: Cannot change owner role | User là Owner |
| WM-33 | Manager không thể xóa Owner | 1. Manager vào Team Management 2. Tìm Remove trên Owner | Không có nút Remove. API: No permission | User là Manager |
| WM-34 | Manager không thể transfer ownership | 1. Manager vào Team Management | Không có nút Transfer. API: Only owner can transfer | User là Manager |

**Module:** TEAM-MANAGEMENT | **Total:** 34 cases | API: /api/workspace-members, /api/workspace-invitations

---

## SHEET 4/19: WS-SETTINGS -- Workspace Settings (53 cases)

| **Feature** | Workspace Settings |
|---|
| **Test requirement** | Truy cập Settings từ Header/Sidebar; điều hướng 6 tab (Overview, Workspace Info, Team, Security, Billing & Credits, Subscription); URL deep-link từng tab (?section=); tab Overview (KPI cards, top members, quick actions, refresh); tab Workspace Info (xem, sửa tên, cancel edit, avatar, status badge); tab Security (password strength meter, toggle visibility, inline validation); tab Billing & Credits (sub-tab toggle Overview/Usage, payment history, credit history filter); tab Subscription (plan comparison, credit packs, banners Expired/Limited/Archived, cancel); access control (chưa đăng nhập, token hết hạn, Member, workspace khác); loading, error, double click |

### 4.1 NAVIGATION & TAB SWITCHING (WS-01 -> WS-10)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WS-01 | Mở Settings từ Header | 1. Đăng nhập 2. Click icon Settings (bánh răng) trên Header | Chuyển đến /profiles/[workspaceId]. Sidebar WorkspaceSettingsSidebar hiển thị 6 tab. Tab Overview được chọn mặc định | Đã đăng nhập, có workspace |
| WS-02 | Mở Settings từ Sidebar | 1. Đăng nhập 2. Sidebar -> Settings | Giống WS-01. Sidebar hiển thị đúng workspace hiện tại | Đã đăng nhập, có workspace |
| WS-03 | Chuyển tab qua sidebar | 1. Click lần lượt từng tab: Overview, Workspace Info, Team, Security, Billing & Credits, Subscription | Mỗi tab hiển thị nội dung tương ứng. Tab đang active có nền primary/10, text primary. URL cập nhật ?section= | Đang ở Settings |
| WS-04 | URL deep-link ?section= | 1. Truy cập /profiles/[id]?section=billing 2. Truy cập /profiles/[id]?section=team 3. Truy cập /profiles/[id]?section=security 4. Truy cập /profiles/[id]?section=subscription | Mỗi lần mở đúng tab tương ứng. Tab Overview là mặc định nếu không có ?section | Đã đăng nhập |
| WS-05 | URL deep-link section không hợp lệ | 1. Truy cập /profiles/[id]?section=invalid | Mở tab Overview (mặc định). Không crash, không lỗi | Đã đăng nhập |
| WS-06 | Chuyển tab nhanh liên tục | 1. Click nhanh qua lại giữa 2-3 tab | Chỉ hiển thị nội dung tab cuối cùng. Không bị race condition, không crash | Đang ở Settings |
| WS-07 | Back/Forward browser | 1. Vào tab Billing 2. Click back 3. Click forward | Back: quay về trang trước Settings. Forward: về tab Billing với ?section=billing | Đã navigate qua các tab |
| WS-08 | Link "All Profiles" ở cuối sidebar | 1. Click "All Profiles" ở cuối WorkspaceSettingsSidebar | Chuyển về /overview | Đang ở Settings |
| WS-09 | Chưa đăng nhập truy cập /profiles/[id] | 1. Logout 2. Truy cập /profiles/[id] | Redirect về /login?redirect=/profiles/[id] | Chưa đăng nhập |
| WS-10 | User không thuộc workspace truy cập Settings | 1. User A copy URL /profiles/[workspaceB_id] 2. Truy cập | Hiển thị "You are not a member of this workspace" hoặc redirect về dashboard | A không thuộc workspace B |

### 4.2 OVERVIEW TAB (WS-11 -> WS-20)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WS-11 | Xem KPI cards Overview | 1. Vào tab Overview | Hiển thị 4 KPI cards: Credits Remaining (balance/max + progress bar), Posts This Month (used/total + progress bar), Total AI Usage (số lần), Workspace Type (plan + badge) | Có workspace với data |
| WS-12 | Credits KPI hiển thị đúng | 1. So sánh Credits Remaining với /credit-usage/wallet API | Balance, maxBalance, % remaining khớp. Progress bar tỉ lệ đúng | Có data |
| WS-13 | Posts KPI hiển thị đúng | 1. So sánh Posts This Month với /quota/workspace/current API | Used, total, % remaining khớp. Progress bar tỉ lệ đúng | Có data |
| WS-14 | AI Usage KPI hiển thị đúng | 1. So sánh Total AI Usage với /workspace-dashboard/summary API | Số lần AI usage khớp dashboard summary | Có data |
| WS-15 | Credits KPI edge: balance = 0 | 1. Dùng hết credits 2. Vào tab Overview | Hiển thị 0 credits, progress bar rỗng hoặc 0%, label "0% remaining" | Balance = 0 |
| WS-16 | Credits KPI edge: balance >= max | 1. Balance bằng hoặc vượt max 2. Vào Overview | Progress bar 100%. Không bị tràn layout | Balance >= max |
| WS-17 | Top Members hiển thị | 1. Workspace có 2+ member có AI usage 2. Vào tab Overview | Danh sách xếp hạng: avatar, tên, usage bar. Người dùng nhiều nhất xếp trên | Có member data |
| WS-18 | Top Members empty state | 1. Workspace mới, chưa có AI usage 2. Vào tab Overview | Hiển thị "No member data yet" | 0 AI usage |
| WS-19 | Refresh button | 1. Click nút Refresh trên Overview header | Hiển thị spinner khi loading. Data KPI + top members được reload. Không reload toàn trang | Đang ở tab Overview |
| WS-20 | Quick Actions navigation | 1. Click "Generate Content" 2. Click "View Posts" 3. Click "Buy Credits" 4. Click "Manage Team" | Generate Content -> /content/ai-generate. View Posts -> /posts. Buy Credits -> tab Billing & Credits. Manage Team -> tab Team | Đang ở tab Overview |

### 4.3 WORKSPACE INFO TAB (WS-21 -> WS-35)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WS-21 | Xem thông tin workspace | 1. Vào tab Workspace Info | Hiển thị avatar/inital, tên workspace, status badge (Active/Suspended/...), plan, Owner badge (nếu là Owner), company name, Type, Description, Avatar URL, Created date | Đang ở Settings |
| WS-22 | Avatar hiển thị đúng (initials) | 1. Workspace không có avatar URL 2. Vào tab Workspace Info | Hiển thị initials: lấy chữ cái đầu các từ trong tên (vd: "My Workspace" -> "MW") | Không có avatarUrl |
| WS-23 | Avatar hiển thị đúng (image) | 1. Workspace có avatarUrl 2. Vào tab Workspace Info | Hiển thị ảnh từ URL. Không bị vỡ/lỗi | Có avatarUrl hợp lệ |
| WS-24 | Status badge hiển thị đúng | 1. Kiểm tra lần lượt: Active, Pending, Suspended, Cancelled | Active: emerald + pulse dot. Pending: amber. Suspended: red. Cancelled: gray | Các trạng thái khác nhau |
| WS-25 | Owner badge | 1. Owner vào tab Workspace Info | Hiển thị badge "Owner" màu amber với icon star bên cạnh tên workspace | User là Owner |
| WS-26 | Member không thấy Owner badge | 1. Member vào tab Workspace Info | KHÔNG hiển thị badge "Owner". Chỉ hiển thị tên workspace | User là Member |
| WS-29 | Cancel edit mode | 1. Owner click Edit 2. Thay đổi tên 3. Click Cancel | Về view mode. Tên giữ nguyên giá trị cũ. Không gọi API | User là Owner |
| WS-31 | Double-click Save Changes | 1. Owner Edit -> đổi tên -> click Save 2 lần nhanh | Lần 1: nút loading "Saving...", disable. Lần 2 không trigger. Chỉ 1 PUT request | User là Owner |
| WS-32 | Loading skeleton | 1. Vào tab Workspace Info khi data chưa load | Hiển thị skeleton cards (placeholder xám) cho đến khi data load xong | Network chậm |
| WS-33 | Network error khi Save | 1. Owner Edit -> đổi tên -> ngắt mạng -> Save | Hiển thị error banner. Form giữ data đã edit. Có mạng lại -> Save OK | User là Owner |
| WS-34 | API error khi Save | 1. Backend trả lỗi (vd: trùng tên) 2. Owner Save | Hiển thị "Update failed" error banner kèm nội dung lỗi từ API | User là Owner |
| WS-35 | Workspace Info: tất cả field hiển thị | 1. So sánh với GET /api/workspaces response | Tên, Type, Company, Description, Avatar URL, Created date khớp API. Field null hiển thị "—" | Có workspace data |

### 4.4 SECURITY TAB (WS-36 -> WS-45)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WS-36 | Security Status Card | 1. Vào tab Security | Card nền xanh: icon shield, "Account Security", badge "Secure" màu emerald | Đang ở Settings |
| WS-37 | Toggle password visibility (3 field) | 1. Nhập vào Current Password, New Password, Confirm Password 2. Click icon mắt từng field | Mỗi field toggle độc lập: hiện text / ẩn thành dấu chấm. Icon đổi visibility/visibility_off | Đang ở tab Security |
| WS-38 | Password strength meter – Weak | 1. Nhập New Password: "abc" (3 ký tự) | Bar 33%, màu đỏ, label "Weak". Nút Update bị disable | Đang ở tab Security |
| WS-39 | Password strength meter – Medium | 1. Nhập New Password: "abcdefgh" (8 ký tự, toàn chữ) | Bar 66%, màu vàng/cam, label "Medium" | Đang ở tab Security |
| WS-40 | Password strength meter – Strong | 1. Nhập New Password: "MyP@ssw0rd2024!" (15+ ký tự, chữ + số + đặc biệt) | Bar 100%, màu emerald, label "Strong" | Đang ở tab Security |
| WS-41 | Inline Confirm Password mismatch | 1. Nhập New Password: "Pass1234" 2. Nhập Confirm: "Pass5678" | Hiển thị text đỏ "Passwords do not match" ngay dưới ô Confirm. Biến mất khi nhập khớp | Đang ở tab Security |
| WS-45 | Security Tips hiển thị | 1. Kéo xuống cuối tab Security | Hiển thị 4 tips với icon check_circle: 12+ chars, tránh thông tin cá nhân, bật 2FA, đổi 3-6 tháng/lần | Đang ở tab Security |

### 4.5 BILLING & CREDITS TAB (WS-46 -> WS-54)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WS-46 | Sub-tab toggle Overview/Usage | 1. Vào tab Billing & Credits 2. Click "Usage" 3. Click "Overview" | Overview: hiển thị Credit Wallet card + Usage cards + Payment History. Usage: hiển thị Credit Usage Summary + Credit History. Tab active được highlight | Đang ở tab Billing |
| WS-47 | Credit Wallet card hiển thị | 1. Vào sub-tab Overview của Billing | Card nền xanh: balance hiển thị, badge "Active". So khớp /credit-usage/wallet API | Có credit data |
| WS-48 | Usage cards hiển thị | 1. Xem 3 cards: AI Credits, Posts Published, Team Members | Mỗi card có progress bar (% used/total), icon, label. Giá trị khớp wallet + dashboard APIs | Có data |
| WS-49 | Payment History hiển thị | 1. Vào sub-tab Overview | Danh sách thanh toán: date, method, amount (USD), status badge (Completed=emerald, Pending=amber, Failed=red). Có phân trang nếu > 10 records | Có payment history |
| WS-50 | Payment History empty state | 1. Workspace chưa có payment 2. Vào sub-tab Overview | Icon receipt + "No payment history yet" | 0 payments |
| WS-51 | Credit History filter tabs | 1. Vào sub-tab Usage 2. Click All / Success / Failed tabs | All: tất cả records. Success: chỉ Success. Failed: chỉ Failed, credits=0. Tab active highlight | Có credit history |
| WS-52 | Credit History pagination | 1. Có > 10 records 2. Click Next / Previous / page number | Page thay đổi, list cập nhật. Previous disable ở page 1. Next disable ở page cuối | > 10 records |
| WS-53 | Credit History empty state | 1. Workspace mới 2. Vào sub-tab Usage | Icon + "No credit usage yet" | 0 credit records |
| WS-54 | Download Invoice button | 1. Click nút "Download Invoice" | Hiển thị toast "Feature coming soon!". Không crash | Đang ở tab Billing |

### 4.6 SUBSCRIPTION TAB (WS-55 -> WS-62)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WS-55 | Current Plan Card hiển thị | 1. Vào tab Subscription | Card gradient: plan icon, plan name, status badge Active (emerald + pulse dot), end date, Billing Cycle="Monthly", Next Payment date, Start Date. Nếu Free plan -> có nút "Upgrade Plan" | Có subscription active |
| WS-56 | Plan Comparison Grid (Personal) | 1. Personal workspace 2. Vào tab Subscription | Hiển thị 3 plans: Free, Personal Plus, Personal Pro. "Most Popular" trên Personal Plus. "Current Plan" trên plan hiện tại. Mỗi plan có checklist features | Personal workspace |
| WS-57 | Plan Comparison Grid (Business) | 1. Business workspace 2. Vào tab Subscription | Hiển thị 2 plans: Business Plus, Business Pro. "Most Popular" trên Business Plus | Business workspace |
| WS-58 | Credit Packs hiển thị | 1. Vào tab Subscription, kéo xuống Buy Credits | Hiển thị wallet balance chip. 4 packs: Starter (100cr/2,000đ), Standard (500cr/3,000đ) + Best Value badge, Growth (1,500cr/4,000đ), Business (5,000cr/5,000đ). Mỗi pack có nút Purchase | Có subscription |
| WS-62 | Subscription Banners (Expired/Limited/Archived) | 1. Kiểm tra banner khi subscription Expired 2. Limited mode 3. Archived | Expired: banner amber, credits remaining, nút Renew + Dismiss. Limited: banner đỏ, ngày expired, danh sách tính năng bị khóa, nút Renew Now + Dismiss. Archived: banner xám, cảnh báo xóa sau 180 ngày, nút Renew + Export Data + Dismiss | Subscription hết hạn |

**Module:** WS-SETTINGS | **Total:** 53 cases | API: /api/workspaces, /api/workspace-members, /api/credit-usage, /api/payment, /api/quota

---

## SHEET 5/19: BRAND -- Brand Kit (63 cases)

| **Feature** | Brand Kit |
|---|---|
| **Test requirement** | CRUD brand: tạo (đầy đủ, tối thiểu), xem (danh sách, chi tiết, phân trang, empty state), sửa (text, Logo URL), xóa mềm & khôi phục; validate (Name rỗng, 1 ký tự, quá dài, khoảng trắng, emoji, ký tự đặc biệt, tiếng Việt, HTML/XSS, trùng tên); Logo URL (hợp lệ, sai định dạng, 404, HTML, không nhập); search (có/không kết quả, có dấu/không dấu, ký tự đặc biệt, khoảng trắng, xóa search, case-insensitive); sort (Name, Date); filter (includeDeleted, kết hợp search+sort); access control (chưa đăng nhập, token, cross-workspace, Member); loading, double click, mất mạng, double delete, restore active, xóa brand có products, hủy delete, brand trong dropdown content/campaign |

*(63 cases detailed in conversation -- abbreviated here for file size)*

Key cases: BR-01 to BR-63 covering all CRUD, validation, search, sort, filter, access control, and cross-feature display (dropdown in Content, Campaign)

**Module:** BRAND | **Total:** 63 cases | API: /api/brands

---

## SHEET 6/19: PRODUCT -- Product Management (66 cases)

| **Feature** | Product Management |
|---|---|
| **Test requirement** | CRUD product: tạo (đầy đủ, tối thiểu, upload ảnh), xem (danh sách, chi tiết, gallery ảnh, phân trang, empty state), sửa (text, ảnh, brand), xóa mềm & khôi phục; validate (Name, BrandId, Price, Stock, ảnh); gallery (1 ảnh, 0 ảnh, broken URL); search + filter + sort; access control; loading, double click, mất mạng, double delete, trùng tên khác brand, sửa sang brand deleted |

*(66 cases detailed in conversation -- abbreviated here for file size)*

Key cases: PD-01 to PD-66 covering all CRUD, gallery edge cases, search+filter+sort combinations, and cross-feature validation

**Module:** PRODUCT | **Total:** 66 cases | API: /api/products

---

## SHEET 7/19: AI -- AI Generate & Assistant (83 cases)

| **Feature** | AI Generate (AI Content Creation) |
|---|---|
| **Test requirement** | Toàn bộ luồng tạo content bằng AI từ `/content/ai-generate`: chat AI Assistant, Apply kết quả vào Editor, Post Preview (Facebook/Instagram/TikTok), Save Post, View Post; Quick Templates (Longer, Formal, Casual, Hashtags, Bullet, Emoji); 2 chế độ (Bảo toàn sản phẩm / Sáng tạo tự do); tạo ảnh/video qua chat; Chat History; Credit consumption; AI Generation Status card trong content detail; validate, loading, typing indicator, double click, mất mạng |

*(83 cases detailed in conversation -- abbreviated here for file size)*

Key flows: Chat with AI -> Apply to editor -> Edit manually -> Save Post -> View Post. Image/Video generated via chat with [IMAGE: url] / [VIDEO_JOB: id] format.

**Module:** AI | **Total:** 83 cases | Pages: /content/ai-generate, /content/{id}

---

## SHEET 8/19: CONTENT-CREATE -- Manual Content Creation (32 cases)

| **Feature** | Manual Content Creation -- Tạo content thủ công (không qua AI) |
|---|---|
| **Test requirement** | Tạo content thủ công với form đầy đủ: BrandId (required), ProductId (required in FE), AdType (TextOnly=0 / ImageText=1 / VideoText=2), Title (max 255), TextContent, ImageUrl (upload drag-drop), VideoUrl (upload drag-drop), Tags (TagPicker), StyleDescription, ContextDescription, Status (Draft=0 / AwaitingApproval=1); Platform selection; Live Preview real-time; Upload media; Character/word counter; Hashtags input; Validate; Loading, double click, mất mạng |
| **Pages** | `/content/create` |
| **API** | POST `/content`, POST `/content/media` |

### 8.1 CREATE CONTENT -- Tạo content cơ bản (CT-01 -> CT-10)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CT-01 | Tạo content TextOnly đầy đủ fields với status Draft | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Từ sidebar, click Content 5. Click nút "Create New Content" góc trên phải 6. Click "Manual Creation" trong dropdown 7. Tại ô Title, nhập: "Khuyến mãi Tết 2026" 8. Tại dropdown Brand, chọn brand "My Brand" 9. Tại dropdown Product, chọn product "Áo Thun Cotton" 10. Tại Content Type, click "Text" 11. Tại ô Content Body, nhập: "Chương trình ưu đãi đặc biệt giảm giá 50% tất cả sản phẩm nhân dịp Tết Nguyên Đán 2026." 12. Tại ô Description, nhập: "Mô tả chương trình khuyến mãi Tết" 13. Tại ô Caption, nhập: "Đón Tết rộn ràng cùng ưu đãi khủng! #Tet2026" 14. Tại Publishing Settings, chọn Status: "Draft" 15. Tại Tags, click TagPicker, chọn "Promotion" và "Seasonal" 16. Click nút "Save Content" | Toast hiển thị **"Content created successfully"** (icon check). Sau 1 giây redirect về /content. Content mới xuất hiện trong Grid: Title "Khuyến mãi Tết 2026", type badge TEXT (màu tím), status badge Draft (xám), brand "My Brand", tags hiển thị "Promotion" + "Seasonal" | Đã đăng nhập, workspace có brand "My Brand" và product "Áo Thun Cotton" |
| CT-02 | Tạo content ImageText với upload ảnh | 1. Đăng nhập test@example.com / Pass1234 2. Content -> Create New Content -> Manual Creation 3. Title: "BST Mùa Hè 2026" 4. Chọn Brand: "My Brand", Product: "Áo Thun Cotton" 5. Content Type: click "Image" 6. Tại vùng Upload Image, click "Click to upload" 7. Chọn file product-image.jpg (2MB, 1200x800) từ máy 8. Quan sát preview ảnh hiển thị trong khung dashed border 9. Nhập Caption: "Bộ sưu tập mùa hè đã có mặt!" 10. Status: Draft 11. Click Save Content | Toast hiển thị **"Content created successfully"**. Redirect về /content. Card hiển thị thumbnail = ảnh vừa upload. Type badge IMAGE (xanh dương). Preview Facebook/Instagram hiển thị ảnh đúng. API payload adType=1 | File product-image.jpg <10MB |
| CT-03 | Tạo content VideoText với upload video | 1. Đăng nhập test@example.com / Pass1234 2. Content -> Create New Content -> Manual Creation 3. Title: "Hướng dẫn sử dụng sản phẩm" 4. Chọn Brand: "My Brand", Product: "Áo Thun Cotton" 5. Content Type: click "Video" 6. Click "Click to upload" trong vùng Upload Video 7. Chọn file tutorial.mp4 (30MB, 30 giây) 8. Quan sát preview video với controls play/pause 9. Nhập Duration: "0:30" 10. Nhập Caption: "Xem ngay cách phối đồ với áo thun!" 11. Status: Draft 12. Click Save Content | Toast hiển thị **"Content created successfully"**. Card hiển thị thumbnail video + icon play. Type badge VIDEO (hồng). Detail có video player. API payload adType=2 | File tutorial.mp4 <100MB |
| CT-04 | Tạo content tối thiểu (chỉ Title + Brand + Product + Type TEXT, bỏ trống field còn lại) | 1. Đăng nhập test@example.com / Pass1234 2. Content -> Create New Content -> Manual Creation 3. Title: "Test" 4. Brand: để auto-select brand đầu tiên 5. Product: chọn 1 product 6. Type: TEXT (mặc định) 7. Bỏ trống Content Body, Description, Caption, Tags, Hashtags, Platforms 8. Status: mặc định "Awaiting Approval" 9. Click Save Content | Toast hiển thị **"Content created successfully"**. TextContent = "" (do caption và description đều trống). Status = Awaiting Approval, card hiển thị badge vàng. Không có thumbnail | Có brand + product |
| CT-05 | Tạo content với Status = Awaiting Approval | 1. Đăng nhập -> Content -> Create 2. Nhập Title: "Bài cần duyệt" 3. Chọn Brand + Product 4. Type: TEXT, nhập Content Body: "Nội dung cần được duyệt trước khi đăng" 5. Tại Publishing Settings -> Status: click nút "Awaiting Approval" 6. Click Save Content | Toast hiển thị **"Content created successfully"**. Status=1 trong DB. Card hiển thị badge vàng "Awaiting Approval". Content xuất hiện ở filter "Awaiting Approval". Sẵn sàng để approver duyệt tại /approvals | -- |
| CT-06 | Thiếu Title -> nút Save bị disable | 1. Đăng nhập -> Content -> Create 2. Bỏ trống ô Title 3. Chọn Brand + Product 4. Nhập Content Body đầy đủ 5. Quan sát nút "Save Content" | Nút **"Save Content"** bị disable: màu xám, opacity 50%, cursor not-allowed. Không thể click. Điều kiện isValid = title.trim().length > 0 && productId && brandId.length > 0 | -- |
| CT-07 | Thiếu Product -> nút Save bị disable | 1. Đăng nhập -> Content -> Create 2. Title: "Test" 3. Chọn Brand 4. Tại dropdown Product: giữ nguyên option "Select product" 5. Nhập đầy đủ Content Body 6. Quan sát nút Save | Nút **"Save Content"** bị disable. form.productId rỗng -> isValid = false | -- |
| CT-08 | Workspace chưa có Brand nào | 1. Đăng nhập vào workspace mới chưa tạo brand 2. Vào Content -> Create New Content -> Manual Creation 3. Quan sát dropdown Brand và Product | Dropdown Brand trống (fetchBrands trả về []). Dropdown Product trống. Nút **"Save Content"** bị disable. Người dùng phải tạo brand trước | Workspace chưa có brand |
| CT-09 | Title 256 ký tự (vượt max 255) | 1. Đăng nhập -> Content -> Create 2. Nhập Title dài 256 ký tự (copy từ notepad) 3. Nhập đầy đủ Brand, Product, Content Body 4. Click Save Content | BE trả lỗi validation: **"Title must not exceed 255 characters"**. Toast lỗi hiển thị (icon error). Form giữ nguyên data đã nhập, nút Save hết disabled | -- |
| CT-10 | TextContent bỏ trống nhưng có Caption hoặc Description | 1. Đăng nhập -> Content -> Create (Type TEXT) 2. Title: "Test" 3. Bỏ trống Content Body 4. Nhập Description: "Mô tả" 5. Nhập Caption: "Caption social" 6. Click Save Content | Toast hiển thị **"Content created successfully"**. FE map textContent = textContent || caption || description || "". Content tạo với textContent = "Caption social" (caption ưu tiên hơn description). Detail hiển thị đúng "Caption social" | -- |

### 8.2 CONTENT TYPE & MEDIA UPLOAD (CT-11 -> CT-21)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CT-11 | Drag-drop ảnh vào vùng upload | 1. Đăng nhập -> Content -> Create -> Type IMAGE 2. Mở File Explorer, tìm file sunset.png (1.5MB) 3. Kéo file từ Explorer thả vào vùng dashed border "Click to upload" 4. Quan sát vùng upload khi đang drag và sau khi thả | Khi drag qua: border đổi màu primary, background primary/5. Khi thả: preview ảnh hiển thị, nút Replace (biểu tượng refresh) góc trên phải, nút Remove (X) góc trên phải. Text dưới ảnh: "Click to replace or drag a new image" | File sunset.png 1.5MB |
| CT-12 | Replace ảnh đã upload | 1. Đăng nhập -> Content -> Create -> Type IMAGE 2. Upload ảnh A (photo1.jpg) 3. Click nút Replace (biểu tượng refresh) trên ảnh A 4. Chọn ảnh B (photo2.jpg) từ file picker 5. Quan sát preview | Preview chuyển từ ảnh A sang ảnh B. Ảnh A bị revoke object URL. imageFileRef.current = file B. Khi Save -> chỉ upload ảnh B. Không có toast | Có 2 file ảnh khác nhau |
| CT-13 | Remove ảnh đã upload | 1. Đăng nhập -> Content -> Create -> Type IMAGE 2. Upload ảnh bất kỳ 3. Click nút X (Close) trên ảnh 4. Quan sát vùng upload | Vùng upload về trạng thái ban đầu: icon add_photo_alternate, text "Click to upload" / "or drag and drop your image here" / "PNG, JPG, WebP up to 10MB". Ảnh cũ bị xóa. Khi Save -> imageUrl = undefined | Đã upload ảnh |
| CT-14 | Upload ảnh 12MB (vượt label 10MB nhưng dưới code limit 50MB) | 1. Đăng nhập -> Content -> Create -> Type IMAGE 2. Upload file photo.jpg (12MB) 3. Nhập Title + Brand + Product 4. Click Save Content | Label hiển thị "up to 10MB" nhưng không enforced. Upload thành công vì 12MB < 50MB (code limit). Toast hiển thị **"Content created successfully"** | File 12MB |
| CT-15 | Upload ảnh >50MB (vượt code limit) | 1. Đăng nhập -> Content -> Create -> Type IMAGE 2. Upload file photo-big.jpg (51MB) 3. Click Save Content | uploadFile throw Error: **"photo-big.jpg is larger than 50 MB. Please compress the video before uploading."** Toast/setSaveError hiển thị message lỗi (màu đỏ). Form không submit. Nút hết disabled | File 51MB |
| CT-16 | Upload file PDF (Type IMAGE) | 1. Đăng nhập -> Content -> Create -> Type IMAGE 2. Click "Click to upload" 3. Trong file picker của OS, thử chọn document.pdf | Input accept="image/*" -> file picker lọc chỉ hiện file ảnh. Nếu user bypass "All Files" và chọn PDF -> BE từ chối, trả lỗi **"Unsupported file format"** | File PDF |
| CT-17 | Upload file .mkv (Type VIDEO) | 1. Đăng nhập -> Content -> Create -> Type VIDEO 2. Click upload video, chọn video.mkv | Input accept="video/*" -> .mkv có thể chọn được (tùy browser). uploadFile gửi lên BE, BE kiểm tra MIME -> nếu không hỗ trợ trả **"Unsupported video format"**. Toast lỗi hiển thị | File .mkv |
| CT-18 | Chuyển đổi Content Type TEXT -> IMAGE -> VIDEO và ngược lại | 1. Đăng nhập -> Content -> Create 2. Type TEXT, nhập Content Body: "Nội dung text" 3. Click nút "Image" -> UI chuyển sang vùng upload ảnh 4. Click nút "Video" -> UI chuyển sang vùng upload video 5. Click nút "Text" -> UI quay lại textarea 6. Quan sát nội dung textarea | Sau khi quay lại TEXT: textarea vẫn hiển thị "Nội dung text" (không bị mất). Description và Caption luôn hiển thị bất kể type. Brand, Product, Title, Tags không bị reset khi chuyển type | -- |
| CT-19 | Character + word counter cho TextContent và Caption | 1. Đăng nhập -> Content -> Create (Type TEXT) 2. Nhập "Hello world!" (12 ký tự) vào Content Body 3. Quan sát counter dưới textarea 4. Nhập "Hello world! This is a test." (29 ký tự) vào Caption 5. Quan sát counter dưới Caption textarea | Content Body counter hiển thị **"12 characters"** và **"2 words"**. Caption counter hiển thị **"29 characters"** và **"6 words"**. Cả 2 counter cập nhật real-time khi gõ/xóa, hoạt động độc lập | -- |
| CT-20 | Description field resize được | 1. Đăng nhập -> Content -> Create 2. Nhập text dài 5 dòng vào Description 3. Kéo góc dưới bên phải textarea để resize | Textarea resize được theo chiều dọc (CSS resize-y). Min height 80px. Không resize được chiều ngang | -- |
| CT-21 | Thumbnail upload riêng (độc lập với ảnh chính) | 1. Đăng nhập -> Content -> Create 2. Type TEXT (không có ảnh chính) 3. Scroll xuống Publishing Settings -> Thumbnail 4. Click "Click to upload thumbnail image" 5. Chọn file thumb.jpg 6. Click Save Content | **[BUG]** Thumbnail có input riêng nhưng CreateContentPayload không có field thumbnail -> KHÔNG gửi lên BE. Sau save, thumbnail bị mất. Toast vẫn hiển thị **"Content created successfully"** nhưng thumbnail không được lưu | File thumb.jpg |

### 8.3 PLATFORM PREVIEW (CT-22 -> CT-26)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CT-22 | Preview Facebook real-time | 1. Đăng nhập test@example.com / Pass1234 2. Content -> Create -> Manual Creation 3. Nhập Title: "Bài test Facebook", Brand, Product 4. Nhập Caption: "Caption cho Facebook" 5. Chọn Platform: checkbox Facebook 6. Click tab "Facebook" trong Post Preview panel bên phải 7. Quan sát mockup | Mockup Facebook hiển thị: avatar (chữ cái đầu brand, gradient xanh), brand name, "Promoting [product] · Just now · public", title in đậm, caption text, hashtags màu xanh (#216fdb), ảnh/video nếu có, thanh Like/Comment/Share. Cập nhật real-time khi sửa Title/Caption. Không có message | -- |
| CT-23 | Preview Instagram real-time | 1. Đăng nhập -> Content -> Create 2. Nhập Title, Brand, Product, Caption, upload ảnh (Type IMAGE) 3. Chọn Platform: Instagram 4. Click tab "Instagram" trong Post Preview 5. Quan sát mockup | Mockup Instagram hiển thị: avatar (viền gradient hồng/cam), brand name, ảnh square aspect 1:1 (hoặc placeholder nếu không có ảnh), thanh tim/comment/share/bookmark, caption + hashtags (#00376b), "View all comments". Cập nhật real-time. Không có message | -- |
| CT-24 | Preview TikTok real-time | 1. Đăng nhập -> Content -> Create 2. Nhập Title, Brand, Product, Caption 3. Chọn Platform: TikTok 4. Click tab "TikTok" trong Post Preview 5. Quan sát mockup | Mockup TikTok hiển thị: nền đen #111111, aspect 9:16, avatar + @brandname, caption trắng, hashtags xanh (#00acee), "original sound - Brand", nút tương tác bên phải (tim, comment, bookmark, share). Nếu không có ảnh/video -> icon play màu xám. Không có message | -- |
| CT-25 | Preview ẩn/hiện tab theo platform đã chọn | 1. Đăng nhập -> Content -> Create 2. Chỉ check Platform: Instagram 3. Quan sát Post Preview tabs 4. Check thêm Facebook | Ban đầu chỉ hiện 1 tab "Instagram". Check thêm Facebook -> tab "Facebook" xuất hiện, tổng 2 tab. Tab TikTok không hiển thị vì không được chọn | -- |
| CT-26 | Preview hiển thị cả 3 tab khi chưa chọn platform nào | 1. Đăng nhập -> Content -> Create 2. Không check platform nào (form.platforms = []) 3. Quan sát Post Preview | Cả 3 tab Facebook, Instagram, TikTok đều hiển thị (isSelected = platforms.length === 0 -> true). Cập nhật real-time khi nhập liệu | -- |

### 8.4 TAGS, HASHTAGS & EXTRA FIELDS (CT-27 -> CT-29)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CT-27 | Chọn Tags từ TagPicker | 1. Đăng nhập -> Content -> Create 2. Nhập Title + Brand + Product 3. Tại ô Tags, click để mở TagPicker dropdown 4. Click chọn "Product Launch" -> chip hiện ra 5. Click chọn "Tutorial" 6. Gõ "CustomTag" vào input -> Enter 7. Click Save Content | Toast hiển thị **"Content created successfully"**. Tags lưu thành ["Product Launch", "Tutorial", "CustomTag"]. Card trong list hiển thị 2 tag đầu + "+1". Detail hiển thị đủ 3 tag dạng chips | -- |
| CT-28 | Nhập Hashtags bằng Enter và dấu phẩy | 1. Đăng nhập -> Content -> Create 2. Tại ô Hashtags, gõ "sale" -> Enter 3. Chip "#sale" xuất hiện 4. Gõ "fashion,summer" -> Enter 5. 2 chip "#fashion" và "#summer" xuất hiện 6. Click X trên chip "#sale" -> chip bị xóa 7. Click Save Content 8. F5 reload trang, vào detail content vừa tạo | **[BUG]** Toast hiển thị **"Content created successfully"**. Nhưng hashtags chỉ có trong FE form state, KHÔNG có trong CreateContentPayload -> không gửi lên BE. Sau reload -> hashtags bị mất hoàn toàn | -- |
| CT-29 | Nhập CTA Link và Internal Notes | 1. Đăng nhập -> Content -> Create 2. Nhập Title + Brand + Product 3. CTA Link: "https://example.com/sale" 4. Internal Notes: "Bản nháp - cần review trước thứ 3" 5. Click Save Content 6. F5 reload, kiểm tra detail | **[BUG]** Toast hiển thị **"Content created successfully"**. Nhưng CTA Link và Internal Notes chỉ có trong FE form, không có trong CreateContentPayload -> không gửi lên BE. Data bị mất sau khi reload | -- |

### 8.5 LOADING, NETWORK & EDGE CASES (CT-30 -> CT-32)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CT-30 | Loading state khi Save content | 1. Đăng nhập test@example.com / Pass1234 2. Content -> Create -> Manual Creation 3. Nhập đầy đủ Title, Brand, Product, Content Body 4. Bật Slow 3G trong DevTools Network tab 5. Click "Save Content" | Nút chuyển thành spinner xoay + text **"Saving..."** + disabled. Không thể click thêm hoặc tương tác form. Sau khi API trả về -> toast **"Content created successfully"** + redirect về /content | -- |
| CT-31 | Double click Save Content | 1. Đăng nhập -> Content -> Create 2. Nhập đầy đủ fields 3. Click "Save Content" 2 lần liên tiếp thật nhanh | Lần 1: setSaving(true) -> nút **"Saving..."** + disabled + spinner. Lần 2: nút đã disabled, không trigger handleSave. Chỉ tạo 1 content, chỉ 1 toast **"Content created successfully"** | -- |
| CT-32 | Mất mạng khi Save | 1. Đăng nhập -> Content -> Create 2. Nhập đầy đủ fields 3. DevTools -> Network -> chọn Offline 4. Click "Save Content" 5. Quan sát 6. Bật lại Online -> click Save lần nữa | Khi Offline: uploadFile/createContent throw error -> setSaveError hiển thị message lỗi (màu đỏ, dạng badge cạnh nút Save). setSaving(false) -> nút hết disabled. Form giữ nguyên data. Khi Online lại: click Save -> toast **"Content created successfully"** -> redirect | Mất mạng |

**Module:** CONTENT-CREATE | **Total:** 32 cases | **Page:** `/content/create` | **API:** POST `/content`, POST `/content/media`

---

## SHEET 9/19: CONTENT-MANAGEMENT -- Content Library & CRUD (80 cases)

| **Feature** | Content Management -- Library, Detail, Edit, Status, Delete, Search, Filter, Sort, Bulk |
|---|---|
| **Test requirement** | Content List (Grid/List toggle, stats bar 4 metrics, pagination 9/page, empty state, card display Text/Image/Video); Content Detail (full fields, AI generation history, metadata sidebar); Edit Content (modal inline, update title/type/text, không đổi được Brand); Status Workflow (Draft -> AwaitingApproval -> Approved/Rejected -> Published -> Scheduled, bulk change, filter theo status); Delete Soft & Restore (xóa mềm, restore qua API, bulk delete, xóa content Published); Search (title + brandName, case-insensitive, no results, clear); Filter (type, status, platform, tag, date range, brand, kết hợp, clear all); Sort (newest/oldest, title A-Z/Z-A, brand A-Z, by status); Bulk Operations (select/deselect, status change, delete, schedule); Card Actions (Preview, View Details, Edit, Duplicate, Post Now, Schedule); Access Control; Loading; Network |
| **Pages** | `/content`, `/content/[id]` |
| **API** | GET `/content`, GET `/content/{id}`, PUT `/content/{id}`, DELETE `/content/{id}`, POST `/content/{id}/restore`, POST `/content/{id}/publish/{integrationId}` |

### 9.1 CONTENT LIST VIEW (CM-01 -> CM-08)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-01 | Grid View mặc định với stats bar và right sidebar | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Từ sidebar, click "Content" 5. Quan sát toàn bộ trang | Header: breadcrumbs "Dashboard > Content Library", nút "Create New Content" (dropdown: Manual Creation, AI Generate, Import Content), nút Refresh, nút Export. Stats bar 4 card: Total Content, Published, Scheduled, Draft / Review (số chính xác). Grid 3 cột responsive. Mỗi card: thumbnail/gradient, title, brand, type badge (TEXT/IMAGE/VIDEO), status badge màu, tags (max 2 + "+N"), ngày tạo. Right sidebar: Content Quota + AI Quick Assistant + Recent Activity | Có ít nhất 3 content |
| CM-02 | Toggle Grid <-> List View | 1. Đăng nhập -> Content 2. Click nút "list" trong filter bar 3. Quan sát 4. Click nút "grid_view" 5. F5 reload 6. Quan sát view sau reload | List View: bảng checkbox, Content (icon + title clickable), Brand, Type, Status (badge + dot), Tags (chips + "+N"), Date, Platforms (icon), menu (...). Sau F5: view mode giữ từ localStorage key "content-view-mode", không reset về Grid | Có content |
| CM-03 | Empty State (chưa có content nào) | 1. Đăng nhập workspace mới chưa có content 2. Từ sidebar, click Content | Stats bar tất cả = 0. Content area hiển thị icon library_books lớn màu xám, text **"No content found"**, subtext **"Try adjusting your filters or create new content"**. Nút Create New Content vẫn hoạt động | Chưa có content |
| CM-04 | Stats bar hiển thị đúng số liệu | 1. Đăng nhập -> Content 2. Đếm thủ công số content từng loại trong Grid 3. So sánh với 4 stats card | Total = tổng tất cả. Published = count "Published". Scheduled = upcomingScheduleCount từ /dashboard/summary. Draft / Review = count "Draft" + "Awaiting Approval". Số phải khớp chính xác | Có content nhiều status |
| CM-05 | Pagination khi >9 items | 1. Đăng nhập -> có 15 content 2. Vào Content -> quan sát cuối Grid 3. Click page 2 4. Click page 1 | Trang 1: 9 card + text **"Showing 9 of 15 results"**. Pagination: nút prev (disabled), nút "1" (active, primary), nút "2". Click "2" -> 6 card + **"Showing 15 of 15 results"**. Click "1" -> quay lại | Có 15 content |
| CM-06 | Card TextOnly (không ảnh) | 1. Đăng nhập -> xem card content Type TEXT, không upload ảnh | Card hiển thị gradient tím (from-purple-500 to-purple-400), icon "article". Type badge TEXT (tím). Title truncate nếu dài | Có content TEXT không ảnh |
| CM-07 | Card ImageText có thumbnail | 1. Đăng nhập -> xem card content Type IMAGE có upload ảnh | Card hiển thị thumbnail ảnh, aspect 4/3. Type badge IMAGE (xanh). Nếu nhiều ảnh -> hiển thị ảnh đầu tiên | Có content IMAGE |
| CM-08 | Card VideoText có thumbnail video | 1. Đăng nhập -> xem card content Type VIDEO có upload video | Card hiển thị thumbnail video (nếu parse được) hoặc gradient hồng (from-rose-500 to-rose-400) + icon "play_circle". Type badge VIDEO (hồng) | Có content VIDEO |

### 9.2 CONTENT DETAIL VIEW (CM-09 -> CM-14)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-09 | Xem detail TextOnly | 1. Đăng nhập test@example.com / Pass1234 2. Content -> click card TextOnly 3. Quan sát /content/[id] | Breadcrumbs: **"Dashboard > Content Library > [title]"**. Hiển thị: Title, Brand, Product, Type TEXT, Status badge, TextContent, Description, Caption, Tags (chips), Created/Updated date. Các nút: Edit, Delete, Change Status, Post Now, Schedule, Duplicate | Có content TEXT |
| CM-10 | Xem detail ImageText có ảnh | 1. Đăng nhập -> Content -> click card IMAGE 2. Quan sát phần ảnh | Ảnh full width. Nếu imageUrl JSON array -> gallery. Brand, Product, Tags hiển thị đúng | Có content IMAGE |
| CM-11 | Xem detail VideoText có video player | 1. Đăng nhập -> Content -> click card VIDEO 2. Click play trên video | Video player với controls: play/pause, timeline, volume. Không autoplay. Duration nếu có | Có content VIDEO |
| CM-12 | Xem AI Generation history trong detail | 1. Đăng nhập -> Content -> click card AI-generated 2. Scroll tìm section AI Generation | Hiển thị section **"AI Generation History"** với danh sách: id, status (Pending/Processing/Completed/Failed), errorMessage, createdAt. Nếu trống -> không hiện section | Content AI-generated |
| CM-13 | Metadata sidebar trong detail | 1. Đăng nhập -> Content -> click content bất kỳ 2. Quan sát metadata | Hiển thị: Created date, Last modified date, Brand, Product, AdType (Text/Image/Video), Status, AI Generated (Yes/No). Nếu scheduled -> thêm Scheduled date, Platform. Nếu posted -> Post status, integration | Có content |
| CM-14 | Content deleted -> truy cập URL trực tiếp | 1. Đăng nhập -> xóa 1 content 2. Copy URL /content/[id] từ history 3. Paste, Enter | BE có thể trả 404 -> **"Content not found"** hoặc redirect /content. Hoặc trả data với isDeleted=true -> hiển thị detail + badge **"Deleted"** + nút Restore (nếu có) | Content đã soft-delete |

### 9.3 EDIT CONTENT (CM-15 -> CM-20)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-15 | Sửa Title qua Edit Modal | 1. Đăng nhập test@example.com / Pass1234 2. Content -> click "..." trên card -> "Edit" 3. Modal Edit mở, sửa Title: "Title đã được sửa" 4. Click Save | Modal đóng. Toast hiển thị **"'Title đã được sửa' updated"** (icon check). Card cập nhật title mới. API PUT /content/{id} với {title, adType, textContent: ""} | Có content |
| CM-16 | Hạn chế Edit (textContent luôn rỗng) | 1. Đăng nhập -> Content -> Edit 2. Tìm field TextContent trong modal | **[HẠN CHẾ]** handleEditSave gửi textContent: "" (cứng). Cần xác nhận edit modal có cho sửa textContent không. Nếu không -> đây là giới hạn của FE | Có content |
| CM-17 | Không thể sửa Brand | 1. Đăng nhập -> Content -> Edit 2. Tìm field Brand | Edit modal không có field Brand. UpdateContentPayload không có brandId. Muốn đổi brand -> phải tạo content mới | -- |
| CM-18 | Sửa content Awaiting Approval | 1. Đăng nhập -> Content -> Edit content "Awaiting Approval" 2. Sửa Title -> Save 3. Quan sát status sau edit | Content sửa được. **[GHI NHẬN]** status sau edit: nếu BE giữ nguyên -> vẫn "Awaiting Approval". Nếu BE reset về "Draft" -> behavior đúng vì content thay đổi cần duyệt lại | Content Awaiting Approval |
| CM-19 | Sửa content Published | 1. Đăng nhập -> Content -> Edit content "Published" 2. Sửa Title -> Save | **[GHI NHẬN]** BE có thể chặn: **"Cannot edit published content"**. Hoặc cho phép nhưng bài trên social không tự update | Content Published |
| CM-20 | Hủy Edit (Cancel) | 1. Đăng nhập -> Content -> Edit 2. Sửa Title 3. Click ngoài modal hoặc nút Close/X | Modal đóng. Card giữ title cũ. Không API call, không toast. Không thay đổi gì | -- |

### 9.4 STATUS WORKFLOW (CM-21 -> CM-27)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-21 | Status Draft -> Awaiting Approval (tạo mới) | 1. Đăng nhập test@example.com / Pass1234 2. Content -> Create -> chọn Status: "Awaiting Approval" 3. Nhập đủ fields -> Save 4. Về Content List | Content hiển thị badge vàng **"Awaiting Approval"**, dot amber. Xuất hiện trong filter "Awaiting Approval". Sẵn sàng duyệt tại /approvals | -- |
| CM-22 | Bulk: Draft -> Awaiting Approval | 1. Đăng nhập -> Content 2. Check 3 content Draft 3. Batch bar: "Set status..." -> "Awaiting Approval" -> Apply | Toast: **"Updated 3 items to Awaiting Approval"**. 3 card đổi badge vàng. Có thể undo (chọn lại Draft -> Apply) | 3 content Draft |
| CM-23 | Bulk: Awaiting Approval -> Draft | 1. Đăng nhập -> Content 2. Check 2 content "Awaiting Approval" 3. Batch status -> "Draft" -> Apply | 2 content về Draft (badge xám). Không còn trong /approvals | 2 content Awaiting Approval |
| CM-24 | Status màu sắc trên card | 1. Đăng nhập -> Content, quan sát badge | Draft: xám + dot xám. Awaiting Approval: vàng (#amber-50/#amber-600) + dot vàng. Approved: xanh lá (#emerald-50/#emerald-600) + dot xanh. Rejected: đỏ (#danger-red/10) + dot đỏ. Published: xanh dương (#blue-50/#blue-600) + dot animate-pulse. Scheduled: xanh dương + dot xanh | Có đủ 5+ status |
| CM-25 | Filter theo Status | 1. Đăng nhập -> Content -> filter "Draft" -> "Published" -> "Awaiting Approval" | Mỗi filter hiển thị đúng status, số khớp stats. Kết hợp được với Type, Tag, Date, Search | Có content nhiều status |
| CM-26 | Stats "Draft / Review" gộp 2 status | 1. Đăng nhập -> Content 2. Đếm Draft + Awaiting Approval 3. So với card "Draft / Review" | stats.draft = count(Draft) + count(Awaiting Approval). Giải thích: cả 2 là trạng thái "chưa publish" | Có cả 2 status |
| CM-27 | Scheduled trong stats và filter | 1. Đăng nhập -> schedule 1 content 2. Quay lại Content | Content badge **"Scheduled"** (xanh dương). Stats Scheduled = upcomingScheduleCount từ API. Filter "Scheduled" có trong dropdown | Có content đã schedule |

### 9.5 DELETE & RESTORE (CM-31 -> CM-37)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-31 | Xóa mềm content | 1. Đăng nhập test@example.com / Pass1234 2. Content -> "..." trên card -> "Delete" 3. Modal: icon thùng rác đỏ, title "Delete Content", tên content 4. Click "Delete" | Modal đóng. Toast hiển thị **"'[title]' deleted"** (icon delete). Card biến mất. Stats Total giảm 1. API DELETE /content/{id} -> isDeleted=true | Có content |
| CM-32 | Hủy xóa (Cancel) | 1. Đăng nhập -> Content -> Delete trên card 2. Modal hiện -> Click Cancel hoặc ngoài modal | Modal đóng. Card vẫn trong list. Không API call. Stats không đổi | -- |
| CM-33 | Restore qua API | 1. Đăng nhập -> xóa content 2. Gọi restoreContent(id) -> POST /content/{id}/restore 3. Reload Content | Content hiển thị lại. Status giữ nguyên trước xóa. Data đầy đủ. Stats Total tăng 1 | Content đã soft-delete |
| CM-34 | Xóa content Published | 1. Đăng nhập -> xóa content "Published" 2. Xác nhận | **[GHI NHẬN]** BE có thể chặn: **"Cannot delete published content"** hoặc **"Archive content first"**. Nếu cho phép -> xóa mềm, bài social vẫn tồn tại | Content Published |
| CM-35 | Double Delete | 1. Đăng nhập -> gọi DELETE /content/{id} với content isDeleted=true | BE trả lỗi: **"Content already deleted"** / 404 / 400. Hoặc success nhưng không thay đổi. Không crash | Content đã deleted |
| CM-36 | Bulk Delete | 1. Đăng nhập -> Content -> check 3 content -> "Delete" | Toast: **"Deleted 3 items"** (nếu OK) hoặc **"Deleted X/3. Some items could not be deleted."** (nếu lỗi). Card biến mất | 3+ content |
| CM-37 | Không có UI Show Deleted | 1. Đăng nhập -> Content -> tìm tab/filter deleted | **[GHI NHẬN]** FE không có filter deleted. Chỉ Admin page mới quản lý deleted content | -- |

### 9.6 SEARCH & FILTER (CM-38 -> CM-49)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-38 | Search theo Title | 1. Đăng nhập test@example.com / Pass1234 2. Content -> search "Khuyến mãi" 3. Quan sát | Chỉ hiện content có title.toLowerCase().includes("khuyến mãi"). Text **"Showing X of Y results"** cập nhật. Filter chip **""Khuyến mãi""** | Có content "Khuyến mãi Tết" |
| CM-39 | Search theo Brand Name | 1. Đăng nhập -> Content -> search "My Brand" | Hiển thị content có brandName.toLowerCase().includes("my brand"). Search chỉ check title + brandName (theo FE code) | Có content nhiều brand |
| CM-40 | Search không kết quả | 1. Đăng nhập -> Content -> search "zzzKhôngTồnTại" | Empty state: icon library_books + **"No content found"** + **"Try adjusting your filters or create new content"**. Nút "Clear all" | -- |
| CM-41 | Xóa search (Clear) | 1. Đăng nhập -> search "test" -> có kết quả 2. Click X trong ô search | Ô search rỗng. List đầy đủ. Chip "test" biến mất. Text **"Showing X of Y results"** về tổng | Đang search |
| CM-42 | Filter Type (TEXT/IMAGE/VIDEO) | 1. Đăng nhập -> Content -> filter Type: "IMAGE" -> "TEXT" | IMAGE: chỉ hiện IMAGE, chip **"Image"** (amber). TEXT: chỉ hiện TEXT. Kết hợp search -> giao | Có cả 3 type |
| CM-43 | Filter theo Status | 1. Đăng nhập -> filter "Published" -> "Draft" | Mỗi filter hiển thị đúng status, số khớp stats. Kết hợp Type+Status -> giao | Có content nhiều status |
| CM-44 | Filter theo Tag | 1. Đăng nhập -> TagFilterSelect -> chọn "Product Launch" | Chỉ hiện content có tag "Product Launch". Chip **"Product Launch"**. Kết hợp search+type+status -> giao | Có tag "Product Launch" |
| CM-45 | Filter Date Range | 1. Đăng nhập -> From: 01/07/2026, To: 19/07/2026 | Chỉ hiện content createdAt trong [01/07, 19/07 23:59:59]. Chips: **"From 2026-07-01"** và **"To 2026-07-19"** | Có content nhiều ngày |
| CM-46 | Filter Platform | 1. Đăng nhập -> filter Platform: "Facebook" | **[GHI NHẬN]** platforms luôn [] từ API -> filter vô tác dụng với data BE. Chỉ hoạt động nếu FE tự thêm platform | -- |
| CM-47 | Kết hợp nhiều filter | 1. Đăng nhập -> Search + Type + Status + Tag + Date cùng lúc | Kết quả = giao tất cả điều kiện. Nếu không có -> empty state | Content đa dạng |
| CM-48 | Clear all filters | 1. Đăng nhập -> áp nhiều filter -> click "Clear all" | Tất cả reset: search rỗng, chips biến mất, list đầy đủ, sort về **"Newest First"** | Đang filter |
| CM-49 | Xóa từng filter chip | 1. Đăng nhập -> filter IMAGE + tag "Seasonal" 2. Click X chip "IMAGE" -> X chip "Seasonal" | Xóa IMAGE: typeFilter reset, vẫn filter tag. Xóa Seasonal: tagFilter reset, list đầy đủ. Các filter khác không ảnh hưởng | Đang có filter |

### 9.7 SORT (CM-50 -> CM-55)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-50 | Sort Newest First (mặc định) | 1. Đăng nhập test@example.com / Pass1234 2. Content -> quan sát thứ tự | createdAt giảm dần. Dropdown hiển thị **"Newest First"** | Có content nhiều ngày |
| CM-51 | Sort Oldest First | 1. Đăng nhập -> chọn "Oldest First" | createdAt tăng dần. Chip **"Oldest First"** | -- |
| CM-52 | Sort Title A-Z | 1. Đăng nhập -> chọn "Title A-Z" | localeCompare a->z. Chip **"Title A-Z"** | Có title đa dạng |
| CM-53 | Sort Title Z-A | 1. Đăng nhập -> chọn "Title Z-A" | localeCompare z->a. Chip **"Title Z-A"** | -- |
| CM-54 | Sort Brand A-Z | 1. Đăng nhập -> chọn "Brand A-Z" | Nhóm theo brandName alphabet. Cùng brand -> không sort phụ. Chip **"Brand A-Z"** | Nhiều brand |
| CM-55 | Sort By Status | 1. Đăng nhập -> chọn "By Status" | Thứ tự: Published(0) -> Scheduled(1) -> Approved(2) -> Awaiting Approval(3) -> Draft(4) -> Rejected(5). Chip **"By Status"** | Nhiều status |

### 9.8 BULK OPERATIONS (CM-56 -> CM-62)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-56 | Bulk select/deselect từng card | 1. Đăng nhập test@example.com / Pass1234 2. Content -> check card 1,2,3 -> uncheck card 2 | Batch bar: **"3 selected"** -> **"2 selected"** kèm nút Set status, Schedule, Delete, Deselect. Checkbox card 2 unchecked | 3+ content |
| CM-57 | Select All / Deselect All | 1. Đăng nhập -> List View -> check header checkbox -> Deselect | Batch bar: **"9 selected"**. Deselect -> tất cả bỏ chọn, batch bar biến mất | 9+ content |
| CM-58 | Bulk Status: Draft -> Awaiting Approval | 1. Đăng nhập -> check 3 Draft -> "Awaiting Approval" -> Apply | Toast: **"Updated 3 items to Awaiting Approval"**. 3 card đổi badge vàng. Reload -> status lưu | 3 Draft |
| CM-59 | Bulk Status -> Published | 1. Đăng nhập -> check 2 -> "Published" -> Apply | Toast: **"Updated 2 items to Published"**. **[GHI NHẬN]** publish thường cần integrationId, bulk bypass có thể bị BE chặn | 2 content |
| CM-60 | Bulk Delete | 1. Đăng nhập -> check 3 -> "Delete" | Toast: **"Deleted 3 items"** hoặc **"Deleted X/3. Some items could not be deleted."** | 3+ content |
| CM-61 | Bulk Schedule (đủ điều kiện) | 1. Đăng nhập -> check 2 Approved cùng brand -> "Schedule" -> chọn integration+time -> Confirm | Nút Schedule active (primary). Modal mở. Sau confirm -> chuyển Scheduled | 2 Approved, same brand |
| CM-62 | Bulk Schedule disabled | 1. Đăng nhập -> check 1 Draft + 1 Approved khác brand -> quan sát nút Schedule | Nút disabled (xám, cursor-not-allowed). Tooltip: **"Only Approved content can be scheduled"** hoặc **"All items must belong to the same brand"** | Content khác status+ brand |

### 9.9 CARD ACTIONS & NAVIGATION (CM-63 -> CM-68)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-63 | Card action: Preview | 1. Đăng nhập test@example.com / Pass1234 2. Content -> "..." -> "Preview" | setPreviewItem được set, UI preview hiển thị (modal/panel) tùy implementation | Có content |
| CM-64 | Card action: View Details | 1. Đăng nhập -> "..." -> "View Details" | Redirect: **/content/[id]**. Breadcrumb cập nhật | Có content |
| CM-65 | Card action: Duplicate | 1. Đăng nhập -> "..." -> "Duplicate" | Toast: **"'[title]' duplicated"** (icon content_copy). **[GHI NHẬN]** Chỉ toast placeholder, KHÔNG gọi API duplicate. Không thực sự nhân bản | -- |
| CM-66 | Card action: Post Now | 1. Đăng nhập -> "..." trên card Approved -> "Post Now" | PostNowModal mở: chọn integration, xác nhận. Gọi publishContent(contentId, integrationId) | Approved + integration |
| CM-67 | Card action: Schedule | 1. Đăng nhập -> "..." -> "Schedule" | Redirect: **/calendar?contentId=[id]**. Calendar mở với content chọn sẵn | Có content |
| CM-68 | Navigate Dashboard qua breadcrumb | 1. Đăng nhập -> Content -> click "Dashboard" | Redirect: **/dashboard**. Sidebar highlight Dashboard | -- |

### 9.10 ACCESS CONTROL (CM-69 -> CM-73)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-69 | Chưa login -> /content | 1. Mở browser, chưa login 2. Truy cập https://[domain]/content | Redirect: **/login**. Sau login -> về /content (hoặc /dashboard tùy middleware) | Chưa login |
| CM-70 | Token hết hạn | 1. Đăng nhập -> xóa token localStorage 2. Thao tác Edit/Save | API 401 -> redirect **/login** + message **"Session expired"**. Data edit bị mất | Token hết hạn |
| CM-71 | User A thấy content User B (cùng WS) | 1. User A tạo content -> logout 2. User B login -> Content | User B thấy content User A (GET /content trả tất cả WS). Behavior đúng cho collaboration | A, B cùng WS |
| CM-72 | Cross-workspace truy cập | 1. User A (WS A) copy URL /content/[id] của WS B 2. Paste, Enter | Nếu BE check: 403 **"Forbidden"** hoặc 404 **"Content not found"**. Nếu không -> leak data (bug bảo mật) | A, B khác WS |
| CM-73 | Member xóa/edit content Owner | 1. Member login -> Content -> Delete/Edit content Owner | **[GHI NHẬN]** FE không phân biệt quyền. BE cần check -> nếu chặn: 403 **"Forbidden"**. Nếu không -> xóa được (bug) | Member + Owner |

### 9.11 LOADING, NETWORK & EDGE (CM-74 -> CM-83)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-74 | Loading skeleton | 1. Đăng nhập test@example.com / Pass1234 2. Slow 3G -> reload Content | 6 card skeleton animate-pulse: khung aspect-[4/3], 3 thanh xám. Xong -> card thật fade-up tuần tự | -- |
| CM-75 | Loading khi Edit save | 1. Đăng nhập -> Edit -> Slow 3G -> Save | Nút spinner + **"Saving..."** + disabled. Toast sau khi xong | -- |
| CM-76 | Double click Edit Save | 1. Đăng nhập -> Edit -> Save 2 lần nhanh | Lần 1: disabled. Lần 2: không trigger. 1 API call | -- |
| CM-77 | Mất mạng Edit/Save | 1. Đăng nhập -> Edit -> Offline -> Save | Toast lỗi. Modal vẫn mở, data còn. Online -> Save OK | Mất mạng |
| CM-78 | Mất mạng load List | 1. Đăng nhập -> Offline -> vào Content | fetchContents catch -> setAllContent([]). Empty state **"No content found"**. Không crash. Online -> Refresh -> load OK | Mất mạng |
| CM-79 | Refresh giữ view mode | 1. Đăng nhập -> List View -> F5 | Vẫn List View (localStorage **"content-view-mode"**). Không reset Grid | -- |
| CM-80 | Refresh mất filter/search | 1. Đăng nhập -> search "test" + page 2 -> F5 | Tất cả reset: search rỗng, page=1, filter mặc định. View mode giữ. **[GHI NHẬN]** State không persist qua URL params | -- |
| CM-81 | Click card -> detail | 1. Đăng nhập -> Grid View: click card -> List View: click title | Grid: click card KHÔNG chuyển trang. List: click title -> redirect **/content/[id]**. Khác biệt giữa 2 view | Có content |
| CM-82 | Menu "..." open/close | 1. Đăng nhập -> "..." card 1 -> click ngoài -> "..." card 2 | Menu 1 đóng, menu 2 mở. Chỉ 1 menu mở tại 1 thời điểm | -- |
| CM-83 | Content Quota sidebar | 1. Đăng nhập -> Content -> quan sát Quota card | Text/Image/Video count / postQuotaLimit. Progress bar gradient. **"used this month"**. 3 dot: xanh lá (Text), tím (Image), hồng (Video). Nút: **"Upgrade Plan"** | Có quota data |

**Module:** CONTENT-MANAGEMENT | **Total:** 80 cases | **Pages:** `/content`, `/content/[id]` | **API:** GET `/content`, GET `/content/{id}`, PUT `/content/{id}`, DELETE `/content/{id}`, POST `/content/{id}/restore`

| Sheet | Module | Cases | Status |
|-------|--------|-------|--------|
| 1 | AUTH | 74 | Complete |
| 2 | WORKSPACE | 22 | Complete |
| 3 | TEAM-MANAGEMENT | 34 | Complete |
| 4 | PROFILE | 59 | Complete |
| 5 | BRAND | 63 | Summary (full details in chat) |
| 6 | PRODUCT | 66 | Summary (full details in chat) |
| 7 | AI | 83 | Summary (full details in chat) |
| 8 | CONTENT-CREATE | 32 | Complete |
| 9 | CONTENT-MANAGEMENT | 80 | Complete |
| 10 | APPROVAL | 52 | Complete |
| 11 | SOCIAL | 60 | Complete |
| 12 | SCHEDULE | 58 | Complete |
| 13 | POSTS | 68 | Complete |
| 14 | CAMPAIGN | 75 | Complete |
| 15 | PAYMENT & CREDIT | TBD | Pending |
| 16 | ANALYTICS | TBD | Pending |
| 17 | NOTIFICATION | TBD | Pending |
| 18 | AUTOMATION | TBD | Pending |
| 19 | ADMIN | TBD | Pending |

**Completed: 826 cases** | **Remaining: 5 sheets**

> Lưu ý: Sheet 5 (BRAND), 6 (PRODUCT), 7 (AI) đã được viết chi tiết đầy đủ trong lịch sử chat. File này lưu bản tóm tắt. Khi cần chi tiết từng case, tham khảo lịch sử hội thoại để có Procedure đầy đủ step-by-step.

---

## SHEET 10/19: APPROVAL -- Content Approval Workflow (52 cases)

| **Feature** | Content Approval -- Duyệt nội dung trước khi publish |
|---|---|
| **Test requirement** | Approvals page `/approvals`: 6 tab filters (All, Pending, Approved, Published, Failed, Rejected), search, brand filter, priority filter (Urgent/Medium/Standard), sort; Batch actions (Approve All, Reject All); Per-row actions theo status: Approve (Pending->Approved), Request Changes (mở Revision Modal->Rejected), Reject (modal confirm->Rejected), Post Now (Approved->Published), Schedule (Approved->Calendar), Delete (Rejected), Review (mở Asset Review Drawer), Lock (disabled, Leader only); Asset Review Drawer: detail content, audit trail timeline, contextual action buttons; Revision Request Modal: suggestion chips, 500-char notes, submit gọi rejectContent; Reject Confirmation Modal: cảnh báo, cancel/reject; Status transition rules (Draft->PendingApproval->Approved/Rejected); Permissions (Owner/Manager/ContentCreator được approve/reject, Viewer không); Loading skeleton, empty state từng tab, double click, mất mạng |
| **Pages** | `/approvals` |
| **API** | PUT `/content/{id}` (status=2 approve, status=3 reject), GET `/content?status=...`, DELETE `/content/{id}` |

### 10.1 APPROVALS LIST VIEW -- Danh sách & filter (AP-01 -> AP-12)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-01 | Truy cập trang Approvals với đủ content các status | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Từ sidebar, click "Approvals" (icon task_alt, nằm giữa Content và Posts) 5. Quan sát toàn bộ trang | Header: breadcrumbs "Dashboard > Approvals", title "Content Approvals", subtitle "Review and manage AI-generated marketing assets". Team avatars: Alex C. (xanh), Jamie L. (emerald), Sam R. (amber), Taylor K. (tím). 6 tabs kèm count: All, Pending (amber dot), Approved (emerald dot), Published (primary dot), Failed (red dot), Rejected (red dot). Bảng danh sách content với cột: checkbox, Content (icon + title), Brand (dot màu + tên + product), Requester (AI/Manual badge), Platform (icon), Date, Urgency (badge), Status (badge + dot), Actions (hiện khi hover) | Có ít nhất 5 content với status khác nhau (Pending, Approved, Published, Rejected) |
| AP-02 | Tab Pending hiển thị đúng content chờ duyệt | 1. Đăng nhập -> Approvals 2. Click tab "Pending" 3. Quan sát danh sách | Chỉ hiện content có status "Awaiting Approval" hoặc "PendingApproval" hoặc "Pending". Mỗi row có nút Approve (green check), Request Changes (icon review), Reject (red block) khi hover. Count trên tab khớp số row | Có content Awaiting Approval |
| AP-03 | Tab Approved hiển thị content đã duyệt | 1. Đăng nhập -> Approvals 2. Click tab "Approved" 3. Quan sát danh sách | Chỉ hiện content status "Approved". Mỗi row có nút Post Now (icon send), Schedule (icon calendar) khi hover. Count khớp | Có content Approved |
| AP-04 | Tab Rejected hiển thị content bị từ chối | 1. Đăng nhập -> Approvals 2. Click tab "Rejected" 3. Quan sát danh sách | Chỉ hiện content status "Rejected". Mỗi row có nút Delete (red trash) khi hover. Count khớp | Có content Rejected |
| AP-05 | Tab Published hiển thị content đã đăng | 1. Đăng nhập -> Approvals 2. Click tab "Published" | Chỉ hiện content status "Published". Không có action buttons (đã đăng rồi). Count khớp | Có content Published |
| AP-06 | Tab All hiển thị tất cả | 1. Đăng nhập -> Approvals 2. Đang ở tab khác -> click "All" | Hiển thị tất cả content bất kể status. Count = tổng các tab | -- |
| AP-07 | Tab Failed hiển thị content lỗi publish | 1. Đăng nhập -> Approvals 2. Click tab "Failed" | Chỉ hiện content status "PublishFailed" / "PostFailed" / "Failed". Row hiển thị badge đỏ "Failed" | Có content publish failed |
| AP-08 | Search trong Approvals | 1. Đăng nhập -> Approvals 2. Nhập "Khuyến mãi" vào ô search 3. Quan sát | Chỉ hiện content có title hoặc brandName chứa "Khuyến mãi". Filter kết hợp với tab đang active (giao). Text "Showing X results" cập nhật | Có content "Khuyến mãi Tết" |
| AP-09 | Filter theo Brand | 1. Đăng nhập -> Approvals 2. Chọn 1 brand từ dropdown Brand filter 3. Quan sát | Chỉ hiện content thuộc brand đã chọn. Kết hợp với tab active + search -> giao tất cả. Dropdown chỉ hiện brand từ content hiện có | Có content nhiều brand |
| AP-10 | Filter theo Priority (Urgency) | 1. Đăng nhập -> Approvals 2. Chọn filter Priority: "Urgent" 3. Quan sát | Chỉ hiện content có priority "Urgent" (tags chứa "Product Launch" hoặc "Promotion"). Badge Urgency hiển thị màu đỏ. Priority được derive từ tags | Có content với tag "Product Launch" |
| AP-11 | Sort trong Approvals | 1. Đăng nhập -> Approvals 2. Chọn sort: "Newest First" -> "Oldest First" -> "Title A-Z" -> "Brand A-Z" | Mỗi sort cập nhật thứ tự bảng đúng. Default sort có thể là newest first | Có content đa dạng |
| AP-12 | Empty state từng tab | 1. Đăng nhập -> Approvals 2. Click tab "Rejected" khi chưa có content Rejected nào | Hiển thị message empty state tương ứng, ví dụ: "No rejected content" hoặc "All content has been reviewed". Không crash, không trắng trang | Chưa có content Rejected |

### 10.2 APPROVE CONTENT -- Duyệt nội dung (AP-13 -> AP-21)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-13 | Approve 1 content từ row action | 1. Đăng nhập test@example.com / Pass1234 2. Vào Approvals -> tab Pending 3. Hover vào 1 row -> click nút Approve (green check icon) 4. Quan sát | Row biến mất khỏi tab Pending. Count Pending giảm 1. Chuyển sang tab Approved -> content vừa approve hiển thị ở đó. API PUT /content/{id} với status=2. Content có thể Post Now hoặc Schedule | Có content Awaiting Approval |
| AP-14 | Approve 1 content từ Asset Review Drawer | 1. Đăng nhập -> Approvals -> tab Pending 2. Hover -> click nút Review (eye icon) 3. Asset Review Drawer mở bên phải 4. Click nút "Approve" ở footer drawer 5. Quan sát | Drawer đóng. Row biến mất khỏi Pending. Content chuyển sang Approved. Toast không bắt buộc (có thể silent) | Có content Pending |
| AP-15 | Batch Approve All | 1. Đăng nhập -> Approvals -> tab Pending 2. Check chọn 3 content Pending 3. Batch action bar hiện "3 selected" 4. Click "Approve All" (nút xanh emerald) 5. Quan sát | Cả 3 row biến mất. Count Pending giảm 3. Tab Approved tăng 3. Gọi approveContent() cho từng id tuần tự | Có 3+ content Pending |
| AP-16 | Select All -> Approve All | 1. Đăng nhập -> Approvals -> tab Pending 2. Check checkbox "Select All" ở header bảng 3. Click "Approve All" | Tất cả content Pending trên trang hiện tại được approve. Batch bar: "N selected" -> "Approve All" | Có nhiều content Pending |
| AP-17 | Approve content -> status Approved hiển thị trên card Content List | 1. Đăng nhập -> Approvals -> Approve 1 content 2. Quay lại Content page (sidebar -> Content) 3. Tìm content vừa approve | Content hiển thị badge "Approved" (xanh lá emerald). Có thể Post Now hoặc Schedule từ Content page. Filter "Approved" hiển thị content này | -- |
| AP-18 | Approve content -> xuất hiện trong Post Now / Schedule | 1. Đăng nhập -> Approvals -> Approve 1 content 2. Vào tab Approved -> hover row | Row hiển thị nút Post Now (send icon) và Schedule (calendar icon). Click Post Now -> PostNowModal mở. Click Schedule -> redirect /calendar?contentId=[id] | Content vừa Approved |
| AP-19 | Double click Approve | 1. Đăng nhập -> Approvals -> Pending 2. Click Approve 2 lần liên tiếp nhanh | Lần 1: API PUT status=2. Lần 2: content đã Approved -> không còn trong Pending -> không thể click lại. Hoặc nếu còn trong list -> API lần 2 vẫn success (idempotent) | Có content Pending |
| AP-20 | Approve content đã Approved (idempotent) | 1. Đăng nhập -> Approvals -> Approved 2. Gọi API approveContent(id) với content đã Approved | PUT /content/{id} status=2 lần nữa -> BE trả success (status không đổi). Không crash, không lỗi | Content Approved |
| AP-21 | Approve content từ Draft (skip PendingApproval) | 1. Đăng nhập -> dùng API: updateContent(id, {status: 2}) với content Draft | BE check validateStatusTransition: Draft -> Approved KHÔNG hợp lệ -> trả lỗi. Message: "Invalid status transition" hoặc tương tự | Content Draft |

### 10.3 REJECT CONTENT -- Từ chối nội dung (AP-22 -> AP-28)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-22 | Reject 1 content từ row action (có confirm modal) | 1. Đăng nhập test@example.com / Pass1234 2. Vào Approvals -> tab Pending 3. Hover row -> click Reject (red block icon) 4. Modal "Reject Asset?" hiện ra với cảnh báo 5. Click "Reject" trong modal | Modal đóng. Row biến mất khỏi Pending. Tab Rejected tăng 1. API PUT /content/{id} status=3. Content chuyển sang Rejected | Có content Pending |
| AP-23 | Hủy Reject (Cancel modal) | 1. Đăng nhập -> Approvals -> Pending 2. Click Reject -> modal hiện 3. Click "Cancel" hoặc click ngoài modal | Modal đóng. Row vẫn trong Pending, không thay đổi. Không API call | -- |
| AP-24 | Batch Reject All | 1. Đăng nhập -> Approvals -> Pending 2. Check 3 content -> batch bar "3 selected" 3. Click "Reject All" (nút đỏ) 4. Quan sát | Cả 3 row biến mất khỏi Pending. Count Rejected tăng 3. Gọi rejectContent() cho từng id. Có thể có confirm modal cho batch | Có 3+ content Pending |
| AP-25 | Reject content -> hiển thị trong tab Rejected với nút Delete | 1. Đăng nhập -> Approvals -> Reject 1 content 2. Click tab "Rejected" 3. Hover row vừa reject | Row hiển thị badge đỏ "Rejected". Hover -> hiện nút Delete (red trash). Không có nút Approve hay Request Changes | Content vừa Rejected |
| AP-26 | Reject content -> status Rejected trên Content List | 1. Đăng nhập -> Approvals -> Reject 1 content 2. Vào Content page 3. Tìm content đó | Badge "Rejected" (đỏ danger, dot đỏ). Có thể edit và submit lại (đổi về PendingApproval) | -- |
| AP-27 | Reject content từ Asset Review Drawer | 1. Đăng nhập -> Approvals -> Pending 2. Click Review (eye) -> drawer mở 3. Click "Reject" trong footer drawer | Drawer đóng. Content chuyển sang Rejected. Tương tự AP-22 nhưng qua drawer | Có content Pending |
| AP-28 | Reject content đã Rejected | 1. Đăng nhập -> Approvals -> Rejected 2. Gọi API rejectContent(id) với content đã Rejected | PUT /content/{id} status=3 lần nữa -> BE trả success (idempotent). Không crash | Content Rejected |

### 10.4 REQUEST CHANGES -- Yêu cầu chỉnh sửa (AP-29 -> AP-34)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-29 | Request Changes mở Revision Modal | 1. Đăng nhập test@example.com / Pass1234 2. Approvals -> Pending 3. Hover row -> click Request Changes (icon review) 4. Quan sát | Revision Request Modal mở: hiển thị content info card (title, brand, platform badges, priority badge), info banner "Your feedback will be sent to AISAM for regeneration", các suggestion chips: "Adjust copy", "Change image", "Fix CTA", "Branding issue", "Tone of voice", "Formatting", "Other". Textarea 500 ký tự với progress bar. Nút "Submit Revision Request" | Có content Pending |
| AP-30 | Chọn suggestion chips trong Revision Modal | 1. Đăng nhập -> Approvals -> Pending -> Request Changes 2. Click chip "Adjust copy" -> chip được highlight 3. Click chip "Change image" -> cả 2 được highlight 4. Click lại "Adjust copy" -> chip bỏ highlight | Chips toggle on/off. Có thể chọn nhiều chips cùng lúc. Chips selected có màu khác (highlight) | -- |
| AP-31 | Nhập notes trong Revision Modal | 1. Đăng nhập -> Request Changes 2. Nhập "Cần chỉnh sửa headline cho hấp dẫn hơn" vào textarea 3. Quan sát progress bar | Textarea nhập tối đa 500 ký tự. Progress bar cập nhật real-time (vd: "45/500"). Nếu vượt 500 -> không cho nhập thêm | -- |
| AP-32 | Submit Revision Request (thực chất gọi rejectContent) | 1. Đăng nhập -> Request Changes 2. Chọn chips: "Adjust copy", nhập notes: "Sửa headline" 3. Click "Submit Revision Request" 4. Quan sát | Modal đóng. Content chuyển sang Rejected (vì code gọi rejectContent(id)). Row biến mất khỏi Pending, xuất hiện trong Rejected. Notes và chips được lưu (nếu có API hỗ trợ) | Có content Pending |
| AP-33 | Submit Revision Request không chọn chips, không notes | 1. Đăng nhập -> Request Changes 2. Không chọn chip, không nhập notes 3. Click "Submit Revision Request" | Vẫn submit được (không bắt buộc chips/notes). Content chuyển sang Rejected. Gọi rejectContent(id) | -- |
| AP-34 | Hủy Revision Modal (Cancel/Close) | 1. Đăng nhập -> Request Changes 2. Chọn chips, nhập notes 3. Click ngoài modal hoặc nút Close/X | Modal đóng. Content vẫn Pending, không thay đổi. Chips và notes bị reset (không lưu) | -- |

### 10.5 ASSET REVIEW DRAWER -- Xem chi tiết (AP-35 -> AP-42)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-35 | Mở Asset Review Drawer | 1. Đăng nhập test@example.com / Pass1234 2. Approvals -> Pending 3. Hover row -> click Review (eye icon) 4. Quan sát drawer | Drawer mở từ bên phải (slide-in). Hiển thị: gradient preview area với type icon, Title (headline), Brand, Product, Status badge, Priority badge. Detail cards: Headline content, Created date, Requester (AI/Manual), Brand name, Priority label | Có content Pending |
| AP-36 | Audit Trail timeline trong Review Drawer | 1. Đăng nhập -> mở Review Drawer 2. Scroll xuống phần Audit Trail | Timeline hiển thị các sự kiện theo thứ tự thời gian: "Content created" (AI hoặc Manual) với timestamp, "Content review assigned" (assigned to team members), "Submitted for approval" (status: Pending). Mỗi event có icon, text, thời gian | -- |
| AP-37 | Contextual action buttons trong Review Drawer (Pending) | 1. Đăng nhập -> Approvals -> Pending -> Review 2. Quan sát footer drawer | Footer hiển thị 3 nút: [Approve] (xanh), [Revise] (mở Revision Modal), [Reject] (đỏ). Click mỗi nút thực hiện action tương ứng | Content Pending |
| AP-38 | Contextual action buttons trong Review Drawer (Approved) | 1. Đăng nhập -> Approved -> Review 2. Quan sát footer | Footer hiển thị 2 nút: [Post Now], [Schedule]. Không có Approve/Reject vì đã Approved | Content Approved |
| AP-39 | Contextual action buttons trong Review Drawer (Rejected) | 1. Đăng nhập -> Rejected -> Review 2. Quan sát footer | Footer hiển thị nút [Delete Rejected Content] (đỏ). Không có Approve/Reject vì đã Rejected | Content Rejected |
| AP-40 | Đóng Review Drawer | 1. Đăng nhập -> mở Review Drawer 2. Click nút Close/X hoặc click ngoài drawer | Drawer đóng (slide-out). Danh sách approvals không thay đổi. Không có action nào được thực hiện | -- |
| AP-41 | Review Drawer hiển thị content có ảnh | 1. Đăng nhập -> Approvals -> content IMAGE -> Review | Gradient preview area hiển thị ảnh thumbnail (nếu có). Type icon là "image". Detail hiển thị đúng type IMAGE | Content IMAGE |
| AP-42 | Review Drawer hiển thị content AI-generated | 1. Đăng nhập -> Approvals -> content do AI tạo -> Review | Requester hiển thị badge "AI" (gradient circle với chữ "AI"). Timeline audit: event đầu tiên là AI generation. Khác với content Manual (Requester badge "Manual") | Content AI-generated |

### 10.6 POST-APPROVAL ACTIONS (AP-43 -> AP-46)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-43 | Post Now từ Approved row | 1. Đăng nhập test@example.com / Pass1234 2. Approvals -> tab Approved 3. Hover row -> click Post Now (send icon) 4. Quan sát | PostNowModal mở: chọn social integration, xác nhận post. Gọi publishContent(contentId, integrationId). Content chuyển sang Published -> xuất hiện trong tab Published | Content Approved + có integration |
| AP-44 | Schedule từ Approved row | 1. Đăng nhập -> Approvals -> Approved 2. Hover row -> click Schedule (calendar icon) 3. Quan sát | Redirect đến /calendar?contentId=[id]. Trang Calendar mở với content được chọn sẵn để chọn ngày giờ + integration | Content Approved |
| AP-45 | Post Now từ Asset Review Drawer (Approved) | 1. Đăng nhập -> Approved -> Review (drawer) 2. Click "Post Now" trong footer drawer | PostNowModal mở, drawer vẫn mở hoặc đóng (tùy implementation). Sau khi post -> content chuyển Published | Content Approved |
| AP-46 | Không hiển thị Post Now/Schedule cho content chưa Approved | 1. Đăng nhập -> Approvals -> Pending 2. Hover row -> quan sát actions | Row Pending chỉ hiện: Approve, Request Changes, Reject, Review (eye), Lock (disabled). KHÔNG có Post Now hoặc Schedule | Content Pending |

### 10.7 POST-REJECTION ACTIONS (AP-47 -> AP-49)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-47 | Delete content từ Rejected row | 1. Đăng nhập test@example.com / Pass1234 2. Approvals -> tab Rejected 3. Hover row -> click Delete (red trash) 4. Confirm dialog (window.confirm) -> OK | Row biến mất. Count Rejected giảm 1. API DELETE /content/{id}. Content bị xóa mềm (isDeleted=true) | Có content Rejected |
| AP-48 | Hủy Delete Rejected (Cancel confirm) | 1. Đăng nhập -> Rejected -> Delete 2. Confirm dialog -> Cancel | Row vẫn trong Rejected. Không API call | -- |
| AP-49 | Resubmit content Rejected (đổi status về PendingApproval) | 1. Đăng nhập -> Content page hoặc API 2. updateContent(id, {status: 1}) với content Rejected 3. Vào Approvals | ValidateStatusTransition: Rejected -> PendingApproval hợp lệ. Content xuất hiện lại trong tab Pending, sẵn sàng duyệt lại | Content Rejected |

### 10.8 PERMISSIONS & ACCESS CONTROL (AP-50 -> AP-55)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-50 | Owner có thể Approve/Reject | 1. Đăng nhập Owner 2. Approvals -> Approve 1 content -> Reject 1 content | Cả 2 action thành công. Permission ManageContent = true cho Owner | Owner |
| AP-51 | Manager có thể Approve/Reject | 1. Đăng nhập Manager 2. Approvals -> Approve/Reject content | Cả 2 action thành công. ManageContent = true cho Manager | Manager |
| AP-52 | ContentCreator có thể Approve/Reject | 1. Đăng nhập ContentCreator 2. Approvals -> Approve/Reject | Cả 2 action thành công. ManageContent = true cho ContentCreator | ContentCreator |
| AP-53 | Viewer KHÔNG thể Approve/Reject | 1. Đăng nhập Viewer 2. Vào Approvals 3. Quan sát UI và thử gọi API | UI: các nút Approve/Reject/Request Changes bị ẩn hoặc disabled. API: PUT /content/{id} status=2/3 -> 403 Forbidden. Viewer chỉ có quyền GET (xem) | Viewer |
| AP-54 | Chưa đăng nhập truy cập /approvals | 1. Mở browser, chưa login 2. Truy cập https://[domain]/approvals | Redirect về /login. Sau login -> redirect về /approvals (hoặc /dashboard) | Chưa login |
| AP-55 | Token hết hạn khi Approve/Reject | 1. Đăng nhập -> Approvals 2. Xóa token khỏi localStorage 3. Click Approve hoặc Reject | API 401 -> redirect /login + message "Session expired". UI có thể refresh token tự động | Token hết hạn |

### 10.9 EDGE CASES & UI (AP-56 -> AP-60)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AP-56 | Lock button luôn disabled | 1. Đăng nhập test@example.com / Pass1234 2. Approvals -> hover row bất kỳ 3. Quan sát nút Lock (biểu tượng khóa) | Nút Lock luôn disabled (xám, cursor not-allowed). Tooltip khi hover: "Leader only". Không ai click được | -- |
| AP-57 | Loading skeleton khi load Approvals | 1. Đăng nhập -> Slow 3G -> Approvals | Hiển thị skeleton loading (shimmer hoặc spinner). Sau khi load -> bảng hiển thị với data | -- |
| AP-58 | Export CSV | 1. Đăng nhập -> Approvals 2. Click nút Export (nếu có) | Tải file CSV chứa các cột: Title, Brand, Type, Status, Platforms, Created. Data khớp với bảng hiển thị | Có content |
| AP-59 | Real-time update sau khi Approve/Reject (polling/refetch) | 1. Đăng nhập -> Approvals -> Approve 1 content 2. Quan sát tab counts và list | Sau approve: row biến mất khỏi Pending (remove khỏi local state). Count Pending giảm, Approved tăng. Nếu có polling -> tự động. Nếu không -> cần manual refresh | -- |
| AP-60 | Double click Reject -> confirm modal | 1. Đăng nhập -> Approvals -> Pending 2. Click Reject 2 lần nhanh | Lần 1: mở confirm modal. Lần 2: modal đã mở -> không mở thêm modal thứ 2. Chỉ reject 1 lần sau khi confirm | -- |

**Module:** APPROVAL | **Total:** 52 cases | **Page:** `/approvals` | **API:** PUT `/content/{id}`, GET `/content`, DELETE `/content/{id}`

---

## SHEET 11/19: SOCIAL -- Social Accounts Management (60 cases)

| **Feature** | Social Accounts -- Kết nối & quản lý tài khoản mạng xã hội (Facebook, Instagram, TikTok) |
|---|---|
| **Test requirement** | Social Accounts page `/social`: list view với Grid card, stats (connected/expired accounts count), filter bar (platform, status, search), empty state; Connect Account flow: chọn platform (Facebook/Instagram/TikTok), chọn Brand, OAuth redirect, callback, xử lý lỗi callback; SocialAccountCard: hiển thị platform icon, account name, status badge (connected/expired/error), số targets, nút Manage/Delete; Manage Targets Modal: load available targets từ provider, hiển thị targets (profile picture, name, type, category, trạng thái linked/unlinked/locked), chọn brand, link targets với brand (1 Page chỉ 1 Brand, 1 Brand có nhiều Page); Disconnect/Delete: single delete với confirm modal ("All linked targets will be unlinked"), bulk delete; OAuth flows per platform (Facebook relay, Instagram relay, TikTok in-browser exchange); Access control; Loading skeleton; Toast messages |
| **Pages** | `/social` |
| **API** | GET `/social/accounts/me`, GET `/social-auth/{platform}`, POST `/social-auth/{platform}/callback`, GET `/social/accounts/{id}/available-targets`, POST `/social/accounts/{id}/link-targets`, DELETE `/social/accounts/{id}`, DELETE `/social/integrations/{id}`, GET `/social/integrations/brand/{brandId}` |

### 11.1 SOCIAL ACCOUNTS LIST -- Danh sách tài khoản (SC-01 -> SC-10)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-01 | Truy cập trang Social Accounts với tài khoản đã kết nối | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Từ sidebar, click "Social Accounts" (icon public, mục Marketing) 5. Quan sát toàn bộ trang | Header: breadcrumbs "Dashboard > Social Accounts", title "Social Accounts", subtitle "Manage your connected social media accounts". Stats cards: tổng accounts, connected count, expired count, có thể có platforms breakdown. Grid: các SocialAccountCard với platform icon (Facebook xanh, Instagram hồng, TikTok đen), account name, status badge (connected xanh/expired cam/error đỏ), số targets (pages), avatar. Nút "Connect Account" góc trên phải. Filter bar: search, platform filter, status filter | Đã kết nối ít nhất 1 social account |
| SC-02 | Empty State (chưa có tài khoản nào) | 1. Đăng nhập vào workspace mới chưa kết nối social 2. Từ sidebar, click "Social Accounts" | Hiển thị SocialEmptyState: icon public lớn, text "No social accounts yet", subtext "Connect your first social media account to start publishing", nút "Connect Account" nổi bật. Có thể có illustration. Stats = 0 | Chưa có tài khoản |
| SC-03 | SocialAccountCard hiển thị đúng thông tin | 1. Đăng nhập -> Social Accounts 2. Quan sát 1 card tài khoản Facebook đã kết nối | Card hiển thị: platform icon (màu xanh Facebook), account name (từ providerUserId), status badge "Connected" (xanh lá), số targets (vd: "3 pages"), avatar/profile picture. Nút "Manage" và nút Delete (thùng rác) ở góc hoặc khi hover | Có tài khoản Facebook |
| SC-04 | SocialAccountCard hiển thị trạng thái Expired | 1. Đăng nhập -> Social Accounts 2. Quan sát card tài khoản có expiresAt < hiện tại | Status badge "Expired" (màu cam). Icon cảnh báo. Card vẫn hiển thị nhưng có thể có cảnh báo "Token expired. Reconnect needed" | Tài khoản có token hết hạn |
| SC-05 | SocialAccountCard hiển thị trạng thái Error | 1. Đăng nhập -> Social Accounts 2. Quan sát card tài khoản có isActive = false | Status badge "Error" (màu đỏ). Card có thể hiển thị khác biệt (opacity thấp hơn). Nút Manage có thể bị disable | Tài khoản bị vô hiệu hóa |
| SC-06 | Search trong Social Accounts | 1. Đăng nhập -> Social Accounts 2. Nhập tên tài khoản vào ô search 3. Quan sát | Chỉ hiện card có accountName chứa từ khóa (case-insensitive). Các card không khớp bị ẩn. Kết hợp với filter platform + status -> giao | Có nhiều tài khoản |
| SC-07 | Filter theo Platform | 1. Đăng nhập -> Social Accounts 2. Chọn filter Platform: "Facebook" 3. Quan sát 4. Đổi sang "Instagram" | "Facebook": chỉ hiện card Facebook. "Instagram": chỉ hiện card Instagram. Filter chip hiển thị platform đã chọn | Có tài khoản Facebook và Instagram |
| SC-08 | Filter theo Status | 1. Đăng nhập -> Social Accounts 2. Filter status: "Connected" -> "Expired" | Mỗi filter hiển thị đúng trạng thái. Kết hợp platform + status -> giao | Có tài khoản nhiều trạng thái |
| SC-09 | Stats cards hiển thị đúng số liệu | 1. Đăng nhập -> Social Accounts 2. Đếm thủ công số tài khoản từng loại 3. So với stats cards | Stats hiển thị đúng: tổng accounts, connected, expired, error. Số khớp với danh sách | Có tài khoản nhiều trạng thái |
| SC-10 | Loading skeleton khi load Social Accounts | 1. Đăng nhập -> Slow 3G -> Social Accounts 2. Quan sát trạng thái loading | Hiển thị 6 skeleton cards (animate-pulse): khung xám với placeholder cho icon, title, status, targets. Sau khi load -> cards thật hiển thị | -- |

### 11.2 CONNECT ACCOUNT -- Kết nối tài khoản (SC-11 -> SC-22)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-11 | Mở Connect Account Modal | 1. Đăng nhập test@example.com / Pass1234 2. Social Accounts -> click "Connect Account" 3. Quan sát modal | ConnectAccountModal mở. Hiển thị 3 nút chọn platform dạng grid: Facebook (icon xanh), Instagram (icon hồng/cam), TikTok (icon đen). Dropdown Brand để chọn brand. Security info box: "Your credentials are never stored on our servers." Nút Connect bị disable cho đến khi chọn brand | -- |
| SC-12 | Chọn platform và brand trong Connect Modal | 1. Đăng nhập -> Connect Account 2. Click chọn platform "Facebook" -> nút được highlight 3. Mở dropdown Brand -> chọn "My Brand" 4. Quan sát nút Connect | Platform Facebook được highlight (border đổi màu). Brand hiển thị trong dropdown. Sau khi chọn cả 2 -> nút "Connect" enabled (không còn disabled) | Có brand "My Brand" |
| SC-13 | Kết nối Facebook thành công (full flow) | 1. Đăng nhập -> Social Accounts -> Connect Account 2. Chọn platform: Facebook, chọn Brand: "My Brand" 3. Click Connect 4. Redirect đến Facebook OAuth consent screen 5. Đăng nhập Facebook, cấp quyền (pages_manage_posts, pages_read_engagement) 6. Facebook redirect về /auth/facebook/callback (hoặc social-callback) 7. FE gọi POST /api/social-auth/facebook/callback 8. Redirect về /social?manageAccount={id} 9. Manage Targets Modal tự động mở | Toast không hiển thị trong flow OAuth (do redirect). Kết quả: SocialAccount mới xuất hiện trong list với platform Facebook, status Connected. Manage Targets Modal mở sẵn để chọn pages | GOOGLE_CLIENT_ID đã config, Facebook App đã setup |
| SC-14 | Kết nối Instagram thành công | 1. Đăng nhập -> Social Accounts -> Connect 2. Chọn platform: Instagram, chọn Brand 3. Click Connect 4. Instagram OAuth -> cấp quyền 5. Redirect về /social-callback/instagram -> /auth/instagram/callback -> /auth/instagram/complete 6. POST /api/social-auth/instagram/callback 7. Redirect /social?manageAccount={id} | SocialAccount Instagram mới xuất hiện. Manage Targets Modal mở để liên kết Instagram Business Account với Brand | Instagram App đã setup |
| SC-15 | Kết nối TikTok thành công | 1. Đăng nhập -> Social Accounts -> Connect 2. Chọn platform: TikTok, chọn Brand 3. Click Connect 4. TikTok OAuth -> cấp quyền 5. Redirect về /social-callback/tiktok 6. In-browser: đọc token từ localStorage, gọi POST /api/social-auth/tiktok/callback 7. Auto-link targets nếu có brandId trong sessionStorage 8. Redirect /social | SocialAccount TikTok mới xuất hiện. Nếu auto-link -> targets đã được liên kết. Nếu không -> cần vào Manage để link thủ công | TikTok App đã setup |
| SC-16 | Hủy OAuth giữa chừng (user từ chối quyền) | 1. Đăng nhập -> Connect -> chọn Facebook/Brand -> Connect 2. Tại Facebook consent screen -> click "Cancel" hoặc "Deny" 3. Facebook redirect về callback URL với error params | FE callback page nhận error=access_denied. Hiển thị message lỗi: "Failed to connect Facebook account. Redirecting..." Tự redirect về /social sau vài giây. Không tạo SocialAccount | -- |
| SC-17 | Connect bị lỗi mạng khi lấy auth URL | 1. Đăng nhập -> Connect -> chọn platform + brand 2. DevTools -> Offline 3. Click Connect | API GET /api/social-auth/facebook lỗi -> toast hiển thị message: **"Failed to get authorization URL"** hoặc error từ API. Modal vẫn mở, có thể thử lại | Mất mạng |
| SC-18 | Connect bị lỗi callback (code/state không hợp lệ) | 1. Đăng nhập -> giả lập callback với code sai 2. Gọi POST /api/social-auth/facebook/callback với code="invalid" | API trả lỗi. FE callback page hiển thị message lỗi. Redirect về /social sau timeout. Không tạo SocialAccount | -- |
| SC-19 | TikTok callback thiếu code hoặc state | 1. Truy cập /social-callback/tiktok không có query params 2. Quan sát | HTML page hiển thị error: **"TikTok returned incomplete callback parameters."** Có nút "Back to Social Accounts" để quay về. Không crash | Chưa login cũng được (page có xử lý) |
| SC-20 | TikTok callback thiếu token/workspace trong localStorage | 1. Mở browser ẩn danh, xóa localStorage 2. Truy cập /social-callback/tiktok?code=xxx&state=yyy | HTML page hiển thị error: **"AISAM login or workspace session is missing."** Có nút "Back to Social Accounts". Không gọi API | Chưa login |
| SC-21 | TikTok callback timeout (30s) | 1. Đăng nhập -> Connect TikTok -> callback 2. Giả lập network chậm >30s khi gọi POST /api/social-auth/tiktok/callback | HTML page hiển thị error timeout: **"Request timed out."** Có nút retry hoặc "Back to Social Accounts" | Network chậm |
| SC-22 | Connect Modal đóng khi click ra ngoài | 1. Đăng nhập -> Connect Account (modal mở) 2. Click ra ngoài vùng modal | Modal đóng. Không có platform hoặc brand nào được lưu. Có thể mở lại bình thường | -- |

### 11.3 MANAGE TARGETS -- Quản lý Page/Target (SC-23 -> SC-32)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-23 | Mở Manage Targets Modal | 1. Đăng nhập test@example.com / Pass1234 2. Social Accounts -> click "Manage" trên 1 card Facebook 3. Quan sát modal | ManageTargetsModal mở. Title: "Choose Brand & Page". Loading spinner khi đang fetch available targets. Sau khi load: dropdown Brand (label "Brand đăng bài") auto-select brand từ sessionStorage hoặc brand đã link. Constraint text: "Một Brand có thể chọn nhiều Page. Một Page chỉ được thuộc một Brand." Danh sách targets với checkbox, avatar, name, type, category | Có tài khoản Facebook |
| SC-24 | Load available targets từ Facebook | 1. Đăng nhập -> Manage Facebook account 2. Quan sát danh sách targets sau khi load | Danh sách hiển thị tất cả Facebook Pages mà user quản lý. Mỗi target: profile picture (hoặc icon page), name, type ("page"), category. Targets chưa link -> checkbox unchecked, màu bình thường. Targets đã link vào brand đang chọn -> auto-checked, màu xanh lá, text "Linked to this brand". Targets đã link vào brand khác -> checkbox disabled, màu đỏ, text "Already linked to {brandName}" | Facebook user có 3+ pages |
| SC-25 | Chọn brand trong Manage Targets | 1. Đăng nhập -> Manage account 2. Đổi brand từ dropdown Brand 3. Quan sát danh sách targets | Khi đổi brand: targets đã link vào brand mới -> auto-checked (xanh). Targets đã link vào brand cũ -> unchecked (bình thường). Targets link vào brand thứ 3 -> vẫn disabled (đỏ) | Có nhiều brand, targets đã link vào các brand khác |
| SC-26 | Chọn targets và Save Mapping | 1. Đăng nhập -> Manage account 2. Chọn brand "My Brand" 3. Check 2 pages chưa link 4. Click "Save Mapping" | Nút "Save Mapping" chuyển spinner. API POST /api/social/accounts/{id}/link-targets với body: {provider: "facebook", providerTargetIds: [...], brandId: "..."}. Toast hiển thị: **"Targets linked successfully"**. Modal đóng. Card cập nhật số targets (+2). sessionStorage "social_connect_brand_id" bị xóa | Có 2+ pages chưa link |
| SC-27 | Deselect targets (bỏ link) | 1. Đăng nhập -> Manage account 2. Bỏ check 1 target đang "Linked to this brand" 3. Click Save Mapping | API gọi link-targets với danh sách mới (không bao gồm target bị bỏ). Target đó bị unlink khỏi brand. Card cập nhật số targets (-1) | Có target đã link |
| SC-28 | Select All / Deselect All | 1. Đăng nhập -> Manage account 2. Click "Select All" 3. Click "Deselect All" | Select All: tất cả targets khả dụng (chưa bị lock) được check. Deselect All: bỏ check tất cả. Các targets bị lock (đã link brand khác) không bị ảnh hưởng | Có nhiều targets |
| SC-29 | Save Mapping thất bại (brand không hợp lệ) | 1. Đăng nhập -> Manage account 2. Không chọn brand (nếu có thể) -> Save Mapping | Nếu brand rỗng -> nút Save disabled hoặc API trả lỗi. Message lỗi hiển thị dưới danh sách targets: **"Please select a brand"** hoặc lỗi từ API | -- |
| SC-30 | Save Mapping khi không chọn target nào | 1. Đăng nhập -> Manage account -> Deselect All -> Save Mapping | API gọi link-targets với providerTargetIds = []. Kết quả: tất cả targets của brand này bị unlink. Card cập nhật số targets = 0. Toast: **"Targets linked successfully"** | Có target đã link |
| SC-31 | URL auto-open Manage Modal (?manageAccount=id) | 1. Đăng nhập -> truy cập /social?manageAccount={accountId} 2. Quan sát | Trang Social load, sau đó ManageTargetsModal tự động mở cho account có id tương ứng. Đây là flow sau khi OAuth callback redirect | Có account với id đó |
| SC-32 | Đóng Manage Targets Modal | 1. Đăng nhập -> Manage account (modal mở) 2. Click ngoài modal hoặc nút Close/X | Modal đóng. Danh sách accounts không thay đổi (chưa Save). Có thể mở lại và thấy targets vẫn ở trạng thái cũ | Đang trong Manage modal |

### 11.4 DISCONNECT / DELETE -- Xóa tài khoản (SC-33 -> SC-40)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-33 | Xóa 1 tài khoản (confirm modal) | 1. Đăng nhập test@example.com / Pass1234 2. Social Accounts -> click nút Delete (thùng rác) trên card Facebook 3. DisconnectConfirmModal mở: "Are you sure you want to delete {displayName}?" + warning: "All linked targets will be unlinked." 4. Click "Delete" | Modal đóng. Nút Delete chuyển spinner. API DELETE /api/social/accounts/{id}. Toast hiển thị: **"Account deleted successfully"**. Card biến mất khỏi list. Stats cập nhật | Có tài khoản Facebook |
| SC-34 | Hủy Delete (Cancel modal) | 1. Đăng nhập -> Delete account 2. Modal confirm hiện -> click "Cancel" hoặc ngoài modal | Modal đóng. Card vẫn trong list. Không API call. Không toast | -- |
| SC-35 | Bulk Delete nhiều tài khoản | 1. Đăng nhập -> Social Accounts 2. Check chọn 2 tài khoản (checkbox trên card hoặc select mode) 3. BulkActionsBar hiện: "2 selected" + nút "Delete Selected" 4. Click "Delete Selected" 5. Xác nhận (nếu có confirm) | Gọi DELETE cho từng id tuần tự. Toast: **"2 accounts deleted"** hoặc **"Deleted X/2 accounts. Some could not be deleted."** Các card biến mất. Stats cập nhật | Có 2+ tài khoản |
| SC-36 | Xóa tài khoản đang có targets linked | 1. Đăng nhập -> xóa tài khoản Facebook có 3 pages đã link 2. Xác nhận Delete | API DELETE /api/social/accounts/{id}. Tất cả SocialIntegrations liên kết cũng bị xóa (cascade hoặc soft-delete). Brand không còn pages nào từ tài khoản này | Tài khoản có 3 targets |
| SC-37 | Xóa tài khoản -> targets bị unlink khỏi brand | 1. Đăng nhập -> xóa tài khoản 2. Vào Manage Targets của brand cũ | Brand không còn targets từ tài khoản đã xóa. Nếu brand có targets từ tài khoản khác -> vẫn hiển thị bình thường | -- |
| SC-38 | Double click Delete | 1. Đăng nhập -> Delete 1 account 2. Click Delete 2 lần liên tiếp nhanh | Lần 1: mở confirm modal, nút loading. Lần 2: không trigger (modal đã mở hoặc nút đã disabled). Chỉ xóa 1 lần | -- |
| SC-39 | Delete account đang loading -> chuyển spinner | 1. Đăng nhập -> Delete account 2. Quan sát nút Delete trong quá trình xóa | Nút Delete trên card chuyển thành spinner nhỏ. Không thể click thêm. Sau khi xong -> card biến mất | -- |
| SC-40 | Xóa Integration riêng lẻ (qua API) | 1. Đăng nhập -> gọi API DELETE /api/social/integrations/{integrationId} 2. Reload Social Accounts | Integration bị xóa. Target đó không còn link với brand. Nếu vào Manage Targets -> target hiển thị lại ở trạng thái unchecked (có thể link lại) | Có integration |

### 11.5 OAUTH CALLBACK HANDLING -- Xử lý callback (SC-41 -> SC-47)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-41 | Facebook callback thành công | 1. Đăng nhập -> Connect Facebook -> authorize trên Facebook 2. Facebook redirect về callback URL với ?code=xxx&state=yyy 3. Quan sát callback page | Callback page hiển thị loading/spinner ngắn. Gọi POST /api/social-auth/facebook/callback. Nếu thành công -> redirect /social?manageAccount={newAccountId}. Manage Targets Modal tự mở. Toast không hiển thị (do redirect) | OAuth code hợp lệ |
| SC-42 | Facebook callback relay (BE nhận callback) | 1. Flow: Facebook redirect về BE URL (do cấu hình) 2. SocialAuthRelayController GET /api/social-auth/facebook/callback 3. Quan sát | BE đọc FrontendSettings.BaseUrl, redirect (302) về FE tại /social-callback/facebook?{toàn bộ query string}. FE xử lý tiếp như SC-41. User không thấy sự khác biệt | BE callback URL được cấu hình |
| SC-43 | Instagram callback relay chain | 1. Đăng nhập -> Connect Instagram -> authorize 2. Instagram redirect về /social-callback/instagram (public URL) 3. Quan sát redirect chain | /social-callback/instagram -> 302 redirect về /auth/instagram/callback (qua INSTAGRAM_LOCAL_CALLBACK_URL) -> 302 redirect về /auth/instagram/complete -> page client gọi POST /api/social-auth/instagram/callback -> redirect /social?manageAccount={id}. Tổng 3 lần redirect, user thấy loading | OAuth code hợp lệ |
| SC-44 | TikTok callback in-browser exchange | 1. Đăng nhập -> Connect TikTok -> authorize 2. TikTok redirect về /social-callback/tiktok?code=xxx&state=yyy 3. Quan sát page HTML | Page HTML hiển thị card với spinner animation + text "Connecting your TikTok account...". JS đọc code, state, token (localStorage "aisam_token"), workspace (localStorage "aisam_active_workspace"). Gọi POST /api/social-auth/tiktok/callback với Authorization header. Nếu có brandId trong sessionStorage -> auto-link targets. Sau đó redirect /social. Nếu thành công -> toast: **"TikTok account connected successfully"** | OAuth code hợp lệ, đã login |
| SC-45 | TikTok callback redirect về local (TIKTOK_LOCAL_CALLBACK_URL set) | 1. ENV TIKTOK_LOCAL_CALLBACK_URL được set 2. TikTok redirect về social-callback/tiktok (public URL) 3. Route kiểm tra origin khác -> 302 redirect về local callback URL | User được redirect về localhost callback thay vì public URL. Sau đó flow in-browser exchange tiếp tục như SC-44 | ENV được set |
| SC-46 | Instagram callback thiếu INSTAGRAM_LOCAL_CALLBACK_URL | 1. ENV không set INSTAGRAM_LOCAL_CALLBACK_URL 2. Instagram redirect về /social-callback/instagram | Route dùng default: http://localhost:3000/auth/instagram/callback. Nếu đang ở production domain -> redirect về localhost có thể fail. Ghi nhận thực tế | ENV không set |
| SC-47 | Callback page hiển thị error và redirect về /social | 1. Giả lập callback với code sai 2. Callback page nhận lỗi từ API | Page hiển thị message lỗi (vd: "Failed to connect Facebook account. Redirecting..."). Sau 2-3 giây tự redirect về /social. Không tạo SocialAccount | Code sai |

### 11.6 SOCIAL ACCOUNT CARD ACTIONS (SC-48 -> SC-52)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-48 | Card hiển thị số targets đúng | 1. Đăng nhập test@example.com / Pass1234 2. Social Accounts -> quan sát card Facebook 3. So sánh số targets hiển thị với thực tế | Số targets hiển thị trên card (vd: "3 pages") khớp với số SocialIntegrations của account đó. Sau khi Manage Targets -> số cập nhật | Có tài khoản với targets |
| SC-49 | Card hiển thị Account Name từ providerUserId | 1. Đăng nhập -> Social Accounts -> quan sát card | Account name hiển thị là accountName (được map từ providerUserId hoặc AccountId trong BE DTO). Không phải là ID thô | Có tài khoản |
| SC-50 | Card platform icon đúng màu | 1. Đăng nhập -> Social Accounts 2. Quan sát icon các card khác platform | Facebook: icon màu xanh #1877F2. Instagram: icon gradient hồng/cam/tím. TikTok: icon đen #111111. Mỗi card hiển thị đúng icon platform | Có tài khoản cả 3 platform |
| SC-51 | Card hiển thị trạng thái token sắp hết hạn | 1. Đăng nhập -> Social Accounts 2. Quan sát card có expiresAt gần hiện tại (trong 7 ngày) | Có thể hiển thị badge "Expiring soon" hoặc warning icon. Ghi nhận thực tế: FE hiện chỉ phân biệt connected/expired/error, có thể không có cảnh báo sắp hết hạn | Token sắp hết hạn |
| SC-52 | Refresh token tự động / manual | 1. Đăng nhập -> Social Accounts 2. Tìm nút Refresh Token (nếu có) | Ghi nhận: FE hiện không có nút Refresh Token. Token hết hạn -> phải reconnect. Nếu BE có refresh token mechanism -> token tự động được refresh khi gọi API | Token sắp hết hạn |

### 11.7 PERMISSIONS & ACCESS CONTROL (SC-53 -> SC-56)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-53 | Chưa đăng nhập truy cập /social | 1. Mở browser, chưa login 2. Truy cập https://[domain]/social | Redirect về /login. Sau login -> redirect về /social. Không hiển thị nội dung | Chưa login |
| SC-54 | Token hết hạn khi thao tác | 1. Đăng nhập -> Social Accounts 2. Xóa token localStorage 3. Click Connect/Manage/Delete | API 401 -> redirect /login + message "Session expired". Hoặc tự refresh token | Token hết hạn |
| SC-55 | Cross-workspace: tài khoản social của workspace A không hiển thị trong workspace B | 1. Đăng nhập User A (workspace A) -> connect Facebook 2. Switch sang workspace B 3. Vào Social Accounts | Không thấy tài khoản Facebook đã connect ở workspace A. API GET /social/accounts/me filter theo X-Workspace-Id | A có 2 workspace |
| SC-56 | Viewer không có quyền Connect/Delete (nếu có RBAC) | 1. Đăng nhập Viewer 2. Social Accounts -> quan sát UI | Ghi nhận: FE không phân biệt quyền cho social page (không có feature gate). API endpoints yêu cầu [Authorize] nhưng không check role cụ thể. Viewer có thể connect/delete nếu BE không chặn. Cần ghi nhận thực tế | Viewer |

### 11.8 EDGE CASES & UI (SC-57 -> SC-60)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-57 | Connect lại tài khoản đã tồn tại (cùng provider + providerUserId) | 1. Đăng nhập -> đã có Facebook account 2. Connect Account -> chọn Facebook + Brand -> Connect 3. Facebook OAuth -> authorize cùng tài khoản 4. Callback xử lý | Ghi nhận: BE có thể tạo SocialAccount mới (trùng providerUserId) hoặc update account cũ. Nếu update -> token mới thay token cũ, targets giữ nguyên. Nếu tạo mới -> 2 account cùng providerUserId. Cần ghi nhận thực tế | Đã có Facebook account |
| SC-58 | Mất mạng khi load Social Accounts list | 1. Đăng nhập -> DevTools Offline -> Social Accounts | API GET /social/accounts/me fail -> hiển thị empty state hoặc error message. Có nút Retry? Ghi nhận thực tế. Không crash | Mất mạng |
| SC-59 | Mất mạng khi Save Mapping trong Manage Targets | 1. Đăng nhập -> Manage account -> chọn targets 2. DevTools Offline -> Save Mapping | API POST link-targets fail -> error message hiển thị dưới danh sách targets (màu đỏ). Targets vẫn ở trạng thái checked. Có thể thử lại | Mất mạng |
| SC-60 | Refresh trang Social Accounts giữ nguyên state | 1. Đăng nhập -> Social Accounts -> filter "Instagram" + search "test" 2. F5 reload | Ghi nhận: filter/search state có thể mất (không persist qua URL params). List accounts load lại đầy đủ. View không bị crash | -- |

**Module:** SOCIAL | **Total:** 60 cases | **Page:** `/social` | **API:** GET `/social/accounts/me`, GET/POST `/social-auth/{platform}`, GET/POST `/social/accounts/{id}/*`, DELETE `/social/accounts/{id}`, DELETE `/social/integrations/{id}`

---

## SHEET 12/19: SCHEDULE -- Content Scheduling & Calendar (58 cases)

| **Feature** | Content Schedule -- Lập lịch đăng bài, Calendar View, Background Publishing |
|---|---|
| **Test requirement** | Calendar page `/calendar`: 3 views (Month grid + Day Detail panel, Week columns, List table), feature gate (Free plan bị chặn), workflow summary 4 cards (Pending/Completed/Failed/Recurring); Create Schedule: chọn content (Approved/Published), chọn social integration (cùng brand), date + time; Validate (content chưa Approved, integration sai brand, thời gian quá khứ, trùng lịch); Bulk Schedule: stagger posting (offset 15m/30m/1h/2h), TikTok video restriction, multi-brand warning; Edit/Reschedule; Delete + Undo; Filters (brand, platform, status); Polling 30s; Background worker (claim SQL FOR UPDATE SKIP LOCKED, max 3 retries, Failed -> reset content Draft); Dashboard upcoming schedules widget; URL prefill (?contentId=); Feature gate (schedulePost); Access control |
| **Pages** | `/calendar` |
| **API** | GET/POST `/content-schedules`, GET/PUT/DELETE `/content-schedules/{id}`, POST `/content-schedules/bulk`, GET `/content-schedules/upcoming` |
| **Model** | `ContentCalendar` (content_id, integration_id, scheduled_at, status: Pending=0/Processing=1/Completed=2/Failed=3, attempt_count, last_error, is_deleted) |

### 12.1 CALENDAR PAGE -- Trang Calendar & Views (SC-01 -> SC-11)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-01 | Truy cập Calendar với schedule đã có | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Từ sidebar, click "Calendar" (icon calendar_month, mục Content Workspace) 5. Quan sát toàn bộ trang | Header: breadcrumbs "Dashboard > Calendar". View switcher: Month / Week / List (Month active mặc định). Nút "+ Schedule" góc trên phải. Workflow Summary 4 card: Pending (icon schedule, amber), Completed (check_circle, emerald), Failed (error, red), "Advanced Recurring — Coming Soon" (auto_awesome, tím, badge "Roadmap"). Calendar grid 7 cột (Sun-Sat), các ngày có schedule hiển thị chip màu theo status. Day Detail Panel bên phải hiển thị schedule của ngày được chọn. Filter bar: Brand dropdown, Platform icon buttons, Status dropdown. Legend bar: Completed=emerald, Pending=amber, Processing=sky, Failed=red | Plan Plus trở lên, có ít nhất 3 schedule |
| SC-02 | Feature Gate: Free plan bị chặn | 1. Đăng nhập user dùng Free plan 2. Từ sidebar, click "Calendar" | Hiển thị lock icon + text **"Content Calendar — This feature requires a paid Plus plan or higher..."** + nút "View Plans" link đến /pricing. Không hiển thị calendar grid hay schedule | Free plan |
| SC-03 | Feature Gate: Loading khi đang check subscription | 1. Đăng nhập -> bật Slow 3G 2. Vào Calendar 3. Quan sát trạng thái chờ | Hiển thị spinner xoay + text **"Checking subscription — Syncing your current workspace plan..."**. Sau khi check xong -> hiển thị calendar (nếu có quyền) hoặc gate (nếu Free) | -- |
| SC-04 | Month View mặc định | 1. Đăng nhập -> Calendar 2. Quan sát giao diện | Lịch dạng lưới 7 cột (CN -> T7). Mỗi ô ngày hiển thị số ngày, tối đa 3 schedule chip (màu theo status). Nếu >3 -> hiển thị "+N more". Ngày hiện tại được highlight. Ngày có schedule Completed -> chip xanh lá. Pending -> chip vàng. Failed -> chip đỏ | Có schedule nhiều ngày |
| SC-05 | Day Detail Panel (Month View) | 1. Đăng nhập -> Calendar (Month View) 2. Click vào 1 ngày có schedule 3. Quan sát panel bên phải | Panel hiển thị: tên ngày (vd: "Monday, July 20"), badge số lượng schedule. Mỗi schedule card hiển thị: date badge, type icon (article/image/play_circle), title, status badge, platform icon, giờ, brand name, nút Delete (X, hiện khi hover). Click card -> mở Edit modal | Có schedule trong ngày đó |
| SC-06 | Week View | 1. Đăng nhập -> Calendar 2. Click nút "Week" trong view switcher 3. Quan sát | 7 cột ngang (CN-T7) hiển thị tuần hiện tại. Mỗi cột có header ngày + schedule cards. Nút prev/next week (< >) để chuyển tuần. Schedule card hiển thị: platform icon, title, status dot màu, giờ, brand name. Click card -> Edit modal | Có schedule trong tuần |
| SC-07 | List View | 1. Đăng nhập -> Calendar 2. Click nút "List" trong view switcher 3. Quan sát | Bảng với cột sortable (click header để sort asc/desc): Date (date + time), Content (type icon + title), Brand (dot màu + tên), Platform (icon badge), Status (badge màu + dot), Attempts (số lần + last error), Actions (nút Delete). Không có Day Detail Panel | Có schedule |
| SC-08 | Workflow Summary cards hiển thị đúng số | 1. Đăng nhập -> Calendar 2. Đếm số schedule Pending/Completed/Failed 3. So với 3 card summary | Card Pending = count(status=Pending). Completed = count(Completed). Failed = count(Failed). Card "Recurring" luôn hiển thị "Coming Soon" + badge Roadmap | Có schedule đủ status |
| SC-09 | Empty State Month View (không có schedule) | 1. Đăng nhập vào workspace chưa có schedule 2. Calendar | Calendar grid hiển thị bình thường. Click vào ngày bất kỳ -> Day Detail Panel hiển thị message: **"No schedules for this day"** + icon event_busy. Không crash | Chưa có schedule |
| SC-10 | Empty State Week View | 1. Đăng nhập -> Calendar -> Week View khi tuần không có schedule | Mỗi ô ngày hiển thị text **"No schedules"** + icon event_busy | Chưa có schedule trong tuần |
| SC-11 | Empty State List View | 1. Đăng nhập -> Calendar -> List View khi không có schedule khớp filter | Hiển thị text **"No schedules match your filters"** + nút "Clear all filters" | Filter quá hẹp |

### 12.2 CREATE SCHEDULE -- Tạo lịch đăng bài (SC-12 -> SC-22)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-12 | Mở Create Schedule Modal | 1. Đăng nhập test@example.com / Pass1234 2. Calendar -> click "+ Schedule" 3. Quan sát modal | Modal mở. Hiển thị: dropdown "Select Content" (danh sách content: title + brandName), danh sách "Social Accounts" (checkbox các integration active), input Date, input Time, nút Create (hiển thị "Create N Schedules" với N = số integration được chọn). Nút Create disabled khi chưa chọn content hoặc integration | Có content Approved + integration |
| SC-13 | Tạo schedule thành công (1 content, 1 integration) | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn 1 content Approved từ dropdown 3. Tích 1 integration Facebook 4. Chọn Date: 25/07/2026, Time: 10:00 5. Click "Create 1 Schedule" | Nút chuyển spinner. API POST /content-schedules. Modal đóng. Schedule mới hiển thị trong Calendar (chip vàng Pending). Day Detail Panel hiển thị card mới. Summary Pending tăng 1. Toast: **"Schedule created"** (nếu FE có toast) | Content Approved + Facebook integration cùng brand |
| SC-14 | Tạo schedule với nhiều integration (1 content, 3 platforms) | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn 1 content 3. Tích 3 integration: Facebook, Instagram, TikTok 4. Date: 25/07, Time: 10:00 5. Click "Create 3 Schedules" | API gọi bulkCreateSchedules với 3 items (cùng contentId, 3 integrationId khác nhau). Tạo 3 schedule riêng biệt. Calendar hiển thị 3 chip cho ngày đó. Summary Pending tăng 3 | Content có đủ 3 integration cùng brand |
| SC-15 | Tạo schedule thất bại: content chưa Approved | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn content status Draft 3. Chọn integration -> Date/Time 4. Click Create | Dropdown content có thể không hiển thị content Draft (chỉ hiện Approved/Published). Nếu chọn được -> BE trả lỗi: **"Content must be approved before scheduling"**. Toast lỗi hiển thị | Content Draft |
| SC-16 | Tạo schedule thất bại: integration sai brand | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn content thuộc Brand A 3. Thử chọn integration thuộc Brand B | FE: integration sai brand bị gray out + text **"Linked to another brand"**, checkbox disabled. Nếu bypass -> BE trả lỗi **"Social integration not found"** (404) | Content Brand A, integration Brand B |
| SC-17 | Tạo schedule thất bại: thời gian quá khứ | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn content + integration 3. Date: ngày hôm qua, Time: 10:00 4. Click Create | BE trả lỗi: **"Scheduled time must be in the future"**. Toast lỗi. Modal vẫn mở, data giữ nguyên. Có thể sửa lại Date | -- |
| SC-18 | Tạo schedule thất bại: trùng lịch (duplicate) | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn content + integration đã có schedule active 3. Click Create | BE catch PostgreSQL unique constraint (23505). Trả lỗi: **"Content already has an active schedule"** (409 Conflict). Toast lỗi hiển thị | Content đã có schedule active |
| SC-19 | Tạo schedule: TikTok ẩn khi content không phải VIDEO | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn content type TEXT hoặc IMAGE 3. Quan sát danh sách integration | TikTok integration bị ẩn hoặc gray out kèm warning màu amber: nội dung không phải video. Các integration Facebook, Instagram vẫn chọn được bình thường | Content TEXT/IMAGE |
| SC-20 | Tạo schedule: không có integration active | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn content của brand chưa có integration active 3. Quan sát | Danh sách Social Accounts trống. Hiển thị inline message: **"No social accounts connected"** hoặc **"No active social accounts for this brand"**. Nút Create disabled | Brand chưa connect integration |
| SC-21 | URL prefill: ?contentId=xxx | 1. Đăng nhập -> truy cập /calendar?contentId={id} 2. Quan sát | Create Schedule Modal tự động mở. Content có id tương ứng được chọn sẵn trong dropdown. Date/Time tự động set về "now". User chỉ cần chọn integration và chỉnh thời gian | Content Approved |
| SC-22 | Đóng Create Modal không Save | 1. Đăng nhập -> "+ Schedule" 2. Chọn content + integration + date 3. Click ngoài modal hoặc nút Close/X | Modal đóng. Không có schedule được tạo. Mở lại modal -> form reset (không giữ data cũ) | -- |

### 12.3 BULK SCHEDULE -- Lập lịch hàng loạt (SC-23 -> SC-29)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-23 | Mở Bulk Schedule Modal từ Content page | 1. Đăng nhập test@example.com / Pass1234 2. Content -> check 3 content Approved cùng brand 3. Batch bar -> click "Schedule" 4. Quan sát | BulkScheduleModal mở. Hiển thị: input datetime-local cho start time, checkbox "Stagger posting" với interval buttons (15m, 30m, 1h, 2h), danh sách Social Accounts (checkbox), preview danh sách items với thời gian offset. Nút Create hiển thị số schedules | 3 content Approved cùng brand |
| SC-24 | Bulk Schedule với Stagger posting | 1. Đăng nhập -> Bulk Schedule với 3 content 2. Start time: 10:00 3. Tích "Stagger posting" -> chọn interval 30m 4. Chọn 1 integration Facebook 5. Click Create | Content 1: 10:00, Content 2: 10:30, Content 3: 11:00. Preview hiển thị đúng thời gian offset. 3 schedule được tạo. Toast hiển thị kết quả | -- |
| SC-25 | Bulk Schedule không Stagger | 1. Đăng nhập -> Bulk Schedule 3 content 2. Không tích Stagger 3. Chọn integration -> Create | Tất cả schedule cùng 1 thời điểm (10:00). 3 schedule được tạo | -- |
| SC-26 | Bulk Schedule: TikTok hidden cho content không phải video | 1. Đăng nhập -> Bulk Schedule với 2 content (1 TEXT, 1 VIDEO) 2. Quan sát integration list | TikTok integration bị ẩn hoặc uncheck + warning: không phải tất cả content đều là video. Các integration Facebook, Instagram vẫn hoạt động | Mix content TEXT + VIDEO |
| SC-27 | Bulk Schedule: multi-brand warning | 1. Đăng nhập -> Bulk Schedule với 2 content khác brand 2. Quan sát | Cảnh báo màu amber: **"Make sure the social account works for all brands"**. Người dùng vẫn có thể chọn integration và tạo schedule, nhưng BE sẽ từ chối integration không cùng brand với từng content | 2 content khác brand |
| SC-28 | Bulk Schedule: partial success | 1. Đăng nhập -> Bulk Schedule 3 content 2. 1 content đã có schedule active (trùng) 3. Click Create | BE trả về BulkCreateResultDto: successCount=2, failedCount=1. Toast: **"2/3 schedules created. Content already has an active schedule"**. 2 schedule mới hiển thị, 1 fail được báo | 1 content có schedule |
| SC-29 | Bulk Schedule: close modal | 1. Đăng nhập -> Bulk Schedule modal -> click ngoài | Modal đóng. Không schedule nào được tạo | -- |

### 12.4 EDIT / RESCHEDULE (SC-30 -> SC-34)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-30 | Edit schedule (đổi thời gian) | 1. Đăng nhập test@example.com / Pass1234 2. Calendar -> click vào schedule card (Month/Week) 3. Edit Modal mở: hiển thị integration hiện tại, Date, Time 4. Đổi Date sang 26/07, Time sang 14:00 5. Click Save | API PUT /content-schedules/{id} với {scheduledAt: mới}. Modal đóng. Schedule cập nhật sang ngày mới. Calendar chip di chuyển sang ngày 26. Nếu là List View -> row cập nhật. Toast có thể hiển thị "Schedule updated" | Schedule Pending |
| SC-31 | Edit schedule (đổi integration) | 1. Đăng nhập -> Calendar -> Edit schedule 2. Đổi integration từ Facebook sang Instagram (cùng brand) 3. Click Save | Schedule cập nhật integration mới. Platform icon trên card đổi từ Facebook sang Instagram. BE validate integration phải cùng brand | Có 2+ integration cùng brand |
| SC-32 | Edit schedule thất bại: schedule đã Completed | 1. Đăng nhập -> Calendar -> click schedule Completed 2. Thử Edit (nếu UI cho phép) 3. Click Save | BE trả lỗi: **"Completed schedules cannot be updated"** (400). Toast lỗi. Schedule không thay đổi. Nếu FE chặn -> nút Edit bị disable hoặc không hiển thị | Schedule Completed |
| SC-33 | Edit schedule thất bại: thời gian quá khứ | 1. Đăng nhập -> Edit schedule Pending 2. Đổi Date sang ngày hôm qua -> Save | BE trả lỗi: **"Scheduled time must be in the future"**. Modal vẫn mở, data giữ nguyên | Schedule Pending |
| SC-34 | Edit schedule: Failed -> reschedule tự reset về Pending | 1. Đăng nhập -> Calendar -> Edit schedule Failed 2. Đổi thời gian -> Save | BE tự động reset status từ Failed -> Pending (và attempt_count về 0?). Schedule hiển thị lại chip vàng Pending thay vì đỏ Failed | Schedule Failed |

### 12.5 DELETE & UNDO (SC-35 -> SC-39)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-35 | Delete schedule | 1. Đăng nhập test@example.com / Pass1234 2. Calendar -> Day Detail Panel -> hover schedule card -> click nút X (Delete) 3. Quan sát | Schedule bị xóa khỏi UI ngay lập tức. API DELETE /content-schedules/{id} (soft delete: isDeleted=true, isActive=false). Toast hiển thị: **"Schedule deleted"** kèm nút **Undo** | Có schedule |
| SC-36 | Undo Delete | 1. Đăng nhập -> Delete 1 schedule 2. Click nút "Undo" trong toast 3. Quan sát | Schedule hiển thị lại trong UI (prepend vào state). Toast hiển thị: **"Schedule restored"**. **[GHI NHẬN]** Undo chỉ restore in-memory state, BE vẫn isDeleted=true. Nếu F5 -> schedule biến mất vĩnh viễn | Vừa delete schedule |
| SC-37 | Delete schedule Completed | 1. Đăng nhập -> xóa schedule status Completed 2. Quan sát | Vẫn xóa được (soft delete). Ghi nhận: bài đã post trên social không bị ảnh hưởng | Schedule Completed |
| SC-38 | Delete trong List View | 1. Đăng nhập -> Calendar -> List View 2. Click nút Delete ở cột Actions | Tương tự SC-35. Row biến mất khỏi bảng. Toast + Undo | Schedule trong List View |
| SC-39 | Undo rồi F5 -> schedule mất | 1. Đăng nhập -> Delete -> Undo 2. F5 reload trang | **[GHI NHẬN]** Sau reload, schedule không còn trong list (vì BE đã isDeleted=true, Undo chỉ là FE state). Đây là behavior cần lưu ý khi test | Vừa Undo |

### 12.6 FILTERS & SEARCH (SC-40 -> SC-45)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-40 | Filter theo Brand | 1. Đăng nhập test@example.com / Pass1234 2. Calendar -> chọn brand từ dropdown Brand filter 3. Quan sát | Chỉ hiện schedule của content thuộc brand đã chọn. Các ngày không có schedule brand đó -> trống. Dropdown brand populated từ data schedule hiện có | Có schedule nhiều brand |
| SC-41 | Filter theo Platform | 1. Đăng nhập -> Calendar 2. Click icon button "Facebook" trong filter bar 3. Quan sát | Chỉ hiện schedule có integration platform = Facebook. Icon button được highlight. Click lại -> bỏ filter | Có schedule Facebook và Instagram |
| SC-42 | Filter theo Status | 1. Đăng nhập -> Calendar 2. Chọn Status: "Pending" -> "Completed" -> "Failed" | Mỗi filter hiển thị đúng status. Schedule chip đổi màu tương ứng. Kết hợp brand + platform + status -> giao | Có schedule đủ status |
| SC-43 | Clear All Filters | 1. Đăng nhập -> Calendar -> áp brand + platform + status 2. Click "Clear All Filters" | Tất cả filter reset. Calendar hiển thị toàn bộ schedule. Nút Clear All biến mất | Đang có filter |
| SC-44 | Polling 30s tự động refresh | 1. Đăng nhập -> Calendar 2. Mở tab khác -> tạo schedule mới qua API 3. Quay lại tab Calendar, đợi < 30s | Trong vòng 30s, Calendar tự động refetch và hiển thị schedule mới. Hoặc khi tab visibility change -> refetch. Custom event "onScheduleChange" cũng trigger refetch | -- |
| SC-45 | Sort trong List View | 1. Đăng nhập -> Calendar -> List View 2. Click header cột "Date" -> "Content" -> "Status" | Mỗi lần click toggle asc/desc. Mũi tên sort hiển thị trên header đang active. Data sắp xếp đúng | Có schedule trong List View |

### 12.7 BACKGROUND PUBLISHING -- Worker tự động đăng (SC-46 -> SC-51)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-46 | Worker claim due schedules | 1. Tạo schedule với scheduledAt = now + 1 phút 2. Đợi đến thời điểm scheduledAt 3. Quan sát Calendar sau vài phút | Background worker (poll 15-60s) pick up schedule. Status chuyển từ Pending -> Processing -> Completed. Nếu publish thành công -> status Completed (xanh lá), executedAt được set. Content status -> Published | Schedule Pending, scheduledAt gần |
| SC-47 | Worker retry khi publish fail (attempt 1) | 1. Giả lập publish fail (sai integration token) 2. Tạo schedule với scheduledAt = now 3. Đợi worker chạy | Worker attempt lần 1 fail. Status -> Failed (tạm). AttemptCount = 1, lastError = error message. Worker sẽ retry trong lần chạy sau (nếu attemptCount < 3) | Integration token sai |
| SC-48 | Worker retry lần 2 | 1. Schedule Failed với attemptCount=1 2. Đợi worker retry | Worker claim lại schedule (status=Failed AND attemptCount < 3). Attempt lần 2. Nếu vẫn fail -> attemptCount=2 | Schedule Failed, attempt=1 |
| SC-49 | Worker max retry (3 lần) -> Failed vĩnh viễn | 1. Schedule fail 3 lần liên tiếp 2. Quan sát sau lần thứ 3 | Status -> Failed (vĩnh viễn). AttemptCount = 3. Content status bị revert từ Approved -> Draft. Schedule không được retry nữa. Summary Failed tăng 1 | Schedule fail 2 lần trước |
| SC-50 | Worker publish thành công sau retry | 1. Schedule fail lần 1 2. Sửa integration token thành hợp lệ 3. Worker retry -> publish thành công | Status -> Completed. AttemptCount reset. executedAt = thời điểm publish thành công. Content status -> Published | Token đã sửa |
| SC-51 | Worker không claim khi workspace hết hạn | 1. Workspace bị expired 2. Tạo schedule Pending đến hạn 3. Quan sát | Worker bỏ qua schedule. Status vẫn Pending. Không retry. Log/error ghi nhận workspace inactive | Workspace expired |

### 12.8 PERMISSIONS & ACCESS (SC-52 -> SC-55)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-52 | Chưa đăng nhập -> /calendar | 1. Mở browser, chưa login 2. Truy cập https://[domain]/calendar | Redirect về /login. Sau login -> về /calendar | Chưa login |
| SC-53 | Token hết hạn khi tạo/sửa schedule | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Xóa token localStorage 3. Click Create | API 401 -> redirect /login + message **"Session expired"** | Token hết hạn |
| SC-54 | User không có quyền schedulePost (Free plan) | 1. Đăng nhập Free plan 2. Gọi API POST /content-schedules qua console | BE có thể chặn ở tầng subscription check. Nếu không -> schedule vẫn được tạo dù FE gate chặn UI. Ghi nhận thực tế | Free plan |
| SC-55 | Cross-workspace: schedule WS A không hiển thị trong WS B | 1. WS A tạo schedule 2. Switch sang WS B -> Calendar | Không thấy schedule của WS A. API GET /content-schedules filter theo X-Workspace-Id | 2 workspace |

### 12.9 EDGE CASES & UI (SC-56 -> SC-58)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| SC-56 | Legend bar hiển thị đúng màu | 1. Đăng nhập -> Calendar 2. Quan sát thanh legend dưới filter bar | 4 dot màu kèm text: Completed = emerald, Pending = amber, Processing = sky, Failed = red. Màu khớp với chip schedule trên calendar | -- |
| SC-57 | Tạo schedule cho content Published (đã đăng rồi) | 1. Đăng nhập -> Calendar -> "+ Schedule" 2. Chọn content status Published 3. Tạo schedule | BE cho phép schedule content Published (post lại). Schedule được tạo bình thường, status Pending. Worker sẽ publish lại khi đến giờ | Content Published |
| SC-58 | Double click Create Schedule | 1. Đăng nhập -> "+ Schedule" -> chọn content + integration + date 2. Click Create 2 lần nhanh | Lần 1: nút loading/disabled. Lần 2: không trigger. Chỉ tạo 1 bộ schedule. Không duplicate | -- |

**Module:** SCHEDULE | **Total:** 58 cases | **Page:** `/calendar` | **API:** `/content-schedules`, `/content-schedules/{id}`, `/content-schedules/bulk`, `/content-schedules/upcoming`

---

## SHEET 13/19: POSTS -- Published Posts Management (68 cases)

| **Feature** | Posts -- Quản lý danh sách bài đăng đã published, xem chi tiết, xóa |
|---|---|
| **Test requirement** | Posts page `/posts`: StatsCards (Published count, Total count, Quota used/total với progress bar), Filters (search theo title/brand/caption, brand dropdown, status dropdown: All/Published/Draft), Sort (contentTitle, status, publishedAt asc/desc), PostTable với cột checkbox, Post (icon + title + caption), Platform & Brand (platform icon + label, brand name, type badge TEXT/IMAGE/VIDEO), Status badge (dot màu + text), Date (formatted), Pagination (số trang, prev/next, hiển thị "Showing X-Y of Z posts"); BulkActionsBar (hiện khi chọn ít nhất 1 row, "N selected" + Delete Selected + Clear); PostDetailModal (title, status badge, platform badge, brand, type, publishedAt, Post ID, Content ID, External ID, Caption); DeleteConfirmModal (single: "Delete Post" + tên content, bulk: "Delete N Posts" + danh sách, cảnh báo "This action cannot be undone"); Empty state ("No posts found" + "Try adjusting your filters"); Loading state (spinner); Permissions; Edge cases |
| **Pages** | `/posts` |
| **API** | GET `/posts?page=&pageSize=&brandId=&status=` (PagedResult), GET `/posts/{id}`, DELETE `/posts/{id}` (soft delete: isDeleted=true) |
| **Model** | `Post` (id, content_id, integration_id, external_post_id, published_at, status: ContentStatusEnum, is_deleted, created_at) -> `PostListItemDto` (Id, ContentId, IntegrationId, ExternalPostId, PublishedAt, Status, ContentTitle, BrandId, BrandName, Platform, Type, Caption) |

### 13.1 POSTS LIST VIEW -- Danh sách bài đăng (PS-01 -> PS-12)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-01 | Truy cập trang Posts với bài đăng đã có | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Từ sidebar, click "Posts" (icon post_add, nằm giữa Calendar và Approvals) 5. Quan sát toàn bộ trang | Header: breadcrumbs "Dashboard > Posts", icon gradient từ-primary/10 đến-secondary/10, title "Posts", subtitle "N posts published" (N = totalCount). StatsCards (3 card): Published (icon task_alt, emerald), Total (icon inventory_2, blue), Quota (icon data_usage, purple + progress bar). Filter bar: icon filter_list, search input "Search posts...", Brand dropdown "All Brands", Status dropdown "All Statuses". Bảng danh sách posts với cột: checkbox, Post (icon + title + caption), Platform & Brand (platform icon + label, brand name, type badge), Status (badge + dot màu), Date. Pagination ở footer: "Showing X-Y of Z posts", nút prev/next, số trang | Có ít nhất 5 bài đăng với các status khác nhau |
| PS-02 | Trang Posts hiển thị đúng danh sách bài đăng | 1. Đăng nhập -> Posts 2. Quan sát từng row trong bảng | Mỗi row hiển thị: checkbox, content icon (article/image/movie), content title (truncate nếu dài), caption (line-clamp 1), platform icon + tên (Facebook/Instagram/TikTok), brand name, type badge (TEXT/IMAGE/VIDEO), status badge với dot màu (Published=emerald, Draft=gray, PendingApproval=amber, Approved=sky, Rejected=red), ngày đăng (dd MMM yyyy). Row hover -> bg-surface-container-low, cursor pointer | Có ít nhất 1 bài đăng |
| PS-03 | Empty State (chưa có bài đăng nào) | 1. Đăng nhập vào workspace mới chưa có post nào 2. Từ sidebar, click "Posts" | Hiển thị icon post_add màu outline/20 (text-4xl), text "No posts found" (text-body-sm, font-medium), subtext "Try adjusting your filters" (text-[11px], text-outline/60). StatsCards hiển thị: Published=0, Total=0, Quota hiển thị used/total. Filter bar vẫn hiển thị đầy đủ. Không crash, không trắng trang | Chưa có bài đăng nào |
| PS-04 | Loading skeleton khi load Posts | 1. Đăng nhập -> DevTools -> Network tab -> chọn Slow 3G 2. Vào Posts 3. Quan sát trạng thái loading | Hiển thị spinner xoay (w-8 h-8 border-2 border-primary/30 border-t-primary animate-spin) ở giữa vùng bảng. Sau khi load xong -> bảng hiển thị với data đầy đủ. StatsCards cũng loading/cập nhật | -- |
| PS-05 | Row hiển thị icon đúng theo content type | 1. Đăng nhập -> Posts 2. Quan sát icon của các row có type khác nhau | Type TEXT -> icon "article" (màu outline/30). Type IMAGE -> icon "image". Type VIDEO -> icon "movie". Icon nằm trong khung 40x40 rounded-lg gradient từ-primary/5 đến-secondary/5, border outline-variant/20 | Có bài đăng TEXT, IMAGE, VIDEO |
| PS-06 | Row hiển thị status badge với dot màu đúng | 1. Đăng nhập -> Posts 2. Quan sát status badge của từng row | Published: bg-emerald-50, text-emerald-600, border-emerald-500/20, dot xanh emerald-500. Draft: bg-gray-50, text-gray-600, border-gray-500/20, dot xám. PendingApproval: bg-amber-50, text-amber-600, border-amber-500/20, dot vàng amber-500. Approved: bg-sky-50, text-sky-600, border-sky-500/20, dot xanh sky-500. Rejected: bg-danger-red/10, text-danger-red, border-danger-red/20, dot đỏ | Có bài đăng với đủ các status |
| PS-07 | Row hiển thị platform icon và brand + type | 1. Đăng nhập -> Posts 2. Quan sát cột "Platform & Brand" của row Facebook | Platform icon Facebook hiển thị màu xanh #1877F2 kèm text "Facebook". Bên dưới hiển thị brand name + type badge TEXT/IMAGE/VIDEO (uppercase, tracking-wider). Row TikTok -> icon đen #111111, text "TikTok". Row Instagram -> gradient hồng/cam/tím | Có bài đăng đa nền tảng |
| PS-08 | Row hiển thị caption bị truncate | 1. Đăng nhập -> Posts 2. Tìm bài đăng có caption dài (hơn 1 dòng) 3. Quan sát | Caption hiển thị tối đa 1 dòng (line-clamp-1), phần vượt quá bị ẩn với dấu "...". Title hiển thị bên trên caption (truncate nếu dài, max-w-[280px]) | Có bài đăng caption dài |
| PS-09 | Click row mở PostDetailModal | 1. Đăng nhập test@example.com / Pass1234 2. Posts -> click vào 1 row (bất kỳ đâu trừ checkbox) 3. Quan sát | PostDetailModal mở. Modal hiển thị đầy đủ thông tin của bài đăng được click. Danh sách posts phía sau bị overlay đen + blur. Click checkbox -> không mở modal (stopPropagation) | Có bài đăng |
| PS-10 | Checkbox trong row hoạt động độc lập | 1. Đăng nhập -> Posts 2. Click checkbox của row thứ 1 -> checkbox được check 3. Click checkbox của row thứ 2 -> checkbox được check 4. Bỏ check row thứ 1 | Mỗi checkbox hoạt động độc lập. Checked -> selectedIds thêm id. Unchecked -> selectedIds bỏ id. Click vào checkbox không mở PostDetailModal. Row click vào vùng khác -> vẫn mở modal | Có ít nhất 2 bài đăng |
| PS-11 | Select All checkbox | 1. Đăng nhập -> Posts 2. Click checkbox "Select All" ở header bảng (cột checkbox đầu tiên) 3. Quan sát 4. Click lại Select All lần nữa | Lần 1: tất cả checkbox của các row được check. Select All checkbox ở trạng thái checked. BulkActionsBar hiện "N posts selected". Lần 2: tất cả checkbox bị bỏ. BulkActionsBar biến mất. Select All không ở trạng thái checked | Có ít nhất 2 bài đăng |
| PS-12 | Select All indeterminate state | 1. Đăng nhập -> Posts 2. Check 1 row bất kỳ 3. Quan sát checkbox Select All ở header | Checkbox Select All ở trạng thái indeterminate (gạch ngang, không fully checked). Không phải trạng thái checked hoàn toàn. Click Select All -> tất cả được check (không còn indeterminate) | Có ít nhất 2 bài đăng |

### 13.2 FILTERS & SEARCH -- Bộ lọc & tìm kiếm (PS-13 -> PS-24)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-13 | Search trong Posts (theo content title) | 1. Đăng nhập test@example.com / Pass1234 2. Posts -> nhập "Khuyến mãi" vào ô search 3. Quan sát | Chỉ hiện bài đăng có contentTitle chứa "Khuyến mãi" (case-insensitive, client-side filter). Các bài không khớp bị ẩn khỏi bảng. Pagination cập nhật. Filter kết hợp với brand + status dropdown (giao). Text hiển thị số lượng posts mới | Có bài đăng "Khuyến mãi Tết" |
| PS-14 | Search trong Posts (theo brand name) | 1. Đăng nhập -> Posts 2. Nhập tên brand "Nike" vào ô search 3. Quan sát | Chỉ hiện bài đăng có brandName chứa "Nike" (case-insensitive). Search hoạt động trên cả contentTitle, brandName, caption | Có bài đăng của brand "Nike" |
| PS-15 | Search trong Posts (theo caption) | 1. Đăng nhập -> Posts 2. Nhập 1 đoạn text có trong caption "giảm giá 50%" vào ô search 3. Quan sát | Chỉ hiện bài đăng có caption chứa "giảm giá 50%" (case-insensitive). Search client-side trên 3 trường: contentTitle, brandName, caption | Có bài đăng caption chứa "giảm giá 50%" |
| PS-16 | Search không phân biệt hoa thường | 1. Đăng nhập -> Posts 2. Nhập "KHUYẾN MÃI" (uppercase) vào search 3. Quan sát | Kết quả giống hệt PS-13 khi nhập "Khuyến mãi" (lowercase). Search dùng .toLowerCase() so sánh | Có bài đăng "Khuyến mãi Tết" |
| PS-17 | Search không có kết quả khớp | 1. Đăng nhập -> Posts 2. Nhập "xyzabc123khongtontai" vào ô search 3. Quan sát | Bảng hiển thị empty state: "No posts found" + "Try adjusting your filters". Không crash. Có thể xóa search để hiện lại danh sách | Có bài đăng nhưng không khớp |
| PS-18 | Xóa search về empty string | 1. Đăng nhập -> Posts -> nhập search "abc" 2. Xóa hết text trong ô search (để trống) 3. Quan sát | Danh sách posts hiển thị lại đầy đủ (không filter search). Pagination reset về page 1 do handleFilterChange -> setPage(1) | Có bài đăng |
| PS-19 | Brand filter | 1. Đăng nhập test@example.com / Pass1234 2. Posts -> chọn 1 brand từ dropdown "All Brands" -> chọn "Brand A" 3. Quan sát | Chỉ hiện bài đăng có brandId = "Brand A". Dropdown brand được populate từ danh sách brand có trong posts (brandId + brandName duy nhất). API GET /posts?brandId={id} được gọi (BE filter). Kết hợp với search + status filter -> giao | Có ít nhất 2 brand khác nhau |
| PS-20 | Brand filter hiển thị đúng danh sách brand | 1. Đăng nhập -> Posts 2. Quan sát dropdown Brand | Dropdown hiển thị "All Brands" (default) + danh sách các brand từ posts hiện có (không trùng lặp). Mỗi option hiển thị brand name, value là brandId. Không có brand nào bị thiếu. Danh sách dynamic (useMemo từ posts data) | Có bài đăng nhiều brand |
| PS-21 | Status filter: All Statuses | 1. Đăng nhập -> Posts 2. Status dropdown mặc định "All Statuses" 3. Quan sát | Hiển thị tất cả bài đăng bất kể status. API GET /posts không gửi param status (undefined) | Có bài đăng |
| PS-22 | Status filter: Published | 1. Đăng nhập -> Posts 2. Chọn status "Published" từ dropdown 3. Quan sát | Chỉ hiện bài đăng có status "Published". API GET /posts?status=Published. Badge status đều màu emerald. Count khớp số row hiển thị | Có bài đăng status Published |
| PS-23 | Status filter: Draft | 1. Đăng nhập -> Posts 2. Chọn status "Draft" từ dropdown 3. Quan sát | Chỉ hiện bài đăng có status "Draft". API GET /posts?status=Draft. Badge status đều màu gray. Count khớp số row. Chuyển về "All Statuses" -> hiện lại tất cả | Có bài đăng status Draft |
| PS-24 | Clear All Filters | 1. Đăng nhập -> Posts 2. Nhập search "abc", chọn brand "Brand A", chọn status "Published" 3. Click nút "Clear All" (icon clear_all, text "Clear All") 4. Quan sát | Tất cả filter reset: search = "", brand = "", status = "". Nút "Clear All" biến mất (hasActiveFilters = false). Danh sách posts hiển thị đầy đủ. Page reset về 1 | Đang có filter active |

### 13.3 SORTING -- Sắp xếp (PS-25 -> PS-32)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-25 | Sort theo Content Title (mặc định asc) | 1. Đăng nhập test@example.com / Pass1234 2. Posts -> click header cột "Post" (contentTitle) 3. Quan sát | Lần click đầu: sort asc (expand_less icon màu primary hiển thị). Danh sách sắp xếp theo contentTitle A->Z (localeCompare). Các header khác hiển thị icon unfold_more (xám) | Có ít nhất 3 bài đăng |
| PS-26 | Toggle sort Content Title asc -> desc | 1. Đăng nhập -> Posts 2. Click header "Post" lần 1 (asc) 3. Click header "Post" lần 2 | Lần 2: sort desc (expand_more icon màu primary). Danh sách sắp xếp theo contentTitle Z->A. Icon đổi từ expand_less sang expand_more | Có ít nhất 3 bài đăng |
| PS-27 | Sort theo Status | 1. Đăng nhập -> Posts 2. Click header "Status" 3. Click lại lần 2 | Lần 1: asc (A->Z: Approved->Draft->PendingApproval->Published->Rejected). Lần 2: desc (Z->A). Icon sort hiển thị đúng trên header Status, các header khác unfold_more | Có bài đăng nhiều status |
| PS-28 | Sort theo Date (mặc định desc) | 1. Đăng nhập -> Posts 2. Quan sát mặc định 3. Click header "Date" để toggle | Mặc định: sortKey="publishedAt", sortDir="desc" -> bài mới nhất lên đầu. Click header Date -> toggle asc (bài cũ nhất lên đầu). Icon sort đúng (desc = expand_more, asc = expand_less) | Có bài đăng nhiều ngày khác nhau |
| PS-29 | Chuyển đổi sort key giữa các cột | 1. Đăng nhập -> Posts 2. Click header "Post" -> asc 3. Click header "Status" -> asc 4. Click header "Date" -> asc | Mỗi lần chuyển sort key -> sortDir về asc (mặc định). Icon sort chỉ hiển thị trên cột đang active (màu primary). Các cột khác hiển thị unfold_more (màu outline/20) | Có ít nhất 3 bài đăng |
| PS-30 | Sort không ảnh hưởng đến pagination | 1. Đăng nhập -> Posts có 25+ bài đăng 2. Sort theo title asc 3. Chuyển trang sang page 2 4. Quan sát | Page 2 cũng được sort theo title asc. Sort state được giữ khi chuyển trang. Thứ tự sort áp dụng cho toàn bộ data (client-side sortPosts) | Có 25+ bài đăng |
| PS-31 | Sort trên cột Platform & Brand không có | 1. Đăng nhập -> Posts 2. Click header cột "Platform & Brand" | Cột "Platform & Brand" không có cursor-pointer, không có icon sort, không phản hồi khi click. Đây là cột không sortable (không có onSort handler) | -- |
| PS-32 | Sort kết hợp với filter/search | 1. Đăng nhập -> Posts 2. Filter brand "Brand A" 3. Sort theo title asc | Danh sách được lọc theo Brand A TRƯỚC, sau đó sắp xếp theo title asc. Filter thực hiện trước (BE hoặc client-side), sort thực hiện sau (client-side sortPosts) | Có bài đăng nhiều brand |

### 13.4 POST DETAIL MODAL -- Modal chi tiết bài đăng (PS-33 -> PS-40)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-33 | Mở Post Detail Modal từ row | 1. Đăng nhập test@example.com / Pass1234 2. Posts -> click vào 1 row (không click checkbox) 3. Quan sát modal | Modal hiển thị overlay đen + blur (bg-black/50 backdrop-blur-sm), nội dung ở giữa màn hình (max-w-lg). Header modal: icon article màu primary + title "Post Details" + subtitle "Published post record". Nút Close (X icon) ở góc phải trên. Nội dung: contentTitle (text-lg, font-bold), hàng status badge + platform badge, detail section (bg-surface-container-low, rounded-xl): Brand, Type (uppercase), Published At (dd MMM yyyy), Post ID (font-mono), Content ID (font-mono), External ID (nếu có, font-mono, max-w-[200px] truncate). Caption section (nếu có): tiêu đề "Caption" + text đầy đủ (không truncate). Footer: nút "Close" (border, hover đổi màu) | Có bài đăng |
| PS-34 | Modal hiển thị External Post ID khi có | 1. Đăng nhập -> Posts 2. Click vào bài đăng có externalPostId (đã đăng lên social thật) 3. Quan sát modal | Phần "External ID" hiển thị externalPostId dạng font-mono, max-w-[200px], truncate nếu dài. Đây là ID của bài đăng trên nền tảng social (Facebook/TikTok post ID) | Có bài đăng với externalPostId |
| PS-35 | Modal không hiển thị External Post ID khi null | 1. Đăng nhập -> Posts 2. Click vào bài đăng không có externalPostId (chưa đăng lên social thật) 3. Quan sát modal | Không hiển thị dòng "External ID". Chỉ hiển thị Post ID và Content ID. Giao diện không bị lỗi | Có bài đăng không có externalPostId |
| PS-36 | Modal hiển thị caption đầy đủ | 1. Đăng nhập -> Posts 2. Click vào bài đăng có caption dài 3. Quan sát phần Caption trong modal | Caption hiển thị toàn bộ nội dung (không bị truncate như trong row). Text nằm trong khung bg-surface-container-low, border outline-variant/10, rounded-xl, padding p-4. Khác với row chỉ hiển thị line-clamp-1 | Có bài đăng caption dài |
| PS-37 | Modal không hiển thị Caption section khi caption null | 1. Đăng nhập -> Posts 2. Click vào bài đăng không có caption 3. Quan sát modal | Không có section "Caption". Modal hiển thị gọn gàng, không có khoảng trống thừa. Các detail khác hiển thị bình thường | Có bài đăng không có caption |
| PS-38 | Đóng Post Detail Modal bằng nút Close (X) | 1. Đăng nhập -> Posts -> mở modal 2. Click nút X (icon close) ở góc trên phải header modal 3. Quan sát | Modal đóng. Trang Posts trở lại bình thường, không overlay. selectedIds không thay đổi. Có thể click mở lại modal hoặc click row khác | Modal đang mở |
| PS-39 | Đóng Post Detail Modal bằng nút Close ở footer | 1. Đăng nhập -> Posts -> mở modal 2. Click nút "Close" ở footer modal (border + text "Close") 3. Quan sát | Modal đóng. Hành vi giống hệt PS-38 | Modal đang mở |
| PS-40 | Đóng Post Detail Modal bằng click ra ngoài | 1. Đăng nhập -> Posts -> mở modal 2. Click vào vùng overlay bên ngoài modal (vùng đen mờ) 3. Quan sát | Modal đóng. onClick trên overlay + container ngoài gọi onClose. Click vào bên trong modal không đóng (stopPropagation). Không crash | Modal đang mở |

### 13.5 DELETE POST -- Xóa bài đăng (PS-41 -> PS-48)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-41 | Xóa 1 bài đăng (single delete qua BulkActionsBar) | 1. Đăng nhập test@example.com / Pass1234 2. Posts -> check chọn 1 bài đăng 3. BulkActionsBar hiện: "1 post selected" + "Delete Selected" + "Clear" 4. Click "Delete Selected" 5. DeleteConfirmModal mở -> click "Delete" trong modal | Modal hiển thị: icon thùng rác đỏ + title "Delete Post" + subtitle "This action cannot be undone" + text "Are you sure you want to delete {contentTitle}?" + cảnh báo "Deleted posts cannot be recovered." + nút Cancel + nút Delete (đỏ). Sau khi click Delete: nút chuyển spinner, API DELETE /posts/{id} được gọi. Toast hiển thị: **"1 post(s) deleted"**. Row biến mất khỏi bảng. Total count giảm 1. selectedIds rỗng. StatsCards cập nhật (Published giảm 1 nếu post Published, Total giảm 1) | Có bài đăng |
| PS-42 | Xóa 1 bài đăng từ row action (nếu có) | 1. Đăng nhập -> Posts 2. Hover row -> quan sát nếu có nút Delete riêng 3. Ghi nhận thực tế | **[GHI NHẬN]** FE hiện tại không có nút Delete riêng trên mỗi row. Người dùng phải check chọn row -> BulkActionsBar -> Delete Selected. Đây là UX cần ghi nhận khi test | Có bài đăng |
| PS-43 | Hủy xóa bài đăng (Cancel trong confirm modal) | 1. Đăng nhập -> Posts -> check 1 row -> Delete Selected 2. DeleteConfirmModal mở -> click "Cancel" hoặc click ngoài modal 3. Quan sát | Modal đóng. Row vẫn trong bảng, vẫn được check. Không có API call. Không toast. Có thể chọn lại và Delete | Có bài đăng |
| PS-44 | Xóa bài đăng thất bại (API lỗi) | 1. Đăng nhập -> Posts -> check 1 row -> Delete Selected -> Confirm 2. DevTools -> chặn request DELETE hoặc giả lập lỗi server 3. Quan sát | API trả lỗi (4xx/5xx). Toast hiển thị: **"Failed to delete post(s)"** (error). Row vẫn trong bảng. Nút Delete hết spinner, có thể thử lại. selectedIds không bị clear | Giả lập lỗi API |
| PS-45 | Xóa bài đăng đã bị xóa (idempotent) | 1. Đăng nhập -> Posts -> xóa 1 bài đăng thành công 2. Giữ lại id, gọi DELETE /posts/{id} lần 2 qua console | BE check post.Content.WorkspaceId (hoặc post.IsDeleted) -> trả lỗi **"Post not found."** (404). Message: "Post not found." Toast lỗi nếu gọi qua FE | Vừa xóa xong |
| PS-46 | Xóa bài đăng -> content vẫn tồn tại | 1. Đăng nhập -> Posts -> xóa 1 bài đăng 2. Vào Content page 3. Tìm content có id = contentId của post đã xóa | Content vẫn tồn tại trong Content Library (post chỉ là record publish, xóa post không xóa content). Soft delete post: isDeleted=true. Content không bị ảnh hưởng | Có bài đăng |
| PS-47 | Cancel Delete bằng click ngoài modal | 1. Đăng nhập -> Posts -> check row -> Delete Selected 2. Click ra ngoài vùng modal (vùng overlay đen) 3. Quan sát | Modal đóng (onClick overlay gọi onCancel). Hành vi giống hệt PS-43. selectedIds không đổi. Không API call | Modal đang mở |
| PS-48 | Delete bài đăng không ảnh hưởng đến PerformanceReports | 1. Đăng nhập -> Posts -> xóa 1 bài đăng có performance reports 2. Kiểm tra BE (qua API hoặc DB) | Post bị soft delete (isDeleted=true). PerformanceReports liên kết vẫn giữ nguyên (không cascade delete). Analytics data vẫn có thể truy xuất nếu cần | Bài đăng có performance data |

### 13.6 BULK ACTIONS -- Thao tác hàng loạt (PS-49 -> PS-56)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-49 | BulkActionsBar hiển thị khi chọn 1+ row | 1. Đăng nhập test@example.com / Pass1234 2. Posts -> check chọn 1 row 3. Quan sát BulkActionsBar | Bar hiển thị với animation slide-in-from-top-2: bg-primary/5, border-primary/20, rounded-2xl. Icon checklist màu primary. Text "1 post selected" + subtitle "Choose an action to perform". Nút "Delete Selected" (đỏ, icon delete) + nút "Clear" (xám, border) | Có bài đăng |
| PS-50 | BulkActionsBar hiển thị khi chọn nhiều row | 1. Đăng nhập -> Posts -> check chọn 3 row 2. Quan sát | Text "3 posts selected" (plural). Các nút vẫn hoạt động bình thường. Bar animation mượt, không flash | Có ít nhất 3 bài đăng |
| PS-51 | Bulk Delete nhiều bài đăng | 1. Đăng nhập -> Posts -> check 3 row 2. BulkActionsBar -> click "Delete Selected" 3. DeleteConfirmModal mở với header "Delete 3 Posts" 4. Quan sát modal | Modal hiển thị: title "Delete 3 Posts" + subtitle "This action cannot be undone". Khung danh sách các posts (bg-surface-container-low, max-h-32, overflow-y-auto): mỗi item dot đỏ + contentTitle hoặc id. Cảnh báo "Deleted posts cannot be recovered." Footer: Cancel + Delete (đỏ + spinner khi loading) | Có ít nhất 3 bài đăng |
| PS-52 | Bulk Delete -> confirm | 1. Tiếp tục từ PS-51 2. Click nút "Delete" trong modal 3. Quan sát | Nút chuyển spinner (w-4 h-4 border-2 border-white/30 border-t-white animate-spin). Gọi DELETE /posts/{id} cho từng post tuần tự (for...of). Toast hiển thị: **"3 post(s) deleted"**. Cả 3 row biến mất khỏi bảng. selectedIds rỗng. BulkActionsBar biến mất. StatsCards cập nhật. Nếu có lỗi 1 trong 3 -> toast: **"Failed to delete post(s)"**, các post đã xóa vẫn mất khỏi UI, các post chưa xóa vẫn còn | Có 3 bài đăng |
| PS-53 | Clear selection (nút Clear trong BulkActionsBar) | 1. Đăng nhập -> Posts -> check 3 row 2. Click nút "Clear" trong BulkActionsBar 3. Quan sát | Tất cả checkbox bị bỏ check. selectedIds = []. BulkActionsBar biến mất. Không API call. Các row không thay đổi | Đã chọn 3 row |
| PS-54 | BulkActionsBar biến mất khi không còn selection | 1. Đăng nhập -> Posts -> check 1 row (BulkActionsBar hiện) 2. Bỏ check row đó (uncheck) 3. Quan sát | BulkActionsBar biến mất (return null khi selectedCount === 0). Trang Posts trở lại giao diện bình thường | Đã chọn 1 row |
| PS-55 | BulkActionsBar loading state khi đang xóa | 1. Đăng nhập -> Posts -> check 2 row -> Delete Selected -> Confirm 2. Quan sát nút "Delete Selected" trong quá trình xóa | Nút "Delete Selected" bị disabled (opacity-50) và hiển thị spinner nhỏ (w-3.5 h-3.5 border-2) thay cho icon delete. Nút "Clear" vẫn hoạt động nhưng có thể không nên dùng lúc này | Đang xóa |
| PS-56 | Bulk Delete hủy giữa chừng | 1. Đăng nhập -> Posts -> check 3 row -> Delete Selected 2. DeleteConfirmModal mở -> click "Cancel" | Modal đóng. Tất cả 3 row vẫn được check. selectedIds giữ nguyên. Không API call. BulkActionsBar vẫn hiện "3 posts selected" | Modal đang mở |

### 13.7 PAGINATION -- Phân trang (PS-57 -> PS-63)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-57 | Phân trang hiển thị đúng thông tin | 1. Đăng nhập test@example.com / Pass1234 2. Posts -> quan sát footer pagination (có 25+ bài đăng) | Footer hiển thị: text "Showing 1-10 of 25 posts" (page=1, pageSize=10). Nút prev (chevron_left, disabled ở page 1). Các số trang (1, 2, 3). Nút next (chevron_right, enabled). Page hiện tại được highlight (bg-primary text-on-primary) | Có 25+ bài đăng |
| PS-58 | Chuyển sang trang 2 | 1. Đăng nhập -> Posts -> click số 2 trong pagination 2. Quan sát | API gọi GET /posts?page=2&pageSize=10. Bảng hiển thị 10 bài tiếp theo. Text cập nhật "Showing 11-20 of 25 posts". Page 2 được highlight. Nút prev enabled. selectedIds reset (FE không reset nhưng UI có thể thay đổi) | Có 25+ bài đăng |
| PS-59 | Nút Next và Previous | 1. Đăng nhập -> Posts (trang 1) 2. Click nút Next (>) -> sang trang 2 3. Click nút Prev (<) -> về trang 1 | Nút prev disabled ở trang 1 (opacity-30, cursor-not-allowed). Nút next disabled ở trang cuối. Các nút hoạt động mượt, không bị double click | Có 25+ bài đăng |
| PS-60 | Trang cuối cùng (page = totalPages) | 1. Đăng nhập -> Posts 2. Click số trang cuối cùng 3. Quan sát | Text "Showing 21-25 of 25 posts". Nút next bị disabled (opacity-30, cursor-not-allowed). Nút prev enabled. Không có số trang > totalPages | Có 25 bài đăng (totalPages=3) |
| PS-61 | Pagination với ít hơn 1 trang | 1. Đăng nhập vào workspace có 5 bài đăng (pageSize=10) 2. Posts -> quan sát pagination | Chỉ hiển thị 1 trang (page=1, totalPages=1). Nút prev và next đều disabled. Text "Showing 1-5 of 5 posts". Không có ellipsis (...) | Có 5 bài đăng |
| PS-62 | Pagination ellipsis khi nhiều trang | 1. Đăng nhập -> Posts có 100+ bài đăng 2. Duyệt qua các trang | Pagination hiển thị: 1, ..., các trang giữa, ..., totalPages. Khi đang ở trang 7 -> hiển thị 1, ..., 5, 6, 7, 8, 9, ..., totalPages (max 5 số liên tiếp). Ellipsis hiển thị dạng "..." (không click được) | Có 100+ bài đăng |
| PS-63 | Pagination reset về page 1 khi filter | 1. Đăng nhập -> Posts -> chuyển sang trang 3 2. Chọn status filter "Draft" 3. Quan sát | Page tự động reset về 1 (handleFilterChange -> setPage(1)). API GET /posts?page=1&pageSize=10&status=Draft. Pagination hiển thị đúng với data đã filter | Có bài đăng nhiều trang |

### 13.8 STATS & QUOTA -- Thống kê & hạn ngạch (PS-64 -> PS-69)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-64 | StatsCards hiển thị đúng số liệu | 1. Đăng nhập test@example.com / Pass1234 2. Posts 3. Đếm thủ công số bài Published và Total 4. So với StatsCards | Card Published: icon task_alt (emerald-50 background, emerald-600 icon), số publishedCount = số bài status Published trong danh sách hiện tại. Card Total: icon inventory_2 (blue-50, blue-600), số totalCount từ API (tất cả post, không bị filter). Card Quota: icon data_usage (purple-50, purple-600), hiển thị used/total | Có bài đăng |
| PS-65 | Published count khớp với số bài Published trong bảng | 1. Đăng nhập -> Posts 2. Đếm số row có status "Published" 3. So với số trên card Published | Số trên card Published = số bài status "Published" trong danh sách posts hiện tại (posts.filter(p => p.status === "Published").length). Lưu ý: có thể khác totalCount nếu đang filter | Có bài đăng |
| PS-66 | Quota card hiển thị progress bar | 1. Đăng nhập -> Posts 2. Quan sát card Quota | Hiển thị quotaUsed/quotaTotal (lấy từ API fetchPostQuota). Progress bar (h-2, rounded-full): màu primary nếu <=50%, màu warning-amber nếu 50-80%, màu danger-red nếu >80%. Width = min(quotaPercent, 100)%. Nếu quotaTotal = 0 -> không hiển thị progress bar | Có quota data |
| PS-67 | Quota card không có progress bar khi total = 0 | 1. Đăng nhập vào workspace có postQuota.total = 0 hoặc null 2. Posts -> quan sát card Quota | Card Quota hiển thị "— / — used" (null values). Progress bar không hiển thị (điều kiện quotaTotal && quotaTotal > 0). Card không crash | quotaTotal = 0 |
| PS-68 | StatsCards cập nhật sau khi xóa bài đăng | 1. Đăng nhập -> Posts -> ghi nhận số Published và Total 2. Xóa 1 bài đăng status Published 3. Quan sát StatsCards | Card Published giảm 1 (posts state đã filter out post bị xóa). Card Total: totalCount được refetch? **[GHI NHẬN]** FE hiện chỉ gọi fetchPostQuota từ workspaceService (1 lần), totalCount lấy từ API lần đầu. Sau khi xóa, posts state cập nhật nhưng totalCount không refetch tự động -> số có thể lệch. Cần ghi nhận thực tế khi test | Vừa xóa bài đăng |
| PS-69 | StatsCards hiển thị khi chưa có bài đăng | 1. Đăng nhập vào workspace chưa có post 2. Posts | Published=0, Total=0, Quota hiển thị used/total. Cả 3 card vẫn hiển thị bình thường, không crash. Progress bar quota vẫn hiển thị nếu quotaTotal > 0 | Chưa có post |

### 13.9 PERMISSIONS & ACCESS CONTROL (PS-70 -> PS-77)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-70 | Chưa đăng nhập truy cập /posts | 1. Mở browser, chưa login 2. Truy cập https://[domain]/posts | Redirect về /login. Sau khi login -> redirect về /posts (hoặc /dashboard). Không hiển thị nội dung posts | Chưa login |
| PS-71 | Token hết hạn khi load Posts | 1. Đăng nhập -> Posts 2. Xóa token khỏi localStorage 3. F5 reload trang | API GET /posts trả 401. FE client xử lý -> redirect /login + message **"Session expired"**. Hoặc token tự refresh. Không hiển thị data | Token hết hạn |
| PS-72 | Token hết hạn khi xóa bài đăng | 1. Đăng nhập -> Posts -> check row 2. Xóa token khỏi localStorage 3. Delete Selected -> Confirm | API DELETE /posts/{id} trả 401. FE xử lý: có thể hiển thị toast lỗi hoặc redirect /login. Nếu lỗi được catch trong try/catch -> toast **"Failed to delete post(s)"**. Row vẫn trong bảng | Token hết hạn |
| PS-73 | Cross-workspace: posts của WS A không hiển thị trong WS B | 1. WS A có 5 bài đăng 2. Switch sang WS B (không có post nào) 3. Vào Posts | Không thấy bài đăng của WS A. API GET /posts filter theo X-Workspace-Id (BE: GetPagedByWorkspaceAsync). Empty state hiển thị "No posts found" | 2 workspace |
| PS-74 | Cross-workspace: xóa post của WS khác qua API trực tiếp | 1. Gọi DELETE /posts/{postId} với token của WS B (postId thuộc WS A) | BE check post.Content.WorkspaceId != workspaceId từ token -> trả lỗi 404 **"Post not found."**. Không xóa được post của workspace khác | Post WS A, token WS B |
| PS-75 | Viewer / Member có thể xem Posts | 1. Đăng nhập với role Viewer hoặc Member 2. Vào Posts | Có thể xem danh sách posts bình thường. GET /posts không check role cụ thể (chỉ [Authorize]). StatsCards, filters, sort, pagination hoạt động bình thường | Viewer/Member |
| PS-76 | Viewer / Member có thể xóa Posts | 1. Đăng nhập Viewer/Member 2. Posts -> check row -> Delete Selected -> Confirm | **[GHI NHẬN]** API DELETE /posts/{id} chỉ check [Authorize] và workspace ownership, không check role cụ thể. Nếu BE không chặn -> Viewer/Member có thể xóa post. Cần ghi nhận thực tế và báo bug nếu không mong muốn | Viewer/Member |
| PS-77 | User bị xóa khỏi workspace -> không thấy posts | 1. User bị kick khỏi workspace 2. F5 Posts hoặc chuyển trang | API GET /posts (X-Workspace-Id) trả lỗi 403/404 vì user không còn trong workspace. FE redirect hoặc hiển thị lỗi **"Workspace not found or access denied"** | Vừa bị kick |

### 13.10 EDGE CASES & UI (PS-78 -> PS-85)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PS-78 | Mất mạng khi load Posts page | 1. Đăng nhập -> DevTools Offline 2. Vào Posts (hoặc F5) | API GET /posts fail. Toast hiển thị: **"Failed to load posts"** (error). Bảng hiển thị empty state "No posts found". StatsCards hiển thị 0. Không crash, có thể thử lại khi có mạng | Mất mạng |
| PS-79 | Mất mạng khi Delete post | 1. Đăng nhập -> Posts -> check row -> Delete Selected -> Confirm 2. DevTools Offline trước khi click | API DELETE fail. Toast: **"Failed to delete post(s)"** (error). Row vẫn trong bảng, vẫn checked. Có thể thử lại | Mất mạng |
| PS-80 | Double click Delete trong confirm modal | 1. Đăng nhập -> Posts -> check row -> Delete Selected 2. Click nút "Delete" 2 lần liên tiếp nhanh trong confirm modal | Lần 1: nút chuyển loading/disabled (isLoading=true). Lần 2: nút disabled -> không trigger. Chỉ gọi DELETE 1 lần. Toast hiển thị **"1 post(s) deleted"** 1 lần | Có bài đăng |
| PS-81 | Bài đăng có contentTitle null | 1. Đăng nhập -> Posts 2. Tìm bài đăng có contentTitle = null (hoặc empty) 3. Quan sát row và modal | Row hiển thị "Untitled" thay vì contentTitle. Modal hiển thị "Untitled" (text-lg, font-bold). Không crash, không hiển thị "null" | Có bài đăng không có title |
| PS-82 | Bài đăng có platform null hoặc không xác định | 1. Đăng nhập -> Posts 2. Tìm bài đăng có platform = null hoặc platform không có trong PLATFORM_CONFIG 3. Quan sát | Platform hiển thị dấu "—" (text-label-xs text-outline). Không hiển thị icon. Không crash. Tương tự trong PostDetailModal | Có bài đăng platform null |
| PS-83 | Refresh trang Posts giữ nguyên page? | 1. Đăng nhập -> Posts -> chọn filter brand + status + search, sort theo title, ở page 2 2. F5 reload trang 3. Quan sát | **[GHI NHẬN]** Filter, search, sort, page state đều ở trong useState -> sau F5 tất cả reset về default (page=1, no filter, sort publishedAt desc). Không persist qua URL params. Người dùng phải filter lại từ đầu | Đang ở trang 2 với filter |
| PS-84 | Content type hiển thị đúng uppercase trong modal | 1. Đăng nhập -> Posts -> click row có type "VIDEO" 2. Quan sát modal | Type hiển thị "VIDEO" (uppercase, font-semibold, tracking-wide) trong detail section. Không phải "Video" hay "video" | Có bài đăng type VIDEO |
| PS-85 | Sidebar navigation highlight đúng mục Posts | 1. Đăng nhập -> Posts 2. Quan sát sidebar | Mục "Posts" trong sidebar được highlight/active. Icon post_add có màu primary (hoặc khác biệt với các mục khác). Các mục khác không bị highlight | Đang ở trang Posts |

**Module:** POSTS | **Total:** 68 cases | **Page:** `/posts` | **API:** `/posts`, `/posts/{id}`, DELETE `/posts/{id}`

---

## SHEET 14/19: CAMPAIGN -- Ad Campaign Management (75 cases)

| **Feature** | Campaign -- Quản lý chiến dịch quảng cáo Facebook/Instagram: tạo, sửa, xóa, triển khai, theo dõi hiệu suất |
|---|---|
| **Test requirement** | Campaigns page `/campaigns`: CampaignStatsCards (Total, Active, Total Spend, Impressions, Clicks, Conversions + budget utilization bar), CampaignFilterBar (search, status filter: All/Active/Paused/Completed/Draft, objective filter: 6 loại, sort: Newest/Oldest/Budget High-Low/Budget Low-High/Spend High-Low/Name A-Z), CampaignCard grid (checkbox, name, brand, product/content badges, platform badge, objective icon+label, budget, date range, days remaining, 4 metrics: impressions/CTR/spend/conversions, budget progress bar, action buttons theo status), BulkActionsBar (Duplicate Selected, Delete Selected, Clear), CampaignEmptyState (animated, context-aware: filters active vs fresh), CampaignStatsCards; CreateCampaignModal: name, platform (Facebook/Instagram), Facebook account selector, ad account selector (loaded API từ account), brand, product (optional), content (optional), landing URL (optional), targeting presets (Vietnam/US/Worldwide/Custom JSON), objective grid (6 options: AWARENESS/TRAFFIC/ENGAGEMENT/LEADS/SALES/APP_PROMOTION), total budget VND, start/end date; EditCampaignModal: giống Create nhưng pre-populated, deployed campaign chỉ cho sửa name (cảnh báo amber); CampaignDetailModal: full-screen, header + status + objective + days remaining, 4 performance cards, budget utilization bar, details table, Ad Sets section (daily budget, metrics, ads within: Facebook Ad ID, CTA, link URL); DeleteConfirmModal (single + bulk); StartConfirmModal (cảnh báo real charges); Status actions: Deploy (Draft), Start (Paused), Pause (Active), Restart (Completed); Duplicate campaign |
| **Pages** | `/campaigns` |
| **API** | GET `/campaigns?page=&pageSize=&searchTerm=&sortBy=&sortDirection=`, GET `/campaigns/{id}`, POST `/campaigns`, PUT `/campaigns/{id}`, DELETE `/campaigns/{id}`, POST `/campaigns/{id}/deploy`, POST `/campaigns/{id}/sync-insights`, POST `/campaigns/{id}/cleanup`, POST `/campaigns/{id}/restore`, POST `/campaigns/{id}/duplicate` |
| **Model** | `AdCampaign` (id, profileId, workspaceId, brandId, adAccountId, productId, contentId, targeting jsonb, facebookCampaignId, platform, name, objective, budget, startDate, endDate, landingUrl, isActive, isDeleted, deploymentStatus: None/InProgress/Completed/Failed, deploymentStep, impressions, clicks, spend, conversions) -> `AdCampaignResponseDto` + AdSets (AdSetSummaryDto + Ads: AdSummaryDto) |
| **Status derive** | FE derive status từ isActive + endDate + startDate + facebookCampaignId: DRAFT (chưa deploy), PAUSED (đã deploy, isActive=false), ACTIVE (đang chạy), COMPLETED (endDate < now) |

### 14.1 CAMPAIGN LIST VIEW -- Danh sách chiến dịch (CM-01 -> CM-10)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-01 | Truy cập trang Campaigns với chiến dịch đã có | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Từ sidebar, click "Campaigns" (icon campaign, mục Marketing) 5. Quan sát toàn bộ trang | Header: breadcrumbs "Dashboard > Campaigns", icon gradient animate-float, title "Campaigns", subtitle **"Manage your advertising campaigns"**. StatsCards (6 card): Total Campaigns (icon campaign, blue), Active (icon trending_up, emerald), Total Spend (icon payments, amber), Impressions (icon visibility, purple), Clicks (icon ads_click, sky), Conversions (icon check_circle, rose). Budget utilization bar ở dưới: hiển thị message **"X% of total budget spent"** kèm số paused/completed. CampaignFilterBar: search placeholder **"Search campaigns..."**, status dropdown, objective dropdown, sort dropdown, result count badge hiển thị **"N campaigns"**. Grid gồm các CampaignCard. Nút **"+ New Campaign"** góc trên phải | Có ít nhất 3 chiến dịch với các status khác nhau |
| CM-02 | CampaignCard hiển thị đầy đủ thông tin | 1. Đăng nhập -> Campaigns 2. Quan sát 1 card chiến dịch ACTIVE | Card hiển thị: checkbox chọn, tên campaign (font-semibold), brand name + product/content badges, platform badge (Facebook xanh/Instagram hồng), objective icon + label (vd: TRAFFIC -> icon trending_up), budget (format VND), date range (dd/MM/yyyy), days remaining (nếu còn hạn), 4 metrics: Impressions (format K/M), CTR (%), Spend (VND), Conversions (số). Progress bar budget (đã spend/tổng, màu xanh/vàng/đỏ theo tỉ lệ). Action buttons ở footer: Pause (nếu ACTIVE), View Details, Edit, Delete. Icon platform và objective đúng màu sắc | Có campaign ACTIVE |
| CM-03 | CampaignCard hiển thị status badge đúng màu | 1. Đăng nhập -> Campaigns 2. Quan sát badge của các campaign khác status | ACTIVE: badge xanh emerald, text "Active". PAUSED: badge cam/amber, text "Paused". COMPLETED: badge xám/sky, text "Completed". DRAFT: badge xám nhạt, text "Draft". Mỗi badge có dot màu tương ứng. Vị trí badge nằm góc trên phải card | Có campaign đủ 4 status |
| CM-04 | CampaignCard action buttons theo status | 1. Đăng nhập -> Campaigns 2. Quan sát footer actions của từng status | DRAFT: nút "Deploy" (primary/xanh), View Details, Edit, Delete. PAUSED (đã deploy): nút "Start" (play icon, emerald), View Details, Edit, Delete. ACTIVE: nút "Pause" (pause icon, amber), View Details, Edit, Delete. COMPLETED: nút "Restart" (replay icon), View Details, Edit (disabled/ẩn), Delete. Các nút action chính (Deploy/Start/Pause/Restart) nổi bật hơn, có loading spinner khi đang xử lý | Có campaign đủ 4 status |
| CM-05 | CampaignCard metrics hiển thị 0 cho campaign chưa có data | 1. Đăng nhập -> Campaigns 2. Quan sát card DRAFT hoặc mới tạo chưa chạy | Impressions = 0, CTR = 0%, Spend = 0 ₫, Conversions = 0. Budget progress bar = 0%. Hiển thị message **"0%"** hoặc **"No spend"** trên progress bar. Không crash, không hiển thị NaN hay undefined | Campaign DRAFT |
| CM-06 | CampaignCard days remaining cho campaign sắp hết hạn | 1. Đăng nhập -> Campaigns 2. Quan sát card có endDate < 7 ngày | Hiển thị badge **"X days left"** (cảnh báo amber nếu < 7 ngày, đỏ nếu < 3 ngày). Không hiển thị số âm (endDate quá khứ -> COMPLETED). Định dạng ngày đúng | Campaign sắp hết hạn |
| CM-07 | CampaignCard days remaining cho campaign không có endDate | 1. Đăng nhập -> Campaigns 2. Quan sát card không có endDate (null) | Không hiển thị days remaining. Hoặc hiển thị message **"Ongoing"** / **"No end date"**. Không crash, không hiển thị NaN | Campaign không set endDate |
| CM-08 | CampaignCard budget progress bar | 1. Đăng nhập -> Campaigns 2. Quan sát progress bar trên các card | Bar hiển thị tỉ lệ spend/budget kèm text **"X% spent"**. Màu xanh (primary) nếu < 50%, vàng (amber) nếu 50-80%, đỏ (danger) nếu > 80%. Width = min(spend/budget * 100, 100)%. Budget = 0 -> bar width 0% hoặc ẩn kèm text **"No budget"** | Campaign đã spend > 0 |
| CM-09 | Loading skeleton khi load Campaigns | 1. Đăng nhập -> DevTools -> Slow 3G 2. Vào Campaigns 3. Quan sát trạng thái loading | Hiển thị skeleton cards (animate-pulse): khung xám với placeholder cho icon, title, metrics, progress bar. Sau khi load -> cards thật hiển thị. StatsCards cũng có skeleton hoặc hiển thị 0 trong lúc loading | -- |
| CM-10 | Empty State: chưa có campaign nào | 1. Đăng nhập vào workspace mới chưa tạo campaign 2. Vào Campaigns | Hiển thị CampaignEmptyState: animated icon campaign + floating accent badges (trending_up, payments, target), text **"No campaigns yet"**, subtext mô tả tính năng, feature highlights **"Set objectives & targeting"**, **"Manage budgets & spend"**, **"Track performance metrics"**, nút **"Create Your First Campaign"** nổi bật. StatsCards hiển thị 0. FilterBar vẫn hiển thị | Chưa có campaign |

### 14.2 FILTERS & SEARCH -- Bộ lọc & tìm kiếm (CM-11 -> CM-20)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-11 | Search theo tên campaign | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> nhập "Tết" vào ô search 3. Quan sát | Chỉ hiện campaign có name chứa "Tết" (case-insensitive, API searchTerm). Search được gửi qua API GET /campaigns?searchTerm=Tết. Result badge hiển thị **"N results"** (N là số campaign khớp). Pagination cập nhật | Có campaign "Khuyến mãi Tết" |
| CM-12 | Search clear (xóa text) | 1. Đăng nhập -> Campaigns -> nhập search "abc" 2. Click nút X trong ô search hoặc xóa text thủ công 3. Quan sát | Danh sách hiển thị lại tất cả campaign. API gọi không có searchTerm. Result badge hiển thị tổng số **"N campaigns"**. Toast không bắt buộc | Đang có search |
| CM-13 | Status filter: Active | 1. Đăng nhập -> Campaigns 2. Chọn status "Active" từ dropdown 3. Quan sát | Chỉ hiện campaign status ACTIVE (isActive=true, endDate > now). Không hiện PAUSED, COMPLETED, DRAFT. Result badge hiển thị **"N Active campaigns"**. StatsCards Active khớp số card. Kết hợp với search + objective -> giao | Có campaign Active |
| CM-14 | Status filter: Paused | 1. Đăng nhập -> Campaigns 2. Chọn status "Paused" | Chỉ hiện campaign status PAUSED (đã deploy, isActive=false). Badge cam "Paused" | Có campaign Paused |
| CM-15 | Status filter: Completed | 1. Đăng nhập -> Campaigns 2. Chọn status "Completed" | Chỉ hiện campaign status COMPLETED (endDate < now hoặc đã manually complete). Badge xám/sky "Completed" | Có campaign Completed |
| CM-16 | Status filter: Draft | 1. Đăng nhập -> Campaigns 2. Chọn status "Draft" | Chỉ hiện campaign status DRAFT (chưa deploy, chưa có facebookCampaignId). Card hiển thị nút "Deploy" | Có campaign Draft |
| CM-17 | Objective filter | 1. Đăng nhập -> Campaigns 2. Chọn Objective: "Traffic" từ dropdown 3. Chọn "Sales" | Traffic: chỉ hiện campaign objective TRAFFIC. Sales: chỉ hiện SALES. Mỗi filter hiển thị đúng mục tiêu. Objective icon trên card khớp với filter. Kết hợp với search + status -> giao | Có campaign nhiều objective |
| CM-18 | Clear All Filters | 1. Đăng nhập -> Campaigns -> chọn status "Active" + objective "Traffic" + search "abc" 2. Click "Clear all" | Tất cả filter reset: search = "", status = "All", objective = "All". Danh sách hiển thị đầy đủ. Nút **"Clear all"** biến mất. Result badge hiển thị **"N campaigns"** (tổng số). Không có toast | Đang có filter |
| CM-19 | Filter không có kết quả | 1. Đăng nhập -> Campaigns 2. Search "xyzabc123" không tồn tại 3. Quan sát | CampaignEmptyState hiển thị text **"No matching campaigns"** (context-aware, khác với "No campaigns yet"). Suggest **"Try adjusting your filters"**. Không crash, không trắng trang | Có campaign nhưng không khớp |
| CM-20 | Empty state khi filter active vs khi không có campaign | 1. Đăng nhập -> Campaigns (có campaign) -> filter không khớp 2. Quan sát 3. Clear filter 4. So sánh | Khi có filter active + không kết quả: hiển thị **"No matching campaigns"** + **"Try adjusting your filters"**. Khi không có filter + chưa có campaign: hiển thị **"No campaigns yet"** + nút **"Create Your First Campaign"**. Hai empty state khác nhau rõ rệt | Có campaign |

### 14.3 SORTING -- Sắp xếp (CM-21 -> CM-26)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-21 | Sort Newest First (mặc định) | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> quan sát thứ tự mặc định | Campaign mới nhất (createdAt lớn nhất) hiển thị đầu tiên. Sort dropdown hiển thị **"Newest"**. API GET /campaigns?sortBy=createdAt&sortDirection=desc. Subtitle có thể hiển thị **"Sorted by newest"** | Có nhiều campaign |
| CM-22 | Sort Oldest First | 1. Đăng nhập -> Campaigns 2. Chọn sort "Oldest" từ dropdown 3. Quan sát | Campaign cũ nhất hiển thị đầu tiên. API GET /campaigns?sortBy=createdAt&sortDirection=asc. Dropdown hiển thị **"Oldest"** | Có nhiều campaign |
| CM-23 | Sort Budget High-Low | 1. Đăng nhập -> Campaigns 2. Chọn sort "Budget High-Low" | Campaign có budget lớn nhất -> nhỏ nhất. API GET /campaigns?sortBy=budget&sortDirection=desc. Dropdown hiển thị **"Budget High-Low"** | Có campaign budget khác nhau |
| CM-24 | Sort Budget Low-High | 1. Đăng nhập -> Campaigns 2. Chọn sort "Budget Low-High" | Campaign có budget nhỏ nhất -> lớn nhất. API GET /campaigns?sortBy=budget&sortDirection=asc | Có campaign budget khác nhau |
| CM-25 | Sort Spend High-Low | 1. Đăng nhập -> Campaigns 2. Chọn sort "Spend High-Low" | Campaign có spend lớn nhất -> nhỏ nhất. API GET /campaigns?sortBy=spend&sortDirection=desc. Campaign chưa spend -> cuối danh sách | Có campaign đã spend |
| CM-26 | Sort Name A-Z | 1. Đăng nhập -> Campaigns 2. Chọn sort "Name A-Z" 3. Chọn lại "Name Z-A" (nếu có) | Campaign sắp xếp theo tên. API GET /campaigns?sortBy=name&sortDirection=asc/desc. Dropdown hiển thị đúng sort đang active | Có campaign tên khác nhau |

### 14.4 CREATE CAMPAIGN -- Tạo chiến dịch (CM-27 -> CM-38)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-27 | Mở Create Campaign Modal | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> click "+ New Campaign" 3. Quan sát modal | CreateCampaignModal mở với form đầy đủ. Các trường: Name (input text), Platform (2 toggle: Facebook highlight xanh, Instagram), Facebook Account (dropdown, loaded từ social accounts), Ad Account (dropdown, loaded từ API khi chọn Facebook Account), Brand (dropdown), Product (optional dropdown, filter theo brand), Content (optional dropdown, filter theo brand), Landing URL (optional input), Targeting (4 preset buttons: Vietnam/US/Worldwide/Custom + textarea JSON khi chọn Custom), Objective (grid 6 options: AWARENESS icon campaign, TRAFFIC icon trending_up, ENGAGEMENT icon thumb_up, LEADS icon person_add, SALES icon shopping_cart, APP_PROMOTION icon phone_android), Total Budget (input VND), Start Date, End Date. Nút "Create Campaign" ở footer | Đã kết nối Facebook account |
| CM-28 | Chọn Platform Facebook | 1. Đăng nhập -> "+ New Campaign" 2. Click nút Facebook -> được highlight 3. Click Instagram -> Facebook bỏ highlight, Instagram highlight | Chỉ chọn được 1 platform tại 1 thời điểm (toggle). Facebook: hiển thị Facebook Account dropdown. Instagram: hiển thị note **"Instagram ads run through Facebook Ad Accounts"** bên dưới | -- |
| CM-29 | Load Ad Accounts khi chọn Facebook Account | 1. Đăng nhập -> "+ New Campaign" 2. Chọn Facebook Account từ dropdown 3. Quan sát Ad Account dropdown | Ad Account dropdown hiển thị loading spinner + text **"Loading ad accounts..."**. Sau khi load -> chọn được ad account. Nếu không có ad account -> hiển thị message **"No ad accounts found"** | Facebook account có ad accounts |
| CM-30 | Chọn brand -> load product và content | 1. Đăng nhập -> "+ New Campaign" 2. Chọn brand "Brand A" từ dropdown 3. Quan sát Product và Content dropdown | Product dropdown load sản phẩm của Brand A. Content dropdown load content của Brand A. Nếu brand không có product/content -> dropdown trống hoặc hiển thị **"No products"** / **"No content"** | Brand A có products và contents |
| CM-31 | Chọn Targeting preset: Vietnam | 1. Đăng nhập -> "+ New Campaign" 2. Click nút "Vietnam" trong Targeting 3. Quan sát | Nút Vietnam được highlight. Các nút khác bỏ highlight. Textarea JSON không hiển thị (chỉ hiện khi chọn Custom). Giá trị targeting được set thành preset Vietnam (vd: geo_locations countries=['VN']) | -- |
| CM-32 | Chọn Targeting preset: Custom (JSON) | 1. Đăng nhập -> "+ New Campaign" 2. Click nút "Custom" 3. Quan sát | Nút Custom được highlight. Textarea JSON hiển thị để nhập targeting thủ công. Placeholder hoặc giá trị mẫu. Có thể nhập JSON tự do. Validate JSON format (nếu FE có) | -- |
| CM-33 | Chọn Objective từ grid | 1. Đăng nhập -> "+ New Campaign" 2. Click objective "TRAFFIC" 3. Click "SALES" | Mỗi lần click -> objective được chọn (highlight, border đổi màu). Chỉ chọn được 1 objective. Objective cũ bỏ highlight. Icon + label hiển thị đúng màu sắc | -- |
| CM-34 | Tạo campaign thành công với đầy đủ thông tin | 1. Đăng nhập -> "+ New Campaign" 2. Name: "Test Campaign Q3", Platform: Facebook, Facebook Account: chọn, Ad Account: chọn, Brand: "Brand A", Objective: TRAFFIC, Budget: 5000000, Start: 01/08/2026, End: 31/08/2026 3. Click "Create Campaign" | Nút chuyển spinner "Creating...". API POST /campaigns với body CreateAdCampaignRequest. Modal đóng. Campaign mới xuất hiện trong grid với status DRAFT (isActive=false). StatsCards cập nhật (Total tăng 1). Toast: **"Campaign created successfully"**. Campaign có thể Deploy | Brand A đã có Facebook account + ad account |
| CM-35 | Tạo campaign tối thiểu (chỉ required fields) | 1. Đăng nhập -> "+ New Campaign" 2. Name: "Min Campaign", Platform: Facebook, Facebook Account: chọn, Ad Account: chọn, Brand: "Brand A" 3. Không chọn Product, Content, Objective, Budget, Date 4. Click Create | Nếu objective, budget, dates không required -> tạo thành công, toast **"Campaign created successfully"**. Budget=0, objective rỗng. Nếu required -> hiển thị validation message **"Objective is required"** / **"Budget is required"**. **[GHI NHẬN]** Cần ghi nhận chính xác required fields từ validation FE và BE | Brand A có Facebook account |
| CM-36 | Tạo campaign thất bại: thiếu Name | 1. Đăng nhập -> "+ New Campaign" 2. Không nhập Name 3. Click Create | Validation FE: hiển thị message **"Campaign name is required"** dưới ô Name. Nút Create disabled hoặc báo lỗi. BE: trả 400 **"Name is required"** nếu bypass FE | -- |
| CM-37 | Tạo campaign thất bại: thiếu Brand | 1. Đăng nhập -> "+ New Campaign" -> nhập Name 2. Không chọn Brand -> Click Create | Validation: hiển thị message **"Brand is required"**. BE: trả 400 nếu BrandId null/empty | -- |
| CM-38 | Tạo campaign thất bại: thiếu Ad Account | 1. Đăng nhập -> "+ New Campaign" -> nhập Name, chọn Brand 2. Không chọn Ad Account -> Click Create | Validation: hiển thị message **"Ad account is required"**. Nếu không có Facebook account được chọn -> không load được ad accounts | -- |

### 14.5 EDIT CAMPAIGN -- Chỉnh sửa chiến dịch (CM-39 -> CM-46)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-39 | Mở Edit Campaign Modal cho campaign DRAFT | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> card DRAFT -> click Edit 3. Quan sát modal | EditCampaignModal mở với tất cả trường được pre-populated từ data campaign. Tất cả trường đều editable (không bị khóa). Form giống CreateCampaignModal. Title modal: "Edit Campaign". Nút footer: "Save Changes" | Có campaign DRAFT |
| CM-40 | Edit campaign DRAFT: đổi budget và objective | 1. Đăng nhập -> Edit campaign DRAFT 2. Đổi Budget: 10000000, Objective: SALES 3. Click "Save Changes" | API PUT /campaigns/{id} với body update. Modal đóng. Card campaign cập nhật: budget mới 10M, objective icon SALES. Toast: **"Campaign updated successfully"** | Campaign DRAFT |
| CM-41 | Edit campaign đã deploy (có facebookCampaignId) | 1. Đăng nhập -> Edit campaign ACTIVE đã deploy 2. Quan sát modal | Modal hiển thị amber warning banner: **"This campaign has been deployed to Facebook. Only the campaign name can be modified."** hoặc tương tự. Các trường bị khóa (disabled, xám): Budget, Start/End Date, Targeting, Product, Content, Ad Account, Objective. Chỉ có Name là editable. Nút Save vẫn hoạt động để lưu name | Campaign ACTIVE đã deploy |
| CM-42 | Edit campaign deployed: chỉ đổi Name | 1. Đăng nhập -> Edit campaign deployed 2. Đổi Name thành "Updated Name" 3. Click "Save Changes" | API PUT /campaigns/{id} với body chỉ có name. BE UpdateAsync: nếu deployed -> chỉ update name (các field khác bị block). Card campaign cập nhật tên mới. Các field khác không đổi. Toast: **"Campaign updated successfully"** | Campaign deployed |
| CM-43 | Edit campaign deployed: thử đổi budget (bị chặn) | 1. Đăng nhập -> Edit campaign deployed 2. Thử sửa Budget (field disabled) 3. Nếu bypass FE gọi API PUT với budget mới | FE: field Budget bị disabled, không sửa được. BE: nếu nhận update budget -> trả lỗi **"Cannot modify budget after deployment"** (400). Campaign không đổi | Campaign deployed |
| CM-44 | Edit campaign PAUSED (đã deploy, đang tạm dừng) | 1. Đăng nhập -> Edit campaign PAUSED 2. Quan sát | Hành vi giống CM-41: deployed campaign -> chỉ sửa được Name. Cảnh báo amber hiển thị **"This campaign has been deployed to Facebook. Only the campaign name can be modified."** | Campaign PAUSED đã deploy |
| CM-45 | Edit campaign COMPLETED | 1. Đăng nhập -> card COMPLETED -> click Edit | **[GHI NHẬN]** Nút Edit có thể bị ẩn hoặc disabled cho campaign COMPLETED. Nếu mở được -> chỉ sửa được Name (nếu đã deploy) hoặc đầy đủ (nếu chưa deploy). Cần ghi nhận thực tế | Campaign COMPLETED |
| CM-46 | Hủy Edit (đóng modal không Save) | 1. Đăng nhập -> Edit campaign -> thay đổi vài field 2. Click ngoài modal hoặc nút Close/X | Modal đóng. Campaign không thay đổi. Mở lại Edit -> data vẫn là giá trị cũ (không lưu thay đổi trước đó). Không có toast hay message (silent close) | -- |

### 14.6 STATUS ACTIONS -- Deploy / Start / Pause / Restart (CM-47 -> CM-56)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-47 | Deploy campaign DRAFT lên Facebook | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> card DRAFT -> click "Deploy" 3. Quan sát | Nút Deploy chuyển spinner "Deploying...". API POST /campaigns/{id}/deploy. BE thực hiện 4-step pipeline: (1) Create Facebook campaign, (2) Create ad set (daily budget = total/ngày), (3) Create ad creative (từ content hoặc brand content), (4) Create ad. Nếu thành công: deploymentStatus=Completed, facebookCampaignId được set. Campaign status chuyển từ DRAFT -> ACTIVE (isActive=true). Card cập nhật: hiển thị metrics, action buttons đổi thành Pause + View Details + Edit + Delete. Toast: **"Campaign deployed to Facebook successfully"** | Campaign DRAFT, Facebook App đã config, ad account hợp lệ |
| CM-48 | Deploy campaign thất bại (lỗi Facebook API) | 1. Đăng nhập -> Deploy campaign với ad account không hợp lệ 2. Quan sát | API deploy fail ở 1 trong 4 steps. deploymentStatus=Failed, deploymentStep ghi nhận step bị fail. Campaign vẫn DRAFT. Toast hiển thị: **"Failed to deploy campaign: {error message}"**. Nút Deploy vẫn hiển thị để thử lại. Có thể gọi cleanup | Ad account không hợp lệ |
| CM-49 | Cleanup sau khi deploy fail | 1. Campaign DRAFT bị fail deploy (deploymentStatus=Failed) 2. FE có nút Cleanup hoặc gọi API cleanup 3. Click Cleanup | API POST /campaigns/{id}/cleanup. BE xóa Facebook campaign, ad sets, ads, creatives đã tạo dở. Xóa facebookCampaignId. Reset deploymentStatus về None. Toast: **"Deployment cleaned up successfully"**. Campaign về trạng thái DRAFT sạch, có thể deploy lại | Campaign deployment Failed |
| CM-50 | Pause campaign ACTIVE | 1. Đăng nhập -> card ACTIVE -> click "Pause" 2. Quan sát | Nút Pause chuyển spinner. Gọi updateCampaignStatus(isActive=false) -> API PUT /campaigns/{id} với isActive=false. BE đồng thời pause Facebook campaign qua Facebook API. Campaign status chuyển ACTIVE -> PAUSED. Badge đổi thành cam "Paused". Action button đổi thành "Start". Toast: **"Campaign paused"** | Campaign ACTIVE đã deploy |
| CM-51 | Start campaign PAUSED (đã deploy) | 1. Đăng nhập -> card PAUSED (đã deploy) -> click "Start" 2. StartConfirmModal mở: "Are you sure you want to start this campaign?" + amber warning "Real advertising charges will apply" 3. Click "Confirm" | API PUT /campaigns/{id} với isActive=true. BE resume Facebook campaign. Campaign status PAUSED -> ACTIVE. Badge đổi thành xanh "Active". Toast: **"Campaign started successfully"**. Nếu Cancel -> modal đóng, campaign vẫn PAUSED | Campaign PAUSED đã deploy |
| CM-52 | Start campaign chưa deploy (DRAFT) | 1. Đăng nhập -> card DRAFT -> quan sát nút Start? | **[GHI NHẬN]** Nếu campaign DRAFT chưa deploy -> nút Start có thể ẩn hoặc là "Deploy". Khi click Start (applyCampaign) -> nếu chưa deploy, BE sẽ auto-deploy trước khi activate (có message **"Deploying and activating campaign..."**). Cần ghi nhận flow thực tế | Campaign DRAFT |
| CM-53 | Pause campaign chưa deploy | 1. Đăng nhập -> card DRAFT -> quan sát nút Pause | **[GHI NHẬN]** Nút Pause chỉ hiển thị cho campaign ACTIVE. DRAFT không có nút Pause. Nếu gọi API updateCampaignStatus(isActive=false) với campaign DRAFT -> BE trả lỗi **"Cannot pause a draft campaign"** hoặc success vô nghĩa | Campaign DRAFT |
| CM-55 | Sync Insights cho campaign ACTIVE | 1. Đăng nhập -> card ACTIVE -> nếu có nút Sync 2. Click Sync Insights | API POST /campaigns/{id}/sync-insights. BE fetch Facebook insights (impressions, clicks, spend, conversions). Card metrics cập nhật với số mới. Toast: **"Campaign insights synced"** hoặc **"Insights updated"** | Campaign ACTIVE đã deploy |
| CM-56 | Double click Deploy | 1. Đăng nhập -> card DRAFT -> click Deploy 2 lần nhanh | Lần 1: nút loading/disabled hiển thị **"Deploying..."**. Lần 2: không trigger. Chỉ deploy 1 lần. Toast **"Campaign deployed to Facebook successfully"** hiển thị 1 lần. Không tạo duplicate Facebook campaign | Campaign DRAFT |

### 14.7 DELETE CAMPAIGN -- Xóa chiến dịch (CM-57 -> CM-63)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-57 | Xóa 1 campaign DRAFT | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> card DRAFT -> click Delete 3. DeleteConfirmModal mở: "Delete Campaign" + tên campaign 4. Click "Delete" | API DELETE /campaigns/{id} (soft delete: isDeleted=true). Card biến mất khỏi grid. StatsCards cập nhật (Total giảm 1). Toast: **"Campaign deleted successfully"** | Campaign DRAFT |
| CM-58 | Xóa campaign ACTIVE (đang chạy) | 1. Đăng nhập -> card ACTIVE -> Delete -> Confirm | API DELETE /campaigns/{id}. BE pause Facebook campaign + ad sets + ads trước khi soft delete (SoftDeleteAsync). Campaign bị xóa khỏi grid. Facebook campaign bị pause (không xóa trên Facebook). Toast: **"Campaign deleted successfully"** | Campaign ACTIVE |
| CM-59 | Hủy xóa campaign (Cancel modal) | 1. Đăng nhập -> card -> Delete 2. DeleteConfirmModal mở -> click "Cancel" hoặc click ngoài | Modal đóng. Campaign vẫn trong grid. Không API call. Không toast. Không message (silent close) | -- |
| CM-60 | Xóa campaign đã deploy -> Facebook campaign bị pause | 1. Đăng nhập -> xóa campaign ACTIVE 2. Kiểm tra Facebook Ads Manager | Facebook campaign hiển thị status **"PAUSED"** (không bị xóa). Ad sets và ads cũng hiển thị **"Paused"**. Data được giữ lại trên Facebook. BE chỉ soft delete (isDeleted=true). Toast FE: **"Campaign deleted successfully"** | Campaign ACTIVE |
| CM-61 | Restore campaign đã xóa | 1. Đăng nhập -> xóa 1 campaign 2. Gọi API restore (nếu FE có UI) 3. Quan sát | API POST /campaigns/{id}/restore. BE set isDeleted=false, trả message **"Campaign restored successfully"**. Campaign hiển thị lại trong grid với trạng thái cũ. Nếu FE không có UI restore -> cần gọi qua API tool | Vừa xóa campaign |
| CM-63 | Double click Delete | 1. Đăng nhập -> card -> Delete 2. Click nút Delete 2 lần nhanh trong confirm modal | Lần 1: nút loading/disabled hiển thị spinner + **"Deleting..."**. Lần 2: không trigger. Chỉ gọi DELETE 1 lần. Toast **"Campaign deleted successfully"** hiển thị 1 lần | -- |

### 14.8 DUPLICATE CAMPAIGN -- Nhân bản (CM-64 -> CM-68)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-64 | Duplicate 1 campaign | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> card -> nếu có nút Duplicate -> click 3. Quan sát | API POST /campaigns/{id}/duplicate. BE clone campaign với name "{tên cũ} (copy)". Campaign mới xuất hiện trong grid với status DRAFT (chưa deploy, isActive=false, không có facebookCampaignId). Các field được copy: brand, product, content, objective, budget, targeting, dates, landingUrl. Toast: **"Campaign duplicated successfully"** | Có campaign |
| CM-65 | Duplicate campaign đã deploy | 1. Đăng nhập -> duplicate campaign ACTIVE 2. Quan sát campaign mới | Campaign mới có status DRAFT (không kế thừa facebookCampaignId, deploymentStatus=None). Badge hiển thị **"Draft"**. Budget, objective, targeting được copy. Có thể deploy độc lập. Campaign gốc không bị ảnh hưởng. Toast: **"Campaign duplicated successfully"** | Campaign ACTIVE |
| CM-66 | Duplicate campaign với name đã tồn tại | 1. Đăng nhập -> duplicate campaign có tên "Sale T7" 2. Campaign mới tên "Sale T7 (copy)" 3. Duplicate lần nữa | Lần 2: tên **"Sale T7 (copy) (copy)"**. BE không check trùng tên, không có message cảnh báo trùng. Chấp nhận tên trùng. Toast: **"Campaign duplicated successfully"** | Campaign "Sale T7" |
| CM-67 | Bulk Duplicate nhiều campaign | 1. Đăng nhập -> Campaigns -> check chọn 2 campaign 2. BulkActionsBar -> click "Duplicate Selected" 3. Quan sát | API POST /campaigns/{id}/duplicate cho từng campaign tuần tự. 2 campaign mới xuất hiện trong grid (DRAFT). Campaign gốc không đổi. Toast: **"2 campaigns duplicated"** hoặc từng toast riêng | 2 campaign |
| CM-68 | Duplicate thất bại | 1. Đăng nhập -> duplicate campaign 2. Giả lập lỗi API | Toast: **"Failed to duplicate campaign"**. Không có campaign mới được tạo. Campaign gốc không bị ảnh hưởng | Lỗi API |

### 14.9 CAMPAIGN DETAIL MODAL -- Xem chi tiết (CM-69 -> CM-75)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-69 | Mở Campaign Detail Modal | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> card -> click "View Details" 3. Quan sát modal | Modal full-screen (hoặc large) mở. Header: objective icon + name, brand name, product/content badges. Status badge hiển thị **"Active"** / **"Draft"** / **"Paused"** / **"Completed"**. Objective badge hiển thị label (vd: **"Traffic"**). Days remaining hiển thị **"X days left"** hoặc **"Ongoing"**. Performance Overview: 4 card (Impressions, Clicks + CTR%, Spend, Conversions). Budget Utilization: progress bar + text **"X% of budget spent"**. Campaign Details table: Start Date, End Date, Ad Account, Facebook Campaign ID, Product, Content, Landing URL, Created, Updated. Ad Sets section: mỗi ad set hiển thị daily budget, status, impressions, clicks, spend + danh sách ads bên trong (Facebook Ad ID, CTA, link URL). Footer: nút **"Close"** + contextual action (**"Deploy to Facebook"** / **"Restart"**) | Có campaign đã deploy với ad sets |
| CM-70 | Campaign Detail Modal cho campaign DRAFT | 1. Đăng nhập -> DRAFT -> View Details 2. Quan sát | Không có Ad Sets section (chưa deploy). Metrics = 0. Budget progress = 0%. Không có Facebook Campaign ID. Status hiển thị **"Draft"**. Footer có nút **"Deploy to Facebook"**. Không lỗi | Campaign DRAFT |
| CM-71 | Campaign Detail Modal cho campaign có Ad Sets | 1. Đăng nhập -> ACTIVE đã deploy -> View Details 2. Quan sát Ad Sets section | Mỗi ad set hiển thị: name (hoặc **"Ad Set 1"**), daily budget (VND), status badge, impressions, clicks, spend. Mở rộng ad set -> hiển thị ads: Facebook Ad ID, CTA (call to action), Link URL. Metrics tổng hợp từ tất cả ad sets | Campaign ACTIVE có ad sets |
| CM-72 | Budget utilization bar trong Detail Modal | 1. Đăng nhập -> ACTIVE -> View Details 2. Quan sát budget bar | Bar hiển thị spend/budget. Màu: xanh (< 50%), vàng (50-80%), đỏ (> 80%). Text hiển thị **"X% of budget spent"**. Nếu budget = 0 -> hiển thị **"No budget set"** hoặc ẩn bar | Campaign ACTIVE đã spend |
| CM-74 | Campaign Detail real-time data? | 1. Đăng nhập -> mở Detail Modal 2. Để modal mở vài phút 3. Quan sát metrics | **[GHI NHẬN]** Metrics không tự refresh real-time. Có thể hiển thị message **"Data refreshed at [time]"** nếu có. Khi đóng và mở lại modal -> data được fetch mới từ API GET /campaigns/{id}. Có thể có nút **"Sync Insights"** để refresh | Campaign ACTIVE |
| CM-75 | Campaign Detail Modal cho campaign có nhiều Ad Sets | 1. Đăng nhập -> campaign có 3+ ad sets -> View Details 2. Quan sát Ad Sets section | Danh sách ad sets scrollable (nếu cao quá). Mỗi ad set có thể collapse/expand. Tổng metrics = sum của tất cả ad sets. Không crash, không tràn layout | Campaign có 3+ ad sets |

### 14.10 STATS CARDS -- Thống kê tổng quan (CM-76 -> CM-81)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-76 | CampaignStatsCards hiển thị đúng số liệu | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> quan sát 6 stats cards 3. Đếm thủ công và so sánh | Total Campaigns = count tất cả campaign, label **"Total Campaigns"**. Active = count campaign ACTIVE, label **"Active"**. Total Spend = sum spend (format VND), label **"Total Spend"**. Impressions = sum impressions (format K/M), label **"Impressions"**. Clicks = sum clicks, label **"Clicks"**. Conversions = sum conversions, label **"Conversions"**. Mỗi card có icon màu + value. Số khớp với thực tế | Có nhiều campaign |
| CM-77 | Budget Utilization bar tổng | 1. Đăng nhập -> Campaigns 2. Quan sát bar dưới StatsCards | Bar hiển thị tổng spend / tổng budget. Text: **"X% of total budget spent"**. Kèm số campaign PAUSED và COMPLETED: **"N paused, M completed"**. Màu sắc theo tỉ lệ | Có campaign đã spend |
| CM-78 | StatsCards cập nhật sau khi tạo campaign mới | 1. Đăng nhập -> Campaigns -> ghi nhận số Total 2. Tạo campaign mới 3. Quan sát StatsCards | Total Campaigns tăng 1. Active không đổi (campaign mới là DRAFT). Các card khác không đổi. Toast khi tạo: **"Campaign created successfully"** | Vừa tạo campaign |
| CM-79 | StatsCards cập nhật sau khi xóa campaign | 1. Đăng nhập -> Campaigns -> ghi nhận số Total 2. Xóa 1 campaign ACTIVE 3. Quan sát | Total giảm 1. Active giảm 1. Total Spend, Impressions, Clicks, Conversions giảm tương ứng. Toast khi xóa: **"Campaign deleted successfully"** | Vừa xóa campaign |
| CM-80 | StatsCards cập nhật sau khi Pause/Start | 1. Đăng nhập -> Campaigns -> Pause 1 campaign ACTIVE 2. Quan sát | Active giảm 1. Paused count trong budget bar tăng 1. Các card khác không đổi. Toast: **"Campaign paused"**. Start lại: Active tăng 1, toast **"Campaign started successfully"** | Vừa pause |
| CM-81 | StatsCards khi không có campaign nào | 1. Đăng nhập vào workspace mới 2. Campaigns | Tất cả 6 card hiển thị **0**. Budget utilization bar = 0% hoặc ẩn, text **"0% of budget spent"** hoặc không hiển thị. Không crash, không NaN | Chưa có campaign |

### 14.11 BULK ACTIONS -- Thao tác hàng loạt (CM-82 -> CM-87)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-82 | BulkActionsBar hiển thị khi chọn campaign | 1. Đăng nhập test@example.com / Pass1234 2. Campaigns -> check chọn 2 campaign 3. Quan sát | BulkActionsBar hiện: icon checklist + text **"2 campaigns selected"** + subtitle **"Choose an action to perform"** + nút **"Duplicate Selected"** + nút **"Delete Selected"** (đỏ) + nút **"Clear"**. Animation slide-in | 2 campaign |
| CM-83 | Bulk Delete nhiều campaign | 1. Đăng nhập -> check 2 campaign -> BulkActionsBar -> "Delete Selected" 2. DeleteConfirmModal mở: "Delete 2 Campaigns" + danh sách tên 3. Click "Delete" | Nút loading. Gọi DELETE /campaigns/{id} tuần tự. Cả 2 card biến mất. Toast: **"2 campaigns deleted"**. BulkActionsBar biến mất. StatsCards cập nhật | 2 campaign |
| CM-84 | Bulk Duplicate nhiều campaign | 1. Đăng nhập -> check 2 campaign -> "Duplicate Selected" | Nút loading. Gọi duplicate từng campaign. 2 campaign mới (DRAFT) xuất hiện trong grid. Toast: **"2 campaigns duplicated"**. Campaign gốc không đổi | 2 campaign |
| CM-85 | Clear selection | 1. Đăng nhập -> check 3 campaign -> BulkActionsBar hiện 2. Click "Clear" | Tất cả checkbox bỏ check. BulkActionsBar biến mất. Campaign không thay đổi. Không toast, không message (silent) | 3 campaign selected |
| CM-86 | Select All campaigns | 1. Đăng nhập -> Campaigns 2. Click checkbox Select All (nếu có) | Tất cả campaign trên grid được check. BulkActionsBar hiện **"N campaigns selected"** (N = tổng số campaign). Có thể thực hiện bulk actions | Nhiều campaign |
| CM-87 | Bulk Actions loading state | 1. Đăng nhập -> check 3 campaign -> Delete Selected -> Confirm 2. Quan sát BulkActionsBar trong quá trình xóa | Nút **"Delete Selected"** chuyển spinner + disabled, text đổi thành **"Deleting..."**. Nút **"Duplicate Selected"** disabled. Nút **"Clear"** có thể vẫn active. Sau khi xong -> toast **"3 campaigns deleted"**, bar biến mất | 3 campaign |

### 14.12 PERMISSIONS & ACCESS (CM-88 -> CM-93)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-88 | Chưa đăng nhập truy cập /campaigns | 1. Mở browser, chưa login 2. Truy cập https://[domain]/campaigns | Redirect về /login. Sau login -> redirect về /campaigns. Không hiển thị nội dung | Chưa login |
| CM-89 | Token hết hạn khi thao tác campaign | 1. Đăng nhập -> Campaigns 2. Xóa token localStorage 3. Click Create/Edit/Delete | API 401 -> redirect /login + message **"Session expired"**. Hoặc toast lỗi tùy FE xử lý | Token hết hạn |
| CM-90 | Cross-workspace: campaign WS A không hiển thị trong WS B | 1. WS A có 3 campaign 2. Switch sang WS B -> Campaigns | Không thấy campaign của WS A. API GET /campaigns filter theo X-Workspace-Id. Empty state hiển thị **"No campaigns yet"** hoặc **"No matching campaigns"** (nếu có filter). Toast không bắt buộc | 2 workspace |
| CM-91 | Viewer có thể tạo campaign? | 1. Đăng nhập Viewer 2. Campaigns -> "+ New Campaign" 3. Điền form -> Create | **[GHI NHẬN]** FE có thể không chặn. BE AdCampaignService.CreateAsync kiểm tra workspace membership nhưng không check role cụ thể. Nếu tạo được -> toast **"Campaign created successfully"**. Nếu bị chặn -> message **"Insufficient permissions"** hoặc 403 | Viewer |
| CM-92 | Viewer có thể deploy/delete campaign? | 1. Đăng nhập Viewer 2. Campaigns -> Deploy/Delete campaign | **[GHI NHẬN]** BE [Authorize] + workspace ownership check, không check role. Nếu Viewer không bị chặn -> toast **"Campaign deployed to Facebook successfully"** / **"Campaign deleted successfully"**. Nếu bị chặn -> message **"Insufficient permissions"** (403). Cần ghi nhận thực tế | Viewer + campaign |
| CM-93 | Member bị kick khỏi workspace -> không thấy campaign | 1. User bị kick 2. F5 Campaigns | API GET /campaigns trả lỗi. FE hiển thị message **"Access denied"** hoặc **"Workspace not found"**. Redirect về dashboard hoặc hiển thị trang lỗi | Vừa bị kick |

### 14.13 EDGE CASES & UI (CM-94 -> CM-100)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CM-94 | Mất mạng khi load Campaigns | 1. Đăng nhập -> DevTools Offline 2. Vào Campaigns (hoặc F5) | Toast lỗi: **"Failed to load campaigns"**. Hiển thị empty state hoặc skeleton. StatsCards = 0. Không crash, có thể thử lại khi có mạng | Mất mạng |
| CM-95 | Mất mạng khi tạo campaign | 1. Đăng nhập -> "+ New Campaign" -> điền form 2. DevTools Offline -> Create | API POST fail. Toast: **"Failed to create campaign"**. Modal vẫn mở, data giữ nguyên. Có thể thử lại | Mất mạng |
| CM-96 | Mất mạng khi deploy campaign | 1. Đăng nhập -> DRAFT -> Deploy 2. Mất mạng giữa quá trình deploy | API POST deploy fail. deploymentStatus=Failed hoặc vẫn None. Toast: **"Failed to deploy campaign"**. Campaign vẫn DRAFT. Cần cleanup trước khi deploy lại | Mất mạng |
| CM-97 | Tạo campaign với ngày End trước Start | 1. Đăng nhập -> "+ New Campaign" 2. Start Date: 01/08/2026, End Date: 01/07/2026 3. Click Create | FE validation: hiển thị message **"End date must be after start date"** dưới ô End Date, viền đỏ. BE: trả 400 **"End date must be after start date"** nếu bypass FE. Không tạo được campaign | -- |
| CM-98 | Tạo campaign với budget = 0 | 1. Đăng nhập -> "+ New Campaign" 2. Budget: 0 3. Click Create | **[GHI NHẬN]** FE có thể không validate budget > 0. Nếu cho phép -> tạo thành công với toast **"Campaign created successfully"**. Nếu validate -> message **"Budget must be greater than 0"**. Nếu deploy với budget = 0 -> Facebook API báo lỗi **"Invalid budget amount"** | -- |
| CM-99 | Platform Instagram: chọn content Instagram | 1. Đăng nhập -> "+ New Campaign" 2. Platform: Instagram 3. Chọn content từ dropdown 4. Quan sát | **[GHI NHẬN]** Hiển thị note **"Instagram ads run through Facebook Ad Accounts"**. Nếu chọn content Instagram -> BE sẽ boost existing post (message **"Boosting existing Instagram post"**). Nếu không có content Instagram -> deploy tạo ad creative mới. Cần ghi nhận flow thực tế | Content Instagram |
| CM-100 | Refresh trang Campaigns giữ nguyên state? | 1. Đăng nhập -> Campaigns -> filter Active + objective Traffic + sort Budget High-Low 2. F5 reload | **[GHI NHẬN]** Filter, search, sort state reset về default sau F5 (không persist qua URL). Page hiển thị lại với message subtitle **"N campaigns"** (tất cả). Sort dropdown hiển thị **"Newest"**. Người dùng phải filter lại từ đầu | Đang có filter |

**Module:** CAMPAIGN | **Total:** 75 cases | **Page:** `/campaigns` | **API:** `/campaigns`, `/campaigns/{id}`, POST deploy/duplicate/restore/sync-insights/cleanup


---


## SHEET 15/20: PAYMENT & SUBSCRIPTION -- Thanh toán & Gói đăng ký (55 cases)

| **Feature** | Payment & Subscription -- Quản lý gói subscription, thanh toán qua PayOS, tạo Business workspace |
|---|---|
| **Test requirement** | Pricing page `/pricing`: Tab-based UI (Subscription Plans, Credit Packs), plan category toggle Personal/Business, yearly/monthly billing toggle, feature comparison table, current plan badge + credit balance, plan upgrade flow; Payment flow: chọn plan/credit pack -> tạo PayOS checkout -> redirect PayOS -> callback/webhook -> activate subscription/grant credits -> sync; Business workspace creation flow (3-step: Overview -> Workspace & Plan -> Payment with QR); Subscription management: view current plan, upgrade, renewal stacking, cancel, downgrade, expiry handling; Payment history: paginated list with transaction ID, amount, currency, status, date |
| **Pages** | `/pricing`, `/profiles/[id]?section=subscription` |
| **API** | POST `/payment/checkout`, POST `/payment/business-workspace-checkout`, POST `/payment/business-workspace-checkout/sync`, POST `/payment/callback`, POST `/payment/webhook`, GET `/payment/history`, GET `/payment/subscription/current` |
| **Model** | `Payment` (id, userId, subscriptionId, workspaceId, pendingWorkspaceName, requestedPlan, amount, currency, status, paymentType, creditPackCode, creditAmount, paymentMethod, transactionId, invoiceUrl, isDeleted, createdAt), `Subscription` (id, workspaceId, plan, quotaPostsPerMonth, quotaAIContentPerDay, quotaAIImagesPerDay, quotaPlatforms, quotaAccounts, analysisLevel, quotaAdBudgetMonthly, quotaAdCampaigns, startDate, endDate, isActive, isDeleted, payOSOrderCode, payOSPaymentLinkId) |
| **Plans** | Personal: Free (50 credits/week, 20 posts/mo), Plus (2,000đ, 300 posts/mo, 50 prompt/day, 10 image/day), Premium (3,000đ, 1,000 posts/mo, 200 prompt/day, 30 image/day), PlusTrial (0đ, 300 posts/mo, 10 prompt/day, 3 image/day). Business: Plus (4,000đ, 5,000 posts/mo), Premium (5,000đ, 20,000 posts/mo), PlusTrial (0đ, 1,000 posts/mo). Credit Packs: Starter (2,000đ, 100 credits), Standard (3,000đ, 500 credits), Growth (4,000đ, 1,500 credits), Business (5,000đ, 5,000 credits) |

### 15.1 PRICING PAGE -- Trang giá & gói dịch vụ (PY-01 -> PY-14)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PY-01 | Truy cập trang Pricing với gói hiện tại | 1. Truy cập https://[domain]/login 2. Nhập Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Từ sidebar hoặc header, click "Pricing" hoặc "Upgrade Plan" 5. Quan sát toàn bộ trang | Header hiển thị title "Pricing", subtitle "Choose the plan that fits your needs". Tab "Subscription Plans" active mặc định, tab "Credit Packs" inactive. Toggle Personal/Business (theo workspace type hiện tại). Badge "Current Plan" hiển thị plan đang dùng + credit balance. Card plan hiển thị: tên plan, giá/tháng, list features, nút "Current Plan" (disabled, xanh nhạt) cho plan đang dùng, nút "Upgrade" cho plan cao hơn. Toggle Monthly/Yearly (Yearly = tiết kiệm 17%) | Đăng nhập với plan Free |
| PY-02 | Toggle Monthly/Yearly billing | 1. Đăng nhập -> Pricing -> tab Subscription Plans 2. Click toggle "Yearly" 3. Click lại "Monthly" | Yearly: giá hiển thị = price * 10 (tiết kiệm ~17% so với 12 tháng). Text hiển thị "Save 17% with yearly billing". Monthly: giá hiển thị theo tháng. Tất cả plan cards cập nhật giá đồng thời | Đang ở trang Pricing |
| PY-03 | Toggle Personal/Business plan category | 1. Đăng nhập vào Personal workspace -> Pricing 2. Quan sát toggle hoặc dropdown Personal/Business 3. Chọn Business | Personal mặc định hiển thị Personal plans (Free, Plus, Premium). Chọn Business -> hiển thị Business plans (Plus, Premium) với giá và quota khác (cao hơn). Workspace vẫn là Personal, toggle chỉ để xem. Nút upgrade cho Business plan redirect tạo Business workspace | Personal workspace |
| PY-04 | Feature Comparison Table hiển thị đầy đủ | 1. Đăng nhập -> Pricing -> Subscription Plans 2. Cuộn xuống dưới các plan cards 3. Quan sát bảng so sánh | Bảng hiển thị các feature dạng row: Posts/Month, AI Text/Day, AI Images/Day, Platforms, Social Accounts, Analysis Level, Ad Budget/Month, Ad Campaigns, Team Members, API Access, Priority Support, etc. Mỗi cột là 1 plan (Free, Plus, Premium). Dấu check (xanh) hoặc dấu gạch ngang (xám). Số liệu khớp với plan definition | -- |
| PY-05 | Current Plan badge hiển thị đúng | 1. Đăng nhập user Plan Premium -> Pricing 2. Quan sát card Premium | Card Premium hiển thị badge "Current Plan" (màu primary, nổi bật). Nút trong card là "Current Plan" (disabled). Các plan khác hiển thị nút "Upgrade" hoặc "Downgrade" (tùy plan) | Plan Premium |
| PY-06 | Plan Free không có nút Upgrade/Downgrade | 1. Đăng nhập user Free -> Pricing 2. Quan sát card Free | Card Free hiển thị badge "Current Plan". Nút "Current Plan" disabled. Không có nút "Downgrade". Các plan cao hơn hiển thị nút "Upgrade" | Free plan |
| PY-07 | Pricing page khi chưa đăng nhập | 1. Mở browser ẩn danh, chưa login 2. Truy cập https://[domain]/pricing | Trang Pricing hiển thị public (không cần login). Không có badge "Current Plan". Tất cả plan hiển thị nút "Get Started" hoặc "Sign Up". Click nút -> redirect về /register | Chưa đăng nhập |
| PY-08 | Pricing page với plan PlusTrial | 1. Đăng nhập user đang dùng PlusTrial 2. Pricing -> Subscription Plans | Card PlusTrial hiển thị badge "Current Plan". Giá "Free". Hạn sử dụng (nếu có trial end date). Nút upgrade lên Plus/Premium. Credit balance hiển thị 100 (trial credits) | Plan PlusTrial |
| PY-09 | Tab Credit Packs trên Pricing | 1. Đăng nhập -> Pricing 2. Click tab "Credit Packs" 3. Quan sát | Nội dung tab hiển thị: wallet balance bar (current/max + progress bar), 4 credit pack cards (Starter, Standard, Growth, Business). Mỗi card: tên pack, số credits, giá VND, nút "Buy Now" (disabled nếu balance+credits > maxBalance). Description text | Đăng nhập |
| PY-10 | Credit pack bị disable khi vượt max balance | 1. Đăng nhập user balance = 14,500 (Personal max 15,000) 2. Pricing -> Credit Packs 3. Quan sát card Standard (500 credits) | Card Standard: nút "Buy Now" disabled (xám, cursor not-allowed). Card Starter (100 credits) vẫn enabled (14,500+100=14,600 <= 15,000). FE logic: balance + pack.credits > maxBalance -> disabled | Balance 14,500 |
| PY-11 | Yearly billing discount hiển thị đúng | 1. Đăng nhập -> Pricing -> toggle Yearly 2. Quan sát giá card Plus | Giá: "20,000₫/year" (2,000*10). Badge "Save 17%". Text "4,000₫ saved vs monthly". Định dạng số VND có dấu phân cách | -- |
| PY-12 | Loading skeleton khi load Pricing | 1. Đăng nhập -> DevTools Slow 3G -> Pricing 2. Quan sát trạng thái loading | Hiển thị skeleton cards (animate-pulse) cho plan cards. Sau khi load -> cards thật hiển thị. Badge "Current Plan" xuất hiện sau khi biết subscription hiện tại | -- |
| PY-13 | Refresh Pricing sau khi upgrade | 1. Đăng nhập Free -> Pricing 2. Mua plan Plus (tab khác) -> F5 Pricing | Card Plus hiển thị "Current Plan" + disabled. Card Free hiển thị "Downgrade" (hoặc ẩn). Credit balance mới hiển thị (500 credits). Không cache data cũ | Vừa upgrade lên Plus |
| PY-14 | Mất mạng khi load Pricing | 1. Đăng nhập -> DevTools Offline -> Pricing | API GET /payment/subscription/current fail. Hiển thị skeleton/empty cards. Toast: **"Failed to load plans"**. Có nút Retry | Mất mạng |

### 15.2 PAYMENT CHECKOUT & PAYOS -- Thanh toán qua PayOS (PY-15 -> PY-28)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PY-15 | Tạo checkout subscription thành công | 1. Đăng nhập test@example.com / Pass1234 2. Pricing -> plan Plus -> Upgrade -> Confirm 3. Quan sát response và redirect | API POST /payment/checkout với body: {paymentType: 1, planCode: "Plus", returnUrl, cancelUrl}. Response chứa checkoutUrl (PayOS URL). FE redirect hoặc hiển thị QR. BE tạo Payment status Pending, Subscription (isActive=false). PayOS orderCode được lưu | PayOS đã config |
| PY-16 | PayOS config chưa được cấu hình | 1. Đăng nhập -> Pricing -> Upgrade Plus 2. PayOS settings chưa set 3. Click Proceed | API trả lỗi 503: **"PayOS is not configured"** (PAYOS_NOT_CONFIGURED). Toast: **"Payment service unavailable"**. Không tạo Payment record | PayOS chưa config |
| PY-17 | Checkout với plan Free (amount = 0) | 1. Đăng nhập user Premium -> Pricing -> card Free -> click "Downgrade" | API trả lỗi: **"Plan does not require payment"** (PLAN_DOES_NOT_REQUIRE_PAYMENT). Không tạo PayOS checkout | Plan Free amount = 0 |
| PY-18 | Checkout với plan PlusTrial (amount = 0) | 1. Đăng nhập -> Pricing -> PlusTrial -> Upgrade | API trả lỗi: **"Plan does not require payment"**. Tương tự PY-17 | Plan PlusTrial |
| PY-19 | Checkout không có returnUrl/cancelUrl | 1. Gọi POST /payment/checkout với returnUrl = null | API trả lỗi 503: **"PayOS URL is not configured"** (PAYOS_URL_NOT_CONFIGURED) | FrontendSettings chưa set |
| PY-20 | PayOS checkout URL redirect | 1. Đăng nhập -> tạo checkout thành công 2. FE redirect sang checkoutUrl 3. Quan sát trang PayOS | Trình duyệt redirect sang PayOS checkout page. Hiển thị: số tiền (VND), mô tả đơn hàng, phương thức thanh toán (QR code, internet banking, ví điện tử). Không lỗi CORS | PayOS đã config |
| PY-21 | PayOS callback: thanh toán thành công | 1. Đăng nhập -> tạo checkout -> thanh toán trên PayOS 2. PayOS redirect về FE callback URL | Browser redirect về returnUrl với ?code=00&status=PAID&orderCode=xxx. FE gọi POST /payment/callback?{params}. Nếu thành công -> toast **"Payment successful"**. Subscription được activate. Credits được cộng | Thanh toán thành công |
| PY-22 | PayOS callback: thanh toán thất bại | 1. Đăng nhập -> tạo checkout -> PayOS -> hủy 2. PayOS redirect về cancelUrl với ?cancel=true&status=CANCELLED | FE hiển thị message: **"Payment was cancelled"**. Payment status = Failed. Subscription không được activate. Có nút "Try Again" | Thanh toán thất bại |
| PY-23 | PayOS webhook (server-to-server) | 1. Đăng nhập -> tạo checkout -> thanh toán 2. PayOS gửi webhook POST /api/payment/webhook | BE verify HMAC-SHA256 signature. Nếu hợp lệ + status PAID -> activate subscription, grant credits. Subscription được activate ngay cả khi user chưa redirect về | Webhook đến |
| PY-24 | Webhook signature không hợp lệ | 1. Gửi webhook với signature sai | API trả lỗi 400: **"Invalid signature"** (PAYOS_SIGNATURE_INVALID). Không xử lý payment | -- |
| PY-25 | Webhook thiếu signature / thiếu reference | 1. Gửi webhook không có header signature 2. Gửi webhook không có orderCode | Thiếu signature: **"Signature is required"** (400). Thiếu reference: **"Reference is required"** (400). Không xử lý | -- |
| PY-26 | Webhook idempotent: gọi 2 lần cùng payment | 1. Webhook lần 1 -> xử lý thành công (activate + grant) 2. Webhook lần 2 giống hệt | Lần 2: BE check payment đã Success -> trả success (200) nhưng không cộng thêm credits. Idempotent. Balance không gấp đôi | Webhook lần 1 đã xong |
| PY-27 | PayOS API lỗi khi tạo checkout | 1. Đăng nhập -> Upgrade Plus 2. Giả lập PayOS API lỗi (network fail, 5xx) | API trả lỗi 502/503: **"Failed to create payment request"**. Payment status Failed. Toast: **"Payment failed. Please try again."** | PayOS API lỗi |
| PY-28 | Webhook + Callback race condition | 1. Tạo checkout -> thanh toán 2. Webhook và callback đến đồng thời | BE transaction Serializable -> xử lý tuần tự. Cái đến trước xử lý, cái sau thấy đã Success -> trả success. Không duplicate, không lỗi | Đồng thời |

### 15.3 SUBSCRIPTION MANAGEMENT -- Quản lý gói đăng ký (PY-29 -> PY-42)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PY-29 | Xem thông tin subscription hiện tại | 1. Đăng nhập test@example.com / Pass1234 2. Settings -> Subscription hoặc `/profiles/[id]?section=subscription` 3. Quan sát | Hiển thị: plan name, status badge "Active" (xanh), startDate, endDate. Quota info: posts X/Y, AI prompts X/Y per day, AI images X/Y per day, platforms, accounts. Nút "Upgrade Plan" -> /pricing. Credit wallet balance | Có subscription active |
| PY-30 | Subscription status Active | 1. Đăng nhập user có subscription active (startDate <= today <= endDate) 2. Settings -> Subscription | Status badge "Active" (xanh emerald). Quota hoạt động. Feature gate mở (Calendar, Analytics...). Credit free top-up hoạt động (Free plan) | Subscription Active |
| PY-31 | Subscription hết hạn (expired) | 1. Đăng nhập user có endDate < today 2. Settings -> Subscription 3. Thử dùng các feature | Status badge "Expired" (cam/đỏ). Quota bị chặn (PROMPT_QUOTA_EXCEEDED, POST_QUOTA_EXCEEDED). Feature gate đóng. Credit free top-up ngừng. Nút "Renew Now" dẫn đến /pricing | Subscription hết hạn |
| PY-32 | Workspace bị expire -> ảnh hưởng features | 1. Đăng nhập workspace có subscriptionExpiredAt < now 2. Vào Dashboard, thử tạo content, xem Calendar | Dashboard hiển thị banner warning: **"Your subscription has expired. Renew now."**. Calendar gate: **"This feature requires a paid plan"**. Tạo content AI -> lỗi PROMPT_QUOTA_EXCEEDED. Wallet không dùng được | Workspace expired |
| PY-33 | Upgrade từ Free lên Plus (full flow) | 1. Đăng nhập Free -> Pricing -> Plus -> Upgrade -> Confirm 2. PayOS -> thanh toán -> callback 3. Kiểm tra subscription mới | Subscription Plus activate, Free deactivate. EndDate = today + 30. Credits +500 (Personal Plus). Workspace subscriptionExpiredAt cập nhật. Payment record Success. Toast: **"Payment successful"** | Free plan |
| PY-34 | Renewal stacking: còn 10 ngày -> gia hạn 30 ngày | 1. Đăng nhập Plus (còn 10 ngày) 2. Mua lại Plus (renew) -> thanh toán | EndDate mới = currentEndDate + 30 = 40 ngày. Subscription cũ deactivate, mới activate. Credits cộng thêm 500. Workspace subscriptionExpiredAt = ngày mới | Plus còn 10 ngày |
| PY-35 | Upgrade từ Premium xuống Plus (downgrade) | 1. Đăng nhập Premium -> Pricing 2. Card Plus -> click "Downgrade" (nếu có) | **[GHI NHẬN]** Hành vi: nếu FE cho phép -> tạo checkout plan Plus. BE deactivate Premium, activate Plus. Cần ghi nhận: downgrade ngay hay chờ hết hạn? Nếu không hỗ trợ -> nút bị ẩn | Plan Premium |
| PY-36 | Gia hạn subscription khi đã hết hạn (renew) | 1. Đăng nhập user hết hạn -> Settings -> Renew Now 2. Chọn plan -> thanh toán | EndDate mới = today + 30. Credits cộng theo plan. Workspace status -> Active. ArchivedAt, DeletedAt bị clear | Subscription hết hạn |
| PY-37 | Cancel subscription | 1. Đăng nhập Plus -> Settings -> Subscription 2. Click "Cancel Subscription" 3. Confirm dialog -> Confirm | **[GHI NHẬN]** BE: có thể set isActive=false ngay hoặc đánh dấu không auto-renew. Subscription vẫn active đến hết endDate. Toast: **"Subscription cancelled"** | Plan Plus |
| PY-38 | Subscription Free plan: không có nút Cancel | 1. Đăng nhập Free -> Settings -> Subscription | Không có nút Cancel. Text **"You are on the Free plan"**. Nút "Upgrade to Plus" dẫn đến /pricing | Free plan |
| PY-39 | Subscription plan precedence (nhiều subscription active) | 1. Workspace có 2 subscription: Free + Plus (lỗi data) 2. GET /payment/subscription/current | BE chọn Premium > Plus > PlusTrial > Free. Nếu có Plus và Free -> trả Plus. Nếu không có active -> trả null | 2 subscription active |
| PY-40 | Subscription isActive=false nhưng endDate > today | 1. Cancel subscription (isActive=false), endDate còn 10 ngày 2. Tạo content, đăng bài | **[GHI NHẬN]** isActive=false -> feature gate đóng, quota không hoạt động. Nhưng user đã trả tiền 30 ngày. Cần ghi nhận có phải bug không | isActive=false, còn hạn |
| PY-41 | Subscription deleted (isDeleted=true) | 1. Subscription bị soft delete 2. GET /payment/subscription/current | BE filter IsDeleted=false -> không trả về. FE hiển thị "No active subscription" hoặc fallback Free plan | Subscription deleted |
| PY-42 | Subscription plan Business Plus hiển thị đúng features | 1. Đăng nhập Business workspace Plus 2. Settings -> Subscription | Posts: 5,000/tháng. Members: 10. Credits grant: 15,000. Số liệu khớp PlanDefinition | Business Plus |

### 15.4 BUSINESS WORKSPACE CREATION -- Tạo Business Workspace (PY-43 -> PY-51)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PY-43 | Truy cập flow tạo Business Workspace | 1. Đăng nhập user 2. Pricing -> chọn Business -> "Get Started" hoặc /pricing?create=business 3. Quan sát | Flow 3 bước: Step 1 "Overview" (mô tả Business workspace), Step 2 "Workspace & Plan" (input name + chọn plan), Step 3 "Payment". Progress bar thể hiện bước hiện tại. Nút Back/Next | Đăng nhập |
| PY-44 | Business Workspace: Step 1 Overview | 1. Đăng nhập -> /pricing?create=business 2. Quan sát Step 1 | Title "Create Business Workspace", mô tả lợi ích (team members, higher quotas, more credits), so sánh Personal vs Business. Nút "Next" sang Step 2 | -- |
| PY-45 | Business Workspace: Step 2 nhập tên và chọn plan | 1. Đăng nhập -> flow tạo Business 2. Nhập Workspace Name: "Công ty TNHH ABC" 3. Chọn plan: Business Plus 4. Click Next | Name hợp lệ (không rỗng, <=255 chars). Plan được highlight. Nếu thiếu name -> nút Next disabled + message **"Workspace name is required"**. Nếu ok -> sang Step 3 | -- |
| PY-46 | Business Workspace: Step 3 Payment | 1. Đăng nhập -> tạo Business workspace 2. Step 3: hiển thị thông tin thanh toán | Hiển thị: workspace name, plan selected, price (4,000đ Business Plus), total amount. QR code PayOS hoặc nút "Proceed to Payment". Nút "Back" quay lại Step 2 | Đã chọn name + plan |
| PY-47 | Business Workspace: Tạo checkout thành công | 1. Đăng nhập -> Step 3 -> "Proceed to Payment" | API POST /payment/business-workspace-checkout: {workspaceName, plan, returnUrl, cancelUrl}. BE: validate name (required, max 255), plan (Plus/Premium). Tạo Payment với PendingWorkspaceName, WorkspaceId=null. Redirect PayOS | PayOS config |
| PY-48 | Business Workspace: Tạo checkout với WorkspaceName rỗng | 1. Đăng nhập -> bỏ trống name 2. POST /payment/business-workspace-checkout | API trả 400: **"Workspace name is required"**. FE nên chặn trước (nút disabled) | -- |
| PY-49 | Business Workspace: Tạo checkout với plan Free | 1. POST business-workspace-checkout với plan=Free | API trả 400: **"Business plan is required"** (BUSINESS_PLAN_REQUIRED). Business chỉ có Plus và Premium | -- |
| PY-50 | Business Workspace: Sync sau thanh toán thành công | 1. Đăng nhập -> tạo Business -> thanh toán PayOS -> callback 2. FE gọi POST /payment/business-workspace-checkout/sync | BE: tạo Workspace (Business type), Subscription active, CreditWallet, cấp credits, thêm user làm Owner. FE redirect về overview workspace mới. Toast: **"Business workspace created successfully"** | Thanh toán thành công |
| PY-51 | Business Workspace: Sync idempotent / sync khi chưa thanh toán | 1. Sync lần 1 -> thành công 2. Sync lần 2 với cùng reference 3. Sync khi Payment Pending | Lần 2: trả success với workspace hiện có, không tạo thêm. Payment Pending: trả 400 **"Payment not completed"**. Reference không tồn tại: 404 **"Payment not found"** | -- |

### 15.5 PAYMENT HISTORY & PERMISSIONS (PY-52 -> PY-55)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PY-52 | Payment history hiển thị sau khi thanh toán | 1. Đăng nhập -> đã có 3 giao dịch 2. `/profiles/[id]?section=subscription` -> tab Payment History | Danh sách paginated Payments: Transaction ID, Amount (VND format), Currency (VND), Status (badge: Success xanh, Pending vàng, Failed đỏ), Payment Type (Subscription/CreditPack), Date (dd MMM yyyy). Có InvoiceUrl -> nút "View Invoice" | Có 3 giao dịch |
| PY-53 | Payment status Refunded | 1. Payment đã Success -> Admin/PayOS refund 2. Kiểm tra subscription | Payment status = Refunded. **[GHI NHẬN]** BE không có logic revert subscription khi refund. Subscription vẫn active. Cần manual xử lý | Payment Refunded |
| PY-54 | Subscription Plan Free/PlusTrial không tạo Payment record | 1. Đăng nhập -> activate Free hoặc PlusTrial subscription | BE tạo Subscription trực tiếp (isActive=true) không qua PayOS. Không tạo Payment. GET /payment/history không có giao dịch này | Free/PlusTrial plan |
| PY-55 | Refresh Pricing nhiều lần liên tiếp | 1. Đăng nhập -> F5 Pricing 5 lần nhanh | API calls bình thường. Không tạo duplicate subscription. Không crash. UI nhất quán sau mỗi lần load | -- |


### 15.6 ADMIN PAYMENTS -- Quan ly thanh toan Admin (PY-56 -> PY-62)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PY-56 | Admin: xem danh sach payments | 1. Dang nhap Admin -> sidebar -> "Payments" 2. Quan sat trang | AdminHeader: "Payments". Filter bar: status dropdown (All/Completed/Pending/Failed). Bang AdminDataTable: Transaction ID (truncate 8 chars, monospace), Amount (format VND), Status (StatusBadge: Success xanh, Pending vang, Failed do), Date. Pagination. Text hien thi "N total transactions". API GET /admin/payments?page=1&pageSize=20 | Admin, co payments |
| PY-57 | Admin: filter payments theo status | 1. Admin -> Payments 2. Chon filter: "Completed" -> "Pending" -> "Failed" -> "All" | Moi filter goi API GET /admin/payments?status=N. Completed=1, Pending=0, Failed=2, All=undefined. Bang cap nhat dung. Pagination reset ve page 1 | Admin, co payments nhieu status |
| PY-58 | Admin: xem danh sach subscriptions | 1. Admin -> sidebar -> "Subscriptions" 2. Quan sat trang | API GET /admin/payments/subscriptions?page=1&pageSize=20. Bang: Plan (text), Start Date, End Date, Active (StatusBadge), Workspace Name, Workspace ID. Pagination | Admin |
| PY-59 | Admin: update subscription | 1. Admin -> Subscriptions -> chon 1 subscription 2. Edit: doi Plan, EndDate, hoac IsActive 3. Save | PATCH /admin/payments/subscriptions/{id} voi body: {plan?, endDate?, isActive?}. Chi update non-null fields. Response: "Subscription updated." Bang cap nhat. Audit log ghi nhan | Admin |
| PY-60 | Admin: revenue stats | 1. Admin -> goi GET /admin/payments/revenue/stats?period=month | Response chua thong ke doanh thu theo period. Hien thi trong Admin Dashboard hoac widget rieng. Period: month/week/year | Admin |
| PY-61 | Admin: subscription validation - khong tim thay | 1. PATCH subscription voi id khong ton tai | API tra 404: "Subscription not found." Toast loi. Khong crash | Admin |
| PY-62 | Chua phai Admin truy cap Payments | 1. User thuong -> truy cap /admin/payments | [Authorize(Roles=Admin)] -> 403. FE hien thi "Access denied" hoac redirect | User thuong |

### 15.7 ADMIN PLANS -- Quan ly goi dich vu Admin (PY-63 -> PY-68)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PY-63 | Admin: xem danh sach plans | 1. Admin -> sidebar -> "Plans" 2. Quan sat trang | GET /admin/plans -> 5 plans (Free, Plus, Premium, Business Plus, Business Pro). Moi card: name, price (VND format), credits, posts/month, members, features count, StatusBadge (Active/Disabled). Nut: Edit, Enable/Disable, Delete. Nut "Add Plan" + "Save All" | Admin |
| PY-64 | Admin: edit plan | 1. Admin -> Plans -> click Edit tren 1 plan 2. Inline edit panel mo: inputs cho Name, Price, Credits, Posts/Mo, Members 3. Features toggle area (12 chips: basicAnalytics, advancedAnalytics, generateText, aiImage, aiVideo, multiPlatformPublish, schedulePost, trendAnalysis, holidaySuggestion, campaignRecommendation, workspaceDashboard, teamManagement) 4. Doi Price + toggle features -> "Save Plan" | Panel dong. Plan cap nhat trong local state. Nut "Save All" hien thi (chua luu vao DB). Neu roi trang -> thay doi mat (chua persist) | Admin |
| PY-65 | Admin: save all plans | 1. Admin -> Plans -> edit 1 plan + add 1 plan moi 2. Click "Save All" | PUT /admin/plans voi body {plans: [...]}. Plans luu vao SystemSetting key "subscription.plans". Toast "Saved!" hien thi 2s. FE Plan list cap nhat. Nut "Save All" bi disabled (khong con unsaved changes) | Admin |
| PY-66 | Admin: add new plan | 1. Admin -> Plans -> "Add Plan" | Plan moi voi id=plan-{timestamp}, name="New Plan", price=0, credits=100, postsPerMonth=50, members=1, basic features, isActive=true. Card xuat hien trong list. Plan co the edit truoc khi Save All | Admin |
| PY-67 | Admin: toggle plan active/inactive | 1. Admin -> Plans -> click "Disable" tren plan dang Active | Plan isActive=false. Card co opacity-60. StatusBadge chuyen "Disabled" (amber). Nut doi thanh "Enable". Click "Enable" -> isActive=true tro lai. Chua persist den khi Save All | Admin |
| PY-68 | Admin: delete plan | 1. Admin -> Plans -> click Delete tren 1 plan 2. Confirm (neu co) | Plan bi xoa khoi local list. Chua persist den khi Save All. Plan Free khong the xoa? **[GHI NHAN]** Co validate khong cho xoa plan dang duoc workspace su dung khong? | Admin |

**Module:** PAYMENT & SUBSCRIPTION | **Total:** 68 cases | **Pages:** `/pricing`, `/profiles/[id]?section=subscription` | **API:** POST `/payment/checkout`, POST `/payment/business-workspace-checkout`, POST `/payment/business-workspace-checkout/sync`, POST `/payment/callback`, POST `/payment/webhook`, GET `/payment/history`, GET `/payment/subscription/current`, GET `/admin/payments`, GET `/admin/payments/revenue/stats`, GET/PATCH `/admin/payments/subscriptions`, GET/PUT `/admin/plans`


---


## SHEET 16/20: CREDIT, QUOTA & WALLET -- Credit, Hạn ngạch & Ví (65 cases)

| **Feature** | Credit, Quota & Wallet -- Quản lý credit wallet, consumption, credit packs, lịch sử credit, quota system, member credit quotas |
|---|---|
| **Test requirement** | Credit Wallet: balance hiển thị + progress bar, auto-create wallet khi chưa có, credit consumption (GenerateText 1, GenerateImage 5, GenerateVideo 20, RegenerateText 1, TrendAnalysis 2, CampaignRecommendation 2), insufficient balance handling, reserved balance cho automation; Credit Pack Purchase: 4 packs (Starter/Standard/Growth/Business), confirm dialog, pre-purchase validation (max balance), PayOS checkout; Credit History page `/credit-history`: paginated list, filter tabs All/Success/Failed, record details (action icon+color, username, feature, credits +/-, timestamp), cost summary card, daily summary chart; Quota System: prompt quota daily (text + image), post quota (weekly Free / monthly Paid), workspace-level quota summary, quota exceeded handling; Member Credit Quotas: SharedPool, MonthlyAssignedLimit, LifetimeAssignedLimit, owner set member quota, member limit exceeded |
| **Pages** | `/credit-pack`, `/credit-history`, `/profiles/[id]?section=subscription` |
| **API** | POST `/payment/checkout` (paymentType=CreditPack), GET `/credit-usage/wallet`, GET `/credit-usage/daily-summary`, GET `/credit-usage`, GET `/quota/workspace/current` |
| **Model** | `CreditWallet` (id, workspaceId, balance, reservedBalance, createdAt, updatedAt), `CreditUsageRecord` (id, workspaceId, userId, aiGenerationId, action, credits, status, createdAt) |
| **Credit Actions** | SubscriptionGrant (1), CreditPackGrant (2), GenerateText (1 credit), RegenerateText (1 credit), GenerateImage (5 credits), GenerateVideo (20 credits), TrendAnalysis (2 credits), CampaignRecommendation (2 credits) |
| **Limits** | Max Balance: Personal 15,000, Business 500,000. Free credits: 50/week (7-day cycle). Member modes: SharedPool, MonthlyAssignedLimit, LifetimeAssignedLimit |

### 16.1 CREDIT WALLET -- Ví credit (CR-01 -> CR-15)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CR-01 | Xem credit wallet balance | 1. Đăng nhập test@example.com / Pass1234 2. Vào `/credit-pack` hoặc `/credit-history` 3. Quan sát wallet info | Hiển thị balance hiện tại. Progress bar balance/maxBalance (Personal 15,000 / Business 500,000). Text "Available Credits" hoặc "Wallet Balance". Số khớp API GET /credit-usage/wallet | Có credits |
| CR-02 | Wallet balance = 0 | 1. Đăng nhập workspace hết credits 2. Vào /credit-pack hoặc /credit-history | Balance = "0 credits". Progress bar 0%. Cảnh báo **"No credits remaining"**. Nút "Buy Credits" dẫn đến /pricing | Balance = 0 |
| CR-03 | Wallet auto-created khi chưa có | 1. Đăng nhập workspace mới, chưa có CreditWallet 2. Tạo content AI -> trigger ConsumeCreditsAsync | BE: EnsureWalletExistsAsync -> tạo wallet balance=0. EnsureCurrentFreeCreditsAsync -> top-up 50 (Free plan). Balance = 50. CreditUsageRecord: SubscriptionGrant +50 | Workspace mới |
| CR-04 | Credit consumption: GenerateText (1 credit) | 1. Đăng nhập user balance = 50 2. Tạo content AI Text -> generate | Balance giảm 1 -> 49. CreditUsageRecord: action GenerateText, credits -1, status Success. Wallet.UpdatedAt cập nhật | Balance 50 |
| CR-05 | Credit consumption: GenerateImage (5 credits) | 1. Đăng nhập user balance = 50 2. Tạo content AI Image -> generate | Balance giảm 5 -> 45. CreditUsageRecord: action GenerateImage, credits -5, status Success | Balance 50 |
| CR-06 | Credit consumption: GenerateVideo (20 credits) | 1. Đăng nhập user balance = 50 2. Tạo content AI Video -> generate | Balance giảm 20 -> 30. CreditUsageRecord: action GenerateVideo, credits -20, status Success | Balance 50 |
| CR-07 | Credit consumption: RegenerateText (1 credit) | 1. Đăng nhập -> regenerate content text đã có | Hành vi giống GenerateText: -1 credit, action RegenerateText. Cũng tính vào prompt usage | Đã có content text |
| CR-08 | Credit consumption: TrendAnalysis (2 credits) | 1. Đăng nhập -> sử dụng tính năng phân tích xu hướng | Balance giảm 2. CreditUsageRecord: action TrendAnalysis, credits -2 | Balance >= 2 |
| CR-09 | Credit consumption: CampaignRecommendation (2 credits) | 1. Đăng nhập -> sử dụng campaign recommendation | Balance giảm 2. CreditUsageRecord: action CampaignRecommendation, credits -2 | Balance >= 2 |
| CR-10 | Credit consumption: không đủ balance | 1. Đăng nhập user balance = 3 2. Tạo AI Image (cần 5 credits) | API check: available = balance - reservedBalance = 3 < 5 -> lỗi INSUFFICIENT_WORKSPACE_CREDITS. CreditUsageRecord status Failed. Toast: **"Insufficient credits"**. Balance không đổi | Balance 3 |
| CR-11 | Credit consumption: reserved balance cho automation | 1. Workspace balance=100, reservedBalance=80 2. User tạo AI Text (1 credit) | Available = 20 >= 1 -> thành công. Balance=99. Nếu user cần 25 -> lỗi (available=20 < 25) | Reserved 80, balance 100 |
| CR-12 | Max balance enforcement: Personal workspace | 1. Personal workspace, balance=14,900 2. Mua credit pack Standard (500 credits) | BE check: 14,900+500=15,400 > max 15,000 -> lỗi CREDIT_BALANCE_LIMIT_EXCEEDED. Toast: **"Maximum 15,000 credits"**. UI credit pack disabled | Balance 14,900 |
| CR-13 | Max balance enforcement: Business workspace | 1. Business workspace, balance=498,000 2. Mua Business pack (5,000 credits) | 498,000+5,000=503,000 > max 500,000 -> lỗi. Mua Standard (500): 498,500 <= 500,000 -> thành công | Balance 498,000 |
| CR-14 | Credit Wallet serializable transaction khi concurrent consume | 1. 2 user trong workspace tạo AI text đồng thời (balance=50) 2. Mỗi user cần 1 credit | BE transaction IsolationLevel.Serializable -> tuần tự. Không overspend, không race condition. Cả 2 thành công (balance 48) | 2 user đồng thời |
| CR-15 | Automation credit reserve / settle / release | 1. Reserve 80 credits (balance 100->reserved 80) 2. Generate thành công -> Settle: balance-5, reserved-5 3. Cancel -> Release: reserved về 0, balance giữ nguyên | Reserve: available=20. Settle: trừ cả balance + reserved, ghi CreditUsageRecord. Release: hoàn reserved, không ghi usage. Automation không charge khi cancel | Automation plan |

### 16.2 CREDIT PACK PURCHASE -- Mua gói credit (CR-16 -> CR-25)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CR-16 | Trang Credit Pack hiển thị wallet và packs | 1. Đăng nhập test@example.com / Pass1234 2. Sidebar -> "Credit Pack" hoặc từ Pricing tab Credit Packs 3. Quan sát | Header: "Credit Wallet" + balance + progress bar. 4 cards: Starter (100 credits, 2,000đ), Standard (500 credits, 3,000đ), Growth (1,500 credits, 4,000đ), Business (5,000 credits, 5,000đ). Mỗi card: icon, tên, credits, giá, mô tả, nút "Buy Now" | Đăng nhập |
| CR-17 | Credit pack pre-purchase confirm dialog | 1. Đăng nhập -> Credit Pack -> Starter -> "Buy Now" 2. Quan sát dialog | Dialog: title "Purchase Starter Pack", current balance, credits to add (+100), new balance, total price (2,000đ). Cảnh báo "Credits cannot be refunded." Footer: Cancel + Confirm | Balance 500 |
| CR-18 | Mua credit pack thành công (full flow) | 1. Đăng nhập -> Starter -> Buy Now -> Confirm 2. Redirect PayOS -> thanh toán -> callback | POST /payment/checkout (paymentType=2, creditPackCode=1). Sau callback: Balance +100. Toast: **"Credits added successfully"**. Payment history có record mới | Thanh toán thành công |
| CR-19 | Webhook cho credit pack purchase | 1. Mua Starter -> thanh toán -> webhook đến | BE: verify signature -> apply: GrantCreditPackCreditsAsync (100 credits). Balance +100. CreditUsageRecord CreditPackGrant. Payment status -> Success. Subscription không đổi | Thanh toán credit pack |
| CR-20 | Credit pack code không hợp lệ | 1. POST /payment/checkout với creditPackCode=99 | API trả 400: **"Invalid credit pack"** (INVALID_CREDIT_PACK). Không tạo checkout | -- |
| CR-21 | Credit pack: không chọn credit pack code | 1. POST /payment/checkout paymentType=2, creditPackCode=null | API trả 400: **"Credit pack is required"** (CREDIT_PACK_REQUIRED) | -- |
| CR-22 | Credit pack: cancel dialog | 1. Đăng nhập -> Starter -> Buy Now 2. Confirm dialog -> Cancel hoặc click ngoài | Dialog đóng. Không checkout. Balance không đổi. Có thể mở lại | -- |
| CR-23 | Credit pack Balance Progress Bar màu sắc | 1. Đăng nhập -> Credit Pack 2. Quan sát progress bar các mức balance | < 30%: xanh primary. 30-60%: xanh ngọc. 60-85%: vàng amber. > 85%: đỏ danger. Width = balance/max*100%. Text: "X / Y credits" | -- |
| CR-24 | Double click Buy Now | 1. Đăng nhập -> Starter -> click "Buy Now" 2 lần nhanh | Lần 1: mở dialog/tạo checkout. Lần 2: bị chặn. Chỉ tạo 1 checkout. Không charge 2 lần | -- |
| CR-25 | Refresh Credit page sau khi mua | 1. Balance 500 -> mua Starter (+100) -> thành công 2. F5 Credit page | Balance = 600. Progress bar tăng. History có record mới. Không cache cũ | Vừa mua credit pack |

### 16.3 CREDIT USAGE HISTORY -- Lịch sử sử dụng credit (CR-26 -> CR-37)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CR-26 | Truy cập Credit History page | 1. Đăng nhập test@example.com / Pass1234 2. Sidebar -> "Credit History" hoặc /credit-history 3. Quan sát toàn bộ trang | Header: "Credit History". Summary card: credit costs (text=1, image=5, video=20). Filter tabs: All, Success, Failed. Danh sách paginated: action icon+color, action name, username, credits (-X hoặc 0), timestamp. Pagination footer | Có 5+ records |
| CR-27 | Credit history hiển thị đúng icon và màu theo action | 1. Đăng nhập -> Credit History -> quan sát các row | GenerateText: icon article, xanh dương. GenerateImage: icon image, tím. GenerateVideo: icon movie, đỏ. SubscriptionGrant/CreditPackGrant: icon wallet, xanh lá (+X). TrendAnalysis: icon trending_up, cam. CampaignRecommendation: icon campaign, hồng. Grant: dương (+X, xanh). Consumption: âm (-X, đỏ) | Có records đủ loại |
| CR-28 | Credit history: filter tab Success | 1. Đăng nhập -> Credit History 2. Click tab "Success" | Chỉ hiện records status Success. Tab Success active (highlight). Credits hiển thị giá trị thực | Có Success và Failed |
| CR-29 | Credit history: filter tab Failed | 1. Đăng nhập -> Credit History 2. Click tab "Failed" | Chỉ hiện records status Failed. Credits hiển thị 0 (không bị trừ). Row có màu đỏ nhạt hoặc badge Failed | Có records Failed |
| CR-30 | Credit history: pagination | 1. Đăng nhập -> Credit History có 25+ records 2. Quan sát pagination | "Showing 1-10 of N records". Nút prev/next, số trang. API: /credit-usage?page=2&pageSize=10 | 25+ records |
| CR-31 | Credit history: daily summary | 1. Đăng nhập -> Credit History 2. API GET /credit-usage/daily-summary?days=7 | Hiển thị biểu đồ/bảng: mỗi ngày có tổng credits used. Chart (bar). 7/30/90 ngày tùy chọn | Có usage 7 ngày |
| CR-32 | Credit history: empty state | 1. Đăng nhập workspace mới, chưa dùng credit 2. Credit History | Icon receipt/credit_card, text "No credit transactions yet". Subtext: "Your credit usage and top-up history will appear here" | Chưa có records |
| CR-33 | Credit history: record link đến AI Generation | 1. Đăng nhập -> Credit History 2. Click row có AiGenerationId | **[GHI NHẬN]** Có thể mở modal chi tiết hoặc redirect đến content. Nếu không có link -> vẫn clickable xem chi tiết | Có record với AiGenerationId |
| CR-34 | Credit history: records của workspace khác không hiển thị | 1. WS A có records 2. Switch WS B -> Credit History | Chỉ hiện records WS B. API filter X-Workspace-Id. Empty state nếu WS B chưa có gì | 2 workspace |
| CR-35 | Credit history: loading skeleton | 1. Đăng nhập -> DevTools Slow 3G -> Credit History | Skeleton rows (animate-pulse): placeholder xám cho icon, action, credits, timestamp. Sau khi load -> data thật | -- |
| CR-36 | Credit history: mất mạng khi load | 1. Đăng nhập -> DevTools Offline -> Credit History | API fail. Toast: **"Failed to load credit history"**. Empty state hoặc error. Có nút Retry | Mất mạng |
| CR-37 | Credit history: record GenerateVideo (20 credits) | 1. Đăng nhập -> tạo AI Video -> thành công 2. Credit History -> record mới nhất | Row: icon movie (đỏ), action "GenerateVideo", credits "-20", username, feature "Video Generation", timestamp. Khớp thời gian tạo video | Vừa tạo video |

### 16.4 FREE CREDITS -- Credit miễn phí (CR-38 -> CR-42)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CR-38 | Free plan credit top-up chu kỳ 7 ngày | 1. Đăng nhập Free, balance=20 2. Qua chu kỳ 7 ngày mới 3. Tạo content (trigger EnsureCurrentFreeCreditsAsync) 4. Kiểm tra balance | BE: Free subscription active + cycle mới + balance < 50 -> top-up lên đúng 50. CreditUsageRecord: SubscriptionGrant +30. Balance = 50 | Free plan, balance 20 |
| CR-39 | Free plan: đã grant trong cycle hiện tại | 1. Đăng nhập Free, balance=45 (đã top-up cycle này) 2. Dùng 5 credits -> balance=40 3. Tạo content tiếp | BE: đã grant -> skip. Balance vẫn 40. Chỉ top-up 1 lần/7 ngày | Balance 40, đã grant |
| CR-40 | Free plan credit top-up: subscription hết hạn | 1. Đăng nhập Free, subscription expired (endDate < today) 2. Balance=10, tạo content | BE: subscription hết hạn -> không top-up. Lỗi INSUFFICIENT_WORKSPACE_CREDITS. Balance giữ nguyên | Free expired |
| CR-41 | Free plan credit top-up: balance >= 50 | 1. Đăng nhập Free, balance=60 (đã mua credit pack) 2. Qua cycle mới -> tạo content | BE: balance >= 50 -> không top-up. Balance giữ nguyên. Hoạt động bình thường | Balance 60, cycle mới |
| CR-42 | Free plan credit top-up: chưa đến cycle mới (mới dùng 3 ngày) | 1. Đăng nhập Free, balance=10 (đã top-up cách đây 3 ngày) 2. Tạo content | BE: vẫn trong cycle hiện tại (chưa đủ 7 ngày) -> không top-up. Lỗi insufficient nếu balance không đủ | Đã top-up 3 ngày trước |

### 16.5 QUOTA SYSTEM -- Hệ thống hạn ngạch (CR-43 -> CR-54)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CR-43 | Xem quota hiện tại của workspace | 1. Đăng nhập test@example.com / Pass1234 2. GET /quota/workspace/current (hoặc UI Posts/Pricing) | Response: planName, subscriptionStatus, windowStart/End, promptQuotaLimit/Usage/Remaining, postQuotaLimit/Usage/Remaining, textContentCount, imageContentCount, videoContentCount | Subscription Active |
| CR-44 | Prompt quota daily limit (Plus: 50/ngày) | 1. Đăng nhập Plus (Personal) 2. Tạo 50 AI text trong 1 ngày (UTC) 3. Tạo text thứ 51 | Lần 51: EnsurePromptQuotaAsync -> usage >= limit -> PROMPT_QUOTA_EXCEEDED (403). Toast: **"Daily prompt quota exceeded (50/50)"**. Không trừ credit | Plus, 50 prompt |
| CR-45 | Prompt quota reset sau UTC midnight | 1. Đăng nhập -> hết 50 prompt 2. Đợi qua midnight UTC -> tạo text mới | Usage reset về 0. Tạo text thành công, usage=1. promptRemaining=49 | Vừa qua midnight |
| CR-46 | Free plan prompt quota = 0 | 1. Đăng nhập Free (Personal) 2. Thử tạo AI Text | promptQuotaLimit=0 -> PROMPT_QUOTA_EXCEEDED. Message: **"AI generation requires a paid plan"**. Link /pricing | Free plan |
| CR-47 | Post quota monthly (Plus: 300/tháng) | 1. Đăng nhập Plus 2. Publish 300 posts trong tháng 3. Thử publish thêm | EnsureWorkspacePostQuotaAsync: usage=300 >= limit -> POST_QUOTA_EXCEEDED (403). Toast: **"Monthly post quota exceeded (300/300)"** | Plus, 300 posts |
| CR-48 | Post quota Free plan (20 posts, weekly window) | 1. Đăng nhập Free 2. Publish 20 posts trong 7 ngày 3. Thử publish thêm | Post quota exceeded. Message: **"Post quota exceeded (20/20)"**. Đợi sang cycle mới (8 ngày sau startDate) -> publish được | Free, 20 posts |
| CR-49 | Free plan post quota weekly window (7 ngày) | 1. Free, startDate=01/07 2. Hết 20 posts trong 01/07-07/07 3. Đợi 08/07 -> publish | Cycle mới từ 08/07 -> post usage reset. Publish thành công. Chu kỳ độc lập, không theo calendar month | Free, hết cycle 1 |
| CR-50 | Image quota daily (Plus: 10/ngày) | 1. Đăng nhập Plus 2. Tạo 10 AI images trong ngày 3. Tạo image thứ 11 | quotaAIImagesPerDay=10, usage=10 -> PROMPT_QUOTA_EXCEEDED. Toast: **"Daily image quota exceeded (10/10)"** | Plus, 10 images |
| CR-51 | Premium plan limits (200 prompt, 30 image/ngày) | 1. Đăng nhập Premium (Personal) 2. Kiểm tra quota | promptQuotaLimit=200, imageQuotaLimit=30, postQuotaLimit=1,000 | Premium Personal |
| CR-52 | Business workspace limits cao hơn | 1. Đăng nhập Business Plus 2. Kiểm tra quota | postQuotaLimit=5,000 (vs Personal 300). AI limits giữ nguyên. Credits grant=15,000 | Business Plus |
| CR-53 | Quota: image quota có tính vào prompt quota không | 1. Đăng nhập Plus 2. Dùng 5 text + 3 image 3. Kiểm tra quota | **[GHI NHẬN]** Prompt usage đếm cả text + image? Hay image quota riêng? BE EnsurePromptQuotaAsync có thể đếm cả 2 loại. Ghi nhận thực tế | Plus, 5 text + 3 image |
| CR-54 | Quota subscription inactive -> limit = 0 | 1. Workspace subscription expired 2. GET /quota/workspace/current | subscriptionStatus="Inactive". Tất cả limit=0. promptRemaining=0, postRemaining=0. Feature gates đóng | Subscription expired |

### 16.6 MEMBER CREDIT QUOTAS -- Hạn ngạch credit thành viên (CR-55 -> CR-60)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CR-55 | Member credit consumption: shared pool | 1. Workspace SharedPool mode, balance=500 2. Member A dùng 10 credits -> balance=490 3. Member B dùng 5 credits -> balance=485 | Trừ từ wallet chung. Member.CreditUsed không check. Không giới hạn riêng | SharedPool, balance 500 |
| CR-56 | Member limit exceeded (MonthlyAssignedLimit) | 1. Member MonthlyAssignedLimit, CreditLimit=100, CreditUsed=99 2. AI Text (1 credit) -> thành công (CreditUsed=100) 3. AI Text tiếp (1 credit) | Lần 3: CreditUsed (100)+1 > 100 -> MEMBER_CREDIT_LIMIT_EXCEEDED. Record status Failed. Wallet không bị trừ. Toast: **"Member credit limit exceeded"** | Hết hạn mức |
| CR-57 | Member MonthlyAssignedLimit reset đầu tháng | 1. Member dùng hết 100/100 trong tháng 7 2. Sang 01/08 -> tạo content | BE check: new month -> reset CreditUsed về 0. Tạo content thành công. Chỉ reset cho MonthlyAssignedLimit | Sang tháng mới |
| CR-58 | Member LifetimeAssignedLimit (không reset) | 1. Member LifetimeAssignedLimit, CreditLimit=500, CreditUsed=500 2. Tạo content | MEMBER_CREDIT_LIMIT_EXCEEDED. Không reset tự động. Owner phải tăng limit thủ công | Hết lifetime limit |
| CR-59 | Member chưa có CreditLimit set (non-shared pool) | 1. Member LifetimeAssignedLimit nhưng CreditLimit null/0 2. Tạo AI Text | BE: CreditLimit <= 0 -> INVALID_MEMBER_CREDIT_LIMIT. Toast: **"Member credit limit not set. Contact owner."** | CreditLimit null |
| CR-60 | Owner set member credit quota | 1. Owner -> Workspace Members -> chọn member 2. Set QuotaMode=MonthlyAssignedLimit, CreditLimit=200 -> Save | Member limit 200/tháng. CreditUsed=0. Khi tạo content -> trừ từ limit này. Hết -> lỗi. Wallet không bị ảnh hưởng (trừ khi SharedPool) | Owner |

### 16.7 PERMISSIONS & EDGE CASES (CR-61 -> CR-65)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CR-61 | Chưa đăng nhập truy cập /credit-pack /credit-history | 1. Chưa login -> /credit-pack 2. Chưa login -> /credit-history | Redirect /login. Sau login -> redirect về trang tương ứng | Chưa login |
| CR-62 | Token hết hạn khi xem credit wallet / history | 1. Đăng nhập -> Credit Pack 2. Xóa token -> F5 | API 401 -> redirect /login + **"Session expired"**. Không hiển thị balance | Token hết hạn |
| CR-63 | Cross-workspace: credit wallet / history của WS A không hiển thị WS B | 1. WS A balance 500, có records 2. Switch WS B -> Credit Pack / History | Hiển thị data WS B. API filter X-Workspace-Id. Hai workspace độc lập | 2 workspace |
| CR-64 | Viewer / Member có thể mua credit pack? | 1. Đăng nhập Viewer/Member 2. Credit Pack -> Buy Now | **[GHI NHẬN]** BE check workspace membership, không check role. Viewer/Member có thể mua nếu không bị chặn. Cần ghi nhận và báo bug nếu không mong muốn | Viewer/Member |
| CR-65 | Mất mạng khi load Credit History -> Retry | 1. Đăng nhập -> DevTools Offline -> Credit History 2. Bật mạng -> Retry | API fail -> toast lỗi + empty state. Sau Retry -> load data bình thường. Không crash | Mất mạng rồi có lại |


### 16.8 ADMIN CREDIT OVERSIGHT -- Giam sat credit Admin (CR-66 -> CR-72)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| CR-66 | Admin: xem Credit Oversight page | 1. Admin -> sidebar -> "AI & Credit" 2. Quan sat trang | GET /admin/credit-oversight/summary. Stats cards: Total AI Generations (icon smart_toy), Weekly AI Usage (icon trending_up), Est. Credits Used (icon toll). Daily AI chart (BarChart 7 ngay gan nhat, cot tim). AI Cost Analysis: Total Generations, Est. Total Cost (VND), Avg Cost/Generation (100 VND), Weekly Trend (+ count). Nut "Adjust Credits" | Admin |
| CR-67 | Admin: Daily AI chart hien thi dung | 1. Admin -> Credit Oversight 2. Quan sat bieu do 7 ngay | BarChart: Truc X = ten ngay (Mon, Tue...), Truc Y = so generations. Hover bar -> tooltip hien thi so. Data tu API dailyAiData. Neu khong co data -> chart empty | Admin |
| CR-68 | Admin: Adjust Credits mo modal | 1. Admin -> Credit Oversight -> click "Adjust Credits" 2. Quan sat modal | Modal: Workspace dropdown (populated tu fetchAdminWorkspaces), Amount input (so nguyen, am = tru, duong = cong), Reason input (required). Nut Confirm disabled khi thieu workspace/amount/reason. Cancel de dong | Admin |
| CR-69 | Admin: Adjust Credits thanh cong | 1. Admin -> modal -> chon workspace + amount=100 + reason="Compensation" 2. Click Confirm | POST /admin/credit-oversight/adjust: {workspaceId, amount: 100, reason}. BE: goi CreditService.AdminAdjustCreditsAsync. Wallet balance cap nhat. Modal dong. alert("Credits adjusted successfully!"). Audit log ghi nhan | Admin |
| CR-70 | Admin: Adjust Credits fail validation | 1. Admin -> modal -> khong chon workspace, amount=0, reason="" -> Confirm | Nut Confirm disabled. FE validate: phai co workspace + amount != 0 + reason not empty. Neu bypass FE -> BE tra 400: "Amount cannot be zero" hoac "Reason is required" | Admin |
| CR-71 | Admin: Adjust Credits am (tru credit) | 1. Admin -> modal -> amount=-50, reason="Penalty" -> Confirm | BE: AdminAdjustCreditsAsync(amount=-50). Wallet balance giam 50 (neu du). CreditUsageRecord ghi admin adjustment. Neu khong du balance -> **[GHI NHAN]** BE co cho phep balance am khong? Hay tra loi? | Admin |
| CR-72 | Chua phai Admin truy cap Credit Oversight | 1. User thuong -> /admin/credit-oversight | [Authorize(Roles=Admin)] -> 403. FE hien thi "Access denied" | User thuong |

**Module:** CREDIT, QUOTA & WALLET | **Total:** 72 cases | **Pages:** `/credit-pack`, `/credit-history`, `/profiles/[id]?section=subscription` | **API:** POST `/payment/checkout` (CreditPack), GET `/credit-usage/wallet`, GET `/credit-usage/daily-summary`, GET `/credit-usage`, GET `/quota/workspace/current`, GET `/admin/credit-oversight/summary`, POST `/admin/credit-oversight/adjust`



---



## SHEET 17/20: ANALYTICS & DASHBOARD -- Phan tich & Bang dieu khien (48 cases)

| **Feature** | Analytics & Dashboard -- Bang dieu khien tong quan, bao cao phan tich hieu suat nguoi dung |
|---|---|
| **Test requirement** | Dashboard page `/dashboard`: Hero card + greeting, KPI grid (Published Posts, AI Usage, AI Credits with progress bar, Posts This Month with progress bar), Daily Credit Usage chart (AreaChart, toggle 7D/30D/90D), Upcoming Schedules panel, Recent Campaigns table with CSV export, Audience widgets (Geographic Distribution, Top Demographics, Device Breakdown), AI Content Suggestions (4 cards), Platform Distribution bar chart; Analytics page `/analytics`: AnalyticsFilterBar (date range, campaign filter, brand filter, platform filter, refresh), AnalyticsKpiCards (4 cards: Total Reach, Total Interactions, Avg. CPE, Published Posts -- each with sparkline SVG + trend indicator), AnalyticsChart (Spend vs Engagement, daily/weekly toggle, interactive tooltips), AnalyticsPerformanceTable (campaign: Name, Reach, Clicks, CTR, ROAS), AnalyticsTopPosts (post table: Post, Brand, Platform, Published, Impressions, Engagement, Clicks, CTR), AnalyticsAiInsights (25s timeout, 8s cooldown), AnalyticsEfficiencyCard, Export CSV |
| **Pages** | `/dashboard`, `/analytics` |
| **API** | GET `/analytics/overview`, GET `/analytics/time-series`, GET `/analytics/channel-breakdown`, GET `/analytics/campaign-breakdown`, GET `/analytics/top-posts`, GET `/analytics/sync-status`, GET `/analytics/usage-breakdown`, GET `/analytics/ai-recommendations`, GET `/analytics/audience`, GET `/dashboard/summary`, GET `/workspace-dashboard/summary` |
| **Model** | `AnalyticsOverviewDto` (dateRange, totals, changes, sparklines, dataFreshness), `AnalyticsTotals` (impressions, reach, engagement, clicks, conversions, ctr, spend, estimatedRevenue, publishedPosts, activeCampaigns), `AnalyticsChanges` (impressionsPct, engagementPct, ctrPct, spendPct, clicksPct, conversionRatePct, cpaPct, roasPct), `AnalyticsTimeSeriesDto` (granularity, points), `AnalyticsChannelBreakdownDto` (platform, integrationId, displayName, impressions, reach, engagement, clicks, ctr, spend, publishedPosts), `CampaignAnalyticsItemDto` (campaignId, name, brandName, platform, objective, status, budget, impressions, reach, engagement, clicks, ctr, spend, estimatedRevenue, conversions, cpa, roas), `TopPostItemDto` (postId, contentId, contentTitle, brandName, platform, publishedAt, externalPostId, impressions, reach, engagement, clicks, ctr), `AudienceBreakdownDto` (geographic, demographics, devices), `UsageBreakdownDto` (items: category, count, percentage), `DashboardSummaryDto` (draftContentCount, publishedContentCount, pendingApprovalContentCount, upcomingScheduleCount, failedScheduleCount, activeSocialIntegrationCount, publishedPostCount, unreadNotificationCount), `WorkspaceDashboardSummaryDto` (workspaceId, creditBalance, creditsUsed, publishedPostCount, postQuotaLimit, postsRemaining, aiUsageCount, activeMemberCount, topMembers) |
| **Feature Gates** | basicAnalytics: ALL plans, advancedAnalytics: PersonalPro+, workspaceDashboard: BusinessPlus+. AI Insights + Efficiency Card gated behind advancedAnalytics |

### 17.1 USER DASHBOARD -- Bang dieu khien tong quan (AN-01 -> AN-15)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AN-01 | Truy cap Dashboard voi du lieu day du | 1. Truy cap https://[domain]/login 2. Nhap Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Quan sat toan bo trang Dashboard | Header: Hero card voi greeting (Xin chao + ten), ngay thang, platform connection indicators (Facebook, Instagram, TikTok icons + status dots). KPI Grid 4 card: Published Posts (so + icon), AI Usage (so lan goi AI), AI Credits (balance/max + progress bar mau), Posts This Month (used/total + progress bar). Daily Credit Usage chart (AreaChart) mac dinh 7D. Upcoming Schedules panel (danh sach bai sap dang + status badges). Recent Campaigns table (Name, Objective, Budget, Spend, Status + nut CSV export). Audience widgets (Geographic Distribution % bars, Top Demographics % bars, Device Breakdown % bars). AI Content Suggestions (4 card: icon + title + mo ta). Platform Distribution (bar chart: Facebook, Instagram, TikTok). Footer: version + system status | Co du content, posts, campaigns, credits |
| AN-02 | Dashboard KPI: Published Posts hien thi dung | 1. Dang nhap -> Dashboard 2. Dem so posts da publish trong workspace 3. So voi KPI card | Card "Published Posts" hien thi dung so. Neu 0 -> hien thi "0". Icon phu hop. So khop API GET /dashboard/summary -> publishedPostCount | Co posts |
| AN-03 | Dashboard KPI: AI Usage hien thi dung | 1. Dang nhap -> Dashboard 2. Dem so lan su dung AI trong workspace 3. So voi KPI card | Card "AI Usage" hien thi dung so lan goi AI (aiUsageCount). So khop API GET /workspace-dashboard/summary | Co AI usage |
| AN-04 | Dashboard KPI: AI Credits voi progress bar | 1. Dang nhap -> Dashboard 2. Quan sat card AI Credits | Hien thi "balance / maxBalance" (vd: "350 / 15,000"). Progress bar mau (xanh <30%, vang 30-60%, do >85%). So khop API GET /credit-usage/wallet | Co credits |
| AN-05 | Dashboard KPI: Posts This Month voi progress bar | 1. Dang nhap -> Dashboard 2. Quan sat card Posts This Month | Hien thi "used / total" (vd: "45 / 300"). Progress bar theo used/total. Neu used >= total -> bar do (100%). So khop API GET /quota/workspace/current -> postQuota | Co post quota |
| AN-06 | Dashboard: Daily Credit Usage chart toggle | 1. Dang nhap -> Dashboard 2. Click toggle 7D -> 30D -> 90D | 7D: AreaChart hien thi credit usage 7 ngay gan nhat. 30D: 30 ngay. 90D: 90 ngay. Chart cap nhat animation. Neu khong du data -> chart hien thi flat line hoac empty | Co credit usage data |
| AN-07 | Dashboard: Upcoming Schedules panel | 1. Dang nhap -> Dashboard -> co schedule sap toi 2. Quan sat Schedules panel | Danh sach 6 upcoming schedules. Moi item: content title, platform icon, scheduled time, status badge (Pending vang, Completed xanh). AI insight text neu co. Click item -> redirect Calendar. Neu khong co schedule -> hien thi "No upcoming schedules" | Co schedules |
| AN-08 | Dashboard: Recent Campaigns table | 1. Dang nhap -> Dashboard -> co campaigns 2. Quan sat bang Recent Campaigns | Bang hien thi 5 campaigns gan nhat: Name, Objective (icon + label), Budget (VND format), Spend (VND format), Status (badge mau). Nut "Export CSV" -> tai file CSV: Name, Objective, Budget, Spend, Status. Click row -> redirect Campaigns page | Co 5+ campaigns |
| AN-09 | Dashboard: Audience Geographic Distribution | 1. Dang nhap -> Dashboard 2. Quan sat widget Geographic Distribution | Hien thi danh sach quoc gia voi % bar: US (38%), UK (22%), Germany (15%), Japan (12%), Others (13%) -- fallback data neu khong co Facebook data. Neu co Facebook page_fans_country -> hien thi du lieu thuc. Moi item: ten quoc gia + % + thanh bar mau | -- |
| AN-10 | Dashboard: Audience Top Demographics | 1. Dang nhap -> Dashboard 2. Quan sat widget Top Demographics | Hien thi nhom tuoi voi % bar: 18-24 (30%), 25-34 (35%), 35-44 (25%), 45+ (10%) -- fallback data. Moi item: age group + % + thanh bar mau. Neu co Facebook page_fans_gender_age -> hien thi du lieu thuc | -- |
| AN-11 | Dashboard: Audience Device Breakdown | 1. Dang nhap -> Dashboard 2. Quan sat widget Device Breakdown | Hien thi: Desktop (52%), Mobile (38%), Tablet (10%) -- fallback data. Moi item: device icon + % + thanh bar. Co the co icon desktop/phone/tablet | -- |
| AN-12 | Dashboard: AI Content Suggestions | 1. Dang nhap -> Dashboard 2. Quan sat section AI Content Suggestions | 4 card suggestion: Eco-Friendly Packaging, Morning Routine Series, Behind the Scenes, Holiday Gift Guide. Moi card: icon (lightbulb/auto_awesome/bulb/tips_and_updates), title, mo ta ngan. Click vao card -> co the mo Create Content modal voi suggestion pre-filled | -- |
| AN-13 | Dashboard: Platform Distribution chart | 1. Dang nhap -> Dashboard -> co posts da dang 2. Quan sat Platform Distribution bar chart | Bar chart hien thi so luong posts theo platform: Facebook (xanh), Instagram (hong), TikTok (den). Chieu cao bar tuong ung so posts. Label hien thi so luong posts moi platform. Neu chua co posts -> bar bang 0 | Co posts nhieu platform |
| AN-14 | Dashboard: Empty state khi chua co du lieu | 1. Dang nhap workspace moi, chua tao content/posts/campaigns 2. Dashboard | KPI cards hien thi 0. Charts hien thi empty/flat line. Schedules: "No upcoming schedules". Campaigns: "No campaigns yet". Audience: fallback data. Platform Distribution: 0 all. Khong crash | Workspace moi |
| AN-15 | Dashboard: Loading skeleton | 1. Dang nhap -> DevTools Slow 3G -> Dashboard | Hien thi skeleton: placeholder xam cho KPI cards, charts, tables, audience widgets. Sau khi load -> data that. Animation pulse. Khong crash | -- |

### 17.2 USER ANALYTICS PAGE -- Trang bao cao phan tich (AN-16 -> AN-30)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AN-16 | Truy cap Analytics page voi du lieu | 1. Dang nhap test@example.com / Pass1234 2. Sidebar -> "Analytics" hoac /analytics 3. Quan sat toan bo trang | Header: breadcrumbs "Dashboard > Analysis", title "Reports & Analytics", nut "Refresh" + "Export Report". Skeleton loading trong luc fetch data. Sau khi load: AnalyticsFilterBar (date range, campaign, brand, platform filters), AnalyticsKpiCards (4 cards), AnalyticsChart (Spend vs Engagement), AnalyticsPerformanceTable (campaigns), AnalyticsTopPosts (top posts), AnalyticsAiInsights + AnalyticsEfficiencyCard (neu advancedAnalytics) | Co du lieu analytics |
| AN-17 | Analytics: FilterBar date range | 1. Dang nhap -> Analytics 2. Chon date range "7d" -> "30d" -> "90d" | "7d": hien thi du lieu 7 ngay gan nhat. "30d": 30 ngay (default). "90d": 90 ngay. FilterBar hien thi date range dang chon (highlight). Data tu dong refetch khi doi date range. KPI cards + chart + tables cap nhat | Co du lieu |
| AN-18 | Analytics: FilterBar campaign filter | 1. Dang nhap -> Analytics 2. Chon campaign filter: "active" -> "paused" -> "completed" | Moi filter hien thi campaign co status tuong ung. "all" (default): tat ca campaign. KPI cards + chart + table chi tinh toan cho campaign duoc filter. Neu khong co campaign khớp -> hien thi 0 | Co campaign nhieu status |
| AN-19 | Analytics: FilterBar brand filter | 1. Dang nhap -> Analytics 2. Chon brand tu dropdown ("all" -> "Brand A" -> "Brand B") | Dropdown duoc populate tu API fetchBrands(). Moi brand hien thi analytics data rieng. "all": tong hop tat ca brand. Data refetch khi doi brand | Co nhieu brand |
| AN-20 | Analytics: FilterBar platform filter | 1. Dang nhap -> Analytics 2. Chon platform: "facebook" -> "instagram" -> "tiktok" -> "all" | Moi filter hien thi analytics data cho platform tuong ung. "all": tong hop tat ca platform. Data refetch khi doi platform | Co posts nhieu platform |
| AN-21 | Analytics KPI Cards hien thi dung | 1. Dang nhap -> Analytics 2. Quan sat 4 KPI cards | Total Reach: tot.impressions (hoac tot.reach), trend arrow + % thay doi (changes.impressionsPct). Total Interactions: tot.engagement, trend + %. Avg. CPE: spend/engagement (cost per engagement), trend. Published Posts: tot.publishedPosts, trend. Moi card co sparkline SVG (7 ngay gan nhat). Trend len -> xanh la + arrow up. Trend xuong -> do + arrow down | Co analytics data |
| AN-22 | Analytics KPI: Sparkline SVG hien thi dung | 1. Dang nhap -> Analytics 2. Quan sat sparkline trong moi KPI card | Sparkline 7 diem (7 ngay), duong mau primary hoac xam. Neu data bang 0 -> flat line. Responsive SVG, khong bi vo layout. Hover sparkline -> tooltip hien thi gia tri tung ngay | Co sparkline data |
| AN-23 | Analytics KPI: Trend indicator (tang/giam) | 1. Dang nhap -> Analytics 2. So sanh period hien tai vs truoc 3. Quan sat trend arrow | Neu metric tang so voi ky truoc: arrow up + xanh la + % duong. Neu giam: arrow down + do + % am. Neu khong thay doi: flat arrow + xam + 0%. % duoc tinh: changes.impressionsPct, v.v. Khong hien thi NaN hoac undefined | Co data 2 period |
| AN-24 | AnalyticsChart: Spend vs Engagement | 1. Dang nhap -> Analytics 2. Quan sat bieu do chinh | Bieu do hien thi 2 duong: Spend (VND, truc Y trai) va Engagement (so luong, truc Y phai). Truc X: ngay (daily view) hoac tuan (weekly view). Toggle daily/weekly de chuyen doi. Hover data point -> tooltip hien thi gia tri chi tiet (ngay, spend, engagement, cpc, impressions) | Co time-series data |
| AN-25 | AnalyticsChart: Daily/Weekly toggle | 1. Dang nhap -> Analytics 2. Click toggle "Daily" -> "Weekly" | Daily: moi diem la 1 ngay. Weekly: gom 7 ngay thanh 1 diem, labels hien thi tuan (vd: "Week 1"). Chart cap nhat animation | 14+ ngay data |
| AN-26 | AnalyticsPerformanceTable: bang campaign | 1. Dang nhap -> Analytics 2. Quan sat bang Campaign Performance | Bang hien thi danh sach campaign: Name, Reach, Clicks, CTR (%), ROAS (return on ad spend). Sap xep theo impressions mac dinh (desc). Co the click header de sort (impressions, clicks, ctr, spend, engagement). Row hover -> mau nen thay doi. Click row -> co the mo Campaign Detail | Co campaign |
| AN-27 | AnalyticsTopPosts: bang top posts | 1. Dang nhap -> Analytics -> co posts da dang 2. Quan sat bang Top Posts | Bang hien thi top 10 posts theo engagement: Post (title), Brand, Platform (icon + name), Published (date), Impressions, Engagement, Clicks, CTR (%). Neu khong co posts -> hien thi "No post data available for this period". Sap xep theo engagement desc | Co posts da publish |
| AN-28 | AnalyticsAiInsights: AI recommendations | 1. Dang nhap plan PersonalPro+ -> Analytics 2. Click nut "Generate Insights" (neu co) hoac xem AI Insights widget | Goi API GET /analytics/ai-recommendations. Hien thi 3-5 recommentadion ngan gon kem emoji (vd: Tang cuong quang cao...). Co timeout 25s (AbortController). Cooldown 8s giua cac lan click. Nut hien thi "Wait Ns to retry" trong cooldown. Neu API fail -> hien thi message loi hoac empty | advancedAnalytics feature, co data |
| AN-29 | AnalyticsAiInsights: timeout 25s | 1. Dang nhap -> Analytics -> Generate Insights 2. Network cham >25s | Sau 25s: AbortController huy request. Hien thi message **"Request timed out"** hoac empty. Nut reset ve trang thai co the click lai sau cooldown 8s | Network cham |
| AN-30 | AnalyticsEfficiencyCard: hieu suat | 1. Dang nhap plan PersonalPro+ -> Analytics 2. Quan sat AnalyticsEfficiencyCard | Hien thi cac chi so hieu suat: CPA (cost per acquisition), ROAS, conversion rate. Dang card hoac panel compact nam ben phai. Neu khong du data -> hien thi "N/A" | advancedAnalytics feature |

### 17.3 ANALYTICS EXPORT & EDGE CASES (AN-31 -> AN-40)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AN-31 | Export CSV tu Analytics page | 1. Dang nhap -> Analytics 2. Click "Export Report" | Tai file CSV: `analytics-report-YYYY-MM-DD.csv`. Cot: Date, Spend, CPC, Impressions, Engagement, Clicks, CTR, Published Posts. Data khop voi chartData. Client-side generate CSV. Khong loi | Co analytics data |
| AN-32 | Export CSV khi khong co data | 1. Dang nhap -> Analytics (workspace moi) 2. Click "Export Report" | handleExport return early vi !data. Khong tai file. Khong crash. Co the hien thi toast **"No data to export"** (tuy FE) | Chua co data |
| AN-33 | Refresh Analytics data | 1. Dang nhap -> Analytics 2. Click nut Refresh (icon refresh) | Data tu dong refetch tat ca API (overview, time-series, channel, campaigns, top posts). KPI cards + chart + tables cap nhat. Nut loading/spin khi refetch. Khong reload trang | -- |
| AN-34 | Analytics: FilterBar co data thay doi khi switch workspace | 1. WS A -> Analytics (filter brand A) 2. Switch WS B -> Analytics | Filter reset ve default (date=30d, campaign=all, brand=all, platform=all). Data cua WS B duoc load. Khong hien thi data WS A | 2 workspace |
| AN-35 | Analytics: empty state khi khong co campaign nao | 1. Dang nhap workspace moi, chua co campaign 2. Analytics | KPI cards hien thi 0. AnalyticsChart: flat line hoac empty. PerformanceTable: "No campaigns yet". TopPosts: "No post data available for this period". AI Insights: khong co gi de phan tich. Khong crash | Chua co campaign |
| AN-36 | Analytics: loading skeleton | 1. Dang nhap -> DevTools Slow 3G -> Analytics | Hien thi 4 animated placeholder cards + chart placeholder. Sau khi load -> data that. Khong crash | -- |
| AN-37 | Analytics: mat mang khi load | 1. Dang nhap -> DevTools Offline -> Analytics | API fail. Hien thi skeleton hoac empty state. Co the co toast **"Failed to load analytics data"**. Khong crash | Mat mang |
| AN-38 | Analytics: audience data fallback khi khong co Facebook | 1. Dang nhap workspace khong connect Facebook 2. Dashboard -> Audience widgets | Hien thi fallback data: US 38%, UK 22%, Germany 15%, Japan 12%, Others 13%. Age: 18-24/25-34/35-44/45+. Devices: Desktop 52%, Mobile 38%, Tablet 10%. Khong crash, khong trong | Chua connect Facebook |
| AN-39 | Analytics: Sync Status hien thi | 1. Dang nhap -> Analytics 2. Xem AnalyticsSyncStatus (neu FE hien thi) | Hien thi trang thai dong bo tung platform: Facebook (healthy/not_configured), Instagram, TikTok, Twitter, Google, YouTube. Last synced time. "not_configured" neu chua setup | -- |
| AN-40 | Analytics: double click Refresh | 1. Dang nhap -> Analytics -> click Refresh 2 lan nhanh | Lan 1: nut loading/spin. Lan 2: khong trigger (da dang fetch). Chi fetch 1 lan. Khong duplicate request | -- |

### 17.4 FEATURE GATES & PERMISSIONS (AN-41 -> AN-45)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AN-41 | basicAnalytics: khong bi chan voi Free plan | 1. Dang nhap user Free -> Analytics | Trang Analytics load binh thuong. FilterBar + KPI cards + chart + PerformanceTable + TopPosts hien thi. basicAnalytics available tren ALL plans | Free plan |
| AN-42 | advancedAnalytics: bi chan voi Free & PersonalPlus | 1. Dang nhap Free -> Analytics 2. Quan sat phan ben phai | Khong co AnalyticsAiInsights + AnalyticsEfficiencyCard. Thay vao do la upsell card: "Upgrade to Personal Pro" hoac "Unlock advanced analytics" + nut link /pricing. Chart + tables van hien thi | Free / PersonalPlus |
| AN-43 | advancedAnalytics: hien thi voi PersonalPro+ | 1. Dang nhap PersonalPro -> Analytics | AnalyticsAiInsights widget hien thi (nut Generate Insights). AnalyticsEfficiencyCard hien thi (CPA, ROAS, conversion rate). Khong co upsell card | PersonalPro |
| AN-44 | Chua dang nhap truy cap /analytics | 1. Mo browser, chua login 2. Truy cap https://[domain]/analytics | Redirect ve /login. Sau login -> redirect ve /analytics. Khong hien thi du lieu | Chua login |
| AN-45 | Token het han khi load Analytics | 1. Dang nhap -> Analytics 2. Xoa token localStorage 3. F5 reload | API GET /analytics/overview tra 401. FE redirect /login + message **"Session expired"** | Token het han |

### 17.5 EDGE CASES & UI (AN-46 -> AN-48)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AN-46 | Analytics: thay doi date range khi dang fetch | 1. Dang nhap -> Analytics -> chon "90d" (fetch lau) 2. Ngay lap tuc chon "7d" | Request "90d" bi cancel (cancelled flag pattern). Chi request "7d" duoc thuc thi. Data hien thi cua 7d. Khong co race condition. Khong hien thi data cua 90d sau khi 7d da load | Network cham |
| AN-47 | Dashboard: CountUp animation cho KPI so | 1. Dang nhap -> Dashboard 2. Quan sat animation so | KPI so (Published Posts, AI Usage) duoc animated tu 0 -> target value. Dung IntersectionObserver de trigger animation khi scroll into view. Animation muot (ease-out), khong bi giat. Neu so 0 -> khong can animate | -- |
| AN-48 | Analytics: Custom date range fallback | 1. Dang nhap -> Analytics -> FilterBar chon "custom" | **[GHI NHAN]** Custom option fallback ve 30 ngay (chua implement date picker). Data hien thi 30 ngay. Khong crash. Can ghi nhan: co hien thi date picker khi chon custom khong? | -- |


### 17.4 ADMIN DASHBOARD -- Bang dieu khien Admin (AN-49 -> AN-60)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AN-49 | Admin: truy cap Admin Dashboard | 1. Dang nhap Admin -> sidebar -> "Dashboard" (admin section) 2. Quan sat toan bo trang | AdminHeader: "Dashboard". KPI Cards 4: Total Users, Total Workspaces, Total Content, Total Revenue (VND). Quick Actions: Manage Users, Manage Workspaces, View Payments, System Settings. System Info: "Super Admin" / "Full System Access". AdminTopWorkspaces component (charts + table). API: GET /admin/dashboard/summary, charts, top-workspaces | Admin |
| AN-50 | Admin Dashboard KPI: Total Users | 1. Admin -> Dashboard -> quan sat card Total Users | Hien thi tong so users. So khop API /admin/dashboard/summary -> totalUsers. Format so co dau phay | Admin |
| AN-51 | Admin Dashboard KPI: Total Revenue | 1. Admin -> Dashboard -> quan sat card Total Revenue | Hien thi tong doanh thu VND. So khop API -> totalRevenue | Admin |
| AN-52 | AdminTopWorkspaces: Period & Top N filter | 1. Admin -> Dashboard -> AdminTopWorkspaces 2. Chon period: Today/Week/Month/Year/All 3. Chon limit: 10/20/50/100 | Moi filter refetch GET /admin/dashboard/top-workspaces?limit=N&period=X. Charts + table cap nhat | Admin |
| AN-53 | AdminTopWorkspaces: SaaS Revenue vs Ad Spend chart | 1. Admin -> Dashboard -> quan sat chart | Grouped bar: moi workspace 2 cot (SaaS Revenue, Ad Spend). Truc Y VND. Hover tooltip | Admin |
| AN-54 | AdminTopWorkspaces: Ad Revenue vs Ad Spend chart | 1. Admin -> Dashboard -> quan sat chart | Grouped bar so sanh Ad Revenue vs Ad Spend. Phat hien workspace ROAS tot | Admin |
| AN-55 | AdminTopWorkspaces: Performance table | 1. Admin -> Dashboard -> quan sat bang | Bang: Workspace, SaaS Revenue, Ad Spend, Ad Revenue, ROAS, Engagement. Sort theo revenue desc. Workspace name clickable | Admin |
| AN-56 | Admin Dashboard: Quick Actions dieu huong | 1. Admin -> Dashboard -> click Manage Users / Workspaces / Payments / Settings | Link den /admin/users, /admin/workspaces, /admin/payments, /admin/settings. Breadcrumbs dung | Admin |
| AN-57 | Admin Dashboard: Charts endpoint | 1. Admin -> GET /admin/dashboard/charts | Response: userRegistrations[], revenue[], contentCreated[], aiGenerations[], revenue30Day[] (7/30 ngay). Dung cho chart tren Dashboard | Admin |
| AN-58 | User thuong truy cap Admin Dashboard | 1. User thuong -> /admin/dashboard | [Authorize(Roles=Admin)] -> 403. FE "Access denied" hoac redirect | User thuong |
| AN-59 | Admin Dashboard: loading skeleton | 1. Admin -> DevTools Slow 3G -> Dashboard | KPI cards skeleton + charts placeholder. Sau load -> data that | Admin |
| AN-60 | Admin Dashboard: empty state (he thong moi) | 1. Admin -> Dashboard (0 users, 0 workspaces) | KPI = 0. Charts flat/empty. Tables "No data". Khong crash | He thong moi |

### 17.5 ADMIN ANALYTICS -- Bao cao phan tich Admin (AN-61 -> AN-73)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AN-61 | Admin: truy cap Admin Analytics | 1. Admin -> sidebar -> "Analytics" (admin section) 2. Quan sat trang | Loading "Dang tai du lieu bao cao...". Sau load: date range dropdown (7/30/90d), Refresh, Export CSV. Platform KPIs 5 cards: Impressions, Clicks, CTR, Ad Spend, Est. Revenue. System Stats 4 cards. Charts: Top Workspaces by Revenue, Spend vs Revenue, ROAS, Engagement. Workspace Performance table. Top Campaigns table | Admin |
| AN-62 | Admin Analytics: Date range dropdown | 1. Admin -> Analytics -> chon 7d/30d/90d | Moi filter refetch overview + workspace-comparison + top-campaigns. Charts + tables cap nhat | Admin |
| AN-63 | Admin Analytics: Platform KPIs | 1. Admin -> Analytics -> quan sat 5 KPI cards | Impressions (K/M/B format), Clicks, CTR (%), Ad Spend (VND), Est. Revenue (VND). So khop GET /admin/analytics/overview -> totals | Admin, co data |
| AN-64 | Admin Analytics: System Stats | 1. Admin -> Analytics -> quan sat 4 System Stats cards | Total Users, Workspaces, Content Items, Total Revenue (toan he thong). So khop API -> systemStats | Admin |
| AN-65 | Admin Analytics: Top Workspaces by Revenue chart | 1. Admin -> Analytics -> quan sat chart | Horizontal bar chart, top 5/10/20/50. Hover -> workspace name + revenue. Moi workspace mau rieng | Admin |
| AN-66 | Admin Analytics: Spend vs Revenue chart | 1. Admin -> Analytics -> quan sat chart | Grouped bar: moi workspace 2 cot (spend xanh, revenue cam). Hover tooltip | Admin |
| AN-67 | Admin Analytics: ROAS by Workspace chart | 1. Admin -> Analytics -> quan sat chart | Bar chart: ROAS value. ROAS>1: xanh (loi nhuan). ROAS<1: do (lo) | Admin |
| AN-68 | Admin Analytics: Engagement by Workspace chart | 1. Admin -> Analytics -> quan sat chart | Bar chart: engagement per workspace, sap xep desc | Admin |
| AN-69 | Admin Analytics: Workspace Performance table | 1. Admin -> Analytics -> quan sat bang | Bang: Posts, Campaigns, Impressions, Clicks, CTR, Spend, Revenue, ROAS, Status. Sortable. StatusBadge Active/Inactive | Admin |
| AN-70 | Admin Analytics: Top Campaigns table | 1. Admin -> Analytics -> quan sat bang (conditional) | Campaign name, Status badge, Impressions, Clicks, CTR, Spend, CPA, ROAS. Neu khong co data -> bang an | Admin, co campaigns |
| AN-71 | Admin Analytics: Export CSV server-side | 1. Admin -> Analytics -> click "Export CSV" | GET /admin/analytics/export?from=&to=. Tra file CSV: admin-report-YYYYMMDD-YYYYMMDD.csv. 2 sections: Summary + Per Workspace. Text escape dung | Admin |
| AN-72 | Admin Analytics: Export CSV khong co data | 1. Admin -> Analytics (he thong moi) -> Export | Van tai CSV, section 2 trong, section 1 = 0. Khong crash | Admin |
| AN-73 | Admin Analytics: Chart animation sequential | 1. Admin -> Analytics -> load trang | Charts staggered animation (animationDelay). isAnimating state fade. Hover tooltip sau animation | Admin |

**Module:** ANALYTICS & DASHBOARD | **Total:** 73 cases | **Pages:** `/dashboard`, `/analytics`, `/admin/dashboard`, `/admin/analytics` | **API:** GET `/analytics/overview`, `/analytics/time-series`, `/analytics/channel-breakdown`, `/analytics/campaign-breakdown`, `/analytics/top-posts`, `/analytics/sync-status`, `/analytics/usage-breakdown`, `/analytics/ai-recommendations`, `/analytics/audience`, `/dashboard/summary`, `/workspace-dashboard/summary`, `/admin/dashboard/summary`, `/admin/dashboard/charts`, `/admin/dashboard/top-workspaces`, `/admin/analytics/overview`, `/admin/analytics/workspace-comparison`, `/admin/analytics/top-campaigns`, `/admin/analytics/export`



---



## SHEET 18/20: NOTIFICATION -- Thong bao (45 cases)

| **Feature** | Notification -- He thong thong bao in-app: bell icon + badge, dropdown, notifications page, mark read, delete |
|---|---|
| **Test requirement** | Header Notification Bell: `notifications_active` icon + unread badge (do, max "9+"), polling 30s GET `/notifications/unread-count`; Bell Dropdown: header "Notifications" + "Mark all read" (neu co unread), body scrollable max-h-72, 5 notifications gan nhat (icon type-dependent, title line-clamp-1, time-ago, unread dot xanh), empty state "No notifications yet", footer "View all notifications" link; Notifications page `/notifications`: breadcrumbs, filter tabs All/Unread (pill toggle + count badges), list voi motion stagger, pagination 20/page, skeleton loading (5 cards), empty states (All: "No notifications yet", Unread: "All caught up!"), click item -> mark read + detail modal (type icon, title, date, full message, close button), delete button (appear on hover), mark all read (toast + optimistic update); Notification Types: SystemUpdate (he thong/publishing fail), PostScheduled (lien lich dang bai), ApprovalNeeded (can duyet, planned), AiSuggestion (AI goi y, planned), PerformanceAlert (canh bao hieu suat, planned); Notification Model: id, profileId, workspaceId, title (max 255), message, type (enum int), targetId?, targetType?, isRead (default false), isDeleted (soft delete), createdAt |
| **Pages** | `/notifications` |
| **API** | GET `/notifications?page=&pageSize=`, GET `/notifications/{id}`, POST `/notifications/{id}/mark-read`, POST `/notifications/mark-all-read`, GET `/notifications/unread-count`, DELETE `/notifications/{id}` |
| **Model** | `Notification` (id, profileId, workspaceId, title, message, type: NotificationTypeEnum, targetId?, targetType?, isRead, isDeleted, createdAt) |

### 18.1 NOTIFICATION BELL & DROPDOWN -- Chuong thong bao (NT-01 -> NT-15)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| NT-01 | Bell icon hien thi tren Header | 1. Truy cap https://[domain]/login 2. Nhap Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Quan sat Header | Icon `notifications_active` xuat hien o goc tren phai Header, canh avatar/workspace selector. Icon co mau outline hoac primary. Neu chua co unread -> khong co badge. Click vao icon -> mo dropdown | Da dang nhap |
| NT-02 | Bell badge hien thi unread count | 1. Dang nhap user co 3 thong bao chua doc 2. Quan sat bell icon | Badge do (`bg-danger-red`) hien thi so "3". Vi tri: `-top-0.5 -right-0.5`. Badge nho (w-4 h-4), text trang, font-xs. Neu so > 9 -> hien thi "9+" | Co 3 unread |
| NT-03 | Bell badge khong hien thi khi unread = 0 | 1. Dang nhap user da doc het thong bao 2. Quan sat bell icon | Khong co badge. Chi hien thi icon `notifications_active`. Khong co so 0 | unreadCount = 0 |
| NT-04 | Mo bell dropdown | 1. Dang nhap test@example.com / Pass1234 2. Click bell icon 3. Quan sat dropdown | Dropdown mo: rong `w-80`, nam duoi bell, canh phai. Header: title "Notifications" + nut "Mark all read" (chi hien thi neu unread > 0). Body: danh sach toi da 5 thong bao gan nhat (fetch GET /notifications?page=1&pageSize=5). Footer: link "View all notifications" -> /notifications. Click ngoai dropdown -> dong | Co 3+ notifications |
| NT-05 | Dropdown: loading state | 1. Dang nhap -> DevTools Slow 3G 2. Click bell icon | Hien thi spinner xoay (w-5 h-5 border-2) trong dropdown body. Sau khi fetch xong -> danh sach hien thi. Khong crash | Network cham |
| NT-06 | Dropdown: empty state | 1. Dang nhap user chua co thong bao nao 2. Click bell icon | Hien thi icon `notifications_off` (outline/20, text-2xl) + text "No notifications yet". Khong co "Mark all read". Footer van co "View all notifications" link | 0 notifications |
| NT-07 | Dropdown: hien thi notification items | 1. Dang nhap -> co 5 thong bao 2. Click bell -> quan sat item | Moi item: icon (type-dependent, nen tron mau nhat), title (line-clamp-1, font-semibold), time-ago (text-xs, text-outline), unread dot (`w-1.5 h-1.5 bg-primary rounded-full`, chi hien thi neu isRead=false). Hover item -> bg-surface-container-low. Click item -> navigate /notifications + dong dropdown | Co thong bao nhieu type |
| NT-08 | Dropdown: icon hien thi dung theo type | 1. Dang nhap -> co thong bao nhieu type 2. Click bell -> quan sat icon | PostScheduled (CONTENT_PUBLISHED): icon `task_alt`, mau success-green. AiSuggestion: icon `auto_awesome`, mau secondary. PerformanceAlert (CAMPAIGN): icon `campaign`, mau primary. ApprovalNeeded (APPROVAL): icon `approval`, mau amber. SystemUpdate (SYSTEM): icon `notifications`, mau primary | Co nhieu type |
| NT-09 | Dropdown: time-ago hien thi dung | 1. Dang nhap -> click bell 2. Quan sat time-ago text | Vua tao: "Just now". 1-59 phut: "Xm ago". 1-23 gio: "Xh ago". 1-6 ngay: "Xd ago". > 7 ngay: formatted date (dd MMM). Cap nhat moi 60s. Khong hien thi "NaN" hoac undefined | Co thong bao nhieu thoi gian |
| NT-10 | Dropdown: Mark all read | 1. Dang nhap -> co 3 thong bao unread 2. Click bell -> click "Mark all read" 3. Quan sat | Nut chuyen "Marking..." + spinner. API POST /notifications/mark-all-read. Sau thanh cong: tat ca item trong dropdown -> isRead=true (mat unread dot). Bell badge bien mat. unreadCount = 0. Nut "Mark all read" bien mat khoi dropdown header. Toast co the hien thi **"All notifications marked as read"** | Co 3 unread |
| NT-11 | Dropdown: Mark all read khi khong co unread | 1. Dang nhap -> da doc het 2. Click bell -> quan sat | Nut "Mark all read" khong hien thi (dieu kien: unreadCount > 0). Chi co title "Notifications" | unreadCount = 0 |
| NT-12 | Dropdown: click item navigate | 1. Dang nhap -> click bell -> click 1 notification item 2. Quan sat | Browser navigate toi /notifications. Dropdown dong. Tren trang notifications -> item duoc click chua duoc mark read tu dong (phai click tren list de mark read) | -- |
| NT-13 | Dropdown: dong khi click ngoai | 1. Dang nhap -> mo bell dropdown 2. Click ra ngoai vung dropdown (vd: sidebar, main content) | Dropdown dong (click outside via `notifRef`). Khong crash. Mo lai -> van hien thi data cu (khong re-fetch ngay) | Dropdown dang mo |
| NT-14 | Dropdown: dong khi click bell lan 2 | 1. Dang nhap -> mo bell dropdown 2. Click bell icon lan nua | Dropdown dong (toggle). Khong re-fetch API. Mo lan 3 -> fetch lai data moi | Dropdown dang mo |
| NT-15 | Polling 30s: badge tu cap nhat | 1. Dang nhap -> co 2 unread 2. Mo tab khac -> tao them notification moi (API) 3. Quay lai tab cu, doi < 30s | Trong vong 30s, `setInterval` goi GET /notifications/unread-count. Badge cap nhat tu 2 -> 3. Khong can F5. Neu co notification moi -> badge xuat hien (neu dang o 0). Polling chay lien tuc ngay ca khi dropdown dang mo | Co notification moi |

### 18.2 NOTIFICATIONS PAGE -- Trang danh sach thong bao (NT-16 -> NT-30)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| NT-16 | Truy cap Notifications page | 1. Dang nhap test@example.com / Pass1234 2. Sidebar -> "Notifications" (icon notifications) hoac tu dropdown -> "View all notifications" 3. Quan sat toan bo trang | Breadcrumbs: "Dashboard > Notifications". Page title "Notifications" + unread count badge (neu > 0). Filter tabs: "All" + count, "Unread" + count (pill toggle). Danh sach notification list voi image lazy animation. Pagination footer (neu > 20 items). Nut "Mark all read" (animation, chi hien thi khi unread > 0) | Co 25+ notifications |
| NT-17 | Notification list: hien thi item | 1. Dang nhap -> Notifications -> quan sat list item | Moi item: type icon (nen tron mau nhat), title (truncate neu dai), message (line-clamp-2, text-xs), time-ago, unread dot (animate pulse, bg-primary). Delete button (icon close/thung rac, chi hien thi khi hover: `opacity-0 group-hover:opacity-100`). Click item -> mark read + mo detail modal. Hover item -> bg-surface-container-low | Co notifications |
| NT-18 | Filter tab: All | 1. Dang nhap -> Notifications 2. Tab "All" dang active (mac dinh) 3. Quan sat | Hien thi tat ca notifications (da doc + chua doc). Tab "All" co badge dem tong so. Tab "Unread" co badge dem so chua doc | -- |
| NT-19 | Filter tab: Unread | 1. Dang nhap -> Notifications 2. Click tab "Unread" 3. Quan sat | Chi hien thi notifications co isRead=false. Tab "Unread" active (highlight). Neu khong co unread -> empty state "All caught up!" + icon `mark_email_read`. Filter client-side (khong re-fetch API) | Co ca read + unread |
| NT-20 | Empty state: All filter | 1. Dang nhap user chua co notification nao 2. Notifications | Hien thi icon `notifications_off` + text "No notifications yet" + subtext "When you receive notifications, they'll appear here." Khong co pagination. Filter tabs van hien thi (All=0, Unread=0) | 0 notifications |
| NT-21 | Empty state: Unread filter | 1. Dang nhap -> da doc het notification 2. Notifications -> tab "Unread" | Hien thi icon `mark_email_read` + text "All caught up!" + subtext "You've read all your notifications." Khac voi empty state All filter | unreadCount = 0, co read notifications |
| NT-22 | Loading skeleton | 1. Dang nhap -> DevTools Slow 3G -> Notifications | Hien thi 5 skeleton placeholder cards (`animate-pulse`): khung xam cho icon, title, message, time-ago. Sau khi load -> data that. Khong crash | -- |
| NT-23 | Pagination: hien thi dung | 1. Dang nhap -> Notifications co 45 items 2. Quan sat pagination footer | Hien thi "Page 1 of 3". Nut prev (<) disabled o page 1. Nut next (>) enabled. So trang: 1, 2, 3. Page 1 highlight. Click page 2 -> fetch GET /notifications?page=2&pageSize=20. List cap nhat. Scroll to top? | 45 notifications |
| NT-24 | Pagination: trang cuoi | 1. Dang nhap -> Notifications -> page 3 (trang cuoi) 2. Quan sat | Nut next disabled. Nut prev enabled. Danh sach hien thi 5 items (45 - 2*20 = 5). Khong crash | 45 notifications |
| NT-25 | Pagination: chi 1 trang | 1. Dang nhap -> Notifications co 10 items 2. Quan sat | Pagination footer khong hien thi hoac hien thi "Page 1 of 1" voi ca 2 nut prev/next disabled | 10 notifications |
| NT-26 | Click item: mark read + mo detail modal | 1. Dang nhap -> Notifications 2. Click 1 notification unread | Goi POST /notifications/{id}/mark-read -> item isRead=true (mat unread dot). Unread count giam 1. Goi GET /notifications/{id} -> mo detail modal. Modal hien thi: type icon + title + formatted date + full message (khong truncate). Backdrop `bg-black/40 backdrop-blur-sm`. Close button (X) + click ngoai de dong | Co notification unread |
| NT-27 | Detail modal: dong | 1. Dang nhap -> Notifications -> mo detail modal 2. Click nut Close (X) hoac click ngoai modal | Modal dong. List notifications phia sau hien thi lai. Item vua click da mat unread dot. selected notification state cleared | Detail modal dang mo |
| NT-28 | Detail modal: loading / not found | 1. Dang nhap -> Notifications 2. Click item -> API GET /notifications/{id} cham 3. Gia lap API 404 | Loading: skeleton placeholder trong modal. 404: "Notification not found" hoac message loi. Modal co nut Close | API cham hoac 404 |
| NT-29 | Delete notification tu list | 1. Dang nhap -> Notifications 2. Hover 1 item -> click nut Delete (thung rac) 3. Quan sat | Goi DELETE /notifications/{id}. Item bi xoa khoi list (remove khoi local state). Neu item chua doc -> unreadCount giam 1 (Math.max(0, unreadCount-1)). Pagination cap nhat (neu can). Toast co the hien thi **"Notification deleted"**. Khong can confirm dialog | Co notification |
| NT-30 | Delete notification: double click | 1. Dang nhap -> Notifications -> hover item 2. Click Delete 2 lan nhanh | Lan 1: goi API DELETE. Lan 2: item da bi xoa khoi state -> khong con de click (hoac disabled). Chi goi DELETE 1 lan. Khong crash | -- |

### 18.3 MARK READ / MARK ALL READ (NT-31 -> NT-37)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| NT-31 | Mark single notification as read | 1. Dang nhap -> Notifications 2. Click 1 item unread | POST /notifications/{id}/mark-read -> BE set IsRead=true. Tra ve true. FE: item's isRead=true (mat unread dot). Unread count giam 1. Detail modal mo (goi them GET /notifications/{id}) | Notification unread |
| NT-32 | Mark read notification da doc roi | 1. Dang nhap -> Notifications -> item da read 2. Click item | POST /notifications/{id}/mark-read -> BE set IsRead=true (no-op). FE van xu ly binh thuong (isRead da true). Detail modal mo. Khong crash | Notification da read |
| NT-33 | Mark read notification cua workspace khac | 1. Dang nhap WS A -> lay ID notification cua WS B 2. Goi POST /notifications/{WS_B_ID}/mark-read | BE check: notification.WorkspaceId != WS A -> tra 404 **"Notification not found"**. Khong bi expose notification workspace khac | Workspace B notification |
| NT-34 | Mark all read tu Notifications page | 1. Dang nhap -> Notifications -> co 5 unread 2. Click nut "Mark all read" (header page) 3. Quan sat | Nut hien thi spinner "Marking...". POST /notifications/mark-all-read. Sau thanh cong: tat ca item -> isRead=true (mat unread dot). Unread count = 0. Nut "Mark all read" bien mat (motion animation). Badge tren header cung ve 0 (polling). Toast: **"All notifications marked as read"** | Co 5 unread |
| NT-35 | Mark all read khi da doc het | 1. Dang nhap -> da doc het notification 2. Notifications | Nut "Mark all read" khong hien thi (unreadCount = 0). Khong crash | unreadCount = 0 |
| NT-36 | Mark all read optimistic update | 1. Dang nhap -> Notifications -> co 3 unread 2. Mark all read 3. Quan sat UI truoc khi API response | FE optimistic: tat ca item chuyen isRead=true + unreadCount=0 NGAY LAP TUC (khong doi API). Nut loading "Marking...". Neu API fail -> rollback? **[GHI NHAN]** Can ghi nhan: FE co rollback khi API fail khong? Hay giu nguyen optimistic state? | Co 3 unread |
| NT-37 | Mark all read: cross-workspace isolation | 1. WS A co 3 unread, WS B co 5 unread 2. WS A -> Notifications -> Mark all read 3. Switch WS B -> Notifications | WS B van con 5 unread. BE MarkAllAsReadByWorkspaceIdAsync chi mark cho WS A. WS B khong bi anh huong | 2 workspace |

### 18.4 UNREAD COUNT & POLLING (NT-38 -> NT-41)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| NT-38 | Unread count API tra ve dung | 1. Dang nhap -> co 3 unread, 2 read 2. Goi GET /notifications/unread-count | Response: `{ success: true, data: { count: 3 } }`. Chi dem IsRead=false AND IsDeleted=false. Read + Deleted khong tinh | Co 3 unread |
| NT-39 | Unread count = 0 | 1. Dang nhap -> da doc het hoac chua co notification 2. GET /notifications/unread-count | Response: `{ count: 0 }`. Bell khong co badge. Dashboard unreadNotificationCount = 0 | 0 unread |
| NT-40 | Polling 30s: chi chay khi dang nhap | 1. Dang nhap -> bell badge = 3 2. Xoa localStorage token (logout) 3. Doi 30s | setInterval van chay nhung API tra 401 -> apiClient xu ly redirect hoac ignore. Badge khong cap nhat. Khong crash, khong infinite loop | Vua logout |
| NT-41 | Polling 30s: clear khi component unmount | 1. Dang nhap -> Dashboard (bell badge = 3) 2. Navigate sang trang khac khong co Header (neu co) | Neu Header unmount -> `clearInterval` duoc goi. Khong bi memory leak. Polling chi chay khi Header component mounted | -- |

### 18.5 NOTIFICATION TRIGGERS -- Nguon tao thong bao (NT-42 -> NT-45)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| NT-42 | Notification tao khi schedule content | 1. Dang nhap -> Calendar -> tao schedule cho 1 Approved content 2. Kiem tra Notifications | Notification moi: type=PostScheduled, title lien quan den content, message chua ten content/thoi gian. TargetId = scheduleId, TargetType = "content_schedule". IsRead = false. Xuat hien trong bell + notifications list | Tao schedule thanh cong |
| NT-43 | Notification tao khi publish fail | 1. Dang nhap -> tao schedule -> de worker fail (sai integration token) 2. Doi worker retry 3 lan fail 3. Kiem tra Notifications | Notification moi: type=SystemUpdate, title/body: **"Content publishing failed"** hoac tuong tu. Message ghi ro reason fail (vd: "Invalid token"). TargetId = scheduleId. Notification chi gui cho profile lien quan | Schedule fail |
| NT-44 | Manager duoc thong bao khi content can duyet | 1. ContentCreator tao content -> submit PendingApproval 2. Dang nhap Manager -> kiem tra Notifications | **[GHI NHAN]** ApprovalNeeded (type=0) duoc dinh nghia trong enum nhung chua co code tao. Can ghi nhan: Manager co nhan duoc notification khong? Hay phai tu vao Approvals page? | Content PendingApproval |
| NT-45 | AI goi y tao notification | 1. Dang nhap -> AI tao suggestion 2. Kiem tra Notifications | **[GHI NHAN]** AiSuggestion (type=3) duoc dinh nghia nhung chua co code tao. Can ghi nhan thuc te: co notification AI suggestion khong? | AI suggestion duoc tao |


### 18.6 ADMIN BROADCAST -- Phat thong bao Admin (NT-46 -> NT-50)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| NT-46 | Admin broadcast notification den tat ca user | 1. Dang nhap Admin 2. Vao /admin/broadcast -> nhap title + message 3. Click Send (excludeAdmins=false) 4. Dang nhap user thuong -> Notifications | POST /admin/notifications/broadcast. Notification moi: type=SystemUpdate, title/message nhu input. WorkspaceId=Guid.Empty. Xuat hien trong bell + list cua TUNG user thuong. BE lap qua all users -> tim profile dau tien -> tao notification | Admin |
| NT-47 | Admin broadcast exclude admins | 1. Admin -> broadcast voi excludeAdmins=true 2. Kiem tra notification Admin khac + user thuong | Admin khac: KHONG nhan. User thuong: CO nhan. BE: admin.Role != Admin -> chi tao cho non-admin | Admin |
| NT-48 | Admin broadcast: user co nhieu profile | 1. User co 2 profile (2 workspace) 2. Admin broadcast 3. Kiem tra notification | BE chi lay profiles.FirstOrDefault() -> tao 1 notification. Profile thu 2 khong co. **[GHI NHAN]** Neu profile dau khong phai active -> user co the khong thay | User 2 profile |
| NT-49 | Admin broadcast: validation title trong | 1. Admin -> broadcast voi title="" | **[GHI NHAN]** Model co [Required] attribute -> EF throw validation error. FE co chan khong? Can ghi nhan thuc te | -- |
| NT-50 | Admin broadcast: user chua co profile | 1. Admin broadcast 2. User B chua tung login | profiles.FirstOrDefault() = null -> bo qua. User chi nhan notification sau khi login + tao profile | User chua co profile |

**Module:** NOTIFICATION | **Total:** 50 cases | **Pages:** `/notifications` | **API:** GET `/notifications`, GET `/notifications/{id}`, POST `/notifications/{id}/mark-read`, POST `/notifications/mark-all-read`, GET `/notifications/unread-count`, DELETE `/notifications/{id}`, POST `/admin/notifications/broadcast`



---


## SHEET 19/20: AUTOMATION -- AI Content Automation (78 cases)

| **Feature** | Automation -- Tu dong hoa tao noi dung AI: import CSV/Google Sheets, tao plan thu cong, AI generation (text/image/video), credit reserve/settle/release, approval & scheduling, clone template, auto-approve |
|---|---|
| **Test requirement** | Automation page `/automation`: Header "AI Campaign Autopilot", Summary Cards (Total Plans, Awaiting Confirmation, Processing, Est. Credits), Plans List (name, total items, status badge, valid/total ratio), Import Modal (CSV upload, plan name, timezone selection: Vietnam UTC+7, Singapore UTC+8, Japan UTC+9, UTC), CSV template download (UTF-8 BOM with header + 2 example rows), Plan Detail Slide-out Panel: header with name/filename/timezone/buttons (Confirm, Cancel, Approve, Retry theo status), polling 3s khi Generating, item cards (row index, topic, platform, content type, brand, date, credits used/estimated, generated content preview, validation errors, last error, action buttons), Advanced Operations (Clone as Template, Auto-approve toggle), Performance cards (published, impressions, engagement, CTR), Page Selection Modal (multi-page approval with checkboxes), Edit Item Modal (brand, product, topic, platform, content type, date/time, objective, tone, CTA, notes), TikTok content type warning; CSV Import validation: brand exist in workspace, product belong to brand, topic required, platform facebook/instagram/tiktok (aliases fb/ig/tik tok), TikTok requires Video/Auto, date/time future, max 10MB; Google Sheets import: HTTPS docs.google.com URL, must contain /d/ segment, link sharing enabled; AI Generation flow: GeneratingText (1 credit) -> GeneratingMedia (Image=5 credits, Video=20 credits) -> AwaitingApproval, BackgroundService polling 250ms active/5s idle, video async start/poll resume; Approval flow: find active SocialIntegration for brand+platform, single -> auto-select, multiple -> NeedsAttention until user picks pages, create ContentCalendar entries; Plan lifecycle: Uploaded -> Validating -> AwaitingConfirmation -> (Confirm) Generating -> (auto/manual) AwaitingApproval -> Scheduling -> Completed/PartiallyFailed/Failed; Cancel: reject in-progress items, release credits; Retry: reset GenerationFailed items to Pending, re-reserve credits; Clone: shiftDays 1-3650, TemplateSourcePlanId reference |
| **Pages** | `/automation` |
| **API** | GET `/automation-plans`, GET `/automation-plans/{id}`, POST `/automation-plans` (JSON create), POST `/automation-plans/import-csv` (multipart), POST `/automation-plans/import-google-sheet`, POST `/automation-plans/{id}/confirm`, POST `/automation-plans/{id}/retry`, POST `/automation-plans/{id}/cancel`, POST `/automation-plans/{id}/clone`, PUT `/automation-plans/{id}/auto-approve`, PUT `/automation-plans/{id}/items/{itemId}`, GET `/automation-plans/{id}/performance`, POST `/automation-plans/{id}/approve`, GET `/automation-plans/{id}/items/{itemId}/targets`, POST `/automation-plans/{id}/items/{itemId}/approve-targets`, POST `/automation-plans/{id}/items/{itemId}/reject` |
| **Model** | `AutomationPlan` (id, workspaceId, profileId, name, sourceFileName, timezone, status: AutomationPlanStatusEnum, totalItems, validItems, failedItems, estimatedCredits, reservedCredits, usedCredits, releasedCredits, autoApprove, templateSourcePlanId, isDeleted, createdAt, updatedAt, confirmedAt), `AutomationItem` (id, automationPlanId, rowIndex, platform, idempotencyKey: SHA256, brandId, productId, contentId, contentCalendarId, topic, objective, requestedContentType: AutomationContentTypeEnum, tone, cta, notes, scheduledAt, status: AutomationItemStatusEnum, estimatedCredits, usedCredits, validationErrors: jsonb, sourceJson, generationAttemptCount, lastError, videoJobId, videoProvider) |
| **Enums** | PlanStatus: Uploaded=0, Validating=1, AwaitingConfirmation=2, Generating=3, AwaitingApproval=4, Scheduling=5, Completed=6, PartiallyFailed=7, Failed=8, Cancelled=9. ItemStatus: Pending=0, NeedsAttention=1, GeneratingText=2, GeneratingMedia=3, QualityCheck=4, AwaitingApproval=5, Approved=6, Scheduled=7, Published=8, Rejected=9, GenerationFailed=10, PublishFailed=11. ContentType: Text=0, Image=1, Video=2, Auto=3 |
| **Background** | GenerationBackgroundService (250ms active / 5s idle polling), OperationsBackgroundService (15s polling, up to 5 auto-approve plans + up to 100 item status sync per cycle) |

### 19.1 AUTOMATION PAGE & LIST -- Trang Automation & danh sach plan (AT-01 -> AT-08)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-01 | Truy cap Automation page voi plan da co | 1. Truy cap https://[domain]/login 2. Nhap Email: test@example.com, Password: Pass1234 3. Click Sign In 4. Sidebar -> "AI Automation" (icon auto_awesome_motion) 5. Quan sat toan bo trang | Header: "AI Campaign Autopilot" + subtitle "Content Automation Plans". Summary Cards: Total Plans, Awaiting Confirmation, Processing, Est. Credits. Plans List: moi plan hien thi name, total items, status badge (mau theo status), valid/total ratio (vd: "45/50 valid"). Nut "+ New Automation" / "Import CSV" goc tren phai. Plan cards clickable -> mo detail slide-out panel | Co it nhat 2 automation plans |
| AT-02 | Summary Cards hien thi dung so lieu | 1. Dang nhap -> Automation -> co plans nhieu status 2. Dem thu cong + so voi cards | Total Plans = tong so plans (khong tinh deleted). Awaiting Confirmation = count status=2. Processing = count status=3 (Generating). Est. Credits = sum estimatedCredits cua plans dang active. Cards cap nhat khi plans thay doi | Co plans nhieu status |
| AT-03 | Plans List: status badge hien thi dung mau | 1. Dang nhap -> Automation -> quan sat status badges | AwaitingConfirmation: badge xanh duong, text "Awaiting Confirmation". Generating: badge cam/vang + spinner animation. AwaitingApproval: badge tim. Completed: badge xanh la. PartiallyFailed: badge cam. Failed: badge do. Cancelled: badge xam. Moi badge co dot mau tuong ung | Co plans nhieu status |
| AT-04 | Plans List: empty state | 1. Dang nhap workspace moi, chua co automation plan 2. Automation | Hien thi empty state: icon auto_awesome_motion, text "No automation plans yet". Subtext mo ta tinh nang. Nut "Import CSV" hoac "Create Plan" noi bat. Summary cards hien thi 0. Khong crash | Chua co plan |
| AT-05 | Click plan card -> mo detail slide-out panel | 1. Dang nhap -> Automation -> click 1 plan card 2. Quan sat panel | Slide-out panel mo tu ben phai (hoac modal). Header: plan name, source file name (neu CSV), timezone, status badge. Buttons theo status: Cancel (neu Generating), Confirm (neu AwaitingConfirmation), Approve & Schedule (neu AwaitingApproval), Retry Failed (neu co GenerationFailed). Summary: valid/needs-fix/estimated credits. Advanced Operations: Clone (Use as Template), Auto-approve toggle. Performance cards (published, impressions, engagement, CTR). Item cards list: row index, topic, platform, content type, brand, date, credits used/estimated, generated preview, validation errors, last error. Dong panel -> click X hoac click ngoai | Click plan |
| AT-06 | Plan detail: polling 3s khi Generating | 1. Dang nhap -> Automation -> chon plan dang Generating 2. Mo detail panel -> quan sat | Panel tu dong refresh moi 3 giay (polling). Item cards cap nhat status (Pending -> GeneratingText -> GeneratingMedia -> AwaitingApproval). Credits used tang dan. Plan status co the chuyen sang AwaitingApproval. Khi plan khong con Generating -> polling dung | Plan Generating |
| AT-07 | Plan detail: Performance cards | 1. Dang nhap -> Automation -> chon plan da Completed 2. Quan sat Performance section | 4 cards: Published (so items da publish), Impressions (tong), Engagement (tong), CTR (avg %). So khop API GET /automation-plans/{id}/performance. Neu plan moi chua co data -> hien thi 0 | Plan Completed |
| AT-08 | Plan detail: dong panel | 1. Dang nhap -> Automation -> mo detail panel 2. Click nut X hoac click ngoai panel | Panel dong. Plan list hien thi lai. Selected plan state cleared. Mo plan khac -> panel hien thi dung plan duoc chon | Panel dang mo |

### 19.2 CSV IMPORT -- Import du lieu CSV (AT-09 -> AT-18)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-09 | Mo Import CSV Modal | 1. Dang nhap test@example.com / Pass1234 2. Automation -> click "+ New Automation" -> chon "Import CSV" (hoac nut Import CSV) 3. Quan sat modal | Import CSV Modal mo. Hien thi: input Plan Name (required, max 200), dropdown Timezone (Vietnam UTC+7, Singapore UTC+8, Japan UTC+9, UTC, default UTC), file upload zone (drag & drop hoac click browse, accept .csv, max 10MB). Nut "Download CSV Template" de tai file mau. Nut "Import" (disabled khi chua chon file + name). Nut Cancel | -- |
| AT-10 | Download CSV template | 1. Dang nhap -> Import CSV Modal 2. Click "Download CSV Template" | Tai file CSV UTF-8 BOM: header + 2 dong du lieu mau. Header: Brand, Product, Topic, Objective, Platforms, ContentType, Tone, CTA, Notes, Date, Time. Dong 1: "Brand A", "Product X", "Chu de vi du 1", "Awareness", "facebook,instagram", "Auto", "Chuyen nghiep", "Mua ngay", "Ghi chu", "2026-08-01", "10:00". Dong 2: tuong tu. File mo duoc trong Excel, tieng Viet hien thi dung | -- |
| AT-11 | Import CSV thanh cong | 1. Dang nhap -> Import CSV Modal 2. Plan Name: "Chien dich T8", Timezone: Vietnam (UTC+7) 3. Chon file CSV hop le (3 rows: header + 2 data) 4. Click Import 5. Quan sat | Nut Import chuyen spinner "Importing...". POST /automation-plans/import-csv (multipart/form-data). Sau thanh cong: modal dong. Plan moi xuat hien trong list voi status AwaitingConfirmation (hoac Uploaded -> Validating -> AwaitingConfirmation). Summary cards cap nhat. Toast: **"Plan imported successfully"**. Mo detail -> thay items da duoc split theo platform | File CSV hop le |
| AT-12 | Import CSV: file rong / khong phai CSV | 1. Dang nhap -> Import CSV Modal 2. Chon file .txt hoac file CSV rong (0 byte) | BE validate: file extension phai la .csv + length > 0. Tra loi 400: **"Invalid CSV file"**. Toast hien thi message loi. Modal khong dong. Co the chon lai file khac | File sai |
| AT-13 | Import CSV: thieu header hoac khong co data row | 1. Dang nhap -> Import CSV chi co header, khong co data | BE validate: CSV phai co it nhat 1 header + 1 data row. Tra loi 400: **"CSV must contain at least one data row"**. Toast loi. Modal giu nguyen | CSV thieu data |
| AT-14 | Import CSV: validation brand khong ton tai | 1. CSV co row voi Brand="BrandKhongTonTai" 2. Import | BE validate: brand phai ton tai trong workspace. Item bi danh dau NeedsAttention + validationErrors: **"Brand 'BrandKhongTonTai' not found"**. ValidItems khong tinh item nay. FailedItems tang. Plan van duoc tao (AwaitingConfirmation) nhung item bi loi hien thi trong detail | Brand khong ton tai |
| AT-15 | Import CSV: validation product khong thuoc brand | 1. CSV: Brand="Brand A", Product="Product cua Brand B" 2. Import | BE validate: product phai thuoc brand. Item bi danh dau NeedsAttention + validationErrors: **"Product does not belong to brand"**. Tuong tu AT-14 | Product sai brand |
| AT-16 | Import CSV: validation TikTok + Text | 1. CSV row: Platforms="tiktok", ContentType="Text" 2. Import | BE validate: TikTok requires Video hoac Auto. Item bi danh dau NeedsAttention + validationErrors: **"TikTok requires Video or Auto content type"**. Item khong the confirm (phai edit truoc) | TikTok + Text |
| AT-17 | Import CSV: validation scheduled date qua khu | 1. CSV row: Date="2024-01-01", Time="10:00" 2. Import | BE validate: scheduledAt must be future. Item bi danh dau NeedsAttention + validationErrors: **"Scheduled date must be in the future"**. Plan van duoc tao | Date qua khu |
| AT-18 | Import CSV: nhieu platform tren 1 row | 1. CSV row: Platforms="facebook, instagram, tiktok" 2. Import | BE split 1 row thanh 3 items (1 per platform). Moi item co rowIndex trung nhau, platform khac nhau, idempotencyKey rieng. TotalItems = 3 (neu 1 row). Credit estimate: Facebook+Instagram+Text = 1+1 credits, TikTok+Auto = 21 credits | CSV 1 row, 3 platforms |

### 19.3 MANUAL CREATE & EDIT ITEMS -- Tao plan thu cong & sua item (AT-19 -> AT-25)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-19 | Tao plan thu cong thanh cong | 1. Dang nhap -> Automation -> "+ New Automation" -> "Create Manually" (neu co) hoac goi POST /automation-plans 2. Dien form: name, timezone, rows[] (brandId, topic, platforms, contentType, scheduledAt) 3. Click Create | POST /automation-plans voi JSON body. Plan duoc tao, status AwaitingConfirmation. Items split theo platform. Toast: **"Plan created successfully"**. Xuat hien trong list | Co brand + integration |
| AT-20 | Tao plan thu cong: thieu topic | 1. POST /automation-plans voi row thieu topic (rong hoac chi whitespace) | BE validate: topic required. Tra loi 400: **"Topic is required"**. Khong tao plan | -- |
| AT-21 | Edit item truoc khi confirm | 1. Dang nhap -> Automation -> chon plan AwaitingConfirmation 2. Detail panel -> click edit icon tren 1 item card 3. Edit Item Modal mo: pre-populated data (brand, product, topic, platform, content type, date/time, objective, tone, CTA, notes) 4. Doi topic + content type -> Save | PUT /automation-plans/{id}/items/{itemId}. Item cap nhat. Re-validate: TikTok+Text -> loi. Platform check conflict (neu da ton tai same rowIndex+platform). IdempotencyKey tinh lai. Toast: **"Item updated"** | Plan AwaitingConfirmation |
| AT-22 | Edit item: khong cho phep sau khi confirm | 1. Dang nhap -> Automation -> chon plan da Generating 2. Detail panel -> tim nut edit tren item | **[GHI NHAN]** PUT /automation-plans/{id}/items/{itemId} chi cho phep khi plan status = AwaitingConfirmation. Neu plan da confirm -> API tra loi 400. FE co the an nut edit hoac disable | Plan Generating |
| AT-23 | Edit item: sua TikTok+Text thanh TikTok+Video | 1. Plan AwaitingConfirmation, item TikTok+Text bi loi 2. Edit item -> doi ContentType sang Video -> Save | Item re-validated: TikTok+Video OK. Status chuyen tu NeedsAttention -> Pending. ValidationErrors cleared. ValidItems tang, FailedItems giam. Item co the duoc confirm | Plan AwaitingConfirmation |
| AT-24 | Edit item: platform conflict | 1. Plan co 2 items: row 1 + Facebook, row 1 + Instagram 2. Edit item Facebook -> doi platform sang Instagram -> Save | BE check: same rowIndex + platform Instagram da ton tai -> loi 400: **"Item with this platform already exists for the row"**. Toast loi. Item khong doi | Conflict |
| AT-25 | Edit Item Modal: TikTok warning | 1. Dang nhap -> Edit Item -> chon platform TikTok + content type Text | UI hien thi warning amber: **"TikTok requires Video or Auto content type"**. Nut Save co the bi disabled hoac van cho phep nhung BE se reject. Content Type dropdown co the chi hien thi Video/Auto khi chon TikTok | TikTok selected |

### 19.4 GOOGLE SHEET IMPORT (AT-26 -> AT-29)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-26 | Import Google Sheet thanh cong | 1. Dang nhap -> Automation -> "+ New Automation" -> "Import from Google Sheets" 2. Nhap Plan Name, Timezone, URL Google Sheet (HTTPS, public, link sharing enabled) 3. Click Import | POST /automation-plans/import-google-sheet. BE: export sheet as CSV -> delegate ImportCsvAsync. Plan duoc tao. Toast: **"Google Sheet imported successfully"**. Cung validation nhu CSV import | Google Sheet public, co data |
| AT-27 | Import Google Sheet: URL khong phai docs.google.com | 1. Nhap URL: https://example.com/sheet 2. Import | BE validate: host must be docs.google.com. Tra loi 400: **"Invalid Google Sheets URL"**. Toast loi | URL sai |
| AT-28 | Import Google Sheet: thieu /d/ segment | 1. Nhap URL: https://docs.google.com/spreadsheets/ (khong co sheet ID) 2. Import | BE validate: URL must contain /d/ segment. Tra loi 400: **"Sheet ID not found in URL"**. Toast loi | URL thieu ID |
| AT-29 | Import Google Sheet: sheet khong public (403) | 1. URL sheet chua bat link sharing 2. Import | BE goi export CSV -> Google tra 403. Tra loi 400: **"Unable to access sheet. Make sure sharing is enabled."**. Toast loi | Sheet private |

### 19.5 PLAN CONFIRM & GENERATION -- Xac nhan & sinh noi dung AI (AT-30 -> AT-40)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-30 | Confirm plan thanh cong | 1. Dang nhap -> Automation -> chon plan AwaitingConfirmation (co ValidItems > 0) 2. Detail panel -> click "Confirm Plan" 3. Quan sat | POST /automation-plans/{id}/confirm. BE: reserve credits -> set ConfirmedAt -> set status Generating. Plan status chuyen sang Generating. Detail panel bat dau polling 3s. Items co status Pending duoc background worker pick up. Credits reserved hien thi trong plan. Toast: **"Plan confirmed. AI generation started."** | Plan AwaitingConfirmation, ValidItems > 0 |
| AT-31 | Confirm plan: khong co valid items | 1. Plan co tat ca items NeedsAttention (khong co Pending) 2. Click Confirm | BE validate: ValidItems > 0. Tra loi 400: **"Plan has no valid items to generate"**. Plan van AwaitingConfirmation. Toast loi. User phai edit/sua cac item loi truoc | 0 valid items |
| AT-32 | Confirm plan: credit khong du | 1. Plan estimatedCredits = 500, wallet balance = 100 2. Click Confirm | BE CreditService.ReserveAsync -> check wallet.Balance < estimatedCredits -> loi INSUFFICIENT_WORKSPACE_CREDITS. Toast: **"Insufficient credits to run this plan"**. Plan van AwaitingConfirmation. Khong bi tru credits | Balance 100 |
| AT-33 | Generation: text content duoc tao | 1. Plan da confirm, item Pending voi content type Text 2. Background worker process item 3. Quan sat item card trong detail | Item status: Pending -> GeneratingText -> AwaitingApproval. Content duoc tao (title + caption). UsedCredits = 1. Plan UsedCredits tang 1. Content hien thi trong item card (text preview). GenerationAttemptCount = 1. CreditUsageRecord ghi GenerateText -1 | Plan Generating, item Text |
| AT-34 | Generation: image content duoc tao | 1. Plan da confirm, item voi content type Image 2. Background worker process 3. Quan sat item card | Item status: Pending -> GeneratingText -> GeneratingMedia -> AwaitingApproval. Text + image duoc tao. UsedCredits = 6 (1 text + 5 image). Image hien thi thumbnail trong item card. Content co image URL. CreditUsageRecord ghi GenerateText -1 + GenerateImage -5 | Plan Generating, item Image |
| AT-35 | Generation: content type Auto + platform decision | 1. Plan da confirm, item platform=Facebook, contentType=Auto 2. Background worker process | BE: Auto + Facebook -> generates Text (1 credit) + Image (5 credits). Neu platform=TikTok + Auto -> generates Text (1 credit) + Video (20 credits). Neu platform=Facebook + content type=Text -> chi Text (1 credit). Decision logic dung | Plan Generating, item Auto |
| AT-36 | Generation: plan Failed (all items fail) | 1. Plan confirm, tat ca items GenerationFailed (vi du: sai prompt, AI error) 2. Background worker finish | Plan status: Generating -> Failed. Reserved credits released. UsedCredits giu nguyen. Plan khong the retry tru khi co item GenerationFailed (can retry). Toast/polling update plan status | All items fail |
| AT-37 | Generation: plan PartiallyFailed | 1. Plan 5 items: 3 thanh cong (AwaitingApproval), 2 GenerationFailed 2. Background worker finish | Plan status: Generating -> PartiallyFailed. Successful items van AwaitingApproval. Failed items co the retry. Reserved credits released cho phan chua dung. Performance cards chi hien thi data cua items thanh cong | 3 success, 2 fail |
| AT-38 | Generation: item GenerationFailed voi lastError | 1. Item fail trong qua trinh generate (network, AI error) 2. Quan sat item card | Item status: GenerationFailed. lastError hien thi message loi (truncate 2000 chars). GenerationAttemptCount tang. Item card hien thi background do nhat + lastError text. Nut "Retry" hien thi tren item card (neu co) | Item fail |
| AT-39 | Generation: image prompt rules (no humans, no faces) | 1. Item Image duoc generate 2. Kiem tra prompt gui den AI | BE tao prompt image kem quy tac: NO HUMANS, NO FACES, NO HANDS, NO BODY PARTS. Product reference images duoc gui kem (up to 3: 1 primary + 2 supporting). Product knowledge profile duoc dua vao prompt | Item Image |
| AT-40 | Generation: product knowledge trong AI prompt | 1. Item co productId -> generate text 2. Kiem tra prompt | Prompt chua: product Name, Category, Description, PrimaryUse, USP, TargetAudience, VisualIdentity, KnowledgeProfile. Product URL duoc append vao caption (tru khi notes chua "no link" signals). Tieng Viet prompts | Item co product |

### 19.6 VIDEO GENERATION (AT-41 -> AT-45)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-41 | Video generation: start -> queued | 1. Plan da confirm, item TikTok+Auto (can Video) 2. Background worker: generate text xong -> start video 3. Quan sat | BE: goi IAIVideoProvider.StartVideoGenerationAsync (4s, 9:16 aspect ratio). Neu queued/processing: luu VideoJobId + VideoProvider vao item. Item status: GeneratingMedia (khong phai AwaitingApproval). Background worker se poll lai sau | Item can video |
| AT-42 | Video generation: resume sau khi server restart | 1. Item dang GeneratingMedia + co VideoJobId 2. Server restart -> background worker khoi dong lai 3. Quan sat | Background worker tim item GeneratingMedia + VideoJobId -> resume poll video status (khong restart text). Khi video complete -> tai video, upload storage -> item -> AwaitingApproval. VideoJobId duoc clear (hoac giu nguyen) | Server vua restart |
| AT-43 | Video generation: complete | 1. Video job hoan thanh 2. Background worker poll -> complete | BE: tai video tu provider -> upload to IMediaStorageService (automation-videos/ path). Content cap nhat video URL. Settle 20 credits. UsedCredits item = 21 (1 text + 20 video). Item status -> AwaitingApproval. Video preview hien thi trong item card | Video complete |
| AT-44 | Video generation: fail | 1. Video job fail 2. Background worker poll -> fail | Item status -> GenerationFailed. lastError: **"Video generation failed: {reason}"**. UsedCredits chi tinh cho text (1 credit). Video credit khong bi tru (chua settle). Item co the retry | Video fail |
| AT-45 | Video generation: timeout / qua lau | 1. Video job running > thoi gian cho phep 2. Background worker poll | **[GHI NHAN]** BE co timeout cho video generation khong? Hay poll vo han? Can ghi nhan: max poll time, retry limit. Neu video treo -> item co the bi GenerationFailed sau N attempts | Video running lau |

### 19.7 APPROVAL & SCHEDULING -- Duyet & len lich dang bai (AT-46 -> AT-55)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-46 | Approve plan: single integration auto-select | 1. Plan AwaitingApproval, item co brand+platform co exactly 1 active SocialIntegration 2. Detail panel -> click "Approve & Schedule" | BE: tim integration -> auto-select. Tao ContentCalendar entry voi scheduledAt cua item. Content status -> Approved. Tao Approval audit record. Item status: AwaitingApproval -> Scheduled. Plan status: AwaitingApproval -> Scheduling -> Completed. Toast: **"Plan approved and scheduled"** | 1 integration |
| AT-47 | Approve plan: nhieu integrations (can chon pages) | 1. Plan AwaitingApproval, item co brand+platform co 2 Facebook pages 2. Click "Approve & Schedule" -> Page Selection Modal mo | Modal hien thi danh sach Facebook pages: checkbox, page name, platform icon. Can chon it nhat 1 page de Approve. Neu khong chon -> item -> NeedsAttention + message **"Multiple pages available. Please select."**. Sau khi chon + confirm -> tao ContentCalendar cho moi page duoc chon. Item -> Scheduled | 2+ Facebook pages |
| AT-48 | Approve plan: 0 active integrations | 1. Plan AwaitingApproval, brand chua connect integration 2. Click Approve | BE: khong tim thay active SocialIntegration -> item -> NeedsAttention + validationErrors: **"No active social integration found for {brand} ({platform})"**. Plan status giu nguyen AwaitingApproval (hoac PartiallyFailed). Toast canh bao: **"Some items could not be scheduled"** | 0 integrations |
| AT-49 | Page Selection Modal: chon pages | 1. Plan AwaitingApproval, item co 3 Instagram pages 2. Page Selection Modal mo 3. Check 2 pages -> click Confirm | POST /automation-plans/{id}/items/{itemId}/approve-targets voi integrationIds. Tao 2 ContentCalendar entries. Item -> Scheduled. Content duoc share len ca 2 pages (schedule rieng). Performance cap nhat | 3 pages |
| AT-50 | Reject item | 1. Plan AwaitingApproval -> detail panel -> click Reject button tren item card 2. Nhap notes (optional) -> Confirm | POST /automation-plans/{id}/items/{itemId}/reject. Item status: AwaitingApproval -> Rejected. Content status -> Rejected. Approval audit record duoc tao. Plan status recalculate: co the PartiallyFailed neu con items AwaitingApproval. Toast: **"Item rejected"** | Item AwaitingApproval |
| AT-51 | Auto-approve plan | 1. Plan co autoApprove=true, Generating -> tat ca items AwaitingApproval 2. OperationsBackgroundService process | BE: plan AwaitingApproval + AutoApprove -> tu dong approve. Tim single integration -> auto-select, tao schedules. Neu multiple integrations -> item -> NeedsAttention. Max 5 auto-approve plans per 15s cycle. Plan -> Completed/PartiallyFailed | AutoApprove enabled |
| AT-52 | Approve: brand+platform nhieu pages + auto-approve | 1. Plan auto-approve, item co 2 Facebook pages 2. Operations worker process | BE: tim nhieu integrations -> khong auto-select -> item -> NeedsAttention + message **"Multiple pages available. Please select."**. Can manual approve sau. Plan PartiallyFailed | AutoApprove, 2 pages |
| AT-53 | Approve: existing schedule reuse (idempotent) | 1. Plan da approve -> item Scheduled co ContentCalendarId 2. Approve lai (duplicate call) | BE: tim ContentCalendar da ton tai -> reuse (khong tao duplicate). Unique index ContentCalendarId filter dam bao idempotent. Item van Scheduled. Khong crash, khong duplicate schedule | Da approve |
| AT-54 | Approve: schedule bi xoa/cancel -> auto-recovery | 1. Item Scheduled -> schedule bi cancel/delete 2. Operations worker detect | OperationsBackgroundService: tim schedule cancelled -> try find replacement integration. Neu co -> tao schedule moi. Neu khong -> item -> AwaitingApproval (can manual approve lai). Plan status cap nhat | Schedule cancelled |
| AT-55 | Plan Completed khi tat ca items Scheduled/Published | 1. Plan AwaitingApproval -> Approve tat ca items thanh cong 2. Quan sat plan status | Plan status: Scheduling -> Completed. Summary cards cap nhat. Performance cards hien thi data. Khong con polling. Khi item Published -> operations worker update item status Scheduled -> Published | All items Scheduled |

### 19.8 CREDIT MANAGEMENT -- Quan ly credit trong automation (AT-56 -> AT-62)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-56 | Credit reserve khi confirm plan | 1. Plan estimatedCredits = 100, wallet balance = 500 2. Click Confirm Plan | BE: AutomationCreditService.ReserveAsync. Wallet reservedBalance += 100. Available balance = 400. Plan ReservedCredits = 100. Neu goi ReserveAsync lan 2 -> idempotent (ReservedCredits > 0 -> return success) | Balance 500, plan est. 100 |
| AT-57 | Credit settle sau moi generation step | 1. Item generate text thanh cong 2. Background worker settle | BE: SettleAsync(itemId, userId, action=GenerateText, amount=1, expectedItemUsedCredits=1). Wallet reservedBalance -= 1, balance -= 1. Item UsedCredits = 1. Plan UsedCredits = 1. CreditUsageRecord ghi GenerateText -1. Idempotent: neu UsedCredits >= expectedItemUsedCredits -> skip | Text generation success |
| AT-58 | Credit release khi plan Completed | 1. Plan est. 100, used 80, reserved 100 2. Plan Completed | BE: ReleaseAsync. Plan ReservedCredits 100 -> 0. Wallet reservedBalance -= 20 (phan khong dung). ReleasedCredits ghi 20. Available balance tang 20 tro lai | Plan Completed, used 80/100 |
| AT-59 | Credit release khi cancel plan | 1. Plan dang Generating, reserved 100, used 30 2. Click Cancel Plan | BE: CancelAsync -> ReleaseAsync. Reserved 100 -> 0. Wallet reservedBalance -= 70 (100-30). Items in-progress -> Rejected. Items completed giu nguyen. Plan -> Cancelled. Credits da dung (30) khong hoan lai | Plan Generating, cancel |
| AT-60 | Credit settle fail rollback | 1. Item generate text, settle 1 credit 2. Wallet balance update fail | BE: try-catch trong SettleAsync. Neu ConsumeCreditsAsync fail -> rollback: wallet reservedBalance += 1 (hoan lai reserved). Item UsedCredits giu nguyen. GenerationAttemptCount tang. Item -> GenerationFailed | Settlement fail |
| AT-61 | Credit estimate per content type | 1. Tao plan voi cac content type khac nhau 2. Kiem tra estimatedCredits | Text: 1/item. Image: 6/item (1 text + 5 image). Video: 21/item (1 text + 20 video). Auto: 21/item (max estimate). Plan EstimatedCredits = sum tat ca items. Hien thi trong detail panel | Plan AwaitingConfirmation |
| AT-62 | Credit retry: khong charge lai step da hoan thanh | 1. Item da generate text (UsedCredits=1), sau do fail video generation (GenerationFailed) 2. Retry item | BE: retry -> reset item -> generate text lai. SettleAsync text: expectedItemUsedCredits=1. Check: UsedCredits=1 >= 1 -> skip (khong charge lai text). Video: settlle expectedItemUsedCredits=21 -> UsedCredits=1 < 21 -> charge 20 credits. Khong charge duplicate | Retry sau text complete |

### 19.9 CLONE & AUTO-APPROVE (AT-63 -> AT-68)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-63 | Clone plan as template | 1. Dang nhap -> Automation -> detail plan da Completed 2. Advanced Operations -> "Clone (Use as Template)" 3. Nhap Name: "Chien dich T9", Shift Days: 7 -> Confirm | POST /automation-plans/{id}/clone. Plan moi duoc tao voi name moi. Tat ca items duoc copy, scheduledAt += 7 ngay. TemplateSourcePlanId = source plan ID. Clone status = AwaitingConfirmation. Items Pending, chua duoc confirm. Toast: **"Plan cloned successfully"** | Plan Completed |
| AT-64 | Clone plan: shiftDays validation | 1. Clone -> Shift Days: 0 (hoac 4000) | BE validate: shiftDays must be 1-3650. Tra loi 400: **"Shift days must be between 1 and 3650"**. Toast loi. Khong tao plan | shiftDays invalid |
| AT-65 | Clone plan: khong clone failed items | 1. Source plan co 3 items: 2 success, 1 GenerationFailed 2. Clone plan | BE CloneAsync: chi copy items Pending + Approved + Scheduled (khong copy GenerationFailed, Rejected). Clone plan chi co valid items. TotalItems = valid items cua source | Plan co failed items |
| AT-66 | Auto-approve toggle ON | 1. Dang nhap -> Automation -> detail plan AwaitingConfirmation 2. Advanced Operations -> toggle Auto-approve ON 3. Quan sat | PUT /automation-plans/{id}/auto-approve (enabled=true). Plan AutoApprove = true. UI toggle chuyen ON (mau xanh). Chi Owner/Manager co quyen toggle. Sau khi confirm + generate xong -> operations worker tu dong approve | Plan AwaitingConfirmation |
| AT-67 | Auto-approve toggle OFF | 1. Plan dang AutoApprove=true -> toggle OFF | PUT auto-approve (enabled=false). Plan AutoApprove = false. Sau khi generate xong -> plan AwaitingApproval (can manual approve). Toggle OFF (xam) | AutoApprove ON |
| AT-68 | Auto-approve: khong co quyen toggle | 1. Dang nhap ContentCreator hoac Viewer 2. Automation -> detail plan -> Advanced Operations | **[GHI NHAN]** Controller check role: chi Owner/Manager duoc phep. Neu ContentCreator/Viewer -> API 403: **"Only Owner or Manager can toggle auto-approve"**. FE co the an toggle hoac disabled | ContentCreator/Viewer |

### 19.10 RETRY & CANCEL (AT-69 -> AT-73)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-69 | Retry failed plan | 1. Plan PartiallyFailed, co 3 items GenerationFailed 2. Detail panel -> click "Retry Failed" 3. Quan sat | POST /automation-plans/{id}/retry. BE: release old reserved credits -> re-reserve cho items failed. Reset GenerationFailed items -> Pending (clear LastError, GenerationAttemptCount giu nguyen). Plan status -> Generating. Background worker pick up lai. Polling bat dau. Toast: **"Retrying failed items"** | Plan PartiallyFailed, items GenerationFailed |
| AT-70 | Retry single item | 1. Plan PartiallyFailed, chi muon retry 1 item 2. Click Retry button tren item card (hoac API ?itemId=xxx) | POST /automation-plans/{id}/retry?itemId=xxx. Chi reset item do -> Pending. Cac item GenerationFailed khac giu nguyen. Plan -> Generating. Re-reserve credits | 1 item can retry |
| AT-71 | Retry: item co validation errors khong the retry | 1. Item bi NeedsAttention (validation error) khong phai GenerationFailed 2. Click Retry / goi API retry | BE: chi retry items GenerationFailed. Items NeedsAttention bi bo qua. Phai edit fix validation truoc -> status -> Pending -> confirm plan. Toast neu khong co item nao de retry: **"No failed items to retry"** | Item validation error |
| AT-72 | Cancel plan dang Generating | 1. Plan Generating, 5 items: 2 GeneratingText, 3 Pending 2. Click "Cancel Plan" -> Confirm | POST /automation-plans/{id}/cancel. BE: items Pending/GeneratingText/GeneratingMedia -> Rejected (lastError: "Generation cancelled by user."). Items da AwaitingApproval -> giu nguyen. Plan status -> Cancelled. Release reserved credits. Toast: **"Plan cancelled"** | Plan Generating |
| AT-73 | Cancel plan: khong the cancel Completed/Cancelled | 1. Plan da Completed hoac Cancelled 2. Goi POST /automation-plans/{id}/cancel | BE validate: plan status khong the Completed hoac Cancelled. Tra loi 400: **"Plan cannot be cancelled"**. Toast loi. Khong thay doi | Plan Completed |

### 19.11 PERFORMANCE & EDGE CASES (AT-74 -> AT-78)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AT-74 | Performance: xem thong ke plan | 1. Dang nhap -> Automation -> chon plan Completed 2. Quan sat Performance cards + goi GET /automation-plans/{id}/performance | Response: planId, totalItems, scheduledItems, publishedItems, failedItems, impressions, engagement, averageCtr, estimatedRevenue. So khop du lieu thuc te tu ContentCalendar + PerformanceReports. Neu chua co data -> 0 | Plan Completed |
| AT-75 | Idempotency key: khong duplicate item | 1. Import CSV 2 row trung lap (cung brand, topic, platform, scheduledAt) 2. Import lai file same name | BE: idempotencyKey = SHA256(planId:rowIndex:platform). Neu trung -> unique constraint violation -> skip hoac update. Neu import lai file moi (planId khac) -> keys khac -> khong conflict. Import lai cung file (cung planId) -> co the duplicate | Row trung |
| AT-76 | CSV import: file > 10MB | 1. Chon file CSV 15MB 2. Click Import | BE: request size limit 10MB. ASP.NET Core tu dong reject -> 413 Payload Too Large. FE co the validate file size truoc khi upload | File 15MB |
| AT-77 | Plan detail: edit item thay doi scheduledAt | 1. Plan AwaitingConfirmation -> edit item -> doi scheduledAt 2. Save | Item cap nhat. Neu doi scheduledAt keo theo idempotencyKey thay doi (neu key tinh tu scheduledAt)? **[GHI NHAN]** idempotencyKey = SHA256(planId:rowIndex:platform) -> KHONG phu thuoc scheduledAt. safe to change | Edit scheduledAt |
| AT-78 | Plan detail: hien thi content preview cho item da generate | 1. Plan AwaitingApproval, item da generate text + image 2. Mo item card | Text: hien thi title + caption preview (truncate). Image: thumbnail + click mo full size. Video: video player thumbnail. Co link den Content page. Neu item GenerationFailed -> khong co preview, chi co lastError | Items generated |

**Module:** AUTOMATION | **Total:** 78 cases | **Pages:** `/automation` | **API:** GET/POST `/automation-plans`, POST import-csv/import-google-sheet/confirm/retry/cancel/clone, PUT auto-approve/items, GET performance/targets, POST approve/approve-targets/reject



---


## SHEET 20/20: ADMIN MANAGEMENT -- Quan tri he thong (57 cases)

| **Feature** | Admin Management -- Quan ly Users, Workspaces, Content, Settings/Security, Tools, Audit Logs, Service Health |
|---|---|
| **Test requirement** | Admin Users page `/admin/users`: table (Email, Name, Role badge, Status badge, Actions), pagination, search; User Detail `/admin/users/[id]`: user details card, Workspaces list, Sessions list, Subscriptions list, Payment history (1-year toggle), Campaigns list, "Login as User" impersonate button, Delete button; Admin Workspaces `/admin/workspaces`: table (Name, Type badge Personal/Business, Status badge Active/Limited/Archived/Deleted, Created, Actions), search + type filter; Workspace Detail `/admin/workspaces/[id]`: details card, Members table, Recent Posts table (click modal); Admin Content `/admin/content`: moderation queue table (Title, AI Generated badge, Status badge, Created, Actions Flag/Unflag/Delete), search + status filter, detail modal (text/video/images); Admin Settings `/admin/settings`: hub page grid (AI Providers, Email, System, Security); AI Providers: model selector, credit cost, image provider; Email: SMTP config; System: maintenance toggle, rate limits, feature toggles; Security: change password form; Admin Tools `/admin/tools`: Seed Demo Users (count input), Seed Demo Content (count input); Admin Audit Logs `/admin/audit-logs`: paginated table (Action, Target, Actor, Date), detail page with before/after JSON diff; Admin Service Health `/admin/service-health`: background service status cards, auto-refresh 30s; AdminSidebar: 15 nav items, active route detection, logout |
| **Pages** | `/admin/users`, `/admin/users/[id]`, `/admin/workspaces`, `/admin/workspaces/[id]`, `/admin/content`, `/admin/settings`, `/admin/settings/ai-providers`, `/admin/settings/email`, `/admin/settings/system`, `/admin/settings/security`, `/admin/tools`, `/admin/audit-logs`, `/admin/audit-logs/[id]`, `/admin/service-health` |
| **API** | GET `/admin/users`, GET `/admin/users/{id}`, PATCH `/admin/users/{id}/status`, DELETE `/admin/users/{id}`, PATCH `/admin/users/{id}/role`, POST `/admin/users/{id}/impersonate`, GET `/admin/workspaces`, GET `/admin/workspaces/{id}`, PATCH `/admin/workspaces/{id}/status`, DELETE `/admin/workspaces/{id}`, GET `/admin/content`, PATCH `/admin/content/{id}/status`, DELETE `/admin/content/{id}`, GET/PATCH `/admin/settings`, POST `/admin/tools/seed-demo-users`, POST `/admin/tools/seed-demo-content`, GET `/admin/audit-logs`, GET `/admin/audit-logs/{id}`, GET `/admin/service-health` |
| **Model** | User: email, name, role, isEmailVerified (isActive), createdAt. Workspace: name, type (Personal=1/Business=2), status (Active=0/Limited=1/Archived=2/Deleted=3). Content: title, status (Draft=0 to Flagged=5). AuditLog: action, targetTable, targetId, actorId, oldValues JSON, newValues JSON, notes, createdAt. SystemSetting: key, value, description, updatedBy |

### 20.1 ADMIN USERS -- Quan ly nguoi dung (AD-01 -> AD-12)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AD-01 | Admin: xem danh sach users | 1. Dang nhap Admin -> sidebar -> "Users" 2. Quan sat trang | Bang: Email, Name, Role (badge: Admin do, Manager xanh, ContentCreator tim, Viewer xam), Status (Active xanh/Inactive do), Actions (Activate/Deactivate, Promote/Demote, Delete). Pagination. Search input. API: GET /admin/users?page=1&pageSize=20 | Admin, co 5+ users |
| AD-02 | Admin: search users | 1. Admin -> Users -> nhap "test" vao search 2. Quan sat | Goi GET /admin/users?searchTerm=test. Bang chi hien users co email/name chua "test". Pagination cap nhat. Xoa search -> hien lai tat ca | Admin |
| AD-03 | Admin: xem user detail | 1. Admin -> Users -> click 1 user row 2. Quan sat trang detail | GET /admin/users/{id}. User details card: Email, Name, Role, Status, Created. Workspaces list (user tham gia). Sessions list. Subscriptions list. Payment history (1-year toggle). Campaigns list. Nut "Login as User" (impersonate). Nut Delete (disabled neu user la Admin) | Admin |
| AD-04 | Admin: activate/deactivate user | 1. Admin -> Users -> click Activate/Deactivate 2. Quan sat | PATCH /admin/users/{id}/status (isActive: bool). User status cap nhat. StatusBadge thay doi. Audit log ghi nhan. Toast "User status updated" | Admin |
| AD-05 | Admin: change user role | 1. Admin -> User detail -> change role: Manager -> ContentCreator 2. Save | PATCH /admin/users/{id}/role (role: int). Role badge cap nhat. Audit log ghi nhan. Khong the change own role -> 403 | Admin |
| AD-06 | Admin: delete user | 1. Admin -> User detail -> click Delete (user khong phai Admin) 2. Confirm | DELETE /admin/users/{id}. User bi xoa. Redirect ve Users list. Audit log. Khong the delete Admin user -> 403 "Cannot delete admin users" | Admin, user thuong |
| AD-07 | Admin: impersonate user | 1. Admin -> User detail -> "Login as User" 2. Quan sat | POST /admin/users/{id}/impersonate -> tra ve TokenResponse (JWT cua user do). FE navigate sang dashboard user. Admin co the xem workspace nhu user do. Audit log ghi nhan impersonate | Admin |
| AD-08 | Admin: delete chinh minh | 1. Admin A -> User detail cua Admin A 2. Thuc hien delete/change role | BE check: user.Role == Admin -> 403. Khong the delete admin. Khong the change own role. Nut disabled/hoac API reject | Admin |
| AD-09 | Admin: user detail - Payment history 1-year toggle | 1. Admin -> User detail -> Payment history 2. Toggle "Show last year" ON/OFF | ON: payments trong 1 nam gan nhat. OFF: payments gan day. Table cap nhat | Admin |
| AD-10 | User thuong truy cap /admin/users | 1. User thuong -> /admin/users | [Authorize(Roles=Admin)] -> 403. FE redirect hoac "Access denied" | User thuong |
| AD-11 | Admin Users: loading + empty state | 1. Admin -> Users (Slow 3G) -> skeleton loading 2. He thong moi -> 0 users | Loading: skeleton table rows. Empty: "No users found". Search khong co ket qua: tuong tu | Admin |
| AD-12 | Admin Users: pagination | 1. Admin -> Users co 45 items 2. Duyet qua cac trang | "Page 1 of 3". Prev/Next. Page hien tai highlight. Chuyen trang -> API page thay doi. 20 users/page | Admin |

### 20.2 ADMIN WORKSPACES -- Quan ly workspace (AD-13 -> AD-20)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AD-13 | Admin: xem danh sach workspaces | 1. Admin -> sidebar -> "Workspaces" 2. Quan sat trang | Bang: Name, Type (badge Personal xanh/Business tim), Status (badge Active xanh, Limited vang, Archived cam, Deleted do), Created, Actions (Limit/Activate, Delete). Search + type filter dropdown (All/Personal/Business). Pagination | Admin |
| AD-14 | Admin: filter workspaces theo type | 1. Admin -> Workspaces -> chon type filter "Personal" -> "Business" -> "All" | Moi filter goi GET /admin/workspaces?type=N (1=Personal, 2=Business, null=All). Bang cap nhat. Pagination reset | Admin |
| AD-15 | Admin: workspace detail | 1. Admin -> Workspaces -> click 1 workspace 2. Quan sat trang detail | GET /admin/workspaces/{id}. Details card: Name, Type, Status, Created. Members table: User, Email, Role, Status, Joined. Recent Posts table: Title, Status, Created. Click post -> modal mo xem full text/video/images. Nut Delete workspace | Admin |
| AD-16 | Admin: set workspace status | 1. Admin -> Workspaces -> click Limit/Activate 2. Quan sat | PATCH /admin/workspaces/{id}/status (status: int). Status cap nhat (0=Active, 1=Limited, 2=Archived, 3=Deleted). Audit log. Toast "Workspace status updated" | Admin |
| AD-17 | Admin: delete workspace | 1. Admin -> Workspace detail -> Delete 2. Confirm | DELETE /admin/workspaces/{id}. Workspace bi xoa vinh vien. Redirect list. Audit log. Members khong con thay workspace | Admin |
| AD-18 | Admin Workspaces: search | 1. Admin -> Workspaces -> nhap ten workspace vao search | GET /admin/workspaces?searchTerm=xxx. Bang loc. Xoa search -> tat ca | Admin |
| AD-19 | Admin Workspaces: post detail modal | 1. Admin -> Workspace detail -> click 1 post 2. Quan sat modal | Modal: title, full text content, image gallery, video player (neu co). Status badge. Close button + click outside | Admin, co posts |
| AD-20 | Admin Workspaces: empty state | 1. Admin -> Workspaces (he thong moi) | Bang "No workspaces found". Filter/search van hien thi | Admin |

### 20.3 ADMIN CONTENT -- Kiem duyet noi dung (AD-21 -> AD-27)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AD-21 | Admin: xem danh sach content | 1. Admin -> sidebar -> "Content" 2. Quan sat trang | Bang: Title, AI Generated badge (neu co), Status badge (Draft/Pending/Approved/Rejected/Published/Flagged, mau tuong ung), Created, Actions (Flag/Unflag, Force Delete). Search + status filter dropdown (All..Flagged). Pagination. Click row -> detail modal | Admin |
| AD-22 | Admin: filter content theo status | 1. Admin -> Content -> chon status "Flagged" -> "Pending" -> "All" | GET /admin/content?status=N. Bang loc dung. Search ket hop | Admin |
| AD-23 | Admin: flag/unflag content | 1. Admin -> Content -> click Flag tren 1 content 2. Click Unflag | PATCH /admin/content/{id}/status -> status=5 (Flagged)/status truoc do. Row cap nhat. Audit log | Admin |
| AD-24 | Admin: force delete content | 1. Admin -> Content -> click Force Delete 2. Confirm | DELETE /admin/content/{id}. Content bi xoa. Row bien mat. Audit log | Admin |
| AD-25 | Admin: content detail modal | 1. Admin -> Content -> click row 2. Quan sat modal | Modal: full text content, video player (neu co), image gallery (neu co). Status badge. AI Generated indicator. Close button | Admin |
| AD-26 | Admin Content: search | 1. Admin -> Content -> nhap search keyword | GET /admin/content?search=xxx. Loc content theo title. Ket hop status filter | Admin |
| AD-27 | Admin Content: set content status | 1. Admin -> Content -> change status cua 1 content (duyet, reject...) | PATCH /admin/content/{id}/status (status: int). Content status cap nhat. Audit log | Admin |

### 20.4 ADMIN SETTINGS -- Cau hinh he thong (AD-28 -> AD-37)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AD-28 | Admin: Settings hub page | 1. Admin -> sidebar -> "Settings" 2. Quan sat trang | Card grid: AI Providers, Email Configuration, System Settings, Security. Moi card: icon + title + mo ta + nut link den sub-page | Admin |
| AD-29 | Admin: AI Providers settings | 1. Admin -> Settings -> "AI Providers" 2. Quan sat trang | Form: Default Model (text input, vd: gemini-2.5-flash), Credit Cost/Text Gen, Image Provider (dropdown: vertex-ai/openrouter), Image Credit Cost. Save button. GET/PATCH /admin/settings voi key: ai.default_model, ai.credit_cost, ai.image_provider, ai.image_credit_cost | Admin |
| AD-30 | Admin: Email settings | 1. Admin -> Settings -> "Email" 2. Quan sat trang | Form: SMTP Host, SMTP Port, Username, Password (masked), From Name, From Email. Save. Keys: email.smtp_host, email.smtp_port, email.username, email.from_name, email.from_email | Admin |
| AD-31 | Admin: System settings | 1. Admin -> Settings -> "System" 2. Quan sat trang | Form: Maintenance Mode (toggle), Rate Limits (API rpm, AI/hr, Upload MB, Session timeout min), Feature Toggles (AI Image, AI Video, Social Publish, Team Mgmt - toggle switches). Save. Keys: system.maintenance_mode, system.rate_limit, system.ai_limit, system.max_upload, system.session_timeout, system.enabled_features | Admin |
| AD-32 | Admin: Security - change password | 1. Admin -> Settings -> "Security" 2. Quan sat trang | Form: Current Password, New Password, Confirm Password. Show/hide toggle cho moi field. Validation: all required, passwords match, min 8 chars. Save -> password updated. Toast | Admin |
| AD-33 | Admin Settings: save thanh cong | 1. Admin -> any settings page -> thay doi + Save | PATCH /admin/settings voi dict key-value. Tra ve success. Toast "Settings saved". Audit log | Admin |
| AD-34 | Admin Settings: load existing values | 1. Admin -> Settings pages -> quan sat form | GET /admin/settings -> key-value pairs. Form pre-populated voi gia tri hien tai. Neu key chua ton tai -> default/empty | Admin, da config truoc |
| AD-35 | Admin Settings: maintenance mode ON | 1. Admin -> System -> Maintenance Mode ON -> Save 2. User thuong truy cap trang | **[GHI NHAN]** Khi maintenance mode ON, user thuong co bi chan khong? FE check system.maintenance_mode -> hien thi maintenance page? Can ghi nhan thuc te | Admin |
| AD-36 | Admin Settings: validation - thieu required fields | 1. Admin -> any settings -> bo trong field required -> Save | FE validate: hien thi message "Field is required". Nut Save disabled hoac API reject | Admin |
| AD-37 | Admin Settings: security password validation | 1. Admin -> Security -> Current Password sai 2. Password moi < 8 ky tu 3. Confirm khong khop | Hien thi message: "Current password is incorrect" / "Minimum 8 characters" / "Passwords do not match". Nut Save bi disable/loi | Admin |

### 20.5 ADMIN TOOLS, AUDIT LOGS & SERVICE HEALTH (AD-38 -> AD-48)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AD-38 | Admin: Seed Demo Users | 1. Admin -> sidebar -> "Dev Tools" (an trong production) 2. Nhap count=5 -> click "Seed Demo Users" 3. Quan sat | POST /admin/tools/seed-demo-users?count=5. Tao 5 users: demo1@aisam-demo.com..demo5@aisam-demo.com, password Demo@123. Response: {Created: [...], Count: 5}. Hien thi feedback. Users xuat hien trong Users list. Chi hoat dong trong Development environment | Dev environment |
| AD-39 | Admin: Seed Demo Content | 1. Admin -> Tools -> count=10 -> "Seed Demo Content" | POST /admin/tools/seed-demo-content?count=10. Tao 10 content items phan bo vao existing workspaces. Response: {Created: 10}. Feedback hien thi. Content xuat hien trong Content list. Can co active workspaces | Dev environment, co workspaces |
| AD-40 | Admin Tools: dev-only guard | 1. Truy cap /admin/tools trong Production | BE: check !_env.IsDevelopment() -> 404 Not Found. FE: sidebar an "Dev Tools" trong production (hidden) | Production |
| AD-41 | Admin: Audit Logs list | 1. Admin -> sidebar -> "Audit Logs" 2. Quan sat trang | GET /admin/audit-logs?page=1&pageSize=20. Bang: Action (vd: SetUserStatus, DeleteWorkspace), Target (table + ID prefix), Actor, Date. Paginated. Click row -> detail page | Admin, co logs |
| AD-42 | Admin: Audit Log detail | 1. Admin -> Audit Logs -> click 1 log 2. Quan sat trang detail | GET /admin/audit-logs/{id}. Metadata card: Action, Target, Actor, Date. Side-by-side JSON diff: "Before" (OldValues, do) vs "After" (NewValues, xanh). JSON formatted, readable. Back button | Admin |
| AD-43 | Admin: Audit Logs - empty state | 1. Admin -> Audit Logs (he thong moi, chua co hanh dong) | Bang "No audit logs". Pagination an/disabled | Admin |
| AD-44 | Admin: Service Health page | 1. Admin -> sidebar -> "Service Health" 2. Quan sat trang | GET /admin/service-health. Cards per background service: ten service, status dot (green ok/red error/yellow warning), success/failure counts, last heartbeat, last error. Overall status. Auto-refresh 30s. Manual refresh button | Admin |
| AD-45 | Admin: Service Health - auto-refresh | 1. Admin -> Service Health 2. Doi 30s | Data tu dong refresh. Status dots + counts cap nhat. Khong can F5. Interval clear khi navigate away | Admin |
| AD-46 | Admin: Service Health - manual refresh | 1. Admin -> Service Health -> click Refresh | Data refetch ngay. Nut loading/spin. Countdown reset | Admin |
| AD-47 | Admin: AdminSidebar active route | 1. Admin -> navigate qua cac trang admin 2. Quan sat sidebar | Mục hien tai duoc highlight (bg khac, text primary). 15 nav items: Dashboard, Users, Workspaces, Payments, Subscriptions, Plans, Content, AI & Credit, Analytics, Audit Logs, Service Health, System Health, Broadcast, Dev Tools, Settings. Logout button footer | Admin |
| AD-48 | Admin: AdminSidebar collapse/expand | 1. Admin -> thu nho sidebar (neu responsive) | **[GHI NHAN]** Sidebar co collapse tren mobile khong? Hien thi icon + tooltip thay vi text? Can ghi nhan thuc te | Admin, mobile |

### 20.6 ADMIN PERMISSIONS & EDGE CASES (AD-49 -> AD-57)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| AD-49 | Admin role verification (DB re-check) | 1. Admin -> goi API admin bat ky 2. BE AdminService check | BE: lay admin user tu DB -> check Role == Admin. Neu khong phai -> 403 ngay ca khi JWT claims co Admin role. Double verification | Admin |
| AD-50 | Audit log ghi nhan moi hanh dong admin | 1. Admin -> thuc hien bat ky mutation (status, role, delete) 2. Kiem tra Audit Logs | AuditLog moi: actorId = adminUserId, action = ten method, targetTable = table name, targetId = id, oldValues JSON, newValues JSON, notes. Xuat hien trong Audit Logs page | Admin |
| AD-51 | Unauthorized: user thuong goi API admin | 1. User thuong -> goi truc tiep API GET /admin/users | [Authorize(Roles=Admin)] -> 401/403. Response: "Access denied" | User thuong |
| AD-52 | Admin: mat mang khi load trang admin | 1. Admin -> DevTools Offline -> Users/Workspaces/Content | API fail. Hien thi skeleton hoac error message. Nut Retry (neu co). Khong crash | Mat mang |
| AD-53 | Admin Settings: batch update nhieu keys | 1. Admin -> thay doi ca AI + Email + System settings -> Save | PATCH /admin/settings voi dict nhieu keys. Tat ca duoc upsert batch. Khong bi mat key khong duoc gui | Admin |
| AD-54 | Admin Content: AI Generated indicator | 1. Admin -> Content -> tim content duoc tao boi AI | Row hien thi badge "AI Generated" (mau secondary/tim). Content tao boi user thuong -> khong co badge | Admin, co AI content |
| AD-55 | Admin Workspace: Members table hien thi role | 1. Admin -> Workspace detail -> Members table | Moi member: User (email), Role (badge Owner/Manager/ContentCreator/Viewer), Status (Active/Inactive), Joined date | Admin |
| AD-56 | Admin: double-click mutation (idempotent) | 1. Admin -> click Delete/Status change 2 lan nhanh | Lan 1: xu ly. Lan 2: nut disabled hoac API return success (da thay doi) hoac 404 (da xoa). Khong crash, khong duplicate | Admin |
| AD-57 | Admin Settings: maintenance mode redirect | 1. Admin -> System -> Maintenance Mode ON 2. User -> truy cap bat ky trang | **[GHI NHAN]** FE co check maintenance mode khong? Hien thi "System under maintenance" page? Hay middleware xu ly? Can ghi nhan thuc te | Maintenance ON |

**Module:** ADMIN MANAGEMENT | **Total:** 57 cases | **Pages:** `/admin/users`, `/admin/users/[id]`, `/admin/workspaces`, `/admin/workspaces/[id]`, `/admin/content`, `/admin/settings` (+ sub-pages), `/admin/tools`, `/admin/audit-logs`, `/admin/audit-logs/[id]`, `/admin/service-health` | **API:** `/admin/users`, `/admin/workspaces`, `/admin/content`, `/admin/settings`, `/admin/tools`, `/admin/audit-logs`, `/admin/service-health`

