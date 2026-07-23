import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/chat_repository.dart';
import '../../data/models/conversation_model.dart';
part 'conversation_list_provider.g.dart';

@riverpod
class ConversationListNotifier extends _$ConversationListNotifier {
  @override
  Future<List<ConversationModel>> build() async {
    return _fetchConversations();
  }

  Future<List<ConversationModel>> _fetchConversations({String? searchTerm}) async {
    final repository = ref.read(chatRepositoryProvider);
    return repository.getConversations(page: 1, pageSize: 50, searchTerm: searchTerm);
  }

  Future<void> refresh({String? searchTerm}) async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchConversations(searchTerm: searchTerm));
  }

  Future<bool> deleteConversation(String id) async {
    try {
      final repository = ref.read(chatRepositoryProvider);
      await repository.deleteConversation(id);
      await refresh();
      return true;
    } catch (e) {
      return false;
    }
  }
}
