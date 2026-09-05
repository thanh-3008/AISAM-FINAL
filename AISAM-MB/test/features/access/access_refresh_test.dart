import 'package:aisam_mb/core/network/access_events.dart';
import 'package:aisam_mb/features/access/data/access_context.dart';
import 'package:aisam_mb/features/access/data/access_repository.dart';
import 'package:aisam_mb/features/access/presentation/access_providers.dart';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

class CapabilityRepository extends AccessRepository {
  CapabilityRepository() : super(Dio());
  bool edit = true;
  int calls = 0;
  @override
  Future<Map<String, bool>> actions(String id) async {
    calls++;
    return {'View': true, 'Edit': edit};
  }
  @override
  Future<Map<String, dynamic>> ownAnalytics() async {
    calls++;
    return {'contentCount': 3};
  }
}

void main() {
  for (final reason in ['Brand revoke', 'Channel revoke', 'permission refresh', 'workspace switch']) {
    test('$reason reloads content capabilities from current server state', () async {
      final repository = CapabilityRepository();
      final container = ProviderContainer(overrides: [
        accessRepositoryProvider.overrideWithValue(repository),
        accessContextProvider.overrideWith((ref) async {
          final revision = ref.watch(accessRevisionProvider);
          return AccessContext(workspaceId: reason == 'workspace switch' ? 'w$revision' : 'w',
              userId: 'u', role: 'Manager', version: '$revision');
        }),
      ]);
      addTearDown(container.dispose);
      final subscription = container.listen(contentActionsProvider('content'), (_, _) {});
      addTearDown(subscription.close);
      expect((await container.read(contentActionsProvider('content').future))['Edit'], true);
      repository.edit = false;
      container.read(accessRevisionProvider.notifier).state++;
      await container.pump();
      expect((await container.read(contentActionsProvider('content').future))['Edit'], false);
      expect(repository.calls, 2);
    });
  }

  test('403 invalidates content capability state without an auth dependency', () async {
    final repository = CapabilityRepository();
    final container = ProviderContainer(overrides: [
      accessRepositoryProvider.overrideWithValue(repository),
      accessContextProvider.overrideWith((ref) async => const AccessContext(
          workspaceId: 'w', userId: 'u', role: 'ContentCreator', version: '1')),
    ]);
    addTearDown(container.dispose);
    final subscription = container.listen(contentActionsProvider('content'), (_, _) {});
    addTearDown(subscription.close);
    await container.read(contentActionsProvider('content').future);
    container.read(accessDeniedProvider.notifier).state = true;
    await container.pump();
    await expectLater(container.read(contentActionsProvider('content').future), throwsStateError);
    expect(repository.calls, 1);
  });

  test('Creator to Viewer downgrade clears cached own analytics', () async {
    final repository = CapabilityRepository();
    final container = ProviderContainer(overrides: [
      accessRepositoryProvider.overrideWithValue(repository),
      accessContextProvider.overrideWith((ref) async {
        final allowed = ref.watch(accessRevisionProvider) == 0;
        return AccessContext(workspaceId: 'w', userId: 'u', role: allowed ? 'ContentCreator' : 'Viewer',
            version: '$allowed', canViewOwnAnalytics: allowed);
      }),
    ]);
    addTearDown(container.dispose);
    final subscription = container.listen(ownAnalyticsProvider, (_, _) {});
    addTearDown(subscription.close);
    await container.read(ownAnalyticsProvider.future);
    container.read(accessRevisionProvider.notifier).state++;
    await container.pump();
    await expectLater(container.read(ownAnalyticsProvider.future), throwsStateError);
    expect(repository.calls, 1);
  });
}
