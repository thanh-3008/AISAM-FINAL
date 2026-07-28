# ProductService - Unit Test Cases

## Function Code: PR-01 | Function Name: CreateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 40 |
| Lack of test cases | 0 |
| Test requirement | Creates product with brand workspace validation, name validation, and image file handling |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand belongs to active workspace, valid product data | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand belongs to different workspace | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Product name empty or null | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Product name boundary 255 chars | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Name | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Product A", meaningful name) | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;empty string ("") | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;boundary (255 chars) | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;BrandId | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (brand in workspace) | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (brand from different workspace) | | O | | |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (ProductResponseDto created, ProductId returned) | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (brand not found in workspace) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (product name required) | | | O | |
| &nbsp;&nbsp;Log message | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Product created: Product A in workspace" | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create product failed: brand not in workspace" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create product failed: name required" | | | O | |
| **Result** | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | B |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |

---

## Function Code: PR-02 | Function Name: GetPagedAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 35 |
| Lack of test cases | 0 |
| Test requirement | Paginated product list from active workspace brands; cross-workspace isolation and brand filtering |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace has products from multiple brands | O | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace has 0 products | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User not a workspace member | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand filter applied (returns subset) | | | O | |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace with products, user is member) | O | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user NOT member) | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;brandId (filter) | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;null (all brands) | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (specific brand in workspace) | | | O | |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (PagedResult with all products from workspace brands) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (empty PagedResult, TotalCount=0) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (PagedResult filtered by brand) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (access denied, user not workspace member) | | | | O |
| &nbsp;&nbsp;Log message | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Products fetched for workspace: X items" | O | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"No products found in workspace" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetPaged failed: user not workspace member" | | | | O |
| **Result** | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |