# AIService - Unit Test Cases

## Function Code: AI-01 | Function Name: GenerateDraftAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 35 |
| Lack of test cases | 0 |
| Test requirement | AI draft generation with credit consumption; handles missing Gemini config, successful generation, failed credit charge, and missing API key |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini API key missing (config not set) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini configured, returns valid AI text response | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini succeeds but credit deduction fails (insufficient balance) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini configured, returns empty/null text | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;prompt | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Create an ad", meaningful prompt) | O | O | O | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Failed generation, "Gemini API key is not configured") | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Completed, GeneratedText="Generated ad copy", credit consumed) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Generation hidden, credit charge failed) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (Completed but empty GeneratedText, no error thrown) | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GenerateDraft failed: Gemini API key not configured" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GenerateDraft completed: 1 credit consumed" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GenerateDraft: text hidden, credit charge failed" | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GenerateDraft completed: empty response from Gemini" | | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | A | N | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |

---

## Function Code: AI-02 | Function Name: ChatAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 45 |
| Lack of test cases | 0 |
| Test requirement | AI chat with message saving, credit consumption, generation response handling, brand/product context inclusion, error handling |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 7 | 0 | 0 | 5 | 2 | 0 | 7 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini succeeds, returns conversational response (no generation) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini returns generation response with Content field | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand and product selected, included in prompt context | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Credit deduction succeeds (1 credit consumed) | O | O | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Credit deduction fails (insufficient balance) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Gemini returns error/invalid response | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand belongs to different profile | | | | | | O | |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (messages saved, conversational response, no content created) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (content created from generation, status=Ready, Post created) | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (prompt includes brand name, product name, ad type context) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (credit deduction failed, credit error returned) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (AI error message stored, clear error returned to user) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (NotFound, brand not owned by profile) | | | | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Chat: conversational response, messages saved" | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Chat: generation response, content created" | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Chat: brand/product context added to prompt" | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Chat: credit deduction failed, insufficient credits" | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Chat: Gemini error, storing AI error message" | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Chat: brand not found for profile" | | | | | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | A | A | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | |

---

## Function Code: AI-03 | Function Name: ContentImprovement (ImproveAsync + ApproveGenerationAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 27 |
| Lack of test cases | 0 |
| Test requirement | Improve existing content with quota check; approve AI generation with text copy and PendingApproval status |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content exists, prompt quota available (ImproveAsync: normal) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Prompt quota exceeded (ImproveAsync: rejected) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;AI generation with GeneratedText, owned by profile (Approve: normal) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;AI generation from different profile (Approve: rejected) | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;contentId / generationId | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (existing content with quota) | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (generation owned by profile) | | | O | O |
| **Confirm** | | | | |
| &nbsp;&nbsp;Return | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Improve: Success (improved content, credit consumed) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Improve: Error (Forbidden, PROMPT_QUOTA_EXCEEDED) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Approve: Success (text copied, status=PendingApproval) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Approve: Error (NotFound, generation not accessible) | | | | O |
| &nbsp;&nbsp;Log message | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Content improved: AI enhancement applied" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Improve failed: prompt quota exceeded" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Generation approved: text copied, PendingApproval" | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"ApproveGeneration failed: not found for profile" | | | | O |
| **Result** | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |
---

## Function Code: AI-04 | Function Name: AIOutputRetrieval (GetGenerationsAsync + ChatInWorkspaceAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 16 |
| Lack of test cases | 0 |
| Test requirement | Get AI generations with profile ownership validation; chat in workspace context with credit consumption |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Content belongs to different profile (GetGenerationsAsync: cross-profile isolation) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Workspace exists, user is member with credit balance (ChatInWorkspaceAsync: normal flow) | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;contentId / workspaceId | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (content from another profile, GetGenerationsAsync) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (workspace where user is member, ChatInWorkspaceAsync) | | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (NotFound, generations not accessible by profile) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (chat response, workspace credit consumed) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetGenerations failed: content not found for profile" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"ChatInWorkspace: response generated, workspace credit consumed" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |