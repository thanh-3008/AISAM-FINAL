import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_button.dart';
import '../../../core/shared/app_snackbar.dart';
import 'providers/content_editor_controller.dart';
import 'providers/content_permissions.dart';
import '../../../core/storage/secure_storage.dart';
import '../data/models/content_request.dart';
import '../data/models/content_model.dart';
import '../data/models/enums.dart';
import '../../../core/state/base_state.dart';
import '../../profile/presentation/providers/brand_controller.dart';
import 'dart:convert';

// In-memory draft fallback
final _drafts = <String,String>{};

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
  late final String _scope;
  bool _loaded = false;
  bool _saved = false;
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _titleController;
  late final TextEditingController _contentController;
  late final TextEditingController _brandIdController;

  @override
  void initState() {
    super.initState();
    final storage=ref.read(secureStorageProvider);
    _scope='${storage.cachedUserId}:${storage.cachedWorkspaceId}';
    _titleController = TextEditingController(text: widget.prefillTitle ?? '');
    _contentController = TextEditingController(text: widget.prefillContent ?? '');
    _brandIdController = TextEditingController(text: widget.prefillBrandId ?? '');
    
    if (widget.contentId == null && widget.prefillContent == null) {
      _loadDraft();
    }
  }

  void _loadDraft() {
    if (_drafts[_scope] != null) {
      final map = jsonDecode(_drafts[_scope]!);
      setState(() {
        _titleController.text = map['title'] ?? '';
        _contentController.text = map['content'] ?? '';
        _brandIdController.text = map['brandId'] ?? '';
      });
    }
  }

  void _saveDraft() {
    if (widget.contentId != null || _saved) return;
    final map = {
      'title': _titleController.text,
      'content': _contentController.text,
      'brandId': _brandIdController.text,
    };
    _drafts[_scope] = jsonEncode(map);
  }

  void _clearDraft() {
    _saved = true;
    _drafts.remove(_scope);
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
    final storage=ref.read(secureStorageProvider);
    if(_scope!='${storage.cachedUserId}:${storage.cachedWorkspaceId}') {
      AppSnackbar.showError(context,'Workspace changed. Reopen the editor.');
      return;
    }
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
    final storage=ref.read(secureStorageProvider);
    if(_scope!='${storage.cachedUserId}:${storage.cachedWorkspaceId}') {
      return const Scaffold(body:Center(child:Text('Workspace changed. Reopen the editor.')));
    }
    if(widget.contentId != null) {
      final permissions=ref.watch(contentPermissionsProvider(widget.contentId!));
      final detail=ref.watch(contentDetailControllerProvider(widget.contentId!));
      if(permissions.isLoading || detail.isLoading) return const Scaffold(body:Center(child:CircularProgressIndicator()));
      if(permissions.valueOrNull?[1]!=true || !detail.hasValue) {
        return Scaffold(appBar:AppBar(),body:const Center(child:Text('Content is unavailable or editing is not allowed.')));
      }
      if(!_loaded) {
        final content=detail.requireValue;
        _titleController.text=content.title??'';
        _contentController.text=content.textContent;
        _loaded=true;
      }
    }
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
          if (context.mounted) context.go('/content/${content.id}');
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
                  ref.watch(brandControllerProvider).when(
                    data: (brands) {
                      if (brands.isEmpty) {
                        return TextFormField(
                          controller: _brandIdController,
                          decoration: const InputDecoration(
                            labelText: 'Brand ID * (UUID)',
                            helperText: 'Chưa có Brand nào, vui lòng tạo Brand trước',
                          ),
                          validator: (value) => value == null || value.isEmpty ? 'Required' : null,
                        );
                      }
                      final currentVal = _brandIdController.text.isNotEmpty && brands.any((b) => b.id == _brandIdController.text)
                          ? _brandIdController.text
                          : null;
                      return DropdownButtonFormField<String>(
                        value: currentVal,
                        decoration: const InputDecoration(
                          labelText: 'Chọn Brand *',
                          prefixIcon: Icon(Icons.business_outlined),
                        ),
                        items: brands.map((b) => DropdownMenuItem(
                          value: b.id,
                          child: Text(b.name, overflow: TextOverflow.ellipsis),
                        )).toList(),
                        onChanged: (val) {
                          if (val != null) {
                            setState(() {
                              _brandIdController.text = val;
                            });
                          }
                        },
                        validator: (value) => (_brandIdController.text.isEmpty) ? 'Vui lòng chọn Brand' : null,
                      );
                    },
                    loading: () => const LinearProgressIndicator(),
                    error: (_, __) => TextFormField(
                      controller: _brandIdController,
                      decoration: const InputDecoration(labelText: 'Brand ID * (UUID)'),
                      validator: (value) => value == null || value.isEmpty ? 'Required' : null,
                    ),
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
                const Text('Mobile edits plain text. Saving text changes replaces existing rich formatting. Use Web to edit formatting.'),
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
