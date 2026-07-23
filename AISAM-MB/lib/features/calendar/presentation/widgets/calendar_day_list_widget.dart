import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../data/models/schedule_status_enum.dart';
import '../../data/models/content_schedule_model.dart';

class CalendarDayListWidget extends StatelessWidget {
  final DateTime date;
  final List<ContentScheduleModel> schedules;

  const CalendarDayListWidget({
    super.key,
    required this.date,
    required this.schedules,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          '${DateFormat.EEEE('vi').format(date)}, ${date.day} Tháng ${date.month}',
          style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 16),
        if (schedules.isEmpty)
          Center(
            child: Padding(
              padding: const EdgeInsets.all(32.0),
              child: Text(
                'Không có bài đăng nào trong ngày.',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.grey),
              ),
            ),
          )
        else
          ListView.separated(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            itemCount: schedules.length,
            separatorBuilder: (context, index) => const SizedBox(height: 12),
            itemBuilder: (context, index) {
              final schedule = schedules[index];
              return _buildScheduleItem(context, schedule);
            },
          ),
      ],
    );
  }

  Widget _buildScheduleItem(BuildContext context, ContentScheduleModel schedule) {
    final isFailed = schedule.status == ScheduleStatusEnum.failed;
    final timeFormat = DateFormat('hh:mm a').format(schedule.scheduledAt.toLocal()).split(' ');
    final timeStr = timeFormat[0]; // e.g. 09:00
    final amPmStr = timeFormat[1]; // e.g. AM

    return Container(
      decoration: BoxDecoration(
        color: isFailed ? Theme.of(context).colorScheme.errorContainer.withOpacity(0.2) : Theme.of(context).colorScheme.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: isFailed ? Theme.of(context).colorScheme.error.withOpacity(0.3) : Theme.of(context).colorScheme.outlineVariant.withOpacity(0.3),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.02),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      clipBehavior: Clip.hardEdge,
      child: Stack(
        children: [
          if (isFailed)
            Positioned(
              left: 0,
              top: 0,
              bottom: 0,
              width: 4,
              child: Container(color: Theme.of(context).colorScheme.error),
            ),
          Padding(
            padding: const EdgeInsets.all(12.0),
            child: Row(
              children: [
                // Time
                SizedBox(
                  width: 60,
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        timeStr,
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      Text(
                        amPmStr,
                        style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant),
                      ),
                    ],
                  ),
                ),

                // Image Thumbnail
                Container(
                  width: 64,
                  height: 64,
                  margin: const EdgeInsets.only(right: 16),
                  decoration: BoxDecoration(
                    color: Theme.of(context).colorScheme.surfaceContainerHigh,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  clipBehavior: Clip.hardEdge,
                  child: Center(
                          child: Icon(Icons.image, color: Theme.of(context).colorScheme.outlineVariant, size: 28),
                        ),
                ),

                // Content
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Row(
                        children: [
                          Icon(
                            schedule.platform?.toLowerCase() == 'facebook' ? Icons.facebook : Icons.public,
                            size: 16,
                            color: Colors.blue,
                          ),
                          const SizedBox(width: 8),
                          _buildStatusBadge(context, schedule.status),
                        ],
                      ),
                      const SizedBox(height: 4),
                      Text(
                        schedule.title ?? 'Untitled Post',
                        style: Theme.of(context).textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.bold),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                  ),
                ),

                // Trailing Action
                IconButton(
                  icon: Icon(isFailed ? Icons.refresh : Icons.more_vert),
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                  onPressed: () {
                    // Handle action
                  },
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatusBadge(BuildContext context, ScheduleStatusEnum status) {
    Color bgColor;
    Color textColor;
    String label;
    IconData? icon;

    switch (status) {
      case ScheduleStatusEnum.completed:
        bgColor = Colors.green.shade50;
        textColor = Colors.green.shade700;
        label = 'Đã đăng';
        break;
      case ScheduleStatusEnum.failed:
        bgColor = Colors.red.shade50;
        textColor = Colors.red.shade700;
        label = 'Thất bại';
        icon = Icons.error;
        break;
      case ScheduleStatusEnum.processing:
        bgColor = Colors.blue.shade50;
        textColor = Colors.blue.shade700;
        label = 'Đang xử lý';
        break;
      case ScheduleStatusEnum.pending:
      default:
        bgColor = Colors.orange.shade50;
        textColor = Colors.orange.shade800;
        label = 'Lên lịch';
        break;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (icon != null) ...[
            Icon(icon, size: 12, color: textColor),
            const SizedBox(width: 4),
          ],
          Text(
            label,
            style: Theme.of(context).textTheme.labelSmall?.copyWith(color: textColor, fontWeight: FontWeight.bold),
          ),
        ],
      ),
    );
  }
}
