import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/brand_model.dart';
import '../models/brand_request.dart';
part 'brand_repository.g.dart';

List<BrandResponseModel> _parseBrandList(List<dynamic> items) {
  return items.map((e) => BrandResponseModel.fromJson(e)).toList();
}

class BrandRepository {
  final Dio _dio;

  BrandRepository(this._dio);

  Future<List<BrandResponseModel>> getBrands() async {
    try {
      // Backend returns a PagedResult for Brands, so we handle it accordingly
      final response = await _dio.get('/Brands');
      final data = response.data['data'];
      final items = data != null ? data['data'] as List? : null;
      if (items == null || items.isEmpty) return [];
      return await compute(_parseBrandList, items);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<BrandResponseModel> getBrandById(String id) async {
    try {
      final response = await _dio.get('/Brands/$id');
      return BrandResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<BrandResponseModel> createBrand(CreateBrandRequest request) async {
    try {
      final response = await _dio.post('/Brands', data: request.toJson());
      return BrandResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<BrandResponseModel> updateBrand(String id, UpdateBrandRequest request) async {
    try {
      final response = await _dio.put('/Brands/$id', data: request.toJson());
      return BrandResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> deleteBrand(String id) async {
    try {
      await _dio.delete('/Brands/$id');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
BrandRepository brandRepository(BrandRepositoryRef ref) {
  return BrandRepository(ref.read(dioProvider));
}
