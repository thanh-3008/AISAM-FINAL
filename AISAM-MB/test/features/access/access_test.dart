import 'package:aisam_mb/features/access/data/access_context.dart';
import 'package:aisam_mb/features/access/data/access_repository.dart';
import 'package:aisam_mb/features/access/presentation/access_providers.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:dio/dio.dart';

class FakeAccessRepository extends AccessRepository {
  int analyticsCalls = 0;
  FakeAccessRepository() : super(Dio());
  @override
  Future<Map<String, dynamic>> ownAnalytics() async {
    analyticsCalls++;
    return {'contentCount': 2};
  }
}

void main() {
  test('missing capability flags never grant access from role alone', () {
    final access = AccessContext.fromJson({'workspaceId': 'w', 'userId': 'u', 'role': 'Owner', 'version': '1'});
    expect(access.canViewAnalytics, isFalse);
    expect(access.canPublish, isFalse);
    expect(access.canCreateContent, isFalse);
  });

  for (final allowed in [false, true]) {
    test('own analytics follows current server capability: $allowed', () async {
      final repository = FakeAccessRepository();
      final container = ProviderContainer(overrides: [
        accessContextProvider.overrideWith((ref) async => AccessContext(
          workspaceId: 'w', userId: 'u', role: allowed ? 'ContentCreator' : 'Viewer',
          version: '1', canViewOwnAnalytics: allowed)),
        accessRepositoryProvider.overrideWithValue(repository),
      ]);
      addTearDown(container.dispose);
      if (allowed) {
        expect((await container.read(ownAnalyticsProvider.future))['contentCount'], 2);
        expect(repository.analyticsCalls, 1);
      } else {
        await expectLater(container.read(ownAnalyticsProvider.future), throwsStateError);
        expect(repository.analyticsCalls, 0);
      }
    });
  }
}
