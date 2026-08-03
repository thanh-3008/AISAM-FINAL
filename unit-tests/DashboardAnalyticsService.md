# DashboardAnalyticsService - Unit Test Cases

## Function Code: DB-01 | Function Name: GetWorkspaceSummaryAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 20 |
| Lack of test cases | 0 |
| Test requirement | Returns workspace-scoped dashboard KPI cards (credits, posts, AI usage); validates workspace isolation |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace has content, posts, credits activity | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Requesting user is not a workspace member | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace with active data, user is member) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is NOT member) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (QuotaSummaryDto with workspace-only counts for credits, posts, schedules, AI usage) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (access denied, user not a workspace member) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Workspace summary fetched: credits=X, posts=Y" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetWorkspaceSummary failed: user not a member" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: DB-02 | Function Name: GetSummaryAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 47 |
| Lack of test cases | 0 |
| Test requirement | Returns profile-scoped dashboard KPI cards with counts for content, posts, upcoming schedules, unread notifications, and credit balance |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 0 | 0 | 4 | 1 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has content (5), posts (3), upcoming schedules (2), unread notifications (4), credits (100) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has no content or activity (new user) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has content but no upcoming schedules | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Multiple brands in profile, content across all brands | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile not found or deleted | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (active profile with data) | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (active profile, no data) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (deleted or not found profile) | | | | | O |
| **Confirm** | | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (SummaryDto: 5 content, 3 posts, 2 upcoming, 4 unread, 100 credits) | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (all zeros for empty profile) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (profile not found or deleted) | | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Dashboard summary: 5 content, 3 posts, 2 upcoming, 4 unread, 100 credits" | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Dashboard summary: new profile, all zeros" | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetSummary failed: profile not found" | | | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |