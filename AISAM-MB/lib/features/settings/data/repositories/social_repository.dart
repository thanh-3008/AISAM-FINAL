import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../models/social_integration_model.dart';
import '../models/available_target_model.dart';
part 'social_repository.g.dart';

List<SocialIntegrationModel> _parseSocialIntegrationList(List<dynamic> items) {
  return items.map((json) => SocialIntegrationModel.fromJson(json)).toList();
}

List<AvailableTargetModel> _parseAvailableTargetList(List<dynamic> items) {
  return items.map((json) => AvailableTargetModel.fromJson(json)).toList();
}

@riverpod
SocialRepository socialRepository(SocialRepositoryRef ref) {
  final dio = ref.read(dioProvider);
  return SocialRepository(dio);
}

class SocialRepository {
  final Dio _dio;

  SocialRepository(this._dio);

  Future<List<SocialIntegrationModel>> getIntegrationsByBrand(String brandId) async {
    final response = await _dio.get('/social/integrations/brand/$brandId');
    final data = response.data['data'];
    if (data == null) return [];
    
    final items = data as List;
    if (items.isEmpty) return [];
    return await compute(_parseSocialIntegrationList, items);
  }

  Future<void> deleteIntegration(String integrationId) async {
    await _dio.delete('/social/integrations/$integrationId');
  }

  Future<String> getAuthUrl(String platform) async {
    final response = await _dio.get('/social-auth/$platform');
    final data = response.data['data'];
    return data['authUrl'] as String;
  }

  Future<String> handleCallback(String platform, String code, String state) async {
    final response = await _dio.post('/social-auth/$platform/callback', data: {
      'code': code,
      'state': state,
    });
    
    return response.data['data']['id'] as String;
  }

  Future<List<AvailableTargetModel>> getAvailableTargets(String accountId) async {
    final response = await _dio.get('/social/accounts/$accountId/available-targets');
    final data = response.data['data'];
    if (data == null) return [];

    final items = data as List;
    if (items.isEmpty) return [];
    return await compute(_parseAvailableTargetList, items);
  }

  Future<void> linkTargets(String accountId, List<String> targetIds, String brandId, String provider) async {
    await _dio.post('/social/accounts/$accountId/link-targets', data: {
      'provider': provider,
      'providerTargetIds': targetIds,
      'brandId': brandId,
    });
  }
}
