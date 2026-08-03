## Function Code: CT-01 | Function Name: CreateAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 95 |
| Lack of test cases | 0 |
| Test requirement | Validates content creation with brand ownership, status validation, product affiliation, image URL serialization, and input validation |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 10 | 0 | 0 | 4 | 5 | 1 | 10 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 |
|---|---|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand exists and belongs to requesting profile (ProfileId matches) | O | O | O | | O | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand exists but belongs to a different profile (ProfileId differs) | | | | O | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content does not yet exist in repository | O | O | O | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Product exists but belongs to a different brand (BrandId mismatch) | | | | | O | | | | | |
| &nbsp;&nbsp;Input Fields | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;BrandId | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (belongs to requesting profile) | O | O | O | | O | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (belongs to different profile) | | | | O | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;AdType | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (TextOnly, Video, Image) | O | O | O | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Status | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Draft, null value) | O | | | O | O | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (PendingApproval) | | O | | | | O | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (Published, not allowed at creation) | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (Approved, not allowed at creation) | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (Rejected, not allowed at creation) | | | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;ProductId | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (null, no product) | O | O | O | O | | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (belongs to different brand) | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;TextContent | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Needs review", co y nghia) | O | O | O | O | O | O | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Draft text") | | | | | | | | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;boundary 5000 chars (chuoi rat dai) | | | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;ImageUrl | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (null, no image) | O | O | O | O | O | | O | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (single URL, serialized to JSON array) | | | | | | O | | | | |
| **Confirm** | | | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (ContentResponseDto with correct ProfileId, Status=PendingApproval) | O | O | | | | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (ContentResponseDto with Status=Draft) | | | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (BadRequest, StatusCode=400, no content created) | | | O | | O | | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (NotFound, StatusCode=404, no content created) | | | | O | | | | | | |
| &nbsp;&nbsp;Exception | | | | | | | | | | |
| &nbsp;&nbsp;Log message | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content created successfully, status: PendingApproval" | O | O | | | | O | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content created successfully, status: Draft" | | | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content creation failed: lifecycle status not allowed at creation" | | | O | | | | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content creation failed: brand does not belong to profile" | | | | O | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content creation failed: product does not belong to brand" | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content created with image URL serialized to JSON array" | | | | | | O | | | | |
| **Result** | | | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | A | A | A | N | A | N | N | B |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | | | |

---## Function Code: CT-03 | Function Name: PublishAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 175 |
| Lack of test cases | 0 |
| Test requirement | Validates content publishing to social platforms: provider integration, token handling, workspace status, quota enforcement, content status validation, and ownership checks |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 18 | 0 | 0 | 5 | 13 | 0 | 18 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 | UTCID14 | UTCID15 | UTCID16 | UTCID17 | UTCID18 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content Status=Approved (ready to publish) | O | O | O | | O | O | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content Status=Published (re-publish) | | | | O | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content Status=Draft (not valid for publish) | | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content Status=PendingApproval (not valid) | | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Integration active and belongs to same profile | O | O | O | O | O | O | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Integration belongs to different profile | | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Integration inactive or deleted | | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Account token valid and decryptable | O | O | | O | O | O | | O | O | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Token decrypt fails (missing key) | | | O | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Account token expired | | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Provider returns Success | O | | | O | | O | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Provider returns Failure | | O | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace Active | O | O | O | O | | O | | O | O | O | O | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace Limited/Expired | | | | | | | | | | | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace post quota available | O | O | O | O | | O | | O | O | O | O | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace post quota exceeded | | | | | O | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile post quota exceeded | | | | | | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Integration not found (null) | | | | | | | | | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Provider rate limit reached | | | | | | | | | | | | | | | | O | | |
| &nbsp;&nbsp;Input Fields | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;contentId | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (existing content ID) | O | O | O | O | O | O | O | O | O | O | O | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;integrationId | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (active integration, same profile) | O | O | O | O | | O | | O | O | | O | O | O | O | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (different profile integration) | | | | | | | O | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;inactive (integration deleted) | | | | | | | | | | O | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;null (integration not found) | | | | | | | | | | | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (matches content and integration) | O | O | O | O | | O | | O | O | O | O | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (matches content, different from integration) | | | | | | | O | | | | | | | | | | | |
| **Confirm** | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Status=Published, Post saved with ExternalPostId) | O | | | O | | O | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (BadGateway/502, provider failure) | | O | | | | | | | | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Unauthorized/401, SOCIAL_RECONNECT_REQUIRED) | | | O | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Forbidden/403, POST_QUOTA_EXCEEDED) | | | | | O | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (NotFound/404, integration not found or inactive) | | | | | | | O | | | O | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (BadRequest/400, content status not valid for publish) | | | | | | | | O | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Forbidden/403, workspace expired/read-only) | | | | | | | | | | | | | | O | | | | |
| &nbsp;&nbsp;Exception | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Log message | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content published successfully, ExternalPostId: ..." | O | | | O | | O | | | | | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content publish failed: provider error" | | O | | | | | | | | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content publish failed: token decrypt error" | | | O | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content publish failed: post quota exceeded" | | | | | O | | | | | | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content publish failed: integration not found" | | | | | | | O | | | O | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content publish failed: status not valid (Draft)" | | | | | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content publish failed: status not valid (PendingApproval)" | | | | | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content publish failed: token expired, SOCIAL_RECONNECT_REQUIRED" | | | | | | | | | | | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content publish failed: workspace read-only/expired" | | | | | | | | | | | | | | O | | | | |
| **Result** | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | N | A | N | A | A | A | A | A | A | A | A | A | A | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | | | | | | | | | | | |