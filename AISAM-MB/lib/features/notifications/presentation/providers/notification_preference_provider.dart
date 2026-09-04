import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../domain/notification_preference_model.dart';
import '../../data/local_preference_repository.dart';

part 'notification_preference_provider.g.dart';

@riverpod
class NotificationPreferenceState extends _$NotificationPreferenceState {
  @override
  FutureOr<List<NotificationPreferenceModel>> build() async {
    return _fetchPreferences();
  }

  Future<List<NotificationPreferenceModel>> _fetchPreferences() async {
    final repo = await ref.read(localPreferenceRepositoryProvider.future);
    return repo.getPreferences();
  }

  Future<void> togglePreference(int type, bool isEnabled) async {
    final repo = await ref.read(localPreferenceRepositoryProvider.future);
    final previousState = state.valueOrNull ?? [];
    
    // Optimistic update
    final updatedPrefs = [...previousState];
    final index = updatedPrefs.indexWhere((p) => p.notificationType == type);
    
    if (index >= 0) {
      updatedPrefs[index] = updatedPrefs[index].copyWith(isEnabled: isEnabled);
    } else {
      updatedPrefs.add(NotificationPreferenceModel(notificationType: type, isEnabled: isEnabled));
    }
    
    state = AsyncData(updatedPrefs);

    try {
      await repo.savePreferences(updatedPrefs);
    } catch (e, st) {
      // Revert on failure
      state = AsyncData(previousState);
      state = AsyncError(e, st);
    }
  }
}

@riverpod
class MasterPushEnabledState extends _$MasterPushEnabledState {
  @override
  FutureOr<bool> build() async {
    final repo = await ref.read(localPreferenceRepositoryProvider.future);
    return repo.getMasterPushEnabled();
  }

  Future<void> toggle(bool isEnabled) async {
    final repo = await ref.read(localPreferenceRepositoryProvider.future);
    final previousState = state.valueOrNull ?? true;
    
    state = AsyncData(isEnabled);

    try {
      await repo.saveMasterPushEnabled(isEnabled);
    } catch (e, st) {
      state = AsyncData(previousState);
      state = AsyncError(e, st);
    }
  }
}
