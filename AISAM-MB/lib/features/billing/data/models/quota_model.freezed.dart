// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'quota_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

QuotaModel _$QuotaModelFromJson(Map<String, dynamic> json) {
  return _QuotaModel.fromJson(json);
}

/// @nodoc
mixin _$QuotaModel {
  String get planName => throw _privateConstructorUsedError;
  String get subscriptionStatus => throw _privateConstructorUsedError;
  DateTime? get windowStart => throw _privateConstructorUsedError;
  DateTime? get windowEnd => throw _privateConstructorUsedError;
  int get promptQuotaLimit => throw _privateConstructorUsedError;
  int get promptUsage => throw _privateConstructorUsedError;
  int get promptRemaining => throw _privateConstructorUsedError;
  int get postQuotaLimit => throw _privateConstructorUsedError;
  int get postUsage => throw _privateConstructorUsedError;
  int get postRemaining => throw _privateConstructorUsedError;
  int get textContentCount => throw _privateConstructorUsedError;
  int get imageContentCount => throw _privateConstructorUsedError;
  int get videoContentCount => throw _privateConstructorUsedError;
  int get creditBalance => throw _privateConstructorUsedError;
  int get creditsUsed => throw _privateConstructorUsedError;
  int get maxBalanceCap => throw _privateConstructorUsedError;
  int? get memberCreditLimit => throw _privateConstructorUsedError;
  int get memberCreditUsed => throw _privateConstructorUsedError;
  int? get memberQuotaMode => throw _privateConstructorUsedError;

  /// Serializes this QuotaModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of QuotaModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $QuotaModelCopyWith<QuotaModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $QuotaModelCopyWith<$Res> {
  factory $QuotaModelCopyWith(
    QuotaModel value,
    $Res Function(QuotaModel) then,
  ) = _$QuotaModelCopyWithImpl<$Res, QuotaModel>;
  @useResult
  $Res call({
    String planName,
    String subscriptionStatus,
    DateTime? windowStart,
    DateTime? windowEnd,
    int promptQuotaLimit,
    int promptUsage,
    int promptRemaining,
    int postQuotaLimit,
    int postUsage,
    int postRemaining,
    int textContentCount,
    int imageContentCount,
    int videoContentCount,
    int creditBalance,
    int creditsUsed,
    int maxBalanceCap,
    int? memberCreditLimit,
    int memberCreditUsed,
    int? memberQuotaMode,
  });
}

/// @nodoc
class _$QuotaModelCopyWithImpl<$Res, $Val extends QuotaModel>
    implements $QuotaModelCopyWith<$Res> {
  _$QuotaModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of QuotaModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? planName = null,
    Object? subscriptionStatus = null,
    Object? windowStart = freezed,
    Object? windowEnd = freezed,
    Object? promptQuotaLimit = null,
    Object? promptUsage = null,
    Object? promptRemaining = null,
    Object? postQuotaLimit = null,
    Object? postUsage = null,
    Object? postRemaining = null,
    Object? textContentCount = null,
    Object? imageContentCount = null,
    Object? videoContentCount = null,
    Object? creditBalance = null,
    Object? creditsUsed = null,
    Object? maxBalanceCap = null,
    Object? memberCreditLimit = freezed,
    Object? memberCreditUsed = null,
    Object? memberQuotaMode = freezed,
  }) {
    return _then(
      _value.copyWith(
            planName: null == planName
                ? _value.planName
                : planName // ignore: cast_nullable_to_non_nullable
                      as String,
            subscriptionStatus: null == subscriptionStatus
                ? _value.subscriptionStatus
                : subscriptionStatus // ignore: cast_nullable_to_non_nullable
                      as String,
            windowStart: freezed == windowStart
                ? _value.windowStart
                : windowStart // ignore: cast_nullable_to_non_nullable
                      as DateTime?,
            windowEnd: freezed == windowEnd
                ? _value.windowEnd
                : windowEnd // ignore: cast_nullable_to_non_nullable
                      as DateTime?,
            promptQuotaLimit: null == promptQuotaLimit
                ? _value.promptQuotaLimit
                : promptQuotaLimit // ignore: cast_nullable_to_non_nullable
                      as int,
            promptUsage: null == promptUsage
                ? _value.promptUsage
                : promptUsage // ignore: cast_nullable_to_non_nullable
                      as int,
            promptRemaining: null == promptRemaining
                ? _value.promptRemaining
                : promptRemaining // ignore: cast_nullable_to_non_nullable
                      as int,
            postQuotaLimit: null == postQuotaLimit
                ? _value.postQuotaLimit
                : postQuotaLimit // ignore: cast_nullable_to_non_nullable
                      as int,
            postUsage: null == postUsage
                ? _value.postUsage
                : postUsage // ignore: cast_nullable_to_non_nullable
                      as int,
            postRemaining: null == postRemaining
                ? _value.postRemaining
                : postRemaining // ignore: cast_nullable_to_non_nullable
                      as int,
            textContentCount: null == textContentCount
                ? _value.textContentCount
                : textContentCount // ignore: cast_nullable_to_non_nullable
                      as int,
            imageContentCount: null == imageContentCount
                ? _value.imageContentCount
                : imageContentCount // ignore: cast_nullable_to_non_nullable
                      as int,
            videoContentCount: null == videoContentCount
                ? _value.videoContentCount
                : videoContentCount // ignore: cast_nullable_to_non_nullable
                      as int,
            creditBalance: null == creditBalance
                ? _value.creditBalance
                : creditBalance // ignore: cast_nullable_to_non_nullable
                      as int,
            creditsUsed: null == creditsUsed
                ? _value.creditsUsed
                : creditsUsed // ignore: cast_nullable_to_non_nullable
                      as int,
            maxBalanceCap: null == maxBalanceCap
                ? _value.maxBalanceCap
                : maxBalanceCap // ignore: cast_nullable_to_non_nullable
                      as int,
            memberCreditLimit: freezed == memberCreditLimit
                ? _value.memberCreditLimit
                : memberCreditLimit // ignore: cast_nullable_to_non_nullable
                      as int?,
            memberCreditUsed: null == memberCreditUsed
                ? _value.memberCreditUsed
                : memberCreditUsed // ignore: cast_nullable_to_non_nullable
                      as int,
            memberQuotaMode: freezed == memberQuotaMode
                ? _value.memberQuotaMode
                : memberQuotaMode // ignore: cast_nullable_to_non_nullable
                      as int?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$QuotaModelImplCopyWith<$Res>
    implements $QuotaModelCopyWith<$Res> {
  factory _$$QuotaModelImplCopyWith(
    _$QuotaModelImpl value,
    $Res Function(_$QuotaModelImpl) then,
  ) = __$$QuotaModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String planName,
    String subscriptionStatus,
    DateTime? windowStart,
    DateTime? windowEnd,
    int promptQuotaLimit,
    int promptUsage,
    int promptRemaining,
    int postQuotaLimit,
    int postUsage,
    int postRemaining,
    int textContentCount,
    int imageContentCount,
    int videoContentCount,
    int creditBalance,
    int creditsUsed,
    int maxBalanceCap,
    int? memberCreditLimit,
    int memberCreditUsed,
    int? memberQuotaMode,
  });
}

/// @nodoc
class __$$QuotaModelImplCopyWithImpl<$Res>
    extends _$QuotaModelCopyWithImpl<$Res, _$QuotaModelImpl>
    implements _$$QuotaModelImplCopyWith<$Res> {
  __$$QuotaModelImplCopyWithImpl(
    _$QuotaModelImpl _value,
    $Res Function(_$QuotaModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of QuotaModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? planName = null,
    Object? subscriptionStatus = null,
    Object? windowStart = freezed,
    Object? windowEnd = freezed,
    Object? promptQuotaLimit = null,
    Object? promptUsage = null,
    Object? promptRemaining = null,
    Object? postQuotaLimit = null,
    Object? postUsage = null,
    Object? postRemaining = null,
    Object? textContentCount = null,
    Object? imageContentCount = null,
    Object? videoContentCount = null,
    Object? creditBalance = null,
    Object? creditsUsed = null,
    Object? maxBalanceCap = null,
    Object? memberCreditLimit = freezed,
    Object? memberCreditUsed = null,
    Object? memberQuotaMode = freezed,
  }) {
    return _then(
      _$QuotaModelImpl(
        planName: null == planName
            ? _value.planName
            : planName // ignore: cast_nullable_to_non_nullable
                  as String,
        subscriptionStatus: null == subscriptionStatus
            ? _value.subscriptionStatus
            : subscriptionStatus // ignore: cast_nullable_to_non_nullable
                  as String,
        windowStart: freezed == windowStart
            ? _value.windowStart
            : windowStart // ignore: cast_nullable_to_non_nullable
                  as DateTime?,
        windowEnd: freezed == windowEnd
            ? _value.windowEnd
            : windowEnd // ignore: cast_nullable_to_non_nullable
                  as DateTime?,
        promptQuotaLimit: null == promptQuotaLimit
            ? _value.promptQuotaLimit
            : promptQuotaLimit // ignore: cast_nullable_to_non_nullable
                  as int,
        promptUsage: null == promptUsage
            ? _value.promptUsage
            : promptUsage // ignore: cast_nullable_to_non_nullable
                  as int,
        promptRemaining: null == promptRemaining
            ? _value.promptRemaining
            : promptRemaining // ignore: cast_nullable_to_non_nullable
                  as int,
        postQuotaLimit: null == postQuotaLimit
            ? _value.postQuotaLimit
            : postQuotaLimit // ignore: cast_nullable_to_non_nullable
                  as int,
        postUsage: null == postUsage
            ? _value.postUsage
            : postUsage // ignore: cast_nullable_to_non_nullable
                  as int,
        postRemaining: null == postRemaining
            ? _value.postRemaining
            : postRemaining // ignore: cast_nullable_to_non_nullable
                  as int,
        textContentCount: null == textContentCount
            ? _value.textContentCount
            : textContentCount // ignore: cast_nullable_to_non_nullable
                  as int,
        imageContentCount: null == imageContentCount
            ? _value.imageContentCount
            : imageContentCount // ignore: cast_nullable_to_non_nullable
                  as int,
        videoContentCount: null == videoContentCount
            ? _value.videoContentCount
            : videoContentCount // ignore: cast_nullable_to_non_nullable
                  as int,
        creditBalance: null == creditBalance
            ? _value.creditBalance
            : creditBalance // ignore: cast_nullable_to_non_nullable
                  as int,
        creditsUsed: null == creditsUsed
            ? _value.creditsUsed
            : creditsUsed // ignore: cast_nullable_to_non_nullable
                  as int,
        maxBalanceCap: null == maxBalanceCap
            ? _value.maxBalanceCap
            : maxBalanceCap // ignore: cast_nullable_to_non_nullable
                  as int,
        memberCreditLimit: freezed == memberCreditLimit
            ? _value.memberCreditLimit
            : memberCreditLimit // ignore: cast_nullable_to_non_nullable
                  as int?,
        memberCreditUsed: null == memberCreditUsed
            ? _value.memberCreditUsed
            : memberCreditUsed // ignore: cast_nullable_to_non_nullable
                  as int,
        memberQuotaMode: freezed == memberQuotaMode
            ? _value.memberQuotaMode
            : memberQuotaMode // ignore: cast_nullable_to_non_nullable
                  as int?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$QuotaModelImpl implements _QuotaModel {
  const _$QuotaModelImpl({
    this.planName = '',
    this.subscriptionStatus = '',
    this.windowStart,
    this.windowEnd,
    this.promptQuotaLimit = 0,
    this.promptUsage = 0,
    this.promptRemaining = 0,
    this.postQuotaLimit = 0,
    this.postUsage = 0,
    this.postRemaining = 0,
    this.textContentCount = 0,
    this.imageContentCount = 0,
    this.videoContentCount = 0,
    this.creditBalance = 0,
    this.creditsUsed = 0,
    this.maxBalanceCap = 0,
    this.memberCreditLimit,
    this.memberCreditUsed = 0,
    this.memberQuotaMode,
  });

  factory _$QuotaModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$QuotaModelImplFromJson(json);

  @override
  @JsonKey()
  final String planName;
  @override
  @JsonKey()
  final String subscriptionStatus;
  @override
  final DateTime? windowStart;
  @override
  final DateTime? windowEnd;
  @override
  @JsonKey()
  final int promptQuotaLimit;
  @override
  @JsonKey()
  final int promptUsage;
  @override
  @JsonKey()
  final int promptRemaining;
  @override
  @JsonKey()
  final int postQuotaLimit;
  @override
  @JsonKey()
  final int postUsage;
  @override
  @JsonKey()
  final int postRemaining;
  @override
  @JsonKey()
  final int textContentCount;
  @override
  @JsonKey()
  final int imageContentCount;
  @override
  @JsonKey()
  final int videoContentCount;
  @override
  @JsonKey()
  final int creditBalance;
  @override
  @JsonKey()
  final int creditsUsed;
  @override
  @JsonKey()
  final int maxBalanceCap;
  @override
  final int? memberCreditLimit;
  @override
  @JsonKey()
  final int memberCreditUsed;
  @override
  final int? memberQuotaMode;

  @override
  String toString() {
    return 'QuotaModel(planName: $planName, subscriptionStatus: $subscriptionStatus, windowStart: $windowStart, windowEnd: $windowEnd, promptQuotaLimit: $promptQuotaLimit, promptUsage: $promptUsage, promptRemaining: $promptRemaining, postQuotaLimit: $postQuotaLimit, postUsage: $postUsage, postRemaining: $postRemaining, textContentCount: $textContentCount, imageContentCount: $imageContentCount, videoContentCount: $videoContentCount, creditBalance: $creditBalance, creditsUsed: $creditsUsed, maxBalanceCap: $maxBalanceCap, memberCreditLimit: $memberCreditLimit, memberCreditUsed: $memberCreditUsed, memberQuotaMode: $memberQuotaMode)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$QuotaModelImpl &&
            (identical(other.planName, planName) ||
                other.planName == planName) &&
            (identical(other.subscriptionStatus, subscriptionStatus) ||
                other.subscriptionStatus == subscriptionStatus) &&
            (identical(other.windowStart, windowStart) ||
                other.windowStart == windowStart) &&
            (identical(other.windowEnd, windowEnd) ||
                other.windowEnd == windowEnd) &&
            (identical(other.promptQuotaLimit, promptQuotaLimit) ||
                other.promptQuotaLimit == promptQuotaLimit) &&
            (identical(other.promptUsage, promptUsage) ||
                other.promptUsage == promptUsage) &&
            (identical(other.promptRemaining, promptRemaining) ||
                other.promptRemaining == promptRemaining) &&
            (identical(other.postQuotaLimit, postQuotaLimit) ||
                other.postQuotaLimit == postQuotaLimit) &&
            (identical(other.postUsage, postUsage) ||
                other.postUsage == postUsage) &&
            (identical(other.postRemaining, postRemaining) ||
                other.postRemaining == postRemaining) &&
            (identical(other.textContentCount, textContentCount) ||
                other.textContentCount == textContentCount) &&
            (identical(other.imageContentCount, imageContentCount) ||
                other.imageContentCount == imageContentCount) &&
            (identical(other.videoContentCount, videoContentCount) ||
                other.videoContentCount == videoContentCount) &&
            (identical(other.creditBalance, creditBalance) ||
                other.creditBalance == creditBalance) &&
            (identical(other.creditsUsed, creditsUsed) ||
                other.creditsUsed == creditsUsed) &&
            (identical(other.maxBalanceCap, maxBalanceCap) ||
                other.maxBalanceCap == maxBalanceCap) &&
            (identical(other.memberCreditLimit, memberCreditLimit) ||
                other.memberCreditLimit == memberCreditLimit) &&
            (identical(other.memberCreditUsed, memberCreditUsed) ||
                other.memberCreditUsed == memberCreditUsed) &&
            (identical(other.memberQuotaMode, memberQuotaMode) ||
                other.memberQuotaMode == memberQuotaMode));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hashAll([
    runtimeType,
    planName,
    subscriptionStatus,
    windowStart,
    windowEnd,
    promptQuotaLimit,
    promptUsage,
    promptRemaining,
    postQuotaLimit,
    postUsage,
    postRemaining,
    textContentCount,
    imageContentCount,
    videoContentCount,
    creditBalance,
    creditsUsed,
    maxBalanceCap,
    memberCreditLimit,
    memberCreditUsed,
    memberQuotaMode,
  ]);

  /// Create a copy of QuotaModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$QuotaModelImplCopyWith<_$QuotaModelImpl> get copyWith =>
      __$$QuotaModelImplCopyWithImpl<_$QuotaModelImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$QuotaModelImplToJson(this);
  }
}

abstract class _QuotaModel implements QuotaModel {
  const factory _QuotaModel({
    final String planName,
    final String subscriptionStatus,
    final DateTime? windowStart,
    final DateTime? windowEnd,
    final int promptQuotaLimit,
    final int promptUsage,
    final int promptRemaining,
    final int postQuotaLimit,
    final int postUsage,
    final int postRemaining,
    final int textContentCount,
    final int imageContentCount,
    final int videoContentCount,
    final int creditBalance,
    final int creditsUsed,
    final int maxBalanceCap,
    final int? memberCreditLimit,
    final int memberCreditUsed,
    final int? memberQuotaMode,
  }) = _$QuotaModelImpl;

  factory _QuotaModel.fromJson(Map<String, dynamic> json) =
      _$QuotaModelImpl.fromJson;

  @override
  String get planName;
  @override
  String get subscriptionStatus;
  @override
  DateTime? get windowStart;
  @override
  DateTime? get windowEnd;
  @override
  int get promptQuotaLimit;
  @override
  int get promptUsage;
  @override
  int get promptRemaining;
  @override
  int get postQuotaLimit;
  @override
  int get postUsage;
  @override
  int get postRemaining;
  @override
  int get textContentCount;
  @override
  int get imageContentCount;
  @override
  int get videoContentCount;
  @override
  int get creditBalance;
  @override
  int get creditsUsed;
  @override
  int get maxBalanceCap;
  @override
  int? get memberCreditLimit;
  @override
  int get memberCreditUsed;
  @override
  int? get memberQuotaMode;

  /// Create a copy of QuotaModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$QuotaModelImplCopyWith<_$QuotaModelImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
