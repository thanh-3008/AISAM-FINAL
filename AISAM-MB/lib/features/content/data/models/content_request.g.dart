// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'content_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CreateContentRequestImpl _$$CreateContentRequestImplFromJson(
  Map<String, dynamic> json,
) => _$CreateContentRequestImpl(
  brandId: json['brandId'] as String,
  teamId: json['teamId'] as String?,
  productId: json['productId'] as String?,
  adType: $enumDecode(_$AdTypeEnumEnumMap, json['adType']),
  title: json['title'] as String?,
  textContent: json['textContent'] as String,
  richTextJson: json['richTextJson'] as String?,
  richTextVersion: (json['richTextVersion'] as num?)?.toInt(),
  imageUrl: json['imageUrl'] as String?,
  imageUrls: (json['imageUrls'] as List<dynamic>?)
      ?.map((e) => e as String)
      .toList(),
  videoUrl: json['videoUrl'] as String?,
  thumbnailUrl: json['thumbnailUrl'] as String?,
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
  'teamId': instance.teamId,
  'productId': instance.productId,
  'adType': _$AdTypeEnumEnumMap[instance.adType]!,
  'title': instance.title,
  'textContent': instance.textContent,
  'richTextJson': instance.richTextJson,
  'richTextVersion': instance.richTextVersion,
  'imageUrl': instance.imageUrl,
  'imageUrls': instance.imageUrls,
  'videoUrl': instance.videoUrl,
  'thumbnailUrl': instance.thumbnailUrl,
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
  ContentStatusEnum.flagged: 5,
  ContentStatusEnum.rejectedByPlatform: 6,
  ContentStatusEnum.failed: 7,
};

_$UpdateContentRequestImpl _$$UpdateContentRequestImplFromJson(
  Map<String, dynamic> json,
) => _$UpdateContentRequestImpl(
  productId: json['productId'] as String?,
  adType: $enumDecodeNullable(_$AdTypeEnumEnumMap, json['adType']),
  title: json['title'] as String?,
  textContent: json['textContent'] as String?,
  richTextJson: json['richTextJson'] as String?,
  richTextVersion: (json['richTextVersion'] as num?)?.toInt(),
  imageUrl: json['imageUrl'] as String?,
  imageUrls: (json['imageUrls'] as List<dynamic>?)
      ?.map((e) => e as String)
      .toList(),
  videoUrl: json['videoUrl'] as String?,
  styleDescription: json['styleDescription'] as String?,
  contextDescription: json['contextDescription'] as String?,
  representativeCharacter: json['representativeCharacter'] as String?,
  status: $enumDecodeNullable(_$ContentStatusEnumEnumMap, json['status']),
  tags: (json['tags'] as List<dynamic>?)?.map((e) => e as String).toList(),
);

Map<String, dynamic> _$$UpdateContentRequestImplToJson(
  _$UpdateContentRequestImpl instance,
) => <String, dynamic>{
  'productId': instance.productId,
  'adType': _$AdTypeEnumEnumMap[instance.adType],
  'title': instance.title,
  'textContent': instance.textContent,
  'richTextJson': instance.richTextJson,
  'richTextVersion': instance.richTextVersion,
  'imageUrl': instance.imageUrl,
  'imageUrls': instance.imageUrls,
  'videoUrl': instance.videoUrl,
  'styleDescription': instance.styleDescription,
  'contextDescription': instance.contextDescription,
  'representativeCharacter': instance.representativeCharacter,
  'status': _$ContentStatusEnumEnumMap[instance.status],
  'tags': instance.tags,
};
