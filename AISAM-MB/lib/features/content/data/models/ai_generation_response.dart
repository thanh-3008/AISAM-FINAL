import 'package:freezed_annotation/freezed_annotation.dart';
import 'enums.dart';

part 'ai_generation_response.freezed.dart';
part 'ai_generation_response.g.dart';

@freezed
class AiGenerationResponseModel with _$AiGenerationResponseModel {
  const factory AiGenerationResponseModel({
    required String aiGenerationId,
    required String contentId,
    String? generatedText,
    String? generatedImageUrl,
    String? generatedVideoUrl,
    String? videoJobId,
    String? providerUsed,
    required AiStatusEnum status,
    String? errorMessage,
    required DateTime createdAt,
  }) = _AiGenerationResponseModel;

  factory AiGenerationResponseModel.fromJson(Map<String, dynamic> json) =>
      _$AiGenerationResponseModelFromJson(json);
}
