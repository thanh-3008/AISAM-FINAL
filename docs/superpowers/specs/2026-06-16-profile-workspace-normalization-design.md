# Thiết Kế Chuẩn Hóa Profile Và Workspace

Ngày: 2026-06-16
Trạng thái: Đã chốt trong trao đổi, chờ người dùng review bản spec đã ghi
Phạm vi: Chuẩn hóa toàn bộ FE + BE với cơ chế tương thích ngắn hạn trong giai đoạn migration

## 1. Mục tiêu

Xử lý tận gốc nhóm lỗi lặp lại liên quan đến `workspace` và `profile` bằng cách chuẩn hóa toàn bộ dự án theo một quy tắc duy nhất:

- `workspace` là context chính của dashboard
- `profile` là context phụ, chỉ dùng ở các tính năng thực sự cần

Công việc này phải loại bỏ tình trạng hiện tại, nơi frontend và backend đôi khi đang coi `profile.id` như `workspace.id`, hoặc suy ra context này từ context kia thông qua các fallback không an toàn.

## 2. Vấn đề hiện tại

Codebase hiện tại vẫn đang trộn lẫn khái niệm profile và workspace ở nhiều nơi:

- frontend migration trong storage có thể map `profile.id` cũ sang `workspace.id`
- một số flow ở frontend có thể ghi `workspace.id` vào active profile state
- một số hook ở frontend vẫn coi dữ liệu kiểu profile như dữ liệu kiểu workspace
- các request protected có thể gửi sai `X-Workspace-Id`
- contract của backend ở nhiều chỗ đang yêu cầu cả workspace và profile, nhưng quan hệ sở hữu giữa hai context này chưa được cưỡng chế một cách nhất quán

Các lỗi đã quan sát được từ trạng thái hiện tại gồm:

- tạo brand thất bại với lỗi membership vì gửi sai workspace header
- lỗi hydration và lỗi runtime React xuất hiện khi logic đồng bộ context không ổn định
- các tính năng dashboard hoạt động không nhất quán khi local storage còn state cũ

## 3. Mục tiêu chi tiết

- Biến `activeWorkspace` thành nguồn sự thật duy nhất cho toàn bộ hành vi dashboard theo workspace
- Đảm bảo `X-Workspace-Id` luôn lấy từ workspace state đã được xác thực
- Đảm bảo `activeProfile` không bao giờ được dùng thay cho workspace
- Giới hạn việc dùng `profileId` vào đúng các tính năng thật sự cần profile context
- Có migration tương thích ngắn hạn để user local hiện tại không bị gãy ngay
- Làm cho backend validate rõ quan hệ profile-thuộc-workspace khi endpoint cần cả hai ID

## 4. Ngoài phạm vi

- Không thiết kế lại rộng các business rule không liên quan trực tiếp đến workspace/profile context
- Không giữ lớp tương thích kép cho legacy behavior trong dài hạn
- Không refactor UI không liên quan ngoài các phần cần thiết để ổn định context handling

## 5. Mô hình context

### 5.1 Quy tắc chính

- `workspace` là context chính cho tất cả các page dashboard và các API protected theo workspace
- `profile` là context phụ, chỉ được load và sử dụng khi một tính năng cụ thể yêu cầu

### 5.2 Quy tắc phía frontend

- `aisam_active_workspace` là storage key duy nhất đại diện cho active workspace context
- `aisam_active_profile` là storage độc lập và không được dùng để suy ra workspace context
- logic runtime ở frontend không được copy `profile.id` sang workspace state
- logic runtime ở frontend không được copy `workspace.id` sang profile state

### 5.3 Quy tắc phía backend

- các endpoint protected theo workspace validate membership từ `X-Workspace-Id`
- các endpoint cần profile context phải validate thêm rằng profile được chọn hợp lệ trong workspace đang active
- backend không được dựa vào giả định ngầm hoặc legacy assumption rằng profile ID và workspace ID có thể thay thế nhau

## 6. Chiến lược được chọn

Phương án khuyến nghị: chuẩn hóa đầy đủ contract trên cả FE và BE, đi kèm migration tương thích ngắn hạn.

Lý do:

- sửa FE trước chỉ giảm triệu chứng nhưng không làm contract rõ ràng
- thêm lớp adapter tạm thời chỉ kéo dài nợ kỹ thuật và giữ lại sự mơ hồ
- chuẩn hóa toàn phần giúp loại bỏ cả nhóm lỗi thay vì vá từng màn riêng lẻ

## 7. Thiết kế phía frontend

### 7.1 Storage

`workspace-store` và `profile-store` tiếp tục tồn tại riêng biệt.

Quy tắc:

- `workspace-store` chỉ lưu `{ id, name, workspaceType }`
- `profile-store` chỉ lưu `{ id, name, profileType }`
- dữ liệu legacy có thể được đọc trong giai đoạn migration, nhưng không được coi là runtime source of truth

Hành vi migration:

1. Đọc các giá trị đang có trong storage
2. Load dữ liệu workspace/profile thật từ API
3. Validate giá trị đã lưu với dữ liệu từ server
4. Ghi lại storage theo format đã chuẩn hóa
5. Xóa state legacy không hợp lệ nếu không map an toàn được

### 7.2 Hooks

#### `useWorkspaces`

Trách nhiệm:

- load danh sách workspace
- validate active workspace đang lưu
- chọn fallback workspace chỉ từ danh sách workspace hợp lệ
- expose active workspace state mà không phụ thuộc vào dữ liệu profile

Ràng buộc:

- không có fallback path nào coi profile record là workspace record
- không có logic đồng bộ storage nào có thể tạo uncontrolled re-render loop

#### `useProfiles`

Trách nhiệm:

- load dữ liệu profile theo active workspace hoặc theo feature context cần profile
- validate profile đang lưu còn thuộc workspace hiện tại hay không
- clear profile state khi đổi workspace nếu profile cũ không còn hợp lệ

Quy tắc hybrid:

- nếu workspace hiện tại chỉ có đúng 1 profile hợp lệ thì auto-select
- nếu có nhiều profile hợp lệ thì để user chọn rõ ở các feature cần profile
- nếu page hiện tại không cần profile thì dashboard vẫn chạy bình thường khi `activeProfile` là null

### 7.3 API Client

Quy tắc:

- `X-Workspace-Id` luôn được lấy từ active workspace state
- `X-Profile-Id` chỉ được gắn khi request hoặc feature hiện tại thực sự cần profile hợp lệ
- thiếu profile không được làm hỏng các page chỉ cần workspace

Xử lý lỗi:

- workspace thiếu hoặc không hợp lệ thì clear workspace context và đưa user về luồng chọn workspace
- profile không hợp lệ thì chỉ clear profile context

### 7.4 Hành vi của page và component

Các page chỉ cần workspace phải hoạt động bình thường dù không có active profile:

- dashboard
- brands
- analytics
- team / workspace members
- billing / workspace settings
- notifications
- social pages chỉ đọc workspace-scoped state

Các page hoặc flow cần profile:

- content drafting/creation khi dữ liệu content gắn với profile
- social linking flow theo profile
- AI flow có lưu hoặc scope dữ liệu theo profile

Các page này phải:

- auto-select nếu chỉ có 1 profile hợp lệ
- yêu cầu user chọn profile nếu có nhiều profile hợp lệ
- fail sớm ở UI thay vì gửi request sai

## 8. Thiết kế phía backend

### 8.1 Chuẩn hóa contract

Controller và service phải được phân loại thành hai nhóm:

- thao tác chỉ cần workspace
- thao tác cần cả workspace + profile

Các thao tác chỉ cần workspace không được phụ thuộc vào profile context.

Các thao tác cần workspace + profile phải validate:

- user hiện tại là thành viên của workspace đang active
- profile được tham chiếu thực sự tồn tại
- profile đó hợp lệ trong workspace đang active

### 8.2 Quy tắc validate

Lỗi từ backend phải rõ nghĩa và phân biệt được:

- thiếu workspace context
- workspace không tồn tại
- user không phải thành viên của workspace
- endpoint yêu cầu profile nhưng chưa có profile
- profile không tồn tại
- profile không thuộc workspace đang active

Điều này giúp frontend không phải đoán lỗi và tránh logic fallback mong manh dựa trên message.

### 8.3 Ranh giới service

Cần rà lại các service hiện đang mặc định nhận cả workspace ID và profile ID.

Thay đổi dự kiến:

- bỏ phụ thuộc profile ở các workflow thực tế chỉ cần workspace
- chỉ giữ profile parameter ở các service mà hành vi lưu trữ hoặc business logic thực sự phụ thuộc vào profile
- gom kiểm tra ownership giữa workspace/profile về một logic nhất quán thay vì để assumption rải rác

## 9. Luồng dữ liệu

### 9.1 Sau khi đăng nhập

1. Load danh sách workspace
2. Resolve `activeWorkspace` từ dữ liệu đã lưu hợp lệ hoặc một fallback workspace hợp lệ
3. Chỉ load profile candidates liên quan đến active workspace khi cần
4. Resolve `activeProfile` theo quy tắc hybrid

### 9.2 Khi đổi workspace

1. Cập nhật active workspace
2. Đánh giá lại active profile hiện tại còn hợp lệ hay không
3. Clear active profile nếu profile đó không thuộc workspace mới
4. Chỉ auto-select nếu workspace mới có đúng 1 profile hợp lệ

### 9.3 Khi gọi API protected

- luôn gửi workspace header cho các route protected theo workspace
- chỉ gửi profile header cho các route phụ thuộc vào profile
- không bao giờ suy ra header này từ header kia

## 10. Hình dạng của kế hoạch migration

Migration tương thích ngắn hạn:

1. Vẫn đọc client state cũ trong một giai đoạn chuyển tiếp
2. Validate nó với dữ liệu workspace/profile thật
3. Ghi lại normalized storage ngay sau khi validate xong
4. Loại bỏ ngay các runtime fallback nguy hiểm
5. Không để lại runtime path nào mà profile có thể đóng vai trò workspace

Đây là tương thích ngắn hạn có kiểm soát, không phải adapter layer vĩnh viễn.

## 11. Xử lý lỗi

### Frontend

- thiếu workspace: chặn flow và đưa user về chọn workspace
- workspace không hợp lệ: clear workspace và quay lại flow chọn context
- thiếu profile ở feature bắt buộc phải có profile: hiển thị UI chọn profile
- profile không hợp lệ: chỉ clear profile và yêu cầu chọn lại khi cần

### Backend

- trả về lỗi phân biệt rõ cho workspace và profile
- không gom lỗi ownership của workspace/profile thành generic internal error

## 12. Chiến lược kiểm thử

### Frontend

Cần test:

- storage migration không còn map profile ID sang workspace ID
- workspace selection vẫn đúng sau refresh
- profile bị clear khi chuyển sang workspace mà profile cũ không còn hợp lệ
- các page chỉ cần workspace vẫn load bình thường khi không có active profile
- các page cần profile sẽ yêu cầu chọn hoặc auto-select đúng theo quy tắc
- API client gắn header đúng cho request chỉ cần workspace và request cần cả workspace + profile

### Backend

Cần test:

- route protected theo workspace fail khi thiếu hoặc sai workspace header
- endpoint chỉ cần workspace pass mà không cần profile nếu contract cho phép
- endpoint cần profile fail khi thiếu profile
- endpoint cần profile fail khi profile không hợp lệ trong active workspace
- endpoint pass khi cặp workspace/profile hợp lệ

### Regression flow

- login thường
- Google login
- reset password
- create brand
- vào workspace settings
- create content
- social connect
- analytics/dashboard/team load đúng context

## 13. Tiêu chí hoàn thành

Công việc chỉ được coi là hoàn thành khi:

- không còn bất kỳ code path nào ở frontend coi profile là workspace hoặc workspace là profile
- workspace là nguồn sự thật duy nhất cho dashboard context
- profile là optional theo mặc định và chỉ bắt buộc ở các feature được chỉ rõ
- backend validation phản ánh đúng mô hình đó
- user local đang có legacy state hoặc được migrate sạch, hoặc bị reset context an toàn mà không làm hỏng session
- các lỗi misrouting giữa workspace/profile đã biết không còn tái hiện

## 14. Rủi ro

- một số backend service có thể vẫn còn assumption legacy gián tiếp qua repository hoặc controller usage
- một số màn frontend nhìn có vẻ chỉ cần workspace nhưng thực tế vẫn đang phụ thuộc vào profile-scoped contract
- rollout không đầy đủ trên tất cả protected route sẽ để sót regression ẩn

Giảm thiểu rủi ro:

- phân loại endpoint bị ảnh hưởng trước khi implement (xem chi tiết tại `docs/superpowers/plans/2026-06-16-profile-workspace-endpoint-matrix.md`)
- cập nhật FE và BE theo một trình tự có phối hợp
- chạy regression ở các flow có rủi ro cao đã biết

