// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'content_schedule_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ContentScheduleModelImpl _$$ContentScheduleModelImplFromJson(
  Map<String, dynamic> json,
) => _$ContentScheduleModelImpl(
  id: json['id'] as String,
  profileId: json['profileId'] as String,
  contentId: json['contentId'] as String,
  integrationId: json['integrationId'] as String,
  scheduledAt: DateTime.parse(json['scheduledAt'] as String),
  executedAt: json['executedAt'] == null
      ? null
      : DateTime.parse(json['executedAt'] as String),
  status: $enumDecode(_$ScheduleStatusEnumEnumMap, json['status']),
  attemptCount: (json['attemptCount'] as num?)?.toInt() ?? 0,
  lastError: json['lastError'] as String?,
  title: json['title'] as String?,
  brandName: json['brandName'] as String?,
  type: json['type'] as String?,
  platform: json['platform'] as String?,
);

Map<String, dynamic> _$$ContentScheduleModelImplToJson(
  _$ContentScheduleModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'profileId': instance.profileId,
  'contentId': instance.contentId,
  'integrationId': instance.integrationId,
  'scheduledAt': instance.scheduledAt.toIso8601String(),
  'executedAt': instance.executedAt?.toIso8601String(),
  'status': _$ScheduleStatusEnumEnumMap[instance.status]!,
  'attemptCount': instance.attemptCount,
  'lastError': instance.lastError,
  'title': instance.title,
  'brandName': instance.brandName,
  'type': instance.type,
  'platform': instance.platform,
};

const _$ScheduleStatusEnumEnumMap = {
  ScheduleStatusEnum.pending: 'Pending',
  ScheduleStatusEnum.processing: 'Processing',
  ScheduleStatusEnum.completed: 'Completed',
  ScheduleStatusEnum.failed: 'Failed',
  ScheduleStatusEnum.cancelled: 'Cancelled',
};
