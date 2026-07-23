import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/workspace_repository.dart';
import '../../data/models/workspace_model.dart';
import 'workspace_controller.dart';
import '../../../../core/errors/app_exception.dart';

part 'workspace_member_controller.g.dart';

@riverpod
class WorkspaceMemberController extends _$WorkspaceMemberController {
  @override
  AsyncValue<List<WorkspaceMemberResponseModel>> build() {
    ref.watch(activeWorkspaceControllerProvider);
    _fetchMembers();
    return const AsyncValue.loading();
  }

  Future<void> _fetchMembers() async {
    try {
      state = const AsyncValue.loading();
      final repository = ref.read(workspaceRepositoryProvider);
      final members = await repository.getWorkspaceMembers();
      state = AsyncValue.data(members);
    } catch (e, st) {
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refreshMembers() async {
    await _fetchMembers();
  }
}
