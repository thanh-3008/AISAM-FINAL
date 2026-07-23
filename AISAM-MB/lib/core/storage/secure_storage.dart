import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

part 'secure_storage.g.dart';

class SecureStorage {
  final FlutterSecureStorage _storage;

  SecureStorage(this._storage);

  static const String _accessTokenKey = 'accessToken';
  static const String _refreshTokenKey = 'refreshToken';
  static const String _activeWorkspaceIdKey = 'activeWorkspaceId';
  static const String _activeProfileIdKey = 'activeProfileId';
  static const String _userIdKey = 'userId';

  Future<void> saveTokens({required String accessToken, required String refreshToken}) async {
    await _storage.write(key: _accessTokenKey, value: accessToken);
    await _storage.write(key: _refreshTokenKey, value: refreshToken);
  }

  Future<void> saveActiveWorkspaceId(String workspaceId) async {
    await _storage.write(key: _activeWorkspaceIdKey, value: workspaceId);
  }

  Future<void> saveActiveProfileId(String profileId) async {
    await _storage.write(key: _activeProfileIdKey, value: profileId);
  }

  Future<void> saveUserId(String userId) async {
    await _storage.write(key: _userIdKey, value: userId);
  }

  Future<String?> getAccessToken() async => await _storage.read(key: _accessTokenKey);
  Future<String?> getRefreshToken() async => await _storage.read(key: _refreshTokenKey);
  Future<String?> getActiveWorkspaceId() async => await _storage.read(key: _activeWorkspaceIdKey);
  Future<String?> getActiveProfileId() async => await _storage.read(key: _activeProfileIdKey);
  Future<String?> getUserId() async {
    final id = await _storage.read(key: _userIdKey);
    if (id != null) return id;

    // Fallback for users already logged in
    final token = await getAccessToken();
    if (token != null) {
      try {
        final parts = token.split('.');
        if (parts.length == 3) {
          String payload = parts[1];
          while (payload.length % 4 != 0) {
            payload += '=';
          }
          final decoded = utf8.decode(base64Url.decode(payload));
          final json = jsonDecode(decoded);
          final extractedId = json['nameid'] as String?;
          if (extractedId != null) {
            await saveUserId(extractedId);
            return extractedId;
          }
        }
      } catch (_) {}
    }
    return null;
  }

  Future<void> clearAll() async {
    await _storage.deleteAll();
  }
}

@riverpod
SecureStorage secureStorage(SecureStorageRef ref) {
  return SecureStorage(const FlutterSecureStorage());
}
