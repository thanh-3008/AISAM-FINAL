// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'notification_preference_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$NotificationPreferenceModelImpl _$$NotificationPreferenceModelImplFromJson(
  Map<String, dynamic> json,
) => _$NotificationPreferenceModelImpl(
  notificationType: (json['notificationType'] as num).toInt(),
  isEnabled: json['isEnabled'] as bool? ?? true,
);

Map<String, dynamic> _$$NotificationPreferenceModelImplToJson(
  _$NotificationPreferenceModelImpl instance,
) => <String, dynamic>{
  'notificationType': instance.notificationType,
  'isEnabled': instance.isEnabled,
};
