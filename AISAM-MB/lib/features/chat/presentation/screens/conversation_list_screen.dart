import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../providers/conversation_list_provider.dart';
import '../../../profile/presentation/providers/brand_controller.dart';

class ConversationListScreen extends ConsumerWidget {
  const ConversationListScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final listAsync = ref.watch(conversationListNotifierProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('AI Chat'),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () => _startNewChat(context),
          ),
        ],
      ),
      body: listAsync.when(
        data: (items) {
          if (items.isEmpty) {
            return _buildEmptyState(context);
          }
          return RefreshIndicator(
            onRefresh: () => ref.read(conversationListNotifierProvider.notifier).refresh(),
            child: ListView.separated(
              itemCount: items.length,
              separatorBuilder: (context, index) => const Divider(height: 1),
              itemBuilder: (context, index) {
                final conv = items[index];
                return Dismissible(
                  key: Key(conv.id),
                  background: Container(
                    color: Colors.red,
                    alignment: Alignment.centerRight,
                    padding: const EdgeInsets.only(right: 16),
                    child: const Icon(Icons.delete, color: Colors.white),
                  ),
                  direction: DismissDirection.endToStart,
                  onDismissed: (direction) {
                    ref.read(conversationListNotifierProvider.notifier).deleteConversation(conv.id);
                  },
                  child: ListTile(
                    leading: CircleAvatar(
                      backgroundColor: Theme.of(context).colorScheme.primaryContainer,
                      child: Icon(Icons.chat_bubble_outline, color: Theme.of(context).colorScheme.primary),
                    ),
                    title: Text(
                      conv.title ?? 'New Conversation',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontWeight: FontWeight.bold),
                    ),
                    subtitle: Text(
                      conv.lastMessage ?? 'No messages yet',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    trailing: conv.lastMessageAt != null
                        ? Text(
                            '${conv.lastMessageAt!.hour.toString().padLeft(2, '0')}:${conv.lastMessageAt!.minute.toString().padLeft(2, '0')} - ${conv.lastMessageAt!.day}/${conv.lastMessageAt!.month}',
                            style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Colors.grey),
                          )
                        : null,
                    onTap: () {
                      context.push('/chat/${conv.id}');
                    },
                  ),
                );
              },
            ),
          );
        },
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (err, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Error: $err'),
              ElevatedButton(
                onPressed: () => ref.read(conversationListNotifierProvider.notifier).refresh(),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildEmptyState(BuildContext context) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.forum_outlined, size: 64, color: Colors.grey[400]),
          const SizedBox(height: 16),
          Text(
            'No conversations yet',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(color: Colors.grey),
          ),
          const SizedBox(height: 8),
          ElevatedButton(
            onPressed: () => _startNewChat(context),
            child: const Text('Start Chatting'),
          ),
        ],
      ),
    );
  }

  Future<void> _startNewChat(BuildContext context) async {
    final brandId = await showModalBottomSheet<String>(
      context: context,
      isScrollControlled: true,
      builder: (context) => const _ChatBrandPickerSheet(),
    );
    if (brandId == null || !context.mounted) return;
    context.push('/chat/new?brandId=${Uri.encodeQueryComponent(brandId)}');
  }
}

class _ChatBrandPickerSheet extends ConsumerStatefulWidget {
  const _ChatBrandPickerSheet();

  @override
  ConsumerState<_ChatBrandPickerSheet> createState() => _ChatBrandPickerSheetState();
}

class _ChatBrandPickerSheetState extends ConsumerState<_ChatBrandPickerSheet> {
  String? _selectedBrandId;

  @override
  Widget build(BuildContext context) {
    final brands = ref.watch(brandControllerProvider);
    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text('Chọn Brand cho cuộc trò chuyện', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 8),
            const Text('Brand không thể thay đổi sau khi cuộc trò chuyện được tạo.'),
            const SizedBox(height: 16),
            brands.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (error, stackTrace) => const Text('Không thể tải danh sách Brand.'),
              data: (items) => items.isEmpty
                  ? const Text('Bạn chưa có Brand khả dụng trong Workspace này.')
                  : DropdownButtonFormField<String>(
                      initialValue: _selectedBrandId,
                      decoration: const InputDecoration(labelText: 'Brand'),
                      items: items
                          .map((brand) => DropdownMenuItem(value: brand.id, child: Text(brand.name)))
                          .toList(),
                      onChanged: (value) => setState(() => _selectedBrandId = value),
                    ),
            ),
            const SizedBox(height: 20),
            FilledButton(
              onPressed: _selectedBrandId == null
                  ? null
                  : () => Navigator.pop(context, _selectedBrandId),
              child: const Text('Bắt đầu trò chuyện'),
            ),
          ],
        ),
      ),
    );
  }
}
