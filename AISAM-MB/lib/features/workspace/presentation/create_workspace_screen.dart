import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_button.dart';
import '../../../core/shared/app_snackbar.dart';
import '../data/models/workspace_request.dart';
import 'providers/workspace_controller.dart';
import '../../../core/state/base_state.dart';
import '../data/models/workspace_model.dart';

class CreateWorkspaceScreen extends ConsumerStatefulWidget {
  const CreateWorkspaceScreen({super.key});

  @override
  ConsumerState<CreateWorkspaceScreen> createState() => _CreateWorkspaceScreenState();
}

class _CreateWorkspaceScreenState extends ConsumerState<CreateWorkspaceScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descController = TextEditingController();

  @override
  void dispose() {
    _nameController.dispose();
    _descController.dispose();
    super.dispose();
  }

  void _onSubmit() {
    if (_formKey.currentState!.validate()) {
      ref.read(createWorkspaceControllerProvider.notifier).createWorkspace(
        CreateWorkspaceRequest(
          name: _nameController.text.trim(),
          description: _descController.text.trim(),
          workspaceType: 0, // Default Business
        )
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final createState = ref.watch(createWorkspaceControllerProvider);
    final isLoading = createState.maybeWhen(
      loading: () => true,
      orElse: () => false,
    );

    ref.listen<BaseState<WorkspaceResponseModel>>(createWorkspaceControllerProvider, (previous, next) {
      next.maybeWhen(
        error: (error) => AppSnackbar.showError(context, error.toString()),
        data: (workspace) {
          AppSnackbar.showSuccess(context, 'Workspace created successfully!');
          context.pop(); // Go back to list
        },
        orElse: () {},
      );
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('Create Workspace'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                TextFormField(
                  controller: _nameController,
                  decoration: const InputDecoration(labelText: 'Workspace Name *'),
                  validator: (value) {
                    if (value == null || value.isEmpty) return 'Please enter a name';
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _descController,
                  decoration: const InputDecoration(labelText: 'Description (Optional)'),
                  maxLines: 3,
                ),
                const SizedBox(height: 32),
                AppButton(
                  text: 'Create',
                  isLoading: isLoading,
                  onPressed: _onSubmit,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
