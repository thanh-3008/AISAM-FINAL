// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'profile_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CreateProfileRequestImpl _$$CreateProfileRequestImplFromJson(
  Map<String, dynamic> json,
) => _$CreateProfileRequestImpl(
  name: json['name'] as String,
  profileType: (json['profileType'] as num).toInt(),
  companyName: json['companyName'] as String?,
  bio: json['bio'] as String?,
);

Map<String, dynamic> _$$CreateProfileRequestImplToJson(
  _$CreateProfileRequestImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'profileType': instance.profileType,
  'companyName': instance.companyName,
  'bio': instance.bio,
};

_$UpdateProfileRequestImpl _$$UpdateProfileRequestImplFromJson(
  Map<String, dynamic> json,
) => _$UpdateProfileRequestImpl(
  name: json['name'] as String?,
  profileType: (json['profileType'] as num?)?.toInt(),
  companyName: json['companyName'] as String?,
  bio: json['bio'] as String?,
  avatarUrl: json['avatarUrl'] as String?,
);

Map<String, dynamic> _$$UpdateProfileRequestImplToJson(
  _$UpdateProfileRequestImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'profileType': instance.profileType,
  'companyName': instance.companyName,
  'bio': instance.bio,
  'avatarUrl': instance.avatarUrl,
};
