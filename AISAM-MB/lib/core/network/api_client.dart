import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../storage/secure_storage.dart';
import 'auth_interceptor.dart';
import '../config/env_config.dart';
import '../services/logger_service.dart';

part 'api_client.g.dart';

@riverpod
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
  dio.interceptors.add(AuthInterceptor(storage, dio));
  
  if (EnvConfig.isDebugMode) {
    dio.interceptors.add(LogInterceptor(
      requestBody: true,
      responseBody: true,
      logPrint: (obj) => LoggerService.d(obj.toString()),
    ));
  }

  return dio;
}
