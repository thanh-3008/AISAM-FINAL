import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/chat_provider.dart';
import '../widgets/chat_bubble.dart';
import '../widgets/message_input.dart';
import '../widgets/typing_indicator.dart';
import '../../../content/data/models/enums.dart'; // For AdTypeEnum fallback

class ChatScreen extends ConsumerStatefulWidget {
  final String? conversationId;
  final String? initialBrandId;

  const ChatScreen({super.key, this.conversationId, this.initialBrandId});

  @override
  ConsumerState<ChatScreen> createState() => _ChatScreenState();
}

class _ChatScreenState extends ConsumerState<ChatScreen> {
  final ScrollController _scrollController = ScrollController();

  void _scrollToBottom() {
    if (_scrollController.hasClients) {
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 300),
        curve: Curves.easeOut,
      );
    }
  }

  void _handleSend(String text) {
    ref.read(chatNotifierProvider(widget.conversationId).notifier).sendMessage(
      message: text,
      brandId: widget.initialBrandId,
      adType: AdTypeEnum.textOnly, // Fallback, could prompt user to choose
    );
    Future.delayed(const Duration(milliseconds: 100), _scrollToBottom);
  }

  @override
  Widget build(BuildContext context) {
    final chatState = ref.watch(chatNotifierProvider(widget.conversationId));
    
    // Auto scroll when new messages appear
    ref.listen(chatNotifierProvider(widget.conversationId), (previous, next) {
      if (previous?.conversation?.messages.length != next.conversation?.messages.length) {
        Future.delayed(const Duration(milliseconds: 100), _scrollToBottom);
      }
    });

    return Scaffold(
      appBar: AppBar(
        title: Text(chatState.conversation?.title ?? 'AI Chat'),
      ),
      body: Column(
        children: [
          Expanded(
            child: chatState.isLoading && (chatState.conversation?.messages.isEmpty ?? true)
                ? const Center(child: CircularProgressIndicator())
                : ListView.builder(
                    controller: _scrollController,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    itemCount: (chatState.conversation?.messages.length ?? 0) + (chatState.isTyping ? 1 : 0),
                    itemBuilder: (context, index) {
                      final messages = chatState.conversation?.messages ?? [];
                      if (index < messages.length) {
                        return ChatBubble(message: messages[index]);
                      } else {
                        return const TypingIndicator();
                      }
                    },
                  ),
          ),
          if (chatState.error != null)
            Padding(
              padding: const EdgeInsets.all(8.0),
              child: Text(
                chatState.error!,
                style: const TextStyle(color: Colors.red),
                textAlign: TextAlign.center,
              ),
            ),
          MessageInput(
            onSend: _handleSend,
            isLoading: chatState.isTyping || chatState.isLoading,
          ),
        ],
      ),
    );
  }
}
