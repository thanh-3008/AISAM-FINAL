// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'workspace_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CreateWorkspaceRequestImpl _$$CreateWorkspaceRequestImplFromJson(
  Map<String, dynamic> json,
) => _$CreateWorkspaceRequestImpl(
  name: json['name'] as String,
  description: json['description'] as String?,
  workspaceType: (json['workspaceType'] as num).toInt(),
);

Map<String, dynamic> _$$CreateWorkspaceRequestImplToJson(
  _$CreateWorkspaceRequestImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
  'workspaceType': instance.workspaceType,
};

_$UpdateWorkspaceRequestImpl _$$UpdateWorkspaceRequestImplFromJson(
  Map<String, dynamic> json,
) => _$UpdateWorkspaceRequestImpl(
  name: json['name'] as String,
  description: json['description'] as String?,
);

Map<String, dynamic> _$$UpdateWorkspaceRequestImplToJson(
  _$UpdateWorkspaceRequestImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
};
