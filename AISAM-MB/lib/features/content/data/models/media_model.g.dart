// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'media_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ContentMediaUploadResponseModelImpl
_$$ContentMediaUploadResponseModelImplFromJson(Map<String, dynamic> json) =>
    _$ContentMediaUploadResponseModelImpl(
      url: json['url'] as String,
      fileName: json['fileName'] as String,
      contentType: json['contentType'] as String,
      size: (json['size'] as num).toInt(),
    );

Map<String, dynamic> _$$ContentMediaUploadResponseModelImplToJson(
  _$ContentMediaUploadResponseModelImpl instance,
) => <String, dynamic>{
  'url': instance.url,
  'fileName': instance.fileName,
  'contentType': instance.contentType,
  'size': instance.size,
};
