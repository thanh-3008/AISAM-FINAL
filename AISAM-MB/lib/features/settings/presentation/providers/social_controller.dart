import '../../../../core/network/access_events.dart';
import '../../../access/presentation/access_providers.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../profile/data/repositories/brand_repository.dart';
import '../../data/repositories/social_repository.dart';
import '../../data/models/social_integration_model.dart';
import '../../../../core/errors/app_exception.dart';

part 'social_controller.g.dart';

@riverpod
class SocialController extends _$SocialController {
  int _generation = 0;

  @override
  AsyncValue<List<SocialIntegrationModel>> build() {
    ++_generation;
    ref.onDispose(() => ++_generation);
    ref.watch(accessContextProvider);
    if (ref.watch(accessDeniedProvider)) {
      return AsyncValue.error(StateError('Access denied'), StackTrace.current);
    }
    _fetchIntegrations();
    return const AsyncValue.loading();
  }

  Future<void> _fetchIntegrations() async {
    final generation = ++_generation;
    try {
      await ref.read(accessContextProvider.future);
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      final repository = ref.read(socialRepositoryProvider);
      
      // We must get all brands first, because the API requires brandId
      final brandsList = await ref.read(brandRepositoryProvider).getBrands();
      
      List<SocialIntegrationModel> allIntegrations = [];
      if (brandsList.isNotEmpty) {
        for (var brand in brandsList) {
          final integrations = await repository.getIntegrationsByBrand(brand.id);
          allIntegrations.addAll(integrations);
        }
      }
      
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.data(allIntegrations);
    } catch (e, st) {
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refresh() async {
    await _fetchIntegrations();
  }

  Future<void> deleteIntegration(String id) async {
    final repository = ref.read(socialRepositoryProvider);
    await repository.deleteIntegration(id);
    await refresh();
  }
}
