// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'chat_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ChatResponseImpl _$$ChatResponseImplFromJson(Map<String, dynamic> json) =>
    _$ChatResponseImpl(
      response: json['response'] as String,
      conversationId: json['conversationId'] as String,
      shouldCreateContent: json['shouldCreateContent'] as bool,
      createdContentId: json['createdContentId'] as String?,
    );

Map<String, dynamic> _$$ChatResponseImplToJson(_$ChatResponseImpl instance) =>
    <String, dynamic>{
      'response': instance.response,
      'conversationId': instance.conversationId,
      'shouldCreateContent': instance.shouldCreateContent,
      'createdContentId': instance.createdContentId,
    };
