import 'package:freezed_annotation/freezed_annotation.dart';

part 'social_integration_model.freezed.dart';
part 'social_integration_model.g.dart';

@freezed
class SocialIntegrationModel with _$SocialIntegrationModel {
  const factory SocialIntegrationModel({
    required String id,
    required String platform,
    String? name,
    String? type,
    String? category,
    String? profilePictureUrl,
    @Default(true) bool isActive,
    String? brandName,
  }) = _SocialIntegrationModel;

  factory SocialIntegrationModel.fromJson(Map<String, dynamic> json) =>
      _$SocialIntegrationModelFromJson(json);
}
