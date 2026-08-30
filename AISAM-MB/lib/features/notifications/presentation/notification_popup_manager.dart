import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'providers/notification_provider.dart';
import 'providers/notification_preference_provider.dart';
import '../domain/notification_model.dart';
import '../domain/notification_preference_model.dart';
import 'package:go_router/go_router.dart';

class NotificationPopupManager extends ConsumerStatefulWidget {
  final Widget child;

  const NotificationPopupManager({super.key, required this.child});

  @override
  ConsumerState<NotificationPopupManager> createState() => _NotificationPopupManagerState();
}

class _NotificationPopupManagerState extends ConsumerState<NotificationPopupManager> {
  Timer? _timer;
  String? _lastNotificationId;

  @override
  void initState() {
    super.initState();
    _startPolling();
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _startPolling() {
    // Poll every 30 seconds
    _timer = Timer.periodic(const Duration(seconds: 30), (_) async {
      final notifications = await ref.read(notificationListStateProvider.future);
      
      // Check master switch
      final masterEnabled = await ref.read(masterPushEnabledStateProvider.future);
      if (!masterEnabled) return;
      
      // Check specific preferences
      final preferences = await ref.read(notificationPreferenceStateProvider.future);
      
      if (notifications.isNotEmpty) {
        final latest = notifications.first;
        if (_lastNotificationId != null && latest.id != _lastNotificationId && !latest.isRead) {
          
          final pref = preferences.firstWhere(
            (p) => p.notificationType == _getTypeInt(latest.type), 
            orElse: () => NotificationPreferenceModel(notificationType: _getTypeInt(latest.type), isEnabled: true)
          );
          
          if (pref.isEnabled) {
            _showPopup(latest);
          }
        }
        _lastNotificationId = latest.id;
      }
    });
  }

  int _getTypeInt(String typeStr) {
    switch (typeStr) {
      case 'ApprovalNeeded': return 0;
      case 'PostScheduled': return 1;
      case 'PerformanceAlert': return 2;
      case 'AiSuggestion': return 3;
      case 'SystemUpdate': return 4;
      default: return -1;
    }
  }

  void _showPopup(NotificationModel notification) {
    if (!mounted) return;
    
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            Icon(_getIconForType(notification.type), color: Colors.white),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(notification.title, style: const TextStyle(fontWeight: FontWeight.bold)),
                  Text(notification.message, maxLines: 2, overflow: TextOverflow.ellipsis),
                ],
              ),
            ),
          ],
        ),
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        duration: const Duration(seconds: 4),
        action: SnackBarAction(
          label: 'View',
          textColor: Theme.of(context).colorScheme.primaryContainer,
          onPressed: () {
            // Navigate to notifications tab or screen
            context.go('/settings'); // Assuming notifications is accessible from settings or has its own route
          },
        ),
      ),
    );
  }

  IconData _getIconForType(String type) {
    if (type == 'ApprovalNeeded') return Icons.check_circle_outline;
    if (type == 'PostScheduled') return Icons.schedule;
    if (type == 'PerformanceAlert') return Icons.trending_up;
    if (type == 'AiSuggestion') return Icons.auto_awesome;
    if (type == 'SystemUpdate') return Icons.system_update;
    return Icons.notifications;
  }

  @override
  Widget build(BuildContext context) {
    // We just wrap the child
    return widget.child;
  }
}
