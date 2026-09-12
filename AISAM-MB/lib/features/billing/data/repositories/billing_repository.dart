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

  Future<QuotaModel> getCurrentQuota() async {
    try {
      // 1. Fetch Subscription & Quota
      final quotaFuture = _dio.get('/quota/workspace/current').catchError((_) => Response(
            requestOptions: RequestOptions(path: '/quota/workspace/current'),
            data: {'data': <String, dynamic>{}},
          ));

      // 2. Fetch Credit Wallet
      final walletFuture = _dio.get('/credit-usage/wallet').catchError((_) => Response(
            requestOptions: RequestOptions(path: '/credit-usage/wallet'),
            data: {'data': null},
          ));

      // 3. Fetch Workspace Dashboard Summary
      final summaryFuture = _dio.get('/workspace-dashboard/summary').catchError((_) => Response(
            requestOptions: RequestOptions(path: '/workspace-dashboard/summary'),
            data: {'data': null},
          ));

      // 4. Fetch Workspace Members (for personal member quota)
      final membersFuture = _dio.get('/workspace-members').catchError((_) => Response(
            requestOptions: RequestOptions(path: '/workspace-members'),
            data: {'data': null},
          ));

      final results = await Future.wait([quotaFuture, walletFuture, summaryFuture, membersFuture]);

      final quotaResp = results[0];
      final walletResp = results[1];
      final summaryResp = results[2];
      final membersResp = results[3];

      final quotaMap = (quotaResp.data?['data'] as Map<String, dynamic>?) ?? <String, dynamic>{};
      final walletMap = walletResp.data?['data'] as Map<String, dynamic>?;
      final summaryMap = summaryResp.data?['data'] as Map<String, dynamic>?;
      final membersList = membersResp.data?['data'] as List?;

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
