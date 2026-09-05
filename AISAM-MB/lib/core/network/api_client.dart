import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../storage/secure_storage.dart';
import 'auth_interceptor.dart';
import 'access_events.dart';
import '../config/env_config.dart';
import '../services/logger_service.dart';
import '../../app/router.dart';

part 'api_client.g.dart';

@Riverpod(keepAlive: true)
Dio dio(DioRef ref) {
  final dio = Dio(BaseOptions(
    baseUrl: EnvConfig.apiBaseUrl.replaceAll(RegExp(r'/api/?$'), ''),
    connectTimeout: Duration(milliseconds: EnvConfig.connectTimeoutMs),
    receiveTimeout: Duration(milliseconds: EnvConfig.receiveTimeoutMs),
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    },
  ));

  dio.interceptors.add(InterceptorsWrapper(
    onRequest: (options, handler) {
      if (!options.path.startsWith('/api/') && !options.path.startsWith('api/')) {
        options.path = options.path.startsWith('/') ? '/api${options.path}' : '/api/${options.path}';
      }
      handler.next(options);
    },
  ));

  final storage = ref.watch(secureStorageProvider);
  dio.interceptors.add(InterceptorsWrapper(
    onError: (error, handler) {
      if (error.response?.statusCode == 403) {
        ref.read(accessDeniedProvider.notifier).state = true;
        if (!error.requestOptions.path.endsWith('/access/context')) {
          ref.read(accessRevisionProvider.notifier).state++;
        }
      }
      handler.next(error);
    },
    onResponse: (response, handler) {
      final path = response.requestOptions.path;
      if (response.requestOptions.method != 'GET' &&
          (path.contains('/teams') || path.contains('/workspace-members') || path.contains('/collaboration-tasks'))) {
        ref.read(accessRevisionProvider.notifier).state++;
      }
      handler.next(response);
    },
  ));
  dio.interceptors.add(AuthInterceptor(
    storage, 
    dio,
    onSessionExpired: () {
      try {
        ref.read(routerProvider).go('/login');
      } catch (e) {
        LoggerService.e('Navigation to login failed: $e');
      }
    },
  ));
  
  if (EnvConfig.isDebugMode) {
    dio.interceptors.add(InterceptorsWrapper(
      onResponse: (response, handler) {
        LoggerService.d('HTTP ${response.requestOptions.method}: ${response.statusCode}');
        handler.next(response);
      },
      onError: (error, handler) {
        LoggerService.w('HTTP ${error.requestOptions.method}: ${error.response?.statusCode ?? "network error"}');
        handler.next(error);
      },
    ));
  }

  return dio;
}
