import 'package:freezed_annotation/freezed_annotation.dart';

part 'available_target_model.freezed.dart';
part 'available_target_model.g.dart';

@freezed
class AvailableTargetModel with _$AvailableTargetModel {
  const factory AvailableTargetModel({
    required String providerTargetId,
    required String name,
    required String type,
    String? category,
    String? profilePictureUrl,
    @Default(true) bool isActive,
    String? linkedBrandId,
    String? linkedBrandName,
    String? linkedIntegrationId,
  }) = _AvailableTargetModel;

  factory AvailableTargetModel.fromJson(Map<String, dynamic> json) =>
      _$AvailableTargetModelFromJson(json);
}
