# NotificationService - Unit Test Cases

## Function Code: NT-01 | Function Name: GetPagedAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 28 |
| Lack of test cases | 0 |
| Test requirement | Paginated notification list for active profile with filtering; validates profile ownership and empty results |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 2 | 1 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 10 notifications in DB | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 0 notifications | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Notifications belong to different profile | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with notifications) | O | O | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (PagedResult with 10 notifications, scoped to profile) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (empty PagedResult, TotalCount=0) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Returns only active profile notifications (cross-profile isolation) | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Notifications fetched: 10 items for active profile" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"No notifications found for profile" | | O | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: NT-02 | Function Name: MarkReadAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 17 |
| Lack of test cases | 0 |
| Test requirement | Marks single notification as read with profile ownership check |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Unread notification belongs to active profile | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Notification belongs to different profile | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;notificationId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (notification owned by requesting profile) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (notification owned by different profile) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (notification marked as IsRead=true) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (NotFound, notification not accessible by this profile) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Notification marked as read" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"MarkRead failed: notification not found for profile" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: NT-03 | Function Name: MarkAllReadAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 30 |
| Lack of test cases | 0 |
| Test requirement | Marks all notifications as read for profile; handles empty list and cross-profile isolation |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 2 | 1 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 5 unread notifications | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 0 unread notifications | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Other profile also has unread notifications | | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with unread notifications) | O | O | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (5 notifications marked as read for profile) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (no changes, 0 notifications to update) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Other profile notifications remain unread (cross-profile isolation) | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Marked all as read: 5 notifications updated" | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"No unread notifications to mark" | | O | |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: NT-04 | Function Name: GetUnreadCountAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 28 |
| Lack of test cases | 0 |
| Test requirement | Returns unread notification count for profile badge display; ensures profile isolation |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 2 | 0 | 1 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 7 unread notifications | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 0 unread notifications (all read) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile has 100 unread notifications (large count) | | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with 7 unread) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with 0 unread) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile with 100 unread) | | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (count=7) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (count=0) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (count=100) | | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Unread count: 7" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Unread count: 0" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Unread count: 100 (high volume)" | | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | B |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |