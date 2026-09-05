// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'access_context.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$AccessContextImpl _$$AccessContextImplFromJson(Map<String, dynamic> json) =>
    _$AccessContextImpl(
      workspaceId: json['workspaceId'] as String,
      userId: json['userId'] as String,
      role: json['role'] as String,
      version: json['version'] as String,
      teamIds:
          (json['teamIds'] as List<dynamic>?)
              ?.map((e) => e as String)
              .toList() ??
          const [],
      canViewAnalytics: json['canViewAnalytics'] as bool? ?? false,
      canViewOwnAnalytics: json['canViewOwnAnalytics'] as bool? ?? false,
      canManageTeams: json['canManageTeams'] as bool? ?? false,
      canManageTasks: json['canManageTasks'] as bool? ?? false,
      canCreateContent: json['canCreateContent'] as bool? ?? false,
      canReviewContent: json['canReviewContent'] as bool? ?? false,
      canPublish: json['canPublish'] as bool? ?? false,
    );

Map<String, dynamic> _$$AccessContextImplToJson(_$AccessContextImpl instance) =>
    <String, dynamic>{
      'workspaceId': instance.workspaceId,
      'userId': instance.userId,
      'role': instance.role,
      'version': instance.version,
      'teamIds': instance.teamIds,
      'canViewAnalytics': instance.canViewAnalytics,
      'canViewOwnAnalytics': instance.canViewOwnAnalytics,
      'canManageTeams': instance.canManageTeams,
      'canManageTasks': instance.canManageTasks,
      'canCreateContent': instance.canCreateContent,
      'canReviewContent': instance.canReviewContent,
      'canPublish': instance.canPublish,
    };
