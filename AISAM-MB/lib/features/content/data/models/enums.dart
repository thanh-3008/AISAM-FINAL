import 'package:freezed_annotation/freezed_annotation.dart';

enum ContentStatusEnum {
  @JsonValue(0) draft,
  @JsonValue(1) pendingApproval,
  @JsonValue(2) approved,
  @JsonValue(3) rejected,
  @JsonValue(4) published,
}

enum AiStatusEnum {
  @JsonValue(0) pending,
  @JsonValue(1) completed,
  @JsonValue(2) failed,
  @JsonValue(3) processing,
}

enum AdTypeEnum {
  @JsonValue(0) textOnly,
  @JsonValue(1) imageText,
  @JsonValue(2) videoText,
}
