// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'profile_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$ProfileResponseModelImpl _$$ProfileResponseModelImplFromJson(
  Map<String, dynamic> json,
) => _$ProfileResponseModelImpl(
  id: json['id'] as String,
  userId: json['userId'] as String,
  name: json['name'] as String,
  profileType: (json['profileType'] as num).toInt(),
  subscriptionId: json['subscriptionId'] as String?,
  companyName: json['companyName'] as String?,
  bio: json['bio'] as String?,
  avatarUrl: json['avatarUrl'] as String?,
  status: (json['status'] as num).toInt(),
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt: DateTime.parse(json['updatedAt'] as String),
  isOwner: json['isOwner'] as bool,
  memberRole: json['memberRole'] as String?,
);

Map<String, dynamic> _$$ProfileResponseModelImplToJson(
  _$ProfileResponseModelImpl instance,
) => <String, dynamic>{
  'id': instance.id,
  'userId': instance.userId,
  'name': instance.name,
  'profileType': instance.profileType,
  'subscriptionId': instance.subscriptionId,
  'companyName': instance.companyName,
  'bio': instance.bio,
  'avatarUrl': instance.avatarUrl,
  'status': instance.status,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt.toIso8601String(),
  'isOwner': instance.isOwner,
  'memberRole': instance.memberRole,
};
