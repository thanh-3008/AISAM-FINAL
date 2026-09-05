import '../../../../core/network/access_events.dart';
import '../../../access/presentation/access_providers.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/billing_repository.dart';
import '../../data/models/quota_model.dart';
import '../../../../core/errors/app_exception.dart';

part 'billing_controller.g.dart';

@riverpod
class BillingController extends _$BillingController {
  int _generation = 0;

  @override
  AsyncValue<QuotaModel> build() {
    ++_generation;
    ref.onDispose(() => ++_generation);
    ref.watch(accessContextProvider);
    if (ref.watch(accessDeniedProvider)) {
      return AsyncValue.error(StateError('Access denied'), StackTrace.current);
    }
    _fetchQuota();
    return const AsyncValue.loading();
  }

  Future<void> _fetchQuota() async {
    final generation = ++_generation;
    try {
      await ref.read(accessContextProvider.future);
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      final repository = ref.read(billingRepositoryProvider);
      final quota = await repository.getCurrentQuota();
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.data(quota);
    } catch (e, st) {
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refreshQuota() async {
    await _fetchQuota();
  }
}
