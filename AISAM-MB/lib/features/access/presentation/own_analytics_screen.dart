import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'access_providers.dart';

class OwnAnalyticsScreen extends ConsumerWidget {
  const OwnAnalyticsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final summary = ref.watch(ownAnalyticsProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Analytics cá nhân')),
      body: summary.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (_, _) => const Center(child: Text('Không thể tải analytics cá nhân.')),
        data: (data) => RefreshIndicator(
          onRefresh: () async { ref.invalidate(ownAnalyticsProvider); await ref.read(ownAnalyticsProvider.future); },
          child: ListView(children: [
            for (final item in const {'contentCount': 'Nội dung', 'impressions': 'Lượt hiển thị', 'engagement': 'Tương tác', 'clicks': 'Lượt nhấp'}.entries)
              ListTile(title: Text(item.value), trailing: Text('${data[item.key] ?? 0}')),
          ]),
        ),
      ),
    );
  }
}
