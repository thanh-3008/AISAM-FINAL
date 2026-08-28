import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/storage/secure_storage.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/auth_request.dart';
import '../models/auth_response.dart';
import '../../../../core/network/api_endpoints.dart';

part 'auth_repository.g.dart';

class AuthRepository {
  final Dio _dio;
  final SecureStorage _storage;

  AuthRepository(this._dio, this._storage);

  Future<AuthResponseModel> login(LoginRequest request) async {
    try {
      final response = await _dio.post(
        ApiEndpoints.login,
        data: request.toJson(),
      );
      final authResponse = AuthResponseModel.fromJson(response.data['data']);
      await _storage.saveTokens(
        accessToken: authResponse.accessToken,
        refreshToken: authResponse.refreshToken,
      );
      await _storage.saveUserId(authResponse.user.id);
      return authResponse;
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> register(RegisterRequest request) async {
    try {
      await _dio.post(
        ApiEndpoints.register,
        data: request.toJson(),
      );
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> forgotPassword(ForgotPasswordRequest request) async {
    try {
      await _dio.post(
        ApiEndpoints.forgotPassword,
        data: request.toJson(),
      );
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<AuthResponseModel> googleLogin(GoogleLoginRequest request) async {
    try {
      final response = await _dio.post(
        ApiEndpoints.googleLogin,
        data: request.toJson(),
      );
      final authResponse = AuthResponseModel.fromJson(response.data['data']);
      await _storage.saveTokens(
        accessToken: authResponse.accessToken,
        refreshToken: authResponse.refreshToken,
      );
      await _storage.saveUserId(authResponse.user.id);
      return authResponse;
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> logout() async {
    try {
      final refreshToken = await _storage.getRefreshToken();
      if (refreshToken != null) {
        await _dio.post(
          ApiEndpoints.logout,
          data: {'refreshToken': refreshToken},
        );
      }
    } catch (e) {
      // Ignore network errors on logout
    } finally {
      await _storage.clearAll();
    }
  }
}

@riverpod
AuthRepository authRepository(AuthRepositoryRef ref) {
  return AuthRepository(
    ref.read(dioProvider),
    ref.read(secureStorageProvider),
  );
}
