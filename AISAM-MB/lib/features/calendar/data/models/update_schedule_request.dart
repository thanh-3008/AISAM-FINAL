import 'package:freezed_annotation/freezed_annotation.dart';

part 'update_schedule_request.freezed.dart';
part 'update_schedule_request.g.dart';

@freezed
class UpdateScheduleRequest with _$UpdateScheduleRequest {
  const factory UpdateScheduleRequest({
    String? integrationId,
    DateTime? scheduledAt,
  }) = _UpdateScheduleRequest;

  factory UpdateScheduleRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateScheduleRequestFromJson(json);
}
