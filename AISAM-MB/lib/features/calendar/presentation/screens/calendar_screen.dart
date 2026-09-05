import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:table_calendar/table_calendar.dart';
import 'package:intl/intl.dart';
import '../providers/calendar_provider.dart';
import '../../../../core/shared/aisam_logo_widget.dart';
import '../../../../core/shared/profile_avatar_widget.dart';
import '../widgets/calendar_day_list_widget.dart';
import '../../data/models/content_schedule_model.dart';
import '../../data/models/schedule_status_enum.dart';
import '../../../content/presentation/widgets/schedule_post_bottom_sheet.dart';

class CalendarScreen extends ConsumerStatefulWidget {
  const CalendarScreen({super.key});

  @override
  ConsumerState<CalendarScreen> createState() => _CalendarScreenState();
}

class _CalendarScreenState extends ConsumerState<CalendarScreen> {
  DateTime _selectedDate = DateTime.now();
  DateTime _focusedDate = DateTime.now();
  CalendarFormat _calendarFormat = CalendarFormat.week;
  PageController? _pageController;

  List<ContentScheduleModel> _getEventsForDay(
      Map<DateTime, List<ContentScheduleModel>> grouped, DateTime day) {
    final normalized = DateTime(day.year, day.month, day.day);
    return grouped[normalized] ?? [];
  }

  @override
  Widget build(BuildContext context) {
    final schedulesAsync = ref.watch(calendarNotifierProvider);

    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.surface.withValues(alpha: 0.8),
        elevation: 0,
        scrolledUnderElevation: 0,
        title: const AisamLogoWidget(),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            color: Theme.of(context).colorScheme.primary,
            onPressed: () {
              ref.read(calendarNotifierProvider.notifier).refresh();
            },
          ),
          IconButton(
            icon: const Icon(Icons.expand_circle_down),
            color: Theme.of(context).colorScheme.primary,
            onPressed: () {
              setState(() {
                _focusedDate = DateTime.now();
                _selectedDate = DateTime.now();
              });
            },
          ),
          const ProfileAvatarWidget(),
          const SizedBox(width: 8),
        ],
      ),
      body: schedulesAsync.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Error loading schedules: $error'),
              ElevatedButton(
                onPressed: () => ref.read(calendarNotifierProvider.notifier).refresh(),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
        data: (schedules) {
          final grouped = ref.read(calendarNotifierProvider.notifier).groupSchedulesByDate(schedules);
          final dailySchedules = _getEventsForDay(grouped, _selectedDate);

          return RefreshIndicator(
            onRefresh: () async {
              await ref.read(calendarNotifierProvider.notifier).refresh();
            },
            child: ListView(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              children: [
                // Header Section
                Row(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Lịch trình',
                          style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                                fontWeight: FontWeight.bold,
                                color: Theme.of(context).colorScheme.onSurface,
                              ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Tháng ${DateFormat('MM, yyyy').format(_focusedDate)}',
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: Theme.of(context).colorScheme.onSurfaceVariant,
                              ),
                        ),
                      ],
                    ),
                    ElevatedButton.icon(
                      onPressed: () {
                        showModalBottomSheet(
                          context: context,
                          isScrollControlled: true,
                          backgroundColor: Colors.transparent,
                          builder: (context) => const SchedulePostBottomSheet(),
                        );
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Theme.of(context).colorScheme.primary,
                        foregroundColor: Theme.of(context).colorScheme.onPrimary,
                        elevation: 0,
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                      ),
                      icon: const Icon(Icons.add, size: 18),
                      label: const Text('Lên lịch nhanh', style: TextStyle(fontWeight: FontWeight.bold)),
                    ),
                  ],
                ),
                const SizedBox(height: 24),

                // Weekly Calendar Glance
                GestureDetector(
                  behavior: HitTestBehavior.opaque,
                  onDoubleTap: () {
                    setState(() {
                      _calendarFormat = _calendarFormat == CalendarFormat.week
                          ? CalendarFormat.month
                          : CalendarFormat.week;
                    });
                  },
                  child: Container(
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.surface,
                    borderRadius: BorderRadius.circular(24),
                    border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withValues(alpha: 0.3)),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.04),
                        blurRadius: 24,
                        offset: const Offset(0, 8),
                      ),
                    ],
                  ),
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    children: [
                      // Custom Header
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          IconButton(
                            icon: const Icon(Icons.chevron_left),
                            onPressed: () {
                              _pageController?.previousPage(duration: const Duration(milliseconds: 300), curve: Curves.easeOut);
                            },
                          ),
                          Text(
                            DateFormat('MM/yyyy').format(_focusedDate),
                            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.bold,
                                  color: Theme.of(context).colorScheme.onSurface,
                                ),
                          ),
                          IconButton(
                            icon: const Icon(Icons.chevron_right),
                            onPressed: () {
                              _pageController?.nextPage(duration: const Duration(milliseconds: 300), curve: Curves.easeOut);
                            },
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),

                      TableCalendar<ContentScheduleModel>(
                          firstDay: DateTime.utc(2020, 1, 1),
                        lastDay: DateTime.utc(2030, 12, 31),
                        focusedDay: _focusedDate,
                        calendarFormat: _calendarFormat,
                        startingDayOfWeek: StartingDayOfWeek.monday,
                        headerVisible: false, // Use our custom header
                        daysOfWeekHeight: 24,
                        rowHeight: 70, // Increase height for dots
                        selectedDayPredicate: (day) => isSameDay(_selectedDate, day),
                        onDaySelected: (selectedDay, focusedDay) {
                          setState(() {
                            _selectedDate = selectedDay;
                            _focusedDate = focusedDay;
                          });
                        },
                        onCalendarCreated: (controller) {
                          _pageController = controller;
                        },
                        onPageChanged: (focusedDay) {
                          setState(() {
                            _focusedDate = focusedDay;
                          });
                        },
                        eventLoader: (day) => _getEventsForDay(grouped, day),
                        calendarBuilders: CalendarBuilders(
                          dowBuilder: (context, day) {
                            final text = DateFormat.E('vi').format(day).toUpperCase();
                            return Center(
                              child: Text(
                                text,
                                style: Theme.of(context).textTheme.labelSmall?.copyWith(
                                      color: isSameDay(day, _selectedDate)
                                          ? Theme.of(context).colorScheme.primary
                                          : Theme.of(context).colorScheme.onSurfaceVariant,
                                      fontWeight: isSameDay(day, _selectedDate) ? FontWeight.bold : FontWeight.normal,
                                    ),
                              ),
                            );
                          },
                          defaultBuilder: (context, day, focusedDay) {
                            return _buildCalendarDay(day, isSelected: false, isToday: false);
                          },
                          selectedBuilder: (context, day, focusedDay) {
                            return _buildCalendarDay(day, isSelected: true, isToday: false);
                          },
                          todayBuilder: (context, day, focusedDay) {
                            return _buildCalendarDay(day, isSelected: isSameDay(day, _selectedDate), isToday: true);
                          },
                          outsideBuilder: (context, day, focusedDay) {
                            return Opacity(opacity: 0.5, child: _buildCalendarDay(day, isSelected: false, isToday: false));
                          },
                          markerBuilder: (context, date, events) {
                            if (events.isEmpty) return const SizedBox();
                            return Positioned(
                              bottom: 0,
                              child: Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: events.take(3).map((event) {
                                  return Container(
                                    margin: const EdgeInsets.symmetric(horizontal: 1.5),
                                    width: 6,
                                    height: 6,
                                    decoration: BoxDecoration(
                                      shape: BoxShape.circle,
                                      color: _getStatusColor(event.status),
                                    ),
                                  );
                                }).toList(),
                              ),
                            );
                          },
                        ),
                      ), // Close TableCalendar
                      const SizedBox(height: 16),

                      // Legend
                      Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          _buildLegendItem('Đã đăng', Colors.green),
                          const SizedBox(width: 16),
                          _buildLegendItem('Lên lịch', Colors.orange),
                          const SizedBox(width: 16),
                          _buildLegendItem('Thất bại', Colors.red),
                        ],
                      ),
                    ],
                  ),
                ),
                ), // Close GestureDetector for the whole container
                const SizedBox(height: 24),

                // Post List for Selected Day
                CalendarDayListWidget(
                  date: _selectedDate,
                  schedules: dailySchedules,
                ),
                const SizedBox(height: 32),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildLegendItem(String label, Color color) {
    return Row(
      children: [
        Container(
          width: 8,
          height: 8,
          decoration: BoxDecoration(shape: BoxShape.circle, color: color),
        ),
        const SizedBox(width: 6),
        Text(
          label,
          style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant),
        ),
      ],
    );
  }

  Widget _buildCalendarDay(DateTime day, {required bool isSelected, required bool isToday}) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 36,
            height: 36,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: isSelected
                  ? Theme.of(context).colorScheme.primary
                  : (isToday ? Colors.blue : Colors.transparent),
            ),
            child: Center(
              child: Text(
                '${day.day}',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: isSelected
                          ? Theme.of(context).colorScheme.onPrimary
                          : (isToday ? Colors.white : Theme.of(context).colorScheme.onSurface),
                      fontWeight: isSelected || isToday ? FontWeight.bold : FontWeight.normal,
                    ),
              ),
            ),
          ),
          const SizedBox(height: 10), // Space for markers
        ],
      ),
    );
  }

  Color _getStatusColor(ScheduleStatusEnum status) {
    switch (status) {
      case ScheduleStatusEnum.completed:
        return Colors.green;
      case ScheduleStatusEnum.failed:
        return Colors.red;
      case ScheduleStatusEnum.processing:
        return Colors.blue;
      case ScheduleStatusEnum.pending:
      default:
        return Colors.orange;
    }
  }
}
