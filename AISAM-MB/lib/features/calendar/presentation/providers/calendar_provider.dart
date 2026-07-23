import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/calendar_repository.dart';
import '../../data/models/content_schedule_model.dart';
import '../../data/models/create_schedule_request.dart';
import '../../data/models/update_schedule_request.dart';

part 'calendar_provider.g.dart';

@riverpod
class CalendarNotifier extends _$CalendarNotifier {
  @override
  Future<List<ContentScheduleModel>> build() async {
    return _fetchSchedules();
  }

  Future<List<ContentScheduleModel>> _fetchSchedules() async {
    final repository = ref.read(calendarRepositoryProvider);
    // Fetch upcoming schedules
    return repository.getUpcomingSchedules(limit: 500);
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchSchedules());
  }

  Future<void> createSchedule(CreateScheduleRequest request) async {
    final repository = ref.read(calendarRepositoryProvider);
    await repository.createSchedule(request);
    await refresh();
  }

  Future<void> updateSchedule(String id, UpdateScheduleRequest request) async {
    final repository = ref.read(calendarRepositoryProvider);
    await repository.updateSchedule(id, request);
    await refresh();
  }

  Future<void> deleteSchedule(String id) async {
    final repository = ref.read(calendarRepositoryProvider);
    await repository.deleteSchedule(id);
    await refresh();
  }

  Map<DateTime, List<ContentScheduleModel>> groupSchedulesByDate(List<ContentScheduleModel> schedules) {
    final map = <DateTime, List<ContentScheduleModel>>{};
    for (var schedule in schedules) {
      // Fix timezone (UTC -> local)
      final localDate = schedule.scheduledAt.toLocal();
      final date = DateTime(localDate.year, localDate.month, localDate.day);
      if (map.containsKey(date)) {
        map[date]!.add(schedule);
      } else {
        map[date] = [schedule];
      }
    }
    return map;
  }
}
