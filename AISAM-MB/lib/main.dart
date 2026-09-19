import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'app/app.dart';
import 'core/services/push_notification_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await dotenv.load(fileName: ".env");
  await initializeDateFormatting('vi_VN', null);

  // Initialize Firebase and Push Notifications
  await PushNotificationService.instance.initialize();

  runApp(
    const ProviderScope(
      child: MyApp(),
    ),
  );
}
