# Báo Cáo So Sánh Tính Năng: Web (AISAM-FE) và Mobile (AISAM-MB)

Tài liệu này trình bày phân tích chi tiết và đối chiếu các tính năng đã được triển khai trên hai nền tảng Frontend của hệ thống AISAM: **Next.js Web (AISAM-FE)** và **Flutter Mobile (AISAM-MB)**.

---

## 1. Module Xác Thực (Authentication)
| Tính năng | AISAM-FE (Web) | AISAM-MB (Mobile) | Đánh giá |
| :--- | :--- | :--- | :--- |
| Đăng nhập (Email/Password) | Đầy đủ | Đầy đủ | Đồng nhất |
| Đăng ký tài khoản | Đầy đủ | Đầy đủ | Đồng nhất |
| Quên mật khẩu | Đầy đủ | Đầy đủ | Đồng nhất |
| OAuth Callback (Social Login) | Hỗ trợ (`social-callback`) | Chưa có màn hình cụ thể | Mobile hiện tại tập trung vào xác thực cơ bản qua API nội bộ |

## 2. Quản Lý Workspace & Team
| Tính năng | AISAM-FE (Web) | AISAM-MB (Mobile) | Đánh giá |
| :--- | :--- | :--- | :--- |
| Tạo / Chọn Workspace | Đầy đủ | Đầy đủ | Cả hai đều hỗ trợ đổi Workspace context |
| Quản lý thành viên (Team/Members) | Hỗ trợ chi tiết (thêm, xóa, phân quyền) | **Không có** | Mobile được thiết kế tinh gọn, ưu tiên thao tác cá nhân thay vì quản trị hệ thống |
| Dashboard riêng cho Workspace | Có (`workspace-dashboard`) | Gộp chung vào Home Dashboard | Phù hợp với UI không gian nhỏ của Mobile |

## 3. Brand & Profile (Hồ Sơ Thương Hiệu)
| Tính năng | AISAM-FE (Web) | AISAM-MB (Mobile) | Đánh giá |
| :--- | :--- | :--- | :--- |
| Quản lý Profile (Công ty/Cá nhân) | Đầy đủ | Đầy đủ (`profile_list`, `create_profile`) | Đồng nhất |
| Quản lý Brand | Đầy đủ | Đầy đủ (`brand_list`, `create_brand`) | Đồng nhất |
| Quản lý Sản phẩm (Product) | Đầy đủ | Đầy đủ (`product_list`, `create_product`) | Đồng nhất |

## 4. Quản Lý Nội Dung & Chiến Dịch (Content & Campaigns)
| Tính năng | AISAM-FE (Web) | AISAM-MB (Mobile) | Đánh giá |
| :--- | :--- | :--- | :--- |
| Danh sách nội dung (Draft, Published...) | Đầy đủ | Đầy đủ (`content_list`) | Đồng nhất |
| Trình soạn thảo Nội dung | Editor giàu tính năng (Web) | Editor cơ bản (`content_editor`) | Web mạnh hơn về biên tập chi tiết |
| Quản lý Chiến dịch (Campaigns) | Hỗ trợ quản lý theo cụm | **Không có** | Mobile bỏ qua khái niệm Campaign để thao tác nhanh từng bài viết |
| Tự động hóa (Automation) | Có chức năng lập lịch / quy tắc | **Không có** | Tính năng nâng cao chỉ có trên Web |

## 5. AI Generate & Chatbot (Tương Tác AI)
> [!IMPORTANT]
> Đây là một trong những điểm khác biệt lớn nhất về mặt trải nghiệm (UX) giữa hai nền tảng.

| Tính năng | AISAM-FE (Web) | AISAM-MB (Mobile) | Đánh giá |
| :--- | :--- | :--- | :--- |
| Trợ lý tạo nội dung (AI Draft) | Tích hợp chặt chẽ trên màn hình `ai-generate` | Tách rời thành `ai_generate_screen` | Cùng gọi API sinh nội dung |
| AI Chat | Nằm một bên panel trong lúc tạo nội dung. Dùng `sessionStorage` để giữ luồng tạm thời. Không có Inbox tổng. | **Module Chat độc lập (Mạnh mẽ hơn)** | Mobile xây dựng hẳn một ứng dụng nhắn tin thu nhỏ (`conversation_list_screen` và `chat_screen`) quản lý toàn bộ lịch sử trò chuyện. |

## 6. Lịch & Phê Duyệt (Calendar & Approvals)
| Tính năng | AISAM-FE (Web) | AISAM-MB (Mobile) | Đánh giá |
| :--- | :--- | :--- | :--- |
| Lịch nội dung (Calendar) | Chế độ xem Tháng/Tuần trực quan | Chế độ xem Lịch thu gọn dạng Grid & List theo ngày | Mobile tối ưu không gian dọc hiển thị dạng danh sách thẻ (Card) |
| Phê duyệt nội dung (Approvals) | Bảng danh sách chờ duyệt | Danh sách List/Detail duyệt nhanh | Đồng nhất về luồng nghiệp vụ |

## 7. Mạng Xã Hội (Social Integration)
| Tính năng | AISAM-FE (Web) | AISAM-MB (Mobile) | Đánh giá |
| :--- | :--- | :--- | :--- |
| Liên kết kênh Social (Facebook, TikTok...) | Có module quản lý kết nối | Chưa hỗ trợ UI liên kết | Việc cấp quyền đăng bài chéo hiện phụ thuộc vào Web |
| Xem trước bài đăng mạng xã hội | Chi tiết mô phỏng giao diện MXH | Mô phỏng cơ bản | - |

## 8. Tín Dụng & Thanh Toán (Credit & Monetization)
| Tính năng | AISAM-FE (Web) | AISAM-MB (Mobile) | Đánh giá |
| :--- | :--- | :--- | :--- |
| Quản lý gói Credit | Màn hình `credit-pack` | **Không có** | Quản lý hóa đơn chỉ trên Web |
| Lịch sử trừ Credit | Màn hình `credit-history` | **Không có** | Tính minh bạch hóa đơn đặt tại Web |

---

## 🎯 TỔNG KẾT VÀ NHẬN ĐỊNH

1. **Vai trò của AISAM-FE (Web): Khối Quản Trị Hệ Thống (Command Center)**
   - Phù hợp với các tác vụ phức tạp: Biên tập chi tiết, lập chiến dịch (Campaign), liên kết mạng xã hội (Social OAuth), thiết lập tự động hóa, thanh toán và quản trị thành viên.
   
2. **Vai trò của AISAM-MB (Mobile): Khối Tác Nghiệp Nhanh (On-the-go Companion)**
   - Ứng dụng di động được thiết kế tập trung mạnh vào các hành vi thao tác nhanh: **Duyệt bài viết (Approval)**, xem lịch chạy nội dung hôm nay (Calendar), và **Trò chuyện với AI (AI Chat)**.
   - Điểm sáng của Mobile là biến AI thành một "người đồng hành" qua Module Chat độc lập (có hộp thư lưu trữ), thay vì chỉ là công cụ panel phụ như trên Web.
   - Mobile lược bỏ hoàn toàn những tính năng nặng về cấu hình (Team Management, Automation, Billing, Social Linking) nhằm giữ app nhẹ, mượt mà và tập trung đúng mục đích nghiệp vụ di động.
