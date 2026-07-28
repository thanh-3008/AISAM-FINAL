import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:video_player/video_player.dart';
import '../../../../core/shared/app_loading_indicator.dart';
import '../../../../core/shared/app_snackbar.dart';
import '../providers/content_editor_controller.dart';

class TiktokVideoReviewScreen extends ConsumerStatefulWidget {
  final String contentId;
  const TiktokVideoReviewScreen({super.key, required this.contentId});

  @override
  ConsumerState<TiktokVideoReviewScreen> createState() => _TiktokVideoReviewScreenState();
}

class _TiktokVideoReviewScreenState extends ConsumerState<TiktokVideoReviewScreen> {
  VideoPlayerController? _videoPlayerController;
  bool _isPlaying = true;
  String? _loadedVideoUrl;

  @override
  void dispose() {
    _videoPlayerController?.dispose();
    super.dispose();
  }

  Future<void> _initializeVideo(String url) async {
    if (_loadedVideoUrl == url) return;
    _loadedVideoUrl = url;
    
    _videoPlayerController?.dispose();
    _videoPlayerController = VideoPlayerController.networkUrl(Uri.parse(url));
    
    try {
      await _videoPlayerController!.initialize();
      await _videoPlayerController!.setLooping(true);
      await _videoPlayerController!.play();
      if (mounted) {
        setState(() {});
      }
    } catch (e) {
      debugPrint('Error initializing video: $e');
    }
  }

  void _togglePlayPause() {
    if (_videoPlayerController == null || !_videoPlayerController!.value.isInitialized) return;
    
    setState(() {
      if (_videoPlayerController!.value.isPlaying) {
        _videoPlayerController!.pause();
        _isPlaying = false;
      } else {
        _videoPlayerController!.play();
        _isPlaying = true;
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    // We are putting this inside features/content/presentation/screens
    // But content_editor_controller is in features/content/presentation/providers
    // Wait, the path is `lib/features/content/presentation/providers/content_editor_controller.dart`
    final detailState = ref.watch(contentDetailControllerProvider(widget.contentId));

    return Scaffold(
      backgroundColor: Colors.black,
      body: detailState.when(
        loading: () => const Center(child: AppLoadingIndicator(color: Colors.white)),
        error: (error, stack) => Center(child: Text('Error: $error', style: const TextStyle(color: Colors.red))),
        data: (content) {
          if (content.videoUrl == null || content.videoUrl!.isEmpty) {
            return const Center(child: Text('Video is still processing or unavailable.', style: TextStyle(color: Colors.white)));
          }

          _initializeVideo(content.videoUrl!);

          return Stack(
            fit: StackFit.expand,
            children: [
              // 1. Video Background
              GestureDetector(
                onTap: _togglePlayPause,
                child: _videoPlayerController != null && _videoPlayerController!.value.isInitialized
                    ? AspectRatio(
                        aspectRatio: _videoPlayerController!.value.aspectRatio,
                        child: VideoPlayer(_videoPlayerController!),
                      )
                    : const Center(child: AppLoadingIndicator(color: Colors.white)),
              ),

              // Play/Pause Overlay Icon
              if (!_isPlaying)
                Center(
                  child: GestureDetector(
                    onTap: _togglePlayPause,
                    child: const Icon(Icons.play_arrow, size: 80, color: Colors.white54),
                  ),
                ),

              // 2. Top App Bar (Back button)
              Positioned(
                top: MediaQuery.of(context).padding.top + 10,
                left: 10,
                child: IconButton(
                  icon: const Icon(Icons.arrow_back, color: Colors.white, size: 30),
                  onPressed: () => context.pop(),
                ),
              ),

              // 3. Right Side Action Buttons
              Positioned(
                bottom: 120,
                right: 16,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    _buildActionButton(
                      icon: Icons.check_circle,
                      label: 'Approve',
                      onTap: () {
                        AppSnackbar.showSuccess(context, 'Video Approved!');
                        context.pop();
                      },
                    ),
                    const SizedBox(height: 20),
                    _buildActionButton(
                      icon: Icons.edit,
                      label: 'Edit Post',
                      onTap: () {
                        context.push('/content/${widget.contentId}/edit');
                      },
                    ),
                    const SizedBox(height: 20),
                    _buildActionButton(
                      icon: Icons.refresh,
                      label: 'Retry',
                      onTap: () {
                        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Regenerate coming soon')));
                      },
                    ),
                  ],
                ),
              ),

              // 4. Bottom Left Text Overlay
              Positioned(
                bottom: 40,
                left: 16,
                right: 80, // leave space for right buttons
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(
                      content.title ?? 'AI Generated Video',
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: 18,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      content.textContent,
                      maxLines: 3,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildActionButton({required IconData icon, required String label, required VoidCallback onTap}) {
    return GestureDetector(
      onTap: onTap,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: const BoxDecoration(
              color: Colors.black45,
              shape: BoxShape.circle,
            ),
            child: Icon(icon, color: Colors.white, size: 28),
          ),
          const SizedBox(height: 4),
          Text(
            label,
            style: const TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.w500),
          ),
        ],
      ),
    );
  }
}
