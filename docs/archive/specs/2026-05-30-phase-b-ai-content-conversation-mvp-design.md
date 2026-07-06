# Spec Phase B: AI, Content, Conversation MVP

Lần review gần nhất: 2026-05-30

## 1. Mục tiêu

Hoàn thiện Phase 5 của backend AISAM với flow cốt lõi:

- Quản lý nội dung nội bộ theo profile và brand.
- Sinh và cải thiện text content bằng Gemini.
- Duyệt một AI generation để copy text vào content draft.
- Chat text với AI và lưu conversation history.

Phase B tái sử dụng source cũ tại `docs/code-references/PRN232_Backend` nhưng chỉ migrate phần cần cho MVP. Không copy nguyên service cũ nếu service kéo dependency ngoài phạm vi.

## 2. Quyết định đã chốt

- AI MVP là text-only.
- Không triển khai Vertex Imagen hoặc Supabase storage trong Phase B.
- Content MVP gồm CRUD, list/filter, detail, clone, soft delete và restore.
- Không triển khai publish hoặc approval trong Phase B.
- Conversation dùng active profile context từ header `X-Profile-Id`.
- Header `X-Profile-Id` bắt buộc cho toàn bộ Content, AI và Conversation API.
- Active profile phải tồn tại, chưa bị xóa và thuộc JWT user.
- Team/shared profile chưa được hỗ trợ trong Phase B.

## 3. Hiện trạng codebase

Active codebase đã có entity và `DbSet`:

- `Content`
- `AiGeneration`
- `Conversation`
- `ChatMessage`

Active codebase chưa có:

- Content controller/service/repository.
- AI controller/service/repository/config.
- Conversation controller/service/repository.
- Active-profile middleware.
- DTO Phase B.
- DI registrations Phase B.

Schema hiện tại dự kiến đã đủ cho Phase B. Không tạo migration nếu đối chiếu schema cuối cùng không phát hiện thiếu cột hoặc quan hệ bắt buộc.

## 4. Source cũ dùng làm baseline

### Controller

- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/ContentController.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/GeminiController.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/ConversationController.cs`

### Helper

- `docs/code-references/PRN232_Backend/AISAM.API/Utils/UserClaimsHelper.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Utils/ProfileContextHelper.cs`

### Service

- `docs/code-references/PRN232_Backend/AISAM.Services/Service/ContentService.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/Service/AIService.cs`
- `docs/code-references/PRN232_Backend/AISAM.Services/Service/ConversationService.cs`

### Repository

- `docs/code-references/PRN232_Backend/AISAM.Repositories/Repository/ContentRepository.cs`
- `docs/code-references/PRN232_Backend/AISAM.Repositories/Repository/AiGenerationRepository.cs`
- `docs/code-references/PRN232_Backend/AISAM.Repositories/Repository/ConversationRepository.cs`

### DTO và model

- `docs/code-references/PRN232_Backend/AISAM.Common/Dtos/Request/CreateContentRequest.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Dtos/Request/UpdateContentRequest.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Dtos/Response/ContentResponseDto.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Dtos/Response/ConversationResponseDto.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Dtos/Response/ConversationDetailDto.cs`
- `docs/code-references/PRN232_Backend/AISAM.Common/Models/GeminiModels.cs`

## 5. Vì sao không copy nguyên source cũ

`ContentService` cũ phụ thuộc:

- Social integration.
- Post publishing.
- Approval.
- Team member permissions.
- Subscription/quota placeholder.

`AIService` cũ phụ thuộc:

- Social integration.
- Notification.
- Team member permissions.
- Provider services.
- Vertex Imagen.
- Supabase storage.

Các dependency này thuộc Phase C, Phase E hoặc Phase H. Copy nguyên source cũ sẽ làm Phase B kéo thêm module ngoài phạm vi, trái với guardrails làm từng bước nhỏ và build/test ngay.

## 6. Kiến trúc Phase B

### B1. Active profile context

Thêm helper và middleware:

- `UserClaimsHelper`: đọc JWT user ID từ `ClaimTypes.NameIdentifier` hoặc `sub`.
- `ProfileContextHelper`: đọc `X-Profile-Id` và lấy active profile đã resolve từ `HttpContext.Items`.
- `ActiveProfileMiddleware`: validate active profile cho endpoint Phase B.

Middleware thực hiện:

1. Bỏ qua request không thuộc `/api/content`, `/api/ai`, `/api/conversations`.
2. Yêu cầu request đã authenticate.
3. Parse `X-Profile-Id`.
4. Query profile.
5. Reject nếu profile không tồn tại, đã bị xóa hoặc không thuộc JWT user.
6. Lưu profile ID đã validate trong `HttpContext.Items`.

Response:

- Thiếu hoặc sai `X-Profile-Id`: HTTP `401`.
- Profile không thuộc JWT user: HTTP `403`.

### B2. Content MVP

API:

- `POST /api/content`
- `GET /api/content`
- `GET /api/content/{contentId}`
- `PUT /api/content/{contentId}`
- `POST /api/content/{contentId}/clone`
- `DELETE /api/content/{contentId}`
- `POST /api/content/{contentId}/restore`

Behavior:

- Mọi request dùng active profile từ middleware.
- Khi create, service kiểm tra Brand thuộc active profile.
- Nếu có Product, service kiểm tra Product thuộc Brand.
- `ProfileId` của content luôn lấy từ active profile, không tin giá trị body.
- Content mới có trạng thái `Draft`.
- Clone tạo content mới với trạng thái `Draft`.
- Restore reset status về `Draft`.
- Detail/update/delete/restore/clone chỉ tác động lên content thuộc active profile.
- List chỉ trả content thuộc active profile; hỗ trợ filter theo brand, search term, ad type, deleted state và status.

Ngoài phạm vi:

- Publish immediately.
- Submit approval.
- Social publishing.
- Team permission.
- Subscription quota enforcement.

### B3. AI text MVP

API:

- `POST /api/ai/generate-draft`
- `POST /api/ai/improve/{contentId}`
- `POST /api/ai/approve/{aiGenerationId}`
- `GET /api/ai/generations/{contentId}`
- `POST /api/ai/chat`

Behavior:

- Tất cả endpoint có `[Authorize]` và dùng active profile từ middleware.
- Không tin `UserId` hoặc `ProfileId` trong request body.
- Generate draft kiểm tra Brand thuộc active profile.
- Nếu có Product, kiểm tra Product thuộc Brand.
- Generate draft tạo Content trạng thái `Draft`, tạo `AiGeneration` trạng thái `Pending`, rồi gọi Gemini text API.
- Improve tạo generation mới cho content thuộc active profile.
- Approve generation copy `GeneratedText` vào Content nhưng giữ Content ở trạng thái `Draft`.
- List generations chỉ trả generation của content thuộc active profile.

Gemini error behavior:

- Thiếu API key không làm API host crash.
- Lỗi key, quota hoặc network chuyển `AiGeneration.Status` thành `Failed`.
- Lưu `ErrorMessage`.
- Trả response rõ ràng cho client.
- Không ghi API key vào log.

### B4. Conversation MVP

API:

- `GET /api/conversations`
- `GET /api/conversations/{id}`
- `DELETE /api/conversations/{id}`

Conversation được tạo hoặc dùng lại trong `POST /api/ai/chat`.

Behavior:

- Conversation luôn thuộc active profile.
- Chat nhận Brand/Product context tùy chọn.
- Nếu có Brand, Brand phải thuộc active profile.
- Nếu có Product, Product phải thuộc Brand.
- Lưu message của user trước khi gọi Gemini.
- Khi Gemini thành công, lưu message AI.
- Nếu Gemini fail, lưu lỗi phù hợp và trả response rõ ràng.
- List/detail/delete chỉ truy cập conversation thuộc active profile.

## 7. Config

Thêm vào `AISAM.API/.env.example`:

```text
# Optional for API startup, required for AI text endpoints
GEMINI_API_KEY=
GEMINI_MODEL=gemini-2.5-flash
GEMINI_MAX_TOKENS=2048
GEMINI_TEMPERATURE=0.7
```

Thêm environment override tương ứng trong `AISAM.API/Program.cs`.

Đăng ký:

- `GeminiSettings`.
- `HttpClient` cho AI service.
- Repository/service Phase B.
- Active-profile middleware sau authentication và trước authorization/controller execution.

## 8. Error handling

| Trường hợp | HTTP/behavior |
| --- | --- |
| JWT thiếu hoặc không hợp lệ | `401` |
| `X-Profile-Id` thiếu hoặc sai format | `401` |
| Profile không thuộc JWT user | `403` |
| Brand/Product/Content/Conversation không thuộc active profile | `404` |
| Product không thuộc Brand | `400` |
| Gemini thiếu config hoặc lỗi external API | Generation `Failed`, response có lỗi rõ ràng |
| Schema/config startup không liên quan AI | Không được làm API host crash |

Không copy message mojibake từ source cũ. Message mới phải readable và nhất quán.

## 9. Database impact

Kiểm tra lại migration hiện có cho:

- `contents`
- `ai_generations`
- `conversations`
- `chat_messages`

Mặc định:

- Không tạo migration mới.
- Không sửa entity nếu schema hiện tại đã đủ.

Nếu phát hiện thiếu schema bắt buộc:

- Tạo một migration nhỏ riêng cho Phase B.
- Ghi rõ bảng/cột ảnh hưởng.
- Có lệnh rollback migration.

## 10. Test requirements

### Middleware

- Reject khi thiếu `X-Profile-Id`.
- Reject khi header sai format.
- Reject khi profile thuộc user khác.
- Cho phép profile thuộc JWT user.

### Content

- Create content dùng active profile và Brand hợp lệ.
- Reject Brand thuộc profile khác.
- Reject Product không thuộc Brand.
- List/detail/update chỉ thấy content thuộc active profile.
- Clone tạo content Draft mới.
- Soft delete và restore hoạt động; restore reset status Draft.

### AI

- API host vẫn chạy khi thiếu Gemini key.
- Generate/improve với Gemini thiếu config tạo generation Failed.
- Fake HTTP Gemini response tạo generation Completed.
- Approve generation copy text vào Content và giữ status Draft.
- Không truy cập generation của content thuộc profile khác.

### Conversation

- Chat lưu user message và AI message khi Gemini success.
- List/detail/delete chỉ truy cập conversation thuộc active profile.
- Không truy cập chéo profile.

### Verification

```text
dotnet build AISAM.sln
dotnet test AISAM.sln
```

Swagger/API smoke:

- Swagger mở được.
- Health vẫn trả `200`.
- Swagger có Content/AI/Conversation paths.
- Content CRUD smoke với JWT và `X-Profile-Id`.
- AI missing-config smoke trả lỗi graceful.
- AI success smoke chạy khi có Gemini API key hợp lệ.

## 11. Rollback

Nếu Phase B cần rollback:

1. Gỡ DI registrations Phase B trong `Program.cs`.
2. Gỡ active-profile middleware registration.
3. Gỡ controller Phase B.
4. Gỡ service/repository/interface/DTO mới.
5. Revert migration riêng Phase B nếu có.

Vì Phase B không sửa module Auth/Profile/Brand/Product hiện tại, rollback không được ảnh hưởng API Phase A.

## 12. Definition of Done

- Content CRUD/list/detail/clone/delete/restore hoạt động với ownership đúng.
- AI text generate/improve/approve/list generations hoạt động hoặc fail graceful khi thiếu Gemini config.
- AI chat text lưu conversation history.
- Content/AI/Conversation endpoints yêu cầu JWT và `X-Profile-Id`.
- Không kéo Vertex Imagen, Supabase, Social, Approval, Team, Notification hoặc quota vào Phase B.
- Không tạo migration nếu schema hiện tại đã đủ.
- `dotnet build AISAM.sln` pass.
- `dotnet test AISAM.sln` pass.
- Swagger/API smoke test pass.
