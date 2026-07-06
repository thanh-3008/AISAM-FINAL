# AISAM Backend Codebase

Last reviewed: 2026-06-01

This document describes the current backend codebase in this repository. It is based on the active source files at the repository root, not the older reference snapshot under `docs/code-references/PRN232_Backend`.

## 1. Executive Summary

AISAM-BE is a .NET 8 ASP.NET Core backend organized as a layered solution:

- `AISAM.API`: HTTP API, dependency injection, authentication, Swagger, filters and middleware.
- `AISAM.Services`: business logic for authentication, email, profiles, brands, products, content, AI, conversation, social publishing and posts history.
- `AISAM.Repositories`: Entity Framework Core `DbContext`, migrations and repository implementations.
- `AISAM.Data`: entity models and enum definitions.
- `AISAM.Common`: shared DTOs, response wrappers, settings models and common request/response shapes.
- `tests/AISAM.IntegrationTests`: xUnit test project with repository, service, controller and middleware coverage for the active MVP surface.

The active API surface is currently an MVP subset, but it is broader than the original foundation:

- Authentication and account/session management.
- Profile CRUD/search/soft delete restore.
- Brand CRUD/pagination/search/soft delete restore.
- Product CRUD/pagination/search/soft delete restore.
- Content CRUD, clone, restore and publish.
- Gemini text generation, improve, approve and chat.
- Conversation history.
- Facebook social auth/account/integration flow.
- Posts history.
- Notification read APIs.
- Content scheduling CRUD and upcoming schedules.
- Dashboard summary.
- Scheduled posting worker and Development-only scheduler trigger.
- Health endpoint.

The data model is still broader than the exposed API. `AisamContext` already contains DbSets and relationships for teams, subscriptions, approvals, ads, reports, notifications, payments and scheduling. Those remaining modules are still model-ready rather than fully active in the root solution.

## 2. Solution Structure

```text
AISAM.sln
+-- AISAM.API/
+-- AISAM.Common/
+-- AISAM.Data/
+-- AISAM.Repositories/
+-- AISAM.Services/
+-- tests/AISAM.IntegrationTests/
+-- docs/
```

### Project Dependencies

`AISAM.API` references:

- `AISAM.Services`
- `AISAM.Repositories`
- `AISAM.Common`

`AISAM.Services` references:

- `AISAM.Repositories`
- `AISAM.Data`
- `AISAM.Common`

`AISAM.Repositories` references:

- `AISAM.Data`
- `AISAM.Common`

`AISAM.Common` references:

- `AISAM.Data`

`tests/AISAM.IntegrationTests` references:

- `AISAM.API`

This creates a conventional ASP.NET architecture: API -> services -> repositories -> data.

## 3. Runtime and Configuration

### Framework

All active projects target `net8.0` with nullable reference types and implicit usings enabled.

### HTTP Hosting

`AISAM.API/Properties/launchSettings.json` defines:

- HTTP profile: `http://localhost:5027`
- HTTPS profile: `https://localhost:7192;http://localhost:5027`
- Browser launch URL: `swagger`

Swagger is always enabled in `Program.cs` through:

- `app.UseSwagger()`
- `app.UseSwaggerUI()`

### Configuration Sources

`Program.cs` loads configuration from:

- `appsettings.json`
- `appsettings.Development.json`
- optional `AISAM.API/.env`
- selected environment variables

Relevant environment variables:

- `CONNECTION_STRING`
- `FRONTEND_BASE_URL`
- `JWT_SECRET_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `FACEBOOK_APP_ID`
- `FACEBOOK_APP_SECRET`
- `FACEBOOK_REDIRECT_URI`
- `FACEBOOK_GRAPH_API_VERSION`
- `FACEBOOK_BASE_URL`
- `FACEBOOK_OAUTH_URL`
- `GOOGLE_CLIENT_ID`
- `GOOGLE_CLIENT_SECRET`
- `SMTP_HOST`
- `SMTP_PORT`
- `SMTP_USERNAME`
- `SMTP_PASSWORD`
- `FROM_EMAIL`
- `GEMINI_API_KEY`
- `GEMINI_MODEL`
- `GEMINI_MAX_TOKENS`
- `GEMINI_TEMPERATURE`

Development connection string defaults to:

```text
Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=123
```

### Required JWT Setting

`Program.cs` throws at startup if `JwtSettings:SecretKey` is empty. In local development this can come from `JWT_SECRET_KEY` in `.env` or the process environment.

### Data Protection

ASP.NET Data Protection keys are persisted to:

```text
AISAM.API/.keys
```

### CORS

The configured CORS policy allows any origin, header and method:

```text
AllowAnyOrigin()
AllowAnyHeader()
AllowAnyMethod()
```

This is convenient for development but should be tightened before production.

## 4. Dependency Injection

`Program.cs` registers the currently active service and repository layer:

Repositories:

- `IUserRepository` -> `UserRepository`
- `ISessionRepository` -> `SessionRepository`
- `IProfileRepository` -> `ProfileRepository`
- `IBrandRepository` -> `BrandRepository`
- `IProductRepository` -> `ProductRepository`
- `IContentRepository` -> `ContentRepository`
- `IAiGenerationRepository` -> `AiGenerationRepository`
- `IConversationRepository` -> `ConversationRepository`
- `ISocialAccountRepository` -> `SocialAccountRepository`
- `ISocialIntegrationRepository` -> `SocialIntegrationRepository`
- `IPostRepository` -> `PostRepository`
- `INotificationRepository` -> `NotificationRepository`
- `IContentCalendarRepository` -> `ContentCalendarRepository`
- `IPerformanceReportRepository` -> `PerformanceReportRepository`

Services:

- `IAuthService` -> `AuthService`
- `IEmailService` -> `EmailService`
- `IProfileService` -> `ProfileService`
- `IBrandService` -> `BrandService`
- `IProductService` -> `ProductService`
- `IContentService` -> `ContentService`
- `IAIService` -> `AIService`
- `IConversationService` -> `ConversationService`
- `ISocialService` -> `SocialService`
- `IPostService` -> `PostService`
- `INotificationService` -> `NotificationService`
- `IContentScheduleService` -> `ContentScheduleService`
- `IDashboardService` -> `DashboardService`
- `IScheduledPostingService` -> `ScheduledPostingService`

Supporting infrastructure:

- `IOAuthStateStore` -> `MemoryOAuthStateStore`
- `ISocialTokenProtector` -> `SocialTokenProtector`
- `IGeminiTextClient` -> `GeminiTextClient`
- typed `HttpClient` for `FacebookProvider` and `GoogleProvider`
- `IProviderService` registrations for `FacebookProvider` and `GoogleProvider`
- hosted service `ScheduledPostingBackgroundService`

Entity Framework is registered only when a non-empty connection string exists. PostgreSQL is accessed through an `NpgsqlDataSource` with dynamic JSON enabled.

## 5. API Layer

The active controllers are:

- `AuthController`
- `ProfileController`
- `BrandController`
- `ProductController`
- `ContentController`
- `GeminiController`
- `ConversationController`
- `SocialAuthController`
- `SocialAccountsController`
- `SocialIntegrationController`
- `PostsController`
- `NotificationsController`
- `ContentSchedulesController`
- `DashboardController`
- `DevSchedulerController` (Development only)
- `HealthController`

All controllers return `GenericResponse<T>` or `GenericResponse<object>` for a consistent envelope.

### 5.1 Auth API

Route base:

```text
api/Auth
```

Endpoints:

- `POST api/Auth/register`
- `POST api/Auth/login`
- `POST api/Auth/google`
- `POST api/Auth/refresh`
- `POST api/Auth/logout`
- `POST api/Auth/logout-all`
- `GET api/Auth/sessions`
- `POST api/Auth/change-password`
- `GET api/Auth/me`
- `POST api/Auth/forgot-password`
- `POST api/Auth/reset-password`
- `POST api/Auth/change-password-with-token`
- `GET api/Auth/verify-email?token=...`
- `POST api/Auth/verify-email/resend`

Protected endpoints use `[Authorize]` and extract the current user from JWT claims.

Auth capabilities:

- Register with email/password.
- Login with email/password.
- Login with Google ID token.
- Issue JWT access tokens and refresh tokens.
- Store refresh tokens as session rows.
- Revoke one session or all user sessions.
- List active sessions.
- Change password.
- Forgot/reset password by token.
- Verify email by token.
- Resend email verification.
- Return current JWT user claims.

Important implementation details:

- Password hashing uses `HMACSHA512` with a generated key stored as `PasswordSalt`.
- Refresh tokens are 64 random bytes encoded as Base64.
- Email verification and password reset tokens are generated from 32 random bytes and URL-safe transformed.
- Google login uses `GoogleJsonWebSignature.ValidateAsync` with configured Google client ID as audience.
- JWT claims include `NameIdentifier`, `Email`, `Role`, `Jti`, and optionally `Name`.

### 5.2 Profile API

Route base:

```text
api/profiles
```

Endpoints:

- `GET api/profiles/user/{userId}?search=&isDeleted=`
- `GET api/profiles/{id}`
- `POST api/profiles/user/{userId}`
- `PUT api/profiles/{id}`
- `DELETE api/profiles/{id}`
- `PATCH api/profiles/{id}/restore`

The controller is protected with `[Authorize]`.

Profile capabilities:

- Search/list profiles by user.
- Get profile detail.
- Create profile from multipart form data.
- Update profile from multipart form data.
- Soft-delete profile by setting status to `Cancelled`.
- Restore deleted profile by setting status to `Pending`.

Current limitation:

- `CreateProfileRequest` and `UpdateProfileRequest` accept `AvatarFile`, but `ProfileService` explicitly rejects file upload in this MVP backend. Clients should use `AvatarUrl` instead.

Security note:

- Some profile endpoints accept `userId` from the route and do not compare it to the authenticated user claim in the controller/service. Brand and product flows enforce ownership more explicitly.

### 5.3 Brand API

Route base:

```text
api/brands
```

Endpoints:

- `GET api/brands?profileId=&page=&pageSize=&searchTerm=&sortBy=&sortDescending=&includeDeleted=`
- `GET api/brands/{id}`
- `POST api/brands`
- `PUT api/brands/{id}`
- `DELETE api/brands/{id}`
- `POST api/brands/{id}/restore`

The controller is protected with `[Authorize]`.

Brand capabilities:

- List brands by profile with pagination, search and sorting.
- Get brand detail.
- Create brand.
- Partially update brand fields.
- Soft-delete brand through `IsDeleted`.
- Restore soft-deleted brand.

Ownership:

- `BrandService` checks that the brand/profile belongs to the JWT user before reading or mutating brand data.

Brand fields:

- `ProfileId`
- `Name`
- `Description`
- `LogoUrl`
- `Slogan`
- `Usp`
- `TargetAudience`
- `IsDeleted`
- timestamps

Brand DTOs also expose active product/content counts from loaded navigation collections.

### 5.4 Product API

Route base:

```text
api/products
```

Endpoints:

- `GET api/products?brandId=&page=&pageSize=&searchTerm=&sortBy=&sortDescending=&includeDeleted=`
- `GET api/products/{id}`
- `POST api/products`
- `PUT api/products/{id}`
- `DELETE api/products/{id}`
- `POST api/products/{id}/restore`

The controller is protected with `[Authorize]`.

Product capabilities:

- List products with optional brand filter, pagination, search and sorting.
- Get product detail.
- Create product.
- Update product.
- Soft-delete product through `IsDeleted`.
- Restore soft-deleted product.

Ownership:

- Product access is allowed only when `product.Brand.Profile.UserId` matches the JWT user.
- When creating or moving a product to another brand, `ProductService` validates brand ownership.

Current limitation:

- Product create/update accepts `ImageFiles`, but `ProductService` explicitly rejects image uploads in this MVP backend.
- The `Images` column is currently serialized as JSON text. New product creation stores an empty list.

### 5.6 Notification API

Route base:

```text
api/notifications
```

Endpoints:

- `GET api/notifications`
- `GET api/notifications/{notificationId}`
- `POST api/notifications/{notificationId}/mark-read`
- `POST api/notifications/mark-all-read`
- `GET api/notifications/unread-count`

Behavior:

- All endpoints require JWT and `X-Profile-Id`.
- Reads and updates are scoped to the active profile only.
- Notification state is persisted in the `notifications` table.

### 5.7 Content Scheduling API

Route base:

```text
api/content-schedules
```

Endpoints:

- `POST api/content-schedules`
- `GET api/content-schedules`
- `GET api/content-schedules/{scheduleId}`
- `PUT api/content-schedules/{scheduleId}`
- `DELETE api/content-schedules/{scheduleId}`
- `GET api/content-schedules/upcoming`

Behavior:

- Schedules are one-time only in the active MVP.
- Create/update validates content ownership, integration ownership and brand match.
- Published content cannot be scheduled again.
- Delete is soft delete.
- Create/update/delete emits internal notifications.

### 5.8 Dashboard API

Route base:

```text
api/dashboard
```

Endpoint:

- `GET api/dashboard/summary`

Current summary fields:

- `DraftContentCount`
- `PublishedContentCount`
- `PendingApprovalContentCount`
- `UpcomingScheduleCount`
- `FailedScheduleCount`
- `ActiveSocialIntegrationCount`
- `PublishedPostCount`
- `UnreadNotificationCount`

### 5.9 Scheduled Posting Worker

The active runtime now contains:

- `ScheduledPostingService`
- `ScheduledPostingBackgroundService`

Worker behavior:

- scans due schedules
- reuses `IContentService.PublishAsync`
- marks schedule `Completed` on success
- marks schedule `Failed` and stores `LastError` on failure
- creates internal notifications for success/failure

Development-only trigger:

```text
POST /api/dev/scheduler/run-now
```

This controller is only mapped when `ASPNETCORE_ENVIRONMENT=Development`.

### 5.5 Health API

Route base:

```text
api/Health
```

Endpoint:

- `GET api/Health`

Returns a success response with:

- `status = Healthy`
- `service = AISAM Backend`
- UTC timestamp

This endpoint is public.

## 6. Middleware and Filters

### ExceptionHandlerMiddleware

`AISAM.API/Middleware/ExceptionHandlerMiddleware.cs` catches unhandled exceptions and returns a `GenericResponse<object>` error response. It maps:

- `UnauthorizedAccessException` -> 401
- `ArgumentException` -> 400
- default -> 500

### ValidationFilter

`AISAM.API/Filters/ValidationFilter.cs` is registered globally. It intercepts invalid `ModelState` and returns HTTP 400 with a `GenericResponse<object>` containing validation errors.

ASP.NET's default automatic model state invalid response is suppressed:

```text
SuppressModelStateInvalidFilter = true
```

This makes the custom filter responsible for validation response shape.

## 7. Service Layer

### AuthService

Handles all account and token logic.

Responsibilities:

- Check duplicate user email during registration.
- Create password hash/salt.
- Create user rows.
- Generate email verification tokens.
- Send verification emails.
- Validate login credentials.
- Validate Google ID token.
- Generate JWT access token.
- Generate refresh token.
- Persist sessions.
- Rotate refresh token by revoking old session and creating a new one.
- Revoke sessions on logout/password reset.
- Generate and validate password reset tokens.
- Verify email tokens.

Dependencies:

- `IUserRepository`
- `ISessionRepository`
- `IEmailService`
- `JwtSettings`
- `GoogleSettings`

### EmailService

Sends SMTP emails and builds HTML email templates.

Supported email methods:

- Email verification.
- Password reset.
- Welcome email.
- Team invitation.
- Generic notification email.
- Low-level `SendEmailAsync`.

Current behavior:

- If SMTP host or username is missing, the service logs a warning and returns `false` instead of throwing.
- Email templates contain Vietnamese text, but the source currently appears to have mojibake/encoding corruption in several literals.

Dependencies:

- `EmailSettings`
- `FrontendSettings`
- `ILogger<EmailService>`

### ProfileService

Handles profile CRUD and soft-delete restore.

Responsibilities:

- Verify user exists before listing/creating profiles.
- Map `Profile` entities to `ProfileResponseDto`.
- Reject avatar file upload in the current MVP.
- Create/update profile metadata.
- Soft-delete by status.
- Restore cancelled profiles.

Profile deletion uses `ProfileStatusEnum.Cancelled`, not a separate `IsDeleted` boolean.

### BrandService

Handles profile-scoped brand operations.

Responsibilities:

- Ensure profile exists.
- Ensure profile belongs to current JWT user.
- List/paginate/search/sort brands by profile.
- Create/update brand.
- Soft-delete/restore via `Brand.IsDeleted`.
- Map `Brand` to `BrandResponseDto`.

### ProductService

Handles brand-scoped product operations.

Responsibilities:

- Ensure brand exists.
- Ensure brand/profile belongs to current JWT user.
- List/paginate/search/sort products.
- Filter products to those visible to the user.
- Create/update/delete/restore product.
- Serialize/deserialize product image URL list.
- Reject product image upload in the current MVP.

## 8. Repository Layer

Repositories wrap EF Core queries and persistence.

### UserRepository

Supports:

- Get user by ID.
- Get user by email.
- Create user.
- Update user.
- Lookup by password reset token.
- Lookup by email verification token.
- Paged user listing for admin-style views.

Paged user listing includes social account count by checking profiles and non-deleted social accounts.

### SessionRepository

Supports:

- Get session by ID.
- Get active session by refresh token.
- List active unexpired sessions by user.
- Create session.
- Update session.
- Revoke one session.
- Revoke all active sessions for user.
- Delete expired/inactive sessions.

### ProfileRepository

Supports:

- Get profile by ID, excluding `Cancelled`.
- Get profile including deleted/cancelled.
- List/search by user.
- Filter deleted/non-deleted profiles.
- Create/update profile.
- Soft-delete by setting `Status = Cancelled`.
- Restore by setting `Status = Pending`.
- Existence check.

Search uses PostgreSQL `ILIKE` through `EF.Functions.ILike`.

### BrandRepository

Supports:

- Get brand by ID, excluding deleted.
- Get brand including deleted.
- Paged list by profile.
- Search by name/description.
- Sort by name or created date.
- Add brand.
- Update brand.

Queries include profile, products and contents.

### ProductRepository

Supports:

- Get product by ID, excluding deleted.
- Get product including deleted.
- Paged list with optional brand filter.
- Search by name/description.
- Sort by name, price or created date.
- List products by brand.
- Add product.
- Update product.

Queries include brand and brand profile for ownership checks.

## 9. Data Model

`AisamContext` contains a broad model for an AI/social/ads platform.

### Active MVP Entities

These are directly used by active controllers/services:

- `User`
- `Session`
- `Profile`
- `Brand`
- `Product`

### Broader Platform Entities Present in EF Model

These are configured as DbSets and have relationships, but active root controllers/services are not yet implemented for them:

- `SocialAccount`
- `SocialIntegration`
- `Post`
- `Content`
- `Asset`
- `Team`
- `TeamMember`
- `TeamBrand`
- `Subscription`
- `Approval`
- `Ad`
- `AdCampaign`
- `AdSet`
- `AdCreative`
- `PerformanceReport`
- `ContentCalendar`
- `AiGeneration`
- `Notification`
- `Payment`
- `ContentTemplate`
- `AuditLog`
- `Conversation`
- `ChatMessage`

### Important Relationships

User:

- Has many `Sessions`.
- Has many `Profiles`.
- Has many `TeamMembers`.

Profile:

- Belongs to `User`.
- Optionally references `Subscription`.
- Has many `Brands`.
- Has many `SocialAccounts`.
- Has many `SocialIntegrations`.
- Has many `Teams`.
- Has many `Approvals`.
- Has many `AdCampaigns`.
- Has many `ContentCalendars`.
- Has many `Notifications`.
- Has many `Conversations`.

Brand:

- Belongs to `Profile`.
- Has many `Products`.
- Has many `Contents`.
- Has many `SocialIntegrations`.
- Has many `TeamBrands`.
- Has many `AdCampaigns`.

Product:

- Belongs to `Brand`.
- Has many `Contents`.

Content:

- Belongs to `Brand`.
- Optionally belongs to `Product`.
- Has approvals, posts, ad creatives, AI generations and calendar schedules.

Social:

- `SocialAccount` belongs to `Profile`.
- `SocialIntegration` connects `Profile`, `Brand` and `SocialAccount`.

Ads:

- `AdCampaign` belongs to `Profile` and `Brand`.
- `AdSet` belongs to `AdCampaign`.
- `AdCreative` belongs to `Content`.
- `Ad` belongs to `AdSet` and `AdCreative`.

Collaboration:

- `Team` belongs to `Profile`.
- `TeamMember` connects a team and user.
- `TeamBrand` connects a team and brand.

AI chat:

- `Conversation` belongs to profile and optionally brand/product.
- `ChatMessage` belongs to conversation and can reference AI generation/content.

### Delete Behavior

Most parent-child relationships use cascade delete in EF configuration. Some optional relationships use `SetNull`, for example:

- `Content.Product`
- `Asset.User`
- `Payment.Subscription`
- `Conversation.Brand`
- `Conversation.Product`
- `ChatMessage.AiGeneration`
- `ChatMessage.Content`

Soft delete is implemented inconsistently by entity:

- `Profile`: `Status = Cancelled`.
- `Brand`: `IsDeleted = true`.
- `Product`: `IsDeleted = true`.
- Other entities may have `IsDeleted` fields, but no active service/controller in root handles them.

## 10. Enums

`AISAM.Data/Enumeration` defines:

- `UserRoleEnum`: `User`, `Vendor`, `Admin`
- `ProfileTypeEnum`: `Free`, `Basic`, `Pro`
- `ProfileStatusEnum`: `Pending`, `Active`, `Suspended`, `Cancelled`
- `SubscriptionPlanEnum`: `Free`, `Plus`, `Premium`, `PlusTrial`
- `SocialPlatformEnum`: `Facebook`, `Instagram`, `TikTok`, `Twitter`, `Google`, `YouTube`
- `ContentStatusEnum`: `Draft`, `PendingApproval`, `Approved`, `Rejected`, `Published`
- `AdTypeEnum`: `TextOnly`, `ImageText`, `VideoText`
- `AiStatusEnum`: `Pending`, `Completed`, `Failed`
- `NotificationTypeEnum`: `ApprovalNeeded`, `PostScheduled`, `PerformanceAlert`, `AiSuggestion`, `SystemUpdate`
- `PaymentStatusEnum`: `Pending`, `Success`, `Failed`, `Refunded`
- `RepeatTypeEnum`: `None`, `Daily`, `Weekly`, `Monthly`
- `DefaultBucketEnum`: `Avatar`, `BrandAssets`, `ProductMedia`, `ContentMedia`, `AiGenerated`
- `AssetTypeEnum`: `Video`, `Image`, `Audio`, `Document`
- `TeamStatusEnum`: `Active`, `Inactive`, `Archived`
- `TeamMemberRoleEnum`: `Copywriter`, `Designer`

Note: `TeamMemberRoleEnum` currently contains only `Copywriter` and `Designer`, despite broader team/approval concepts in older docs.

## 11. Common Layer

### GenericResponse

`GenericResponse<T>` standardizes response envelopes:

- `Success`
- `Message`
- `Data`
- `Errors`
- `StatusCode`

It includes factory helpers for success and error responses.

### Pagination

`PaginationRequest` contains:

- `Page`
- `PageSize`
- `SearchTerm`
- `SortBy`
- `SortDescending`

`PagedResult<T>` contains:

- `Data`
- `TotalCount`
- `Page`
- `PageSize`
- calculated total pages/has next/has previous behavior.

### Request DTOs

Active request DTOs include:

- Auth: `RegisterRequest`, `LoginRequest`, `RefreshTokenRequest`, `LogoutRequest`, `ChangePasswordRequest`, `GoogleLoginRequest`
- Email/password: `EmailVerificationRequest`, `ForgotPasswordRequest`, `ResetPasswordRequest`
- Profile: `CreateProfileRequest`, `UpdateProfileRequest`
- Brand: `CreateBrandRequest`, `UpdateBrandRequest`
- Product: `ProductCreateRequest`, `ProductUpdateRequestDto`

### Response DTOs

Active response DTOs include:

- Auth: `TokenResponse`, `UserDto`, `SessionDto`
- User: `UserResponseDto`, `UserListDto`
- Profile: `ProfileResponseDto`
- Brand: `BrandResponseDto`
- Product: `ProductResponseDto`

### Social DTOs

`AISAM.Common/Models/SocialDtos.cs` contains social account/target DTOs. In the active root codebase these are not currently exposed by a controller, but they align with the broader social model in `AISAM.Data`.

## 12. Database and Migrations

The repository uses EF Core migrations under:

```text
AISAM.Repositories/Migrations
```

Existing migrations:

- `20251102025736_Initial`
- `20260124120929_AddCustomAuthenticationTables`
- `20260124133308_verifytoken`
- `20260124135926_UpdatePasswordSaltLength`
- `20260127160619_UpdateSubscriptionPayOS`

`AisamContextFactory` exists for design-time EF tooling. It loads configuration from `AISAM.API/appsettings.json`, `AISAM.API/appsettings.Development.json` and environment variables.

The active app uses PostgreSQL through `Npgsql.EntityFrameworkCore.PostgreSQL`.

## 13. Packages and External Integrations

Active package references include:

- ASP.NET Core Web SDK.
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Tools`
- `DotNetEnv`
- `Swashbuckle.AspNetCore`
- `FluentValidation` and `FluentValidation.AspNetCore`
- `Google.Apis.Auth`
- `Supabase`
- `Mscc.GenerativeAI`
- `Google.Cloud.AIPlatform.V1`
- `Stripe.net`
- `BCrypt.Net-Next`

Important mismatch:

- Gemini text generation is now active through `IGeminiTextClient` and `GeminiTextClient`.
- Supabase upload, Vertex image generation, Stripe and BCrypt are still not used by active exposed workflows. Password hashing uses `HMACSHA512`, not BCrypt.

## 14. Testing

The test project exists at:

```text
tests/AISAM.IntegrationTests
```

It references `AISAM.API` and uses:

- xUnit
- `Microsoft.NET.Test.Sdk`
- `coverlet.collector`
- `xunit.runner.visualstudio`

The current suite contains meaningful tests for:

- Foundation ownership and disabled-upload behavior.
- Active profile middleware.
- Content lifecycle service and controller.
- Content publish orchestration and publish controller.
- Gemini client parsing and missing-key behavior.
- AI generation, approve and chat orchestration.
- AI controller profile propagation.
- Conversation history ownership and controller propagation.
- Social provider/state/protection infrastructure.
- Social repository, service and controller behavior.
- Posts service and controller behavior.
- Notification repository, service and controller behavior.
- Content schedule repository, service and controller behavior.
- Dashboard summary aggregation.
- Scheduled posting worker and Development-only scheduler boundary.

Latest local run on 2026-06-01:

```text
dotnet test AISAM.sln
Passed: 100, Failed: 0, Skipped: 0
```

## 15. Current Runtime Verification Notes

Swagger and Health were verified locally on 2026-06-01:

```text
http://localhost:5283
```

Verified endpoints:

- `/swagger/v1/swagger.json`
- `/api/Health`

Verified Swagger paths:

- `/api/content`
- `/api/ai/generate-draft`
- `/api/ai/chat`
- `/api/conversations`
- `/api/social-auth/facebook`
- `/api/social/accounts/me`
- `/api/content/{contentId}/publish/{integrationId}`
- `/api/posts`
- `/api/notifications`
- `/api/content-schedules`
- `/api/dashboard/summary`
- `/api/dev/scheduler/run-now` (Development only)

Verified authentication boundary:

- `GET /api/content` without JWT returns HTTP `401`.
- `POST /api/ai/generate-draft` without JWT returns HTTP `401`.
- `GET /api/conversations` without JWT returns HTTP `401`.
- `GET /api/social-auth/facebook` without JWT returns HTTP `401`.
- `GET /api/social/accounts/me` without JWT returns HTTP `401`.
- `GET /api/posts` without JWT returns HTTP `401`.
- `GET /api/notifications` without JWT returns HTTP `401`.
- `GET /api/content-schedules` without JWT returns HTTP `401`.
- `GET /api/dashboard/summary` without JWT returns HTTP `401`.
- `POST /api/dev/scheduler/run-now` without JWT returns HTTP `401` in `Development`.
- `POST /api/dev/scheduler/run-now` returns HTTP `404` outside `Development`.

Protected APIs for active profile-scoped modules require:

```text
Authorization: Bearer <access-token>
X-Profile-Id: <owned-profile-guid>
```

This header requirement currently applies to:

- `/api/content`
- `/api/content-schedules`
- `/api/dashboard`
- `/api/dev/scheduler` (Development only)
- `/api/ai`
- `/api/conversations`
- `/api/social-auth`
- `/api/social`
- `/api/posts`
- `/api/notifications`

The API host starts without `GEMINI_API_KEY` and without Facebook App credentials. Gemini text endpoints then return a recorded failed generation or a graceful chat error instead of preventing startup. Facebook auth URL endpoint returns a clear `503` configuration error instead of crashing the host.

Local PostgreSQL is reachable in this environment, and authenticated HTTP smoke now works. Two remaining external blockers still apply:

- real Gemini success smoke requires a valid `GEMINI_API_KEY`
- real Facebook OAuth/publish smoke requires `FACEBOOK_APP_ID`, `FACEBOOK_APP_SECRET`, redirect URI and Page permissions

Build/restore may fail in this environment when NuGet access is blocked by TLS/certificate issues. The local build output may still allow running the already-built DLL, but a clean build requires NuGet restore to work.

## 16. Docs and Reference Material

The repository includes several large documentation/reference files:

- `README.md`
- `docs/main/setup-guide.md`
- `docs/reference/specification-answers.md`
- `docs/reference/backend-progress-vs-srs.md`
- `docs/archive/plans/backend-code-plan.md`
- `docs/main/development-guardrails.md`
- `docs/api/api-spec.md`
- `docs/database/db-spec.md`
- `docs/code-references/PRN232_Backend`

`docs/code-references/PRN232_Backend` appears to contain a richer/older backend snapshot with many controllers and services that are not active in the root solution. It is useful for comparison or future implementation guidance, but should not be treated as the current compiled API unless those files are copied/implemented in the active project directories.

## 17. Implementation Gaps and Risks

### API Surface vs Data Model

The EF model is broad, but the exposed API is narrow. Many tables have relationships but no root-level service/controller implementation. This can confuse readers because older docs describe modules like content, social publishing, ads, payments and notifications as if they are complete.

Current active modules:

- Auth
- Profile
- Brand
- Product
- Health
- Content library
- Gemini text generation and improve/approve flow
- AI chat
- Conversation history
- Social/Facebook auth and Page linking
- Facebook Page publishing from content
- Posts history
- Notifications
- Content scheduling
- Dashboard summary
- Scheduled posting worker

Model-ready but not active as APIs:

- Approvals
- Team management
- Ads
- Payments
- Analytics

### Authorization Consistency

Brand/product ownership checks are explicit. Profile ownership checks were stabilized in Phase A. Content, AI, Conversation, Social and Posts APIs additionally require `X-Profile-Id`, validated against the JWT user by `ActiveProfileMiddleware`.

### File Upload

Profile avatar and product image upload DTOs exist, and controllers accept multipart form data, but services reject file upload as not enabled in the MVP. Client code should use URL fields until storage implementation is added.

### Email Encoding

Several Vietnamese email template strings appear corrupted in source. This should be corrected with UTF-8 source encoding and reviewed in actual email clients.

### Package Drift

Several package references are not used by active code paths. This may be intentional for upcoming features, but it increases restore/build surface and can obscure the real runtime dependencies.

### Test Coverage

The suite now covers ownership boundaries, soft delete behavior, content validation, Gemini client behavior, AI generation/chat orchestration, conversation history, social OAuth/state handling, Facebook publish orchestration and posts history. Real Facebook OAuth/publish still needs external credentials and permissions.

## 18. Suggested Mental Model for Contributors

When changing this codebase, start from the active layer:

1. Add/update DTOs in `AISAM.Common`.
2. Add/update entity shape in `AISAM.Data` if persistence changes.
3. Add/update EF relationships in `AISAM.Repositories/AISAMContext.cs`.
4. Add migration under `AISAM.Repositories/Migrations`.
5. Add repository interface/implementation if query logic is reused.
6. Add service interface/implementation for business rules.
7. Register the repository/service in `Program.cs`.
8. Add controller endpoints in `AISAM.API`.
9. Add tests in `tests/AISAM.IntegrationTests`.

For any module that already exists only as an entity, do not assume the API is implemented. Check for:

- Controller under `AISAM.API/Controllers`
- Service interface under `AISAM.Services/IServices`
- Service implementation under `AISAM.Services/Service`
- Repository interface under `AISAM.Repositories/IRepositories`
- Repository implementation under `AISAM.Repositories/Repository`
- DI registration in `Program.cs`

If any of these are missing, the feature is only partially present.
