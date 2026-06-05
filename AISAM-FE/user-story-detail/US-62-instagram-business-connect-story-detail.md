# US-62 - Ket noi Instagram Business

## Mo ta

La nguoi dung, toi muon ket noi Instagram Business de mo rong social publishing sau khi Facebook flow on dinh.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `requirement.md`: he thong dinh huong ho tro Facebook, Instagram va TikTok; Instagram Business Account ket noi OAuth trong pham vi duoc ho tro; mot so tinh nang co the bi gioi han boi app review/permission.
- `BACKEND_CODE_PLAN.md`: MVP backend chay truoc, chua om Instagram/TikTok day du; Phase 6 hien chi uu tien Facebook provider/publishing.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase C da active Facebook social publishing; public social endpoints chi mo cho `facebook`; Instagram/TikTok/Twitter la Phase H4 optional.
- Active backend `AISAM-BE`: co `SocialPlatformEnum.Instagram`, provider contract chung, social repositories, OAuth state/token protection; nhung chua co Instagram provider/controller route.

## Trang thai backend hien tai

Backend da co:

- `SocialPlatformEnum`:

```ts
Facebook = 0
Instagram = 1
TikTok = 2
Twitter = 3
Google = 4
YouTube = 5
```

- `IProviderService` contract chung.
- OAuth state store.
- Social token protector.
- Social account/integration repositories.
- Facebook OAuth/account/target/publish flow active.
- Active endpoints:

```text
GET  /api/social-auth/facebook
POST /api/social-auth/facebook/callback
GET  /api/social/accounts/me
GET  /api/social/accounts/{socialAccountId}/available-targets
GET  /api/social/accounts/{socialAccountId}/linked-targets
POST /api/social/accounts/{socialAccountId}/link-targets
DELETE /api/social/accounts/{socialAccountId}
GET  /api/social/integrations/brand/{brandId}
DELETE /api/social/integrations/{socialIntegrationId}
```

Backend chua co active:

- `InstagramProvider`.
- `InstagramSettings`.
- `GET /api/social-auth/instagram`.
- `POST /api/social-auth/instagram/callback`.
- Instagram Business account/page discovery.
- Instagram media publishing.
- Instagram token refresh/reconnect flow.
- Instagram-specific target mapping.

Quan trong: `SocialService` hien tai hard-code chi support Facebook:

```text
Only Facebook is supported in Phase C.
```

Ket luan: frontend co the chuan bi UI Instagram Business connect, nhung phai disabled/planned state cho den khi backend Phase H4 active.

## Muc tieu frontend

Mo rong trang social accounts de ho tro Instagram Business:

```text
/dashboard/social-accounts
/social-callback/instagram
```

Nguoi dung co the:

- Thay Instagram Business la kenh planned/coming soon khi backend chua active.
- Khi backend active, bam connect Instagram Business.
- Di qua OAuth flow.
- Xem Instagram Business accounts/targets kha dung.
- Link Instagram target voi brand.
- Xem trang thai active/expired/revoked.
- Reconnect/disconnect Instagram account.

## User flow du kien

### Flow 1 - Backend chua active

1. User vao `/dashboard/social-accounts`.
2. UI hien card Instagram Business voi badge `Planned` hoac `Coming soon`.
3. Connect button disabled.
4. Neu user bam, hien:

```text
Instagram Business integration chua active trong backend hien tai.
```

### Flow 2 - Connect Instagram Business khi backend active

1. User co active profile.
2. User bam `Connect Instagram`.
3. Frontend goi `GET /api/social-auth/instagram`.
4. Backend tra `authUrl` va `state`.
5. Browser redirect den Meta/Instagram OAuth.
6. OAuth callback ve frontend `/social-callback/instagram?code=...&state=...`.
7. Frontend POST callback len backend.
8. Backend tao `SocialAccount` platform `Instagram`.
9. UI refresh social account list.

### Flow 3 - Link Instagram target voi brand

1. User mo Instagram account.
2. Frontend goi available targets.
3. User chon Instagram Business account target.
4. User chon brand.
5. Frontend link target voi brand.
6. UI hien integration trong brand/social section.

## Frontend scope

Pages/components can implement/cap nhat:

```text
/dashboard/social-accounts
/social-callback/instagram
SocialAccountsPage
SocialProviderCard
InstagramConnectButton
InstagramBackendNotReadyState
SocialCallbackPage
AvailableTargetsDialog
LinkedTargetsTable
ReconnectSocialAccountButton
DisconnectSocialAccountButton
```

Can cap nhat endpoint map/type de co provider generic:

```ts
type SocialProvider = "facebook" | "instagram"
```

Nhung active production flow chi goi Instagram khi feature flag/backend capability bat.

## Backend API du kien

Backend hien tai chua expose cac endpoint Instagram duoi day. Day la contract de frontend chuan bi cho Phase H4.

### Get Instagram auth URL

```http
GET /api/social-auth/instagram
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<AuthUrlResponse>
```

```ts
interface AuthUrlResponse {
  authUrl: string
  state: string
}
```

### Handle Instagram callback

```http
POST /api/social-auth/instagram/callback
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "code": "oauth-code",
  "state": "oauth-state"
}
```

Response:

```ts
ApiResponse<SocialAccountDto>
```

### List accounts

Co the tai su dung endpoint hien co:

```http
GET /api/social/accounts/me
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response tra ca Facebook va Instagram accounts:

```ts
ApiResponse<SocialAccountDto[]>
```

### List Instagram targets

Co the tai su dung endpoint hien co:

```http
GET /api/social/accounts/{socialAccountId}/available-targets
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Target type du kien:

```text
instagram_business_account
```

### Link Instagram target voi brand

Co the tai su dung endpoint hien co:

```http
POST /api/social/accounts/{socialAccountId}/link-targets
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "provider": "instagram",
  "brandId": "brand-guid",
  "providerTargetIds": ["instagram-business-account-id"]
}
```

## API response types du kien

```ts
interface SocialAccountDto {
  id: string
  profileId: string
  provider: "facebook" | "instagram"
  providerUserId: string
  isActive: boolean
  expiresAt?: string
  createdAt: string
  updatedAt?: string
  targets: SocialTargetDto[]
}

interface SocialTargetDto {
  id: string
  providerTargetId: string
  name: string
  type: "page" | "instagram_business_account"
  isActive: boolean
}

interface AvailableTargetDto {
  providerTargetId: string
  name: string
  type: "page" | "instagram_business_account"
  metadata?: Record<string, unknown>
}
```

## API status handling

Frontend can xu ly:

- `200`: connect/list/link thanh cong.
- `400`: invalid callback/state/target.
- `401`: token thieu/het han, redirect login.
- `403`: thieu permission Meta/Instagram.
- `404`: endpoint Instagram chua active hoac account/target khong ton tai.
- `409`: target da link voi brand/duplicate.
- `503`: Instagram provider/config chua cau hinh.
- `500`: loi he thong.

Khi backend tra `404` cho Instagram auth:

```text
Instagram Business API chua active trong backend hien tai.
```

Khi backend tra `503`:

```text
Instagram integration chua duoc cau hinh.
```

## Business rules

- User phai dang nhap va co active profile.
- Instagram connect phai gui `Authorization` va `X-Profile-Id`.
- OAuth `state` phai khop provider va active profile.
- Instagram target phai thuoc social account dang link.
- Brand duoc link phai thuoc active profile.
- Token phai duoc backend ma hoa, frontend khong bao gio luu access token Instagram.
- Neu account token het han/revoked, UI hien reconnect action.
- Instagram Business publishing co the phu thuoc Facebook Page/Instagram Business account mapping cua Meta.
- Neu app chua duoc Meta review/permission, UI phai hien loi ro.

## UI requirements

### Social provider cards

Facebook:

- Active connect button neu backend Facebook active.

Instagram:

- Hien logo/name `Instagram Business`.
- Badge:
  - `Coming soon` khi backend chua active.
  - `Not configured` khi backend active nhung thieu config.
  - `Connected` khi da co account active.
  - `Expired` khi token het han.
- Connect button disabled neu backend chua active.

### Instagram callback page

Can xu ly:

- Missing code/state.
- Loading khi POST callback.
- Success redirect ve social accounts page.
- Error state voi message backend.

### Available targets dialog

Can hien:

- Instagram Business account name.
- Provider target id.
- Type.
- Brand selector.
- Link button.

### Backend not ready state

```text
Instagram Business chua active.
```

Mo ta phu:

```text
Backend can hoan thanh Phase H4 Instagram provider truoc khi bat ket noi va publish Instagram.
```

## Acceptance criteria

- Social accounts page co card Instagram Business.
- Khi backend chua active, Instagram connect disabled va hien planned/coming soon state.
- Frontend khong goi `/api/social-auth/instagram` neu feature flag/backend capability chua bat.
- Khi backend active, click connect goi `GET /api/social-auth/instagram` voi `Authorization` va `X-Profile-Id`.
- Callback page `/social-callback/instagram` doc `code` va `state`.
- Callback success refresh social account list.
- Callback fail hien message backend.
- Instagram account list render chung voi Facebook account list.
- Available targets dialog support type `instagram_business_account`.
- Link target request gui provider `"instagram"`.
- `401` redirect login.
- `403` hien permission error.
- `404` hien backend-not-ready/not found tuy ngu canh.
- `503` hien integration not configured.
- Frontend khong luu Instagram access token.

## Suggested frontend types

```ts
export type SocialProvider = "facebook" | "instagram"

export interface SocialProviderCapability {
  provider: SocialProvider
  isBackendActive: boolean
  isConfigured?: boolean
  canConnect: boolean
}

export interface SocialAccountDto {
  id: string
  profileId: string
  provider: SocialProvider
  providerUserId: string
  isActive: boolean
  expiresAt?: string
  createdAt: string
  updatedAt?: string
  targets: SocialTargetDto[]
}

export interface SocialTargetDto {
  id: string
  providerTargetId: string
  name: string
  type: "page" | "instagram_business_account"
  isActive: boolean
}
```

## Suggested API client methods

```ts
export async function getInstagramAuthUrl() {
  return fetchWithAuth<ApiResponse<AuthUrlResponse>>(
    "/social-auth/instagram"
  )
}

export async function completeInstagramCallback(payload: {
  code: string
  state: string
}) {
  return fetchWithAuth<ApiResponse<SocialAccountDto>>(
    "/social-auth/instagram/callback",
    {
      method: "POST",
      body: JSON.stringify(payload),
    }
  )
}

export async function linkInstagramTargets(
  socialAccountId: string,
  payload: {
    brandId: string
    providerTargetIds: string[]
  }
) {
  return fetchWithAuth<ApiResponse<SocialAccountDto>>(
    `/social/accounts/${socialAccountId}/link-targets`,
    {
      method: "POST",
      body: JSON.stringify({
        provider: "instagram",
        ...payload,
      }),
    }
  )
}
```

## Test cases frontend

- Social accounts page hien Instagram Business card.
- Backend capability disabled thi connect button disabled.
- Click disabled Instagram card hien planned message.
- Khi capability enabled, click connect goi auth URL endpoint.
- Missing active profile thi khong goi Instagram API.
- Callback page thieu code/state hien invalid callback state.
- Callback API `200` redirect ve social accounts.
- Callback API `400` hien invalid state/code.
- API `403` hien permission/app review error.
- API `503` hien not configured.
- Available targets dialog hien `instagram_business_account`.
- Link target gui provider `instagram`.

## Dependencies / blockers

- Backend can hoan thanh Phase H4 Instagram provider.
- Can them `InstagramProvider` hoac mo rong Meta/Facebook provider cho Instagram Business.
- Can them `InstagramSettings` va env config.
- Can expose `/api/social-auth/instagram` va callback.
- Can Meta app permissions cho Instagram Business/Graph API.
- Can quyet dinh Instagram publish scope MVP: connect only, publish image/text, hay analytics.
- Can test account/page/Instagram Business account hop le trong Meta sandbox/app review.
