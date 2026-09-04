import 'package:flutter/material.dart';

class RejectReasonDialog extends StatefulWidget {
  const RejectReasonDialog({super.key});

  static Future<String?> show(BuildContext context) {
    return showModalBottomSheet<String>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => const RejectReasonDialog(),
    );
  }

  @override
  State<RejectReasonDialog> createState() => _RejectReasonDialogState();
}

class _RejectReasonDialogState extends State<RejectReasonDialog> {
  String? _selectedReason;
  final TextEditingController _customReasonController = TextEditingController();

  final List<String> _presetReasons = [
    'Thông tin không chính xác',
    'Hình ảnh/Video không phù hợp',
    'Sai lỗi chính tả/ngữ pháp',
    'Chưa đúng thông điệp thương hiệu',
    'Khác (tự nhập)',
  ];

  @override
  void dispose() {
    _customReasonController.dispose();
    super.dispose();
  }

  void _submit() {
    if (_selectedReason == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vui lòng chọn hoặc nhập lý do từ chối')),
      );
      return;
    }

    if (_selectedReason == 'Khác (tự nhập)') {
      if (_customReasonController.text.trim().isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Vui lòng nhập lý do')),
        );
        return;
      }
      Navigator.of(context).pop(_customReasonController.text.trim());
    } else {
      Navigator.of(context).pop(_selectedReason);
    }
  }

  @override
  Widget build(BuildContext context) {
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
                'Lý do từ chối',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => Navigator.pop(context),
              )
            ],
          ),
          const SizedBox(height: 16),
          ..._presetReasons.map((reason) {
            return RadioListTile<String>(
              title: Text(reason),
              value: reason,
              groupValue: _selectedReason,
              contentPadding: EdgeInsets.zero,
              onChanged: (value) {
                setState(() {
                  _selectedReason = value;
                });
              },
            );
          }),
          if (_selectedReason == 'Khác (tự nhập)') ...[
            const SizedBox(height: 8),
            TextField(
              controller: _customReasonController,
              decoration: InputDecoration(
                hintText: 'Nhập lý do của bạn...',
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              ),
              maxLines: 3,
            ),
          ],
          const SizedBox(height: 24),
          ElevatedButton(
            onPressed: _submit,
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
              backgroundColor: Theme.of(context).colorScheme.error,
              foregroundColor: Theme.of(context).colorScheme.onError,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: const Text('Xác nhận từ chối', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }
}
