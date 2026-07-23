// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'quota_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$QuotaModelImpl _$$QuotaModelImplFromJson(Map<String, dynamic> json) =>
    _$QuotaModelImpl(
      planName: json['planName'] as String? ?? '',
      subscriptionStatus: json['subscriptionStatus'] as String? ?? '',
      windowStart: json['windowStart'] == null
          ? null
          : DateTime.parse(json['windowStart'] as String),
      windowEnd: json['windowEnd'] == null
          ? null
          : DateTime.parse(json['windowEnd'] as String),
      promptQuotaLimit: (json['promptQuotaLimit'] as num?)?.toInt() ?? 0,
      promptUsage: (json['promptUsage'] as num?)?.toInt() ?? 0,
      promptRemaining: (json['promptRemaining'] as num?)?.toInt() ?? 0,
      postQuotaLimit: (json['postQuotaLimit'] as num?)?.toInt() ?? 0,
      postUsage: (json['postUsage'] as num?)?.toInt() ?? 0,
      postRemaining: (json['postRemaining'] as num?)?.toInt() ?? 0,
      textContentCount: (json['textContentCount'] as num?)?.toInt() ?? 0,
      imageContentCount: (json['imageContentCount'] as num?)?.toInt() ?? 0,
      videoContentCount: (json['videoContentCount'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$$QuotaModelImplToJson(_$QuotaModelImpl instance) =>
    <String, dynamic>{
      'planName': instance.planName,
      'subscriptionStatus': instance.subscriptionStatus,
      'windowStart': instance.windowStart?.toIso8601String(),
      'windowEnd': instance.windowEnd?.toIso8601String(),
      'promptQuotaLimit': instance.promptQuotaLimit,
      'promptUsage': instance.promptUsage,
      'promptRemaining': instance.promptRemaining,
      'postQuotaLimit': instance.postQuotaLimit,
      'postUsage': instance.postUsage,
      'postRemaining': instance.postRemaining,
      'textContentCount': instance.textContentCount,
      'imageContentCount': instance.imageContentCount,
      'videoContentCount': instance.videoContentCount,
    };
