import 'package:json_annotation/json_annotation.dart';

enum ScheduleStatusEnum {
  @JsonValue('Pending')
  pending,
  
  @JsonValue('Processing')
  processing,
  
  @JsonValue('Completed')
  completed,
  
  @JsonValue('Failed')
  failed,
  
  @JsonValue('Cancelled')
  cancelled,
}
