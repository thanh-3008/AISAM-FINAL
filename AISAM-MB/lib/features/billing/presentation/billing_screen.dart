import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/shared/app_loading_indicator.dart';
import 'providers/billing_controller.dart';

class BillingScreen extends ConsumerWidget {
  const BillingScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final billingState = ref.watch(billingControllerProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Billing & Credit'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () => ref.read(billingControllerProvider.notifier).refreshQuota(),
          )
        ],
      ),
      body: billingState.when(
        data: (quota) => Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Gói đăng ký hiện tại', style: Theme.of(context).textTheme.titleMedium),
                      const SizedBox(height: 8),
                      Text(
                        quota.planName.isNotEmpty ? quota.planName : 'Không xác định',
                        style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                              fontWeight: FontWeight.bold,
                              color: Theme.of(context).colorScheme.primary,
                            ),
                      ),
                      if (quota.subscriptionStatus.isNotEmpty) ...[
                        const SizedBox(height: 4),
                        Text(
                          'Trạng thái: ${quota.subscriptionStatus}',
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: quota.subscriptionStatus.toLowerCase() == 'active' ? Colors.green : Colors.red,
                              ),
                        ),
                      ],
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Số Token còn lại', style: Theme.of(context).textTheme.titleMedium),
                      const SizedBox(height: 8),
                      Text(
                        '${quota.promptRemaining} Tokens',
                        style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold, color: Colors.orange),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Đã dùng: ${quota.promptUsage} / ${quota.promptQuotaLimit}',
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
        loading: () => const Center(child: AppLoadingIndicator()),
        error: (error, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Error: $error', textAlign: TextAlign.center),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () => ref.read(billingControllerProvider.notifier).refreshQuota(),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
