import 'dart:math';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../data/repositories/publishing_repository.dart';

class MobileComposer extends StatefulWidget {
  final String contentId, scope;
  final PublishingRepository repository;
  final bool canEdit;
  const MobileComposer({
    super.key,
    required this.contentId,
    required this.scope,
    required this.repository,
    required this.canEdit,
  });
  @override
  State<MobileComposer> createState() => _MobileComposerState();
}

class _MobileComposerState extends State<MobileComposer> {
  List<Map<String, dynamic>> items = [], destinations = [];
  List<dynamic> operations = [];
  final selected = <String>{};
  String? version, key, error;
  bool busy = false, dirty = false, loaded = false;
  String get journalKey => 'publish:${widget.scope}:${widget.contentId}';
  @override
  void initState() {
    super.initState();
    run(load);
  }

  Future<void> run(Future<void> Function() action) async {
    if (busy) return;
    setState(() {
      busy = true;
      error = null;
    });
    try {
      await action();
    } catch (e) {
      if (mounted) setState(() => error = e.toString());
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  Future<void> load() async {
    final media = await widget.repository.media(widget.contentId);
    final preview = await widget.repository.preview(widget.contentId);
    final prefs = await SharedPreferences.getInstance();
    if (!mounted) return;
    setState(() {
      version = media['version'] as String;
      items = (media['items'] as List)
          .map((e) => Map<String, dynamic>.from(e))
          .toList();
      destinations = (preview['destinations'] as List)
          .map((e) => Map<String, dynamic>.from(e))
          .toList();
      selected.clear();
      key = prefs.getString(journalKey);
      loaded = true;
      dirty = false;
    });
    if (key != null) await check();
  }

  Future<void> check() async {
    final result = await widget.repository.operations(widget.contentId, key!);
    if (mounted) setState(() => operations = result);
  }

  Future<void> pick(bool video) async {
    final picker = ImagePicker();
    final files = video
        ? [
            await picker.pickVideo(source: ImageSource.gallery),
          ].whereType<XFile>().toList()
        : await picker.pickMultiImage();
    if (!mounted || files.isEmpty) return;
    if (items.length + files.length > 10)
      throw StateError('Maximum 10 media items.');
    var total = 0;
    for (final file in files) {
      total += await file.length();
    }
    if (total > 200 * 1024 * 1024)
      throw StateError('Maximum 200 MiB per selection.');
    for (final file in files) {
      final length = await file.length();
      if (length > 50 * 1024 * 1024)
        throw StateError('${file.name}: maximum 50 MiB.');
      final suffix = file.name.toLowerCase().split('.').last;
      final mime = switch (suffix) {
        'png' => 'image/png',
        'jpg' || 'jpeg' => 'image/jpeg',
        'webp' => 'image/webp',
        'mp4' => 'video/mp4',
        'mov' => 'video/quicktime',
        'webm' => 'video/webm',
        _ => null,
      };
      if (mime == null) throw StateError('Unsupported media: ${file.name}');
      final part = MultipartFile.fromBytes(
        await file.readAsBytes(),
        filename: file.name,
        contentType: DioMediaType.parse(mime),
      );
      if (!mounted) return;
      final result = await widget.repository.upload(widget.contentId, [part]);
      final row = Map<String, dynamic>.from(result.first);
      if (row['assetId'] == null || row['error'] != null)
        throw StateError('${file.name}: ${row['error']}');
      if (!mounted) return;
      setState(() {
        items.add({...row, 'mimeType': mime, 'isCover': false});
        dirty = true;
      });
    }
  }

  Future<void> save() async {
    await widget.repository.saveMedia(widget.contentId, version!, items);
    if (mounted) await load();
  }

  Future<void> publish() async {
    // Write the journal key before sending. Unknown outcomes are read, never replayed.
    final value = List.generate(
      24,
      (_) => Random.secure().nextInt(256).toRadixString(16).padLeft(2, '0'),
    ).join();
    final prefs = await SharedPreferences.getInstance();
    if (!mounted) return;
    if (!await prefs.setString(journalKey, value))
      throw StateError('Cannot save publish journal.');
    if (!mounted) return;
    setState(() => key = value);
    final result = await widget.repository.publish(
      widget.contentId,
      version!,
      selected.toList(),
      value,
    );
    if (mounted) setState(() => operations = result);
  }

  @override
  Widget build(BuildContext context) => Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      const Divider(),
      const Text(
        'Media & publishing',
        style: TextStyle(fontWeight: FontWeight.bold),
      ),
      if (busy) const LinearProgressIndicator(),
      if (error != null)
        Text(error!, style: const TextStyle(color: Colors.red)),
      for (var i = 0; i < items.length; i++)
        ListTile(
          title: Text('${i + 1}. ${items[i]['mimeType'] ?? 'Media'}'),
          subtitle: Text(
            '${items[i]['url'] ?? ''}',
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
          trailing: widget.canEdit && key == null
              ? Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    IconButton(
                      tooltip: 'Move up',
                      icon: const Icon(Icons.arrow_upward),
                      onPressed: busy || i == 0
                          ? null
                          : () {
                              setState(() {
                                final row = items.removeAt(i);
                                items.insert(i - 1, row);
                                dirty = true;
                              });
                            },
                    ),
                    IconButton(
                      tooltip: 'Remove',
                      icon: const Icon(Icons.remove_circle_outline),
                      onPressed: busy
                          ? null
                          : () {
                              setState(() {
                                items.removeAt(i);
                                dirty = true;
                              });
                            },
                    ),
                  ],
                )
              : null,
        ),
      if (widget.canEdit && key == null && loaded)
        Wrap(
          spacing: 8,
          children: [
            if (items.isEmpty)
              TextButton(
                onPressed: busy
                    ? null
                    : () => run(() async {
                        await widget.repository.importLegacy(
                          widget.contentId,
                          version!,
                        );
                        if (mounted) await load();
                      }),
                child: const Text('Import legacy media'),
              ),
            TextButton(
              onPressed: busy ? null : () => run(() => pick(false)),
              child: const Text('Add images'),
            ),
            TextButton(
              onPressed: busy ? null : () => run(() => pick(true)),
              child: const Text('Add video'),
            ),
            TextButton(
              onPressed: busy || !dirty ? null : () => run(save),
              child: const Text('Save media'),
            ),
            TextButton(
              onPressed: busy || dirty
                  ? null
                  : () => run(() async {
                      await widget.repository.submit(widget.contentId);
                      if (mounted) await load();
                    }),
              child: const Text('Submit for review'),
            ),
          ],
        ),
      if (dirty)
        const Text(
          'Save media before publishing. Unsaved order is lost when leaving this screen.',
        ),
      for (final destination in destinations)
        CheckboxListTile(
          title: Text('${destination['name']} (${destination['platform']})'),
          subtitle: destination['error'] == null
              ? null
              : Text('${destination['error']}'),
          value: selected.contains(destination['id']),
          onChanged:
              busy || dirty || key != null || destination['error'] != null
              ? null
              : (value) {
                  setState(() {
                    if (value == true) {
                      selected.add(destination['id'] as String);
                    } else {
                      selected.remove(destination['id']);
                    }
                  });
                },
        ),
      if (key == null)
        FilledButton(
          onPressed: busy || dirty || !loaded || selected.isEmpty
              ? null
              : () => run(publish),
          child: const Text('Publish selected'),
        ),
      if (key != null) ...[
        const Text(
          'A publish request has been recorded. Check its result before taking further action.',
        ),
        TextButton(
          onPressed: busy ? null : () => run(check),
          child: const Text('Refresh publish status'),
        ),
        for (final op in operations)
          ListTile(
            title: Text('${op['integrationId']}: ${op['status']}'),
            subtitle: Text('${op['errorCode'] ?? op['providerId'] ?? ''}'),
          ),
      ],
      TextButton(
        onPressed: busy || dirty ? null : () => run(load),
        child: const Text('Reload media and permissions'),
      ),
    ],
  );
}
