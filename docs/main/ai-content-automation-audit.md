# Kiểm tra AI Content Automation

## Phạm vi

Đã rà luồng: CSV/Google Sheets → validate → reserve credit → generate text/image/video → retry/resume → approve/reject → tạo lịch → scheduler đăng → đồng bộ kết quả → performance.

## Kết quả

- Build Backend và Frontend thành công.
- Migration Automation Plan, video/schedule link, operations và reserved credit đã áp dụng.
- Idempotency item: unique `(AutomationPlanId, RowIndex, Platform)` và `IdempotencyKey`.
- Idempotency lịch: unique `ContentCalendarId` trên AutomationItem và kiểm tra active schedule trước khi tạo.
- Video job được lưu để tiếp tục poll sau khi restart.
- Credit chỉ quyết toán khi từng bước thành công; retry không tính lại bước đã hoàn tất.
- TikTok chỉ nhận Video/Auto từ bước validate.
- Item lỗi không chặn item khác; có retry, cancel và release credit.
- Auto-approve bị giới hạn cho Owner/Manager.
- Google Sheets bị giới hạn domain để tránh SSRF.

## Kiểm thử

- Automation và multi-platform scheduling tests liên quan: đạt.
- Toàn bộ test suite hiện có 282/301 test đạt; 19 test cũ ngoài Automation đang sai kỳ vọng hoặc dùng fake chưa hoàn thiện ở middleware, quota, payment và scheduler retry.
- Nullable warning tại `ScheduledPostingService.cs` đã được xử lý; build Automation không còn warning.
