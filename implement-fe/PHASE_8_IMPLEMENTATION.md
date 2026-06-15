# Phase 8 Implementation - Hardening, Docs, Backend-Missing Framework

Tai lieu nay mo rong Phase 8 trong `FRONTEND_CODE_PLAN.md`.

Scope theo `user_story_list.md`:

- US-56: Kiem thu ownership va boundary chinh.
- US-57: Tai lieu hoa setup va smoke test backend/frontend.
- Tao framework dung chung cho cac module `backend-dependent` va `backend-missing`.

Trang thai:

- Hardening/docs: `backend-ready`.
- Backend-missing framework: `backend-ready` o muc UI utility, khong tao API call.

## Nguyen tac

- Khong tao API call cho module chua co backend controller active.
- Khong them endpoint missing vao `src/lib/api/endpoints.ts`.
- Route backend-missing phai render on dinh, giai thich ro module dang cho backend migration.
- Cac page active tu Phase 1-7 phai co loading/empty/error state co ban.
- Phase 8 khong implement route business post-MVP; route cu the nam o Phase 9, Phase 10 va Phase 11.

## Task 8.1 - Shared State Components

Muc tieu:

- Chuan hoa loading, empty, error va render error handling cho toan app.

File can tao:

```text
AISAM-FE/src/components/states/page-loading.tsx
AISAM-FE/src/components/states/page-empty.tsx
AISAM-FE/src/components/states/page-error.tsx
AISAM-FE/src/components/states/error-boundary.tsx
AISAM-FE/src/components/states/index.ts
AISAM-FE/src/app/error.tsx
AISAM-FE/src/app/global-error.tsx
```

Contract:

```ts
type PageLoadingProps = {
  title?: string
  description?: string
  compact?: boolean
}

type PageEmptyProps = {
  title: string
  description?: string
  actionLabel?: string
  onAction?: () => void
}

type PageErrorProps = {
  title?: string
  description?: string
  errorCode?: string
  retryLabel?: string
  onRetry?: () => void
}
```

Definition of Done:

- Import duoc tu `src/components/states`.
- Dashboard, brand/product list, content list, posts, notifications, schedules dung chung state components.
- Render error khong lam sap app shell.

## Task 8.2 - Backend Contract Missing Foundation

Muc tieu:

- Co mot cach fail ro rang cho module trong README/user story nhung backend local chua expose controller.

File can tao:

```text
AISAM-FE/src/lib/api/backend-contract-missing-error.ts
AISAM-FE/src/features/backend-missing/components/backend-missing-page.tsx
AISAM-FE/src/features/backend-missing/components/backend-missing-badge.tsx
AISAM-FE/src/features/backend-missing/config/backend-missing-modules.ts
```

Contract:

```ts
export class BackendContractMissingError extends Error {
  readonly code = "BACKEND_CONTRACT_MISSING"
  readonly moduleName: string

  constructor(moduleName: string, message?: string) {
    super(message ?? `${moduleName} backend contract is not exposed yet.`)
    this.moduleName = moduleName
  }
}
```

Config shape:

```ts
type BackendMissingModule = {
  userStory: string
  title: string
  summary: string
  status: "backend-dependent" | "backend-missing"
  blockedActions: string[]
  availableToday?: Array<{ label: string; href: string }>
}
```

Definition of Done:

- Backend-dependent adapters o Phase 9/10/11 co the import error chung.
- Backend-missing pages co UI nhat quan.
- Khong import query hook/API adapter cho module missing.

## Task 8.3 - Ownership and Boundary Smoke Checklist

Muc tieu:

- Co checklist test FE cho ownership/profile boundary theo US-56.

File can tao:

```text
AISAM-FE/FRONTEND_TEST_CHECKLIST.md
```

Noi dung bat buoc:

- Auth: register/login/refresh/logout/logout-all/me/sessions/change password/email verification.
- Profile: user khong duoc truy cap profile cua user khac.
- Brand/Product: data phai bi scope theo profile/brand ownership.
- Content/AI/Conversation: request phai co `X-Profile-Id`.
- Social/Posts/Notifications/Schedules: request phai co `X-Profile-Id`.
- Payment/Quota/Admin: danh dau backend-dependent, khong smoke API neu backend local chua expose.
- Chua migrate: route shell render, khong goi API.

Definition of Done:

- Checklist co route, prerequisite, expected result, known limitation.
- Team co the dung checklist de smoke sau moi phase.

## Task 8.4 - Env Setup Guide

Muc tieu:

- Nguoi moi clone repo setup FE duoc voi backend local.

File can tao:

```text
AISAM-FE/ENV_SETUP.md
```

Noi dung bat buoc:

- Node/pnpm version khuyen nghi.
- Lenh `pnpm install`, `pnpm dev`, `pnpm lint`, `pnpm build`.
- Env vars FE:

```text
NEXT_PUBLIC_API_BASE_URL=http://localhost:5283/api
NEXT_PUBLIC_APP_ENV=development
NEXT_PUBLIC_ENABLE_DEV_TOOLS=true
```

- Backend prerequisite: database, JWT, Gemini, Facebook neu test AI/social.
- `DevSchedulerController` chi available trong backend Development.
- Danh sach route can `Authorization` va route can them `X-Profile-Id`.
- Known limitations: payment/quota/admin/backend-missing modules khong goi API that khi controller chua co.

Definition of Done:

- Setup guide khong copy secret backend sang frontend.
- Ghi ro limitation theo `FRONTEND_CODE_PLAN.md`.

## Verify Phase 8

Chay:

```text
cd AISAM-FE
pnpm lint
pnpm build
```

Manual smoke:

- Route active chinh hien loading/empty/error dung.
- `BackendContractMissingError` render thanh UI message ro rang.
- `ENV_SETUP.md` va `FRONTEND_TEST_CHECKLIST.md` co the dung de setup/test.

## Risks

- Tao HTTP adapter that cho route missing.
- Dung fake data ma khong gan nhan backend-dependent/backend-missing.
- Dat payment/admin/post-MVP route cu the vao Phase 8 thay vi Phase 9/10/11.
