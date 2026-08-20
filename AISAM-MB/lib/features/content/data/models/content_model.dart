import 'package:freezed_annotation/freezed_annotation.dart';
import 'enums.dart';

part 'content_model.freezed.dart';
part 'content_model.g.dart';

@freezed
class ContentResponseModel with _$ContentResponseModel {
  const factory ContentResponseModel({
    required String id,
    required String profileId,
    required String brandId,
    String? workspaceId,
    String? brandName,
    String? productId,
    required AdTypeEnum adType,
    String? title,
    @Default('') String textContent,
    String? imageUrl,
    String? videoUrl,
    String? thumbnailUrl,
    String? tags,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    String? platformRejectionReason,
    String? rejectedPlatform,
    required bool isAiGenerated,
    required ContentStatusEnum status,
    required DateTime createdAt,
    required DateTime updatedAt,
  }) = _ContentResponseModel;

  factory ContentResponseModel.fromJson(Map<String, dynamic> json) =>
      _$ContentResponseModelFromJson(json);
}
