# US-56 - Kiểm thử ownership và boundary chính

## Mô tả

**Là** nhóm phát triển,
**tôi muốn** có test cho auth, profile, brand, product, content, social, payment và scheduling,
**để** giảm regression khi thay đổi code.

## Phạm vi

### Trong phạm vi

- **Auth tests**: login, register, refresh token, logout, role claim trong JWT.
- **Profile tests**: CRUD profile, active profile scope, soft-delete/restore, ownership check.
- **Brand tests**: CRUD brand theo profile, search/sort/pagination, soft-delete/restore.
- **Product tests**: CRUD product theo brand, filter/search, ownership qua brand chain.
- **Content tests**: CRUD content, clone, status flow (Draft→PendingApproval→Approved→Published), soft-delete/restore.
- **AI tests**: generate-draft, improve, approve, chat, quota enforcement.
- **Social tests**: Facebook OAuth, link/unlink page, integration CRUD, token refresh.
- **Publish tests**: publish now, post record creation, quota check (POST_QUOTA_EXCEEDED).
- **Schedule tests**: CRUD schedule, worker execution, status transitions.
- **Notification tests**: list, mark read, unread count.
- **Dashboard tests**: summary metrics accuracy.
- **Payment tests**: checkout, callback, webhook, history, subscription current.
- **Quota tests**: EnsurePromptQuota, EnsurePostQuota, summary.

### Ngoài phạm vi

- Admin endpoints (US-51→55) — chưa có BE.
- Instagram/TikTok (US-62,63) — chưa có BE.
- AI image/video (US-64,65) — chưa có BE.
- E2E test — chỉ unit test + integration test.

## Backend hiện tại

Tất cả controller/service/repository cần test đều đã active trong BE codebase. Có sẵn `AISAM.IntegrationTests` project với `PaymentControllerTests`, `PaymentRepositoryTests`.

## Loại test cần viết

1. **Unit test**: Service layer — mock repository, test business logic.
2. **Integration test**: Controller + Repository — test với DB thật hoặc in-memory.
3. **Ownership test**: Mỗi endpoint kiểm tra profile/user ownership đúng.
4. **Boundary test**: Pagination edge cases (page=0, pageSize > max), empty list, soft-deleted items.
