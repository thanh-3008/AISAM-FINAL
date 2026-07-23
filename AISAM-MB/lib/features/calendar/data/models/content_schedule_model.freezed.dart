// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'content_schedule_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

ContentScheduleModel _$ContentScheduleModelFromJson(Map<String, dynamic> json) {
  return _ContentScheduleModel.fromJson(json);
}

/// @nodoc
mixin _$ContentScheduleModel {
  String get id => throw _privateConstructorUsedError;
  String get profileId => throw _privateConstructorUsedError;
  String get contentId => throw _privateConstructorUsedError;
  String get integrationId => throw _privateConstructorUsedError;
  DateTime get scheduledAt => throw _privateConstructorUsedError;
  DateTime? get executedAt => throw _privateConstructorUsedError;
  ScheduleStatusEnum get status => throw _privateConstructorUsedError;
  int get attemptCount => throw _privateConstructorUsedError;
  String? get lastError => throw _privateConstructorUsedError;
  String? get title => throw _privateConstructorUsedError;
  String? get brandName => throw _privateConstructorUsedError;
  String? get type => throw _privateConstructorUsedError;
  String? get platform => throw _privateConstructorUsedError;

  /// Serializes this ContentScheduleModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of ContentScheduleModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $ContentScheduleModelCopyWith<ContentScheduleModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ContentScheduleModelCopyWith<$Res> {
  factory $ContentScheduleModelCopyWith(
    ContentScheduleModel value,
    $Res Function(ContentScheduleModel) then,
  ) = _$ContentScheduleModelCopyWithImpl<$Res, ContentScheduleModel>;
  @useResult
  $Res call({
    String id,
    String profileId,
    String contentId,
    String integrationId,
    DateTime scheduledAt,
    DateTime? executedAt,
    ScheduleStatusEnum status,
    int attemptCount,
    String? lastError,
    String? title,
    String? brandName,
    String? type,
    String? platform,
  });
}

/// @nodoc
class _$ContentScheduleModelCopyWithImpl<
  $Res,
  $Val extends ContentScheduleModel
>
    implements $ContentScheduleModelCopyWith<$Res> {
  _$ContentScheduleModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of ContentScheduleModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? profileId = null,
    Object? contentId = null,
    Object? integrationId = null,
    Object? scheduledAt = null,
    Object? executedAt = freezed,
    Object? status = null,
    Object? attemptCount = null,
    Object? lastError = freezed,
    Object? title = freezed,
    Object? brandName = freezed,
    Object? type = freezed,
    Object? platform = freezed,
  }) {
    return _then(
      _value.copyWith(
            id: null == id
                ? _value.id
                : id // ignore: cast_nullable_to_non_nullable
                      as String,
            profileId: null == profileId
                ? _value.profileId
                : profileId // ignore: cast_nullable_to_non_nullable
                      as String,
            contentId: null == contentId
                ? _value.contentId
                : contentId // ignore: cast_nullable_to_non_nullable
                      as String,
            integrationId: null == integrationId
                ? _value.integrationId
                : integrationId // ignore: cast_nullable_to_non_nullable
                      as String,
            scheduledAt: null == scheduledAt
                ? _value.scheduledAt
                : scheduledAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
            executedAt: freezed == executedAt
                ? _value.executedAt
                : executedAt // ignore: cast_nullable_to_non_nullable
                      as DateTime?,
            status: null == status
                ? _value.status
                : status // ignore: cast_nullable_to_non_nullable
                      as ScheduleStatusEnum,
            attemptCount: null == attemptCount
                ? _value.attemptCount
                : attemptCount // ignore: cast_nullable_to_non_nullable
                      as int,
            lastError: freezed == lastError
                ? _value.lastError
                : lastError // ignore: cast_nullable_to_non_nullable
                      as String?,
            title: freezed == title
                ? _value.title
                : title // ignore: cast_nullable_to_non_nullable
                      as String?,
            brandName: freezed == brandName
                ? _value.brandName
                : brandName // ignore: cast_nullable_to_non_nullable
                      as String?,
            type: freezed == type
                ? _value.type
                : type // ignore: cast_nullable_to_non_nullable
                      as String?,
            platform: freezed == platform
                ? _value.platform
                : platform // ignore: cast_nullable_to_non_nullable
                      as String?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$ContentScheduleModelImplCopyWith<$Res>
    implements $ContentScheduleModelCopyWith<$Res> {
  factory _$$ContentScheduleModelImplCopyWith(
    _$ContentScheduleModelImpl value,
    $Res Function(_$ContentScheduleModelImpl) then,
  ) = __$$ContentScheduleModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String id,
    String profileId,
    String contentId,
    String integrationId,
    DateTime scheduledAt,
    DateTime? executedAt,
    ScheduleStatusEnum status,
    int attemptCount,
    String? lastError,
    String? title,
    String? brandName,
    String? type,
    String? platform,
  });
}

/// @nodoc
class __$$ContentScheduleModelImplCopyWithImpl<$Res>
    extends _$ContentScheduleModelCopyWithImpl<$Res, _$ContentScheduleModelImpl>
    implements _$$ContentScheduleModelImplCopyWith<$Res> {
  __$$ContentScheduleModelImplCopyWithImpl(
    _$ContentScheduleModelImpl _value,
    $Res Function(_$ContentScheduleModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of ContentScheduleModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? profileId = null,
    Object? contentId = null,
    Object? integrationId = null,
    Object? scheduledAt = null,
    Object? executedAt = freezed,
    Object? status = null,
    Object? attemptCount = null,
    Object? lastError = freezed,
    Object? title = freezed,
    Object? brandName = freezed,
    Object? type = freezed,
    Object? platform = freezed,
  }) {
    return _then(
      _$ContentScheduleModelImpl(
        id: null == id
            ? _value.id
            : id // ignore: cast_nullable_to_non_nullable
                  as String,
        profileId: null == profileId
            ? _value.profileId
            : profileId // ignore: cast_nullable_to_non_nullable
                  as String,
        contentId: null == contentId
            ? _value.contentId
            : contentId // ignore: cast_nullable_to_non_nullable
                  as String,
        integrationId: null == integrationId
            ? _value.integrationId
            : integrationId // ignore: cast_nullable_to_non_nullable
                  as String,
        scheduledAt: null == scheduledAt
            ? _value.scheduledAt
            : scheduledAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
        executedAt: freezed == executedAt
            ? _value.executedAt
            : executedAt // ignore: cast_nullable_to_non_nullable
                  as DateTime?,
        status: null == status
            ? _value.status
            : status // ignore: cast_nullable_to_non_nullable
                  as ScheduleStatusEnum,
        attemptCount: null == attemptCount
            ? _value.attemptCount
            : attemptCount // ignore: cast_nullable_to_non_nullable
                  as int,
        lastError: freezed == lastError
            ? _value.lastError
            : lastError // ignore: cast_nullable_to_non_nullable
                  as String?,
        title: freezed == title
            ? _value.title
            : title // ignore: cast_nullable_to_non_nullable
                  as String?,
        brandName: freezed == brandName
            ? _value.brandName
            : brandName // ignore: cast_nullable_to_non_nullable
                  as String?,
        type: freezed == type
            ? _value.type
            : type // ignore: cast_nullable_to_non_nullable
                  as String?,
        platform: freezed == platform
            ? _value.platform
            : platform // ignore: cast_nullable_to_non_nullable
                  as String?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$ContentScheduleModelImpl implements _ContentScheduleModel {
  const _$ContentScheduleModelImpl({
    required this.id,
    required this.profileId,
    required this.contentId,
    required this.integrationId,
    required this.scheduledAt,
    this.executedAt,
    required this.status,
    this.attemptCount = 0,
    this.lastError,
    this.title,
    this.brandName,
    this.type,
    this.platform,
  });

  factory _$ContentScheduleModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$ContentScheduleModelImplFromJson(json);

  @override
  final String id;
  @override
  final String profileId;
  @override
  final String contentId;
  @override
  final String integrationId;
  @override
  final DateTime scheduledAt;
  @override
  final DateTime? executedAt;
  @override
  final ScheduleStatusEnum status;
  @override
  @JsonKey()
  final int attemptCount;
  @override
  final String? lastError;
  @override
  final String? title;
  @override
  final String? brandName;
  @override
  final String? type;
  @override
  final String? platform;

  @override
  String toString() {
    return 'ContentScheduleModel(id: $id, profileId: $profileId, contentId: $contentId, integrationId: $integrationId, scheduledAt: $scheduledAt, executedAt: $executedAt, status: $status, attemptCount: $attemptCount, lastError: $lastError, title: $title, brandName: $brandName, type: $type, platform: $platform)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ContentScheduleModelImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.profileId, profileId) ||
                other.profileId == profileId) &&
            (identical(other.contentId, contentId) ||
                other.contentId == contentId) &&
            (identical(other.integrationId, integrationId) ||
                other.integrationId == integrationId) &&
            (identical(other.scheduledAt, scheduledAt) ||
                other.scheduledAt == scheduledAt) &&
            (identical(other.executedAt, executedAt) ||
                other.executedAt == executedAt) &&
            (identical(other.status, status) || other.status == status) &&
            (identical(other.attemptCount, attemptCount) ||
                other.attemptCount == attemptCount) &&
            (identical(other.lastError, lastError) ||
                other.lastError == lastError) &&
            (identical(other.title, title) || other.title == title) &&
            (identical(other.brandName, brandName) ||
                other.brandName == brandName) &&
            (identical(other.type, type) || other.type == type) &&
            (identical(other.platform, platform) ||
                other.platform == platform));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    profileId,
    contentId,
    integrationId,
    scheduledAt,
    executedAt,
    status,
    attemptCount,
    lastError,
    title,
    brandName,
    type,
    platform,
  );

  /// Create a copy of ContentScheduleModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$ContentScheduleModelImplCopyWith<_$ContentScheduleModelImpl>
  get copyWith =>
      __$$ContentScheduleModelImplCopyWithImpl<_$ContentScheduleModelImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$ContentScheduleModelImplToJson(this);
  }
}

abstract class _ContentScheduleModel implements ContentScheduleModel {
  const factory _ContentScheduleModel({
    required final String id,
    required final String profileId,
    required final String contentId,
    required final String integrationId,
    required final DateTime scheduledAt,
    final DateTime? executedAt,
    required final ScheduleStatusEnum status,
    final int attemptCount,
    final String? lastError,
    final String? title,
    final String? brandName,
    final String? type,
    final String? platform,
  }) = _$ContentScheduleModelImpl;

  factory _ContentScheduleModel.fromJson(Map<String, dynamic> json) =
      _$ContentScheduleModelImpl.fromJson;

  @override
  String get id;
  @override
  String get profileId;
  @override
  String get contentId;
  @override
  String get integrationId;
  @override
  DateTime get scheduledAt;
  @override
  DateTime? get executedAt;
  @override
  ScheduleStatusEnum get status;
  @override
  int get attemptCount;
  @override
  String? get lastError;
  @override
  String? get title;
  @override
  String? get brandName;
  @override
  String? get type;
  @override
  String? get platform;

  /// Create a copy of ContentScheduleModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$ContentScheduleModelImplCopyWith<_$ContentScheduleModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}
