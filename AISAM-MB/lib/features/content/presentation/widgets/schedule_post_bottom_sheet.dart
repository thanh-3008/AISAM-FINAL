import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import '../../../../core/shared/app_snackbar.dart';
import '../../../calendar/presentation/providers/calendar_provider.dart';
import '../../../calendar/data/models/create_schedule_request.dart';
import '../../../settings/presentation/providers/social_controller.dart';
import '../../../settings/data/models/social_integration_model.dart';
import '../providers/content_list_controller.dart';
import '../../data/models/content_model.dart';

class SchedulePostBottomSheet extends ConsumerStatefulWidget {
  final String? contentId;
  const SchedulePostBottomSheet({super.key, this.contentId});

  @override
  ConsumerState<SchedulePostBottomSheet> createState() => _SchedulePostBottomSheetState();
}

class _SchedulePostBottomSheetState extends ConsumerState<SchedulePostBottomSheet> {
  DateTime? _selectedDate;
  TimeOfDay? _selectedTime;
  SocialIntegrationModel? _selectedIntegration;
  String? _selectedContentId;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _selectedContentId = widget.contentId;
    _selectedDate = DateTime.now().add(const Duration(days: 1));
    _selectedTime = const TimeOfDay(hour: 9, minute: 0);
  }

  Future<void> _pickDate() async {
    final date = await showDatePicker(
      context: context,
      initialDate: _selectedDate ?? DateTime.now(),
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 365)),
    );
    if (date != null) {
      setState(() {
        _selectedDate = date;
      });
    }
  }

  Future<void> _pickTime() async {
    final time = await showTimePicker(
      context: context,
      initialTime: _selectedTime ?? TimeOfDay.now(),
    );
    if (time != null) {
      setState(() {
        _selectedTime = time;
      });
    }
  }

  Future<void> _handleSchedule() async {
    if (_selectedContentId == null) {
      AppSnackbar.showError(context, 'Vui lòng chọn bài viết để lên lịch');
      return;
    }
    if (_selectedIntegration == null) {
      AppSnackbar.showError(context, 'Vui lòng chọn kênh mạng xã hội');
      return;
    }
    if (_selectedDate == null || _selectedTime == null) {
      AppSnackbar.showError(context, 'Vui lòng chọn ngày giờ đăng');
      return;
    }

    final scheduledAt = DateTime(
      _selectedDate!.year,
      _selectedDate!.month,
      _selectedDate!.day,
      _selectedTime!.hour,
      _selectedTime!.minute,
    ).toUtc();

    if (scheduledAt.isBefore(DateTime.now().toUtc())) {
      AppSnackbar.showError(context, 'Thời gian lên lịch phải ở tương lai');
      return;
    }

    setState(() => _isLoading = true);
    try {
      final request = CreateScheduleRequest(
        contentId: _selectedContentId!,
        integrationId: _selectedIntegration!.id,
        scheduledAt: scheduledAt,
      );
      await ref.read(calendarNotifierProvider.notifier).createSchedule(request);
      if (mounted) {
        AppSnackbar.showSuccess(context, 'Lên lịch thành công!');
        Navigator.pop(context, true);
      }
    } catch (e) {
      if (mounted) {
        AppSnackbar.showError(context, 'Lỗi: $e');
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final integrationsState = ref.watch(socialControllerProvider);
    final contentsState = ref.watch(contentListControllerProvider);

    return Container(
      padding: EdgeInsets.only(
        left: 24,
        right: 24,
        top: 24,
        bottom: MediaQuery.of(context).viewInsets.bottom + 24,
      ),
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Lên lịch nhanh',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => Navigator.pop(context),
              )
            ],
          ),
          const SizedBox(height: 24),
          
          if (widget.contentId == null) ...[
            const Text('Chọn bài viết', style: TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            contentsState.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (e, st) => Text('Lỗi tải danh sách bài viết: $e', style: const TextStyle(color: Colors.red)),
              data: (contents) {
                if (contents.isEmpty) {
                  return const Text('Bạn chưa có bài viết nào. Hãy tạo bài viết trước.');
                }
                return DropdownButtonFormField<String>(
                  initialValue: _selectedContentId,
                  decoration: InputDecoration(
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                    contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                  ),
                  hint: const Text('Chọn một bài viết'),
                  items: contents.map((ContentResponseModel c) {
                    return DropdownMenuItem<String>(
                      value: c.id,
                      child: Text(c.title ?? 'Không có tiêu đề', maxLines: 1, overflow: TextOverflow.ellipsis),
                    );
                  }).toList(),
                  onChanged: (val) {
                    setState(() {
                      _selectedContentId = val;
                    });
                  },
                );
              },
            ),
            const SizedBox(height: 24),
          ],
          
          const Text('Chọn kênh mạng xã hội', style: TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          integrationsState.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (e, st) => Text('Lỗi tải danh sách kênh: $e', style: const TextStyle(color: Colors.red)),
            data: (integrations) {
              if (integrations.isEmpty) {
                return const Text('Chưa có kênh nào được kết nối. Hãy vào Cài đặt để thêm kênh.');
              }
              return DropdownButtonFormField<SocialIntegrationModel>(
                initialValue: _selectedIntegration,
                decoration: InputDecoration(
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                  contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                ),
                hint: const Text('Chọn một kênh'),
                items: integrations.map((integration) {
                  return DropdownMenuItem(
                    value: integration,
                    child: Text('${integration.name ?? integration.platform} (${integration.brandName ?? ""})'),
                  );
                }).toList(),
                onChanged: (val) {
                  setState(() => _selectedIntegration = val);
                },
              );
            },
          ),
          const SizedBox(height: 24),

          const Text('Chọn thời gian', style: TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          Row(
            children: [
              Expanded(
                child: InkWell(
                  onTap: _pickDate,
                  borderRadius: BorderRadius.circular(12),
                  child: Container(
                    padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 12),
                    decoration: BoxDecoration(
                      border: Border.all(color: Theme.of(context).colorScheme.outlineVariant),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.calendar_today, size: 20),
                        const SizedBox(width: 8),
                        Text(
                          _selectedDate == null ? 'Ngày' : DateFormat('dd/MM/yyyy').format(_selectedDate!),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: InkWell(
                  onTap: _pickTime,
                  borderRadius: BorderRadius.circular(12),
                  child: Container(
                    padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 12),
                    decoration: BoxDecoration(
                      border: Border.all(color: Theme.of(context).colorScheme.outlineVariant),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.access_time, size: 20),
                        const SizedBox(width: 8),
                        Text(
                          _selectedTime == null ? 'Giờ' : _selectedTime!.format(context),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 32),

          ElevatedButton(
            onPressed: _isLoading ? null : _handleSchedule,
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              backgroundColor: Theme.of(context).colorScheme.primary,
              foregroundColor: Theme.of(context).colorScheme.onPrimary,
            ),
            child: _isLoading
                ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                : const Text('Xác nhận lên lịch', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }
}
