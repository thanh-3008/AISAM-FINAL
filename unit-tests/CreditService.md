# CreditService - Unit Test Cases

## Function Code: CR-01 | Function Name: ConsumeCreditsAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 90 |
| Lack of test cases | 0 |
| Test requirement | Validates credit consumption: shared pool, lifetime assigned limit, monthly reset, insufficient balance, individual assigned limit, workspace membership validation |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 9 | 0 | 0 | 3 | 5 | 1 | 9 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 |
|---|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Member has SharedPool quota mode (uses workspace shared balance) | O | | | O | | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Member has LifetimeAssignedLimit (CreditLimit=50, CreditUsed=40) | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Member has MonthlyAssignedLimit (CreditLimit=100, CreditUsed=90, prev month) | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Member has IndividualAssignedLimit (CreditLimit=30, CreditUsed=25) | | | | | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Wallet balance sufficient (Balance=100/500) | O | O | O | | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Wallet balance insufficient (Balance=2) | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No credit wallet exists for workspace | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User is not a member of workspace | | | | | | | | O | |
| &nbsp;&nbsp;Input Fields | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;credits | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (25, within shared pool balance) | O | | | | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (20, exceeds lifetime limit 50, 40+20=60 > 50) | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (20, monthly reset on new cycle) | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (5, exceeds wallet balance 2) | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (5, within individual limit 30, 25+5=30) | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (1, any amount) | | | | | | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;boundary (0, zero credits consumed) | | | | | | | | | O |
| **Confirm** | | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (wallet Balance=75, shared pool consumption) | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (MEMBER_CREDIT_LIMIT_EXCEEDED, lifetime) | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (monthly reset, Balance=480, CreditUsed=20) | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (INSUFFICIENT_CREDITS, shared pool exhausted) | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (individual limit, CreditUsed=30) | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (MEMBER_CREDIT_LIMIT_EXCEEDED, individual) | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (WALLET_NOT_FOUND) | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (USER_NOT_MEMBER) | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (zero credits, no balance change) | | | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credits consumed from shared pool: 25, remaining: 75" | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit consumption rejected: lifetime limit exceeded (60/50)" | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Monthly credit limit reset, 20 credits consumed: 20/100" | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit consumption rejected: insufficient balance (need 5, have 2)" | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credits consumed from individual limit: 30/30" | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit consumption rejected: individual limit exceeded" | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit consumption rejected: wallet not found" | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit consumption rejected: user not workspace member" | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Zero credits consumed, no balance change" | | | | | | | | | O |
| **Result** | | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | A | N | A | A | A | B |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | | |

---

## Function Code: CR-02 | Function Name: GrantSubscriptionCreditsAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 40 |
| Lack of test cases | 0 |
| Test requirement | Validates subscription credit grants for personal plans with max balance overflow rejection; successful grants for Free and Plus plans |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 3 | 1 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Personal workspace, wallet Balance=14500, Premium plan grants 2000 (overflow: 16500 > 15000 max) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Personal workspace, wallet Balance=0, Free plan grants 50 (within 15000 max) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Personal workspace, wallet Balance=0, Plus plan grants 500 (within max) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business workspace, wallet Balance=490000, Premium grants 20000 (overflow: 510000 > 500000) | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;WorkspaceType | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Personal (max balance=15000) | O | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Business (max balance=500000) | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;plan | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Premium (2000 credits for Personal) | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Free (50 credits) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Plus (500 credits) | | | O | |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (CREDIT_BALANCE_LIMIT_EXCEEDED, Balance=14500 unchanged) | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Balance=50, CreditUsageRecord Action=SubscriptionGrant) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Balance=500, CreditUsageRecord Action=SubscriptionGrant) | | | O | |
| &nbsp;&nbsp;Log message | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit grant rejected: exceeds personal max balance (16500/15000)" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Free credits granted: 50, balance: 50" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Subscription credits granted: 500, balance: 500" | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit grant rejected: exceeds business max balance (510000/500000)" | | | | O |
| **Result** | | | | |
| &nbsp;&nbsp;Type(N/A/B) | A | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |

---

## Function Code: CR-03 | Function Name: EnsureCurrentFreeCreditsAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 50 |
| Lack of test cases | 0 |
| Test requirement | Validates free credit reset at 7-day cycle, expired subscription skip, within-cycle retention, and initial wallet creation |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 0 | 0 | 3 | 2 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Free subscription Active, StartDate 8 days ago (new 7-day cycle reached) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Free subscription Active, EndDate in past (expired) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Free subscription Active, StartDate 3 days ago (still within 7-day cycle) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No credit wallet created yet (first time) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business workspace (not applicable for free credits) | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Wallet Balance=7 (carryover from previous cycle) | O | O | O | | |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Personal ws, free sub in new cycle) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Personal ws, expired free sub) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Personal ws, within cycle) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Personal ws, no wallet yet) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Business ws) | | | | | O |
| **Confirm** | | | | | |
| &nbsp;&nbsp;Return | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Balance reset to 50, SubscriptionGrant record created) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Balance unchanged=7, no reset for expired sub) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Balance unchanged=7, still within cycle) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Wallet created, Balance=50, first SubscriptionGrant) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Skipped (Business workspace, free credits not applicable) | | | | | O |
| &nbsp;&nbsp;Log message | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Free credits reset: new 7-day cycle, balance reset to 50" | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Free credits skipped: subscription expired" | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Free credits retained: still within 7-day cycle" | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Free credits: wallet created with 50 initial credits" | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Free credits skipped: business workspace" | | | | | O |
| **Result** | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |

---

## Function Code: CR-04 | Function Name: RecordUsageAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 25 |
| Lack of test cases | 0 |
| Test requirement | Validates credit usage record persistence with correct metadata, failed usage with error, and accumulation of multiple records |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 2 | 1 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No prior CreditUsageRecord for workspace/user | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Prior usage records exist (accumulation test) | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Credit consumption failed (provider error or quota) | | O | |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;action | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (GenerateText, successful consumption) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (GenerateImage, but consumption failed) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;credits | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (1 credit) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (5 credits, attempted but failed) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;status | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Success | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Failed (provider returned error) | | O | |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (CreditUsageRecord saved, Action=GenerateText, Credits=1, Status=Success) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (CreditUsageRecord saved, Action=GenerateImage, Credits=5, Status=Failed, ErrorMessage set) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;2 usage records for same workspace/user (accumulation verified) | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit usage recorded: GenerateText, 1 credit, Success" | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Credit usage recorded: GenerateImage, 5 credits, Failed: provider error" | | O | |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |