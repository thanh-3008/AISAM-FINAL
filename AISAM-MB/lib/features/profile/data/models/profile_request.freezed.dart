// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'profile_request.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CreateProfileRequest _$CreateProfileRequestFromJson(Map<String, dynamic> json) {
  return _CreateProfileRequest.fromJson(json);
}

/// @nodoc
mixin _$CreateProfileRequest {
  String get name => throw _privateConstructorUsedError;
  int get profileType => throw _privateConstructorUsedError;
  String? get companyName => throw _privateConstructorUsedError;
  String? get bio => throw _privateConstructorUsedError;

  /// Serializes this CreateProfileRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CreateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CreateProfileRequestCopyWith<CreateProfileRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CreateProfileRequestCopyWith<$Res> {
  factory $CreateProfileRequestCopyWith(
    CreateProfileRequest value,
    $Res Function(CreateProfileRequest) then,
  ) = _$CreateProfileRequestCopyWithImpl<$Res, CreateProfileRequest>;
  @useResult
  $Res call({String name, int profileType, String? companyName, String? bio});
}

/// @nodoc
class _$CreateProfileRequestCopyWithImpl<
  $Res,
  $Val extends CreateProfileRequest
>
    implements $CreateProfileRequestCopyWith<$Res> {
  _$CreateProfileRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CreateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = null,
    Object? profileType = null,
    Object? companyName = freezed,
    Object? bio = freezed,
  }) {
    return _then(
      _value.copyWith(
            name: null == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                      as String,
            profileType: null == profileType
                ? _value.profileType
                : profileType // ignore: cast_nullable_to_non_nullable
                      as int,
            companyName: freezed == companyName
                ? _value.companyName
                : companyName // ignore: cast_nullable_to_non_nullable
                      as String?,
            bio: freezed == bio
                ? _value.bio
                : bio // ignore: cast_nullable_to_non_nullable
                      as String?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CreateProfileRequestImplCopyWith<$Res>
    implements $CreateProfileRequestCopyWith<$Res> {
  factory _$$CreateProfileRequestImplCopyWith(
    _$CreateProfileRequestImpl value,
    $Res Function(_$CreateProfileRequestImpl) then,
  ) = __$$CreateProfileRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String name, int profileType, String? companyName, String? bio});
}

/// @nodoc
class __$$CreateProfileRequestImplCopyWithImpl<$Res>
    extends _$CreateProfileRequestCopyWithImpl<$Res, _$CreateProfileRequestImpl>
    implements _$$CreateProfileRequestImplCopyWith<$Res> {
  __$$CreateProfileRequestImplCopyWithImpl(
    _$CreateProfileRequestImpl _value,
    $Res Function(_$CreateProfileRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CreateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = null,
    Object? profileType = null,
    Object? companyName = freezed,
    Object? bio = freezed,
  }) {
    return _then(
      _$CreateProfileRequestImpl(
        name: null == name
            ? _value.name
            : name // ignore: cast_nullable_to_non_nullable
                  as String,
        profileType: null == profileType
            ? _value.profileType
            : profileType // ignore: cast_nullable_to_non_nullable
                  as int,
        companyName: freezed == companyName
            ? _value.companyName
            : companyName // ignore: cast_nullable_to_non_nullable
                  as String?,
        bio: freezed == bio
            ? _value.bio
            : bio // ignore: cast_nullable_to_non_nullable
                  as String?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CreateProfileRequestImpl implements _CreateProfileRequest {
  const _$CreateProfileRequestImpl({
    required this.name,
    required this.profileType,
    this.companyName,
    this.bio,
  });

  factory _$CreateProfileRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$CreateProfileRequestImplFromJson(json);

  @override
  final String name;
  @override
  final int profileType;
  @override
  final String? companyName;
  @override
  final String? bio;

  @override
  String toString() {
    return 'CreateProfileRequest(name: $name, profileType: $profileType, companyName: $companyName, bio: $bio)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CreateProfileRequestImpl &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.profileType, profileType) ||
                other.profileType == profileType) &&
            (identical(other.companyName, companyName) ||
                other.companyName == companyName) &&
            (identical(other.bio, bio) || other.bio == bio));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, name, profileType, companyName, bio);

  /// Create a copy of CreateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CreateProfileRequestImplCopyWith<_$CreateProfileRequestImpl>
  get copyWith =>
      __$$CreateProfileRequestImplCopyWithImpl<_$CreateProfileRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$CreateProfileRequestImplToJson(this);
  }
}

abstract class _CreateProfileRequest implements CreateProfileRequest {
  const factory _CreateProfileRequest({
    required final String name,
    required final int profileType,
    final String? companyName,
    final String? bio,
  }) = _$CreateProfileRequestImpl;

  factory _CreateProfileRequest.fromJson(Map<String, dynamic> json) =
      _$CreateProfileRequestImpl.fromJson;

  @override
  String get name;
  @override
  int get profileType;
  @override
  String? get companyName;
  @override
  String? get bio;

  /// Create a copy of CreateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CreateProfileRequestImplCopyWith<_$CreateProfileRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}

UpdateProfileRequest _$UpdateProfileRequestFromJson(Map<String, dynamic> json) {
  return _UpdateProfileRequest.fromJson(json);
}

/// @nodoc
mixin _$UpdateProfileRequest {
  String? get name => throw _privateConstructorUsedError;
  int? get profileType => throw _privateConstructorUsedError;
  String? get companyName => throw _privateConstructorUsedError;
  String? get bio => throw _privateConstructorUsedError;
  String? get avatarUrl => throw _privateConstructorUsedError;

  /// Serializes this UpdateProfileRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of UpdateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $UpdateProfileRequestCopyWith<UpdateProfileRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $UpdateProfileRequestCopyWith<$Res> {
  factory $UpdateProfileRequestCopyWith(
    UpdateProfileRequest value,
    $Res Function(UpdateProfileRequest) then,
  ) = _$UpdateProfileRequestCopyWithImpl<$Res, UpdateProfileRequest>;
  @useResult
  $Res call({
    String? name,
    int? profileType,
    String? companyName,
    String? bio,
    String? avatarUrl,
  });
}

/// @nodoc
class _$UpdateProfileRequestCopyWithImpl<
  $Res,
  $Val extends UpdateProfileRequest
>
    implements $UpdateProfileRequestCopyWith<$Res> {
  _$UpdateProfileRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of UpdateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = freezed,
    Object? profileType = freezed,
    Object? companyName = freezed,
    Object? bio = freezed,
    Object? avatarUrl = freezed,
  }) {
    return _then(
      _value.copyWith(
            name: freezed == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                      as String?,
            profileType: freezed == profileType
                ? _value.profileType
                : profileType // ignore: cast_nullable_to_non_nullable
                      as int?,
            companyName: freezed == companyName
                ? _value.companyName
                : companyName // ignore: cast_nullable_to_non_nullable
                      as String?,
            bio: freezed == bio
                ? _value.bio
                : bio // ignore: cast_nullable_to_non_nullable
                      as String?,
            avatarUrl: freezed == avatarUrl
                ? _value.avatarUrl
                : avatarUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$UpdateProfileRequestImplCopyWith<$Res>
    implements $UpdateProfileRequestCopyWith<$Res> {
  factory _$$UpdateProfileRequestImplCopyWith(
    _$UpdateProfileRequestImpl value,
    $Res Function(_$UpdateProfileRequestImpl) then,
  ) = __$$UpdateProfileRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String? name,
    int? profileType,
    String? companyName,
    String? bio,
    String? avatarUrl,
  });
}

/// @nodoc
class __$$UpdateProfileRequestImplCopyWithImpl<$Res>
    extends _$UpdateProfileRequestCopyWithImpl<$Res, _$UpdateProfileRequestImpl>
    implements _$$UpdateProfileRequestImplCopyWith<$Res> {
  __$$UpdateProfileRequestImplCopyWithImpl(
    _$UpdateProfileRequestImpl _value,
    $Res Function(_$UpdateProfileRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of UpdateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = freezed,
    Object? profileType = freezed,
    Object? companyName = freezed,
    Object? bio = freezed,
    Object? avatarUrl = freezed,
  }) {
    return _then(
      _$UpdateProfileRequestImpl(
        name: freezed == name
            ? _value.name
            : name // ignore: cast_nullable_to_non_nullable
                  as String?,
        profileType: freezed == profileType
            ? _value.profileType
            : profileType // ignore: cast_nullable_to_non_nullable
                  as int?,
        companyName: freezed == companyName
            ? _value.companyName
            : companyName // ignore: cast_nullable_to_non_nullable
                  as String?,
        bio: freezed == bio
            ? _value.bio
            : bio // ignore: cast_nullable_to_non_nullable
                  as String?,
        avatarUrl: freezed == avatarUrl
            ? _value.avatarUrl
            : avatarUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$UpdateProfileRequestImpl implements _UpdateProfileRequest {
  const _$UpdateProfileRequestImpl({
    this.name,
    this.profileType,
    this.companyName,
    this.bio,
    this.avatarUrl,
  });

  factory _$UpdateProfileRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$UpdateProfileRequestImplFromJson(json);

  @override
  final String? name;
  @override
  final int? profileType;
  @override
  final String? companyName;
  @override
  final String? bio;
  @override
  final String? avatarUrl;

  @override
  String toString() {
    return 'UpdateProfileRequest(name: $name, profileType: $profileType, companyName: $companyName, bio: $bio, avatarUrl: $avatarUrl)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$UpdateProfileRequestImpl &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.profileType, profileType) ||
                other.profileType == profileType) &&
            (identical(other.companyName, companyName) ||
                other.companyName == companyName) &&
            (identical(other.bio, bio) || other.bio == bio) &&
            (identical(other.avatarUrl, avatarUrl) ||
                other.avatarUrl == avatarUrl));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, name, profileType, companyName, bio, avatarUrl);

  /// Create a copy of UpdateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$UpdateProfileRequestImplCopyWith<_$UpdateProfileRequestImpl>
  get copyWith =>
      __$$UpdateProfileRequestImplCopyWithImpl<_$UpdateProfileRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$UpdateProfileRequestImplToJson(this);
  }
}

abstract class _UpdateProfileRequest implements UpdateProfileRequest {
  const factory _UpdateProfileRequest({
    final String? name,
    final int? profileType,
    final String? companyName,
    final String? bio,
    final String? avatarUrl,
  }) = _$UpdateProfileRequestImpl;

  factory _UpdateProfileRequest.fromJson(Map<String, dynamic> json) =
      _$UpdateProfileRequestImpl.fromJson;

  @override
  String? get name;
  @override
  int? get profileType;
  @override
  String? get companyName;
  @override
  String? get bio;
  @override
  String? get avatarUrl;

  /// Create a copy of UpdateProfileRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$UpdateProfileRequestImplCopyWith<_$UpdateProfileRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}
