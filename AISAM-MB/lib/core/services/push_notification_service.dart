import 'dart:async';
import 'dart:io';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../features/notifications/data/notification_api_client.dart';
import 'logger_service.dart';

/// Top-level background message handler for FCM.
/// Must be annotated with @pragma('vm:entry-point') so Flutter engine preserves it.
@pragma('vm:entry-point')
Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  try {
    await Firebase.initializeApp();
  } catch (_) {
    // Handled silently if default app is already initialized
  }
}

class PushNotificationService {
  PushNotificationService._();
  static final PushNotificationService instance = PushNotificationService._();

  FirebaseMessaging? _messaging;
  FlutterLocalNotificationsPlugin? _localNotifications;
  String? _fcmToken;
  bool _initialized = false;

  static const AndroidNotificationChannel _channel = AndroidNotificationChannel(
    'aisam_high_importance',
    'AISAM Thông báo quan trọng',
    description: 'Kênh thông báo ưu tiên cao của AISAM khi tắt màn hình hoặc chạy ngầm.',
    importance: Importance.max,
    playSound: true,
    enableVibration: true,
  );

  String? get fcmToken => _fcmToken;
  bool get isInitialized => _initialized;

  Future<void> initialize({Function(RemoteMessage)? onNotificationOpened}) async {
    if (_initialized) return;

    try {
      // 1. Initialize Firebase Core
      try {
        await Firebase.initializeApp();
      } catch (e) {
        LoggerService.w('Firebase.initializeApp() skipped or already active: $e');
      }

      // If Firebase apps list is still empty (e.g. running in pure unit test without mocks), exit gracefully
      if (Firebase.apps.isEmpty) {
        LoggerService.w('PushNotificationService: No Firebase app configured, skipping FCM initialization.');
        return;
      }

      _messaging = FirebaseMessaging.instance;
      _localNotifications = FlutterLocalNotificationsPlugin();

      // 2. Register Background Handler
      FirebaseMessaging.onBackgroundMessage(firebaseMessagingBackgroundHandler);

      // 3. Setup Android Channel & Local Notifications
      const androidSettings = AndroidInitializationSettings('@mipmap/ic_launcher');
      const darwinSettings = DarwinInitializationSettings(
        requestAlertPermission: true,
        requestBadgePermission: true,
        requestSoundPermission: true,
      );

      const initSettings = InitializationSettings(
        android: androidSettings,
        iOS: darwinSettings,
      );

      await _localNotifications?.initialize(
        settings: initSettings,
        onDidReceiveNotificationResponse: (response) {
          LoggerService.i('Notification tapped: ${response.payload}');
        },
      );

      // Create Android Notification Channel
      await _localNotifications
          ?.resolvePlatformSpecificImplementation<AndroidFlutterLocalNotificationsPlugin>()
          ?.createNotificationChannel(_channel);

      // 4. Request OS Permissions
      final settings = await _messaging?.requestPermission(
        alert: true,
        badge: true,
        sound: true,
        provisional: false,
      );
      LoggerService.i('Push notification authorization status: ${settings?.authorizationStatus}');

      // 5. Configure Foreground Presentation Options
      await _messaging?.setForegroundNotificationPresentationOptions(
        alert: true,
        badge: true,
        sound: true,
      );

      // 6. Listen to Foreground Messages
      FirebaseMessaging.onMessage.listen((RemoteMessage message) {
        _handleForegroundMessage(message);
      });

      // 7. Listen to Notification Opened while in Background/Terminated
      FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
        LoggerService.i('User opened app from notification: ${message.messageId}');
        onNotificationOpened?.call(message);
      });

      // Check if opened from a terminated state
      final initialMessage = await _messaging?.getInitialMessage();
      if (initialMessage != null) {
        LoggerService.i('App launched from terminated state by notification: ${initialMessage.messageId}');
        onNotificationOpened?.call(initialMessage);
      }

      // 8. Fetch and Cache FCM Token
      try {
        _fcmToken = await _messaging?.getToken();
        LoggerService.i('FCM Token: ${_fcmToken != null ? "Acquired (${_fcmToken!.substring(0, 10)}...)" : "null"}');
      } catch (e) {
        LoggerService.w('Could not retrieve FCM token: $e');
      }

      _initialized = true;
    } catch (e) {
      LoggerService.e('PushNotificationService initialization failed: $e');
    }
  }

  void _handleForegroundMessage(RemoteMessage message) {
    final notification = message.notification;
    if (notification == null) return;

    _localNotifications?.show(
      id: notification.hashCode,
      title: notification.title,
      body: notification.body,
      notificationDetails: NotificationDetails(
        android: AndroidNotificationDetails(
          _channel.id,
          _channel.name,
          channelDescription: _channel.description,
          importance: Importance.max,
          priority: Priority.high,
          icon: '@mipmap/ic_launcher',
          playSound: true,
          enableVibration: true,
        ),
        iOS: const DarwinNotificationDetails(
          presentAlert: true,
          presentBadge: true,
          presentSound: true,
        ),
      ),
      payload: message.data['notificationId'],
    );
  }

  /// Register current device token with AISAM Backend
  Future<void> registerWithBackend(NotificationApiClient apiClient) async {
    try {
      if (_messaging != null && _fcmToken == null) {
        _fcmToken = await _messaging?.getToken();
      }

      if (_fcmToken == null || _fcmToken!.isEmpty) {
        LoggerService.w('No FCM token available to register with backend.');
        return;
      }

      final platform = kIsWeb ? 'web' : (Platform.isIOS ? 'ios' : 'android');
      await apiClient.registerDeviceToken(
        token: _fcmToken!,
        platform: platform,
        deviceName: platform,
      );
      LoggerService.i('Device token successfully registered with backend.');
    } catch (e) {
      LoggerService.w('Failed to register device token with backend: $e');
    }
  }

  /// Unregister device token with AISAM Backend (e.g. upon user logout)
  Future<void> unregisterWithBackend(NotificationApiClient apiClient) async {
    try {
      if (_fcmToken == null || _fcmToken!.isEmpty) return;

      await apiClient.unregisterDeviceToken(_fcmToken!);
      LoggerService.i('Device token successfully unregistered with backend.');
    } catch (e) {
      LoggerService.w('Failed to unregister device token with backend: $e');
    }
  }
}

final pushNotificationServiceProvider = Provider<PushNotificationService>((ref) {
  return PushNotificationService.instance;
});
