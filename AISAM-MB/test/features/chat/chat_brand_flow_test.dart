import 'package:aisam_mb/features/chat/data/models/chat_request.dart';
import 'package:aisam_mb/features/chat/data/models/chat_response.dart';
import 'package:aisam_mb/features/chat/data/models/conversation_detail_model.dart';
import 'package:aisam_mb/features/chat/data/repositories/chat_repository.dart';
import 'package:aisam_mb/features/chat/presentation/providers/chat_provider.dart';
import 'package:aisam_mb/features/chat/presentation/screens/chat_screen.dart';
import 'package:aisam_mb/features/content/data/models/enums.dart';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets(
    'new conversation sends the selected Brand and keeps it for follow-up',
    (tester) async {
      final repository = _FakeChatRepository();
      await tester.pumpWidget(
        ProviderScope(
          overrides: [chatRepositoryProvider.overrideWithValue(repository)],
          child: const MaterialApp(
            home: ChatScreen(initialBrandId: 'brand-new'),
          ),
        ),
      );

      await tester.enterText(find.byType(TextField), 'first message');
      await tester.pump();
      await tester.tap(find.byIcon(Icons.send));
      await tester.pumpAndSettle();

      expect(repository.requests.single.brandId, 'brand-new');

      await tester.enterText(find.byType(TextField), 'follow-up');
      await tester.pump();
      await tester.tap(find.byIcon(Icons.send));
      await tester.pumpAndSettle();

      expect(repository.requests, hasLength(2));
      expect(repository.requests.last.brandId, 'brand-new');
    },
  );

  testWidgets('chat cannot send a request when no Brand is available', (
    tester,
  ) async {
    final repository = _FakeChatRepository();
    await tester.pumpWidget(
      ProviderScope(
        overrides: [chatRepositoryProvider.overrideWithValue(repository)],
        child: const MaterialApp(home: ChatScreen()),
      ),
    );

    await tester.enterText(find.byType(TextField), 'message without brand');
    await tester.pump();
    await tester.tap(find.byIcon(Icons.send));
    await tester.pumpAndSettle();

    expect(repository.requests, isEmpty);
    expect(
      find.text('Vui lòng chọn Brand trước khi gửi tin nhắn.'),
      findsOneWidget,
    );
  });

  test('existing conversation reply reuses its loaded Brand', () async {
    final repository = _FakeChatRepository(
      conversation: _conversation(
        id: 'conversation-existing',
        brandId: 'brand-existing',
      ),
    );
    final container = ProviderContainer(
      overrides: [chatRepositoryProvider.overrideWithValue(repository)],
    );
    final subscription = container.listen(
      chatNotifierProvider('conversation-existing'),
      (previous, next) {},
      fireImmediately: true,
    );
    addTearDown(() {
      subscription.close();
      container.dispose();
    });
    await pumpEventQueue();

    await container
        .read(chatNotifierProvider('conversation-existing').notifier)
        .sendMessage(message: 'existing reply', adType: AdTypeEnum.textOnly);

    expect(repository.requests.single.brandId, 'brand-existing');
    expect(repository.requests.single.conversationId, 'conversation-existing');
  });
}

class _FakeChatRepository extends ChatRepository {
  _FakeChatRepository({this.conversation}) : super(Dio());

  final ConversationDetailModel? conversation;
  final List<ChatRequest> requests = [];

  @override
  Future<ConversationDetailModel> getConversationById(String id) async =>
      conversation ?? _conversation(id: id, brandId: 'brand-default');

  @override
  Future<ChatResponse> sendMessage(ChatRequest request) async {
    requests.add(request);
    return ChatResponse(
      response: 'AI response',
      conversationId: request.conversationId ?? 'conversation-new',
      shouldCreateContent: false,
    );
  }
}

ConversationDetailModel _conversation({
  required String id,
  required String brandId,
}) => ConversationDetailModel(
  id: id,
  profileId: 'profile',
  brandId: brandId,
  adType: AdTypeEnum.textOnly,
  isActive: true,
  messageCount: 0,
);
