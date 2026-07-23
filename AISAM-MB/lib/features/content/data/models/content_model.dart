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
    String? brandName,
    String? productId,
    required AdTypeEnum adType,
    String? title,
    required String textContent,
    String? imageUrl,
    String? videoUrl,
    String? tags,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    required bool isAiGenerated,
    required ContentStatusEnum status,
    required DateTime createdAt,
    required DateTime updatedAt,
  }) = _ContentResponseModel;

  factory ContentResponseModel.fromJson(Map<String, dynamic> json) =>
      _$ContentResponseModelFromJson(json);
}
