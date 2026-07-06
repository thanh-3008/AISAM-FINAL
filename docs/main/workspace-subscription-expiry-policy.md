# Workspace Subscription Expiry Policy

Status: Approved product policy, implementation audit required  
Approved: 2026-06-24  
Scope: Personal Workspace, Business Workspace, subscription entitlements, credit retention, and expiry lifecycle

This document is the canonical policy for workspace creation and subscription expiry. It supersedes any older statement that all expired workspaces may continue using Free/basic features.

## 1. Core principles

1. A subscription determines which features a workspace may use.
2. Credits pay for eligible AI operations; credits never unlock a feature by themselves.
3. Every account has exactly one Personal Workspace.
4. An account may own or join multiple Business Workspaces.
5. Personal has a Free tier. Business has no Free tier.
6. Data and purchased/remaining credits are not deleted merely because a subscription expires.

## 2. Workspace creation

### Personal Workspace

- Created automatically when the account is registered for the first time.
- An account cannot create a second Personal Workspace.
- Starts with Personal Free entitlements and the approved Free credit/reset policy.

### Business Workspace

- Creation starts from the workspace management screen at `/overview`.
- The system creates only a pending Business purchase/payment before payment succeeds; no Workspace row exists yet.
- Starting a Business purchase does not grant credits.
- Successful payment atomically creates the Business Workspace, activates the selected plan, and grants plan credits exactly once.
- The user returns to `/overview` after the create-and-pay flow completes.

## 3. Personal subscription expiry

When Personal Plus or Personal Pro expires:

- The Personal Workspace remains active.
- Effective entitlements downgrade to Personal Free.
- Remaining credits are retained.
- Credits may be spent only on AI operations included in Personal Free.
- Plus/Pro-only features are locked even when the wallet still has credits.
- The workspace may upgrade again at any time.

Personal expiry is an entitlement downgrade, not Business Limited Mode.

## 4. Business subscription expiry

Business Plus and Business Pro require an active paid subscription. There is no Business Free fallback.

Immediately after the paid subscription expires:

- The workspace enters read-only `Limited` mode.
- Data, members, ownership, and credit balance are retained.
- Credits cannot be spent while the workspace has no active Business subscription.
- Members may sign in and view permitted existing data/team information.
- Create, edit, AI generation, publish, schedule, invite, role changes, quota changes, and other write operations are blocked.
- Only the Owner may access billing and renew the subscription.

Existing lifecycle milestones remain:

| Time since expiry | Workspace state | Access |
|---|---|---|
| Less than 90 days | `Limited` | Read-only; Owner may renew |
| 90-180 days | `Archived` | Owner: view/export/renew; members: view only |
| More than 180 days | `EligibleForDeletion` | Admin may soft-delete |

Renewal restores `Active` status, restores plan entitlements, preserves old credits, and grants renewal credits once.

## 5. Credit policy and abuse prevention

- Business workspace creation never grants credits.
- Business workspaces never receive the Personal Free recurring credit grant.
- Plan credits are granted only after a verified successful subscription payment or renewal.
- Credit-pack credits do not activate or extend a subscription.
- All grants must use an idempotency key tied to payment/subscription period and grant type.
- Replaying a webhook, retrying a callback, cancelling and resubscribing, or recreating a workspace must not duplicate a grant.
- Credit balance and credit entitlement are separate checks: an operation requires both sufficient credits and an eligible active plan.
- Maximum wallet balance rules continue to apply.

Recommended ledger source values:

```text
SubscriptionGrant
CreditPackPurchase
Promotion
Refund
Adjustment
Usage
```

## 6. Entitlement decision table

| Workspace | Subscription condition | Effective access | Can spend retained credits? |
|---|---|---|---|
| Personal | Free/no paid subscription | Personal Free | Yes, Free AI features only |
| Personal | Active Plus/Pro | Paid plan entitlements | Yes, eligible plan features |
| Business | Pending payment | Payment/setup only | No |
| Business | Active Plus/Pro | Paid Business entitlements | Yes, eligible plan features |
| Business | Limited/Archived/no active plan | Read-only lifecycle access | No |

## 7. Required enforcement order

For every metered operation, backend authorization must evaluate:

1. Authentication and workspace membership.
2. Workspace lifecycle status.
3. Effective plan entitlement for the workspace type.
4. Role permission.
5. Member quota, when applicable.
6. Workspace credit balance.
7. Execute operation and debit credits only after success.

Frontend feature gates are UX only. Backend enforcement is authoritative.

## 8. Implementation requirements

- Ensure Free/basic fallback is restricted to Personal Workspace.
- Ensure expired Business Workspace cannot consume credits.
- Persist a pending Business purchase/payment without creating a Workspace before verified payment.
- Ensure no credit grant occurs on Business creation.
- Make payment/webhook credit grants idempotent.
- Show retained balance in Business Limited/Archived screens without enabling spend actions.
- Route Business creation from `/overview` and return to `/overview` after successful payment.

## 9. Minimum acceptance tests

1. Account registration creates exactly one Personal Workspace.
2. A second Personal Workspace cannot be created.
3. Business creation without payment grants zero credits and cannot use features.
4. Business payment activates the workspace and grants credits once.
5. Replayed payment callback/webhook does not grant credits twice.
6. Expired Personal Plus/Pro can use retained credits for Personal Free AI features only.
7. Expired Personal cannot use paid-only features despite having credits.
8. Expired Business retains data and wallet balance but cannot consume credits.
9. Business renewal restores access and preserves the previous balance.
10. Business workspace recreation cannot farm Free credits.
