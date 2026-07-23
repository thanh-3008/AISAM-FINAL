// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'ai_generation_response.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

AiGenerationResponseModel _$AiGenerationResponseModelFromJson(
  Map<String, dynamic> json,
) {
  return _AiGenerationResponseModel.fromJson(json);
}

/// @nodoc
mixin _$AiGenerationResponseModel {
  String get aiGenerationId => throw _privateConstructorUsedError;
  String get contentId => throw _privateConstructorUsedError;
  String? get generatedText => throw _privateConstructorUsedError;
  String? get generatedImageUrl => throw _privateConstructorUsedError;
  String? get generatedVideoUrl => throw _privateConstructorUsedError;
  String? get videoJobId => throw _privateConstructorUsedError;
  String? get providerUsed => throw _privateConstructorUsedError;
  AiStatusEnum get status => throw _privateConstructorUsedError;
  String? get errorMessage => throw _privateConstructorUsedError;
  DateTime get createdAt => throw _privateConstructorUsedError;

  /// Serializes this AiGenerationResponseModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of AiGenerationResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $AiGenerationResponseModelCopyWith<AiGenerationResponseModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $AiGenerationResponseModelCopyWith<$Res> {
  factory $AiGenerationResponseModelCopyWith(
    AiGenerationResponseModel value,
    $Res Function(AiGenerationResponseModel) then,
  ) = _$AiGenerationResponseModelCopyWithImpl<$Res, AiGenerationResponseModel>;
  @useResult
  $Res call({
    String aiGenerationId,
    String contentId,
    String? generatedText,
    String? generatedImageUrl,
    String? generatedVideoUrl,
    String? videoJobId,
    String? providerUsed,
    AiStatusEnum status,
    String? errorMessage,
    DateTime createdAt,
  });
}

/// @nodoc
class _$AiGenerationResponseModelCopyWithImpl<
  $Res,
  $Val extends AiGenerationResponseModel
>
    implements $AiGenerationResponseModelCopyWith<$Res> {
  _$AiGenerationResponseModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of AiGenerationResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? aiGenerationId = null,
    Object? contentId = null,
    Object? generatedText = freezed,
    Object? generatedImageUrl = freezed,
    Object? generatedVideoUrl = freezed,
    Object? videoJobId = freezed,
    Object? providerUsed = freezed,
    Object? status = null,
    Object? errorMessage = freezed,
    Object? createdAt = null,
  }) {
    return _then(
      _value.copyWith(
            aiGenerationId: null == aiGenerationId
                ? _value.aiGenerationId
                : aiGenerationId // ignore: cast_nullable_to_non_nullable
                      as String,
            contentId: null == contentId
                ? _value.contentId
                : contentId // ignore: cast_nullable_to_non_nullable
                      as String,
            generatedText: freezed == generatedText
                ? _value.generatedText
                : generatedText // ignore: cast_nullable_to_non_nullable
                      as String?,
            generatedImageUrl: freezed == generatedImageUrl
                ? _value.generatedImageUrl
                : generatedImageUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            generatedVideoUrl: freezed == generatedVideoUrl
                ? _value.generatedVideoUrl
                : generatedVideoUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            videoJobId: freezed == videoJobId
                ? _value.videoJobId
                : videoJobId // ignore: cast_nullable_to_non_nullable
                      as String?,
            providerUsed: freezed == providerUsed
                ? _value.providerUsed
                : providerUsed // ignore: cast_nullable_to_non_nullable
                      as String?,
            status: null == status
                ? _value.status
                : status // ignore: cast_nullable_to_non_nullable
                      as AiStatusEnum,
            errorMessage: freezed == errorMessage
                ? _value.errorMessage
                : errorMessage // ignore: cast_nullable_to_non_nullable
                      as String?,
            createdAt: null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$AiGenerationResponseModelImplCopyWith<$Res>
    implements $AiGenerationResponseModelCopyWith<$Res> {
  factory _$$AiGenerationResponseModelImplCopyWith(
    _$AiGenerationResponseModelImpl value,
    $Res Function(_$AiGenerationResponseModelImpl) then,
  ) = __$$AiGenerationResponseModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String aiGenerationId,
    String contentId,
    String? generatedText,
    String? generatedImageUrl,
    String? generatedVideoUrl,
    String? videoJobId,
    String? providerUsed,
    AiStatusEnum status,
    String? errorMessage,
    DateTime createdAt,
  });
}

/// @nodoc
class __$$AiGenerationResponseModelImplCopyWithImpl<$Res>
    extends
        _$AiGenerationResponseModelCopyWithImpl<
          $Res,
          _$AiGenerationResponseModelImpl
        >
    implements _$$AiGenerationResponseModelImplCopyWith<$Res> {
  __$$AiGenerationResponseModelImplCopyWithImpl(
    _$AiGenerationResponseModelImpl _value,
    $Res Function(_$AiGenerationResponseModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of AiGenerationResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? aiGenerationId = null,
    Object? contentId = null,
    Object? generatedText = freezed,
    Object? generatedImageUrl = freezed,
    Object? generatedVideoUrl = freezed,
    Object? videoJobId = freezed,
    Object? providerUsed = freezed,
    Object? status = null,
    Object? errorMessage = freezed,
    Object? createdAt = null,
  }) {
    return _then(
      _$AiGenerationResponseModelImpl(
        aiGenerationId: null == aiGenerationId
            ? _value.aiGenerationId
            : aiGenerationId // ignore: cast_nullable_to_non_nullable
                  as String,
        contentId: null == contentId
            ? _value.contentId
            : contentId // ignore: cast_nullable_to_non_nullable
                  as String,
        generatedText: freezed == generatedText
            ? _value.generatedText
            : generatedText // ignore: cast_nullable_to_non_nullable
                  as String?,
        generatedImageUrl: freezed == generatedImageUrl
            ? _value.generatedImageUrl
            : generatedImageUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        generatedVideoUrl: freezed == generatedVideoUrl
            ? _value.generatedVideoUrl
            : generatedVideoUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        videoJobId: freezed == videoJobId
            ? _value.videoJobId
            : videoJobId // ignore: cast_nullable_to_non_nullable
                  as String?,
        providerUsed: freezed == providerUsed
            ? _value.providerUsed
            : providerUsed // ignore: cast_nullable_to_non_nullable
                  as String?,
        status: null == status
            ? _value.status
            : status // ignore: cast_nullable_to_non_nullable
                  as AiStatusEnum,
        errorMessage: freezed == errorMessage
            ? _value.errorMessage
            : errorMessage // ignore: cast_nullable_to_non_nullable
                  as String?,
        createdAt: null == createdAt
            ? _value.createdAt
            : createdAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$AiGenerationResponseModelImpl implements _AiGenerationResponseModel {
  const _$AiGenerationResponseModelImpl({
    required this.aiGenerationId,
    required this.contentId,
    this.generatedText,
    this.generatedImageUrl,
    this.generatedVideoUrl,
    this.videoJobId,
    this.providerUsed,
    required this.status,
    this.errorMessage,
    required this.createdAt,
  });

  factory _$AiGenerationResponseModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$AiGenerationResponseModelImplFromJson(json);

  @override
  final String aiGenerationId;
  @override
  final String contentId;
  @override
  final String? generatedText;
  @override
  final String? generatedImageUrl;
  @override
  final String? generatedVideoUrl;
  @override
  final String? videoJobId;
  @override
  final String? providerUsed;
  @override
  final AiStatusEnum status;
  @override
  final String? errorMessage;
  @override
  final DateTime createdAt;

  @override
  String toString() {
    return 'AiGenerationResponseModel(aiGenerationId: $aiGenerationId, contentId: $contentId, generatedText: $generatedText, generatedImageUrl: $generatedImageUrl, generatedVideoUrl: $generatedVideoUrl, videoJobId: $videoJobId, providerUsed: $providerUsed, status: $status, errorMessage: $errorMessage, createdAt: $createdAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$AiGenerationResponseModelImpl &&
            (identical(other.aiGenerationId, aiGenerationId) ||
                other.aiGenerationId == aiGenerationId) &&
            (identical(other.contentId, contentId) ||
                other.contentId == contentId) &&
            (identical(other.generatedText, generatedText) ||
                other.generatedText == generatedText) &&
            (identical(other.generatedImageUrl, generatedImageUrl) ||
                other.generatedImageUrl == generatedImageUrl) &&
            (identical(other.generatedVideoUrl, generatedVideoUrl) ||
                other.generatedVideoUrl == generatedVideoUrl) &&
            (identical(other.videoJobId, videoJobId) ||
                other.videoJobId == videoJobId) &&
            (identical(other.providerUsed, providerUsed) ||
                other.providerUsed == providerUsed) &&
            (identical(other.status, status) || other.status == status) &&
            (identical(other.errorMessage, errorMessage) ||
                other.errorMessage == errorMessage) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    aiGenerationId,
    contentId,
    generatedText,
    generatedImageUrl,
    generatedVideoUrl,
    videoJobId,
    providerUsed,
    status,
    errorMessage,
    createdAt,
  );

  /// Create a copy of AiGenerationResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$AiGenerationResponseModelImplCopyWith<_$AiGenerationResponseModelImpl>
  get copyWith =>
      __$$AiGenerationResponseModelImplCopyWithImpl<
        _$AiGenerationResponseModelImpl
      >(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$AiGenerationResponseModelImplToJson(this);
  }
}

abstract class _AiGenerationResponseModel implements AiGenerationResponseModel {
  const factory _AiGenerationResponseModel({
    required final String aiGenerationId,
    required final String contentId,
    final String? generatedText,
    final String? generatedImageUrl,
    final String? generatedVideoUrl,
    final String? videoJobId,
    final String? providerUsed,
    required final AiStatusEnum status,
    final String? errorMessage,
    required final DateTime createdAt,
  }) = _$AiGenerationResponseModelImpl;

  factory _AiGenerationResponseModel.fromJson(Map<String, dynamic> json) =
      _$AiGenerationResponseModelImpl.fromJson;

  @override
  String get aiGenerationId;
  @override
  String get contentId;
  @override
  String? get generatedText;
  @override
  String? get generatedImageUrl;
  @override
  String? get generatedVideoUrl;
  @override
  String? get videoJobId;
  @override
  String? get providerUsed;
  @override
  AiStatusEnum get status;
  @override
  String? get errorMessage;
  @override
  DateTime get createdAt;

  /// Create a copy of AiGenerationResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$AiGenerationResponseModelImplCopyWith<_$AiGenerationResponseModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}
