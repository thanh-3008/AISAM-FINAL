import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../../core/shared/empty_state_widget.dart';
import 'providers/content_list_controller.dart';
import '../data/models/content_model.dart';
import '../data/models/enums.dart';
import '../../access/presentation/access_providers.dart';

class ContentListScreen extends ConsumerStatefulWidget {
  const ContentListScreen({super.key});

  @override
  ConsumerState<ContentListScreen> createState() => _ContentListScreenState();
}

class _ContentListScreenState extends ConsumerState<ContentListScreen> {
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent - 200) {
      ref.read(contentListControllerProvider.notifier).loadMore();
    }
  }

  @override
  Widget build(BuildContext context) {
    final contentState = ref.watch(contentListControllerProvider);
    final canCreate = ref.watch(accessContextProvider).valueOrNull?.canCreateContent == true;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Content Management'),
        actions: [if (canCreate) ...[
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () => context.push('/content/create'),
          ),
          IconButton(
            icon: const Icon(Icons.auto_awesome),
            tooltip: 'AI Generate Draft',
            onPressed: () => context.push('/content/generate-ai'),
          ),
        ]],
      ),
      body: contentState.when(
        data: (contents) {
          if (contents.isEmpty) {
            return const EmptyStateWidget(
              title: 'No Content Found',
              message: 'Start by creating your first content or generate with AI.',
              icon: Icons.article,
            );
          }
          return RefreshIndicator(
            onRefresh: () => ref.read(contentListControllerProvider.notifier).refresh(),
            child: ListView.separated(
              controller: _scrollController,
              itemCount: contents.length,
              separatorBuilder: (_, __) => const Divider(),
              itemBuilder: (context, index) {
                final content = contents[index];
                return _ContentListItem(content: content);
              },
            ),
          );
        },
        loading: () => const Center(child: AppLoadingIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Error: $error', textAlign: TextAlign.center),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () => ref.read(contentListControllerProvider.notifier).refresh(),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ContentListItem extends StatelessWidget {
  final ContentResponseModel content;

  const _ContentListItem({required this.content});

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: content.imageUrl != null
          ? Image.network(content.imageUrl!, width: 50, height: 50, fit: BoxFit.cover)
          : const Icon(Icons.article, size: 40),
      title: Text(content.title ?? 'Untitled Content'),
      subtitle: Text(
        content.textContent,
        maxLines: 2,
        overflow: TextOverflow.ellipsis,
      ),
      trailing: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          if (content.isAiGenerated)
            const Icon(Icons.auto_awesome, color: Colors.purple, size: 16),
          const SizedBox(height: 4),
          _buildStatusBadge(content.status),
        ],
      ),
      onTap: () {
        context.push('/content/${content.id}');
      },
    );
  }

  Widget _buildStatusBadge(ContentStatusEnum status) {
    Color color;
    String text;
    switch (status) {
      case ContentStatusEnum.draft:
        color = Colors.grey;
        text = 'Draft';
        break;
      case ContentStatusEnum.pendingApproval:
        color = Colors.orange;
        text = 'Pending';
        break;
      case ContentStatusEnum.approved:
        color = Colors.blue;
        text = 'Approved';
        break;
      case ContentStatusEnum.rejected:
        color = Colors.red;
        text = 'Rejected';
        break;
      case ContentStatusEnum.published:
        color = Colors.green;
        text = 'Published';
        break;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(4),
        border: Border.all(color: color),
      ),
      child: Text(
        text,
        style: TextStyle(color: color, fontSize: 10, fontWeight: FontWeight.bold),
      ),
    );
  }
}
