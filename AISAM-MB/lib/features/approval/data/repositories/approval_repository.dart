import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../content/data/models/content_model.dart';
import '../../../content/data/models/enums.dart';

part 'approval_repository.g.dart';

List<ContentResponseModel> _parseContentList(List<dynamic> items) {
  final result = <ContentResponseModel>[];
  for (final item in items) {
    if (item is Map) {
      try {
        result.add(ContentResponseModel.fromJson(Map<String, dynamic>.from(item)));
      } catch (e, st) {
        debugPrint('Failed to parse approval item: $e\n$st\nData: $item');
      }
    }
  }
  return result;
}

class ApprovalRepository {
  final Dio _dio;

  ApprovalRepository(this._dio);

  Future<List<ContentResponseModel>> getPendingApprovals({int page = 1, int pageSize = 100}) async {
    try {
      final itemsMap = <String, ContentResponseModel>{};

      // 1. Primary: Query /content?status=1 (PendingApproval)
      // Matches web implementation and avoids backend PermissionReviewQueue filter block
      // for users who are not delegated reviewers.
      try {
        final queryParams = {
          'page': page,
          'pageSize': pageSize,
          'status': 1,
        };
        final response = await _dio.get('/content', queryParameters: queryParams);
        final data = response.data is Map ? response.data['data'] : null;
        if (data != null && data['data'] is List) {
          final parsed = await compute(_parseContentList, data['data'] as List);
          for (final item in parsed) {
            if (item.status == ContentStatusEnum.pendingApproval) {
              itemsMap[item.id] = item;
            }
          }
        }
      } on DioException catch (dioErr) {
        if (dioErr.response?.statusCode == 401 ||
            dioErr.type == DioExceptionType.connectionError ||
            dioErr.type == DioExceptionType.connectionTimeout) {
          throw ExceptionHandler.handle(dioErr);
        }
        debugPrint('ApprovalRepository: GET /content?status=1 error: $dioErr');
      }

      // 2. Secondary: Query /content/review-queue to catch delegated brand review items
      try {
        final rqResponse = await _dio.get('/content/review-queue', queryParameters: {
          'page': page,
          'pageSize': pageSize,
        });
        final rqData = rqResponse.data is Map ? rqResponse.data['data'] : null;
        if (rqData != null && rqData['data'] is List) {
          final parsed = await compute(_parseContentList, rqData['data'] as List);
          for (final item in parsed) {
            if (item.status == ContentStatusEnum.pendingApproval) {
              itemsMap[item.id] = item;
            }
          }
        }
      } on DioException catch (dioErr) {
        if (dioErr.response?.statusCode == 401) {
          throw ExceptionHandler.handle(dioErr);
        }
        debugPrint('ApprovalRepository: GET /content/review-queue error: $dioErr');
      }

      // 3. Fallback: If still empty, query /content without status filter and filter client-side
      if (itemsMap.isEmpty) {
        try {
          final fallbackResponse = await _dio.get('/content', queryParameters: {
            'page': page,
            'pageSize': pageSize,
          });
          final fbData = fallbackResponse.data is Map ? fallbackResponse.data['data'] : null;
          if (fbData != null && fbData['data'] is List) {
            final parsed = await compute(_parseContentList, fbData['data'] as List);
            for (final item in parsed) {
              if (item.status == ContentStatusEnum.pendingApproval) {
                itemsMap[item.id] = item;
              }
            }
          }
        } catch (fbErr) {
          debugPrint('ApprovalRepository: GET /content fallback error: $fbErr');
        }
      }

      return itemsMap.values.toList();
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> approveContent(String id) async {
    try {
      final response = await _dio.post('/content/$id/approve');
      final data = response.data;
      if (data is Map && data['data'] is Map) {
        return ContentResponseModel.fromJson(Map<String, dynamic>.from(data['data']));
      }
      throw UnknownException('Dữ liệu phản hồi duyệt bài không hợp lệ.');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> rejectContent(String id, {String? reason}) async {
    try {
      final response = await _dio.post('/content/$id/reject', data: {'notes': reason});
      final data = response.data;
      if (data is Map && data['data'] is Map) {
        return ContentResponseModel.fromJson(Map<String, dynamic>.from(data['data']));
      }
      throw UnknownException('Dữ liệu phản hồi từ chối bài không hợp lệ.');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<List<ContentResponseModel>> getHistoryApprovals({int page = 1, int pageSize = 100}) async {
    try {
      final queryParams = {
        'page': page,
        'pageSize': pageSize,
        'sortBy': 'updatedAt',
        'sortDescending': true,
      };
      final response = await _dio.get('/content', queryParameters: queryParams);
      final data = response.data is Map ? response.data['data'] : null;
      if (data == null || data['data'] == null) return [];
      
      final items = data['data'] as List;
      if (items.isEmpty) return [];
      final parsedItems = await compute(_parseContentList, items);
      return parsedItems.where((e) => e.status == ContentStatusEnum.approved || e.status == ContentStatusEnum.rejected).toList();
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
ApprovalRepository approvalRepository(ApprovalRepositoryRef ref) {
  return ApprovalRepository(ref.read(dioProvider));
}
