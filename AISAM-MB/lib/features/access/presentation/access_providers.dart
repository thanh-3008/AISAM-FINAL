import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/access_events.dart';
import '../../workspace/presentation/providers/workspace_controller.dart';
import '../data/access_context.dart';
import '../data/access_repository.dart';

final accessContextProvider = FutureProvider<AccessContext>((ref) async {
  ref.watch(accessRevisionProvider);
  final workspace = await ref.watch(activeWorkspaceControllerProvider.future);
  if (workspace == null) throw StateError('Select a workspace');
  return ref.watch(accessRepositoryProvider).context(workspace.id);
});

final contentActionsProvider = FutureProvider.autoDispose.family<Map<String, bool>, String>((ref, id) async {
  if (ref.watch(accessDeniedProvider)) throw StateError('Access denied');
  await ref.watch(accessContextProvider.future);
  return ref.watch(accessRepositoryProvider).actions(id);
});

final ownAnalyticsProvider = FutureProvider.autoDispose<Map<String, dynamic>>((ref) async {
  if (ref.watch(accessDeniedProvider)) throw StateError('Access denied');
  final access = await ref.watch(accessContextProvider.future);
  if (!access.canViewOwnAnalytics) throw StateError('Access denied');
  return ref.watch(accessRepositoryProvider).ownAnalytics();
});
