import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/media_repository.dart';
import '../../data/models/media_model.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';

part 'media_controller.g.dart';

@riverpod
class MediaUploadController extends _$MediaUploadController {
  @override
  BaseState<ContentMediaUploadResponseModel> build() {
    return const BaseState.initial();
  }

  Future<void> uploadMedia(String filePath) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(mediaRepositoryProvider);
      final response = await repository.uploadMedia(filePath);
      state = BaseState.data(response);
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }
}
