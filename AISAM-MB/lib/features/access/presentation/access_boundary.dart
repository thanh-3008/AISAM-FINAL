import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/network/access_events.dart';
import '../data/access_repository.dart';
import 'access_providers.dart';

class AccessBoundary extends ConsumerStatefulWidget {
  final Widget child;
  const AccessBoundary({super.key, required this.child});

  @override
  ConsumerState<AccessBoundary> createState() => _AccessBoundaryState();
}

class _AccessBoundaryState extends ConsumerState<AccessBoundary> with WidgetsBindingObserver {
  Timer? _timer;
  bool _refreshing = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _timer = Timer.periodic(const Duration(seconds: 30), (_) => _refresh());
  }

  Future<void> _refresh() async {
    final current = ref.read(accessContextProvider).valueOrNull;
    if (current == null || _refreshing) return;
    _refreshing = true;
    try {
      final updated = await ref.read(accessRepositoryProvider).context(current.workspaceId);
      if (!mounted) return;
      if (updated != current) ref.read(accessRevisionProvider.notifier).state++;
    } catch (_) {
      if (mounted) ref.read(accessDeniedProvider.notifier).state = true;
    } finally {
      _refreshing = false;
    }
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) _refresh();
  }

  @override
  void dispose() {
    _timer?.cancel();
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final access = ref.watch(accessContextProvider);
    final denied = ref.watch(accessDeniedProvider);
    if (denied || access.hasError) {
      return Scaffold(body: Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
        const Text('Quyền truy cập đã thay đổi. Vui lòng chọn lại workspace.'),
        TextButton(onPressed: () => context.go('/overview'), child: const Text('Chọn workspace')),
      ])));
    }
    if (access.isLoading) return const Scaffold(body: Center(child: CircularProgressIndicator()));
    return KeyedSubtree(key: ValueKey(access.valueOrNull?.version), child: widget.child);
  }
}
