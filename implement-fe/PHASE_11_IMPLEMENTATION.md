# Phase 11 Implementation - Admin App Seam

Tai lieu nay mo rong Phase 11 trong `FRONTEND_CODE_PLAN.md`.

Scope theo `user_story_list.md`:

- US-51: Dang nhap voi vai tro admin.
- US-52: Xem danh sach nguoi dung trong admin.
- US-53: Quan ly profile, subscription va payment trong admin.
- US-54: Seed du lieu demo duoc bao ve.
- US-55: Chan truy cap admin voi non-admin.

Trang thai: `backend-dependent`.

Ly do:

- Auth backend tra user role trong token/current user context, nen frontend co the tao admin role gate.
- Backend local hien tai chua expose controller admin active ro rang cho users/payment/subscription/seed demo operations.
- README coi Admin Tools la target product, nen frontend can co IA va seam nhung khong goi API gia.

## Nguyen Tac

- Admin route phai protected va can role `Admin`.
- Non-admin phai thay access denied UX ro rang.
- Khong goi `/api/admin/*`, `/api/users/*`, `/api/payment/admin/*` neu controller contract chua co trong backend local.
- Admin API adapters phai throw `BackendContractMissingError`.
- User app billing Phase 9 va admin billing Phase 11 phai tach route/feature boundary.

## Tong Quan Thu Tu Lam

1. Task 11.1 - Tao admin route group va role gate
2. Task 11.2 - Tao admin dashboard shell
3. Task 11.3 - Tao admin users shell
4. Task 11.4 - Tao admin profile/subscription/payment shell
5. Task 11.5 - Tao seed demo tools shell
6. Task 11.6 - Tao admin API seam va verification docs
7. Chay verify tong Phase 11

## Task 11.1 - Admin Route Group Va Role Gate

Muc tieu:

- Co admin boundary rieng, khong tron voi user workspace.

File can tao:

```text
AISAM-FE/src/app/(admin)/admin/layout.tsx
AISAM-FE/src/app/(admin)/admin/page.tsx
AISAM-FE/src/features/admin/components/admin-layout-shell.tsx
AISAM-FE/src/features/admin/components/admin-access-denied.tsx
AISAM-FE/src/features/admin/hooks/use-admin-guard.ts
```

Role mapping:

```ts
export const userRoleValues = {
  User: 0,
  Admin: 1,
} as const
```

Neu backend role trong `/auth/me` la string thay vi number, hook phai support ca `"Admin"` va `1`.

Definition of Done:

- Chua login redirect/blocked theo auth guard chung.
- Login non-admin thay access denied.
- Admin user vao duoc admin shell.

## Task 11.2 - Admin Dashboard Shell

Cover: US-51, US-55.

File can tao:

```text
AISAM-FE/src/features/admin/components/admin-dashboard-shell.tsx
AISAM-FE/src/features/admin/components/admin-module-card.tsx
AISAM-FE/src/features/admin/config/admin-navigation.ts
```

Noi dung UI:

- Users
- Profiles
- Payments
- Subscriptions
- Demo Data
- Backend contract status

Definition of Done:

- Admin dashboard render duoc khong can admin API.
- Moi card noi ro `backend-dependent` neu action can API chua expose.

## Task 11.3 - Admin Users Shell

Cover: US-52.

Route/file can tao:

```text
AISAM-FE/src/app/(admin)/admin/users/page.tsx
AISAM-FE/src/app/(admin)/admin/users/[id]/page.tsx
AISAM-FE/src/features/admin/users/components/admin-user-list-shell.tsx
AISAM-FE/src/features/admin/users/components/admin-user-detail-shell.tsx
AISAM-FE/src/features/admin/users/api/get-admin-users.ts
AISAM-FE/src/features/admin/users/api/get-admin-user-detail.ts
AISAM-FE/src/features/admin/users/api/update-user-status.ts
```

Implementation rule:

- API files throw `BackendContractMissingError`.
- List shell co filters/search UI disabled hoac backend-dependent.
- Detail shell khong fake profile/payment/subscription data.

Definition of Done:

- `/admin/users` va `/admin/users/[id]` render shell.
- Non-admin khong vao duoc.

## Task 11.4 - Admin Profile/Subscription/Payment Shell

Cover: US-53.

Route/file can tao:

```text
AISAM-FE/src/app/(admin)/admin/profiles/page.tsx
AISAM-FE/src/app/(admin)/admin/payments/page.tsx
AISAM-FE/src/app/(admin)/admin/subscriptions/page.tsx
AISAM-FE/src/features/admin/profiles/components/admin-profile-list-shell.tsx
AISAM-FE/src/features/admin/payments/components/admin-payment-list-shell.tsx
AISAM-FE/src/features/admin/subscriptions/components/admin-subscription-list-shell.tsx
AISAM-FE/src/features/admin/profiles/api/get-admin-profiles.ts
AISAM-FE/src/features/admin/payments/api/get-admin-payments.ts
AISAM-FE/src/features/admin/subscriptions/api/get-admin-subscriptions.ts
```

Implementation rule:

- API files throw `BackendContractMissingError`.
- Payment/subscription enums reuse constants from Phase 9.
- Admin pages phai noi ro day la operations shell, khong phai user billing.

Definition of Done:

- Admin profile/payment/subscription routes render.
- Khong co fake rows nhu data production.

## Task 11.5 - Seed Demo Tools Shell

Cover: US-54.

Route/file can tao:

```text
AISAM-FE/src/app/(admin)/admin/demo-data/page.tsx
AISAM-FE/src/features/admin/demo/components/demo-data-tools-shell.tsx
AISAM-FE/src/features/admin/demo/components/demo-seed-warning.tsx
AISAM-FE/src/features/admin/demo/api/seed-demo-user.ts
AISAM-FE/src/features/admin/demo/api/seed-demo-batch.ts
```

Implementation rule:

- Seed action disabled until backend controller contract exists.
- UI phai can confirm destructive/side-effect action trong tuong lai.
- Khong tao local fake seed result.

Definition of Done:

- Admin thay duoc demo tools la planned/backend-dependent.
- Button state va warning ro rang.

## Task 11.6 - Admin API Seam Va Docs

Muc tieu:

- Chot boundary de khi backend admin controller co that thi FE noi vao khong refactor lon.

File can tao:

```text
AISAM-FE/src/features/admin/api/admin-contract.ts
AISAM-FE/src/features/admin/api/admin-contract-status.ts
AISAM-FE/src/types/admin.ts
```

Contract local nen co:

```ts
export type AdminContractStatus = {
  users: "backend-dependent" | "backend-ready"
  profiles: "backend-dependent" | "backend-ready"
  payments: "backend-dependent" | "backend-ready"
  subscriptions: "backend-dependent" | "backend-ready"
  demoData: "backend-dependent" | "backend-ready"
}
```

Docs can cap nhat:

```text
AISAM-FE/FRONTEND_TEST_CHECKLIST.md
AISAM-FE/ENV_SETUP.md
```

Noi dung can them:

- Cach login admin.
- Expected non-admin access denied.
- Admin pages backend-dependent, khong smoke API cho den khi backend expose controller.

Definition of Done:

- Admin seam ro rang, co types va status.
- Docs ghi dung limitation.

## Verify Tong Phase 11

Chay:

```text
cd AISAM-FE
pnpm lint
pnpm build
```

Manual smoke:

1. Non-auth user khong vao admin.
2. Non-admin user thay access denied.
3. Admin user vao duoc `/admin`.
4. `/admin/users`, `/admin/profiles`, `/admin/payments`, `/admin/subscriptions`, `/admin/demo-data` render shell.
5. Khong co request HTTP nao den admin endpoint chua ton tai.
6. Admin API files throw `BackendContractMissingError`.

## Deliverables Sau Phase 11

Can co toi thieu:

- `src/app/(admin)/admin/*`
- `src/features/admin/*`
- `src/types/admin.ts`
- Admin guard va access denied components.
- Admin backend-dependent API seam.

## Rui Ro Can Tranh

- Cho non-admin thay du lieu admin shell ma khong gate.
- Goi endpoint admin suy dien.
- Dung user billing adapter Phase 9 cho admin operation.
- Fake seed demo thanh cong.
- Claim admin users/payment/subscription da active khi backend local chua co controller.
