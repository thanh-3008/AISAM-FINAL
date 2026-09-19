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
      final response = await _dio.get(
        '/content/review-queue',
        queryParameters: {'page': page, 'pageSize': pageSize},
      );
      final data = response.data is Map ? response.data['data'] : null;
      final items = (data is Map ? (data['data'] ?? data['items']) : data) as List? ?? [];
      if (items.isEmpty) return [];
      return await compute(_parseContentList, items);
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
    final trimmedReason = reason?.trim() ?? '';
    if (trimmedReason.length < 5) {
      throw ValidationException('Lý do từ chối bài viết phải có ít nhất 5 ký tự.');
    }
    if (trimmedReason.length > 1000) {
      throw ValidationException('Lý do từ chối không được vượt quá 1000 ký tự.');
    }
    try {
      final response = await _dio.post('/content/$id/reject', data: {'notes': trimmedReason});
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
