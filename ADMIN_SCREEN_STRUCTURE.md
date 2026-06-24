# Tài Liệu Cấu Trúc Màn Hình AISAM Admin

Phạm vi: Admin là một web app riêng (`FEAdmin`), chạy độc lập với user frontend (`AISAM-FE`). Hai frontend dùng chung backend API và chung database, nhưng có layout, session, route guard và navigation riêng.

## 1. Tổng Số Màn

| Nhóm chức năng admin | Số màn trong nhóm | Ghi chú ngắn |
| --- | ---: | --- |
| Xác thực admin | 1 | Admin Login |
| Tổng quan vận hành | 1 | Admin Dashboard |
| Quản lý người dùng | 2 | User List, User Detail |
| Quản lý business data | 1 | Profiles |
| Quản lý subscription/payment | 2 | Subscriptions, Payments |
| Công cụ demo/vận hành | 1 | Demo Data |
| **Tổng route page admin** | **8** | Không tính tab/dialog nội bộ |

## 2. Danh Sách Màn Admin

### 1. `/login`

Màn đăng nhập riêng cho quản trị viên, dùng email/password và kiểm tra role `Admin` trước khi vào hệ thống. Nếu user không phải admin thì xóa admin session và hiển thị lỗi truy cập.

File path đề xuất: `admin-app/src/app/login/page.tsx` hoặc `admin-app/src/pages/Login.tsx` tùy framework.

### 2. `/`

Màn dashboard tổng quan của admin, hiển thị các module quản trị: Users, Profiles, Payments, Subscriptions, Demo Data và trạng thái backend contract. Đây là trang vào chính sau khi admin đăng nhập thành công.

File path đề xuất: `admin-app/src/app/page.tsx` hoặc `admin-app/src/pages/Dashboard.tsx`.

### 3. `/users`

Màn danh sách người dùng để admin tìm kiếm, lọc, sort, phân trang và xem trạng thái tài khoản/gói đăng ký. Action quản trị chỉ bật khi backend admin users API sẵn sàng.

File path đề xuất: `admin-app/src/app/users/page.tsx` hoặc `admin-app/src/pages/users/UserList.tsx`.

### 4. `/users/[id]`

Màn chi tiết user, dùng để xem thông tin user, profile liên quan, subscription và lịch sử payment. Có thể có tab nội bộ `Profiles`, `Subscription`, `Payments`, `Audit`.

File path đề xuất: `admin-app/src/app/users/[id]/page.tsx` hoặc `admin-app/src/pages/users/UserDetail.tsx`.

### 5. `/profiles`

Màn quản lý profile toàn hệ thống, cho admin xem/lọc profile, trạng thái profile và dữ liệu vận hành liên quan. Đây là admin operations page, tách khỏi màn profile của user app.

File path đề xuất: `admin-app/src/app/profiles/page.tsx` hoặc `admin-app/src/pages/profiles/ProfileList.tsx`.

### 6. `/payments`

Màn quản lý payment ở cấp hệ thống, dùng để theo dõi giao dịch, trạng thái thanh toán, mã đơn và lỗi callback. Không dùng lại user billing UI.

File path đề xuất: `admin-app/src/app/payments/page.tsx` hoặc `admin-app/src/pages/payments/PaymentList.tsx`.

### 7. `/subscriptions`

Màn quản lý subscription toàn hệ thống, dùng để xem gói đang active, trạng thái gia hạn, hạn mức và thông tin quota liên quan. Các cập nhật subscription phải phụ thuộc backend admin contract.

File path đề xuất: `admin-app/src/app/subscriptions/page.tsx` hoặc `admin-app/src/pages/subscriptions/SubscriptionList.tsx`.

### 8. `/demo-data`

Màn công cụ seed demo data có cảnh báo rủi ro và confirm trước khi chạy action. Button seed phải disabled hoặc báo backend-not-ready cho tới khi backend expose endpoint admin tools.

File path đề xuất: `admin-app/src/app/demo-data/page.tsx` hoặc `admin-app/src/pages/demo/DemoData.tsx`.

## 3. Luồng Chính Admin

```text
/login
  -> kiểm tra credential
  -> kiểm tra role Admin
    -> /
      -> /users
        -> /users/[id]
      -> /profiles
      -> /payments
      -> /subscriptions
      -> /demo-data
    -> non-admin
      -> /login
```

## 4. Navigation Admin

Admin có layout/navigation riêng, không dùng sidebar/header của `AISAM-FE`.

Sidebar admin có 6 mục:

| Mục | Route | Mục đích |
| --- | --- | --- |
| Dashboard | `/` | Tổng quan vận hành và trạng thái backend |
| Users | `/users` | Quản lý danh sách và chi tiết user |
| Profiles | `/profiles` | Quản lý profile toàn hệ thống |
| Payments | `/payments` | Theo dõi thanh toán |
| Subscriptions | `/subscriptions` | Theo dõi gói đăng ký/quota |
| Demo Data | `/demo-data` | Seed dữ liệu demo có guard |

Header admin:

- Hiển thị tên admin, role và trạng thái session.
- Có action Logout admin.
- Không có profile selector.
- Không gửi `X-Profile-Id` trong request admin.

File định nghĩa đề xuất:

- `admin-app/src/components/layout/AdminShell.tsx`
- `admin-app/src/components/layout/AdminSidebar.tsx`
- `admin-app/src/components/layout/AdminHeader.tsx`
- `admin-app/src/config/adminNavigation.ts`
- `admin-app/src/auth/adminGuard.ts`
- `admin-app/src/api/adminClient.ts`

## 5. Ghi Chú

- Tài liệu này không còn giả định admin nằm trong `AISAM-FE`.
- `AISAM-FE` và `FEAdmin` dùng chung backend API và chung database.
- Backend phải là nơi enforce quyền `Admin`; frontend guard chỉ là lớp UX/bảo vệ sớm.
- Nếu admin app deploy ở subdomain riêng, route public sẽ là ví dụ `https://admin.aisam.vn/login`, còn route nội bộ vẫn là `/login`, `/users`, `/payments`.
- Nếu admin app deploy dưới path riêng, route public có thể là `/admin/login`, `/admin/users`; khi đó chỉ cần thêm base path `/admin`, không đổi cấu trúc màn.
- Admin session nên tách khỏi user session, ví dụ `admin_access_token`, `admin_refresh_token`, `admin_user`.
- Chỉ role `Admin` được truy cập admin app; non-admin phải bị chặn.
- Backend local hiện chưa expose đầy đủ admin users/payment/subscription/demo seed controllers, nên các action quản trị nên hiển thị backend-dependent.
- Admin API không được gọi endpoint suy diễn khi backend contract chưa tồn tại.
- Admin app không cần active profile và không gửi `X-Profile-Id`.
- Các endpoint auth có thể dùng chung với user app, nhưng sau login FEAdmin phải verify role trước khi lưu admin session.
- Dữ liệu user/profile/payment/subscription là dữ liệu thật trong database chung; mọi màn admin cần tránh mock dữ liệu production.
