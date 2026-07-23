// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'brand_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

BrandResponseModel _$BrandResponseModelFromJson(Map<String, dynamic> json) {
  return _BrandResponseModel.fromJson(json);
}

/// @nodoc
mixin _$BrandResponseModel {
  String get id => throw _privateConstructorUsedError;
  String get userId => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;
  String? get logoUrl => throw _privateConstructorUsedError;
  String? get slogan => throw _privateConstructorUsedError;
  String? get usp => throw _privateConstructorUsedError;
  String? get targetAudience => throw _privateConstructorUsedError;
  String? get profileId => throw _privateConstructorUsedError;
  String? get workspaceId => throw _privateConstructorUsedError;
  DateTime get createdAt => throw _privateConstructorUsedError;
  DateTime get updatedAt => throw _privateConstructorUsedError;
  int get productsCount => throw _privateConstructorUsedError;
  int get contentsCount => throw _privateConstructorUsedError;

  /// Serializes this BrandResponseModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of BrandResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $BrandResponseModelCopyWith<BrandResponseModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $BrandResponseModelCopyWith<$Res> {
  factory $BrandResponseModelCopyWith(
    BrandResponseModel value,
    $Res Function(BrandResponseModel) then,
  ) = _$BrandResponseModelCopyWithImpl<$Res, BrandResponseModel>;
  @useResult
  $Res call({
    String id,
    String userId,
    String name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
    String? profileId,
    String? workspaceId,
    DateTime createdAt,
    DateTime updatedAt,
    int productsCount,
    int contentsCount,
  });
}

/// @nodoc
class _$BrandResponseModelCopyWithImpl<$Res, $Val extends BrandResponseModel>
    implements $BrandResponseModelCopyWith<$Res> {
  _$BrandResponseModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of BrandResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? userId = null,
    Object? name = null,
    Object? description = freezed,
    Object? logoUrl = freezed,
    Object? slogan = freezed,
    Object? usp = freezed,
    Object? targetAudience = freezed,
    Object? profileId = freezed,
    Object? workspaceId = freezed,
    Object? createdAt = null,
    Object? updatedAt = null,
    Object? productsCount = null,
    Object? contentsCount = null,
  }) {
    return _then(
      _value.copyWith(
            id: null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                      as String,
            userId: null == userId
                ? _value.userId
                : userId // ignore: cast_nullable_to_non_nullable
                      as String,
            name: null == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                      as String,
            description: freezed == description
                ? _value.description
                : description // ignore: cast_nullable_to_non_nullable
                      as String?,
            logoUrl: freezed == logoUrl
                ? _value.logoUrl
                : logoUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            slogan: freezed == slogan
                ? _value.slogan
                : slogan // ignore: cast_nullable_to_non_nullable
                      as String?,
            usp: freezed == usp
                ? _value.usp
                : usp // ignore: cast_nullable_to_non_nullable
                      as String?,
            targetAudience: freezed == targetAudience
                ? _value.targetAudience
                : targetAudience // ignore: cast_nullable_to_non_nullable
                      as String?,
            profileId: freezed == profileId
                ? _value.profileId
                : profileId // ignore: cast_nullable_to_non_nullable
                      as String?,
            workspaceId: freezed == workspaceId
                ? _value.workspaceId
                : workspaceId // ignore: cast_nullable_to_non_nullable
                      as String?,
            createdAt: null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
            updatedAt: null == updatedAt
                ? _value.updatedAt
                : updatedAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
            productsCount: null == productsCount
                ? _value.productsCount
                : productsCount // ignore: cast_nullable_to_non_nullable
                      as int,
            contentsCount: null == contentsCount
                ? _value.contentsCount
                : contentsCount // ignore: cast_nullable_to_non_nullable
                      as int,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$BrandResponseModelImplCopyWith<$Res>
    implements $BrandResponseModelCopyWith<$Res> {
  factory _$$BrandResponseModelImplCopyWith(
    _$BrandResponseModelImpl value,
    $Res Function(_$BrandResponseModelImpl) then,
  ) = __$$BrandResponseModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String id,
    String userId,
    String name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
    String? profileId,
    String? workspaceId,
    DateTime createdAt,
    DateTime updatedAt,
    int productsCount,
    int contentsCount,
  });
}

/// @nodoc
class __$$BrandResponseModelImplCopyWithImpl<$Res>
    extends _$BrandResponseModelCopyWithImpl<$Res, _$BrandResponseModelImpl>
    implements _$$BrandResponseModelImplCopyWith<$Res> {
  __$$BrandResponseModelImplCopyWithImpl(
    _$BrandResponseModelImpl _value,
    $Res Function(_$BrandResponseModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of BrandResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? userId = null,
    Object? name = null,
    Object? description = freezed,
    Object? logoUrl = freezed,
    Object? slogan = freezed,
    Object? usp = freezed,
    Object? targetAudience = freezed,
    Object? profileId = freezed,
    Object? workspaceId = freezed,
    Object? createdAt = null,
    Object? updatedAt = null,
    Object? productsCount = null,
    Object? contentsCount = null,
  }) {
    return _then(
      _$BrandResponseModelImpl(
        id: null == id
            ? _value.id
            : id // ignore: cast_nullable_to_non_nullable
                  as String,
        userId: null == userId
            ? _value.userId
            : userId // ignore: cast_nullable_to_non_nullable
                  as String,
        name: null == name
            ? _value.name
            : name // ignore: cast_nullable_to_non_nullable
                  as String,
        description: freezed == description
            ? _value.description
            : description // ignore: cast_nullable_to_non_nullable
                  as String?,
        logoUrl: freezed == logoUrl
            ? _value.logoUrl
            : logoUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        slogan: freezed == slogan
            ? _value.slogan
            : slogan // ignore: cast_nullable_to_non_nullable
                  as String?,
        usp: freezed == usp
            ? _value.usp
            : usp // ignore: cast_nullable_to_non_nullable
                  as String?,
        targetAudience: freezed == targetAudience
            ? _value.targetAudience
            : targetAudience // ignore: cast_nullable_to_non_nullable
                  as String?,
        profileId: freezed == profileId
            ? _value.profileId
            : profileId // ignore: cast_nullable_to_non_nullable
                  as String?,
        workspaceId: freezed == workspaceId
            ? _value.workspaceId
            : workspaceId // ignore: cast_nullable_to_non_nullable
                  as String?,
        createdAt: null == createdAt
            ? _value.createdAt
            : createdAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
        updatedAt: null == updatedAt
            ? _value.updatedAt
            : updatedAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
        productsCount: null == productsCount
            ? _value.productsCount
            : productsCount // ignore: cast_nullable_to_non_nullable
                  as int,
        contentsCount: null == contentsCount
            ? _value.contentsCount
            : contentsCount // ignore: cast_nullable_to_non_nullable
                  as int,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$BrandResponseModelImpl implements _BrandResponseModel {
  const _$BrandResponseModelImpl({
    required this.id,
    required this.userId,
    required this.name,
    this.description,
    this.logoUrl,
    this.slogan,
    this.usp,
    this.targetAudience,
    this.profileId,
    this.workspaceId,
    required this.createdAt,
    required this.updatedAt,
    required this.productsCount,
    required this.contentsCount,
  });

  factory _$BrandResponseModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$BrandResponseModelImplFromJson(json);

  @override
  final String id;
  @override
  final String userId;
  @override
  final String name;
  @override
  final String? description;
  @override
  final String? logoUrl;
  @override
  final String? slogan;
  @override
  final String? usp;
  @override
  final String? targetAudience;
  @override
  final String? profileId;
  @override
  final String? workspaceId;
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;
  @override
  final int productsCount;
  @override
  final int contentsCount;

  @override
  String toString() {
    return 'BrandResponseModel(id: $id, userId: $userId, name: $name, description: $description, logoUrl: $logoUrl, slogan: $slogan, usp: $usp, targetAudience: $targetAudience, profileId: $profileId, workspaceId: $workspaceId, createdAt: $createdAt, updatedAt: $updatedAt, productsCount: $productsCount, contentsCount: $contentsCount)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$BrandResponseModelImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.userId, userId) || other.userId == userId) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.description, description) ||
                other.description == description) &&
            (identical(other.logoUrl, logoUrl) || other.logoUrl == logoUrl) &&
            (identical(other.slogan, slogan) || other.slogan == slogan) &&
            (identical(other.usp, usp) || other.usp == usp) &&
            (identical(other.targetAudience, targetAudience) ||
                other.targetAudience == targetAudience) &&
            (identical(other.profileId, profileId) ||
                other.profileId == profileId) &&
            (identical(other.workspaceId, workspaceId) ||
                other.workspaceId == workspaceId) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt) &&
            (identical(other.updatedAt, updatedAt) ||
                other.updatedAt == updatedAt) &&
            (identical(other.productsCount, productsCount) ||
                other.productsCount == productsCount) &&
            (identical(other.contentsCount, contentsCount) ||
                other.contentsCount == contentsCount));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    userId,
    name,
    description,
    logoUrl,
    slogan,
    usp,
    targetAudience,
    profileId,
    workspaceId,
    createdAt,
    updatedAt,
    productsCount,
    contentsCount,
  );

  /// Create a copy of BrandResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$BrandResponseModelImplCopyWith<_$BrandResponseModelImpl> get copyWith =>
      __$$BrandResponseModelImplCopyWithImpl<_$BrandResponseModelImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$BrandResponseModelImplToJson(this);
  }
}

abstract class _BrandResponseModel implements BrandResponseModel {
  const factory _BrandResponseModel({
    required final String id,
    required final String userId,
    required final String name,
    final String? description,
    final String? logoUrl,
    final String? slogan,
    final String? usp,
    final String? targetAudience,
    final String? profileId,
    final String? workspaceId,
    required final DateTime createdAt,
    required final DateTime updatedAt,
    required final int productsCount,
    required final int contentsCount,
  }) = _$BrandResponseModelImpl;

  factory _BrandResponseModel.fromJson(Map<String, dynamic> json) =
      _$BrandResponseModelImpl.fromJson;

  @override
  String get id;
  @override
  String get userId;
  @override
  String get name;
  @override
  String? get description;
  @override
  String? get logoUrl;
  @override
  String? get slogan;
  @override
  String? get usp;
  @override
  String? get targetAudience;
  @override
  String? get profileId;
  @override
  String? get workspaceId;
  @override
  DateTime get createdAt;
  @override
  DateTime get updatedAt;
  @override
  int get productsCount;
  @override
  int get contentsCount;

  /// Create a copy of BrandResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$BrandResponseModelImplCopyWith<_$BrandResponseModelImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
