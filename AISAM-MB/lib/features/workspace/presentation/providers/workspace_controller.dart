import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/storage/secure_storage.dart';
import '../../data/repositories/workspace_repository.dart';
import '../../data/models/workspace_model.dart';
import '../../data/models/workspace_request.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';

part 'workspace_controller.g.dart';

@riverpod
class WorkspaceController extends _$WorkspaceController {
  @override
  AsyncValue<List<WorkspaceResponseModel>> build() {
    _fetchWorkspaces();
    return const AsyncValue.loading();
  }

  Future<void> _fetchWorkspaces() async {
    try {
      state = const AsyncValue.loading();
      final repository = ref.read(workspaceRepositoryProvider);
      final workspaces = await repository.getWorkspaces();
      state = AsyncValue.data(workspaces);
    } catch (e, st) {
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refreshWorkspaces() async {
    await _fetchWorkspaces();
  }

  Future<bool> selectWorkspace(String workspaceId) async {
    try {
      final storage = ref.read(secureStorageProvider);
      await storage.saveActiveWorkspaceId(workspaceId);
      ref.read(activeWorkspaceControllerProvider.notifier).refresh();
      return true;
    } catch (e) {
      return false;
    }
  }
}

@riverpod
class CreateWorkspaceController extends _$CreateWorkspaceController {
  @override
  BaseState<WorkspaceResponseModel> build() {
    return const BaseState.initial();
  }

  Future<void> createWorkspace(CreateWorkspaceRequest request) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(workspaceRepositoryProvider);
      final workspace = await repository.createWorkspace(request);
      state = BaseState.data(workspace);
      // Automatically refresh the list of workspaces
      ref.read(workspaceControllerProvider.notifier).refreshWorkspaces();
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }
}

@riverpod
class ActiveWorkspaceController extends _$ActiveWorkspaceController {
  @override
  Future<WorkspaceResponseModel?> build() async {
    return _fetchActiveWorkspace();
  }

  Future<WorkspaceResponseModel?> _fetchActiveWorkspace() async {
    final storage = ref.read(secureStorageProvider);
    final activeId = await storage.getActiveWorkspaceId();
    if (activeId == null) return null;
    
    // Attempt to find it in the cached workspace list if available
    final workspaces = ref.read(workspaceControllerProvider).valueOrNull;
    if (workspaces != null) {
      try {
        return workspaces.firstWhere((w) => w.id == activeId);
      } catch (_) {}
    }
    
    // Otherwise fetch from repo
    try {
      final repository = ref.read(workspaceRepositoryProvider);
      return await repository.getWorkspaceById(activeId);
    } catch (e) {
      return null;
    }
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchActiveWorkspace());
  }

  Future<void> clear() async {
    state = const AsyncValue.data(null);
  }
}
