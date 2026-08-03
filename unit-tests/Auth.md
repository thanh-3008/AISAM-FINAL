# Auth - Unit Test Cases

## Function Code: AUTH-01 | Function Name: RegisterAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 42 |
| Lack of test cases | 0 |
| Test requirement | Dang ky nguoi dung moi voi email/password, tu dong tao Personal Workspace kem Owner membership, CreditWallet 50 credits, Free Subscription |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|
| 13 | 1 | 0 | 6 | 5 | 3 | 14 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 | UTCID08 | UTCID09 | UTCID10 | UTCID11 | UTCID12 | UTCID13 | UTCID14 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Database va Email service san sang (ket noi thanh cong) | O | O | O | O | O | O | O | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Email CHUA ton tai trong CSDL (nguoi dung moi) | O | O | O | O | | O | O | O | O | O | O | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Email DA ton tai trong CSDL (nguoi dung da dang ky truoc do) | | | | | O | | | | | | | | | |
| &nbsp;&nbsp;Input Fields | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Email | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;dung dinh dang (user@example.com, co ky tu @ va ten mien hop le) | O | O | O | | | O | O | O | O | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;sai dinh dang (thieu ky tu @, khong co ten mien) | | | | O | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;chuoi rong ("") hoac null (khong nhap email) | | | | | | | | | | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;vuot do dai toi da (tren 255 ky tu) | | | | | | | | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;boundary do dai toi thieu (a@b.c, dung 5 ky tu) | | | | | | | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Password | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;dung (Password123! - co chu hoa, chu thuong, so, ky tu dac biet) | O | O | O | O | O | O | O | O | O | O | O | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;boundary do dai toi da (60 ky tu) | | | | | | | | | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;FullName | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;dung ("Test User", chuoi khong rong, co y nghia) | O | O | O | O | O | | | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;chuoi rong ("") hoac null (khong nhap ten) | | | | | | O | O | | | | | | | |
| **Confirm** | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;TokenResponse thanh cong (JWT access token + refresh token + User info, workspace da duoc tao) | O | O | O | | | | | O | O | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error response (InvalidOperationException voi thong bao loi cu the) | | | | O | O | O | O | | | | O | O | O | |
| &nbsp;&nbsp;Exception | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;InvalidOperationException (duplicate email, validation email/password/name) | | | | O | O | O | O | | | | O | O | O | |
| &nbsp;&nbsp;Log message | | | | | | | | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"User registered successfully" (dang ky thanh cong, user + workspace da duoc tao) | O | O | O | | | | | O | O | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Registration failed: ..." (dang ky that bai, kem theo ly do cu the) | | | | O | O | O | O | | | | O | O | O | |
| **Result** | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | A | A | A | A | N | N | B | B | B | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P | P | P | P | P | P | P | F |
| &nbsp;&nbsp;Executed Date | | | | | | | | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | | | | | | | | |

---

## Function Code: AUTH-02 | Function Name: LoginAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 22 |
| Lack of test cases | 0 |
| Test requirement | Xac thuc nguoi dung bang email/password, tra ve JWT access token + refresh token, cap nhat LastLoginAt |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 5 | 1 | 0 | 3 | 2 | 1 | 6 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 |
|---|---|---|---|---|---|---|
| **Condition** | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User DA ton tai trong CSDL (da dang ky, co password hash hop le) | O | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;User KHONG ton tai trong CSDL (email chua duoc dang ky) | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Tai khoan User dang o trang thai Active (chua bi khoa) | O | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;lastLoginAt dang la gia tri cu (can duoc cap nhat sau khi login) | | | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Database va JWT service san sang | O | O | O | O | O | O |
| &nbsp;&nbsp;Input Fields | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Email | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;dung dinh dang, co ton tai trong DB (user@example.com) | O | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;dung dinh dang, nhung KHONG ton tai trong DB (notregistered@example.com) | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Password | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;dung mat khau (Password123!, khop voi password hash trong DB) | O | | | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;sai mat khau (WrongPassword456!, khong khop voi hash) | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;chuoi rong ("" - khong nhap mat khau) | | | O | | | |
| **Confirm** | | | | | | |
| &nbsp;&nbsp;Return | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;TokenResponse thanh cong (JWT access token + refresh token + User info, LastLoginAt duoc cap nhat thanh thoi gian hien tai) | O | | | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error Unauthorized (401) - "Invalid email or password" | | O | O | O | | |
| &nbsp;&nbsp;Exception | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UnauthorizedAccessException (sai email hoac mat khau, hoac email khong ton tai) | | O | O | O | | |
| &nbsp;&nbsp;Log message | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"User logged in successfully" (dang nhap thanh cong, kem UserId) | O | | | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Login failed: invalid credentials" (dang nhap that bai) | | O | O | O | | |
| **Result** | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | B | A | N | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | F |
| &nbsp;&nbsp;Executed Date | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | DF-AUTH02-01 |

---

## Function Code: AUTH-03 | Function Name: GoogleLoginAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 68 |
| Lack of test cases | 0 |
| Test requirement | Dang nhap bang Google OAuth ID token: xac thuc token, tu dong tao user moi neu chua ton tai, cap nhat thong tin neu name thay doi, tra ve JWT |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 6 | 1 | 0 | 3 | 3 | 0 | 7 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 | UTCID06 | UTCID07 |
|---|---|---|---|---|---|---|---|
| **Condition** | | | | | | | |
| &nbsp;&nbsp;Precondition | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Google ID token HOP LE (da duoc Google xac thuc, chu ky dung, con han) | O | O | O | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Google ID token KHONG hop le (token da het han hoac chu ky sai) | | | | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;User chua ton tai trong CSDL (nguoi dung moi, lan dau dang nhap Google) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User da ton tai trong CSDL (da tung dang nhap Google truoc do) | | O | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;User chua xac thuc email (IsEmailVerified = false) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Name trong Google token KHAC voi Name trong CSDL (da doi ten) | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Google API khong kha dung (network error, timeout khi verify token) | | | | | | | O |
| &nbsp;&nbsp;Input Fields | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;IdToken (Google Identity JWT) | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (Google ID token hop le, payload co email + name + sub, con han) | O | O | O | O | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (token da het han, expired > 1h) | | | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;invalid (token co chu ky sai, da bi chinh sua payload) | | | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;userAgent | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (trinh duyet Chrome, "Mozilla/5.0...") | O | O | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;ipAddress | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (dia chi IP cua client, "192.168.1.1") | O | O | O | O | O | O | O |
| **Confirm** | | | | | | | |
| &nbsp;&nbsp;Return | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;TokenResponse (tai khoan MOI duoc tao trong DB, JWT access + refresh tra ve) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;TokenResponse (dang nhap thanh cong, JWT tra ve, thong tin user hien co) | | O | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Name trong DB duoc cap nhat thanh name moi tu Google | | | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Email verified status duoc giu nguyen (false) | | | O | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error Unauthorized (401) - "Invalid Google token" | | | | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error ServiceUnavailable (503) - "Google authentication service unavailable" | | | | | | | O |
| &nbsp;&nbsp;Exception | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UnauthorizedAccessException (Google token khong hop le hoac da het han) | | | | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;HttpRequestException (Google API khong phan hoi, network timeout) | | | | | | | O |
| &nbsp;&nbsp;Log message | | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Google login: new user created" (tai khoan moi, kem UserId) | O | | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Google login: user authenticated" (dang nhap thanh cong) | | O | O | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Google login: invalid token" (token khong hop le) | | | | | O | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Google login: service unavailable" (Google API loi) | | | | | | | O |
| **Result** | | | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | N | N | N | A | A | A |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | | | |

---

## Function Code: AUTH-04 | Function Name: RefreshTokenAsync

| | |
|---|---|
| Created By | QA Team |
| Executed By | ___ |
| Date | ___ |
| Lines of code | 24 |
| Lack of test cases | 0 |
| Test requirement | Lam moi access token bang refresh token: xac thuc session, rotate token, phat hien token reuse (thu hoi tat ca session), kiem tra IP |

| Passed | Failed | Untested | N | A | B | Total Test Cases |
|---|---|---|---|---|---|---|---|
| 4 | 1 | 0 | 2 | 2 | 0 | 5 |

| | UTCID01 | UTCID02 | UTCID03 | UTCID04 | UTCID05 |
|---|---|---|---|---|---|
| **Condition** | | | | | |
| &nbsp;&nbsp;Precondition | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Session dang Active (IsActive = true, ExpiresAt > hien tai) | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Session da het han (ExpiresAt < hien tai, qua thoi gian song) | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Refresh token DA BI SU DUNG LAI (reuse detection - token da bi xoay truoc do) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Refresh token DA BI THU HOI (da bi revoke boi Admin hoac logout) | | | | O | |
| &nbsp;&nbsp;&nbsp;&nbsp;IP request GIONG voi IP luc tao session (cung mot thiet bi/mang) | O | O | | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;IP request KHAC voi IP luc tao session (khac thiet bi/mang) | | | O | | |
| &nbsp;&nbsp;Input Fields | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;RefreshToken (string) | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (token ton tai trong DB, session con active, chua bi thu hoi) | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;userAgent | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (trinh duyet hien tai, "Mozilla/5.0...") | O | O | O | O | O |
| &nbsp;&nbsp;&nbsp;&nbsp;ipAddress | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;valid (dia chi IP client hien tai) | O | O | O | O | O |
| **Confirm** | | | | | |
| &nbsp;&nbsp;Return | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;TokenResponse (cap moi access token + refresh token, token cu bi xoay/thay the) | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;Error Unauthorized (401) - session da het han | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error Unauthorized (401) - token reuse detected, TAT CA session cua user bi thu hoi | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;Error Unauthorized (401) - token da bi thu hoi | | | | O | |
| &nbsp;&nbsp;Exception | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;UnauthorizedAccessException (session expired, token reused, hoac token revoked) | | O | O | O | |
| &nbsp;&nbsp;Log message | | | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Token refreshed successfully" (cap moi thanh cong) | O | | | | O |
| &nbsp;&nbsp;&nbsp;&nbsp;"Token refresh failed: session expired" | | O | | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Token reuse detected - all sessions revoked" (bao mat) | | | O | | |
| &nbsp;&nbsp;&nbsp;&nbsp;"Token refresh failed: token revoked" | | | | O | |
| **Result** | | | | | |
| &nbsp;&nbsp;Type(N/A/B) | N | A | A | A | N |
| &nbsp;&nbsp;Passed/Failed | P | P | P | P | P |
| &nbsp;&nbsp;Executed Date | | | | | |
| &nbsp;&nbsp;Defect ID | | | | | |
