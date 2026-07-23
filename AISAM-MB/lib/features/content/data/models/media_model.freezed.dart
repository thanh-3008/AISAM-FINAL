// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'media_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

ContentMediaUploadResponseModel _$ContentMediaUploadResponseModelFromJson(
  Map<String, dynamic> json,
) {
  return _ContentMediaUploadResponseModel.fromJson(json);
}

/// @nodoc
mixin _$ContentMediaUploadResponseModel {
  String get url => throw _privateConstructorUsedError;
  String get fileName => throw _privateConstructorUsedError;
  String get contentType => throw _privateConstructorUsedError;
  int get size => throw _privateConstructorUsedError;

  /// Serializes this ContentMediaUploadResponseModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of ContentMediaUploadResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $ContentMediaUploadResponseModelCopyWith<ContentMediaUploadResponseModel>
  get copyWith => throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ContentMediaUploadResponseModelCopyWith<$Res> {
  factory $ContentMediaUploadResponseModelCopyWith(
    ContentMediaUploadResponseModel value,
    $Res Function(ContentMediaUploadResponseModel) then,
  ) =
      _$ContentMediaUploadResponseModelCopyWithImpl<
        $Res,
        ContentMediaUploadResponseModel
      >;
  @useResult
  $Res call({String url, String fileName, String contentType, int size});
}

/// @nodoc
class _$ContentMediaUploadResponseModelCopyWithImpl<
  $Res,
  $Val extends ContentMediaUploadResponseModel
>
    implements $ContentMediaUploadResponseModelCopyWith<$Res> {
  _$ContentMediaUploadResponseModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of ContentMediaUploadResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? url = null,
    Object? fileName = null,
    Object? contentType = null,
    Object? size = null,
  }) {
    return _then(
      _value.copyWith(
            url: null == url
                ? _value.url
                : url // ignore: cast_nullable_to_non_nullable
                      as String,
            fileName: null == fileName
                ? _value.fileName
                : fileName // ignore: cast_nullable_to_non_nullable
                      as String,
            contentType: null == contentType
                ? _value.contentType
                : contentType // ignore: cast_nullable_to_non_nullable
                      as String,
            size: null == size
                ? _value.size
                : size // ignore: cast_nullable_to_non_nullable
                      as int,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$ContentMediaUploadResponseModelImplCopyWith<$Res>
    implements $ContentMediaUploadResponseModelCopyWith<$Res> {
  factory _$$ContentMediaUploadResponseModelImplCopyWith(
    _$ContentMediaUploadResponseModelImpl value,
    $Res Function(_$ContentMediaUploadResponseModelImpl) then,
  ) = __$$ContentMediaUploadResponseModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({String url, String fileName, String contentType, int size});
}

/// @nodoc
class __$$ContentMediaUploadResponseModelImplCopyWithImpl<$Res>
    extends
        _$ContentMediaUploadResponseModelCopyWithImpl<
          $Res,
          _$ContentMediaUploadResponseModelImpl
        >
    implements _$$ContentMediaUploadResponseModelImplCopyWith<$Res> {
  __$$ContentMediaUploadResponseModelImplCopyWithImpl(
    _$ContentMediaUploadResponseModelImpl _value,
    $Res Function(_$ContentMediaUploadResponseModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of ContentMediaUploadResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? url = null,
    Object? fileName = null,
    Object? contentType = null,
    Object? size = null,
  }) {
    return _then(
      _$ContentMediaUploadResponseModelImpl(
        url: null == url
            ? _value.url
            : url // ignore: cast_nullable_to_non_nullable
                  as String,
        fileName: null == fileName
            ? _value.fileName
            : fileName // ignore: cast_nullable_to_non_nullable
                  as String,
        contentType: null == contentType
            ? _value.contentType
            : contentType // ignore: cast_nullable_to_non_nullable
                  as String,
        size: null == size
            ? _value.size
            : size // ignore: cast_nullable_to_non_nullable
                  as int,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$ContentMediaUploadResponseModelImpl
    implements _ContentMediaUploadResponseModel {
  const _$ContentMediaUploadResponseModelImpl({
    required this.url,
    required this.fileName,
    required this.contentType,
    required this.size,
  });

  factory _$ContentMediaUploadResponseModelImpl.fromJson(
    Map<String, dynamic> json,
  ) => _$$ContentMediaUploadResponseModelImplFromJson(json);

  @override
  final String url;
  @override
  final String fileName;
  @override
  final String contentType;
  @override
  final int size;

  @override
  String toString() {
    return 'ContentMediaUploadResponseModel(url: $url, fileName: $fileName, contentType: $contentType, size: $size)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ContentMediaUploadResponseModelImpl &&
            (identical(other.url, url) || other.url == url) &&
            (identical(other.fileName, fileName) ||
                other.fileName == fileName) &&
            (identical(other.contentType, contentType) ||
                other.contentType == contentType) &&
            (identical(other.size, size) || other.size == size));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode =>
      Object.hash(runtimeType, url, fileName, contentType, size);

  /// Create a copy of ContentMediaUploadResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$ContentMediaUploadResponseModelImplCopyWith<
    _$ContentMediaUploadResponseModelImpl
  >
  get copyWith =>
      __$$ContentMediaUploadResponseModelImplCopyWithImpl<
        _$ContentMediaUploadResponseModelImpl
      >(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$ContentMediaUploadResponseModelImplToJson(this);
  }
}

abstract class _ContentMediaUploadResponseModel
    implements ContentMediaUploadResponseModel {
  const factory _ContentMediaUploadResponseModel({
    required final String url,
    required final String fileName,
    required final String contentType,
    required final int size,
  }) = _$ContentMediaUploadResponseModelImpl;

  factory _ContentMediaUploadResponseModel.fromJson(Map<String, dynamic> json) =
      _$ContentMediaUploadResponseModelImpl.fromJson;

  @override
  String get url;
  @override
  String get fileName;
  @override
  String get contentType;
  @override
  int get size;

  /// Create a copy of ContentMediaUploadResponseModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$ContentMediaUploadResponseModelImplCopyWith<
    _$ContentMediaUploadResponseModelImpl
  >
  get copyWith => throw _privateConstructorUsedError;
}
