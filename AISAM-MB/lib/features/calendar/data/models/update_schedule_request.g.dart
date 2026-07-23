// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'update_schedule_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$UpdateScheduleRequestImpl _$$UpdateScheduleRequestImplFromJson(
  Map<String, dynamic> json,
) => _$UpdateScheduleRequestImpl(
  integrationId: json['integrationId'] as String?,
  scheduledAt: json['scheduledAt'] == null
      ? null
      : DateTime.parse(json['scheduledAt'] as String),
);

Map<String, dynamic> _$$UpdateScheduleRequestImplToJson(
  _$UpdateScheduleRequestImpl instance,
) => <String, dynamic>{
  'integrationId': instance.integrationId,
  'scheduledAt': instance.scheduledAt?.toIso8601String(),
};
