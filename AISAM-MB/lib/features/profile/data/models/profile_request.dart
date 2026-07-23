import 'package:freezed_annotation/freezed_annotation.dart';

part 'profile_request.freezed.dart';
part 'profile_request.g.dart';

@freezed
class CreateProfileRequest with _$CreateProfileRequest {
  const factory CreateProfileRequest({
    required String name,
    required int profileType,
    String? companyName,
    String? bio,
  }) = _CreateProfileRequest;

  factory CreateProfileRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateProfileRequestFromJson(json);
}

@freezed
class UpdateProfileRequest with _$UpdateProfileRequest {
  const factory UpdateProfileRequest({
    String? name,
    int? profileType,
    String? companyName,
    String? bio,
    String? avatarUrl,
  }) = _UpdateProfileRequest;

  factory UpdateProfileRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateProfileRequestFromJson(json);
}
