import 'package:freezed_annotation/freezed_annotation.dart';
import 'enums.dart';
import 'dart:convert';

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
      _$ContentResponseModelFromJson({
        ...json,
        // Backend PlainText is canonical; display as text, never HTML.
        'textContent': json['plainText'] ?? json['textContent'] ?? '',
        'updatedAt': json['updatedAt'] ?? json['createdAt'],
      });
}

extension ContentMediaCompatibility on ContentResponseModel {
  List<String> get legacyImageUrls {
    final raw=imageUrl;
    if(raw==null || raw.isEmpty) return [];
    dynamic value=raw;
    try { value=jsonDecode(raw); } catch(_) { /* Legacy single URL. */ }
    final values=value is List?value:[value];
    return values.whereType<String>().where((v){final uri=Uri.tryParse(v);return uri!=null && ['https','http'].contains(uri.scheme);}).toList();
  }
}
