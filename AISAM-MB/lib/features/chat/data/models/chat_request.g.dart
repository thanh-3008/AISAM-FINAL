// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'chat_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ChatRequestImpl _$$ChatRequestImplFromJson(Map<String, dynamic> json) =>
    _$ChatRequestImpl(
      brandId: json['brandId'] as String?,
      productId: json['productId'] as String?,
      adType: $enumDecode(_$AdTypeEnumEnumMap, json['adType']),
      message: json['message'] as String,
      conversationId: json['conversationId'] as String?,
    );

Map<String, dynamic> _$$ChatRequestImplToJson(_$ChatRequestImpl instance) =>
    <String, dynamic>{
      'brandId': instance.brandId,
      'productId': instance.productId,
      'adType': _$AdTypeEnumEnumMap[instance.adType]!,
      'message': instance.message,
      'conversationId': instance.conversationId,
    };

const _$AdTypeEnumEnumMap = {
  AdTypeEnum.textOnly: 0,
  AdTypeEnum.imageText: 1,
  AdTypeEnum.videoText: 2,
};
