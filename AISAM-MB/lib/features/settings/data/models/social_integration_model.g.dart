// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'social_integration_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$SocialIntegrationModelImpl _$$SocialIntegrationModelImplFromJson(
  Map<String, dynamic> json,
) => _$SocialIntegrationModelImpl(
  id: json['id'] as String,
  platform: json['platform'] as String,
  name: json['name'] as String?,
  type: json['type'] as String?,
  category: json['category'] as String?,
  profilePictureUrl: json['profilePictureUrl'] as String?,
  isActive: json['isActive'] as bool? ?? true,
  brandName: json['brandName'] as String?,
);

Map<String, dynamic> _$$SocialIntegrationModelImplToJson(
  _$SocialIntegrationModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'platform': instance.platform,
  'name': instance.name,
  'type': instance.type,
  'category': instance.category,
  'profilePictureUrl': instance.profilePictureUrl,
  'isActive': instance.isActive,
  'brandName': instance.brandName,
};
