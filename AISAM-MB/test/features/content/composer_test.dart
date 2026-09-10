import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:aisam_mb/features/content/data/repositories/publishing_repository.dart';
import 'package:aisam_mb/features/content/presentation/widgets/mobile_composer.dart';

class FakePublishing extends PublishingRepository {
  int publishes = 0, reads = 0;
  List<Map<String, dynamic>>? saved;
  FakePublishing() : super(Dio());
  @override
  Future<Map<String, dynamic>> media(String id) async => {
    'version': 'v',
    'items': [
      {'assetId': 'a', 'url': 'first', 'mimeType': 'image/png'},
      {'assetId': 'b', 'url': 'second', 'mimeType': 'image/png'},
    ],
  };
  @override
  Future<Map<String, dynamic>> preview(String id) async => {
    'destinations': [
      {'id': 'channel', 'name': 'Page', 'platform': 'Facebook', 'error': null},
      {
        'id': 'denied',
        'name': 'Denied',
        'platform': 'Facebook',
        'error': 'ACCESS_DENIED_CHANNEL',
      },
    ],
  };
  @override
  Future<Map<String, dynamic>> saveMedia(
    String id,
    String version,
    List<Map<String, dynamic>> items,
  ) async {
    saved = List.of(items);
    return media(id);
  }

  @override
  Future<List<dynamic>> publish(
    String id,
    String version,
    List<String> destinations,
    String key,
  ) async {
    publishes++;
    throw StateError('Uncertain network outcome');
  }

  @override
  Future<List<dynamic>> operations(String id, String key) async {
    reads++;
    return [];
  }
}

Widget app(FakePublishing repo, {bool edit = true}) => MaterialApp(
  home: Scaffold(
    body: SingleChildScrollView(
      child: MobileComposer(
        contentId: 'id',
        scope: 'actor:workspace',
        repository: repo,
        canEdit: edit,
      ),
    ),
  ),
);
void main() {
  setUp(() => SharedPreferences.setMockInitialValues({}));
  testWidgets('reorder requires save and sends reordered assets', (
    tester,
  ) async {
    final repo = FakePublishing();
    await tester.pumpWidget(app(repo));
    await tester.pumpAndSettle();
    await tester.tap(find.byTooltip('Move up').last);
    await tester.pump();
    await tester.tap(find.text('Save media'));
    await tester.pumpAndSettle();
    expect(repo.saved!.map((e) => e['assetId']), ['b', 'a']);
  });
  testWidgets(
    'read-only user cannot mutate media and denied channel is disabled',
    (tester) async {
      await tester.pumpWidget(app(FakePublishing(), edit: false));
      await tester.pumpAndSettle();
      expect(find.text('Add images'), findsNothing);
      final denied = tester.widget<CheckboxListTile>(
        find.widgetWithText(CheckboxListTile, 'Denied (Facebook)'),
      );
      expect(denied.onChanged, isNull);
    },
  );
  testWidgets('unknown publish keeps journal after remount and never replays', (
    tester,
  ) async {
    final repo = FakePublishing();
    await tester.pumpWidget(app(repo));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Page (Facebook)'));
    await tester.pump();
    await tester.ensureVisible(find.text('Publish selected'));
    await tester.tap(find.text('Publish selected'));
    await tester.pumpAndSettle();
    expect(repo.publishes, 1);
    expect(find.text('Publish selected'), findsNothing);
    final prefs = await SharedPreferences.getInstance();
    expect(prefs.getString('publish:actor:workspace:id'), isNotNull);
    await tester.pumpWidget(const SizedBox());
    await tester.pumpWidget(app(repo));
    await tester.pumpAndSettle();
    expect(repo.reads, 1);
    expect(repo.publishes, 1);
    expect(find.text('Publish selected'), findsNothing);
  });
}
