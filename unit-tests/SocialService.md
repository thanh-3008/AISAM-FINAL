# SocialService - Unit Test Cases

## Function Code: SC-01 | Function Name: GetAuthUrlAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 16 |
| Lack of test cases | 0 |
| Test requirement | Creates OAuth state for active profile; validates profile ownership and provider support |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile exists and belongs to requesting user | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Unsupported provider requested | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (existing profile GUID) | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;provider | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Facebook, supported platform) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (unsupported-platform, not in platform list) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (OAuth state URL with state token and redirect URI for Facebook) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (invalid or unsupported social platform provider) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"OAuth authorization URL generated for Facebook" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetAuthUrl failed: unsupported provider" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: SC-02 | Function Name: LinkAccountAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 40 |
| Lack of test cases | 0 |
| Test requirement | Links social account via OAuth callback; handles existing account token update, invalid code, and profile mismatch |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Valid OAuth callback received, new Facebook account | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Facebook account already linked to this profile | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;OAuth callback invalid (expired/bad authorization code) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile does not belong to requesting user | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;code | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (fresh OAuth authorization code from Facebook) | O | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (expired or malformed code) | | | O | |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (SocialAccountDto created with encrypted access token) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (existing account token updated, account ID unchanged) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (OAuth callback validation failed, bad token/code) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (profile does not belong to user) | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Facebook account linked successfully, new account created" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Facebook account re-linked, token updated" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkAccount failed: invalid OAuth callback code" | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkAccount failed: profile ownership mismatch" | | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |

---

## Function Code: SC-03 | Function Name: LinkAccountInWorkspaceAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 64 |
| Lack of test cases | 0 |
| Test requirement | Links social account in workspace context; creates with protected token; validates workspace membership and platform support; prevents cross-workspace moving |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 7 | 0 | 0 | 4 | 3 | 0 | 7 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace exists, user is active member | O | O | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;User not a member of workspace | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Account already linked to another workspace | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;New Facebook account, fresh OAuth callback | O | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;New TikTok account with refresh token | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Invalid OAuth callback code | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Unsupported platform | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (existing workspace, user is member) | O | O | O | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (existing workspace, user NOT member) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;platform | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Facebook | O | O | | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;TikTok | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (unsupported-provider) | | | | | | | O |
| **Confirm** | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Facebook account created with encrypted token in workspace) | O | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (account already linked to another workspace) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (TikTok account created with protected refresh token) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (user not a workspace member) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (invalid OAuth callback code) | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (unsupported social platform provider) | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Facebook account linked in workspace, token encrypted" | O | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkAccountInWorkspace failed: account in another workspace" | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"TikTok account linked in workspace, refresh token protected" | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkAccountInWorkspace failed: not a workspace member" | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkAccountInWorkspace failed: invalid OAuth code" | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkAccountInWorkspace failed: unsupported provider" | | | | | | | O |
| **Result** | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | A | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | |

---

## Function Code: SC-04 | Function Name: LinkSelectedTargetsInWorkspaceAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 100 |
| Lack of test cases | 0 |
| Test requirement | Links selected pages/groups in workspace context; creates integrations with protected tokens; validates membership, brand ownership, target permissions, and duplicate handling |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 10 | 0 | 0 | 4 | 6 | 0 | 10 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 |
|---|---|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Member has non-owner profile, brand owned by workspace member | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;TikTok account linked, ready for integration | | O | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Facebook account linked, valid pages selected | O | O | | O | O | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace member, active status | O | O | O | | O | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;User not a workspace member | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand belongs to different profile | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Page already linked (duplicate integration) | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Social account belongs to different workspace | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No target selected (empty list) | | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Account token expired | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Facebook API returns permission error | | | | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is member) | O | O | | O | O | | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is NOT member) | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (other workspace, account not owned) | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;targets | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (selected Facebook pages list) | O | | | O | O | O | | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (TikTok target) | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;empty (no pages selected) | | | | | | | | O | | |
| **Confirm** | | | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (integration created with protected page token, member profile preserved) | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (TikTok integration created) | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (user not a workspace member) | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (brand does not belong to profile) | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (page already linked to this brand) | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (account belongs to another workspace) | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (no new integration for already linked target) | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (no targets selected) | | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (account token expired, reconnect required) | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Facebook permission denied for selected page) | | | | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Pages linked in workspace: integration created with protected token" | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"TikTok integration created in workspace" | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargets failed: not a workspace member" | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargets failed: brand ownership mismatch" | | | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargets failed: page already linked to brand" | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargets failed: account in another workspace" | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Target already linked, skipped" | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargets failed: empty target list" | | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargets failed: account token expired" | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargets failed: Facebook permission denied" | | | | | | | | | | O |
| **Result** | | | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A | A | A | A | N | A | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | | | |

---

## Function Code: SC-05 | Function Name: LinkSelectedTargetsForAccountAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 65 |
| Lack of test cases | 0 |
| Test requirement | Links pages/groups with brand validation; creates integration with protected token; handles brand ownership mismatch, invalid account, and network errors |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 7 | 0 | 0 | 3 | 4 | 0 | 7 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Facebook account linked to profile, brand belongs to profile | O | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand belongs to different profile (ownership mismatch) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Social account not found (invalid account ID) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Target page list includes page with missing permissions | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Facebook API returns network timeout | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Account token decryption fails | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;socialAccountId | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (linked Facebook account of requesting user) | O | O | | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (not found in DB) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;brandId | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (brand owned by profile) | O | | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (brand owned by different profile) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;targets | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (selected Facebook pages with proper permissions) | O | | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (page without manage permission) | | | | | O | | |
| **Confirm** | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (integration created with encrypted page token) | O | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (brand does not belong to requesting profile) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (social account not found) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (page permission missing for target) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Facebook API network timeout) | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (token decryption failed) | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Pages linked: integration created with protected page token" | O | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargetsForAccount failed: brand ownership mismatch" | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargetsForAccount failed: social account not found" | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargetsForAccount failed: page permission missing" | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargetsForAccount failed: Facebook API timeout" | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"LinkTargetsForAccount failed: token decryption error" | | | | | | | O |
| **Result** | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | A | A | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | |

---

## Function Code: SC-06 | Function Name: UnlinkAccountAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 20 |
| Lack of test cases | 0 |
| Test requirement | Soft deletes social account and all associated integrations; validates account ownership |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Social account linked to requesting profile with active integrations | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Social account belongs to different profile | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;socialAccountId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (linked account owned by requesting profile) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (linked account owned by different profile) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (account soft deleted, all integrations soft deleted, cascade complete) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (account not found or not owned by profile) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Social account unlinked: account and all integrations soft deleted" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"UnlinkAccount failed: account ownership mismatch" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: SC-07 | Function Name: UnlinkTargetAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 19 |
| Lack of test cases | 0 |
| Test requirement | Soft deletes only the requested integration; validates integration ownership |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Integration exists and belongs to active profile | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Integration belongs to different profile | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;integrationId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (integration owned by requesting profile) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (integration owned by different profile) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (only requested integration soft deleted, other integrations unchanged) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (integration not found or ownership mismatch) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Target unlinked: integration soft deleted" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"UnlinkTarget failed: integration ownership mismatch" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |

---

## Function Code: SC-08 | Function Name: GetWorkspaceAccountsAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 17 |
| Lack of test cases | 0 |
| Test requirement | Lists workspace accounts without decrypting stored tokens; validates workspace membership |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace has 2 linked social accounts (Facebook + TikTok) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;User not a member of workspace | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;workspaceId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is member) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is NOT member) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (list of 2 accounts, tokens remain encrypted, no decryption performed) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (user not a workspace member, access denied) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Workspace accounts listed: 2 accounts found, tokens not decrypted" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetWorkspaceAccounts failed: user not a member" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |