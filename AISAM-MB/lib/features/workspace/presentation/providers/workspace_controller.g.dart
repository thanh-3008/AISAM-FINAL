// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'workspace_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

String _$workspaceControllerHash() =>
    r'6d4c8a39145d9de6acb1b27c0450a686af393580';

/// See also [WorkspaceController].
@ProviderFor(WorkspaceController)
final workspaceControllerProvider =
    AutoDisposeNotifierProvider<
      WorkspaceController,
      AsyncValue<List<WorkspaceResponseModel>>
    >.internal(
      WorkspaceController.new,
      name: r'workspaceControllerProvider',
      debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
          ? null
          : _$workspaceControllerHash,
      dependencies: null,
      allTransitiveDependencies: null,
    );

typedef _$WorkspaceController =
    AutoDisposeNotifier<AsyncValue<List<WorkspaceResponseModel>>>;
String _$createWorkspaceControllerHash() =>
    r'248f085826777a8fd7779774a99c3c2794e55ac8';

/// See also [CreateWorkspaceController].
@ProviderFor(CreateWorkspaceController)
final createWorkspaceControllerProvider =
    AutoDisposeNotifierProvider<
      CreateWorkspaceController,
      BaseState<WorkspaceResponseModel>
    >.internal(
      CreateWorkspaceController.new,
      name: r'createWorkspaceControllerProvider',
      debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
          ? null
          : _$createWorkspaceControllerHash,
      dependencies: null,
      allTransitiveDependencies: null,
    );

typedef _$CreateWorkspaceController =
    AutoDisposeNotifier<BaseState<WorkspaceResponseModel>>;
String _$activeWorkspaceControllerHash() =>
    r'6c7212f5f96947ed3ea1fc68ecec2ac59ffc7b65';

/// See also [ActiveWorkspaceController].
@ProviderFor(ActiveWorkspaceController)
final activeWorkspaceControllerProvider =
    AutoDisposeAsyncNotifierProvider<
      ActiveWorkspaceController,
      WorkspaceResponseModel?
    >.internal(
      ActiveWorkspaceController.new,
      name: r'activeWorkspaceControllerProvider',
      debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
          ? null
          : _$activeWorkspaceControllerHash,
      dependencies: null,
      allTransitiveDependencies: null,
    );

typedef _$ActiveWorkspaceController =
    AutoDisposeAsyncNotifier<WorkspaceResponseModel?>;
// ignore_for_file: type=lint
// ignore_for_file: subtype_of_sealed_class, invalid_use_of_internal_member, invalid_use_of_visible_for_testing_member, deprecated_member_use_from_same_package
