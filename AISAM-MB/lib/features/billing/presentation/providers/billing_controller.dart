import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/billing_repository.dart';
import '../../data/models/quota_model.dart';
import '../../../../core/errors/app_exception.dart';

part 'billing_controller.g.dart';

@riverpod
class BillingController extends _$BillingController {
  @override
  AsyncValue<QuotaModel> build() {
    _fetchQuota();
    return const AsyncValue.loading();
  }

  Future<void> _fetchQuota() async {
    try {
      state = const AsyncValue.loading();
      final repository = ref.read(billingRepositoryProvider);
      final quota = await repository.getCurrentQuota();
      state = AsyncValue.data(quota);
    } catch (e, st) {
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refreshQuota() async {
    await _fetchQuota();
  }
}
