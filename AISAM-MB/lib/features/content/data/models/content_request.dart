import 'package:freezed_annotation/freezed_annotation.dart';
import 'enums.dart';

part 'content_request.freezed.dart';
part 'content_request.g.dart';

@freezed
class CreateContentRequest with _$CreateContentRequest {
  const factory CreateContentRequest({
    required String brandId,
    String? teamId,
    String? productId,
    required AdTypeEnum adType,
    String? title,
    required String textContent,
    String? richTextJson,
    int? richTextVersion,
    String? imageUrl,
    List<String>? imageUrls,
    String? videoUrl,
    String? thumbnailUrl,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    ContentStatusEnum? status,
    @Default(false) bool isAiGenerated,
    List<String>? tags,
  }) = _CreateContentRequest;

  factory CreateContentRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateContentRequestFromJson(json);
}

@freezed
class UpdateContentRequest with _$UpdateContentRequest {
  const factory UpdateContentRequest({
    String? productId,
    AdTypeEnum? adType,
    String? title,
    String? textContent,
    String? richTextJson,
    int? richTextVersion,
    String? imageUrl,
    List<String>? imageUrls,
    String? videoUrl,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    ContentStatusEnum? status,
    List<String>? tags,
  }) = _UpdateContentRequest;

  factory UpdateContentRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateContentRequestFromJson(json);
}
