import 'package:dio/dio.dart';
import '../../../../core/errors/app_exception.dart';

/// Workspace/auth headers are supplied by the shared authenticated Dio client.
/// Callers retain the same key across uncertain publish outcomes.
class PublishingRepository {
  final Dio dio;
  final bool Function()? isCurrent;
  PublishingRepository(this.dio, {this.isCurrent});

  Future<void> submit(String id) async {
    await _request('/content/$id/submit', method: 'POST');
  }

  Future<dynamic> _request(
    String path, {
    String method = 'GET',
    dynamic data,
    Map<String, dynamic>? query,
  }) async {
    try {
      if (isCurrent != null && !isCurrent!())
        throw StateError('Workspace changed. Reopen this screen.');
      final response = await dio.request(
        path,
        data: data,
        queryParameters: query,
        options: Options(method: method),
      );
      final body = response.data;
      if (isCurrent != null && !isCurrent!())
        throw StateError('Workspace changed. Reload this screen.');
      if (body is! Map || body['success'] != true) {
        throw StateError('Invalid publishing response');
      }
      return body['data'];
    } catch (error) {
      throw ExceptionHandler.handle(error);
    }
  }

  Future<List<bool>> contentPermissions(String id) async {
    final data = await _request(
      '/permissions/check',
      method: 'POST',
      data: [
        for (final permission in [2, 4, 5, 7])
          {'kind': 2, 'resourceId': id, 'permission': permission},
      ],
    );
    if (data is! List || data.length != 4 || data.any((v) => v is! bool)) {
      throw StateError('Invalid permission response');
    }
    return data.cast<bool>();
  }

  Future<Map<String, dynamic>> media(String id) async =>
      Map<String, dynamic>.from(await _request('/content/$id/media'));
  Future<void> importLegacy(String id, String version) async {
    await _request(
      '/content/$id/media/import-legacy',
      method: 'POST',
      data: {'expectedVersion': version},
    );
  }

  Future<Map<String, dynamic>> saveMedia(
    String id,
    String version,
    List<Map<String, dynamic>> items,
  ) async => Map<String, dynamic>.from(
    await _request(
      '/content/$id/media',
      method: 'PUT',
      data: {
        'expectedVersion': version,
        'items': [
          for (var i = 0; i < items.length; i++)
            {
              'assetId': items[i]['assetId'],
              'sortOrder': i,
              'isCover': items[i]['isCover'] == true,
              'altText': items[i]['altText'],
              'caption': items[i]['caption'],
            },
        ],
      },
    ),
  );
  Future<List<dynamic>> upload(String id, List<MultipartFile> files) async =>
      List<dynamic>.from(
        await _request(
          '/content/$id/media/upload',
          method: 'POST',
          data: FormData.fromMap({'files': files}),
        ),
      );
  Future<Map<String, dynamic>> preview(String id) async =>
      Map<String, dynamic>.from(await _request('/content/$id/publish-preview'));
  Future<List<dynamic>> publish(
    String id,
    String version,
    List<String> destinations,
    String key,
  ) async {
    final data = await _request(
      '/content/$id/publish-operations',
      method: 'POST',
      data: {
        'expectedVersion': version,
        'integrationIds': destinations,
        'idempotencyKey': key,
      },
    );
    return List<dynamic>.from(data['operations']);
  }

  Future<List<dynamic>> operations(String id, String key) async =>
      List<dynamic>.from(
        await _request('/content/$id/publish-operations', query: {'key': key}),
      );
}
