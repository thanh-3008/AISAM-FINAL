import 'package:freezed_annotation/freezed_annotation.dart';
import 'enums.dart';
import 'dart:convert';

part 'content_model.freezed.dart';

@freezed
class ContentResponseModel with _$ContentResponseModel {
  const factory ContentResponseModel({
    required String id,
    required String profileId,
    required String brandId,
    String? workspaceId,
    String? brandName,
    String? creatorId,
    String? creatorName,
    String? productId,
    String? teamId,
    required AdTypeEnum adType,
    String? title,
    @Default('') String textContent,
    String? richTextJson,
    int? richTextVersion,
    String? imageUrl,
    List<String>? imageUrls,
    String? videoUrl,
    String? thumbnailUrl,
    String? tags,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    String? rejectionReason,
    String? platformRejectionReason,
    String? rejectedPlatform,
    required bool isAiGenerated,
    required ContentStatusEnum status,
    required DateTime createdAt,
    required DateTime updatedAt,
  }) = _ContentResponseModel;

  factory ContentResponseModel.fromJson(Map<String, dynamic> json) {
    final nowIso = DateTime.now().toIso8601String();
    final createdAtRaw = json['createdAt']?.toString() ?? nowIso;
    final updatedAtRaw = json['updatedAt']?.toString() ?? json['createdAt']?.toString() ?? nowIso;

    // Normalize status (supports int 0..7 or string names)
    dynamic rawStatus = json['status'];
    int statusInt = 0;
    if (rawStatus is int) {
      statusInt = (rawStatus >= 0 && rawStatus <= 7) ? rawStatus : 0;
    } else if (rawStatus is String) {
      final parsed = int.tryParse(rawStatus);
      if (parsed != null && parsed >= 0 && parsed <= 7) {
        statusInt = parsed;
      } else {
        final lower = rawStatus.trim().toLowerCase();
        switch (lower) {
          case 'pendingapproval':
          case 'pending_approval':
          case 'pending':
          case 'awaiting approval':
          case 'awaitingapproval':
            statusInt = 1;
            break;
          case 'approved':
            statusInt = 2;
            break;
          case 'rejected':
            statusInt = 3;
            break;
          case 'published':
            statusInt = 4;
            break;
          case 'flagged':
            statusInt = 5;
            break;
          case 'rejectedbyplatform':
          case 'rejected_by_platform':
            statusInt = 6;
            break;
          case 'failed':
            statusInt = 7;
            break;
          default:
            statusInt = 0;
            break;
        }
      }
    }

    // Normalize adType (supports int 0..2 or strings)
    dynamic rawAdType = json['adType'];
    int adTypeInt = 0;
    if (rawAdType is int) {
      adTypeInt = (rawAdType >= 0 && rawAdType <= 2) ? rawAdType : 0;
    } else if (rawAdType is String) {
      final parsed = int.tryParse(rawAdType);
      if (parsed != null && parsed >= 0 && parsed <= 2) {
        adTypeInt = parsed;
      } else {
        final lower = rawAdType.trim().toLowerCase();
        if (lower.contains('video')) {
          adTypeInt = 2;
        } else if (lower.contains('image')) {
          adTypeInt = 1;
        } else {
          adTypeInt = 0;
        }
      }
    }

    final isAi = json['isAiGenerated'] == true ||
        json['isAiGenerated'] == 1 ||
        json['isAiGenerated']?.toString().toLowerCase() == 'true';

    final textContent = json['plainText']?.toString() ??
        json['textContent']?.toString() ??
        '';

    List<String>? parsedImageUrls;
    if (json['imageUrls'] is List) {
      parsedImageUrls = (json['imageUrls'] as List)
          .map((e) => e.toString())
          .where((v) {
            final uri = Uri.tryParse(v);
            return uri != null && ['https', 'http'].contains(uri.scheme);
          })
          .toList();
    }

    final createdAt = DateTime.tryParse(createdAtRaw) ?? DateTime.now();
    final updatedAt = DateTime.tryParse(updatedAtRaw) ?? createdAt;

    return ContentResponseModel(
      id: json['id']?.toString() ?? '',
      profileId: json['profileId']?.toString() ?? '',
      brandId: json['brandId']?.toString() ?? '',
      workspaceId: json['workspaceId']?.toString(),
      brandName: json['brandName']?.toString(),
      creatorId: json['creatorId']?.toString(),
      creatorName: json['creatorName']?.toString(),
      productId: json['productId']?.toString(),
      teamId: json['teamId']?.toString(),
      adType: (adTypeInt >= 0 && adTypeInt < AdTypeEnum.values.length)
          ? AdTypeEnum.values[adTypeInt]
          : AdTypeEnum.textOnly,
      title: json['title']?.toString(),
      textContent: textContent,
      richTextJson: json['richTextJson']?.toString(),
      richTextVersion: json['richTextVersion'] is int
          ? json['richTextVersion'] as int
          : int.tryParse(json['richTextVersion']?.toString() ?? ''),
      imageUrl: json['imageUrl']?.toString(),
      imageUrls: parsedImageUrls,
      videoUrl: json['videoUrl']?.toString(),
      thumbnailUrl: json['thumbnailUrl']?.toString(),
      tags: json['tags'] is List
          ? (json['tags'] as List).join(', ')
          : json['tags']?.toString(),
      styleDescription: json['styleDescription']?.toString(),
      contextDescription: json['contextDescription']?.toString(),
      representativeCharacter: json['representativeCharacter']?.toString(),
      rejectionReason: json['rejectionReason']?.toString(),
      platformRejectionReason: json['platformRejectionReason']?.toString(),
      rejectedPlatform: json['rejectedPlatform']?.toString(),
      isAiGenerated: isAi,
      status: (statusInt >= 0 && statusInt < ContentStatusEnum.values.length)
          ? ContentStatusEnum.values[statusInt]
          : ContentStatusEnum.draft,
      createdAt: createdAt,
      updatedAt: updatedAt,
    );
  }
}

extension ContentMediaCompatibility on ContentResponseModel {
  List<String> get legacyImageUrls {
    if (imageUrls != null && imageUrls!.isNotEmpty) {
      return imageUrls!;
    }
    final raw=imageUrl;
    if(raw==null || raw.isEmpty) return [];
    dynamic value=raw;
    try { value=jsonDecode(raw); } catch(_) { /* Legacy single URL. */ }
    final values=value is List?value:[value];
    return values.whereType<String>().where((v){final uri=Uri.tryParse(v);return uri!=null && ['https','http'].contains(uri.scheme);}).toList();
  }
}
