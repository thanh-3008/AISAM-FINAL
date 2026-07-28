import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_button.dart';
import '../../../core/shared/app_snackbar.dart';
import 'providers/content_editor_controller.dart';
import '../data/models/content_request.dart';
import '../data/models/content_model.dart';
import '../data/models/enums.dart';
import '../../../core/state/base_state.dart';
import 'dart:convert';

// In-memory draft fallback
String? _inMemoryDraft;

class ContentEditorScreen extends ConsumerStatefulWidget {
  final String? contentId; // null = create
  final String? prefillBrandId; 
  final String? prefillTitle;
  final String? prefillContent;

  const ContentEditorScreen({
    super.key, 
    this.contentId, 
    this.prefillBrandId,
    this.prefillTitle,
    this.prefillContent,
  });

  @override
  ConsumerState<ContentEditorScreen> createState() => _ContentEditorScreenState();
}

class _ContentEditorScreenState extends ConsumerState<ContentEditorScreen> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _titleController;
  late final TextEditingController _contentController;
  late final TextEditingController _brandIdController;

  @override
  void initState() {
    super.initState();
    _titleController = TextEditingController(text: widget.prefillTitle ?? '');
    _contentController = TextEditingController(text: widget.prefillContent ?? '');
    _brandIdController = TextEditingController(text: widget.prefillBrandId ?? '');
    
    if (widget.contentId == null && widget.prefillContent == null) {
      _loadDraft();
    }
  }

  void _loadDraft() {
    if (_inMemoryDraft != null) {
      final map = jsonDecode(_inMemoryDraft!);
      setState(() {
        _titleController.text = map['title'] ?? '';
        _contentController.text = map['content'] ?? '';
        _brandIdController.text = map['brandId'] ?? '';
      });
    }
  }

  void _saveDraft() {
    if (widget.contentId != null) return; // Don't save draft for edit mode
    final map = {
      'title': _titleController.text,
      'content': _contentController.text,
      'brandId': _brandIdController.text,
    };
    _inMemoryDraft = jsonEncode(map);
  }

  void _clearDraft() {
    _inMemoryDraft = null;
  }

  @override
  void dispose() {
    _saveDraft();
    _titleController.dispose();
    _contentController.dispose();
    _brandIdController.dispose();
    super.dispose();
  }

  void _onSubmit() {
    if (_formKey.currentState!.validate()) {
      if (widget.contentId == null) {
        ref.read(contentEditorControllerProvider.notifier).createContent(
          CreateContentRequest(
            brandId: _brandIdController.text.trim(),
            adType: AdTypeEnum.textOnly,
            title: _titleController.text.trim(),
            textContent: _contentController.text.trim(),
          )
        );
      } else {
        ref.read(contentEditorControllerProvider.notifier).updateContent(
          widget.contentId!,
          UpdateContentRequest(
            title: _titleController.text.trim(),
            textContent: _contentController.text.trim(),
          )
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final editorState = ref.watch(contentEditorControllerProvider);
    final isLoading = editorState.maybeWhen(loading: () => true, orElse: () => false);

    ref.listen<BaseState<ContentResponseModel>>(contentEditorControllerProvider, (previous, next) {
      next.maybeWhen(
        error: (error) => AppSnackbar.showError(context, error.toString()),
        data: (content) async {
          AppSnackbar.showSuccess(context, 'Content saved successfully!');
          if (widget.contentId == null) {
            _clearDraft();
          }
          if (context.mounted) context.pop();
        },
        orElse: () {},
      );
    });

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.contentId == null ? 'Create Content' : 'Edit Content'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Form(
            key: _formKey,
            child: ListView(
              children: [
                if (widget.contentId == null) ...[
                  TextFormField(
                    controller: _brandIdController,
                    decoration: const InputDecoration(labelText: 'Brand ID * (UUID)'),
                    validator: (value) => value == null || value.isEmpty ? 'Required' : null,
                  ),
                  const SizedBox(height: 16),
                ],
                TextFormField(
                  controller: _titleController,
                  decoration: const InputDecoration(labelText: 'Title (Optional)'),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _contentController,
                  decoration: const InputDecoration(
                    labelText: 'Content *',
                    alignLabelWithHint: true,
                  ),
                  maxLines: 10,
                  validator: (value) => value == null || value.isEmpty ? 'Required' : null,
                ),
                const SizedBox(height: 32),
                AppButton(
                  text: 'Save Content',
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
