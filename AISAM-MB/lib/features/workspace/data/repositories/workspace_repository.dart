import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/workspace_model.dart';
import '../models/workspace_request.dart';
part 'workspace_repository.g.dart';

List<WorkspaceResponseModel> _parseWorkspaceList(List<dynamic> items) {
  return items.map((e) => WorkspaceResponseModel.fromJson(e)).toList();
}

List<WorkspaceMemberResponseModel> _parseWorkspaceMemberList(List<dynamic> items) {
  return items.map((e) => WorkspaceMemberResponseModel.fromJson(e)).toList();
}

class WorkspaceRepository {
  final Dio _dio;

  WorkspaceRepository(this._dio);

  Future<List<WorkspaceResponseModel>> getWorkspaces() async {
    try {
      final response = await _dio.get('/Workspaces');
      final data = response.data['data'] as List?;
      if (data == null || data.isEmpty) return [];
      return await compute(_parseWorkspaceList, data);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<WorkspaceResponseModel> getWorkspaceById(String id) async {
    try {
      final response = await _dio.get('/Workspaces/$id');
      return WorkspaceResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<WorkspaceResponseModel> createWorkspace(CreateWorkspaceRequest request) async {
    try {
      final response = await _dio.post('/Workspaces', data: request.toJson());
      return WorkspaceResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<WorkspaceResponseModel> updateWorkspace(String id, UpdateWorkspaceRequest request) async {
    try {
      final response = await _dio.put('/Workspaces/$id', data: request.toJson());
      return WorkspaceResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> deleteWorkspace(String id) async {
    try {
      await _dio.delete('/Workspaces/$id');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<List<WorkspaceMemberResponseModel>> getWorkspaceMembers() async {
    try {
      final response = await _dio.get('/workspace-members');
      final data = response.data['data'] as List?;
      if (data == null || data.isEmpty) return [];
      return await compute(_parseWorkspaceMemberList, data);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
WorkspaceRepository workspaceRepository(WorkspaceRepositoryRef ref) {
  return WorkspaceRepository(ref.read(dioProvider));
}
