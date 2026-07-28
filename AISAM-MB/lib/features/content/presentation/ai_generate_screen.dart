import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_button.dart';
import '../../../core/shared/app_snackbar.dart';
import 'providers/ai_generation_controller.dart';
import '../data/models/ai_generation_request.dart';
import '../data/models/enums.dart';
import '../../../core/state/base_state.dart';
import '../data/models/ai_generation_response.dart';

class AiGenerateScreen extends ConsumerStatefulWidget {
  const AiGenerateScreen({super.key});

  @override
  ConsumerState<AiGenerateScreen> createState() => _AiGenerateScreenState();
}

class _AiGenerateScreenState extends ConsumerState<AiGenerateScreen> {
  final _formKey = GlobalKey<FormState>();
  final _brandIdController = TextEditingController();
  final _titleController = TextEditingController();
  final _promptController = TextEditingController();

  @override
  void dispose() {
    _brandIdController.dispose();
    _titleController.dispose();
    _promptController.dispose();
    super.dispose();
  }

  void _onGenerate() {
    if (_formKey.currentState!.validate()) {
      ref.read(aiGenerationControllerProvider.notifier).generateDraft(
        CreateDraftRequest(
          brandId: _brandIdController.text.trim(),
          adType: AdTypeEnum.textOnly,
          title: _titleController.text.trim(),
          prompt: _promptController.text.trim(),
        )
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final aiState = ref.watch(aiGenerationControllerProvider);
    final isLoading = aiState.maybeWhen(loading: () => true, orElse: () => false);

    ref.listen<BaseState<AiGenerationResponseModel>>(aiGenerationControllerProvider, (previous, next) {
      next.maybeWhen(
        error: (error) => AppSnackbar.showError(context, error.toString()),
        data: (response) {
          // Navigate to editor with pre-filled content
          context.pushReplacement('/content/create', extra: {
            'brandId': _brandIdController.text.trim(),
            'title': _titleController.text.trim(),
            'content': response.generatedText,
          });
        },
        orElse: () {},
      );
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('AI Content Generation'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Form(
            key: _formKey,
            child: ListView(
              children: [
                const Text(
                  'Describe what you want Gemini to write for you.',
                  style: TextStyle(fontSize: 16),
                ),
                const SizedBox(height: 24),
                TextFormField(
                  controller: _brandIdController,
                  decoration: const InputDecoration(labelText: 'Brand ID * (UUID)'),
                  validator: (value) => value == null || value.isEmpty ? 'Required' : null,
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _titleController,
                  decoration: const InputDecoration(labelText: 'Title / Topic (Optional)'),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _promptController,
                  decoration: const InputDecoration(
                    labelText: 'Prompt Instructions *',
                    alignLabelWithHint: true,
                    hintText: 'e.g. Write a social media post about our new summer collection...',
                  ),
                  maxLines: 5,
                  validator: (value) => value == null || value.isEmpty ? 'Required' : null,
                ),
                const SizedBox(height: 32),
                AppButton(
                  text: 'Generate with AI',
                  isLoading: isLoading,
                  onPressed: _onGenerate,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
