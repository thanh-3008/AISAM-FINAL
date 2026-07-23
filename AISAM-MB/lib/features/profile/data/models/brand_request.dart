import 'package:freezed_annotation/freezed_annotation.dart';

part 'brand_request.freezed.dart';
part 'brand_request.g.dart';

@freezed
class CreateBrandRequest with _$CreateBrandRequest {
  const factory CreateBrandRequest({
    required String name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
  }) = _CreateBrandRequest;

  factory CreateBrandRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateBrandRequestFromJson(json);
}

@freezed
class UpdateBrandRequest with _$UpdateBrandRequest {
  const factory UpdateBrandRequest({
    String? name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
  }) = _UpdateBrandRequest;

  factory UpdateBrandRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateBrandRequestFromJson(json);
}
