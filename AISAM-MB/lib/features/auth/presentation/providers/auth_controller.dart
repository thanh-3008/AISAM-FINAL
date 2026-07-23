import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/state/base_state.dart';
import '../../../../core/errors/app_exception.dart';
import '../../data/models/auth_request.dart';
import '../../data/models/auth_response.dart';
import '../../data/repositories/auth_repository.dart';

part 'auth_controller.g.dart';

@riverpod
class AuthController extends _$AuthController {
  @override
  BaseState<AuthResponseModel> build() {
    return const BaseState.initial();
  }

  Future<void> login(String email, String password) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(authRepositoryProvider);
      final response = await repository.login(LoginRequest(email: email, password: password));
      state = BaseState.data(response);
    } on AppException catch (e) {
      state = BaseState.error(e);
    } catch (e) {
      state = BaseState.error(UnknownException(e.toString()));
    }
  }

  Future<void> register({
    required String email,
    required String password,
    required String confirmPassword,
    String? fullName,
  }) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(authRepositoryProvider);
      await repository.register(RegisterRequest(
        email: email,
        password: password,
        confirmPassword: confirmPassword,
        fullName: fullName,
      ));
      state = const BaseState.empty(); // Registration success, maybe redirect to login
    } on AppException catch (e) {
      state = BaseState.error(e);
    } catch (e) {
      state = BaseState.error(UnknownException(e.toString()));
    }
  }

  Future<void> forgotPassword(String email) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(authRepositoryProvider);
      await repository.forgotPassword(ForgotPasswordRequest(email: email));
      state = const BaseState.empty(); // Success
    } on AppException catch (e) {
      state = BaseState.error(e);
    } catch (e) {
      state = BaseState.error(UnknownException(e.toString()));
    }
  }

  Future<void> googleLogin(String idToken) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(authRepositoryProvider);
      final response = await repository.googleLogin(GoogleLoginRequest(idToken: idToken));
      state = BaseState.data(response);
    } on AppException catch (e) {
      state = BaseState.error(e);
    } catch (e) {
      state = BaseState.error(UnknownException(e.toString()));
    }
  }

  Future<void> logout() async {
    try {
      final repository = ref.read(authRepositoryProvider);
      await repository.logout();
    } finally {
      state = const BaseState.initial();
    }
  }
}
