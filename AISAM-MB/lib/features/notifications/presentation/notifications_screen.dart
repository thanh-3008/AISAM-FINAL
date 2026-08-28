import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/shared/empty_state_widget.dart';
import '../../settings/presentation/providers/language_provider.dart';

class NotificationsScreen extends ConsumerStatefulWidget {
  const NotificationsScreen({super.key});

  @override
  ConsumerState<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends ConsumerState<NotificationsScreen> {
  bool _pushEnabled = true;
  bool _approvalEnabled = true;
  bool _workspaceInviteEnabled = true;

  final List<Map<String, dynamic>> _notifications = [];

  @override
  Widget build(BuildContext context) {
    final langState = ref.watch(languageControllerProvider);
    final isEn = (langState.value ?? 'vi') == 'en';
    
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      appBar: AppBar(
        title: Text(isEn ? 'Notifications' : 'Thông báo'),
        backgroundColor: Theme.of(context).colorScheme.surface.withOpacity(0.8),
        elevation: 0,
        scrolledUnderElevation: 0,
      ),
      body: ListView(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        children: [
          _buildSectionHeader(isEn ? 'System' : 'Hệ thống', context),
          _buildSectionCard(
            context,
            children: [
              _buildSwitchTile(
                context: context,
                icon: Icons.notifications_active_outlined,
                title: isEn ? 'Enable Push Notifications' : 'Cho phép thông báo Push',
                subtitle: isEn ? 'Receive important notifications on your device' : 'Nhận thông báo quan trọng trên thiết bị',
                value: _pushEnabled,
                onChanged: (val) {
                  setState(() => _pushEnabled = val);
                  if (!val) {
                    setState(() {
                      _approvalEnabled = false;
                      _workspaceInviteEnabled = false;
                    });
                  }
                },
              ),
            ],
          ),
          const SizedBox(height: 24),
          _buildSectionHeader(isEn ? 'Customization' : 'Tuỳ chỉnh', context),
          _buildSectionCard(
            context,
            children: [
              _buildSwitchTile(
                context: context,
                icon: Icons.check_circle_outline,
                title: isEn ? 'Post Approval' : 'Thông báo duyệt bài',
                subtitle: isEn ? 'When a post requires your approval' : 'Khi có bài viết cần bạn phê duyệt',
                value: _approvalEnabled,
                onChanged: _pushEnabled
                    ? (val) => setState(() => _approvalEnabled = val)
                    : null,
              ),
              _buildDivider(context),
              _buildSwitchTile(
                context: context,
                icon: Icons.group_add_outlined,
                title: isEn ? 'Workspace Invitations' : 'Lời mời Workspace',
                subtitle: isEn ? 'When someone invites you to a new workspace' : 'Khi có người mời bạn vào workspace mới',
                value: _workspaceInviteEnabled,
                onChanged: _pushEnabled
                    ? (val) => setState(() => _workspaceInviteEnabled = val)
                    : null,
              ),
            ],
          ),
          const SizedBox(height: 24),
          // --- LỊCH SỬ THÔNG BÁO ---
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Padding(
                padding: const EdgeInsets.only(left: 8.0, bottom: 8.0),
                child: Text(
                  isEn ? 'NOTIFICATION HISTORY' : 'LỊCH SỬ THÔNG BÁO',
                  style: Theme.of(context).textTheme.labelSmall?.copyWith(
                        color: Theme.of(context).colorScheme.onSurfaceVariant,
                        fontWeight: FontWeight.bold,
                        letterSpacing: 1.2,
                      ),
                ),
              ),
              TextButton(
                onPressed: () {
                  setState(() {
                    for (final n in _notifications) {
                      n['isRead'] = true;
                    }
                  });
                },
                child: Text(isEn ? 'Mark all as read' : 'Đánh dấu tất cả đã đọc'),
              ),
            ],
          ),
          _buildSectionCard(
            context,
            children: [
              if (_notifications.isEmpty)
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 32),
                  child: EmptyStateWidget(
                    title: isEn ? 'No recent notifications' : 'Không có thông báo gần đây',
                    message: isEn ? 'You have read all notifications.' : 'Bạn đã xem hết tất cả thông báo.',
                    icon: Icons.notifications_none,
                  ),
                )
              else
                ...List.generate(_notifications.length, (i) {
                  final n = _notifications[i];
                  final isLast = i == _notifications.length - 1;
                  return Column(
                    children: [
                      _buildNotificationTile(context, n, isEn),
                      if (!isLast) _buildDivider(context),
                    ],
                  );
                }),
            ],
          ),
          const SizedBox(height: 32),
        ],
      ),
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

  Widget _buildSwitchTile({
    required BuildContext context,
    required IconData icon,
    required String title,
    String? subtitle,
    required bool value,
    required void Function(bool)? onChanged,
  }) {
    return SwitchListTile(
      secondary: Icon(icon, color: Theme.of(context).colorScheme.onSurfaceVariant),
      title: Text(title, style: Theme.of(context).textTheme.bodyLarge),
      subtitle: subtitle != null
          ? Text(subtitle,
              style: Theme.of(context)
                  .textTheme
                  .bodySmall
                  ?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant))
          : null,
      value: value,
      onChanged: onChanged,
      activeColor: Theme.of(context).colorScheme.primary,
    );
  }

  Widget _buildNotificationTile(BuildContext context, Map<String, dynamic> n, bool isEn) {
    final isRead = n['isRead'] as bool;
    return ListTile(
      tileColor: isRead ? null : Theme.of(context).colorScheme.primaryContainer.withValues(alpha: 0.15),
      leading: CircleAvatar(
        backgroundColor: Theme.of(context).colorScheme.secondaryContainer,
        child: Icon(n['icon'] as IconData, size: 20, color: Theme.of(context).colorScheme.secondary),
      ),
      title: Row(
        children: [
          Expanded(child: Text(n['title'] as String, style: Theme.of(context).textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.bold))),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
            decoration: BoxDecoration(
              color: isRead
                  ? Theme.of(context).colorScheme.surfaceContainerHighest
                  : Theme.of(context).colorScheme.primary,
              borderRadius: BorderRadius.circular(12),
            ),
            child: Text(
              isRead ? (isEn ? 'Read' : 'Đã đọc') : (isEn ? 'New' : 'Mới'),
              style: TextStyle(
                fontSize: 10,
                color: isRead ? Theme.of(context).colorScheme.onSurfaceVariant : Colors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
      subtitle: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SizedBox(height: 2),
          Text(n['body'] as String, style: Theme.of(context).textTheme.bodySmall),
          const SizedBox(height: 4),
          Text(n['time'] as String, style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant)),
        ],
      ),
      isThreeLine: true,
      onTap: () => setState(() => n['isRead'] = true),
    );
  }
}
