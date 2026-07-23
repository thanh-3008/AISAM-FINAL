import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/media_model.dart';

part 'media_repository.g.dart';

class MediaRepository {
  final Dio _dio;

  MediaRepository(this._dio);

  Future<ContentMediaUploadResponseModel> uploadMedia(String filePath) async {
    try {
      final formData = FormData.fromMap({
        'File': await MultipartFile.fromFile(filePath),
      });

      final response = await _dio.post('/Content/media', data: formData);
      return ContentMediaUploadResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
MediaRepository mediaRepository(MediaRepositoryRef ref) {
  return MediaRepository(ref.read(dioProvider));
}
