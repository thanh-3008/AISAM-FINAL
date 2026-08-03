# AutomationService - Unit Test Cases

## Function Code: AT-01 | Function Name: ImportCsvAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 17 |
| Lack of test cases | 0 |
| Test requirement | Imports CSV file, parses rows into automation items; handles empty CSV and invalid format |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Valid CSV with header and 10 data rows, brand exists in workspace | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;CSV has only header row (0 data rows) | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;file (CSV) | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (CSV with header + 10 data rows, correct columns) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (CSV with only header, no data rows) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (AutomationPlanDto with 10 items, status=AwaitingConfirmation) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (CSV must contain header and at least one data row) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"CSV imported: 10 items parsed, status AwaitingConfirmation" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"CSV import failed: no data rows found" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: AT-02 | Function Name: CreateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 29 |
| Lack of test cases | 0 |
| Test requirement | Splits one row into platform-specific items with stable unique keys; validates workspace and brand |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 2 | 1 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace and brand valid, data row with multiple platforms specified | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Single platform specified (only Facebook) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand not found in workspace | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;row data | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (multi-platform: Facebook+TikTok+Instagram) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (single platform: Facebook only) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (with invalid brand reference) | | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (3 platform items created with stable keys, status=AwaitingConfirmation) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (1 platform item created) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (brand not found in workspace, row skipped) | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Automation created: 3 platform items with unique keys" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Automation created: 1 platform item" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Automation create: brand not found, row skipped" | | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: AT-03 | Function Name: ConfirmAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 23 |
| Lack of test cases | 0 |
| Test requirement | Confirms automation plan; validates status and credits; handles invalid content rejection |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Plan status=AwaitingConfirmation, valid items, sufficient credits | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Plan status=Generating (not AwaitingConfirmation) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Insufficient credits for confirmation | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;planId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (AwaitingConfirmation plan, credits available) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (already Generating, cannot confirm twice) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (AwaitingConfirmation plan, credits insufficient) | | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (plan status=Generating, credits reserved, items validated) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (plan not in AwaitingConfirmation status) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (insufficient credits for automation plan) | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Automation confirmed: generating items, credits reserved" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Confirm failed: plan not in AwaitingConfirmation status" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Confirm failed: insufficient credits" | | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: AT-04 | Function Name: UpdateItemAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 23 |
| Lack of test cases | 0 |
| Test requirement | Revalidates invalid item before re-confirmation; handles already valid items and workspace ownership |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Item was invalid (NeedsAttention), now corrected by user | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Item is already valid (no validation needed) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Item belongs to another workspace | | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;itemId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (NeedsAttention item, corrected data) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (already valid item, no changes needed) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (item from different workspace) | | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (item revalidated, status changed to Valid) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (item already valid, no changes applied) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (item not found in workspace) | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Item updated: revalidation passed, status=Valid" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Item already valid, skipped revalidation" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"UpdateItem failed: item not in workspace" | | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |