import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/storage/secure_storage.dart';
import '../../data/repositories/profile_repository.dart';
import '../../data/models/profile_model.dart';
import '../../data/models/profile_request.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';
import '../../../workspace/presentation/providers/workspace_controller.dart';

part 'profile_controller.g.dart';

@riverpod
class ProfileController extends _$ProfileController {
  @override
  AsyncValue<List<ProfileResponseModel>> build() {
    ref.watch(activeWorkspaceControllerProvider);
    _fetchProfiles();
    return const AsyncValue.loading();
  }

  Future<void> _fetchProfiles() async {
    try {
      state = const AsyncValue.loading();
      final repository = ref.read(profileRepositoryProvider);
      final profiles = await repository.getProfiles();
      state = AsyncValue.data(profiles);
    } catch (e, st) {
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refreshProfiles() async {
    await _fetchProfiles();
  }

  Future<bool> selectProfile(String profileId) async {
    try {
      final storage = ref.read(secureStorageProvider);
      await storage.saveActiveProfileId(profileId);
      return true;
    } catch (e) {
      return false;
    }
  }
}

@riverpod
class CreateProfileController extends _$CreateProfileController {
  @override
  BaseState<ProfileResponseModel> build() {
    return const BaseState.initial();
  }

  Future<void> createProfile(CreateProfileRequest request) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(profileRepositoryProvider);
      final profile = await repository.createProfile(request);
      state = BaseState.data(profile);
      ref.read(profileControllerProvider.notifier).refreshProfiles();
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }
}
