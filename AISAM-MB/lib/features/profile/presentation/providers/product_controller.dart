import '../../../../core/network/access_events.dart';
import '../../../access/presentation/access_providers.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/product_repository.dart';
import '../../data/models/product_model.dart';
import '../../data/models/product_request.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';

part 'product_controller.g.dart';

@riverpod
class ProductController extends _$ProductController {
  int _generation = 0;

  @override
  AsyncValue<List<ProductResponseModel>> build(String? brandId) {
    ++_generation;
    ref.onDispose(() => ++_generation);
    ref.watch(accessContextProvider);
    if (ref.watch(accessDeniedProvider)) {
      return AsyncValue.error(StateError('Access denied'), StackTrace.current);
    }
    _fetchProducts(brandId);
    return const AsyncValue.loading();
  }

  Future<void> _fetchProducts(String? brandId) async {
    final generation = ++_generation;
    try {
      await ref.read(accessContextProvider.future);
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      final repository = ref.read(productRepositoryProvider);
      final products = await repository.getProducts(brandId: brandId);
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.data(products);
    } catch (e, st) {
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refreshProducts(String? brandId) async {
    await _fetchProducts(brandId);
  }
}

@riverpod
class CreateProductController extends _$CreateProductController {
  @override
  BaseState<ProductResponseModel> build() {
    return const BaseState.initial();
  }

  Future<void> createProduct(CreateProductRequest request) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(productRepositoryProvider);
      final product = await repository.createProduct(request);
      state = BaseState.data(product);
      ref.read(productControllerProvider(request.brandId).notifier).refreshProducts(request.brandId);
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }
}
