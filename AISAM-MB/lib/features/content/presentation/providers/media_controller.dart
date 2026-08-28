import 'dart:io';
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
      final file = File(filePath);
      if (!await file.exists()) {
        throw ValidationException('File does not exist.');
      }
      
      final fileSize = await file.length();
      const maxSize = 50 * 1024 * 1024; // 50MB
      if (fileSize > maxSize) {
        throw ValidationException('Media file must be 50MB or smaller.');
      }

      final lastDotIndex = filePath.lastIndexOf('.');
      final ext = lastDotIndex != -1 ? filePath.substring(lastDotIndex).toLowerCase() : '';
      const allowedExts = ['.jpg', '.jpeg', '.png', '.webp', '.gif', '.mp4', '.webm', '.mov', '.quicktime'];
      if (!allowedExts.contains(ext)) {
        throw ValidationException('Please upload a valid image (JPEG, PNG, WebP, GIF) or video (MP4, WebM, MOV) file.');
      }

      final repository = ref.read(mediaRepositoryProvider);
      final response = await repository.uploadMedia(filePath);
      state = BaseState.data(response);
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }
}
