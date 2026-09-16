import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'dart:async';
import 'api_client.dart';
import '../../features/workspace/presentation/providers/workspace_controller.dart';

enum WorkspaceRoleV2 { owner, workspaceManager, member }

enum TeamRoleV2 { manager, contentCreator, viewer }

class TeamScope {
  final String teamId, brandId;
  final TeamRoleV2? role;
  final List<String> channelIds;
  TeamScope(this.teamId, this.brandId, this.role, this.channelIds);
}

class RbacContext {
  final String revision;
  final WorkspaceRoleV2? workspaceRole;
  final List<TeamScope> scopes;
  final List<String> actions;
  final bool isV2;
  RbacContext(
    this.revision,
    this.workspaceRole,
    this.scopes,
    this.actions,
    this.isV2,
  );
  factory RbacContext.fromJson(Map<String, dynamic> json) {
    final revision = json['revision'];
    if (revision is! String || revision.isEmpty) {
      throw const FormatException('Thiếu phiên bản quyền.');
    }
    if (!json.containsKey('contractVersion') &&
        !json.containsKey('workspaceRole')) {
      return RbacContext(revision, null, [], [], false);
    }
    final role = {
      'Owner': WorkspaceRoleV2.owner,
      'WorkspaceManager': WorkspaceRoleV2.workspaceManager,
      'Member': WorkspaceRoleV2.member,
    }[json['workspaceRole']];
    if (json['contractVersion'] != 2 ||
        role == null ||
        json['scopes'] is! List ||
        json['actions'] is! List) {
      throw const FormatException('Phiên bản phân quyền chưa được hỗ trợ.');
    }
    final scopes = (json['scopes'] as List).map((s) {
      if (s is! Map ||
          s['teamId'] is! String ||
          s['brandId'] is! String ||
          s['channelIds'] is! List) {
        throw const FormatException('Phạm vi Team không hợp lệ.');
      }
      final teamRole = {
        'Manager': TeamRoleV2.manager,
        'ContentCreator': TeamRoleV2.contentCreator,
        'Viewer': TeamRoleV2.viewer,
      }[s['role']];
      if (s['role'] != null && teamRole == null) {
        throw const FormatException('Vai trò Team không hợp lệ.');
      }
      return TeamScope(
        s['teamId'],
        s['brandId'],
        teamRole,
        List<String>.from(s['channelIds']),
      );
    }).toList();
    return RbacContext(
      revision,
      role,
      scopes,
      List<String>.from(json['actions']),
      true,
    );
  }
  bool canCreate(TeamScope scope) =>
      isV2 &&
      scopes.contains(scope) &&
      scope.teamId.isNotEmpty &&
      scope.brandId.isNotEmpty &&
      scope.teamId != '00000000-0000-0000-0000-000000000000' &&
      (workspaceRole == WorkspaceRoleV2.owner ||
          workspaceRole == WorkspaceRoleV2.workspaceManager ||
          (workspaceRole == WorkspaceRoleV2.member &&
              (scope.role == TeamRoleV2.manager ||
                  scope.role == TeamRoleV2.contentCreator)));
}

final rbacContextProvider = FutureProvider.autoDispose<RbacContext>((
  ref,
) async {
  final workspace = await ref.watch(activeWorkspaceControllerProvider.future);
  if (workspace == null) throw StateError('Chọn workspace trước.');
  final timer = Timer(const Duration(seconds: 60), ref.invalidateSelf);
  ref.onDispose(timer.cancel);
  final response = await ref.watch(dioProvider).get('/permissions/context');
  return RbacContext.fromJson(Map<String, dynamic>.from(response.data['data']));
});
