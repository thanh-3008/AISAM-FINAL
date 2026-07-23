// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'product_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$CreateProductRequestImpl _$$CreateProductRequestImplFromJson(
  Map<String, dynamic> json,
) => _$CreateProductRequestImpl(
  name: json['name'] as String,
  description: json['description'] as String?,
  price: (json['price'] as num?)?.toDouble(),
  stock: (json['stock'] as num?)?.toInt(),
  brandId: json['brandId'] as String,
);

Map<String, dynamic> _$$CreateProductRequestImplToJson(
  _$CreateProductRequestImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
  'price': instance.price,
  'stock': instance.stock,
  'brandId': instance.brandId,
};

_$UpdateProductRequestImpl _$$UpdateProductRequestImplFromJson(
  Map<String, dynamic> json,
) => _$UpdateProductRequestImpl(
  name: json['name'] as String?,
  description: json['description'] as String?,
  price: (json['price'] as num?)?.toDouble(),
  stock: (json['stock'] as num?)?.toInt(),
);

Map<String, dynamic> _$$UpdateProductRequestImplToJson(
  _$UpdateProductRequestImpl instance,
) => <String, dynamic>{
  'name': instance.name,
  'description': instance.description,
  'price': instance.price,
  'stock': instance.stock,
};
