// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'content_editor_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

String _$contentEditorControllerHash() =>
    r'62924a3a8dd381a03be4b52898c383f99cfab423';

/// See also [ContentEditorController].
@ProviderFor(ContentEditorController)
final contentEditorControllerProvider =
    AutoDisposeNotifierProvider<
      ContentEditorController,
      BaseState<ContentResponseModel>
    >.internal(
      ContentEditorController.new,
      name: r'contentEditorControllerProvider',
      debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
          ? null
          : _$contentEditorControllerHash,
      dependencies: null,
      allTransitiveDependencies: null,
    );

typedef _$ContentEditorController =
    AutoDisposeNotifier<BaseState<ContentResponseModel>>;
String _$contentDetailControllerHash() =>
    r'4c449b08cb259ac11347050a231560958145aa9e';

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

abstract class _$ContentDetailController
    extends BuildlessAutoDisposeNotifier<AsyncValue<ContentResponseModel>> {
  late final String id;

  AsyncValue<ContentResponseModel> build(String id);
}

/// See also [ContentDetailController].
@ProviderFor(ContentDetailController)
const contentDetailControllerProvider = ContentDetailControllerFamily();

/// See also [ContentDetailController].
class ContentDetailControllerFamily
    extends Family<AsyncValue<ContentResponseModel>> {
  /// See also [ContentDetailController].
  const ContentDetailControllerFamily();

  /// See also [ContentDetailController].
  ContentDetailControllerProvider call(String id) {
    return ContentDetailControllerProvider(id);
  }

  @override
  ContentDetailControllerProvider getProviderOverride(
    covariant ContentDetailControllerProvider provider,
  ) {
    return call(provider.id);
  }

  static const Iterable<ProviderOrFamily>? _dependencies = null;

  @override
  Iterable<ProviderOrFamily>? get dependencies => _dependencies;

  static const Iterable<ProviderOrFamily>? _allTransitiveDependencies = null;

  @override
  Iterable<ProviderOrFamily>? get allTransitiveDependencies =>
      _allTransitiveDependencies;

  @override
  String? get name => r'contentDetailControllerProvider';
}

/// See also [ContentDetailController].
class ContentDetailControllerProvider
    extends
        AutoDisposeNotifierProviderImpl<
          ContentDetailController,
          AsyncValue<ContentResponseModel>
        > {
  /// See also [ContentDetailController].
  ContentDetailControllerProvider(String id)
    : this._internal(
        () => ContentDetailController()..id = id,
        from: contentDetailControllerProvider,
        name: r'contentDetailControllerProvider',
        debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
            ? null
            : _$contentDetailControllerHash,
        dependencies: ContentDetailControllerFamily._dependencies,
        allTransitiveDependencies:
            ContentDetailControllerFamily._allTransitiveDependencies,
        id: id,
      );

  ContentDetailControllerProvider._internal(
    super._createNotifier, {
    required super.name,
    required super.dependencies,
    required super.allTransitiveDependencies,
    required super.debugGetCreateSourceHash,
    required super.from,
    required this.id,
  }) : super.internal();

  final String id;

  @override
  AsyncValue<ContentResponseModel> runNotifierBuild(
    covariant ContentDetailController notifier,
  ) {
    return notifier.build(id);
  }

  @override
  Override overrideWith(ContentDetailController Function() create) {
    return ProviderOverride(
      origin: this,
      override: ContentDetailControllerProvider._internal(
        () => create()..id = id,
        from: from,
        name: null,
        dependencies: null,
        allTransitiveDependencies: null,
        debugGetCreateSourceHash: null,
        id: id,
      ),
    );
  }

  @override
  AutoDisposeNotifierProviderElement<
    ContentDetailController,
    AsyncValue<ContentResponseModel>
  >
  createElement() {
    return _ContentDetailControllerProviderElement(this);
  }

  @override
  bool operator ==(Object other) {
    return other is ContentDetailControllerProvider && other.id == id;
  }

  @override
  int get hashCode {
    var hash = _SystemHash.combine(0, runtimeType.hashCode);
    hash = _SystemHash.combine(hash, id.hashCode);

    return _SystemHash.finish(hash);
  }
}

@Deprecated('Will be removed in 3.0. Use Ref instead')
// ignore: unused_element
mixin ContentDetailControllerRef
    on AutoDisposeNotifierProviderRef<AsyncValue<ContentResponseModel>> {
  /// The parameter `id` of this provider.
  String get id;
}

class _ContentDetailControllerProviderElement
    extends
        AutoDisposeNotifierProviderElement<
          ContentDetailController,
          AsyncValue<ContentResponseModel>
        >
    with ContentDetailControllerRef {
  _ContentDetailControllerProviderElement(super.provider);

  @override
  String get id => (origin as ContentDetailControllerProvider).id;
}

// ignore_for_file: type=lint
// ignore_for_file: subtype_of_sealed_class, invalid_use_of_internal_member, invalid_use_of_visible_for_testing_member, deprecated_member_use_from_same_package
