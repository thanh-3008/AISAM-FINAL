import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../core/errors/generic_response.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_endpoints.dart';

part 'auth_repository.g.dart';

class AuthRepository {
  final Dio _dio;

  AuthRepository(this._dio);

  Future<GenericResponse<Map<String, dynamic>>> login(Map<String, dynamic> body) async {
    final response = await _dio.post(ApiEndpoints.login, data: body);
    return GenericResponse.fromJson(response.data, (json) => json as Map<String, dynamic>);
  }

  Future<GenericResponse<Map<String, dynamic>>> register(Map<String, dynamic> body) async {
    final response = await _dio.post(ApiEndpoints.register, data: body);
    return GenericResponse.fromJson(response.data, (json) => json as Map<String, dynamic>);
  }

  Future<GenericResponse<Map<String, dynamic>>> googleLogin(Map<String, dynamic> body) async {
    final response = await _dio.post(ApiEndpoints.googleLogin, data: body);
    return GenericResponse.fromJson(response.data, (json) => json as Map<String, dynamic>);
  }
}

@riverpod
AuthRepository authRepository(AuthRepositoryRef ref) {
  final dio = ref.watch(dioProvider);
  return AuthRepository(dio);
}
