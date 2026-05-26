# 📱 AISAM - AI-Powered Social Media Advertising Manager

**SOFTWARE REQUIREMENT SPECIFICATION (SRS)**

---

## 📋 Document Control

Tài liệu này được xây dựng theo nguyên tắc **source code hiện tại là baseline triển khai**.
- ✅ **Implemented Features**: Mô tả các chức năng đã có trong source
- 🔄 **Planned/Future**: Các chức năng sắp phát triển hoặc định hướng tương lai
- ⏳ **Optional**: Các tính năng không ưu tiên hoặc enterprise features

---

## 📊 QUICK OVERVIEW - PHÂN TÍCH HỆ THỐNG

### ✅ **NHÓM 1: CHỨC NĂNG ĐÃ TRIỂN KHAI (14 modules)**

| # | Chức Năng | Chi Tiết |
|---|-----------|---------|
| 1 | Xác thực & Tài khoản | Đăng ký, đăng nhập, Google OAuth, JWT Bearer, refresh token |
| 2 | Quản lý Profile & Gói | Profile, subscription (Free/Plus/Premium/PlusTrial), PayOS |
| 3 | Kết nối Facebook | OAuth, liên kết page, ad accounts, targets |
| 4 | Quản lý Brand | CRUD brand, assign/unassign to team, brand context |
| 5 | Quản lý Sản phẩm | CRUD product, upload ảnh Supabase, liên kết brand |
| 6 | Thư viện Nội dung | CRUD content, 3 loại (Text/Image/Video), clone, restore |
| 7 | AI sinh Text | Google Gemini - draft, chat, improve |
| 8 | AI sinh Ảnh | Vertex AI Imagen - quảng cáo ảnh |
| 9 | AI Chat & Improve | Chat context brand/product |
| 10 | Duyệt Nội dung | Workflow approval, team permissions |
| 11 | Đăng bài & Lịch | Publish now hoặc schedule, background service |
| 12 | Quảng cáo Facebook | Campaign, ad set, creative, insights |
| 13 | Dashboard & Reports | Stats, analytics, Facebook insights |
| 14 | Admin Tools | Quản lý user, payment, subscription |

---

### 🔄 **NHÓM 2: CHỨC NĂNG ĐANG/SẮP PHÁT TRIỂN (8 features)**

| # | Chức Năng | Trạng Thái | Timeline |
|---|-----------|-----------|----------|
| 1 | Mở rộng AI (GPT-4o, DALL-E) | Planned | Mid-term |
| 2 | Sentiment & Trend Prediction | Future | Long-term |
| 3 | AI Video Generation | Proposed | Long-term |
| 4 | AI Strategy & Optimization | Enterprise | Long-term |
| 5 | Instagram/TikTok/Twitter | Planned | Mid-term |
| 6 | Dynamic Subscription Plans | Admin Feature | Mid-term |
| 7 | Stripe/VNPay Gateway | Optional | Long-term |
| 8 | Team Governance Rules | Planned | Short-term |

---

### ⚠️ **NHÓM 3: ĐIỀU CẦN LÀM RÕ - ACTION ITEMS**

| Vấn Đề | Tác Động | Cần Làm |
|--------|---------|--------|
| **Team Permission** | Ai được quyền duyệt/đăng? | Enforce rõ 1 Leader per team |
| **Subscription Plans** | Plans dùng enum hay CRUD? | Xác định nhu cầu business |
| **Instagram Ready?** | Enum có nhưng provider chưa? | Quyết định roadmap |
| **Background Job** | Retry policy? Monitoring? | Kiểm tra ScheduledPosting Service |
| **AI Video** | VideoUrl có nhưng chưa sinh? | Quyết định timeline phát triển |
| **Budget Optimization** | Tự động điều chỉnh? | Thêm vào roadmap |
| **Provider Architecture** | Có abstraction layer? | Refactor nếu cần |
| **Test Coverage** | Luồng chính có test? | Bổ sung unit/integration tests |

---

## 🎯 1. PROJECT OVERVIEW

### 📝 **Mô Tả Hệ Thống**

AISAM (AI-Powered Social Media Advertising Manager) là nền tảng **SaaS** cho phép:

✨ **Tính năng chính:**
- 🎨 Tạo nội dung quảng cáo bằng AI
- 📱 Quản lý brand, sản phẩm, nội dung
- 📅 Lập lịch & xuất bản bài viết
- 📊 Quản lý quảng cáo Facebook
- 💳 Quản lý subscription & thanh toán

**Kiến trúc:**
```
Backend:        .NET 8 + ASP.NET Core + PostgreSQL
Frontend User:  Next.js 15 + React 19 + Tailwind CSS
Frontend Admin: Next.js 15 + React 19 + Tailwind CSS
```

---

### 🤖 **AI Capabilities (Hiện Tại)**

| Loại | Công Nghệ | Chức Năng |
|------|-----------|----------|
| **Text AI** | Google Gemini | Draft, chat, improve content, prompt gen |
| **Image AI** | Vertex AI Imagen | Sinh ảnh quảng cáo từ visual prompt |
| **Context** | Brand/Product Data | Personalize nội dung theo thương hiệu |

**Nguyên tắc:** Tích hợp API hiện có - KHÔNG tự huấn luyện mô hình AI

---

### 📌 **Social & Ads Support (Hiện Tại)**

| Nền Tảng | Trạng Thái | Tính Năng |
|----------|-----------|---------|
| **Facebook** | ✅ Hoàn chỉnh | Graph API, Marketing API, OAuth |
| **Google** | 🔸 OAuth only | Login, provider integration |
| **Instagram** | ⏳ Planned | Enum có, provider chưa ready |
| **TikTok/Twitter** | ⏳ Planned | UI enum, no implementation |

---

### 💰 **Payment Gateway**

| Gateway | Trạng Thái | Chức Năng |
|---------|-----------|----------|
| **PayOS** | ✅ Active | Checkout, webhook, subscription |
| **Stripe** | ⏳ Optional Future | - |
| **VNPay** | ⏳ Optional Future | - |

---

## 📋 2. CURRENT IMPLEMENTED FEATURES (16 Modules)

### 2.1 🔐 **Xác Thực & Tài Khoản**

**Endpoints & Features:**
```
POST   /auth/register              - Sign up
POST   /auth/login                 - Sign in
POST   /auth/google-login          - OAuth
POST   /auth/refresh-token         - Refresh
POST   /auth/logout                - Logout
POST   /auth/logout-all            - Logout all sessions
POST   /auth/verify-email          - Verify email
POST   /auth/forgot-password       - Reset request
POST   /auth/reset-password        - Reset with token
GET    /auth/sessions              - Session list
```

**Security:**
- JWT Bearer token + Refresh token
- Email verification
- Password reset token

**Frontend UI:**
- Login, Sign-up screens
- Forgot password
- Email verification
- Password update
- Security settings
- Account overview

---

### 2.2 👤 **Quản Lý Profile & Subscription**

**Subscription Plans:**
- `Free` - Free plan
- `Plus` - Pro plan
- `Premium` - Enterprise plan
- `PlusTrial` - Trial plan

**Features:**
```
GET    /subscription/active              - Current plan
POST   /subscription/checkout            - Create PayOS link
POST   /subscription/confirm             - Confirm payment
GET    /subscription/history             - Payment history
POST   /subscription/change-plan         - Upgrade/downgrade
POST   /subscription/cancel              - Cancel subscription
```

**Payment Integration:**
- PayOS checkout link creation
- Webhook for payment confirmation
- Plan activation
- Cancellation logic

---

### 2.3 📱 **Kết Nối Facebook**

**OAuth Flow:**
```
1. GET /social/oauth-url/{provider}     → Redirect to OAuth
2. Callback                               → Save token & account
3. GET /social/accounts                   → List connected accounts
4. GET /social/available-targets          → List pages/accounts
5. POST /social/link-target               → Link to brand
6. POST /social/link-ad-account           → Link ad account
```

**Facebook Permissions:**
- `pages_manage_posts` - Post management
- `pages_read_engagement` - Read insights
- `pages_show_list` - List pages

**Data Stored:**
- Social account info
- Access token (encrypted)
- Targets/pages linked
- Ad account mapping

---

### 2.4 🏢 **Quản Lý Brand**

**CRUD Operations:**
```
POST   /brand                    - Create brand
GET    /brand/{brandId}          - Get detail
PUT    /brand/{brandId}          - Update
DELETE /brand/{brandId}          - Delete (soft)
POST   /brand/{brandId}/restore  - Restore
GET    /brand                    - List by profile/team
POST   /team/{teamId}/brand      - Assign to team
DELETE /team/{teamId}/brand      - Unassign
```

**Brand Model:**
```
{
  id, profileId, teamId,
  name, description,
  logo_url, slogan, usp,
  target_audience,
  created, updated, deleted
}
```

**Usage:**
- AI prompt context
- Content generation
- Ad campaign association
- Team brand assignment

---

### 2.5 📦 **Quản Lý Sản Phẩm**

**CRUD Operations:**
```
POST   /product                  - Create
GET    /product/{productId}      - Get detail
PUT    /product/{productId}      - Update
DELETE /product/{productId}      - Delete (soft)
POST   /product/{id}/restore     - Restore
GET    /product?brand={brandId}  - List by brand
GET    /product?search=...       - Search
POST   /product/{id}/images      - Upload images
```

**Product Model:**
```
{
  id, brandId,
  name, description, price,
  images: [{ url, alt }],
  created, updated, deleted
}
```

**Image Upload:**
- Supabase Storage
- Validate: image/video types
- Size limits enforcement
- JSON array format

---

### 2.6 📄 **Thư Viện Nội Dung**

**Content Types:**
1. **TextOnly** - Text chỉ
2. **ImageText** - Text + Image
3. **VideoText** - Text + Video

**CRUD Operations:**
```
POST   /content                           - Create
GET    /content/{contentId}               - Get detail
PUT    /content/{contentId}               - Update
DELETE /content/{contentId}               - Delete (soft)
POST   /content/{contentId}/restore       - Restore
POST   /content/{contentId}/clone         - Clone
GET    /content?brand={}&filter=...       - List & filter
```

**Content Model:**
```
{
  id, brandId, productId, profileId,
  type: TextOnly|ImageText|VideoText,
  title, text_content,
  image_url, video_url,
  style_description,
  context_description,
  representative_character,
  status, approvals[], calendars[],
  posts[], creatives[]
}
```

---

### 2.7 🤖 **AI Sinh Text (Gemini)**

**Endpoints:**
```
POST   /ai/generate-draft         - Generate content draft
POST   /ai/chat                   - Chat with AI
POST   /ai/improve/{contentId}    - Improve content
POST   /ai/approve/{generationId} - Approve generation
GET    /ai/generations/{contentId} - Get generation history
```

**Prompt Context:**
```
Brand:   name, description, slogan, usp, target_audience
Product: name, description, price
User:    message, adType
```

**Engine:** Google Gemini

---

### 2.8 🖼️ **AI Sinh Ảnh (Vertex AI Imagen)**

**Flow:**
```
1. User: generate-draft (ImageText type)
2. Backend: create visual prompt (Gemini)
3. Call: Vertex AI Imagen API
4. Result: Generate image
5. Upload: Supabase Storage
6. Return: Image URL
```

**Supported:**
- ImageText content type
- Ad creative generation
- Social post images

**Not Yet:**
- Video generation (VideoText)

---

### 2.9 💬 **AI Chat & Conversation**

**Endpoints:**
```
POST   /ai/chat                      - Send message
GET    /ai/conversations             - List chats
GET    /ai/conversations/{chatId}    - Get chat detail
DELETE /ai/conversations/{chatId}    - Delete chat
GET    /ai/conversation-messages     - Get messages in chat
```

**Features:**
- Conversation history
- Brand context awareness
- Product context in prompts
- Link to AI generation
- Audit trail

---

### 2.10 ✔️ **Duyệt Nội Dung (Approval Workflow)**

**Entities:**
- Teams (CRUD)
- Team members (role, permissions)
- Approvals (pending, approve, reject)
- Notifications

**Approval Workflow:**
```
POST   /approval/submit                    - Submit for approval
GET    /approval/pending                   - List pending
POST   /approval/{approvalId}/approve      - Approve
POST   /approval/{approvalId}/reject       - Reject
GET    /approval/by-content/{contentId}    - By content
GET    /approval/by-approver/{userId}      - By approver
GET    /approval/pending-count             - Count pending
```

**Permissions:**
- Enforce by team role
- Check on submit/approve
- Auto-notify on status change

---

### 2.11 📢 **Đăng Bài & Lập Lịch**

**Publish Now:**
```
POST /content/{contentId}/publish/{integrationId}
{
  content validated ✓
  integration verified ✓
  → call FacebookProvider
  → create Post record
}
```

**Scheduled Posts:**
```
POST   /content-calendar/schedule/{contentId}
POST   /content-calendar/schedule-recurring
PUT    /content-calendar/{scheduleId}
DELETE /content-calendar/{scheduleId}
GET    /content-calendar/upcoming
GET    /content-calendar/by-team/{teamId}
```

**Background Service:**
- `ScheduledPostingBackgroundService`
- Process due schedules
- Retry on failure
- Logging & monitoring

---

### 2.12 📊 **Quảng Cáo Facebook (Ads)**

**Entities:**
- Ad Campaign
- Ad Set (budget, schedule, targeting)
- Ad Creative (from content or post)
- Ad

**Workflow:**
```
1. Create Campaign (brand/profile/ad-account)
2. Create Ad Set (budget, schedule, targeting)
3. Create Ad Creative (from content or post)
4. Create Ad (in ad-set)
5. Track insights via Facebook API
```

**Features:**
```
POST   /ad-campaign                    - Create campaign
POST   /ad-set/{campaignId}            - Create ad set
POST   /ad-creative                    - Create creative
POST   /ad/{creativesId}               - Create ad
GET    /ad/{adId}/preview              - Preview
PUT    /ad/{adId}/status               - Update status
DELETE /ad/{adId}                      - Delete
GET    /ad/{adId}/insights             - Get insights
GET    /ad-campaign/{id}/reports       - Get reports
```

**Storage:**
- Save Facebook IDs locally
- Map to content/post
- Track lifecycle

---

### 2.13 📈 **Dashboard & Reports**

**Endpoints:**
```
GET    /dashboard/stats           - Dashboard stats
GET    /dashboard/posts           - Posts data
GET    /dashboard/campaigns       - Campaign data
GET    /dashboard/reports         - Generate reports
```

**Data Source:**
- Facebook Insights API
- System analytics
- User activity logs

**UI Components:**
- Analytics dashboard
- Charts (Recharts)
- Data tables
- Export functionality

**Current Scope:**
- Basic operational data
- Facebook metrics
- No AI-powered predictions (future)

---

### 2.14 🔔 **Notification & Conversation**

**Notification Endpoints:**
```
GET    /notification              - List all
GET    /notification/{id}         - Get detail
POST   /notification/{id}/mark    - Mark read
POST   /notification/mark-all     - Mark all read
GET    /notification/unread-count - Unread count
```

**Triggers:**
- Approval status changes
- Content publishing
- Team assignments
- Payment confirmations

**Conversation Module:**
```
GET    /conversation              - List
GET    /conversation/{id}         - Get detail
DELETE /conversation/{id}         - Delete
```

---

### 2.15 💾 **Storage Management (Supabase)**

**Service:** `SupabaseStorageService`

**Operations:**
```
- Upload (image/video validation)
- Download
- List files
- Delete
- Generate signed URLs
- Public URLs
```

**File Validation:**
- Image types: jpg, png, gif, webp
- Video types: mp4, mov, avi
- Size limits enforced

**Usage:**
- Product images
- AI generated images
- User-uploaded media

---

### 2.16 🛠️ **Admin Tools**

**Admin Frontend:**
- User dashboard
- User details
- Profile list by user
- Payments list
- Subscriptions list

**Admin Endpoints:**
```
GET    /admin/users                              - List users
GET    /admin/users/{userId}                     - User detail
GET    /admin/payment/all                        - All payments
GET    /admin/payment/user/{userId}              - User payments
GET    /admin/subscription/all                   - All subscriptions
GET    /admin/subscription/user/{userId}         - User subs
```

**Admin Actions:**
```
POST   /admin/tools/seed-demo-user               - Create demo user
POST   /admin/tools/seed-batch-users             - Batch seed
POST   /admin/tools/update-payment-method        - Update payment
POST   /admin/tools/update-profile-status        - Update status
POST   /admin/tools/update-subscription-plan     - Change plan
```

---

## 🔄 3. CURRENT SYSTEM FLOWS (9 Main Flows)

### 3.1 🔐 **Authentication & Account Flow**

```
User Input (Sign-up/Login)
    ↓
Backend Process:
  - Register: Validate email → Create user → Send verification
  - Login: Validate credentials → Generate tokens
  - Google OAuth: Redirect → Callback → Create/Link account
    ↓
Token Management:
  - Access token (short-lived)
  - Refresh token (long-lived)
    ↓
User Actions:
  - Manage sessions
  - Change password
  - Reset password (forgot)
  - Logout / Logout all
```

---

### 3.2 💳 **Subscription & Payment Flow**

```
User Select Plan
    ↓
Frontend → Backend:
  POST /subscription/checkout
    ↓
Backend:
  - Validate plan
  - Create PayOS checkout link
  - Return URL
    ↓
User Payment:
  - Redirect to PayOS
  - Complete payment
    ↓
Webhook/Confirmation:
  - PayOS callback → Backend
  - Activate subscription
  - Send confirmation email
    ↓
User Can:
  - View active plan
  - Change plan (upgrade/downgrade)
  - Cancel subscription
  - View payment history
```

---

### 3.3 📱 **Facebook Connection Flow**

```
User Request Connection
    ↓
System:
  1. Generate OAuth URL
  2. Redirect to Facebook OAuth
  3. User approves permissions
    ↓
Callback:
  - Save social account
  - Store access token (encrypted)
    ↓
User Actions:
  - Get available pages/targets
  - Link targets to brand
  - Link Facebook ad account
    ↓
Ongoing:
  - Check token validity
  - Refresh if needed
  - Handle permission errors
```

---

### 3.4 🏢 **Brand, Product & Content Flow**

```
Create/Update Brand
  ↓ Brand assigned to team/profile
    ↓
Add Products to Brand
  ↓ Upload product images
    ↓
Create Content
  - Select brand/product
  - Choose type (Text/Image/Video)
  - Input title, content, context
  - Save with status
    ↓
Content Actions:
  - Edit/Clone
  - Delete/Restore
  - Submit for approval
  - Publish or schedule
  - Use for ad creative
```

---

### 3.5 🤖 **AI Generation Flow**

```
User Request:
  - Generate draft OR Chat
  - Provide: brand, product, type, message
    ↓
Backend Validation:
  - Check user permissions
  - Verify brand/product access
    ↓
Prompt Building:
  - Brand context (name, slogan, USP, target)
  - Product context (if provided)
  - User message
    ↓
Text Generation (Gemini):
  - Call API
  - Store result in AiGeneration
  - Link to content/conversation
    ↓
If ImageText:
  - Generate visual prompt (Gemini)
  - Call Vertex AI Imagen
  - Upload to Supabase
  - Return image URL
    ↓
User Can:
  - Improve draft
  - Approve generation
  - Chat iteratively
```

---

### 3.6 ✔️ **Approval Flow**

```
Content Submit for Approval
    ↓
Create Approval Record:
  - Status: Pending
  - Assigned to approver(s)
    ↓
Approver Actions:
  - View pending list
  - Read content detail
  - Review content
    ↓
Decision:
  - Approve → Content ready to publish
  - Reject → Return to creator with feedback
    ↓
System:
  - Update approval status
  - Notify creator
  - Update content status
    ↓
Creator:
  - View feedback
  - Make changes
  - Resubmit if rejected
```

---

### 3.7 📅 **Publishing & Scheduling Flow**

**Publish Now:**
```
POST /content/{id}/publish/{integrationId}
  ↓
Validate:
  - Content exists & approved
  - Integration connected
    ↓
Call Provider:
  - Facebook Graph API
  - Post to page/timeline
    ↓
Store:
  - Save Post record
  - Link to content
  - Update content status
    ↓
Result:
  - Published ✓
  - Ready for analytics
```

**Schedule:**
```
POST /content-calendar/schedule/{contentId}
{
  scheduledTime,
  recurring (optional)
}
  ↓
Background Service:
  - Monitor due schedules
  - At scheduled time:
    * Publish automatically
    * Store in Post
    * Log success/failure
    ↓
Features:
  - One-time or recurring
  - Update/delete schedule
  - View upcoming
  - Retry on failure
```

---

### 3.8 📊 **Ad Campaign Flow**

```
Step 1: Create Campaign
  ↓
  POST /ad-campaign
  {
    brand, profile, ad_account
  }
  ↓ Facebook Marketing API
  ↓ Store campaign + Facebook ID

Step 2: Create Ad Set
  ↓
  POST /ad-set/{campaignId}
  {
    budget, schedule, targeting
  }
  ↓ Facebook API
  ↓ Store ad set + Facebook ID

Step 3: Create Ad Creative
  ↓
  Option A: From Content
    - Select content
    - Map to creative format
  Option B: From Facebook Post
    - Select existing post
    ↓
  POST /ad-creative
  ↓ Facebook API
  ↓ Store creative + Facebook ID

Step 4: Create Ad
  ↓
  POST /ad/{creative_id}
  {
    ad_set_id
  }
  ↓ Facebook API
  ↓ Store ad + Facebook ID

Step 5: Manage & Monitor
  ↓
  - Preview ads
  - Update status
  - Pull insights
  - Track performance
```

---

### 3.9 📈 **Reporting Flow**

```
User Access Dashboard/Reports
    ↓
Backend Collect Data:
  - System stats (users, content, posts)
  - Ad data from Facebook Insights API
  - Performance metrics
    ↓
Frontend Display:
  - Stats dashboard
  - Charts (Recharts)
  - Data tables
  - Report generation
    ↓
Export:
  - PDF/Excel export
  - Scheduled reports (optional)
    ↓
Current Scope:
  - Basic operational metrics
  - Facebook performance data
  - No AI predictions yet
```

---

## 🏗️ 4. ADMIN FEATURES

### 4.1 👥 **Admin User Management**

**Features:**
```
GET    /admin/users                    - List users
GET    /admin/users/{userId}           - User detail
GET    /admin/users/{userId}/profiles  - User profiles
GET    /admin/profile/me               - Admin profile
```

**Visible Data:**
- User info (name, email, created date)
- Account status
- Profiles linked
- Subscription info
- Payment history

---

### 4.2 💳 **Admin Payment & Subscription**

**Endpoints:**
```
GET    /admin/payment/all                        - All payments
GET    /admin/payment/user/{userId}              - User payments
GET    /admin/subscription/all                   - All subscriptions
GET    /admin/subscription/user/{userId}         - User subscriptions
POST   /admin/tools/update-payment-method        - Update method
POST   /admin/tools/update-profile-status        - Update status
POST   /admin/tools/update-subscription-plan     - Change plan
```

**Admin Dashboard:**
- Payments list
- Subscriptions list
- User payment/subscription detail
- Manual status updates
- Plan changes

---

### 4.3 🛠️ **Admin Tools**

**Available Tools:**
```
POST   /admin/tools/seed-demo-user        - Create 1 demo user
POST   /admin/tools/seed-batch-users      - Create batch users
POST   /admin/tools/update-payment-method - Change payment gateway
POST   /admin/tools/update-profile-status - Active/Inactive profile
POST   /admin/tools/update-subscription-plan - Change plan
```

**Use Cases:**
- Development & testing
- Demo data setup
- Emergency status fixes
- Manual plan adjustments

---

## 🧠 5. AI CAPABILITIES ANALYSIS

### 5.1 ✅ **Current AI Features**

| Feature | Technology | Status | 
|---------|-----------|--------|
| Text generation | Google Gemini | ✅ Live |
| Chat interface | Google Gemini | ✅ Live |
| Content improvement | Google Gemini | ✅ Live |
| Image generation | Vertex AI Imagen | ✅ Live (ImageText only) |
| Prompt generation | Google Gemini | ✅ Live |
| Conversation history | Database | ✅ Live |

### 5.2 💡 **Prompt Context Strategy**

**Brand Context:**
- Name (e.g., "Nike")
- Description (e.g., "Sports apparel brand")
- Slogan (e.g., "Just Do It")
- USP (e.g., "High-performance, innovation-driven")
- Target Audience (e.g., "Athletes, fitness enthusiasts 18-45")

**Product Context:**
- Name, Description, Price

**User Message:** Direct request or chat input

**Example Prompt:**
```
You are creating marketing content for brand: Nike
Brand Description: Innovative sports apparel
Brand Slogan: Just Do It
Brand USP: Premium quality, cutting-edge innovation
Target Audience: Athletes and fitness enthusiasts

For product: Air Max 90 Sneakers ($120)
Create compelling ad copy for social media.
```

---

### 5.3 🚀 **Future AI Enhancements (Not Yet Implemented)**

| Feature | Status | Priority | 
|---------|--------|----------|
| GPT-4o integration | Planned | Medium |
| DALL-E support | Planned | Medium |
| Sentiment analysis | Future | Low |
| Trend prediction | Future | Low |
| Budget recommendations | Proposed | Medium |
| Campaign optimization | Proposed | Medium |
| AI video generation | Proposed | Low |

---

## 🛣️ 6. TECHNOLOGY STACK

### Backend
```
Framework:    ASP.NET Core 8 Web API
Database:     PostgreSQL
ORM:          Entity Framework Core
Validation:   FluentValidation
Auth:         JWT Bearer
API Docs:     Swagger/OpenAPI
```

### Frontend (User)
```
Framework:    Next.js 15
UI Library:   React 19
Language:     TypeScript
Styling:      Tailwind CSS
Components:   Radix UI / shadcn
Data:         TanStack Query
Charts:       Recharts
Icons:        lucide-react
```

### Frontend (Admin)
```
Framework:    Next.js 15
UI Library:   React 19
Language:     TypeScript
Styling:      Tailwind CSS
Components:   Radix UI
Tables:       TanStack Table
Data:         TanStack Query
Charts:       Recharts
Icons:        lucide-react
```

### AI Services
```
Text & Chat:   Google Gemini
Image Gen:     Google Vertex AI Imagen
Prompt Gen:    Google Gemini
```

### Infrastructure
```
Storage:       Supabase Storage
Database:      PostgreSQL (on Supabase)
Social APIs:   Facebook Graph, Marketing APIs
Payment:       PayOS
```

---

## 📌 7. KEY LIMITATIONS & CONSTRAINTS

### Current Limitations

| Area | Limitation | Impact |
|------|-----------|--------|
| **Platforms** | Only Facebook fully implemented | Instagram/TikTok expansion needed |
| **AI** | Gemini + Imagen only | No GPT-4o or DALL-E |
| **Video** | VideoText stored but no AI video gen | Video creation manual |
| **Analytics** | Basic dashboard only | No AI insights/predictions |
| **Admin** | Enum-based plans | No dynamic plan CRUD |
| **Governance** | Team roles exist but not strictly enforced | Need 1-Leader-per-team validation |
| **Error Handling** | Basic error handling | Need robust retry/fallback policies |
| **Testing** | Partial test coverage | Need comprehensive test suite |

---

## 🎯 8. RECOMMENDED PRIORITIES

### 🔴 **SHORT-TERM (1-3 months)**

1. **🔒 Enforce Team Governance**
   - Implement 1 Leader per team rule
   - Add database constraints
   - Validate in services

2. **✅ Robust Error Handling**
   - Implement retry policies
   - Add fallback strategies
   - Improve error logging

3. **🧪 Increase Test Coverage**
   - Unit tests for services
   - Integration tests for APIs
   - Focus: payment, publishing, approval, AI

4. **🔌 Provider Abstraction**
   - Create AI provider interface
   - Abstract Social provider logic
   - Abstract Payment provider logic

5. **📋 API Documentation**
   - Complete API documentation
   - Add endpoint examples
   - Document error scenarios

### 🟡 **MID-TERM (3-6 months)**

6. **Dynamic Subscription Plans** - Admin CRUD for plans
7. **Instagram Support** - Full provider implementation
8. **Advanced Analytics** - Improved reporting, exports
9. **Provider Monitoring** - Track AI/Social/Payment provider health
10. **Background Job Queue** - Robust job processing

### 🟢 **LONG-TERM (6+ months)**

11. **Multi-Model AI** - GPT-4o, DALL-E integration
12. **Video Generation** - AI video creation
13. **Advanced Analytics** - Sentiment, trends, predictions
14. **Campaign Optimization** - AI-powered optimization
15. **Multi-Payment Gateway** - Stripe, VNPay support

---

## 📝 9. NOTES FOR DEVELOPERS

### Architecture Principles
- ✅ Controller-Service-Repository pattern
- ✅ Separated user and admin frontends
- ✅ API-driven architecture
- ✅ Database-first approach

### Code Quality
- Use FluentValidation for all inputs
- Implement proper error handling
- Add logging throughout
- Create reusable components

### Security
- Never log sensitive data
- Use encryption for tokens/secrets
- Validate all user inputs
- Check permissions on every endpoint

### Testing
- Write tests for business logic
- Mock external services
- Test error scenarios
- Aim for >80% coverage

### Deployment
- Use environment variables
- Implement blue-green deployment
- Monitor error logs
- Setup alerts for failures

---

## 📞 Support & Documentation

For more details:
- API: `https://api.example.com/swagger`
- Admin: `https://admin.example.com`
- User: `https://app.example.com`

---

**Document Version:** 1.0
**Last Updated:** May 2026
**Status:** Active Development
