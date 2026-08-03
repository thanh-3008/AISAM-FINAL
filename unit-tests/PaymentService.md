# PaymentService - Unit Test Cases

## Function Code: PM-01 | Function Name: CreateCheckoutAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 65 |
| Lack of test cases | 0 |
| Test requirement | Creates PayOS checkout for subscription or credit pack purchase; handles missing config, invalid plan, workspace not found, and PayOS API errors |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 7 | 0 | 0 | 3 | 4 | 0 | 7 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PayOS config missing (ClientId/ApiKey/ChecksumKey unset) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PayOS config fully configured | | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace exists, user is member | | O | O | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace not found in DB | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PayOS API available | | O | O | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PayOS API returns network error | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PlanCode | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Plus) | O | O | | O | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Premium) | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;PaymentType | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CreditPack (credit pack purchase) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Subscription (default) | O | O | | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;CreditPackCode | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Growth (1500 credits) | | | O | | | | |
| **Confirm** | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 503 (PAYOS_NOT_CONFIGURED) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (checkoutUrl, Payment Pending, Subscription inactive) | | O | | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (CreditPack checkoutUrl, no Subscription) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 404 (WORKSPACE_NOT_FOUND) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 502 (PAYOS_API_ERROR) | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Checkout created successfully for subscription" | | O | | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Checkout created for credit pack: Growth" | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"PayOS checkout failed: configuration missing" | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"PayOS checkout failed: workspace not found" | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"PayOS checkout failed: API error" | | | | | | | O |
| **Result** | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | A | N | N | A | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | |

---

## Function Code: PM-02 | Function Name: CreateBusinessWorkspaceCheckoutAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 35 |
| Lack of test cases | 0 |
| Test requirement | Creates business workspace payment checkout; does not create workspace before payment; validates workspace name and plan code |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 1 | 2 | 1 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PayOS configured, workspace DB empty, no subscription exists | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace name valid ("Paid Business") | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace name empty string ("") | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PlanCode=Plus (valid for Business) | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PlanCode=Free (invalid for Business) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace name boundary 255 chars | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;WorkspaceName | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Paid Business", 12 chars) | O | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;empty string ("") | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;boundary (255 chars) | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;PlanCode | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Plus) | O | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (Free) | | | O | |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (checkoutUrl, Payment Pending with PendingWorkspaceName, no workspace created) | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (workspace name required, max 255) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Business workspace requires Plus or Premium plan) | | | O | |
| &nbsp;&nbsp;Log message | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Business workspace checkout created, workspace not created yet" | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Business workspace checkout failed: name required" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Business workspace checkout failed: Free plan not allowed" | | | O | |
| **Result** | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | B |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |

---

## Function Code: PM-03 | Function Name: HandleCallbackAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 20 |
| Lack of test cases | 0 |
| Test requirement | Processes PayOS callback; validates signature before processing; handles valid and invalid callbacks |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PayOS configured, payment Pending in DB | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Callback query has signature field | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Callback query missing signature field | O | |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;orderCode | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (123, matches pending payment) | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;status | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PAID | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;signature | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;missing (not in query) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (correct PayOS signature) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (PAYOS_SIGNATURE_REQUIRED) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (payment activated, subscription activated, credits granted) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"PayOS callback rejected: signature required" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"PayOS callback processed: payment activated" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: PM-06 | Function Name: HandleWebhookAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 225 |
| Lack of test cases | 0 |
| Test requirement | Processes PayOS webhook: payment success, subscription activation/renewal, credit grants/packs, overflow rejection, idempotency, signature validation, null values in signed data, error handling |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 23 | 0 | 0 | 10 | 13 | 0 | 23 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 | UTCID14 | UTCID15 | UTCID16 | UTCID17 | UTCID18 | UTCID19 | UTCID20 | UTCID21 | UTCID22 | UTCID23 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business workspace payment Pending (WorkspaceId=null) | O | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Subscription payment Pending, linked to workspace | | O | | | | | | | | | O | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Renewal payment Pending, existing active subscription | | | O | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Credit pack payment Pending | | | | O | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Credit overflow (subscription grant > max) | | | | | O | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Credit pack overflow (> max balance) | | | | | | O | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Payment already processed (idempotent) | | | | | | | O | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Payload missing signature | | | | | | | | O | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Payload has null values in signed data | | | | | | | | | O | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Payload has invalid/tampered signature | | | | | | | | | | O | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Unknown order code (not in DB) | | | | | | | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Payment already Failed in DB | | | | | | | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Credit pack daily limit exceeded | | | | | | | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Subscription already cancelled | | | | | | | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace already deleted | | | | | | | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Multiple items in payload | | | | | | | | | | | | | | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Empty payload (no data) | | | | | | | | | | | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Premium plan business workspace checkout | | | | | | | | | | | | | | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Renewal with no existing active subscription | | | | | | | | | | | | | | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Amount mismatch (payload amount != payment amount) | | | | | | | | | | | | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Webhook status=CANCELLED (payment cancelled) | | | | | | | | | | | | | | | | | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;payload (JSON) | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (signature + orderCode + status=PAID, business ws) | O | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (signature + orderCode + status=PAID, subscription) | | O | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (signature + orderCode + status=PAID, renewal) | | | O | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (signature + orderCode + status=PAID, credit pack) | | | | O | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (signature + orderCode + status=PAID, overflow) | | | | | O | O | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (signature + orderCode + status=PAID, idempotent) | | | | | | | O | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (no signature field in JSON) | | | | | | | | O | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (signature + null fields in data) | | | | | | | | | O | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (tampered signature) | | | | | | | | | | O | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (unknown order code) | | | | | | | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (already failed payment) | | | | | | | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (daily limit exceeded) | | | | | | | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (cancelled subscription) | | | | | | | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (deleted workspace) | | | | | | | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (multiple items) | | | | | | | | | | | | | | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (empty payload) | | | | | | | | | | | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Premium plan) | | | | | | | | | | | | | | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (renewal without active sub) | | | | | | | | | | | | | | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (amount mismatch) | | | | | | | | | | | | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (status=CANCELLED) | | | | | | | | | | | | | | | | | | | | | | | O |
| **Confirm** | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (workspace created, Business type, Active, 10 members, owner, subscription Plus, credits 15000) | O | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Payment=Success, Subscription active, MemberLimit=50, credits 50000) | | O | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (prev sub deactivated, renewal active, workspace unarchived, credits 15000) | | | O | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Payment=Success, credits added, sub unchanged, expiry unchanged) | | | | O | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Payment=Failed, sub inactive, balance unchanged) | | | | | O | O | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (idempotent, no changes) | | | | | | | O | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 400 (PAYOS_SIGNATURE_REQUIRED) | | | | | | | | O | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (signature accepted despite null fields) | | | | | | | | | O | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 400 (INVALID_SIGNATURE) | | | | | | | | | | O | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 404 (ORDER_NOT_FOUND) | | | | | | | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Payment already Failed, no further action) | | | | | | | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 429 (DAILY_CREDIT_LIMIT_EXCEEDED) | | | | | | | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Subscription already cancelled) | | | | | | | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Workspace already deleted) | | | | | | | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 400 (MULTIPLE_ITEMS_NOT_SUPPORTED) | | | | | | | | | | | | | | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 400 (EMPTY_PAYLOAD) | | | | | | | | | | | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Premium plan, credits 100000) | | | | | | | | | | | | | | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (No active subscription to renew) | | | | | | | | | | | | | | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error 400 (AMOUNT_MISMATCH) | | | | | | | | | | | | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Payment cancelled, subscription deactivated, credits not granted) | | | | | | | | | | | | | | | | | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: business workspace created and activated" | O | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: subscription activated, credits granted" | | O | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: subscription renewed, previous deactivated" | | | O | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: credit pack added, credits +1500" | | | | O | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: credit grant rejected, exceeds maximum balance" | | | | | O | O | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: idempotent, payment already processed" | | | | | | | O | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: signature required" | | | | | | | | O | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: signature validated with null data fields" | | | | | | | | | O | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: invalid signature" | | | | | | | | | | O | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: order not found" | | | | | | | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: payment already failed, skipped" | | | | | | | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: daily credit limit exceeded" | | | | | | | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: subscription already cancelled" | | | | | | | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: workspace deleted" | | | | | | | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: multiple items not supported" | | | | | | | | | | | | | | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: empty payload" | | | | | | | | | | | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: Premium workspace created" | | | | | | | | | | | | | | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: no active subscription to renew" | | | | | | | | | | | | | | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook rejected: amount mismatch" | | | | | | | | | | | | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Webhook: payment cancelled, subscription deactivated" | | | | | | | | | | | | | | | | | | | | | | | O |
| **Result** | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | N | A | A | N | A | N | A | A | A | A | A | A | A | A | A | N | A | N | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | | | | | | | | | | | | | | | | |

---

## Function Code: PM-07 | Function Name: SynchronizeBusinessWorkspaceCheckoutAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 25 |
| Lack of test cases | 0 |
| Test requirement | Synchronizes business workspace checkout; validates idempotency, rejects pending payments, handles invalid transaction references |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Payment already Success (checkout completed via webhook) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Payment still Pending (payment not yet confirmed) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Transaction reference not found in DB | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;transactionId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (matches completed Payment.TransactionId) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (matches pending Payment.TransactionId) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (not found in DB) | | | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (true, Payment unchanged, WorkspaceId unchanged, Subscription unchanged) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Payment not completed, cannot sync pending checkout) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Transaction reference not found) | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Business workspace sync: already completed, idempotent return" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Business workspace sync failed: payment still pending" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Business workspace sync failed: transaction not found" | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |