# WorkspaceMemberService - Unit Test Cases

## Function Code: WM-01 | Function Name: GetMembersAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 12 |
| Lack of test cases | 0 |
| Test requirement | Lists workspace members; allows active member access; rejects non-members |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace has 3 members, requester is active member | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Requester not a workspace member | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is active member) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is NOT member) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (list of 3 members with roles and quota info) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (access denied, not a workspace member) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Members listed: 3 active members" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetMembers failed: user not workspace member" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: WM-02 | Function Name: UpdateRoleAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 18 |
| Lack of test cases | 0 |
| Test requirement | Updates member role; Owner-only for non-owner targets; blocks non-owners and owner-targeting |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Owner updates Manager to ContentCreator (valid) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Manager tries to update another member (not Owner, rejected) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Owner tries to update another Owner (rejected, cannot change Owner role) | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;memberId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Manager, can be changed) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (ContentCreator) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Owner, cannot be changed) | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;newRole | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (ContentCreator) | O | O | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (member role updated to ContentCreator) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Forbidden, only Owner can update roles) | | O | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Member role updated: Manager -> ContentCreator" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"UpdateRole failed: only Owner can change roles" | | O | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: WM-03 | Function Name: UpdateQuotaAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 15 |
| Lack of test cases | 0 |
| Test requirement | Assigns monthly credit limit for Business Pro members; rejects for Business Plus plan |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business Pro plan, Owner sets MonthlyAssignedLimit=100 for member | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Business Plus plan, attempt to set AssignedLimit (not supported) | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;memberId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace member) | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;monthlyLimit | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (100 credits) | O | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (QuotaMode=MonthlyAssigned, CreditLimit=100) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (assigned quota not available for this plan) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Member quota updated: Monthly 100 credits" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"UpdateQuota failed: plan does not support assigned limits" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: WM-04 | Function Name: RemoveAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 14 |
| Lack of test cases | 0 |
| Test requirement | Removes member from workspace; Owner-only for non-owner targets; blocks non-owners and allows access in limited workspace |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 2 | 1 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Owner removes ContentCreator (valid) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Manager tries to remove another member (rejected) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Limited workspace, member removal allowed, member list still accessible | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;memberId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (ContentCreator, removable) | O | O | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (member removed, member count decreased) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Forbidden, only Owner can remove members) | | O | |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Member removed from workspace" | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Remove member failed: only Owner can remove" | | O | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: WM-05 | Function Name: TransferOwnershipAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 22 |
| Lack of test cases | 0 |
| Test requirement | Transfers workspace ownership to Manager; rejects non-manager targets, non-owners, and limited workspaces |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Owner transfers to Manager (valid target role) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Owner transfers to ContentCreator (invalid target, not Manager) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Limited workspace, transfer attempted (rejected) | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;targetMemberId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Manager role, eligible for ownership) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (ContentCreator role, not eligible) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Manager, but workspace is Limited) | | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (ownership transferred, old owner becomes Manager) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (target must have Manager role) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (cannot transfer ownership in limited workspace) | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Ownership transferred to Manager, previous owner demoted" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"TransferOwnership failed: target must be Manager" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"TransferOwnership failed: workspace is limited" | | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |