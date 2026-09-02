// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'notification_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$NotificationModelImpl _$$NotificationModelImplFromJson(
  Map<String, dynamic> json,
) => _$NotificationModelImpl(
  id: json['id'] as String,
  type: json['type'] as String,
  title: json['title'] as String,
  message: json['message'] as String,
  targetId: json['targetId'] as String?,
  targetType: json['targetType'] as String?,
  isRead: json['isRead'] as bool,
  createdAt: DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$$NotificationModelImplToJson(
  _$NotificationModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'type': instance.type,
  'title': instance.title,
  'message': instance.message,
  'targetId': instance.targetId,
  'targetType': instance.targetType,
  'isRead': instance.isRead,
  'createdAt': instance.createdAt.toIso8601String(),
};
