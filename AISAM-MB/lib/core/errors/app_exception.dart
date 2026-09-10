import 'package:dio/dio.dart';

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

class AccessDeniedException extends AppException {
  AccessDeniedException(super.message, {super.code, super.originalError});
}

class ConflictException extends AppException {
  ConflictException(super.message, {super.code, super.originalError});
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
        return NetworkException(
          'Network connection timeout.',
          originalError: error,
        );
      case DioExceptionType.badResponse:
        final statusCode = error.response?.statusCode;
        String message = 'Unexpected server response.';
        String? code;

        try {
          if (error.response?.data != null) {
            final data = error.response!.data;
            if (data is Map<String, dynamic>) {
              // Try to map to GenericResponse
              final detail = data['error'];
              message =
                  (detail is Map ? detail['errorMessage'] : null)?.toString() ??
                  data['message']?.toString() ??
                  message;
              code =
                  (detail is Map ? detail['errorCode'] : null)?.toString() ??
                  data['errorCode']?.toString();
            }
          }
        } catch (_) {
          // Ignore parsing error
        }

        if (statusCode == 401) {
          return UnauthorizedException(
            message,
            code: code,
            originalError: error,
          );
        } else if (statusCode == 403 || statusCode == 404) {
          return AccessDeniedException(
            message,
            code: code,
            originalError: error,
          );
        } else if (statusCode == 409) {
          return ConflictException(message, code: code, originalError: error);
        } else if (statusCode == 400 || statusCode == 422) {
          return ValidationException(message, code: code, originalError: error);
        } else if (statusCode == 413) {
          return ValidationException(
            'Payload too large. File must be smaller.',
            code: code,
            originalError: error,
          );
        } else if (statusCode != null && statusCode >= 500) {
          return ServerException(message, code: code, originalError: error);
        }
        return UnknownException(message, code: code, originalError: error);
      default:
        return UnknownException(
          'An unexpected error occurred.',
          originalError: error,
        );
    }
  }
}
