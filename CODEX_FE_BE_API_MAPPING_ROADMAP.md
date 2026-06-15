# CODEX TASK BRIEF — AISAM FE ↔ BE API Mapping & Integration Roadmap

> **Agent đọc file:** Codex  
> **Mục tiêu:** Kết nối Frontend và Backend AISAM thành dự án hoàn chỉnh bằng cách chuẩn hóa API client, sửa URL sai, map endpoint FE↔BE, thay mock bằng API thật theo ưu tiên P0→P3.  
> **Phạm vi:** Toàn bộ mapping + roadmap hoàn chỉnh cho AISAM-FE và AISAM-BE.  
> **Ngày tạo:** 2026-06-15

---

## 0. Cách Codex cần dùng file này

Codex cần đọc file này như **source of truth** khi refactor kết nối API giữa FE và BE.

Thứ tự đọc khuyến nghị:

1. Đọc mục **1. Mục tiêu cuối cùng**.
2. Đọc mục **2. Quy tắc không được phá vỡ**.
3. Làm theo **3. P0 — Bug critical cần sửa ngay** trước.
4. Sau đó làm theo **4. P1**, **5. P2**, **6. P3**.
5. Dùng **7. Snippet code chuẩn hóa** để refactor `apiClient`, `apiTypes`, `useApi`.
6. Dùng **8. Danh sách file cần kiểm tra/sửa** để mở đúng file.
7. Dùng **9. Checklist endpoint FE↔BE** để tick từng endpoint.
8. Xem **Appendix A/B** để đối chiếu chi tiết từ tài liệu phân tích gốc.

---

## 1. Mục tiêu cuối cùng

Sau khi hoàn thành, dự án phải đạt các điểm sau:

- FE gọi đúng endpoint thật của BE, không gọi URL sai.
- Tất cả request cần profile/workspace context phải gửi đúng header tương ứng:
  - `X-Profile-Id` cho route đi qua `ActiveProfileMiddleware`;
  - `X-Workspace-Id` cho route đi qua `ActiveWorkspaceMiddleware`;
  - nhiều route hiện đi qua cả hai middleware nên `apiClient` cần gửi **cả hai header** khi store có đủ context.
- Không bypass `apiClient` bằng `fetch()` trực tiếp trừ trường hợp đặc biệt có lý do rõ ràng.
- `apiClient` xử lý được:
  - token access;
  - refresh token;
  - race condition khi nhiều request cùng 401;
  - JSON/non-JSON response;
  - `AbortController`;
  - multipart/form-data;
  - error message rõ ràng.
- Các generic type API được gom về một file chung.
- Mock data không trộn trực tiếp trong production service nếu BE đã có endpoint thật.
- Mỗi endpoint có trạng thái rõ ràng: đã nối thật, thiếu service, mock-only, hoặc BE chưa có.

---

## 2. Quy tắc không được phá vỡ

- **Không thêm dependency mới** như Axios, SWR, React Query nếu chưa cần. Ưu tiên native `fetch()` thông qua `apiClient`.
- Không đổi tên route BE nếu FE có thể sửa URL để khớp BE.
- Không xóa mock data ngay nếu UI đang phụ thuộc vào mock; hãy tách mock sang `src/mocks/*` hoặc giữ fallback có kiểm soát.
- Không dùng `catch {}` rỗng. Phải log hoặc throw `ApiError` có message rõ.
- Không sửa hàng loạt page UI nếu chỉ cần sửa service/hook.
- Không hardcode userId/profileId khi có thể lấy từ auth/profile/workspace store.
- Không để request cần auth đi ngoài `apiClient`.
- Không thay đổi cấu trúc response BE nếu chưa cần; FE nên normalize response.

---

## 3. P0 — Bug critical cần sửa ngay

### P0.1 Fix header profile/workspace context

**File cần sửa:**

```txt
AISAM-FE/src/lib/apiClient.ts
AISAM-FE/src/stores/workspace-store.ts
AISAM-FE/src/stores/profile-store.ts
```

**Vấn đề:** FE hiện chỉ gửi `X-Workspace-Id`. BE hiện chạy cả:

- `ActiveProfileMiddleware` yêu cầu `X-Profile-Id` cho `/api/content`, `/api/content-schedules`, `/api/dev/scheduler`, `/api/ai`, `/api/conversations`, `/api/social-auth`, `/api/social`, `/api/posts`, `/api/notifications`.
- `ActiveWorkspaceMiddleware` yêu cầu `X-Workspace-Id` cho `/api/ai`, `/api/brands`, `/api/content`, `/api/content-schedules`, `/api/dashboard`, `/api/products`, `/api/quota`, `/api/workspace-members`, `/api/workspace-invitations`, `/api/workspace-dashboard`, `/api/payment`, `/api/posts`, `/api/social`, `/api/social-auth`, `/api/conversations`, `/api/notifications`.

Vì vậy nhiều endpoint cần **đồng thời** `X-Profile-Id` và `X-Workspace-Id`.

**Action:**

- Không thay thế toàn bộ `X-Workspace-Id` bằng `X-Profile-Id`.
- Bổ sung `X-Profile-Id` từ `profile-store`.
- Tiếp tục gửi `X-Workspace-Id` từ `workspace-store`.
- Nếu profile/workspace chưa có trong store, không hardcode fallback GUID; để BE trả lỗi rõ ràng hoặc UI yêu cầu chọn context.
- Kiểm tra lại luồng chọn active profile/workspace để đảm bảo hai store lưu đúng entity, không dùng workspace id thay cho profile id.

**Expected result:** Các API content, ai, social, posts, notifications, payment, quota, dashboard, content-schedules, conversations không còn lỗi 401/403 do thiếu context header.

---

### P0.2 Fix URL sai FE → BE

| # | FE đang gọi sai | BE route đúng | File cần sửa | Action |
|---|------------------|---------------|--------------|--------|
| 1 | `/workspaces/members` | `/workspace-members` | `AISAM-FE/src/services/workspaceService.ts` | Sửa URL service member |
| 2 | `/workspaces/invitations` | `/workspace-invitations` | `AISAM-FE/src/services/workspaceInvitationService.ts` | Sửa URL invite |
| 3 | `/workspaces/invitations/accept` | `/workspace-invitations/accept` | `AISAM-FE/src/services/workspaceInvitationService.ts` | Sửa URL accept invite |
| 4 | `/quota/profile/{profileId}` | `/quota/workspace/current` | `AISAM-FE/src/services/workspaceService.ts` | Sửa `fetchPostQuota()` |
| 5 | `/workspaces/user/{userId}` | `/workspaces` GET mine | `AISAM-FE/src/hooks/useWorkspaces.ts` | Bỏ endpoint không tồn tại |
| 6 | `/credits/deduct` | Không tồn tại | `AISAM-FE/src/services/workspaceService.ts` | Không gọi API này; giữ mock/fallback hoặc map sang payment/quota nếu BE có logic tương ứng |
| 7 | `/credits/history` | Không tồn tại | `AISAM-FE/src/services/workspaceService.ts` | Không gọi API này; giữ mock/fallback hoặc tạo BE endpoint sau nếu cần |

**Lưu ý workspace invitations:** BE hiện chỉ có `POST /workspace-invitations` và `POST /workspace-invitations/accept`. Các hàm FE dạng `GET /workspaces/invitations/accept?token=...`, `GET /workspaces/invitations`, hoặc `DELETE /workspaces/invitations/{id}` không chỉ sai prefix mà còn chưa có endpoint tương ứng ở BE; phải giữ mock/TODO hoặc bổ sung BE endpoint trước khi nối thật.

---

### P0.3 Không bypass `apiClient`

**Files cần sửa:**

```txt
AISAM-FE/src/hooks/useWorkspaces.ts
AISAM-FE/src/hooks/useProfiles.ts
```

**Vấn đề:** 2 hook này dùng `fetch()` trực tiếp, bỏ qua token refresh, profile header, error handling.

**Action:**

- Replace direct `fetch()` bằng `apiClient<T>()`.
- Dùng `AbortController` nếu hook fetch khi mount.
- Không dùng module-level flag kiểu `fetchingWorkspaces` / `fetchingProfiles` để chặn request vì dễ gây race trong React Strict Mode.

---

### P0.4 Fix token refresh race condition

**File cần sửa:**

```txt
AISAM-FE/src/lib/apiClient.ts
AISAM-FE/src/lib/auth.ts
```

**Vấn đề:** Nếu nhiều request cùng nhận 401, FE có thể gọi refresh token nhiều lần song song.

**Action:** Dùng singleton `refreshPromise` để chỉ cho phép 1 refresh request chạy tại một thời điểm.

---

### P0.5 Fix notification service đang hardcode mock

**File cần sửa:**

```txt
AISAM-FE/src/services/notificationService.ts
```

**Vấn đề:** `useMockData = true` hardcode khiến notification không bao giờ gọi BE.

**Action:**

- Đổi thành flag theo env:

```ts
const USE_MOCK_NOTIFICATIONS = process.env.NEXT_PUBLIC_USE_MOCK_API === "true";
```

- Mặc định production phải gọi BE thật.

---

### P0.6 Fix payload `chatWithAI()` không khớp BE

**File cần sửa:**

```txt
AISAM-FE/src/services/contentService.ts
```

**Vấn đề:** FE gửi `{ message, history }`, nhưng BE `ChatRequest` cần các field:

```ts
{
  brandId?: string;
  productId?: string;
  adType?: string;
  message: string;
  conversationId?: string;
}
```

**Action:** Refactor `chatWithAI()` để truyền đúng payload theo BE.

---

## 4. P1 — Chuẩn hóa API layer và service quan trọng

### P1.1 Tạo shared API types

**File mới:**

```txt
AISAM-FE/src/lib/apiTypes.ts
```

**Mục tiêu:** Xóa duplicate `GenericResponse<T>` và `PagedResult<T>` trong nhiều service.

---

### P1.2 Hợp nhất `apiClient` và `apiFetch`

**File cần sửa:**

```txt
AISAM-FE/src/lib/apiClient.ts
AISAM-FE/src/services/contentService.ts
AISAM-FE/src/services/workspaceInvitationService.ts
```

**Action:**

- Chỉ export `apiClient` là client chính.
- Nếu cần raw/multipart thì thêm option `rawBody` hoặc `isFormData`.
- Không để `apiFetch` tồn tại song song nếu không có document rõ ràng.

---

### P1.3 Tạo hook dùng chung `useApi`

**File mới:**

```txt
AISAM-FE/src/hooks/useApi.ts
```

**Mục tiêu:** Dùng chung loading/error/refetch/abort cho các page/hook gọi API.

---

### P1.4 Bổ sung Auth service riêng

**File mới khuyến nghị:**

```txt
AISAM-FE/src/services/authService.ts
```

**Endpoint cần đưa vào service:**

| Endpoint | Method | Function đề xuất |
|---|---:|---|
| `/auth/register` | POST | `register()` |
| `/auth/login` | POST | `login()` |
| `/auth/me` | GET | `getCurrentUser()` |
| `/auth/logout` | POST | `logoutSession()` |
| `/auth/logout-all` | POST | `logoutAllSessions()` |
| `/auth/verify-email` | GET | `verifyEmail()` |
| `/auth/verify-email/resend` | POST | `resendEmailVerification()` |
| `/auth/forgot-password` | POST | `forgotPassword()` |
| `/auth/reset-password` | POST | `resetPassword()` |
| `/auth/google` | POST | `googleLogin()` |
| `/auth/sessions` | GET | `getActiveSessions()` |

---

### P1.5 Hoàn thiện Content + AI + Social

**Content missing:**

| Endpoint | Method | Function cần thêm |
|---|---:|---|
| `/content/{id}/clone` | POST | `cloneContent(id)` |
| `/content/{id}/publish/{integrationId}` | POST | `publishContent(id, integrationId)` |

**AI missing:**

| Endpoint | Method | Function cần thêm |
|---|---:|---|
| `/ai/improve/{contentId}` | POST | `improveContent(contentId, payload)` |
| `/ai/approve/{aiGenerationId}` | POST | `approveAIGeneration(aiGenerationId)` |
| `/ai/generations/{contentId}` | GET | `getAIGenerations(contentId)` |
| `/conversations` | GET | `getConversations(params)` |
| `/conversations/{id}` | GET | `getConversationById(id)` |
| `/conversations/{id}` | DELETE | `deleteConversation(id)` |

**Social cần chuyển từ mock sang API thật:**

| Endpoint | Method | Function cần nối thật |
|---|---:|---|
| `/social-auth/facebook` | GET | `getFacebookAuthUrl()` |
| `/social-auth/facebook/callback` | POST | `handleFacebookCallback()` |
| `/social/accounts/me` | GET | `fetchSocialAccounts()` |
| `/social/accounts/{id}/available-targets` | GET | `getAvailableTargets(id)` |
| `/social/accounts/{id}/linked-targets` | GET | `getLinkedTargets(id)` |
| `/social/accounts/{id}/link-targets` | POST | `linkTargets(id, payload)` |
| `/social/accounts/{id}` | DELETE | `deleteSocialAccount(id)` |
| `/social/integrations/{id}` | DELETE | `deleteIntegration(id)` |

---

## 5. P2 — CRUD còn thiếu nhưng không chặn core flow ngay

### P2.1 Profile CRUD

| Endpoint | Method | Function cần thêm |
|---|---:|---|
| `/profiles/user/{userId}` | POST multipart | `createProfile(userId, formData)` |
| `/profiles/{id}` | GET | `getProfileById(id)` |
| `/profiles/{id}` | PUT multipart | `updateProfile(id, formData)` |
| `/profiles/{id}` | DELETE | `deleteProfile(id)` |
| `/profiles/{id}/restore` | PATCH | `restoreProfile(id)` |

---

### P2.2 Workspace management

| Endpoint | Method | Function cần thêm |
|---|---:|---|
| `/workspaces/{id}` | GET | `getWorkspaceById(id)` |
| `/workspaces` | POST | `createWorkspace(payload)` |
| `/workspaces/{id}` | PUT | `updateWorkspace(id, payload)` |
| `/workspaces/{id}` | DELETE | `deleteWorkspace(id)` |
| `/workspace-members/{memberId}/role` | PUT | `updateWorkspaceMemberRole(memberId, payload)` |
| `/workspace-members/{memberId}/quota` | PUT | `updateWorkspaceMemberQuota(memberId, payload)` |
| `/workspace-members/{memberId}` | DELETE | `removeWorkspaceMember(memberId)` |
| `/workspace-members/ownership-transfer` | POST | `transferWorkspaceOwnership(payload)` |

---

### P2.3 Brand/Product CRUD

**Brand:**

| Endpoint | Method | Function cần thêm |
|---|---:|---|
| `/brands/{id}` | GET | đã có qua `resolveBrandName`, nên chuẩn hóa thành `getBrandById(id)` |
| `/brands` | POST | `createBrand(payload)` |
| `/brands/{id}` | PUT | `updateBrand(id, payload)` |
| `/brands/{id}` | DELETE | `deleteBrand(id)` |
| `/brands/{id}/restore` | POST | `restoreBrand(id)` |

**Product:**

| Endpoint | Method | Function cần thêm |
|---|---:|---|
| `/products/{id}` | GET | đã có qua `resolveProductName`, nên chuẩn hóa thành `getProductById(id)` |
| `/products` | POST multipart | `createProduct(formData)` |
| `/products/{id}` | PUT multipart | `updateProduct(id, formData)` |
| `/products/{id}` | DELETE | `deleteProduct(id)` |
| `/products/{id}/restore` | POST | `restoreProduct(id)` |

---

## 6. P3 — Module mock-only hoặc BE chưa có

Những module này chưa nên ép nối API thật nếu BE chưa có controller.

| Module | FE file | BE controller | Action |
|---|---|---|---|
| Team | `AISAM-FE/src/services/teamService.ts` | Chưa có | Giữ mock hoặc tạo BE sau |
| Analytics | `AISAM-FE/src/services/analyticsService.ts` | Chưa có | Giữ mock hoặc tạo BE sau |
| Campaigns | `AISAM-FE/src/services/campaignService.ts` | Chưa có | Giữ mock hoặc tạo BE sau |
| Posts create/update/delete/retry | `AISAM-FE/src/services/postService.ts` | BE chỉ có GET list/detail | Không gọi endpoint không tồn tại |

---

## 7. Snippet code chuẩn hóa

### 7.1 File mới: `AISAM-FE/src/lib/apiTypes.ts`

```ts
export interface ApiResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: {
    errorCode?: string;
    errorMessage?: string;
    validationErrors?: Record<string, string[]>;
  };
  timestamp?: string;
}

export interface PagedResult<T> {
  data?: T[];
  items?: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
}

export class ApiError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public errorCode?: string,
    public validationErrors?: Record<string, string[]>
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export function getPagedItems<T>(paged: PagedResult<T>): T[] {
  return paged.items ?? paged.data ?? [];
}
```

---

### 7.2 Refactor chuẩn: `AISAM-FE/src/lib/apiClient.ts`

```ts
import { getToken, refreshAccessToken } from "./auth";
import { ApiError } from "./apiTypes";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { getStoredActiveProfile } from "@/stores/profile-store";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

let refreshPromise: Promise<string | null> | null = null;

async function safeRefreshToken(): Promise<string | null> {
  if (refreshPromise) return refreshPromise;

  refreshPromise = refreshAccessToken().finally(() => {
    refreshPromise = null;
  });

  return refreshPromise;
}

function buildHeaders(customHeaders?: Record<string, string>) {
  const token = getToken();
  const activeProfile = getStoredActiveProfile?.();
  const activeWorkspace = getStoredActiveWorkspace?.();

  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(activeProfile?.id ? { "X-Profile-Id": activeProfile.id } : {}),
    ...(activeWorkspace?.id ? { "X-Workspace-Id": activeWorkspace.id } : {}),
    ...(customHeaders || {}),
  };

  return { headers, token };
}

async function handleResponse<T>(response: Response): Promise<T> {
  const text = await response.text();
  let result: any = null;

  try {
    result = text ? JSON.parse(text) : null;
  } catch {
    if (!response.ok) {
      throw new ApiError(
        `Server trả về lỗi ${response.status}: ${text.slice(0, 200)}`,
        response.status
      );
    }
    return null as T;
  }

  if (!response.ok) {
    throw new ApiError(
      result?.message ||
        result?.error?.errorMessage ||
        response.statusText ||
        "Đã có lỗi xảy ra",
      response.status,
      result?.error?.errorCode,
      result?.error?.validationErrors
    );
  }

  return result as T;
}

type ApiClientOptions = {
  method?: string;
  data?: unknown;
  headers?: Record<string, string>;
  signal?: AbortSignal;
  rawBody?: boolean;
};

export async function apiClient<T = unknown>(
  endpoint: string,
  options: ApiClientOptions = {}
): Promise<T> {
  const { data, headers: customHeaders, signal, rawBody, method } = options;
  const { headers, token } = buildHeaders(customHeaders);

  const isFormData = typeof FormData !== "undefined" && data instanceof FormData;
  const shouldStringify = data !== undefined && !rawBody && !isFormData;

  const config: RequestInit = {
    method: method || (data !== undefined ? "POST" : "GET"),
    body: rawBody || isFormData ? (data as BodyInit) : shouldStringify ? JSON.stringify(data) : undefined,
    headers: {
      ...(shouldStringify ? { "Content-Type": "application/json" } : {}),
      ...headers,
    },
    signal,
  };

  const response = await fetch(`${API_URL}${endpoint}`, config);

  if (response.status === 401 && token) {
    const newToken = await safeRefreshToken();
    if (!newToken) {
      throw new ApiError("Phiên đăng nhập hết hạn", 401);
    }

    const retryResponse = await fetch(`${API_URL}${endpoint}`, {
      ...config,
      headers: {
        ...(config.headers as Record<string, string>),
        Authorization: `Bearer ${newToken}`,
      },
    });

    return handleResponse<T>(retryResponse);
  }

  return handleResponse<T>(response);
}
```

> Ghi chú cho Codex: không dùng workspace id làm fallback cho profile id. `X-Profile-Id` và `X-Workspace-Id` là hai context khác nhau; chỉ gửi header nào khi store tương ứng có dữ liệu thật.

---

### 7.3 File mới: `AISAM-FE/src/hooks/useApi.ts`

```ts
"use client";

import { useCallback, useEffect, useRef, useState } from "react";

interface UseApiOptions<T> {
  immediate?: boolean;
  fallback?: T;
  deps?: unknown[];
}

interface UseApiReturn<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function useApi<T>(
  fetcher: (signal: AbortSignal) => Promise<T>,
  options: UseApiOptions<T> = {}
): UseApiReturn<T> {
  const { immediate = true, fallback, deps = [] } = options;

  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(immediate);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const execute = useCallback(async () => {
    abortRef.current?.abort();

    const controller = new AbortController();
    abortRef.current = controller;

    setLoading(true);
    setError(null);

    try {
      const result = await fetcher(controller.signal);
      if (!controller.signal.aborted) {
        setData(result);
      }
    } catch (err: unknown) {
      if (err instanceof DOMException && err.name === "AbortError") return;

      const message = err instanceof Error ? err.message : "Đã có lỗi xảy ra";

      if (!controller.signal.aborted) {
        setError(message);
        if (fallback !== undefined) setData(fallback);
      }
    } finally {
      if (!controller.signal.aborted) {
        setLoading(false);
      }
    }
  }, deps);

  useEffect(() => {
    if (immediate) execute();
    return () => abortRef.current?.abort();
  }, [execute, immediate]);

  return { data, loading, error, refetch: execute };
}
```

---

### 7.4 Ví dụ refactor service dùng `ApiResponse`

```ts
import { apiClient } from "@/lib/apiClient";
import type { ApiResponse, PagedResult } from "@/lib/apiTypes";

export async function fetchContents(params?: FetchContentParams, signal?: AbortSignal) {
  const query = buildContentQuery(params);

  const res = await apiClient<ApiResponse<PagedResult<ContentApiItem>>>(
    `/content?${query}`,
    { signal }
  );

  if (!res.data) {
    throw new Error(res.message || "Không có dữ liệu content");
  }

  return res.data;
}
```

---

## 8. Danh sách file cần kiểm tra/sửa

### 8.1 FE core

```txt
AISAM-FE/src/lib/apiClient.ts
AISAM-FE/src/lib/auth.ts
AISAM-FE/src/lib/apiTypes.ts              [NEW]
AISAM-FE/src/hooks/useApi.ts              [NEW]
AISAM-FE/src/stores/workspace-store.ts
AISAM-FE/src/stores/profile-store.ts
AISAM-FE/.env.local
AISAM-FE/package.json
```

### 8.2 FE hooks

```txt
AISAM-FE/src/hooks/useWorkspaces.ts
AISAM-FE/src/hooks/useProfiles.ts
AISAM-FE/src/hooks/useFeatureGate.ts
```

### 8.3 FE services

```txt
AISAM-FE/src/services/authService.ts       [NEW]
AISAM-FE/src/services/postService.ts
AISAM-FE/src/services/campaignService.ts
AISAM-FE/src/services/contentService.ts
AISAM-FE/src/services/scheduleService.ts
AISAM-FE/src/services/socialAccountService.ts
AISAM-FE/src/services/workspaceService.ts
AISAM-FE/src/services/teamService.ts
AISAM-FE/src/services/analyticsService.ts
AISAM-FE/src/services/brandService.ts
AISAM-FE/src/services/notificationService.ts
AISAM-FE/src/services/paymentService.ts
AISAM-FE/src/services/profileSettingsService.ts
AISAM-FE/src/services/workspaceInvitationService.ts
```

### 8.4 FE mocks nên tách riêng

```txt
AISAM-FE/src/mocks/index.ts                [NEW]
AISAM-FE/src/mocks/contentMocks.ts         [NEW]
AISAM-FE/src/mocks/postMocks.ts            [NEW]
AISAM-FE/src/mocks/scheduleMocks.ts        [NEW]
AISAM-FE/src/mocks/socialMocks.ts          [NEW]
AISAM-FE/src/mocks/notificationMocks.ts    [NEW]
AISAM-FE/src/mocks/workspaceMocks.ts       [NEW]
```

### 8.5 BE controllers cần đối chiếu

```txt
AISAM-BE/AISAM.API/Controllers/AuthController.cs
AISAM-BE/AISAM.API/Controllers/ProfileController.cs
AISAM-BE/AISAM.API/Controllers/WorkspaceController.cs
AISAM-BE/AISAM.API/Controllers/WorkspaceMemberController.cs
AISAM-BE/AISAM.API/Controllers/WorkspaceInvitationController.cs
AISAM-BE/AISAM.API/Controllers/BrandController.cs
AISAM-BE/AISAM.API/Controllers/ProductController.cs
AISAM-BE/AISAM.API/Controllers/ContentController.cs
AISAM-BE/AISAM.API/Controllers/GeminiController.cs
AISAM-BE/AISAM.API/Controllers/ConversationController.cs
AISAM-BE/AISAM.API/Controllers/SocialAuthController.cs
AISAM-BE/AISAM.API/Controllers/SocialAccountsController.cs
AISAM-BE/AISAM.API/Controllers/SocialIntegrationController.cs
AISAM-BE/AISAM.API/Controllers/PostsController.cs
AISAM-BE/AISAM.API/Controllers/ContentSchedulesController.cs
AISAM-BE/AISAM.API/Controllers/NotificationsController.cs
AISAM-BE/AISAM.API/Controllers/DashboardController.cs
AISAM-BE/AISAM.API/Controllers/WorkspaceDashboardController.cs
AISAM-BE/AISAM.API/Controllers/PaymentController.cs
AISAM-BE/AISAM.API/Controllers/QuotaController.cs
```

### 8.6 Markdown/API docs cần đọc

```txt
API_PAYLOAD_REFERENCE.md
CODEX_FE_BE_API_MAPPING_ROADMAP.md         [THIS FILE]
```

---

## 9. Checklist endpoint FE↔BE

### Legend

| Icon | Ý nghĩa |
|---|---|
| ✅ | FE gọi API thật |
| ⚠️ | FE có hàm nhưng mock-only |
| ❌ | BE có endpoint nhưng FE thiếu service |
| 🔀 | FE đang dùng `fetch()` trực tiếp/bypass `apiClient` |
| 🔲 | FE/BE đều chưa implement hoặc module chưa migrate |

---

### 9.1 AUTH

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/auth/register` | POST | ❌ inline | Tạo `authService.register()` |
| [ ] | `/auth/login` | POST | ❌ inline | Tạo `authService.login()` |
| [ ] | `/auth/refresh` | POST | ✅ | Giữ trong `auth.ts`, bảo đảm refresh race safe |
| [ ] | `/auth/me` | GET | ❌ | Tạo `getCurrentUser()` |
| [ ] | `/auth/logout` | POST | ✅ | Chuẩn hóa vào service nếu cần |
| [ ] | `/auth/logout-all` | POST | ❌ | Tạo `logoutAllSessions()` |
| [ ] | `/auth/verify-email` | GET | ❌ | Tạo `verifyEmail()` |
| [ ] | `/auth/verify-email/resend` | POST | ❌ | Tạo `resendEmailVerification()` |
| [ ] | `/auth/forgot-password` | POST | ❌ | Tạo `forgotPassword()` |
| [ ] | `/auth/reset-password` | POST | ❌ | Tạo `resetPassword()` |
| [ ] | `/auth/change-password` | POST | ✅ | Chuẩn hóa response/error |
| [ ] | `/auth/change-password-with-token` | POST | ❌ | Tạo service nếu UI cần |
| [ ] | `/auth/google` | POST | ❌ inline | Tạo `googleLogin()` |
| [ ] | `/auth/sessions` | GET | ❌ | Tạo `getActiveSessions()` |

---

### 9.2 PROFILE

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/profiles/user/{userId}` | GET | 🔀 | Đổi từ direct fetch sang `apiClient` |
| [ ] | `/profiles/user/{userId}` | POST multipart | ❌ | Tạo `createProfile()` |
| [ ] | `/profiles/{id}` | GET | ❌ | Tạo `getProfileById()` |
| [ ] | `/profiles/{id}` | PUT multipart | ❌ | Tạo `updateProfile()` |
| [ ] | `/profiles/{id}` | DELETE | ❌ | Tạo `deleteProfile()` |
| [ ] | `/profiles/{id}/restore` | PATCH | ❌ | Tạo `restoreProfile()` |

---

### 9.3 WORKSPACE

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/workspaces` | GET | ✅ | Giữ, chuẩn hóa type |
| [ ] | `/workspaces/{id}` | GET | ❌ | Tạo `getWorkspaceById()` |
| [ ] | `/workspaces` | POST | ❌ | Tạo `createWorkspace()` |
| [ ] | `/workspaces/{id}` | PUT | ❌ | Tạo `updateWorkspace()` |
| [ ] | `/workspaces/{id}` | DELETE | ❌ | Tạo `deleteWorkspace()` nếu admin UI cần |
| [ ] | `/workspaces/user/{userId}` | GET | URL sai | Xóa/sửa thành `/workspaces` |

---

### 9.4 WORKSPACE MEMBERS

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/workspace-members` | GET | ✅ nhưng URL có chỗ sai | Đảm bảo service không gọi `/workspaces/members` |
| [ ] | `/workspace-members/{memberId}/role` | PUT | ❌ | Tạo `updateWorkspaceMemberRole()` |
| [ ] | `/workspace-members/{memberId}/quota` | PUT | ❌ | Tạo `updateWorkspaceMemberQuota()` |
| [ ] | `/workspace-members/{memberId}` | DELETE | ❌ | Tạo `removeWorkspaceMember()` |
| [ ] | `/workspace-members/ownership-transfer` | POST | ❌ | Tạo `transferOwnership()` |

---

### 9.5 WORKSPACE INVITATIONS

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/workspace-invitations` | POST | ✅ nhưng URL có chỗ sai | Sửa từ `/workspaces/invitations` |
| [ ] | `/workspace-invitations/accept` | POST | ✅ nhưng URL có chỗ sai | Sửa từ `/workspaces/invitations/accept` |
| [ ] | token/detail/cancel/list invitation | varies | URL sai/không rõ BE | Đối chiếu controller trước khi nối |

---

### 9.6 BRAND KIT

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/brands` | GET | ✅ | Chuẩn hóa type |
| [ ] | `/brands/{id}` | GET | ✅ qua helper | Tạo service rõ `getBrandById()` |
| [ ] | `/brands` | POST | ❌ | Tạo `createBrand()` |
| [ ] | `/brands/{id}` | PUT | ❌ | Tạo `updateBrand()` |
| [ ] | `/brands/{id}` | DELETE | ❌ | Tạo `deleteBrand()` |
| [ ] | `/brands/{id}/restore` | POST | ❌ | Tạo `restoreBrand()` |

---

### 9.7 PRODUCT

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/products` | GET | ✅ | Chuẩn hóa type |
| [ ] | `/products/{id}` | GET | ✅ qua helper | Tạo service rõ `getProductById()` |
| [ ] | `/products` | POST multipart | ❌ | Tạo `createProduct()` |
| [ ] | `/products/{id}` | PUT multipart | ❌ | Tạo `updateProduct()` |
| [ ] | `/products/{id}` | DELETE | ❌ | Tạo `deleteProduct()` |
| [ ] | `/products/{id}/restore` | POST | ❌ | Tạo `restoreProduct()` |

---

### 9.8 CONTENT HUB

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/content` | GET | ✅ | Chuẩn hóa type/error |
| [ ] | `/content/{id}` | GET | ✅ | Chuẩn hóa type/error |
| [ ] | `/content` | POST | ✅ | Chuẩn hóa type/error |
| [ ] | `/content/{id}` | PUT | ✅ | Đảm bảo method PUT rõ ràng |
| [ ] | `/content/{id}/clone` | POST | ❌ | Tạo `cloneContent()` |
| [ ] | `/content/{id}/publish/{integrationId}` | POST | ❌ | Tạo `publishContent()` |
| [ ] | `/content/{id}` | DELETE | ✅ | Dùng `apiClient` thay `apiFetch` |
| [ ] | `/content/{id}/restore` | POST | ✅ | Chuẩn hóa |

---

### 9.9 AI + CONVERSATIONS

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/ai/generate-draft` | POST | ✅ | Chuẩn hóa response/error |
| [ ] | `/ai/improve/{contentId}` | POST | ❌ | Tạo service |
| [ ] | `/ai/approve/{aiGenerationId}` | POST | ❌ | Tạo service |
| [ ] | `/ai/generations/{contentId}` | GET | ❌ | Tạo service |
| [ ] | `/ai/chat` | POST | ✅ nhưng payload sai | Sửa payload `{ brandId, productId, adType, message, conversationId }` |
| [ ] | `/conversations` | GET | ❌ | Tạo service |
| [ ] | `/conversations/{id}` | GET | ❌ | Tạo service |
| [ ] | `/conversations/{id}` | DELETE | ❌ | Tạo service |

---

### 9.10 SOCIAL

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/social-auth/facebook` | GET | ⚠️ mock-only | Gọi API thật |
| [ ] | `/social-auth/facebook/callback` | POST | ⚠️ mock-only | Gọi API thật |
| [ ] | `/social/accounts/me` | GET | ⚠️ mock-only | Gọi API thật |
| [ ] | `/social/accounts/{id}/available-targets` | GET | ⚠️ mock-only | Gọi API thật |
| [ ] | `/social/accounts/{id}/linked-targets` | GET | ⚠️ mock-only | Gọi API thật |
| [ ] | `/social/accounts/{id}/link-targets` | POST | ⚠️ mock-only | Gọi API thật |
| [ ] | `/social/accounts/{id}` | DELETE | ⚠️ mock-only | Gọi API thật |
| [ ] | `/social/integrations/{id}` | DELETE | ❌ | Tạo service |
| [ ] | `/social/integrations/brand/{brandId}` | GET | ✅ | Chuẩn hóa |

---

### 9.11 POSTS

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/posts` | GET | ✅ | Chuẩn hóa |
| [ ] | `/posts/{id}` | GET | ❌ | Tạo `getPostById()` |
| [ ] | create/update/delete/retry post | varies | ⚠️ mock-only, BE không có endpoint | Không gọi API không tồn tại; giữ mock hoặc tạo BE sau |

---

### 9.12 SCHEDULES

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/content-schedules` | GET | ✅ | Chuẩn hóa |
| [ ] | `/content-schedules/upcoming` | GET | ✅ | Chuẩn hóa |
| [ ] | `/content-schedules/{id}` | GET | ✅ | Chuẩn hóa |
| [ ] | `/content-schedules` | POST | ✅ | Chuẩn hóa |
| [ ] | `/content-schedules/{id}` | PUT | ✅ | Chuẩn hóa |
| [ ] | `/content-schedules/{id}` | DELETE | ✅ | Chuẩn hóa |

---

### 9.13 NOTIFICATIONS

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/notifications` | GET | ⚠️ hardcode mock | Bỏ `useMockData=true` |
| [ ] | `/notifications/{id}` | GET | ⚠️ hardcode mock | Gọi API thật |
| [ ] | `/notifications/{id}/mark-read` | POST | ⚠️ hardcode mock | Gọi API thật |
| [ ] | `/notifications/mark-all-read` | POST | ⚠️ hardcode mock | Gọi API thật |
| [ ] | `/notifications/unread-count` | GET | ⚠️ hardcode mock | Gọi API thật |
| [ ] | delete notification | DELETE | ⚠️ BE không có endpoint | Không gọi API không tồn tại |

---

### 9.14 DASHBOARD

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/dashboard/summary` | GET | ✅ | Chuẩn hóa |
| [ ] | `/workspace-dashboard/summary` | GET | ❌ | Tạo service nếu UI workspace dashboard cần |

---

### 9.15 PAYMENT & QUOTA

| Done | Endpoint | Method | FE status | Action |
|---|---|---:|---|---|
| [ ] | `/payment/checkout` | POST | ✅ | Chuẩn hóa |
| [ ] | `/payment/callback` | POST | PayOS redirect | Không cần FE gọi trực tiếp |
| [ ] | `/payment/webhook` | POST | PayOS webhook | Không cần FE gọi trực tiếp |
| [ ] | `/payment/history` | GET | ✅ | Chuẩn hóa |
| [ ] | `/payment/subscription/current` | GET | ✅ | Chuẩn hóa |
| [ ] | `/quota/workspace/current` | GET | FE gọi URL sai | Sửa từ `/quota/profile/{profileId}` |

---

## 10. Definition of Done

Codex chỉ coi task hoàn thành khi:

- [ ] `apiClient.ts` gửi đúng `X-Profile-Id` từ `profile-store`.
- [ ] `apiClient.ts` vẫn gửi đúng `X-Workspace-Id` từ `workspace-store` cho route cần workspace context.
- [ ] Không dùng workspace id làm fallback cho profile id hoặc ngược lại.
- [ ] `apiClient.ts` có singleton refresh token promise.
- [ ] `apiClient.ts` parse được JSON và non-JSON error.
- [ ] `apiClient.ts` hỗ trợ `AbortSignal`.
- [ ] `apiClient.ts` hỗ trợ `FormData` không set sai `Content-Type`.
- [ ] Không còn direct `fetch()` trong `useWorkspaces.ts` và `useProfiles.ts`.
- [ ] Các URL sai trong bảng P0.2 đã sửa.
- [ ] `notificationService.ts` không còn hardcode `useMockData = true`.
- [ ] `chatWithAI()` gửi đúng payload BE.
- [ ] `GenericResponse<T>` và `PagedResult<T>` duplicate được thay bằng `ApiResponse<T>` và `PagedResult<T>` từ `apiTypes.ts`.
- [ ] Các endpoint thiếu P1 có service hoặc TODO rõ ràng.
- [ ] Không còn gọi endpoint BE không tồn tại trừ mock fallback có kiểm soát.
- [ ] Build/lint không lỗi TypeScript do import/type mới.
- [ ] BE integration tests liên quan auth/context/workspace/profile vẫn pass hoặc lỗi được ghi rõ.

---

## 11. Lệnh kiểm tra sau khi sửa

Chạy từ thư mục FE:

```bash
cd AISAM-FE
npm install
npm run lint
npm run build
npm run dev
```

Nếu BE cần chạy cùng:

```bash
cd AISAM-BE
# Chạy theo script hiện có của solution .NET trong repo
dotnet test AISAM.sln
```

Manual test tối thiểu:

- Login thành công.
- Refresh page không mất session.
- Chọn active profile/workspace.
- Gọi được content list.
- Gọi được schedule list.
- Gọi được notifications thật nếu BE đang chạy.
- Gọi quota dùng `/quota/workspace/current`.
- Request tới content/ai/social/posts/notifications gửi cả `X-Profile-Id` và `X-Workspace-Id` khi đã chọn đủ context.
- Request tới quota/payment/dashboard/workspace members gửi `X-Workspace-Id`.
- Không còn request tới `/workspaces/user/{userId}`, `/workspaces/members`, `/workspaces/invitations`, `/quota/profile/{profileId}`.
- Không nối thật các hàm invitation GET/DELETE nếu BE chưa có endpoint tương ứng.

---

## 12. Appendix A — Tài liệu phân tích API Fetching gốc

# 📊 Phân tích API Fetching — Dự án AISAM

## 📁 Tổng quan File đã đọc

| Nhóm | Files | Mô tả |
|------|-------|-------|
| **Core** | [apiClient.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/lib/apiClient.ts), [auth.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/lib/auth.ts) | HTTP client + Auth token management |
| **Services (13)** | postService, campaignService, contentService, scheduleService, socialAccountService, workspaceService, teamService, analyticsService, brandService, notificationService, paymentService, profileSettingsService, workspaceInvitationService | Tất cả service tương tác API |
| **Hooks (3)** | [useWorkspaces.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useWorkspaces.ts), [useProfiles.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useProfiles.ts), [useFeatureGate.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useFeatureGate.ts) | State management hooks |
| **Stores (2)** | [workspace-store.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/stores/workspace-store.ts), [profile-store.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/stores/profile-store.ts) | localStorage persistence |
| **Config** | [package.json](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/package.json), [.env.local](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/.env.local), [API_PAYLOAD_REFERENCE.md](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/API_PAYLOAD_REFERENCE.md) | Dependencies + API docs |

---

## 🔴 12 Vấn đề nghiêm trọng tìm thấy

### 1. ❌ THIẾU HEADER CONTEXT ĐẦY ĐỦ — **BUG CRITICAL**

> [!CAUTION]
> Đính chính sau khi kiểm tra repo hiện tại: Backend yêu cầu `X-Profile-Id` ở `ActiveProfileMiddleware` và vẫn yêu cầu `X-Workspace-Id` ở `ActiveWorkspaceMiddleware`. `apiClient.ts` hiện chỉ gửi `X-Workspace-Id`, nên thiếu profile context; không được đổi workspace header thành profile header.

```diff
// apiClient.ts hiện tại (dòng 14-17)
  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
-   ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
+   ...(profile?.id ? { "X-Profile-Id": profile.id } : {}),
+   ...(workspace?.id ? { "X-Workspace-Id": workspace.id } : {}),
    ...(customHeaders || {}),
  };
```

**Hậu quả:** Các API đi qua `ActiveProfileMiddleware` bị `401/403` vì thiếu `X-Profile-Id`; nếu bỏ `X-Workspace-Id` thì các API đi qua `ActiveWorkspaceMiddleware` cũng sẽ lỗi.

---

### 2. ❌ `GenericResponse<T>` bị DUPLICATE 7 lần

Cùng một interface `GenericResponse<T>` được khai báo lại ở **7 files khác nhau**:

| File | Dòng |
|------|------|
| [postService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/postService.ts#L28-L35) | 28-35 |
| [contentService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L6-L13) | 6-13 |
| [scheduleService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/scheduleService.ts#L23-L30) | 23-30 |
| [socialAccountService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L57-L64) | 57-64 |
| [workspaceService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts#L4-L11) | 4-11 |
| [notificationService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/notificationService.ts#L3-L10) | 3-10 |
| [paymentService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/paymentService.ts#L3-L8) | 3-8 |

Tương tự, `PagedResult<T>` bị duplicate ở 3 files.

---

### 3. ❌ Cách dùng `apiClient` không nhất quán — 3 pattern khác nhau

| Pattern | File ví dụ | Cách dùng |
|---------|-----------|-----------|
| **A: Dùng `apiClient`** | contentService, scheduleService, brandService, notificationService, paymentService | `apiClient("/endpoint")` |
| **B: Dùng `fetch` trực tiếp** | [useWorkspaces.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useWorkspaces.ts#L120-L121), [useProfiles.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useProfiles.ts#L95-L96) | `fetch(\`${API_URL}/profiles/...\`)` — **bypass apiClient hoàn toàn!** |
| **C: Hỗn hợp** | socialAccountService, contentService | Vừa dùng `apiClient` vừa dùng `apiFetch` |

> [!WARNING]
> `useWorkspaces.ts` và `useProfiles.ts` gọi `fetch()` trực tiếp, bỏ qua **token refresh**, **workspace header**, và **error handling** của `apiClient`. Điều này sẽ gây lỗi khi token hết hạn.

---

### 4. ❌ Mock data trộn lẫn Production code

| Service | Mock Strategy | Vấn đề |
|---------|---------------|--------|
| `campaignService` | 100% mock, **KHÔNG gọi API** | Không có API call nào |
| `teamService` | 100% mock, **KHÔNG gọi API** | Không có API call nào |
| `analyticsService` | 100% mock, **KHÔNG gọi API** | Không có API call nào |
| `socialAccountService` | Hỗn hợp: `fetchSocialIntegrations` gọi API, còn lại 100% mock | Không đồng nhất |
| `notificationService` | Có biến `useMockData = true` hardcode | Sẽ **KHÔNG BAO GIỜ** gọi API thật |
| `postService` | try-catch fallback mock | OK nhưng mock data nặng ~16KB |
| `contentService` | try-catch fallback mock | OK |
| `scheduleService` | try-catch fallback mock + localStorage sync | Phức tạp không cần thiết |

---

### 5. ❌ Hai hàm `apiClient` vs `apiFetch` gây nhầm lẫn

```typescript
// apiClient: Tự thêm Content-Type, method mặc định POST nếu có data
export async function apiClient(endpoint, options) { ... }

// apiFetch: KHÔNG tự thêm Content-Type, KHÔNG set method
export async function apiFetch(endpoint, options) { ... }
```

Cả hai tồn tại song song mà **không có document** giải thích khi nào dùng cái nào. Chỉ 2 chỗ dùng `apiFetch`: [contentService.ts L278](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L278) và [workspaceInvitationService.ts L144](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceInvitationService.ts#L144).

---

### 6. ❌ `apiClient` mặc định method POST khi có `data`

```typescript
// apiClient.ts dòng 49
method: data ? "POST" : "GET",
```

Khi gọi `updateContent` (cần PUT), phải truyền thêm `method: "PUT"` nhưng dễ quên:

```typescript
// contentService.ts dòng 237-239 — phải nhớ truyền method
await apiClient(`/content/${id}`, { data, method: "PUT" })
```

---

### 7. ❌ Error swallowing — `catch {}` trống

Hầu hết mọi service đều có pattern:
```typescript
try {
  const res = await apiClient(...);
  // process response
} catch { /* fallback */ }
```

**Không log error**, không notify user, không phân biệt network error vs 400 vs 500.

---

### 8. ❌ Race condition ở `useWorkspaces` và `useProfiles`

```typescript
// useWorkspaces.ts dòng 115-116
if (fetchingWorkspaces) return;  // ← Nếu component mount 2 lần, lần 2 return empty
fetchingWorkspaces = true;
```

Dùng module-level flag `fetchingWorkspaces` thay vì proper deduplication → khi Strict Mode mount/unmount, request thứ 2 bị bỏ qua hoàn toàn.

---

### 9. ❌ `handleResponse` không xử lý non-JSON responses

```typescript
// apiClient.ts dòng 23
const result = await response.json().catch(() => null);
```

Nếu server trả về HTML (502 nginx error page) hoặc empty body, `result` = `null` → không có error message hữu ích.

---

### 10. ❌ Không có request cancellation (AbortController)

Khi user navigate giữa các trang, các request cũ vẫn tiếp tục chạy → memory leak + state update trên unmounted component.

---

### 11. ❌ Token refresh race condition

```typescript
// apiClient.ts dòng 31-41 - retryWithRefresh
async function retryWithRefresh(endpoint, config) {
  const newToken = await refreshAccessToken();
  // ...
}
```

Nếu 5 API calls cùng nhận 401 → gọi `refreshAccessToken()` 5 lần song song → 4 lần thất bại vì refresh token đã bị rotate.

---

### 12. ❌ Dependencies thiếu: Không có state management library

```json
// package.json dependencies
{
  "motion": "^12.40.0",
  "next": "16.2.7",
  "react": "19.2.4",
  "react-dom": "19.2.4"
}
```

Không có bất kỳ data fetching library nào (SWR, React Query, Axios). Tất cả đều dùng raw `fetch()`.

---

## ✅ Đề xuất phương án Fetch API tối ưu

### Phương án: **Chuẩn hóa apiClient + Custom Hooks (không thêm dependency mới)**

> [!IMPORTANT]
> Tuân thủ nguyên tắc coding: **KHÔNG thêm library mới không cần thiết**, ưu tiên tái sử dụng code hiện có.

Dựa trên stack hiện tại (Next.js 16 + React 19, không có SWR/React Query), phương án phù hợp nhất là **chuẩn hóa apiClient hiện có** thay vì thêm library mới.

---

### Bước 1: Tạo file shared types — `src/lib/apiTypes.ts` [NEW]

```typescript
// Tập trung tất cả generic API types vào 1 file duy nhất
export interface ApiResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: {
    errorCode?: string;
    errorMessage?: string;
    validationErrors?: Record<string, string[]>;
  };
  timestamp?: string;
}

export interface PagedResult<T> {
  data: T[];         // Lưu ý: BE có 2 format - "data" hoặc "items"
  items?: T[];       // Một số endpoint dùng "items" thay vì "data"
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
}

export class ApiError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public errorCode?: string,
    public validationErrors?: Record<string, string[]>
  ) {
    super(message);
    this.name = "ApiError";
  }
}
```

---

### Bước 2: Fix `apiClient.ts` — Sửa 6 vấn đề cùng lúc

```typescript
import { getToken, refreshAccessToken } from "./auth";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { getStoredActiveProfile } from "@/stores/profile-store";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

// ── Singleton refresh promise (fix race condition #11) ──
let refreshPromise: Promise<string | null> | null = null;

async function safeRefreshToken(): Promise<string | null> {
  if (refreshPromise) return refreshPromise;
  refreshPromise = refreshAccessToken().finally(() => {
    refreshPromise = null;
  });
  return refreshPromise;
}

// ── Build headers (fix #1: profile + workspace context) ──
function buildHeaders(customHeaders?: Record<string, string>) {
  const token = getToken();
  const workspace = getStoredActiveWorkspace();
  const profile = getStoredActiveProfile();
  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(profile?.id ? { "X-Profile-Id": profile.id } : {}),
    ...(workspace?.id ? { "X-Workspace-Id": workspace.id } : {}),
    ...(customHeaders || {}),
  };
  return { headers, token };
}

// ── Handle response (fix #9: better error handling) ──
async function handleResponse<T>(response: Response): Promise<T> {
  const text = await response.text();
  let result: any = null;
  
  try {
    result = text ? JSON.parse(text) : null;
  } catch {
    if (!response.ok) {
      throw new ApiError(
        `Server trả về lỗi ${response.status}: ${text.slice(0, 200)}`,
        response.status
      );
    }
    return null as T;
  }
  
  if (!response.ok) {
    throw new ApiError(
      result?.message || result?.error?.errorMessage || response.statusText || "Đã có lỗi xảy ra",
      response.status,
      result?.error?.errorCode,
      result?.error?.validationErrors
    );
  }
  
  return result;
}

// ── Main API function (fix #5: hợp nhất apiClient + apiFetch) ──
export async function apiClient<T = any>(
  endpoint: string,
  options: {
    method?: string;
    data?: any;
    headers?: Record<string, string>;
    signal?: AbortSignal;     // fix #10: support cancellation
    rawBody?: boolean;        // cho multipart/form-data
  } = {}
): Promise<T> {
  const { data, headers: customHeaders, signal, rawBody, ...rest } = options;
  const { headers, token } = buildHeaders(customHeaders);

  const config: RequestInit = {
    method: rest.method || (data ? "POST" : "GET"),
    body: rawBody ? data : data ? JSON.stringify(data) : undefined,
    headers: {
      ...(rawBody ? {} : { "Content-Type": "application/json" }),
      ...headers,
    },
    signal,
    ...rest,
  };

  const response = await fetch(`${API_URL}${endpoint}`, config);

  // Auto-retry on 401 (fix #11: singleton refresh)
  if (response.status === 401 && token) {
    const newToken = await safeRefreshToken();
    if (!newToken) throw new ApiError("Phiên đăng nhập hết hạn", 401);

    const retryHeaders = {
      ...(config.headers as Record<string, string>),
      Authorization: `Bearer ${newToken}`,
    };
    const retryResponse = await fetch(`${API_URL}${endpoint}`, {
      ...config,
      headers: retryHeaders,
    });
    return handleResponse<T>(retryResponse);
  }

  return handleResponse<T>(response);
}
```

---

### Bước 3: Tạo custom hook `useApi` — `src/hooks/useApi.ts` [NEW]

```typescript
"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { apiClient } from "@/lib/apiClient";

interface UseApiOptions<T> {
  /** Gọi API ngay khi mount */
  immediate?: boolean;
  /** Fallback data khi API lỗi */
  fallback?: T;
  /** Dependencies để re-fetch */
  deps?: any[];
}

interface UseApiReturn<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function useApi<T>(
  fetcher: (signal: AbortSignal) => Promise<T>,
  options: UseApiOptions<T> = {}
): UseApiReturn<T> {
  const { immediate = true, fallback, deps = [] } = options;
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(immediate);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const execute = useCallback(async () => {
    // Cancel previous request (fix #10)
    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setLoading(true);
    setError(null);

    try {
      const result = await fetcher(controller.signal);
      if (!controller.signal.aborted) {
        setData(result);
      }
    } catch (err: any) {
      if (err.name === "AbortError") return;
      const message = err.message || "Đã có lỗi xảy ra";
      if (!controller.signal.aborted) {
        setError(message);
        if (fallback !== undefined) setData(fallback);
      }
    } finally {
      if (!controller.signal.aborted) {
        setLoading(false);
      }
    }
  }, deps);

  useEffect(() => {
    if (immediate) execute();
    return () => abortRef.current?.abort();
  }, [execute, immediate]);

  return { data, loading, error, refetch: execute };
}
```

---

### Bước 4: Chuẩn hóa Services — Ví dụ refactor `contentService.ts`

```typescript
// TRƯỚC (hiện tại):
import { apiClient, apiFetch } from "@/lib/apiClient";

interface GenericResponse<T> {        // ← duplicate lần thứ 7
  success: boolean;
  // ...
}

export async function fetchContents(params?) {
  try {
    const res: GenericResponse<PagedResult<ContentApiItem>> = await apiClient(`/content?${query}`);
    // ...
  } catch { /* fallback */ }            // ← nuốt error
  return fallbackFetchContents(params);  // ← mock lẫn production
}

// SAU (đề xuất):
import { apiClient } from "@/lib/apiClient";
import type { ApiResponse, PagedResult } from "@/lib/apiTypes";

export async function fetchContents(params?, signal?: AbortSignal) {
  const query = buildQuery(params);
  const res = await apiClient<ApiResponse<PagedResult<ContentApiItem>>>(
    `/content?${query}`,
    { signal }
  );
  
  if (!res?.data) {
    throw new Error(res?.message || "Không có dữ liệu");
  }
  return res.data;
}

// Mock data tách riêng file: src/mocks/contentMocks.ts
```

---

### Bước 5: Fix `useWorkspaces.ts` — Dùng `apiClient` thay vì `fetch` trực tiếp

```diff
// Thay vì:
- const res = await fetch(`${API_URL}/profiles/user/${userId}`, {
-   headers: { Authorization: `Bearer ${getToken()}` },
- });
- const result = await res.json();

// Dùng:
+ const result = await apiClient<ApiResponse<WorkspaceData[]>>(
+   `/workspaces`
+ );
```

---

### Bước 6: Tách Mock data khỏi Production code

```
src/
├── services/           # Chỉ chứa API calls thuần
│   ├── contentService.ts
│   └── ...
├── mocks/              # Mock data riêng [NEW]
│   ├── index.ts        # Flag: const USE_MOCK = process.env.NODE_ENV === 'development'
│   ├── contentMocks.ts
│   ├── postMocks.ts
│   └── ...
└── lib/
    ├── apiClient.ts    # Core HTTP client
    └── apiTypes.ts     # Shared types [NEW]
```

---

## 📋 Thứ tự ưu tiên thực hiện

| # | Hành động | Mức độ | Ảnh hưởng |
|---|-----------|--------|-----------|
| 1 | Bổ sung `X-Profile-Id` và giữ `X-Workspace-Id` đúng context | 🔴 Critical | Các API profile/workspace context đang dễ bị 401/403 |
| 2 | Fix token refresh race condition | 🔴 Critical | Logout đột ngột khi multi-request |
| 3 | Tạo `apiTypes.ts` + xóa duplicate | 🟡 High | Code maintainability |
| 4 | Hợp nhất `apiClient` + `apiFetch` | 🟡 High | Giảm confusion |
| 5 | Fix `useWorkspaces`/`useProfiles` dùng `apiClient` | 🟡 High | Token refresh bị bypass |
| 6 | Tạo `useApi` hook | 🟢 Medium | Better DX + request cancellation |
| 7 | Tách mock data ra folder riêng | 🟢 Medium | Clean architecture |
| 8 | Cải thiện error handling (không nuốt error) | 🟢 Medium | Debug-ability |

---

## ⚠️ Lưu ý quan trọng

> [!IMPORTANT]
> **Không cần thêm library mới** (SWR, React Query, Axios). Với React 19 + Next.js 16, dùng native `fetch()` + custom hooks là đủ. React 19 có built-in `use()` hook và cải thiện Suspense, đủ cho data fetching patterns phổ biến.

> [!NOTE]
> Nếu sau này dự án phát triển lớn hơn (cần caching, optimistic updates, infinite scroll), có thể cân nhắc thêm **SWR** (lightweight, ~4KB) thay vì React Query (~12KB) vì dự án AISAM ưu tiên bundle size nhỏ.


---

## 13. Appendix B — Tài liệu API Fetch Mapping gốc

# 📋 API Fetch Mapping — FE ↔ BE theo User Case

> Tài liệu đối chiếu từng endpoint giữa Backend Controllers và Frontend Services.
> Giúp theo dõi endpoint nào đã kết nối thật, endpoint nào chỉ chạy mock, endpoint nào còn thiếu.

## Chú thích trạng thái

| Icon | Ý nghĩa |
|------|---------|
| ✅ | FE gọi API thật → fallback mock khi lỗi |
| ⚠️ | FE có service nhưng **100% mock**, KHÔNG gọi BE |
| ❌ | BE có endpoint nhưng FE **chưa có** service tương ứng |
| 🔲 | Cả FE lẫn BE đều chưa implement |
| 🔀 | FE gọi bằng `fetch()` trực tiếp, bypass `apiClient` |

---

## 1️⃣ AUTH — Xác thực (US-01 → US-11)

> BE: [AuthController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/AuthController.cs) — Route: `api/auth`
> FE: [auth.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/lib/auth.ts), [profileSettingsService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/profileSettingsService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-01 | `/api/auth/register` | POST | `Register()` | FE xử lý inline trong trang đăng ký | ❌ Chưa có service riêng |
| US-02 | `/api/auth/login` | POST | `Login()` | FE xử lý inline trong trang đăng nhập | ❌ Chưa có service riêng |
| US-03 | `/api/auth/refresh` | POST | `RefreshToken()` | [auth.ts → refreshAccessToken()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/lib/auth.ts#L74-L99) | ✅ Gọi API thật |
| US-04 | `/api/auth/me` | GET | `GetCurrentUser()` | Không có FE service | ❌ Thiếu |
| US-05 | `/api/auth/logout` | POST | `Logout()` | [auth.ts → logout()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/lib/auth.ts#L121-L146) | ✅ Gọi API thật (best-effort) |
| US-06 | `/api/auth/logout-all` | POST | `LogoutAllSessions()` | Không có FE service | ❌ Thiếu |
| US-07 | `/api/auth/verify-email` | GET | `VerifyEmail()` | Không có FE service | ❌ Thiếu |
| US-08 | `/api/auth/verify-email/resend` | POST | `ResendEmailVerification()` | Không có FE service | ❌ Thiếu |
| US-09 | `/api/auth/forgot-password` | POST | `ForgotPassword()` | Không có FE service | ❌ Thiếu |
| US-10 | `/api/auth/reset-password` | POST | `ResetPassword()` | Không có FE service | ❌ Thiếu |
| — | `/api/auth/change-password` | POST | `ChangePassword()` | [profileSettingsService.ts → changePassword()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/profileSettingsService.ts#L18-L30) | ✅ Gọi API thật → mock fallback |
| — | `/api/auth/change-password-with-token` | POST | `ChangePasswordWithToken()` | Không có FE service | ❌ Thiếu |
| US-11 | `/api/auth/google` | POST | `GoogleLogin()` | FE xử lý inline trong trang login | ❌ Chưa có service riêng |
| — | `/api/auth/sessions` | GET | `GetActiveSessions()` | Không có FE service | ❌ Thiếu |

---

## 2️⃣ PROFILE — Business Profile (US-12 → US-14)

> BE: [ProfileController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/ProfileController.cs) — Route: `api/profiles`
> FE: [useProfiles.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useProfiles.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-13 | `/api/profiles/user/{userId}` | GET | `GetUserProfiles()` | [useProfiles.ts → fetchProfiles()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useProfiles.ts#L67-L111) | 🔀 Gọi `fetch()` trực tiếp, bypass apiClient |
| US-12 | `/api/profiles/user/{userId}` | POST | `CreateProfileForm()` (multipart) | Không có FE service | ❌ Thiếu |
| — | `/api/profiles/{id}` | GET | `GetProfile()` | Không có FE service | ❌ Thiếu |
| US-14 | `/api/profiles/{id}` | PUT | `UpdateProfile()` (multipart) | Không có FE service | ❌ Thiếu |
| — | `/api/profiles/{id}` | DELETE | `DeleteProfile()` | Không có FE service | ❌ Thiếu |
| — | `/api/profiles/{id}/restore` | PATCH | `RestoreProfile()` | Không có FE service | ❌ Thiếu |

---

## 3️⃣ WORKSPACE (US-69 → US-79)

> BE: [WorkspaceController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/WorkspaceController.cs) — Route: `api/workspaces`
> FE: [useWorkspaces.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useWorkspaces.ts), [workspaceService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-70 | `/api/workspaces` | GET | `GetMine()` | [workspaceService.ts → fetchWorkspaces()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts#L35-L42) | ✅ Gọi API thật → mock fallback |
| — | `/api/workspaces/{id}` | GET | `GetById()` | Không có FE service | ❌ Thiếu |
| US-69 | `/api/workspaces` | POST | `Create()` | Không có FE service | ❌ Thiếu |
| — | `/api/workspaces/{id}` | PUT | `Update()` | Không có FE service | ❌ Thiếu |
| — | `/api/workspaces/{id}` | DELETE | `AdminSoftDelete()` (Admin only) | Không có FE service | ❌ Thiếu |

> [useWorkspaces.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/hooks/useWorkspaces.ts#L154-L156) cũng gọi thêm `/api/workspaces/user/{userId}` bằng `fetch()` trực tiếp — endpoint này **KHÔNG TỒN TẠI** trong BE WorkspaceController! (Chỉ có `/api/workspaces` GET).

---

## 4️⃣ WORKSPACE MEMBERS (US-71, US-72)

> BE: [WorkspaceMemberController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/WorkspaceMemberController.cs) — Route: `api/workspace-members`
> FE: [workspaceService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-71 | `/api/workspace-members` | GET | `GetMembers()` | [workspaceService.ts → fetchWorkspaceMembers()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts#L337-L348) | ✅ Gọi API thật → mock fallback |
| US-72 | `/api/workspace-members/{memberId}/role` | PUT | `UpdateRole()` | Không có FE service | ❌ Thiếu |
| — | `/api/workspace-members/{memberId}/quota` | PUT | `UpdateQuota()` | Không có FE service | ❌ Thiếu |
| — | `/api/workspace-members/{memberId}` | DELETE | `Remove()` | Không có FE service | ❌ Thiếu |
| US-72 | `/api/workspace-members/ownership-transfer` | POST | `TransferOwnership()` | Không có FE service | ❌ Thiếu |

> [!WARNING]
> FE gọi `/workspaces/members` nhưng BE route thật là `/workspace-members` — **URL sai!**

---

## 5️⃣ WORKSPACE INVITATIONS (US-71)

> BE: [WorkspaceInvitationController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/WorkspaceInvitationController.cs) — Route: `api/workspace-invitations`
> FE: [workspaceInvitationService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceInvitationService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-71 | `/api/workspace-invitations` | POST | `Invite()` | [inviteMember()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceInvitationService.ts#L118-L140) | ✅ → mock fallback |
| US-71 | `/api/workspace-invitations/accept` | POST | `Accept()` | [acceptInvitation()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceInvitationService.ts#L154-L173) | ✅ → mock fallback |

> [!WARNING]
> FE gọi `/workspaces/invitations` nhưng BE route là `/workspace-invitations` — **URL sai!**
> FE cũng có thêm hàm `getInvitationByToken()`, `cancelInvitation()`, `getWorkspaceInvitations()` nhưng BE hiện chưa có endpoint GET/DELETE tương ứng. Không được chỉ sửa prefix rồi coi là đã nối API thật; phải giữ mock/TODO hoặc bổ sung BE endpoint.

---

## 6️⃣ BRAND KIT (US-15, US-16)

> BE: [BrandController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/BrandController.cs) — Route: `api/brands`
> FE: [brandService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/brandService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-15 | `/api/brands` | GET | `GetBrands()` (paged) | [fetchBrands()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/brandService.ts#L23-L34) | ✅ → mock fallback |
| US-15 | `/api/brands/{id}` | GET | `GetById()` | [contentService.ts → resolveBrandName()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L333-L343) | ✅ → fallback |
| US-15 | `/api/brands` | POST | `Create()` | Không có FE service | ❌ Thiếu |
| US-15 | `/api/brands/{id}` | PUT | `Update()` | Không có FE service | ❌ Thiếu |
| US-15 | `/api/brands/{id}` | DELETE | `SoftDelete()` | Không có FE service | ❌ Thiếu |
| US-15 | `/api/brands/{id}/restore` | POST | `Restore()` | Không có FE service | ❌ Thiếu |

---

## 7️⃣ PRODUCT (US-17, US-18)

> BE: [ProductController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/ProductController.cs) — Route: `api/products`
> FE: [brandService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/brandService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-17 | `/api/products` | GET | `GetProducts()` (paged) | [fetchProducts()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/brandService.ts#L36-L48) | ✅ → mock fallback |
| — | `/api/products/{id}` | GET | `GetById()` | [resolveProductName()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L345-L351) | ✅ → fallback |
| US-17 | `/api/products` | POST | `Create()` (multipart) | Không có FE service | ❌ Thiếu |
| US-17 | `/api/products/{id}` | PUT | `Update()` (multipart) | Không có FE service | ❌ Thiếu |
| US-17 | `/api/products/{id}` | DELETE | `SoftDelete()` | Không có FE service | ❌ Thiếu |
| US-17 | `/api/products/{id}/restore` | POST | `Restore()` | Không có FE service | ❌ Thiếu |

---

## 8️⃣ CONTENT HUB (US-19 → US-21)

> BE: [ContentController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/ContentController.cs) — Route: `api/content`
> FE: [contentService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-20 | `/api/content` | GET | `GetPaged()` | [fetchContents()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L130-L165) | ✅ → mock fallback |
| US-20 | `/api/content/{id}` | GET | `GetById()` | [fetchContentById()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L185-L195) | ✅ → mock fallback |
| US-19 | `/api/content` | POST | `Create()` | [createContent()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L197-L207) | ✅ → mock fallback |
| US-20 | `/api/content/{id}` | PUT | `Update()` | [updateContent()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L235-L246) | ✅ → mock fallback |
| US-21 | `/api/content/{id}/clone` | POST | `Clone()` | Không có FE service | ❌ Thiếu |
| US-34 | `/api/content/{id}/publish/{integrationId}` | POST | `Publish()` | Không có FE service | ❌ Thiếu |
| US-20 | `/api/content/{id}` | DELETE | `SoftDelete()` | [deleteContent()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L276-L284) | ✅ → mock fallback |
| US-20 | `/api/content/{id}/restore` | POST | `Restore()` | [restoreContent()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L293-L301) | ✅ → mock fallback |

---

## 9️⃣ AI — Gemini (US-22 → US-28)

> BE: [GeminiController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/GeminiController.cs) — Route: `api/ai`
> BE: [ConversationController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/ConversationController.cs) — Route: `api/conversations`
> FE: [contentService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-22 | `/api/ai/generate-draft` | POST | `GenerateDraft()` | [generateAIDraft()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L305-L315) | ✅ → null fallback |
| US-23 | `/api/ai/improve/{contentId}` | POST | `Improve()` | Không có FE service | ❌ Thiếu |
| US-24 | `/api/ai/approve/{aiGenerationId}` | POST | `Approve()` | Không có FE service | ❌ Thiếu |
| US-25 | `/api/ai/generations/{contentId}` | GET | `GetGenerations()` | Không có FE service | ❌ Thiếu |
| US-26 | `/api/ai/chat` | POST | `Chat()` | [chatWithAI()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/contentService.ts#L317-L327) | ✅ → null fallback |
| US-27 | `/api/conversations` | GET | `GetPaged()` | Không có FE service | ❌ Thiếu |
| US-27 | `/api/conversations/{id}` | GET | `GetById()` | Không có FE service | ❌ Thiếu |
| US-28 | `/api/conversations/{id}` | DELETE | `SoftDelete()` | Không có FE service | ❌ Thiếu |

> [!WARNING]
> FE `chatWithAI()` gửi `{ message, history }` nhưng BE `ChatRequest` cần `{ brandId, productId, adType, message, conversationId }` — **payload không khớp!**

---

## 🔟 SOCIAL — Facebook OAuth & Integrations (US-29 → US-33)

> BE: [SocialAuthController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/SocialAuthController.cs) — Route: `api/social-auth`
> BE: [SocialAccountsController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/SocialAccountsController.cs) — Route: `api/social/accounts`
> BE: [SocialIntegrationController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/SocialIntegrationController.cs) — Route: `api/social/integrations`
> FE: [socialAccountService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-29 | `/api/social-auth/facebook` | GET | `GetFacebookAuthUrl()` | [getFacebookAuthUrl()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L206-L211) | ⚠️ 100% mock — không gọi BE |
| US-29 | `/api/social-auth/facebook/callback` | POST | `HandleFacebookCallback()` | [handleFacebookCallback()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L213-L233) | ⚠️ 100% mock |
| US-30 | `/api/social/accounts/me` | GET | `GetMyAccounts()` | [fetchSocialAccounts()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L202-L204) | ⚠️ 100% mock |
| US-31 | `/api/social/accounts/{id}/available-targets` | GET | `GetAvailableTargets()` | [getAvailableTargets()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L235-L240) | ⚠️ 100% mock |
| US-31 | `/api/social/accounts/{id}/linked-targets` | GET | `GetLinkedTargets()` | [getLinkedTargets()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L242-L245) | ⚠️ 100% mock |
| US-32 | `/api/social/accounts/{id}/link-targets` | POST | `LinkTargets()` | [linkTargets()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L247-L268) | ⚠️ 100% mock |
| US-33 | `/api/social/accounts/{id}` | DELETE | `DeleteAccount()` | [deleteSocialAccount()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L270-L277) | ⚠️ 100% mock |
| US-33 | `/api/social/integrations/{id}` | DELETE | `DeleteIntegration()` | Không có FE service | ❌ Thiếu |
| — | `/api/social/integrations/brand/{brandId}` | GET | `GetByBrand()` | [fetchSocialIntegrations()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/socialAccountService.ts#L279-L315) | ✅ → mock fallback |

---

## 1️⃣1️⃣ POSTS (US-34, US-35)

> BE: [PostsController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/PostsController.cs) — Route: `api/posts`
> FE: [postService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/postService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-35 | `/api/posts` | GET | `GetPaged()` | [fetchPosts()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/postService.ts#L152-L172) | ✅ → mock fallback |
| US-35 | `/api/posts/{id}` | GET | `GetById()` | Không có FE service | ❌ Thiếu |
| — | — | — | — | [createPost()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/postService.ts#L178-L209) | ⚠️ 100% mock — BE không có create endpoint |
| — | — | — | — | [updatePost()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/postService.ts#L211-L216) | ⚠️ 100% mock |
| — | — | — | — | [deletePost()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/postService.ts#L218-L222) | ⚠️ 100% mock |
| — | — | — | — | [retryPost()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/postService.ts#L224-L229) | ⚠️ 100% mock |

---

## 1️⃣2️⃣ SCHEDULES (US-40 → US-42)

> BE: [ContentSchedulesController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/ContentSchedulesController.cs) — Route: `api/content-schedules`
> FE: [scheduleService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/scheduleService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-41 | `/api/content-schedules` | GET | `GetPaged()` | [fetchSchedules()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/scheduleService.ts#L132-L152) | ✅ → mock fallback |
| US-41 | `/api/content-schedules/upcoming` | GET | `GetUpcoming()` | [fetchUpcomingSchedules()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/scheduleService.ts#L154-L175) | ✅ → mock fallback |
| — | `/api/content-schedules/{id}` | GET | `GetById()` | [fetchScheduleById()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/scheduleService.ts#L309-L316) | ✅ → mock fallback |
| US-40 | `/api/content-schedules` | POST | `Create()` | [createSchedule()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/scheduleService.ts#L187-L231) | ✅ → mock fallback |
| US-41 | `/api/content-schedules/{id}` | PUT | `Update()` | [updateSchedule()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/scheduleService.ts#L233-L279) | ✅ → mock fallback |
| US-41 | `/api/content-schedules/{id}` | DELETE | `Delete()` | [deleteSchedule()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/scheduleService.ts#L281-L307) | ✅ → mock fallback |

---

## 1️⃣3️⃣ NOTIFICATIONS (US-36 → US-39)

> BE: [NotificationsController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/NotificationsController.cs) — Route: `api/notifications`
> FE: [notificationService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/notificationService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-37 | `/api/notifications` | GET | `GetPaged()` | [getNotifications()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/notificationService.ts#L124-L160) | ⚠️ `useMockData=true` hardcode → không bao giờ gọi BE |
| US-37 | `/api/notifications/{id}` | GET | `GetById()` | [getNotificationDetail()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/notificationService.ts#L162-L185) | ⚠️ `useMockData=true` |
| US-38 | `/api/notifications/{id}/mark-read` | POST | `MarkRead()` | [markNotificationRead()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/notificationService.ts#L187-L205) | ⚠️ `useMockData=true` |
| US-38 | `/api/notifications/mark-all-read` | POST | `MarkAllRead()` | [markAllNotificationsRead()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/notificationService.ts#L207-L221) | ⚠️ `useMockData=true` |
| US-39 | `/api/notifications/unread-count` | GET | `GetUnreadCount()` | [getUnreadCount()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/notificationService.ts#L223-L234) | ⚠️ `useMockData=true` |
| — | — | — | — | [deleteNotification()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/notificationService.ts#L236-L254) | ⚠️ mock — BE không có delete endpoint |

---

## 1️⃣4️⃣ DASHBOARD (US-43, US-79)

> BE: [DashboardController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/DashboardController.cs) — Route: `api/dashboard`
> BE: [WorkspaceDashboardController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/WorkspaceDashboardController.cs) — Route: `api/workspace-dashboard`
> FE: [workspaceService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-43 | `/api/dashboard/summary` | GET | `GetSummary()` | [fetchWorkspaceDashboard()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts#L44-L69) | ✅ → mock fallback |
| US-79 | `/api/workspace-dashboard/summary` | GET | `GetSummary()` | Không có FE service | ❌ Thiếu |

---

## 1️⃣5️⃣ PAYMENT & QUOTA (US-44 → US-50, US-73 → US-77)

> BE: [PaymentController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/PaymentController.cs) — Route: `api/payment`
> BE: [QuotaController.cs](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-BE/AISAM.API/Controllers/QuotaController.cs) — Route: `api/quota`
> FE: [paymentService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/paymentService.ts), [profileSettingsService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/profileSettingsService.ts), [workspaceService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts)

| US | API Endpoint | Method | BE Controller | FE Service/Hàm | Trạng thái |
|----|-------------|--------|---------------|-----------------|------------|
| US-44 | `/api/payment/checkout` | POST | `CreateCheckout()` | [profileSettingsService → createCheckout()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/profileSettingsService.ts#L174-L185) | ✅ → null fallback |
| — | — | — | — | [paymentService → createPayment()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/paymentService.ts#L29-L49) | ✅ → mock fallback |
| US-47 | `/api/payment/callback` | POST | `HandleCallback()` (anonymous) | FE không gọi trực tiếp | — (PayOS redirect) |
| US-47 | `/api/payment/webhook` | POST | `HandleWebhook()` (anonymous) | FE không gọi trực tiếp | — (PayOS webhook) |
| US-45 | `/api/payment/history` | GET | `GetHistory()` | [getPaymentHistory()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/profileSettingsService.ts#L83-L105) | ✅ → mock fallback |
| US-46 | `/api/payment/subscription/current` | GET | `GetCurrentSubscription()` | [getCurrentSubscription()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/profileSettingsService.ts#L132-L140) + [fetchCreditWallet()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts#L71-L94) | ✅ → mock fallback |
| US-48 | `/api/quota/workspace/current` | GET | `GetCurrentWorkspaceQuota()` | [fetchPostQuota()](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/workspaceService.ts#L96-L114) | ⚠️ FE gọi sai URL `/quota/profile/{profileId}` — BE route là `/quota/workspace/current` |

> [!CAUTION]
> FE `fetchPostQuota()` gọi `/quota/profile/{profileId}` nhưng BE QuotaController route là `/api/quota/workspace/current` — **URL hoàn toàn sai!**

---

## 1️⃣6️⃣ TEAM (US-59) — 🔲 Chưa migrate

> BE: **Không có TeamController**
> FE: [teamService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/teamService.ts) — **100% mock**

| US | FE Hàm | Trạng thái |
|----|--------|------------|
| US-59 | `fetchTeams()`, `fetchMembers()`, `createTeam()`, `updateTeam()`, `deleteTeam()`, `inviteMember()`, `updateMemberRole()`, `removeMember()` | ⚠️ Tất cả 100% mock, không có BE |

---

## 1️⃣7️⃣ ANALYTICS (US-67) — 🔲 Chưa migrate

> BE: **Không có AnalyticsController**
> FE: [analyticsService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/analyticsService.ts) — **100% mock**

| US | FE Hàm | Trạng thái |
|----|--------|------------|
| US-67 | `fetchAnalytics()`, `exportReport()` | ⚠️ 100% mock, không có BE |

---

## 1️⃣8️⃣ CAMPAIGNS (US-60) — 🔲 Chưa migrate

> BE: **Không có CampaignController**
> FE: [campaignService.ts](file:///c:/Users/Kietv/Downloads/To%20do%20list/AISAM-FINAL/AISAM-FE/src/services/campaignService.ts) — **100% mock**

| US | FE Hàm | Trạng thái |
|----|--------|------------|
| US-60 | `fetchCampaigns()`, `createCampaign()`, `updateCampaign()`, `deleteCampaign()`, `applyCampaign()`, `restartCampaign()`, `updateCampaignStatus()` | ⚠️ Tất cả 100% mock, không có BE |

---

## 📊 TỔNG KẾT

### Thống kê tổng quan

| Trạng thái | Số endpoint | Tỷ lệ |
|-----------|-------------|--------|
| ✅ FE gọi API thật (+ fallback mock) | **24** | 35% |
| ⚠️ FE có hàm nhưng 100% mock | **18** | 26% |
| ❌ BE có endpoint — FE thiếu service | **21** | 31% |
| 🔀 FE dùng `fetch()` trực tiếp | **2** | 3% |
| 🔲 Chưa có cả BE lẫn FE | **3 modules** | — |

### 🔴 Lỗi URL không khớp FE → BE (CẦN FIX NGAY)

| FE gọi | BE route thật | File |
|--------|--------------|------|
| `/workspaces/members` | `/workspace-members` | workspaceService.ts |
| `POST /workspaces/invitations` | `POST /workspace-invitations` | workspaceInvitationService.ts |
| `POST /workspaces/invitations/accept` | `POST /workspace-invitations/accept` | workspaceInvitationService.ts |
| `/quota/profile/{profileId}` | `/quota/workspace/current` | workspaceService.ts |
| `/workspaces/user/{userId}` | `/workspaces` (GET mine) | useWorkspaces.ts |
| `/credits/deduct` | **Không tồn tại** | workspaceService.ts |
| `/credits/history` | **Không tồn tại** | workspaceService.ts |

### ⚠️ FE có hàm nhưng BE chưa có endpoint tương ứng

| FE gọi | Trạng thái BE | File |
|--------|---------------|------|
| `GET /workspaces/invitations/accept?token=...` | Chưa có GET invitation detail/public preview endpoint | workspaceInvitationService.ts |
| `GET /workspaces/invitations` | Chưa có list invitations endpoint | workspaceInvitationService.ts |
| `DELETE /workspaces/invitations/{id}` | Chưa có cancel/delete invitation endpoint | workspaceInvitationService.ts |

### Ưu tiên bổ sung FE service theo nhóm

| Ưu tiên | Nhóm | Endpoint thiếu |
|---------|------|----------------|
| 🔴 P0 | Fix URL sai + phân loại endpoint chưa có BE | 7 endpoint/hàm ở bảng trên |
| 🔴 P0 | Auth services | register, login, google, me, forgot/reset password, verify email |
| 🟡 P1 | Content CRUD đầy đủ | clone, publish |
| 🟡 P1 | AI nâng cao | improve, approve, generations, conversations |
| 🟡 P1 | Social chuyển mock → API | accounts/me, auth, targets, link |
| 🟢 P2 | Profile CRUD | create, update, delete, restore |
| 🟢 P2 | Brand/Product CRUD | create, update, delete, restore |
| 🟢 P2 | Workspace management | create, update, member role/quota |
| ⚪ P3 | Notification bỏ `useMockData=true` | Chỉ cần đổi flag |
