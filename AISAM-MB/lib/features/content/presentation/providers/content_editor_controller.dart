import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/content_repository.dart';
import '../../data/models/content_model.dart';
import '../../data/models/content_request.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';
import 'content_list_controller.dart';

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
  @override
  AsyncValue<ContentResponseModel> build(String id) {
    _fetchDetail(id);
    return const AsyncValue.loading();
  }

  Future<void> _fetchDetail(String id) async {
    try {
      state = const AsyncValue.loading();
      final repository = ref.read(contentRepositoryProvider);
      final content = await repository.getContentById(id);
      state = AsyncValue.data(content);
    } catch (e, st) {
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
