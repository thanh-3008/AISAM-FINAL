import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../core/errors/generic_response.dart';
import '../../../core/network/api_client.dart';
import 'package:flutter/foundation.dart';
import '../../../core/errors/app_exception.dart';
import '../../../core/network/api_endpoints.dart';
import '../domain/dashboard_summary.dart';
import '../../content/data/models/content_model.dart';
part 'dashboard_repository.g.dart';

List<ContentResponseModel> _parseContentList(List<dynamic> items) {
  return items.map((e) => ContentResponseModel.fromJson(e)).toList();
}

class DashboardRepository {
  final Dio _dio;

  DashboardRepository(this._dio);

  Future<GenericResponse<CombinedDashboardSummary>> getSummary() async {
    try {
      // 1. Try fetching advanced workspace dashboard
      final response = await _dio.get(ApiEndpoints.workspaceDashboardSummary);
      return GenericResponse.fromJson(
        response.data,
        (json) => CombinedDashboardSummary.fromJson(json as Map<String, dynamic>),
      );
    } on DioException catch (e) {
      // 2. If 403 Forbidden (WORKSPACE_FEATURE_NOT_AVAILABLE), fallback to basic dashboard
      if (e.response?.statusCode == 403) {
        final errCode = e.response?.data?['error']?['errorCode'];
        if (errCode == 'WORKSPACE_FEATURE_NOT_AVAILABLE') {
          try {
            final basicResponse = await _dio.get('/dashboard/summary');
            return GenericResponse.fromJson(
              basicResponse.data,
              (json) => CombinedDashboardSummary.fromJson(json as Map<String, dynamic>),
            );
          } catch (innerError) {
            throw ExceptionHandler.handle(innerError);
          }
        }
      }
      throw ExceptionHandler.handle(e);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<List<ContentResponseModel>> getRecentActivities() async {
    try {
      final queryParams = {
        'page': 1,
        'pageSize': 5,
        'sortBy': 'createdAt',
        'sortDescending': true,
      };
      final response = await _dio.get('/Content', queryParameters: queryParams);
      final items = response.data['data']['items'] as List;
      if (items.isEmpty) return [];
      return await compute(_parseContentList, items);
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
