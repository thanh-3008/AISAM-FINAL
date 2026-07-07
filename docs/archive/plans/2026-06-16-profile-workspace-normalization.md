# Profile Workspace Normalization Implementation Plan

> Product policy update 2026-06-24: use `docs/main/workspace-subscription-expiry-policy.md` for Personal Free fallback, Business paid-only lifecycle, credit retention, and creation flow. This plan covers context normalization and does not override that policy.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Chuẩn hóa triệt để context `workspace` và `profile` trên cả frontend và backend để loại bỏ hoàn toàn các fallback sai kiểu `profile.id == workspace.id`, đồng thời giữ migration tương thích ngắn hạn cho user đang có local state cũ.

**Architecture:** Frontend sẽ được chuẩn hóa để `activeWorkspace` là nguồn sự thật duy nhất cho dashboard và request header `X-Workspace-Id`, còn `activeProfile` chỉ được load/validate khi một feature thật sự cần. Backend sẽ được phân loại lại theo hai nhóm endpoint `workspace-only` và `workspace+profile`, sau đó siết validation ownership giữa profile và workspace, đồng bộ error contract với frontend.

**Tech Stack:** Next.js 16, React 19, TypeScript, ASP.NET Core 8, C#, xUnit integration tests, localStorage-based FE context stores.

---

## File Structure

### Frontend core context files

- Modify: `AISAM-FE/src/stores/workspace-store.ts`
  - Chỉ còn chịu trách nhiệm lưu/đọc/clear active workspace, migration legacy an toàn.
- Modify: `AISAM-FE/src/stores/profile-store.ts`
  - Giữ active profile độc lập, không còn assumption liên quan workspace.
- Modify: `AISAM-FE/src/hooks/useWorkspaces.ts`
  - Nguồn sự thật cho danh sách workspace, chọn active workspace, cache, migration runtime.
- Modify: `AISAM-FE/src/hooks/useProfiles.ts`
  - Load và validate profile theo workspace, áp dụng hybrid selection rule.
- Modify: `AISAM-FE/src/lib/apiClient.ts`
  - Chuẩn hóa cách gắn `X-Workspace-Id` và `X-Profile-Id`, chuẩn hóa error handling theo context.

### Frontend feature surfaces

- Modify: `AISAM-FE/src/app/overview/page.tsx`
  - Tạo workspace/profile đúng chuẩn, bỏ logic set profile từ workspace.
- Modify: `AISAM-FE/src/app/(dashboard)/content/create/page.tsx`
  - Bỏ fallback gán `workspace.id` vào `profile.id`, chuyển sang flow yêu cầu chọn profile nếu cần.
- Modify: `AISAM-FE/src/app/(dashboard)/brands/page.tsx`
  - Giữ hoạt động workspace-only, không phụ thuộc profile nếu backend không cần.
- Modify: `AISAM-FE/src/app/profiles/[id]/page.tsx`
  - Rà lại route settings theo workspace context.
- Modify: `AISAM-FE/src/components/layout/Header.tsx`
  - Giữ render ổn định theo workspace context mới.
- Modify: `AISAM-FE/src/components/layout/Sidebar.tsx`
  - Chỉ hiển thị/chuyển workspace, không kéo theo profile sai.
- Modify: `AISAM-FE/src/services/contentService.ts`
  - Rà các request cần profile/workspace.
- Modify: `AISAM-FE/src/services/socialAccountService.ts`
  - Rà các request cần profile/workspace.
- Modify: `AISAM-FE/src/services/workspaceService.ts`
  - Giữ contract workspace-only rõ ràng.

### Backend context and validation files

- Modify: `AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs`
  - Giữ workspace validation làm lớp đầu tiên cho protected routes.
- Modify: `AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs`
  - Bổ sung validate profile theo active workspace khi route cần profile.
- Modify: `AISAM-BE/AISAM.API/Utils/WorkspaceContextHelper.cs`
  - Giữ helper workspace context rõ ràng.
- Modify: `AISAM-BE/AISAM.API/Utils/ProfileContextHelper.cs`
  - Nếu cần, bổ sung helper/metadata cho profile context đã validate.
- Modify: `AISAM-BE/AISAM.API/Controllers/ContentController.cs`
- Modify: `AISAM-BE/AISAM.API/Controllers/ContentSchedulesController.cs`
- Modify: `AISAM-BE/AISAM.API/Controllers/GeminiController.cs`
- Modify: `AISAM-BE/AISAM.API/Controllers/SocialAccountsController.cs`
- Modify: `AISAM-BE/AISAM.API/Controllers/SocialAuthController.cs`
  - Phân loại endpoint nào cần profile, endpoint nào chỉ cần workspace.

### Backend tests

- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ActiveWorkspaceMiddlewareTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ActiveProfileMiddlewareTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ContentControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ContentSchedulesControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/SocialControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/AIControllerTests.cs`
  - Xóa assumption `workspaceId == profileId`, thêm test ownership failure/success đúng chuẩn.

### Frontend tests

- Create: `AISAM-FE/src/stores/__tests__/workspace-store.test.ts`
- Create: `AISAM-FE/src/hooks/__tests__/useWorkspaces.test.ts`
- Create: `AISAM-FE/src/hooks/__tests__/useProfiles.test.ts`
- Create: `AISAM-FE/src/lib/__tests__/apiClient-context.test.ts`
  - Nếu repo chưa có test runner FE, task đầu tiên sẽ xác nhận và thêm test tối thiểu hoặc document gap.

---

### Task 1: Audit contract workspace/profile và khóa danh sách endpoint bị ảnh hưởng

**Files:**
- Modify: `docs/superpowers/specs/2026-06-16-profile-workspace-normalization-design.md`
- Create: `docs/superpowers/plans/2026-06-16-profile-workspace-endpoint-matrix.md`

- [x] **Step 1: Tạo ma trận endpoint workspace/profile**

Tạo file `docs/superpowers/plans/2026-06-16-profile-workspace-endpoint-matrix.md` với nội dung ban đầu:

```md
# Profile Workspace Endpoint Matrix

## Workspace-only endpoints

- `GET /api/workspaces`
- `GET /api/brands`
- `POST /api/brands`
- `GET /api/workspace-members`
- `GET /api/dashboard`

## Workspace + profile endpoints

- `POST /api/content`
- `POST /api/content-schedules`
- `POST /api/social-auth/{provider}/callback`
- `POST /api/ai/chat`

## To verify from code

- `AISAM.API/Controllers/ContentController.cs`
- `AISAM.API/Controllers/ContentSchedulesController.cs`
- `AISAM.API/Controllers/GeminiController.cs`
- `AISAM.API/Controllers/SocialAccountsController.cs`
- `AISAM.API/Controllers/SocialAuthController.cs`
```

- [x] **Step 2: Xác minh controller mapping bằng tìm kiếm**

Run:

```powershell
rg -n "GetActiveWorkspaceIdOrThrow|GetActiveProfileIdOrThrow" AISAM-BE\AISAM.API\Controllers
```

Expected:

- Hiển thị đầy đủ controller đang dùng workspace helper và profile helper
- Không có controller quan trọng nào bị bỏ sót khỏi ma trận

- [x] **Step 3: Cập nhật ma trận thành danh sách cuối cùng**

Chỉnh file ma trận để mỗi endpoint được gắn nhãn một trong ba loại:

```md
- `POST /api/content` -> workspace + profile required
- `GET /api/brands` -> workspace only
- `POST /api/social-auth/facebook/callback` -> workspace + profile required
```

- [x] **Step 4: Commit**

```bash
git add docs/superpowers/plans/2026-06-16-profile-workspace-endpoint-matrix.md
git commit -m "docs: add workspace profile endpoint matrix"
```

### Task 2: Viết test thất bại cho frontend storage migration

**Files:**
- Create: `AISAM-FE/src/stores/__tests__/workspace-store.test.ts`
- Modify: `AISAM-FE/src/stores/workspace-store.ts`

- [x] **Step 1: Viết test chứng minh legacy profile không được coi là workspace mặc định**

```ts
import { describe, expect, it, beforeEach } from "vitest";
import { clearActiveWorkspace, getStoredActiveWorkspace } from "@/stores/workspace-store";

describe("workspace-store migration", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("does not blindly promote legacy profile id into workspace state", () => {
    localStorage.setItem("aisam_active_profile", JSON.stringify({
      id: "11111111-1111-1111-1111-111111111111",
      name: "Legacy Profile",
      profileType: 2,
    }));

    const workspace = getStoredActiveWorkspace();

    expect(workspace).toBeNull();
  });

  it("returns normalized workspace when workspace storage is already valid", () => {
    localStorage.setItem("aisam_active_workspace", JSON.stringify({
      id: "22222222-2222-2222-2222-222222222222",
      name: "Main Workspace",
      workspaceType: 2,
    }));

    expect(getStoredActiveWorkspace()).toEqual({
      id: "22222222-2222-2222-2222-222222222222",
      name: "Main Workspace",
      workspaceType: 2,
    });
  });
});
```

- [x] **Step 2: Chạy test để xác nhận đang fail**

Run:

```powershell
npm.cmd test -- workspace-store.test.ts
```

Expected:

- FAIL vì repo hiện chưa có runner FE hoặc `getStoredActiveWorkspace()` vẫn promote legacy profile sang workspace

- [x] **Step 3: Implement tối thiểu để test pass**

Cập nhật `AISAM-FE/src/stores/workspace-store.ts` theo hướng:

```ts
export function getStoredActiveWorkspace(): ActiveWorkspace | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const parsed = JSON.parse(raw) as Partial<ActiveWorkspace>;
    if (!parsed.id || !parsed.name || typeof parsed.workspaceType !== "number") {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }

    return {
      id: parsed.id,
      name: parsed.name,
      workspaceType: parsed.workspaceType,
    };
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}
```

- [x] **Step 4: Chạy test để xác nhận pass**

Run:

```powershell
npm.cmd test -- workspace-store.test.ts
```

Expected:

- PASS hoặc nếu runner chưa có thì task tiếp theo phải thêm runner trước khi tiếp tục FE tests

- [x] **Step 5: Commit**

```bash
git add AISAM-FE/src/stores/workspace-store.ts AISAM-FE/src/stores/__tests__/workspace-store.test.ts
git commit -m "test: lock workspace store migration behavior"
```

### Task 3: Chuẩn hóa `useWorkspaces` để chỉ dùng workspace contract

**Files:**
- Modify: `AISAM-FE/src/hooks/useWorkspaces.ts`
- Create: `AISAM-FE/src/hooks/__tests__/useWorkspaces.test.ts`

- [x] **Step 1: Viết test thất bại cho `useWorkspaces`**

```ts
import { describe, expect, it, vi } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { useWorkspaces } from "@/hooks/useWorkspaces";

vi.mock("@/lib/apiClient", () => ({
  apiClient: vi.fn(async (endpoint: string) => {
    if (endpoint === "/workspaces") {
      return {
        success: true,
        data: [
          { id: "w1", name: "Workspace A", workspaceType: 2, status: 1, currentUserRole: 0, createdAt: "", updatedAt: "" },
        ],
      };
    }
    throw new Error(`unexpected endpoint: ${endpoint}`);
  }),
}));

describe("useWorkspaces", () => {
  it("loads workspaces only from workspace endpoint", async () => {
    const { result } = renderHook(() => useWorkspaces());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.workspaces).toHaveLength(1);
    expect(result.current.activeWorkspace?.id).toBe("w1");
  });
});
```

- [x] **Step 2: Chạy test và xác nhận fail**

Run:

```powershell
npm.cmd test -- useWorkspaces.test.ts
```

Expected:

- FAIL vì hook hiện còn fallback `/profiles/user/{userId}`

- [x] **Step 3: Implement tối thiểu**

Sửa `AISAM-FE/src/hooks/useWorkspaces.ts` để:

```ts
// remove fetchFromProfiles
// workspace hook must only read /workspaces
const res: any = await apiClient("/workspaces");
if (res?.success && Array.isArray(res.data)) {
  mapped = res.data.map((w: any) => ({
    id: w.id,
    userId,
    name: w.name,
    workspaceType: w.workspaceType ?? 1,
    plan: w.workspaceType === 2 ? "Business" : "Personal",
    status: w.status ?? 1,
    createdAt: w.createdAt,
    updatedAt: w.updatedAt,
    isOwner: w.currentUserRole === 0,
    memberRole: w.currentUserRole !== undefined
      ? ["Owner", "Manager", "ContentCreator", "Viewer"][w.currentUserRole] ?? "Viewer"
      : "Owner",
  }));
}
```

- [x] **Step 4: Chạy test xác nhận pass**

Run:

```powershell
npm.cmd test -- useWorkspaces.test.ts
```

Expected:

- PASS

- [x] **Step 5: Commit**

```bash
git add AISAM-FE/src/hooks/useWorkspaces.ts AISAM-FE/src/hooks/__tests__/useWorkspaces.test.ts
git commit -m "refactor: normalize workspace hook around workspace endpoint"
```

### Task 4: Chuẩn hóa `useProfiles` theo active workspace và hybrid selection

**Files:**
- Modify: `AISAM-FE/src/hooks/useProfiles.ts`
- Create: `AISAM-FE/src/hooks/__tests__/useProfiles.test.ts`

- [x] **Step 1: Viết test thất bại cho hybrid selection**

```ts
import { describe, expect, it, vi } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { useProfiles } from "@/hooks/useProfiles";

vi.mock("@/lib/apiClient", () => ({
  apiClient: vi.fn(async () => ({
    success: true,
    data: [
      { id: "p1", userId: "u1", name: "Only Profile", profileType: 2, status: 1 },
    ],
  })),
}));

describe("useProfiles", () => {
  it("auto-selects the only valid profile", async () => {
    const { result } = renderHook(() => useProfiles());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.activeProfile?.id).toBe("p1");
  });
});
```

- [x] **Step 2: Chạy test và xác nhận fail**

Run:

```powershell
npm.cmd test -- useProfiles.test.ts
```

Expected:

- FAIL hoặc chưa phản ánh được quan hệ workspace/profile

- [x] **Step 3: Implement tối thiểu**

Điều chỉnh `AISAM-FE/src/hooks/useProfiles.ts` theo nguyên tắc:

```ts
// keep active profile independent
// clear stored profile when it is not found in the valid profile list
const stored = getStoredActiveProfile();
const storedMatch = stored ? profiles.find((p) => p.id === stored.id) : null;
const fallbackMatch = profiles.length === 1
  ? profiles[0]
  : profiles.find((p) => p.status === 1) || null;
const activeProfile = storedMatch || fallbackMatch;
```

Và khi workspace đổi:

```ts
if (storedId && !storedMatch && profiles.length > 1) {
  clearActiveProfile();
}
```

- [x] **Step 4: Chạy test xác nhận pass**

Run:

```powershell
npm.cmd test -- useProfiles.test.ts
```

Expected:

- PASS

- [x] **Step 5: Commit**

```bash
git add AISAM-FE/src/hooks/useProfiles.ts AISAM-FE/src/hooks/__tests__/useProfiles.test.ts
git commit -m "refactor: normalize profile hook with hybrid selection"
```

### Task 5: Chuẩn hóa API client và FE error contract

**Files:**
- Modify: `AISAM-FE/src/lib/apiClient.ts`
- Create: `AISAM-FE/src/lib/__tests__/apiClient-context.test.ts`

- [x] **Step 1: Viết test thất bại cho header injection**

```ts
import { describe, expect, it, vi, beforeEach } from "vitest";
import { apiClient } from "@/lib/apiClient";

describe("apiClient context headers", () => {
  beforeEach(() => {
    localStorage.clear();
    global.fetch = vi.fn(async () => ({
      ok: true,
      status: 200,
      json: async () => ({ success: true }),
    })) as any;
  });

  it("always sends workspace header from workspace storage", async () => {
    localStorage.setItem("aisam_active_workspace", JSON.stringify({
      id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      name: "Workspace",
      workspaceType: 2,
    }));

    await apiClient("/brands");

    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/brands"),
      expect.objectContaining({
        headers: expect.objectContaining({
          "X-Workspace-Id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        }),
      }),
    );
  });
});
```

- [x] **Step 2: Chạy test và xác nhận fail nếu header logic còn mơ hồ**

Run:

```powershell
npm.cmd test -- apiClient-context.test.ts
```

Expected:

- FAIL nếu logic hiện tại còn clear/gắn header không đúng ngữ nghĩa

- [x] **Step 3: Implement tối thiểu**

Giữ `buildHeaders()` theo nguyên tắc:

```ts
const headers: Record<string, string> = {
  ...(token ? { Authorization: `Bearer ${token}` } : {}),
  ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
  ...(profile ? { "X-Profile-Id": profile.id } : {}),
  ...(customHeaders || {}),
};
```

Và chuẩn hóa error map:

```ts
const ERROR_MAP: Record<string, string> = {
  "Missing or invalid X-Workspace-Id header.": "Chưa chọn Workspace. Vào Overview để chọn workspace.",
  "Missing or invalid X-Profile-Id header.": "Chưa chọn Profile cho tính năng này.",
  "You are not a member of this workspace.": "Bạn không phải thành viên của workspace này.",
  "Profile does not belong to active workspace.": "Profile không thuộc workspace đang chọn.",
};
```

- [x] **Step 4: Chạy test xác nhận pass**

Run:

```powershell
npm.cmd test -- apiClient-context.test.ts
```

Expected:

- PASS

- [x] **Step 5: Commit**

```bash
git add AISAM-FE/src/lib/apiClient.ts AISAM-FE/src/lib/__tests__/apiClient-context.test.ts
git commit -m "refactor: normalize context headers in api client"
```

### Task 6: Dọn các flow FE còn gán profile từ workspace

**Files:**
- Modify: `AISAM-FE/src/app/overview/page.tsx`
- Modify: `AISAM-FE/src/app/(dashboard)/content/create/page.tsx`
- Modify: `AISAM-FE/src/app/(dashboard)/brands/page.tsx`
- Modify: `AISAM-FE/src/components/layout/Sidebar.tsx`

- [x] **Step 1: Viết checklist regression cục bộ cho các flow FE**

Tạo ghi chú tạm trong commit message hoặc scratch note để kiểm thử:

```md
- overview create workspace
- overview select workspace
- brand page loads with workspace only
- content create blocks when profile is required and missing
```

- [x] **Step 2: Sửa `overview/page.tsx`**

Xóa logic kiểu:

```ts
storeActiveProfile({ id: w.id, name: w.name, profileType: w.workspaceType });
```

Thay bằng logic:

```ts
selectWorkspace(w);
clearActiveProfile();
```

Nếu vừa tạo workspace vừa tạo profile thật từ API thì chỉ lưu profile bằng profile ID thật từ response.

- [x] **Step 3: Sửa `content/create/page.tsx`**

Xóa logic:

```ts
if (!storedProfile) {
  storeActiveProfile({ id: storedWs.id, name: storedWs.name, profileType: storedWs.workspaceType });
  storedProfile = getStoredActiveProfile();
}
```

Thay bằng:

```ts
if (!storedProfile) {
  setSaving(false);
  setSaveError("Tính năng này yêu cầu chọn Profile hợp lệ trong workspace hiện tại.");
  return;
}
```

- [x] **Step 4: Sửa `brands/page.tsx` để chỉ phụ thuộc workspace**

Giữ modal create brand gửi `profileId` chỉ khi có profile thật:

```ts
profileId={activeProfile?.id || ""}
```

Nhưng page load và create brand không được tự tạo profile fallback từ workspace.

- [x] **Step 5: Chạy lint/test tối thiểu**

Run:

```powershell
npm.cmd run lint
```

Expected:

- Không có lỗi mới phát sinh từ các file vừa sửa
- Nếu repo còn lint cũ không liên quan, ghi rõ vào notes thực thi

- [x] **Step 6: Commit**

```bash
git add AISAM-FE/src/app/overview/page.tsx AISAM-FE/src/app/(dashboard)/content/create/page.tsx AISAM-FE/src/app/(dashboard)/brands/page.tsx AISAM-FE/src/components/layout/Sidebar.tsx
git commit -m "fix: remove workspace to profile fallback in dashboard flows"
```

### Task 7: Viết test thất bại cho backend profile ownership theo workspace

**Files:**
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ActiveProfileMiddlewareTests.cs`
- Modify: `AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs`

- [x] **Step 1: Viết test thất bại**

```csharp
[Fact]
public async Task InvokeAsync_ReturnsForbidden_WhenProfileDoesNotBelongToActiveWorkspace()
{
    using var db = TestDbFactory.CreateContext();
    var user = TestDataFactory.AddUser(db);
    var workspaceA = TestDataFactory.AddWorkspace(db, user.Id);
    var workspaceB = TestDataFactory.AddWorkspace(db, user.Id);
    var profile = TestDataFactory.AddProfile(db, user.Id);

    var context = TestHttpContextFactory.CreateAuthenticated(user.Id);
    context.Request.Path = "/api/content";
    context.Request.Headers["X-Workspace-Id"] = workspaceA.Id.ToString();
    context.Request.Headers["X-Profile-Id"] = profile.Id.ToString();
    context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceB.Id;

    var middleware = new ActiveProfileMiddleware(_ => Task.CompletedTask);

    await middleware.InvokeAsync(context, db);

    Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
}
```

- [x] **Step 2: Chạy test xác nhận fail**

Run:

```powershell
dotnet test AISAM-BE\tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "FullyQualifiedName~ActiveProfileMiddlewareTests"
```

Expected:

- FAIL vì middleware hiện chỉ check profile tồn tại/chủ user, chưa check ownership với active workspace

- [x] **Step 3: Implement tối thiểu**

Trong `ActiveProfileMiddleware.cs`, sau khi lấy được profile:

```csharp
if (context.Items.TryGetValue(WorkspaceContextHelper.ActiveWorkspaceItemKey, out var workspaceValue) &&
    workspaceValue is Guid activeWorkspaceId &&
    profile.WorkspaceId != activeWorkspaceId)
{
    await WriteErrorAsync(context, HttpStatusCode.Forbidden, "Profile does not belong to active workspace.");
    return;
}
```

Nếu model hiện chưa có `profile.WorkspaceId`, task thực thi phải thay bằng repository/service check phù hợp đã được xác định ở Task 1.

- [x] **Step 4: Chạy test xác nhận pass**

Run:

```powershell
dotnet test AISAM-BE\tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "FullyQualifiedName~ActiveProfileMiddlewareTests"
```

Expected:

- PASS

- [x] **Step 5: Commit**

```bash
git add AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs AISAM-BE/tests/AISAM.IntegrationTests/ActiveProfileMiddlewareTests.cs
git commit -m "test: enforce profile ownership in active profile middleware"
```

### Task 8: Chuẩn hóa backend controller contract theo hai nhóm endpoint

**Files:**
- Modify: `AISAM-BE/AISAM.API/Controllers/ContentController.cs`
- Modify: `AISAM-BE/AISAM.API/Controllers/ContentSchedulesController.cs`
- Modify: `AISAM-BE/AISAM.API/Controllers/GeminiController.cs`
- Modify: `AISAM-BE/AISAM.API/Controllers/SocialAccountsController.cs`
- Modify: `AISAM-BE/AISAM.API/Controllers/SocialAuthController.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ContentControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ContentSchedulesControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/SocialControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/AIControllerTests.cs`

- [x] **Step 1: Viết test thất bại cho workspace-only endpoint**

Ví dụ cho brand/dashboard path nếu cần:

```csharp
[Fact]
public async Task WorkspaceOnlyEndpoint_DoesNotRequireProfileContext()
{
    var controller = CreateDashboardController();
    controller.ControllerContext.HttpContext.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = Guid.NewGuid();

    var result = await controller.GetSummary(CancellationToken.None);

    Assert.NotNull(result);
}
```

- [x] **Step 2: Viết test thất bại cho workspace+profile endpoint**

```csharp
[Fact]
public async Task ContentEndpoint_FailsWhenProfileContextMissing()
{
    var controller = CreateContentController();
    controller.ControllerContext.HttpContext.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = Guid.NewGuid();

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
        controller.Create(new CreateContentRequest(), CancellationToken.None));
}
```

- [x] **Step 3: Implement tối thiểu**

Giữ helper rõ ràng trong controller:

```csharp
private Guid GetWorkspaceId() => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
private Guid GetProfileId() => ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
```

Và chỉ gọi `GetProfileId()` ở các action thực sự cần:

```csharp
var result = await _contentService.CreateInWorkspaceAsync(
    GetWorkspaceId(),
    GetProfileId(),
    request,
    cancellationToken);
```

Trong các action workspace-only, tuyệt đối không truy cập profile helper.

- [x] **Step 4: Chạy test đúng scope**

Run:

```powershell
dotnet test AISAM-BE\tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "FullyQualifiedName~ContentControllerTests|FullyQualifiedName~ContentSchedulesControllerTests|FullyQualifiedName~SocialControllerTests|FullyQualifiedName~AIControllerTests"
```

Expected:

- PASS cho contract mới
- Các test cũ dùng `workspaceId = profileId` phải được cập nhật

- [x] **Step 5: Commit**

```bash
git add AISAM-BE/AISAM.API/Controllers/ContentController.cs AISAM-BE/AISAM.API/Controllers/ContentSchedulesController.cs AISAM-BE/AISAM.API/Controllers/GeminiController.cs AISAM-BE/AISAM.API/Controllers/SocialAccountsController.cs AISAM-BE/AISAM.API/Controllers/SocialAuthController.cs AISAM-BE/tests/AISAM.IntegrationTests/ContentControllerTests.cs AISAM-BE/tests/AISAM.IntegrationTests/ContentSchedulesControllerTests.cs AISAM-BE/tests/AISAM.IntegrationTests/SocialControllerTests.cs AISAM-BE/tests/AISAM.IntegrationTests/AIControllerTests.cs
git commit -m "refactor: separate workspace-only and profile-bound controller flows"
```

### Task 9: Cập nhật backend regression tests đang hardcode `workspaceId = profileId`

**Files:**
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/AIControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ContentControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ContentSchedulesControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/ConversationControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/NotificationsControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/PostsControllerTests.cs`
- Modify: `AISAM-BE/tests/AISAM.IntegrationTests/SocialControllerTests.cs`

- [x] **Step 1: Tìm tất cả test còn assumption sai**

Run:

```powershell
rg -n "ActiveWorkspaceItemKey\] = profileId|workspaceId\) = profileId|workspaceId = profileId" AISAM-BE\tests\AISAM.IntegrationTests
```

Expected:

- Liệt kê chính xác các test đang gán workspace theo profile

- [x] **Step 2: Sửa fixture/test setup**

Mẫu sửa:

```csharp
var profileId = Guid.NewGuid();
var workspaceId = Guid.NewGuid();
context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;
```

Nếu test cần ownership hợp lệ thì dựng model nhất quán:

```csharp
context.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = new WorkspaceMember
{
    WorkspaceId = workspaceId,
    UserId = userId,
    IsActive = true,
};
```

- [x] **Step 3: Chạy full integration tests theo context**

Run:

```powershell
dotnet test AISAM-BE\tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj
```

Expected:

- Không còn test fail chỉ vì assumption `workspace == profile`

- [x] **Step 4: Commit**

```bash
git add AISAM-BE/tests/AISAM.IntegrationTests
git commit -m "test: remove legacy workspace profile identity assumptions"
```

### Task 10: Regression verification end-to-end và cập nhật tài liệu

**Files:**
- Modify: `docs/superpowers/specs/2026-06-16-profile-workspace-normalization-design.md`
- Modify: `docs/superpowers/plans/2026-06-16-profile-workspace-endpoint-matrix.md`
- Modify: `README.md`

- [x] **Step 1: Chạy regression FE/BE chính**

Run:

```powershell
dotnet test AISAM-BE\tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj
```

Run:

```powershell
npm.cmd run lint
```

Expected:

- Backend integration pass
- Frontend không có lỗi mới trong các file đã chỉnh

- [x] **Step 2: Chạy manual smoke checklist**

Manual checklist:

```md
- login thường thành công
- Google login mở được flow đúng config
- reset password mở đúng route FE
- tạo brand không còn lỗi membership giả
- vào workspace settings đúng workspace
- tạo content yêu cầu profile hợp lệ nếu cần
- social connect không còn dùng nhầm workspace như profile
```

- [x] **Step 3: Cập nhật docs sau triển khai**

Thêm vào spec hoặc README phần quyết định cuối:

```md
## Final Normalized Rules

- Workspace is the primary dashboard context
- Profile is secondary and feature-specific
- No runtime fallback may copy workspace ID into profile ID or profile ID into workspace ID
```

- [x] **Step 4: Commit**

```bash
git add docs/superpowers/specs/2026-06-16-profile-workspace-normalization-design.md docs/superpowers/plans/2026-06-16-profile-workspace-endpoint-matrix.md README.md
git commit -m "docs: record normalized workspace profile rules"
```

## Self-Review

### Spec coverage

- Context model FE/BE: covered by Tasks 2, 3, 4, 5, 7, 8
- Migration ngắn hạn: covered by Tasks 2, 3, 4, 6
- Error handling: covered by Tasks 5, 7, 8
- Test strategy: covered by Tasks 2, 3, 4, 7, 8, 9, 10
- Completion criteria and regression: covered by Tasks 9 and 10

### Placeholder scan

- Không dùng `TODO`, `TBD`, hoặc “implement later” trong các bước
- Mọi task đều có file cụ thể, command cụ thể, và expected outcome cụ thể

### Type consistency

- FE normalized types dùng `ActiveWorkspace` và `ActiveProfile` riêng biệt
- BE normalized helper flow dùng `WorkspaceContextHelper` và `ProfileContextHelper` tách riêng
- Tất cả test mới đều giả định `workspaceId` và `profileId` là hai identity khác nhau, chỉ ghép lại qua ownership hợp lệ

