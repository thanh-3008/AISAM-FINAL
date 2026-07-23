// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'content_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CreateContentRequestImpl _$$CreateContentRequestImplFromJson(
  Map<String, dynamic> json,
) => _$CreateContentRequestImpl(
  brandId: json['brandId'] as String,
  productId: json['productId'] as String?,
  adType: $enumDecode(_$AdTypeEnumEnumMap, json['adType']),
  title: json['title'] as String?,
  textContent: json['textContent'] as String,
  imageUrl: json['imageUrl'] as String?,
  videoUrl: json['videoUrl'] as String?,
  styleDescription: json['styleDescription'] as String?,
  contextDescription: json['contextDescription'] as String?,
  representativeCharacter: json['representativeCharacter'] as String?,
  status: $enumDecodeNullable(_$ContentStatusEnumEnumMap, json['status']),
  isAiGenerated: json['isAiGenerated'] as bool? ?? false,
  tags: (json['tags'] as List<dynamic>?)?.map((e) => e as String).toList(),
);

Map<String, dynamic> _$$CreateContentRequestImplToJson(
  _$CreateContentRequestImpl instance,
) => <String, dynamic>{
  'brandId': instance.brandId,
  'productId': instance.productId,
  'adType': _$AdTypeEnumEnumMap[instance.adType]!,
  'title': instance.title,
  'textContent': instance.textContent,
  'imageUrl': instance.imageUrl,
  'videoUrl': instance.videoUrl,
  'styleDescription': instance.styleDescription,
  'contextDescription': instance.contextDescription,
  'representativeCharacter': instance.representativeCharacter,
  'status': _$ContentStatusEnumEnumMap[instance.status],
  'isAiGenerated': instance.isAiGenerated,
  'tags': instance.tags,
};

const _$AdTypeEnumEnumMap = {
  AdTypeEnum.textOnly: 0,
  AdTypeEnum.imageText: 1,
  AdTypeEnum.videoText: 2,
};

const _$ContentStatusEnumEnumMap = {
  ContentStatusEnum.draft: 0,
  ContentStatusEnum.pendingApproval: 1,
  ContentStatusEnum.approved: 2,
  ContentStatusEnum.rejected: 3,
  ContentStatusEnum.published: 4,
};

_$UpdateContentRequestImpl _$$UpdateContentRequestImplFromJson(
  Map<String, dynamic> json,
) => _$UpdateContentRequestImpl(
  title: json['title'] as String?,
  textContent: json['textContent'] as String?,
  imageUrl: json['imageUrl'] as String?,
  videoUrl: json['videoUrl'] as String?,
  status: $enumDecodeNullable(_$ContentStatusEnumEnumMap, json['status']),
  tags: (json['tags'] as List<dynamic>?)?.map((e) => e as String).toList(),
);

Map<String, dynamic> _$$UpdateContentRequestImplToJson(
  _$UpdateContentRequestImpl instance,
) => <String, dynamic>{
  'title': instance.title,
  'textContent': instance.textContent,
  'imageUrl': instance.imageUrl,
  'videoUrl': instance.videoUrl,
  'status': _$ContentStatusEnumEnumMap[instance.status],
  'tags': instance.tags,
};
