import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

final languageControllerProvider = StateNotifierProvider<LanguageController, AsyncValue<String>>((ref) {
  return LanguageController();
});

class LanguageController extends StateNotifier<AsyncValue<String>> {
  LanguageController() : super(const AsyncValue.loading()) {
    _init();
  }

  Future<void> _init() async {
    final prefs = await SharedPreferences.getInstance();
    final lang = prefs.getString('app_language') ?? 'vi';
    if (mounted) state = AsyncValue.data(lang);
  }

  Future<void> setLanguage(String langCode) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('app_language', langCode);
    if (mounted) state = AsyncValue.data(langCode);
  }
}
