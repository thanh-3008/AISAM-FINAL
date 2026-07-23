// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'workspace_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

WorkspaceResponseModel _$WorkspaceResponseModelFromJson(
  Map<String, dynamic> json,
) {
  return _WorkspaceResponseModel.fromJson(json);
}

/// @nodoc
mixin _$WorkspaceResponseModel {
  String get id => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;
  String? get logoUrl => throw _privateConstructorUsedError;
  int get workspaceType =>
      throw _privateConstructorUsedError; // enum from backend
  int get status => throw _privateConstructorUsedError; // enum
  int get currentUserRole => throw _privateConstructorUsedError; // enum
  DateTime get createdAt => throw _privateConstructorUsedError;
  DateTime get updatedAt => throw _privateConstructorUsedError;

  /// Serializes this WorkspaceResponseModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of WorkspaceResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $WorkspaceResponseModelCopyWith<WorkspaceResponseModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $WorkspaceResponseModelCopyWith<$Res> {
  factory $WorkspaceResponseModelCopyWith(
    WorkspaceResponseModel value,
    $Res Function(WorkspaceResponseModel) then,
  ) = _$WorkspaceResponseModelCopyWithImpl<$Res, WorkspaceResponseModel>;
  @useResult
  $Res call({
    String id,
    String name,
    String? description,
    String? logoUrl,
    int workspaceType,
    int status,
    int currentUserRole,
    DateTime createdAt,
    DateTime updatedAt,
  });
}

/// @nodoc
class _$WorkspaceResponseModelCopyWithImpl<
  $Res,
  $Val extends WorkspaceResponseModel
>
    implements $WorkspaceResponseModelCopyWith<$Res> {
  _$WorkspaceResponseModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of WorkspaceResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? name = null,
    Object? description = freezed,
    Object? logoUrl = freezed,
    Object? workspaceType = null,
    Object? status = null,
    Object? currentUserRole = null,
    Object? createdAt = null,
    Object? updatedAt = null,
  }) {
    return _then(
      _value.copyWith(
            id: null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
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
            workspaceType: null == workspaceType
                ? _value.workspaceType
                : workspaceType // ignore: cast_nullable_to_non_nullable
                      as int,
            status: null == status
                ? _value.status
                : status // ignore: cast_nullable_to_non_nullable
                      as int,
            currentUserRole: null == currentUserRole
                ? _value.currentUserRole
                : currentUserRole // ignore: cast_nullable_to_non_nullable
                      as int,
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
abstract class _$$WorkspaceResponseModelImplCopyWith<$Res>
    implements $WorkspaceResponseModelCopyWith<$Res> {
  factory _$$WorkspaceResponseModelImplCopyWith(
    _$WorkspaceResponseModelImpl value,
    $Res Function(_$WorkspaceResponseModelImpl) then,
  ) = __$$WorkspaceResponseModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String id,
    String name,
    String? description,
    String? logoUrl,
    int workspaceType,
    int status,
    int currentUserRole,
    DateTime createdAt,
    DateTime updatedAt,
  });
}

/// @nodoc
class __$$WorkspaceResponseModelImplCopyWithImpl<$Res>
    extends
        _$WorkspaceResponseModelCopyWithImpl<$Res, _$WorkspaceResponseModelImpl>
    implements _$$WorkspaceResponseModelImplCopyWith<$Res> {
  __$$WorkspaceResponseModelImplCopyWithImpl(
    _$WorkspaceResponseModelImpl _value,
    $Res Function(_$WorkspaceResponseModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of WorkspaceResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? name = null,
    Object? description = freezed,
    Object? logoUrl = freezed,
    Object? workspaceType = null,
    Object? status = null,
    Object? currentUserRole = null,
    Object? createdAt = null,
    Object? updatedAt = null,
  }) {
    return _then(
      _$WorkspaceResponseModelImpl(
        id: null == id
            ? _value.id
            : id // ignore: cast_nullable_to_non_nullable
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
        workspaceType: null == workspaceType
            ? _value.workspaceType
            : workspaceType // ignore: cast_nullable_to_non_nullable
                  as int,
        status: null == status
            ? _value.status
            : status // ignore: cast_nullable_to_non_nullable
                  as int,
        currentUserRole: null == currentUserRole
            ? _value.currentUserRole
            : currentUserRole // ignore: cast_nullable_to_non_nullable
                  as int,
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
class _$WorkspaceResponseModelImpl implements _WorkspaceResponseModel {
  const _$WorkspaceResponseModelImpl({
    required this.id,
    required this.name,
    this.description,
    this.logoUrl,
    required this.workspaceType,
    required this.status,
    required this.currentUserRole,
    required this.createdAt,
    required this.updatedAt,
  });

  factory _$WorkspaceResponseModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$WorkspaceResponseModelImplFromJson(json);

  @override
  final String id;
  @override
  final String name;
  @override
  final String? description;
  @override
  final String? logoUrl;
  @override
  final int workspaceType;
  // enum from backend
  @override
  final int status;
  // enum
  @override
  final int currentUserRole;
  // enum
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;

  @override
  String toString() {
    return 'WorkspaceResponseModel(id: $id, name: $name, description: $description, logoUrl: $logoUrl, workspaceType: $workspaceType, status: $status, currentUserRole: $currentUserRole, createdAt: $createdAt, updatedAt: $updatedAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$WorkspaceResponseModelImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.description, description) ||
                other.description == description) &&
            (identical(other.logoUrl, logoUrl) || other.logoUrl == logoUrl) &&
            (identical(other.workspaceType, workspaceType) ||
                other.workspaceType == workspaceType) &&
            (identical(other.status, status) || other.status == status) &&
            (identical(other.currentUserRole, currentUserRole) ||
                other.currentUserRole == currentUserRole) &&
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
    name,
    description,
    logoUrl,
    workspaceType,
    status,
    currentUserRole,
    createdAt,
    updatedAt,
  );

  /// Create a copy of WorkspaceResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$WorkspaceResponseModelImplCopyWith<_$WorkspaceResponseModelImpl>
  get copyWith =>
      __$$WorkspaceResponseModelImplCopyWithImpl<_$WorkspaceResponseModelImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$WorkspaceResponseModelImplToJson(this);
  }
}

abstract class _WorkspaceResponseModel implements WorkspaceResponseModel {
  const factory _WorkspaceResponseModel({
    required final String id,
    required final String name,
    final String? description,
    final String? logoUrl,
    required final int workspaceType,
    required final int status,
    required final int currentUserRole,
    required final DateTime createdAt,
    required final DateTime updatedAt,
  }) = _$WorkspaceResponseModelImpl;

  factory _WorkspaceResponseModel.fromJson(Map<String, dynamic> json) =
      _$WorkspaceResponseModelImpl.fromJson;

  @override
  String get id;
  @override
  String get name;
  @override
  String? get description;
  @override
  String? get logoUrl;
  @override
  int get workspaceType; // enum from backend
  @override
  int get status; // enum
  @override
  int get currentUserRole; // enum
  @override
  DateTime get createdAt;
  @override
  DateTime get updatedAt;

  /// Create a copy of WorkspaceResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$WorkspaceResponseModelImplCopyWith<_$WorkspaceResponseModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}

WorkspaceMemberResponseModel _$WorkspaceMemberResponseModelFromJson(
  Map<String, dynamic> json,
) {
  return _WorkspaceMemberResponseModel.fromJson(json);
}

/// @nodoc
mixin _$WorkspaceMemberResponseModel {
  String get id => throw _privateConstructorUsedError;
  String get userId => throw _privateConstructorUsedError;
  String get email => throw _privateConstructorUsedError;
  String? get fullName => throw _privateConstructorUsedError;
  int get role => throw _privateConstructorUsedError; // enum
  DateTime get joinedAt => throw _privateConstructorUsedError;

  /// Serializes this WorkspaceMemberResponseModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of WorkspaceMemberResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $WorkspaceMemberResponseModelCopyWith<WorkspaceMemberResponseModel>
  get copyWith => throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $WorkspaceMemberResponseModelCopyWith<$Res> {
  factory $WorkspaceMemberResponseModelCopyWith(
    WorkspaceMemberResponseModel value,
    $Res Function(WorkspaceMemberResponseModel) then,
  ) =
      _$WorkspaceMemberResponseModelCopyWithImpl<
        $Res,
        WorkspaceMemberResponseModel
      >;
  @useResult
  $Res call({
    String id,
    String userId,
    String email,
    String? fullName,
    int role,
    DateTime joinedAt,
  });
}

/// @nodoc
class _$WorkspaceMemberResponseModelCopyWithImpl<
  $Res,
  $Val extends WorkspaceMemberResponseModel
>
    implements $WorkspaceMemberResponseModelCopyWith<$Res> {
  _$WorkspaceMemberResponseModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of WorkspaceMemberResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? userId = null,
    Object? email = null,
    Object? fullName = freezed,
    Object? role = null,
    Object? joinedAt = null,
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
            email: null == email
                ? _value.email
                : email // ignore: cast_nullable_to_non_nullable
                      as String,
            fullName: freezed == fullName
                ? _value.fullName
                : fullName // ignore: cast_nullable_to_non_nullable
                      as String?,
            role: null == role
                ? _value.role
                : role // ignore: cast_nullable_to_non_nullable
                      as int,
            joinedAt: null == joinedAt
                ? _value.joinedAt
                : joinedAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$WorkspaceMemberResponseModelImplCopyWith<$Res>
    implements $WorkspaceMemberResponseModelCopyWith<$Res> {
  factory _$$WorkspaceMemberResponseModelImplCopyWith(
    _$WorkspaceMemberResponseModelImpl value,
    $Res Function(_$WorkspaceMemberResponseModelImpl) then,
  ) = __$$WorkspaceMemberResponseModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String id,
    String userId,
    String email,
    String? fullName,
    int role,
    DateTime joinedAt,
  });
}

/// @nodoc
class __$$WorkspaceMemberResponseModelImplCopyWithImpl<$Res>
    extends
        _$WorkspaceMemberResponseModelCopyWithImpl<
          $Res,
          _$WorkspaceMemberResponseModelImpl
        >
    implements _$$WorkspaceMemberResponseModelImplCopyWith<$Res> {
  __$$WorkspaceMemberResponseModelImplCopyWithImpl(
    _$WorkspaceMemberResponseModelImpl _value,
    $Res Function(_$WorkspaceMemberResponseModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of WorkspaceMemberResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? userId = null,
    Object? email = null,
    Object? fullName = freezed,
    Object? role = null,
    Object? joinedAt = null,
  }) {
    return _then(
      _$WorkspaceMemberResponseModelImpl(
        id: null == id
            ? _value.id
            : id // ignore: cast_nullable_to_non_nullable
                  as String,
        userId: null == userId
            ? _value.userId
            : userId // ignore: cast_nullable_to_non_nullable
                  as String,
        email: null == email
            ? _value.email
            : email // ignore: cast_nullable_to_non_nullable
                  as String,
        fullName: freezed == fullName
            ? _value.fullName
            : fullName // ignore: cast_nullable_to_non_nullable
                  as String?,
        role: null == role
            ? _value.role
            : role // ignore: cast_nullable_to_non_nullable
                  as int,
        joinedAt: null == joinedAt
            ? _value.joinedAt
            : joinedAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$WorkspaceMemberResponseModelImpl
    implements _WorkspaceMemberResponseModel {
  const _$WorkspaceMemberResponseModelImpl({
    required this.id,
    required this.userId,
    required this.email,
    this.fullName,
    required this.role,
    required this.joinedAt,
  });

  factory _$WorkspaceMemberResponseModelImpl.fromJson(
    Map<String, dynamic> json,
  ) => _$$WorkspaceMemberResponseModelImplFromJson(json);

  @override
  final String id;
  @override
  final String userId;
  @override
  final String email;
  @override
  final String? fullName;
  @override
  final int role;
  // enum
  @override
  final DateTime joinedAt;

  @override
  String toString() {
    return 'WorkspaceMemberResponseModel(id: $id, userId: $userId, email: $email, fullName: $fullName, role: $role, joinedAt: $joinedAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$WorkspaceMemberResponseModelImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.userId, userId) || other.userId == userId) &&
            (identical(other.email, email) || other.email == email) &&
            (identical(other.fullName, fullName) ||
                other.fullName == fullName) &&
            (identical(other.role, role) || other.role == role) &&
            (identical(other.joinedAt, joinedAt) ||
                other.joinedAt == joinedAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, id, userId, email, fullName, role, joinedAt);

  /// Create a copy of WorkspaceMemberResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$WorkspaceMemberResponseModelImplCopyWith<
    _$WorkspaceMemberResponseModelImpl
  >
  get copyWith =>
      __$$WorkspaceMemberResponseModelImplCopyWithImpl<
        _$WorkspaceMemberResponseModelImpl
      >(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$WorkspaceMemberResponseModelImplToJson(this);
  }
}

abstract class _WorkspaceMemberResponseModel
    implements WorkspaceMemberResponseModel {
  const factory _WorkspaceMemberResponseModel({
    required final String id,
    required final String userId,
    required final String email,
    final String? fullName,
    required final int role,
    required final DateTime joinedAt,
  }) = _$WorkspaceMemberResponseModelImpl;

  factory _WorkspaceMemberResponseModel.fromJson(Map<String, dynamic> json) =
      _$WorkspaceMemberResponseModelImpl.fromJson;

  @override
  String get id;
  @override
  String get userId;
  @override
  String get email;
  @override
  String? get fullName;
  @override
  int get role; // enum
  @override
  DateTime get joinedAt;

  /// Create a copy of WorkspaceMemberResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$WorkspaceMemberResponseModelImplCopyWith<
    _$WorkspaceMemberResponseModelImpl
  >
  get copyWith => throw _privateConstructorUsedError;
}
