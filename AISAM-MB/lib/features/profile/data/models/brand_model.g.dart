// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'brand_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$BrandResponseModelImpl _$$BrandResponseModelImplFromJson(
  Map<String, dynamic> json,
) => _$BrandResponseModelImpl(
  id: json['id'] as String,
  userId: json['userId'] as String,
  name: json['name'] as String,
  description: json['description'] as String?,
  logoUrl: json['logoUrl'] as String?,
  slogan: json['slogan'] as String?,
  usp: json['usp'] as String?,
  targetAudience: json['targetAudience'] as String?,
  profileId: json['profileId'] as String?,
  workspaceId: json['workspaceId'] as String?,
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt: DateTime.parse(json['updatedAt'] as String),
  productsCount: (json['productsCount'] as num).toInt(),
  contentsCount: (json['contentsCount'] as num).toInt(),
);

Map<String, dynamic> _$$BrandResponseModelImplToJson(
  _$BrandResponseModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'userId': instance.userId,
  'name': instance.name,
  'description': instance.description,
  'logoUrl': instance.logoUrl,
  'slogan': instance.slogan,
  'usp': instance.usp,
  'targetAudience': instance.targetAudience,
  'profileId': instance.profileId,
  'workspaceId': instance.workspaceId,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt.toIso8601String(),
  'productsCount': instance.productsCount,
  'contentsCount': instance.contentsCount,
};
