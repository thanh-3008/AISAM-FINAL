// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'brand_request.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CreateBrandRequest _$CreateBrandRequestFromJson(Map<String, dynamic> json) {
  return _CreateBrandRequest.fromJson(json);
}

/// @nodoc
mixin _$CreateBrandRequest {
  String get name => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;
  String? get logoUrl => throw _privateConstructorUsedError;
  String? get slogan => throw _privateConstructorUsedError;
  String? get usp => throw _privateConstructorUsedError;
  String? get targetAudience => throw _privateConstructorUsedError;

  /// Serializes this CreateBrandRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CreateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CreateBrandRequestCopyWith<CreateBrandRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CreateBrandRequestCopyWith<$Res> {
  factory $CreateBrandRequestCopyWith(
    CreateBrandRequest value,
    $Res Function(CreateBrandRequest) then,
  ) = _$CreateBrandRequestCopyWithImpl<$Res, CreateBrandRequest>;
  @useResult
  $Res call({
    String name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
  });
}

/// @nodoc
class _$CreateBrandRequestCopyWithImpl<$Res, $Val extends CreateBrandRequest>
    implements $CreateBrandRequestCopyWith<$Res> {
  _$CreateBrandRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CreateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = null,
    Object? description = freezed,
    Object? logoUrl = freezed,
    Object? slogan = freezed,
    Object? usp = freezed,
    Object? targetAudience = freezed,
  }) {
    return _then(
      _value.copyWith(
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
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CreateBrandRequestImplCopyWith<$Res>
    implements $CreateBrandRequestCopyWith<$Res> {
  factory _$$CreateBrandRequestImplCopyWith(
    _$CreateBrandRequestImpl value,
    $Res Function(_$CreateBrandRequestImpl) then,
  ) = __$$CreateBrandRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
  });
}

/// @nodoc
class __$$CreateBrandRequestImplCopyWithImpl<$Res>
    extends _$CreateBrandRequestCopyWithImpl<$Res, _$CreateBrandRequestImpl>
    implements _$$CreateBrandRequestImplCopyWith<$Res> {
  __$$CreateBrandRequestImplCopyWithImpl(
    _$CreateBrandRequestImpl _value,
    $Res Function(_$CreateBrandRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CreateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = null,
    Object? description = freezed,
    Object? logoUrl = freezed,
    Object? slogan = freezed,
    Object? usp = freezed,
    Object? targetAudience = freezed,
  }) {
    return _then(
      _$CreateBrandRequestImpl(
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
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CreateBrandRequestImpl implements _CreateBrandRequest {
  const _$CreateBrandRequestImpl({
    required this.name,
    this.description,
    this.logoUrl,
    this.slogan,
    this.usp,
    this.targetAudience,
  });

  factory _$CreateBrandRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$CreateBrandRequestImplFromJson(json);

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
  String toString() {
    return 'CreateBrandRequest(name: $name, description: $description, logoUrl: $logoUrl, slogan: $slogan, usp: $usp, targetAudience: $targetAudience)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CreateBrandRequestImpl &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.description, description) ||
                other.description == description) &&
            (identical(other.logoUrl, logoUrl) || other.logoUrl == logoUrl) &&
            (identical(other.slogan, slogan) || other.slogan == slogan) &&
            (identical(other.usp, usp) || other.usp == usp) &&
            (identical(other.targetAudience, targetAudience) ||
                other.targetAudience == targetAudience));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    name,
    description,
    logoUrl,
    slogan,
    usp,
    targetAudience,
  );

  /// Create a copy of CreateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CreateBrandRequestImplCopyWith<_$CreateBrandRequestImpl> get copyWith =>
      __$$CreateBrandRequestImplCopyWithImpl<_$CreateBrandRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$CreateBrandRequestImplToJson(this);
  }
}

abstract class _CreateBrandRequest implements CreateBrandRequest {
  const factory _CreateBrandRequest({
    required final String name,
    final String? description,
    final String? logoUrl,
    final String? slogan,
    final String? usp,
    final String? targetAudience,
  }) = _$CreateBrandRequestImpl;

  factory _CreateBrandRequest.fromJson(Map<String, dynamic> json) =
      _$CreateBrandRequestImpl.fromJson;

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

  /// Create a copy of CreateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CreateBrandRequestImplCopyWith<_$CreateBrandRequestImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

UpdateBrandRequest _$UpdateBrandRequestFromJson(Map<String, dynamic> json) {
  return _UpdateBrandRequest.fromJson(json);
}

/// @nodoc
mixin _$UpdateBrandRequest {
  String? get name => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;
  String? get logoUrl => throw _privateConstructorUsedError;
  String? get slogan => throw _privateConstructorUsedError;
  String? get usp => throw _privateConstructorUsedError;
  String? get targetAudience => throw _privateConstructorUsedError;

  /// Serializes this UpdateBrandRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of UpdateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $UpdateBrandRequestCopyWith<UpdateBrandRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $UpdateBrandRequestCopyWith<$Res> {
  factory $UpdateBrandRequestCopyWith(
    UpdateBrandRequest value,
    $Res Function(UpdateBrandRequest) then,
  ) = _$UpdateBrandRequestCopyWithImpl<$Res, UpdateBrandRequest>;
  @useResult
  $Res call({
    String? name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
  });
}

/// @nodoc
class _$UpdateBrandRequestCopyWithImpl<$Res, $Val extends UpdateBrandRequest>
    implements $UpdateBrandRequestCopyWith<$Res> {
  _$UpdateBrandRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of UpdateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = freezed,
    Object? description = freezed,
    Object? logoUrl = freezed,
    Object? slogan = freezed,
    Object? usp = freezed,
    Object? targetAudience = freezed,
  }) {
    return _then(
      _value.copyWith(
            name: freezed == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                      as String?,
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
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$UpdateBrandRequestImplCopyWith<$Res>
    implements $UpdateBrandRequestCopyWith<$Res> {
  factory _$$UpdateBrandRequestImplCopyWith(
    _$UpdateBrandRequestImpl value,
    $Res Function(_$UpdateBrandRequestImpl) then,
  ) = __$$UpdateBrandRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String? name,
    String? description,
    String? logoUrl,
    String? slogan,
    String? usp,
    String? targetAudience,
  });
}

/// @nodoc
class __$$UpdateBrandRequestImplCopyWithImpl<$Res>
    extends _$UpdateBrandRequestCopyWithImpl<$Res, _$UpdateBrandRequestImpl>
    implements _$$UpdateBrandRequestImplCopyWith<$Res> {
  __$$UpdateBrandRequestImplCopyWithImpl(
    _$UpdateBrandRequestImpl _value,
    $Res Function(_$UpdateBrandRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of UpdateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = freezed,
    Object? description = freezed,
    Object? logoUrl = freezed,
    Object? slogan = freezed,
    Object? usp = freezed,
    Object? targetAudience = freezed,
  }) {
    return _then(
      _$UpdateBrandRequestImpl(
        name: freezed == name
            ? _value.name
            : name // ignore: cast_nullable_to_non_nullable
                  as String?,
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
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$UpdateBrandRequestImpl implements _UpdateBrandRequest {
  const _$UpdateBrandRequestImpl({
    this.name,
    this.description,
    this.logoUrl,
    this.slogan,
    this.usp,
    this.targetAudience,
  });

  factory _$UpdateBrandRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$UpdateBrandRequestImplFromJson(json);

  @override
  final String? name;
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
  String toString() {
    return 'UpdateBrandRequest(name: $name, description: $description, logoUrl: $logoUrl, slogan: $slogan, usp: $usp, targetAudience: $targetAudience)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$UpdateBrandRequestImpl &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.description, description) ||
                other.description == description) &&
            (identical(other.logoUrl, logoUrl) || other.logoUrl == logoUrl) &&
            (identical(other.slogan, slogan) || other.slogan == slogan) &&
            (identical(other.usp, usp) || other.usp == usp) &&
            (identical(other.targetAudience, targetAudience) ||
                other.targetAudience == targetAudience));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    name,
    description,
    logoUrl,
    slogan,
    usp,
    targetAudience,
  );

  /// Create a copy of UpdateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$UpdateBrandRequestImplCopyWith<_$UpdateBrandRequestImpl> get copyWith =>
      __$$UpdateBrandRequestImplCopyWithImpl<_$UpdateBrandRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$UpdateBrandRequestImplToJson(this);
  }
}

abstract class _UpdateBrandRequest implements UpdateBrandRequest {
  const factory _UpdateBrandRequest({
    final String? name,
    final String? description,
    final String? logoUrl,
    final String? slogan,
    final String? usp,
    final String? targetAudience,
  }) = _$UpdateBrandRequestImpl;

  factory _UpdateBrandRequest.fromJson(Map<String, dynamic> json) =
      _$UpdateBrandRequestImpl.fromJson;

  @override
  String? get name;
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

  /// Create a copy of UpdateBrandRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$UpdateBrandRequestImplCopyWith<_$UpdateBrandRequestImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
