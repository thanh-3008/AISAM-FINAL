import 'dart:async';
import 'package:aisam_mb/core/network/access_events.dart';
import 'package:aisam_mb/features/access/data/access_context.dart';
import 'package:aisam_mb/features/access/presentation/access_providers.dart';
import 'package:aisam_mb/features/profile/data/models/brand_model.dart';
import 'package:aisam_mb/features/profile/data/repositories/brand_repository.dart';
import 'package:aisam_mb/features/profile/presentation/providers/brand_controller.dart';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

class DeferredBrands extends BrandRepository {
  DeferredBrands() : super(Dio());
  final requests = <Completer<List<BrandResponseModel>>>[];
  @override
  Future<List<BrandResponseModel>> getBrands() {
    final request = Completer<List<BrandResponseModel>>();
    requests.add(request);
    return request.future;
  }
}

BrandResponseModel brand(String id) => BrandResponseModel(id: id, userId: 'u', name: id,
    createdAt: DateTime.utc(2026), updatedAt: DateTime.utc(2026), productsCount: 0, contentsCount: 0);

void main() {
  for (final reason in ['workspace switch', 'Brand revoke', 'Channel revoke', 'Manager scope reduction', 'Owner to Viewer']) {
    test('$reason discards an older protected Brand response', () async {
      final repository = DeferredBrands();
      final container = ProviderContainer(overrides: [
        brandRepositoryProvider.overrideWithValue(repository),
        accessContextProvider.overrideWith((ref) async {
          final revision = ref.watch(accessRevisionProvider);
          return AccessContext(workspaceId: reason == 'workspace switch' ? '$revision' : 'w', userId: 'u', role: reason == 'Owner to Viewer' ? (revision == 0 ? 'Owner' : 'Viewer') : 'Manager', version: '$revision');
        }),
      ]);
      addTearDown(container.dispose);
      final listener = container.listen(brandControllerProvider, (_, _) {});
      addTearDown(listener.close);
      await container.read(accessContextProvider.future);
      await Future<void>.delayed(Duration.zero);
      await container.pump();
      expect(repository.requests, isNotEmpty, reason: container.read(brandControllerProvider).toString());
      final oldRequests = [...repository.requests];
      container.read(accessRevisionProvider.notifier).state++;
      await container.pump();
      await container.read(accessContextProvider.future);
      await Future<void>.delayed(Duration.zero);
      await container.pump();
      expect(container.read(brandControllerProvider).isLoading, true);
      final newRequests = repository.requests.skip(oldRequests.length).toList();
      expect(newRequests, isNotEmpty);
      for (final request in newRequests) { request.complete([brand('current')]); }
      await container.pump();
      for (final request in oldRequests) { request.complete([brand('revoked')]); }
      await container.pump();
      expect(container.read(brandControllerProvider).requireValue.single.id, 'current');
    });
  }

  test('403 clears Brand state and rejects a response already in flight', () async {
    final repository = DeferredBrands();
    final container = ProviderContainer(overrides: [
      brandRepositoryProvider.overrideWithValue(repository),
      accessContextProvider.overrideWith((ref) async => const AccessContext(workspaceId: 'w', userId: 'u', role: 'Manager', version: '1')),
    ]);
    addTearDown(container.dispose);
    final listener = container.listen(brandControllerProvider, (_, _) {});
    addTearDown(listener.close);
    await container.pump();
    await container.read(accessContextProvider.future);
    await Future<void>.delayed(Duration.zero);
    await container.pump();
    expect(repository.requests, isNotEmpty);
    container.read(accessDeniedProvider.notifier).state = true;
    await container.pump();
    for (final request in repository.requests) { request.complete([brand('revoked')]); }
    await container.pump();
    expect(container.read(brandControllerProvider).hasError, true);
    expect(container.read(brandControllerProvider).hasValue, false);
  });
}
