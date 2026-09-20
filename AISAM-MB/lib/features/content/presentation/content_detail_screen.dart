import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../../core/shared/app_snackbar.dart';
import 'providers/content_editor_controller.dart';
import 'providers/content_permissions.dart';
import '../data/models/content_model.dart';
import '../data/models/enums.dart';
import 'widgets/schedule_post_bottom_sheet.dart';
import 'widgets/mobile_composer.dart';
import '../data/repositories/publishing_repository.dart';
import '../../../core/network/api_client.dart';
import '../../../core/storage/secure_storage.dart';
import '../../workspace/presentation/providers/workspace_controller.dart';

class ContentDetailScreen extends ConsumerWidget {
  final String contentId;
  const ContentDetailScreen({super.key, required this.contentId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    ref.watch(activeWorkspaceControllerProvider);
    final detailState = ref.watch(contentDetailControllerProvider(contentId));
    final permissions = ref.watch(contentPermissionsProvider(contentId)).valueOrNull;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Content Details'),
        actions: [
          if (detailState.hasValue && permissions?[1] == true) IconButton(
            icon: const Icon(Icons.calendar_month, color: Colors.blue),
            onPressed: () {
              showModalBottomSheet(
                context: context,
                isScrollControlled: true,
                backgroundColor: Colors.transparent,
                builder: (context) => SchedulePostBottomSheet(contentId: contentId),
              );
            },
          ),
          if (detailState.hasValue && permissions?[1] == true) IconButton(
            icon: const Icon(Icons.edit),
            onPressed: () => context.push('/content/$contentId/edit'),
          ),
          if (detailState.hasValue && detailState.value?.status == ContentStatusEnum.approved && permissions?[1] == true) IconButton(
            icon: const Icon(Icons.undo, color: Colors.orange),
            tooltip: 'Withdraw Approval',
            onPressed: () async {
              final confirm = await showDialog<bool>(
                context: context,
                builder: (context) => AlertDialog(
                  title: const Text('Withdraw Approval'),
                  content: const Text('Are you sure you want to withdraw approval and return this content to draft?'),
                  actions: [
                    TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancel')),
                    TextButton(
                      onPressed: () => Navigator.pop(context, true),
                      child: const Text('Withdraw', style: TextStyle(color: Colors.orange)),
                    ),
                  ],
                ),
              );

              if (confirm == true && context.mounted) {
                try {
                  await ref.read(contentDetailControllerProvider(contentId).notifier).withdrawContent(contentId);
                  if (context.mounted) {
                    AppSnackbar.showSuccess(context, 'Approval withdrawn. Content returned to draft.');
                  }
                } catch (e) {
                  if (context.mounted) {
                    AppSnackbar.showError(context, e.toString());
                  }
                }
              }
            },
          ),
          if (detailState.hasValue && permissions?[2] == true) IconButton(
            icon: const Icon(Icons.delete, color: Colors.red),
            onPressed: () async {
              final confirm = await showDialog<bool>(
                context: context,
                builder: (context) => AlertDialog(
                  title: const Text('Delete Content'),
                  content: const Text('Are you sure you want to delete this content?'),
                  actions: [
                    TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Cancel')),
                    TextButton(
                      onPressed: () => Navigator.pop(context, true),
                      child: const Text('Delete', style: TextStyle(color: Colors.red)),
                    ),
                  ],
                ),
              );

              if (confirm == true && context.mounted) {
                try {
                  await ref.read(contentDetailControllerProvider(contentId).notifier).deleteContent(contentId);
                  if (context.mounted) {
                    AppSnackbar.showSuccess(context, 'Content deleted successfully.');
                    context.pop();
                  }
                } catch (e) {
                  if (context.mounted) {
                    AppSnackbar.showError(context, e.toString());
                  }
                }
              }
            },
          ),
        ],
      ),
      body: detailState.when(
        data: (content) => permissions == null
            ? const Center(child: CircularProgressIndicator())
            : permissions[0] != true
                ? const Center(child: Text('Không có quyền xem nội dung này trong workspace hiện tại.'))
                : _buildDetail(context, content, ref, permissions[1] == true),
        loading: () => const Center(child: AppLoadingIndicator()),
        error: (error, stack) => Center(child: Text('Error: $error')),
      ),
    );
  }

  Widget _buildDetail(BuildContext context, ContentResponseModel content, WidgetRef ref, bool canEdit) {
    final storage = ref.read(secureStorageProvider);
    final scope = '${storage.cachedUserId}:${storage.cachedWorkspaceId}:${storage.cachedProfileId}';
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          for(final url in content.legacyImageUrls)
            Image.network(url, width: double.infinity, height: 200, fit: BoxFit.cover,
                errorBuilder:(_,error,stack)=>const Text('Image unavailable')),
          const SizedBox(height: 16),
          Text(
            content.title ?? 'Untitled',
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          if (content.creatorName != null && content.creatorName!.isNotEmpty) ...[
            const SizedBox(height: 4),
            Text(
              'By ${content.creatorName}',
              style: TextStyle(fontSize: 13, color: Colors.grey.shade600, fontStyle: FontStyle.italic),
            ),
          ],
          const SizedBox(height: 8),
          Row(
            children: [
              Chip(label: Text(content.status.name.toUpperCase())),
              const SizedBox(width: 8),
              if (content.isAiGenerated)
                const Chip(
                  avatar: Icon(Icons.auto_awesome, color: Colors.purple, size: 16),
                  label: Text('AI Generated'),
                ),
            ],
          ),
          const SizedBox(height: 16),
          const Text('Content:', style: TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          Text(content.textContent),
          if (content.legacyImageUrls.isNotEmpty) ...[
            const SizedBox(height: 16),
            const Text('Images:', style: TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            SizedBox(
              height: 100,
              child: ListView.separated(
                scrollDirection: Axis.horizontal,
                itemCount: content.legacyImageUrls.length,
                separatorBuilder: (_, __) => const SizedBox(width: 8),
                itemBuilder: (context, index) {
                  return ClipRRect(
                    borderRadius: BorderRadius.circular(8),
                    child: Image.network(
                      content.legacyImageUrls[index],
                      width: 100,
                      height: 100,
                      fit: BoxFit.cover,
                      errorBuilder: (_, __, ___) => Container(
                        width: 100,
                        height: 100,
                        color: Colors.grey.shade200,
                        child: const Icon(Icons.broken_image, color: Colors.grey),
                      ),
                    ),
                  );
                },
              ),
            ),
          ],
          MobileComposer(key: ValueKey('$scope:$contentId'), contentId:contentId,scope:scope,
              repository:PublishingRepository(ref.read(dioProvider),isCurrent:()=>scope=='${storage.cachedUserId}:${storage.cachedWorkspaceId}:${storage.cachedProfileId}'),canEdit:canEdit),
        ],
      ),
    );
  }
}
