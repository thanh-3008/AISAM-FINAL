import 'package:freezed_annotation/freezed_annotation.dart';

part 'dashboard_summary.freezed.dart';
part 'dashboard_summary.g.dart';

@freezed
class WorkspaceDashboardSummaryDto with _$WorkspaceDashboardSummaryDto {
  const factory WorkspaceDashboardSummaryDto({
    required String workspaceId,
    @Default(0) int creditBalance,
    @Default(0) int creditsUsed,
    @Default(0) int publishedPostCount,
    @Default(0) int postQuotaLimit,
    @Default(0) int postsRemaining,
    @Default(0) int aiUsageCount,
    @Default(0) int activeMemberCount,
    @Default([]) List<WorkspaceTopMemberDto> topMembers,
  }) = _WorkspaceDashboardSummaryDto;

  factory WorkspaceDashboardSummaryDto.fromJson(Map<String, dynamic> json) =>
      _$WorkspaceDashboardSummaryDtoFromJson(json);
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
