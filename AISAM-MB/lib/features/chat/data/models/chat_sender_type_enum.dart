import 'package:json_annotation/json_annotation.dart';

enum ChatSenderTypeEnum {
  @JsonValue(0) user,
  @JsonValue(1) ai,
  @JsonValue(2) system,
}
