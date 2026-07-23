// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'create_schedule_request.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CreateScheduleRequest _$CreateScheduleRequestFromJson(
  Map<String, dynamic> json,
) {
  return _CreateScheduleRequest.fromJson(json);
}

/// @nodoc
mixin _$CreateScheduleRequest {
  String get contentId => throw _privateConstructorUsedError;
  String get integrationId => throw _privateConstructorUsedError;
  DateTime get scheduledAt => throw _privateConstructorUsedError;

  /// Serializes this CreateScheduleRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CreateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CreateScheduleRequestCopyWith<CreateScheduleRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CreateScheduleRequestCopyWith<$Res> {
  factory $CreateScheduleRequestCopyWith(
    CreateScheduleRequest value,
    $Res Function(CreateScheduleRequest) then,
  ) = _$CreateScheduleRequestCopyWithImpl<$Res, CreateScheduleRequest>;
  @useResult
  $Res call({String contentId, String integrationId, DateTime scheduledAt});
}

/// @nodoc
class _$CreateScheduleRequestCopyWithImpl<
  $Res,
  $Val extends CreateScheduleRequest
>
    implements $CreateScheduleRequestCopyWith<$Res> {
  _$CreateScheduleRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CreateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? contentId = null,
    Object? integrationId = null,
    Object? scheduledAt = null,
  }) {
    return _then(
      _value.copyWith(
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
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CreateScheduleRequestImplCopyWith<$Res>
    implements $CreateScheduleRequestCopyWith<$Res> {
  factory _$$CreateScheduleRequestImplCopyWith(
    _$CreateScheduleRequestImpl value,
    $Res Function(_$CreateScheduleRequestImpl) then,
  ) = __$$CreateScheduleRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String contentId, String integrationId, DateTime scheduledAt});
}

/// @nodoc
class __$$CreateScheduleRequestImplCopyWithImpl<$Res>
    extends
        _$CreateScheduleRequestCopyWithImpl<$Res, _$CreateScheduleRequestImpl>
    implements _$$CreateScheduleRequestImplCopyWith<$Res> {
  __$$CreateScheduleRequestImplCopyWithImpl(
    _$CreateScheduleRequestImpl _value,
    $Res Function(_$CreateScheduleRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CreateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? contentId = null,
    Object? integrationId = null,
    Object? scheduledAt = null,
  }) {
    return _then(
      _$CreateScheduleRequestImpl(
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
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CreateScheduleRequestImpl implements _CreateScheduleRequest {
  const _$CreateScheduleRequestImpl({
    required this.contentId,
    required this.integrationId,
    required this.scheduledAt,
  });

  factory _$CreateScheduleRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$CreateScheduleRequestImplFromJson(json);

  @override
  final String contentId;
  @override
  final String integrationId;
  @override
  final DateTime scheduledAt;

  @override
  String toString() {
    return 'CreateScheduleRequest(contentId: $contentId, integrationId: $integrationId, scheduledAt: $scheduledAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CreateScheduleRequestImpl &&
            (identical(other.contentId, contentId) ||
                other.contentId == contentId) &&
            (identical(other.integrationId, integrationId) ||
                other.integrationId == integrationId) &&
            (identical(other.scheduledAt, scheduledAt) ||
                other.scheduledAt == scheduledAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, contentId, integrationId, scheduledAt);

  /// Create a copy of CreateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CreateScheduleRequestImplCopyWith<_$CreateScheduleRequestImpl>
  get copyWith =>
      __$$CreateScheduleRequestImplCopyWithImpl<_$CreateScheduleRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$CreateScheduleRequestImplToJson(this);
  }
}

abstract class _CreateScheduleRequest implements CreateScheduleRequest {
  const factory _CreateScheduleRequest({
    required final String contentId,
    required final String integrationId,
    required final DateTime scheduledAt,
  }) = _$CreateScheduleRequestImpl;

  factory _CreateScheduleRequest.fromJson(Map<String, dynamic> json) =
      _$CreateScheduleRequestImpl.fromJson;

  @override
  String get contentId;
  @override
  String get integrationId;
  @override
  DateTime get scheduledAt;

  /// Create a copy of CreateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CreateScheduleRequestImplCopyWith<_$CreateScheduleRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}
