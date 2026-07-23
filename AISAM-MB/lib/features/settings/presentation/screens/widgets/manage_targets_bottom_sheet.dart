import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../profile/presentation/providers/brand_controller.dart';
import '../../../../profile/data/models/brand_model.dart';
import '../../../data/models/available_target_model.dart';
import '../../../data/repositories/social_repository.dart';
import '../../../../../core/shared/app_snackbar.dart';

final availableTargetsProvider = FutureProvider.autoDispose.family<List<AvailableTargetModel>, String>((ref, accountId) async {
  final repository = ref.read(socialRepositoryProvider);
  return repository.getAvailableTargets(accountId);
});

class ManageTargetsBottomSheet extends ConsumerStatefulWidget {
  final String accountId;
  final String platform;
  final String? preselectedBrandId;

  const ManageTargetsBottomSheet({
    super.key,
    required this.accountId,
    required this.platform,
    this.preselectedBrandId,
  });

  @override
  ConsumerState<ManageTargetsBottomSheet> createState() => _ManageTargetsBottomSheetState();
}

class _ManageTargetsBottomSheetState extends ConsumerState<ManageTargetsBottomSheet> {
  String? _selectedBrandId;
  final Set<String> _selectedTargetIds = {};
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    _selectedBrandId = widget.preselectedBrandId;
  }

  Future<void> _handleSave() async {
    if (_selectedBrandId == null) {
      AppSnackbar.showError(context, 'Vui lòng chọn một thương hiệu (Brand)');
      return;
    }
    if (_selectedTargetIds.isEmpty) {
      AppSnackbar.showError(context, 'Vui lòng chọn ít nhất một trang/profile');
      return;
    }

    setState(() => _isSaving = true);
    try {
      final repository = ref.read(socialRepositoryProvider);
      await repository.linkTargets(
        widget.accountId,
        _selectedTargetIds.toList(),
        _selectedBrandId!,
        widget.platform,
      );
      
      if (mounted) {
        AppSnackbar.showSuccess(context, 'Đã gán tài khoản thành công!');
        Navigator.pop(context, true);
      }
    } catch (e) {
      if (mounted) {
        AppSnackbar.showError(context, 'Lỗi khi gán tài khoản: $e');
      }
    } finally {
      if (mounted) {
        setState(() => _isSaving = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final brandsAsync = ref.watch(brandControllerProvider);
    final targetsAsync = ref.watch(availableTargetsProvider(widget.accountId));

    return Container(
      padding: EdgeInsets.only(
        left: 24,
        right: 24,
        top: 24,
        bottom: MediaQuery.of(context).viewInsets.bottom + 24,
      ),
      constraints: BoxConstraints(maxHeight: MediaQuery.of(context).size.height * 0.8),
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
                'Quản lý Trang / Profile',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              IconButton(
                icon: const Icon(Icons.close),
                onPressed: () => Navigator.pop(context),
              )
            ],
          ),
          const SizedBox(height: 16),
          const Text('Chọn thương hiệu (Brand) để gán', style: TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          brandsAsync.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (e, st) => Text('Lỗi tải danh sách Brand: $e', style: const TextStyle(color: Colors.red)),
            data: (brands) {
              if (brands.isEmpty) {
                return const Text('Bạn chưa có Brand nào. Hãy tạo Brand trước.');
              }
              return DropdownButtonFormField<String>(
                initialValue: _selectedBrandId,
                decoration: InputDecoration(
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                  contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                  fillColor: widget.preselectedBrandId != null ? Theme.of(context).colorScheme.surfaceContainerHighest : null,
                  filled: widget.preselectedBrandId != null,
                ),
                hint: const Text('Chọn một thương hiệu'),
                items: brands.map((BrandResponseModel b) {
                  return DropdownMenuItem<String>(
                    value: b.id,
                    child: Text(b.name),
                  );
                }).toList(),
                onChanged: widget.preselectedBrandId != null ? null : (val) {
                  setState(() {
                    _selectedBrandId = val;
                  });
                },
              );
            },
          ),
          const SizedBox(height: 24),
          const Text('Chọn các trang (Targets) muốn liên kết', style: TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          
          Expanded(
            child: targetsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (e, st) => Center(child: Text('Lỗi tải danh sách trang: $e', style: const TextStyle(color: Colors.red))),
              data: (targets) {
                if (targets.isEmpty) {
                  return const Center(child: Text('Không tìm thấy trang/profile nào khả dụng.'));
                }
                return ListView.builder(
                  shrinkWrap: true,
                  itemCount: targets.length,
                  itemBuilder: (context, index) {
                    final target = targets[index];
                    final isLinked = target.linkedBrandId != null;
                    
                    return CheckboxListTile(
                      value: _selectedTargetIds.contains(target.providerTargetId) || isLinked,
                      onChanged: isLinked ? null : (bool? val) {
                        setState(() {
                          if (val == true) {
                            _selectedTargetIds.add(target.providerTargetId);
                          } else {
                            _selectedTargetIds.remove(target.providerTargetId);
                          }
                        });
                      },
                      title: Text(target.name),
                      subtitle: isLinked ? Text('Đã gán cho Brand: ${target.linkedBrandName}') : null,
                      secondary: target.profilePictureUrl != null
                          ? CircleAvatar(backgroundImage: NetworkImage(target.profilePictureUrl!))
                          : const CircleAvatar(child: Icon(Icons.public)),
                    );
                  },
                );
              },
            ),
          ),
          
          const SizedBox(height: 16),
          ElevatedButton(
            onPressed: _isSaving ? null : _handleSave,
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: _isSaving 
                ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
                : const Text('Lưu kết nối'),
          ),
        ],
      ),
    );
  }
}
