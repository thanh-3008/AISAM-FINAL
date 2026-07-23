// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'workspace_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$WorkspaceResponseModelImpl _$$WorkspaceResponseModelImplFromJson(
  Map<String, dynamic> json,
) => _$WorkspaceResponseModelImpl(
  id: json['id'] as String,
  name: json['name'] as String,
  description: json['description'] as String?,
  logoUrl: json['logoUrl'] as String?,
  workspaceType: (json['workspaceType'] as num).toInt(),
  status: (json['status'] as num).toInt(),
  currentUserRole: (json['currentUserRole'] as num).toInt(),
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt: DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$$WorkspaceResponseModelImplToJson(
  _$WorkspaceResponseModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'name': instance.name,
  'description': instance.description,
  'logoUrl': instance.logoUrl,
  'workspaceType': instance.workspaceType,
  'status': instance.status,
  'currentUserRole': instance.currentUserRole,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt.toIso8601String(),
};

_$WorkspaceMemberResponseModelImpl _$$WorkspaceMemberResponseModelImplFromJson(
  Map<String, dynamic> json,
) => _$WorkspaceMemberResponseModelImpl(
  id: json['id'] as String,
  userId: json['userId'] as String,
  email: json['email'] as String,
  fullName: json['fullName'] as String?,
  role: (json['role'] as num).toInt(),
  joinedAt: DateTime.parse(json['joinedAt'] as String),
);

Map<String, dynamic> _$$WorkspaceMemberResponseModelImplToJson(
  _$WorkspaceMemberResponseModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'userId': instance.userId,
  'email': instance.email,
  'fullName': instance.fullName,
  'role': instance.role,
  'joinedAt': instance.joinedAt.toIso8601String(),
};
