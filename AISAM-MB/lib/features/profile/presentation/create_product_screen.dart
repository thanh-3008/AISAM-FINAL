import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_button.dart';
import '../../../core/shared/app_snackbar.dart';
import '../data/models/product_request.dart';
import 'providers/product_controller.dart';
import '../../../core/state/base_state.dart';
import '../data/models/product_model.dart';

class CreateProductScreen extends ConsumerStatefulWidget {
  final String brandId;
  const CreateProductScreen({super.key, required this.brandId});

  @override
  ConsumerState<CreateProductScreen> createState() => _CreateProductScreenState();
}

class _CreateProductScreenState extends ConsumerState<CreateProductScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _priceController = TextEditingController();
  final _stockController = TextEditingController();

  @override
  void dispose() {
    _nameController.dispose();
    _priceController.dispose();
    _stockController.dispose();
    super.dispose();
  }

  void _onSubmit() {
    if (_formKey.currentState!.validate()) {
      ref.read(createProductControllerProvider.notifier).createProduct(
        CreateProductRequest(
          name: _nameController.text.trim(),
          brandId: widget.brandId,
          price: double.tryParse(_priceController.text.trim()),
          stock: int.tryParse(_stockController.text.trim()) ?? 0,
        )
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final createState = ref.watch(createProductControllerProvider);
    final isLoading = createState.maybeWhen(
      loading: () => true,
      orElse: () => false,
    );

    ref.listen<BaseState<ProductResponseModel>>(createProductControllerProvider, (previous, next) {
      next.maybeWhen(
        error: (error) => AppSnackbar.showError(context, error.toString()),
        data: (product) {
          AppSnackbar.showSuccess(context, 'Product created successfully!');
          context.pop(); // Go back to list
        },
        orElse: () {},
      );
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('Create Product'),
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
                  decoration: const InputDecoration(labelText: 'Product Name *'),
                  validator: (value) {
                    if (value == null || value.isEmpty) return 'Please enter a name';
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _priceController,
                  decoration: const InputDecoration(labelText: 'Price'),
                  keyboardType: const TextInputType.numberWithOptions(decimal: true),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _stockController,
                  decoration: const InputDecoration(labelText: 'Stock'),
                  keyboardType: TextInputType.number,
                ),
                const SizedBox(height: 32),
                AppButton(
                  text: 'Create Product',
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
