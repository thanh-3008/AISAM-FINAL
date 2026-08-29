import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../services/logger_service.dart';

part 'secure_storage.g.dart';

class SecureStorage {
  final FlutterSecureStorage _storage;

  String? _cachedAccessToken;
  String? _cachedRefreshToken;
  String? _cachedWorkspaceId;
  String? _cachedProfileId;
  String? _cachedUserId;
  bool _isInitialized = false;

  SecureStorage(this._storage);

  static const String _accessTokenKey = 'accessToken';
  static const String _refreshTokenKey = 'refreshToken';
  static const String _activeWorkspaceIdKey = 'activeWorkspaceId';
  static const String _activeProfileIdKey = 'activeProfileId';
  static const String _userIdKey = 'userId';

  Future<void> initCache() async {
    if (_isInitialized) return;
    _cachedAccessToken = await _storage.read(key: _accessTokenKey);
    _cachedRefreshToken = await _storage.read(key: _refreshTokenKey);
    _cachedWorkspaceId = await _storage.read(key: _activeWorkspaceIdKey);
    _cachedProfileId = await _storage.read(key: _activeProfileIdKey);
    _cachedUserId = await _storage.read(key: _userIdKey);
    _isInitialized = true;
  }

  Future<void> saveTokens({required String accessToken, required String refreshToken}) async {
    _cachedAccessToken = accessToken;
    _cachedRefreshToken = refreshToken;
    await _storage.write(key: _accessTokenKey, value: accessToken);
    await _storage.write(key: _refreshTokenKey, value: refreshToken);
  }

  Future<void> saveActiveWorkspaceId(String workspaceId) async {
    _cachedWorkspaceId = workspaceId;
    await _storage.write(key: _activeWorkspaceIdKey, value: workspaceId);
  }

  Future<void> saveActiveProfileId(String profileId) async {
    _cachedProfileId = profileId;
    await _storage.write(key: _activeProfileIdKey, value: profileId);
  }

  Future<void> saveUserId(String userId) async {
    _cachedUserId = userId;
    await _storage.write(key: _userIdKey, value: userId);
  }

  Future<String?> getAccessToken() async {
    await initCache();
    return _cachedAccessToken;
  }
  
  Future<String?> getRefreshToken() async {
    await initCache();
    return _cachedRefreshToken;
  }
  
  Future<String?> getActiveWorkspaceId() async {
    await initCache();
    return _cachedWorkspaceId;
  }
  
  Future<String?> getActiveProfileId() async {
    await initCache();
    return _cachedProfileId;
  }

  // Fast synchronous getters for Interceptor and Router
  String? get cachedAccessToken => _cachedAccessToken;
  String? get cachedRefreshToken => _cachedRefreshToken;
  String? get cachedWorkspaceId => _cachedWorkspaceId;
  String? get cachedProfileId => _cachedProfileId;
  String? get cachedUserId => _cachedUserId;

  Future<String?> getUserId() async {
    await initCache();
    if (_cachedUserId != null) return _cachedUserId;

    // Fallback for users already logged in
    final token = _cachedAccessToken;
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
      } catch (e, st) {
        LoggerService.e('JWT Decode Error: $e', st);
      }
    }
    return null;
  }

  Future<void> clearAll() async {
    _cachedAccessToken = null;
    _cachedRefreshToken = null;
    _cachedWorkspaceId = null;
    _cachedProfileId = null;
    _cachedUserId = null;
    await _storage.deleteAll();
  }
}

@Riverpod(keepAlive: true)
SecureStorage secureStorage(SecureStorageRef ref) {
  return SecureStorage(const FlutterSecureStorage());
}
