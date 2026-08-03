# RemainingWorkspaceOwnership - Unit Test Cases

## Function Code: WO-00 | Function Name: WorkspaceOwnershipValidation (Columns + Queries)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 42 |
| Lack of test cases | 0 |
| Test requirement | Verifies WorkspaceId column NOT NULL on all ownership entities; validates workspace-scoped queries isolate data correctly across workspaces |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 0 | 0 | 5 | 0 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;All entity types have WorkspaceId column defined | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Content/Posts/Calendar/Conversation/Notification/Social seeded for 2 workspaces | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Legacy rows exist without WorkspaceId | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Entity type (Brand, Content, SocialAccount, etc.) | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Brand (WorkspaceId NOT NULL required) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Content (WorkspaceId NOT NULL required) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Payment (WorkspaceId NULLABLE - allowed until Business checkout) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ContentRepository.GetPagedByWorkspaceIdAsync | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Legacy rows (WorkspaceId=null, excluded from workspace queries) | | | | | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand.WorkspaceId: IsNullable=false (required column) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content.WorkspaceId: IsNullable=false | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Payment.WorkspaceId: IsNullable=true (allowed until business ws paid) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;All workspace queries return only data from requested workspace (6 entity types, each count=1) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Legacy rows (WorkspaceId=null) excluded from workspace-scoped queries | | | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"WorkspaceId column validation: Brand NOT NULL verified" | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"WorkspaceId column validation: Content NOT NULL verified" | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Payment.WorkspaceId nullable confirmed" | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Workspace query isolation: 6 entity types validated, each count=1" | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Legacy rows excluded from workspace queries" | | | | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |