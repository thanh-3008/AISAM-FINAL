import 'package:freezed_annotation/freezed_annotation.dart';

part 'profile_model.freezed.dart';
part 'profile_model.g.dart';

@freezed
class ProfileResponseModel with _$ProfileResponseModel {
  const factory ProfileResponseModel({
    required String id,
    required String userId,
    required String name,
    required int profileType,
    String? subscriptionId,
    String? companyName,
    String? bio,
    String? avatarUrl,
    required int status,
    required DateTime createdAt,
    required DateTime updatedAt,
    required bool isOwner,
    String? memberRole,
  }) = _ProfileResponseModel;

  factory ProfileResponseModel.fromJson(Map<String, dynamic> json) =>
      _$ProfileResponseModelFromJson(json);
}
