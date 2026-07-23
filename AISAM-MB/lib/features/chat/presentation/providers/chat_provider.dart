import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/chat_repository.dart';
import '../../data/models/conversation_detail_model.dart';
import '../../data/models/chat_message_model.dart';
import '../../data/models/chat_request.dart';
import '../../data/models/chat_sender_type_enum.dart';
import '../../../content/data/models/enums.dart';

part 'chat_provider.g.dart';

class ChatState {
  final ConversationDetailModel? conversation;
  final bool isLoading;
  final bool isTyping;
  final String? error;

  ChatState({
    this.conversation,
    this.isLoading = false,
    this.isTyping = false,
    this.error,
  });

  ChatState copyWith({
    ConversationDetailModel? conversation,
    bool? isLoading,
    bool? isTyping,
    String? error,
  }) {
    return ChatState(
      conversation: conversation ?? this.conversation,
      isLoading: isLoading ?? this.isLoading,
      isTyping: isTyping ?? this.isTyping,
      error: error ?? this.error,
    );
  }
}

@riverpod
class ChatNotifier extends _$ChatNotifier {
  @override
  ChatState build(String? conversationId) {
    if (conversationId != null) {
      _loadHistory(conversationId);
    }
    return ChatState(isLoading: conversationId != null);
  }

  Future<void> _loadHistory(String id) async {
    state = state.copyWith(isLoading: true, error: null);
    try {
      final repository = ref.read(chatRepositoryProvider);
      final detail = await repository.getConversationById(id);
      state = state.copyWith(conversation: detail, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> sendMessage({
    required String message,
    String? brandId,
    String? productId,
    required AdTypeEnum adType,
  }) async {
    // Optimistic UI update
    final userMessage = ChatMessageModel(
      id: DateTime.now().millisecondsSinceEpoch.toString(),
      senderType: ChatSenderTypeEnum.user,
      message: message,
      createdAt: DateTime.now(),
    );

    final currentMessages = state.conversation?.messages.toList() ?? [];
    currentMessages.add(userMessage);

    // Update state to show user message and typing indicator
    state = state.copyWith(
      conversation: state.conversation?.copyWith(messages: currentMessages) ??
          ConversationDetailModel(
            id: state.conversation?.id ?? '', // We might not have an ID yet for new conversations
            profileId: '',
            adType: adType,
            isActive: true,
            messageCount: currentMessages.length,
            messages: currentMessages,
          ),
      isTyping: true,
      error: null,
    );

    try {
      final repository = ref.read(chatRepositoryProvider);
      final response = await repository.sendMessage(ChatRequest(
        brandId: brandId,
        productId: productId,
        adType: adType,
        message: message,
        conversationId: state.conversation?.id.isNotEmpty == true ? state.conversation!.id : null,
      ));

      // Append AI response
      final aiMessage = ChatMessageModel(
        id: DateTime.now().millisecondsSinceEpoch.toString(),
        senderType: ChatSenderTypeEnum.ai,
        message: response.response,
        createdAt: DateTime.now(),
        contentId: response.createdContentId,
      );

      final newMessages = state.conversation!.messages.toList()..add(aiMessage);
      
      // Update state with new conversation ID if it was just created
      state = state.copyWith(
        conversation: state.conversation!.copyWith(
          id: response.conversationId,
          messages: newMessages,
          messageCount: newMessages.length,
        ),
        isTyping: false,
      );
    } catch (e) {
      state = state.copyWith(isTyping: false, error: e.toString());
    }
  }
}
