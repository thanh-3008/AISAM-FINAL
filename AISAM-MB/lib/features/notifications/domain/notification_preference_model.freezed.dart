// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'notification_preference_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

NotificationPreferenceModel _$NotificationPreferenceModelFromJson(
  Map<String, dynamic> json,
) {
  return _NotificationPreferenceModel.fromJson(json);
}

/// @nodoc
mixin _$NotificationPreferenceModel {
  int get notificationType => throw _privateConstructorUsedError;
  bool get isEnabled => throw _privateConstructorUsedError;

  /// Serializes this NotificationPreferenceModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of NotificationPreferenceModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $NotificationPreferenceModelCopyWith<NotificationPreferenceModel>
  get copyWith => throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $NotificationPreferenceModelCopyWith<$Res> {
  factory $NotificationPreferenceModelCopyWith(
    NotificationPreferenceModel value,
    $Res Function(NotificationPreferenceModel) then,
  ) =
      _$NotificationPreferenceModelCopyWithImpl<
        $Res,
        NotificationPreferenceModel
      >;
  @useResult
  $Res call({int notificationType, bool isEnabled});
}

/// @nodoc
class _$NotificationPreferenceModelCopyWithImpl<
  $Res,
  $Val extends NotificationPreferenceModel
>
    implements $NotificationPreferenceModelCopyWith<$Res> {
  _$NotificationPreferenceModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of NotificationPreferenceModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? notificationType = null, Object? isEnabled = null}) {
    return _then(
      _value.copyWith(
            notificationType: null == notificationType
                ? _value.notificationType
                : notificationType // ignore: cast_nullable_to_non_nullable
                      as int,
            isEnabled: null == isEnabled
                ? _value.isEnabled
                : isEnabled // ignore: cast_nullable_to_non_nullable
                      as bool,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$NotificationPreferenceModelImplCopyWith<$Res>
    implements $NotificationPreferenceModelCopyWith<$Res> {
  factory _$$NotificationPreferenceModelImplCopyWith(
    _$NotificationPreferenceModelImpl value,
    $Res Function(_$NotificationPreferenceModelImpl) then,
  ) = __$$NotificationPreferenceModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int notificationType, bool isEnabled});
}

/// @nodoc
class __$$NotificationPreferenceModelImplCopyWithImpl<$Res>
    extends
        _$NotificationPreferenceModelCopyWithImpl<
          $Res,
          _$NotificationPreferenceModelImpl
        >
    implements _$$NotificationPreferenceModelImplCopyWith<$Res> {
  __$$NotificationPreferenceModelImplCopyWithImpl(
    _$NotificationPreferenceModelImpl _value,
    $Res Function(_$NotificationPreferenceModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of NotificationPreferenceModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({Object? notificationType = null, Object? isEnabled = null}) {
    return _then(
      _$NotificationPreferenceModelImpl(
        notificationType: null == notificationType
            ? _value.notificationType
            : notificationType // ignore: cast_nullable_to_non_nullable
                  as int,
        isEnabled: null == isEnabled
            ? _value.isEnabled
            : isEnabled // ignore: cast_nullable_to_non_nullable
                  as bool,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$NotificationPreferenceModelImpl
    implements _NotificationPreferenceModel {
  const _$NotificationPreferenceModelImpl({
    required this.notificationType,
    this.isEnabled = true,
  });

  factory _$NotificationPreferenceModelImpl.fromJson(
    Map<String, dynamic> json,
  ) => _$$NotificationPreferenceModelImplFromJson(json);

  @override
  final int notificationType;
  @override
  @JsonKey()
  final bool isEnabled;

  @override
  String toString() {
    return 'NotificationPreferenceModel(notificationType: $notificationType, isEnabled: $isEnabled)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$NotificationPreferenceModelImpl &&
            (identical(other.notificationType, notificationType) ||
                other.notificationType == notificationType) &&
            (identical(other.isEnabled, isEnabled) ||
                other.isEnabled == isEnabled));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, notificationType, isEnabled);

  /// Create a copy of NotificationPreferenceModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$NotificationPreferenceModelImplCopyWith<_$NotificationPreferenceModelImpl>
  get copyWith =>
      __$$NotificationPreferenceModelImplCopyWithImpl<
        _$NotificationPreferenceModelImpl
      >(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$NotificationPreferenceModelImplToJson(this);
  }
}

abstract class _NotificationPreferenceModel
    implements NotificationPreferenceModel {
  const factory _NotificationPreferenceModel({
    required final int notificationType,
    final bool isEnabled,
  }) = _$NotificationPreferenceModelImpl;

  factory _NotificationPreferenceModel.fromJson(Map<String, dynamic> json) =
      _$NotificationPreferenceModelImpl.fromJson;

  @override
  int get notificationType;
  @override
  bool get isEnabled;

  /// Create a copy of NotificationPreferenceModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$NotificationPreferenceModelImplCopyWith<_$NotificationPreferenceModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}
