# AISAM Playwright E2E

The suite uses the real Next.js application and deterministic browser-level API mocks. It therefore runs without Supabase, OAuth, PayOS, SMTP, or AI credentials while still exercising routing, middleware, forms, local session state, API requests, and rendered UI.

## Run

```bash
npm run build
npm run test:e2e
```

The suite starts the production server from `.next`. This is deliberate: the
Next.js development compiler can reload routes while a browser suite is
running, whereas E2E should exercise the same stable output that is deployed.

Install the Chromium binary once if needed:

```bash
npx playwright install chromium
```

To test an already-running deployment instead of starting Next.js:

```bash
E2E_BASE_URL=https://your-environment.example npm run test:e2e
```

## Critical flow inventory

Covered now: public/legal pages; login validation; user/admin login and redirects; forgot-password privacy behavior; registration validation; user/admin route isolation; workspace/profile API context; content library; manual content entry; representative user and admin page smoke coverage.

The complete product risk inventory to retain as the suite grows is: authentication and session refresh/logout; email verification/reset; workspace selection/creation/settings/deletion; members/invitations/roles/quotas/ownership transfer; brand and product CRUD/import; manual and AI content generation/media; content review/approval/rejection/restore/publish; social OAuth/targets/disconnect; calendar scheduling/rescheduling/bulk/delete; published posts; campaigns CRUD/deploy/activate/insights/bulk; automation import/generate/approve/retry/cancel; dashboard/analytics; notifications; subscription/checkout/callback/cancel; credits/quota/history; and admin users/workspaces/content/payments/plans/credits/settings/audit/health/broadcast/tools with authorization boundaries.

Third-party redirects and money-moving callbacks should be tested in a dedicated staging project with provider sandboxes; they are intentionally not executed by this deterministic CI suite.
