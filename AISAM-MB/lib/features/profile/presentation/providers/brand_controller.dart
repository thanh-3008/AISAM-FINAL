import '../../../../core/network/access_events.dart';
import '../../../access/presentation/access_providers.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/brand_repository.dart';
import '../../data/models/brand_model.dart';
import '../../data/models/brand_request.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';

part 'brand_controller.g.dart';

@riverpod
class BrandController extends _$BrandController {
  int _generation = 0;

  @override
  AsyncValue<List<BrandResponseModel>> build() {
    ++_generation;
    ref.onDispose(() => ++_generation);
    ref.watch(accessContextProvider);
    if (ref.watch(accessDeniedProvider)) {
      return AsyncValue.error(StateError('Access denied'), StackTrace.current);
    }
    _fetchBrands();
    return const AsyncValue.loading();
  }

  Future<void> _fetchBrands() async {
    final generation = ++_generation;
    try {
      await ref.read(accessContextProvider.future);
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      final repository = ref.read(brandRepositoryProvider);
      final brands = await repository.getBrands();
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.data(brands);
    } catch (e, st) {
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
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
