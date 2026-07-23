// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'ai_generation_request.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CreateDraftRequest _$CreateDraftRequestFromJson(Map<String, dynamic> json) {
  return _CreateDraftRequest.fromJson(json);
}

/// @nodoc
mixin _$CreateDraftRequest {
  String get brandId => throw _privateConstructorUsedError;
  String? get productId => throw _privateConstructorUsedError;
  AdTypeEnum get adType => throw _privateConstructorUsedError;
  String? get title => throw _privateConstructorUsedError;
  String get prompt => throw _privateConstructorUsedError;

  /// Serializes this CreateDraftRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CreateDraftRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CreateDraftRequestCopyWith<CreateDraftRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CreateDraftRequestCopyWith<$Res> {
  factory $CreateDraftRequestCopyWith(
    CreateDraftRequest value,
    $Res Function(CreateDraftRequest) then,
  ) = _$CreateDraftRequestCopyWithImpl<$Res, CreateDraftRequest>;
  @useResult
  $Res call({
    String brandId,
    String? productId,
    AdTypeEnum adType,
    String? title,
    String prompt,
  });
}

/// @nodoc
class _$CreateDraftRequestCopyWithImpl<$Res, $Val extends CreateDraftRequest>
    implements $CreateDraftRequestCopyWith<$Res> {
  _$CreateDraftRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CreateDraftRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? brandId = null,
    Object? productId = freezed,
    Object? adType = null,
    Object? title = freezed,
    Object? prompt = null,
  }) {
    return _then(
      _value.copyWith(
            brandId: null == brandId
                ? _value.brandId
                : brandId // ignore: cast_nullable_to_non_nullable
                      as String,
            productId: freezed == productId
                ? _value.productId
                : productId // ignore: cast_nullable_to_non_nullable
                      as String?,
            adType: null == adType
                ? _value.adType
                : adType // ignore: cast_nullable_to_non_nullable
                      as AdTypeEnum,
            title: freezed == title
                ? _value.title
                : title // ignore: cast_nullable_to_non_nullable
                      as String?,
            prompt: null == prompt
                ? _value.prompt
                : prompt // ignore: cast_nullable_to_non_nullable
                      as String,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CreateDraftRequestImplCopyWith<$Res>
    implements $CreateDraftRequestCopyWith<$Res> {
  factory _$$CreateDraftRequestImplCopyWith(
    _$CreateDraftRequestImpl value,
    $Res Function(_$CreateDraftRequestImpl) then,
  ) = __$$CreateDraftRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String brandId,
    String? productId,
    AdTypeEnum adType,
    String? title,
    String prompt,
  });
}

/// @nodoc
class __$$CreateDraftRequestImplCopyWithImpl<$Res>
    extends _$CreateDraftRequestCopyWithImpl<$Res, _$CreateDraftRequestImpl>
    implements _$$CreateDraftRequestImplCopyWith<$Res> {
  __$$CreateDraftRequestImplCopyWithImpl(
    _$CreateDraftRequestImpl _value,
    $Res Function(_$CreateDraftRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CreateDraftRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? brandId = null,
    Object? productId = freezed,
    Object? adType = null,
    Object? title = freezed,
    Object? prompt = null,
  }) {
    return _then(
      _$CreateDraftRequestImpl(
        brandId: null == brandId
            ? _value.brandId
            : brandId // ignore: cast_nullable_to_non_nullable
                  as String,
        productId: freezed == productId
            ? _value.productId
            : productId // ignore: cast_nullable_to_non_nullable
                  as String?,
        adType: null == adType
            ? _value.adType
            : adType // ignore: cast_nullable_to_non_nullable
                  as AdTypeEnum,
        title: freezed == title
            ? _value.title
            : title // ignore: cast_nullable_to_non_nullable
                  as String?,
        prompt: null == prompt
            ? _value.prompt
            : prompt // ignore: cast_nullable_to_non_nullable
                  as String,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CreateDraftRequestImpl implements _CreateDraftRequest {
  const _$CreateDraftRequestImpl({
    required this.brandId,
    this.productId,
    required this.adType,
    this.title,
    required this.prompt,
  });

  factory _$CreateDraftRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$CreateDraftRequestImplFromJson(json);

  @override
  final String brandId;
  @override
  final String? productId;
  @override
  final AdTypeEnum adType;
  @override
  final String? title;
  @override
  final String prompt;

  @override
  String toString() {
    return 'CreateDraftRequest(brandId: $brandId, productId: $productId, adType: $adType, title: $title, prompt: $prompt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CreateDraftRequestImpl &&
            (identical(other.brandId, brandId) || other.brandId == brandId) &&
            (identical(other.productId, productId) ||
                other.productId == productId) &&
            (identical(other.adType, adType) || other.adType == adType) &&
            (identical(other.title, title) || other.title == title) &&
            (identical(other.prompt, prompt) || other.prompt == prompt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, brandId, productId, adType, title, prompt);

  /// Create a copy of CreateDraftRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CreateDraftRequestImplCopyWith<_$CreateDraftRequestImpl> get copyWith =>
      __$$CreateDraftRequestImplCopyWithImpl<_$CreateDraftRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$CreateDraftRequestImplToJson(this);
  }
}

abstract class _CreateDraftRequest implements CreateDraftRequest {
  const factory _CreateDraftRequest({
    required final String brandId,
    final String? productId,
    required final AdTypeEnum adType,
    final String? title,
    required final String prompt,
  }) = _$CreateDraftRequestImpl;

  factory _CreateDraftRequest.fromJson(Map<String, dynamic> json) =
      _$CreateDraftRequestImpl.fromJson;

  @override
  String get brandId;
  @override
  String? get productId;
  @override
  AdTypeEnum get adType;
  @override
  String? get title;
  @override
  String get prompt;

  /// Create a copy of CreateDraftRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CreateDraftRequestImplCopyWith<_$CreateDraftRequestImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

ImproveContentRequest _$ImproveContentRequestFromJson(
  Map<String, dynamic> json,
) {
  return _ImproveContentRequest.fromJson(json);
}

/// @nodoc
mixin _$ImproveContentRequest {
  String get content => throw _privateConstructorUsedError;
  String get instructions => throw _privateConstructorUsedError;
  String get prompt => throw _privateConstructorUsedError;

  /// Serializes this ImproveContentRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of ImproveContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $ImproveContentRequestCopyWith<ImproveContentRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ImproveContentRequestCopyWith<$Res> {
  factory $ImproveContentRequestCopyWith(
    ImproveContentRequest value,
    $Res Function(ImproveContentRequest) then,
  ) = _$ImproveContentRequestCopyWithImpl<$Res, ImproveContentRequest>;
  @useResult
  $Res call({String content, String instructions, String prompt});
}

/// @nodoc
class _$ImproveContentRequestCopyWithImpl<
  $Res,
  $Val extends ImproveContentRequest
>
    implements $ImproveContentRequestCopyWith<$Res> {
  _$ImproveContentRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of ImproveContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? content = null,
    Object? instructions = null,
    Object? prompt = null,
  }) {
    return _then(
      _value.copyWith(
            content: null == content
                ? _value.content
                : content // ignore: cast_nullable_to_non_nullable
                      as String,
            instructions: null == instructions
                ? _value.instructions
                : instructions // ignore: cast_nullable_to_non_nullable
                      as String,
            prompt: null == prompt
                ? _value.prompt
                : prompt // ignore: cast_nullable_to_non_nullable
                      as String,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$ImproveContentRequestImplCopyWith<$Res>
    implements $ImproveContentRequestCopyWith<$Res> {
  factory _$$ImproveContentRequestImplCopyWith(
    _$ImproveContentRequestImpl value,
    $Res Function(_$ImproveContentRequestImpl) then,
  ) = __$$ImproveContentRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String content, String instructions, String prompt});
}

/// @nodoc
class __$$ImproveContentRequestImplCopyWithImpl<$Res>
    extends
        _$ImproveContentRequestCopyWithImpl<$Res, _$ImproveContentRequestImpl>
    implements _$$ImproveContentRequestImplCopyWith<$Res> {
  __$$ImproveContentRequestImplCopyWithImpl(
    _$ImproveContentRequestImpl _value,
    $Res Function(_$ImproveContentRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of ImproveContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? content = null,
    Object? instructions = null,
    Object? prompt = null,
  }) {
    return _then(
      _$ImproveContentRequestImpl(
        content: null == content
            ? _value.content
            : content // ignore: cast_nullable_to_non_nullable
                  as String,
        instructions: null == instructions
            ? _value.instructions
            : instructions // ignore: cast_nullable_to_non_nullable
                  as String,
        prompt: null == prompt
            ? _value.prompt
            : prompt // ignore: cast_nullable_to_non_nullable
                  as String,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$ImproveContentRequestImpl implements _ImproveContentRequest {
  const _$ImproveContentRequestImpl({
    required this.content,
    required this.instructions,
    required this.prompt,
  });

  factory _$ImproveContentRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$ImproveContentRequestImplFromJson(json);

  @override
  final String content;
  @override
  final String instructions;
  @override
  final String prompt;

  @override
  String toString() {
    return 'ImproveContentRequest(content: $content, instructions: $instructions, prompt: $prompt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ImproveContentRequestImpl &&
            (identical(other.content, content) || other.content == content) &&
            (identical(other.instructions, instructions) ||
                other.instructions == instructions) &&
            (identical(other.prompt, prompt) || other.prompt == prompt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(runtimeType, content, instructions, prompt);

  /// Create a copy of ImproveContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$ImproveContentRequestImplCopyWith<_$ImproveContentRequestImpl>
  get copyWith =>
      __$$ImproveContentRequestImplCopyWithImpl<_$ImproveContentRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$ImproveContentRequestImplToJson(this);
  }
}

abstract class _ImproveContentRequest implements ImproveContentRequest {
  const factory _ImproveContentRequest({
    required final String content,
    required final String instructions,
    required final String prompt,
  }) = _$ImproveContentRequestImpl;

  factory _ImproveContentRequest.fromJson(Map<String, dynamic> json) =
      _$ImproveContentRequestImpl.fromJson;

  @override
  String get content;
  @override
  String get instructions;
  @override
  String get prompt;

  /// Create a copy of ImproveContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$ImproveContentRequestImplCopyWith<_$ImproveContentRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}
