# Analytics Ask AI Fix Validation Report

## 1. Task Summary

This implementation fixes confirmed Ask AI lifecycle defects and adds diagnosis telemetry. It does not change database pool configuration, PgBouncer settings, or database concurrency strategy. It does not claim that Ask AI consumed the 15 reported session clients.

Implemented scope:

- Propagated the existing request `CancellationToken` through the previously missing Ask AI totals and campaign database calls.
- Added `X-Correlation-ID` propagation from frontend to controller, service, repository-stage logs, and LLM provider logs.
- Added structured duration/outcome/cancellation logging for cache, database, prompt, LLM, parse, second-generation, and request completion stages.
- Added sequential provider-attempt telemetry and stopped fallback traversal on cancellation.
- Removed raw Gemini response logging from the Ask AI service.
- Added frontend timeout/abort/status/category telemetry.
- Aborted obsolete Ask AI requests on date-context change and component unmount.
- Prevented stale analytics refresh responses from overwriting newer request results.

## 2. Files Changed

| File | Purpose | Exact change | Risk |
|---|---|---|---|
| [AnalyticsController.cs](../AISAM-BE/AISAM.API/Controllers/AnalyticsController.cs) | Ask AI HTTP boundary | Accept or generate validated correlation ID, return it in `X-Correlation-ID`, set request trace ID, and create an `ILogger` scope. | Low; response payload contract unchanged. |
| [IAnalyticsService.cs](../AISAM-BE/AISAM.Services/IServices/IAnalyticsService.cs) | Service contract | Added optional correlation ID to the Ask AI method. | Low; existing callers remain source-compatible. |
| [AnalyticsService.cs](../AISAM-BE/AISAM.Services/Service/AnalyticsService.cs) | Ask AI orchestration | Fixed missing cancellation tokens; added stage timers, outcomes, cancellation handling, JSON parse telemetry, and sanitized EMAXCONNSESSION detection. | Medium; logging runs on every Ask AI request. |
| [FallbackTextProvider.cs](../AISAM-BE/AISAM.Services/Service/FallbackTextProvider.cs) | Provider fallback chain | Added per-provider attempt timing/classification and immediate cancellation propagation. | Medium; cancellation no longer proceeds to later fallback providers. |
| [GeminiTextClient.cs](../AISAM-BE/AISAM.Services/Service/GeminiTextClient.cs) | Gemini HTTP client | Added per-model attempt logs without logging prompts/responses. | Low. |
| [analyticsService.ts](../AISAM-FE/src/services/analyticsService.ts) | Ask AI frontend request | Added caller signal support, correlation header, timeout/abort timing, status and error classification. | Low; return type and API endpoint unchanged. |
| [AnalyticsAiInsights.tsx](../AISAM-FE/src/components/analytics/AnalyticsAiInsights.tsx) | Ask AI component lifecycle | Aborts active request on date context change/unmount and ignores obsolete results. | Low; only obsolete requests are cancelled. |
| [analytics page](../AISAM-FE/src/app/%28dashboard%29/analytics/page.tsx) | Analytics refresh lifecycle | Added request identity invalidation for effect loads and manual refreshes. | Low; concurrent unrelated users/pages are unaffected. |
| [apiClient.ts](../AISAM-FE/src/lib/apiClient.ts) | Frontend error metadata | Preserves HTTP status/category on errors and non-enumerable status on successful response objects. | Low; normal response JSON shape is unchanged. |

## 3. Confirmed Fixes

### FIXED

- The first Ask AI `GetAggregatedTotalsAsync` call now receives the request token.
- The Ask AI campaign breakdown call now receives the request token.
- All existing repository calls in the Ask AI service that support cancellation use the same request token.
- A caller-provided `X-Correlation-ID` is reused; an invalid or missing ID is replaced with one generated at the backend boundary.
- Frontend timeout abort and component/context abort are distinguished in telemetry.
- Manual refresh and filter/workspace loads cannot commit a response whose request identity is obsolete.
- Provider fallback stops on `OperationCanceledException`; it does not start additional providers after cancellation.
- Raw LLM content is no longer written to backend logs by the Ask AI service.
- No pool-size, timeout, PgBouncer, or database parallelism changes were made.

### INSTRUMENTED

Backend structured log events include:

- `AskAI.RequestStarted`
- `AskAI.CacheCheck`
- `AskAI.Database.AggregatedTotals`
- `AskAI.Database.ChannelBreakdown`
- `AskAI.Database.TopPosts`
- `AskAI.Database.CampaignBreakdown`
- `AskAI.Database.PreviousPeriod`
- `AskAI.Database.Brands`
- `AskAI.PromptBuild`
- `AskAI.LLM.PrimaryOrFallbackGeneration`
- `AskAI.LLM.SecondGeneration`
- `AskAI.LLM.ParseResponse`
- `AskAI.LLM.ProviderAttempt`
- `AskAI.LLM.ProviderModelAttempt`
- `AskAI.RequestCompleted`

Stage logs carry `CorrelationId`, `WorkspaceId`, `DurationMs`, `Outcome`, `Cancelled`, and `ExceptionType` where applicable. Provider logs carry provider name, attempt order, duration, success, failure category, and cancellation.

Frontend logs emit `[AskAI.Telemetry]` with correlation ID, ISO request start/end, duration, timeout flag, abort flag, HTTP status when available, outcome, and error category.

### NOT FIXED — REQUIRES RUNTIME EVIDENCE

- The actual consumer of the 15 session-mode clients.
- The number of production API replicas/processes and independent Npgsql pools.
- Whether the incident's timeout occurred in DB acquisition, DB command execution, Gemini, fallback, parse retry, or another layer.
- Whether production requests continued after client disconnect during the incident.
- Npgsql pool wait/active/idle metrics and Supabase/PgBouncer session metrics.

The repository has no OpenTelemetry, Prometheus, Application Insights, or Npgsql metrics integration identified in the inspected source. Immediate diagnosis therefore uses structured logs. A future metrics integration should expose per-process pool wait/active/idle counts and command/request duration histograms.

## 4. Correlation Flow

```text
Frontend fetchAiRecommendations
  -> X-Correlation-ID request header
  -> AnalyticsController reads/validates ID
  -> Response X-Correlation-ID and HttpContext.TraceIdentifier
  -> ILogger scope in controller/service
  -> AnalyticsService stage logs
  -> repository operation stage logs
  -> FallbackTextProvider provider-attempt logs
  -> GeminiTextClient model-attempt logs
  -> final response, cancellation, or exception log
```

The provider and repository services inherit the same async logging scope; no separate provider correlation ID is generated.

## 5. Ask AI Stage Timing Evidence

No live Ask AI request was executed because this workspace has no configured safe runtime/database/provider test target. Therefore no stage duration is fabricated.

Expected log shape:

```text
AskAI.Database.AggregatedTotals CorrelationId=<id> WorkspaceId=<id> DurationMs=<n> Outcome=SUCCESS Cancelled=False ExceptionType=
AskAI.LLM.PrimaryOrFallbackGeneration CorrelationId=<id> WorkspaceId=<id> DurationMs=<n> Outcome=SUCCESS Cancelled=False ExceptionType=
AskAI.LLM.ProviderAttempt ProviderName=Gemini AttemptOrder=1 DurationMs=<n> Success=True FailureCategory= Cancelled=False
AskAI.LLM.ParseResponse CorrelationId=<id> WorkspaceId=<id> DurationMs=<n> Outcome=SUCCESS Cancelled=False ExceptionType=
AskAI.RequestCompleted CorrelationId=<id> WorkspaceId=<id> DurationMs=<n> Outcome=SUCCESS Cancelled=False ExceptionType=
```

## 6. Cancellation Validation

### Calls now receiving `CancellationToken`

- `GetAggregatedTotalsAsync` in Ask AI.
- `GetCampaignBreakdownPagedAsync` in Ask AI.
- Existing channel, top-posts, previous-period, and brand repository calls continue to receive the same request token.
- Gemini HTTP POST and response content reads receive the token.
- Fallback provider calls receive the token and stop immediately when cancellation is raised.

### Calls that cannot yet be fully verified

- Browser-to-server cancellation propagation under the deployed reverse proxy has not been tested live.
- The frontend timer produces a client-side abort after 65 seconds, but no live slow-provider run was available to measure backend shutdown time.
- A non-request `HttpClient` timeout can appear as `OperationCanceledException`/`TaskCanceledException`; the new provider logs classify it as an LLM timeout, while request cancellation is classified as client cancellation.
- Some non-Ask-AI operations elsewhere in the application may still have incomplete token propagation; this task did not refactor unrelated flows.

After frontend abort, the caller signal aborts the internal controller. The backend action token is expected to be cancelled by ASP.NET Core, and the supported DB/HTTP operations now receive that token. The result still requires deployment-level verification.

## 7. EMAXCONNSESSION Status

**ROOT CAUSE STILL NOT PROVEN**

Missing evidence:

1. Supabase/PgBouncer metrics and logs for the incident window.
2. PostgreSQL activity snapshots showing session owners, states, wait events, and client addresses.
3. Effective redacted production connection strings, including pool mode and pool size.
4. API replica/container/process inventory and background-worker inventory.
5. Per-process Npgsql pool active/idle/busy/waiting metrics.
6. Correlated request logs showing whether Ask AI overlapped the rejected connection attempts.

The implementation logs sanitized detection of `EMAXCONNSESSION` or `max clients reached in session mode` as `AskAI.Database.ConnectionLimitReached`. It does not expose database connection strings or internal database details to the frontend.

## 8. Build/Test Results

| Validation | Result |
|---|---|
| `dotnet build AISAM-BE/AISAM.sln --no-restore` | **PASS**, 0 errors; one existing `PayOSPaymentService.cs` CS8601 warning. |
| Focused `GeminiTextClientTests` via test runner | **PASS**, 2/2. |
| `dotnet test ... --filter FullyQualifiedName~GeminiTextClientTests` | **PASS**, 2/2. |
| `Push-Location AISAM-FE; npm run lint` | **PASS**. |
| `Push-Location AISAM-FE; npm run build` | **PASS**; Next.js warning that `middleware` convention is deprecated. |
| `Push-Location AISAM-FE; npm test -- --run` | **PASS**, 12 files and 44 tests. |
| Controlled Ask AI live success test | **NOT RUN**; no safe configured runtime target. |
| Controlled cancellation/slow-provider test | **NOT RUN**; no safe configured runtime target. |
| Controlled EMAXCONNSESSION test | **NOT RUN**; production stress testing is not appropriate and no isolated database target was provided. |

## 9. Remaining Risks

- Sequential provider fallback can still make a failed request slow; it is now observable and cancellation-aware.
- Invalid JSON can still trigger a second generation; it is now explicitly logged.
- The frontend timeout remains 65 seconds by design; aligning end-to-end budgets requires an observed latency target.
- Production pool topology and session ownership remain unknown.
- Multiple API replicas or background services may create independent pools.
- Notification polling remains separate baseline traffic; its contribution to the incident is unmeasured.
- Current logs are sufficient for immediate diagnosis but are not a replacement for pool/request metrics.
