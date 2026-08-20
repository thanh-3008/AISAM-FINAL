import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../profile/data/repositories/brand_repository.dart';
import '../../data/repositories/social_repository.dart';
import '../../data/models/social_integration_model.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../workspace/presentation/providers/workspace_controller.dart';

part 'social_controller.g.dart';

@riverpod
class SocialController extends _$SocialController {
  @override
  AsyncValue<List<SocialIntegrationModel>> build() {
    ref.watch(activeWorkspaceControllerProvider);
    _fetchIntegrations();
    return const AsyncValue.loading();
  }

  Future<void> _fetchIntegrations() async {
    try {
      state = const AsyncValue.loading();
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
      
      state = AsyncValue.data(allIntegrations);
    } catch (e, st) {
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
