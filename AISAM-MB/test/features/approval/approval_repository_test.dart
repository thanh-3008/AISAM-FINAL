import 'package:flutter_test/flutter_test.dart';
import 'package:dio/dio.dart';
import 'package:aisam_mb/features/approval/data/repositories/approval_repository.dart';
import 'package:aisam_mb/features/content/data/models/enums.dart';

class MockDio extends Fake implements Dio {
  final Map<String, dynamic> responses;
  final Map<String, DioException> errors;

  MockDio({this.responses = const {}, this.errors = const {}});

  @override
  Future<Response<T>> get<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
    ProgressCallback? onReceiveProgress,
  }) async {
    final key = '$path?${queryParameters?.entries.map((e) => '${e.key}=${e.value}').join('&')}';
    if (errors.containsKey(key)) {
      throw errors[key]!;
    }
    if (errors.containsKey(path)) {
      throw errors[path]!;
    }
    if (responses.containsKey(key)) {
      return Response<T>(
        data: responses[key] as T,
        statusCode: 200,
        requestOptions: RequestOptions(path: path),
      );
    }
    if (responses.containsKey(path)) {
      return Response<T>(
        data: responses[path] as T,
        statusCode: 200,
        requestOptions: RequestOptions(path: path),
      );
    }
    return Response<T>(
      data: {'success': true, 'data': {'data': []}} as T,
      statusCode: 200,
      requestOptions: RequestOptions(path: path),
    );
  }

  @override
  Future<Response<T>> post<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
    ProgressCallback? onSendProgress,
    ProgressCallback? onReceiveProgress,
  }) async {
    if (responses.containsKey(path)) {
      return Response<T>(
        data: responses[path] as T,
        statusCode: 200,
        requestOptions: RequestOptions(path: path),
      );
    }
    return Response<T>(
      data: {'success': true, 'data': {}} as T,
      statusCode: 200,
      requestOptions: RequestOptions(path: path),
    );
  }
}

void main() {
  group('ApprovalRepository Tests', () {
    final sampleItem1 = {
      'id': '11111111-1111-1111-1111-111111111111',
      'profileId': '22222222-2222-2222-2222-222222222222',
      'brandId': '33333333-3333-3333-3333-333333333333',
      'adType': 1,
      'title': 'Pending Post 1',
      'status': 1, // PendingApproval
      'createdAt': '2026-09-12T00:00:00.000Z',
    };

    final sampleItem2 = {
      'id': '44444444-4444-4444-4444-444444444444',
      'profileId': '22222222-2222-2222-2222-222222222222',
      'brandId': '55555555-5555-5555-5555-555555555555',
      'adType': 0,
      'title': 'Pending Post 2 (Review Queue)',
      'status': 'PendingApproval',
      'createdAt': '2026-09-12T01:00:00.000Z',
    };

    test('getPendingApprovals returns items from /content?status=1', () async {
      final mockDio = MockDio(responses: {
        '/content?page=1&pageSize=100&status=1': {
          'success': true,
          'data': {
            'data': [sampleItem1],
            'totalCount': 1,
          }
        },
      });

      final repository = ApprovalRepository(mockDio);
      final result = await repository.getPendingApprovals();

      expect(result.length, 1);
      expect(result.first.id, sampleItem1['id']);
      expect(result.first.status, ContentStatusEnum.pendingApproval);
    });

    test('getPendingApprovals merges and deduplicates items from review-queue', () async {
      final mockDio = MockDio(responses: {
        '/content?page=1&pageSize=100&status=1': {
          'success': true,
          'data': {
            'data': [sampleItem1],
            'totalCount': 1,
          }
        },
        '/content/review-queue?page=1&pageSize=100': {
          'success': true,
          'data': {
            'data': [sampleItem1, sampleItem2], // sampleItem1 duplicate + sampleItem2
            'totalCount': 2,
          }
        },
      });

      final repository = ApprovalRepository(mockDio);
      final result = await repository.getPendingApprovals();

      expect(result.length, 2);
      expect(result.any((e) => e.id == sampleItem1['id']), isTrue);
      expect(result.any((e) => e.id == sampleItem2['id']), isTrue);
    });

    test('getPendingApprovals falls back to /content client filtering when primary is empty', () async {
      final draftItem = {
        'id': '99999999-9999-9999-9999-999999999999',
        'profileId': '22222222-2222-2222-2222-222222222222',
        'brandId': '33333333-3333-3333-3333-333333333333',
        'adType': 0,
        'status': 0, // Draft
      };

      final mockDio = MockDio(responses: {
        '/content?page=1&pageSize=100&status=1': {
          'success': true,
          'data': {'data': []},
        },
        '/content/review-queue?page=1&pageSize=100': {
          'success': true,
          'data': {'data': []},
        },
        '/content?page=1&pageSize=100': {
          'success': true,
          'data': {
            'data': [draftItem, sampleItem1],
            'totalCount': 2,
          }
        },
      });

      final repository = ApprovalRepository(mockDio);
      final result = await repository.getPendingApprovals();

      expect(result.length, 1);
      expect(result.first.id, sampleItem1['id']);
      expect(result.first.status, ContentStatusEnum.pendingApproval);
    });

    test('getHistoryApprovals filters approved and rejected posts', () async {
      final approvedItem = {
        ...sampleItem1,
        'id': 'approved-1',
        'status': 2, // Approved
      };
      final rejectedItem = {
        ...sampleItem1,
        'id': 'rejected-1',
        'status': 3, // Rejected
      };
      final pendingItem = {
        ...sampleItem1,
        'id': 'pending-1',
        'status': 1, // PendingApproval
      };

      final mockDio = MockDio(responses: {
        '/content?page=1&pageSize=100&sortBy=updatedAt&sortDescending=true': {
          'success': true,
          'data': {
            'data': [approvedItem, pendingItem, rejectedItem],
            'totalCount': 3,
          }
        },
      });

      final repository = ApprovalRepository(mockDio);
      final result = await repository.getHistoryApprovals();

      expect(result.length, 2);
      expect(result.map((e) => e.id), containsAll(['approved-1', 'rejected-1']));
    });
  });
}
