## Function Code: QT-01 | Function Name: GetWorkspaceSummaryAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 20 |
| Lack of test cases | 0 |
| Test requirement | Verify that GetWorkspaceSummaryAsync returns derived usage, quota limits, and remaining counts using only active workspace member usage |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 1 | 0 | 0 | 1 | 0 | 0 | 1 |

| | UTCID01 |
|---|---|
| **Condition** | |
| &nbsp;&nbsp;Precondition | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace exists (Business type, 2 active members: Owner + Member) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Active subscription exists (Premium plan, QuotaAIContentPerDay=200, window June 1-30 2026) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace prompt usage=5, workspace post usage=15 (mock count) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile records exist for owner and member users | O |
| &nbsp;&nbsp;Input Fields | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (existing workspace with active subscription) | O |
| **Confirm** | |
| &nbsp;&nbsp;Return | |
| &nbsp;&nbsp;&nbsp;&nbsp;GenericResponse with Success=true, Data not null | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Data.PostUsage=15 | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Data.PostQuotaLimit=20000 | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Data.PostRemaining=19985 | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Data.PromptUsage=5 | O |
| &nbsp;&nbsp;Exception | |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception thrown | O |
| &nbsp;&nbsp;Log message | |
| **Result** | |
| &nbsp;&nbsp;Type(N/A/B) | N |
| &nbsp;&nbsp;Passed/Failed | P |
| &nbsp;&nbsp;Executed Date | |
| &nbsp;&nbsp;Defect ID | |

---

## Function Code: QT-02 | Function Name: EnsureWorkspacePostQuotaAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 12 |
| Lack of test cases | 0 |
| Test requirement | Verify that EnsureWorkspacePostQuotaAsync returns Forbidden with POST_QUOTA_EXCEEDED error when workspace post usage exceeds quota |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 1 | 0 | 0 | 0 | 1 | 0 | 1 |

| | UTCID01 |
|---|---|
| **Condition** | |
| &nbsp;&nbsp;Precondition | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace exists (Personal type, 1 active member) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Active subscription exists (Free plan, QuotaAIContentPerDay=1, window June 1-7 2026) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace post usage=20 (Free plan allows only 1, so quota exceeded) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace prompt usage=0 | O |
| &nbsp;&nbsp;Input Fields | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (existing workspace with exceeded post quota) | O |
| **Confirm** | |
| &nbsp;&nbsp;Return | |
| &nbsp;&nbsp;&nbsp;&nbsp;GenericResponse with Success=false, StatusCode=403 (Forbidden) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error.ErrorCode="POST_QUOTA_EXCEEDED" | O |
| &nbsp;&nbsp;Exception | |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception thrown | O |
| &nbsp;&nbsp;Log message | |
| **Result** | |
| &nbsp;&nbsp;Type(N/A/B) | A |
| &nbsp;&nbsp;Passed/Failed | P |
| &nbsp;&nbsp;Executed Date | |
| &nbsp;&nbsp;Defect ID | |

---

## Function Code: QT-03 | Function Name: EnsurePromptQuotaAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 10 |
| Lack of test cases | 0 |
| Test requirement | Verify that EnsurePromptQuotaAsync returns Forbidden with PROMPT_QUOTA_EXCEEDED error code when daily prompt quota is exceeded |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 1 | 0 | 0 | 0 | 1 | 0 | 1 |

| | UTCID01 |
|---|---|
| **Condition** | |
| &nbsp;&nbsp;Precondition | |
| &nbsp;&nbsp;&nbsp;&nbsp;Active subscription exists (Free plan, QuotaAIContentPerDay=1, window June 1-30 2026) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Current prompt usage=1 (equals daily quota limit) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Current post usage=0 | O |
| &nbsp;&nbsp;Input Fields | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with exceeded prompt quota) | O |
| **Confirm** | |
| &nbsp;&nbsp;Return | |
| &nbsp;&nbsp;&nbsp;&nbsp;GenericResponse with Success=false, StatusCode=403 (Forbidden) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error.ErrorCode="PROMPT_QUOTA_EXCEEDED" | O |
| &nbsp;&nbsp;Exception | |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception thrown | O |
| &nbsp;&nbsp;Log message | |
| **Result** | |
| &nbsp;&nbsp;Type(N/A/B) | A |
| &nbsp;&nbsp;Passed/Failed | P |
| &nbsp;&nbsp;Executed Date | |
| &nbsp;&nbsp;Defect ID | |

---

## Function Code: QT-04 | Function Name: EnsurePostQuotaAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 10 |
| Lack of test cases | 0 |
| Test requirement | Verify that EnsurePostQuotaAsync returns Forbidden with POST_QUOTA_EXCEEDED error code when monthly post quota is exceeded |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 1 | 0 | 0 | 0 | 1 | 0 | 1 |

| | UTCID01 |
|---|---|
| **Condition** | |
| &nbsp;&nbsp;Precondition | |
| &nbsp;&nbsp;&nbsp;&nbsp;Active subscription exists (Free plan, QuotaPostsPerMonth=1, window June 1-30 2026) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Current post usage=1 (equals monthly quota limit) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Current prompt usage=0 | O |
| &nbsp;&nbsp;Input Fields | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with exceeded post quota) | O |
| **Confirm** | |
| &nbsp;&nbsp;Return | |
| &nbsp;&nbsp;&nbsp;&nbsp;GenericResponse with Success=false, StatusCode=403 (Forbidden) | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error.ErrorCode="POST_QUOTA_EXCEEDED" | O |
| &nbsp;&nbsp;Exception | |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception thrown | O |
| &nbsp;&nbsp;Log message | |
| **Result** | |
| &nbsp;&nbsp;Type(N/A/B) | A |
| &nbsp;&nbsp;Passed/Failed | P |
| &nbsp;&nbsp;Executed Date | |
| &nbsp;&nbsp;Defect ID | |
