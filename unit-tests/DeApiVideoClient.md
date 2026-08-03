# DeApiVideoClient - Unit Test Cases

## Function Code: VC-01 | Function Name: TryExtractVideoUrl

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 17 |
| Lack of test cases | 0 |
| Test requirement | Parses video URL from de-api JSON response; returns null when URL field missing |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;JSON response contains resultUrl field with valid URL | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;JSON response missing resultUrl field | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;jsonResponse | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ({"resultUrl":"https://video.cdn.com/output.mp4"}) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ({"status":"processing"}, no URL field) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (video URL string returned) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;null (no resultUrl field in response) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Video URL extracted successfully" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"TryExtractVideoUrl: resultUrl field not found in response" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |