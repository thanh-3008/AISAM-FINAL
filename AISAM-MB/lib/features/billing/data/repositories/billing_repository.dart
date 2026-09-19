import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/storage/secure_storage.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/quota_model.dart';

part 'billing_repository.g.dart';

class BillingRepository {
  final Dio _dio;
  final SecureStorage? _storage;

  BillingRepository(this._dio, [this._storage]);

  Future<Response<dynamic>?> _safeGetSupplementary(String path) async {
    try {
      return await _dio.get(path);
    } on DioException catch (e) {
      // Re-throw authentication/authorization errors so they are not swallowed
      if (e.response?.statusCode == 401 || e.response?.statusCode == 403) {
        rethrow;
      }
      return null;
    } catch (_) {
      return null;
    }
  }

  Future<QuotaModel> getCurrentQuota() async {
    try {
      // 1. Primary Subscription & Quota: must fail loudly on network / auth / server error
      final quotaFuture = _dio.get('/quota/workspace/current');

      // 2. Supplementary Futures: rethrow auth/permission errors, return null on missing endpoints
      final walletFuture = _safeGetSupplementary('/credit-usage/wallet');
      final summaryFuture = _safeGetSupplementary('/workspace-dashboard/summary');
      final membersFuture = _safeGetSupplementary('/workspace-members');

      final results = await Future.wait([
        quotaFuture,
        walletFuture,
        summaryFuture,
        membersFuture,
      ]);

      final quotaResp = results[0] as Response<dynamic>;
      final walletResp = results[1] as Response<dynamic>?;
      final summaryResp = results[2] as Response<dynamic>?;
      final membersResp = results[3] as Response<dynamic>?;

      final quotaMap = (quotaResp.data?['data'] as Map<String, dynamic>?) ?? <String, dynamic>{};
      final walletMap = walletResp?.data?['data'] as Map<String, dynamic>?;
      final summaryMap = summaryResp?.data?['data'] as Map<String, dynamic>?;
      final membersList = membersResp?.data?['data'] as List?;

      // Determine real token/credit balance
      int creditBalance = 0;
      if (walletMap != null && walletMap['balance'] != null) {
        creditBalance = (walletMap['balance'] as num).toInt();
      } else if (summaryMap != null && summaryMap['creditBalance'] != null) {
        creditBalance = (summaryMap['creditBalance'] as num).toInt();
      }

      int creditsUsed = 0;
      if (summaryMap != null && summaryMap['creditsUsed'] != null) {
        creditsUsed = (summaryMap['creditsUsed'] as num).toInt();
      }

      int maxBalanceCap = 0;
      if (summaryMap != null && summaryMap['maxBalanceCap'] != null) {
        maxBalanceCap = (summaryMap['maxBalanceCap'] as num).toInt();
      }

      // Check current user's membership quota
      int? memberCreditLimit;
      int memberCreditUsed = 0;
      int? memberQuotaMode;

      final currentUserId = _storage?.cachedUserId;
      if (membersList != null && currentUserId != null) {
        for (final m in membersList) {
          if (m is Map<String, dynamic> && m['userId'] == currentUserId) {
            memberQuotaMode = (m['quotaMode'] as num?)?.toInt();
            memberCreditLimit = (m['creditLimit'] as num?)?.toInt();
            memberCreditUsed = (m['creditUsed'] as num?)?.toInt() ?? 0;
            break;
          }
        }
      }

      // Merge into complete QuotaModel
      final baseQuota = QuotaModel.fromJson(quotaMap);
      return baseQuota.copyWith(
        creditBalance: creditBalance,
        creditsUsed: creditsUsed,
        maxBalanceCap: maxBalanceCap,
        memberCreditLimit: memberCreditLimit,
        memberCreditUsed: memberCreditUsed,
        memberQuotaMode: memberQuotaMode,
      );
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
BillingRepository billingRepository(BillingRepositoryRef ref) {
  final dio = ref.watch(dioProvider);
  final storage = ref.watch(secureStorageProvider);
  return BillingRepository(dio, storage);
}
