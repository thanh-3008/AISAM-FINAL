import 'package:freezed_annotation/freezed_annotation.dart';

part 'access_context.freezed.dart';
part 'access_context.g.dart';

@freezed
class AccessContext with _$AccessContext {
  const factory AccessContext({
    required String workspaceId,
    required String userId,
    required String role,
    required String version,
    @Default([]) List<String> teamIds,
    @Default(false) bool canViewAnalytics,
    @Default(false) bool canViewOwnAnalytics,
    @Default(false) bool canManageTeams,
    @Default(false) bool canManageTasks,
    @Default(false) bool canCreateContent,
    @Default(false) bool canReviewContent,
    @Default(false) bool canPublish,
  }) = _AccessContext;

  factory AccessContext.fromJson(Map<String, dynamic> json) => _$AccessContextFromJson(json);
}
