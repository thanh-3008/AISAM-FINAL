// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'update_schedule_request.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

UpdateScheduleRequest _$UpdateScheduleRequestFromJson(
  Map<String, dynamic> json,
) {
  return _UpdateScheduleRequest.fromJson(json);
}

/// @nodoc
mixin _$UpdateScheduleRequest {
  String? get integrationId => throw _privateConstructorUsedError;
  DateTime? get scheduledAt => throw _privateConstructorUsedError;

  /// Serializes this UpdateScheduleRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of UpdateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $UpdateScheduleRequestCopyWith<UpdateScheduleRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $UpdateScheduleRequestCopyWith<$Res> {
  factory $UpdateScheduleRequestCopyWith(
    UpdateScheduleRequest value,
    $Res Function(UpdateScheduleRequest) then,
  ) = _$UpdateScheduleRequestCopyWithImpl<$Res, UpdateScheduleRequest>;
  @useResult
  $Res call({String? integrationId, DateTime? scheduledAt});
}

/// @nodoc
class _$UpdateScheduleRequestCopyWithImpl<
  $Res,
  $Val extends UpdateScheduleRequest
>
    implements $UpdateScheduleRequestCopyWith<$Res> {
  _$UpdateScheduleRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of UpdateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? integrationId = freezed, Object? scheduledAt = freezed}) {
    return _then(
      _value.copyWith(
            integrationId: freezed == integrationId
                ? _value.integrationId
                : integrationId // ignore: cast_nullable_to_non_nullable
                      as String?,
            scheduledAt: freezed == scheduledAt
                ? _value.scheduledAt
                : scheduledAt // ignore: cast_nullable_to_non_nullable
                      as DateTime?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$UpdateScheduleRequestImplCopyWith<$Res>
    implements $UpdateScheduleRequestCopyWith<$Res> {
  factory _$$UpdateScheduleRequestImplCopyWith(
    _$UpdateScheduleRequestImpl value,
    $Res Function(_$UpdateScheduleRequestImpl) then,
  ) = __$$UpdateScheduleRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String? integrationId, DateTime? scheduledAt});
}

/// @nodoc
class __$$UpdateScheduleRequestImplCopyWithImpl<$Res>
    extends
        _$UpdateScheduleRequestCopyWithImpl<$Res, _$UpdateScheduleRequestImpl>
    implements _$$UpdateScheduleRequestImplCopyWith<$Res> {
  __$$UpdateScheduleRequestImplCopyWithImpl(
    _$UpdateScheduleRequestImpl _value,
    $Res Function(_$UpdateScheduleRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of UpdateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? integrationId = freezed, Object? scheduledAt = freezed}) {
    return _then(
      _$UpdateScheduleRequestImpl(
        integrationId: freezed == integrationId
            ? _value.integrationId
            : integrationId // ignore: cast_nullable_to_non_nullable
                  as String?,
        scheduledAt: freezed == scheduledAt
            ? _value.scheduledAt
            : scheduledAt // ignore: cast_nullable_to_non_nullable
                  as DateTime?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$UpdateScheduleRequestImpl implements _UpdateScheduleRequest {
  const _$UpdateScheduleRequestImpl({this.integrationId, this.scheduledAt});

  factory _$UpdateScheduleRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$UpdateScheduleRequestImplFromJson(json);

  @override
  final String? integrationId;
  @override
  final DateTime? scheduledAt;

  @override
  String toString() {
    return 'UpdateScheduleRequest(integrationId: $integrationId, scheduledAt: $scheduledAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$UpdateScheduleRequestImpl &&
            (identical(other.integrationId, integrationId) ||
                other.integrationId == integrationId) &&
            (identical(other.scheduledAt, scheduledAt) ||
                other.scheduledAt == scheduledAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, integrationId, scheduledAt);

  /// Create a copy of UpdateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$UpdateScheduleRequestImplCopyWith<_$UpdateScheduleRequestImpl>
  get copyWith =>
      __$$UpdateScheduleRequestImplCopyWithImpl<_$UpdateScheduleRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$UpdateScheduleRequestImplToJson(this);
  }
}

abstract class _UpdateScheduleRequest implements UpdateScheduleRequest {
  const factory _UpdateScheduleRequest({
    final String? integrationId,
    final DateTime? scheduledAt,
  }) = _$UpdateScheduleRequestImpl;

  factory _UpdateScheduleRequest.fromJson(Map<String, dynamic> json) =
      _$UpdateScheduleRequestImpl.fromJson;

  @override
  String? get integrationId;
  @override
  DateTime? get scheduledAt;

  /// Create a copy of UpdateScheduleRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$UpdateScheduleRequestImplCopyWith<_$UpdateScheduleRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}
