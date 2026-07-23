import 'package:freezed_annotation/freezed_annotation.dart';

part 'create_schedule_request.freezed.dart';
part 'create_schedule_request.g.dart';

@freezed
class CreateScheduleRequest with _$CreateScheduleRequest {
  const factory CreateScheduleRequest({
    required String contentId,
    required String integrationId,
    required DateTime scheduledAt,
  }) = _CreateScheduleRequest;

  factory CreateScheduleRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateScheduleRequestFromJson(json);
}
