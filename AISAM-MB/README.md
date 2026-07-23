# AISAM Mobile App (AISAM-MB)

Đây là mã nguồn ứng dụng di động Flutter của dự án **AISAM** (AI Ad Manager).

## Yêu cầu môi trường

Để chạy và phát triển dự án này, bạn cần cài đặt:
- **Flutter SDK** (phiên bản mới nhất hoặc >= 3.0)
- **Dart SDK**
- **Android Studio** hoặc **VS Code** (kèm theo các plugin Flutter/Dart tương ứng)
- Một thiết bị Android thật (cắm qua cáp USB) hoặc máy ảo Android Emulator.

## Hướng dẫn chạy Local (Phát triển nội bộ)

Dự án Mobile này kết nối với Backend `.NET` (`AISAM-BE`) và sử dụng OAuth (như Facebook, TikTok) để kết nối mạng xã hội. Vì Backend đang chạy ở môi trường `localhost` (thường là port 5027) trên máy tính của bạn, thiết bị di động (hoặc máy ảo) sẽ không thể gọi trực tiếp `http://localhost:5027` trừ khi bạn cấu hình port forwarding (adb reverse).

Hãy làm theo các bước dưới đây để chạy app trên máy:

### 1. Khởi động Backend (AISAM-BE)
Mở terminal tại thư mục `AISAM-BE` và chạy lệnh sau để khởi động API:
```bash
dotnet run --project AISAM.API
```
*Lưu ý:* Backend sẽ chạy ở cổng `5027` (hoặc cấu hình tuỳ theo máy bạn). Đảm bảo API đang chạy thành công.

### 2. Thiết lập kết nối localhost cho thiết bị di động (Quan trọng)
Để thiết bị di động (Android) có thể gọi `http://localhost:5027` (hoặc cổng tương ứng) và để chức năng đăng nhập mạng xã hội qua WebView hoạt động đúng với callback của Backend, bạn bắt buộc phải dùng lệnh `adb reverse`.

Mở một terminal mới (đảm bảo thiết bị đã được kết nối với máy tính, gõ `adb devices` để kiểm tra) và chạy:
```bash
adb reverse tcp:5027 tcp:5027
```
*(Thay số 5027 bằng cổng thực tế của Backend nếu Backend của bạn chạy ở cổng khác).*

Lệnh này giúp chuyển hướng toàn bộ traffic gửi tới `localhost:5027` trên điện thoại về lại máy tính của bạn. Bạn **phải chạy lệnh này mỗi khi cắm lại cáp hoặc khởi động lại máy ảo**.

### 3. Cài đặt các thư viện phụ thuộc của Flutter
Mở terminal tại thư mục `AISAM-MB` và chạy:
```bash
flutter pub get
```

### 4. Build các file tự sinh (Tuỳ chọn nếu có lỗi build)
Dự án sử dụng `freezed` và `json_serializable` để tạo data class, hoặc `riverpod` để tạo provider. Nếu bạn có thay đổi model hoặc khi clone code mới về, hãy chạy lệnh sau để sinh file `.g.dart` và `.freezed.dart`:
```bash
flutter pub run build_runner build --delete-conflicting-outputs
```

### 5. Chạy ứng dụng
Sau khi đã chạy lệnh `adb reverse`, bạn có thể tiến hành chạy app:
```bash
flutter run
```
Hoặc chạy trực tiếp từ nút Run/Debug trong Android Studio / VS Code.

---

## Lưu ý về tính năng Mạng Xã Hội (Social Connection)
- Ứng dụng sử dụng `webview_flutter` để đăng nhập OAuth mạng xã hội (Facebook, TikTok...).
- Trong quá trình đăng nhập, WebView sẽ chuyển hướng tới các đường dẫn callback của Backend (ví dụ: `http://localhost:5027/api/social-auth/facebook/callback`).
- Nếu WebView báo lỗi *net::ERR_CONNECTION_REFUSED*, điều đó có nghĩa là lệnh `adb reverse tcp:5027 tcp:5027` chưa được chạy hoặc thiết bị đã bị ngắt kết nối. Hãy chạy lại lệnh `adb reverse`.
## Hướng dẫn Build APK (Deploy cho User)

Khi bạn muốn đóng gói ứng dụng để đưa cho người dùng cuối cài đặt (file APK) và không dùng `localhost` nữa, hãy làm theo các bước sau:

### 1. Triển khai Backend (AISAM-BE) lên Server
Đầu tiên, bạn cần có một Server thực tế (VD: VPS, Azure, AWS...) để chạy Backend. Đảm bảo API đã được đưa lên mạng và có một đường dẫn public (Ví dụ: `https://api.aisam.com`).

### 2. Cấu hình đường dẫn API cho Mobile
Tạo hoặc mở file `.env` ở thư mục gốc của dự án `AISAM-MB` (ngang hàng với thư mục `lib`). Đổi địa chỉ `API_BASE_URL` thành đường dẫn Backend thực tế của bạn:
```env
API_BASE_URL=https://api.aisam.com/api
```
*(Lưu ý: Không dùng `localhost` hay `10.0.2.2` vì điện thoại của người dùng sẽ không thể truy cập vào máy tính của bạn).*

### 3. Build file APK
Mở terminal tại thư mục `AISAM-MB` và chạy lệnh sau để đóng gói ứng dụng:
```bash
flutter build apk --release
```

### 4. Lấy file APK
Sau khi tiến trình build hoàn tất, file APK sẽ được tạo ra tại đường dẫn:
`build/app/outputs/flutter-apk/app-release.apk`

Bạn chỉ cần gửi file `app-release.apk` này cho người dùng cài đặt là họ có thể sử dụng trực tiếp kết nối với hệ thống Server thực tế mà không cần cấu hình gì thêm.
