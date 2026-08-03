# FoundationTests - Unit Test Cases

## Function Code: FD-01 | Function Name: GetProfileByIdAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 30 |
| Lack of test cases | 0 |
| Test requirement | Get profile by ID with user ownership check; returns NotFound for other user; returns Forbidden for route mismatch |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 3 | 0 | 0 | 1 | 2 | 0 | 3 |

| | UTCID01 | UTCID02 | UTCID03 |
|---|---|---|---|
| **Condition** | | | |
| &nbsp;&nbsp;Precondition | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile belongs to requesting user (normal flow) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile belongs to different user (ownership check) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Route userId does not match JWT userId (Forbidden) | | | O |
| &nbsp;&nbsp;Input Fields | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile owned by user) | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile owned by different user) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;userId | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (matches JWT claim) | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;mismatch (route userId vs JWT userId) | | | O |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Success (ProfileResponseDto with matching UserId) | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (NotFound, profile owned by different user) | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (Forbidden, route user does not match JWT user) | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Profile retrieved by ID" | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetProfile failed: profile owned by different user" | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"GetProfile failed: route/JWT user mismatch" | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P |
| &nbsp;&nbsp;Executed Date | | | |
| &nbsp;&nbsp;Defect ID | | | |

---

## Function Code: FD-02 | Function Name: ProfileLifecycle (CreateProfileAsync + UpdateProfileAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 55 |
| Lack of test cases | 0 |
| Test requirement | Creates profile with avatar upload validation; updates profile with ownership check; rejects avatar file upload in non-enabled environments |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 6 | 0 | 0 | 2 | 4 | 0 | 6 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
|---|---|---|---|---|---|---|
| **Condition** | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Valid profile data, no avatar file (CreateAsync: normal) | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Avatar file provided, upload not enabled (CreateAsync: rejected) | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Update existing profile owned by user (UpdateAsync: normal) | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Update profile owned by different user (UpdateAsync: rejected) | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Avatar file in update, upload not enabled (UpdateAsync: rejected) | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile deleted, update attempted (UpdateAsync: rejected) | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Name | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Test profile") | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;AvatarFile | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;null (no file) | O | | O | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (FormFile with image/png) | | O | | | O | |
| **Confirm** | | | | | | |
| &nbsp;&nbsp;Return | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;CreateAsync: Success (ProfileResponseDto created) | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;CreateAsync: Error (upload is not enabled) | | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Success (name updated) | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Error (NotFound, profile owned by different user) | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Error (upload is not enabled for avatar) | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Error (profile deleted) | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Profile created successfully" | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create profile failed: avatar upload not enabled" | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Profile updated: new name" | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Update profile failed: ownership mismatch" | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Update profile failed: avatar upload not enabled" | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Update profile failed: profile deleted" | | | | | | O |
| **Result** | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | A | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | |

---

## Function Code: FD-03 | Function Name: ProfileSoftDelete (DeleteProfileAsync + RestoreProfileAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 40 |
| Lack of test cases | 0 |
| Test requirement | Soft deletes profile with ownership check; restores soft-deleted profile with ownership check |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 0 | 0 | 2 | 2 | 0 | 4 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 |
|---|---|---|---|---|
| **Condition** | | | | |
| &nbsp;&nbsp;Precondition | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile active, owned by user (DeleteAsync: normal) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile owned by different user (DeleteAsync: rejected) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile soft-deleted, owned by user (RestoreAsync: normal) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Profile active/owned by different user (RestoreAsync: rejected) | | | | O |
| &nbsp;&nbsp;Input Fields | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;profileId | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (active profile owned by user) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (profile owned by different user) | | O | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (soft-deleted profile owned by user) | | | O | |
| **Confirm** | | | |
| &nbsp;&nbsp;Return | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;DeleteAsync: Success (true, profile soft deleted) | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;DeleteAsync: Error (NotFound, ownership mismatch) | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;RestoreAsync: Success (true, profile restored, IsDeleted=false) | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;RestoreAsync: Error (NotFound, ownership mismatch) | | | | O |
| &nbsp;&nbsp;Log message | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Profile soft deleted" | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Delete profile failed: ownership mismatch" | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Profile restored successfully" | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Restore profile failed: ownership mismatch" | | | | O |
| **Result** | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | |
| &nbsp;&nbsp;Defect ID | | | | |

---

## Function Code: FD-04 | Function Name: ProductCRUD (CreateAsync + UpdateAsync)

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 75 |
| Lack of test cases | 0 |
| Test requirement | Creates product with brand and image file validation; updates product with image file handling |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 8 | 0 | 0 | 4 | 4 | 0 | 8 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 |
|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Valid brand, product name with image files (CreateAsync: normal) | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Brand not in workspace (CreateAsync: rejected) | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;No image files (CreateAsync: normal without images) | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Update existing product with new images (UpdateAsync: normal) | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Update product with no images (UpdateAsync: keep existing) | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Update product owned by different user (UpdateAsync: rejected) | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Image file exceeds max size (CreateAsync: rejected) | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Unsupported image format (CreateAsync: rejected) | | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Name | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("New product") | O | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;BrandId | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (owned brand in workspace) | O | | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (brand from different workspace) | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;ImageFiles | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (1 image, product.png) | O | O | | O | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;null/empty | | | O | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (60MB oversized image) | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (.bmp unsupported format) | | | | | | | | O |
| **Confirm** | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;CreateAsync: Success (product saved with image URLs) | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;CreateAsync: Error (brand not in workspace) | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;CreateAsync: Success (product saved without images) | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Success (product updated with new images) | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Success (product updated, images unchanged) | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UpdateAsync: Error (ownership mismatch) | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (image file exceeds size limit) | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error (unsupported image format) | | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Product created with images" | O | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Create product failed: brand not in workspace" | | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Product created without images" | | | O | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Product updated with new images" | | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Product updated, images unchanged" | | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Update product failed: ownership mismatch" | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Image upload failed: file exceeds max size" | | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Image upload failed: unsupported format" | | | | | | | | O |
| **Result** | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | N | N | N | A | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | |

---

## Function Code: FD-05 | Function Name: EmailService.SendEmailAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 20 |
| Lack of test cases | 0 |
| Test requirement | Sends email via SMTP; returns false when SMTP not configured; handles valid configuration |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 2 | 0 | 0 | 1 | 1 | 0 | 2 |

| | UTCID01 | UTCID02 |
|---|---|---|
| **Condition** | | |
| &nbsp;&nbsp;Precondition | | |
| &nbsp;&nbsp;&nbsp;&nbsp;SMTP server not configured (EmailSettings empty/null) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;SMTP server configured with valid credentials | | O |
| &nbsp;&nbsp;Input Fields | | |
| &nbsp;&nbsp;&nbsp;&nbsp;toEmail | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (user@example.com) | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;subject | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("Subject") | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;htmlBody | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid ("<p>Body</p>") | O | O |
| **Confirm** | | |
| &nbsp;&nbsp;Return | | |
| &nbsp;&nbsp;&nbsp;&nbsp;false (SMTP not configured, email not sent) | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;true (email sent successfully via SMTP) | | O |
| &nbsp;&nbsp;Log message | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Email not sent: SMTP not configured" | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Email sent successfully to user@example.com" | | O |
| **Result** | | |
| &nbsp;&nbsp;Type(N/A/B) | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P |
| &nbsp;&nbsp;Executed Date | | |
| &nbsp;&nbsp;Defect ID | | |