// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'conversation_detail_model.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
  'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models',
);

ConversationDetailModel _$ConversationDetailModelFromJson(
  Map<String, dynamic> json,
) {
  return _ConversationDetailModel.fromJson(json);
}

/// @nodoc
mixin _$ConversationDetailModel {
  String get id => throw _privateConstructorUsedError;
  String get profileId => throw _privateConstructorUsedError;
  String? get brandId => throw _privateConstructorUsedError;
  String? get brandName => throw _privateConstructorUsedError;
  String? get productId => throw _privateConstructorUsedError;
  String? get productName => throw _privateConstructorUsedError;
  AdTypeEnum get adType => throw _privateConstructorUsedError;
  String? get title => throw _privateConstructorUsedError;
  bool get isActive => throw _privateConstructorUsedError;
  String? get lastMessage => throw _privateConstructorUsedError;
  DateTime? get lastMessageAt => throw _privateConstructorUsedError;
  int get messageCount => throw _privateConstructorUsedError;
  List<ChatMessageModel> get messages => throw _privateConstructorUsedError;

  /// Serializes this ConversationDetailModel to a JSON map.
  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;

  /// Create a copy of ConversationDetailModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  $ConversationDetailModelCopyWith<ConversationDetailModel> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ConversationDetailModelCopyWith<$Res> {
  factory $ConversationDetailModelCopyWith(
    ConversationDetailModel value,
    $Res Function(ConversationDetailModel) then,
  ) = _$ConversationDetailModelCopyWithImpl<$Res, ConversationDetailModel>;
  @useResult
  $Res call({
    String id,
    String profileId,
    String? brandId,
    String? brandName,
    String? productId,
    String? productName,
    AdTypeEnum adType,
    String? title,
    bool isActive,
    String? lastMessage,
    DateTime? lastMessageAt,
    int messageCount,
    List<ChatMessageModel> messages,
  });
}

/// @nodoc
class _$ConversationDetailModelCopyWithImpl<
  $Res,
  $Val extends ConversationDetailModel
>
    implements $ConversationDetailModelCopyWith<$Res> {
  _$ConversationDetailModelCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  /// Create a copy of ConversationDetailModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? profileId = null,
    Object? brandId = freezed,
    Object? brandName = freezed,
    Object? productId = freezed,
    Object? productName = freezed,
    Object? adType = null,
    Object? title = freezed,
    Object? isActive = null,
    Object? lastMessage = freezed,
    Object? lastMessageAt = freezed,
    Object? messageCount = null,
    Object? messages = null,
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
            brandId: freezed == brandId
                ? _value.brandId
                : brandId // ignore: cast_nullable_to_non_nullable
                      as String?,
            brandName: freezed == brandName
                ? _value.brandName
                : brandName // ignore: cast_nullable_to_non_nullable
                      as String?,
            productId: freezed == productId
                ? _value.productId
                : productId // ignore: cast_nullable_to_non_nullable
                      as String?,
            productName: freezed == productName
                ? _value.productName
                : productName // ignore: cast_nullable_to_non_nullable
                      as String?,
            adType: null == adType
                ? _value.adType
                : adType // ignore: cast_nullable_to_non_nullable
                      as AdTypeEnum,
            title: freezed == title
                ? _value.title
                : title // ignore: cast_nullable_to_non_nullable
                      as String?,
            isActive: null == isActive
                ? _value.isActive
                : isActive // ignore: cast_nullable_to_non_nullable
                      as bool,
            lastMessage: freezed == lastMessage
                ? _value.lastMessage
                : lastMessage // ignore: cast_nullable_to_non_nullable
                      as String?,
            lastMessageAt: freezed == lastMessageAt
                ? _value.lastMessageAt
                : lastMessageAt // ignore: cast_nullable_to_non_nullable
                      as DateTime?,
            messageCount: null == messageCount
                ? _value.messageCount
                : messageCount // ignore: cast_nullable_to_non_nullable
                      as int,
            messages: null == messages
                ? _value.messages
                : messages // ignore: cast_nullable_to_non_nullable
                      as List<ChatMessageModel>,
          )
          as $Val,
    );
  }
}

/// @nodoc
abstract class _$$ConversationDetailModelImplCopyWith<$Res>
    implements $ConversationDetailModelCopyWith<$Res> {
  factory _$$ConversationDetailModelImplCopyWith(
    _$ConversationDetailModelImpl value,
    $Res Function(_$ConversationDetailModelImpl) then,
  ) = __$$ConversationDetailModelImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({
    String id,
    String profileId,
    String? brandId,
    String? brandName,
    String? productId,
    String? productName,
    AdTypeEnum adType,
    String? title,
    bool isActive,
    String? lastMessage,
    DateTime? lastMessageAt,
    int messageCount,
    List<ChatMessageModel> messages,
  });
}

/// @nodoc
class __$$ConversationDetailModelImplCopyWithImpl<$Res>
    extends
        _$ConversationDetailModelCopyWithImpl<
          $Res,
          _$ConversationDetailModelImpl
        >
    implements _$$ConversationDetailModelImplCopyWith<$Res> {
  __$$ConversationDetailModelImplCopyWithImpl(
    _$ConversationDetailModelImpl _value,
    $Res Function(_$ConversationDetailModelImpl) _then,
  ) : super(_value, _then);

  /// Create a copy of ConversationDetailModel
  /// with the given fields replaced by the non-null parameter values.
  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = null,
    Object? profileId = null,
    Object? brandId = freezed,
    Object? brandName = freezed,
    Object? productId = freezed,
    Object? productName = freezed,
    Object? adType = null,
    Object? title = freezed,
    Object? isActive = null,
    Object? lastMessage = freezed,
    Object? lastMessageAt = freezed,
    Object? messageCount = null,
    Object? messages = null,
  }) {
    return _then(
      _$ConversationDetailModelImpl(
        id: null == id
            ? _value.id
            : id // ignore: cast_nullable_to_non_nullable
                  as String,
        profileId: null == profileId
            ? _value.profileId
            : profileId // ignore: cast_nullable_to_non_nullable
                  as String,
        brandId: freezed == brandId
            ? _value.brandId
            : brandId // ignore: cast_nullable_to_non_nullable
                  as String?,
        brandName: freezed == brandName
            ? _value.brandName
            : brandName // ignore: cast_nullable_to_non_nullable
                  as String?,
        productId: freezed == productId
            ? _value.productId
            : productId // ignore: cast_nullable_to_non_nullable
                  as String?,
        productName: freezed == productName
            ? _value.productName
            : productName // ignore: cast_nullable_to_non_nullable
                  as String?,
        adType: null == adType
            ? _value.adType
            : adType // ignore: cast_nullable_to_non_nullable
                  as AdTypeEnum,
        title: freezed == title
            ? _value.title
            : title // ignore: cast_nullable_to_non_nullable
                  as String?,
        isActive: null == isActive
            ? _value.isActive
            : isActive // ignore: cast_nullable_to_non_nullable
                  as bool,
        lastMessage: freezed == lastMessage
            ? _value.lastMessage
            : lastMessage // ignore: cast_nullable_to_non_nullable
                  as String?,
        lastMessageAt: freezed == lastMessageAt
            ? _value.lastMessageAt
            : lastMessageAt // ignore: cast_nullable_to_non_nullable
                  as DateTime?,
        messageCount: null == messageCount
            ? _value.messageCount
            : messageCount // ignore: cast_nullable_to_non_nullable
                  as int,
        messages: null == messages
            ? _value._messages
            : messages // ignore: cast_nullable_to_non_nullable
                  as List<ChatMessageModel>,
      ),
    );
  }
}

/// @nodoc
@JsonSerializable()
class _$ConversationDetailModelImpl implements _ConversationDetailModel {
  const _$ConversationDetailModelImpl({
    required this.id,
    required this.profileId,
    this.brandId,
    this.brandName,
    this.productId,
    this.productName,
    required this.adType,
    this.title,
    required this.isActive,
    this.lastMessage,
    this.lastMessageAt,
    required this.messageCount,
    final List<ChatMessageModel> messages = const [],
  }) : _messages = messages;

  factory _$ConversationDetailModelImpl.fromJson(Map<String, dynamic> json) =>
      _$$ConversationDetailModelImplFromJson(json);

  @override
  final String id;
  @override
  final String profileId;
  @override
  final String? brandId;
  @override
  final String? brandName;
  @override
  final String? productId;
  @override
  final String? productName;
  @override
  final AdTypeEnum adType;
  @override
  final String? title;
  @override
  final bool isActive;
  @override
  final String? lastMessage;
  @override
  final DateTime? lastMessageAt;
  @override
  final int messageCount;
  final List<ChatMessageModel> _messages;
  @override
  @JsonKey()
  List<ChatMessageModel> get messages {
    if (_messages is EqualUnmodifiableListView) return _messages;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(_messages);
  }

  @override
  String toString() {
    return 'ConversationDetailModel(id: $id, profileId: $profileId, brandId: $brandId, brandName: $brandName, productId: $productId, productName: $productName, adType: $adType, title: $title, isActive: $isActive, lastMessage: $lastMessage, lastMessageAt: $lastMessageAt, messageCount: $messageCount, messages: $messages)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ConversationDetailModelImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.profileId, profileId) ||
                other.profileId == profileId) &&
            (identical(other.brandId, brandId) || other.brandId == brandId) &&
            (identical(other.brandName, brandName) ||
                other.brandName == brandName) &&
            (identical(other.productId, productId) ||
                other.productId == productId) &&
            (identical(other.productName, productName) ||
                other.productName == productName) &&
            (identical(other.adType, adType) || other.adType == adType) &&
            (identical(other.title, title) || other.title == title) &&
            (identical(other.isActive, isActive) ||
                other.isActive == isActive) &&
            (identical(other.lastMessage, lastMessage) ||
                other.lastMessage == lastMessage) &&
            (identical(other.lastMessageAt, lastMessageAt) ||
                other.lastMessageAt == lastMessageAt) &&
            (identical(other.messageCount, messageCount) ||
                other.messageCount == messageCount) &&
            const DeepCollectionEquality().equals(other._messages, _messages));
  }

  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  int get hashCode => Object.hash(
    runtimeType,
    id,
    profileId,
    brandId,
    brandName,
    productId,
    productName,
    adType,
    title,
    isActive,
    lastMessage,
    lastMessageAt,
    messageCount,
    const DeepCollectionEquality().hash(_messages),
  );

  /// Create a copy of ConversationDetailModel
  /// with the given fields replaced by the non-null parameter values.
  @JsonKey(includeFromJson: false, includeToJson: false)
  @override
  @pragma('vm:prefer-inline')
  _$$ConversationDetailModelImplCopyWith<_$ConversationDetailModelImpl>
  get copyWith =>
      __$$ConversationDetailModelImplCopyWithImpl<
        _$ConversationDetailModelImpl
      >(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$ConversationDetailModelImplToJson(this);
  }
}

abstract class _ConversationDetailModel implements ConversationDetailModel {
  const factory _ConversationDetailModel({
    required final String id,
    required final String profileId,
    final String? brandId,
    final String? brandName,
    final String? productId,
    final String? productName,
    required final AdTypeEnum adType,
    final String? title,
    required final bool isActive,
    final String? lastMessage,
    final DateTime? lastMessageAt,
    required final int messageCount,
    final List<ChatMessageModel> messages,
  }) = _$ConversationDetailModelImpl;

  factory _ConversationDetailModel.fromJson(Map<String, dynamic> json) =
      _$ConversationDetailModelImpl.fromJson;

  @override
  String get id;
  @override
  String get profileId;
  @override
  String? get brandId;
  @override
  String? get brandName;
  @override
  String? get productId;
  @override
  String? get productName;
  @override
  AdTypeEnum get adType;
  @override
  String? get title;
  @override
  bool get isActive;
  @override
  String? get lastMessage;
  @override
  DateTime? get lastMessageAt;
  @override
  int get messageCount;
  @override
  List<ChatMessageModel> get messages;

  /// Create a copy of ConversationDetailModel
  /// with the given fields replaced by the non-null parameter values.
  @override
  @JsonKey(includeFromJson: false, includeToJson: false)
  _$$ConversationDetailModelImplCopyWith<_$ConversationDetailModelImpl>
  get copyWith => throw _privateConstructorUsedError;
}
