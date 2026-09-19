import 'package:flutter_test/flutter_test.dart';
import 'package:dio/dio.dart';
import 'package:aisam_mb/core/errors/app_exception.dart';
import 'package:aisam_mb/features/billing/data/models/quota_model.dart';
import 'package:aisam_mb/features/billing/data/repositories/billing_repository.dart';

void main() {
  group('Billing & Quota Tests', () {
    test('QuotaModel correctly parses backend quota JSON with prompt and post quotas', () {
      final json = {
        'planName': 'Plus',
        'subscriptionStatus': 'Active',
        'windowStart': '2026-09-01T00:00:00Z',
        'windowEnd': '2026-09-30T23:59:59Z',
        'promptQuotaLimit': 100,
        'promptUsage': 25,
        'promptRemaining': 75,
        'postQuotaLimit': 50,
        'postUsage': 10,
        'postRemaining': 40,
        'textContentCount': 5,
        'imageContentCount': 3,
        'videoContentCount': 2,
      };

      final quota = QuotaModel.fromJson(json);

      expect(quota.planName, 'Plus');
      expect(quota.subscriptionStatus, 'Active');
      expect(quota.promptQuotaLimit, 100);
      expect(quota.promptUsage, 25);
      expect(quota.promptRemaining, 75);
      expect(quota.postQuotaLimit, 50);
      expect(quota.postUsage, 10);
      expect(quota.postRemaining, 40);
    });

    test('QuotaModel handles nulls gracefully and sets default remaining to zero', () {
      final json = <String, dynamic>{};
      final quota = QuotaModel.fromJson(json);

      expect(quota.planName, '');
      expect(quota.subscriptionStatus, '');
      expect(quota.promptQuotaLimit, 0);
      expect(quota.promptUsage, 0);
      expect(quota.promptRemaining, 0);
      expect(quota.creditBalance, 0);
      expect(quota.creditsUsed, 0);
      expect(quota.maxBalanceCap, 0);
    });

    test('QuotaModel correctly parses account credit wallet tokens and member quota', () {
      final json = {
        'planName': 'Enterprise',
        'subscriptionStatus': 'Active',
        'promptQuotaLimit': 100,
        'promptUsage': 10,
        'promptRemaining': 90,
        'creditBalance': 15000,
        'creditsUsed': 2500,
        'maxBalanceCap': 20000,
        'memberCreditLimit': 5000,
        'memberCreditUsed': 1200,
        'memberQuotaMode': 2,
      };

      final quota = QuotaModel.fromJson(json);

      expect(quota.creditBalance, 15000);
      expect(quota.creditsUsed, 2500);
      expect(quota.maxBalanceCap, 20000);
      expect(quota.memberCreditLimit, 5000);
      expect(quota.memberCreditUsed, 1200);
      expect(quota.memberQuotaMode, 2);
    });
  });

  group('BillingRepository Contract & Error Handling Tests (SYNC-005)', () {
    test('getCurrentQuota aggregates primary and supplementary endpoints successfully', () async {
      final mockDio = MockBillingDio(
        responses: {
          '/quota/workspace/current': {
            'data': {
              'planName': 'Pro',
              'subscriptionStatus': 'Active',
              'promptQuotaLimit': 200,
              'promptUsage': 50,
            },
          },
          '/credit-usage/wallet': {
            'data': {'balance': 5000, 'workspaceId': 'ws-123'},
          },
          '/workspace-dashboard/summary': {
            'data': {'creditsUsed': 1500, 'maxBalanceCap': 10000},
          },
          '/workspace-members': {
            'data': [],
          },
        },
      );

      final repo = BillingRepository(mockDio);
      final quota = await repo.getCurrentQuota();

      expect(quota.planName, 'Pro');
      expect(quota.promptQuotaLimit, 200);
      expect(quota.creditBalance, 5000);
      expect(quota.creditsUsed, 1500);
      expect(quota.maxBalanceCap, 10000);
    });

    test('getCurrentQuota fails loudly when primary /quota/workspace/current returns 500', () async {
      final mockDio = MockBillingDio(
        errors: {
          '/quota/workspace/current': DioException(
            requestOptions: RequestOptions(path: '/quota/workspace/current'),
            response: Response(
              statusCode: 500,
              requestOptions: RequestOptions(path: '/quota/workspace/current'),
              data: {'message': 'Internal database error'},
            ),
            type: DioExceptionType.badResponse,
          ),
        },
      );

      final repo = BillingRepository(mockDio);
      expect(
        () async => await repo.getCurrentQuota(),
        throwsA(isA<ServerException>()),
      );
    });

    test('getCurrentQuota rethrows UnauthorizedException when supplementary endpoint returns 401', () async {
      final mockDio = MockBillingDio(
        responses: {
          '/quota/workspace/current': {
            'data': {'planName': 'Free'},
          },
        },
        errors: {
          '/credit-usage/wallet': DioException(
            requestOptions: RequestOptions(path: '/credit-usage/wallet'),
            response: Response(
              statusCode: 401,
              requestOptions: RequestOptions(path: '/credit-usage/wallet'),
            ),
            type: DioExceptionType.badResponse,
          ),
        },
      );

      final repo = BillingRepository(mockDio);
      expect(
        () async => await repo.getCurrentQuota(),
        throwsA(isA<UnauthorizedException>()),
      );
    });

    test('getCurrentQuota tolerates non-critical 404 on supplementary endpoints without failing primary', () async {
      final mockDio = MockBillingDio(
        responses: {
          '/quota/workspace/current': {
            'data': {
              'planName': 'Enterprise',
              'promptQuotaLimit': 1000,
            },
          },
        },
        errors: {
          '/credit-usage/wallet': DioException(
            requestOptions: RequestOptions(path: '/credit-usage/wallet'),
            response: Response(
              statusCode: 404,
              requestOptions: RequestOptions(path: '/credit-usage/wallet'),
            ),
            type: DioExceptionType.badResponse,
          ),
          '/workspace-dashboard/summary': DioException(
            requestOptions: RequestOptions(path: '/workspace-dashboard/summary'),
            response: Response(
              statusCode: 404,
              requestOptions: RequestOptions(path: '/workspace-dashboard/summary'),
            ),
            type: DioExceptionType.badResponse,
          ),
          '/workspace-members': DioException(
            requestOptions: RequestOptions(path: '/workspace-members'),
            response: Response(
              statusCode: 404,
              requestOptions: RequestOptions(path: '/workspace-members'),
            ),
            type: DioExceptionType.badResponse,
          ),
        },
      );

      final repo = BillingRepository(mockDio);
      final quota = await repo.getCurrentQuota();

      expect(quota.planName, 'Enterprise');
      expect(quota.promptQuotaLimit, 1000);
      expect(quota.creditBalance, 0);
    });
  });
}

class MockBillingDio extends Fake implements Dio {
  final Map<String, dynamic> responses;
  final Map<String, DioException> errors;

  MockBillingDio({this.responses = const {}, this.errors = const {}});

  @override
  Future<Response<T>> get<T>(
    String path, {
    Object? data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
    ProgressCallback? onReceiveProgress,
  }) async {
    if (errors.containsKey(path)) {
      throw errors[path]!;
    }
    if (responses.containsKey(path)) {
      return Response<T>(
        data: responses[path] as T,
        statusCode: 200,
        requestOptions: RequestOptions(path: path),
      );
    }
    return Response<T>(
      data: {'success': true, 'data': <String, dynamic>{}} as T,
      statusCode: 200,
      requestOptions: RequestOptions(path: path),
    );
  }
}
