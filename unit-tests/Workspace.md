# Workspace - Unit Test Cases

## Function Code: WS-01 | Function Name: CreateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 40 |
| Lack of test cases | 0 |
| Test requirement | Create workspace with owner membership, credit wallet, subscription; reject business without payment; reject duplicate personal workspace |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User exists in DB (da dang ky, co UserId hop le) | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;No workspace exists for this user yet | O | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;No successful payment for business plan | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User already has one personal workspace in DB | | | O | |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UserId | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (GUID of existing user) | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Name | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Personal workspace", chuoi khong rong) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Unpaid Business", chuoi khong rong) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Another Personal", chuoi khong rong) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;boundary 255 chars (chuoi dung 255 ky tu, do dai toi da cho phep) | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;WorkspaceType | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Personal (WorkspaceTypeEnum.Personal, free tier) | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Business (WorkspaceTypeEnum.Business, requires payment) | | O | | |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = true, workspace created with 1 owner member (Role=Owner, UserId matches), MemberLimit=1, credit wallet created (Balance=50), subscription + credit usage record created | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = false, StatusCode=409 Conflict, ErrorCode="BUSINESS_WORKSPACE_PAYMENT_REQUIRED", no workspace created in DB | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = false, StatusCode=409 Conflict, ErrorCode="PERSONAL_WORKSPACE_LIMIT_REACHED", only 1 workspace remains in DB | | | O | |
| &nbsp;&nbsp;Exception | | | | |
| &nbsp;&nbsp;Log message | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Personal workspace created, owner membership assigned" | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Business workspace creation rejected: payment required" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Personal workspace limit reached: user already has 1 personal workspace" | | | O | |
| **Result** | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |

---

## Function Code: WS-02 | Function Name: GetByUserIdAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 20 |
| Lack of test cases | 0 |
| Test requirement | Retrieve all workspaces user participates in; reject non-member access; synchronize expired workspace lifecycle status |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 2 | 1 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User exists in DB, participates in 2 workspaces (1 personal as owner, 1 business as owner) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace exists but user is NOT a member (not invited) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business workspace with expired subscription (SubscriptionExpiredAt = 100 days ago) | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;User is the owner of the target workspace | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UserId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (GUID of existing user, member of target workspaces) | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;WorkspaceId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (GUID of existing workspace user is member of) | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (GUID of existing workspace user is NOT member of) | | O | |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = true, Data contains 2 workspaces, all with CurrentUserRole=Owner, one workspace has MemberLimit=1 (personal) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = false, StatusCode=404 NotFound (non-member cannot access workspace) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = true, Data.Status = Archived (WorkspaceStatusEnum.Archived), Data.ArchivedAt is not null (lifecycle synchronized) | | | O |
| &nbsp;&nbsp;Exception | | | |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Fetched 2 workspaces for user" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Workspace not found: user is not a member" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Expired workspace lifecycle synchronized to Archived" | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: WS-03 | Function Name: UpdateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 25 |
| Lack of test cases | 0 |
| Test requirement | Allow owner to rename workspace; forbid non-owner updates; reject updates on expired/read-only workspaces |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business workspace exists in DB, owner created it, manager is a member with Role=Manager (not Owner) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business workspace exists in DB, requesting user is the owner (Role=Owner) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business workspace exists but subscription expired (SubscriptionExpiredAt = 1 day ago), requesting user is owner | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;WorkspaceId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (GUID of existing business workspace) | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;UserId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid but non-owner (Manager role, not authorized to update) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid owner (Role=Owner, authorized to update) | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Name | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid new name ("Changed", khac ten cu) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid new name ("After", khac ten cu "Before") | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid new name ("Blocked", khac ten cu) | | | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = false, StatusCode=403 Forbidden (non-owner cannot update workspace) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = true, Data.Name = "After", workspace name updated in DB to "After" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = false, StatusCode=403 Forbidden, ErrorCode="WORKSPACE_READ_ONLY" (expired workspace cannot be modified) | | | O |
| &nbsp;&nbsp;Exception | | | |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Workspace update rejected: user is not the workspace owner" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Workspace renamed successfully to 'After'" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Cannot update read-only workspace: subscription expired" | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | A | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: WS-04 | Function Name: AdminSoftDeleteAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 30 |
| Lack of test cases | 0 |
| Test requirement | Admin soft-deletes workspace expired > 180 days; reject deletion before eligibility threshold; reject non-admin caller |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace expired 181 days ago (eligible for deletion, > 180 days threshold) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace expired 179 days ago (NOT eligible, < 180 days threshold) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has Admin role (Role=Admin, authorized to soft-delete) | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has normal User role (Role=User, KHONG du quyen soft-delete) | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;WorkspaceId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace expired 181 days, eligible) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace expired 179 days, not eligible) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;UserId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid Admin (GUID of Admin user, authorized) | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid non-Admin (GUID of normal User, KHONG du quyen) | | | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = true, Status = Deleted, DeletedAt not null, GetByIdAsync returns null | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = false, StatusCode=409 Conflict, Status unchanged (Archived) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Result.Success = false, StatusCode=403 Forbidden (non-admin user) | | | O |
| &nbsp;&nbsp;Exception | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;ForbiddenException (non-admin attempted soft-delete) | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Workspace soft-deleted by admin" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Cannot delete workspace: not yet eligible (179 days < 180 threshold)" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Admin role required for workspace deletion" | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |