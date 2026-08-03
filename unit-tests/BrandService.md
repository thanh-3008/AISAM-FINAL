# BrandService - Unit Test Cases

## Function Code: BR-01 | Function Name: CreateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 45 |
| Lack of test cases | 0 |
| Test requirement | Creates brand in workspace with membership and ownership validation; validates name, checks duplicate name |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 0 | 0 | 3 | 2 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace exists, user is active member | O | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User not a member of workspace | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand name already exists in this workspace | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;ProfileId not provided (auto-create default profile) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile belongs to different user | | | O | | |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Name | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Brand A", unique name) | O | O | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (duplicate name "Brand A", already exists) | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;WorkspaceId | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is member) | O | O | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is NOT member) | | | | O | |
| **Confirm** | | | | | |
| &nbsp;&nbsp;Return | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (BrandResponseDto created, BrandId returned) | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (default profile auto-created as "Workspace Profile") | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (profile does not belong to user) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (user not a workspace member) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (brand name already exists in this workspace) | | | | | O |
| &nbsp;&nbsp;Log message | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Brand created in workspace: Brand A" | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Brand creation failed: profile ownership mismatch" | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Brand creation failed: not a workspace member" | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Brand creation failed: duplicate name in workspace" | | | | | O |
| **Result** | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |

---

## Function Code: BR-02 | Function Name: BrandQuery (GetByIdAsync + GetPagedByWorkspaceIdAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 45 |
| Lack of test cases | 0 |
| Test requirement | Get brand by ID with cross-workspace boundary validation; paginated brand list scoped to workspace |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 0 | 0 | 3 | 2 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand in workspace, user is member (GetByIdAsync: normal) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand from different workspace (GetByIdAsync: cross-workspace rejection) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace has 10 brands (GetPagedByWorkspaceIdAsync: normal) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace has 0 brands (GetPagedByWorkspaceIdAsync: empty) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;User not member of workspace (GetPagedByWorkspaceIdAsync: rejected) | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;brandId / workspaceId | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (brand in workspace, user is member) | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (brand from other workspace) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user NOT member) | | | | | O |
| **Confirm** | | | | | |
| &nbsp;&nbsp;Return | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetByIdAsync: Success (BrandResponseDto) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetByIdAsync: Error (NotFound, cross-workspace) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetPagedByWorkspaceIdAsync: Success (PagedResult 10 brands) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetPagedByWorkspaceIdAsync: Success (empty PagedResult) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetPagedByWorkspaceIdAsync: Error (access denied) | | | | | O |
| &nbsp;&nbsp;Log message | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Brand retrieved by ID" | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetById failed: brand not found across workspace boundary" | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Brands fetched for workspace: 10 items" | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"No brands found in workspace" | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetPaged failed: user not workspace member" | | | | | O |
| **Result** | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |