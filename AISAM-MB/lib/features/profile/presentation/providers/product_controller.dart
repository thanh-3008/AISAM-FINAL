import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/product_repository.dart';
import '../../data/models/product_model.dart';
import '../../data/models/product_request.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';

part 'product_controller.g.dart';

@riverpod
class ProductController extends _$ProductController {
  @override
  AsyncValue<List<ProductResponseModel>> build(String? brandId) {
    _fetchProducts(brandId);
    return const AsyncValue.loading();
  }

  Future<void> _fetchProducts(String? brandId) async {
    try {
      state = const AsyncValue.loading();
      final repository = ref.read(productRepositoryProvider);
      final products = await repository.getProducts(brandId: brandId);
      state = AsyncValue.data(products);
    } catch (e, st) {
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
