import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/features/content/data/models/content_model.dart';
import 'package:aisam_mb/features/content/data/models/enums.dart';

void main() {
  group('Content Module Tests', () {
    test('ContentResponseModel deserializes valid backend JSON with newly supported enums', () {
      final json = {
        'id': '11111111-1111-1111-1111-111111111111',
        'profileId': '22222222-2222-2222-2222-222222222222',
        'brandId': '33333333-3333-3333-3333-333333333333',
        'workspaceId': '44444444-4444-4444-4444-444444444444',
        'brandName': 'Test Brand',
        'adType': 1,
        'title': 'Test Post',
        'plainText': 'Hello World AISAM',
        'isAiGenerated': true,
        'status': 5, // flagged
        'createdAt': '2026-09-12T00:00:00.000Z',
        'updatedAt': '2026-09-12T01:00:00.000Z',
      };

      final model = ContentResponseModel.fromJson(json);

      expect(model.id, '11111111-1111-1111-1111-111111111111');
      expect(model.textContent, 'Hello World AISAM');
      expect(model.status, ContentStatusEnum.flagged);
      expect(model.adType, AdTypeEnum.imageText);
    });

    test('ContentResponseModel parses status 6 (rejectedByPlatform) and 7 (failed)', () {
      final jsonRejected = {
        'id': '11111111-1111-1111-1111-111111111111',
        'profileId': '22222222-2222-2222-2222-222222222222',
        'brandId': '33333333-3333-3333-3333-333333333333',
        'adType': 0,
        'plainText': 'Rejected post',
        'isAiGenerated': false,
        'status': 6, // rejectedByPlatform
        'createdAt': '2026-09-12T00:00:00.000Z',
        'updatedAt': '2026-09-12T01:00:00.000Z',
      };

      final modelRejected = ContentResponseModel.fromJson(jsonRejected);
      expect(modelRejected.status, ContentStatusEnum.rejectedByPlatform);

      final jsonFailed = {
        ...jsonRejected,
        'status': 7, // failed
      };
      final modelFailed = ContentResponseModel.fromJson(jsonFailed);
      expect(modelFailed.status, ContentStatusEnum.failed);
    });

    test('ContentResponseModel parses string status "PendingApproval" and string adType "image"', () {
      final json = {
        'id': '11111111-1111-1111-1111-111111111111',
        'profileId': '22222222-2222-2222-2222-222222222222',
        'brandId': '33333333-3333-3333-3333-333333333333',
        'adType': 'image',
        'title': 'String status test',
        'textContent': 'Pending review',
        'status': 'PendingApproval',
      };

      final model = ContentResponseModel.fromJson(json);
      expect(model.status, ContentStatusEnum.pendingApproval);
      expect(model.adType, AdTypeEnum.imageText);
      expect(model.isAiGenerated, false);
      expect(model.textContent, 'Pending review');
    });

    test('ContentResponseModel handles multi-image JSON array in legacyImageUrls', () {
      final json = {
        'id': '11111111-1111-1111-1111-111111111111',
        'profileId': '22222222-2222-2222-2222-222222222222',
        'brandId': '33333333-3333-3333-3333-333333333333',
        'adType': 1,
        'imageUrl': '["https://example.com/img1.png", "https://example.com/img2.jpg"]',
        'status': 1,
      };

      final model = ContentResponseModel.fromJson(json);
      expect(model.legacyImageUrls, ['https://example.com/img1.png', 'https://example.com/img2.jpg']);
    });
  });
}
