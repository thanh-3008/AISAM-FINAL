import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/network/rbac_context.dart';

/// Keeps legacy-only forms from sending old role/ownership contracts to v2.
class RbacScreenGate extends ConsumerStatefulWidget {
  final Widget child;
  final String? unsupportedV2;
  const RbacScreenGate({super.key, required this.child, this.unsupportedV2});

  @override
  ConsumerState<RbacScreenGate> createState() => _RbacScreenGateState();
}

class _RbacScreenGateState extends ConsumerState<RbacScreenGate>
    with WidgetsBindingObserver {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) ref.invalidate(rbacContextProvider);
  }

  @override
  Widget build(BuildContext context) {
    return ref
        .watch(rbacContextProvider)
        .when(
          skipLoadingOnRefresh: true,
          loading: () =>
              const Scaffold(body: Center(child: CircularProgressIndicator())),
          error: (_, _) => _notice(
            'Không xác nhận được quyền truy cập',
            'Vui lòng kiểm tra kết nối hoặc chọn lại workspace. Phiên đăng nhập vẫn được giữ.',
            retry: true,
          ),
          data: (access) => access.isV2 && widget.unsupportedV2 != null
              ? _notice('Thao tác trên website', widget.unsupportedV2!)
              : widget.child,
        );
  }

  Widget _notice(String title, String message, {bool retry = false}) =>
      Scaffold(
        appBar: AppBar(title: Text(title)),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.info_outline, size: 48),
                const SizedBox(height: 16),
                Text(message, textAlign: TextAlign.center),
                if (retry)
                  TextButton(
                    onPressed: () => ref.invalidate(rbacContextProvider),
                    child: const Text('Thử lại'),
                  ),
              ],
            ),
          ),
        ),
      );
}
