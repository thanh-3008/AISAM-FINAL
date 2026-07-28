# AdCampaignService - Unit Test Cases

## Function Code: AC-01 | Function Name: CreateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 62 |
| Lack of test cases | 0 |
| Test requirement | Creates ad campaign with targeting, budget validation, objective, and workspace ownership checks |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 7 | 0 | 0 | 3 | 4 | 0 | 7 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace + User + AdAccount valid, Brand in workspace | O | O | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand not in workspace | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Budget=0 (invalid) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;AdAccountId empty/null | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;StartDate > EndDate (invalid date range) | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Name | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Summer Sale") | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Budget | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (100000 VND) | O | O | O | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;boundary (0 VND) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Objective | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Traffic | O | | O | O | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Conversion | | O | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;AdAccountId | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Facebook ad account ID) | O | O | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;empty/null | | | | | O | | |
| **Confirm** | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (CampaignResponseDto created, CampaignId returned) | O | O | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (brand not in workspace) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (budget must be > 0) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (AdAccountId required) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (StartDate must be before EndDate) | | | | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Campaign created: Summer Sale, budget 100000" | O | O | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create campaign failed: brand not in workspace" | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create campaign failed: budget must be > 0" | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create campaign failed: AdAccountId required" | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create campaign failed: StartDate > EndDate" | | | | | | | O |
| **Result** | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A | A | A | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | |

---

## Function Code: AC-02 | Function Name: DeployAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 45 |
| Lack of test cases | 0 |
| Test requirement | Deploys campaign to Facebook; validates campaign readiness, creative existence, and prevents duplicate deployment |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 0 | 0 | 2 | 3 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Campaign Draft with creative, valid AdAccount, ready to deploy | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Creative missing or incomplete | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Campaign already deployed (status=Active) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Facebook API returns error (network/permission) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Campaign status=Paused, re-deploy attempted | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;campaignId | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Draft campaign with creative) | O | O | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (already deployed campaign) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Paused campaign) | | | | | O |
| **Confirm** | | | | | |
| &nbsp;&nbsp;Return | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (campaign deployed, status=Active, Facebook campaign ID returned) | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (missing creative, cannot deploy) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (campaign already deployed, Facebook ID returned) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Facebook API error, deployment failed) | | | | O | |
| &nbsp;&nbsp;Log message | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Campaign deployed: status=Active, Facebook ID: ..." | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Deploy failed: creative missing" | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Campaign already deployed, returning existing ID" | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Deploy failed: Facebook API error" | | | | O | |
| **Result** | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |

---

## Function Code: AC-03 | Function Name: SyncCampaignInsightsAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 38 |
| Lack of test cases | 0 |
| Test requirement | Syncs campaign insights from Facebook; validates deployed state, handles API errors and missing data |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Campaign deployed with Facebook ID, insights available | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Campaign not yet deployed (status=Draft) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Facebook API returns empty insights data | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Facebook API returns error (token expired) | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;campaignId | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (deployed campaign with FB ID) | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Draft campaign, not deployed) | | O | | |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (insights synced: impressions, clicks, spend updated) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (campaign not deployed, cannot sync) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (empty insights, campaign data unchanged) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Facebook API error, sync failed) | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Insights synced: impressions=X, clicks=Y, spend=Z" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"SyncInsights failed: campaign not deployed" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"SyncInsights: no new insights data available" | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"SyncInsights failed: Facebook API error" | | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |