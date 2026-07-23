import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

class LanguageScreen extends ConsumerStatefulWidget {
  const LanguageScreen({super.key});

  @override
  ConsumerState<LanguageScreen> createState() => _LanguageScreenState();
}

class _LanguageScreenState extends ConsumerState<LanguageScreen> {
  String _selectedLanguage = 'vi';

  @override
  void initState() {
    super.initState();
    _loadSavedLanguage();
  }

  Future<void> _loadSavedLanguage() async {
    final prefs = await SharedPreferences.getInstance();
    final saved = prefs.getString('app_language') ?? 'vi';
    if (mounted) setState(() => _selectedLanguage = saved);
  }

  Future<void> _onLanguageSelected(String langCode) async {
    if (_selectedLanguage == langCode) return;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('app_language', langCode);
    if (!mounted) return;
    setState(() => _selectedLanguage = langCode);
    _showRestartDialog();
  }

  void _showRestartDialog() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Thay đổi ngôn ngữ'),
        content: const Text('Thay đổi ngôn ngữ sẽ có hiệu lực khi bạn khởi động lại ứng dụng.'),
        actions: [
          ElevatedButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('Đã hiểu'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      appBar: AppBar(
        title: const Text('Ngôn ngữ'),
        backgroundColor: Theme.of(context).colorScheme.surface.withValues(alpha: 0.8),
        elevation: 0,
        scrolledUnderElevation: 0,
      ),
      body: ListView(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        children: [
          Container(
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.surface,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withValues(alpha: 0.3)),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.04),
                  blurRadius: 24,
                  offset: const Offset(0, 8),
                ),
              ],
            ),
            clipBehavior: Clip.antiAlias,
            child: Column(
              children: [
                _buildLanguageTile(context: context, title: 'Tiếng Việt', subtitle: 'Vietnamese', value: 'vi'),
                Divider(height: 1, thickness: 1, color: Theme.of(context).colorScheme.outlineVariant.withValues(alpha: 0.3)),
                _buildLanguageTile(context: context, title: 'English', subtitle: 'Tiếng Anh', value: 'en'),
              ],
            ),
          ),
          const SizedBox(height: 16),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 8.0),
            child: Text(
              'Thay đổi ngôn ngữ sẽ có hiệu lực sau khi khởi động lại ứng dụng.',
              style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLanguageTile({
    required BuildContext context,
    required String title,
    required String subtitle,
    required String value,
  }) {
    final isSelected = _selectedLanguage == value;
    return ListTile(
      title: Text(title, style: Theme.of(context).textTheme.bodyLarge?.copyWith(fontWeight: isSelected ? FontWeight.bold : FontWeight.normal)),
      subtitle: Text(subtitle, style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant)),
      trailing: isSelected
          ? Icon(Icons.check_circle, color: Theme.of(context).colorScheme.primary)
          : const Icon(Icons.radio_button_unchecked, color: Colors.grey),
      onTap: () => _onLanguageSelected(value),
    );
  }
}
