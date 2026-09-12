import 'dart:async';
import 'dart:convert';
import 'dart:typed_data';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/core/network/auth_interceptor.dart';
import 'package:aisam_mb/core/storage/secure_storage.dart';

class AsyncAdapter implements HttpClientAdapter {
  final Future<ResponseBody> Function(RequestOptions) respond;
  AsyncAdapter(this.respond);
  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? stream,
    Future<void>? cancel,
  ) => respond(options);
  @override
  void close({bool force = false}) {}
}

ResponseBody body(int status, [Object? data]) => ResponseBody.fromString(
  jsonEncode(data ?? {}),
  status,
  headers: {
    Headers.contentTypeHeader: ['application/json'],
  },
);

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  late SecureStorage storage;
  late Dio dio;
  late Dio refresh;
  setUp(() async {
    FlutterSecureStorage.setMockInitialValues({});
    storage = SecureStorage(const FlutterSecureStorage());
    await storage.initCache();
    await storage.saveUserId('actor-a');
    await storage.saveActiveWorkspaceId('workspace-a');
    await storage.saveActiveProfileId('profile-a');
    await storage.saveTokens(accessToken: 'old', refreshToken: 'refresh-a');
    dio = Dio(BaseOptions(baseUrl: 'https://test.invalid'));
    refresh = Dio(BaseOptions(baseUrl: 'https://test.invalid'));
    dio.interceptors.add(AuthInterceptor(storage, dio, refreshClient: refresh));
  });
  tearDown(() {
    dio.close(force: true);
    refresh.close(force: true);
  });

  test('drops response received after workspace switch', () async {
    dio.httpClientAdapter = AsyncAdapter((request) async {
      expect(request.headers['X-Workspace-Id'], 'workspace-a');
      await storage.saveActiveWorkspaceId('workspace-b');
      return body(200, {'private': 'workspace-a'});
    });
    await expectLater(
      dio.get('/api/content'),
      throwsA(
        isA<DioException>().having(
          (e) => e.type,
          'cancel',
          DioExceptionType.cancel,
        ),
      ),
    );
  });

  test('concurrent 401 requests share one refresh', () async {
    var refreshCalls = 0;
    var requests = 0;
    final bothSent = Completer<void>();
    dio.httpClientAdapter = AsyncAdapter((request) async {
      requests++;
      if (request.headers['Authorization'] == 'Bearer old') {
        if (requests == 2) bothSent.complete();
        return body(401);
      }
      expect(request.headers['Authorization'], 'Bearer new');
      return body(200);
    });
    refresh.httpClientAdapter = AsyncAdapter((request) async {
      refreshCalls++;
      await bothSent.future;
      await Future<void>.delayed(const Duration(milliseconds: 20));
      return body(200, {
        'success': true,
        'data': {'accessToken': 'new', 'refreshToken': 'refresh-new'},
      });
    });
    final results = await Future.wait([
      dio.get('/api/content'),
      dio.get('/api/brands'),
    ]);
    expect(results.map((e) => e.statusCode), [200, 200]);
    expect(refreshCalls, 1);
    expect(requests, 4);
  });

  test('refresh cannot overwrite a newly signed in account', () async {
    dio.httpClientAdapter = AsyncAdapter((_) async => body(401));
    refresh.httpClientAdapter = AsyncAdapter((_) async {
      await storage.saveUserId('actor-b');
      await storage.saveTokens(
        accessToken: 'b-token',
        refreshToken: 'b-refresh',
      );
      return body(200, {
        'success': true,
        'data': {
          'accessToken': 'stale-a-token',
          'refreshToken': 'stale-a-refresh',
        },
      });
    });
    await expectLater(dio.get('/api/content'), throwsA(isA<DioException>()));
    expect(storage.cachedUserId, 'actor-b');
    expect(storage.cachedAccessToken, 'b-token');
    expect(storage.cachedRefreshToken, 'b-refresh');
  });

  test('workspace change during refresh prevents request replay', () async {
    var calls = 0;
    dio.httpClientAdapter = AsyncAdapter((_) async {
      calls++;
      return body(401);
    });
    refresh.httpClientAdapter = AsyncAdapter((_) async {
      await storage.saveActiveWorkspaceId('workspace-b');
      return body(200, {
        'success': true,
        'data': {'accessToken': 'new', 'refreshToken': 'refresh-new'},
      });
    });
    await expectLater(dio.get('/api/content'), throwsA(isA<DioException>()));
    expect(calls, 1);
    expect(storage.cachedWorkspaceId, 'workspace-b');
  });

  test('second 401 terminates without refresh loop', () async {
    var calls = 0;
    var refreshCalls = 0;
    dio.httpClientAdapter = AsyncAdapter((_) async {
      calls++;
      return body(401);
    });
    refresh.httpClientAdapter = AsyncAdapter((_) async {
      refreshCalls++;
      return body(200, {
        'success': true,
        'data': {'accessToken': 'new', 'refreshToken': 'refresh-new'},
      });
    });
    await expectLater(dio.get('/api/content'), throwsA(isA<DioException>()));
    expect(calls, 2);
    expect(refreshCalls, 1);
  });

  test('multipart is never automatically sent twice', () async {
    var calls = 0;
    dio.httpClientAdapter = AsyncAdapter((_) async {
      calls++;
      return body(401);
    });
    refresh.httpClientAdapter = AsyncAdapter(
      (_) async => body(200, {
        'success': true,
        'data': {'accessToken': 'new', 'refreshToken': 'refresh-new'},
      }),
    );
    await expectLater(
      dio.post(
        '/api/content/id/media/upload',
        data: FormData.fromMap({
          'files': MultipartFile.fromBytes([1, 2, 3], filename: 'image.png'),
        }),
      ),
      throwsA(isA<DioException>()),
    );
    expect(calls, 1);
  });
}
