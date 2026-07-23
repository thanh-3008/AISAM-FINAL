import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../core/errors/generic_response.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_endpoints.dart';
import '../domain/dashboard_summary.dart';
import '../../content/data/models/content_model.dart';

part 'dashboard_repository.g.dart';

class DashboardRepository {
  final Dio _dio;

  DashboardRepository(this._dio);

  Future<GenericResponse<WorkspaceDashboardSummaryDto>> getSummary() async {
    final response = await _dio.get(ApiEndpoints.workspaceDashboardSummary);
    return GenericResponse.fromJson(
      response.data,
      (json) => WorkspaceDashboardSummaryDto.fromJson(json as Map<String, dynamic>),
    );
  }

  Future<List<ContentResponseModel>> getRecentActivities() async {
    try {
      final queryParams = {
        'page': 1,
        'pageSize': 5,
        'sortBy': 'createdAt',
        'sortDesc': true,
      };
      final response = await _dio.get('/Content', queryParameters: queryParams);
      final items = response.data['data']['items'] as List;
      return items.map((e) => ContentResponseModel.fromJson(e)).toList();
    } catch (e) {
      return [];
    }
  }
}

@riverpod
DashboardRepository dashboardRepository(DashboardRepositoryRef ref) {
  final dio = ref.watch(dioProvider);
  return DashboardRepository(dio);
}
