import 'package:freezed_annotation/freezed_annotation.dart';
import '../../../content/data/models/enums.dart';
import 'chat_message_model.dart';

part 'conversation_detail_model.freezed.dart';
part 'conversation_detail_model.g.dart';

@freezed
class ConversationDetailModel with _$ConversationDetailModel {
  const factory ConversationDetailModel({
    required String id,
    required String profileId,
    String? brandId,
    String? brandName,
    String? productId,
    String? productName,
    required AdTypeEnum adType,
    String? title,
    required bool isActive,
    String? lastMessage,
    DateTime? lastMessageAt,
    required int messageCount,
    @Default([]) List<ChatMessageModel> messages,
  }) = _ConversationDetailModel;

  factory ConversationDetailModel.fromJson(Map<String, dynamic> json) =>
      _$ConversationDetailModelFromJson(json);
}
