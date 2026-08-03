# AdminService - Unit Test Cases

## Function Code: AD-01 | Function Name: GetUsersAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 10 |
| Lack of test cases | 0 |
| Test requirement | Admin: paginated user list with search |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has Admin role, users exist in system | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has normal User role (not Admin) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PagedResult (paginated list of user records) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Admin role required" | | O |
| &nbsp;&nbsp;Exception | | |
| &nbsp;&nbsp;Log message | | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: AD-02 | Function Name: SetUserStatusAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 19 |
| Lack of test cases | 0 |
| Test requirement | Admin: toggle user email verification (sets IsEmailVerified flag, NOT a general active/deactive status) |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 2 | 1 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, target user exists, currently isActive=true | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, target user exists, currently isActive=false | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has normal User role (not Admin) | | | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (IsEmailVerified set to true) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (IsEmailVerified set to false) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Admin role required" | | | O |
| &nbsp;&nbsp;Exception | | | |
| &nbsp;&nbsp;Log message | | | |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: AD-03 | Function Name: SetUserRoleAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 21 |
| Lack of test cases | 0 |
| Test requirement | Admin: change user role (User/Vendor/Admin). Note: code does direct cast (UserRoleEnum)role with NO enum validation; invalid int values are cast directly. |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, changing another user's role | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has normal User role (not Admin) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, attempting to change own role | | | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (target user role updated successfully) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Admin role required" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Cannot change your own role" | | | O |
| &nbsp;&nbsp;Exception | | | |
| &nbsp;&nbsp;Log message | | | |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: AD-04 | Function Name: GetWorkspacesAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 11 |
| Lack of test cases | 0 |
| Test requirement | Admin: list all workspaces with type filter |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 2 | 0 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, multiple workspaces exist of various types | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Filter by WorkspaceType=Business applied | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PagedResult (all workspaces, unfiltered) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;PagedResult (only Business type workspaces) | | O |
| &nbsp;&nbsp;Exception | | |
| &nbsp;&nbsp;Log message | | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: AD-05 | Function Name: SetContentStatusAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 19 |
| Lack of test cases | 0 |
| Test requirement | Admin: moderate content (flag/approve/reject) |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, target content exists in database | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Target content not found in database | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has normal User role (not Admin) | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Status | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;1 (ContentStatus=Approved) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;2 (ContentStatus=Flagged) | | O | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (content status updated to Approved) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Content not found" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Admin role required" | | | O |
| &nbsp;&nbsp;Exception | | | |
| &nbsp;&nbsp;Log message | | | |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: AD-06 | Function Name: GetUserDetailAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 35 |
| Lack of test cases | 0 |
| Test requirement | Admin: get user detail with workspaces, sessions, subscriptions |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has Admin role, target user exists with data | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has normal User role (not Admin) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UserDetailDto (workspaces, sessions, subscriptions included) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Admin role required" | | O |
| &nbsp;&nbsp;Exception | | |
| &nbsp;&nbsp;Log message | | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: AD-07 | Function Name: DeleteUserAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 22 |
| Lack of test cases | 0 |
| Test requirement | Admin: delete user account (blocks only when target user.Role == Admin; no self-deletion check exists) |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, target user has normal User role | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Target user has Admin role (cannot be deleted) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Target user not found in database | | | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (normal user deleted successfully) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Cannot delete an admin user" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "User not found" | | | O |
| &nbsp;&nbsp;Exception | | | |
| &nbsp;&nbsp;Log message | | | |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: AD-08 | Function Name: SetWorkspaceStatusAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 25 |
| Lack of test cases | 0 |
| Test requirement | Admin: change workspace status (Active/Limited/Archived) |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, valid status transition requested | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Invalid status value 999 (out of valid enum range) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has normal User role (not Admin) | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Status | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;2 (WorkspaceStatus=Limited) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;999 (invalid, out of valid enum range) | | O | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (workspace status changed to Limited) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Invalid workspace status" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Admin role required" | | | O |
| &nbsp;&nbsp;Exception | | | |
| &nbsp;&nbsp;Log message | | | |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: AD-09 | Function Name: GetWorkspaceDetailAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 30 |
| Lack of test cases | 0 |
| Test requirement | Admin: get workspace detail with members, posts |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, workspace exists with members and posts | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace not found in database | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;WorkspaceDetailDto (members list, posts list included) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Workspace not found" | | O |
| &nbsp;&nbsp;Exception | | |
| &nbsp;&nbsp;Log message | | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: AD-10 | Function Name: GetPaymentsAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 25 |
| Lack of test cases | 0 |
| Test requirement | Admin: list all payments with status filter |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 2 | 0 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, payment records exist across multiple statuses | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Filter by payment status=Completed applied | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PagedResult (all payment records, unfiltered) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;PagedResult (only Completed payments) | | O |
| &nbsp;&nbsp;Exception | | |
| &nbsp;&nbsp;Log message | | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: AD-11 | Function Name: GetAllContentAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 22 |
| Lack of test cases | 0 |
| Test requirement | Admin: list all content for moderation |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 2 | 0 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, 50 content items exist across various statuses | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Filter by content status=Flagged applied | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;PagedResult (50 content items, all statuses) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;PagedResult (only Flagged content items) | | O |
| &nbsp;&nbsp;Exception | | |
| &nbsp;&nbsp;Log message | | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: AD-12 | Function Name: DeleteWorkspaceAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 20 |
| Lack of test cases | 0 |
| Test requirement | Admin: hard delete workspace (no status check — any workspace can be deleted regardless of status) |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, workspace exists in database | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller has normal User role (not Admin) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (workspace permanently deleted) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Admin role required" | | O |
| &nbsp;&nbsp;Exception | | |
| &nbsp;&nbsp;Log message | | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: AD-13 | Function Name: DeleteContentAsync

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 18 |
| Lack of test cases | 0 |
| Test requirement | Admin: delete content (moderation) |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Caller is Admin, target content exists in database | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Target content not found in database | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (content permanently deleted) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Content not found" | | O |
| &nbsp;&nbsp;Exception | | |
| &nbsp;&nbsp;Log message | | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---
