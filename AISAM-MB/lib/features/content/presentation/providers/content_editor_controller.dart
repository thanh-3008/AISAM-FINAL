import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/content_repository.dart';
import '../../data/models/content_model.dart';
import '../../data/models/content_request.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';
import 'content_list_controller.dart';
import '../../../workspace/presentation/providers/workspace_controller.dart';

part 'content_editor_controller.g.dart';

@riverpod
class ContentEditorController extends _$ContentEditorController {
  @override
  BaseState<ContentResponseModel> build() {
    return const BaseState.initial();
  }

  Future<void> createContent(CreateContentRequest request) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(contentRepositoryProvider);
      final content = await repository.createContent(request);
      state = BaseState.data(content);
      ref.read(contentListControllerProvider.notifier).refresh();
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }

  Future<void> updateContent(String id, UpdateContentRequest request) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(contentRepositoryProvider);
      final content = await repository.updateContent(id, request);
      state = BaseState.data(content);
      ref.read(contentListControllerProvider.notifier).refresh();
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }
}

@riverpod
class ContentDetailController extends _$ContentDetailController {
  int _generation = 0;
  @override
  AsyncValue<ContentResponseModel> build(String id) {
    ref.watch(activeWorkspaceControllerProvider);
    _generation++;
    ref.onDispose(() => _generation++);
    _fetchDetail(id);
    return const AsyncValue.loading();
  }

  Future<void> _fetchDetail(String id) async {
    final generation = _generation;
    try {
      state = const AsyncValue.loading();
      final repository = ref.read(contentRepositoryProvider);
      final content = await repository.getContentById(id);
      if(generation != _generation) return;
      state = AsyncValue.data(content);
    } catch (e, st) {
      if(generation != _generation) return;
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> deleteContent(String id) async {
    try {
      final repository = ref.read(contentRepositoryProvider);
      await repository.deleteContent(id);
      ref.read(contentListControllerProvider.notifier).refresh();
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}
