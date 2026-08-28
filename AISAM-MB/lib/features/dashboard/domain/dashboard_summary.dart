import 'package:freezed_annotation/freezed_annotation.dart';

part 'dashboard_summary.freezed.dart';
part 'dashboard_summary.g.dart';

@freezed
class CombinedDashboardSummary with _$CombinedDashboardSummary {
  const factory CombinedDashboardSummary({
    // Basic Fields (Always available)
    @Default(0) int draftContentCount,
    @Default(0) int publishedContentCount,
    @Default(0) int pendingApprovalContentCount,
    @Default(0) int upcomingScheduleCount,
    @Default(0) int failedScheduleCount,
    @Default(0) int activeSocialIntegrationCount,
    @Default(0) int publishedPostCount,
    @Default(0) int unreadNotificationCount,

    // Advanced Fields (Nullable, available only for Paid plans)
    String? workspaceId,
    int? creditBalance,
    int? creditsUsed,
    int? postQuotaLimit,
    int? postsRemaining,
    int? aiUsageCount,
    int? activeMemberCount,
    List<WorkspaceTopMemberDto>? topMembers,
  }) = _CombinedDashboardSummary;

  factory CombinedDashboardSummary.fromJson(Map<String, dynamic> json) =>
      _$CombinedDashboardSummaryFromJson(json);
}

@freezed
class WorkspaceTopMemberDto with _$WorkspaceTopMemberDto {
  const factory WorkspaceTopMemberDto({
    required String userId,
    @Default('') String name,
    @Default('') String email,
    @Default(0) int creditsUsed,
    @Default(0) int aiUsageCount,
  }) = _WorkspaceTopMemberDto;

  factory WorkspaceTopMemberDto.fromJson(Map<String, dynamic> json) =>
      _$WorkspaceTopMemberDtoFromJson(json);
}
