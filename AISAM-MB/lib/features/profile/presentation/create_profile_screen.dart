import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_button.dart';
import '../../../core/shared/app_snackbar.dart';
import '../data/models/profile_request.dart';
import 'providers/profile_controller.dart';
import '../../../core/state/base_state.dart';
import '../data/models/profile_model.dart';

class CreateProfileScreen extends ConsumerStatefulWidget {
  const CreateProfileScreen({super.key});

  @override
  ConsumerState<CreateProfileScreen> createState() => _CreateProfileScreenState();
}

class _CreateProfileScreenState extends ConsumerState<CreateProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _companyController = TextEditingController();
  final _bioController = TextEditingController();

  @override
  void dispose() {
    _nameController.dispose();
    _companyController.dispose();
    _bioController.dispose();
    super.dispose();
  }

  void _onSubmit() {
    if (_formKey.currentState!.validate()) {
      ref.read(createProfileControllerProvider.notifier).createProfile(
        CreateProfileRequest(
          name: _nameController.text.trim(),
          profileType: 1, // Default to a valid type
          companyName: _companyController.text.trim().isNotEmpty ? _companyController.text.trim() : null,
          bio: _bioController.text.trim().isNotEmpty ? _bioController.text.trim() : null,
        )
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final createState = ref.watch(createProfileControllerProvider);
    final isLoading = createState.maybeWhen(
      loading: () => true,
      orElse: () => false,
    );

    ref.listen<BaseState<ProfileResponseModel>>(createProfileControllerProvider, (previous, next) {
      next.maybeWhen(
        error: (error) => AppSnackbar.showError(context, error.toString()),
        data: (profile) {
          AppSnackbar.showSuccess(context, 'Profile created successfully!');
          context.pop(); // Go back to list
        },
        orElse: () {},
      );
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('Create Profile'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Form(
            key: _formKey,
            child: ListView(
              children: [
                TextFormField(
                  controller: _nameController,
                  decoration: const InputDecoration(labelText: 'Profile Name *'),
                  validator: (value) {
                    if (value == null || value.isEmpty) return 'Please enter a name';
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _companyController,
                  decoration: const InputDecoration(labelText: 'Company Name (Optional)'),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _bioController,
                  decoration: const InputDecoration(labelText: 'Bio (Optional)'),
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
