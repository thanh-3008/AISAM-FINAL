# useFeatureGate - Unit Test Cases

## Function Code: FG-01 | Function Name: useFeatureGate

| | |
|---|---|
| Created By | Developer |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 85 |
| Lack of test cases | 0 |
| Test requirement | Resolve user plan type from subscription API response, sync plan name to workspace store on mismatch, evaluate feature/permission access based on plan tier, detect user roles (Owner/Viewer), handle disabled feature gate fallback, compute locked features list per plan |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 8 | 0 | 0 | 4 | 2 | 2 | 8 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Active workspace with Free plan, subscription API confirms Free, user role=Owner | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Active workspace with Personal Pro plan, subscription API confirms Personal Pro, user role=Owner | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Active workspace with Business Pro plan, subscription API confirms Business Pro, user role=Owner | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Active workspace with Free plan, subscription API confirms Free, user role=Owner (with unconfirmed sub) | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Active workspace with Free plan, subscription API confirms Free, user role=Viewer | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Subscription API fetch fails (network error), fallback to Free plan from workspace store | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Active workspace with Free plan stored, subscription API returns Personal Pro (plan mismatch/sync needed) | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;FeatureGate enabled=false (feature gates disabled, use default Free plan without subscription fetch) | | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace Plan (from workspace store) | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Free (stored in workspace store from previous session) | O | | | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Personal Pro (stored in workspace store from previous subscription) | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Business Pro (stored in workspace store from Enterprise subscription) | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Subscription API Response (from /api/subscription/current) | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{planName:"Free", status:"active"} (plan confirmed by backend) | O | | | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{planName:"Personal Pro", status:"active"} (plan confirmed by backend) | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{planName:"Business Pro", status:"active"} (plan confirmed by backend) | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Network Error (timeout/503, no response from subscription API) | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{planName:"Personal Pro", status:"active"} (mismatch: store has Free, API says Personal Pro) | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;enabled (feature gate toggle param) | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;true (feature gate is enabled, resolve plan from subscription API) | O | O | O | O | O | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;false (feature gate is disabled, skip API fetch, default to Free plan) | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;User Role (from auth/workspace membership) | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Owner (workspace owner with full permissions) | O | O | O | O | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Viewer (read-only workspace member, limited permissions) | | | | | O | | | |
| **Confirm** | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;plan = PlanType.Free, canAccess(aiImage)=false (feature locked on Free tier) | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;plan = PlanType.PersonalPro, canAccess(aiImage)=true (feature unlocked on Pro tier) | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;plan = PlanType.BusinessPro, canAccess(aiImage)=true (all features unlocked on Business) | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;plan = PlanType.Free, isOwner=true, can(manageBrand)=true (Owner has manageBrand on Free) | O | | | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;plan = PlanType.Free, isViewer=true, can(manageBrand)=false (Viewer denied manageBrand) | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;isOwner = true (user role detected as workspace Owner) | O | O | O | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;isViewer = true (user role detected as workspace Viewer) | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;isResolvingPlan = false (plan resolved, fallback to workspace store plan used) | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;plan synced from Free to PersonalPro after mismatch detected (store updated) | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;plan = PlanType.Free (default plan returned when feature gate is disabled, no API fetch) | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;getLockedFeatures() returns [aiVideo] (aiVideo feature locked on Personal Pro tier) | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;getLockedFeatures() returns [] (no features locked on Business Pro, full access) | | | O | | | | | |
| &nbsp;&nbsp;Exception | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception (plan resolved normally) | O | O | O | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;No exception (subscription API error handled gracefully, fallback used) | | | | | | O | | |
| &nbsp;&nbsp;Side Effect | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace store plan set to "Free" | O | | | O | O | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace store plan set to "Personal Pro" | | O | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace store plan set to "Business Pro" | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace store plan updated (sync: "Free" -> "Personal Pro" from API) | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;No workspace store update (feature gate disabled, no plan change) | | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Free plan resolved for workspace {wsId}, aiImage feature locked, Owner role confirmed" | O | | | | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Personal Pro plan resolved for workspace {wsId}, aiImage unlocked, aiVideo locked" | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Business Pro plan resolved for workspace {wsId}, all features unlocked, Owner role" | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Owner role detected for workspace {wsId}, manageBrand permission granted on Free plan" | O | O | O | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Viewer role detected for workspace {wsId}, manageBrand permission denied on Free plan" | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Subscription API fetch failed, falling back to workspace store plan: Free" | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Plan mismatch detected, syncing workspace store: Free -> Personal Pro" | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"FeatureGate disabled, skipping subscription fetch, defaulting to Free plan" | | | | | | | | O |
| **Result** | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | N | N | A | B | B |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | |

---
