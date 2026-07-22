# AISAM -- Manual Test Cases

**Ngày tạo:** 2026-07-20 | **Loại:** Manual Testing (UI/Browser)

---

## SHEET 1/19: AUTH -- Authentication (74 cases)

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

**Module:** AUTH | **Total:** 74 cases | API: /api/auth/*

---

## SHEET 2/19: WORKSPACE -- Workspace Management (22 cases)

| **Feature** | Workspace Management |
|---|---|
| **Test requirement** | Tạo, xem danh sách, xem chi tiết, sửa tên, chuyển đổi, dashboard summary; validate (tên rỗng, 1 ký tự, quá dài, tiếng Việt, khoảng trắng, ký tự đặc biệt, emoji); loading, double click, empty state; access control (chưa đăng nhập, token hết hạn, không thuộc workspace, workspace bị xóa) |

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WS-01 | Tạo workspace thành công | 1. Đăng nhập test@example.com / Pass1234 2. Click Create Workspace 3. Nhập Name: My Workspace 4. Click Create | Hiển thị Workspace created successfully. Tự động chuyển sang workspace mới. Sidebar hiển thị workspace vừa tạo | Đã đăng nhập |
| WS-02 | Tạo workspace với tên rỗng | 1. Click Create Workspace 2. Để trống Name 3. Click Create | Hiển thị Name is required dưới ô Name. Nút không gửi được form | Đã đăng nhập |
| WS-03 | Tạo workspace với tên 1 ký tự | 1. Nhập Name: A 2. Click Create | Ghi nhận thực tế: nếu cho phép -> tạo thành công. Nếu yêu cầu tối thiểu -> Name must be at least N characters | Đã đăng nhập |
| WS-04 | Tạo workspace với tên quá dài | 1. Nhập Name: 256 ký tự 2. Click Create | Ghi nhận thực tế: nếu bị chặn -> Name must not exceed X characters | Đã đăng nhập |
| WS-05 | Tạo workspace với tên tiếng Việt có dấu | 1. Nhập Name: Xưởng Quảng Cáo Số 1 2. Click Create | Tạo thành công. Sidebar hiển thị đúng dấu, không lỗi font | Đã đăng nhập |
| WS-06 | Tạo workspace với tên toàn khoảng trắng | 1. Nhập Name: 5 dấu cách 2. Click Create | Ghi nhận: nếu FE trim -> báo Name is required. Nếu không -> có thể tạo workspace tên trắng (bug) | Đã đăng nhập |
| WS-07 | Tạo workspace với ký tự đặc biệt | 1. Nhập Name: WS @#$%^&*()! 2. Click Create | Tạo thành công. Tên hiển thị đúng, không bị escape | Đã đăng nhập |
| WS-08 | Tạo workspace với emoji | 1. Nhập Name: Brand 🚀🔥 2. Click Create | Ghi nhận: nếu hỗ trợ -> tạo thành công. Nếu không -> báo lỗi | Đã đăng nhập |
| WS-09 | Loading state khi tạo | 1. Nhập Name hợp lệ 2. Click Create | Nút chuyển thành Creating... kèm spinner, disable. Xong tự chuyển trang | Đã đăng nhập |
| WS-10 | Double click nút Create | 1. Nhập Name hợp lệ 2. Click Create 2 lần liên tiếp | Chỉ tạo 1 workspace. Lần 2 không tác dụng | Đã đăng nhập |
| WS-11 | User tạo workspace thứ 2 | 1. Đã có 1 workspace 2. Click Create Workspace 3. Nhập Name: Second WS 4. Click Create | Tạo thành công. Sidebar hiển thị cả 2. Switcher hoạt động | Đã có 1 workspace |
| WS-12 | Chưa đăng nhập truy cập /overview | 1. Logout 2. Truy cập /overview | Redirect về /login | Chưa đăng nhập |
| WS-13 | Token hết hạn | 1. Chờ token hết hạn 2. F5 | Tự refresh token HOẶC redirect /login + Session expired | Token hết hạn |
| WS-14 | Xem danh sách workspace | 1. Mở workspace switcher | Dropdown hiện tất cả workspace. Active được highlight | Đã có workspace |
| WS-15 | Empty state | 1. User mới vào /overview | Hiển thị No workspaces yet + CTA Create your first workspace | User mới |
| WS-16 | Chuyển đổi workspace | 1. Chọn workspace khác trong switcher | Sidebar + dashboard cập nhật, URL đổi | Có 2+ workspace |
| WS-17 | Xem chi tiết workspace | 1. Vào Workspace Settings | Hiển thị tên, loại, ngày tạo, subscription, members | Đang trong workspace |
| WS-18 | Sửa tên workspace | 1. Edit tên, đổi thành Updated WS 2. Save | Hiển thị Workspace updated. Tên sidebar đổi | User là Owner |
| WS-19 | Member thường sửa tên | 1. Member vào Settings | Không hiển thị nút Edit hoặc disable | User là Member |
| WS-20 | Dashboard summary | 1. Vào /overview | Hiển thị brands, content, posts, credit, post quota | Đang trong workspace |
| WS-21 | User không thuộc workspace truy cập | 1. Copy URL workspace user khác 2. Truy cập | You don't have access hoặc redirect | Không thuộc workspace |
| WS-22 | Workspace bị admin xóa | 1. Admin soft-delete workspace 2. User truy cập | Workspace mất khỏi sidebar, tự chuyển workspace khác | Workspace bị xóa |

**Module:** WORKSPACE | **Total:** 22 cases | API: /api/workspaces, /api/workspace-dashboard/summary

---

## SHEET 3/19: WORKSPACE-MEMBER -- Members & Invitations (34 cases)

| **Feature** | Workspace Members & Invitations |
|---|---|
| **Test requirement** | Mời thành viên qua email (hợp lệ, không tồn tại, đã là thành viên, sai định dạng, rỗng, khoảng trắng); quản lý lời mời (gửi, xem, thu hồi, chấp nhận, token không hợp lệ, hết hạn, đã accept, bị thu hồi, accept sai email); phân quyền (Owner/Manager/Member, đổi role, member không có quyền); quota thành viên; chuyển quyền sở hữu (hủy transfer, Manager không transfer); xóa thành viên; loading, double click, chưa đăng nhập, token hết hạn |

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| WM-01 | Mời thành viên bằng email hợp lệ | 1. Đăng nhập Owner 2. Vào Workspace Members 3. Click Invite Member 4. Nhập Email: member@example.com 5. Chọn Role: Member, Quota: Shared 6. Click Send Invitation | Hiển thị Invitation sent. Lời mời xuất hiện ở Pending. Người nhận có email | Owner, email chưa là thành viên |
| WM-02 | Mời email chưa đăng ký | 1. Nhập Email: newuser@example.com 2. Click Send | Invitation sent. User đăng ký sau vẫn accept được | Email chưa tồn tại |
| WM-03 | Mời email đã là thành viên | 1. Nhập Email của thành viên hiện tại 2. Click Send | User is already a member of this workspace | Đã là thành viên |
| WM-04 | Mời lại email đang có lời mời pending | 1. Email đã có lời mời 2. Mời lại | Ghi nhận: nếu chặn -> An invitation has already been sent. Nếu không -> gửi lại email mới | Có lời mời pending |
| WM-05 | Mời email rỗng | 1. Để trống Email 2. Click Send | Email is required dưới ô Email | -- |
| WM-06 | Mời email sai định dạng | 1. Nhập Email: notanemail 2. Click Send | Please enter a valid email address | -- |
| WM-07 | Mời email có khoảng trắng đầu/cuối | 1. Nhập "  member@example.com  " 2. Click Send | Ghi nhận: FE trim -> gửi thành công. Không trim -> có thể lỗi | -- |
| WM-08 | Mời email tiếng Việt có dấu | 1. Nhập người.dùng@thương-hiệu.vn 2. Click Send | Ghi nhận: hỗ trợ unicode email -> gửi thành công | -- |
| WM-09 | Member thường cố gửi lời mời | 1. Đăng nhập Member 2. Vào Members | Không hiển thị nút Invite | User là Member |
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
| WM-20 | Member cố đổi role | 1. Member vào Members | Không có nút Change Role | User là Member |
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
| WM-33 | Manager không thể xóa Owner | 1. Manager vào Members 2. Tìm Remove trên Owner | Không có nút Remove. API: No permission | User là Manager |
| WM-34 | Manager không thể transfer ownership | 1. Manager vào Members | Không có nút Transfer. API: Only owner can transfer | User là Manager |

**Module:** WORKSPACE-MEMBER | **Total:** 34 cases | API: /api/workspace-members, /api/workspace-invitations

---

## SHEET 4/19: PROFILE -- Profile Settings (59 cases)

| **Feature** | Profile Settings |
|---|---|
| **Test requirement** | CRUD profile: tạo (đầy đủ, tối thiểu), xem (danh sách, chi tiết, empty state, refresh), sửa (text, avatar, ProfileType, xóa avatar), xóa mềm & khôi phục; validate (Name rỗng, 1 ký tự, quá dài, khoảng trắng, emoji, ký tự đặc biệt, tiếng Việt, HTML/XSS; ProfileType rỗng, sai enum; avatar sai định dạng, quá dung lượng, URL lỗi, WebP); search (có/không kết quả, có dấu/không dấu, ký tự đặc biệt, khoảng trắng, xóa search, case-insensitive, search Company); filter (isDeleted, kết hợp search); access control (chưa đăng nhập, token hết hạn, user khác); loading, double click, mất mạng, double delete, restore active, xóa profile cuối cùng |

### 4.1 CREATE PROFILE (PR-01 -> PR-10)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-01 | Tạo profile đầy đủ + upload avatar | 1. Đăng nhập test@example.com / Pass1234 2. Settings -> Profiles -> Create 3. Name: My Profile, Type: Personal 4. Company: My Company, Bio: My bio 5. Upload avatar.jpg (JPEG <2MB) 6. Click Create | Profile created. Xuất hiện trong danh sách với avatar, tên, type, company | Đã đăng nhập |
| PR-02 | Tạo tối thiểu (Name + Type) | 1. Name: Minimal, Type: Personal 2. Để trống Company, Bio, không upload 3. Click Create | Tạo thành công. Avatar placeholder, các trường trống | Đã đăng nhập |
| PR-03 | Thiếu Name | 1. Để trống Name 2. Click Create | Name is required dưới ô Name | Đã đăng nhập |
| PR-04 | Thiếu ProfileType | 1. Name: Test, không chọn Type 2. Click Create | Profile type is required | Đã đăng nhập |
| PR-05 | ProfileType sai enum | 1. API POST với ProfileType = 99 | Server trả lỗi "Invalid profile type" | Đã đăng nhập |
| PR-06 | Tên 1 ký tự | 1. Name: A 2. Click Create | Ghi nhận: cho phép -> OK. Chặn -> Name must be at least N | Đã đăng nhập |
| PR-07 | Tên quá dài | 1. Name: 256 ký tự 2. Click Create | Ghi nhận: chặn -> Name must not exceed X | Đã đăng nhập |
| PR-08 | Tên tiếng Việt có dấu | 1. Name: Nguyễn Văn An 2. Click Create | Tạo thành công, hiển thị đúng dấu mọi nơi | Đã đăng nhập |
| PR-09 | Company tiếng Việt có dấu | 1. Name + Type hợp lệ 2. Company: Công ty TNHH Thương Mại & Dịch Vụ 3. Click Create | Tạo thành công, Company hiển thị đúng dấu | Đã đăng nhập |
| PR-10 | Tên toàn khoảng trắng | 1. Name: 5 dấu cách 2. Click Create | Ghi nhận: trim -> Name is required. Không -> tên trắng (bug) | Đã đăng nhập |

### 4.2 AVATAR & LOADING (PR-11 -> PR-17)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-11 | Avatar quá dung lượng | 1. Upload ảnh >10MB 2. Click Create | File size must not exceed 10MB | File >10MB |
| PR-12 | Upload file không phải ảnh | 1. Upload document.pdf | Input chỉ nhận image/*, không chọn được PDF | File PDF |
| PR-13 | Avatar định dạng WebP | 1. Upload avatar.webp 2. Click Create | Ghi nhận: hỗ trợ -> OK. Không -> báo lỗi định dạng | File .webp |
| PR-14 | Avatar URL thay vì upload | 1. AvatarUrl: https://picsum.photos/200 2. Click Create | Tạo thành công, avatar từ URL | -- |
| PR-15 | Avatar URL không hợp lệ | 1. AvatarUrl: not-a-valid-url | Ghi nhận: FE validate -> Please enter a valid URL | -- |
| PR-16 | Loading state | 1. Nhập đủ, Slow 3G 2. Click Create | Nút Creating... + spinner + disable. Xong -> thông báo | -- |
| PR-17 | Double click Create | 1. Click Create 2 lần | Chỉ tạo 1 profile | -- |

### 4.3 SPECIAL CHARACTERS (PR-18 -> PR-22)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-18 | Tên có emoji | 1. Name: Profile 🚀🔥 2. Click Create | Ghi nhận: hỗ trợ -> OK. Không -> lược bỏ hoặc lỗi | -- |
| PR-19 | Tên toàn ký tự đặc biệt | 1. Name: !@#$%^&*()_+-=[]{} 2. Click Create | Ghi nhận. Quan trọng: không XSS khi render | -- |
| PR-20 | Tên có dấu nháy đơn, ngoặc | 1. Name: O'Brien's Profile (Admin) 2. Click Create | Tạo thành công, Edit/Delete bình thường | -- |
| PR-21 | Tên chứa tab/newline | 1. Name copy từ Notepad có tab/newline 2. Click Create | Ghi nhận: input 1 dòng -> không nhập được newline | -- |
| PR-22 | Tên HTML/script (XSS) | 1. Name: `<script>alert('xss')</script>` 2. Click Create | KHÔNG có popup. Hiển thị text thuần | -- |

### 4.4 VIEW PROFILE (PR-23 -> PR-28)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-23 | Xem danh sách profile | 1. Settings -> Profiles | Danh sách card: avatar, tên, type, company, nút Create | Có profile |
| PR-24 | Empty state | 1. User mới -> Profiles | No profiles yet + CTA Create your first profile | Chưa có profile |
| PR-25 | Xem chi tiết | 1. Click 1 profile | Avatar lớn, Name, Type, Company, Bio. Nút Edit, Delete | -- |
| PR-26 | Verify dữ liệu vừa tạo | 1. Tạo profile với các giá trị cụ thể 2. Click vào xem | Mọi trường khớp dữ liệu đã nhập | -- |
| PR-27 | Avatar URL vs upload | 1. So sánh 2 profile | Cả 2 hiển thị đúng, không lẫn lộn | -- |
| PR-28 | Refresh trang | 1. F5 pages Profiles | Danh sách giữ nguyên, avatar load đúng | -- |

### 4.5 SEARCH (PR-29 -> PR-37)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-29 | Search có kết quả | 1. Search: Nguyen 2. Enter | Chỉ hiện profile tên chứa Nguyen | Có profile phù hợp |
| PR-30 | Search không kết quả | 1. Search: zzzKhôngTồnTại 2. Enter | No profiles found + Clear search | -- |
| PR-31 | Search có dấu | 1. Search: Nguyễn (có dấu) | Ghi nhận: phân biệt dấu -> chỉ hiện có dấu | Có cả 2 loại |
| PR-32 | Search không dấu | 1. Search: Nguyen (không dấu) | Ghi nhận: không phân biệt -> hiện cả 2 | Có cả 2 loại |
| PR-33 | Search ký tự đặc biệt | 1. Search: @#$% | Ghi nhận. Không crash, không SQL injection | -- |
| PR-34 | Search khoảng trắng | 1. Search: " " (1 dấu cách) | Ghi nhận: trim -> toàn bộ. Không -> kết quả lạ | -- |
| PR-35 | Xóa search | 1. Đã search -> click X | Danh sách về đầy đủ, search trống | Đã search |
| PR-36 | Search case-insensitive | 1. Search: nguyen 2. Search: NGUYEN | Cả 2 cùng kết quả | -- |
| PR-37 | Search theo Company | 1. Search: My Company (tên company) | Ghi nhận: API chỉ search Name. Sẽ không có kết quả nếu Name khác | -- |

### 4.6 FILTER + SEARCH (PR-38 -> PR-42)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-38 | Filter isDeleted=true | 1. Bật Show Deleted | Chỉ hiện profile deleted, badge Deleted, nút Restore | Có profile deleted |
| PR-39 | Filter deleted khi chưa xóa gì | 1. Bật Show Deleted | No deleted profiles | Chưa có deleted |
| PR-40 | Filter mặc định (active) | 1. Có 1 deleted + 2 active 2. Vào Profiles | Chỉ hiện 2 active, tổng = 2 | -- |
| PR-41 | Search + Filter deleted | 1. Show Deleted + Search tên deleted | Chỉ hiện profile deleted khớp tên | Có cả active và deleted cùng tên |
| PR-42 | Search + Filter active | 1. Filter mặc định + Search tên deleted | Không hiển thị gì (profile đó đã xóa) | -- |

### 4.7 EDIT PROFILE (PR-43 -> PR-50)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-43 | Sửa text fields | 1. Edit -> đổi Name, Bio, Company 2. Save | Profile updated. Thông tin mới hiển thị | -- |
| PR-44 | Sửa đổi avatar | 1. Edit -> upload ảnh mới 2. Save | Avatar mới thay cũ | Đang có avatar |
| PR-45 | Sửa xóa avatar | 1. Edit -> Remove avatar -> Save | Avatar về placeholder | Đang có avatar |
| PR-46 | Sửa ProfileType (Personal->Business) | 1. Edit -> đổi Type 2. Save | Type hiển thị Business | -- |
| PR-47 | Sửa tên -> khoảng trắng | 1. Edit -> Name: 5 dấu cách 2. Save | Ghi nhận: trim -> Name required. Không -> tên trắng (bug) | -- |
| PR-48 | Sửa upload ảnh quá dung lượng | 1. Edit -> upload >10MB 2. Save | File size must not exceed 10MB. Text giữ nguyên | -- |
| PR-49 | Loading khi sửa | 1. Edit -> Slow 3G -> Save | Saving... + spinner + disable | -- |
| PR-50 | Mất mạng khi Save | 1. Edit -> ngắt mạng -> Save | Network error. Form giữ data. Có mạng -> Save OK | -- |

### 4.8 DELETE & RESTORE (PR-51 -> PR-55)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-51 | Xóa mềm | 1. Delete -> Confirm | Profile deleted. Show Deleted -> hiện với badge | Có 2+ profile |
| PR-52 | Xóa profile duy nhất | 1. Chỉ còn 1 -> Delete | Ghi nhận: chặn -> Cannot delete only profile | 1 profile |
| PR-53 | Double delete | 1. Profile đã deleted -> DELETE nữa | Ghi nhận: Already deleted / Not found | Đã deleted |
| PR-54 | Khôi phục | 1. Show Deleted -> Restore | Profile restored. Về active, đầy đủ data | Đã deleted |
| PR-55 | Restore active | 1. Active profile -> RESTORE | Ghi nhận: chặn -> Profile is not deleted | Active |

### 4.9 ACCESS CONTROL (PR-56 -> PR-59)

| TC-ID | Description | Procedure | Expected Results | Pre-conditions |
|--------|-------------|-----------|------------------|----------------|
| PR-56 | Chưa đăng nhập | 1. Logout -> truy cập /profiles | Redirect /login | -- |
| PR-57 | Token hết hạn | 1. Vào Profiles -> hết hạn -> thao tác | Refresh HOẶC redirect /login | -- |
| PR-58 | User A xem profile B | 1. A copy URL profile B -> truy cập | You don't have permission | A != B |
| PR-59 | User A POST cho B | 1. A gửi POST /api/profiles/user/{B} | 403 Forbidden | A != B |

**Module:** PROFILE | **Total:** 59 cases | API: /api/profiles

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
| 3 | WORKSPACE-MEMBER | 34 | Complete |
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

**Module:** PAYMENT & SUBSCRIPTION | **Total:** 55 cases | **Pages:** `/pricing`, `/profiles/[id]?section=subscription` | **API:** POST `/payment/checkout`, POST `/payment/business-workspace-checkout`, POST `/payment/business-workspace-checkout/sync`, POST `/payment/callback`, POST `/payment/webhook`, GET `/payment/history`, GET `/payment/subscription/current`


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

**Module:** CREDIT, QUOTA & WALLET | **Total:** 65 cases | **Pages:** `/credit-pack`, `/credit-history`, `/profiles/[id]?section=subscription` | **API:** POST `/payment/checkout` (CreditPack), GET `/credit-usage/wallet`, GET `/credit-usage/daily-summary`, GET `/credit-usage`, GET `/quota/workspace/current`

