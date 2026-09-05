import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import 'access_context.dart';

final accessRepositoryProvider = Provider((ref) => AccessRepository(ref.watch(dioProvider)));

class AccessRepository {
  final Dio dio;
  AccessRepository(this.dio);

  Future<AccessContext> context(String workspaceId) async {
    final response = await dio.get('/access/context', options: Options(headers: {'X-Workspace-Id': workspaceId}));
    final result = AccessContext.fromJson(Map<String, dynamic>.from(response.data['data']));
    if (result.workspaceId != workspaceId) throw StateError('Workspace changed');
    return result;
  }

  Future<Map<String, bool>> actions(String contentId) async {
    final response = await dio.get('/access/content/$contentId/actions');
    return Map<String, bool>.from(response.data['data']);
  }

  Future<Map<String, dynamic>> ownAnalytics() async {
    final response = await dio.get('/access/me/analytics');
    return Map<String, dynamic>.from(response.data['data']);
  }
}
