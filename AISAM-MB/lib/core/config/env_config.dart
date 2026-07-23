import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter/foundation.dart';

class EnvConfig {
  EnvConfig._();

  static String get apiBaseUrl =>
      dotenv.env['API_BASE_URL'] ?? 'http://localhost:5027/api';

  static int get connectTimeoutMs =>
      int.tryParse(dotenv.env['CONNECT_TIMEOUT_MS'] ?? '10000') ?? 10000;

  static int get receiveTimeoutMs =>
      int.tryParse(dotenv.env['RECEIVE_TIMEOUT_MS'] ?? '15000') ?? 15000;

  static bool get isDebugMode => kDebugMode;
}
