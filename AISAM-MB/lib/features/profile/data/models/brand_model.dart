import 'package:freezed_annotation/freezed_annotation.dart';

part 'brand_model.freezed.dart';
part 'brand_model.g.dart';

@freezed
class BrandResponseModel with _$BrandResponseModel {
  const factory BrandResponseModel({
    required String id,
    required String userId,
    required String name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
    String? profileId,
    String? workspaceId,
    required DateTime createdAt,
    required DateTime updatedAt,
    required int productsCount,
    required int contentsCount,
  }) = _BrandResponseModel;

  factory BrandResponseModel.fromJson(Map<String, dynamic> json) =>
      _$BrandResponseModelFromJson(json);
}
