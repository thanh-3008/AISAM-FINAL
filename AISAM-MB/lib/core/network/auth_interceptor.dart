import 'dart:async';
import 'package:dio/dio.dart';
import '../storage/secure_storage.dart';
import 'api_endpoints.dart';
import '../services/logger_service.dart';

class AuthInterceptor extends Interceptor {
  final SecureStorage _storage;
  final Dio _dio;
  final Dio? refreshClient;
  final void Function()? onSessionExpired;

  Completer<void>? _refreshCompleter;

  AuthInterceptor(
    this._storage,
    this._dio, {
    this.onSessionExpired,
    this.refreshClient,
  });

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    final options = response.requestOptions;
    if (!options.path.toLowerCase().contains('/auth/') &&
        (options.headers['X-Workspace-Id'] != _storage.cachedWorkspaceId ||
            options.headers['X-Profile-Id'] != _storage.cachedProfileId ||
            options.extra['aisamActor'] != _storage.cachedUserId)) {
      handler.reject(
        DioException(
          requestOptions: options,
          type: DioExceptionType.cancel,
          message: 'Workspace or account changed. Reload this screen.',
        ),
      );
      return;
    }
    handler.next(response);
  }

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    options.extra.putIfAbsent('aisamActor', () => _storage.cachedUserId);
    final accessToken = _storage.cachedAccessToken;
    if (accessToken != null) {
      options.headers['Authorization'] = 'Bearer $accessToken';
    }

    final isAuthEndpoint = options.path.toLowerCase().contains('/auth/');
    if (!isAuthEndpoint && options.extra['aisamRetried'] != true) {
      final workspaceId = _storage.cachedWorkspaceId;
      if (workspaceId != null) {
        options.headers['X-Workspace-Id'] = workspaceId;
      }

      final profileId = _storage.cachedProfileId;
      if (profileId != null) {
        options.headers['X-Profile-Id'] = profileId;
      }
    }

    handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401) {
      if (err.requestOptions.extra['aisamActor'] != _storage.cachedUserId) {
        return handler.next(err);
      }
      // Never loop on a rejected refreshed token or replay an auth request.
      if (err.requestOptions.extra['aisamRetried'] == true ||
          err.requestOptions.path.toLowerCase().contains('/auth/')) {
        return handler.next(err);
      }
      final refreshToken = await _storage.getRefreshToken();
      if (err.requestOptions.extra['aisamActor'] != _storage.cachedUserId) {
        return handler.next(err);
      }
      if (refreshToken == null) {
        LoggerService.w('Refresh token not found. Clearing storage.');
        await _storage.clearAll();
        onSessionExpired?.call();
        return handler.next(err);
      }

      if (_refreshCompleter == null) {
        _refreshCompleter = Completer<void>();
        try {
          LoggerService.i('Attempting to refresh token...');
          final refreshDio =
              refreshClient ??
              Dio(
                BaseOptions(
                  baseUrl: _dio.options.baseUrl,
                  connectTimeout: _dio.options.connectTimeout,
                  receiveTimeout: _dio.options.receiveTimeout,
                ),
              );
          final refreshPath = ApiEndpoints.refresh.startsWith('/')
              ? '/api${ApiEndpoints.refresh}'
              : '/api/${ApiEndpoints.refresh}';
          final response = await refreshDio.post(
            refreshPath,
            data: {'refreshToken': refreshToken},
          );

          if (err.requestOptions.extra['aisamActor'] != _storage.cachedUserId ||
              refreshToken != await _storage.getRefreshToken()) {
            throw StateError('Session changed while refreshing.');
          }

          if (response.statusCode == 200 && response.data['success'] == true) {
            final newAccessToken = response.data['data']['accessToken'];
            final newRefreshToken = response.data['data']['refreshToken'];
            await _storage.saveTokens(
              accessToken: newAccessToken,
              refreshToken: newRefreshToken,
            );
            LoggerService.i('Token refreshed successfully.');
            final completer = _refreshCompleter;
            _refreshCompleter = null;
            completer?.complete();
          } else {
            throw Exception('Refresh API failed with non-success status.');
          }
        } catch (e) {
          LoggerService.e('Token refresh failed: $e');
          if (err.requestOptions.extra['aisamActor'] == _storage.cachedUserId &&
              refreshToken == await _storage.getRefreshToken()) {
            await _storage.clearAll();
            onSessionExpired?.call();
          }
          final completer = _refreshCompleter;
          _refreshCompleter = null;
          // Waiters retry only after checking the current stored token below.
          completer?.complete();
          return handler.next(err);
        }
      } else {
        try {
          LoggerService.i('Waiting for ongoing token refresh...');
          await _refreshCompleter!.future;
        } catch (_) {
          return handler.next(err);
        }
      }

      try {
        final newAccessToken = await _storage.getAccessToken();
        if (newAccessToken == null ||
            err.requestOptions.extra['aisamActor'] != _storage.cachedUserId ||
            err.requestOptions.headers['X-Workspace-Id'] !=
                _storage.cachedWorkspaceId ||
            err.requestOptions.headers['X-Profile-Id'] !=
                _storage.cachedProfileId) {
          return handler.next(err);
        }
        // Multipart streams cannot safely be sent twice; let the caller retry explicitly.
        if (err.requestOptions.data is FormData) return handler.next(err);
        err.requestOptions.extra['aisamRetried'] = true;
        err.requestOptions.headers['Authorization'] = 'Bearer $newAccessToken';

        final cloneReq = await _dio.fetch(err.requestOptions);
        return handler.resolve(cloneReq);
      } on DioException catch (e) {
        return handler.next(e);
      } catch (e) {
        return handler.next(err);
      }
    }

    handler.next(err);
  }
}
