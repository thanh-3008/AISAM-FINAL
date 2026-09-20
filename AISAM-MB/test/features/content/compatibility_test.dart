import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/features/content/data/models/content_model.dart';
import 'package:aisam_mb/features/content/data/models/content_request.dart';
import 'package:aisam_mb/features/content/data/models/enums.dart';
import 'package:aisam_mb/features/content/data/repositories/content_repository.dart';

Map<String, dynamic> fixture() => {
  'id': 'id',
  'profileId': 'profile',
  'brandId': 'brand',
  'adType': 0,
  'isAiGenerated': false,
  'status': 0,
  'createdAt': '2026-09-10T00:00:00Z',
  'textContent': 'legacy',
};

class MockWithdrawDio extends Fake implements Dio {
  String? lastPath;
  @override
  Future<Response<T>> post<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
    ProgressCallback? onSendProgress,
    ProgressCallback? onReceiveProgress,
  }) async {
    lastPath = path;
    return Response<T>(
      data: {'success': true, 'data': true} as T,
      statusCode: 200,
      requestOptions: RequestOptions(path: path),
    );
  }
}

void main() {
  group('Content Compatibility & Batch 3 Sync', () {
    test('canonical plain text takes precedence without parsing HTML', () {
      final model = ContentResponseModel.fromJson({
        ...fixture(),
        'plainText': '<script>alert(1)</script>',
        'richTextVersion': 99,
      });
      expect(model.textContent, '<script>alert(1)</script>');
      expect(model.updatedAt, model.createdAt);
    });

    test('legacy single image and ordered JSON array are supported', () {
      final single = ContentResponseModel.fromJson({
        ...fixture(),
        'imageUrl': 'https://cdn.test/a',
      });
      final multiple = ContentResponseModel.fromJson({
        ...fixture(),
        'imageUrl': '["https://cdn.test/b","javascript:bad","https://cdn.test/a"]',
      });
      expect(single.legacyImageUrls, ['https://cdn.test/a']);
      expect(multiple.legacyImageUrls, ['https://cdn.test/b', 'https://cdn.test/a']);
      expect(single.textContent, 'legacy');
    });

    test('ContentResponseModel parses creator, team, and multi-image array', () {
      final model = ContentResponseModel.fromJson({
        ...fixture(),
        'creatorId': 'user-123',
        'creatorName': 'Alex Author',
        'teamId': 'team-456',
        'richTextJson': '{"root":{}}',
        'richTextVersion': 2,
        'imageUrls': ['https://cdn.test/1.png', 'https://cdn.test/2.jpg'],
      });

      expect(model.creatorId, 'user-123');
      expect(model.creatorName, 'Alex Author');
      expect(model.teamId, 'team-456');
      expect(model.richTextJson, '{"root":{}}');
      expect(model.richTextVersion, 2);
      expect(model.imageUrls, ['https://cdn.test/1.png', 'https://cdn.test/2.jpg']);
      // legacyImageUrls prioritizes imageUrls over legacy imageUrl string
      expect(model.legacyImageUrls, ['https://cdn.test/1.png', 'https://cdn.test/2.jpg']);
    });

    test('CreateContentRequest serializes imageUrls, teamId, and adType', () {
      const request = CreateContentRequest(
        brandId: 'brand-1',
        textContent: 'Campaign text',
        adType: AdTypeEnum.imageText,
        title: 'Campaign 2026',
        imageUrls: ['https://cdn.test/banner.png', 'https://cdn.test/extra.png'],
        teamId: 'team-99',
        productId: 'prod-42',
      );

      final json = request.toJson();
      expect(json['imageUrls'], ['https://cdn.test/banner.png', 'https://cdn.test/extra.png']);
      expect(json['teamId'], 'team-99');
      expect(json['productId'], 'prod-42');
      expect(json['title'], 'Campaign 2026');
    });

    test('UpdateContentRequest serializes contextual and multi-image fields', () {
      const request = UpdateContentRequest(
        title: 'Updated Title',
        textContent: 'Updated text content',
        imageUrls: ['https://cdn.test/new.png'],
        productId: 'prod-100',
        adType: AdTypeEnum.videoText,
        styleDescription: 'Minimalist',
        contextDescription: 'Holiday sale',
        representativeCharacter: 'Character A',
      );

      final json = request.toJson();
      expect(json['title'], 'Updated Title');
      expect(json['textContent'], 'Updated text content');
      expect(json['imageUrls'], ['https://cdn.test/new.png']);
      expect(json['productId'], 'prod-100');
      expect(json['styleDescription'], 'Minimalist');
      expect(json['contextDescription'], 'Holiday sale');
      expect(json['representativeCharacter'], 'Character A');
    });

    test('ContentRepository.withdrawContent calls POST /Content/{id}/withdraw', () async {
      final mockDio = MockWithdrawDio();
      final repo = ContentRepository(mockDio);
      final result = await repo.withdrawContent('content-123');

      expect(result, isTrue);
      expect(mockDio.lastPath, '/Content/content-123/withdraw');
    });
  });
}

