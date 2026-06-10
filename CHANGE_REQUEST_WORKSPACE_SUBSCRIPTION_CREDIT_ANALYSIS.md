# CHANGE REQUEST ANALYSIS

## Workspace-Based Subscription, Credit và Business Workspace MVP

> Trạng thái: **ĐANG TRIỂN KHAI - TASK 9.6 ACTIVE WORKSPACE CONTEXT ĐÃ HOÀN THÀNH**
>
> Tài liệu này chỉ phân tích ảnh hưởng và lập kế hoạch triển khai. Không tự ý refactor, đổi tên hoặc cải tiến ngoài Change Request.
>
> Nguồn nghiệp vụ đã đối chiếu:
>
> - `C:\MyGame\AISAM Subscription, Credit & Workspace Model.docx`
> - `C:\MyGame\AISAM Subscription, Credit & Workspace Model (1).docx`
>
> Cập nhật ngày 09/06/2026.

## Quyết định đã được Product Owner xác nhận

Các câu trả lời dưới đây được lấy từ bản mới nhất của `AISAM Subscription, Credit & Workspace Model.docx` và được xem là quyết định chính thức cho Change Request:

| # | Nội dung đã xác nhận |
|---:|---|
| 1 | Chuyển hoàn toàn active ownership context từ `X-Profile-Id` sang `X-Workspace-Id`. |
| 2 | `Profile` chỉ còn lưu thông tin cá nhân/doanh nghiệp, không còn là ownership/subscription boundary. |
| 3 | Mỗi Profile cũ được migration thành một Personal Workspace. |
| 4 | Mapping plan cũ: Free -> Free, Plus -> Personal Plus, Premium -> Personal Pro, PlusTrial -> Personal Plus Trial. |
| 5 | Team Role MVP dùng Owner, Manager, Content Creator và Viewer theo tài liệu. |
| 6 | AI Chat không trừ Credits trong MVP. |
| 7 | Tạm chấp nhận bảng Credit Pack đề xuất. |
| 8 | PayOS dùng `PaymentType` để phân biệt `Subscription` và `CreditPack`. |
| 9 | Credits cấp khi mua/gia hạn plan: Free 50/7 ngày, Personal Plus 500, Personal Pro 2.000, Business Plus 15.000, Business Pro 50.000. |
| 10 | Post Quota theo plan đã được xác nhận: Free 20/tuần, Personal Plus 300/tháng, Personal Pro 1.000/tháng, Business Plus 5.000/tháng, Business Pro 20.000/tháng. |
| 11 | Feature Matrix và Permission Matrix đã được xác nhận theo các bảng trong tài liệu này. |
| 12 | Monthly Assigned Limit reset theo calendar month, vào ngày 01 hàng tháng. |
| 13 | Trong Limited Mode, chỉ Owner được xem Billing và thực hiện Gia hạn. |
| 14 | Chấp nhận migration nhiều bước để tránh mất dữ liệu. |
| 15 | Plan kế thừa toàn bộ feature của plan thấp hơn. |
| 16 | Mỗi Workspace có đúng một Credit Wallet. |
| 17 | Expired Workspace lifecycle: dưới 90 ngày là Limited Mode; từ 90-180 ngày là Archived; trên 180 ngày Admin có quyền Soft Delete. |
| 18 | Maximum Credit Balance: Personal tối đa 15.000 Credits; Business tối đa 500.000 Credits. |
| 19 | Nếu cộng Credits làm vượt Maximum Credit Balance thì từ chối toàn bộ giao dịch. |
| 20 | Archived Workspace: Owner được View, Export, Renew; Member chỉ được View. |
| 21 | Admin Delete Workspace là Soft Delete. |
| 22 | Mỗi Workspace luôn có đúng một Owner. |
| 23 | Owner chỉ có thể chuyển quyền sở hữu cho một Workspace Member đang có role Manager; sau chuyển quyền Manager trở thành Owner và Owner cũ trở thành Manager. |
| 24 | Owner không thể tự remove chính mình nếu chưa Ownership Transfer. |
| 25 | Member limit: Business Plus tối đa 10 members; Business Pro tối đa 50 members. |

### Workspace Type đã xác nhận

```csharp
public enum WorkspaceTypeEnum
{
    Personal = 1,
    Business = 2
}
```

`WorkspaceTypeEnum` là nguồn xác định cho:

- Feature Gate.
- Maximum Credit Balance.
- Quota Rule.
- Workspace lifecycle và Business-specific capability.

### Điều chỉnh Limited Mode đã xác nhận

- Member vẫn đăng nhập được.
- Member vẫn xem dữ liệu cũ và thông tin team.
- Member không được tạo mới, publish, invite, chỉnh role hoặc dùng Premium feature.
- Chỉ Owner được xem Billing và Gia hạn.

### Credits theo Plan đã xác nhận

| Plan | Credits |
|---|---:|
| Free | 50 / 7 ngày |
| Personal Plus | 500 |
| Personal Pro | 2.000 |
| Business Plus | 15.000 |
| Business Pro | 50.000 |

Credits trong bảng trên là số Credits được cấp khi mua hoặc gia hạn plan.

### Member Limit theo Plan đã xác nhận

| Plan | Members |
|---|---:|
| Business Plus | 10 |
| Business Pro | 50 |

Hệ thống từ chối invite/accept invitation nếu việc thêm member làm vượt giới hạn plan.

### Workspace Ownership đã xác nhận

- Mỗi Workspace luôn phải có đúng một Owner.
- Ownership chỉ được transfer từ Owner hiện tại sang một member có role Manager.
- Ownership Transfer phải thực hiện nguyên tử:
  - Manager được chọn -> Owner.
  - Owner hiện tại -> Manager.
- Owner không thể tự remove chính mình khi vẫn là Owner.
- Workspace không được tồn tại trong trạng thái không có Owner hoặc có nhiều hơn một Owner.

### Post Quota theo Plan đã xác nhận

Publish không tiêu tốn Credits.

| Plan | Posts |
|---|---:|
| Free | 20 / tuần |
| Personal Plus | 300 / tháng |
| Personal Pro | 1.000 / tháng |
| Business Plus | 5.000 / tháng |
| Business Pro | 20.000 / tháng |

### Feature Matrix đã xác nhận

Plan kế thừa toàn bộ feature của plan thấp hơn.

| Plan | Feature hiệu lực sau kế thừa |
|---|---|
| Free | Generate Text, Manual Post, Basic Analytics |
| Personal Plus | Toàn bộ Free + AI Image, Content Calendar, Schedule Post, Multi Platform Publish |
| Personal Pro | Toàn bộ Personal Plus + Trend Analysis, Holiday Suggestion, AI Video, Advanced Analytics, Campaign Recommendation |
| Business Plus | Toàn bộ Personal Pro + Team Management, Shared Credits, Shared Workspace, Workspace Dashboard |
| Business Pro | Toàn bộ Business Plus + Lifetime Assigned Limit, Monthly Assigned Limit, Credit Usage Report, Top Member Analytics |

### Credit Wallet và Maximum Balance đã xác nhận

- Mỗi Workspace có đúng **một** Credit Wallet.
- Personal Workspace có Maximum Credit Balance là **15.000 Credits**.
- Business Workspace có Maximum Credit Balance là **500.000 Credits**.
- Mọi luồng cấp Credits, gia hạn và mua Credit Pack phải kiểm tra Maximum Credit Balance trước khi cộng.
- Nếu số dư mới vượt Maximum Credit Balance thì từ chối toàn bộ giao dịch; không cộng một phần.

### Expired Workspace Lifecycle đã xác nhận

| Thời gian kể từ khi hết hạn | Trạng thái | Quy tắc |
|---|---|---|
| Dưới 90 ngày | Limited Mode | Member vẫn đăng nhập/xem dữ liệu/team; khóa thao tác ghi và Premium/Business feature; chỉ Owner xem Billing/Gia hạn |
| Từ 90 đến 180 ngày | Archived | Owner: View + Export + Renew; Member: View Only |
| Trên 180 ngày | Eligible For Admin Deletion | Admin có quyền Soft Delete Workspace |

### Permission Matrix đã xác nhận

| Role | Được phép | Không được phép |
|---|---|---|
| Owner | Full Access, Billing, Subscription, Invite Member, Remove Member, Assign Quota | Remove Owner |
| Manager | Brand, Product, Content, Campaign, View Team Usage | Billing, Subscription, Remove Owner |
| Content Creator | Generate Content, Generate Image, Generate Video, Create Draft, Publish | Member Management, Billing |
| Viewer | Dashboard, Analytics | Create, Edit, Publish |

---

## 0. Nội dung Change Request đã xác định

### Quyết định nghiệp vụ

- Chuyển Subscription từ `Profile` sang `Workspace`.
- `Workspace` là phạm vi sở hữu của Credits, Brands, Products, Contents và Campaigns.
- Credits chỉ dùng cho các tác vụ AI.
- Publish không trừ Credits, nhưng bị giới hạn bằng Post Quota riêng.
- Free Plan cấp 50 Credits dùng thử và reset mỗi 7 ngày.
- Paid Plan cấp Credits khi mua hoặc gia hạn; Credits không tự reset, không mất khi Subscription hết hạn và được cộng dồn.
- Subscription và Credits là hai khái niệm độc lập:
  - Subscription quyết định feature, team size và business capability được mở khóa.
  - Credits quyết định số lần sử dụng AI.
- Khi Subscription hết hạn, Workspace giữ Credits:
  - Feature Free/basic vẫn có thể dùng Credits.
  - Feature bị khóa theo plan không thể dùng dù còn Credits.
- Khi Business Subscription hết hạn, Workspace đi qua lifecycle Limited Mode -> Archived -> Eligible For Admin Deletion; dữ liệu và team không bị mất trong Limited Mode.
- Gia hạn cùng gói khi Subscription còn hạn sẽ cộng thêm thời gian từ ngày hết hạn hiện tại, không làm mất số ngày còn lại.
- Prompt History MVP chỉ lưu metadata: User, Action, Credits, Time, Status; không lưu full prompt trong lịch sử sử dụng credit.
- MVP hỗ trợ:
  - Personal: Free, Personal Plus, Personal Pro.
  - Business: Business Plus, Business Pro.
  - Business MVP: Workspace, Invite Member, Member Join Workspace, Shared Credits, Lifetime/Monthly Assigned Quota, Role Management và Workspace Dashboard.

### Chi phí AI dự kiến

| Tác vụ | Credits |
|---|---:|
| Generate Caption/Text | 1 |
| Regenerate/Refine | 1 |
| Generate Image | 5 |
| Generate Video | 20 |
| Trend Content Generation | 2 |
| Campaign Recommendation | 2 |

### Ngoài phạm vi Change Request hiện tại

- Department Structure.
- Multi-workspace nâng cao.
- Approval Workflow.
- Audit Log nâng cao.
- Advanced Analytics.
- AI Budget Recommendation.
- Thay đổi thuật toán/provider Gemini, Facebook, PayOS hoặc SMTP.

### Quy tắc mới từ tài liệu Word cần áp dụng

| Nội dung | Quy tắc |
|---|---|
| Free Credits | 50 Credits, reset mỗi 7 ngày |
| Paid Credits | Cấp khi mua/gia hạn, cộng dồn, không tự reset và không mất khi Subscription hết hạn |
| Subscription hết hạn | Giữ Credits nhưng khóa feature Premium và AI feature yêu cầu active Subscription |
| Gia hạn sớm | Cộng thời gian từ ngày hết hạn hiện tại |
| Publish Quota | Free: 20/tuần; Personal Plus: 300/tháng; Personal Pro: 1.000/tháng; Business Plus: 5.000/tháng; Business Pro: 20.000/tháng |
| Business Plus | Shared Credit Pool |
| Business Pro | Shared Pool, Lifetime Assigned Limit hoặc Monthly Assigned Limit |
| Team Role MVP | Owner, Manager, Content Creator, Viewer |
| Workspace Dashboard | Credits còn lại, Posts còn lại, Top Members By Usage, Total AI Usage |
| Prompt History | Lưu metadata và Feature Used; không lưu full prompt |
| Credit Pack | Cộng Credits vào Workspace, không tăng thời hạn/feature và Credits không hết hạn |

### Credit Pack được đề xuất trong tài liệu `(1)`

| Pack | Credits | Giá đề xuất |
|---|---:|---:|
| Starter | 100 | 29.000 VNĐ |
| Standard | 500 | 99.000 VNĐ |
| Growth | 1.500 | 249.000 VNĐ |
| Business | 5.000 | 699.000 VNĐ |

### Assigned Quota được đề xuất trong tài liệu `(1)`

| Quota Mode | Quy tắc |
|---|---|
| Shared Pool | Member dùng trực tiếp Credit Pool chung của Workspace |
| Lifetime Assigned Limit | Giới hạn tổng của member, không tự reset; Owner phải tăng limit khi dùng hết |
| Monthly Assigned Limit | Giới hạn theo tháng; `CreditUsed` reset về 0 khi sang kỳ tháng mới |

- Business Plus chỉ dùng Shared Pool.
- Business Pro hỗ trợ cả ba quota mode.

### Các điểm đã sửa so với bản analysis trước

- Bỏ quy tắc Paid Credits reset 30 ngày và không cộng dồn.
- Thay bằng Paid Credits cộng dồn, không tự reset và không mất khi Subscription hết hạn.
- Bổ sung việc Subscription hết hạn vẫn giữ Credits nhưng khóa feature yêu cầu active Subscription.
- Bổ sung gia hạn sớm cộng dồn thời gian.
- Đổi role MVP từ Owner/Admin/Member thành Owner/Manager/Content Creator/Viewer.
- Đưa Assigned Member Quota và Workspace Dashboard vào phạm vi Business MVP.
- Bổ sung catalog Credit Pack, giá đề xuất và quy tắc Credits không hết hạn để chờ xác nhận.
- Tách Assigned Quota thành Lifetime Assigned Limit và Monthly Assigned Limit.
- Bổ sung Business Workspace expiration lifecycle: Limited Mode, Archived và Eligible For Admin Deletion.

---

# BƯỚC 1 - PHÂN TÍCH THAY ĐỔI

## 1.1. Chức năng bị ảnh hưởng

1. Tạo và chọn Workspace đang hoạt động.
2. Tạo Personal Workspace mặc định cho tài khoản mới.
3. Quản lý thành viên, lời mời và role trong Business Workspace.
4. Subscription checkout, kích hoạt gói và xem gói hiện tại.
5. Shared Credits theo Workspace.
6. Trừ Credits sau khi AI generate thành công.
7. Ghi nhận metadata sử dụng Credits.
8. Kiểm tra Credits trước khi generate/regenerate/refine.
9. Post Quota riêng, không trừ Credits khi publish.
10. Ownership của Brand, Product, Content, Campaign và các dữ liệu liên quan.
11. Dashboard quota/usage theo Workspace.
12. Lịch sử thanh toán theo Workspace.
13. Gia hạn Subscription cộng dồn thời gian và Credits.
14. Giữ Credit balance khi Subscription hết hạn nhưng khóa feature yêu cầu active Subscription.
15. Cấu hình Shared Pool, Lifetime Assigned Limit hoặc Monthly Assigned Limit cho member.
16. Workspace Dashboard: Credits/Posts còn lại, Total AI Usage và Top Members By Usage.
17. Mua Credit Pack và cộng Credits không hết hạn vào Workspace.
18. Chuyển Business Workspace hết hạn sang Read-only Limited Mode.
19. Chuyển Workspace hết hạn sang Archived và áp dụng quyền Owner/Member.
20. Admin Soft Delete Workspace đủ điều kiện sau 180 ngày.
21. Áp dụng Feature Gate, Maximum Credit Balance và Quota Rule theo `WorkspaceTypeEnum`.
22. Ownership Transfer giữa Owner và Manager.
23. Enforce mỗi Workspace có đúng một Owner.
24. Enforce member limit theo Business Plan.

## 1.2. Thành phần bị ảnh hưởng

| Thành phần | Mức ảnh hưởng | Nội dung |
|---|---|---|
| Database | Rất cao | Thêm Workspace/Credit tables, đổi ownership và Subscription relation |
| Backend | Rất cao | Context, middleware, service, repository, business rules |
| Frontend | Cao | Active Workspace, workspace/member screens, quota/payment display |
| API | Cao | Context header và một số endpoint workspace/credit mới |
| Authentication | Trung bình | Tạo Personal Workspace mặc định sau đăng ký |
| Authorization | Cao | Quyền truy cập theo Workspace và Workspace Role |
| Report/Dashboard | Trung bình | Usage và quota tổng hợp theo Workspace |
| Notification | Trung bình | Lời mời tham gia Workspace |

## 1.3. File/thư mục dự kiến cần tạo mới

### Backend - Data Model và Enum

- `AISAM-BE/AISAM.Data/Model/Workspace.cs`
- `AISAM-BE/AISAM.Data/Model/WorkspaceMember.cs`
- `AISAM-BE/AISAM.Data/Model/WorkspaceInvitation.cs`
- `AISAM-BE/AISAM.Data/Model/CreditWallet.cs`
- `AISAM-BE/AISAM.Data/Model/CreditUsageRecord.cs`
- `AISAM-BE/AISAM.Data/Model/CreditPack.cs`
- `AISAM-BE/AISAM.Data/Enumeration/WorkspaceTypeEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/WorkspaceMemberRoleEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/WorkspaceInvitationStatusEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/CreditActionEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/CreditUsageStatusEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/MemberQuotaModeEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/WorkspaceStatusEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/PaymentTypeEnum.cs`

### Backend - DTO, Repository, Service và API

- `AISAM-BE/AISAM.Common/Models/WorkspaceDtos.cs`
- `AISAM-BE/AISAM.Common/Models/WorkspaceInvitationDtos.cs`
- `AISAM-BE/AISAM.Common/Models/CreditDtos.cs`
- Credit Pack DTO.
- `AISAM-BE/AISAM.Repositories/Interface/IWorkspaceRepository.cs`
- `AISAM-BE/AISAM.Repositories/Interface/IWorkspaceMemberRepository.cs`
- `AISAM-BE/AISAM.Repositories/Interface/IWorkspaceInvitationRepository.cs`
- `AISAM-BE/AISAM.Repositories/Interface/ICreditRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/WorkspaceRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/WorkspaceMemberRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/WorkspaceInvitationRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/CreditRepository.cs`
- `AISAM-BE/AISAM.Services/Interface/IWorkspaceService.cs`
- `AISAM-BE/AISAM.Services/Interface/IWorkspaceInvitationService.cs`
- `AISAM-BE/AISAM.Services/Interface/ICreditService.cs`
- `AISAM-BE/AISAM.Services/Service/WorkspaceService.cs`
- `AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs`
- `AISAM-BE/AISAM.Services/Service/CreditService.cs`
- `AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs`
- `AISAM-BE/AISAM.API/Utils/WorkspaceContextHelper.cs`
- `AISAM-BE/AISAM.API/Controllers/WorkspaceController.cs`
- `AISAM-BE/AISAM.API/Controllers/WorkspaceInvitationController.cs`
- `AISAM-BE/AISAM.API/Controllers/CreditsController.cs`
- Credit Pack API/service/repository.

### Migration và Tests

- Migration mới trong `AISAM-BE/AISAM.Repositories/Migrations/`, tên đề xuất: `AddWorkspaceSubscriptionCreditModel`
- Unit/integration tests mới tương ứng trong `AISAM-BE/tests/`

### Frontend

- Active Workspace store/context.
- Workspace API client và types.
- Workspace selector.
- Workspace member/invitation screens.
- Credit usage/quota display.

Tên file Frontend cụ thể sẽ được chốt theo cấu trúc FE tại thời điểm triển khai; không tạo trước khi task Backend tương ứng hoàn tất.

## 1.4. File/thư mục dự kiến cần sửa

### Database và Domain Model

- `AISAM-BE/AISAM.Repositories/AISAMContext.cs`
- `AISAM-BE/AISAM.Repositories/Migrations/AISAMContextModelSnapshot.cs`
- `AISAM-BE/AISAM.Data/Model/Subscription.cs`
- `AISAM-BE/AISAM.Data/Model/Profile.cs`
- `AISAM-BE/AISAM.Data/Model/Brand.cs`
- `AISAM-BE/AISAM.Data/Model/Content.cs`
- `AISAM-BE/AISAM.Data/Model/SocialAccount.cs`
- `AISAM-BE/AISAM.Data/Model/SocialIntegration.cs`
- `AISAM-BE/AISAM.Data/Model/AdCampaign.cs`
- `AISAM-BE/AISAM.Data/Model/ContentCalendar.cs`
- `AISAM-BE/AISAM.Data/Model/Notification.cs`
- `AISAM-BE/AISAM.Data/Model/Conversation.cs`
- `AISAM-BE/AISAM.Data/Model/Team.cs`
- `AISAM-BE/AISAM.Data/Enumeration/SubscriptionPlanEnum.cs`

`Product` và `Post` hiện sở hữu gián tiếp qua Brand/Content; repository/service liên quan vẫn phải kiểm tra và cập nhật truy vấn theo Workspace.

### Backend Context, Auth và Dependency Injection

- `AISAM-BE/AISAM.API/Program.cs`
- `AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- `AISAM-BE/AISAM.API/Utils/ProfileContextHelper.cs`
- Auth service/controller liên quan đến đăng ký tài khoản, chỉ để tạo Personal Workspace mặc định.

### Subscription, Payment, Credits và Quota

- `AISAM-BE/AISAM.Common/Models/PaymentDtos.cs`
- `AISAM-BE/AISAM.Common/Models/QuotaDtos.cs`
- `AISAM-BE/AISAM.Common/Models/DashboardSummaryDto.cs`
- `AISAM-BE/AISAM.Repositories/Interface/ISubscriptionRepository.cs`
- `AISAM-BE/AISAM.Repositories/Interface/IPaymentRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/SubscriptionRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/PaymentRepository.cs`
- `AISAM-BE/AISAM.Services/Service/PayOSPaymentService.cs`
- `AISAM-BE/AISAM.Services/Service/QuotaService.cs`
- `AISAM-BE/AISAM.API/Controllers/PaymentController.cs`
- `AISAM-BE/AISAM.API/Controllers/QuotaController.cs`

### AI, Publishing và Dashboard

- `AISAM-BE/AISAM.Services/Service/AIService.cs`
- `AISAM-BE/AISAM.Services/Service/ContentService.cs`
- Dashboard service/repository/controller liên quan.
- Các repository/service/controller đang truy vấn ownership trực tiếp bằng `ProfileId`.

### Tests hiện có cần cập nhật

- Payment service/repository/controller tests.
- Quota service/controller/integration tests.
- AI service tests.
- Content publish service/controller tests.
- Active Profile middleware tests.
- Brand, Content, Profile, Social, Schedule và Dashboard ownership tests.

## 1.5. File/module KHÔNG nên sửa

- Các migration cũ. Chỉ được thêm migration mới.
- JWT generation, refresh token, password hashing, email verification và Google login core.
- Facebook Graph API/provider implementation.
- Gemini HTTP client/provider implementation.
- PayOS signature/HMAC verification protocol.
- SMTP delivery implementation.
- Scheduler engine/provider.
- Analytics nâng cao ngoài quota/usage được yêu cầu.
- UI component/style không liên quan.
- `.env`, secret thật, `.keys`, `bin/`, `obj/`.
- Tên API/class/method hiện có nếu không bắt buộc phải đổi do chuyển context sang Workspace.

---

# BƯỚC 2 - IMPACT ANALYSIS

## 2.1. Database Impact

### Có thay đổi bảng không?

Có.

#### Bảng mới

- `Workspaces`
- `WorkspaceMembers`
- `WorkspaceInvitations`
- `CreditWallets`
- `CreditUsageRecords`
- `CreditPacks` hoặc bảng cấu hình tương đương.

#### Bảng thay đổi

- `Subscriptions`: thay `ProfileId` bằng `WorkspaceId`.
- Các bảng ownership hiện dùng `ProfileId`: bổ sung/chuyển sang `WorkspaceId`.
- `Profiles`: bỏ vai trò ownership/subscription boundary sau khi migration hoàn tất.
- `WorkspaceMembers`: cần role, `QuotaMode`, `CreditLimit`, `CreditUsed`, `CreditPeriodStart` hoặc dữ liệu kỳ tương đương.
- Database/schema và service transaction phải bảo đảm mỗi Workspace có đúng một Owner.
- `Workspaces`: cần `WorkspaceType`, trạng thái, expiration timestamps và soft-delete fields để biểu diễn Personal/Business, Active, Limited Mode, Archived, Eligible For Admin Deletion và Soft Deleted.
- `CreditWallets`: cần unique constraint trên `WorkspaceId` để bảo đảm 1 Workspace = 1 Credit Wallet; cần hỗ trợ kiểm tra maximum balance theo loại Workspace.
- `Payments`: cần `PaymentType` để phân biệt giao dịch `Subscription` và `CreditPack`.

### Có migration không?

Có. Đề xuất chia migration an toàn thành nhiều bước:

1. Thêm bảng Workspace/Credit và các cột `WorkspaceId` nullable.
2. Backfill một Workspace cho mỗi Profile hiện có và chuyển dữ liệu liên quan.
3. Chuyển code/API sang Workspace context.
4. Sau khi xác minh dữ liệu, đặt `WorkspaceId` non-null và loại bỏ relation/cột legacy không còn dùng.

Đây là chiến lược giảm rủi ro migration, không phải mở rộng chức năng.

### Dữ liệu cũ bị ảnh hưởng không?

Có. Phương án mapping đã được xác nhận:

- Mỗi Profile hiện có được tạo một Personal Workspace.
- Subscription hiện có của Profile được gắn vào Workspace mới.
- Brand, Content, Campaign và dữ liệu liên quan của Profile được gắn vào Workspace mới.
- Plan cũ:
  - `Free` -> `Free`
  - `Plus` -> `PersonalPlus`
  - `Premium` -> `PersonalPro`
  - `PlusTrial` -> `PersonalPlusTrial`

Không xóa dữ liệu cũ trong migration đầu tiên.

Credit balance hiện tại phải được bảo toàn khi migrate. Không được reset Credit balance khi chuyển model hoặc khi Subscription hết hạn.

## 2.2. Backend Impact

### API thay đổi

#### API giữ URL nếu có thể, nhưng đổi context sang Workspace

- Payment checkout/history/subscription current.
- AI generate/regenerate/refine.
- Content publish.
- Brand/Product/Content/Campaign CRUD.
- Dashboard summary.

Các API này sẽ dùng `X-Workspace-Id` thay cho `X-Profile-Id`. Đây là breaking change đã được Product Owner xác nhận.

#### API mới dự kiến

- Workspace CRUD tối thiểu.
- Get/switch active Workspace.
- Invite member.
- Accept invitation.
- List/remove/update role member.
- Set/update Lifetime hoặc Monthly Assigned Quota cho Business Pro member.
- Transfer Workspace ownership từ Owner sang Manager.
- Get current Workspace credits.
- Get credit usage summary/history metadata.
- Get Workspace Dashboard summary.
- Purchase Credit Pack.

#### API cần đổi hoặc thay thế

- `GET /api/quota/profile/{profileId}` không còn đúng ownership.
- Đề xuất thay bằng API Workspace credit/post quota; tên endpoint cuối cùng cần được xác nhận trước khi code.

### Service thay đổi

- Subscription/Payment chuyển từ Profile sang Workspace.
- QuotaService tách rõ Credits và Post Quota.
- Subscription renewal:
  - Nếu gia hạn khi gói còn active, cộng duration từ ngày hết hạn hiện tại.
  - Nếu gia hạn khi đã hết hạn, tính duration từ thời điểm kích hoạt/gia hạn.
  - Cộng Credits mới vào Credit Wallet, không ghi đè số dư cũ.
- AIService:
  - Kiểm tra đủ Credits trước khi gọi AI.
  - Kiểm tra feature yêu cầu Subscription active trước khi dùng Credits.
  - Chỉ trừ Credits sau khi generate thành công.
  - Regenerate/refine tính Credits.
  - Hiện tại luồng `ImproveAsync` chưa kiểm tra quota, nên bắt buộc sửa theo Change Request.
- ContentService publish chỉ kiểm tra Post Quota, không trừ Credits.
- Workspace/Invitation/Credit services được bổ sung.
- Free Credit reset cần cơ chế scheduled processing hoặc cơ chế tính kỳ tương đương; lựa chọn kỹ thuật phải dùng pattern scheduler hiện có, không tự tạo kiến trúc mới.
- Member quota:
  - Business Plus chỉ dùng Shared Pool.
  - Business Pro cho phép Shared Pool, Lifetime Assigned Limit hoặc Monthly Assigned Limit.
  - Lifetime limit không tự reset.
  - Monthly limit reset `CreditUsed` theo tháng.
  - Assigned member hết quota bị chặn dù Workspace vẫn còn Credits.
- Workspace ownership:
  - Ownership Transfer chỉ từ Owner sang Manager.
  - Việc đổi Owner mới và hạ Owner cũ thành Manager phải nguyên tử.
  - Không cho remove Owner hiện tại.
  - Không cho Workspace thiếu Owner hoặc có nhiều Owner.
- Member limit:
  - Business Plus tối đa 10 members.
  - Business Pro tối đa 50 members.
  - Invite/accept invitation phải kiểm tra giới hạn trước khi thêm member.
- Credit Pack:
  - Chỉ cộng Credits vào Workspace.
  - Không tăng Subscription duration.
  - Không mở khóa feature.
  - Credits từ Credit Pack không hết hạn.
  - PayOS Payment phải lưu `PaymentType = CreditPack`; giao dịch mua/gia hạn gói dùng `PaymentType = Subscription`.
- Business Workspace hết hạn:
  - Chuyển sang Read-only Limited Mode.
  - Giữ Workspace, members, dữ liệu và Credit balance.
  - Khóa invite, role management, assigned quota, publishing và Business/Premium features.
  - Member vẫn đăng nhập, xem dữ liệu cũ và xem team.
  - Chỉ Owner được xem billing và gia hạn.
- Workspace expiration lifecycle:
  - Hết hạn dưới 90 ngày: Limited Mode.
  - Hết hạn từ 90 đến 180 ngày: Archived; Owner được View/Export/Renew, Member chỉ View.
  - Hết hạn trên 180 ngày: Admin có quyền Soft Delete.
- Credit Wallet:
  - Mỗi Workspace có đúng một Credit Wallet.
  - Personal maximum balance: 15.000 Credits.
  - Business maximum balance: 500.000 Credits.
  - Mọi thao tác cộng Credits phải kiểm tra maximum balance.
  - Nếu vượt maximum balance thì từ chối toàn bộ giao dịch.
- Workspace Type:
  - `Personal = 1`.
  - `Business = 2`.
  - Feature Gate, Maximum Credit Balance và Quota Rule dựa trên Workspace Type.

### Repository thay đổi

- Truy vấn subscription/payment/quota chuyển từ `ProfileId` sang `WorkspaceId`.
- Các repository ownership của Brand/Content/Campaign và dữ liệu liên quan chuyển sang Workspace.
- Credit deduction và usage record phải được lưu nhất quán trong cùng transaction.
- Credit grant/top-up và maximum balance validation phải được lưu nhất quán trong cùng transaction.
- Soft Delete Workspace phải giữ dữ liệu trong database và loại Workspace khỏi các truy vấn active mặc định.
- Credit usage metadata phải đủ dữ liệu để tính Total AI Usage, Top Members By Usage và Monthly Assigned Limit.

### Lưu ý Prompt History

Quy tắc “không lưu full prompt” áp dụng cho `CreditUsageRecord`. Không tự ý xóa prompt/content đang được các chức năng AI hiện tại sử dụng trong `AiGeneration`, conversation hoặc content nếu chưa có Change Request riêng.

## 2.3. Frontend Impact

### Màn hình thay đổi

- Protected shell/header: chọn và hiển thị Active Workspace.
- Subscription/payment: hiển thị gói của Workspace.
- Dashboard: Credits còn lại, Post Quota và usage theo Workspace.
- Brand/Product/Content/Campaign: gửi Workspace context.

### Màn hình mới

- Workspace setup/create.
- Workspace member list.
- Invite member.
- Accept invitation.
- Role management: Owner, Manager, Content Creator, Viewer.
- Member quota mode: Shared Pool, Lifetime Assigned Limit hoặc Monthly Assigned Limit.
- Credit usage metadata.
- Workspace Dashboard: Credits Remaining, Posts Remaining, Top Members By Usage, Total AI Usage.
- Credit Pack purchase screen.
- Business Workspace Limited Mode screen/state.

### Form thay đổi

- Checkout plan theo Workspace.
- API request cần gửi `X-Workspace-Id`.
- Business Workspace invitation gồm email và role.
- Business Pro invitation/member form cần `QuotaMode`; nếu là Lifetime/Monthly Assigned Limit thì nhập `CreditLimit`.
- Subscription screen phải phân biệt Subscription status và Credit balance.
- Khi Subscription hết hạn nhưng còn Credits, UI phải hiển thị Credits vẫn được giữ và feature Premium đang bị khóa.
- Business Workspace hết hạn phải hiển thị Read-only Limited Mode:
  - Member vẫn đăng nhập, xem dữ liệu cũ và xem team.
  - Chỉ Owner truy cập billing/gia hạn.

Không thay đổi UI/UX hoặc styling ngoài các màn hình/form bắt buộc trên.

## 2.4. Security Impact

Có thay đổi authorization.

- User phải là member của Workspace mới được truy cập dữ liệu Workspace.
- Personal Workspace chỉ có Owner.
- Business Workspace MVP dùng:
  - Owner
  - Manager
  - Content Creator
  - Viewer
- Permission Matrix đã được xác nhận và phải được áp dụng đúng như bảng đầu tài liệu.
- Owner quản lý member, billing, subscription và member quota.
- Owner được transfer ownership cho Manager nhưng không được tự remove khi vẫn là Owner.
- Manager quản lý Brand, Product, Content, Campaign và xem Team Usage.
- Content Creator generate AI content, tạo draft và publish.
- Viewer chỉ xem Dashboard và Analytics.
- Mọi API dùng `X-Workspace-Id` phải kiểm tra membership, không chỉ tin header.
- Shared Credits áp dụng cho toàn Workspace.
- Business Pro member dùng Lifetime/Monthly Assigned Quota phải đồng thời thỏa:
  - Workspace còn Credits.
  - Member chưa dùng hết Assigned Quota.
- Monthly Assigned Limit phải reset usage theo tháng; Lifetime Assigned Limit không tự reset.
- Monthly Assigned Limit reset vào ngày 01 hàng tháng theo calendar month.
- Trong Business Limited Mode, member chỉ được xem dữ liệu cũ; mọi thao tác ghi phải bị chặn theo business rule.

Không thay đổi JWT payload/format nếu không cần thiết. Workspace context có thể được xác thực qua middleware và database membership.

---

# BƯỚC 3 - UPDATE DOCUMENTATION

## 3.1. SRS

### Requirement cũ

```text
User -> Profile -> Subscription
Profile sở hữu dữ liệu và quota.
AI Prompt Quota và Post Quota thuộc Subscription của Profile.
```

### Requirement mới

```text
User -> Workspace -> Subscription
Workspace sở hữu Brands, Products, Contents, Campaigns và shared resources.
Credits chỉ dùng cho AI.
Publish dùng Post Quota riêng.
Free Credits reset mỗi 7 ngày.
Paid Credits không tự reset, không mất khi Subscription hết hạn và cộng dồn khi gia hạn.
Subscription mở khóa feature; Credits giới hạn AI usage.
Business Workspace hỗ trợ member, invitation, shared credits, lifetime/monthly assigned quota, role, workspace usage dashboard và Limited Mode khi hết hạn.
```

### Tài liệu cần cập nhật

- [x] `README.md`
- [x] `requirement.md`
- [x] `BACKEND_CODE_PLAN.md`
- [x] `SETUP_GUIDE.md`
- [x] `user_story_list.md`
- [x] `FRONTEND_CODE_PLAN.md`
- [x] `SPECIFICATION_ANSWERS.md`
- [x] `DEVELOPMENT_GUARDRAILS.md`
- [x] `AISAM_BACKEND_PROGRESS_VS_SRS.md`
- [x] `AISAM-FE/user-story-detail/us-44.md`
- [x] `AISAM-FE/user-story-detail/us-45.md`
- [x] `AISAM-FE/user-story-detail/us-47.md`
- [x] `AISAM-FE/user-story-detail/US-53-admin-profile-subscription-payment-story-detail.md`
- [x] `AISAM-FE/user-story-detail/US-56-ownership-boundary-testing-story-detail.md`
- [x] `AISAM-FE/user-story-detail/US-59-team-permissions-story-detail.md`
- [x] `AISAM-FE/src/features/payment/README.md`
- [x] `BACKEND_CODE_PLAN.md`: da chot thu tu Phase 9 Workspace -> Phase 10 Admin theo Workspace -> Phase 11 Facebook Ads -> Phase 12 Release.
- [x] `AISAM-FE/user-story-detail/US-60-facebook-ads-mvp-story-detail.md`

Các tài liệu tren da duoc cap nhat theo dang approved/planned change. Source code hien tai van duoc mo ta rieng la Profile-based baseline cho den khi migration hoan tat.

## 3.2. Use Case bị ảnh hưởng

- Register account và tạo Personal Workspace.
- Create/select Workspace.
- Invite/join/manage Workspace member.
- Subscribe/upgrade Workspace plan.
- View payment history/current subscription.
- Generate/regenerate/refine AI content.
- Publish content.
- View credit/post quota.
- Manage Brand/Product/Content/Campaign.

Các user story hiện có về payment, ownership boundary, team permission và dynamic plans phải được rà soát, đặc biệt:

- `AISAM-FE/user-story-detail/us-44.md`
- `AISAM-FE/user-story-detail/us-45.md`
- `AISAM-FE/user-story-detail/us-47.md`
- `AISAM-FE/user-story-detail/US-53-admin-profile-subscription-payment-story-detail.md`
- `AISAM-FE/user-story-detail/US-56-ownership-boundary-testing-story-detail.md`
- `AISAM-FE/user-story-detail/US-59-team-permissions-story-detail.md`
- `AISAM-FE/user-story-detail/US-66-dynamic-subscription-plan-management-story-detail.md`

## 3.3. API Document

API document phải ghi rõ:

- Endpoint giữ nguyên và chỉ đổi active context.
- Endpoint mới cho Workspace, Invitation và Credits.
- Header bắt buộc `X-Workspace-Id`.
- Request/response/error cho:
  - Workspace membership invalid.
  - Insufficient credits.
  - Post quota exceeded.
  - Invitation invalid/expired.
  - Unsupported plan.

Tên endpoint mới và việc thay `X-Profile-Id` phải được xác nhận trước khi cập nhật API document chính thức.

## 3.4. Database Document

Database document phải bổ sung:

- Bảng Workspace, WorkspaceMember, WorkspaceInvitation.
- Bảng CreditWallet và CreditUsageRecord.
- Quan hệ Workspace - Subscription.
- Quan hệ Workspace - owned resources.
- Quy tắc unique membership/invitation.
- Quy tắc Free Credits reset mỗi 7 ngày.
- Quy tắc Paid Credits cộng dồn và không mất khi Subscription hết hạn.
- Quy tắc Subscription renewal cộng dồn thời gian.
- Quy tắc Shared Pool và Assigned Quota.
- Quy tắc Lifetime Assigned Limit và Monthly Assigned Limit.
- Quy tắc Credit Pack không hết hạn và không thay đổi Subscription.
- Quy tắc Business Workspace Read-only Limited Mode.
- Quy tắc Post Quota: Free reset theo tuần, Paid reset theo chu kỳ Subscription.
- Quy tắc migration dữ liệu Profile cũ.

## 3.5. Business Flow

### Flow cũ

```text
User
  -> chọn Profile
  -> Profile Subscription
  -> Prompt/Post Quota
  -> sử dụng AI hoặc Publish
```

### Flow mới - AI

```text
User
  -> chọn Workspace
  -> kiểm tra Workspace Membership
  -> kiểm tra feature có yêu cầu active Subscription không
  -> kiểm tra Workspace Credits
  -> nếu Lifetime/Monthly Assigned Quota thì kiểm tra quota của member
  -> gọi AI
  -> AI thành công
  -> trừ Credits và ghi Credit Usage metadata
```

Nếu AI thất bại thì không trừ Credits.

### Flow mới - Publish

```text
User
  -> chọn Workspace
  -> kiểm tra Workspace Membership
  -> kiểm tra Post Quota
  -> Publish
  -> tăng số Post đã dùng
```

Publish không trừ Credits.

### Flow mới - Business Workspace

```text
Owner tạo invitation
  -> người nhận accept invitation
  -> tạo WorkspaceMember
  -> kiểm tra Member Limit của plan
  -> chọn role
  -> Business Plus dùng Shared Pool
  -> Business Pro chọn Shared Pool, Lifetime Assigned Limit hoặc Monthly Assigned Limit
  -> member dùng resources và Credits theo role/quota
```

### Flow mới - Ownership Transfer

```text
Owner chọn một Workspace Member có role Manager
  -> kiểm tra Workspace hiện có đúng một Owner
  -> transaction:
       Manager được chọn -> Owner
       Owner hiện tại -> Manager
  -> commit
  -> Workspace tiếp tục có đúng một Owner
```

Owner hiện tại không thể tự remove nếu chưa hoàn thành Ownership Transfer.

### Flow mới - Subscription và Credits

```text
Workspace mua hoặc gia hạn Paid Plan
  -> kích hoạt/gia hạn Subscription
  -> nếu còn hạn: cộng duration từ ngày hết hạn hiện tại
  -> cộng Credits mới vào Credit Wallet
  -> không ghi đè Credits còn lại

Subscription hết hạn
  -> giữ nguyên Credit balance
  -> feature Free/basic vẫn có thể dùng Credits
  -> khóa feature yêu cầu active Subscription
  -> gia hạn để mở lại feature
```

### Flow mới - Business Workspace hết hạn

```text
Business Subscription hết hạn
  -> Workspace chuyển Read-only Limited Mode
  -> giữ members, dữ liệu và Credit balance
  -> member vẫn đăng nhập, xem dữ liệu cũ và xem team
  -> khóa create/publish/invite/role/quota và Business/Premium features
  -> chỉ Owner xem billing và gia hạn
  -> gia hạn thành công: mở lại Workspace và cộng Credits mới

Hết hạn đủ 90 ngày
  -> Workspace chuyển Archived
  -> Owner được View + Export + Renew
  -> Member chỉ được View

Hết hạn trên 180 ngày
  -> Workspace đủ điều kiện để Admin Soft Delete
```

### Flow mới - Credit Pack

```text
Workspace mua Credit Pack
  -> PayOS thanh toán thành công
  -> PaymentType = CreditPack
  -> kiểm tra Maximum Credit Balance
  -> nếu vượt maximum: từ chối toàn bộ giao dịch
  -> cộng Credits không hết hạn vào Credit Wallet duy nhất của Workspace
  -> không thay đổi Subscription expiry
  -> không mở khóa feature mới
```

## 3.6. Test Cases

### Test case cũ cần sửa

- Subscription/payment theo Profile.
- Quota theo Profile.
- Ownership guard theo `X-Profile-Id`.
- AI prompt quota.
- Content publish quota.
- Dashboard/profile summary.

### Test case mới bắt buộc

1. Register tạo Personal Workspace mặc định.
2. User không thuộc Workspace nhận `403`.
3. Member đọc dữ liệu Workspace theo role.
4. Owner invite member và chọn role.
5. Member accept invitation thành công.
6. Subscription checkout và activate cho Workspace.
7. Free Plan có 50 Credits và reset mỗi 7 ngày.
8. Paid Credits không tự reset theo chu kỳ Subscription.
9. Gia hạn Paid Plan cộng Credits mới vào số dư cũ.
10. Gia hạn sớm cộng thời gian từ ngày hết hạn hiện tại.
11. Subscription hết hạn vẫn giữ nguyên Credits.
12. Subscription hết hạn chặn feature yêu cầu active Subscription dù còn Credits.
13. Business Plus member dùng Shared Pool.
14. Business Pro Lifetime Assigned Limit không tự reset.
15. Business Pro Monthly Assigned Limit reset usage vào ngày 01 hàng tháng.
16. Assigned Quota chặn member khi hết quota dù Workspace còn Credits.
17. Business Pro Owner tăng Assigned Quota cho member.
18. Credit Pack dùng `PaymentType = CreditPack`, cộng đúng Credits, không đổi expiry/feature và Credits không hết hạn.
19. Business Workspace hết hạn chuyển Read-only Limited Mode.
20. Limited Mode giữ nguyên members, dữ liệu và Credits; member vẫn đăng nhập và xem team.
21. Limited Mode chặn create/publish/invite/role/quota và Business/Premium features.
22. Limited Mode chỉ cho Owner xem billing và gia hạn.
23. Gia hạn mở lại Workspace và cộng Credits mới.
24. Workspace Dashboard tổng hợp đúng Credits, Posts, Total AI Usage và Top Members.
25. AI thành công trừ đúng Credits.
26. AI thất bại không trừ Credits.
27. Regenerate/refine trừ Credits.
28. Hết Credits chặn AI.
29. Publish không trừ Credits.
30. Free Post Quota reset mỗi tuần.
31. Paid Post Quota reset theo chu kỳ Subscription.
32. Hết Post Quota chặn publish dù còn Credits.
33. Credit usage lưu User, Feature Used, Credits Consumed, Timestamp, Status; không lưu full prompt.
34. Dữ liệu Workspace A không truy cập được từ Workspace B.
35. Backfill dữ liệu cũ không mất Subscription/Brand/Content/Campaign/Credits.
36. Free Credits khởi tạo/reset đúng 50 Credits mỗi 7 ngày.
37. Paid Plan cấp đúng Credits khi mua/gia hạn: Personal Plus 500, Personal Pro 2.000, Business Plus 15.000, Business Pro 50.000.
38. Post Quota đúng theo plan: 20/tuần, 300/tháng, 1.000/tháng, 5.000/tháng và 20.000/tháng.
39. Feature bị khóa/mở đúng theo Feature Matrix đã xác nhận.
40. Owner, Manager, Content Creator và Viewer được phép/bị chặn đúng theo Permission Matrix.
41. Plan cao hơn kế thừa đầy đủ feature của plan thấp hơn.
42. Mỗi Workspace chỉ có một Credit Wallet; không thể tạo wallet thứ hai.
43. Personal Workspace không thể có Credit balance vượt 15.000.
44. Business Workspace không thể có Credit balance vượt 500.000.
45. Gia hạn và Credit Pack bị từ chối toàn bộ nếu làm vượt Maximum Credit Balance.
46. Workspace hết hạn dưới 90 ngày ở Limited Mode.
47. Workspace hết hạn từ 90 đến 180 ngày chuyển Archived.
48. Workspace hết hạn trên 180 ngày chỉ Admin có quyền Soft Delete.
49. Gia hạn trước khi bị xóa khôi phục Workspace theo lifecycle rule.
50. Feature Gate, Maximum Credit Balance và Quota Rule sử dụng đúng `WorkspaceTypeEnum`.
51. Giao dịch làm vượt Maximum Credit Balance bị từ chối toàn bộ và không thay đổi Payment/Credit balance ngoài trạng thái thất bại cần thiết.
52. Archived Owner được View, Export và Renew.
53. Archived Member chỉ được View.
54. Admin Delete là Soft Delete; dữ liệu vẫn còn trong database và Workspace không xuất hiện trong active query.
55. Workspace luôn có đúng một Owner sau create, migration, member update và ownership transfer.
56. Owner chỉ transfer ownership cho member có role Manager.
57. Ownership Transfer đổi Manager thành Owner và Owner cũ thành Manager trong cùng transaction.
58. Transfer thất bại rollback toàn bộ, Workspace vẫn có Owner ban đầu.
59. Owner không thể tự remove khi chưa transfer ownership.
60. Business Plus từ chối member thứ 11.
61. Business Pro từ chối member thứ 51.
62. Invite/accept invitation không làm vượt Member Limit.

---

# BƯỚC 4 - IMPLEMENTATION PLAN

Mỗi task dưới đây phải commit riêng. Không chuyển task tiếp theo nếu task hiện tại chưa build/test được.

## Task 1 - Ghi nhận contract và migration mapping đã xác nhận

### Mục tiêu

Đồng bộ các quyết định breaking change, role, plan mapping, Credits, Post Quota, Feature Matrix, Permission Matrix và dữ liệu cũ đã được Product Owner xác nhận.

### File sửa

- Tài liệu SRS/API/Database/Business Flow liên quan.

### Cách test

- Review tài liệu và bảo đảm 25 quyết định đã xác nhận được ghi đầy đủ.
- Kiểm tra đủ bảng Credits, Post Quota, Feature Matrix và Permission Matrix trong tài liệu.

### Commit đề xuất

`docs(workspace): approve workspace subscription and credit contracts`

---

## Task 2 - Thêm Workspace và WorkspaceMember foundation

### Mục tiêu

Thêm entity, `WorkspaceTypeEnum`, relation, repository và migration nền tảng; chưa chuyển ownership hiện tại.

### File sửa

- Workspace/WorkspaceMember model, enum, repository.
- `AISAMContext.cs`
- Migration mới và tests.

### Cách test

```powershell
dotnet build
dotnet test
dotnet ef database update
```

### Commit đề xuất

`feat(workspace): add workspace and membership foundation`

---

## Task 3 - Tạo Personal Workspace khi đăng ký

### Mục tiêu

Mỗi tài khoản mới có một Personal Workspace và Owner membership.

### File sửa

- Auth registration service liên quan.
- Workspace service/repository.
- Auth/workspace tests.

### Cách test

- Register user mới.
- Kiểm tra Workspace và Owner membership được tạo.
- Kiểm tra Personal Workspace có `WorkspaceType = Personal`.
- Chạy `dotnet build` và `dotnet test`.

### Commit đề xuất

`feat(auth): create personal workspace on registration`

---

## Task 4 - Thêm Active Workspace context và authorization guard

### Mục tiêu

Đọc `X-Workspace-Id`, kiểm tra membership và cung cấp Workspace context cho API.

### File sửa

- `ActiveWorkspaceMiddleware.cs`
- `WorkspaceContextHelper.cs`
- `Program.cs`
- Tests middleware/authorization.

### Cách test

- Thiếu header, workspace không tồn tại, không phải member và member hợp lệ.
- Chạy `dotnet build` và `dotnet test`.

### Commit đề xuất

`feat(workspace): add active workspace authorization context`

---

## Task 5 - Thêm Workspace Invitation, Role Management và Member Limit MVP

### Mục tiêu

Invite, accept invitation, list member, update role và enforce Business member limit.

### File sửa

- Invitation/member model, repository, service, controller, DTO và tests.

### Cách test

- Owner invite.
- User accept.
- Business Plus chặn member thứ 11.
- Business Pro chặn member thứ 51.
- Kiểm tra permission matrix của từng role sau khi được xác nhận.
- Unauthorized member bị chặn.
- Chạy API test bằng Swagger/Postman.

### Commit đề xuất

`feat(workspace): add member invitation and role management`

---

## Task 5.1 - Thêm Ownership Transfer

### Mục tiêu

Bảo đảm mỗi Workspace luôn có đúng một Owner và hỗ trợ transfer ownership từ Owner sang Manager.

### File sửa

- WorkspaceMember repository/service/controller/DTO.
- Workspace ownership authorization và tests.

### Cách test

- Chỉ Owner được transfer.
- Chỉ member có role Manager được chọn.
- Manager mới thành Owner; Owner cũ thành Manager.
- Hai thay đổi role thực hiện trong cùng transaction.
- Owner không thể tự remove trước khi transfer.
- Workspace không thể có zero hoặc multiple Owners.

### Commit đề xuất

`feat(workspace): add atomic ownership transfer`

---

## Task 6 - Chuyển Subscription và Payment sang Workspace

### Mục tiêu

Checkout, webhook, current subscription, renewal và history hoạt động theo Workspace.

### File sửa

- Subscription model/repository.
- Payment repository/service/controller/DTO.
- Migration và tests.

### Cách test

- PayOS checkout/webhook.
- Current subscription.
- Payment history.
- Gia hạn sớm cộng thời gian từ ngày hết hạn hiện tại.
- Gia hạn cộng Credits mới vào số dư cũ.
- Subscription hết hạn giữ Credits nhưng khóa feature yêu cầu active Subscription.
- Chạy `dotnet build`, `dotnet test` và API test.

### Commit đề xuất

`feat(payment): move subscriptions and payments to workspace`

---

## Task 7 - Thêm Credit Wallet và Credit Usage metadata

### Mục tiêu

Quản lý Credit Wallet, Free Credit reset, Paid Credit cộng dồn và metadata usage.

### File sửa

- Credit entities/repository/service/controller/DTO.
- Migration và tests.

### Cách test

- Khởi tạo Credits theo plan.
- Free Credits reset mỗi 7 ngày.
- Paid Credits không tự reset và không mất khi Subscription hết hạn.
- Paid renewal cộng Credits.
- Usage record có Feature Used nhưng không chứa full prompt.
- Chạy `dotnet build`, `dotnet test` và API test.

### Commit đề xuất

`feat(credits): add workspace credit wallet and usage records`

---

## Task 7.1 - Thêm Shared Pool và Lifetime Assigned Limit

### Mục tiêu

Business Plus dùng Shared Pool; Business Pro cho phép Lifetime Assigned Limit không tự reset.

### File sửa

- WorkspaceMember model/repository/service/controller/DTO.
- Credit service và tests.

### Cách test

- Shared Pool trừ Workspace Credits.
- Lifetime limit kiểm tra cả Workspace balance và member limit.
- Lifetime usage không tự reset.
- Member hết quota bị chặn dù Workspace còn Credits.
- Owner thay đổi member quota.

### Commit đề xuất

`feat(credits): add shared and lifetime member quota modes`

---

## Task 7.2 - Thêm Monthly Assigned Limit

### Mục tiêu

Business Pro member có giới hạn Credits theo tháng và usage tự reset theo kỳ tháng.

### File sửa

- WorkspaceMember model/repository/service/controller/DTO.
- Monthly reset processing và tests.

### Cách test

- Monthly usage tăng sau AI thành công.
- Hết monthly limit thì member bị chặn.
- Ngày 01 hàng tháng, usage reset về 0.
- Workspace Credit balance không bị reset.

### Commit đề xuất

`feat(credits): add monthly member quota mode`

---

## Task 7.3 - Thêm Credit Pack

### Mục tiêu

Cho phép mua thêm Credits mà không thay đổi Subscription.

### Điều kiện bắt đầu

Giá đề xuất đã được tạm chấp nhận. PayOS dùng `PaymentType` để phân biệt giao dịch `Subscription` và `CreditPack`.

### Cách test

- Mua Credit Pack cộng Credits vào Workspace.
- Payment được lưu với `PaymentType = CreditPack`.
- Không thay đổi Subscription expiry/feature.
- Credits từ Credit Pack không hết hạn.

### Commit đề xuất

`feat(credits): add workspace credit pack purchase`

---

## Task 7.4 - Thêm Business Workspace Read-only Limited Mode

### Mục tiêu

Khi Business Subscription hết hạn, giữ nguyên dữ liệu/team/Credits nhưng khóa thao tác ghi và Business/Premium features.

### File sửa

- Workspace/subscription service.
- Workspace authorization guard.
- Controllers/services có thao tác bị khóa.
- Tests.

### Cách test

- Member vẫn đăng nhập, xem được dữ liệu cũ và xem team.
- Create, publish, invite, role và quota update bị chặn.
- Chỉ Owner được xem billing và gia hạn.
- Gia hạn mở lại Workspace và giữ Credits cũ.

### Commit đề xuất

`feat(workspace): add expired business limited mode`

---

## Task 7.4.1 - Thêm Archived và Admin Soft Delete lifecycle

### Mục tiêu

Chuyển Workspace hết hạn sang Archived từ ngày 90 và cho phép Admin Soft Delete sau ngày 180.

### File sửa

- Workspace status/lifecycle service.
- Scheduled processing theo pattern hiện có.
- Admin workspace controller/service liên quan.
- Tests.

### Cách test

- Dưới 90 ngày vẫn Limited Mode.
- Từ 90 đến 180 ngày chuyển Archived.
- Archived Owner được View, Export, Renew; Member chỉ View.
- Trên 180 ngày chỉ Admin được Soft Delete.
- User/Owner không thể tự xóa bằng quyền Admin.
- Soft-deleted Workspace không xuất hiện trong active query nhưng dữ liệu vẫn còn.

### Commit đề xuất

`feat(workspace): add archive and admin soft delete lifecycle`

---

## Task 7.4.2 - Enforce một Credit Wallet và Maximum Balance

### Mục tiêu

Bảo đảm mỗi Workspace có đúng một Credit Wallet và không vượt maximum balance.

### File sửa

- CreditWallet model/configuration/repository/service.
- Migration và tests.

### Cách test

- Unique constraint chặn wallet thứ hai.
- Personal balance không vượt 15.000.
- Business balance không vượt 500.000.
- Gia hạn và Credit Pack làm vượt maximum balance bị từ chối toàn bộ.

### Commit đề xuất

`feat(credits): enforce workspace wallet and maximum balance`

---

## Task 7.5 - Áp dụng Plan Entitlement và Permission Matrix

### Mục tiêu

Áp dụng đúng Credits, Post Quota, Feature Matrix và Permission Matrix đã xác nhận.

### File sửa

- Plan definition/configuration hiện có.
- Feature authorization guard/service.
- Workspace role authorization guard/service.
- Tests.

### Cách test

- Mỗi plan nhận đúng Credits và Post Quota.
- Feature được mở/khóa đúng theo bảng đã xác nhận.
- Plan cao hơn kế thừa toàn bộ feature của plan thấp hơn.
- Owner, Manager, Content Creator và Viewer được phép/bị chặn đúng theo Permission Matrix.
- Không tự suy diễn quyền chưa được liệt kê.

### Commit đề xuất

`feat(subscription): enforce plan features and workspace permissions`

---

## Task 8 - Áp dụng Credits vào từng tác vụ AI

### Mục tiêu

Trừ đúng Credits sau khi AI thành công; AI thất bại không trừ.

### File sửa

- `AIService.cs`
- Credit service/repository.
- AI tests.

### Cách test

- Generate text/image/video/trend/recommendation theo từng task độc lập.
- Regenerate/refine.
- Không đủ Credits.
- Provider failure.

### Commit đề xuất

Tách từng tác vụ AI thành commit riêng, ví dụ:

- `feat(credits): charge credits for successful text generation`
- `feat(credits): charge credits for regenerate and refine`
- `feat(credits): charge credits for successful image generation`

---

## Task 9 - Tách Post Quota khỏi Credits

### Mục tiêu

Publish chỉ kiểm tra Post Quota và không thay đổi Credit Wallet. Free reset mỗi tuần; Paid reset theo chu kỳ Subscription.

### File sửa

- `QuotaService.cs`
- `ContentService.cs`
- Quota/content publish tests.

### Cách test

- Publish thành công không trừ Credits.
- Hết Post Quota bị chặn.
- Còn Post Quota nhưng hết Credits vẫn publish được.
- Free Post Quota reset mỗi tuần.
- Paid Post Quota reset theo chu kỳ Subscription.

### Commit đề xuất

`feat(publishing): enforce workspace post quota without credits`

---

## Task 10 - Migration ownership theo từng domain

### Mục tiêu

Chuyển ownership từ Profile sang Workspace theo từng module, không gom nhiều module vào một commit.

### Task con và commit đề xuất

- Brand: `refactor(brand): move ownership to workspace`
- Product: `refactor(product): enforce workspace ownership through brand`
- Content/Post: `refactor(content): move ownership to workspace`
- Campaign: `refactor(campaign): move ownership to workspace`
- Social integration/calendar/conversation/notification: mỗi module một commit riêng.

### Cách test

- CRUD theo Workspace A.
- Workspace B không truy cập được.
- Build/test/API test sau từng module.

---

## Task 11 - Backfill dữ liệu cũ và khóa schema mới

### Mục tiêu

Backfill dữ liệu Profile cũ, xác minh rồi mới đặt Workspace relation bắt buộc.

### File sửa

- Migration mới, snapshot và migration tests.

### Cách test

- Backup database test.
- Chạy migration trên database có dữ liệu cũ.
- Kiểm tra số lượng và relation trước/sau.
- Rollback migration trên database test.

### Commit đề xuất

`migration(workspace): backfill legacy profile-owned data`

---

## Task 12 - Cập nhật Dashboard và Frontend context

### Mục tiêu

Hiển thị và gửi Active Workspace, Credits, Post Quota và Workspace usage dashboard.

### File sửa

- Dashboard backend liên quan.
- FE workspace store/API/types/screens cần thiết.

### Cách test

- Switch Workspace.
- Dashboard đổi đúng dữ liệu.
- Dashboard hiển thị Credits Remaining, Posts Remaining, Total AI Usage và Top Members By Usage.
- Request gửi đúng `X-Workspace-Id`.
- Workspace isolation hoạt động.

### Commit đề xuất

Tách Backend và Frontend thành commit riêng.

---

## Task 13 - Cập nhật tài liệu và regression test cuối

### Mục tiêu

Đồng bộ SRS, Use Case, API, Database, setup guide và plan với code đã xác minh.

### File sửa

- `README.md`
- `requirement.md`
- `BACKEND_CODE_PLAN.md`
- `SETUP_GUIDE.md`
- API/Database/Use Case documents.

### Cách test

```powershell
dotnet build
dotnet test
dotnet ef database update
```

- Regression test Auth, Payment, AI, Publish, Workspace và ownership.

### Commit đề xuất

`docs(workspace): update workspace subscription and credit documentation`

---

# BƯỚC 5 - KẾT QUẢ XÁC NHẬN

## Các câu trả lời đã được lưu

1. **Đã xác nhận:** thay hoàn toàn `X-Profile-Id` bằng `X-Workspace-Id`.
2. **Đã xác nhận:** Profile chỉ còn thông tin cá nhân/doanh nghiệp.
3. **Đã xác nhận:** mỗi Profile cũ được backfill thành một Personal Workspace.
4. **Đã xác nhận:** Free -> Free, Plus -> Personal Plus, Premium -> Personal Pro, PlusTrial -> Personal Plus Trial.
5. **Đã xác nhận:** dùng Owner, Manager, Content Creator và Viewer.
6. **Đã xác nhận:** AI Chat không trừ Credits trong MVP.
7. **Đã xác nhận tạm thời:** dùng bảng Credit Pack đề xuất.
8. **Đã xác nhận:** PayOS dùng `PaymentType = Subscription | CreditPack`.
9. **Đã xác nhận:** Credits cấp khi mua/gia hạn plan là Free 50/7 ngày, Personal Plus 500, Personal Pro 2.000, Business Plus 15.000, Business Pro 50.000.
10. **Đã xác nhận:** Post Quota theo plan là Free 20/tuần, Personal Plus 300/tháng, Personal Pro 1.000/tháng, Business Plus 5.000/tháng, Business Pro 20.000/tháng.
11. **Đã xác nhận:** Feature Matrix và Permission Matrix theo các bảng đã lưu trong tài liệu.
12. **Đã xác nhận:** Monthly Assigned Limit reset ngày 01 hàng tháng.
13. **Đã xác nhận:** Limited Mode chỉ cho Owner xem Billing và Gia hạn; member vẫn đăng nhập, xem dữ liệu cũ và xem team.
14. **Đã xác nhận:** migration nhiều bước để tránh mất dữ liệu.
15. **Đã xác nhận:** plan kế thừa toàn bộ feature của plan thấp hơn.
16. **Đã xác nhận:** mỗi Workspace có đúng một Credit Wallet.
17. **Đã xác nhận:** expired lifecycle là dưới 90 ngày Limited Mode, 90-180 ngày Archived, trên 180 ngày Admin có quyền Soft Delete.
18. **Đã xác nhận:** Maximum Credit Balance là Personal 15.000 và Business 500.000 Credits.
19. **Đã xác nhận:** giao dịch làm vượt Maximum Credit Balance bị từ chối toàn bộ.
20. **Đã xác nhận:** Archived Owner được View, Export, Renew; Archived Member chỉ được View.
21. **Đã xác nhận:** Admin Delete Workspace là Soft Delete.
22. **Đã xác nhận:** mỗi Workspace luôn có đúng một Owner.
23. **Đã xác nhận:** Owner có thể transfer ownership cho Manager; Manager thành Owner và Owner cũ thành Manager.
24. **Đã xác nhận:** Owner không thể tự remove nếu chưa Ownership Transfer.
25. **Đã xác nhận:** Business Plus tối đa 10 members; Business Pro tối đa 50 members.

## Workspace Type chính thức

```csharp
public enum WorkspaceTypeEnum
{
    Personal = 1,
    Business = 2
}
```

## Điểm chưa được tự suy diễn

- Permission Matrix chỉ cấp/chặn đúng các quyền đã được liệt kê; quyền chưa được liệt kê phải được xử lý bảo thủ và làm rõ khi triển khai.
- Quy tắc retention hoặc hard delete cuối cùng cho dữ liệu đã Soft Delete chưa nằm trong Change Request hiện tại.

## Kết luận

Change Request này có blast radius lớn vì thay đổi ownership boundary của hệ thống. Kế hoạch trên giữ phạm vi đúng yêu cầu, triển khai theo từng module nhỏ, có migration an toàn và không tự ý thay đổi provider, auth core, UI/UX hoặc chức năng nâng cao.

**25 quyết định, WorkspaceTypeEnum, Credits, Post Quota, Member Limit, Feature Matrix và Permission Matrix đã được lưu. Implementation đang ở Phase 9; Task 9.6 Active Workspace context đã hoàn thành, build thành công và pass 154/154 tests. Task tiếp theo là 9.7 Invitation, Role Management và Member Limit.**
