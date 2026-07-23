import 'package:freezed_annotation/freezed_annotation.dart';

part 'workspace_model.freezed.dart';
part 'workspace_model.g.dart';

@freezed
class WorkspaceResponseModel with _$WorkspaceResponseModel {
  const factory WorkspaceResponseModel({
    required String id,
    required String name,
    String? description,
    String? logoUrl,
    required int workspaceType, // enum from backend
    required int status, // enum
    required int currentUserRole, // enum
    required DateTime createdAt,
    required DateTime updatedAt,
  }) = _WorkspaceResponseModel;

  factory WorkspaceResponseModel.fromJson(Map<String, dynamic> json) =>
      _$WorkspaceResponseModelFromJson(json);
}

@freezed
class WorkspaceMemberResponseModel with _$WorkspaceMemberResponseModel {
  const factory WorkspaceMemberResponseModel({
    required String id,
    required String userId,
    required String email,
    String? fullName,
    required int role, // enum
    required DateTime joinedAt,
  }) = _WorkspaceMemberResponseModel;

  factory WorkspaceMemberResponseModel.fromJson(Map<String, dynamic> json) =>
      _$WorkspaceMemberResponseModelFromJson(json);
}
