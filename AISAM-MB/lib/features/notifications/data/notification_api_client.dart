import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../core/network/api_client.dart';
import '../domain/notification_preference_model.dart';
import '../domain/notification_model.dart';

part 'notification_api_client.g.dart';

class NotificationApiClient {
  final Dio _dio;

  NotificationApiClient(this._dio);

  Future<Map<String, dynamic>> getNotifications({
    int page = 1,
    int pageSize = 10,
    int? type,
    String? fromDate,
    String? toDate,
  }) async {
    final query = <String, dynamic>{
      'page': page,
      'pageSize': pageSize,
    };
    if (type != null) query['type'] = type;
    if (fromDate != null) query['fromDate'] = fromDate;
    if (toDate != null) query['toDate'] = toDate;

    final response = await _dio.get('/notifications', queryParameters: query);
    return response.data['data'];
  }

  Future<void> markAsRead(String notificationId) async {
    await _dio.post('/notifications/$notificationId/mark-read');
  }

  Future<void> markAllAsRead() async {
    await _dio.post('/notifications/mark-all-read');
  }

  Future<int> getUnreadCount() async {
    final response = await _dio.get('/notifications/unread-count');
    return response.data['data']['count'] as int;
  }

}

@riverpod
NotificationApiClient notificationApiClient(NotificationApiClientRef ref) {
  return NotificationApiClient(ref.watch(dioProvider));
}
