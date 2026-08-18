import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/profile_model.dart';
import '../models/profile_request.dart';
import '../../../../core/storage/secure_storage.dart';
part 'profile_repository.g.dart';

List<ProfileResponseModel> _parseProfileList(List<dynamic> items) {
  return items.map((e) => ProfileResponseModel.fromJson(e)).toList();
}

class ProfileRepository {
  final Dio _dio;
  final SecureStorage _storage;

  ProfileRepository(this._dio, this._storage);

  Future<List<ProfileResponseModel>> getProfiles() async {
    try {
      final userId = await _storage.getUserId();
      if (userId == null) throw Exception('User ID not found in secure storage');
      final response = await _dio.get('/Profiles/user/$userId');
      final data = response.data['data'] as List?;
      if (data == null || data.isEmpty) return [];
      return await compute(_parseProfileList, data);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ProfileResponseModel> getProfileById(String id) async {
    try {
      final response = await _dio.get('/Profiles/$id');
      return ProfileResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ProfileResponseModel> createProfile(CreateProfileRequest request) async {
    try {
      final formData = FormData.fromMap({
        'Name': request.name,
        'ProfileType': request.profileType,
        if (request.companyName != null) 'CompanyName': request.companyName,
        if (request.bio != null) 'Bio': request.bio,
      });

      final userId = await _storage.getUserId();
      if (userId == null) throw Exception('User ID not found in secure storage');
      final response = await _dio.post('/Profiles/user/$userId', data: formData);
      return ProfileResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ProfileResponseModel> updateProfile(String id, UpdateProfileRequest request, {String? avatarFilePath}) async {
    try {
      final formData = FormData.fromMap({
        if (request.name != null) 'Name': request.name,
        if (request.profileType != null) 'ProfileType': request.profileType,
        if (request.companyName != null) 'CompanyName': request.companyName,
        if (request.bio != null) 'Bio': request.bio,
      });

      if (avatarFilePath != null) {
        formData.files.add(MapEntry(
          'AvatarFile',
          await MultipartFile.fromFile(avatarFilePath),
        ));
      }

      final response = await _dio.put('/Profiles/$id', data: formData);
      return ProfileResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> deleteProfile(String id) async {
    try {
      await _dio.delete('/Profiles/$id');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
ProfileRepository profileRepository(ProfileRepositoryRef ref) {
  return ProfileRepository(
    ref.read(dioProvider),
    ref.read(secureStorageProvider),
  );
}
