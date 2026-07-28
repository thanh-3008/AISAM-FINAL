import 'package:freezed_annotation/freezed_annotation.dart';
import '../../../content/data/models/enums.dart';

part 'conversation_model.freezed.dart';
part 'conversation_model.g.dart';

@freezed
class ConversationModel with _$ConversationModel {
  const factory ConversationModel({
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
  }) = _ConversationModel;

  factory ConversationModel.fromJson(Map<String, dynamic> json) =>
      _$ConversationModelFromJson(json);
}
