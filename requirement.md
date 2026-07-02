# SOFTWARE REQUIREMENTS DOCUMENT
## AI-POWERED SOCIAL MEDIA ADVERTISING MANAGER (AISAM)

## Approved Requirement Change - Workspace Subscription and Credit Model

Nguon quyet dinh: `CHANGE_REQUEST_WORKSPACE_SUBSCRIPTION_CREDIT_ANALYSIS.md`. Day la requirement muc tieu da phe duyet, **chua phai trang thai code hien tai**.

Chinh sach lifecycle moi nhat: `docs/product/workspace-subscription-expiry-policy.md` (approved 2026-06-24).

- Chuyen ownership boundary tu Profile sang Workspace.
- Profile chi con la thong tin ca nhan/doanh nghiep.
- Moi Workspace co dung mot Owner va mot Credit Wallet.
- Owner co the transfer ownership cho Manager trong cung transaction.
- Credits chi dung cho AI; publish dung Post Quota rieng.
- Plan ke thua toan bo feature cua plan thap hon.

| Plan | Credits khi mua/gia han | Post Quota | Member Limit |
|---|---:|---:|---:|
| Free | 50 / 7 ngay | 20 / tuan | Khong co team |
| Personal Plus | 500 | 300 / thang | Khong co team |
| Personal Pro | 2.000 | 1.000 / thang | Khong co team |
| Business Plus | 15.000 | 5.000 / thang | 10 |
| Business Pro | 50.000 | 20.000 / thang | 50 |

- Maximum Credit Balance: Personal 15.000; Business 500.000.
- Giao dich lam vuot maximum balance bi tu choi toan bo.
- Moi account co dung mot Personal Workspace; account co the so huu/tham gia nhieu Business Workspace.
- Personal Plus/Pro het han thi ha entitlement ve Personal Free; Credits con lai chi dung duoc cho feature Free.
- Business khong co Free tier. Workspace moi khong duoc cap Credits va chua duoc su dung cho den khi Business Plus/Pro thanh toan thanh cong.
- Business het han giu du lieu/members/Credits nhung Read-only va khong duoc tieu Credits cho den khi gia han.
- Het han duoi 90 ngay: Limited Mode.
- Het han 90-180 ngay: Archived; Owner View/Export/Renew, Member View Only.
- Het han tren 180 ngay: Admin co quyen Soft Delete.

## 1. Introduction

### 1.1 Purpose
Tai lieu nay mo ta yeu cau tong the cho he thong AISAM, lam co so de thong nhat pham vi san pham, cac module chuc nang, quy tac nghiep vu, rang buoc ky thuat va dinh huong trien khai. Tai lieu duoc viet theo huong san pham muc tieu cua do an, dong thoi phan biet ro pham vi MVP va pham vi mo rong sau MVP.

### 1.2 Product Overview
AISAM la nen tang web ho tro doanh nghiep nho va nhom marketing:
- Quan ly thuong hieu va san pham.
- Su dung AI de tao noi dung quang cao.
- Ket noi kenh mang xa hoi cua doanh nghiep.
- Len lich va xuat ban noi dung.
- Theo doi hieu suat co ban cua noi dung da dang.
- Quan ly goi dang ky, thanh toan va han muc su dung.

### 1.3 Objectives
He thong huong toi cac muc tieu sau:
- Rut ngan thoi gian tao noi dung quang cao.
- Dam bao noi dung nhat quan voi brand.
- Ho tro quy trinh duyet truoc khi dang.
- Ho tro dang bai va dat lich tap trung tu mot nen tang.
- Kiem soat usage/quota va chi phi AI o muc phu hop voi MVP.
- Tao nen tang de mo rong sang nhieu kenh social va capability AI sau nay.

### 1.4 Stakeholders
Tai lieu nay phuc vu cac nhom stakeholder chinh:
- Giang vien/hoi dong danh gia de xem xet muc tieu va pham vi do an.
- Nhom phat trien de thong nhat pham vi chuc nang.
- Nguoi dung doanh nghiep va nhom marketing la doi tuong su dung chinh.
- Quan tri vien he thong la doi tuong van hanh.

### 1.5 Assumptions
Tai lieu nay duoc xay dung tren cac gia dinh sau:
- Nguoi dung co ket noi Internet on dinh de su dung web app va cac dich vu ben thu ba.
- Cac nen tang social, AI va thanh toan cho phep tich hop thong qua API hop le.
- Doanh nghiep da co du lieu brand va product toi thieu de cung cap cho he thong.
- Moi truong demo co the bi gioi han boi app review, test account hoac sandbox mode cua ben thu ba.

## 2. Problem Statement

Doanh nghiep nho thuong gap cac van de:
- Thieu quy trinh tap trung de quan ly brand, san pham va noi dung marketing.
- Mat nhieu thoi gian de viet caption, tao thong diep quang cao va dieu chinh theo tung chien dich.
- Gap kho khi dang bai tren nhieu kenh va theo doi lich dang.
- Khong co cong cu don gian de theo doi han muc su dung AI va tinh trang goi dang ky.
- Chua co bo khung de tong hop hieu suat noi dung da dang de cai thien chien dich sau.

AISAM duoc xay dung de giai quyet cac van de tren bang mot he thong hop nhat, lay AI va quy trinh dang bai lam trong tam.

## 3. Scope

### 3.1 In-Scope
Pham vi tong the cua san pham bao gom:
- User authentication va account management.
- Quan ly subscription, pricing, payment va quota co ban.
- Quan ly business profile, brand kit va product catalog.
- AI generation cho noi dung quang cao.
- Approval workflow cho noi dung.
- Ket noi social business accounts.
- Dang ngay, len lich dang va theo doi trang thai xuat ban.
- Dashboard va analytics co ban.
- Admin tools de van hanh he thong.

### 3.2 Out-of-Scope for Current MVP
Nhung noi dung sau khong nam trong pham vi MVP hien tai:
- Tu huan luyen hoac fine-tune model AI rieng.
- Ho tro day du tat ca social platforms ngoai Facebook, Instagram, TikTok.
- Video editing nang cao hau ky.
- Dynamic Ads, Pixel optimization, A/B testing tu dong.
- Quota nang cao theo diem, theo token, theo tung loai tai nguyen chi tiet.
- Analytics nang cao theo thoi gian thuc, du doan xu huong hoac toi uu chien luoc tu dong.

### 3.3 System Context
AISAM duoc dinh huong la mot he thong web gom:
- User Web App cho nguoi dung doanh nghiep.
- Admin Web System cho quan tri vien.
- Backend API xu ly nghiep vu, xac thuc, AI orchestration va tich hop.
- Database luu user, profile, brand, product, content, subscription, payment va lich su van hanh.
- External services gom social APIs, AI APIs, payment gateway va storage service.

## 4. User Roles

### 4.1 End User
Nguoi dung doanh nghiep hoac nhan su marketing su dung he thong de:
- Quan ly profile, brand, product.
- Tao va chinh sua content.
- Goi AI de sinh hoac cai tien noi dung.
- Gui duyet, phe duyet theo quy trinh duoc cap quyen.
- Len lich hoac dang bai.
- Theo doi quota, subscription va performance co ban.

### 4.2 Team Leader / Approver
Nguoi dung co quyen duyet noi dung truoc khi len lich hoac dang bai:
- Xem hang cho duyet.
- Approve/reject content.
- Theo doi notification lien quan den scheduling, publishing va approval.

### 4.3 Administrator
Quan tri vien van hanh he thong:
- Dang nhap vao he thong admin.
- Quan ly nguoi dung, profile, payment va subscription.
- Cau hinh goi dang ky/pricing theo pham vi duoc phep.
- Theo doi tinh trang tich hop va usage/chi phi AI.

## 5. High-Level Use Cases

### 5.1 End User Use Cases
- Dang ky, dang nhap va quan ly tai khoan.
- Tao business profile, brand kit va product catalog.
- Chon plan, thanh toan va theo doi quota.
- Ket noi social accounts cua doanh nghiep.
- Tao content thu cong hoac bang AI.
- Gui content di duyet, nhan feedback va refine.
- Dang ngay hoac len lich dang bai.
- Theo doi dashboard va chi so co ban.

### 5.2 Team Leader / Approver Use Cases
- Xem danh sach content dang cho duyet.
- Approve/reject content.
- Theo doi thong bao lien quan den publishing va approval.

### 5.3 Administrator Use Cases
- Dang nhap he thong admin.
- Xem va quan ly user, profile, subscription, payment.
- Theo doi usage AI va tinh trang tich hop.
- Xu ly cac tinh huong ho tro van hanh.

## 6. Functional Requirements

### 6.1 Authentication and Account Management
He thong phai ho tro:
- Dang ky tai khoan bang email va mat khau.
- Dang nhap va dang xuat.
- JWT-based authentication.
- Quan ly session va refresh token.
- Quen mat khau, dat lai mat khau.
- Xac minh email.
- Admin login tach biet voi vai tro quan tri.

### 6.2 Subscription, Pricing and Quota
He thong phai ho tro:
- Hien thi danh sach goi dang ky.
- Xem thong tin plan, gia, chu ky va han muc su dung.
- Dang ky goi va thanh toan qua PayOS.
- Theo doi subscription dang active.
- Ho tro auto-renew theo chinh sach he thong.
- Nang cap, gia han hoac thay doi goi.
- Xem lich su giao dich.
- Hien thi quota con lai.
- Tru Credits sau khi AI generate thanh cong; AI that bai khong tru Credits.
- Publish khong tru Credits va duoc kiem soat bang Post Quota rieng.
- Chan AI neu Workspace khong du Credits hoac feature bi khoa theo plan.
- Chan publish neu Workspace het Post Quota.
- Quan ly Credit Wallet duy nhat cua Workspace va tu choi giao dich lam vuot Maximum Credit Balance.
- Ap dung Plan Feature Gate, Member Limit va Permission Matrix da phe duyet.
- Tach entitlement khoi Credit balance: co Credits khong tu dong mo khoa feature.
- Personal het paid plan phai fallback ve Personal Free; Business het paid plan phai Read-only, khong fallback ve Free.
- Credit grant khi payment/renewal phai idempotent va khong duoc cap khi chi tao Business Workspace.

### 6.3 Workspace, Business Profile and Brand Kit
He thong muc tieu phai ho tro:
- Tao Personal Workspace mac dinh khi register.
- Moi account chi co mot Personal Workspace.
- Chi tao moi Business Workspace tu man hinh `/overview`; khong tao Workspace trong dashboard cua Workspace dang active.
- Business Workspace moi phai qua Business Plus/Pro payment; khong ton tai Business Free.
- Chon Active Workspace.
- Moi Workspace co dung mot Owner va mot Credit Wallet.
- Invite/accept member theo Member Limit.
- Ownership Transfer tu Owner sang Manager trong cung transaction.
- Business Workspace lifecycle: Limited Mode, Archived va Admin Soft Delete.

Profile chi con luu thong tin ca nhan/doanh nghiep. Workspace la ownership boundary cua du lieu nghiep vu.

He thong phai ho tro:
- Tao, sua, xoa business profile.
- Tao, sua, xoa brand kit.
- Luu thong tin brand: ten, logo, slogan, color theme, USP, target audience.
- Luu nhan vat dai dien thuong hieu va tone of voice.
- Gan hashtag phu hop cho brand.
- Ho tro nhieu brand tren cung mot tai khoan nguoi dung.

### 6.4 Product Catalog
He thong phai ho tro:
- Them, sua, xoa san pham theo tung brand.
- Luu thong tin san pham: ten, mo ta, gia, selling points chinh.
- Ho tro anh san pham theo pham vi MVP.
- Tim kiem va loc san pham theo brand.

### 6.5 Social Media Connection
He thong phai ho tro:
- Ket noi Facebook Page qua OAuth.
- Ket noi Instagram Business Account qua OAuth trong pham vi duoc ho tro.
- Ket noi TikTok Business Account qua OAuth trong pham vi duoc ho tro.
- Xem trang thai ket noi: active, token expired, revoked, auth error.
- Reconnect khi token het han hoac bi thu hoi.
- Ngat ket noi tai khoan va dung cac tac vu publish lien quan.
- Xem danh sach pages/accounts dang duoc quan ly.

### 6.6 AI Content Generation
He thong phai ho tro:
- Chon brand va product lam context dau vao.
- Chon nen tang dich: Facebook, Instagram.
- Chon muc tieu chien dich: awareness, engagement, conversion.
- Chon tone of voice.
- Nhap prompt bo sung dang free-text.
- Bat/tat su dung nhan vat dai dien thuong hieu.
- Chon loai content: `TextOnly`, `ImageText`, `VideoText`.

Quy tac cho tung loai:
- `TextOnly`: sinh caption/text quang cao bang AI.
- `ImageText`: sinh text va anh AI.
- `VideoText`: sinh text va mo ta/video asset theo kha nang duoc ho tro o tung giai doan.

He thong phai:
- Xu ly generation bat dong bo neu can.
- Theo doi trang thai request: pending, processing, completed, failed.
- Luu lich su generation.
- Ho tro improve content dua tren feedback cua nguoi dung.

### 6.7 Content Review and Approval
He thong phai ho tro:
- Hien thi nhieu phien ban noi dung da sinh.
- Chon va phe duyet phien ban phu hop.
- Nhap feedback de AI tao lai ban moi.
- Reject va xoa noi dung khong phu hop.
- Chi cho phep schedule/publish voi content da duoc approve.

Lifecycle toi thieu cua content:
- `Draft`
- `PendingApproval`
- `Approved`
- `Scheduled`
- `Published`
- `Rejected`

### 6.8 Publishing and Scheduling
He thong phai ho tro:
- Dang ngay mot hoac nhieu kenh.
- Dat lich dang bai cho noi dung da approve.
- Xem calendar view cho lich dang.
- Xem trang thai tung bai: scheduled, published, failed.
- Doi lich hoac huy lich khi bai chua dang.
- Thu lai publish that bai theo thao tac thu cong.
- Hien thi thong bao loi publish.

### 6.9 Dashboard and Analytics
He thong phai ho tro:
- Xem tong quan so bai da dang.
- Xem chi so co ban theo bai dang: reach, impressions, engagement, click-through khi co du lieu.
- Loc theo ngay, kenh va chien dich.
- Xem lich su noi dung da xuat ban.
- Ho tro goi y cai thien chien dich trong cac giai doan mo rong neu co.

### 6.10 Notifications
He thong phai ho tro:
- Hien thi notification noi bo.
- Xem notification da doc/chua doc.
- Danh dau da doc tung thong bao hoac tat ca.
- Sinh notification cho approval, scheduling, publish success/failure va su kien he thong quan trong.

### 6.11 Admin Management
He thong admin phai ho tro:
- Dang nhap admin.
- Quan ly danh sach user.
- Xem chi tiet user, profile, subscription va payment.
- Tim kiem/loc user theo trang thai va goi dang ky.
- Kich hoat/vo hieu hoa tai khoan.
- Quan ly goi dang ky va pricing trong pham vi MVP.
- Theo doi tinh trang tich hop API.
- Theo doi usage va chi phi AI theo ngay/tuan/thang o muc co ban.
- Xem nhat ky loi publish va OAuth.

## 7. Business Rules

- Sau Workspace migration, moi content phai thuoc mot Workspace va mot brand hop le.
- Product neu duoc chon phai thuoc dung brand tuong ung.
- Chi content da duoc approve moi duoc len lich hoac dang bai.
- Credits chi bi tru sau khi AI thanh cong; Post Quota duoc cap nhat sau khi publish thanh cong.
- He thong khong cho phep overage trong MVP khi quota da het.
- Subscription duoc tinh theo chu ky thang va co the auto-renew.
- Payment phai co trang thai ro rang va duoc doi chieu qua callback/xac nhan thanh toan.
- Social connection co the bi gioi han boi token, permission va trang thai app review cua ben thu ba.
- Notification phai gan voi profile hoac user phu hop de tranh lo du lieu.

## 8. Non-Functional Requirements

### 8.1 Security
- He thong phai su dung JWT cho API protected.
- He thong phai phan quyen theo role va context profile.
- Secret, token va thong tin thanh toan phai duoc bao ve.
- OAuth flow phai dam bao state validation va revoke/reconnect handling.

### 8.2 Reliability
- Cac tac vu scheduling va publishing phai co co che ghi nhan trang thai thanh cong/that bai.
- He thong phai bao loi ro rang khi AI provider, payment gateway hoac social API gap su co.
- Background processing phai du on dinh cho demo MVP.

### 8.3 Performance
- Cac thao tac CRUD thong thuong phai phan hoi nhanh.
- Cac tac vu AI, media generation va publishing co the xu ly bat dong bo.
- Dashboard va danh sach du lieu phai ho tro pagination va filter.

### 8.4 Scalability
- Kien truc phai cho phep mo rong them social platforms, AI providers va payment providers.
- He thong phai co kha nang mo rong tu MVP sang multi-module production architecture.

### 8.5 Usability
- Giao dien nguoi dung phai de theo doi quota, trang thai content, lich dang va subscription.
- Giao dien admin phai uu tien thao tac van hanh va kiem soat su co.

### 8.6 Compatibility
- Ung dung web phai hoat dong tren cac trinh duyet pho bien.
- Cac tich hop phai tuan thu API va chinh sach cua nen tang ben thu ba.

## 9. System Architecture Summary

AISAM duoc thiet ke theo kien truc nhieu lop:
- Presentation Layer: User Web App va Admin Web System.
- API Layer: cung cap endpoint cho auth, brand, product, content, AI, payment, scheduling, notification va admin.
- Business Layer: xu ly nghiep vu, validation, quota, approval va orchestration voi external providers.
- Data Layer: luu tru entity va lich su van hanh.
- Integration Layer: ket noi AI providers, social platforms, payment gateway va storage.

Kien truc nay cho phep:
- Tach biet ro giao dien va xu ly nghiep vu.
- Mo rong them provider moi trong tuong lai.
- Kiem soat du lieu va permission theo profile/role.

## 10. External Integrations

He thong co the tich hop voi cac nhom dich vu sau:
- AI text generation service.
- AI image generation service.
- AI video generation service trong giai doan mo rong.
- Social platform APIs cho OAuth, publishing va analytics.
- Payment gateway de xu ly checkout va callback.
- Storage service de luu media va tai nguyen upload.

Moi tich hop ben ngoai deu co the anh huong den:
- toc do phan hoi,
- kha nang availability,
- pham vi tinh nang demo,
- chi phi van hanh.

## 11. Constraints and Limitations

- He thong chi ho tro Facebook, Instagram va TikTok trong dinh huong san pham; cac nen tang khac nhu YouTube, LinkedIn, Twitter/X khong nam trong pham vi do an.
- He thong su dung API AI co san nhu Gemini, Vertex AI Imagen va cac dich vu video AI neu duoc bo sung; khong tu huan luyen mo hinh rieng.
- Mot so tinh nang OAuth, publishing hoac analytics co the bi gioi han trong moi truong demo/test do quy trinh app review cua Meta va TikTok chua hoan tat.
- Chat luong anh/video AI phu thuoc vao gioi han va output cua ben thu ba.
- He thong khong ho tro chinh sua video hau ky nang cao.
- Analytics trong MVP chi tap trung vao chi so co ban.
- Chat luong noi dung AI phu thuoc manh vao du lieu Brand Kit va Product Catalog.
- Quota trong MVP duoc theo doi o muc co ban theo so luot prompt AI va so luot dang bai; cac co che nang cao se thuoc giai doan sau.

## 12. MVP Scope and Future Scope

### 12.1 MVP Scope
MVP cua AISAM tap trung vao:
- Auth va account management.
- Subscription va payment co ban.
- Brand kit va product catalog.
- AI text generation va image-assisted generation o muc MVP.
- Content lifecycle va approval flow co ban.
- Social connection phuc vu demo.
- Publish now, one-time scheduling, notification va dashboard summary.
- Admin monitoring co ban.

### 12.2 Post-MVP Scope
Sau MVP, he thong co the mo rong:
- Dynamic subscription plans va pricing config linh hoat.
- Instagram/TikTok publishing va analytics day du hon.
- AI video generation pipeline day du.
- Quota tracking nang cao theo token, loai content, ngay/thang.
- Analytics nang cao, AI recommendations va optimization.
- Team governance, audit log va monitoring nang cao.

## 13. Acceptance Summary

Tai lieu requirement duoc xem la dat muc tieu neu he thong dat duoc cac nhom nang luc sau:
- Nguoi dung co the dang ky, dang nhap va quan ly tai khoan an toan.
- Nguoi dung co the tao brand, san pham va noi dung trong mot quy trinh thong nhat.
- AI co the ho tro sinh va cai tien noi dung quang cao dua tren context brand/product.
- Noi dung co quy trinh duyet ro rang truoc khi dang.
- He thong co the ket noi kenh social, dang ngay hoac dat lich dang.
- Nguoi dung co the theo doi subscription, quota va lich su giao dich.
- Admin co the van hanh he thong, kiem soat user, payment va tinh trang tich hop.
- Cac gioi han MVP va pham vi mo rong duoc mo ta ro rang, khong gay hieu nham ve muc do hoan thien cua tung tinh nang.

## 14. Requirement Summary by Module

| Module | Muc tieu chinh |
| --- | --- |
| Authentication | Bao mat tai khoan, phien dang nhap va truy cap he thong |
| Subscription & Payment | Quan ly goi dang ky, thanh toan va quota co ban |
| Brand & Product | Quan ly du lieu dau vao cho AI va social publishing |
| AI Content | Sinh, cai tien va luu vet noi dung quang cao |
| Approval | Dam bao content duoc kiem duyet truoc khi publish |
| Social Integration | Ket noi kenh social de publish va dong bo trang thai |
| Scheduling | Ho tro publish ngay va publish theo lich |
| Analytics | Theo doi hieu suat co ban cua content/chien dich |
| Notification | Bao su kien noi bo lien quan den workflow |
| Admin | Van hanh, giam sat va xu ly su co he thong |

## 15. Conclusion

AISAM la mot nen tang quan ly noi dung quang cao dua tren AI, huong den viec ket hop brand management, AI content generation, social publishing va subscription-based SaaS operation trong mot he thong tap trung. Trong pham vi do an, tai lieu nay xac dinh ro muc tieu san pham tong the, pham vi MVP, cac module nghiep vu cot loi va cac gioi han can duoc danh gia dung muc khi demo va bao ve.
