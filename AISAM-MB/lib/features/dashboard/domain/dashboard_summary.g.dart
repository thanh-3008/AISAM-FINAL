// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'dashboard_summary.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CombinedDashboardSummaryImpl _$$CombinedDashboardSummaryImplFromJson(
  Map<String, dynamic> json,
) => _$CombinedDashboardSummaryImpl(
  draftContentCount: (json['draftContentCount'] as num?)?.toInt() ?? 0,
  publishedContentCount: (json['publishedContentCount'] as num?)?.toInt() ?? 0,
  pendingApprovalContentCount:
      (json['pendingApprovalContentCount'] as num?)?.toInt() ?? 0,
  upcomingScheduleCount: (json['upcomingScheduleCount'] as num?)?.toInt() ?? 0,
  failedScheduleCount: (json['failedScheduleCount'] as num?)?.toInt() ?? 0,
  activeSocialIntegrationCount:
      (json['activeSocialIntegrationCount'] as num?)?.toInt() ?? 0,
  publishedPostCount: (json['publishedPostCount'] as num?)?.toInt() ?? 0,
  unreadNotificationCount:
      (json['unreadNotificationCount'] as num?)?.toInt() ?? 0,
  workspaceId: json['workspaceId'] as String?,
  creditBalance: (json['creditBalance'] as num?)?.toInt(),
  creditsUsed: (json['creditsUsed'] as num?)?.toInt(),
  postQuotaLimit: (json['postQuotaLimit'] as num?)?.toInt(),
  postsRemaining: (json['postsRemaining'] as num?)?.toInt(),
  aiUsageCount: (json['aiUsageCount'] as num?)?.toInt(),
  activeMemberCount: (json['activeMemberCount'] as num?)?.toInt(),
  topMembers: (json['topMembers'] as List<dynamic>?)
      ?.map((e) => WorkspaceTopMemberDto.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$$CombinedDashboardSummaryImplToJson(
  _$CombinedDashboardSummaryImpl instance,
) => <String, dynamic>{
  'draftContentCount': instance.draftContentCount,
  'publishedContentCount': instance.publishedContentCount,
  'pendingApprovalContentCount': instance.pendingApprovalContentCount,
  'upcomingScheduleCount': instance.upcomingScheduleCount,
  'failedScheduleCount': instance.failedScheduleCount,
  'activeSocialIntegrationCount': instance.activeSocialIntegrationCount,
  'publishedPostCount': instance.publishedPostCount,
  'unreadNotificationCount': instance.unreadNotificationCount,
  'workspaceId': instance.workspaceId,
  'creditBalance': instance.creditBalance,
  'creditsUsed': instance.creditsUsed,
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
