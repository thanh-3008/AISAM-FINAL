// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'content_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

ContentResponseModel _$ContentResponseModelFromJson(Map<String, dynamic> json) {
  return _ContentResponseModel.fromJson(json);
}

/// @nodoc
mixin _$ContentResponseModel {
  String get id => throw _privateConstructorUsedError;
  String get profileId => throw _privateConstructorUsedError;
  String get brandId => throw _privateConstructorUsedError;
  String? get workspaceId => throw _privateConstructorUsedError;
  String? get brandName => throw _privateConstructorUsedError;
  String? get productId => throw _privateConstructorUsedError;
  AdTypeEnum get adType => throw _privateConstructorUsedError;
  String? get title => throw _privateConstructorUsedError;
  String get textContent => throw _privateConstructorUsedError;
  String? get imageUrl => throw _privateConstructorUsedError;
  String? get videoUrl => throw _privateConstructorUsedError;
  String? get thumbnailUrl => throw _privateConstructorUsedError;
  String? get tags => throw _privateConstructorUsedError;
  String? get styleDescription => throw _privateConstructorUsedError;
  String? get contextDescription => throw _privateConstructorUsedError;
  String? get representativeCharacter => throw _privateConstructorUsedError;
  String? get platformRejectionReason => throw _privateConstructorUsedError;
  String? get rejectedPlatform => throw _privateConstructorUsedError;
  bool get isAiGenerated => throw _privateConstructorUsedError;
  ContentStatusEnum get status => throw _privateConstructorUsedError;
  DateTime get createdAt => throw _privateConstructorUsedError;
  DateTime get updatedAt => throw _privateConstructorUsedError;

  /// Serializes this ContentResponseModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of ContentResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $ContentResponseModelCopyWith<ContentResponseModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ContentResponseModelCopyWith<$Res> {
  factory $ContentResponseModelCopyWith(
    ContentResponseModel value,
    $Res Function(ContentResponseModel) then,
  ) = _$ContentResponseModelCopyWithImpl<$Res, ContentResponseModel>;
  @useResult
  $Res call({
    String id,
    String profileId,
    String brandId,
    String? workspaceId,
    String? brandName,
    String? productId,
    AdTypeEnum adType,
    String? title,
    String textContent,
    String? imageUrl,
    String? videoUrl,
    String? thumbnailUrl,
    String? tags,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    String? platformRejectionReason,
    String? rejectedPlatform,
    bool isAiGenerated,
    ContentStatusEnum status,
    DateTime createdAt,
    DateTime updatedAt,
  });
}

/// @nodoc
class _$ContentResponseModelCopyWithImpl<
  $Res,
  $Val extends ContentResponseModel
>
    implements $ContentResponseModelCopyWith<$Res> {
  _$ContentResponseModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of ContentResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? profileId = null,
    Object? brandId = null,
    Object? workspaceId = freezed,
    Object? brandName = freezed,
    Object? productId = freezed,
    Object? adType = null,
    Object? title = freezed,
    Object? textContent = null,
    Object? imageUrl = freezed,
    Object? videoUrl = freezed,
    Object? thumbnailUrl = freezed,
    Object? tags = freezed,
    Object? styleDescription = freezed,
    Object? contextDescription = freezed,
    Object? representativeCharacter = freezed,
    Object? platformRejectionReason = freezed,
    Object? rejectedPlatform = freezed,
    Object? isAiGenerated = null,
    Object? status = null,
    Object? createdAt = null,
    Object? updatedAt = null,
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
            brandId: null == brandId
                ? _value.brandId
                : brandId // ignore: cast_nullable_to_non_nullable
                      as String,
            workspaceId: freezed == workspaceId
                ? _value.workspaceId
                : workspaceId // ignore: cast_nullable_to_non_nullable
                      as String?,
            brandName: freezed == brandName
                ? _value.brandName
                : brandName // ignore: cast_nullable_to_non_nullable
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
            imageUrl: freezed == imageUrl
                ? _value.imageUrl
                : imageUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            videoUrl: freezed == videoUrl
                ? _value.videoUrl
                : videoUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            thumbnailUrl: freezed == thumbnailUrl
                ? _value.thumbnailUrl
                : thumbnailUrl // ignore: cast_nullable_to_non_nullable
                      as String?,
            tags: freezed == tags
                ? _value.tags
                : tags // ignore: cast_nullable_to_non_nullable
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
            platformRejectionReason: freezed == platformRejectionReason
                ? _value.platformRejectionReason
                : platformRejectionReason // ignore: cast_nullable_to_non_nullable
                      as String?,
            rejectedPlatform: freezed == rejectedPlatform
                ? _value.rejectedPlatform
                : rejectedPlatform // ignore: cast_nullable_to_non_nullable
                      as String?,
            isAiGenerated: null == isAiGenerated
                ? _value.isAiGenerated
                : isAiGenerated // ignore: cast_nullable_to_non_nullable
                      as bool,
            status: null == status
                ? _value.status
                : status // ignore: cast_nullable_to_non_nullable
                      as ContentStatusEnum,
            createdAt: null == createdAt
                ? _value.createdAt
                : createdAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
            updatedAt: null == updatedAt
                ? _value.updatedAt
                : updatedAt // ignore: cast_nullable_to_non_nullable
                      as DateTime,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$ContentResponseModelImplCopyWith<$Res>
    implements $ContentResponseModelCopyWith<$Res> {
  factory _$$ContentResponseModelImplCopyWith(
    _$ContentResponseModelImpl value,
    $Res Function(_$ContentResponseModelImpl) then,
  ) = __$$ContentResponseModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String id,
    String profileId,
    String brandId,
    String? workspaceId,
    String? brandName,
    String? productId,
    AdTypeEnum adType,
    String? title,
    String textContent,
    String? imageUrl,
    String? videoUrl,
    String? thumbnailUrl,
    String? tags,
    String? styleDescription,
    String? contextDescription,
    String? representativeCharacter,
    String? platformRejectionReason,
    String? rejectedPlatform,
    bool isAiGenerated,
    ContentStatusEnum status,
    DateTime createdAt,
    DateTime updatedAt,
  });
}

/// @nodoc
class __$$ContentResponseModelImplCopyWithImpl<$Res>
    extends _$ContentResponseModelCopyWithImpl<$Res, _$ContentResponseModelImpl>
    implements _$$ContentResponseModelImplCopyWith<$Res> {
  __$$ContentResponseModelImplCopyWithImpl(
    _$ContentResponseModelImpl _value,
    $Res Function(_$ContentResponseModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of ContentResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? profileId = null,
    Object? brandId = null,
    Object? workspaceId = freezed,
    Object? brandName = freezed,
    Object? productId = freezed,
    Object? adType = null,
    Object? title = freezed,
    Object? textContent = null,
    Object? imageUrl = freezed,
    Object? videoUrl = freezed,
    Object? thumbnailUrl = freezed,
    Object? tags = freezed,
    Object? styleDescription = freezed,
    Object? contextDescription = freezed,
    Object? representativeCharacter = freezed,
    Object? platformRejectionReason = freezed,
    Object? rejectedPlatform = freezed,
    Object? isAiGenerated = null,
    Object? status = null,
    Object? createdAt = null,
    Object? updatedAt = null,
  }) {
    return _then(
      _$ContentResponseModelImpl(
        id: null == id
            ? _value.id
            : id // ignore: cast_nullable_to_non_nullable
                  as String,
        profileId: null == profileId
            ? _value.profileId
            : profileId // ignore: cast_nullable_to_non_nullable
                  as String,
        brandId: null == brandId
            ? _value.brandId
            : brandId // ignore: cast_nullable_to_non_nullable
                  as String,
        workspaceId: freezed == workspaceId
            ? _value.workspaceId
            : workspaceId // ignore: cast_nullable_to_non_nullable
                  as String?,
        brandName: freezed == brandName
            ? _value.brandName
            : brandName // ignore: cast_nullable_to_non_nullable
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
        imageUrl: freezed == imageUrl
            ? _value.imageUrl
            : imageUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        videoUrl: freezed == videoUrl
            ? _value.videoUrl
            : videoUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        thumbnailUrl: freezed == thumbnailUrl
            ? _value.thumbnailUrl
            : thumbnailUrl // ignore: cast_nullable_to_non_nullable
                  as String?,
        tags: freezed == tags
            ? _value.tags
            : tags // ignore: cast_nullable_to_non_nullable
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
        platformRejectionReason: freezed == platformRejectionReason
            ? _value.platformRejectionReason
            : platformRejectionReason // ignore: cast_nullable_to_non_nullable
                  as String?,
        rejectedPlatform: freezed == rejectedPlatform
            ? _value.rejectedPlatform
            : rejectedPlatform // ignore: cast_nullable_to_non_nullable
                  as String?,
        isAiGenerated: null == isAiGenerated
            ? _value.isAiGenerated
            : isAiGenerated // ignore: cast_nullable_to_non_nullable
                  as bool,
        status: null == status
            ? _value.status
            : status // ignore: cast_nullable_to_non_nullable
                  as ContentStatusEnum,
        createdAt: null == createdAt
            ? _value.createdAt
            : createdAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
        updatedAt: null == updatedAt
            ? _value.updatedAt
            : updatedAt // ignore: cast_nullable_to_non_nullable
                  as DateTime,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$ContentResponseModelImpl implements _ContentResponseModel {
  const _$ContentResponseModelImpl({
    required this.id,
    required this.profileId,
    required this.brandId,
    this.workspaceId,
    this.brandName,
    this.productId,
    required this.adType,
    this.title,
    this.textContent = '',
    this.imageUrl,
    this.videoUrl,
    this.thumbnailUrl,
    this.tags,
    this.styleDescription,
    this.contextDescription,
    this.representativeCharacter,
    this.platformRejectionReason,
    this.rejectedPlatform,
    required this.isAiGenerated,
    required this.status,
    required this.createdAt,
    required this.updatedAt,
  });

  factory _$ContentResponseModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$ContentResponseModelImplFromJson(json);

  @override
  final String id;
  @override
  final String profileId;
  @override
  final String brandId;
  @override
  final String? workspaceId;
  @override
  final String? brandName;
  @override
  final String? productId;
  @override
  final AdTypeEnum adType;
  @override
  final String? title;
  @override
  @JsonKey()
  final String textContent;
  @override
  final String? imageUrl;
  @override
  final String? videoUrl;
  @override
  final String? thumbnailUrl;
  @override
  final String? tags;
  @override
  final String? styleDescription;
  @override
  final String? contextDescription;
  @override
  final String? representativeCharacter;
  @override
  final String? platformRejectionReason;
  @override
  final String? rejectedPlatform;
  @override
  final bool isAiGenerated;
  @override
  final ContentStatusEnum status;
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;

  @override
  String toString() {
    return 'ContentResponseModel(id: $id, profileId: $profileId, brandId: $brandId, workspaceId: $workspaceId, brandName: $brandName, productId: $productId, adType: $adType, title: $title, textContent: $textContent, imageUrl: $imageUrl, videoUrl: $videoUrl, thumbnailUrl: $thumbnailUrl, tags: $tags, styleDescription: $styleDescription, contextDescription: $contextDescription, representativeCharacter: $representativeCharacter, platformRejectionReason: $platformRejectionReason, rejectedPlatform: $rejectedPlatform, isAiGenerated: $isAiGenerated, status: $status, createdAt: $createdAt, updatedAt: $updatedAt)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ContentResponseModelImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.profileId, profileId) ||
                other.profileId == profileId) &&
            (identical(other.brandId, brandId) || other.brandId == brandId) &&
            (identical(other.workspaceId, workspaceId) ||
                other.workspaceId == workspaceId) &&
            (identical(other.brandName, brandName) ||
                other.brandName == brandName) &&
            (identical(other.productId, productId) ||
                other.productId == productId) &&
            (identical(other.adType, adType) || other.adType == adType) &&
            (identical(other.title, title) || other.title == title) &&
            (identical(other.textContent, textContent) ||
                other.textContent == textContent) &&
            (identical(other.imageUrl, imageUrl) ||
                other.imageUrl == imageUrl) &&
            (identical(other.videoUrl, videoUrl) ||
                other.videoUrl == videoUrl) &&
            (identical(other.thumbnailUrl, thumbnailUrl) ||
                other.thumbnailUrl == thumbnailUrl) &&
            (identical(other.tags, tags) || other.tags == tags) &&
            (identical(other.styleDescription, styleDescription) ||
                other.styleDescription == styleDescription) &&
            (identical(other.contextDescription, contextDescription) ||
                other.contextDescription == contextDescription) &&
            (identical(
                  other.representativeCharacter,
                  representativeCharacter,
                ) ||
                other.representativeCharacter == representativeCharacter) &&
            (identical(
                  other.platformRejectionReason,
                  platformRejectionReason,
                ) ||
                other.platformRejectionReason == platformRejectionReason) &&
            (identical(other.rejectedPlatform, rejectedPlatform) ||
                other.rejectedPlatform == rejectedPlatform) &&
            (identical(other.isAiGenerated, isAiGenerated) ||
                other.isAiGenerated == isAiGenerated) &&
            (identical(other.status, status) || other.status == status) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt) &&
            (identical(other.updatedAt, updatedAt) ||
                other.updatedAt == updatedAt));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hashAll([
    runtimeType,
    id,
    profileId,
    brandId,
    workspaceId,
    brandName,
    productId,
    adType,
    title,
    textContent,
    imageUrl,
    videoUrl,
    thumbnailUrl,
    tags,
    styleDescription,
    contextDescription,
    representativeCharacter,
    platformRejectionReason,
    rejectedPlatform,
    isAiGenerated,
    status,
    createdAt,
    updatedAt,
  ]);

  /// Create a copy of ContentResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$ContentResponseModelImplCopyWith<_$ContentResponseModelImpl>
  get copyWith =>
      __$$ContentResponseModelImplCopyWithImpl<_$ContentResponseModelImpl>(
        this,
        _$identity,
      );

  @override
  Map<String, dynamic> toJson() {
    return _$$ContentResponseModelImplToJson(this);
  }
}

abstract class _ContentResponseModel implements ContentResponseModel {
  const factory _ContentResponseModel({
    required final String id,
    required final String profileId,
    required final String brandId,
    final String? workspaceId,
    final String? brandName,
    final String? productId,
    required final AdTypeEnum adType,
    final String? title,
    final String textContent,
    final String? imageUrl,
    final String? videoUrl,
    final String? thumbnailUrl,
    final String? tags,
    final String? styleDescription,
    final String? contextDescription,
    final String? representativeCharacter,
    final String? platformRejectionReason,
    final String? rejectedPlatform,
    required final bool isAiGenerated,
    required final ContentStatusEnum status,
    required final DateTime createdAt,
    required final DateTime updatedAt,
  }) = _$ContentResponseModelImpl;

  factory _ContentResponseModel.fromJson(Map<String, dynamic> json) =
      _$ContentResponseModelImpl.fromJson;

  @override
  String get id;
  @override
  String get profileId;
  @override
  String get brandId;
  @override
  String? get workspaceId;
  @override
  String? get brandName;
  @override
  String? get productId;
  @override
  AdTypeEnum get adType;
  @override
  String? get title;
  @override
  String get textContent;
  @override
  String? get imageUrl;
  @override
  String? get videoUrl;
  @override
  String? get thumbnailUrl;
  @override
  String? get tags;
  @override
  String? get styleDescription;
  @override
  String? get contextDescription;
  @override
  String? get representativeCharacter;
  @override
  String? get platformRejectionReason;
  @override
  String? get rejectedPlatform;
  @override
  bool get isAiGenerated;
  @override
  ContentStatusEnum get status;
  @override
  DateTime get createdAt;
  @override
  DateTime get updatedAt;

  /// Create a copy of ContentResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$ContentResponseModelImplCopyWith<_$ContentResponseModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}
