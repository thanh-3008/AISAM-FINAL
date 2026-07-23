// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'workspace_request.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CreateWorkspaceRequest _$CreateWorkspaceRequestFromJson(
  Map<String, dynamic> json,
) {
  return _CreateWorkspaceRequest.fromJson(json);
}

/// @nodoc
mixin _$CreateWorkspaceRequest {
  String get name => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;
  int get workspaceType => throw _privateConstructorUsedError;

  /// Serializes this CreateWorkspaceRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CreateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CreateWorkspaceRequestCopyWith<CreateWorkspaceRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CreateWorkspaceRequestCopyWith<$Res> {
  factory $CreateWorkspaceRequestCopyWith(
    CreateWorkspaceRequest value,
    $Res Function(CreateWorkspaceRequest) then,
  ) = _$CreateWorkspaceRequestCopyWithImpl<$Res, CreateWorkspaceRequest>;
  @useResult
  $Res call({String name, String? description, int workspaceType});
}

/// @nodoc
class _$CreateWorkspaceRequestCopyWithImpl<
  $Res,
  $Val extends CreateWorkspaceRequest
>
    implements $CreateWorkspaceRequestCopyWith<$Res> {
  _$CreateWorkspaceRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CreateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = null,
    Object? description = freezed,
    Object? workspaceType = null,
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
            workspaceType: null == workspaceType
                ? _value.workspaceType
                : workspaceType // ignore: cast_nullable_to_non_nullable
                      as int,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CreateWorkspaceRequestImplCopyWith<$Res>
    implements $CreateWorkspaceRequestCopyWith<$Res> {
  factory _$$CreateWorkspaceRequestImplCopyWith(
    _$CreateWorkspaceRequestImpl value,
    $Res Function(_$CreateWorkspaceRequestImpl) then,
  ) = __$$CreateWorkspaceRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String name, String? description, int workspaceType});
}

/// @nodoc
class __$$CreateWorkspaceRequestImplCopyWithImpl<$Res>
    extends
        _$CreateWorkspaceRequestCopyWithImpl<$Res, _$CreateWorkspaceRequestImpl>
    implements _$$CreateWorkspaceRequestImplCopyWith<$Res> {
  __$$CreateWorkspaceRequestImplCopyWithImpl(
    _$CreateWorkspaceRequestImpl _value,
    $Res Function(_$CreateWorkspaceRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CreateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? name = null,
    Object? description = freezed,
    Object? workspaceType = null,
  }) {
    return _then(
      _$CreateWorkspaceRequestImpl(
        name: null == name
            ? _value.name
            : name // ignore: cast_nullable_to_non_nullable
                  as String,
        description: freezed == description
            ? _value.description
            : description // ignore: cast_nullable_to_non_nullable
                  as String?,
        workspaceType: null == workspaceType
            ? _value.workspaceType
            : workspaceType // ignore: cast_nullable_to_non_nullable
                  as int,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CreateWorkspaceRequestImpl implements _CreateWorkspaceRequest {
  const _$CreateWorkspaceRequestImpl({
    required this.name,
    this.description,
    required this.workspaceType,
  });

  factory _$CreateWorkspaceRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$CreateWorkspaceRequestImplFromJson(json);

  @override
  final String name;
  @override
  final String? description;
  @override
  final int workspaceType;

  @override
  String toString() {
    return 'CreateWorkspaceRequest(name: $name, description: $description, workspaceType: $workspaceType)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CreateWorkspaceRequestImpl &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.description, description) ||
                other.description == description) &&
            (identical(other.workspaceType, workspaceType) ||
                other.workspaceType == workspaceType));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, name, description, workspaceType);

  /// Create a copy of CreateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CreateWorkspaceRequestImplCopyWith<_$CreateWorkspaceRequestImpl>
  get copyWith =>
      __$$CreateWorkspaceRequestImplCopyWithImpl<_$CreateWorkspaceRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$CreateWorkspaceRequestImplToJson(this);
  }
}

abstract class _CreateWorkspaceRequest implements CreateWorkspaceRequest {
  const factory _CreateWorkspaceRequest({
    required final String name,
    final String? description,
    required final int workspaceType,
  }) = _$CreateWorkspaceRequestImpl;

  factory _CreateWorkspaceRequest.fromJson(Map<String, dynamic> json) =
      _$CreateWorkspaceRequestImpl.fromJson;

  @override
  String get name;
  @override
  String? get description;
  @override
  int get workspaceType;

  /// Create a copy of CreateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CreateWorkspaceRequestImplCopyWith<_$CreateWorkspaceRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}

UpdateWorkspaceRequest _$UpdateWorkspaceRequestFromJson(
  Map<String, dynamic> json,
) {
  return _UpdateWorkspaceRequest.fromJson(json);
}

/// @nodoc
mixin _$UpdateWorkspaceRequest {
  String get name => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;

  /// Serializes this UpdateWorkspaceRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of UpdateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $UpdateWorkspaceRequestCopyWith<UpdateWorkspaceRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $UpdateWorkspaceRequestCopyWith<$Res> {
  factory $UpdateWorkspaceRequestCopyWith(
    UpdateWorkspaceRequest value,
    $Res Function(UpdateWorkspaceRequest) then,
  ) = _$UpdateWorkspaceRequestCopyWithImpl<$Res, UpdateWorkspaceRequest>;
  @useResult
  $Res call({String name, String? description});
}

/// @nodoc
class _$UpdateWorkspaceRequestCopyWithImpl<
  $Res,
  $Val extends UpdateWorkspaceRequest
>
    implements $UpdateWorkspaceRequestCopyWith<$Res> {
  _$UpdateWorkspaceRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of UpdateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? name = null, Object? description = freezed}) {
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
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$UpdateWorkspaceRequestImplCopyWith<$Res>
    implements $UpdateWorkspaceRequestCopyWith<$Res> {
  factory _$$UpdateWorkspaceRequestImplCopyWith(
    _$UpdateWorkspaceRequestImpl value,
    $Res Function(_$UpdateWorkspaceRequestImpl) then,
  ) = __$$UpdateWorkspaceRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String name, String? description});
}

/// @nodoc
class __$$UpdateWorkspaceRequestImplCopyWithImpl<$Res>
    extends
        _$UpdateWorkspaceRequestCopyWithImpl<$Res, _$UpdateWorkspaceRequestImpl>
    implements _$$UpdateWorkspaceRequestImplCopyWith<$Res> {
  __$$UpdateWorkspaceRequestImplCopyWithImpl(
    _$UpdateWorkspaceRequestImpl _value,
    $Res Function(_$UpdateWorkspaceRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of UpdateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? name = null, Object? description = freezed}) {
    return _then(
      _$UpdateWorkspaceRequestImpl(
        name: null == name
            ? _value.name
            : name // ignore: cast_nullable_to_non_nullable
                  as String,
        description: freezed == description
            ? _value.description
            : description // ignore: cast_nullable_to_non_nullable
                  as String?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$UpdateWorkspaceRequestImpl implements _UpdateWorkspaceRequest {
  const _$UpdateWorkspaceRequestImpl({required this.name, this.description});

  factory _$UpdateWorkspaceRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$UpdateWorkspaceRequestImplFromJson(json);

  @override
  final String name;
  @override
  final String? description;

  @override
  String toString() {
    return 'UpdateWorkspaceRequest(name: $name, description: $description)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$UpdateWorkspaceRequestImpl &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.description, description) ||
                other.description == description));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, name, description);

  /// Create a copy of UpdateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$UpdateWorkspaceRequestImplCopyWith<_$UpdateWorkspaceRequestImpl>
  get copyWith =>
      __$$UpdateWorkspaceRequestImplCopyWithImpl<_$UpdateWorkspaceRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$UpdateWorkspaceRequestImplToJson(this);
  }
}

abstract class _UpdateWorkspaceRequest implements UpdateWorkspaceRequest {
  const factory _UpdateWorkspaceRequest({
    required final String name,
    final String? description,
  }) = _$UpdateWorkspaceRequestImpl;

  factory _UpdateWorkspaceRequest.fromJson(Map<String, dynamic> json) =
      _$UpdateWorkspaceRequestImpl.fromJson;

  @override
  String get name;
  @override
  String? get description;

  /// Create a copy of UpdateWorkspaceRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$UpdateWorkspaceRequestImplCopyWith<_$UpdateWorkspaceRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}
