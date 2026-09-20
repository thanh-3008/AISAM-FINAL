import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/features/workspace/presentation/workspace_list_screen.dart';
import 'package:aisam_mb/features/workspace/presentation/providers/workspace_controller.dart';

void main() {
  group('Workspace Module Tests', () {
    test('getWorkspaceRoleName prioritizes workspaceRole V2 when present', () {
      expect(getWorkspaceRoleName(2, 'WorkspaceManager'), equals('Workspace Manager'));
      expect(getWorkspaceRoleName(1, 'Owner'), equals('Owner'));
      expect(getWorkspaceRoleName(0, 'Member'), equals('Member'));
      expect(getWorkspaceRoleName(99, 'CustomRole'), equals('CustomRole'));
    });

    test('getWorkspaceRoleName falls back to legacy roleInt when workspaceRole is null or empty', () {
      expect(getWorkspaceRoleName(1, null), equals('Owner'));
      expect(getWorkspaceRoleName(2, null), equals('Manager'));
      expect(getWorkspaceRoleName(3, null), equals('Content Creator'));
      expect(getWorkspaceRoleName(4, null), equals('Viewer'));
      expect(getWorkspaceRoleName(0, null), equals('Member'));

      expect(getWorkspaceRoleName(1, ''), equals('Owner'));
      expect(getWorkspaceRoleName(2, ''), equals('Manager'));
    });

    test('ActiveWorkspaceController clear resets state to null', () async {
      final container = ProviderContainer();
      addTearDown(container.dispose);

      final notifier = container.read(activeWorkspaceControllerProvider.notifier);
      await notifier.clear();

      final active = container.read(activeWorkspaceControllerProvider);
      expect(active.value, isNull);
    });
  });
}
