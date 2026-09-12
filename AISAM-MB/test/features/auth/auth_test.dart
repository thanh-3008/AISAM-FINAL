import 'package:flutter_test/flutter_test.dart';
import 'package:aisam_mb/features/auth/data/models/auth_response.dart';

void main() {
  group('Auth Module Tests', () {
    test('AuthResponseModel correctly deserializes valid JSON payload', () {
      final json = {
        'accessToken': 'mock-access-token',
        'refreshToken': 'mock-refresh-token',
        'expiresAt': '2026-09-12T12:00:00.000Z',
        'tokenType': 'Bearer',
        'user': {
          'id': 'user-123',
          'email': 'test@aisam.io.vn',
          'fullName': 'Test User',
          'role': 1,
          'isEmailVerified': true,
          'createdAt': '2026-09-12T00:00:00.000Z',
        },
      };

      final response = AuthResponseModel.fromJson(json);

      expect(response.accessToken, 'mock-access-token');
      expect(response.refreshToken, 'mock-refresh-token');
      expect(response.tokenType, 'Bearer');
      expect(response.user.id, 'user-123');
      expect(response.user.email, 'test@aisam.io.vn');
      expect(response.user.fullName, 'Test User');
      expect(response.user.role, 1);
      expect(response.user.isEmailVerified, true);
    });
  });
}
