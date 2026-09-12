import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/api_client.dart';
import '../../../workspace/presentation/providers/workspace_controller.dart';
import '../../data/repositories/publishing_repository.dart';

final contentPermissionsProvider = FutureProvider.autoDispose
    .family<List<bool>, String>((ref, id) async {
      final workspace = ref.watch(activeWorkspaceControllerProvider);
      if (workspace.isLoading ||
          workspace.hasError ||
          workspace.valueOrNull == null) {
        return [false, false, false, false];
      }
      final role = workspace.valueOrNull?.currentUserRole;
      // Owner (1) or Manager (2) always has full content & review permissions
      if (role == 1 || role == 2) {
        return [true, true, true, true];
      }
      try {
        return await PublishingRepository(
          ref.watch(dioProvider),
        ).contentPermissions(id);
      } catch (_) {
        return [false, false, false, false];
      }
    });
