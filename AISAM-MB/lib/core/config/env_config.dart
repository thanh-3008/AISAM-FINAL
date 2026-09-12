import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter/foundation.dart';

class EnvConfig {
  EnvConfig._();

  static String get apiBaseUrl {
    const buildUrl = String.fromEnvironment('API_BASE_URL');
    return buildUrl.isNotEmpty
        ? buildUrl
        : dotenv.env['API_BASE_URL'] ?? 'http://localhost:5027/api';
  }

  static int get connectTimeoutMs =>
      int.tryParse(dotenv.env['CONNECT_TIMEOUT_MS'] ?? '30000') ?? 30000;

  static int get receiveTimeoutMs =>
      int.tryParse(dotenv.env['RECEIVE_TIMEOUT_MS'] ?? '60000') ?? 60000;

  static bool get isDebugMode => kDebugMode;
}
