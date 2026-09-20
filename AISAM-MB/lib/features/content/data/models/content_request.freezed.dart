// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'content_request.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

CreateContentRequest _$CreateContentRequestFromJson(Map<String, dynamic> json) {
  return _CreateContentRequest.fromJson(json);
}

/// @nodoc
mixin _$CreateContentRequest {
  String get brandId => throw _privateConstructorUsedError;
  String? get teamId => throw _privateConstructorUsedError;
  String? get productId => throw _privateConstructorUsedError;
  AdTypeEnum get adType => throw _privateConstructorUsedError;
  String? get title => throw _privateConstructorUsedError;
  String get textContent => throw _privateConstructorUsedError;
  String? get richTextJson => throw _privateConstructorUsedError;
  int? get richTextVersion => throw _privateConstructorUsedError;
  String? get imageUrl => throw _privateConstructorUsedError;
  List<String>? get imageUrls => throw _privateConstructorUsedError;
  String? get videoUrl => throw _privateConstructorUsedError;
  String? get thumbnailUrl => throw _privateConstructorUsedError;
  String? get styleDescription => throw _privateConstructorUsedError;
  String? get contextDescription => throw _privateConstructorUsedError;
  String? get representativeCharacter => throw _privateConstructorUsedError;
  ContentStatusEnum? get status => throw _privateConstructorUsedError;
  bool get isAiGenerated => throw _privateConstructorUsedError;
  List<String>? get tags => throw _privateConstructorUsedError;

  /// Serializes this CreateContentRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of CreateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $CreateContentRequestCopyWith<CreateContentRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $CreateContentRequestCopyWith<$Res> {
  factory $CreateContentRequestCopyWith(
    CreateContentRequest value,
    $Res Function(CreateContentRequest) then,
  ) = _$CreateContentRequestCopyWithImpl<$Res, CreateContentRequest>;
  @useResult
  $Res call({
    String brandId,
    String? teamId,
    String? productId,
    AdTypeEnum adType,
    String? title,
    String textContent,
    String? richTextJson,
    int? richTextVersion,
    String? imageUrl,
    List<String>? imageUrls,
    String? videoUrl,
    String? thumbnailUrl,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    ContentStatusEnum? status,
    bool isAiGenerated,
    List<String>? tags,
  });
}

/// @nodoc
class _$CreateContentRequestCopyWithImpl<
  $Res,
  $Val extends CreateContentRequest
>
    implements $CreateContentRequestCopyWith<$Res> {
  _$CreateContentRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of CreateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? brandId = null,
    Object? teamId = freezed,
    Object? productId = freezed,
    Object? adType = null,
    Object? title = freezed,
    Object? textContent = null,
    Object? richTextJson = freezed,
    Object? richTextVersion = freezed,
    Object? imageUrl = freezed,
    Object? imageUrls = freezed,
    Object? videoUrl = freezed,
    Object? thumbnailUrl = freezed,
    Object? styleDescription = freezed,
    Object? contextDescription = freezed,
    Object? representativeCharacter = freezed,
    Object? status = freezed,
    Object? isAiGenerated = null,
    Object? tags = freezed,
  }) {
    return _then(
      _value.copyWith(
            brandId: null == brandId
                ? _value.brandId
                : brandId // ignore: cast_nullable_to_non_nullable
                      as String,
            teamId: freezed == teamId
                ? _value.teamId
                : teamId // ignore: cast_nullable_to_non_nullable
                      as String?,
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
            textContent: null == textContent
                ? _value.textContent
                : textContent // ignore: cast_nullable_to_non_nullable
                      as String,
            richTextJson: freezed == richTextJson
                ? _value.richTextJson
                : richTextJson // ignore: cast_nullable_to_non_nullable
                      as String?,
            richTextVersion: freezed == richTextVersion
                ? _value.richTextVersion
                : richTextVersion // ignore: cast_nullable_to_non_nullable
                      as int?,
            imageUrl: freezed == imageUrl
                ? _value.imageUrl
                : imageUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            imageUrls: freezed == imageUrls
                ? _value.imageUrls
                : imageUrls // ignore: cast_nullable_to_non_nullable
                      as List<String>?,
            videoUrl: freezed == videoUrl
                ? _value.videoUrl
                : videoUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            thumbnailUrl: freezed == thumbnailUrl
                ? _value.thumbnailUrl
                : thumbnailUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            styleDescription: freezed == styleDescription
                ? _value.styleDescription
                : styleDescription // ignore: cast_nullable_to_non_nullable
                      as String?,
            contextDescription: freezed == contextDescription
                ? _value.contextDescription
                : contextDescription // ignore: cast_nullable_to_non_nullable
                      as String?,
            representativeCharacter: freezed == representativeCharacter
                ? _value.representativeCharacter
                : representativeCharacter // ignore: cast_nullable_to_non_nullable
                      as String?,
            status: freezed == status
                ? _value.status
                : status // ignore: cast_nullable_to_non_nullable
                      as ContentStatusEnum?,
            isAiGenerated: null == isAiGenerated
                ? _value.isAiGenerated
                : isAiGenerated // ignore: cast_nullable_to_non_nullable
                      as bool,
            tags: freezed == tags
                ? _value.tags
                : tags // ignore: cast_nullable_to_non_nullable
                      as List<String>?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$CreateContentRequestImplCopyWith<$Res>
    implements $CreateContentRequestCopyWith<$Res> {
  factory _$$CreateContentRequestImplCopyWith(
    _$CreateContentRequestImpl value,
    $Res Function(_$CreateContentRequestImpl) then,
  ) = __$$CreateContentRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String brandId,
    String? teamId,
    String? productId,
    AdTypeEnum adType,
    String? title,
    String textContent,
    String? richTextJson,
    int? richTextVersion,
    String? imageUrl,
    List<String>? imageUrls,
    String? videoUrl,
    String? thumbnailUrl,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    ContentStatusEnum? status,
    bool isAiGenerated,
    List<String>? tags,
  });
}

/// @nodoc
class __$$CreateContentRequestImplCopyWithImpl<$Res>
    extends _$CreateContentRequestCopyWithImpl<$Res, _$CreateContentRequestImpl>
    implements _$$CreateContentRequestImplCopyWith<$Res> {
  __$$CreateContentRequestImplCopyWithImpl(
    _$CreateContentRequestImpl _value,
    $Res Function(_$CreateContentRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of CreateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? brandId = null,
    Object? teamId = freezed,
    Object? productId = freezed,
    Object? adType = null,
    Object? title = freezed,
    Object? textContent = null,
    Object? richTextJson = freezed,
    Object? richTextVersion = freezed,
    Object? imageUrl = freezed,
    Object? imageUrls = freezed,
    Object? videoUrl = freezed,
    Object? thumbnailUrl = freezed,
    Object? styleDescription = freezed,
    Object? contextDescription = freezed,
    Object? representativeCharacter = freezed,
    Object? status = freezed,
    Object? isAiGenerated = null,
    Object? tags = freezed,
  }) {
    return _then(
      _$CreateContentRequestImpl(
        brandId: null == brandId
            ? _value.brandId
            : brandId // ignore: cast_nullable_to_non_nullable
                  as String,
        teamId: freezed == teamId
            ? _value.teamId
            : teamId // ignore: cast_nullable_to_non_nullable
                  as String?,
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
        textContent: null == textContent
            ? _value.textContent
            : textContent // ignore: cast_nullable_to_non_nullable
                  as String,
        richTextJson: freezed == richTextJson
            ? _value.richTextJson
            : richTextJson // ignore: cast_nullable_to_non_nullable
                  as String?,
        richTextVersion: freezed == richTextVersion
            ? _value.richTextVersion
            : richTextVersion // ignore: cast_nullable_to_non_nullable
                  as int?,
        imageUrl: freezed == imageUrl
            ? _value.imageUrl
            : imageUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        imageUrls: freezed == imageUrls
            ? _value._imageUrls
            : imageUrls // ignore: cast_nullable_to_non_nullable
                  as List<String>?,
        videoUrl: freezed == videoUrl
            ? _value.videoUrl
            : videoUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        thumbnailUrl: freezed == thumbnailUrl
            ? _value.thumbnailUrl
            : thumbnailUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        styleDescription: freezed == styleDescription
            ? _value.styleDescription
            : styleDescription // ignore: cast_nullable_to_non_nullable
                  as String?,
        contextDescription: freezed == contextDescription
            ? _value.contextDescription
            : contextDescription // ignore: cast_nullable_to_non_nullable
                  as String?,
        representativeCharacter: freezed == representativeCharacter
            ? _value.representativeCharacter
            : representativeCharacter // ignore: cast_nullable_to_non_nullable
                  as String?,
        status: freezed == status
            ? _value.status
            : status // ignore: cast_nullable_to_non_nullable
                  as ContentStatusEnum?,
        isAiGenerated: null == isAiGenerated
            ? _value.isAiGenerated
            : isAiGenerated // ignore: cast_nullable_to_non_nullable
                  as bool,
        tags: freezed == tags
            ? _value._tags
            : tags // ignore: cast_nullable_to_non_nullable
                  as List<String>?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$CreateContentRequestImpl implements _CreateContentRequest {
  const _$CreateContentRequestImpl({
    required this.brandId,
    this.teamId,
    this.productId,
    required this.adType,
    this.title,
    required this.textContent,
    this.richTextJson,
    this.richTextVersion,
    this.imageUrl,
    final List<String>? imageUrls,
    this.videoUrl,
    this.thumbnailUrl,
    this.styleDescription,
    this.contextDescription,
    this.representativeCharacter,
    this.status,
    this.isAiGenerated = false,
    final List<String>? tags,
  }) : _imageUrls = imageUrls,
       _tags = tags;

  factory _$CreateContentRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$CreateContentRequestImplFromJson(json);

  @override
  final String brandId;
  @override
  final String? teamId;
  @override
  final String? productId;
  @override
  final AdTypeEnum adType;
  @override
  final String? title;
  @override
  final String textContent;
  @override
  final String? richTextJson;
  @override
  final int? richTextVersion;
  @override
  final String? imageUrl;
  final List<String>? _imageUrls;
  @override
  List<String>? get imageUrls {
    final value = _imageUrls;
    if (value == null) return null;
    if (_imageUrls is EqualUnmodifiableListView) return _imageUrls;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  @override
  final String? videoUrl;
  @override
  final String? thumbnailUrl;
  @override
  final String? styleDescription;
  @override
  final String? contextDescription;
  @override
  final String? representativeCharacter;
  @override
  final ContentStatusEnum? status;
  @override
  @JsonKey()
  final bool isAiGenerated;
  final List<String>? _tags;
  @override
  List<String>? get tags {
    final value = _tags;
    if (value == null) return null;
    if (_tags is EqualUnmodifiableListView) return _tags;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  @override
  String toString() {
    return 'CreateContentRequest(brandId: $brandId, teamId: $teamId, productId: $productId, adType: $adType, title: $title, textContent: $textContent, richTextJson: $richTextJson, richTextVersion: $richTextVersion, imageUrl: $imageUrl, imageUrls: $imageUrls, videoUrl: $videoUrl, thumbnailUrl: $thumbnailUrl, styleDescription: $styleDescription, contextDescription: $contextDescription, representativeCharacter: $representativeCharacter, status: $status, isAiGenerated: $isAiGenerated, tags: $tags)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$CreateContentRequestImpl &&
            (identical(other.brandId, brandId) || other.brandId == brandId) &&
            (identical(other.teamId, teamId) || other.teamId == teamId) &&
            (identical(other.productId, productId) ||
                other.productId == productId) &&
            (identical(other.adType, adType) || other.adType == adType) &&
            (identical(other.title, title) || other.title == title) &&
            (identical(other.textContent, textContent) ||
                other.textContent == textContent) &&
            (identical(other.richTextJson, richTextJson) ||
                other.richTextJson == richTextJson) &&
            (identical(other.richTextVersion, richTextVersion) ||
                other.richTextVersion == richTextVersion) &&
            (identical(other.imageUrl, imageUrl) ||
                other.imageUrl == imageUrl) &&
            const DeepCollectionEquality().equals(
              other._imageUrls,
              _imageUrls,
            ) &&
            (identical(other.videoUrl, videoUrl) ||
                other.videoUrl == videoUrl) &&
            (identical(other.thumbnailUrl, thumbnailUrl) ||
                other.thumbnailUrl == thumbnailUrl) &&
            (identical(other.styleDescription, styleDescription) ||
                other.styleDescription == styleDescription) &&
            (identical(other.contextDescription, contextDescription) ||
                other.contextDescription == contextDescription) &&
            (identical(
                  other.representativeCharacter,
                  representativeCharacter,
                ) ||
                other.representativeCharacter == representativeCharacter) &&
            (identical(other.status, status) || other.status == status) &&
            (identical(other.isAiGenerated, isAiGenerated) ||
                other.isAiGenerated == isAiGenerated) &&
            const DeepCollectionEquality().equals(other._tags, _tags));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    brandId,
    teamId,
    productId,
    adType,
    title,
    textContent,
    richTextJson,
    richTextVersion,
    imageUrl,
    const DeepCollectionEquality().hash(_imageUrls),
    videoUrl,
    thumbnailUrl,
    styleDescription,
    contextDescription,
    representativeCharacter,
    status,
    isAiGenerated,
    const DeepCollectionEquality().hash(_tags),
  );

  /// Create a copy of CreateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$CreateContentRequestImplCopyWith<_$CreateContentRequestImpl>
  get copyWith =>
      __$$CreateContentRequestImplCopyWithImpl<_$CreateContentRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$CreateContentRequestImplToJson(this);
  }
}

abstract class _CreateContentRequest implements CreateContentRequest {
  const factory _CreateContentRequest({
    required final String brandId,
    final String? teamId,
    final String? productId,
    required final AdTypeEnum adType,
    final String? title,
    required final String textContent,
    final String? richTextJson,
    final int? richTextVersion,
    final String? imageUrl,
    final List<String>? imageUrls,
    final String? videoUrl,
    final String? thumbnailUrl,
    final String? styleDescription,
    final String? contextDescription,
    final String? representativeCharacter,
    final ContentStatusEnum? status,
    final bool isAiGenerated,
    final List<String>? tags,
  }) = _$CreateContentRequestImpl;

  factory _CreateContentRequest.fromJson(Map<String, dynamic> json) =
      _$CreateContentRequestImpl.fromJson;

  @override
  String get brandId;
  @override
  String? get teamId;
  @override
  String? get productId;
  @override
  AdTypeEnum get adType;
  @override
  String? get title;
  @override
  String get textContent;
  @override
  String? get richTextJson;
  @override
  int? get richTextVersion;
  @override
  String? get imageUrl;
  @override
  List<String>? get imageUrls;
  @override
  String? get videoUrl;
  @override
  String? get thumbnailUrl;
  @override
  String? get styleDescription;
  @override
  String? get contextDescription;
  @override
  String? get representativeCharacter;
  @override
  ContentStatusEnum? get status;
  @override
  bool get isAiGenerated;
  @override
  List<String>? get tags;

  /// Create a copy of CreateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$CreateContentRequestImplCopyWith<_$CreateContentRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}

UpdateContentRequest _$UpdateContentRequestFromJson(Map<String, dynamic> json) {
  return _UpdateContentRequest.fromJson(json);
}

/// @nodoc
mixin _$UpdateContentRequest {
  String? get productId => throw _privateConstructorUsedError;
  AdTypeEnum? get adType => throw _privateConstructorUsedError;
  String? get title => throw _privateConstructorUsedError;
  String? get textContent => throw _privateConstructorUsedError;
  String? get richTextJson => throw _privateConstructorUsedError;
  int? get richTextVersion => throw _privateConstructorUsedError;
  String? get imageUrl => throw _privateConstructorUsedError;
  List<String>? get imageUrls => throw _privateConstructorUsedError;
  String? get videoUrl => throw _privateConstructorUsedError;
  String? get styleDescription => throw _privateConstructorUsedError;
  String? get contextDescription => throw _privateConstructorUsedError;
  String? get representativeCharacter => throw _privateConstructorUsedError;
  ContentStatusEnum? get status => throw _privateConstructorUsedError;
  List<String>? get tags => throw _privateConstructorUsedError;

  /// Serializes this UpdateContentRequest to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of UpdateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $UpdateContentRequestCopyWith<UpdateContentRequest> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $UpdateContentRequestCopyWith<$Res> {
  factory $UpdateContentRequestCopyWith(
    UpdateContentRequest value,
    $Res Function(UpdateContentRequest) then,
  ) = _$UpdateContentRequestCopyWithImpl<$Res, UpdateContentRequest>;
  @useResult
  $Res call({
    String? productId,
    AdTypeEnum? adType,
    String? title,
    String? textContent,
    String? richTextJson,
    int? richTextVersion,
    String? imageUrl,
    List<String>? imageUrls,
    String? videoUrl,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    ContentStatusEnum? status,
    List<String>? tags,
  });
}

/// @nodoc
class _$UpdateContentRequestCopyWithImpl<
  $Res,
  $Val extends UpdateContentRequest
>
    implements $UpdateContentRequestCopyWith<$Res> {
  _$UpdateContentRequestCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of UpdateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? productId = freezed,
    Object? adType = freezed,
    Object? title = freezed,
    Object? textContent = freezed,
    Object? richTextJson = freezed,
    Object? richTextVersion = freezed,
    Object? imageUrl = freezed,
    Object? imageUrls = freezed,
    Object? videoUrl = freezed,
    Object? styleDescription = freezed,
    Object? contextDescription = freezed,
    Object? representativeCharacter = freezed,
    Object? status = freezed,
    Object? tags = freezed,
  }) {
    return _then(
      _value.copyWith(
            productId: freezed == productId
                ? _value.productId
                : productId // ignore: cast_nullable_to_non_nullable
                      as String?,
            adType: freezed == adType
                ? _value.adType
                : adType // ignore: cast_nullable_to_non_nullable
                      as AdTypeEnum?,
            title: freezed == title
                ? _value.title
                : title // ignore: cast_nullable_to_non_nullable
                      as String?,
            textContent: freezed == textContent
                ? _value.textContent
                : textContent // ignore: cast_nullable_to_non_nullable
                      as String?,
            richTextJson: freezed == richTextJson
                ? _value.richTextJson
                : richTextJson // ignore: cast_nullable_to_non_nullable
                      as String?,
            richTextVersion: freezed == richTextVersion
                ? _value.richTextVersion
                : richTextVersion // ignore: cast_nullable_to_non_nullable
                      as int?,
            imageUrl: freezed == imageUrl
                ? _value.imageUrl
                : imageUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            imageUrls: freezed == imageUrls
                ? _value.imageUrls
                : imageUrls // ignore: cast_nullable_to_non_nullable
                      as List<String>?,
            videoUrl: freezed == videoUrl
                ? _value.videoUrl
                : videoUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            styleDescription: freezed == styleDescription
                ? _value.styleDescription
                : styleDescription // ignore: cast_nullable_to_non_nullable
                      as String?,
            contextDescription: freezed == contextDescription
                ? _value.contextDescription
                : contextDescription // ignore: cast_nullable_to_non_nullable
                      as String?,
            representativeCharacter: freezed == representativeCharacter
                ? _value.representativeCharacter
                : representativeCharacter // ignore: cast_nullable_to_non_nullable
                      as String?,
            status: freezed == status
                ? _value.status
                : status // ignore: cast_nullable_to_non_nullable
                      as ContentStatusEnum?,
            tags: freezed == tags
                ? _value.tags
                : tags // ignore: cast_nullable_to_non_nullable
                      as List<String>?,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$UpdateContentRequestImplCopyWith<$Res>
    implements $UpdateContentRequestCopyWith<$Res> {
  factory _$$UpdateContentRequestImplCopyWith(
    _$UpdateContentRequestImpl value,
    $Res Function(_$UpdateContentRequestImpl) then,
  ) = __$$UpdateContentRequestImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String? productId,
    AdTypeEnum? adType,
    String? title,
    String? textContent,
    String? richTextJson,
    int? richTextVersion,
    String? imageUrl,
    List<String>? imageUrls,
    String? videoUrl,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    ContentStatusEnum? status,
    List<String>? tags,
  });
}

/// @nodoc
class __$$UpdateContentRequestImplCopyWithImpl<$Res>
    extends _$UpdateContentRequestCopyWithImpl<$Res, _$UpdateContentRequestImpl>
    implements _$$UpdateContentRequestImplCopyWith<$Res> {
  __$$UpdateContentRequestImplCopyWithImpl(
    _$UpdateContentRequestImpl _value,
    $Res Function(_$UpdateContentRequestImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of UpdateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? productId = freezed,
    Object? adType = freezed,
    Object? title = freezed,
    Object? textContent = freezed,
    Object? richTextJson = freezed,
    Object? richTextVersion = freezed,
    Object? imageUrl = freezed,
    Object? imageUrls = freezed,
    Object? videoUrl = freezed,
    Object? styleDescription = freezed,
    Object? contextDescription = freezed,
    Object? representativeCharacter = freezed,
    Object? status = freezed,
    Object? tags = freezed,
  }) {
    return _then(
      _$UpdateContentRequestImpl(
        productId: freezed == productId
            ? _value.productId
            : productId // ignore: cast_nullable_to_non_nullable
                  as String?,
        adType: freezed == adType
            ? _value.adType
            : adType // ignore: cast_nullable_to_non_nullable
                  as AdTypeEnum?,
        title: freezed == title
            ? _value.title
            : title // ignore: cast_nullable_to_non_nullable
                  as String?,
        textContent: freezed == textContent
            ? _value.textContent
            : textContent // ignore: cast_nullable_to_non_nullable
                  as String?,
        richTextJson: freezed == richTextJson
            ? _value.richTextJson
            : richTextJson // ignore: cast_nullable_to_non_nullable
                  as String?,
        richTextVersion: freezed == richTextVersion
            ? _value.richTextVersion
            : richTextVersion // ignore: cast_nullable_to_non_nullable
                  as int?,
        imageUrl: freezed == imageUrl
            ? _value.imageUrl
            : imageUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        imageUrls: freezed == imageUrls
            ? _value._imageUrls
            : imageUrls // ignore: cast_nullable_to_non_nullable
                  as List<String>?,
        videoUrl: freezed == videoUrl
            ? _value.videoUrl
            : videoUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        styleDescription: freezed == styleDescription
            ? _value.styleDescription
            : styleDescription // ignore: cast_nullable_to_non_nullable
                  as String?,
        contextDescription: freezed == contextDescription
            ? _value.contextDescription
            : contextDescription // ignore: cast_nullable_to_non_nullable
                  as String?,
        representativeCharacter: freezed == representativeCharacter
            ? _value.representativeCharacter
            : representativeCharacter // ignore: cast_nullable_to_non_nullable
                  as String?,
        status: freezed == status
            ? _value.status
            : status // ignore: cast_nullable_to_non_nullable
                  as ContentStatusEnum?,
        tags: freezed == tags
            ? _value._tags
            : tags // ignore: cast_nullable_to_non_nullable
                  as List<String>?,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$UpdateContentRequestImpl implements _UpdateContentRequest {
  const _$UpdateContentRequestImpl({
    this.productId,
    this.adType,
    this.title,
    this.textContent,
    this.richTextJson,
    this.richTextVersion,
    this.imageUrl,
    final List<String>? imageUrls,
    this.videoUrl,
    this.styleDescription,
    this.contextDescription,
    this.representativeCharacter,
    this.status,
    final List<String>? tags,
  }) : _imageUrls = imageUrls,
       _tags = tags;

  factory _$UpdateContentRequestImpl.fromJson(Map<String, dynamic> json) =>
      _$$UpdateContentRequestImplFromJson(json);

  @override
  final String? productId;
  @override
  final AdTypeEnum? adType;
  @override
  final String? title;
  @override
  final String? textContent;
  @override
  final String? richTextJson;
  @override
  final int? richTextVersion;
  @override
  final String? imageUrl;
  final List<String>? _imageUrls;
  @override
  List<String>? get imageUrls {
    final value = _imageUrls;
    if (value == null) return null;
    if (_imageUrls is EqualUnmodifiableListView) return _imageUrls;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  @override
  final String? videoUrl;
  @override
  final String? styleDescription;
  @override
  final String? contextDescription;
  @override
  final String? representativeCharacter;
  @override
  final ContentStatusEnum? status;
  final List<String>? _tags;
  @override
  List<String>? get tags {
    final value = _tags;
    if (value == null) return null;
    if (_tags is EqualUnmodifiableListView) return _tags;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  @override
  String toString() {
    return 'UpdateContentRequest(productId: $productId, adType: $adType, title: $title, textContent: $textContent, richTextJson: $richTextJson, richTextVersion: $richTextVersion, imageUrl: $imageUrl, imageUrls: $imageUrls, videoUrl: $videoUrl, styleDescription: $styleDescription, contextDescription: $contextDescription, representativeCharacter: $representativeCharacter, status: $status, tags: $tags)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$UpdateContentRequestImpl &&
            (identical(other.productId, productId) ||
                other.productId == productId) &&
            (identical(other.adType, adType) || other.adType == adType) &&
            (identical(other.title, title) || other.title == title) &&
            (identical(other.textContent, textContent) ||
                other.textContent == textContent) &&
            (identical(other.richTextJson, richTextJson) ||
                other.richTextJson == richTextJson) &&
            (identical(other.richTextVersion, richTextVersion) ||
                other.richTextVersion == richTextVersion) &&
            (identical(other.imageUrl, imageUrl) ||
                other.imageUrl == imageUrl) &&
            const DeepCollectionEquality().equals(
              other._imageUrls,
              _imageUrls,
            ) &&
            (identical(other.videoUrl, videoUrl) ||
                other.videoUrl == videoUrl) &&
            (identical(other.styleDescription, styleDescription) ||
                other.styleDescription == styleDescription) &&
            (identical(other.contextDescription, contextDescription) ||
                other.contextDescription == contextDescription) &&
            (identical(
                  other.representativeCharacter,
                  representativeCharacter,
                ) ||
                other.representativeCharacter == representativeCharacter) &&
            (identical(other.status, status) || other.status == status) &&
            const DeepCollectionEquality().equals(other._tags, _tags));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    productId,
    adType,
    title,
    textContent,
    richTextJson,
    richTextVersion,
    imageUrl,
    const DeepCollectionEquality().hash(_imageUrls),
    videoUrl,
    styleDescription,
    contextDescription,
    representativeCharacter,
    status,
    const DeepCollectionEquality().hash(_tags),
  );

  /// Create a copy of UpdateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$UpdateContentRequestImplCopyWith<_$UpdateContentRequestImpl>
  get copyWith =>
      __$$UpdateContentRequestImplCopyWithImpl<_$UpdateContentRequestImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$UpdateContentRequestImplToJson(this);
  }
}

abstract class _UpdateContentRequest implements UpdateContentRequest {
  const factory _UpdateContentRequest({
    final String? productId,
    final AdTypeEnum? adType,
    final String? title,
    final String? textContent,
    final String? richTextJson,
    final int? richTextVersion,
    final String? imageUrl,
    final List<String>? imageUrls,
    final String? videoUrl,
    final String? styleDescription,
    final String? contextDescription,
    final String? representativeCharacter,
    final ContentStatusEnum? status,
    final List<String>? tags,
  }) = _$UpdateContentRequestImpl;

  factory _UpdateContentRequest.fromJson(Map<String, dynamic> json) =
      _$UpdateContentRequestImpl.fromJson;

  @override
  String? get productId;
  @override
  AdTypeEnum? get adType;
  @override
  String? get title;
  @override
  String? get textContent;
  @override
  String? get richTextJson;
  @override
  int? get richTextVersion;
  @override
  String? get imageUrl;
  @override
  List<String>? get imageUrls;
  @override
  String? get videoUrl;
  @override
  String? get styleDescription;
  @override
  String? get contextDescription;
  @override
  String? get representativeCharacter;
  @override
  ContentStatusEnum? get status;
  @override
  List<String>? get tags;

  /// Create a copy of UpdateContentRequest
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$UpdateContentRequestImplCopyWith<_$UpdateContentRequestImpl>
  get copyWith => throw _privateConstructorUsedError;
}
