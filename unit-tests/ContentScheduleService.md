# ContentScheduleService - Unit Test Cases

## Function Code: CS-01 | Function Name: CreateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 50 |
| Lack of test cases | 0 |
| Test requirement | Creates pending schedule for content and integration; validates content/integration ownership and already published content re-scheduling |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 0 | 0 | 3 | 2 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content + integration belong to same profile (normal flow) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Integration belongs to different profile | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content already published, schedule with different integration | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content status is Draft (not published) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Scheduled date is in the past | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;contentId | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (content in profile) | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;integrationId | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (integration in same profile) | O | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (integration from different profile) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;scheduledDate | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (future date, +7 days) | O | O | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (past date, -1 day) | | | | | O |
| **Confirm** | | | | | |
| &nbsp;&nbsp;Return | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (ContentCalendar created with Pending status) | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (NotFound, integration belongs to different profile) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (past date not allowed) | | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule created: content scheduled for future date" | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create schedule failed: integration profile mismatch" | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create schedule failed: past date not allowed" | | | | | O |
| **Result** | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |

---

## Function Code: CS-02 | Function Name: ScheduleManagement (UpdateAsync + GetUpcomingAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 45 |
| Lack of test cases | 0 |
| Test requirement | Updates existing schedule with profile validation; gets upcoming future schedules for profile |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 0 | 0 | 3 | 2 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Pending schedule exists, valid update (UpdateAsync: normal) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedule already completed (UpdateAsync: rejected) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 3 upcoming schedules (GetUpcomingAsync: normal) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 0 upcoming schedules (GetUpcomingAsync: empty) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Schedules from different profile (GetUpcomingAsync: isolation) | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;scheduleId / profileId | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (pending schedule in profile) | O | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (completed schedule in profile) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with no schedules) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with other user schedules) | | | | | O |
| **Confirm** | | | | | |
| &nbsp;&nbsp;Return | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Success (schedule updated) | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Error (BadRequest, schedule already completed) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetUpcomingAsync: Success (3 future schedules for profile) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetUpcomingAsync: Success (empty list) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;GetUpcomingAsync: Only profile-scoped schedules returned | | | | | O |
| &nbsp;&nbsp;Log message | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Schedule updated successfully" | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Update schedule failed: already completed" | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Upcoming schedules fetched: 3 items" | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"No upcoming schedules found" | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Upcoming schedules: profile isolation verified" | | | | | O |
| **Result** | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |