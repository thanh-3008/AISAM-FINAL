# OAuthStateStore - Unit Test Cases

## Function Code: OA-00 | Function Name: OAuthStateManagement (CreateAsync + ConsumeAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 40 |
| Lack of test cases | 0 |
| Test requirement | Stores OAuth state with profile/provider/expiry; consumes state once-only with profile and expiry validation |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 1 | 3 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;State created, consumed once with matching profileId (normal round-trip) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;State expired (ExpiresAt < now) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;ConsumeAsync called with different profileId | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;ConsumeAsync called twice (second call on consumed state) | | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (matching stored state) | O | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;mismatch (different from stored state) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;provider | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Facebook) | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;state (ConsumeAsync parameter) | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (existing state token) | O | O | O | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;ConsumeAsync: Success (returns state with ProfileId and Provider, then state is consumed) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;ConsumeAsync: null (state expired, not returned) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;ConsumeAsync: null (profileId mismatch, not returned) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;ConsumeAsync: null (already consumed, second call returns null) | | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"OAuth state consumed successfully" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"OAuth state expired, returning null" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"OAuth state profile mismatch, returning null" | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"OAuth state already consumed, returning null" | | | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |