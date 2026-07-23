// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'product_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

String _$productControllerHash() => r'a432c77abc75a40ba600803ef8ccd288cc5cf2ca';

/// Copied from Dart SDK
class _SystemHash {
  _SystemHash._();

  static int combine(int hash, int value) {
    // ignore: parameter_assignments
    hash = 0x1fffffff & (hash + value);
    // ignore: parameter_assignments
    hash = 0x1fffffff & (hash + ((0x0007ffff & hash) << 10));
    return hash ^ (hash >> 6);
  }

  static int finish(int hash) {
    // ignore: parameter_assignments
    hash = 0x1fffffff & (hash + ((0x03ffffff & hash) << 3));
    // ignore: parameter_assignments
    hash = hash ^ (hash >> 11);
    return 0x1fffffff & (hash + ((0x00003fff & hash) << 15));
  }
}

abstract class _$ProductController
    extends
        BuildlessAutoDisposeNotifier<AsyncValue<List<ProductResponseModel>>> {
  late final String? brandId;

  AsyncValue<List<ProductResponseModel>> build(String? brandId);
}

/// See also [ProductController].
@ProviderFor(ProductController)
const productControllerProvider = ProductControllerFamily();

/// See also [ProductController].
class ProductControllerFamily
    extends Family<AsyncValue<List<ProductResponseModel>>> {
  /// See also [ProductController].
  const ProductControllerFamily();

  /// See also [ProductController].
  ProductControllerProvider call(String? brandId) {
    return ProductControllerProvider(brandId);
  }

  @override
  ProductControllerProvider getProviderOverride(
    covariant ProductControllerProvider provider,
  ) {
    return call(provider.brandId);
  }

  static const Iterable<ProviderOrFamily>? _dependencies = null;

  @override
  Iterable<ProviderOrFamily>? get dependencies => _dependencies;

  static const Iterable<ProviderOrFamily>? _allTransitiveDependencies = null;

  @override
  Iterable<ProviderOrFamily>? get allTransitiveDependencies =>
      _allTransitiveDependencies;

  @override
  String? get name => r'productControllerProvider';
}

/// See also [ProductController].
class ProductControllerProvider
    extends
        AutoDisposeNotifierProviderImpl<
          ProductController,
          AsyncValue<List<ProductResponseModel>>
        > {
  /// See also [ProductController].
  ProductControllerProvider(String? brandId)
    : this._internal(
        () => ProductController()..brandId = brandId,
        from: productControllerProvider,
        name: r'productControllerProvider',
        debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
            ? null
            : _$productControllerHash,
        dependencies: ProductControllerFamily._dependencies,
        allTransitiveDependencies:
            ProductControllerFamily._allTransitiveDependencies,
        brandId: brandId,
      );

  ProductControllerProvider._internal(
    super._createNotifier, {
    required super.name,
    required super.dependencies,
    required super.allTransitiveDependencies,
    required super.debugGetCreateSourceHash,
    required super.from,
    required this.brandId,
  }) : super.internal();

  final String? brandId;

  @override
  AsyncValue<List<ProductResponseModel>> runNotifierBuild(
    covariant ProductController notifier,
  ) {
    return notifier.build(brandId);
  }

  @override
  Override overrideWith(ProductController Function() create) {
    return ProviderOverride(
      origin: this,
      override: ProductControllerProvider._internal(
        () => create()..brandId = brandId,
        from: from,
        name: null,
        dependencies: null,
        allTransitiveDependencies: null,
        debugGetCreateSourceHash: null,
        brandId: brandId,
      ),
    );
  }

  @override
  AutoDisposeNotifierProviderElement<
    ProductController,
    AsyncValue<List<ProductResponseModel>>
  >
  createElement() {
    return _ProductControllerProviderElement(this);
  }

  @override
  bool operator ==(Object other) {
    return other is ProductControllerProvider && other.brandId == brandId;
  }

  @override
  int get hashCode {
    var hash = _SystemHash.combine(0, runtimeType.hashCode);
    hash = _SystemHash.combine(hash, brandId.hashCode);

    return _SystemHash.finish(hash);
  }
}

@Deprecated('Will be removed in 3.0. Use Ref instead')
// ignore: unused_element
mixin ProductControllerRef
    on AutoDisposeNotifierProviderRef<AsyncValue<List<ProductResponseModel>>> {
  /// The parameter `brandId` of this provider.
  String? get brandId;
}

class _ProductControllerProviderElement
    extends
        AutoDisposeNotifierProviderElement<
          ProductController,
          AsyncValue<List<ProductResponseModel>>
        >
    with ProductControllerRef {
  _ProductControllerProviderElement(super.provider);

  @override
  String? get brandId => (origin as ProductControllerProvider).brandId;
}

String _$createProductControllerHash() =>
    r'cadef69fde8be5b370a8033470426eb905a00428';

/// See also [CreateProductController].
@ProviderFor(CreateProductController)
final createProductControllerProvider =
    AutoDisposeNotifierProvider<
      CreateProductController,
      BaseState<ProductResponseModel>
    >.internal(
      CreateProductController.new,
      name: r'createProductControllerProvider',
      debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
          ? null
          : _$createProductControllerHash,
      dependencies: null,
      allTransitiveDependencies: null,
    );

typedef _$CreateProductController =
    AutoDisposeNotifier<BaseState<ProductResponseModel>>;
// ignore_for_file: type=lint
// ignore_for_file: subtype_of_sealed_class, invalid_use_of_internal_member, invalid_use_of_visible_for_testing_member, deprecated_member_use_from_same_package
