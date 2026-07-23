import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../content/data/models/content_model.dart';
import '../../../content/data/models/enums.dart';

part 'approval_repository.g.dart';

class ApprovalRepository {
  final Dio _dio;

  ApprovalRepository(this._dio);

  Future<List<ContentResponseModel>> getPendingApprovals({int page = 1, int pageSize = 100}) async {
    try {
      final queryParams = {
        'pageNumber': page,
        'pageSize': pageSize,
        'status': 1, // PendingApproval
      };
      final response = await _dio.get('/Content', queryParameters: queryParams);
      final data = response.data['data'];
      if (data == null || data['data'] == null) return [];
      
      final items = data['data'] as List;
      return items.map((e) => ContentResponseModel.fromJson(e)).toList();
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> approveContent(String id) async {
    try {
      final response = await _dio.put('/Content/$id', data: {
        'status': 2, // Approved
      });
      return ContentResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> rejectContent(String id) async {
    try {
      final response = await _dio.put('/Content/$id', data: {
        'status': 3, // Rejected
      });
      return ContentResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> undoContent(String id) async {
    try {
      final response = await _dio.put('/Content/$id', data: {
        'status': 1, // PendingApproval
      });
      return ContentResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<List<ContentResponseModel>> getHistoryApprovals({int page = 1, int pageSize = 100}) async {
    try {
      final queryParams = {
        'pageNumber': page,
        'pageSize': pageSize,
        // Assuming the API accepts multiple statuses, or we can fetch without status and filter client-side, 
        // or the API supports array: status=2&status=3. If not, just sort by updatedAt desc.
        // I will use sortDesc=true and sortBy=updatedAt as this is a generic /Content API.
        'sortBy': 'updatedAt',
        'sortDesc': true,
      };
      final response = await _dio.get('/Content', queryParameters: queryParams);
      final data = response.data['data'];
      if (data == null || data['data'] == null) return [];
      
      final items = data['data'] as List;
      // Filter client side to only Approved(2) and Rejected(3) just in case
      final parsedItems = items.map((e) => ContentResponseModel.fromJson(e)).toList();
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
