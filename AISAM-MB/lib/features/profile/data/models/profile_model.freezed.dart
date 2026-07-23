// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'profile_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

ProfileResponseModel _$ProfileResponseModelFromJson(Map<String, dynamic> json) {
  return _ProfileResponseModel.fromJson(json);
}

/// @nodoc
mixin _$ProfileResponseModel {
  String get id => throw _privateConstructorUsedError;
  String get userId => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;
  int get profileType => throw _privateConstructorUsedError;
  String? get subscriptionId => throw _privateConstructorUsedError;
  String? get companyName => throw _privateConstructorUsedError;
  String? get bio => throw _privateConstructorUsedError;
  String? get avatarUrl => throw _privateConstructorUsedError;
  int get status => throw _privateConstructorUsedError;
  DateTime get createdAt => throw _privateConstructorUsedError;
  DateTime get updatedAt => throw _privateConstructorUsedError;
  bool get isOwner => throw _privateConstructorUsedError;
  String? get memberRole => throw _privateConstructorUsedError;

  /// Serializes this ProfileResponseModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of ProfileResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $ProfileResponseModelCopyWith<ProfileResponseModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ProfileResponseModelCopyWith<$Res> {
  factory $ProfileResponseModelCopyWith(
    ProfileResponseModel value,
    $Res Function(ProfileResponseModel) then,
  ) = _$ProfileResponseModelCopyWithImpl<$Res, ProfileResponseModel>;
  @useResult
  $Res call({
    String id,
    String userId,
    String name,
    int profileType,
    String? subscriptionId,
    String? companyName,
    String? bio,
    String? avatarUrl,
    int status,
    DateTime createdAt,
    DateTime updatedAt,
    bool isOwner,
    String? memberRole,
  });
}

/// @nodoc
class _$ProfileResponseModelCopyWithImpl<
  $Res,
  $Val extends ProfileResponseModel
>
    implements $ProfileResponseModelCopyWith<$Res> {
  _$ProfileResponseModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of ProfileResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? userId = null,
    Object? name = null,
    Object? profileType = null,
    Object? subscriptionId = freezed,
    Object? companyName = freezed,
    Object? bio = freezed,
    Object? avatarUrl = freezed,
    Object? status = null,
    Object? createdAt = null,
    Object? updatedAt = null,
    Object? isOwner = null,
    Object? memberRole = freezed,
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
            profileType: null == profileType
                ? _value.profileType
                : profileType // ignore: cast_nullable_to_non_nullable
                      as int,
            subscriptionId: freezed == subscriptionId
                ? _value.subscriptionId
                : subscriptionId // ignore: cast_nullable_to_non_nullable
                      as String?,
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
            status: null == status
                ? _value.status
                : status // ignore: cast_nullable_to_non_nullable
                      as int,
            createdAt: null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
            updatedAt: null == updatedAt
                ? _value.updatedAt
                : updatedAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
            isOwner: null == isOwner
                ? _value.isOwner
                : isOwner // ignore: cast_nullable_to_non_nullable
                      as bool,
            memberRole: freezed == memberRole
                ? _value.memberRole
                : memberRole // ignore: cast_nullable_to_non_nullable
                      as String?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$ProfileResponseModelImplCopyWith<$Res>
    implements $ProfileResponseModelCopyWith<$Res> {
  factory _$$ProfileResponseModelImplCopyWith(
    _$ProfileResponseModelImpl value,
    $Res Function(_$ProfileResponseModelImpl) then,
  ) = __$$ProfileResponseModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String id,
    String userId,
    String name,
    int profileType,
    String? subscriptionId,
    String? companyName,
    String? bio,
    String? avatarUrl,
    int status,
    DateTime createdAt,
    DateTime updatedAt,
    bool isOwner,
    String? memberRole,
  });
}

/// @nodoc
class __$$ProfileResponseModelImplCopyWithImpl<$Res>
    extends _$ProfileResponseModelCopyWithImpl<$Res, _$ProfileResponseModelImpl>
    implements _$$ProfileResponseModelImplCopyWith<$Res> {
  __$$ProfileResponseModelImplCopyWithImpl(
    _$ProfileResponseModelImpl _value,
    $Res Function(_$ProfileResponseModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of ProfileResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? userId = null,
    Object? name = null,
    Object? profileType = null,
    Object? subscriptionId = freezed,
    Object? companyName = freezed,
    Object? bio = freezed,
    Object? avatarUrl = freezed,
    Object? status = null,
    Object? createdAt = null,
    Object? updatedAt = null,
    Object? isOwner = null,
    Object? memberRole = freezed,
  }) {
    return _then(
      _$ProfileResponseModelImpl(
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
        profileType: null == profileType
            ? _value.profileType
            : profileType // ignore: cast_nullable_to_non_nullable
                  as int,
        subscriptionId: freezed == subscriptionId
            ? _value.subscriptionId
            : subscriptionId // ignore: cast_nullable_to_non_nullable
                  as String?,
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
        status: null == status
            ? _value.status
            : status // ignore: cast_nullable_to_non_nullable
                  as int,
        createdAt: null == createdAt
            ? _value.createdAt
            : createdAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
        updatedAt: null == updatedAt
            ? _value.updatedAt
            : updatedAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
        isOwner: null == isOwner
            ? _value.isOwner
            : isOwner // ignore: cast_nullable_to_non_nullable
                  as bool,
        memberRole: freezed == memberRole
            ? _value.memberRole
            : memberRole // ignore: cast_nullable_to_non_nullable
                  as String?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$ProfileResponseModelImpl implements _ProfileResponseModel {
  const _$ProfileResponseModelImpl({
    required this.id,
    required this.userId,
    required this.name,
    required this.profileType,
    this.subscriptionId,
    this.companyName,
    this.bio,
    this.avatarUrl,
    required this.status,
    required this.createdAt,
    required this.updatedAt,
    required this.isOwner,
    this.memberRole,
  });

  factory _$ProfileResponseModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$ProfileResponseModelImplFromJson(json);

  @override
  final String id;
  @override
  final String userId;
  @override
  final String name;
  @override
  final int profileType;
  @override
  final String? subscriptionId;
  @override
  final String? companyName;
  @override
  final String? bio;
  @override
  final String? avatarUrl;
  @override
  final int status;
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;
  @override
  final bool isOwner;
  @override
  final String? memberRole;

  @override
  String toString() {
    return 'ProfileResponseModel(id: $id, userId: $userId, name: $name, profileType: $profileType, subscriptionId: $subscriptionId, companyName: $companyName, bio: $bio, avatarUrl: $avatarUrl, status: $status, createdAt: $createdAt, updatedAt: $updatedAt, isOwner: $isOwner, memberRole: $memberRole)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ProfileResponseModelImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.userId, userId) || other.userId == userId) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.profileType, profileType) ||
                other.profileType == profileType) &&
            (identical(other.subscriptionId, subscriptionId) ||
                other.subscriptionId == subscriptionId) &&
            (identical(other.companyName, companyName) ||
                other.companyName == companyName) &&
            (identical(other.bio, bio) || other.bio == bio) &&
            (identical(other.avatarUrl, avatarUrl) ||
                other.avatarUrl == avatarUrl) &&
            (identical(other.status, status) || other.status == status) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt) &&
            (identical(other.updatedAt, updatedAt) ||
                other.updatedAt == updatedAt) &&
            (identical(other.isOwner, isOwner) || other.isOwner == isOwner) &&
            (identical(other.memberRole, memberRole) ||
                other.memberRole == memberRole));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    userId,
    name,
    profileType,
    subscriptionId,
    companyName,
    bio,
    avatarUrl,
    status,
    createdAt,
    updatedAt,
    isOwner,
    memberRole,
  );

  /// Create a copy of ProfileResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$ProfileResponseModelImplCopyWith<_$ProfileResponseModelImpl>
  get copyWith =>
      __$$ProfileResponseModelImplCopyWithImpl<_$ProfileResponseModelImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$ProfileResponseModelImplToJson(this);
  }
}

abstract class _ProfileResponseModel implements ProfileResponseModel {
  const factory _ProfileResponseModel({
    required final String id,
    required final String userId,
    required final String name,
    required final int profileType,
    final String? subscriptionId,
    final String? companyName,
    final String? bio,
    final String? avatarUrl,
    required final int status,
    required final DateTime createdAt,
    required final DateTime updatedAt,
    required final bool isOwner,
    final String? memberRole,
  }) = _$ProfileResponseModelImpl;

  factory _ProfileResponseModel.fromJson(Map<String, dynamic> json) =
      _$ProfileResponseModelImpl.fromJson;

  @override
  String get id;
  @override
  String get userId;
  @override
  String get name;
  @override
  int get profileType;
  @override
  String? get subscriptionId;
  @override
  String? get companyName;
  @override
  String? get bio;
  @override
  String? get avatarUrl;
  @override
  int get status;
  @override
  DateTime get createdAt;
  @override
  DateTime get updatedAt;
  @override
  bool get isOwner;
  @override
  String? get memberRole;

  /// Create a copy of ProfileResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$ProfileResponseModelImplCopyWith<_$ProfileResponseModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}
