import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/core/errors/app_exception.dart';

void main() {
  group('ExceptionHandler Unit Tests', () {
    test('extracts error from GenericResponse with message and errorCode', () {
      final dioError = DioException(
        requestOptions: RequestOptions(path: '/api/content/123/approve'),
        response: Response(
          requestOptions: RequestOptions(path: '/api/content/123/approve'),
          statusCode: 403,
          data: {
            'success': false,
            'message': 'You do not have permission to perform this action in the active workspace.',
            'error': {
              'errorCode': 'WORKSPACE_PERMISSION_DENIED',
              'errorMessage': 'You do not have permission to perform this action in the active workspace.',
            },
          },
        ),
        type: DioExceptionType.badResponse,
      );

      final exception = ExceptionHandler.handle(dioError);
      expect(exception, isA<AccessDeniedException>());
      expect(exception.message, 'You do not have permission to perform this action in the active workspace.');
      expect(exception.code, 'WORKSPACE_PERMISSION_DENIED');
    });

    test('extracts error from GenericResponse with status constraint', () {
      final dioError = DioException(
        requestOptions: RequestOptions(path: '/api/content/123/approve'),
        response: Response(
          requestOptions: RequestOptions(path: '/api/content/123/approve'),
          statusCode: 400,
          data: {
            'success': false,
            'message': 'Only pending approval content can be approved.',
            'statusCode': 400,
          },
        ),
        type: DioExceptionType.badResponse,
      );

      final exception = ExceptionHandler.handle(dioError);
      expect(exception, isA<ValidationException>());
      expect(exception.message, 'Only pending approval content can be approved.');
    });

    test('extracts error from RFC 7807 ProblemDetails detail field', () {
      final dioError = DioException(
        requestOptions: RequestOptions(path: '/api/content/123/approve'),
        response: Response(
          requestOptions: RequestOptions(path: '/api/content/123/approve'),
          statusCode: 400,
          data: {
            'title': 'Bad Request',
            'detail': 'Content is already in approved state.',
            'status': 400,
          },
        ),
        type: DioExceptionType.badResponse,
      );

      final exception = ExceptionHandler.handle(dioError);
      expect(exception, isA<ValidationException>());
      expect(exception.message, 'Content is already in approved state.');
    });

    test('extracts error from ASP.NET Core validation errors map', () {
      final dioError = DioException(
        requestOptions: RequestOptions(path: '/api/content/123/reject'),
        response: Response(
          requestOptions: RequestOptions(path: '/api/content/123/reject'),
          statusCode: 400,
          data: {
            'title': 'One or more validation errors occurred.',
            'status': 400,
            'errors': {
              'Notes': ['Rejection notes must be at least 5 characters.'],
            },
          },
        ),
        type: DioExceptionType.badResponse,
      );

      final exception = ExceptionHandler.handle(dioError);
      expect(exception, isA<ValidationException>());
      expect(exception.message, 'Rejection notes must be at least 5 characters.');
    });

    test('handles JSON string in response.data without failing', () {
      final dioError = DioException(
        requestOptions: RequestOptions(path: '/api/content/123/approve'),
        response: Response(
          requestOptions: RequestOptions(path: '/api/content/123/approve'),
          statusCode: 403,
          data: '{"message": "Workspace permission denied", "errorCode": "FORBIDDEN"}',
        ),
        type: DioExceptionType.badResponse,
      );

      final exception = ExceptionHandler.handle(dioError);
      expect(exception, isA<AccessDeniedException>());
      expect(exception.message, 'Workspace permission denied');
      expect(exception.code, 'FORBIDDEN');
    });

    test('handles plain text string in response.data', () {
      final dioError = DioException(
        requestOptions: RequestOptions(path: '/api/content/123/approve'),
        response: Response(
          requestOptions: RequestOptions(path: '/api/content/123/approve'),
          statusCode: 400,
          data: 'Missing or invalid X-Workspace-Id header.',
        ),
        type: DioExceptionType.badResponse,
      );

      final exception = ExceptionHandler.handle(dioError);
      expect(exception, isA<ValidationException>());
      expect(exception.message, 'Missing or invalid X-Workspace-Id header.');
    });

    test('provides helpful fallback for empty 403/404/500 responses', () {
      final forbiddenError = DioException(
        requestOptions: RequestOptions(path: '/api/test'),
        response: Response(requestOptions: RequestOptions(path: '/api/test'), statusCode: 403, data: null),
        type: DioExceptionType.badResponse,
      );
      expect(ExceptionHandler.handle(forbiddenError).message, 'Bạn không có quyền thực hiện thao tác này trong workspace hiện tại.');

      final notFoundError = DioException(
        requestOptions: RequestOptions(path: '/api/test'),
        response: Response(requestOptions: RequestOptions(path: '/api/test'), statusCode: 404, data: null),
        type: DioExceptionType.badResponse,
      );
      final notFoundEx = ExceptionHandler.handle(notFoundError);
      expect(notFoundEx, isA<NotFoundException>());
      expect(notFoundEx.message, 'Không tìm thấy nội dung yêu cầu.');

      final serverError = DioException(
        requestOptions: RequestOptions(path: '/api/test'),
        response: Response(requestOptions: RequestOptions(path: '/api/test'), statusCode: 500, data: null),
        type: DioExceptionType.badResponse,
      );
      expect(ExceptionHandler.handle(serverError), isA<ServerException>());
      expect(ExceptionHandler.handle(serverError).message, 'Máy chủ gặp sự cố nội bộ. Vui lòng thử lại sau.');
    });

    test('handles network timeouts with clear message', () {
      final timeoutError = DioException(
        requestOptions: RequestOptions(path: '/api/test'),
        type: DioExceptionType.connectionTimeout,
      );
      final exception = ExceptionHandler.handle(timeoutError);
      expect(exception, isA<NetworkException>());
      expect(exception.message, 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.');
    });
  });
}
