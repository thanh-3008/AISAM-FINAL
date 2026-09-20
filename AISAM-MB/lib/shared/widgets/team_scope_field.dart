import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/network/rbac_context.dart';

class TeamScopeField extends ConsumerWidget {
  final String brandId;
  final String? value;
  final ValueChanged<String?> onChanged;
  const TeamScopeField({
    super.key,
    required this.brandId,
    required this.value,
    required this.onChanged,
  });
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return ref
        .watch(rbacContextProvider)
        .when(
          loading: () => const LinearProgressIndicator(),
          error: (e, _) => Text('Failed to load Team permissions: $e'),
          data: (access) {
            if (!access.isV2) return const SizedBox.shrink();
            final ids = access.scopes
                .where((s) => s.brandId == brandId && access.canCreate(s))
                .map((s) => s.teamId)
                .toSet();
            return DropdownButtonFormField<String>(
              key: ValueKey('$brandId:${access.revision}:$value'),
              initialValue: ids.contains(value) ? value : null,
              isExpanded: true,
              decoration: const InputDecoration(
                labelText: 'Assigned Team *',
                helperText: 'Team cannot be changed after content creation.',
              ),
              items: ids
                  .map(
                    (id) => DropdownMenuItem(
                      value: id,
                      child: Text(id, overflow: TextOverflow.ellipsis),
                    ),
                  )
                  .toList(),
              onChanged: onChanged,
              validator: (id) => id != null && ids.contains(id)
                  ? null
                  : 'Select an authorized Team for this Brand.',
            );
          },
        );
  }
}
