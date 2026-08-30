import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import '../../../core/shared/empty_state_widget.dart';
import '../../settings/presentation/providers/language_provider.dart';
import 'providers/notification_provider.dart';
import 'providers/notification_preference_provider.dart';
import '../domain/notification_model.dart';
import '../domain/notification_preference_model.dart';

class NotificationsScreen extends ConsumerStatefulWidget {
  const NotificationsScreen({super.key});

  @override
  ConsumerState<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends ConsumerState<NotificationsScreen> {
  @override
  Widget build(BuildContext context) {
    final langState = ref.watch(languageControllerProvider);
    final isEn = (langState.value ?? 'vi') == 'en';
    final preferencesState = ref.watch(notificationPreferenceStateProvider);
    final notificationsState = ref.watch(notificationListStateProvider);
    
    // Approval needed = 0, System update = 4 (for Workspace Invite mockup)
    final approvalPrefs = preferencesState.valueOrNull?.firstWhere(
      (p) => p.notificationType == 0, 
      orElse: () => const NotificationPreferenceModel(notificationType: 0, isEnabled: true)
    );
    final workspacePrefs = preferencesState.valueOrNull?.firstWhere(
      (p) => p.notificationType == 4, 
      orElse: () => const NotificationPreferenceModel(notificationType: 4, isEnabled: true)
    );
    
    final bool approvalEnabled = approvalPrefs?.isEnabled ?? true;
    final bool workspaceInviteEnabled = workspacePrefs?.isEnabled ?? true;
    
    final masterPushState = ref.watch(masterPushEnabledStateProvider);
    final isPushEnabled = masterPushState.valueOrNull ?? true;

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
                value: isPushEnabled,
                onChanged: (val) {
                  ref.read(masterPushEnabledStateProvider.notifier).toggle(val);
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
                value: approvalEnabled,
                onChanged: isPushEnabled
                    ? (val) => ref.read(notificationPreferenceStateProvider.notifier).togglePreference(0, val)
                    : null,
              ),
              _buildDivider(context),
              _buildSwitchTile(
                context: context,
                icon: Icons.group_add_outlined,
                title: isEn ? 'Workspace Invitations' : 'Lời mời Workspace',
                subtitle: isEn ? 'When someone invites you to a new workspace' : 'Khi có người mời bạn vào workspace mới',
                value: workspaceInviteEnabled,
                onChanged: isPushEnabled
                    ? (val) => ref.read(notificationPreferenceStateProvider.notifier).togglePreference(4, val)
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
                  isEn ? 'HISTORY' : 'LỊCH SỬ',
                  style: Theme.of(context).textTheme.labelSmall?.copyWith(
                        color: Theme.of(context).colorScheme.onSurfaceVariant,
                        fontWeight: FontWeight.bold,
                        letterSpacing: 1.2,
                      ),
                ),
              ),
              Row(
                children: [
                  _buildTimeFilter(context, isEn),
                  const SizedBox(width: 8),
                  _buildTypeFilter(context, isEn),
                ],
              ),
            ],
          ),
          Align(
            alignment: Alignment.centerRight,
            child: TextButton(
              onPressed: () {
                ref.read(notificationListStateProvider.notifier).markAllAsRead();
              },
              child: Text(isEn ? 'Mark all as read' : 'Đánh dấu tất cả đã đọc'),
            ),
          ),
          _buildSectionCard(
            context,
            children: [
              notificationsState.when(
                data: (notifications) {
                  if (notifications.isEmpty) {
                    return Padding(
                      padding: const EdgeInsets.symmetric(vertical: 32),
                      child: EmptyStateWidget(
                        title: isEn ? 'No recent notifications' : 'Không có thông báo gần đây',
                        message: isEn ? 'You have read all notifications.' : 'Bạn đã xem hết tất cả thông báo.',
                        icon: Icons.notifications_none,
                      ),
                    );
                  }
                  return Column(
                    children: List.generate(notifications.length, (i) {
                      final n = notifications[i];
                      final isLast = i == notifications.length - 1;
                      return Column(
                        children: [
                          _buildNotificationTile(context, n, isEn),
                          if (!isLast) _buildDivider(context),
                        ],
                      );
                    }),
                  );
                },
                loading: () => const Padding(
                  padding: EdgeInsets.symmetric(vertical: 32),
                  child: Center(child: CircularProgressIndicator()),
                ),
                error: (e, st) => Padding(
                  padding: const EdgeInsets.symmetric(vertical: 32),
                  child: Center(child: Text(isEn ? 'Error loading notifications' : 'Lỗi tải thông báo')),
                ),
              ),
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

  Widget _buildNotificationTile(BuildContext context, NotificationModel n, bool isEn) {
    final isRead = n.isRead;
    IconData iconData = Icons.notifications;
    if (n.type == 'ApprovalNeeded') iconData = Icons.check_circle_outline;
    if (n.type == 'PostScheduled') iconData = Icons.schedule;
    if (n.type == 'PerformanceAlert') iconData = Icons.trending_up;
    if (n.type == 'AiSuggestion') iconData = Icons.auto_awesome;
    if (n.type == 'SystemUpdate') iconData = Icons.system_update;

    final formattedTime = DateFormat('MMM d, HH:mm').format(n.createdAt.toLocal());

    return ListTile(
      tileColor: isRead ? null : Theme.of(context).colorScheme.primaryContainer.withValues(alpha: 0.15),
      leading: CircleAvatar(
        backgroundColor: Theme.of(context).colorScheme.secondaryContainer,
        child: Icon(iconData, size: 20, color: Theme.of(context).colorScheme.secondary),
      ),
      title: Row(
        children: [
          Expanded(child: Text(n.title, style: Theme.of(context).textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.bold))),
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
          Text(n.message, style: Theme.of(context).textTheme.bodySmall),
          const SizedBox(height: 4),
          Text(formattedTime, style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant)),
        ],
      ),
      isThreeLine: true,
      onTap: () {
        if (!isRead) {
          ref.read(notificationListStateProvider.notifier).markAsRead(n.id);
        }
      },
    );
  }

  Widget _buildTimeFilter(BuildContext context, bool isEn) {
    final filter = ref.watch(notificationFilterStateProvider);
    
    // Determine current selection
    String currentValue = 'all';
    if (filter.fromDate != null) {
      final now = DateTime.now();
      final diff = now.difference(filter.fromDate!).inDays;
      if (diff <= 1) currentValue = 'today';
      else if (diff <= 7) currentValue = '7days';
      else if (diff <= 30) currentValue = '30days';
    }

    return DropdownButton<String>(
      value: currentValue,
      icon: const Icon(Icons.arrow_drop_down, size: 20),
      underline: const SizedBox(),
      style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Theme.of(context).colorScheme.primary),
      onChanged: (String? newValue) {
        if (newValue == null) return;
        DateTime? fromDate;
        final now = DateTime.now();
        if (newValue == 'today') fromDate = DateTime(now.year, now.month, now.day);
        if (newValue == '7days') fromDate = now.subtract(const Duration(days: 7));
        if (newValue == '30days') fromDate = now.subtract(const Duration(days: 30));
        
        ref.read(notificationFilterStateProvider.notifier).updateFilter(
          filter.copyWith(fromDate: fromDate, toDate: null)
        );
      },
      items: [
        DropdownMenuItem(value: 'all', child: Text(isEn ? 'All time' : 'Tất cả')),
        DropdownMenuItem(value: 'today', child: Text(isEn ? 'Today' : 'Hôm nay')),
        DropdownMenuItem(value: '7days', child: Text(isEn ? 'Last 7 days' : '7 ngày qua')),
        DropdownMenuItem(value: '30days', child: Text(isEn ? 'Last 30 days' : '30 ngày qua')),
      ],
    );
  }

  Widget _buildTypeFilter(BuildContext context, bool isEn) {
    final filter = ref.watch(notificationFilterStateProvider);
    return DropdownButton<int?>(
      value: filter.type,
      icon: const Icon(Icons.arrow_drop_down, size: 20),
      underline: const SizedBox(),
      style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Theme.of(context).colorScheme.primary),
      onChanged: (int? newValue) {
        ref.read(notificationFilterStateProvider.notifier).updateFilter(
          filter.copyWith(type: newValue)
        );
      },
      items: [
        DropdownMenuItem(value: null, child: Text(isEn ? 'All types' : 'Tất cả loại')),
        DropdownMenuItem(value: 0, child: Text(isEn ? 'Approval' : 'Duyệt bài')),
        DropdownMenuItem(value: 4, child: Text(isEn ? 'Workspace' : 'Lời mời Workspace')),
      ],
    );
  }
}
