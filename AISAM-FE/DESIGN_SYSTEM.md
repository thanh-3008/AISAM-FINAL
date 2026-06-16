# AISAM Design System

**Mục tiêu:** Tài liệu này đóng vai trò là "Single Source of Truth" (Nguồn chân lý duy nhất) cho toàn bộ phong cách thiết kế, màu sắc, typography và UI pattern của dự án AISAM.

## 1. Brand & Style
- **Corporate Modernism & AI-Centric**: Chuyên nghiệp, đáng tin cậy nhưng mang hơi hướng tương lai, thông minh.
- **Glassmorphism**: Hiệu ứng kính mờ (backdrop-blur) ở các Layer cao (Modal, Popover, AI Studio).
- **Minimalism**: Tổ chức theo grid chặt chẽ, tối giản để tối ưu không gian cho dữ liệu lớn.

## 2. Bảng Màu (Colors)
Sử dụng **Modern SaaS Blue** làm chủ đạo và **AI Purple** làm điểm nhấn cho các tính năng AI.

### Surface (Nền)
- `surface`: `#faf8ff` (Nền chính)
- `surface-dim`: `#d8d9e6`
- `surface-container-lowest`: `#ffffff`
- `surface-container`: `#ecedfa`
- `on-surface`: `#191b24` (Chữ trên nền chính)
- `on-surface-variant`: `#424656`

### Primary (Màu chính - SaaS Blue)
- `primary`: `#004ccd`
- `on-primary`: `#ffffff`
- `primary-container`: `#0f62fe`

### Secondary (Màu phụ - AI Purple)
- `secondary`: `#731be5`
- `on-secondary`: `#ffffff`
- `secondary-container`: `#8d42ff`

### Semantic (Trạng thái)
- `success-green`: `#198038`
- `warning-amber`: `#F1C21B`
- `danger-red`: `#DA1E28`
- `error`: `#ba1a1a`

## 3. Typography (Plus Jakarta Sans)
- **Display Large**: 48px, Bold (700), Line Height 60px, Tracking -0.02em
- **Headline Large**: 32px, Bold (700), Line Height 40px, Tracking -0.01em
- **Headline Medium**: 24px, Semi-Bold (600), Line Height 32px
- **Headline Small**: 20px, Semi-Bold (600), Line Height 28px
- **Body Large**: 18px, Regular (400), Line Height 28px
- **Body Medium**: 16px, Regular (400), Line Height 24px (Văn bản tiêu chuẩn)
- **Body Small**: 14px, Regular (400), Line Height 20px
- **Label Medium**: 12px, Semi-Bold (600), Line Height 16px, Tracking 0.05em, Uppercase
- **Label Small**: 11px, Medium (500), Line Height 14px, Tracking 0.05em, Uppercase

## 4. Spacing & Layout
- **Base Grid**: 8px
- **Sidebar Width**: 260px
- **Gutter (Padding mặc định)**: 24px
- **Radius (Bo góc)**:
  - Base (`rounded`): 0.5rem (8px) cho Input, Button.
  - Large (`rounded-lg`): 1rem (16px) cho Card, Container lớn.
  - Full (`rounded-full`): 9999px cho Badges, Chips.

## 5. UI Effects
- **Level 1 Elevation (Card)**: Shadow mờ `0 4px 12px rgba(0,0,0,0.05)`, viền `1px outline-variant`.
- **Level 2 Elevation (AI/Modal)**: Hiệu ứng `.glass-panel` với `backdrop-filter: blur(12px)` và viền sáng mờ.
- **AI Glow**: Lớp viền và inner-shadow ánh tím `rgba(115,27,229,0.2)` đặc trưng cho các phân vùng do AI sinh ra.

---
*(Tài liệu này được đồng bộ trực tiếp vào `AISAM-FE/src/app/globals.css` để sử dụng cho Tailwind CSS v4).*
