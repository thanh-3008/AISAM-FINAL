# SOFTWARE REQUIREMENT SPECIFICATION (SRS)
## AI-POWERED SOCIAL MEDIA ADVERTISING MANAGER (AISAM)
### BAN HOP NHAT GIUA SOURCE CODE HIEN TAI VA DINH HUONG PHAT TRIEN TUONG LAI

## APPROVED CHANGE NOTICE - WORKSPACE SUBSCRIPTION AND CREDITS

Tai lieu chi tiet: `CHANGE_REQUEST_WORKSPACE_SUBSCRIPTION_CREDIT_ANALYSIS.md`.

- Workspace migration dang duoc trien khai; Phase 9 da hoan thanh Task 9.1-9.15.
- Workspace, member/invitation, ownership transfer, subscription/payment, Credit Wallet, member quota, entitlement, Post Quota va AI Credit charging da co code va automated tests.
- Cac domain Brand/Product/Content/Social van con ownership Profile-based cho den Task 9.16, nen mot so API tam thoi can dong thoi `X-Profile-Id` va `X-Workspace-Id`.
- `WorkspaceTypeEnum`: Personal = 1, Business = 2.
- Moi Workspace co dung mot Owner va mot Credit Wallet.
- Credits chi dung cho AI; publish dung Post Quota rieng.
- Business Plus cap 15.000 Credits va toi da 10 members.
- Business Pro cap 50.000 Credits va toi da 50 members.
- Expired lifecycle da co code: Limited Mode, Archived, sau 180 ngay Admin co quyen Soft Delete.

Phan con lai cua Workspace migration: chuyen ownership domain Task 9.16, backfill/schema lock Task 9.17 va regression/dashboard cuoi Phase 9 Task 9.18.

## Document Control
Tai lieu nay duoc xay dung theo nguyen tac source code hien tai la baseline trien khai. Cac chuc nang da co trong source duoc mo ta trong nhom Current Implemented Features. Cac chuc nang xuat hien trong tai lieu yeu cau goc nhung chua ton tai day du trong source duoc phan loai ro la Planned Features, Future Enhancement, Proposed Advanced AI Features hoac Optional Enterprise Features. Tai lieu khong mo ta cac chuc nang chua co nhu mot phan da hoan thanh.

---

## QUICK OVERVIEW - PHAN TICH HE THONG

### NHOM 1: CHUC NANG DA TRIEN KHAI (Currently Implemented - 14 chuc nang chinh)
| # | Chuc Nang | Chi Tiet |
|---|-----------|---------|
| 1 | Xac thuc & Tai khoan | Dang ky, dang nhap, Google OAuth, JWT Bearer, refresh token, session management |
| 2 | Quan ly Ho so & Goi | Profile, subscription (Free/Plus/Premium/PlusTrial), PayOS payment |
| 3 | Ket noi Facebook | OAuth, lien ket page, ad accounts, targets |
| 4 | Quan ly Brand | CRUD brand, assign/unassign to team, brand context cho AI |
| 5 | Quan ly San pham | CRUD product, upload anh Supabase, lien ket brand |
| 6 | Thu vien Noi dung | CRUD content, 3 loai (TextOnly/ImageText/VideoText), clone, restore |
| 7 | AI sinh Text | Google Gemini cho draft, chat, improve content |
| 8 | AI sinh Anh | Google Vertex AI Imagen (cho ImageText) |
| 9 | AI Chat & Improve | Chat voi context brand/product, cai thien noi dung |
| 10 | Duyet Noi dung | Workflow approval, team permissions, notification |
| 11 | Dang bai & Lap lich | Publish ngay hoac schedule (lap lai), background service |
| 12 | Quang cao Facebook | Campaign, ad set, ad creative, ad, preview, reports, insights |
| 13 | Dashboard & Reports | Dashboard stats, analytics co ban, Facebook insights |
| 14 | Admin Tools | Quan ly user, payment, subscription, seed demo data |

---

### NHOM 2: CHUC NANG DANG/SAP PHAT TRIEN (Planned/Future Features)
| # | Chuc Nang | Trang Thai | Du kien Khi nao |
|---|-----------|-----------|-----------------|
| 1 | Mo rong AI (GPT-4o, DALL-E) | Planned | Mid-term |
| 2 | Sentiment Analysis & Trend Prediction | Future Enhancement | Long-term |
| 3 | AI Video Generation | Proposed Feature | Long-term |
| 4 | AI Strategy & Real-time Optimization | Optional Enterprise | Long-term |
| 5 | Instagram/TikTok/Twitter Support | Planned Platform | Mid-term |
| 6 | Dynamic Subscription Plans (CRUD) | Planned Admin Feature | Mid-term |
| 7 | Stripe/VNPay Payment Gateway | Optional Future | Long-term |
| 8 | Team Leader Single-Owner Governance | Planned Rule | Short-term |

---

### NHOM 3: NHUNG DIEU CAN LAM RO & KIEM TRA (19 Specification Questions)
#### Yeu Cau Cu (8 items)
| # | Van De | Tac Dong | Hanh Dong Can Thiet |
|----|--------|---------|-------------------|
| 1 | Team Permission Model | Ai duoc quyen duyet/dang/quan ly team? | Kiem tra code, enforce ro 1 Leader per team |
| 2 | Subscription Plans | Plans hien dung enum, co the config dong? | Xac dinh nhu cau business, co can CRUD hay khong |
| 3 | Instagram Implementation | Enum co Instagram nhung provider chua ready | Quyet dinh co phat trien Instagram khong |
| 4 | Background Job Reliability | Lich dang bai co retry policy, monitoring? | Kiem tra ScheduledPostingBackgroundService |
| 5 | AI Video Flow | VideoUrl field co nhung chua sinh video AI | Quyet dinh phat trien AI video khi nao |
| 6 | Budget Auto-Optimization | Co tu dong dieu chinh ngan sach quang cao khong? | Them vao roadmap neu can |
| 7 | Provider Architecture | AI/Payment/Social providers co abstraction layer? | Can nhac refactor de de mo rong |
| 8 | Test Coverage | Cac luong chinh co test day du khong? | Bo sung unit test & integration test |

#### Yeu Cau Moi (11 items)
| # | Van De | Chi Tiet Can Lam Ro | Uu Tien |
|----|--------|-------------------|---------|
| AI Quota Management | Tinh theo so lan API / token / so bai / combo? Reset theo ngay/tuan/thang? Hard/soft limit? | High |
| Leader Approval Workflow | Content status flow? SLA bao lau? Xu ly Leader vang mat? Quy tac chuyen quyen? | High |
| Prompting Strategy | Template prompt chuan? Luu history? Versioning? Ai duoc chinh sua? | Medium |
| Content Library | Luu tat ca revisions hay latest only? Phan quyen chi tiet? Version control? Soft/hard delete policy? | High |
| Meta OAuth & Token | Refresh token strategy? Scope toi thieu? Encrypt/rotate keys? Token revocation? | High |
| Scheduled Posts | Co che chay (Cron/Queue/Service)? Frequency check? Retry policy + DLQ? Meta rate limits? | High |
| Ads Automation | UI fields -> Meta params mapping? Validation rules? Manual approval truoc tao ads? Edit after created? | Medium |
| Analytics | API nao? Data latency bao lau? Rate limits? Caching strategy? | Medium |
| Payment & Subscription | Tinh tien calendar/30 days? Proration logic? Refund policy? Error handling? | High |
| Data Model | Team-User-Leader relationship? 1 Brand/User? N Products? Multi-tenant isolation? | High |
| Security & RBAC | Roles/permissions? Audit log retention? Data encryption? API security (rate limit, key rotation)? | High |

---

### WORKFLOW CHINH CUA HE THONG
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

### RECOMMEND: Uu tien phat trien ngan han (Next 3 Months)
1. Enforce Team Governance - Kiem soat quyen han ro rang (Leader/Member roles).
2. Hoan thien Error Handling - Retry policy, logging, fallback strategy.
3. Tang Test Coverage - Unit/integration tests cho payment, publishing, approval, AI.
4. Provider Abstraction - Chuan hoa kien truc AI/Social/Payment providers.
5. Admin Dynamic Plans - Cho phep admin CRUD goi subscription dong (neu can).
6. Instagram Support - Neu co yeu cau business tu stakeholders.

---

## 1. PROJECT OVERVIEW
### Mo ta he thong
AISAM (AI-Powered Social Media Advertising Manager) la nen tang SaaS cho phep:
- Tao noi dung quang cao bang AI.
- Quan ly brand, san pham va noi dung.
- Lap lich va xuat ban bai viet.
- Quan ly quang cao Facebook.
- Quan ly subscription va thanh toan.

Kien truc:
- Backend: .NET 8 + ASP.NET Core.
- Frontend User: Next.js 15 + React 19.
- Frontend Admin: Next.js 15 + React 19.

### AI Capabilities (Hien tai)
| Loai | Cong Nghe | Chuc Nang |
|------|-----------|----------|
| Text AI | Google Gemini | Draft, chat, improve content |
| Image AI | Vertex AI Imagen | Sinh anh quang cao |
| Context | Brand/Product Data | Personaliza noi dung theo thuong hieu |

Khong thuc hien: Tu huan luyen mo hinh AI - chi tich hop API hien co.

### Social & Ads (Hien tai)
| Nen Tang | Trang Thai | API |
|----------|-----------|-----|
| Facebook | Hoan chinh | Facebook Graph API + Marketing API |
| Google | OAuth only | Google OAuth |
| Instagram | Planned | - |
| TikTok/Twitter | Planned | - |

### Payment Gateway (Hien tai)
| Gateway | Trang Thai | Chuc Nang |
|---------|-----------|----------|
| PayOS | Hoat dong | Checkout, webhook, subscription management |
| Stripe | Optional Future | - |
| VNPay | Optional Future | - |

---

## 2. CURRENT IMPLEMENTED FEATURES
### 2.1. Account, Authentication, Profile and Subscription
He thong da co chuc nang dang ky, dang nhap, Google login, refresh token, logout, logout all, session list, change password, forgot password, reset password, change password with token, verify email va resend verification email. Backend su dung JWT Bearer va refresh token de quan ly phien dang nhap. Frontend nguoi dung co cac man hinh login, sign-up, forgot password, verify email, update password, overview account, security va profile.

Profile la ngu canh van hanh quan trong cua he thong. Nguoi dung co the lam viec voi profile, subscription va du lieu lien quan den brand/content/payment trong ngu canh profile. Subscription hien tai co cac plan theo enum Free, Plus, Premium va PlusTrial. Nguoi dung co the xem goi hien tai, chon goi, tao PayOS checkout link, xac nhan thanh toan, xem lich su thanh toan, doi goi hoac huy subscription.

### 2.2. Social Account and Facebook Integration
He thong da trien khai luong ket noi social account thong qua OAuth URL va callback. Sau khi ket noi, backend luu social account/token, cho phep lay available targets, link/unlink targets, lay linked targets, lay accounts-with-targets va link Facebook ad account cho brand/social integration. FacebookProvider dung Facebook Graph API voi cac quyen nhu pages_manage_posts, pages_read_engagement va pages_show_list.

O trang thai hien tai, Facebook la nen tang duoc ho tro ro nhat cho publishing va ads workflow. GoogleProvider ton tai trong he thong chu yeu cho OAuth/login/provider integration. Instagram Business, TikTok va Twitter chi nen duoc xem la dinh huong mo rong hoac future platform support neu chua co provider/flow hoan chinh tuong ung.

### 2.3. Brand Kit Management
He thong da co CRUD brand, restore brand, list brand theo profile/team, assign brand to team va unassign brand from team. Brand model gom name, description, logo_url, slogan, usp va target_audience. Brand duoc dung lam context cho AI chat/generate content va lien ket voi product, content, social integration, ad campaign va team brand.

Brand Kit la du lieu nen de AI sinh noi dung nhat quan voi dinh vi thuong hieu. Cac truong nhu slogan, USP va target audience duoc su dung de tang chat luong prompt. Tai lieu nay giu nguyen scope hien tai: Brand Kit la bo du lieu mo ta thuong hieu, khong mo ta them cac capability chua co nhu brand guideline automation hoac AI brand compliance scoring.

### 2.4. Product Management by Brand
He thong da co CRUD product, restore, list/filter theo brand/search/isDeleted, upload anh san pham len Supabase Storage va luu images dang JSON. Product gom brand_id, name, description, price va images. Product duoc lien ket voi Brand va Content, dong thoi duoc dua vao prompt AI khi chat/generate content.

Luong product hien tai cho phep nguoi dung quan ly danh sach san pham cua tung brand, cap nhat mo ta, gia va hinh anh. Cac truong nhu USP rieng cua product trong tai lieu goc chua thay la truong model rieng trong source hien tai; neu can, co the xem la Planned Data Model Enhancement thay vi chuc nang da hoan thanh.

### 2.5. Content Library and Content Lifecycle
He thong da co create, update, delete, restore, clone, get detail va list content. Content co brand, product, ad type, title, text_content, image_url, video_url, style_description, context_description, representative_character, status, approvals, calendars, posts va ad creatives. AdType hien tai gom TextOnly, ImageText va VideoText.

Content la trung tam cua cac luong AI generation, approval, publishing, scheduling va ads creative. Source hien tai ho tro luu VideoUrl va xu ly ad type VideoText o muc du lieu/publishing, nhung chua co flow AI tu sinh video. Vi vay, AI video generation chi duoc mo ta trong Future AI Enhancements.

### 2.6. AI Content Generation and AI Chat
He thong da co api/ai/generate-draft va api/ai/chat. Code sinh text bang Gemini. Voi ad type ImageText, code tao visual prompt bang Gemini roi goi Vertex AI Imagen de sinh anh, sau do upload anh vao Supabase Storage va tra GeneratedImageUrl. AI chat co the su dung brand/product context de tao noi dung phu hop hon voi thuong hieu va san pham.

He thong da co api/ai/improve/{contentId}, api/ai/approve/{aiGenerationId}, api/ai/generations/{contentId} va conversation APIs. Nguoi dung co the cai thien noi dung, duyet generation, lay lich su generation theo content va xem lich su hoi thoai AI.

### 2.7. Approval and Team Permission
He thong da co team CRUD, user teams, member list, permissions, assign brand to team, team stats va kiem tra quyen theo team. Team member co role, permissions va is_active. Approval module co pending, create, get, update, approve, reject, list by content, list by approver, pending count, delete va restore.

Luong duyet noi dung hien tai ho tro gui content vao approval, xem danh sach pending, approve hoac reject va tao notification lien quan. Source co permission/team support, nhung tai lieu khong khang dinh tuyet doi rang moi team chi co duy nhat mot Leader neu code chua enforce ro o model/service. Yeu cau "single Leader per team" tu tai lieu goc duoc chuyen thanh Planned Governance Rule neu nhom muon siet chat bang database constraint va service validation.

### 2.8. Publishing and Scheduled Posts
He thong da co publish ngay qua api/content/{contentId}/publish/{integrationId}. Backend kiem tra content/integration roi goi provider de publish len Facebook va luu Post record. Voi dat lich, he thong co api/content-calendar/schedule/{contentId}, schedule-recurring, update/delete schedule, upcoming va lich theo team.

Backend co ScheduledPostingService va ScheduledPostingBackgroundService de xu ly cac lich den han. Luong hien tai phu hop voi yeu cau cot loi ve scheduled posts, nhung nen tang publish thuc te van tap trung vao Facebook.

### 2.9. Facebook Ads Management
He thong da co ad campaign, ad set, ad creative va ad. Code tao campaign/ad set/ad/creative qua Facebook Marketing API, lay preview, cap nhat status, xoa, pull reports/insights. Creative co the tao tu content hoac tu Facebook post san co.

Ads workflow hien tai bao gom tao campaign theo brand/profile/ad account, tao ad set voi ngan sach/lich/targeting, tao ad creative tu content hoac Facebook post, roi tao ad trong ad set. Backend luu cac Facebook ID tuong ung de quan ly lifecycle va tuong tac tiep voi Facebook Marketing API.

### 2.10. Dashboard, Reports and Analytics
He thong da co api/dashboard/stats, trang dashboard, reports, posts, campaign reports va logic keo insight tu Facebook cho ads. Frontend co components analytics/reports, bieu do va mot so UI/hook lien quan den xuat bao cao. O trang thai hien tai, analytics tap trung vao hien thi du lieu van hanh va chi so co ban.

Cac tinh nang nhu AI phan tich cam xuc, trend prediction, AI strategy recommendation va AI realtime performance optimization chua thay backend service/controller tuong ung, do do duoc phan loai la Future AI Enhancements hoac Optional Enterprise Features.

### 2.11. Notification and Conversation Management
He thong da co notification list, detail, mark read, mark all read va unread count. Conversation module co list, detail, delete va lien ket chat messages voi AI generation/content. Cac module nay ho tro trai nghiem lam viec theo luong va luu lai lich su tuong tac cua nguoi dung voi AI.

### 2.12. Storage Management
Source dung SupabaseStorageService cho upload, download, list, delete, signed-url va public-url. Service co validate loai file anh/video va gioi han kich thuoc file. Supabase Storage hien la storage layer cho anh san pham, anh AI generated va cac media lien quan.

---

## 3. CURRENT SYSTEM FLOWS
### 3.1. Authentication and Account Flow
Nguoi dung truy cap sign-up hoac login. Backend xu ly register/login/google login, tao access token va refresh token, dong thoi ho tro verify email. Sau khi dang nhap, nguoi dung co the quan ly session, doi mat khau, logout hoac logout all. Neu quen mat khau, nguoi dung thuc hien forgot password va reset password qua token.

### 3.2. Profile, Subscription and PayOS Payment Flow
Nguoi dung tao hoac chon profile de lam viec. Khi chon goi subscription, frontend goi backend de tao PayOS checkout link. Sau khi thanh toan, he thong confirm payment hoac nhan webhook tu PayOS de kich hoat subscription cho profile. Nguoi dung co the xem active subscription, danh sach subscriptions, lich su thanh toan, doi plan hoac huy subscription.

### 3.3. Facebook Social Connection Flow
Nguoi dung yeu cau OAuth URL theo provider. He thong chuyen nguoi dung sang OAuth provider va nhan callback. Backend luu social account/token, sau do nguoi dung lay danh sach page/target kha dung, link target vao brand/social integration va co the link Facebook ad account. Neu token het han hoac thieu quyen, backend tra loi de nguoi dung reconnect hoac cap lai quyen.

### 3.4. Brand, Product and Content Management Flow
Nguoi dung tao brand, cap nhat thong tin brand, them product vao brand va upload anh san pham. Khi tao content, nguoi dung chon brand/product/ad type, nhap title/text/context/style va luu content. Content co the duoc chinh sua, clone, delete/restore, gui duyet, dang ngay, dat lich hoac dung de tao ad creative.

### 3.5. AI Generation Flow
Nguoi dung gui request generate draft hoac chat voi AI kem brandId/productId/adType neu co. Backend kiem tra quyen truy cap brand, dung prompt tu brand/product/user message, goi Gemini de sinh text. Neu adType la ImageText, backend tao prompt hinh anh bang Gemini, goi Vertex AI Imagen de sinh anh, upload anh vao Supabase va tra URL. Nguoi dung co the improve content, xem generations va approve AI generation de cap nhat content.

### 3.6. Approval Flow
Nguoi dung submit content vao approval. Nguoi duyet mo pending queue hoac xem approval theo content/approver, doc chi tiet, approve hoac reject. Khi approval thay doi, service cap nhat trang thai lien quan va tao notification cho nguoi dung. Quy tac phan quyen dua tren team role/permissions hien co trong source.

### 3.7. Publishing and Scheduling Flow
Voi dang ngay, backend nhan contentId va integrationId, kiem tra content/integration roi goi Facebook provider de publish noi dung va luu Post record. Voi dat lich, backend tao ContentCalendar cho content, cho phep dat lich mot lan hoac lap lai, cap nhat/xoa lich, xem upcoming schedules hoac lich theo team. Background service xu ly lich den han.

### 3.8. Ads Campaign Flow
Nguoi dung tao ad campaign theo brand/profile/ad account, sau do tao ad set voi ngan sach, lich chay va targeting. Nguoi dung tao ad creative tu content hoac Facebook post, roi tao ad trong ad set. Backend goi Facebook Marketing API de tao campaign/ad set/creative/ad, luu Facebook ID tuong ung, ho tro preview, update status, delete va pull reports/insights.

### 3.9. Reporting Flow
Nguoi dung vao dashboard, reports, posts hoac campaign/ad detail de xem du lieu. Backend cung cap dashboard stats va ad insights/pull reports. Frontend hien thi du lieu o dang bang, bieu do va cac thanh phan bao cao co ban. Nhung tinh nang du doan hoac toi uu tu dong bang AI chua thuoc current flow.

---

## 4. ADMIN FEATURES
### 4.1. Current Admin User and Profile Management
Admin frontend co dashboard danh sach user, trang chi tiet user, danh sach profile theo user va trang chi tiet profile. Backend co api/users, api/users/{id}, api/users/profile/me va profile controller. Admin co the xem du lieu nguoi dung, profile, subscription lien quan va trang thai van hanh co ban.

### 4.2. Current Admin Payment and Subscription Management
Admin frontend co trang payments, subscriptions va subscriptions theo user. Backend co cac endpoint admin payment nhu /payment/admin/all, /payment/admin/subscriptions, /payment/admin/user/{userId}/payments va /payment/admin/user/{userId}/subscriptions. Admin tools co update-payment-method, update-profile-status va update-subscription-plan.

### 4.3. Current Admin Tools
Admin tools ho tro seed demo user, seed batch users, sua phuong thuc thanh toan, sua trang thai profile va sua plan subscription. Day la cac cong cu van hanh va ho tro du lieu demo. Source hien chua co man hinh quan tri tao, xoa, cau hinh plan dong nhu mot entity plan rieng; dynamic subscription plans duoc dua vao Planned Features.

---

## 5. AI CAPABILITIES
### 5.1. Current AI Capabilities
AI hien tai su dung Gemini cho text generation, prompt generation, AI chat va improve content. Voi ImageText, he thong dung Gemini de tao visual prompt roi goi Google Vertex AI Imagen de tao anh. Ket qua anh duoc upload len Supabase Storage. He thong luu AiGeneration, GeneratedText, GeneratedImageUrl, status va lien ket voi Content/Conversation.

### 5.2. Current Prompt Context
Prompt hien tai co the su dung brand name, description, slogan, USP, target audience, product name, product description va product price. Cach tiep can nay phu hop voi huong AI Integration & Prompt Engineering cua do an, tap trung vao khai thac dich vu AI co san thay vi tu phat trien mo hinh AI.

### 5.3. Planned Advanced AI Features
Cac capability nhu GPT-4o integration, DALL-E image generation, AI sentiment analysis, trend prediction, strategy recommendation, budget recommendation, best-time-to-post recommendation, realtime campaign optimization va AI video generation la future enhancement hoac optional enterprise features. Cac capability nay chua duoc xem la da trien khai trong source hien tai va can thiet ke them API, data model, quota policy, monitoring va fallback strategy neu phat trien.

---

## 6. FUTURE ENHANCEMENTS
### 6.1. Planned Multi-model AI Integration
He thong co the mo rong kien truc AI provider de ho tro them GPT-4o cho text reasoning/content strategy va DALL-E cho image generation. Day la planned/future scope, khong thay the Gemini + Vertex AI Imagen hien tai. Proposed architecture nen dung abstraction layer dang IAIProvider hoac strategy pattern de lua chon provider theo plan, quota, chi phi va use case.

### 6.2. Proposed AI Video Generation
AI video generation tu tai lieu goc duoc phan loai la Proposed Advanced AI Feature. Source hien co VideoUrl va AdType VideoText de luu/publish media video, nhung chua co pipeline sinh video AI. De trien khai, he thong can them video generation provider, job queue/background processing, storage policy, progress tracking, cost quota va moderation flow.

### 6.3. Future Sentiment Analysis and Trend Prediction
Sentiment analysis va trend prediction la Future AI Enhancements. Chuc nang nay co the phan tich comment, engagement, hashtag, campaign result hoac du lieu social cong khai de de xuat insight. Hien tai source chua co service/controller chuyen trach cho phan tich cam xuc hoac du doan xu huong, nen tai lieu chi ghi nhan nhu roadmap feature.

### 6.4. Future AI Strategy Recommendation and Optimization
AI strategy recommendation co the de xuat muc tieu chien dich, audience, ngan sach, thoi gian dang bai va noi dung phu hop dua tren brand/product/campaign history. Realtime optimization co the dieu chinh ngan sach, targeting hoac creative dua tren performance. Day la Optional Enterprise Feature va chua duoc trien khai trong source hien tai. Khi phat trien can dam bao audit trail, manual approval va gioi han quyen de tranh tu dong thay doi campaign ngoai kiem soat.

### 6.5. Planned Instagram Expansion
Instagram expansion la Planned Platform Feature. Tai lieu goc co nhac Facebook va Instagram Business, nhung source hien tai trien khai ro nhat Facebook provider. De ho tro Instagram hoan chinh can them provider flow, permission mapping, business account discovery, publishing endpoint, media validation va report mapping theo Instagram Graph API.

### 6.6. Planned Dynamic Subscription Plans
Dynamic subscription plans la Planned Admin Feature. Source hien tai quan ly plan theo enum va admin tools cap nhat subscription plan. Neu muon quan tri plan dong, he thong can them entity SubscriptionPlan, admin CRUD, pricing/quota configuration, versioning, migration strategy va backward compatibility voi subscription hien co.

### 6.7. Optional Multi-payment Gateway Integration
Stripe/VNPay tu tai lieu goc khong phai payment gateway hien tai. He thong dang dung PayOS. Trong tuong lai co the bo sung Stripe/VNPay nhu optional payment providers thong qua payment provider abstraction, nhung khong nen thay doi current PayOS flow neu khong co yeu cau trien khai.

### 6.8. Future Enterprise Governance Rules
Quy tac moi team chi co mot Leader va chi Leader co quyen duyet/dang/quan ly thanh vien la yeu cau nghiep vu hop ly tu tai lieu goc, nhung can kiem tra va enforce ro trong source bang service validation, database constraint hoac permission policy. Trong tai lieu nay, noi dung do duoc dat la Planned Governance Rule neu chua duoc dam bao day du trong code hien tai.

---

## 7. NON-FUNCTIONAL REQUIREMENTS
### 7.1. Security
He thong can duy tri JWT Bearer authentication, refresh token, email verification, password reset token, authorization theo endpoint va context profile/team. OAuth/social token va payment data can duoc bao ve theo nguyen tac least privilege, secret management va khong log sensitive data. Cac future provider nhu GPT-4o, DALL-E, Stripe/VNPay hoac Instagram phai tuan thu cung chinh sach bao mat.

### 7.2. Performance and Background Processing
Cac request thong thuong can phan hoi nhanh va on dinh. Cac tac vu AI generation, image generation, scheduled posting, report pulling va future video generation co the xu ly bat dong bo hoac qua background service/job queue. He thong can tranh block request dai doi voi cac tac vu ton thoi gian hoac co chi phi API cao.

### 7.3. Scalability
Kien truc hien tai theo Controller-Service-Repository va tach frontend user/admin cho phep mo rong module. Cac future enhancements nen duoc them thong qua abstraction layer va feature flags de khong pha vo current flows. Database schema can ho tro migration co kiem soat.

### 7.4. Reliability and Error Handling
He thong can xu ly loi ro rang khi token het han, thieu quyen social, payment callback that bai, AI provider timeout, storage upload loi hoac Facebook Marketing API tra loi. Cac tac vu background can co retry policy, logging va trang thai de nguoi dung/admin theo doi.

### 7.5. Usability
Frontend can dam bao nguoi dung de thao tac voi brand, product, content, AI generation, scheduling va ads workflow. Admin UI can tap trung vao van hanh, tim kiem, kiem tra user/profile/payment/subscription va xu ly du lieu ho tro. Cac tinh nang future can duoc phan quyen va giai thich ro de tranh nguoi dung hieu nham la he thong tu dong thay doi campaign ngoai y muon.

### 7.6. Compatibility
Ung dung web can tuong thich voi cac trinh duyet pho bien nhu Chrome, Edge, Firefox va Safari. Cac media upload can tuan thu validation hien co cua SupabaseStorageService va cac gioi han cua provider social/ads tuong ung.

---

## 8. TECHNOLOGY STACK
- Backend: .NET 8, ASP.NET Core Web API, Entity Framework Core, Npgsql/PostgreSQL, FluentValidation, JWT Bearer, Swagger/OpenAPI, Controller-Service-Repository.
- Frontend User: Next.js 15, React 19, TypeScript, Tailwind CSS, Radix UI/shadcn-style components, TanStack Query, Recharts, lucide-react.
- Frontend Admin: Next.js 15, React 19, TypeScript, Tailwind CSS, Radix UI, TanStack Query/Table, Recharts, lucide-react.
- AI Service: Google Gemini (text/prompt/chat), Google Vertex AI Imagen (image generation). GPT-4o/DALL-E chi la future/planned providers.
- Social/Ads: Facebook Graph API, Facebook OAuth, Facebook Marketing API. Google OAuth/provider co trong he thong. Instagram expansion la planned feature.
- Payment: PayOS (hien tai). Stripe/VNPay la optional future.
- Storage: Supabase Storage.
- Database: PostgreSQL.

---

## 9. LIMITATIONS & FUTURE ROADMAP
### 9.1. Current Limitations
He thong hien tai tap trung vao Facebook cho publishing va ads; Instagram Business chua nen duoc mo ta la hoan thien. AI hien tai tap trung vao Gemini text/chat va Vertex Imagen image generation; GPT-4o, DALL-E va AI video generation chua duoc trien khai. Analytics hien tai la dashboard/report co ban va Facebook insights; sentiment analysis, trend prediction va realtime optimization chua co backend service chuyen trach.

Admin hien tai co xem user/payment/subscription va admin tools de cap nhat du lieu quan trong, nhung chua co dynamic subscription plan management dang CRUD plan day du. Team Leader single-owner governance la yeu cau nghiep vu hop ly nhung can enforce ro bang code neu muon dua thanh rule bat buoc.

### 9.2. Short-term Roadmap
Trong ngan han, he thong nen hoan thien tai lieu API flow, kiem thu cac luong PayOS, Facebook OAuth, scheduled posting va Facebook Marketing API. Nen bo sung test coverage cho service quan trong nhu payment, content publishing, approval, AI generation va ad creation. Nen chuan hoa permission policy cho team/approval de tranh mau thuan giua UI va backend.

### 9.3. Mid-term Roadmap
Trong trung han, he thong co the mo rong dynamic subscription plan management, cai thien analytics, bo sung export report hoan chinh, tang monitoring cho AI/payment/social provider va chuan hoa provider abstraction cho social/AI/payment. Instagram expansion co the duoc trien khai theo tung buoc sau khi hoan tat provider, permission va media validation.

### 9.4. Long-term Roadmap
Trong dai han, AISAM co the phat trien thanh enterprise marketing automation platform voi multi-model AI, AI strategy recommendation, AI video generation, sentiment analysis, trend prediction, realtime campaign optimization va multi-payment gateway. Tat ca cac capability nay can duoc trien khai co kiem soat, co audit trail, quota management, cost monitoring va user approval workflow de phu hop voi moi truong enterprise.
