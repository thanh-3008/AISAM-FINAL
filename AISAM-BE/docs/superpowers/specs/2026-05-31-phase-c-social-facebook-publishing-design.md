# Spec Phase C: Social Integration và Facebook Page Publishing

Lần review gần nhất: 2026-05-31

## 1. Mục tiêu

Hoàn thiện Phase 6 của backend AISAM với flow cốt lõi:

- Kết nối tài khoản Facebook bằng OAuth.
- Lấy danh sách Facebook Page mà tài khoản có quyền quản lý.
- Link Facebook Page vào brand thuộc active profile.
- Publish content lên Facebook Page.
- Lưu lịch sử publish dưới dạng `Post`.
- Cung cấp Posts API chỉ đọc theo active profile.

Phase C tái sử dụng source cũ tại `docs/code-references/PRN232_Backend` nhưng không copy nguyên khối. Code migrate phải bám active-profile context đã hoàn thành ở Phase A/B và không kéo Facebook Ads, scheduling, notification hoặc team permission của các phase sau.

## 2. Quyết định đã chốt

- Triển khai Facebook Page MVP thật: OAuth, list Page, link Page, publish và lưu `Post`.
- Test tự động không phụ thuộc credentials thật; dùng fake HTTP handler hoặc fake provider.
- Chưa có Facebook App credentials và Page test. OAuth/publish smoke test thật được ghi nhận là blocker môi trường.
- Facebook Ads và link ad account không thuộc Phase C.
- Có migrate `GoogleProvider` để giữ provider contract, nhưng không expose Google OAuth qua controller và không hỗ trợ publish YouTube.
- Sửa mapping dư giữa `Post` và `SocialIntegration`, tạo migration cleanup riêng.
- Unlink account hoặc Page integration dùng soft delete, giữ lịch sử `Post`.
- OAuth `state` được lưu bằng `IMemoryCache`, có expiry, khớp active profile và chỉ consume một lần.
- User token và Page token được mã hóa bằng ASP.NET Core Data Protection trước khi lưu database.
- DTO, response, log và lỗi API không được lộ token.
- Posts API Phase C chỉ hỗ trợ list và detail theo active profile.

## 3. Hiện trạng codebase

Active codebase đã có entity và `DbSet`:

- `SocialAccount`
- `SocialIntegration`
- `Post`
- `Content`
- `Brand`
- `Profile`

Active codebase chưa có:

- Provider contract và Facebook/Google provider.
- Social account/integration repository.
- Social service.
- OAuth state store.
- Social token protector.
- Social controllers.
- Post repository và Posts API chỉ đọc.
- Publish endpoint trong `ContentController`.
- Facebook config và environment override.

Schema hiện tại gần đủ nhưng cần cleanup:

- `Post.IntegrationId` và cột `integration_id` là quan hệ đúng.
- Migration snapshot hiện có thêm shadow FK `Post.SocialIntegrationId` do quan hệ `Post` - `SocialIntegration` bị map hai lần.
- Phase C phải sửa mapping và tạo migration cleanup để xóa FK/index/cột dư nếu database thực tế có tồn tại.

## 4. Source cũ dùng làm baseline

### Controller

- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/SocialAuthController.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/SocialAccountController.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/SocialIntegrationController.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/ContentController.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/PostsController.cs`

### Service

- `docs/code-references/PRN232_Backend/AISAM.Services/IServices/IProviderService.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/IServices/ISocialService.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/IServices/IPostService.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/Service/FacebookProvider.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/Service/GoogleProvider.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/Service/SocialService.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/Service/ContentService.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/Service/PostService.cs`

### Repository

- `docs/code-references/PRN232_Backend/AISAM.Repositories/IRepositories/ISocialAccountRepository.cs`
- `docs/code-references/PRN232_Backend/AISAM.Repositories/IRepositories/ISocialIntegrationRepository.cs`
- `docs/code-references/PRN232_Backend/AISAM.Repositories/IRepositories/IPostRepository.cs`
- `docs/code-references/PRN232_Backend/AISAM.Repositories/Repository/SocialAccountRepository.cs`
- `docs/code-references/PRN232_Backend/AISAM.Repositories/Repository/SocialIntegrationRepository.cs`
- `docs/code-references/PRN232_Backend/AISAM.Repositories/Repository/PostRepository.cs`

### DTO, model và config

- `docs/code-references/PRN232_Backend/AISAM.Common/Models/SocialDtos.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Models/PostDtos.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Models/FacebookSettings.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Models/FacebookModels.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Models/GoogleModels.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Dtos/Request/SocialCallbackRequest.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Dtos/Response/SocialIntegrationDto.cs`

## 5. Vì sao không copy nguyên source cũ

`SocialService` cũ còn chứa:

- Facebook ad-account listing và linking.
- Ownership check chưa đầy đủ khi link Page vào brand.
- Tham số có chỗ đặt tên `userId` nhưng thực tế so sánh với `ProfileId`.
- Flow OAuth trả `state` nhưng chưa lưu và verify one-time ở server.

`PostService` cũ còn phụ thuộc:

- Notification.
- Team member permission.
- Content calendar và scheduling.
- Remote post deletion chưa hoàn chỉnh.

`FacebookProvider` cũ là baseline cho Graph API flow, nhưng phải bổ sung:

- Kiểm tra config trước khi tạo OAuth URL hoặc gọi Graph API.
- Không log URL hoặc payload chứa access token.
- Mã hóa token ở persistence boundary.
- Error response ổn định, không trả raw token hoặc secret.

Copy nguyên source cũ sẽ kéo dependency thuộc Phase D hoặc phase Ads vào Phase C, đồng thời giữ lại lỗi ownership và lộ token qua DTO/log.

## 6. Kiến trúc Phase C

### C1. Active profile context

Mở rộng `ActiveProfileMiddleware` để bảo vệ:

- `/api/social-auth`
- `/api/social`
- `/api/posts`

Tất cả endpoint Phase C dùng JWT và header `X-Profile-Id`. Controller lấy profile đã validate qua `ProfileContextHelper`, không nhận `ProfileId` tin cậy từ request body.

### C2. Provider contract

Thêm `IProviderService` với các trách nhiệm:

- Tạo OAuth authorization URL.
- Đổi authorization code lấy social account token.
- Lấy available targets của account.
- Lấy target token cho Page được chọn.
- Publish post lên target.
- Validate hoặc refresh token khi provider hỗ trợ.

Đăng ký:

- `FacebookProvider`: provider active cho flow public Phase C.
- `GoogleProvider`: provider nội bộ để giữ contract và chuẩn bị cho phase sau.

Giới hạn:

- Social controller chỉ chấp nhận provider `facebook`.
- Không expose OAuth Google.
- Không hỗ trợ publish YouTube.
- Không expose Facebook Ads.

### C3. OAuth state store

Thêm `IOAuthStateStore` và implementation dùng `IMemoryCache`.

Khi tạo auth URL:

1. Sinh state ngẫu nhiên an toàn.
2. Lưu state cùng active profile ID, provider và expiry 10 phút.
3. Trả auth URL chứa state.

Khi callback:

1. Yêu cầu state có tồn tại.
2. Yêu cầu provider và active profile khớp.
3. Consume state đúng một lần.
4. Reject state sai, hết hạn hoặc đã dùng.
5. Chỉ sau khi verify state mới đổi code lấy token.

Giới hạn đã chấp nhận:

- State mất hiệu lực khi API restart.
- Chưa hỗ trợ nhiều API instance.
- Khi cần scale-out, thay implementation bằng Redis hoặc persistence store mà không đổi controller contract.

### C4. Token protection

Thêm `ISocialTokenProtector` dùng ASP.NET Core Data Protection.

Quy tắc:

- Mã hóa user access token trước khi lưu `SocialAccount.UserAccessToken`.
- Mã hóa Page access token trước khi lưu `SocialIntegration.AccessToken`.
- Giải mã token ngay trước khi gọi provider.
- Token refresh thành công phải mã hóa token mới trước khi update database.
- DTO social không có trường access token.
- Log không ghi URL query hoặc response body chứa token.

Data Protection key ring hiện đã được cấu hình trong `AISAM.API/.keys`.

### C5. Social account và Page integration

Social service quản lý:

- Link hoặc re-auth Facebook account.
- List account active của active profile.
- List Page có thể link từ Facebook.
- Link Page được chọn vào brand.
- List Page đã link.
- List integration theo brand.
- Soft delete account.
- Soft delete Page integration.

Ownership:

- Account phải thuộc active profile.
- Brand phải thuộc active profile.
- Integration phải thuộc active profile và brand hợp lệ.
- Page được link phải nằm trong danh sách Page trả về cho chính account đó.
- Account hoặc integration đã soft delete không được dùng để publish.
- Query active mặc định lọc `IsDeleted == false` và `IsActive == true`.

Unlink:

- Unlink account soft-delete account và các integration active liên quan.
- Unlink Page integration chỉ soft-delete integration được chọn.
- Không xóa `Post` lịch sử.

### C6. Publish Facebook Page

Thêm endpoint publish vào `ContentController`:

```text
POST /api/content/{contentId}/publish/{integrationId}
```

Flow:

1. Load content thuộc active profile và chưa bị xóa.
2. Reject nếu content đã `Published`.
3. Load integration active thuộc cùng active profile.
4. Kiểm tra integration dùng brand của content.
5. Load account active của integration.
6. Build `PostDto`.
7. Giải mã token và gọi `FacebookProvider.PublishAsync`.
8. Nếu provider thành công, lưu `Post`.
9. Update content thành `Published`.
10. Nếu provider thất bại, giữ nguyên status content và không tạo `Post` thành công giả.

Routing theo content:

- `TextOnly`: gọi Page `/feed`.
- `ImageText` một ảnh: gọi Page `/photos`.
- `ImageText` nhiều ảnh: upload unpublished photos rồi attach vào `/feed`.
- `VideoText`: gọi Page `/videos`.

Token refresh:

- Nếu Page token cũ thất bại, provider thử lấy lại Page token từ user token.
- Nếu retry thành công, lưu Page token mới đã mã hóa.
- Nếu retry thất bại, trả lỗi publish rõ ràng.

### C7. Posts API chỉ đọc

Thêm:

```text
GET /api/posts
GET /api/posts/{postId}
```

Behavior:

- Chỉ trả post thuộc active profile.
- List bắt buộc hỗ trợ pagination và filter tùy chọn theo `brandId` hoặc `status`.
- Detail chỉ trả post thuộc active profile.
- Không gọi Facebook để enrich page name trong list/detail; dùng dữ liệu persistence ổn định.
- Không expose delete post trong Phase C.

## 7. API contract

### OAuth và social account

```text
GET    /api/social-auth/facebook
POST   /api/social-auth/facebook/callback
GET    /api/social/accounts/me
GET    /api/social/accounts/{accountId}/available-targets
GET    /api/social/accounts/{accountId}/linked-targets
POST   /api/social/accounts/{accountId}/link-targets
DELETE /api/social/accounts/{accountId}
```

### Social integration

```text
DELETE /api/social/integrations/{integrationId}
GET    /api/social/integrations/brand/{brandId}
```

### Publish và post history

```text
POST /api/content/{contentId}/publish/{integrationId}
GET  /api/posts
GET  /api/posts/{postId}
```

Các endpoint Phase C:

- Có `[Authorize]`.
- Yêu cầu `X-Profile-Id`.
- Không tin `ProfileId` hoặc token từ body.
- Không trả social token trong response.

## 8. Config

Thêm vào `AISAM.API/.env.example`:

```text
# Optional for API startup, required for Facebook OAuth and Page publishing
FACEBOOK_APP_ID=
FACEBOOK_APP_SECRET=
FACEBOOK_REDIRECT_URI=
FACEBOOK_GRAPH_API_VERSION=
FACEBOOK_BASE_URL=https://graph.facebook.com
FACEBOOK_OAUTH_URL=https://www.facebook.com
```

Thêm environment override tương ứng trong `AISAM.API/Program.cs`.

Đăng ký:

- `FacebookSettings`.
- `IMemoryCache`.
- `IOAuthStateStore`.
- `ISocialTokenProtector`.
- `HttpClient` cho Facebook/Google provider.
- Social/post repositories và services.

API host vẫn startup nếu thiếu Facebook config. Chỉ endpoint cần Facebook mới trả config error.

## 9. Error handling

| Trường hợp | HTTP/behavior |
| --- | --- |
| JWT thiếu hoặc không hợp lệ | `401` |
| `X-Profile-Id` thiếu hoặc sai format | `401` |
| Profile không thuộc JWT user | `403` |
| Provider public khác `facebook` | `400` |
| Facebook config thiếu | `503`, response rõ ràng và nhất quán |
| OAuth state sai, hết hạn hoặc đã dùng | `400` |
| Account, integration, brand, content hoặc post không thuộc active profile | `404` hoặc `403` phù hợp |
| Page không thuộc account đang link | `400` |
| Content đã publish | `400` |
| Facebook lỗi network hoặc permission | Publish fail, content giữ nguyên status |
| Token cũ và refresh Page token đều fail | Publish fail, yêu cầu re-auth phù hợp |

Không trả raw Facebook response nếu có nguy cơ chứa token. Chi tiết kỹ thuật chỉ ghi log đã loại secret.

## 10. Database impact

Entity hiện tại đủ cột cho flow social/publish:

- `social_accounts`
- `social_integrations`
- `posts`

Phase C tạo migration cleanup riêng:

- Sửa mapping `SocialIntegration.Posts` dùng cùng quan hệ với `Post.Integration`.
- Xóa shadow FK, index và cột dư `Post.SocialIntegrationId` nếu tồn tại.
- Giữ `posts.integration_id` làm FK duy nhất tới `social_integrations.id`.

Không tạo bảng OAuth state vì MVP dùng `IMemoryCache`.

## 11. Kiểm thử

### Unit test

- OAuth state: create, consume một lần, expiry, sai profile, sai provider.
- Token protector: protect/unprotect và ciphertext khác plaintext.
- DTO social không expose token.
- Facebook provider bằng fake HTTP handler:
  - auth URL;
  - callback token exchange;
  - list Page;
  - publish text;
  - publish một ảnh;
  - publish nhiều ảnh;
  - publish video;
  - refresh Page token và retry;
  - lỗi config;
  - lỗi Graph API.

### Service/controller test

- Active-profile middleware bảo vệ social/posts route.
- Link account và re-auth.
- Callback reject state sai hoặc dùng lại.
- List Page và link Page kiểm tra ownership account/brand.
- Soft delete account/integration giữ `Post`.
- Publish success tạo `Post` và set content `Published`.
- Publish fail không tạo `Post` giả và không đổi content status.
- Posts list/detail chỉ trả dữ liệu thuộc active profile.
- Google provider không được expose qua social controller.

### Verification

```text
dotnet build AISAM.sln
dotnet test AISAM.sln
dotnet ef migrations list --project AISAM.Repositories --startup-project AISAM.API
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
dotnet run --project AISAM.API
```

Swagger smoke test khi chưa có credentials:

- Swagger tải được.
- Endpoint social/posts xuất hiện.
- Route protected trả `401` nếu thiếu JWT.
- Facebook auth endpoint trả config error rõ ràng khi thiếu config.

Smoke test thật đang bị chặn do chưa có:

- Facebook App ID.
- Facebook App Secret.
- Redirect URI đã đăng ký.
- Facebook Page test và permission cần thiết.

## 12. Ngoài phạm vi

- Facebook Ads, ad-account listing và link ad account.
- Campaign, ad set, ad creative và Marketing API.
- OAuth Google public.
- Publish YouTube.
- Scheduling và background publish.
- Notification.
- Team/shared-profile permission.
- Subscription quota.
- Remote delete post trên Facebook.
- Redis hoặc database-backed OAuth state.

## 13. Definition of Done

- Mapping `Post` - `SocialIntegration` chỉ còn một FK đúng qua `integration_id`.
- Migration cleanup được tạo và verify.
- Facebook OAuth URL, callback, list Page và link Page có API.
- OAuth state được verify one-time với expiry và active profile.
- Social token được mã hóa trong persistence và không lộ qua DTO/log.
- Publish text, một ảnh, nhiều ảnh và video được test bằng fake HTTP handler.
- Publish success lưu `Post` và đổi content sang `Published`.
- Publish fail giữ nguyên content status và không tạo dữ liệu thành công giả.
- Unlink dùng soft delete và giữ lịch sử post.
- Posts API chỉ đọc theo active profile hoạt động.
- Build pass.
- Test pass.
- Swagger smoke test không credentials pass.
- Blocker smoke test thật được ghi rõ nếu credentials vẫn chưa có.
