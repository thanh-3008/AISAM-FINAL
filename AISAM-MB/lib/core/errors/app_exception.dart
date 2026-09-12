import 'dart:convert';
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

class NotFoundException extends AppException {
  NotFoundException(super.message, {super.code, super.originalError});
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
          'Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.',
          originalError: error,
        );
      case DioExceptionType.cancel:
        return UnknownException(
          error.message ?? 'Yêu cầu đã bị hủy.',
          originalError: error,
        );
      case DioExceptionType.badResponse:
        final statusCode = error.response?.statusCode;
        String message = '';
        String? code;

        try {
          dynamic data = error.response?.data;
          // If response data is a String, attempt to decode it as JSON or use as plain text
          if (data is String && data.trim().isNotEmpty) {
            try {
              data = jsonDecode(data);
            } catch (_) {
              final trimmed = data.trim();
              if (!trimmed.startsWith('<') && trimmed.length <= 500) {
                message = trimmed;
              }
            }
          }

          if (data is Map) {
            final detail = data['error'];
            if (detail is Map) {
              message = detail['errorMessage']?.toString() ??
                  detail['message']?.toString() ??
                  '';
              code = detail['errorCode']?.toString();
            } else if (detail is String && detail.trim().isNotEmpty) {
              message = detail.trim();
            }

            if (message.isEmpty) {
              message = data['message']?.toString() ??
                  data['errorMessage']?.toString() ??
                  data['detail']?.toString() ??
                  data['title']?.toString() ??
                  '';
            }
            code ??= data['errorCode']?.toString() ?? data['code']?.toString();

            // Extract ASP.NET Core model validation errors if message is still empty or generic
            if ((message.isEmpty || message == 'One or more validation errors occurred.') &&
                data['errors'] is Map) {
              final errorsMap = data['errors'] as Map;
              for (final val in errorsMap.values) {
                if (val is List && val.isNotEmpty) {
                  message = val.first.toString();
                  break;
                } else if (val is String && val.isNotEmpty) {
                  message = val;
                  break;
                }
              }
            }
          }
        } catch (_) {
          // Ignore parsing error
        }

        // Meaningful Vietnamese fallbacks based on HTTP status code if no server message
        if (message.trim().isEmpty) {
          switch (statusCode) {
            case 400:
              message = 'Yêu cầu không hợp lệ hoặc dữ liệu không đúng.';
              break;
            case 401:
              message = 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
              break;
            case 403:
              message = 'Bạn không có quyền thực hiện thao tác này trong workspace hiện tại.';
              break;
            case 404:
              message = 'Không tìm thấy nội dung yêu cầu.';
              break;
            case 409:
              message = 'Dữ liệu đã bị thay đổi hoặc xung đột. Vui lòng tải lại.';
              break;
            case 413:
              message = 'Dung lượng tải lên vượt quá giới hạn cho phép.';
              break;
            case 422:
              message = 'Dữ liệu không đáp ứng quy chuẩn xử lý.';
              break;
            case 500:
              message = 'Máy chủ gặp sự cố nội bộ. Vui lòng thử lại sau.';
              break;
            case 502:
            case 503:
            case 504:
              message = 'Máy chủ đang bảo trì hoặc tạm thời không phản hồi. Vui lòng thử lại sau.';
              break;
            default:
              message = 'Lỗi phản hồi từ máy chủ (${statusCode ?? "unknown"}).';
          }
        }

        if (statusCode == 401) {
          return UnauthorizedException(
            message,
            code: code,
            originalError: error,
          );
        } else if (statusCode == 403) {
          return AccessDeniedException(
            message,
            code: code,
            originalError: error,
          );
        } else if (statusCode == 404) {
          return NotFoundException(
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
          'Đã xảy ra lỗi không xác định.',
          originalError: error,
        );
    }
  }
}
