// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'dashboard_summary.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$WorkspaceDashboardSummaryDtoImpl _$$WorkspaceDashboardSummaryDtoImplFromJson(
  Map<String, dynamic> json,
) => _$WorkspaceDashboardSummaryDtoImpl(
  workspaceId: json['workspaceId'] as String,
  creditBalance: (json['creditBalance'] as num?)?.toInt() ?? 0,
  creditsUsed: (json['creditsUsed'] as num?)?.toInt() ?? 0,
  publishedPostCount: (json['publishedPostCount'] as num?)?.toInt() ?? 0,
  postQuotaLimit: (json['postQuotaLimit'] as num?)?.toInt() ?? 0,
  postsRemaining: (json['postsRemaining'] as num?)?.toInt() ?? 0,
  aiUsageCount: (json['aiUsageCount'] as num?)?.toInt() ?? 0,
  activeMemberCount: (json['activeMemberCount'] as num?)?.toInt() ?? 0,
  topMembers:
      (json['topMembers'] as List<dynamic>?)
          ?.map(
            (e) => WorkspaceTopMemberDto.fromJson(e as Map<String, dynamic>),
          )
          .toList() ??
      const [],
);

Map<String, dynamic> _$$WorkspaceDashboardSummaryDtoImplToJson(
  _$WorkspaceDashboardSummaryDtoImpl instance,
) => <String, dynamic>{
  'workspaceId': instance.workspaceId,
  'creditBalance': instance.creditBalance,
  'creditsUsed': instance.creditsUsed,
  'publishedPostCount': instance.publishedPostCount,
  'postQuotaLimit': instance.postQuotaLimit,
  'postsRemaining': instance.postsRemaining,
  'aiUsageCount': instance.aiUsageCount,
  'activeMemberCount': instance.activeMemberCount,
  'topMembers': instance.topMembers,
};

_$WorkspaceTopMemberDtoImpl _$$WorkspaceTopMemberDtoImplFromJson(
  Map<String, dynamic> json,
) => _$WorkspaceTopMemberDtoImpl(
  userId: json['userId'] as String,
  name: json['name'] as String? ?? '',
  email: json['email'] as String? ?? '',
  creditsUsed: (json['creditsUsed'] as num?)?.toInt() ?? 0,
  aiUsageCount: (json['aiUsageCount'] as num?)?.toInt() ?? 0,
);

Map<String, dynamic> _$$WorkspaceTopMemberDtoImplToJson(
  _$WorkspaceTopMemberDtoImpl instance,
) => <String, dynamic>{
  'userId': instance.userId,
  'name': instance.name,
  'email': instance.email,
  'creditsUsed': instance.creditsUsed,
  'aiUsageCount': instance.aiUsageCount,
};
