import 'package:freezed_annotation/freezed_annotation.dart';
import 'schedule_status_enum.dart';

part 'content_schedule_model.freezed.dart';
part 'content_schedule_model.g.dart';

@freezed
class ContentScheduleModel with _$ContentScheduleModel {
  const factory ContentScheduleModel({
    required String id,
    required String profileId,
    required String contentId,
    required String integrationId,
    required DateTime scheduledAt,
    DateTime? executedAt,
    required ScheduleStatusEnum status,
    @Default(0) int attemptCount,
    String? lastError,
    String? title,
    String? brandName,
    String? type,
    String? platform,
  }) = _ContentScheduleModel;

  factory ContentScheduleModel.fromJson(Map<String, dynamic> json) =>
      _$ContentScheduleModelFromJson(json);
}
