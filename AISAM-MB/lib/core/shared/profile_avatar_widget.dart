import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class ProfileAvatarWidget extends StatelessWidget {
  const ProfileAvatarWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return IconButton(
      icon: const Icon(Icons.account_circle, size: 32),
      color: Theme.of(context).colorScheme.onSurfaceVariant,
      onPressed: () {
        context.go('/settings');
      },
    );
  }
}
