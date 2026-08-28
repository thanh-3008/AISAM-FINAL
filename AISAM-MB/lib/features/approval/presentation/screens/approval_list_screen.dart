import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_card_swiper/flutter_card_swiper.dart';
import 'package:intl/intl.dart';
import '../providers/approval_provider.dart';
import '../../../content/data/models/enums.dart';
import '../../../content/data/models/content_model.dart';
import 'approval_detail_screen.dart';
import '../../../../core/shared/aisam_logo_widget.dart';
import '../../../../core/shared/profile_avatar_widget.dart';
import '../widgets/reject_reason_dialog.dart';

class ApprovalListScreen extends ConsumerStatefulWidget {
  const ApprovalListScreen({super.key});

  @override
  ConsumerState<ApprovalListScreen> createState() => _ApprovalListScreenState();
}

class _ApprovalListScreenState extends ConsumerState<ApprovalListScreen> {
  final CardSwiperController _swiperController = CardSwiperController();
  bool _showHistory = false;
  final Set<String> _pendingRejectIds = {};

  @override
  void dispose() {
    _swiperController.dispose();
    super.dispose();
  }

  void _showUndoSnackbar(String id, String title) {
    ScaffoldMessenger.of(context).clearSnackBars();
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Đã xử lý: $title'),
        action: SnackBarAction(
          label: 'Hoàn tác',
          onPressed: () async {
            final success = await ref.read(approvalNotifierProvider.notifier).undoContent(id);
            if (success && mounted) {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Đã hoàn tác thành công')),
              );
            }
          },
        ),
        duration: const Duration(seconds: 4),
      ),
    );
  }

  Future<void> _showRejectDialogForSwiper(ContentResponseModel item) async {
    final reason = await RejectReasonDialog.show(context);
    if (reason != null) {
      _pendingRejectIds.add(item.id);
      _swiperController.swipe(CardSwiperDirection.left);
      ref.read(approvalNotifierProvider.notifier).rejectContent(item.id, reason: reason);
      _showUndoSnackbar(item.id, item.title ?? 'Untitled');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.surface.withValues(alpha: 0.8),
        elevation: 0,
        scrolledUnderElevation: 0,
        title: const AisamLogoWidget(),
        actions: [
          IconButton(
            icon: Icon(
              _showHistory ? Icons.fact_check : Icons.history,
              size: 28,
            ),
            color: _showHistory ? Theme.of(context).colorScheme.primary : Theme.of(context).colorScheme.onSurfaceVariant,
            onPressed: () {
              setState(() {
                _showHistory = !_showHistory;
              });
            },
          ),
          const ProfileAvatarWidget(),
          const SizedBox(width: 8),
        ],
      ),
      body: _showHistory ? _buildHistoryTab(context) : _buildPendingTab(context),
    );
  }

  Widget _buildPendingTab(BuildContext context) {
    final pendingAsync = ref.watch(approvalNotifierProvider);

    return pendingAsync.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text('Error: $error', style: const TextStyle(color: Colors.red)),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () => ref.read(approvalNotifierProvider.notifier).refresh(),
              child: const Text('Thử lại'),
            ),
          ],
        ),
      ),
      data: (contents) {
        if (contents.isEmpty) {
          return const Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.done_all, size: 80, color: Colors.green),
                SizedBox(height: 16),
                Text('Tuyệt vời!', style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold)),
                Text('Bạn đã duyệt hết tất cả bài viết hôm nay.', style: TextStyle(color: Colors.grey, fontSize: 16)),
              ],
            ),
          );
        }

        return Column(
          children: [
            // Header Section: Counter & Status
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.secondaryContainer.withValues(alpha: 0.3),
                      borderRadius: BorderRadius.circular(24),
                      border: Border.all(color: Theme.of(context).colorScheme.secondaryContainer.withValues(alpha: 0.5)),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.pending_actions, size: 16, color: Theme.of(context).colorScheme.onSecondaryContainer),
                        const SizedBox(width: 6),
                        Text(
                          '${contents.length} bài chờ duyệt',
                          style: Theme.of(context).textTheme.labelLarge?.copyWith(
                                color: Theme.of(context).colorScheme.onSecondaryContainer,
                                fontWeight: FontWeight.bold,
                              ),
                        ),
                      ],
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.filter_list),
                    color: Theme.of(context).colorScheme.onSurfaceVariant,
                    onPressed: () {
                      // Show filter logic
                    },
                  ),
                ],
              ),
            ),

            // Approval Stack Area
            Expanded(
              child: Padding(
                padding: const EdgeInsets.only(top: 16.0, bottom: 24.0),
                child: CardSwiper(
                  controller: _swiperController,
                  cardsCount: contents.length,
                  allowedSwipeDirection: const AllowedSwipeDirection.symmetric(horizontal: true),
                  onSwipe: (previousIndex, currentIndex, direction) {
                    final item = contents[previousIndex];
                    if (direction == CardSwiperDirection.right) {
                      ref.read(approvalNotifierProvider.notifier).approveContent(item.id);
                      _showUndoSnackbar(item.id, item.title ?? 'Untitled');
                      return true;
                    } else if (direction == CardSwiperDirection.left) {
                      if (_pendingRejectIds.contains(item.id)) {
                        _pendingRejectIds.remove(item.id);
                        return true;
                      } else {
                        _showRejectDialogForSwiper(item);
                        return false;
                      }
                    }
                    return true;
                  },
                  numberOfCardsDisplayed: contents.length > 2 ? 3 : contents.length,
                  backCardOffset: const Offset(0, 30),
                  padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 0),
                  cardBuilder: (context, index, percentThresholdX, percentThresholdY) {
                    final item = contents[index];
                    return _buildApprovalCard(context, item);
                  },
                ),
              ),
            ),

            // Action Buttons Area
            Padding(
              padding: const EdgeInsets.only(bottom: 24.0),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // Reject Button
                  Container(
                    width: 72,
                    height: 72,
                    decoration: BoxDecoration(
                      color: Colors.white,
                      shape: BoxShape.circle,
                      border: Border.all(color: Theme.of(context).colorScheme.errorContainer, width: 2),
                      boxShadow: [
                        BoxShadow(
                          color: Theme.of(context).colorScheme.error.withValues(alpha: 0.15),
                          blurRadius: 24,
                          offset: const Offset(0, 8),
                        ),
                      ],
                    ),
                    child: IconButton(
                      icon: const Icon(Icons.close),
                      color: Theme.of(context).colorScheme.error,
                      iconSize: 36,
                      onPressed: () => _swiperController.swipe(CardSwiperDirection.left),
                    ),
                  ),
                  const SizedBox(width: 24),

                  // Info / Comment Button
                  Container(
                    width: 48,
                    height: 48,
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.surfaceContainerHigh,
                      shape: BoxShape.circle,
                      boxShadow: [
                        BoxShadow(
                          color: Colors.black.withValues(alpha: 0.05),
                          blurRadius: 8,
                          offset: const Offset(0, 2),
                        ),
                      ],
                    ),
                    child: IconButton(
                      icon: const Icon(Icons.chat_bubble_outline),
                      color: Theme.of(context).colorScheme.onSurfaceVariant,
                      iconSize: 24,
                      onPressed: () => _swiperController.swipe(CardSwiperDirection.left),
                    ),
                  ),
                  const SizedBox(width: 24),

                  // Approve Button
                  Container(
                    width: 72,
                    height: 72,
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.primary,
                      shape: BoxShape.circle,
                      border: Border.all(color: Theme.of(context).colorScheme.primary, width: 2),
                      boxShadow: [
                        BoxShadow(
                          color: Theme.of(context).colorScheme.primary.withValues(alpha: 0.25),
                          blurRadius: 24,
                          offset: const Offset(0, 8),
                        ),
                      ],
                    ),
                    child: IconButton(
                      icon: const Icon(Icons.check),
                      color: Theme.of(context).colorScheme.onPrimary,
                      iconSize: 36,
                      onPressed: () => _swiperController.swipe(CardSwiperDirection.right),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
          ],
        );
      },
    );
  }

  Widget _buildApprovalCard(BuildContext context, ContentResponseModel item) {
    return GestureDetector(
      onTap: () {
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => ApprovalDetailScreen(content: item),
          ),
        );
      },
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withValues(alpha: 0.2)),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.06),
              blurRadius: 24,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Card Header
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: Row(
                      children: [
                        Container(
                          width: 40,
                          height: 40,
                          decoration: BoxDecoration(
                            color: Theme.of(context).colorScheme.surfaceContainerHigh,
                            shape: BoxShape.circle,
                            border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withValues(alpha: 0.2)),
                          ),
                          child: const Icon(Icons.business, color: Colors.grey),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                item.brandName ?? 'Unknown Brand',
                                style: Theme.of(context).textTheme.titleSmall?.copyWith(fontWeight: FontWeight.bold),
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                              ),
                              Row(
                                children: [
                                  Container(
                                    width: 8,
                                    height: 8,
                                    decoration: BoxDecoration(
                                      color: Colors.pink.shade500,
                                      shape: BoxShape.circle,
                                    ),
                                  ),
                                  const SizedBox(width: 4),
                                  Text(
                                    item.adType == AdTypeEnum.videoText ? 'Video Ad' : 'Image Ad',
                                    style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.surfaceContainer,
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Row(
                      children: [
                        Icon(Icons.calendar_month, size: 14, color: Theme.of(context).colorScheme.onSurfaceVariant),
                        const SizedBox(width: 4),
                        Text(
                          DateFormat('dd/MM, HH:mm').format(item.createdAt.toLocal()),
                          style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            
            // Card Image Preview
            Expanded(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                child: Container(
                  width: double.infinity,
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(16),
                    color: Theme.of(context).colorScheme.surfaceContainerHighest,
                  ),
                  clipBehavior: Clip.hardEdge,
                  child: Stack(
                    fit: StackFit.expand,
                    children: [
                      if (item.imageUrl != null)
                        Image.network(item.imageUrl!, fit: BoxFit.cover)
                      else
                        Center(
                          child: Icon(
                            item.adType == AdTypeEnum.videoText ? Icons.videocam : Icons.image,
                            size: 64,
                            color: Colors.grey,
                          ),
                        ),
                      // AI Badge overlay
                      Positioned(
                        bottom: 12,
                        right: 12,
                        child: Container(
                          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                          decoration: BoxDecoration(
                            color: Theme.of(context).colorScheme.surface.withValues(alpha: 0.9),
                            borderRadius: BorderRadius.circular(20),
                            border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withValues(alpha: 0.3)),
                            boxShadow: [
                              BoxShadow(color: Colors.black.withValues(alpha: 0.1), blurRadius: 4),
                            ],
                          ),
                          child: Row(
                            children: [
                              Icon(Icons.auto_awesome, size: 16, color: Theme.of(context).colorScheme.primary),
                              const SizedBox(width: 6),
                              Text(
                                'AI Tạo',
                                style: Theme.of(context).textTheme.labelSmall?.copyWith(
                                      color: Theme.of(context).colorScheme.primary,
                                      fontWeight: FontWeight.bold,
                                    ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
            
            // Card Content/Caption Preview
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    item.textContent,
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                          color: Theme.of(context).colorScheme.onSurface,
                          height: 1.5,
                        ),
                    maxLines: 3,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 8),
                  InkWell(
                    onTap: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (context) => ApprovalDetailScreen(content: item),
                        ),
                      );
                    },
                    child: Text(
                      'Xem chi tiết',
                      style: Theme.of(context).textTheme.labelLarge?.copyWith(
                            color: Theme.of(context).colorScheme.primary,
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildHistoryTab(BuildContext context) {
    final historyAsync = ref.watch(historyApprovalNotifierProvider);

    return historyAsync.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(child: Text('Error: $error')),
      data: (contents) {
        if (contents.isEmpty) {
          return const Center(
            child: Text('Chưa có lịch sử duyệt.', style: TextStyle(color: Colors.grey)),
          );
        }
        return RefreshIndicator(
          onRefresh: () async {
            ref.read(historyApprovalNotifierProvider.notifier).refresh();
          },
          child: ListView.separated(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
            itemCount: contents.length,
            separatorBuilder: (context, index) => const SizedBox(height: 12),
            itemBuilder: (context, index) {
              final item = contents[index];
              final isApproved = item.status == ContentStatusEnum.approved;
              
              return Container(
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.surface,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withValues(alpha: 0.3)),
                ),
                child: ListTile(
                  contentPadding: const EdgeInsets.all(12),
                  leading: CircleAvatar(
                    backgroundColor: isApproved ? Colors.green.shade50 : Colors.red.shade50,
                    child: Icon(
                      isApproved ? Icons.check_circle : Icons.cancel,
                      color: isApproved ? Colors.green : Colors.red,
                    ),
                  ),
                  title: Text(item.title ?? 'Untitled', maxLines: 1, overflow: TextOverflow.ellipsis, style: const TextStyle(fontWeight: FontWeight.bold)),
                  subtitle: Text('${item.brandName} • ${DateFormat('dd/MM HH:mm').format(item.updatedAt.toLocal())}'),
                  trailing: IconButton(
                    icon: const Icon(Icons.undo),
                    onPressed: () {
                      _showUndoSnackbar(item.id, item.title ?? 'Untitled');
                    },
                  ),
                  onTap: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (context) => ApprovalDetailScreen(content: item),
                      ),
                    );
                  },
                ),
              );
            },
          ),
        );
      },
    );
  }
}
