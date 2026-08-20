import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../workspace/presentation/providers/workspace_controller.dart';
import '../../auth/presentation/providers/auth_controller.dart';
import '../../../core/shared/aisam_logo_widget.dart';

class SettingsScreen extends ConsumerStatefulWidget {
  const SettingsScreen({super.key});

  @override
  ConsumerState<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends ConsumerState<SettingsScreen> {
  bool _automationEnabled = false;

  @override
  Widget build(BuildContext context) {
    final activeWorkspaceAsync = ref.watch(activeWorkspaceControllerProvider);

    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.surface.withOpacity(0.8),
        elevation: 0,
        scrolledUnderElevation: 0,
        title: const AisamLogoWidget(),
        actions: [
          IconButton(
            icon: const Icon(Icons.expand_circle_down),
            color: Theme.of(context).colorScheme.primary,
            onPressed: () {
              // Action
            },
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        children: [
          Text(
            'Cài đặt',
            style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: Theme.of(context).colorScheme.onSurface,
                ),
          ),
          const SizedBox(height: 24),

          // 4.1 Nhóm Cá nhân
          _buildSectionHeader('Cá nhân', context),
          _buildSectionCard(
            context,
            children: [
              _buildListTile(
                context: context,
                icon: Icons.person_outline,
                title: 'Tài khoản',
                onTap: () => context.push('/settings/account'),
              ),
              _buildDivider(context),
              _buildListTile(
                context: context,
                icon: Icons.notifications_none,
                title: 'Thông báo',
                onTap: () => context.push('/settings/notifications'),
              ),
              _buildDivider(context),
              _buildListTile(
                context: context,
                icon: Icons.language,
                title: 'Ngôn ngữ',
                trailing: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text('Tiếng Việt', style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant)),
                    const SizedBox(width: 4),
                    Icon(Icons.chevron_right, color: Theme.of(context).colorScheme.onSurfaceVariant, size: 20),
                  ],
                ),
                onTap: () => context.push('/settings/language'),
              ),
            ],
          ),
          const SizedBox(height: 24),

          // 4.2 Nhóm Workspace
          _buildSectionHeader('Workspace hiện tại', context),
          _buildSectionCard(
            context,
            children: [
              activeWorkspaceAsync.when(
                data: (workspace) {
                  return _buildListTile(
                    context: context,
                    icon: Icons.workspaces_outline,
                    title: 'Thông tin & Chuyển đổi',
                    subtitle: workspace?.name ?? 'Chưa chọn Workspace',
                    onTap: () => context.push('/overview'),
                  );
                },
                loading: () => const Padding(padding: EdgeInsets.all(16.0), child: Center(child: CircularProgressIndicator())),
                error: (e, st) => Padding(padding: const EdgeInsets.all(16.0), child: Text('Lỗi: $e')),
              ),
              _buildDivider(context),
              _buildListTile(
                context: context,
                icon: Icons.group_outlined,
                title: 'Team (Members/Roles)',
                onTap: () => context.push('/settings/team'),
              ),
              _buildDivider(context),
              _buildListTile(
                context: context,
                icon: Icons.link,
                title: 'Kết nối mạng xã hội',
                subtitle: 'Facebook, TikTok, Instagram...',
                onTap: () => context.push('/settings/social'),
              ),
            ],
          ),
          const SizedBox(height: 24),

          // 4.3 Nhóm Nội dung & vận hành
          _buildSectionHeader('Nội dung & vận hành', context),
          _buildSectionCard(
            context,
            children: [
              _buildListTile(
                context: context,
                icon: Icons.category_outlined,
                title: 'Brands & Products',
                onTap: () => context.push('/brands'),
              ),
              _buildDivider(context),
              _buildSwitchTile(
                context: context,
                icon: Icons.smart_toy_outlined,
                title: 'Automation',
                subtitle: 'Auto-reply, Tagging',
                value: _automationEnabled,
                onChanged: (val) => setState(() => _automationEnabled = val),
              ),
            ],
          ),
          const SizedBox(height: 24),

          // 4.4 Nhóm Tài khoản
          _buildSectionHeader('Tài khoản & hệ thống', context),
          _buildSectionCard(
            context,
            children: [
              _buildListTile(
                context: context,
                icon: Icons.credit_card,
                title: 'Billing & Credit',
                onTap: () => context.push('/settings/billing'),
              ),
              _buildDivider(context),
              _buildListTile(
                context: context,
                icon: Icons.help_outline,
                title: 'Trợ giúp',
                onTap: () => _showComingSoon(context),
              ),
              _buildDivider(context),
              ListTile(
                leading: Icon(Icons.logout, color: Theme.of(context).colorScheme.error),
                title: Text('Đăng xuất', style: Theme.of(context).textTheme.bodyLarge?.copyWith(color: Theme.of(context).colorScheme.error, fontWeight: FontWeight.bold)),
                tileColor: Colors.transparent,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                onTap: () async {
                  final confirm = await showDialog<bool>(
                    context: context,
                    builder: (context) => AlertDialog(
                      title: const Text('Xác nhận đăng xuất'),
                      content: const Text('Bạn có chắc chắn muốn đăng xuất không?'),
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.of(context).pop(false),
                          child: const Text('Hủy'),
                        ),
                        ElevatedButton(
                          onPressed: () => Navigator.of(context).pop(true),
                          style: ElevatedButton.styleFrom(backgroundColor: Theme.of(context).colorScheme.error, foregroundColor: Theme.of(context).colorScheme.onError),
                          child: const Text('Đăng xuất'),
                        ),
                      ],
                    ),
                  );

                  if (confirm == true) {
                    await ref.read(authControllerProvider.notifier).logout();
                    if (context.mounted) {
                      context.go('/login');
                    }
                  }
                },
              ),
            ],
          ),
          const SizedBox(height: 32),
        ],
      ),
    );
  }

  void _showComingSoon(BuildContext context) {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Tính năng đang được phát triển.')),
    );
  }

  Widget _buildSectionHeader(String title, BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(left: 8.0, bottom: 8.0),
      child: Text(
        title.toUpperCase(),
        style: Theme.of(context).textTheme.labelSmall?.copyWith(
              color: Theme.of(context).colorScheme.onSurfaceVariant,
              fontWeight: FontWeight.bold,
              letterSpacing: 1.2,
            ),
      ),
    );
  }

  Widget _buildSectionCard(BuildContext context, {required List<Widget> children}) {
    return Container(
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.3)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 24,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: children,
      ),
    );
  }

  Widget _buildDivider(BuildContext context) {
    return Divider(
      height: 1,
      thickness: 1,
      color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.3),
    );
  }

  Widget _buildListTile({
    required BuildContext context,
    required IconData icon,
    required String title,
    String? subtitle,
    Widget? trailing,
    required VoidCallback onTap,
  }) {
    return ListTile(
      leading: Icon(icon, color: Theme.of(context).colorScheme.onSurfaceVariant),
      title: Text(title, style: Theme.of(context).textTheme.bodyLarge),
      subtitle: subtitle != null ? Text(subtitle, style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant)) : null,
      trailing: trailing ?? Icon(Icons.chevron_right, color: Theme.of(context).colorScheme.onSurfaceVariant),
      onTap: onTap,
    );
  }

  Widget _buildSwitchTile({
    required BuildContext context,
    required IconData icon,
    required String title,
    String? subtitle,
    required bool value,
    required ValueChanged<bool> onChanged,
  }) {
    return SwitchListTile(
      secondary: Icon(icon, color: Theme.of(context).colorScheme.onSurfaceVariant),
      title: Text(title, style: Theme.of(context).textTheme.bodyLarge),
      subtitle: subtitle != null ? Text(subtitle, style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant)) : null,
      value: value,
      onChanged: onChanged,
      activeColor: Theme.of(context).colorScheme.primary,
    );
  }
}
