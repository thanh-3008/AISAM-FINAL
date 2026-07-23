import 'package:freezed_annotation/freezed_annotation.dart';
import '../../../content/data/models/enums.dart';

part 'chat_request.freezed.dart';
part 'chat_request.g.dart';

@freezed
class ChatRequest with _$ChatRequest {
  const factory ChatRequest({
    String? brandId,
    String? productId,
    required AdTypeEnum adType,
    required String message,
    String? conversationId,
  }) = _ChatRequest;

  factory ChatRequest.fromJson(Map<String, dynamic> json) =>
      _$ChatRequestFromJson(json);
}
