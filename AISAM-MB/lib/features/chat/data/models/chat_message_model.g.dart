// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'chat_message_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ChatMessageModelImpl _$$ChatMessageModelImplFromJson(
  Map<String, dynamic> json,
) => _$ChatMessageModelImpl(
  id: json['id'] as String,
  senderType: $enumDecode(_$ChatSenderTypeEnumEnumMap, json['senderType']),
  message: json['message'] as String,
  aiGenerationId: json['aiGenerationId'] as String?,
  contentId: json['contentId'] as String?,
  createdAt: DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$$ChatMessageModelImplToJson(
  _$ChatMessageModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'senderType': _$ChatSenderTypeEnumEnumMap[instance.senderType]!,
  'message': instance.message,
  'aiGenerationId': instance.aiGenerationId,
  'contentId': instance.contentId,
  'createdAt': instance.createdAt.toIso8601String(),
};

const _$ChatSenderTypeEnumEnumMap = {
  ChatSenderTypeEnum.user: 0,
  ChatSenderTypeEnum.ai: 1,
  ChatSenderTypeEnum.system: 2,
};
