import 'package:dio/dio.dart';
import 'generic_response.dart';

abstract class AppException implements Exception {
  final String message;
  final String? code;
  final dynamic originalError;

  AppException(this.message, {this.code, this.originalError});

  @override
  String toString() {
    if (code != null) return '[$code] $message';
    return message;
  }
}

class NetworkException extends AppException {
  NetworkException(super.message, {super.code, super.originalError});
}

class ServerException extends AppException {
  ServerException(super.message, {super.code, super.originalError});
}

class UnauthorizedException extends AppException {
  UnauthorizedException(super.message, {super.code, super.originalError});
}

class ValidationException extends AppException {
  ValidationException(super.message, {super.code, super.originalError});
}

class UnknownException extends AppException {
  UnknownException(super.message, {super.code, super.originalError});
}

class ExceptionHandler {
  static AppException handle(dynamic error) {
    if (error is DioException) {
      return _handleDioError(error);
    }
    if (error is AppException) {
      return error;
    }
    return UnknownException(error.toString(), originalError: error);
  }

  static AppException _handleDioError(DioException error) {
    switch (error.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
      case DioExceptionType.connectionError:
        return NetworkException('Network connection timeout.', originalError: error);
      case DioExceptionType.badResponse:
        final statusCode = error.response?.statusCode;
        String message = 'Unexpected server response.';
        String? code;
        
        try {
          if (error.response?.data != null) {
            final data = error.response!.data;
            if (data is Map<String, dynamic>) {
              // Try to map to GenericResponse
              final resp = GenericResponse<dynamic>.fromJson(data, (json) => json);
              message = resp.message ?? resp.error?.errorMessage ?? message;
              code = resp.error?.errorCode;
            }
          }
        } catch (_) {
          // Ignore parsing error
        }

        if (statusCode == 401) {
          return UnauthorizedException(message, code: code, originalError: error);
        } else if (statusCode == 400 || statusCode == 422) {
          return ValidationException(message, code: code, originalError: error);
        } else if (statusCode != null && statusCode >= 500) {
          return ServerException(message, code: code, originalError: error);
        }
        return UnknownException(message, code: code, originalError: error);
      default:
        return UnknownException('An unexpected error occurred.', originalError: error);
    }
  }
}
