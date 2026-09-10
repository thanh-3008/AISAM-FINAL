import 'dart:convert';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/features/content/data/repositories/publishing_repository.dart';
import 'package:aisam_mb/features/approval/data/repositories/approval_repository.dart';
import 'package:aisam_mb/core/errors/app_exception.dart';

class Adapter implements HttpClientAdapter {
  final ResponseBody Function(RequestOptions) respond;
  Adapter(this.respond);
  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async => respond(options);
  @override
  void close({bool force = false}) {}
}

ResponseBody jsonBody(Object data, {int status = 200}) =>
    ResponseBody.fromString(
      jsonEncode(data),
      status,
      headers: {
        Headers.contentTypeHeader: ['application/json'],
      },
    );
void main() {
  test(
    'permission decisions fail closed on malformed or missing entries',
    () async {
      final dio = Dio()
        ..httpClientAdapter = Adapter(
          (r) => jsonBody({
            'success': true,
            'data': [true],
          }),
        );
      await expectLater(
        PublishingRepository(dio).contentPermissions('content'),
        throwsStateError,
      );
    },
  );
  test(
    'media save serializes only writable fields with contiguous order',
    () async {
      final dio = Dio()
        ..httpClientAdapter = Adapter((r) {
          expect(r.method, 'PUT');
          expect(r.data['expectedVersion'], 'version');
          expect(r.data['items'], [
            {
              'assetId': 'asset',
              'sortOrder': 0,
              'isCover': true,
              'altText': null,
              'caption': null,
            },
          ]);
          return jsonBody({
            'success': true,
            'data': {'version': 'next', 'items': []},
          });
        });
      await PublishingRepository(dio).saveMedia('content', 'version', [
        {
          'assetId': 'asset',
          'isCover': true,
          'url': 'untrusted',
          'workspaceId': 'other',
          'sortOrder': 99,
        },
      ]);
    },
  );
  test(
    'publish keeps explicit idempotency key and reads journal without replay',
    () async {
      final requests = <RequestOptions>[];
      final dio = Dio()
        ..httpClientAdapter = Adapter((r) {
          requests.add(r);
          return jsonBody({
            'success': true,
            'data': r.method == 'POST' ? {'operations': []} : [],
          });
        });
      final repo = PublishingRepository(dio);
      await repo.publish('content', 'version', ['channel'], 'fixed-key');
      await repo.operations('content', 'fixed-key');
      expect(requests[0].data['idempotencyKey'], 'fixed-key');
      expect(requests[1].method, 'GET');
      expect(requests[1].queryParameters['key'], 'fixed-key');
    },
  );
  test('stale workspace never sends a publishing request', () async {
    var calls = 0;
    final dio = Dio()
      ..httpClientAdapter = Adapter((r) {
        calls++;
        return jsonBody({});
      });
    await expectLater(
      PublishingRepository(
        dio,
        isCurrent: () => false,
      ).publish('id', 'v', [], 'key'),
      throwsA(isA<AppException>()),
    );
    expect(calls, 0);
  });
  test(
    'conflict and hidden resource errors preserve backend error codes',
    () async {
      for (final status in [403, 404, 409]) {
        final dio = Dio()
          ..httpClientAdapter = Adapter(
            (r) => jsonBody({
              'success': false,
              'errorCode': 'RESOURCE_ACCESS_DENIED',
            }, status: status),
          );
        await expectLater(
          PublishingRepository(dio).media('id'),
          throwsA(
            isA<AppException>().having(
              (e) => e.code,
              'code',
              'RESOURCE_ACCESS_DENIED',
            ),
          ),
        );
      }
    },
  );
  test('review uses dedicated endpoint rather than status mutation', () async {
    String? path;
    String? method;
    final dio = Dio()
      ..httpClientAdapter = Adapter((r) {
        path = r.path;
        method = r.method;
        return jsonBody({'success': false}, status: 403);
      });
    await expectLater(
      ApprovalRepository(dio).approveContent('id'),
      throwsA(isA<AppException>()),
    );
    expect(path, '/content/id/approve');
    expect(method, 'POST');
  });
}
