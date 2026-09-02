import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../domain/notification_model.dart';
import '../../domain/notification_filter_model.dart';
import '../../data/notification_api_client.dart';

part 'notification_provider.g.dart';

@riverpod
class NotificationFilterState extends _$NotificationFilterState {
  @override
  NotificationFilterModel build() {
    return const NotificationFilterModel();
  }

  void updateFilter(NotificationFilterModel filter) {
    state = filter;
  }
}

@riverpod
class NotificationListState extends _$NotificationListState {
  @override
  FutureOr<List<NotificationModel>> build() async {
    return _fetchNotifications();
  }

  Future<List<NotificationModel>> _fetchNotifications() async {
    final client = ref.read(notificationApiClientProvider);
    final filter = ref.watch(notificationFilterStateProvider);
    
    final response = await client.getNotifications(
      page: 1, // Currently fetching page 1, support pagination later if needed
      pageSize: 50,
    );

    final data = response['data'] as List;
    var list = data.map((e) => NotificationModel.fromJson(e)).toList();

    // Local filtering since Backend is frozen
    if (filter.type != null) {
      final typeString = _getTypeString(filter.type!);
      list = list.where((n) => n.type == typeString).toList();
    }
    
    if (filter.fromDate != null) {
      list = list.where((n) => n.createdAt.isAfter(filter.fromDate!)).toList();
    }
    
    if (filter.toDate != null) {
      list = list.where((n) => n.createdAt.isBefore(filter.toDate!.add(const Duration(days: 1)))).toList();
    }

    return list;
  }

  String _getTypeString(int type) {
    switch (type) {
      case 0: return 'ApprovalNeeded';
      case 1: return 'PostScheduled';
      case 2: return 'PerformanceAlert';
      case 3: return 'AiSuggestion';
      case 4: return 'SystemUpdate';
      default: return 'Unknown';
    }
  }

  Future<void> markAsRead(String id) async {
    final client = ref.read(notificationApiClientProvider);
    await client.markAsRead(id);
    
    // Refresh unread count
    ref.invalidate(unreadNotificationCountProvider);
    
    // Update local state
    final previous = state.valueOrNull ?? [];
    state = AsyncData(previous.map((n) {
      if (n.id == id) {
        return n.copyWith(isRead: true);
      }
      return n;
    }).toList());
  }

  Future<void> markAllAsRead() async {
    final client = ref.read(notificationApiClientProvider);
    await client.markAllAsRead();
    
    ref.invalidate(unreadNotificationCountProvider);
    
    final previous = state.valueOrNull ?? [];
    state = AsyncData(previous.map((n) => n.copyWith(isRead: true)).toList());
  }
}

@riverpod
Future<int> unreadNotificationCount(UnreadNotificationCountRef ref) async {
  final client = ref.watch(notificationApiClientProvider);
  return client.getUnreadCount();
}
