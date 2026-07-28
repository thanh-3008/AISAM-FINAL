// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'product_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ProductResponseModelImpl _$$ProductResponseModelImplFromJson(
  Map<String, dynamic> json,
) => _$ProductResponseModelImpl(
  id: json['id'] as String,
  brandId: json['brandId'] as String,
  name: json['name'] as String,
  description: json['description'] as String?,
  price: (json['price'] as num?)?.toDouble(),
  stock: (json['stock'] as num).toInt(),
  images: (json['images'] as List<dynamic>?)?.map((e) => e as String).toList(),
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt: DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$$ProductResponseModelImplToJson(
  _$ProductResponseModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'brandId': instance.brandId,
  'name': instance.name,
  'description': instance.description,
  'price': instance.price,
  'stock': instance.stock,
  'images': instance.images,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt.toIso8601String(),
};
