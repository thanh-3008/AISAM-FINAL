import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/storage/secure_storage.dart';
import '../../workspace/presentation/providers/workspace_controller.dart';

final selfPerformanceProvider =
    FutureProvider.autoDispose<Map<String, dynamic>>((ref) async {
      final workspace = ref.watch(activeWorkspaceControllerProvider);
      if (workspace.isLoading || workspace.valueOrNull == null)
        throw StateError('Select a workspace.');
      final user = ref.read(secureStorageProvider).cachedUserId;
      if (user == null) throw StateError('Sign in again.');
      final now = DateTime.now().toUtc();
      final result = await ref
          .read(dioProvider)
          .get(
            '/team/member-performance',
            queryParameters: {
              'from': now.subtract(const Duration(days: 30)).toIso8601String(),
              'to': now.toIso8601String(),
              'memberId': user,
            },
          );
      return Map<String, dynamic>.from(result.data['data']);
    });

class MemberPerformanceScreen extends ConsumerWidget {
  const MemberPerformanceScreen({super.key});
  @override
  Widget build(BuildContext context, WidgetRef ref) => Scaffold(
    appBar: AppBar(
      title: const Text('My performance · 30 days'),
      actions: [
        IconButton(
          onPressed: () => ref.invalidate(selfPerformanceProvider),
          icon: const Icon(Icons.refresh),
        ),
      ],
    ),
    body: ref
        .watch(selfPerformanceProvider)
        .when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (e, _) => Center(child: Text('Cannot load performance: $e')),
          data: (data) => ListView(
            children: [
              for (final row in data['items'] as List) ...[
                ListTile(title: Text('${row['name']}')),
                for (final metric in const {
                  'contentsCreated': 'Content created',
                  'creatorPublishedPosts': 'Published posts',
                  'postsWithInsights': 'Posts with insights',
                  'engagement': 'Engagements',
                  'impressions': 'Impressions',
                  'reach': 'Reach',
                  'engagementRate': 'Engagement rate',
                }.entries)
                  ListTile(
                    title: Text(metric.value),
                    trailing: Text('${row[metric.key] ?? '—'}'),
                  ),
              ],
              const Padding(
                padding: EdgeInsets.all(16),
                child: Text(
                  'UTC period. Insights use the latest report per published destination. — means unavailable.',
                ),
              ),
            ],
          ),
        ),
  );
}
