SOFTWARE REQUIREMENT SPECIFICATION (SRS)
AI-POWERED SOCIAL MEDIA ADVERTISING MANAGER (AISAM)
BẢN HỢP NHẤT GIỮA SOURCE CODE HIỆN TẠI VÀ ĐỊNH HƯỚNG PHÁT TRIỂN TƯƠNG LAI

Document Control
Tài liệu này được xây dựng theo nguyên tắc source code hiện tại là baseline triển khai. Các chức năng đã có trong source được mô tả trong nhóm Current Implemented Features. Các chức năng xuất hiện trong tài liệu yêu cầu gốc nhưng chưa tồn tại đầy đủ trong source được phân loại rõ là Planned Features, Future Enhancement, Proposed Advanced AI Features hoặc Optional Enterprise Features. Tài liệu không mô tả các chức năng chưa có như một phần đã hoàn thành.

---

## 📊 QUICK OVERVIEW - PHÂN TÍCH HỆ THỐNG

### ✅ **NHÓM 1: CHỨC NĂNG ĐÃ TRIỂN KHAI (Currently Implemented - 14 chức năng chính)**

| # | Chức Năng | Chi Tiết |
|---|-----------|---------|
| 1 | Xác thực & Tài khoản | Đăng ký, đăng nhập, Google OAuth, JWT Bearer, refresh token, session management |
| 2 | Quản lý Hồ sơ & Gói | Profile, subscription (Free/Plus/Premium/PlusTrial), PayOS payment |
| 3 | Kết nối Facebook | OAuth, liên kết page, ad accounts, targets |
| 4 | Quản lý Brand | CRUD brand, assign/unassign to team, brand context cho AI |
| 5 | Quản lý Sản phẩm | CRUD product, upload ảnh Supabase, liên kết brand |
| 6 | Thư viện Nội dung | CRUD content, 3 loại (TextOnly/ImageText/VideoText), clone, restore |
| 7 | AI sinh Text | Google Gemini cho draft, chat, improve content |
| 8 | AI sinh Ảnh | Google Vertex AI Imagen (cho ImageText) |
| 9 | AI Chat & Improve | Chat với context brand/product, cải thiện nội dung |
| 10 | Duyệt Nội dung | Workflow approval, team permissions, notification |
| 11 | Đăng bài & Lập lịch | Publish ngay hoặc schedule (lặp lại), background service |
| 12 | Quảng cáo Facebook | Campaign, ad set, ad creative, ad, preview, reports, insights |
| 13 | Dashboard & Reports | Dashboard stats, analytics cơ bản, Facebook insights |
| 14 | Admin Tools | Quản lý user, payment, subscription, seed demo data |

---

### 🔄 **NHÓM 2: CHỨC NĂNG ĐANG/SẮP PHÁT TRIỂN (Planned/Future Features)**

| # | Chức Năng | Trạng Thái | Dự kiến Khi nào |
|---|-----------|-----------|-----------------|
| 1 | Mở rộng AI (GPT-4o, DALL-E) | Planned | Mid-term |
| 2 | Sentiment Analysis & Trend Prediction | Future Enhancement | Long-term |
| 3 | AI Video Generation | Proposed Feature | Long-term |
| 4 | AI Strategy & Real-time Optimization | Optional Enterprise | Long-term |
| 5 | Instagram/TikTok/Twitter Support | Planned Platform | Mid-term |
| 6 | Dynamic Subscription Plans (CRUD) | Planned Admin Feature | Mid-term |
| 7 | Stripe/VNPay Payment Gateway | Optional Future | Long-term |
| 8 | Team Leader Single-Owner Governance | Planned Rule | Short-term |

---

### ⚠️ **NHÓM 3: NHỮNG ĐIỀU CẦN LÀM RÕ & KIỂM TRA (19 Specification Questions)**

#### **Yêu Cầu Cũ (8 items):**

| # | Vấn Đề | Tác Động | Hành Động Cần Thiết |
|----|--------|---------|-------------------|
| 1️⃣ | **Team Permission Model** | Ai được quyền duyệt/đăng/quản lý team? | Kiểm tra code, enforce rõ 1 Leader per team |
| 2️⃣ | **Subscription Plans** | Plans hiện dùng enum, có thể config động? | Xác định nhu cầu business, có cần CRUD hay không |
| 3️⃣ | **Instagram Implementation** | Enum có Instagram nhưng provider chưa ready | Quyết định có phát triển Instagram không |
| 4️⃣ | **Background Job Reliability** | Lịch đăng bài có retry policy, monitoring? | Kiểm tra ScheduledPostingBackgroundService |
| 5️⃣ | **AI Video Flow** | VideoUrl field có nhưng chưa sinh video AI | Quyết định phát triển AI video khi nào |
| 6️⃣ | **Budget Auto-Optimization** | Có tự động điều chỉnh ngân sách quảng cáo không? | Thêm vào roadmap nếu cần |
| 7️⃣ | **Provider Architecture** | AI/Payment/Social providers có abstraction layer? | Cân nhắc refactor để dễ mở rộng |
| 8️⃣ | **Test Coverage** | Các luồng chính có test đầy đủ không? | Bổ sung unit test & integration test |

---

#### **Yêu Cầu Mới (11 items):**

| # | Vấn Đề | Chi Tiết Cần Làm Rõ | Ưu Tiên |
|----|--------|-------------------|---------|
| 🤖 | **AI Quota Management** | Tính theo số lần API / token / số bài / combo? Reset theo ngày/tuần/tháng? Hard/soft limit? | 🔴 High |
| ✔️ | **Leader Approval Workflow** | Content status flow? SLA bao lâu? Xử lý Leader vắng mặt? Quy tắc chuyển quyền? | 🔴 High |
| 📝 | **Prompting Strategy** | Template prompt chuẩn? Lưu history? Versioning? Ai được chỉnh sửa? | 🟡 Medium |
| 📚 | **Content Library** | Lưu tất cả revisions hay latest only? Phân quyền chi tiết? Version control? Soft/hard delete policy? | 🔴 High |
| 🔐 | **Meta OAuth & Token** | Refresh token strategy? Scope tối thiểu? Encrypt/rotate keys? Token revocation? | 🔴 High |
| 📅 | **Scheduled Posts** | Cơ chế chạy (Cron/Queue/Service)? Frequency check? Retry policy + DLQ? Meta rate limits? | 🔴 High |
| 🎯 | **Ads Automation** | UI fields → Meta params mapping? Validation rules? Manual approval trước tạo ads? Edit after created? | 🟡 Medium |
| 📊 | **Analytics** | API nào? Data latency bao lâu? Rate limits? Caching strategy? | 🟡 Medium |
| 💳 | **Payment & Subscription** | Tính tiền calendar/30 days? Proration logic? Refund policy? Error handling? | 🔴 High |
| 🏗️ | **Data Model** | Team-User-Leader relationship? 1 Brand/User? N Products? Multi-tenant isolation? | 🔴 High |
| 🔒 | **Security & RBAC** | Roles/permissions? Audit log retention? Data encryption? API security (rate limit, key rotation)? | 🔴 High |

---

### 🎯 **WORKFLOW CHÍNH CỦA HỆ THỐNG**

```
┌─────────────┐
│  User Login │
└──────┬──────┘
       │
       ▼
┌─────────────────────┐
│  Select Profile     │
└──────┬──────────────┘
       │
       ├──────────────────┬──────────────────┬──────────────┐
       │                  │                  │              │
       ▼                  ▼                  ▼              ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────┐
│ Brand & Prod │  │   Content    │  │  Ads Setup   │  │ Dashboard│
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └──────────┘
       │                  │                 │
       │                  ├─ AI Generate    │
       │                  ├─ Review/Approve │
       │                  │                 │
       │                  ├─ Publish Now    │
       │                  ├─ Schedule       │
       │                  │                 │
       │                  └─ Use as Ad Creative
       │                                    │
       ▼                                    ▼
   [PUBLISHED]                      [AD CAMPAIGN ACTIVE]
                                            │
                                            ▼
                                    [TRACKING & ANALYTICS]
```

---

### 💡 **RECOMMEND: Ưu Tiên Phát Triển Ngắn Hạn (Next 3 Months)**

1. **🔒 Enforce Team Governance** - Kiểm soát quyền hạn rõ ràng (Leader/Member roles)
2. **✅ Hoàn thiện Error Handling** - Retry policy, logging, fallback strategy
3. **🧪 Tăng Test Coverage** - Unit/integration tests cho payment, publishing, approval, AI
4. **🔌 Provider Abstraction** - Chuẩn hóa kiến trúc AI/Social/Payment providers
5. **📋 Admin Dynamic Plans** - Cho phép admin CRUD gói subscription động (nếu cần)
6. **📱 Instagram Support** - Nếu có yêu cầu business từ stakeholders

---

## 🎯 1. PROJECT OVERVIEW

### 📝 **Mô Tả Hệ Thống**

AISAM (AI-Powered Social Media Advertising Manager) là nền tảng SaaS cho phép:
- 🎨 Tạo nội dung quảng cáo bằng AI
- 📱 Quản lý brand, sản phẩm và nội dung
- 📅 Lập lịch và xuất bản bài viết
- 📊 Quản lý quảng cáo Facebook
- 💳 Quản lý subscription và thanh toán

**Kiến trúc:**
- Backend: .NET 8 + ASP.NET Core
- Frontend User: Next.js 15 + React 19
- Frontend Admin: Next.js 15 + React 19

---

### 🤖 **AI Capabilities (Hiện Tại)**

| Loại | Công Nghệ | Chức Năng |
|------|-----------|----------|
| **Text AI** | Google Gemini | Draft, chat, improve content |
| **Image AI** | Vertex AI Imagen | Sinh ảnh quảng cáo |
| **Context** | Brand/Product Data | Personaliza nội dung theo thương hiệu |

**Không thực hiện:** Tự huấn luyện mô hình AI - chỉ tích hợp API hiện có

---

### 📌 **Social & Ads (Hiện Tại)**

| Nền Tảng | Trạng Thái | API |
|----------|-----------|-----|
| **Facebook** | ✅ Hoàn chỉnh | Facebook Graph API + Marketing API |
| **Google** | 🔸 OAuth only | Google OAuth |
| **Instagram** | ⏳ Planned | - |
| **TikTok/Twitter** | ⏳ Planned | - |

---

### 💰 **Payment Gateway (Hiện Tại)**

| Gateway | Trạng Thái | Chức Năng |
|---------|-----------|----------|
| **PayOS** | ✅ Hoạt động | Checkout, webhook, subscription management |
| **Stripe** | ⏳ Optional Future | - |
| **VNPay** | ⏳ Optional Future | - |

---

## 📋 2. CURRENT IMPLEMENTED FEATURES

2. CURRENT IMPLEMENTED FEATURES
2.1. Account, Authentication, Profile and Subscription
Hệ thống đã có chức năng đăng ký, đăng nhập, Google login, refresh token, logout, logout all, session list, change password, forgot password, reset password, change password with token, verify email và resend verification email. Backend sử dụng JWT Bearer và refresh token để quản lý phiên đăng nhập. Frontend người dùng có các màn hình login, sign-up, forgot password, verify email, update password, overview account, security và profile.

Profile là ngữ cảnh vận hành quan trọng của hệ thống. Người dùng có thể làm việc với profile, subscription và dữ liệu liên quan đến brand/content/payment trong ngữ cảnh profile. Subscription hiện tại có các plan theo enum Free, Plus, Premium và PlusTrial. Người dùng có thể xem gói hiện tại, chọn gói, tạo PayOS checkout link, xác nhận thanh toán, xem lịch sử thanh toán, đổi gói hoặc hủy subscription.

2.2. Social Account and Facebook Integration
Hệ thống đã triển khai luồng kết nối social account thông qua OAuth URL và callback. Sau khi kết nối, backend lưu social account/token, cho phép lấy available targets, link/unlink targets, lấy linked targets, lấy accounts-with-targets và link Facebook ad account cho brand/social integration. FacebookProvider dùng Facebook Graph API với các quyền như pages_manage_posts, pages_read_engagement và pages_show_list.

Ở trạng thái hiện tại, Facebook là nền tảng được hỗ trợ rõ nhất cho publishing và ads workflow. GoogleProvider tồn tại trong hệ thống chủ yếu cho OAuth/login/provider integration. Instagram Business, TikTok và Twitter chỉ nên được xem là định hướng mở rộng hoặc future platform support nếu chưa có provider/flow hoàn chỉnh tương ứng.

2.3. Brand Kit Management
Hệ thống đã có CRUD brand, restore brand, list brand theo profile/team, assign brand to team và unassign brand from team. Brand model gồm name, description, logo_url, slogan, usp và target_audience. Brand được dùng làm context cho AI chat/generate content và liên kết với product, content, social integration, ad campaign và team brand.

Brand Kit là dữ liệu nền để AI sinh nội dung nhất quán với định vị thương hiệu. Các trường như slogan, USP và target audience được sử dụng để tăng chất lượng prompt. Tài liệu này giữ nguyên scope hiện tại: Brand Kit là bộ dữ liệu mô tả thương hiệu, không mô tả thêm các capability chưa có như brand guideline automation hoặc AI brand compliance scoring.

2.4. Product Management by Brand
Hệ thống đã có CRUD product, restore, list/filter theo brand/search/isDeleted, upload ảnh sản phẩm lên Supabase Storage và lưu images dạng JSON. Product gồm brand_id, name, description, price và images. Product được liên kết với Brand và Content, đồng thời được đưa vào prompt AI khi chat/generate content.

Luồng product hiện tại cho phép người dùng quản lý danh sách sản phẩm của từng brand, cập nhật mô tả, giá và hình ảnh. Các trường như USP riêng của product trong tài liệu gốc chưa thấy là trường model riêng trong source hiện tại; nếu cần, có thể xem là Planned Data Model Enhancement thay vì chức năng đã hoàn thành.

2.5. Content Library and Content Lifecycle
Hệ thống đã có create, update, delete, restore, clone, get detail và list content. Content có brand, product, ad type, title, text_content, image_url, video_url, style_description, context_description, representative_character, status, approvals, calendars, posts và ad creatives. AdType hiện tại gồm TextOnly, ImageText và VideoText.

Content là trung tâm của các luồng AI generation, approval, publishing, scheduling và ads creative. Source hiện tại hỗ trợ lưu VideoUrl và xử lý ad type VideoText ở mức dữ liệu/publishing, nhưng chưa có flow AI tự sinh video. Vì vậy, AI video generation chỉ được mô tả trong Future AI Enhancements.

2.6. AI Content Generation and AI Chat
Hệ thống đã có api/ai/generate-draft và api/ai/chat. Code sinh text bằng Gemini. Với ad type ImageText, code tạo visual prompt bằng Gemini rồi gọi Vertex AI Imagen để sinh ảnh, sau đó upload ảnh vào Supabase Storage và trả GeneratedImageUrl. AI chat có thể sử dụng brand/product context để tạo nội dung phù hợp hơn với thương hiệu và sản phẩm.

Hệ thống đã có api/ai/improve/{contentId}, api/ai/approve/{aiGenerationId}, api/ai/generations/{contentId} và conversation APIs. Người dùng có thể cải thiện nội dung, duyệt generation, lấy lịch sử generation theo content và xem lịch sử hội thoại AI.

2.7. Approval and Team Permission
Hệ thống đã có team CRUD, user teams, member list, permissions, assign brand to team, team stats và kiểm tra quyền theo team. Team member có role, permissions và is_active. Approval module có pending, create, get, update, approve, reject, list by content, list by approver, pending count, delete và restore.

Luồng duyệt nội dung hiện tại hỗ trợ gửi content vào approval, xem danh sách pending, approve hoặc reject và tạo notification liên quan. Source có permission/team support, nhưng tài liệu không khẳng định tuyệt đối rằng mỗi team chỉ có duy nhất một Leader nếu code chưa enforce rõ ở model/service. Yêu cầu “single Leader per team” từ tài liệu gốc được chuyển thành Planned Governance Rule nếu nhóm muốn siết chặt bằng database constraint và service validation.

2.8. Publishing and Scheduled Posts
Hệ thống đã có publish ngay qua api/content/{contentId}/publish/{integrationId}. Backend kiểm tra content/integration rồi gọi provider để publish lên Facebook và lưu Post record. Với đặt lịch, hệ thống có api/content-calendar/schedule/{contentId}, schedule-recurring, update/delete schedule, upcoming và lịch theo team.

Backend có ScheduledPostingService và ScheduledPostingBackgroundService để xử lý các lịch đến hạn. Luồng hiện tại phù hợp với yêu cầu cốt lõi về scheduled posts, nhưng nền tảng publish thực tế vẫn tập trung vào Facebook.

2.9. Facebook Ads Management
Hệ thống đã có ad campaign, ad set, ad creative và ad. Code tạo campaign/ad set/ad/creative qua Facebook Marketing API, lấy preview, cập nhật status, xóa, pull reports/insights. Creative có thể tạo từ content hoặc từ Facebook post sẵn có.

Ads workflow hiện tại bao gồm tạo campaign theo brand/profile/ad account, tạo ad set với ngân sách/lịch/targeting, tạo ad creative từ content hoặc Facebook post, sau đó tạo ad trong ad set. Backend lưu các Facebook ID tương ứng để quản lý lifecycle và tương tác tiếp với Facebook Marketing API.

2.10. Dashboard, Reports and Analytics
Hệ thống đã có api/dashboard/stats, trang dashboard, reports, posts, campaign reports và logic kéo insight từ Facebook cho ads. Frontend có components analytics/reports, biểu đồ và một số UI/hook liên quan đến xuất báo cáo. Ở trạng thái hiện tại, analytics tập trung vào hiển thị dữ liệu vận hành và chỉ số cơ bản.

Các tính năng như AI phân tích cảm xúc, trend prediction, AI strategy recommendation và AI realtime performance optimization chưa thấy backend service/controller tương ứng, do đó được phân loại là Future AI Enhancements hoặc Optional Enterprise Features.

2.11. Notification and Conversation Management
Hệ thống đã có notification list, detail, mark read, mark all read và unread count. Conversation module có list, detail, delete và liên kết chat messages với AI generation/content. Các module này hỗ trợ trải nghiệm làm việc theo luồng và lưu lại lịch sử tương tác của người dùng với AI.

2.12. Storage Management
Source dùng SupabaseStorageService cho upload, download, list, delete, signed-url và public-url. Service có validate loại file ảnh/video và giới hạn kích thước file. Supabase Storage hiện là storage layer cho ảnh sản phẩm, ảnh AI generated và các media liên quan.

3. CURRENT SYSTEM FLOWS
3.1. Authentication and Account Flow
Người dùng truy cập sign-up hoặc login. Backend xử lý register/login/google login, tạo access token và refresh token, đồng thời hỗ trợ verify email. Sau khi đăng nhập, người dùng có thể quản lý session, đổi mật khẩu, logout hoặc logout all. Nếu quên mật khẩu, người dùng thực hiện forgot password và reset password qua token.

3.2. Profile, Subscription and PayOS Payment Flow
Người dùng tạo hoặc chọn profile để làm việc. Khi chọn gói subscription, frontend gọi backend để tạo PayOS checkout link. Sau khi thanh toán, hệ thống confirm payment hoặc nhận webhook từ PayOS để kích hoạt subscription cho profile. Người dùng có thể xem active subscription, danh sách subscriptions, lịch sử thanh toán, đổi plan hoặc hủy subscription.

3.3. Facebook Social Connection Flow
Người dùng yêu cầu OAuth URL theo provider. Hệ thống chuyển người dùng sang OAuth provider và nhận callback. Backend lưu social account/token, sau đó người dùng lấy danh sách page/target khả dụng, link target vào brand/social integration và có thể link Facebook ad account. Nếu token hết hạn hoặc thiếu quyền, backend trả lỗi để người dùng reconnect hoặc cấp lại quyền.

3.4. Brand, Product and Content Management Flow
Người dùng tạo brand, cập nhật thông tin brand, thêm product vào brand và upload ảnh sản phẩm. Khi tạo content, người dùng chọn brand/product/ad type, nhập title/text/context/style và lưu content. Content có thể được chỉnh sửa, clone, delete/restore, gửi duyệt, đăng ngay, đặt lịch hoặc dùng để tạo ad creative.

3.5. AI Generation Flow
Người dùng gửi request generate draft hoặc chat với AI kèm brandId/productId/adType nếu có. Backend kiểm tra quyền truy cập brand, dựng prompt từ brand/product/user message, gọi Gemini để sinh text. Nếu adType là ImageText, backend tạo prompt hình ảnh bằng Gemini, gọi Vertex AI Imagen để sinh ảnh, upload ảnh vào Supabase và trả URL. Người dùng có thể improve content, xem generations và approve AI generation để cập nhật content.

3.6. Approval Flow
Người dùng submit content vào approval. Người duyệt mở pending queue hoặc xem approval theo content/approver, đọc chi tiết, approve hoặc reject. Khi approval thay đổi, service cập nhật trạng thái liên quan và tạo notification cho người dùng. Quy tắc phân quyền dựa trên team role/permissions hiện có trong source.

3.7. Publishing and Scheduling Flow
Với đăng ngay, backend nhận contentId và integrationId, kiểm tra content/integration rồi gọi Facebook provider để publish nội dung và lưu Post record. Với đặt lịch, backend tạo ContentCalendar cho content, cho phép đặt lịch một lần hoặc lặp lại, cập nhật/xóa lịch, xem upcoming schedules hoặc lịch theo team. Background service xử lý lịch đến hạn.

3.8. Ads Campaign Flow
Người dùng tạo ad campaign theo brand/profile/ad account, sau đó tạo ad set với ngân sách, lịch chạy và targeting. Người dùng tạo ad creative từ content hoặc Facebook post, rồi tạo ad trong ad set. Backend gọi Facebook Marketing API để tạo campaign/ad set/creative/ad, lưu Facebook ID tương ứng, hỗ trợ preview, update status, delete và pull reports/insights.

3.9. Reporting Flow
Người dùng vào dashboard, reports, posts hoặc campaign/ad detail để xem dữ liệu. Backend cung cấp dashboard stats và ad insights/pull reports. Frontend hiển thị dữ liệu ở dạng bảng, biểu đồ và các thành phần báo cáo cơ bản. Những tính năng dự đoán hoặc tối ưu tự động bằng AI chưa thuộc current flow.

4. ADMIN FEATURES
4.1. Current Admin User and Profile Management
Admin frontend có dashboard danh sách user, trang chi tiết user, danh sách profile theo user và trang chi tiết profile. Backend có api/users, api/users/{id}, api/users/profile/me và profile controller. Admin có thể xem dữ liệu người dùng, profile, subscription liên quan và trạng thái vận hành cơ bản.

4.2. Current Admin Payment and Subscription Management
Admin frontend có trang payments, subscriptions và subscriptions theo user. Backend có các endpoint admin payment như /payment/admin/all, /payment/admin/subscriptions, /payment/admin/user/{userId}/payments và /payment/admin/user/{userId}/subscriptions. Admin tools có update-payment-method, update-profile-status và update-subscription-plan.

4.3. Current Admin Tools
Admin tools hỗ trợ seed demo user, seed batch users, sửa phương thức thanh toán, sửa trạng thái profile và sửa plan subscription. Đây là các công cụ vận hành và hỗ trợ dữ liệu demo. Source hiện chưa có màn hình quản trị tạo, xóa, cấu hình plan động như một entity plan riêng; dynamic subscription plans được đưa vào Planned Features.

5. AI CAPABILITIES
5.1. Current AI Capabilities
AI hiện tại sử dụng Gemini cho text generation, prompt generation, AI chat và improve content. Với ImageText, hệ thống dùng Gemini để tạo visual prompt rồi gọi Google Vertex AI Imagen để tạo ảnh. Kết quả ảnh được upload lên Supabase Storage. Hệ thống lưu AiGeneration, GeneratedText, GeneratedImageUrl, status và liên kết với Content/Conversation.

5.2. Current Prompt Context
Prompt hiện tại có thể sử dụng brand name, description, slogan, USP, target audience, product name, product description và product price. Cách tiếp cận này phù hợp với hướng AI Integration & Prompt Engineering của đồ án, tập trung vào khai thác dịch vụ AI có sẵn thay vì tự phát triển mô hình AI.

5.3. Planned Advanced AI Features
Các capability như GPT-4o integration, DALL-E image generation, AI sentiment analysis, trend prediction, strategy recommendation, budget recommendation, best-time-to-post recommendation, realtime campaign optimization và AI video generation là future enhancement hoặc optional enterprise features. Các capability này chưa được xem là đã triển khai trong source hiện tại và cần thiết kế thêm API, data model, quota policy, monitoring và fallback strategy nếu phát triển.

6. FUTURE ENHANCEMENTS
6.1. Planned Multi-model AI Integration
Hệ thống có thể mở rộng kiến trúc AI provider để hỗ trợ thêm GPT-4o cho text reasoning/content strategy và DALL-E cho image generation. Đây là planned/future scope, không thay thế Gemini + Vertex AI Imagen hiện tại. Proposed architecture nên dùng abstraction layer dạng IAIProvider hoặc strategy pattern để lựa chọn provider theo plan, quota, chi phí và use case.

6.2. Proposed AI Video Generation
AI video generation từ tài liệu gốc được phân loại là Proposed Advanced AI Feature. Source hiện có VideoUrl và AdType VideoText để lưu/publish media video, nhưng chưa có pipeline sinh video AI. Để triển khai, hệ thống cần thêm video generation provider, job queue/background processing, storage policy, progress tracking, cost quota và moderation flow.

6.3. Future Sentiment Analysis and Trend Prediction
Sentiment analysis và trend prediction là Future AI Enhancements. Chức năng này có thể phân tích comment, engagement, hashtag, campaign result hoặc dữ liệu social công khai để đề xuất insight. Hiện tại source chưa có service/controller chuyên trách cho phân tích cảm xúc hoặc dự đoán xu hướng, nên tài liệu chỉ ghi nhận như roadmap feature.

6.4. Future AI Strategy Recommendation and Optimization
AI strategy recommendation có thể đề xuất mục tiêu chiến dịch, audience, ngân sách, thời gian đăng bài và nội dung phù hợp dựa trên brand/product/campaign history. Realtime optimization có thể điều chỉnh ngân sách, targeting hoặc creative dựa trên performance. Đây là Optional Enterprise Feature và chưa được triển khai trong source hiện tại. Khi phát triển cần đảm bảo audit trail, manual approval và giới hạn quyền để tránh tự động thay đổi campaign ngoài kiểm soát.

6.5. Planned Instagram Expansion
Instagram expansion là Planned Platform Feature. Tài liệu gốc có nhắc Facebook và Instagram Business, nhưng source hiện tại triển khai rõ nhất Facebook provider. Để hỗ trợ Instagram hoàn chỉnh cần thêm provider flow, permission mapping, business account discovery, publishing endpoint, media validation và report mapping theo Instagram Graph API.

6.6. Planned Dynamic Subscription Plans
Dynamic subscription plans là Planned Admin Feature. Source hiện tại quản lý plan theo enum và admin tools cập nhật subscription plan. Nếu muốn quản trị plan động, hệ thống cần thêm entity SubscriptionPlan, admin CRUD, pricing/quota configuration, versioning, migration strategy và backward compatibility với subscription hiện có.

6.7. Optional Multi-payment Gateway Integration
Stripe/VNPay từ tài liệu gốc không phải payment gateway hiện tại. Hệ thống đang dùng PayOS. Trong tương lai có thể bổ sung Stripe/VNPay như optional payment providers thông qua payment provider abstraction, nhưng không nên thay đổi current PayOS flow nếu không có yêu cầu triển khai.

6.8. Future Enterprise Governance Rules
Quy tắc mỗi team chỉ có một Leader và chỉ Leader có quyền duyệt/đăng/quản lý thành viên là yêu cầu nghiệp vụ hợp lý từ tài liệu gốc, nhưng cần kiểm tra và enforce rõ trong source bằng service validation, database constraint hoặc permission policy. Trong tài liệu này, nội dung đó được đặt là Planned Governance Rule nếu chưa được đảm bảo đầy đủ trong code hiện tại.

7. NON-FUNCTIONAL REQUIREMENTS
7.1. Security
Hệ thống cần duy trì JWT Bearer authentication, refresh token, email verification, password reset token, authorization theo endpoint và context profile/team. OAuth/social token và payment data cần được bảo vệ theo nguyên tắc least privilege, secret management và không log sensitive data. Các future provider như GPT-4o, DALL-E, Stripe/VNPay hoặc Instagram phải tuân thủ cùng chính sách bảo mật.

7.2. Performance and Background Processing
Các request thông thường cần phản hồi nhanh và ổn định. Các tác vụ AI generation, image generation, scheduled posting, report pulling và future video generation có thể xử lý bất đồng bộ hoặc qua background service/job queue. Hệ thống cần tránh block request dài đối với các tác vụ tốn thời gian hoặc có chi phí API cao.

7.3. Scalability
Kiến trúc hiện tại theo Controller-Service-Repository và tách frontend user/admin cho phép mở rộng module. Các future enhancements nên được thêm thông qua abstraction layer và feature flags để không phá vỡ current flows. Database schema cần hỗ trợ migration có kiểm soát.

7.4. Reliability and Error Handling
Hệ thống cần xử lý lỗi rõ ràng khi token hết hạn, thiếu quyền social, payment callback thất bại, AI provider timeout, storage upload lỗi hoặc Facebook Marketing API trả lỗi. Các tác vụ background cần có retry policy, logging và trạng thái để người dùng/admin theo dõi.

7.5. Usability
Frontend cần đảm bảo người dùng dễ thao tác với brand, product, content, AI generation, scheduling và ads workflow. Admin UI cần tập trung vào vận hành, tìm kiếm, kiểm tra user/profile/payment/subscription và xử lý dữ liệu hỗ trợ. Các tính năng future cần được phân quyền và giải thích rõ để tránh người dùng hiểu nhầm là hệ thống tự động thay đổi campaign ngoài ý muốn.

7.6. Compatibility
Ứng dụng web cần tương thích với các trình duyệt phổ biến như Chrome, Edge, Firefox và Safari. Các media upload cần tuân thủ validation hiện có của SupabaseStorageService và các giới hạn của provider social/ads tương ứng.

8. TECHNOLOGY STACK
Backend hiện tại sử dụng .NET 8, ASP.NET Core Web API, Entity Framework Core, Npgsql/PostgreSQL, FluentValidation, JWT Bearer, Swagger/OpenAPI và kiến trúc Controller-Service-Repository.

Frontend User hiện tại sử dụng Next.js 15, React 19, TypeScript, Tailwind CSS, Radix UI/shadcn-style components, TanStack Query, Recharts và lucide-react.

Frontend Admin hiện tại sử dụng Next.js 15, React 19, TypeScript, Tailwind CSS, Radix UI, TanStack Query/Table, Recharts và lucide-react.

AI Service hiện tại sử dụng Google Gemini cho text/prompt/chat và Google Vertex AI Imagen cho image generation. GPT-4o và DALL-E chỉ là future/planned AI providers nếu hệ thống mở rộng multi-model AI.

Social/Ads hiện tại sử dụng Facebook Graph API, Facebook OAuth và Facebook Marketing API. Google OAuth/provider có trong hệ thống. Instagram expansion là planned feature, không phải current completed capability.

Payment Gateway hiện tại là PayOS. Stripe/VNPay chỉ là optional future payment gateways nếu có yêu cầu tích hợp thêm.

Storage hiện tại là Supabase Storage. Database hiện tại là PostgreSQL.

9. LIMITATIONS & FUTURE ROADMAP
9.1. Current Limitations
Hệ thống hiện tại tập trung vào Facebook cho publishing và ads; Instagram Business chưa nên được mô tả là hoàn thiện. AI hiện tại tập trung vào Gemini text/chat và Vertex Imagen image generation; GPT-4o, DALL-E và AI video generation chưa được triển khai. Analytics hiện tại là dashboard/report cơ bản và Facebook insights; sentiment analysis, trend prediction và realtime optimization chưa có backend service chuyên trách.

Admin hiện tại có xem user/payment/subscription và admin tools để cập nhật dữ liệu quan trọng, nhưng chưa có dynamic subscription plan management dạng CRUD plan đầy đủ. Team Leader single-owner governance là yêu cầu nghiệp vụ hợp lý nhưng cần enforce rõ bằng code nếu muốn đưa thành rule bắt buộc.

9.2. Short-term Roadmap
Trong ngắn hạn, hệ thống nên hoàn thiện tài liệu API flow, kiểm thử các luồng PayOS, Facebook OAuth, scheduled posting và Facebook Marketing API. Nên bổ sung test coverage cho service quan trọng như payment, content publishing, approval, AI generation và ad creation. Nên chuẩn hóa permission policy cho team/approval để tránh mâu thuẫn giữa UI và backend.

9.3. Mid-term Roadmap
Trong trung hạn, hệ thống có thể mở rộng dynamic subscription plan management, cải thiện analytics, bổ sung export report hoàn chỉnh, tăng monitoring cho AI/payment/social provider và chuẩn hóa provider abstraction cho social/AI/payment. Instagram expansion có thể được triển khai theo từng bước sau khi hoàn tất provider, permission và media validation.

9.4. Long-term Roadmap
Trong dài hạn, AISAM có thể phát triển thành enterprise marketing automation platform với multi-model AI, AI strategy recommendation, AI video generation, sentiment analysis, trend prediction, realtime campaign optimization và multi-payment gateway. Tất cả các capability này cần được triển khai có kiểm soát, có audit trail, quota management, cost monitoring và user approval workflow để phù hợp với môi trường enterprise.
