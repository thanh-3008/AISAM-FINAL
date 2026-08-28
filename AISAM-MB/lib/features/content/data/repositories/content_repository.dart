import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/content_model.dart';
import '../models/content_request.dart';
part 'content_repository.g.dart';

List<ContentResponseModel> _parseContentList(List<dynamic> items) {
  return items.map((e) => ContentResponseModel.fromJson(e)).toList();
}

class ContentRepository {
  final Dio _dio;

  ContentRepository(this._dio);

  Future<List<ContentResponseModel>> getContents({int pageNumber = 1, int pageSize = 10, String? search, String? status}) async {
    try {
      final queryParams = {
        'page': pageNumber,
        'pageSize': pageSize,
        if (search != null) 'search': search,
        if (status != null) 'status': status,
      };
      final response = await _dio.get('/Content', queryParameters: queryParams);
      final data = response.data['data'];
      final items = data != null ? data['data'] as List? : null;
      if (items == null || items.isEmpty) return [];
      return await compute(_parseContentList, items);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> getContentById(String id) async {
    try {
      final response = await _dio.get('/Content/$id');
      return ContentResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> createContent(CreateContentRequest request) async {
    try {
      final response = await _dio.post('/Content', data: request.toJson());
      return ContentResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> updateContent(String id, UpdateContentRequest request) async {
    try {
      final response = await _dio.put('/Content/$id', data: request.toJson());
      return ContentResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> deleteContent(String id) async {
    try {
      await _dio.delete('/Content/$id');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> restoreContent(String id) async {
    try {
      await _dio.post('/Content/$id/restore');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentResponseModel> cloneContent(String id) async {
    try {
      final response = await _dio.post('/Content/$id/clone');
      return ContentResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
ContentRepository contentRepository(ContentRepositoryRef ref) {
  return ContentRepository(ref.read(dioProvider));
}
