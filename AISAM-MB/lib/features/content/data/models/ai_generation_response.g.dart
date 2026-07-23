// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'ai_generation_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$AiGenerationResponseModelImpl _$$AiGenerationResponseModelImplFromJson(
  Map<String, dynamic> json,
) => _$AiGenerationResponseModelImpl(
  aiGenerationId: json['aiGenerationId'] as String,
  contentId: json['contentId'] as String,
  generatedText: json['generatedText'] as String?,
  generatedImageUrl: json['generatedImageUrl'] as String?,
  generatedVideoUrl: json['generatedVideoUrl'] as String?,
  videoJobId: json['videoJobId'] as String?,
  providerUsed: json['providerUsed'] as String?,
  status: $enumDecode(_$AiStatusEnumEnumMap, json['status']),
  errorMessage: json['errorMessage'] as String?,
  createdAt: DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$$AiGenerationResponseModelImplToJson(
  _$AiGenerationResponseModelImpl instance,
) => <String, dynamic>{
  'aiGenerationId': instance.aiGenerationId,
  'contentId': instance.contentId,
  'generatedText': instance.generatedText,
  'generatedImageUrl': instance.generatedImageUrl,
  'generatedVideoUrl': instance.generatedVideoUrl,
  'videoJobId': instance.videoJobId,
  'providerUsed': instance.providerUsed,
  'status': _$AiStatusEnumEnumMap[instance.status]!,
  'errorMessage': instance.errorMessage,
  'createdAt': instance.createdAt.toIso8601String(),
};

const _$AiStatusEnumEnumMap = {
  AiStatusEnum.pending: 0,
  AiStatusEnum.completed: 1,
  AiStatusEnum.failed: 2,
  AiStatusEnum.processing: 3,
};
