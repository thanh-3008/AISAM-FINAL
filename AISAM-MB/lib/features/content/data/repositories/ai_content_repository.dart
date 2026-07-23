import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/ai_generation_request.dart';
import '../models/ai_generation_response.dart';

part 'ai_content_repository.g.dart';

class AiContentRepository {
  final Dio _dio;

  AiContentRepository(this._dio);

  Future<AiGenerationResponseModel> generateDraft(CreateDraftRequest request) async {
    try {
      final response = await _dio.post('/Gemini/generate-draft', data: request.toJson());
      return AiGenerationResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<AiGenerationResponseModel> improveContent(String contentId, ImproveContentRequest request) async {
    try {
      final response = await _dio.post('/Gemini/improve/$contentId', data: request.toJson());
      return AiGenerationResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> approveGeneration(String aiGenerationId) async {
    try {
      await _dio.post('/Gemini/approve/$aiGenerationId');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
AiContentRepository aiContentRepository(AiContentRepositoryRef ref) {
  return AiContentRepository(ref.read(dioProvider));
}
