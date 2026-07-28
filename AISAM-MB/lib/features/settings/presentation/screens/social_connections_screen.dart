import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/shared/app_snackbar.dart';
import '../../../../core/shared/app_loading_indicator.dart';
import '../providers/social_controller.dart';
import '../../data/repositories/social_repository.dart';
import '../../data/models/social_integration_model.dart';
import 'oauth_webview_screen.dart';
import 'widgets/manage_targets_bottom_sheet.dart';
import 'widgets/select_brand_bottom_sheet.dart';

class SocialConnectionsScreen extends ConsumerWidget {
  const SocialConnectionsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final integrationsState = ref.watch(socialControllerProvider);

    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      appBar: AppBar(
        title: const Text('Kết nối mạng xã hội'),
        elevation: 0,
        backgroundColor: Theme.of(context).colorScheme.surface.withOpacity(0.8),
      ),
      body: integrationsState.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Đã có lỗi xảy ra: $error'),
              ElevatedButton(
                onPressed: () => ref.read(socialControllerProvider.notifier).refresh(),
                child: const Text('Thử lại'),
              ),
            ],
          ),
        ),
        data: (integrations) {
          if (integrations.isEmpty) {
            return _buildEmptyState(context);
          }
          return RefreshIndicator(
            onRefresh: () => ref.read(socialControllerProvider.notifier).refresh(),
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: integrations.length,
              itemBuilder: (context, index) {
                final integration = integrations[index];
                return _buildIntegrationCard(context, ref, integration);
              },
            ),
          );
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          _showConnectOptions(context, ref);
        },
        icon: const Icon(Icons.add),
        label: const Text('Thêm kết nối'),
      ),
    );
  }

  Widget _buildEmptyState(BuildContext context) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.link_off, size: 64, color: Theme.of(context).colorScheme.primary.withOpacity(0.5)),
          const SizedBox(height: 16),
          Text(
            'Chưa có kết nối nào',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          Text(
            'Kết nối với Facebook, TikTok... để tự động đăng bài',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.grey),
          ),
        ],
      ),
    );
  }

  Widget _buildIntegrationCard(BuildContext context, WidgetRef ref, SocialIntegrationModel integration) {
    IconData iconData = Icons.link;
    Color iconColor = Colors.grey;

    final platformStr = integration.platform.toLowerCase();
    if (platformStr.contains('facebook')) {
      iconData = Icons.facebook;
      iconColor = Colors.blue;
    } else if (platformStr.contains('tiktok')) {
      iconData = Icons.tiktok;
      iconColor = Colors.black;
    } else if (platformStr.contains('instagram')) {
      iconData = Icons.camera_alt; // basic fallback
      iconColor = Colors.purple;
    }

    return Card(
      margin: const EdgeInsets.only(bottom: 16),
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.5)),
      ),
      child: ListTile(
        contentPadding: const EdgeInsets.all(16),
        leading: CircleAvatar(
          backgroundColor: iconColor.withOpacity(0.1),
          child: integration.profilePictureUrl != null && integration.profilePictureUrl!.isNotEmpty
              ? ClipOval(child: Image.network(integration.profilePictureUrl!, fit: BoxFit.cover, width: 40, height: 40))
              : Icon(iconData, color: iconColor),
        ),
        title: Text(
          integration.name ?? integration.platform,
          style: const TextStyle(fontWeight: FontWeight.bold),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (integration.brandName != null) Text('Brand: ${integration.brandName}'),
            const SizedBox(height: 4),
            Row(
              children: [
                Container(
                  width: 8,
                  height: 8,
                  decoration: BoxDecoration(
                    color: integration.isActive ? Colors.green : Colors.red,
                    shape: BoxShape.circle,
                  ),
                ),
                const SizedBox(width: 4),
                Text(
                  integration.isActive ? 'Đang hoạt động' : 'Đã ngắt kết nối',
                  style: Theme.of(context).textTheme.bodySmall,
                ),
              ],
            ),
          ],
        ),
        trailing: IconButton(
          icon: const Icon(Icons.delete_outline, color: Colors.red),
          onPressed: () => _confirmDelete(context, ref, integration),
        ),
      ),
    );
  }

  Future<void> _confirmDelete(BuildContext context, WidgetRef ref, SocialIntegrationModel integration) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Ngắt kết nối'),
        content: Text('Bạn có chắc chắn muốn ngắt kết nối với ${integration.name ?? integration.platform}?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Hủy')),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Ngắt kết nối', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );

    if (confirm == true && context.mounted) {
      try {
        await ref.read(socialControllerProvider.notifier).deleteIntegration(integration.id);
        if (context.mounted) {
          AppSnackbar.showSuccess(context, 'Đã ngắt kết nối thành công.');
        }
      } catch (e) {
        if (context.mounted) {
          AppSnackbar.showError(context, e.toString());
        }
      }
    }
  }

  void _connectPlatform(BuildContext context, WidgetRef ref, String platform) async {
    Navigator.pop(context); // Close the platform selection sheet
    try {
      // 1. Select Brand First
      final selectedBrandId = await showModalBottomSheet<String>(
        context: context,
        isScrollControlled: true,
        backgroundColor: Colors.transparent,
        builder: (context) => const SelectBrandBottomSheet(),
      );

      if (selectedBrandId == null) {
        // User cancelled brand selection
        return;
      }

      // 2. Show loading while getting auth url
      if (!context.mounted) return;
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (context) => const Center(child: AppLoadingIndicator()),
      );

      final repository = ref.read(socialRepositoryProvider);
      final authUrl = await repository.getAuthUrl(platform);
      
      // 3. Hide loading
      if (context.mounted) {
        Navigator.pop(context);
      }

      // 4. Open WebView and wait for result
      if (!context.mounted) return;
      final result = await Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => OAuthWebViewScreen(authUrl: authUrl, platform: platform),
        ),
      );

      // 4. Handle result
      if (result != null && result is Map<String, String>) {
        final code = result['code'];
        final state = result['state'];
        
        if (code != null && state != null) {
          // Show processing loading
          if (context.mounted) {
            showDialog(
              context: context,
              barrierDismissible: false,
              builder: (context) => const Center(child: AppLoadingIndicator()),
            );
          }
          
          final accountId = await repository.handleCallback(platform, code, state);
          
          if (context.mounted) {
            Navigator.pop(context); // close loading
            
            // Show Manage Targets Bottom Sheet
            await showModalBottomSheet(
              context: context,
              isScrollControlled: true,
              backgroundColor: Colors.transparent,
              builder: (context) => ManageTargetsBottomSheet(
                accountId: accountId,
                platform: platform,
                preselectedBrandId: selectedBrandId,
              ),
            );

            ref.read(socialControllerProvider.notifier).refresh();
          }
        }
      }
    } catch (e) {
      if (context.mounted) {
        // Ensure any loading dialog is closed
        Navigator.of(context).popUntil((route) => route.isFirst || route.settings.name == '/settings/social');
        AppSnackbar.showError(context, 'Lỗi kết nối: $e');
      }
    }
  }

  void _showConnectOptions(BuildContext context, WidgetRef ref) {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (bottomSheetContext) {
        return SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Padding(
                padding: EdgeInsets.all(16.0),
                child: Text('Chọn nền tảng để kết nối', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
              ),
              ListTile(
                leading: const Icon(Icons.facebook, color: Colors.blue),
                title: const Text('Facebook Page'),
                onTap: () => _connectPlatform(context, ref, 'facebook'),
              ),
              ListTile(
                leading: const Icon(Icons.tiktok, color: Colors.black),
                title: const Text('TikTok Account'),
                onTap: () => _connectPlatform(context, ref, 'tiktok'),
              ),
              ListTile(
                leading: const Icon(Icons.camera_alt, color: Colors.purple),
                title: const Text('Instagram Account'),
                onTap: () => _connectPlatform(context, ref, 'instagram'),
              ),
              const SizedBox(height: 16),
            ],
          ),
        );
      },
    );
  }
}
