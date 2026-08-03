# SocialTokenProtector - Unit Test Cases

## Function Code: TP-00 | Function Name: TokenEncryption (Protect + Unprotect)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 16 |
| Lack of test cases | 0 |
| Test requirement | Encrypts/decrypts social tokens using DataProtection; verifies round-trip and ciphertext difference from plaintext |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 2 | 0 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;DataProtection provider initialized with temp key directory | O | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;plaintext | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("social-secret", social token string) | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;ciphertext | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (output from Protect call, encrypted string) | O | |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Protect: ciphertext != plaintext (encrypted output different from input) | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Unprotect: returns original plaintext "social-secret" (round-trip verified) | O | |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Token round-trip: Protect + Unprotect successful" | O | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |