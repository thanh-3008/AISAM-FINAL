import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/shared/app_loading_indicator.dart';
import '../../settings/presentation/providers/language_provider.dart';
import 'providers/billing_controller.dart';

class BillingScreen extends ConsumerWidget {
  const BillingScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final billingState = ref.watch(billingControllerProvider);
    final langState = ref.watch(languageControllerProvider);
    final isEn = (langState.value ?? 'vi') == 'en';

    return Scaffold(
      appBar: AppBar(
        title: Text(isEn ? 'Billing & Credit' : 'Thanh toán & Token'),
        actions: [
          IconButton(
            tooltip: isEn ? 'Refresh' : 'Làm mới',
            icon: const Icon(Icons.refresh),
            onPressed: () => ref.read(billingControllerProvider.notifier).refreshQuota(),
          )
        ],
      ),
      body: billingState.when(
        data: (quota) {
          final isActive = quota.subscriptionStatus.toLowerCase() == 'active';
          final statusLabel = isActive 
              ? (isEn ? 'Active' : 'Đang hoạt động') 
              : (isEn ? 'Inactive' : 'Không hoạt động');

          return RefreshIndicator(
            onRefresh: () => ref.read(billingControllerProvider.notifier).refreshQuota(),
            child: ListView(
              padding: const EdgeInsets.all(16.0),
              children: [
                // 1. Current Subscription Plan Card
                Card(
                  elevation: 2,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  child: Padding(
                    padding: const EdgeInsets.all(20.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Expanded(
                              child: Text(
                                isEn ? 'Current Plan' : 'Gói đăng ký hiện tại',
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                      fontWeight: FontWeight.w600,
                                    ),
                              ),
                            ),
                            const SizedBox(width: 8),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                              decoration: BoxDecoration(
                                color: isActive ? Colors.green.withOpacity(0.12) : Colors.red.withOpacity(0.12),
                                borderRadius: BorderRadius.circular(9999),
                              ),
                              child: Text(
                                statusLabel,
                                style: TextStyle(
                                  fontSize: 12,
                                  fontWeight: FontWeight.bold,
                                  color: isActive ? Colors.green.shade700 : Colors.red.shade700,
                                ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),
                        Text(
                          quota.planName.isNotEmpty ? quota.planName : (isEn ? 'Free Plan' : 'Gói Miễn phí'),
                          style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                                fontWeight: FontWeight.bold,
                                color: Theme.of(context).colorScheme.primary,
                              ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // 2. Real Account Token & Credit Wallet Card
                Card(
                  elevation: 2,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  child: Container(
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(16),
                      gradient: LinearGradient(
                        colors: [
                          const Color(0xFF10B981).withOpacity(0.06),
                          Colors.white,
                        ],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                    ),
                    padding: const EdgeInsets.all(20.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          crossAxisAlignment: CrossAxisAlignment.center,
                          children: [
                            Expanded(
                              child: Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Container(
                                    padding: const EdgeInsets.all(8),
                                    decoration: BoxDecoration(
                                      color: const Color(0xFF10B981).withOpacity(0.12),
                                      borderRadius: BorderRadius.circular(10),
                                    ),
                                    child: const Icon(Icons.token, color: Color(0xFF059669), size: 22),
                                  ),
                                  const SizedBox(width: 10),
                                  Expanded(
                                    child: Text(
                                      isEn ? 'Account Tokens' : 'Token & Credit tài khoản',
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                            fontWeight: FontWeight.w700,
                                            color: const Color(0xFF0F172A),
                                          ),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            const SizedBox(width: 8),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                              decoration: BoxDecoration(
                                color: const Color(0xFF10B981).withOpacity(0.12),
                                borderRadius: BorderRadius.circular(9999),
                              ),
                              child: Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  const Icon(Icons.check_circle, size: 12, color: Color(0xFF059669)),
                                  const SizedBox(width: 4),
                                  Text(
                                    isEn ? 'Synced' : 'Đã đồng bộ',
                                    style: const TextStyle(
                                      fontSize: 11,
                                      fontWeight: FontWeight.w700,
                                      color: Color(0xFF059669),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 14),
                        Wrap(
                          crossAxisAlignment: WrapCrossAlignment.center,
                          children: [
                            Text(
                              _formatNumber(quota.creditBalance),
                              style: const TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                fontSize: 30,
                                fontWeight: FontWeight.w800,
                                color: Color(0xFF059669),
                              ),
                            ),
                            const SizedBox(width: 8),
                            Text(
                              isEn ? 'Credits available' : 'Credit khả dụng',
                              style: const TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                fontSize: 13,
                                fontWeight: FontWeight.w600,
                                color: Color(0xFF64748B),
                              ),
                            ),
                          ],
                        ),
                        if (quota.creditsUsed > 0 || quota.maxBalanceCap > 0) ...[
                          const SizedBox(height: 8),
                          Text(
                            isEn
                                ? 'Used: ${_formatNumber(quota.creditsUsed)} / ${_formatNumber(quota.maxBalanceCap > 0 ? quota.maxBalanceCap : quota.creditBalance + quota.creditsUsed)} Credits'
                                : 'Đã dùng: ${_formatNumber(quota.creditsUsed)} / ${_formatNumber(quota.maxBalanceCap > 0 ? quota.maxBalanceCap : quota.creditBalance + quota.creditsUsed)} Credit',
                            style: TextStyle(
                              fontSize: 13,
                              color: Colors.grey.shade600,
                            ),
                          ),
                          const SizedBox(height: 8),
                          LinearProgressIndicator(
                            value: (quota.maxBalanceCap > 0)
                                ? (quota.creditsUsed / quota.maxBalanceCap).clamp(0.0, 1.0)
                                : ((quota.creditBalance + quota.creditsUsed) > 0
                                    ? (quota.creditsUsed / (quota.creditBalance + quota.creditsUsed)).clamp(0.0, 1.0)
                                    : 0.0),
                            backgroundColor: const Color(0xFF10B981).withOpacity(0.15),
                            valueColor: const AlwaysStoppedAnimation<Color>(Color(0xFF059669)),
                          ),
                        ],
                        if (quota.memberCreditLimit != null && quota.memberCreditLimit! > 0) ...[
                          const SizedBox(height: 12),
                          Container(
                            padding: const EdgeInsets.all(10),
                            decoration: BoxDecoration(
                              color: const Color(0xFF003EC7).withOpacity(0.06),
                              borderRadius: BorderRadius.circular(10),
                              border: Border.all(color: const Color(0xFF003EC7).withOpacity(0.15)),
                            ),
                            child: Row(
                              children: [
                                const Icon(Icons.person_outline, size: 18, color: Color(0xFF003EC7)),
                                const SizedBox(width: 8),
                                Expanded(
                                  child: Text(
                                    isEn
                                        ? 'Your Personal Quota: ${_formatNumber((quota.memberCreditLimit! - quota.memberCreditUsed).clamp(0, quota.memberCreditLimit!))} / ${_formatNumber(quota.memberCreditLimit!)} Credits'
                                        : 'Hạn mức cá nhân: ${_formatNumber((quota.memberCreditLimit! - quota.memberCreditUsed).clamp(0, quota.memberCreditLimit!))} / ${_formatNumber(quota.memberCreditLimit!)} Credit',
                                    style: const TextStyle(
                                      fontSize: 12,
                                      fontWeight: FontWeight.w600,
                                      color: Color(0xFF003EC7),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // 3. Daily AI Content / Prompt Quota Card
                Card(
                  elevation: 2,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  child: Padding(
                    padding: const EdgeInsets.all(20.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Container(
                              padding: const EdgeInsets.all(8),
                              decoration: BoxDecoration(
                                color: Colors.orange.withOpacity(0.12),
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: Icon(Icons.auto_awesome, color: Colors.orange.shade800, size: 20),
                            ),
                            const SizedBox(width: 10),
                            Expanded(
                              child: Text(
                                isEn ? 'Daily AI Prompt Limit' : 'Hạn mức tạo nội dung AI (Ngày)',
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                      fontWeight: FontWeight.w600,
                                    ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),
                        Text(
                          '${quota.promptRemaining} ${isEn ? 'Prompts remaining' : 'lượt còn lại'}',
                          style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                                fontWeight: FontWeight.bold,
                                color: Colors.orange.shade800,
                              ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          isEn
                              ? 'Used: ${quota.promptUsage} / ${quota.promptQuotaLimit} (daily free allowance)'
                              : 'Đã dùng: ${quota.promptUsage} / ${quota.promptQuotaLimit} (hạn mức miễn phí hàng ngày)',
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: Theme.of(context).colorScheme.onSurfaceVariant,
                              ),
                        ),
                        if (quota.promptQuotaLimit > 0) ...[
                          const SizedBox(height: 10),
                          LinearProgressIndicator(
                            value: (quota.promptUsage / quota.promptQuotaLimit).clamp(0.0, 1.0),
                            backgroundColor: Colors.orange.shade100,
                            valueColor: AlwaysStoppedAnimation<Color>(
                              (quota.promptUsage >= quota.promptQuotaLimit) ? Colors.red : Colors.orange,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // 3. Post Publishing Quota Card
                Card(
                  elevation: 2,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  child: Padding(
                    padding: const EdgeInsets.all(20.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Icon(Icons.post_add, color: Theme.of(context).colorScheme.primary, size: 22),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                isEn ? 'Post Quota (Monthly)' : 'Hạn mức đăng bài (Tháng)',
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                      fontWeight: FontWeight.w600,
                                    ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),
                        Text(
                          '${quota.postRemaining} ${isEn ? 'Posts' : 'Bài'}',
                          style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                                fontWeight: FontWeight.bold,
                                color: Theme.of(context).colorScheme.primary,
                              ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          isEn
                              ? 'Used: ${quota.postUsage} / ${quota.postQuotaLimit}'
                              : 'Đã dùng: ${quota.postUsage} / ${quota.postQuotaLimit}',
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: Theme.of(context).colorScheme.onSurfaceVariant,
                              ),
                        ),
                        if (quota.postQuotaLimit > 0) ...[
                          const SizedBox(height: 10),
                          LinearProgressIndicator(
                            value: (quota.postUsage / quota.postQuotaLimit).clamp(0.0, 1.0),
                            backgroundColor: Theme.of(context).colorScheme.primary.withOpacity(0.12),
                            valueColor: AlwaysStoppedAnimation<Color>(
                              (quota.postUsage >= quota.postQuotaLimit) ? Colors.red : Theme.of(context).colorScheme.primary,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
              ],
            ),
          );
        },
        loading: () => const Center(child: AppLoadingIndicator()),
        error: (error, stack) => Center(
          child: Padding(
            padding: const EdgeInsets.all(24.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.error_outline, size: 48, color: Theme.of(context).colorScheme.error),
                const SizedBox(height: 16),
                Text(
                  isEn ? 'Failed to load billing information' : 'Không thể tải thông tin thanh toán & token',
                  style: Theme.of(context).textTheme.titleMedium,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 8),
                Text('$error', textAlign: TextAlign.center, style: TextStyle(color: Theme.of(context).colorScheme.error)),
                const SizedBox(height: 24),
                ElevatedButton.icon(
                  icon: const Icon(Icons.refresh),
                  onPressed: () => ref.read(billingControllerProvider.notifier).refreshQuota(),
                  label: Text(isEn ? 'Retry' : 'Thử lại'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  String _formatNumber(int val) {
    return val.toString().replaceAllMapped(
      RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
      (Match m) => '${m[1]},',
    );
  }
}
