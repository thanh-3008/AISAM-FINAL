import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/conversation_model.dart';
import '../models/conversation_detail_model.dart';

import '../models/chat_request.dart';
import '../models/chat_response.dart';

part 'chat_repository.g.dart';

List<ConversationModel> _parseConversationList(List<dynamic> items) {
  return items.map((e) => ConversationModel.fromJson(e)).toList();
}

class ChatRepository {
  final Dio _dio;

  ChatRepository(this._dio);

  Future<List<ConversationModel>> getConversations({
    int page = 1,
    int pageSize = 20,
    String? searchTerm,
  }) async {
    try {
      final queryParams = {
        'page': page,
        'pageSize': pageSize,
        if (searchTerm != null) 'searchTerm': searchTerm,
      };
      final response = await _dio.get('/conversations', queryParameters: queryParams);
      // Expected: GenericResponse<PagedResult<ConversationResponseDto>>
      final items = response.data['data']['items'] as List;
      if (items.isEmpty) return [];
      return await compute(_parseConversationList, items);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ConversationDetailModel> getConversationById(String id) async {
    try {
      final response = await _dio.get('/conversations/$id');
      return ConversationDetailModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> deleteConversation(String id) async {
    try {
      await _dio.delete('/conversations/$id');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ChatResponse> sendMessage(ChatRequest request) async {
    try {
      final response = await _dio.post('/ai/chat', data: request.toJson());
      return ChatResponse.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
ChatRepository chatRepository(ChatRepositoryRef ref) {
  return ChatRepository(ref.read(dioProvider));
}
