import '../../../../core/network/access_events.dart';
import '../../../access/presentation/access_providers.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/workspace_repository.dart';
import '../../data/models/workspace_model.dart';
import '../../../../core/errors/app_exception.dart';

part 'workspace_member_controller.g.dart';

@riverpod
class WorkspaceMemberController extends _$WorkspaceMemberController {
  int _generation = 0;

  @override
  AsyncValue<List<WorkspaceMemberResponseModel>> build() {
    ++_generation;
    ref.onDispose(() => ++_generation);
    ref.watch(accessContextProvider);
    if (ref.watch(accessDeniedProvider)) {
      return AsyncValue.error(StateError('Access denied'), StackTrace.current);
    }
    _fetchMembers();
    return const AsyncValue.loading();
  }

  Future<void> _fetchMembers() async {
    final generation = ++_generation;
    try {
      await ref.read(accessContextProvider.future);
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      final repository = ref.read(workspaceRepositoryProvider);
      final members = await repository.getWorkspaceMembers();
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.data(members);
    } catch (e, st) {
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refreshMembers() async {
    await _fetchMembers();
  }
}
