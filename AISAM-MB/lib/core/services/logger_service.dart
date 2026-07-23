import 'package:logger/logger.dart';
import '../config/env_config.dart';

class LoggerService {
  LoggerService._();

  static final Logger _logger = Logger(
    printer: PrettyPrinter(
      methodCount: 2,
      errorMethodCount: 8,
      lineLength: 120,
      colors: true,
      printEmojis: true,
      printTime: true,
    ),
    filter: _AppLogFilter(),
  );

  static void v(dynamic message, [dynamic error, StackTrace? stackTrace]) {
    _logger.t(_maskSensitiveInfo(message.toString()), error: error, stackTrace: stackTrace);
  }

  static void d(dynamic message, [dynamic error, StackTrace? stackTrace]) {
    _logger.d(_maskSensitiveInfo(message.toString()), error: error, stackTrace: stackTrace);
  }

  static void i(dynamic message, [dynamic error, StackTrace? stackTrace]) {
    _logger.i(_maskSensitiveInfo(message.toString()), error: error, stackTrace: stackTrace);
  }

  static void w(dynamic message, [dynamic error, StackTrace? stackTrace]) {
    _logger.w(_maskSensitiveInfo(message.toString()), error: error, stackTrace: stackTrace);
  }

  static void e(dynamic message, [dynamic error, StackTrace? stackTrace]) {
    _logger.e(_maskSensitiveInfo(message.toString()), error: error, stackTrace: stackTrace);
  }

  static String _maskSensitiveInfo(String message) {
    if (message.isEmpty) return message;
    
    // Mask tokens and passwords
    final tokenRegex = RegExp(r'(token|password)["\s:=]+([^\s,}\"]+)', caseSensitive: false);
    return message.replaceAllMapped(tokenRegex, (match) {
      return '${match.group(1)}="********"';
    });
  }
}

class _AppLogFilter extends LogFilter {
  @override
  bool shouldLog(LogEvent event) {
    if (!EnvConfig.isDebugMode) {
      // In production, only log warnings and errors
      return event.level == Level.warning || event.level == Level.error;
    }
    return true;
  }
}
