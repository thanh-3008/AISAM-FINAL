import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:dio/dio.dart';
import '../domain/dashboard_summary.dart';
import '../data/dashboard_repository.dart';
import '../../content/data/models/content_model.dart';
import '../../workspace/presentation/providers/workspace_controller.dart';
import '../../access/presentation/access_providers.dart';
import '../../../../core/network/access_events.dart';

part 'dashboard_controller.g.dart';

@riverpod
class DashboardController extends _$DashboardController {
  @override
  FutureOr<CombinedDashboardSummary> build() async {
    ref.watch(activeWorkspaceControllerProvider);
    final access = await ref.watch(accessContextProvider.future);
    if (!access.canViewAnalytics) throw StateError('Access denied');
    return _fetchSummary();
  }

  Future<CombinedDashboardSummary> _fetchSummary() async {
    if (ref.read(accessDeniedProvider)) throw StateError('Access denied');
    final access = await ref.read(accessContextProvider.future);
    if (!access.canViewAnalytics) throw StateError('Access denied');
    try {
      final repo = ref.read(dashboardRepositoryProvider);
      final response = await repo.getSummary();
      
      if (response.success && response.data != null) {
        return response.data!;
      } else {
        throw Exception(response.message ?? 'Failed to load summary');
      }
    } catch (e) {
      if (e is DioException && e.response?.data != null) {
        throw Exception(e.response?.data['message'] ?? e.message);
      }
      rethrow;
    }
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchSummary());
    ref.read(recentActivitiesControllerProvider.notifier).refresh();
  }
}

@riverpod
class RecentActivitiesController extends _$RecentActivitiesController {
  @override
  Future<List<ContentResponseModel>> build() async {
    ref.watch(activeWorkspaceControllerProvider);
    return _fetchRecentActivities();
  }

  Future<List<ContentResponseModel>> _fetchRecentActivities() async {
    final repo = ref.read(dashboardRepositoryProvider);
    return await repo.getRecentActivities();
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchRecentActivities());
  }
}
