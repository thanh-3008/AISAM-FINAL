// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'available_target_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$AvailableTargetModelImpl _$$AvailableTargetModelImplFromJson(
  Map<String, dynamic> json,
) => _$AvailableTargetModelImpl(
  providerTargetId: json['providerTargetId'] as String,
  name: json['name'] as String,
  type: json['type'] as String,
  category: json['category'] as String?,
  profilePictureUrl: json['profilePictureUrl'] as String?,
  isActive: json['isActive'] as bool? ?? true,
  linkedBrandId: json['linkedBrandId'] as String?,
  linkedBrandName: json['linkedBrandName'] as String?,
  linkedIntegrationId: json['linkedIntegrationId'] as String?,
);

Map<String, dynamic> _$$AvailableTargetModelImplToJson(
  _$AvailableTargetModelImpl instance,
) => <String, dynamic>{
  'providerTargetId': instance.providerTargetId,
  'name': instance.name,
  'type': instance.type,
  'category': instance.category,
  'profilePictureUrl': instance.profilePictureUrl,
  'isActive': instance.isActive,
  'linkedBrandId': instance.linkedBrandId,
  'linkedBrandName': instance.linkedBrandName,
  'linkedIntegrationId': instance.linkedIntegrationId,
};
