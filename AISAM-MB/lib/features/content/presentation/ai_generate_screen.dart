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
import '../../../core/network/rbac_context.dart';
import '../../../core/storage/secure_storage.dart';
import '../../../shared/widgets/team_scope_field.dart';

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
  String? _teamId;
  late final String _scope;

  @override
  void initState() {
    super.initState();
    final storage = ref.read(secureStorageProvider);
    _scope = '${storage.cachedUserId}:${storage.cachedWorkspaceId}';
  }

  @override
  void dispose() {
    _brandIdController.dispose();
    _titleController.dispose();
    _promptController.dispose();
    super.dispose();
  }

  void _onGenerate() {
    final storage = ref.read(secureStorageProvider);
    final access = ref.read(rbacContextProvider).valueOrNull;
    if (_scope != '${storage.cachedUserId}:${storage.cachedWorkspaceId}' ||
        access == null || (access.isV2 && !access.scopes.any((s) =>
          s.teamId == _teamId && s.brandId == _brandIdController.text.trim() && access.canCreate(s)))) {
      AppSnackbar.showError(context, 'Chọn Team có quyền tạo nội dung trong workspace hiện tại.');
      return;
    }
    if (_formKey.currentState!.validate()) {
      ref.read(aiGenerationControllerProvider.notifier).generateDraft(
        CreateDraftRequest(
          brandId: _brandIdController.text.trim(),
          teamId: _teamId,
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
          // The API already persisted this draft under the selected Team.
          context.pushReplacement('/content/${response.contentId}');
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
                  onChanged: (_) => setState(() => _teamId = null),
                  decoration: const InputDecoration(labelText: 'Brand ID * (UUID)'),
                  validator: (value) => value == null || value.isEmpty ? 'Required' : null,
                ),
                const SizedBox(height: 16),
                TeamScopeField(brandId: _brandIdController.text.trim(), value: _teamId,
                  onChanged: (id) => setState(() => _teamId = id)),
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
