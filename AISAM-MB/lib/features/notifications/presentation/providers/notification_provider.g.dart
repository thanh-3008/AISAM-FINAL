// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'notification_provider.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

String _$unreadNotificationCountHash() =>
    r'f53deaa65b78164ade8d329f2e9b8eb3f48baeb3';

/// See also [unreadNotificationCount].
@ProviderFor(unreadNotificationCount)
final unreadNotificationCountProvider = AutoDisposeFutureProvider<int>.internal(
  unreadNotificationCount,
  name: r'unreadNotificationCountProvider',
  debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
      ? null
      : _$unreadNotificationCountHash,
  dependencies: null,
  allTransitiveDependencies: null,
);

@Deprecated('Will be removed in 3.0. Use Ref instead')
// ignore: unused_element
typedef UnreadNotificationCountRef = AutoDisposeFutureProviderRef<int>;
String _$notificationFilterStateHash() =>
    r'45f0ef7a8d7a09a4f7cc2caa2470607b068767db';

/// See also [NotificationFilterState].
@ProviderFor(NotificationFilterState)
final notificationFilterStateProvider =
    AutoDisposeNotifierProvider<
      NotificationFilterState,
      NotificationFilterModel
    >.internal(
      NotificationFilterState.new,
      name: r'notificationFilterStateProvider',
      debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
          ? null
          : _$notificationFilterStateHash,
      dependencies: null,
      allTransitiveDependencies: null,
    );

typedef _$NotificationFilterState =
    AutoDisposeNotifier<NotificationFilterModel>;
String _$notificationListStateHash() =>
    r'7aa820812d3b94dacda1a9a7085db7ff5385faac';

/// See also [NotificationListState].
@ProviderFor(NotificationListState)
final notificationListStateProvider =
    AutoDisposeAsyncNotifierProvider<
      NotificationListState,
      List<NotificationModel>
    >.internal(
      NotificationListState.new,
      name: r'notificationListStateProvider',
      debugGetCreateSourceHash: const bool.fromEnvironment('dart.vm.product')
          ? null
          : _$notificationListStateHash,
      dependencies: null,
      allTransitiveDependencies: null,
    );

typedef _$NotificationListState =
    AutoDisposeAsyncNotifier<List<NotificationModel>>;
// ignore_for_file: type=lint
// ignore_for_file: subtype_of_sealed_class, invalid_use_of_internal_member, invalid_use_of_visible_for_testing_member, deprecated_member_use_from_same_package
