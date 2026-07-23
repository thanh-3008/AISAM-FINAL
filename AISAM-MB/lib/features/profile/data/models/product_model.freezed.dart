// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'product_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

ProductResponseModel _$ProductResponseModelFromJson(Map<String, dynamic> json) {
  return _ProductResponseModel.fromJson(json);
}

/// @nodoc
mixin _$ProductResponseModel {
  String get id => throw _privateConstructorUsedError;
  String get brandId => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;
  double? get price => throw _privateConstructorUsedError;
  int get stock => throw _privateConstructorUsedError;
  List<String>? get images => throw _privateConstructorUsedError;
  DateTime get createdAt => throw _privateConstructorUsedError;
  DateTime get updatedAt => throw _privateConstructorUsedError;

  /// Serializes this ProductResponseModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of ProductResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $ProductResponseModelCopyWith<ProductResponseModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ProductResponseModelCopyWith<$Res> {
  factory $ProductResponseModelCopyWith(
    ProductResponseModel value,
    $Res Function(ProductResponseModel) then,
  ) = _$ProductResponseModelCopyWithImpl<$Res, ProductResponseModel>;
  @useResult
  $Res call({
    String id,
    String brandId,
    String name,
    String? description,
    double? price,
    int stock,
    List<String>? images,
    DateTime createdAt,
    DateTime updatedAt,
  });
}

/// @nodoc
class _$ProductResponseModelCopyWithImpl<
  $Res,
  $Val extends ProductResponseModel
>
    implements $ProductResponseModelCopyWith<$Res> {
  _$ProductResponseModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of ProductResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? brandId = null,
    Object? name = null,
    Object? description = freezed,
    Object? price = freezed,
    Object? stock = null,
    Object? images = freezed,
    Object? createdAt = null,
    Object? updatedAt = null,
  }) {
    return _then(
      _value.copyWith(
            id: null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                      as String,
            brandId: null == brandId
                ? _value.brandId
                : brandId // ignore: cast_nullable_to_non_nullable
                      as String,
            name: null == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                      as String,
            description: freezed == description
                ? _value.description
                : description // ignore: cast_nullable_to_non_nullable
                      as String?,
            price: freezed == price
                ? _value.price
                : price // ignore: cast_nullable_to_non_nullable
                      as double?,
            stock: null == stock
                ? _value.stock
                : stock // ignore: cast_nullable_to_non_nullable
                      as int,
            images: freezed == images
                ? _value.images
                : images // ignore: cast_nullable_to_non_nullable
                      as List<String>?,
            createdAt: null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
            updatedAt: null == updatedAt
                ? _value.updatedAt
                : updatedAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$ProductResponseModelImplCopyWith<$Res>
    implements $ProductResponseModelCopyWith<$Res> {
  factory _$$ProductResponseModelImplCopyWith(
    _$ProductResponseModelImpl value,
    $Res Function(_$ProductResponseModelImpl) then,
  ) = __$$ProductResponseModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String id,
    String brandId,
    String name,
    String? description,
    double? price,
    int stock,
    List<String>? images,
    DateTime createdAt,
    DateTime updatedAt,
  });
}

/// @nodoc
class __$$ProductResponseModelImplCopyWithImpl<$Res>
    extends _$ProductResponseModelCopyWithImpl<$Res, _$ProductResponseModelImpl>
    implements _$$ProductResponseModelImplCopyWith<$Res> {
  __$$ProductResponseModelImplCopyWithImpl(
    _$ProductResponseModelImpl _value,
    $Res Function(_$ProductResponseModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of ProductResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? brandId = null,
    Object? name = null,
    Object? description = freezed,
    Object? price = freezed,
    Object? stock = null,
    Object? images = freezed,
    Object? createdAt = null,
    Object? updatedAt = null,
  }) {
    return _then(
      _$ProductResponseModelImpl(
        id: null == id
            ? _value.id
            : id // ignore: cast_nullable_to_non_nullable
                  as String,
        brandId: null == brandId
            ? _value.brandId
            : brandId // ignore: cast_nullable_to_non_nullable
                  as String,
        name: null == name
            ? _value.name
            : name // ignore: cast_nullable_to_non_nullable
                  as String,
        description: freezed == description
            ? _value.description
            : description // ignore: cast_nullable_to_non_nullable
                  as String?,
        price: freezed == price
            ? _value.price
            : price // ignore: cast_nullable_to_non_nullable
                  as double?,
        stock: null == stock
            ? _value.stock
            : stock // ignore: cast_nullable_to_non_nullable
                  as int,
        images: freezed == images
            ? _value._images
            : images // ignore: cast_nullable_to_non_nullable
                  as List<String>?,
        createdAt: null == createdAt
            ? _value.createdAt
            : createdAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
        updatedAt: null == updatedAt
            ? _value.updatedAt
            : updatedAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$ProductResponseModelImpl implements _ProductResponseModel {
  const _$ProductResponseModelImpl({
    required this.id,
    required this.brandId,
    required this.name,
    this.description,
    this.price,
    required this.stock,
    final List<String>? images,
    required this.createdAt,
    required this.updatedAt,
  }) : _images = images;

  factory _$ProductResponseModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$ProductResponseModelImplFromJson(json);

  @override
  final String id;
  @override
  final String brandId;
  @override
  final String name;
  @override
  final String? description;
  @override
  final double? price;
  @override
  final int stock;
  final List<String>? _images;
  @override
  List<String>? get images {
    final value = _images;
    if (value == null) return null;
    if (_images is EqualUnmodifiableListView) return _images;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;

  @override
  String toString() {
    return 'ProductResponseModel(id: $id, brandId: $brandId, name: $name, description: $description, price: $price, stock: $stock, images: $images, createdAt: $createdAt, updatedAt: $updatedAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ProductResponseModelImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.brandId, brandId) || other.brandId == brandId) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.description, description) ||
                other.description == description) &&
            (identical(other.price, price) || other.price == price) &&
            (identical(other.stock, stock) || other.stock == stock) &&
            const DeepCollectionEquality().equals(other._images, _images) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt) &&
            (identical(other.updatedAt, updatedAt) ||
                other.updatedAt == updatedAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    brandId,
    name,
    description,
    price,
    stock,
    const DeepCollectionEquality().hash(_images),
    createdAt,
    updatedAt,
  );

  /// Create a copy of ProductResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$ProductResponseModelImplCopyWith<_$ProductResponseModelImpl>
  get copyWith =>
      __$$ProductResponseModelImplCopyWithImpl<_$ProductResponseModelImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$ProductResponseModelImplToJson(this);
  }
}

abstract class _ProductResponseModel implements ProductResponseModel {
  const factory _ProductResponseModel({
    required final String id,
    required final String brandId,
    required final String name,
    final String? description,
    final double? price,
    required final int stock,
    final List<String>? images,
    required final DateTime createdAt,
    required final DateTime updatedAt,
  }) = _$ProductResponseModelImpl;

  factory _ProductResponseModel.fromJson(Map<String, dynamic> json) =
      _$ProductResponseModelImpl.fromJson;

  @override
  String get id;
  @override
  String get brandId;
  @override
  String get name;
  @override
  String? get description;
  @override
  double? get price;
  @override
  int get stock;
  @override
  List<String>? get images;
  @override
  DateTime get createdAt;
  @override
  DateTime get updatedAt;

  /// Create a copy of ProductResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$ProductResponseModelImplCopyWith<_$ProductResponseModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}
