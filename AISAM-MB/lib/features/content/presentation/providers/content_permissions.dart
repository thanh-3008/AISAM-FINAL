import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/api_client.dart';
import '../../../workspace/presentation/providers/workspace_controller.dart';
import '../../data/repositories/publishing_repository.dart';
import '../../../../core/network/rbac_context.dart';

final contentPermissionsProvider = FutureProvider.autoDispose
    .family<List<bool>, String>((ref, id) async {
      final workspace = ref.watch(activeWorkspaceControllerProvider);
      if (workspace.isLoading ||
          workspace.hasError ||
          workspace.valueOrNull == null) {
        return [false, false, false, false];
      }
      try {
        // Re-check decisions after a permission revision changes. Context errors deny access.
        await ref.watch(rbacContextProvider.future);
        return await PublishingRepository(
          ref.watch(dioProvider),
        ).contentPermissions(id);
      } catch (_) {
        return [false, false, false, false];
      }
    });
