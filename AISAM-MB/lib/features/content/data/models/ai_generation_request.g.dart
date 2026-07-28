// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'ai_generation_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CreateDraftRequestImpl _$$CreateDraftRequestImplFromJson(
  Map<String, dynamic> json,
) => _$CreateDraftRequestImpl(
  brandId: json['brandId'] as String,
  productId: json['productId'] as String?,
  adType: $enumDecode(_$AdTypeEnumEnumMap, json['adType']),
  title: json['title'] as String?,
  prompt: json['prompt'] as String,
);

Map<String, dynamic> _$$CreateDraftRequestImplToJson(
  _$CreateDraftRequestImpl instance,
) => <String, dynamic>{
  'brandId': instance.brandId,
  'productId': instance.productId,
  'adType': _$AdTypeEnumEnumMap[instance.adType]!,
  'title': instance.title,
  'prompt': instance.prompt,
};

const _$AdTypeEnumEnumMap = {
  AdTypeEnum.textOnly: 0,
  AdTypeEnum.imageText: 1,
  AdTypeEnum.videoText: 2,
};

_$ImproveContentRequestImpl _$$ImproveContentRequestImplFromJson(
  Map<String, dynamic> json,
) => _$ImproveContentRequestImpl(
  content: json['content'] as String,
  instructions: json['instructions'] as String,
  prompt: json['prompt'] as String,
);

Map<String, dynamic> _$$ImproveContentRequestImplToJson(
  _$ImproveContentRequestImpl instance,
) => <String, dynamic>{
  'content': instance.content,
  'instructions': instance.instructions,
  'prompt': instance.prompt,
};
