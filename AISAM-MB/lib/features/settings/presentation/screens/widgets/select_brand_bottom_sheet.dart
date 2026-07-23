import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../profile/presentation/providers/brand_controller.dart';
import '../../../../profile/data/models/brand_model.dart';
import '../../../../../core/shared/app_snackbar.dart';

class SelectBrandBottomSheet extends ConsumerStatefulWidget {
  const SelectBrandBottomSheet({super.key});

  @override
  ConsumerState<SelectBrandBottomSheet> createState() => _SelectBrandBottomSheetState();
}

class _SelectBrandBottomSheetState extends ConsumerState<SelectBrandBottomSheet> {
  String? _selectedBrandId;

  void _handleContinue() {
    if (_selectedBrandId == null) {
      AppSnackbar.showError(context, 'Vui lòng chọn một thương hiệu');
      return;
    }
    Navigator.pop(context, _selectedBrandId);
  }

  @override
  Widget build(BuildContext context) {
    final brandsAsync = ref.watch(brandControllerProvider);

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
                'Chọn Thương hiệu',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => Navigator.pop(context),
              )
            ],
          ),
          const SizedBox(height: 16),
          const Text('Vui lòng chọn Thương hiệu bạn muốn kết nối mạng xã hội:'),
          const SizedBox(height: 16),
          brandsAsync.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (e, st) => Text('Lỗi tải danh sách Brand: $e', style: const TextStyle(color: Colors.red)),
            data: (brands) {
              if (brands.isEmpty) {
                return const Text('Bạn chưa có Thương hiệu nào. Hãy tạo Thương hiệu trước.');
              }
              return DropdownButtonFormField<String>(
                initialValue: _selectedBrandId,
                decoration: InputDecoration(
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                  contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                ),
                hint: const Text('Chọn Thương hiệu'),
                items: brands.map((BrandResponseModel b) {
                  return DropdownMenuItem<String>(
                    value: b.id,
                    child: Text(b.name),
                  );
                }).toList(),
                onChanged: (val) {
                  setState(() {
                    _selectedBrandId = val;
                  });
                },
              );
            },
          ),
          const SizedBox(height: 24),
          ElevatedButton(
            onPressed: _handleContinue,
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: const Text('Tiếp tục'),
          ),
        ],
      ),
    );
  }
}
