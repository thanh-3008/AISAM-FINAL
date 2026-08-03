# ScheduledPostingService - Unit Test Cases

## Function Code: SP-01 | Function Name: RunDueSchedulesAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 165 |
| Lack of test cases | 0 |
| Test requirement | Executes due schedules: publishes content, handles success/failure/quota/expired workspace, creates notifications, atomically claims schedules, blocks expired workspaces without falling back to profile publish |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 17 | 0 | 0 | 5 | 12 | 0 | 17 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 | UTCID14 | UTCID15 | UTCID16 | UTCID17 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Due schedule exists (scheduled <= now), publish succeeds | O | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Publish fails (provider error), schedule retried less than max attempts | | O | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Post quota exceeded in workspace, publish blocked | | | O | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule already completed (not re-processed) | | | | O | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace subscription expired (Expired status), schedule blocked | | | | | O | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace is Limited status, schedule blocked | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Social integration inactive or deleted | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content already deleted by user before schedule runs | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Account token expired, cannot refresh | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Token protector throws on unprotect (key missing) | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Provider returns rate limit (retry later) | | | | | | | | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Marked failed, max attempts reached (~3 attempts) | | | | | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Notification created on publish failure | | | | | | | | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Notification created on quota exceeded | | | | | | | | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Notification created on workspace expired | | | | | | | | | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Multiple due schedules processed in batch | | | | | | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Concurrent claim (atomic lock) prevents double processing | | | | | | | | | | | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;(background job, khong co input truc tiep - tu dong query DB lay due schedules) | | | | | | | | | | | | | | | | | |
| **Confirm** | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Completed, content published, Post saved | O | | | | | | | | | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Failed, attempt count incremented, error recorded | | O | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Failed, post quota exceeded error recorded | | | O | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule skipped (already completed) | | | | O | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Failed, workspace expired reason | | | | | O | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Failed, workspace limited reason | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Failed, integration inactive reason | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule skipped, content no longer exists | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Failed, token expired reason | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Failed, token decrypt error reason | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule retried (attempt count < max), not yet failed | | | | | | | | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule marked Failed, max attempts exhausted | | | | | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Failure notification saved to DB | | | | | | | | | | | | | O | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Multiple schedules processed (batch success) | | | | | | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Concurrent claim prevents double execution | | | | | | | | | | | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule executed: content published, marked Completed" | O | | | | | | | | | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule failed (attempt X/3): provider error" | | O | | | | | | | | | O | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule failed: post quota exceeded" | | | O | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule skipped: already completed" | | | | O | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule blocked: workspace expired" | | | | | O | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule blocked: workspace limited" | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule failed: integration inactive" | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule skipped: content deleted" | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule failed: account token expired" | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule failed: token decryption error" | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Failure notification created for schedule" | | | | | | | | | | | | | O | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Batch processed: X schedules executed" | | | | | | | | | | | | | | | | O | |
| **Result** | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | N | A | A | A | N | A | A | A | A | A | A | A | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | | | | | | | | | | |