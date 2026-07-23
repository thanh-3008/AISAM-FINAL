// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'conversation_detail_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ConversationDetailModelImpl _$$ConversationDetailModelImplFromJson(
  Map<String, dynamic> json,
) => _$ConversationDetailModelImpl(
  id: json['id'] as String,
  profileId: json['profileId'] as String,
  brandId: json['brandId'] as String?,
  brandName: json['brandName'] as String?,
  productId: json['productId'] as String?,
  productName: json['productName'] as String?,
  adType: $enumDecode(_$AdTypeEnumEnumMap, json['adType']),
  title: json['title'] as String?,
  isActive: json['isActive'] as bool,
  lastMessage: json['lastMessage'] as String?,
  lastMessageAt: json['lastMessageAt'] == null
      ? null
      : DateTime.parse(json['lastMessageAt'] as String),
  messageCount: (json['messageCount'] as num).toInt(),
  messages:
      (json['messages'] as List<dynamic>?)
          ?.map((e) => ChatMessageModel.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
);

Map<String, dynamic> _$$ConversationDetailModelImplToJson(
  _$ConversationDetailModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'profileId': instance.profileId,
  'brandId': instance.brandId,
  'brandName': instance.brandName,
  'productId': instance.productId,
  'productName': instance.productName,
  'adType': _$AdTypeEnumEnumMap[instance.adType]!,
  'title': instance.title,
  'isActive': instance.isActive,
  'lastMessage': instance.lastMessage,
  'lastMessageAt': instance.lastMessageAt?.toIso8601String(),
  'messageCount': instance.messageCount,
  'messages': instance.messages,
};

const _$AdTypeEnumEnumMap = {
  AdTypeEnum.textOnly: 0,
  AdTypeEnum.imageText: 1,
  AdTypeEnum.videoText: 2,
};
