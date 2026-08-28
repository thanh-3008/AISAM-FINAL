// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'dashboard_summary.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CombinedDashboardSummary _$CombinedDashboardSummaryFromJson(
  Map<String, dynamic> json,
) {
  return _CombinedDashboardSummary.fromJson(json);
}

/// @nodoc
mixin _$CombinedDashboardSummary {
  // Basic Fields (Always available)
  int get draftContentCount => throw _privateConstructorUsedError;
  int get publishedContentCount => throw _privateConstructorUsedError;
  int get pendingApprovalContentCount => throw _privateConstructorUsedError;
  int get upcomingScheduleCount => throw _privateConstructorUsedError;
  int get failedScheduleCount => throw _privateConstructorUsedError;
  int get activeSocialIntegrationCount => throw _privateConstructorUsedError;
  int get publishedPostCount => throw _privateConstructorUsedError;
  int get unreadNotificationCount =>
      throw _privateConstructorUsedError; // Advanced Fields (Nullable, available only for Paid plans)
  String? get workspaceId => throw _privateConstructorUsedError;
  int? get creditBalance => throw _privateConstructorUsedError;
  int? get creditsUsed => throw _privateConstructorUsedError;
  int? get postQuotaLimit => throw _privateConstructorUsedError;
  int? get postsRemaining => throw _privateConstructorUsedError;
  int? get aiUsageCount => throw _privateConstructorUsedError;
  int? get activeMemberCount => throw _privateConstructorUsedError;
  List<WorkspaceTopMemberDto>? get topMembers =>
      throw _privateConstructorUsedError;

  /// Serializes this CombinedDashboardSummary to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CombinedDashboardSummary
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CombinedDashboardSummaryCopyWith<CombinedDashboardSummary> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CombinedDashboardSummaryCopyWith<$Res> {
  factory $CombinedDashboardSummaryCopyWith(
    CombinedDashboardSummary value,
    $Res Function(CombinedDashboardSummary) then,
  ) = _$CombinedDashboardSummaryCopyWithImpl<$Res, CombinedDashboardSummary>;
  @useResult
  $Res call({
    int draftContentCount,
    int publishedContentCount,
    int pendingApprovalContentCount,
    int upcomingScheduleCount,
    int failedScheduleCount,
    int activeSocialIntegrationCount,
    int publishedPostCount,
    int unreadNotificationCount,
    String? workspaceId,
    int? creditBalance,
    int? creditsUsed,
    int? postQuotaLimit,
    int? postsRemaining,
    int? aiUsageCount,
    int? activeMemberCount,
    List<WorkspaceTopMemberDto>? topMembers,
  });
}

/// @nodoc
class _$CombinedDashboardSummaryCopyWithImpl<
  $Res,
  $Val extends CombinedDashboardSummary
>
    implements $CombinedDashboardSummaryCopyWith<$Res> {
  _$CombinedDashboardSummaryCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CombinedDashboardSummary
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? draftContentCount = null,
    Object? publishedContentCount = null,
    Object? pendingApprovalContentCount = null,
    Object? upcomingScheduleCount = null,
    Object? failedScheduleCount = null,
    Object? activeSocialIntegrationCount = null,
    Object? publishedPostCount = null,
    Object? unreadNotificationCount = null,
    Object? workspaceId = freezed,
    Object? creditBalance = freezed,
    Object? creditsUsed = freezed,
    Object? postQuotaLimit = freezed,
    Object? postsRemaining = freezed,
    Object? aiUsageCount = freezed,
    Object? activeMemberCount = freezed,
    Object? topMembers = freezed,
  }) {
    return _then(
      _value.copyWith(
            draftContentCount: null == draftContentCount
                ? _value.draftContentCount
                : draftContentCount // ignore: cast_nullable_to_non_nullable
                      as int,
            publishedContentCount: null == publishedContentCount
                ? _value.publishedContentCount
                : publishedContentCount // ignore: cast_nullable_to_non_nullable
                      as int,
            pendingApprovalContentCount: null == pendingApprovalContentCount
                ? _value.pendingApprovalContentCount
                : pendingApprovalContentCount // ignore: cast_nullable_to_non_nullable
                      as int,
            upcomingScheduleCount: null == upcomingScheduleCount
                ? _value.upcomingScheduleCount
                : upcomingScheduleCount // ignore: cast_nullable_to_non_nullable
                      as int,
            failedScheduleCount: null == failedScheduleCount
                ? _value.failedScheduleCount
                : failedScheduleCount // ignore: cast_nullable_to_non_nullable
                      as int,
            activeSocialIntegrationCount: null == activeSocialIntegrationCount
                ? _value.activeSocialIntegrationCount
                : activeSocialIntegrationCount // ignore: cast_nullable_to_non_nullable
                      as int,
            publishedPostCount: null == publishedPostCount
                ? _value.publishedPostCount
                : publishedPostCount // ignore: cast_nullable_to_non_nullable
                      as int,
            unreadNotificationCount: null == unreadNotificationCount
                ? _value.unreadNotificationCount
                : unreadNotificationCount // ignore: cast_nullable_to_non_nullable
                      as int,
            workspaceId: freezed == workspaceId
                ? _value.workspaceId
                : workspaceId // ignore: cast_nullable_to_non_nullable
                      as String?,
            creditBalance: freezed == creditBalance
                ? _value.creditBalance
                : creditBalance // ignore: cast_nullable_to_non_nullable
                      as int?,
            creditsUsed: freezed == creditsUsed
                ? _value.creditsUsed
                : creditsUsed // ignore: cast_nullable_to_non_nullable
                      as int?,
            postQuotaLimit: freezed == postQuotaLimit
                ? _value.postQuotaLimit
                : postQuotaLimit // ignore: cast_nullable_to_non_nullable
                      as int?,
            postsRemaining: freezed == postsRemaining
                ? _value.postsRemaining
                : postsRemaining // ignore: cast_nullable_to_non_nullable
                      as int?,
            aiUsageCount: freezed == aiUsageCount
                ? _value.aiUsageCount
                : aiUsageCount // ignore: cast_nullable_to_non_nullable
                      as int?,
            activeMemberCount: freezed == activeMemberCount
                ? _value.activeMemberCount
                : activeMemberCount // ignore: cast_nullable_to_non_nullable
                      as int?,
            topMembers: freezed == topMembers
                ? _value.topMembers
                : topMembers // ignore: cast_nullable_to_non_nullable
                      as List<WorkspaceTopMemberDto>?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CombinedDashboardSummaryImplCopyWith<$Res>
    implements $CombinedDashboardSummaryCopyWith<$Res> {
  factory _$$CombinedDashboardSummaryImplCopyWith(
    _$CombinedDashboardSummaryImpl value,
    $Res Function(_$CombinedDashboardSummaryImpl) then,
  ) = __$$CombinedDashboardSummaryImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    int draftContentCount,
    int publishedContentCount,
    int pendingApprovalContentCount,
    int upcomingScheduleCount,
    int failedScheduleCount,
    int activeSocialIntegrationCount,
    int publishedPostCount,
    int unreadNotificationCount,
    String? workspaceId,
    int? creditBalance,
    int? creditsUsed,
    int? postQuotaLimit,
    int? postsRemaining,
    int? aiUsageCount,
    int? activeMemberCount,
    List<WorkspaceTopMemberDto>? topMembers,
  });
}

/// @nodoc
class __$$CombinedDashboardSummaryImplCopyWithImpl<$Res>
    extends
        _$CombinedDashboardSummaryCopyWithImpl<
          $Res,
          _$CombinedDashboardSummaryImpl
        >
    implements _$$CombinedDashboardSummaryImplCopyWith<$Res> {
  __$$CombinedDashboardSummaryImplCopyWithImpl(
    _$CombinedDashboardSummaryImpl _value,
    $Res Function(_$CombinedDashboardSummaryImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CombinedDashboardSummary
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? draftContentCount = null,
    Object? publishedContentCount = null,
    Object? pendingApprovalContentCount = null,
    Object? upcomingScheduleCount = null,
    Object? failedScheduleCount = null,
    Object? activeSocialIntegrationCount = null,
    Object? publishedPostCount = null,
    Object? unreadNotificationCount = null,
    Object? workspaceId = freezed,
    Object? creditBalance = freezed,
    Object? creditsUsed = freezed,
    Object? postQuotaLimit = freezed,
    Object? postsRemaining = freezed,
    Object? aiUsageCount = freezed,
    Object? activeMemberCount = freezed,
    Object? topMembers = freezed,
  }) {
    return _then(
      _$CombinedDashboardSummaryImpl(
        draftContentCount: null == draftContentCount
            ? _value.draftContentCount
            : draftContentCount // ignore: cast_nullable_to_non_nullable
                  as int,
        publishedContentCount: null == publishedContentCount
            ? _value.publishedContentCount
            : publishedContentCount // ignore: cast_nullable_to_non_nullable
                  as int,
        pendingApprovalContentCount: null == pendingApprovalContentCount
            ? _value.pendingApprovalContentCount
            : pendingApprovalContentCount // ignore: cast_nullable_to_non_nullable
                  as int,
        upcomingScheduleCount: null == upcomingScheduleCount
            ? _value.upcomingScheduleCount
            : upcomingScheduleCount // ignore: cast_nullable_to_non_nullable
                  as int,
        failedScheduleCount: null == failedScheduleCount
            ? _value.failedScheduleCount
            : failedScheduleCount // ignore: cast_nullable_to_non_nullable
                  as int,
        activeSocialIntegrationCount: null == activeSocialIntegrationCount
            ? _value.activeSocialIntegrationCount
            : activeSocialIntegrationCount // ignore: cast_nullable_to_non_nullable
                  as int,
        publishedPostCount: null == publishedPostCount
            ? _value.publishedPostCount
            : publishedPostCount // ignore: cast_nullable_to_non_nullable
                  as int,
        unreadNotificationCount: null == unreadNotificationCount
            ? _value.unreadNotificationCount
            : unreadNotificationCount // ignore: cast_nullable_to_non_nullable
                  as int,
        workspaceId: freezed == workspaceId
            ? _value.workspaceId
            : workspaceId // ignore: cast_nullable_to_non_nullable
                  as String?,
        creditBalance: freezed == creditBalance
            ? _value.creditBalance
            : creditBalance // ignore: cast_nullable_to_non_nullable
                  as int?,
        creditsUsed: freezed == creditsUsed
            ? _value.creditsUsed
            : creditsUsed // ignore: cast_nullable_to_non_nullable
                  as int?,
        postQuotaLimit: freezed == postQuotaLimit
            ? _value.postQuotaLimit
            : postQuotaLimit // ignore: cast_nullable_to_non_nullable
                  as int?,
        postsRemaining: freezed == postsRemaining
            ? _value.postsRemaining
            : postsRemaining // ignore: cast_nullable_to_non_nullable
                  as int?,
        aiUsageCount: freezed == aiUsageCount
            ? _value.aiUsageCount
            : aiUsageCount // ignore: cast_nullable_to_non_nullable
                  as int?,
        activeMemberCount: freezed == activeMemberCount
            ? _value.activeMemberCount
            : activeMemberCount // ignore: cast_nullable_to_non_nullable
                  as int?,
        topMembers: freezed == topMembers
            ? _value._topMembers
            : topMembers // ignore: cast_nullable_to_non_nullable
                  as List<WorkspaceTopMemberDto>?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CombinedDashboardSummaryImpl implements _CombinedDashboardSummary {
  const _$CombinedDashboardSummaryImpl({
    this.draftContentCount = 0,
    this.publishedContentCount = 0,
    this.pendingApprovalContentCount = 0,
    this.upcomingScheduleCount = 0,
    this.failedScheduleCount = 0,
    this.activeSocialIntegrationCount = 0,
    this.publishedPostCount = 0,
    this.unreadNotificationCount = 0,
    this.workspaceId,
    this.creditBalance,
    this.creditsUsed,
    this.postQuotaLimit,
    this.postsRemaining,
    this.aiUsageCount,
    this.activeMemberCount,
    final List<WorkspaceTopMemberDto>? topMembers,
  }) : _topMembers = topMembers;

  factory _$CombinedDashboardSummaryImpl.fromJson(Map<String, dynamic> json) =>
      _$$CombinedDashboardSummaryImplFromJson(json);

  // Basic Fields (Always available)
  @override
  @JsonKey()
  final int draftContentCount;
  @override
  @JsonKey()
  final int publishedContentCount;
  @override
  @JsonKey()
  final int pendingApprovalContentCount;
  @override
  @JsonKey()
  final int upcomingScheduleCount;
  @override
  @JsonKey()
  final int failedScheduleCount;
  @override
  @JsonKey()
  final int activeSocialIntegrationCount;
  @override
  @JsonKey()
  final int publishedPostCount;
  @override
  @JsonKey()
  final int unreadNotificationCount;
  // Advanced Fields (Nullable, available only for Paid plans)
  @override
  final String? workspaceId;
  @override
  final int? creditBalance;
  @override
  final int? creditsUsed;
  @override
  final int? postQuotaLimit;
  @override
  final int? postsRemaining;
  @override
  final int? aiUsageCount;
  @override
  final int? activeMemberCount;
  final List<WorkspaceTopMemberDto>? _topMembers;
  @override
  List<WorkspaceTopMemberDto>? get topMembers {
    final value = _topMembers;
    if (value == null) return null;
    if (_topMembers is EqualUnmodifiableListView) return _topMembers;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  @override
  String toString() {
    return 'CombinedDashboardSummary(draftContentCount: $draftContentCount, publishedContentCount: $publishedContentCount, pendingApprovalContentCount: $pendingApprovalContentCount, upcomingScheduleCount: $upcomingScheduleCount, failedScheduleCount: $failedScheduleCount, activeSocialIntegrationCount: $activeSocialIntegrationCount, publishedPostCount: $publishedPostCount, unreadNotificationCount: $unreadNotificationCount, workspaceId: $workspaceId, creditBalance: $creditBalance, creditsUsed: $creditsUsed, postQuotaLimit: $postQuotaLimit, postsRemaining: $postsRemaining, aiUsageCount: $aiUsageCount, activeMemberCount: $activeMemberCount, topMembers: $topMembers)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CombinedDashboardSummaryImpl &&
            (identical(other.draftContentCount, draftContentCount) ||
                other.draftContentCount == draftContentCount) &&
            (identical(other.publishedContentCount, publishedContentCount) ||
                other.publishedContentCount == publishedContentCount) &&
            (identical(
                  other.pendingApprovalContentCount,
                  pendingApprovalContentCount,
                ) ||
                other.pendingApprovalContentCount ==
                    pendingApprovalContentCount) &&
            (identical(other.upcomingScheduleCount, upcomingScheduleCount) ||
                other.upcomingScheduleCount == upcomingScheduleCount) &&
            (identical(other.failedScheduleCount, failedScheduleCount) ||
                other.failedScheduleCount == failedScheduleCount) &&
            (identical(
                  other.activeSocialIntegrationCount,
                  activeSocialIntegrationCount,
                ) ||
                other.activeSocialIntegrationCount ==
                    activeSocialIntegrationCount) &&
            (identical(other.publishedPostCount, publishedPostCount) ||
                other.publishedPostCount == publishedPostCount) &&
            (identical(
                  other.unreadNotificationCount,
                  unreadNotificationCount,
                ) ||
                other.unreadNotificationCount == unreadNotificationCount) &&
            (identical(other.workspaceId, workspaceId) ||
                other.workspaceId == workspaceId) &&
            (identical(other.creditBalance, creditBalance) ||
                other.creditBalance == creditBalance) &&
            (identical(other.creditsUsed, creditsUsed) ||
                other.creditsUsed == creditsUsed) &&
            (identical(other.postQuotaLimit, postQuotaLimit) ||
                other.postQuotaLimit == postQuotaLimit) &&
            (identical(other.postsRemaining, postsRemaining) ||
                other.postsRemaining == postsRemaining) &&
            (identical(other.aiUsageCount, aiUsageCount) ||
                other.aiUsageCount == aiUsageCount) &&
            (identical(other.activeMemberCount, activeMemberCount) ||
                other.activeMemberCount == activeMemberCount) &&
            const DeepCollectionEquality().equals(
              other._topMembers,
              _topMembers,
            ));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    draftContentCount,
    publishedContentCount,
    pendingApprovalContentCount,
    upcomingScheduleCount,
    failedScheduleCount,
    activeSocialIntegrationCount,
    publishedPostCount,
    unreadNotificationCount,
    workspaceId,
    creditBalance,
    creditsUsed,
    postQuotaLimit,
    postsRemaining,
    aiUsageCount,
    activeMemberCount,
    const DeepCollectionEquality().hash(_topMembers),
  );

  /// Create a copy of CombinedDashboardSummary
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CombinedDashboardSummaryImplCopyWith<_$CombinedDashboardSummaryImpl>
  get copyWith =>
      __$$CombinedDashboardSummaryImplCopyWithImpl<
        _$CombinedDashboardSummaryImpl
      >(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$CombinedDashboardSummaryImplToJson(this);
  }
}

abstract class _CombinedDashboardSummary implements CombinedDashboardSummary {
  const factory _CombinedDashboardSummary({
    final int draftContentCount,
    final int publishedContentCount,
    final int pendingApprovalContentCount,
    final int upcomingScheduleCount,
    final int failedScheduleCount,
    final int activeSocialIntegrationCount,
    final int publishedPostCount,
    final int unreadNotificationCount,
    final String? workspaceId,
    final int? creditBalance,
    final int? creditsUsed,
    final int? postQuotaLimit,
    final int? postsRemaining,
    final int? aiUsageCount,
    final int? activeMemberCount,
    final List<WorkspaceTopMemberDto>? topMembers,
  }) = _$CombinedDashboardSummaryImpl;

  factory _CombinedDashboardSummary.fromJson(Map<String, dynamic> json) =
      _$CombinedDashboardSummaryImpl.fromJson;

  // Basic Fields (Always available)
  @override
  int get draftContentCount;
  @override
  int get publishedContentCount;
  @override
  int get pendingApprovalContentCount;
  @override
  int get upcomingScheduleCount;
  @override
  int get failedScheduleCount;
  @override
  int get activeSocialIntegrationCount;
  @override
  int get publishedPostCount;
  @override
  int get unreadNotificationCount; // Advanced Fields (Nullable, available only for Paid plans)
  @override
  String? get workspaceId;
  @override
  int? get creditBalance;
  @override
  int? get creditsUsed;
  @override
  int? get postQuotaLimit;
  @override
  int? get postsRemaining;
  @override
  int? get aiUsageCount;
  @override
  int? get activeMemberCount;
  @override
  List<WorkspaceTopMemberDto>? get topMembers;

  /// Create a copy of CombinedDashboardSummary
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CombinedDashboardSummaryImplCopyWith<_$CombinedDashboardSummaryImpl>
  get copyWith => throw _privateConstructorUsedError;
}

WorkspaceTopMemberDto _$WorkspaceTopMemberDtoFromJson(
  Map<String, dynamic> json,
) {
  return _WorkspaceTopMemberDto.fromJson(json);
}

/// @nodoc
mixin _$WorkspaceTopMemberDto {
  String get userId => throw _privateConstructorUsedError;
  String get name => throw _privateConstructorUsedError;
  String get email => throw _privateConstructorUsedError;
  int get creditsUsed => throw _privateConstructorUsedError;
  int get aiUsageCount => throw _privateConstructorUsedError;

  /// Serializes this WorkspaceTopMemberDto to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of WorkspaceTopMemberDto
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $WorkspaceTopMemberDtoCopyWith<WorkspaceTopMemberDto> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $WorkspaceTopMemberDtoCopyWith<$Res> {
  factory $WorkspaceTopMemberDtoCopyWith(
    WorkspaceTopMemberDto value,
    $Res Function(WorkspaceTopMemberDto) then,
  ) = _$WorkspaceTopMemberDtoCopyWithImpl<$Res, WorkspaceTopMemberDto>;
  @useResult
  $Res call({
    String userId,
    String name,
    String email,
    int creditsUsed,
    int aiUsageCount,
  });
}

/// @nodoc
class _$WorkspaceTopMemberDtoCopyWithImpl<
  $Res,
  $Val extends WorkspaceTopMemberDto
>
    implements $WorkspaceTopMemberDtoCopyWith<$Res> {
  _$WorkspaceTopMemberDtoCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of WorkspaceTopMemberDto
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? userId = null,
    Object? name = null,
    Object? email = null,
    Object? creditsUsed = null,
    Object? aiUsageCount = null,
  }) {
    return _then(
      _value.copyWith(
            userId: null == userId
                ? _value.userId
                : userId // ignore: cast_nullable_to_non_nullable
                      as String,
            name: null == name
                ? _value.name
                : name // ignore: cast_nullable_to_non_nullable
                      as String,
            email: null == email
                ? _value.email
                : email // ignore: cast_nullable_to_non_nullable
                      as String,
            creditsUsed: null == creditsUsed
                ? _value.creditsUsed
                : creditsUsed // ignore: cast_nullable_to_non_nullable
                      as int,
            aiUsageCount: null == aiUsageCount
                ? _value.aiUsageCount
                : aiUsageCount // ignore: cast_nullable_to_non_nullable
                      as int,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$WorkspaceTopMemberDtoImplCopyWith<$Res>
    implements $WorkspaceTopMemberDtoCopyWith<$Res> {
  factory _$$WorkspaceTopMemberDtoImplCopyWith(
    _$WorkspaceTopMemberDtoImpl value,
    $Res Function(_$WorkspaceTopMemberDtoImpl) then,
  ) = __$$WorkspaceTopMemberDtoImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String userId,
    String name,
    String email,
    int creditsUsed,
    int aiUsageCount,
  });
}

/// @nodoc
class __$$WorkspaceTopMemberDtoImplCopyWithImpl<$Res>
    extends
        _$WorkspaceTopMemberDtoCopyWithImpl<$Res, _$WorkspaceTopMemberDtoImpl>
    implements _$$WorkspaceTopMemberDtoImplCopyWith<$Res> {
  __$$WorkspaceTopMemberDtoImplCopyWithImpl(
    _$WorkspaceTopMemberDtoImpl _value,
    $Res Function(_$WorkspaceTopMemberDtoImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of WorkspaceTopMemberDto
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? userId = null,
    Object? name = null,
    Object? email = null,
    Object? creditsUsed = null,
    Object? aiUsageCount = null,
  }) {
    return _then(
      _$WorkspaceTopMemberDtoImpl(
        userId: null == userId
            ? _value.userId
            : userId // ignore: cast_nullable_to_non_nullable
                  as String,
        name: null == name
            ? _value.name
            : name // ignore: cast_nullable_to_non_nullable
                  as String,
        email: null == email
            ? _value.email
            : email // ignore: cast_nullable_to_non_nullable
                  as String,
        creditsUsed: null == creditsUsed
            ? _value.creditsUsed
            : creditsUsed // ignore: cast_nullable_to_non_nullable
                  as int,
        aiUsageCount: null == aiUsageCount
            ? _value.aiUsageCount
            : aiUsageCount // ignore: cast_nullable_to_non_nullable
                  as int,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$WorkspaceTopMemberDtoImpl implements _WorkspaceTopMemberDto {
  const _$WorkspaceTopMemberDtoImpl({
    required this.userId,
    this.name = '',
    this.email = '',
    this.creditsUsed = 0,
    this.aiUsageCount = 0,
  });

  factory _$WorkspaceTopMemberDtoImpl.fromJson(Map<String, dynamic> json) =>
      _$$WorkspaceTopMemberDtoImplFromJson(json);

  @override
  final String userId;
  @override
  @JsonKey()
  final String name;
  @override
  @JsonKey()
  final String email;
  @override
  @JsonKey()
  final int creditsUsed;
  @override
  @JsonKey()
  final int aiUsageCount;

  @override
  String toString() {
    return 'WorkspaceTopMemberDto(userId: $userId, name: $name, email: $email, creditsUsed: $creditsUsed, aiUsageCount: $aiUsageCount)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$WorkspaceTopMemberDtoImpl &&
            (identical(other.userId, userId) || other.userId == userId) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.email, email) || other.email == email) &&
            (identical(other.creditsUsed, creditsUsed) ||
                other.creditsUsed == creditsUsed) &&
            (identical(other.aiUsageCount, aiUsageCount) ||
                other.aiUsageCount == aiUsageCount));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, userId, name, email, creditsUsed, aiUsageCount);

  /// Create a copy of WorkspaceTopMemberDto
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$WorkspaceTopMemberDtoImplCopyWith<_$WorkspaceTopMemberDtoImpl>
  get copyWith =>
      __$$WorkspaceTopMemberDtoImplCopyWithImpl<_$WorkspaceTopMemberDtoImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$WorkspaceTopMemberDtoImplToJson(this);
  }
}

abstract class _WorkspaceTopMemberDto implements WorkspaceTopMemberDto {
  const factory _WorkspaceTopMemberDto({
    required final String userId,
    final String name,
    final String email,
    final int creditsUsed,
    final int aiUsageCount,
  }) = _$WorkspaceTopMemberDtoImpl;

  factory _WorkspaceTopMemberDto.fromJson(Map<String, dynamic> json) =
      _$WorkspaceTopMemberDtoImpl.fromJson;

  @override
  String get userId;
  @override
  String get name;
  @override
  String get email;
  @override
  int get creditsUsed;
  @override
  int get aiUsageCount;

  /// Create a copy of WorkspaceTopMemberDto
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$WorkspaceTopMemberDtoImplCopyWith<_$WorkspaceTopMemberDtoImpl>
  get copyWith => throw _privateConstructorUsedError;
}
