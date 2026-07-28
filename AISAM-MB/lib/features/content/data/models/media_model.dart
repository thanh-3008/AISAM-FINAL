import 'package:freezed_annotation/freezed_annotation.dart';

part 'media_model.freezed.dart';
part 'media_model.g.dart';

@freezed
class ContentMediaUploadResponseModel with _$ContentMediaUploadResponseModel {
  const factory ContentMediaUploadResponseModel({
    required String url,
    required String fileName,
    required String contentType,
    required int size,
  }) = _ContentMediaUploadResponseModel;

  factory ContentMediaUploadResponseModel.fromJson(Map<String, dynamic> json) =>
      _$ContentMediaUploadResponseModelFromJson(json);
}
