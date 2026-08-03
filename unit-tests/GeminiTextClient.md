# GeminiTextClient - Unit Test Cases

## Function Code: GC-01 | Function Name: GenerateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 37 |
| Lack of test cases | 0 |
| Test requirement | Generates text via Gemini API; validates API key before HTTP call; returns trimmed text; handles empty response and API errors |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini API key missing (not configured) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini API key configured, valid response with text | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini API returns empty response (no candidates) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini API returns error (500/rate limit) | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;prompt | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Generate ad copy for product X") | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;apiKey | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;null (not configured) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (configured from appsettings) | | O | O | O |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (InvalidOperationException, "Gemini API key is not configured") | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (trimmed text response from Gemini) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (empty response, no text generated) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (API failure, status code or rate limit) | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GenerateAsync failed: API key not configured" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GenerateAsync: text generated successfully" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GenerateAsync: empty response from Gemini" | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GenerateAsync failed: API error" | | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | A | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |