// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'content_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ContentResponseModelImpl _$$ContentResponseModelImplFromJson(
  Map<String, dynamic> json,
) => _$ContentResponseModelImpl(
  id: json['id'] as String,
  profileId: json['profileId'] as String,
  brandId: json['brandId'] as String,
  brandName: json['brandName'] as String?,
  productId: json['productId'] as String?,
  adType: $enumDecode(_$AdTypeEnumEnumMap, json['adType']),
  title: json['title'] as String?,
  textContent: json['textContent'] as String,
  imageUrl: json['imageUrl'] as String?,
  videoUrl: json['videoUrl'] as String?,
  tags: json['tags'] as String?,
  styleDescription: json['styleDescription'] as String?,
  contextDescription: json['contextDescription'] as String?,
  representativeCharacter: json['representativeCharacter'] as String?,
  isAiGenerated: json['isAiGenerated'] as bool,
  status: $enumDecode(_$ContentStatusEnumEnumMap, json['status']),
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt: DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$$ContentResponseModelImplToJson(
  _$ContentResponseModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'profileId': instance.profileId,
  'brandId': instance.brandId,
  'brandName': instance.brandName,
  'productId': instance.productId,
  'adType': _$AdTypeEnumEnumMap[instance.adType]!,
  'title': instance.title,
  'textContent': instance.textContent,
  'imageUrl': instance.imageUrl,
  'videoUrl': instance.videoUrl,
  'tags': instance.tags,
  'styleDescription': instance.styleDescription,
  'contextDescription': instance.contextDescription,
  'representativeCharacter': instance.representativeCharacter,
  'isAiGenerated': instance.isAiGenerated,
  'status': _$ContentStatusEnumEnumMap[instance.status]!,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt.toIso8601String(),
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
