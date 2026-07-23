// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_schedule_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CreateScheduleRequestImpl _$$CreateScheduleRequestImplFromJson(
  Map<String, dynamic> json,
) => _$CreateScheduleRequestImpl(
  contentId: json['contentId'] as String,
  integrationId: json['integrationId'] as String,
  scheduledAt: DateTime.parse(json['scheduledAt'] as String),
);

Map<String, dynamic> _$$CreateScheduleRequestImplToJson(
  _$CreateScheduleRequestImpl instance,
) => <String, dynamic>{
  'contentId': instance.contentId,
  'integrationId': instance.integrationId,
  'scheduledAt': instance.scheduledAt.toIso8601String(),
};
