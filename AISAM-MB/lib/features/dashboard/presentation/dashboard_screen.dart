import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:fl_chart/fl_chart.dart';

import 'package:go_router/go_router.dart';
import 'dashboard_controller.dart';
import '../../content/data/models/enums.dart';
import '../../auth/presentation/providers/auth_controller.dart';
import '../../workspace/presentation/providers/workspace_controller.dart';
import '../../../core/shared/aisam_logo_widget.dart';
import '../../../core/shared/profile_avatar_widget.dart';

class DashboardScreen extends ConsumerWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final dashboardState = ref.watch(dashboardControllerProvider);
    final recentActivitiesState = ref.watch(recentActivitiesControllerProvider);
    final authState = ref.watch(authControllerProvider);
    final activeWorkspaceAsync = ref.watch(activeWorkspaceControllerProvider);

    final userName = authState.maybeWhen(
      data: (response) => response.user.fullName ?? 'Người dùng',
      orElse: () => 'Người dùng',
    );
    final shortName = userName.split(' ').last;

    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.surface.withOpacity(0.8),
        elevation: 0,
        scrolledUnderElevation: 0,
        title: const AisamLogoWidget(),
        actions: const [
          ProfileAvatarWidget(),
          SizedBox(width: 8),
        ],
      ),
      body: dashboardState.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Error: $error', style: const TextStyle(color: Colors.red)),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () {
                  ref.read(dashboardControllerProvider.notifier).refresh();
                },
                child: const Text('Thử lại'),
              ),
            ],
          ),
        ),
        data: (data) => RefreshIndicator(
          onRefresh: () async {
            await ref.read(dashboardControllerProvider.notifier).refresh();
            ref.read(recentActivitiesControllerProvider.notifier).refresh();
          },
          child: ListView(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            children: [
              // Header Section
              Text(
                'Chào, $shortName',
                style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: Theme.of(context).colorScheme.onSurface,
                    ),
              ),
              const SizedBox(height: 4),
              Text(
                'Sẵn sàng để quản lý chiến dịch hôm nay?',
                style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                      color: Theme.of(context).colorScheme.onSurfaceVariant,
                    ),
              ),
              const SizedBox(height: 16),

              // Workspace Switcher Pill
              Align(
                alignment: Alignment.centerLeft,
                child: InkWell(
                  onTap: () => context.push('/overview'),
                  borderRadius: BorderRadius.circular(24),
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.surfaceContainerLow,
                      border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.5)),
                      borderRadius: BorderRadius.circular(24),
                    ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Container(
                        width: 12,
                        height: 12,
                        decoration: BoxDecoration(
                          color: Theme.of(context).colorScheme.primary,
                          shape: BoxShape.circle,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Text(
                        activeWorkspaceAsync.valueOrNull?.name ?? 'Đang tải...',
                        style: Theme.of(context).textTheme.labelLarge?.copyWith(
                              fontWeight: FontWeight.bold,
                              color: Theme.of(context).colorScheme.onSurface,
                            ),
                      ),
                      const SizedBox(width: 4),
                      Icon(Icons.unfold_more, size: 20, color: Theme.of(context).colorScheme.onSurfaceVariant),
                    ],
                  ),
                ),
                ),
              ),
              const SizedBox(height: 24),

              // Quick Stats Grid
              GridView.count(
                crossAxisCount: 2,
                crossAxisSpacing: 16,
                mainAxisSpacing: 16,
                childAspectRatio: 1.1,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                children: [
                  _buildBentoStatCard(
                    context,
                    value: data.publishedPostCount.toString(),
                    label: 'Bài đã đăng',
                    icon: Icons.send,
                    iconBgColor: Theme.of(context).colorScheme.primaryContainer,
                    iconColor: Theme.of(context).colorScheme.onPrimaryContainer,
                  ),
                  _buildBentoStatCard(
                    context,
                    value: data.activeMemberCount.toString(),
                    label: 'Thành viên',
                    icon: Icons.group,
                    iconBgColor: Theme.of(context).colorScheme.secondaryContainer,
                    iconColor: Theme.of(context).colorScheme.onSecondaryContainer,
                  ),
                  _buildBentoStatCard(
                    context,
                    value: data.creditBalance.toString(),
                    label: 'Credit còn lại',
                    icon: Icons.monetization_on,
                    iconBgColor: Theme.of(context).colorScheme.tertiaryContainer,
                    iconColor: Theme.of(context).colorScheme.onTertiaryContainer,
                  ),
                  _buildBentoStatCard(
                    context,
                    value: data.aiUsageCount.toString(),
                    label: 'Lượt dùng AI',
                    icon: Icons.auto_awesome,
                    iconBgColor: Colors.orange.shade100,
                    iconColor: Colors.orange.shade800,
                  ),
                ],
              ),
              const SizedBox(height: 24),

              // Report Summary Card
              Container(
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.surface,
                  borderRadius: BorderRadius.circular(24),
                  border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.3)),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.04),
                      blurRadius: 24,
                      offset: const Offset(0, 8),
                    ),
                  ],
                ),
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          'Top Members Usage',
                          style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
                        ),
                        Icon(Icons.insights, color: Theme.of(context).colorScheme.onSurfaceVariant),
                      ],
                    ),
                    const SizedBox(height: 24),
                    if (data.topMembers.isNotEmpty)
                      SizedBox(
                        height: 180,
                        child: BarChart(
                          BarChartData(
                            alignment: BarChartAlignment.spaceAround,
                            maxY: (data.topMembers.map((e) => e.creditsUsed).reduce((a, b) => a > b ? a : b) * 1.2).toDouble(),
                            barTouchData: BarTouchData(enabled: false),
                            titlesData: FlTitlesData(
                              show: true,
                              bottomTitles: AxisTitles(
                                sideTitles: SideTitles(
                                  showTitles: true,
                                  getTitlesWidget: (value, meta) {
                                    if (value.toInt() >= 0 && value.toInt() < data.topMembers.length) {
                                      final name = data.topMembers[value.toInt()].name;
                                      return Padding(
                                        padding: const EdgeInsets.only(top: 8.0),
                                        child: Text(
                                          name.length > 5 ? '${name.substring(0, 5)}...' : name,
                                          style: const TextStyle(fontSize: 10, fontWeight: FontWeight.bold),
                                        ),
                                      );
                                    }
                                    return const SizedBox();
                                  },
                                ),
                              ),
                              leftTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                              topTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                              rightTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                            ),
                            borderData: FlBorderData(show: false),
                            gridData: const FlGridData(show: false),
                            barGroups: data.topMembers.asMap().entries.map((entry) {
                              return BarChartGroupData(
                                x: entry.key,
                                barRods: [
                                  BarChartRodData(
                                    toY: entry.value.creditsUsed.toDouble(),
                                    color: Theme.of(context).colorScheme.primary,
                                    width: 20,
                                    borderRadius: const BorderRadius.vertical(top: Radius.circular(6)),
                                    backDrawRodData: BackgroundBarChartRodData(
                                      show: true,
                                      toY: (data.topMembers.map((e) => e.creditsUsed).reduce((a, b) => a > b ? a : b) * 1.2).toDouble(),
                                      color: Theme.of(context).colorScheme.surfaceContainerHighest.withOpacity(0.4),
                                    ),
                                  ),
                                ],
                              );
                            }).toList(),
                          ),
                        ),
                      )
                    else
                      const SizedBox(
                        height: 120,
                        child: Center(child: Text('Chưa có dữ liệu')),
                      ),
                    const SizedBox(height: 20),
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Theme.of(context).colorScheme.primaryContainer.withOpacity(0.3),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Icon(Icons.tips_and_updates, color: Theme.of(context).colorScheme.primary, size: 20),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Text(
                              'Workspace đã dùng ${data.creditsUsed} credits tuần này. Tiếp tục phát huy nhé!',
                              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                    color: Theme.of(context).colorScheme.onSurface,
                                  ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),

              // Recent Activity List
              Text(
                'Hoạt động gần đây',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 16),
              recentActivitiesState.when(
                loading: () => const Center(child: CircularProgressIndicator()),
                error: (err, stack) => Text('Error loading activities: $err'),
                data: (activities) {
                  if (activities.isEmpty) {
                    return Container(
                      decoration: BoxDecoration(
                        color: Theme.of(context).colorScheme.surface,
                        borderRadius: BorderRadius.circular(24),
                        border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.3)),
                      ),
                      padding: const EdgeInsets.all(32.0),
                      child: const Center(child: Text('Chưa có hoạt động nào.')),
                    );
                  }
                  return Container(
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.surface,
                      borderRadius: BorderRadius.circular(24),
                      border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.3)),
                      boxShadow: [
                        BoxShadow(
                          color: Colors.black.withOpacity(0.02),
                          blurRadius: 16,
                          offset: const Offset(0, 4),
                        ),
                      ],
                    ),
                    child: ListView.separated(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: activities.length,
                      separatorBuilder: (context, index) => Divider(
                        height: 1,
                        color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.2),
                        indent: 72,
                      ),
                      itemBuilder: (context, index) {
                        final item = activities[index];
                        return InkWell(
                          onTap: () {
                            context.push('/content/${item.id}');
                          },
                          borderRadius: BorderRadius.circular(24),
                          child: Padding(
                            padding: const EdgeInsets.all(16.0),
                            child: Row(
                              children: [
                                _buildActivityIcon(item.status, item.adType, context),
                                const SizedBox(width: 16),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        item.title ?? 'Untitled',
                                        style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold),
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                      const SizedBox(height: 4),
                                      Text(
                                        _getActivityDescription(item.status),
                                        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                              color: Theme.of(context).colorScheme.onSurfaceVariant,
                                            ),
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                    ],
                                  ),
                                ),
                                const SizedBox(width: 8),
                                Text(
                                  _getTimeAgo(item.createdAt.toLocal()),
                                  style: Theme.of(context).textTheme.labelSmall?.copyWith(
                                        color: Theme.of(context).colorScheme.onSurfaceVariant,
                                      ),
                                ),
                              ],
                            ),
                          ),
                        );
                      },
                    ),
                  );
                },
              ),
              const SizedBox(height: 32),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildBentoStatCard(
    BuildContext context, {
    required String value,
    required String label,
    required IconData icon,
    required Color iconBgColor,
    required Color iconColor,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: Theme.of(context).colorScheme.outlineVariant.withOpacity(0.3)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 16,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              color: iconBgColor,
              shape: BoxShape.circle,
            ),
            child: Icon(icon, color: iconColor, size: 24),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                value,
                style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: Theme.of(context).colorScheme.onSurface,
                    ),
              ),
              Text(
                label,
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: Theme.of(context).colorScheme.onSurfaceVariant,
                    ),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildActivityIcon(ContentStatusEnum status, AdTypeEnum adType, BuildContext context) {
    Color bgColor;
    Color iconColor;
    IconData icon;

    switch (status) {
      case ContentStatusEnum.published:
      case ContentStatusEnum.approved:
        bgColor = Colors.green.shade100;
        iconColor = Colors.green.shade700;
        icon = Icons.check_circle;
        break;
      case ContentStatusEnum.pendingApproval:
        bgColor = Colors.orange.shade100;
        iconColor = Colors.orange.shade800;
        icon = Icons.schedule;
        break;
      case ContentStatusEnum.rejected:
        bgColor = Colors.red.shade100;
        iconColor = Colors.red.shade600;
        icon = Icons.cancel;
        break;
      default:
        bgColor = Theme.of(context).colorScheme.surfaceContainerHighest;
        iconColor = Theme.of(context).colorScheme.onSurfaceVariant;
        icon = adType == AdTypeEnum.videoText ? Icons.videocam : Icons.image;
    }

    return Container(
      width: 44,
      height: 44,
      decoration: BoxDecoration(
        color: bgColor,
        shape: BoxShape.circle,
      ),
      child: Icon(icon, color: iconColor, size: 24),
    );
  }

  String _getActivityDescription(ContentStatusEnum status) {
    switch (status) {
      case ContentStatusEnum.published:
        return 'Đã đăng thành công';
      case ContentStatusEnum.approved:
        return 'Đã được duyệt';
      case ContentStatusEnum.pendingApproval:
        return 'Đang chờ duyệt từ Manager';
      case ContentStatusEnum.rejected:
        return 'Bị từ chối';
      case ContentStatusEnum.draft:
        return 'Bản nháp';
    }
  }

  String _getTimeAgo(DateTime date) {
    final diff = DateTime.now().difference(date);
    if (diff.inDays > 0) {
      return '${diff.inDays}d trước';
    } else if (diff.inHours > 0) {
      return '${diff.inHours}h trước';
    } else if (diff.inMinutes > 0) {
      return '${diff.inMinutes}m trước';
    } else {
      return 'Vừa xong';
    }
  }
}
