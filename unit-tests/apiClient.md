# apiClient - Unit Test Cases

## Function Code: API-01 | Function Name: apiClient

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 25 |
| Lack of test cases | 0 |
| Test requirement | HTTP client with auto headers (Authorization Bearer, X-Workspace-Id, X-Profile-Id), 401 token refresh retry with 5min buffer, invalid GUID workspace auto-clearing, error extraction from multiple response formats |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 7 | 0 | 0 | 4 | 2 | 1 | 7 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Token valid (expiry > 5min from now), workspace selected (valid GUID in localStorage) | O | | | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Token valid, no workspace selected (X-Workspace-Id absent, X-Profile-Id present) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Token valid, workspace GUID invalid format (auto-clear from localStorage triggered) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Token expired (within 5min buffer), refresh token valid and succeeds | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Token valid, server returns 500 Internal Server Error | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Token valid, server returns 401 Unauthorized (triggers token refresh) | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Initial 401 Unauthorized received, refresh token also expired/fails | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Endpoint URL | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;GET /api/workspaces (valid route with auth) | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Request Method | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;GET (no request body) | O | O | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;POST (with valid JSON body) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Request Body | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;none (GET request, no payload) | O | O | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{name: "test-workspace"} (valid JSON payload) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Server Response Status Code | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;200 OK with valid JSON body | O | O | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;500 Internal Server Error with error body | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;401 Unauthorized (first attempt, triggers refresh) | | | | | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Token Refresh Result | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;success (new access token received, placed in storage) | | | | O | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;fail (refresh token expired, redirect to /login) | | | | | | | O |
| **Confirm** | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;{success:true, data:[workspace objects]} (200 OK parsed JSON) | O | O | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Retried response with new access token (after 401 + refresh) | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "500: Server error" thrown (non-200, handled by error interceptor) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Session expired, re-login required" thrown, redirect to /login | | | | | | | O |
| &nbsp;&nbsp;Exception | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception (success path) | O | O | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;HttpErrorResponse exception (500 status) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Refresh failure exception (redirect) | | | | | | | O |
| &nbsp;&nbsp;Headers Sent (Request) | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Authorization Bearer + X-Workspace-Id + X-Profile-Id (full context) | O | | | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Authorization Bearer + X-Profile-Id only (no workspace selected) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Authorization Bearer + X-Profile-Id (workspace GUID cleared after invalid) | | | O | | | | |
| &nbsp;&nbsp;Side Effect | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;None (normal flow) | O | O | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;localStorage workspace GUID removed (invalid GUID detected) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Access token refreshed in localStorage | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Access token cleared, redirect to /login | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Request OK with workspace context (GET /api/workspaces)" | O | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Request OK without workspace (GET /api/workspaces)" | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Invalid WS GUID detected, auto-cleared from localStorage" | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Server error 500 from endpoint POST /api/workspaces" | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"401 Unauthorized, retried with refreshed access token successfully" | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"401 Unauthorized, refresh token also failed, redirecting to /login" | | | | | | | O |
| **Result** | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | N | A | B | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | |

---

## Function Code: API-02 | Function Name: retryWithRefresh

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 15 |
| Lack of test cases | 0 |
| Test requirement | Retry a failed HTTP request with a newly refreshed access token, handle network failure on retry or expired refresh token |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;401 response received from original request, refresh token valid and returns new access token | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;401 response received, refresh token expired/null (cannot obtain new token) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;401 response received, refresh succeeds (new token obtained), retry request fails with network error | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace GUID + Profile ID present in localStorage (headers preserved on retry) | O | O | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Refresh Token (from localStorage) | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (non-expired, returns {accessToken, refreshToken} from /auth/refresh) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;expired or null (token not present or past expiry) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Original Request Config | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid request object with url, method, headers (preserved for retry) | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Retry Request Outcome | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;success (200 OK, new access token used in Authorization header) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;not attempted (refresh token failed, no retry issued) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;network error (timeout, DNS failure, or connection refused on retry) | | | O |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Parsed JSON response from successful retry (200 OK with data) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Session expired, re-login required" thrown (refresh token invalid) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Network error on retry after refresh" thrown | | | O |
| &nbsp;&nbsp;Exception | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception (successful retry) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;TokenRefreshException thrown (expired refresh token) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;NetworkException thrown (retry request failed) | | | O |
| &nbsp;&nbsp;Side Effect | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;New access token saved to localStorage | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Access token cleared from localStorage (refresh failed) | | O | |
| &nbsp;&nbsp;Log message | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Request retried successfully with new access token from refresh" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Refresh token expired, redirecting user to /login page" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Retry request failed due to network error after successful token refresh" | | | O |
| **Result** | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: API-03 | Function Name: handleResponse

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 79 |
| Lack of test cases | 0 |
| Test requirement | Parse HTTP response body, extract error messages from multiple formats (string, object with message, validationErrors array, title, detail), 401 auto-redirect to /login with token removal, handle empty response body |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 7 | 0 | 0 | 4 | 3 | 0 | 7 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;HTTP 200 OK with valid JSON body containing success and data fields | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;HTTP 400 Bad Request with error as plain string in response body | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;HTTP 400 Bad Request with error as object {error: {message: "..."}} | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;HTTP 400 Bad Request with error object containing validationErrors array {error: {validationErrors: {field: ["msg"]}}} | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;HTTP 400 Bad Request with error object containing title field {title: "..."} | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;HTTP 500 Internal Server Error with error object containing detail field {detail: "..."} | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;HTTP 500 Internal Server Error with empty response body (no JSON to parse) | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;User is on /login page (skip redirect, avoid redirect loop) | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User is NOT on /login page (perform redirect after 401) | | | O | O | | | |
| &nbsp;&nbsp;Input Fields | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;HTTP Response Status Code | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;200 (OK, successful response) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;400 (Bad Request, client error with body) | | O | O | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;500 (Internal Server Error, server error with/without body) | | | | | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Response Body Content | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{success:true, data:{workspace objects}} (valid success response) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{error: "Validation failed: name is required"} (error as string) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{error: {message: "Resource not found"}} (error as nested object) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{error: {validationErrors: {name: ["required"], email: ["invalid"]}}} (validation errors map) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{title: "Bad Request Problem"} (ProblemDetails-like format) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{detail: "An unexpected error occurred on the server"} (detail error field) | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"" (empty string, no JSON body to parse) | | | | | | | O |
| **Confirm** | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Parsed JSON data object (successful 200 response, data field extracted) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Validation failed: name is required" thrown (error string extracted) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Resource not found" thrown (error.message extracted from object) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "required" thrown (validationErrors first message extracted) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "Bad Request Problem" thrown (title field extracted) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "An unexpected error occurred on the server" thrown (detail field) | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error "500 Internal Server Error" thrown (status code used as fallback) | | | | | | | O |
| &nbsp;&nbsp;Exception | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception (successful parse) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;ClientErrorException thrown (400 with error message) | | O | O | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;ServerErrorException thrown (500 with detail or status fallback) | | | | | | O | O |
| &nbsp;&nbsp;Side Effect (401 Unauthorized) | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;None (not a 401 response) | O | | | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Access token removed from localStorage, window redirected to /login | | | O | O | | | |
| &nbsp;&nbsp;Log message | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Response 200 parsed successfully, data returned" | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Error string extracted from 400 response: Validation failed" | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Error object message extracted from 400 response + 401 redirect triggered" | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Validation errors extracted from 400 response + 401 redirect triggered" | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Title field extracted from 400 response: Bad Request Problem" | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Detail field extracted from 500 response: unexpected error" | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Empty response body, using status code 500 as error message" | | | | | | | O |
| **Result** | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | A | N | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | |

---
