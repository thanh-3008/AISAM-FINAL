import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import '../../../core/shared/app_button.dart';
import '../../../core/shared/app_snackbar.dart';
import '../data/models/brand_request.dart';
import 'providers/brand_controller.dart';
import '../../../core/state/base_state.dart';
import '../data/models/brand_model.dart';

class CreateBrandScreen extends ConsumerStatefulWidget {
  const CreateBrandScreen({super.key});

  @override
  ConsumerState<CreateBrandScreen> createState() => _CreateBrandScreenState();
}

class _CreateBrandScreenState extends ConsumerState<CreateBrandScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descController = TextEditingController();
  File? _logoImage;
  
  Future<void> _pickImage() async {
    final picker = ImagePicker();
    final pickedFile = await picker.pickImage(source: ImageSource.gallery);
    if (pickedFile != null) {
      setState(() {
        _logoImage = File(pickedFile.path);
      });
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descController.dispose();
    super.dispose();
  }

  void _onSubmit() {
    if (_formKey.currentState!.validate()) {
      ref.read(createBrandControllerProvider.notifier).createBrand(
        CreateBrandRequest(
          name: _nameController.text.trim(),
          description: _descController.text.trim().isNotEmpty ? _descController.text.trim() : null,
          logoUrl: _logoImage?.path, // Currently using local path. Pending BE upload endpoint.
        )
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final createState = ref.watch(createBrandControllerProvider);
    final isLoading = createState.maybeWhen(
      loading: () => true,
      orElse: () => false,
    );

    ref.listen<BaseState<BrandResponseModel>>(createBrandControllerProvider, (previous, next) {
      next.maybeWhen(
        error: (error) => AppSnackbar.showError(context, error.toString()),
        data: (brand) {
          AppSnackbar.showSuccess(context, 'Brand created successfully!');
          context.pop(); // Go back to list
        },
        orElse: () {},
      );
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('Create Brand'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Form(
            key: _formKey,
            child: ListView(
              children: [
                Center(
                  child: GestureDetector(
                    onTap: _pickImage,
                    child: CircleAvatar(
                      radius: 50,
                      backgroundColor: Theme.of(context).colorScheme.primaryContainer,
                      backgroundImage: _logoImage != null ? FileImage(_logoImage!) : null,
                      child: _logoImage == null
                          ? Icon(Icons.add_a_photo, size: 32, color: Theme.of(context).colorScheme.primary)
                          : null,
                    ),
                  ),
                ),
                const SizedBox(height: 24),
                TextFormField(
                  controller: _nameController,
                  decoration: const InputDecoration(labelText: 'Brand Name *'),
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
                  text: 'Create Brand',
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
