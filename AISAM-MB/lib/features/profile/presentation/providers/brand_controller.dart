import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/brand_repository.dart';
import '../../data/models/brand_model.dart';
import '../../data/models/brand_request.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';
import '../../../workspace/presentation/providers/workspace_controller.dart';

part 'brand_controller.g.dart';

@riverpod
class BrandController extends _$BrandController {
  @override
  AsyncValue<List<BrandResponseModel>> build() {
    ref.watch(activeWorkspaceControllerProvider);
    _fetchBrands();
    return const AsyncValue.loading();
  }

  Future<void> _fetchBrands() async {
    try {
      state = const AsyncValue.loading();
      final repository = ref.read(brandRepositoryProvider);
      final brands = await repository.getBrands();
      state = AsyncValue.data(brands);
    } catch (e, st) {
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refreshBrands() async {
    await _fetchBrands();
  }
}

@riverpod
class CreateBrandController extends _$CreateBrandController {
  @override
  BaseState<BrandResponseModel> build() {
    return const BaseState.initial();
  }

  Future<void> createBrand(CreateBrandRequest request) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(brandRepositoryProvider);
      final brand = await repository.createBrand(request);
      state = BaseState.data(brand);
      ref.invalidate(brandControllerProvider);
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }
}
