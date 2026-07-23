import 'package:freezed_annotation/freezed_annotation.dart';

part 'workspace_request.freezed.dart';
part 'workspace_request.g.dart';

@freezed
class CreateWorkspaceRequest with _$CreateWorkspaceRequest {
  const factory CreateWorkspaceRequest({
    required String name,
    String? description,
    required int workspaceType,
  }) = _CreateWorkspaceRequest;

  factory CreateWorkspaceRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateWorkspaceRequestFromJson(json);
}

@freezed
class UpdateWorkspaceRequest with _$UpdateWorkspaceRequest {
  const factory UpdateWorkspaceRequest({
    required String name,
    String? description,
  }) = _UpdateWorkspaceRequest;

  factory UpdateWorkspaceRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateWorkspaceRequestFromJson(json);
}
