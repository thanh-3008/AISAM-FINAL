import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/features/billing/data/models/quota_model.dart';

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
}
