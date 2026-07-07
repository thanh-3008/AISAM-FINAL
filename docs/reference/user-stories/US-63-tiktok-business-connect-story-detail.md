# US-63 - Ket noi TikTok Business

## 1. Thong tin user story

**Ma story:** US-63  
**Ten story:** Ket noi TikTok Business  
**Vai tro:** Nguoi dung marketing / chu profile  
**Muc tieu:** Ket noi tai khoan TikTok Business vao he thong de mo rong kenh social sau MVP.  
**Mo ta goc:** La nguoi dung, toi muon ket noi TikTok Business de mo rong pham vi social sau MVP.

## 2. Boi canh tu requirement va backend hien tai

Requirement co dinh huong ho tro ket noi TikTok Business Account thong qua OAuth, tuy nhien trong MVP backend hien tai moi ho tro Facebook flow. Cac tai lieu backend plan va codebase update xep Instagram/TikTok/Twitter vao giai doan sau MVP, khong phai pham vi da hoan tat.

Codebase BE hien tai da co nen tang dung chung cho social provider:

- `SocialPlatformEnum` da co gia tri `TikTok = 2`.
- Entity `SocialAccount` co truong token, expiry, refresh token va social platform.
- Entity `SocialIntegration` co `ExternalId`, `AccessToken`, `RefreshToken`, `ExpiresAt` va lien ket brand/profile.
- `IProviderService` da mo ta contract tong quat cho provider OAuth, lay target, lay target access token va publish.
- Facebook provider/flow da active qua `SocialAuthController`, `SocialAccountsController`, `SocialService`.

Nhung backend chua co TikTok provider active:

- Chua co endpoint `/api/social-auth/tiktok`.
- Chua co endpoint `/api/social-auth/tiktok/callback`.
- Chua co `TikTokProvider` / `TikTokSettings` tuong duong Facebook.
- `SocialService` hien dang hard-code Facebook va nem loi neu provider khac Facebook.
- Chua co API discover/link TikTok Business target rieng.
- Chua co publish flow TikTok.

Vi vay frontend cua US-63 can duoc thiet ke theo huong feature-ready: UI va state model san sang cho TikTok, nhung hanh vi connect that su phu thuoc backend Phase H4.

## 3. Pham vi frontend

### In scope

- Hien thi TikTok Business nhu mot provider trong man hinh Social Connections.
- Cho phep FE nhan biet trang thai TikTok theo capability tra ve tu backend:
  - chua ho tro,
  - da cau hinh nhung chua ket noi,
  - dang ket noi,
  - da ket noi,
  - token het han / can reconnect,
  - loi app review / permission.
- Chuan bi luong OAuth callback cho TikTok:
  - redirect sang TikTok auth URL,
  - nhan `code`, `state`, `error` tu URL callback,
  - goi backend callback endpoint,
  - hien thi ket qua thanh cong/that bai.
- Hien thi danh sach TikTok Business account/target sau khi backend cung cap.
- Cho phep gan TikTok Business target vao brand/profile khi backend ho tro link target.
- Khong luu access token, refresh token hoac secret tren frontend.

### Out of scope

- Implement TikTok OAuth truc tiep tren frontend.
- Luu token TikTok o localStorage/sessionStorage.
- Publish TikTok content neu backend chua co provider publish.
- Quan ly TikTok Ads. Phan ads thuoc US-60 hoac story rieng.
- Xu ly app review, TikTok developer app, client key/secret tren frontend.

## 4. Gia dinh backend/API can co

Frontend nen implement theo contract du kien sau. Neu backend dat ten endpoint khac, FE can tao adapter service rieng de tranh anh huong UI.

### 4.1. Lay URL ket noi TikTok

```http
GET /api/social-auth/tiktok?profileId={profileId}&brandId={brandId?}
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response thanh cong:

```json
{
  "authUrl": "https://www.tiktok.com/v2/auth/authorize?...",
  "state": "opaque-state",
  "provider": "tiktok"
}
```

Frontend behavior:

- Khi user bam Connect TikTok, FE goi API nay.
- Neu thanh cong, redirect browser toi `authUrl`.
- Neu backend tra `501 Not Implemented` hoac `provider_not_supported`, FE hien thi trang thai "Coming soon" / "Backend chua bat TikTok".

### 4.2. Xu ly callback TikTok

```http
POST /api/social-auth/tiktok/callback
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

Request:

```json
{
  "code": "oauth-code-from-tiktok",
  "state": "opaque-state",
  "profileId": "profile-id",
  "brandId": "optional-brand-id"
}
```

Response:

```json
{
  "socialAccountId": "social-account-id",
  "provider": "tiktok",
  "displayName": "TikTok Business Account",
  "expiresAt": "2026-07-03T10:00:00Z",
  "requiresTargetSelection": true
}
```

Frontend behavior:

- Route FE du kien: `/social-callback/tiktok`.
- Doc `code`, `state`, `error`, `error_description` tu query string.
- Neu co `error`, hien thi loi va CTA quay lai Social Connections.
- Neu co `code`, goi callback API.
- Neu `requiresTargetSelection = true`, dieu huong toi Social Connections va mo dialog chon target.

### 4.3. Lay danh sach social accounts

```http
GET /api/social/accounts/me
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response can ho tro provider TikTok:

```json
[
  {
    "id": "social-account-id",
    "provider": "tiktok",
    "displayName": "AISAM TikTok Business",
    "status": "connected",
    "expiresAt": "2026-07-03T10:00:00Z",
    "connectedAt": "2026-06-03T10:00:00Z",
    "avatarUrl": "https://...",
    "requiresReconnect": false
  }
]
```

### 4.4. Lay TikTok Business target kha dung

```http
GET /api/social/accounts/{socialAccountId}/available-targets
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
[
  {
    "id": "target-external-id",
    "name": "AISAM Official",
    "type": "tiktok_business_account",
    "avatarUrl": "https://...",
    "canPublish": true
  }
]
```

### 4.5. Link target vao brand

```http
POST /api/social/accounts/{socialAccountId}/link-targets
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

Request:

```json
{
  "brandId": "brand-id",
  "targets": [
    {
      "externalId": "target-external-id",
      "name": "AISAM Official",
      "type": "tiktok_business_account"
    }
  ]
}
```

Response:

```json
{
  "linked": true,
  "brandId": "brand-id",
  "provider": "tiktok",
  "targetsCount": 1
}
```

## 5. Dieu kien tien quyet

- User da dang nhap.
- User co active profile.
- FE co co che chon profile hien tai va gui `X-Profile-Id`.
- Backend da expose capability/config de FE biet TikTok dang enabled hay disabled.
- Backend da cau hinh TikTok client key, client secret, redirect URI va scopes phu hop.
- TikTok developer app duoc phe duyet cho cac permission can dung trong moi truong demo/production.

## 6. UX/UI detail

### 6.1. Man hinh Social Connections

Them provider card cho TikTok Business trong danh sach kenh social.

Noi dung card:

- Ten: `TikTok Business`
- Mo ta ngan: `Ket noi tai khoan TikTok Business de san sang cho publishing sau MVP.`
- Icon/brand mark TikTok theo asset hop le cua ung dung.
- Badge trang thai:
  - `Coming soon` neu backend chua ho tro.
  - `Not connected` neu backend da enabled nhung user chua ket noi.
  - `Connected` neu da co social account.
  - `Needs reconnect` neu token het han hoac backend bao can ket noi lai.
  - `Permission issue` neu TikTok tu choi scope/app review.
- CTA:
  - `Connect` khi provider enabled va chua ket noi.
  - `Reconnect` khi can reconnect.
  - `Manage` khi da ket noi.
  - Disabled button khi backend chua enabled.

### 6.2. Ket noi TikTok

Luon bat dau tu backend, khong tao OAuth URL tren frontend.

Flow:

1. User bam `Connect`.
2. FE goi `GET /api/social-auth/tiktok`.
3. FE hien thi loading tren button.
4. FE redirect den `authUrl` do backend tra ve.
5. TikTok redirect ve `/social-callback/tiktok`.
6. FE goi callback API.
7. FE hien thi ket qua va refresh danh sach social accounts.

### 6.3. Callback screen

Trang callback can co cac state:

- `processing`: dang xu ly ket noi.
- `success`: ket noi thanh cong.
- `missing_code`: callback thieu `code`.
- `oauth_denied`: user tu choi permission tren TikTok.
- `invalid_state`: state khong hop le hoac het han.
- `provider_not_supported`: backend chua bat TikTok.
- `permission_missing`: TikTok app chua duoc cap scope.
- `unknown_error`: loi khac.

CTA sau callback:

- `Back to Social Connections`.
- `Select TikTok account` neu backend yeu cau chon target.
- `Try again` neu loi co the retry.

### 6.4. Quan ly TikTok target

Khi da ket noi social account, man hinh manage can hien thi:

- Ten TikTok Business account.
- Avatar neu backend co.
- Thoi diem ket noi.
- Thoi diem token het han neu backend tra ve.
- Danh sach target da link voi brand.
- CTA `Refresh accounts` neu backend ho tro reload targets.
- CTA `Disconnect` chi hien thi khi backend co API disconnect.

## 7. Business rules

- Chi user da authenticated moi co the bat dau connect TikTok.
- Moi request social phai gui `Authorization` va `X-Profile-Id`.
- User chi duoc link TikTok target vao brand thuoc profile hien tai.
- OAuth `state` phai duoc backend tao, validate va rang buoc voi user/profile/provider.
- Frontend khong bao gio doc, hien thi hoac luu TikTok access token/refresh token.
- Neu provider TikTok chua enabled, FE phai hien thi disabled state thay vi goi API that bai lien tuc.
- Neu backend tra token expired/requires reconnect, FE phai hien thi reconnect thay vi cho publish.
- Neu TikTok chi cho phep sandbox/test account, FE phai hien thi thong diep loi ro rang nhung khong de lo chi tiet secret/config.

## 8. Data model frontend de xuat

```ts
type SocialProvider = "facebook" | "instagram" | "tiktok";

type SocialConnectionStatus =
  | "coming_soon"
  | "not_configured"
  | "not_connected"
  | "connecting"
  | "connected"
  | "needs_reconnect"
  | "permission_issue"
  | "error";

type SocialAccountSummary = {
  id: string;
  provider: SocialProvider;
  displayName: string;
  status: SocialConnectionStatus;
  avatarUrl?: string | null;
  connectedAt?: string | null;
  expiresAt?: string | null;
  requiresReconnect?: boolean;
};

type SocialTarget = {
  id: string;
  name: string;
  type: "facebook_page" | "instagram_business_account" | "tiktok_business_account";
  avatarUrl?: string | null;
  canPublish?: boolean;
};
```

## 9. Acceptance criteria

### AC1 - Hien thi TikTok Business provider

Given user da dang nhap va vao Social Connections  
When danh sach provider duoc render  
Then user thay card `TikTok Business` cung trang thai dung theo backend capability.

### AC2 - Provider chua duoc backend ho tro

Given backend chua enabled TikTok  
When user nhin card TikTok  
Then CTA connect bi disabled hoac hien `Coming soon`  
And FE khong bat dau OAuth flow.

### AC3 - Bat dau ket noi TikTok

Given backend da enabled TikTok va user co active profile  
When user bam `Connect`  
Then FE goi API lay auth URL voi bearer token va `X-Profile-Id`  
And redirect user toi URL do backend tra ve.

### AC4 - Xu ly callback thanh cong

Given TikTok redirect ve `/social-callback/tiktok?code=...&state=...`  
When FE goi callback API thanh cong  
Then FE hien thi ket noi thanh cong  
And reload danh sach social accounts  
And TikTok card chuyen sang `Connected`.

### AC5 - Xu ly callback loi OAuth

Given TikTok redirect ve callback voi `error`  
When FE doc query string  
Then FE khong goi callback API neu khong co `code`  
And hien thong bao loi phu hop  
And cho phep user quay lai Social Connections.

### AC6 - Hien thi reconnect khi token het han

Given backend tra TikTok account co `requiresReconnect = true`  
When user vao Social Connections  
Then TikTok card hien `Needs reconnect`  
And CTA chinh la `Reconnect`.

### AC7 - Chon va link TikTok Business target

Given account TikTok da ket noi va backend tra danh sach target  
When user chon target va brand  
Then FE goi API link target  
And hien target da link trong man hinh manage.

### AC8 - Bao ve token va secret

Given OAuth flow thanh cong  
When FE luu state ung dung  
Then FE chi luu metadata can thiet  
And khong luu access token/refresh token/client secret tren browser storage.

## 10. Error handling

| Truong hop | Xu ly frontend |
| --- | --- |
| `401 Unauthorized` | Dua user ve login hoac refresh token theo flow hien co |
| `403 Forbidden` | Hien thong bao user khong co quyen thao tac profile/brand |
| `404 Profile/Brand not found` | Yeu cau chon lai profile/brand |
| `409 Already connected` | Refresh danh sach social accounts va hien connected state |
| `422 Missing scope` | Hien permission issue va CTA reconnect |
| `501 Provider not supported` | Hien Coming soon / Backend chua enabled TikTok |
| Network timeout | Hien retry, khong redirect neu chua co auth URL |
| Invalid state | Hien loi callback het han va CTA thu lai |

## 11. Test cases frontend

- Render TikTok provider card trong Social Connections.
- Render dung disabled state khi capability TikTok la disabled.
- Bam Connect goi dung endpoint va redirect den `authUrl`.
- Callback thanh cong goi `POST /api/social-auth/tiktok/callback` voi `code`, `state`, `profileId`.
- Callback co `error` khong goi backend callback va hien loi.
- Backend tra `provider_not_supported` hien Coming soon.
- Backend tra `requiresReconnect` hien CTA Reconnect.
- Target selection dialog render TikTok Business target va submit dung payload.
- Khong co token/refresh token trong localStorage/sessionStorage sau connect.

## 12. Dependency va blocker

- Can backend implement TikTok provider theo `IProviderService`.
- Can backend expose TikTok auth start/callback endpoints.
- Can backend bo hard-code Facebook-only trong `SocialService`.
- Can cau hinh TikTok Business OAuth app, redirect URI va scopes.
- Can quyet dinh scope post-MVP:
  - chi connect va link account,
  - publish video/photo,
  - analytics,
  - hay TikTok Ads.
- Can backend capability endpoint hoac config response de FE biet provider nao enabled.

## 13. Definition of Done

- Co TikTok Business provider card tren UI social connection.
- FE co route callback TikTok va xu ly cac state thanh cong/that bai.
- FE service layer co ham connect/callback/list targets/link targets cho TikTok theo contract backend.
- UI khong luu token/secret TikTok tren browser.
- Da co test cho connect flow, callback flow, disabled provider va reconnect state.
- Neu backend chua implement TikTok, FE van hien thi trang thai Coming soon/Not configured ro rang va khong gay loi trai nghiem nguoi dung.
