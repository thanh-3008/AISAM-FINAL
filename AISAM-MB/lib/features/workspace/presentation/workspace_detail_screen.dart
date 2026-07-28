import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/models/workspace_model.dart';
import 'providers/workspace_controller.dart';
import '../../../core/shared/app_loading_indicator.dart';

// Assuming we fetch specific workspace, for now we will just find it from the list
class WorkspaceDetailScreen extends ConsumerWidget {
  final String workspaceId;

  const WorkspaceDetailScreen({super.key, required this.workspaceId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final workspaceState = ref.watch(workspaceControllerProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Workspace Details'),
      ),
      body: workspaceState.when(
        data: (workspaces) {
          final workspace = workspaces.firstWhere(
            (w) => w.id == workspaceId,
            orElse: () => WorkspaceResponseModel(
              id: '',
              name: 'Not Found',
              workspaceType: 0,
              status: 0,
              currentUserRole: 0,
              createdAt: DateTime.now(), // dummy for not found, typically we'd throw or handle better
              updatedAt: DateTime.now(),
            ),
          );

          if (workspace.id.isEmpty) {
            return const Center(child: Text('Workspace not found.'));
          }

          return Padding(
            padding: const EdgeInsets.all(24.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Name: ${workspace.name}', style: Theme.of(context).textTheme.titleLarge),
                const SizedBox(height: 8),
                Text('Description: ${workspace.description ?? 'None'}'),
                const SizedBox(height: 8),
                Text('Role: ${workspace.currentUserRole}'),
                const SizedBox(height: 24),
                const Text('Members:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
                const Expanded(
                  child: Center(
                    child: Text('Member list API integration pending...'),
                  ),
                )
              ],
            ),
          );
        },
        loading: () => const Center(child: AppLoadingIndicator()),
        error: (error, stack) => Center(child: Text('Error: $error')),
      ),
    );
  }
}
