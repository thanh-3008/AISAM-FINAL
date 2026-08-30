import 'package:shared_preferences/shared_preferences.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../domain/notification_preference_model.dart';
import 'dart:convert';

part 'local_preference_repository.g.dart';

class LocalPreferenceRepository {
  final SharedPreferences _prefs;
  static const String _prefKey = 'notification_preferences_v2';
  static const String _masterPushKey = 'master_push_enabled';

  LocalPreferenceRepository(this._prefs);

  bool getMasterPushEnabled() {
    return _prefs.getBool(_masterPushKey) ?? true;
  }

  Future<void> saveMasterPushEnabled(bool isEnabled) async {
    await _prefs.setBool(_masterPushKey, isEnabled);
  }

  List<NotificationPreferenceModel> getPreferences() {
    final String? jsonString = _prefs.getString(_prefKey);
    if (jsonString == null) {
      // Default: everything enabled
      return [
        const NotificationPreferenceModel(notificationType: 0, isEnabled: true), // ApprovalNeeded
        const NotificationPreferenceModel(notificationType: 1, isEnabled: true), // PostScheduled
        const NotificationPreferenceModel(notificationType: 2, isEnabled: true), // PerformanceAlert
        const NotificationPreferenceModel(notificationType: 3, isEnabled: true), // AiSuggestion
        const NotificationPreferenceModel(notificationType: 4, isEnabled: true), // SystemUpdate
      ];
    }
    
    final List<dynamic> jsonList = jsonDecode(jsonString);
    return jsonList.map((e) => NotificationPreferenceModel.fromJson(e)).toList();
  }

  Future<void> savePreferences(List<NotificationPreferenceModel> preferences) async {
    final jsonList = preferences.map((e) => e.toJson()).toList();
    final jsonString = jsonEncode(jsonList);
    await _prefs.setString(_prefKey, jsonString);
  }
}

@riverpod
Future<SharedPreferences> sharedPreferences(SharedPreferencesRef ref) async {
  return SharedPreferences.getInstance();
}

@riverpod
Future<LocalPreferenceRepository> localPreferenceRepository(LocalPreferenceRepositoryRef ref) async {
  final prefs = await ref.watch(sharedPreferencesProvider.future);
  return LocalPreferenceRepository(prefs);
}
