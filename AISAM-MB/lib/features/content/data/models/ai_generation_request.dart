import 'package:freezed_annotation/freezed_annotation.dart';
import 'enums.dart';

part 'ai_generation_request.freezed.dart';
part 'ai_generation_request.g.dart';

@freezed
class CreateDraftRequest with _$CreateDraftRequest {
  const factory CreateDraftRequest({
    required String brandId,
    String? productId,
    required AdTypeEnum adType,
    String? title,
    required String prompt,
  }) = _CreateDraftRequest;

  factory CreateDraftRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateDraftRequestFromJson(json);
}

@freezed
class ImproveContentRequest with _$ImproveContentRequest {
  const factory ImproveContentRequest({
    required String content,
    required String instructions,
    required String prompt,
  }) = _ImproveContentRequest;

  factory ImproveContentRequest.fromJson(Map<String, dynamic> json) =>
      _$ImproveContentRequestFromJson(json);
}
