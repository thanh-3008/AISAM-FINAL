// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'notification_filter_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

/// @nodoc
mixin _$NotificationFilterModel {
  int? get type => throw _privateConstructorUsedError;
  DateTime? get fromDate => throw _privateConstructorUsedError;
  DateTime? get toDate => throw _privateConstructorUsedError;

  /// Create a copy of NotificationFilterModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $NotificationFilterModelCopyWith<NotificationFilterModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $NotificationFilterModelCopyWith<$Res> {
  factory $NotificationFilterModelCopyWith(
    NotificationFilterModel value,
    $Res Function(NotificationFilterModel) then,
  ) = _$NotificationFilterModelCopyWithImpl<$Res, NotificationFilterModel>;
  @useResult
  $Res call({int? type, DateTime? fromDate, DateTime? toDate});
}

/// @nodoc
class _$NotificationFilterModelCopyWithImpl<
  $Res,
  $Val extends NotificationFilterModel
>
    implements $NotificationFilterModelCopyWith<$Res> {
  _$NotificationFilterModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of NotificationFilterModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? type = freezed,
    Object? fromDate = freezed,
    Object? toDate = freezed,
  }) {
    return _then(
      _value.copyWith(
            type: freezed == type
                ? _value.type
                : type // ignore: cast_nullable_to_non_nullable
                      as int?,
            fromDate: freezed == fromDate
                ? _value.fromDate
                : fromDate // ignore: cast_nullable_to_non_nullable
                      as DateTime?,
            toDate: freezed == toDate
                ? _value.toDate
                : toDate // ignore: cast_nullable_to_non_nullable
                      as DateTime?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$NotificationFilterModelImplCopyWith<$Res>
    implements $NotificationFilterModelCopyWith<$Res> {
  factory _$$NotificationFilterModelImplCopyWith(
    _$NotificationFilterModelImpl value,
    $Res Function(_$NotificationFilterModelImpl) then,
  ) = __$$NotificationFilterModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int? type, DateTime? fromDate, DateTime? toDate});
}

/// @nodoc
class __$$NotificationFilterModelImplCopyWithImpl<$Res>
    extends
        _$NotificationFilterModelCopyWithImpl<
          $Res,
          _$NotificationFilterModelImpl
        >
    implements _$$NotificationFilterModelImplCopyWith<$Res> {
  __$$NotificationFilterModelImplCopyWithImpl(
    _$NotificationFilterModelImpl _value,
    $Res Function(_$NotificationFilterModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of NotificationFilterModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? type = freezed,
    Object? fromDate = freezed,
    Object? toDate = freezed,
  }) {
    return _then(
      _$NotificationFilterModelImpl(
        type: freezed == type
            ? _value.type
            : type // ignore: cast_nullable_to_non_nullable
                  as int?,
        fromDate: freezed == fromDate
            ? _value.fromDate
            : fromDate // ignore: cast_nullable_to_non_nullable
                  as DateTime?,
        toDate: freezed == toDate
            ? _value.toDate
            : toDate // ignore: cast_nullable_to_non_nullable
                  as DateTime?,
      ),
    );
  }
}

/// @nodoc

class _$NotificationFilterModelImpl implements _NotificationFilterModel {
  const _$NotificationFilterModelImpl({this.type, this.fromDate, this.toDate});

  @override
  final int? type;
  @override
  final DateTime? fromDate;
  @override
  final DateTime? toDate;

  @override
  String toString() {
    return 'NotificationFilterModel(type: $type, fromDate: $fromDate, toDate: $toDate)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$NotificationFilterModelImpl &&
            (identical(other.type, type) || other.type == type) &&
            (identical(other.fromDate, fromDate) ||
                other.fromDate == fromDate) &&
            (identical(other.toDate, toDate) || other.toDate == toDate));
  }

  @override
  int get hashCode => Object.hash(runtimeType, type, fromDate, toDate);

  /// Create a copy of NotificationFilterModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$NotificationFilterModelImplCopyWith<_$NotificationFilterModelImpl>
  get copyWith =>
      __$$NotificationFilterModelImplCopyWithImpl<
        _$NotificationFilterModelImpl
      >(this, _$identity);
}

abstract class _NotificationFilterModel implements NotificationFilterModel {
  const factory _NotificationFilterModel({
    final int? type,
    final DateTime? fromDate,
    final DateTime? toDate,
  }) = _$NotificationFilterModelImpl;

  @override
  int? get type;
  @override
  DateTime? get fromDate;
  @override
  DateTime? get toDate;

  /// Create a copy of NotificationFilterModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$NotificationFilterModelImplCopyWith<_$NotificationFilterModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}
