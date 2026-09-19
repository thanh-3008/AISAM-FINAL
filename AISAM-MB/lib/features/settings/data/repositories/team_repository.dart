import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/team_model.dart';

class TeamRepository {
  final Dio _dio;

  TeamRepository(this._dio);

  Future<List<TeamSummaryModel>> getTeams({int page = 1, int pageSize = 50}) async {
    try {
      final response = await _dio.get(
        '/teams/manage',
        queryParameters: {'page': page, 'pageSize': pageSize},
      );

      final rawData = response.data?['data'];
      List items = [];
      if (rawData is Map) {
        items = (rawData['data'] ?? rawData['items']) as List? ?? [];
      } else if (rawData is List) {
        items = rawData;
      }
      return items
          .map((e) => TeamSummaryModel.fromJson(e as Map<String, dynamic>))
          .toList();
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<TeamDetailModel> createTeam({
    required String name,
    String? description,
    List<Map<String, dynamic>>? members,
  }) async {
    try {
      final response = await _dio.post(
        '/teams',
        data: {
          'name': name.trim(),
          'description': description?.trim().isEmpty == true ? null : description?.trim(),
          'members': members ?? [],
        },
      );
      final data = response.data['data'] as Map<String, dynamic>;
      return TeamDetailModel.fromJson(data);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<TeamDetailModel> getTeamById(String teamId) async {
    try {
      final response = await _dio.get('/teams/$teamId');
      final data = response.data['data'] as Map<String, dynamic>;
      return TeamDetailModel.fromJson(data);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<CreditWalletModel> getWorkspaceWallet() async {
    try {
      final response = await _dio.get('/credit-usage/wallet');
      final data = response.data['data'] as Map<String, dynamic>;
      return CreditWalletModel.fromJson(data);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> addTeamMember({
    required String teamId,
    required String userId,
    String role = 'ContentCreator',
  }) async {
    try {
      await _dio.post(
        '/teams/$teamId/members',
        data: {
          'userId': userId,
          'role': role,
        },
      );
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> removeTeamMember({
    required String teamId,
    required String userId,
  }) async {
    try {
      await _dio.delete('/teams/$teamId/members/$userId');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> inviteWorkspaceMember({
    required String email,
    int role = 3,
    int quotaMode = 1,
    int? creditLimit,
  }) async {
    try {
      await _dio.post(
        '/workspace-invitations',
        data: {
          'email': email.trim(),
          'role': role,
          'quotaMode': quotaMode,
          if (creditLimit != null && creditLimit > 0) 'creditLimit': creditLimit,
        },
      );
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

final teamRepositoryProvider = Provider<TeamRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return TeamRepository(dio);
});
