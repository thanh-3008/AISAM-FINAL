import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/quota_model.dart';

part 'billing_repository.g.dart';

class BillingRepository {
  final Dio _dio;

  BillingRepository(this._dio);

  Future<QuotaModel> getCurrentQuota() async {
    try {
      final response = await _dio.get('/quota/workspace/current');
      final data = response.data['data'];
      return QuotaModel.fromJson(data);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
BillingRepository billingRepository(BillingRepositoryRef ref) {
  final dio = ref.watch(dioProvider);
  return BillingRepository(dio);
}
