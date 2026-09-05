import 'dart:async';
import 'package:dio/dio.dart';
import '../storage/secure_storage.dart';
import 'api_endpoints.dart';
import '../services/logger_service.dart';

class AuthInterceptor extends Interceptor {
  final SecureStorage _storage;
  final Dio _dio;
  final void Function()? onSessionExpired;
  
  Completer<void>? _refreshCompleter;

  AuthInterceptor(this._storage, this._dio, {this.onSessionExpired});

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    final accessToken = _storage.cachedAccessToken;
    if (accessToken != null) {
      options.headers['Authorization'] = 'Bearer $accessToken';
    }

    final isAuthEndpoint = options.path.contains('/Auth/');
    if (!isAuthEndpoint) {
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
      final refreshToken = await _storage.getRefreshToken();
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
          final refreshDio = Dio(BaseOptions(baseUrl: _dio.options.baseUrl));
          final refreshPath = ApiEndpoints.refresh.startsWith('/') 
              ? '/api${ApiEndpoints.refresh}' 
              : '/api/${ApiEndpoints.refresh}';
          final response = await refreshDio.post(refreshPath, data: {
            'refreshToken': refreshToken,
          });

          if (response.statusCode == 200 && response.data['success'] == true) {
            final newAccessToken = response.data['data']['accessToken'];
            final newRefreshToken = response.data['data']['refreshToken'];
            await _storage.saveTokens(accessToken: newAccessToken, refreshToken: newRefreshToken);
            LoggerService.i('Token refreshed successfully.');
            final completer = _refreshCompleter;
            _refreshCompleter = null;
            completer?.complete();
          } else {
            throw Exception('Refresh API failed with non-success status.');
          }
        } catch (e) {
          LoggerService.e('Token refresh failed.');
          await _storage.clearAll();
          onSessionExpired?.call();
          final completer = _refreshCompleter;
          _refreshCompleter = null;
          completer?.completeError(e);
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
        err.requestOptions.headers['Authorization'] = 'Bearer $newAccessToken';
        
        final isAuthEndpoint = err.requestOptions.path.contains('/Auth/');
        if (!isAuthEndpoint) {
          final workspaceId = await _storage.getActiveWorkspaceId();
          if (workspaceId != null) {
            err.requestOptions.headers['X-Workspace-Id'] = workspaceId;
          }
          final profileId = await _storage.getActiveProfileId();
          if (profileId != null) {
            err.requestOptions.headers['X-Profile-Id'] = profileId;
          }
        }

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
