// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'brand_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CreateBrandRequestImpl _$$CreateBrandRequestImplFromJson(
  Map<String, dynamic> json,
) => _$CreateBrandRequestImpl(
  name: json['name'] as String,
  description: json['description'] as String?,
  logoUrl: json['logoUrl'] as String?,
  slogan: json['slogan'] as String?,
  usp: json['usp'] as String?,
  targetAudience: json['targetAudience'] as String?,
);

Map<String, dynamic> _$$CreateBrandRequestImplToJson(
  _$CreateBrandRequestImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
  'logoUrl': instance.logoUrl,
  'slogan': instance.slogan,
  'usp': instance.usp,
  'targetAudience': instance.targetAudience,
};

_$UpdateBrandRequestImpl _$$UpdateBrandRequestImplFromJson(
  Map<String, dynamic> json,
) => _$UpdateBrandRequestImpl(
  name: json['name'] as String?,
  description: json['description'] as String?,
  logoUrl: json['logoUrl'] as String?,
  slogan: json['slogan'] as String?,
  usp: json['usp'] as String?,
  targetAudience: json['targetAudience'] as String?,
);

Map<String, dynamic> _$$UpdateBrandRequestImplToJson(
  _$UpdateBrandRequestImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
  'logoUrl': instance.logoUrl,
  'slogan': instance.slogan,
  'usp': instance.usp,
  'targetAudience': instance.targetAudience,
};
