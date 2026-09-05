import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../../core/shared/app_snackbar.dart';
import 'providers/content_editor_controller.dart';
import '../data/models/content_model.dart';
import 'widgets/schedule_post_bottom_sheet.dart';
import '../../access/presentation/access_providers.dart';

class ContentDetailScreen extends ConsumerWidget {
  final String contentId;
  const ContentDetailScreen({super.key, required this.contentId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detailState = ref.watch(contentDetailControllerProvider(contentId));
    final actions = ref.watch(contentActionsProvider(contentId)).valueOrNull ?? const <String, bool>{};

    return Scaffold(
      appBar: AppBar(
        title: const Text('Content Details'),
        actions: [
          if (actions['Schedule'] == true)
          IconButton(
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
          if (actions['Edit'] == true) IconButton(
            icon: const Icon(Icons.edit),
            onPressed: () => context.push('/content/$contentId/edit'),
          ),
          if (actions['Delete'] == true) IconButton(
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
        data: (content) => _buildDetail(context, content),
        loading: () => const Center(child: AppLoadingIndicator()),
        error: (error, stack) => Center(child: Text('Error: $error')),
      ),
    );
  }

  Widget _buildDetail(BuildContext context, ContentResponseModel content) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (content.imageUrl != null)
            Image.network(content.imageUrl!, width: double.infinity, height: 200, fit: BoxFit.cover),
          const SizedBox(height: 16),
          Text(
            content.title ?? 'Untitled',
            style: Theme.of(context).textTheme.headlineSmall,
          ),
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
        ],
      ),
    );
  }
}
