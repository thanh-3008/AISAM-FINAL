import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:dio/dio.dart';
import '../data/auth_repository.dart';
import '../../../core/storage/secure_storage.dart';
import '../../../app/router.dart';

part 'auth_controller.g.dart';

@riverpod
class AuthController extends _$AuthController {
  @override
  FutureOr<void> build() {}

  Future<void> login(String email, String password) async {
    state = const AsyncValue.loading();
    try {
      final repo = ref.read(authRepositoryProvider);
      final response = await repo.login({
        'email': email,
        'password': password,
      });

      if (response.success && response.data != null) {
        final storage = ref.read(secureStorageProvider);
        await storage.saveTokens(
          accessToken: response.data!['accessToken'],
          refreshToken: response.data!['refreshToken'],
        );
        
        // redirect
        ref.read(routerProvider).go('/dashboard');
        state = const AsyncValue.data(null);
      } else {
        state = AsyncValue.error(response.message ?? 'Login failed', StackTrace.current);
      }
    } catch (e, st) {
      if (e is DioException && e.response?.data != null) {
        final message = e.response?.data['message'] ?? e.message;
        state = AsyncValue.error(message, st);
      } else {
        state = AsyncValue.error(e, st);
      }
    }
  }
}
