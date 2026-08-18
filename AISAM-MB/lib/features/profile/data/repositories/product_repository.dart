import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/product_model.dart';
import '../models/product_request.dart';
part 'product_repository.g.dart';

List<ProductResponseModel> _parseProductList(List<dynamic> items) {
  return items.map((e) => ProductResponseModel.fromJson(e)).toList();
}

class ProductRepository {
  final Dio _dio;

  ProductRepository(this._dio);

  Future<List<ProductResponseModel>> getProducts({String? brandId}) async {
    try {
      final response = await _dio.get(
        '/Products',
        queryParameters: brandId != null ? {'brandId': brandId} : null,
      );
      final data = response.data['data'];
      final items = data != null ? data['data'] as List? : null;
      if (items == null || items.isEmpty) return [];
      return await compute(_parseProductList, items);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ProductResponseModel> getProductById(String id) async {
    try {
      final response = await _dio.get('/Products/$id');
      return ProductResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ProductResponseModel> createProduct(CreateProductRequest request) async {
    try {
      final response = await _dio.post('/Products', data: request.toJson());
      return ProductResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ProductResponseModel> updateProduct(String id, UpdateProductRequest request) async {
    try {
      final response = await _dio.put('/Products/$id', data: request.toJson());
      return ProductResponseModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> deleteProduct(String id) async {
    try {
      await _dio.delete('/Products/$id');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
ProductRepository productRepository(ProductRepositoryRef ref) {
  return ProductRepository(ref.read(dioProvider));
}
