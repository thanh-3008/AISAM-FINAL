import 'package:flutter_test/flutter_test.dart';
import 'package:dio/dio.dart';
import 'package:aisam_mb/features/notifications/data/notification_api_client.dart';
import 'package:aisam_mb/core/services/push_notification_service.dart';

void main() {
  group('Push Notification & Device Token Tests', () {
    late Dio dio;
    late List<RequestOptions> capturedRequests;
    late NotificationApiClient client;

    setUp(() {
      capturedRequests = [];
      dio = Dio(BaseOptions(baseUrl: 'http://localhost/api'));
      dio.interceptors.add(
        InterceptorsWrapper(
          onRequest: (options, handler) {
            capturedRequests.add(options);
            return handler.resolve(
              Response(
                requestOptions: options,
                statusCode: 200,
                data: {
                  'success': true,
                  'data': true,
                  'message': 'OK',
                },
              ),
            );
          },
        ),
      );
      client = NotificationApiClient(dio);
    });

    test('registerDeviceToken sends correct POST payload to /notifications/devices/register', () async {
      await client.registerDeviceToken(
        token: 'test_fcm_token_999',
        platform: 'android',
        deviceName: 'Pixel 8',
      );

      expect(capturedRequests.length, 1);
      final req = capturedRequests.first;
      expect(req.method, 'POST');
      expect(req.path, '/notifications/devices/register');
      expect(req.data, {
        'token': 'test_fcm_token_999',
        'platform': 'android',
        'deviceName': 'Pixel 8',
      });
    });

    test('unregisterDeviceToken sends correct POST payload to /notifications/devices/unregister', () async {
      await client.unregisterDeviceToken('test_fcm_token_999');

      expect(capturedRequests.length, 1);
      final req = capturedRequests.first;
      expect(req.method, 'POST');
      expect(req.path, '/notifications/devices/unregister');
      expect(req.data, {
        'token': 'test_fcm_token_999',
      });
    });

    test('PushNotificationService singleton exists and handles uninitialized Firebase gracefully', () async {
      final service = PushNotificationService.instance;
      expect(service, isNotNull);
      expect(service.fcmToken, isNull);

      // Register without cached token must return smoothly without throwing exceptions
      await service.registerWithBackend(client);
      await service.unregisterWithBackend(client);
    });
  });
}
