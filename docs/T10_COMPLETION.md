# T10 — Composer nhiều media và preview

Hoàn thành phần phát triển ngày 09/09/2026. Chưa nghiệm thu đăng lên provider thật; phần này thuộc T11 cùng các điều kiện sandbox của T07–T09.

## Luồng sử dụng

1. Tạo và lưu draft để có Content ID, sau đó mở trang chi tiết. Nút tạo đã đổi thành “Lưu draft và mở composer”. Nội dung Rich Text được lưu bằng contract T09.
2. Trong **Media collection**, chọn/thả nhiều ảnh hoặc video. Tối đa 10 media, 50MB/file, 200MB/lần chọn. Không cắt bỏ âm thầm file vượt giới hạn.
3. Upload tuần tự, trạng thái từng file Pending/Uploading/Done/Failed. Progress là trạng thái xử lý và số file, không giả hiển thị phần trăm byte. Nút thử lại chỉ gửi file chưa thành công; file Done giữ lại.
4. Đổi thứ tự bằng kéo thả hoặc nút Lên/Xuống; bỏ file/media; chọn cover trong collection. Lưu media gửi ExpectedVersion và danh sách asset có thứ tự tới API T06.
5. Lưu media làm nội dung trở về Draft. Web chặn Post Now, Schedule và Submit for Approval khi còn thay đổi media chưa lưu hoặc file chưa upload. Sau khi lưu, gửi duyệt lại rồi đăng/lên lịch qua luồng hiện có.
6. Nội dung cũ có nút nhập media từ chính content được cấp quyền. Không nhập URL tùy ý. Media legacy thiếu metadata vẫn có thể bị capability từ chối; tải lại file gốc để có metadata nếu cần.

Cover hiện là dữ liệu collection; adapter chưa công bố tùy chọn thumbnail/cover riêng nên UI không hứa provider sẽ dùng cover khác thứ tự đăng. Preview hiển thị thứ tự thực tế; không tự reorder khi chọn cover.

## Khôi phục và xung đột

- Danh sách asset/thứ tự chưa lưu ở sessionStorage theo actor + workspace + content. Khi tải lại phải chọn “Khôi phục để đối chiếu”; không tự ghi đè server. Người dùng chủ động áp bản nháp lên version vừa đọc sau khi xem số media hai phía.
- File Blob chưa upload lưu IndexedDB theo cùng phạm vi; reload chuyển Uploading/Failed về Pending. File Done được loại khỏi hàng đợi lưu trữ. Nếu storage không khả dụng/đầy, UI báo phải giữ trang mở hoặc chọn lại file.
- Version conflict giữ draft, không tự retry ghi đè. Người dùng reload và đối chiếu. File có thể đã lên storage nhưng chưa được gắn khi mất kết nối; không coi đó là bài đã đăng.
- Phản hồi media muộn sau đổi content/workspace không được thêm vào composer mới. API client vẫn kiểm tra workspace như trước.

## Preview và publish

API mới `GET /api/content/{id}/publish-preview` trả version, trạng thái có approval snapshot, media, caption và capability/error của từng destination có quyền xem. Backend kiểm tra ContentView/SocialView/PostPublish và dùng PublishingCapabilities T07. Không trả credential.

Preview dùng media/caption đã khóa nếu có snapshot; draft dùng dữ liệu hiện hành. Kênh thiếu quyền/không tương thích được hiển thị với lý do và không được thêm vào lựa chọn đăng. Người dùng có thể bỏ chọn; không tự chuyển platform hay tách một yêu cầu thành nhiều bài. Caption giữ fallback plain text T09 và đếm code point.

Post Now gọi publish-operations với khóa mới được lưu localStorage **trước** request, theo actor/workspace/content. Timeout không tạo khóa khác hoặc gọi lại tự động. Reopen đọc khóa cũ. Endpoint `GET /api/content/{id}/publish-operations?key=...` chỉ trả operation của chính actor trong workspace sau ContentView check.

UI theo dõi từng destination và provider ID/error code; poll mỗi 3 giây tối đa 20 lần trong một lần mở, sau đó có nút kiểm tra thủ công. NeedsAttention/unknown outcome không được retry mới. Khi mọi kết quả đã xác định, người dùng có thể chủ động chuẩn bị lượt mới chỉ cho các kênh lỗi được phân loại; các kênh Published không được chọn lại. Kết quả cũ giữ trong mã lượt mới. Link `/social` hỗ trợ reconnect.

Giữ khóa publish trong localStorage nhằm tránh đăng trùng sau đóng/mở tab. UI chưa cung cấp nút xóa journal hoặc cho phép đăng lại cùng content sau thành công; muốn đăng lại có chủ đích cần tạo/clone content. Không xóa localStorage để xử lý timeout chưa đối soát.

## Kiểm thử và giới hạn xác nhận

- **94/94 test web đạt**: permission/incompatible destination, timeout/reopen không replay, partial retry bỏ kênh thành công, file retry chỉ file lỗi, reorder/version conflict, khôi phục draft và không cắt bớt file vượt giới hạn.
- **Build production Next.js đạt**; còn cảnh báo middleware convention hiện hữu.
- **505/508 test backend đạt**, ba lỗi PromptEnhancer cũ. HTTP test bổ sung kiểm tra preview không lộ token, quyền đọc và journal chỉ thuộc actor; không thay thế JWT/membership production test.
- **Edge headless / IndexedDB thực đạt**: File giữ tên/type/bytes sau reload, key tài khoản khác không đọc được, file Done được xóa. Script: `AISAM-FE/scripts/check-composer-storage.cjs` (mặc định dùng Edge; đổi bằng COMPOSER_BROWSER_CHANNEL).
- Không thêm migration trong T10; chưa migrate database nguồn, chưa gọi publish provider thật.
- Tạo content vẫn dùng bước lưu draft trước khi mở collection; upload cũ ở màn tạo vẫn khả dụng. Composer mới quản lý collection đầy đủ trên trang chi tiết, không dùng trường legacy để thay thế collection khi publish.
- Scheduling vẫn đi qua calendar/API hiện có và snapshot của T06/T08. Kiểm chứng tương tác toàn luồng với tài khoản thật, network interruption trên VPS và mọi tổ hợp provider nằm ở T11. Cover/thumbnail riêng và capability vượt adapter hiện hành không được công bố hỗ trợ.

Task tiếp theo: **T11 — Kiểm thử tổng thể, migration staging và bàn giao**.
