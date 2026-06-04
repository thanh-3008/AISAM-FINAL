# AISAM Frontend (AISAM-FE)

This is a [Next.js](https://nextjs.org) project for the AISAM (AI-Powered Social Media Advertising Manager) Frontend, bootstrapped with `create-next-app` using Next.js 15, React 19, and Tailwind CSS v4.

## Getting Started

First, make sure you have installed the dependencies:

```bash
npm install
```

Then, run the development server:

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Project Structure

The project follows the standard Next.js App Router structure:

```text
src/
+---app/
|   |   favicon.ico
|   |   globals.css             # Tailwind v4 configuration and global styles
|   |   layout.tsx              # Root layout (fonts, metadata, etc.)
|   |   page.tsx                # Landing Page
|   |   
|   +---(auth)/                 # Authentication routes (US-01, US-02)
|   |   +---login/
|   |   |       page.tsx        # Login Page
|   |   |       
|   |   \---register/
|   |           page.tsx        # Register Page
|   |           
|   \---(dashboard)/            # Dashboard routes (require login)
|       |   layout.tsx          # Dashboard layout (Sidebar + Header)
|       |   
|       +---ai-studio/          # AI Content Generation (US-22)
|       +---campaigns/          # Smart Campaigns Management
|       +---content/            # Content Library (US-20)
|       \---dashboard/          # Main Dashboard Overview (US-43)
|               page.tsx
|               
\---components/
    \---layout/                 # Reusable layout components
            Header.tsx          # Dashboard Header
            Sidebar.tsx         # Dashboard Sidebar Navigation
```

## Styling & Design System
- The project uses **Tailwind CSS v4**. All custom tokens (colors, typography, spacing) are defined in `src/app/globals.css` using the `@theme` directive.
- Check out `DESIGN_SYSTEM.md` for the complete design guidelines, color palettes, and typography rules.
- We use [Material Symbols Outlined](https://fonts.google.com/icons) for iconography.

## Hướng dẫn sử dụng (Usage Guide)

Sau khi khởi chạy dự án, bạn có thể truy cập các đường dẫn sau để xem và kiểm tra các màn hình đã được xây dựng:

1. **Landing Page (`/`)**: Trang chủ giới thiệu sản phẩm.
   - Truy cập: [http://localhost:3000](http://localhost:3000)

2. **Trang Đăng Nhập (`/login`)**: Giao diện đăng nhập (US-02).
   - Truy cập: [http://localhost:3000/login](http://localhost:3000/login)
   - Lưu ý: Hiện tại là giao diện tĩnh, khi nhấn nút "Sign In" hệ thống sẽ giả lập thời gian tải và hiển thị thông báo thành công nếu nhập đúng email `demo@aisam.ai` và mật khẩu `password123`.

3. **Trang Đăng Ký (`/register`)**: Giao diện tạo tài khoản mới (US-01).
   - Truy cập: [http://localhost:3000/register](http://localhost:3000/register)
   - Có tích hợp thanh hiển thị độ mạnh mật khẩu và giả lập quá trình đăng ký.

4. **Trang Dashboard (`/dashboard`)**: Bảng điều khiển chính (US-43).
   - Truy cập: [http://localhost:3000/dashboard](http://localhost:3000/dashboard)
   - Bao gồm Sidebar, Header, hiển thị các thông số tổng quan (Stats), danh sách bài đăng gần đây, thanh thao tác nhanh và Quota sử dụng.

*Lưu ý: Mọi dữ liệu hiện tại đang là Mock Data (dữ liệu giả lập) trên Frontend. Chức năng kết nối API tới Backend sẽ được tích hợp trong các giai đoạn tiếp theo.*
