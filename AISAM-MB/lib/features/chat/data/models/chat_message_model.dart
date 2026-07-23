import 'package:freezed_annotation/freezed_annotation.dart';
import 'chat_sender_type_enum.dart';

part 'chat_message_model.freezed.dart';
part 'chat_message_model.g.dart';

@freezed
class ChatMessageModel with _$ChatMessageModel {
  const factory ChatMessageModel({
    required String id,
    required ChatSenderTypeEnum senderType,
    required String message,
    String? aiGenerationId,
    String? contentId,
    required DateTime createdAt,
  }) = _ChatMessageModel;

  factory ChatMessageModel.fromJson(Map<String, dynamic> json) =>
      _$ChatMessageModelFromJson(json);
}
