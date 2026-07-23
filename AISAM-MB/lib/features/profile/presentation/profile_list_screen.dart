import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../../core/shared/empty_state_widget.dart';
import 'providers/profile_controller.dart';
import '../data/models/profile_model.dart';
import '../../../core/services/logger_service.dart';

class ProfileListScreen extends ConsumerWidget {
  const ProfileListScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileState = ref.watch(profileControllerProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Profiles'),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () => context.push('/profiles/create'),
          ),
        ],
      ),
      body: profileState.when(
        data: (profiles) {
          if (profiles.isEmpty) {
            return const EmptyStateWidget(
              title: 'No Profiles Found',
              message: 'You don\'t have any profiles yet.',
              icon: Icons.person_off,
            );
          }
          return RefreshIndicator(
            onRefresh: () => ref.read(profileControllerProvider.notifier).refreshProfiles(),
            child: ListView.builder(
              itemCount: profiles.length,
              itemBuilder: (context, index) {
                final profile = profiles[index];
                return _ProfileListItem(profile: profile);
              },
            ),
          );
        },
        loading: () => const Center(child: AppLoadingIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Error: $error', textAlign: TextAlign.center),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () => ref.read(profileControllerProvider.notifier).refreshProfiles(),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ProfileListItem extends ConsumerWidget {
  final ProfileResponseModel profile;

  const _ProfileListItem({required this.profile});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return ListTile(
      leading: CircleAvatar(
        backgroundImage: profile.avatarUrl != null ? NetworkImage(profile.avatarUrl!) : null,
        child: profile.avatarUrl == null ? Text(profile.name.substring(0, 1).toUpperCase()) : null,
      ),
      title: Text(profile.name),
      subtitle: Text(profile.companyName ?? 'Personal Profile'),
      trailing: const Icon(Icons.chevron_right),
      onTap: () async {
        LoggerService.i('Selected profile: ${profile.id}');
        await ref.read(profileControllerProvider.notifier).selectProfile(profile.id);
        if (context.mounted) {
          context.push('/profiles/${profile.id}/brands');
        }
      },
    );
  }
}
