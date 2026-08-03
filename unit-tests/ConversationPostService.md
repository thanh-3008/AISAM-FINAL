# ConversationPostService - Unit Test Cases

## Function Code: CV-00 | Function Name: ConversationCRUD (GetByIdAsync + SoftDeleteAsync + GetPagedAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 20 |
| Lack of test cases | 0 |
| Test requirement | Validates conversation CRUD operations with profile ownership checks: get by ID, soft delete, paginated list |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 1 | 3 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Conversation belongs to active profile (GetPagedAsync returns it) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Conversation belongs to different profile (GetByIdAsync fails) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Conversation belongs to different profile (SoftDeleteAsync fails) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Conversation already deleted (SoftDeleteAsync on deleted item) | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (active profile with conversations) | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;conversationId | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (own conversation) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (other profile conversation) | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (already deleted conversation) | | | | O |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetPagedAsync: Success (conversations scoped to profile) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetByIdAsync: Error (NotFound, conversation from other profile) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;SoftDeleteAsync: Error (NotFound, cannot delete other profile conversation) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;SoftDeleteAsync: Error (Already deleted, conversation not found) | | | | O |
| &nbsp;&nbsp;Log message | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Conversations retrieved for profile" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Conversation access denied: profile ownership mismatch" | | O | O | O |
| **Result** | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |

---

## Function Code: PS-00 | Function Name: PostCRUD (GetPagedAsync + GetByIdAsync + GetPagedByWorkspaceAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 27 |
| Lack of test cases | 0 |
| Test requirement | Validates post CRUD operations with profile and workspace ownership checks: paginated list, get by ID, workspace-scoped list |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Posts exist for active profile (GetPagedAsync returns them) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Post belongs to different profile (GetByIdAsync fails) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace has posts, user is NOT a member (GetPagedByWorkspaceAsync fails) | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId/workspaceId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with posts, or workspace where user is member) | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is NOT member) | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;postId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (own post for profile) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (other profile post) | | O | |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetPagedAsync: Success (posts scoped to profile with brand/status filters) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetByIdAsync: Error (NotFound, post from other profile) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetPagedByWorkspaceAsync: Error (access denied, user not workspace member) | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Posts retrieved: profile-scoped with filters" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Post access denied: ownership mismatch" | | O | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |