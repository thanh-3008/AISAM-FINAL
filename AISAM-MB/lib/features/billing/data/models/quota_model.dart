import 'package:freezed_annotation/freezed_annotation.dart';

part 'quota_model.freezed.dart';
part 'quota_model.g.dart';

@freezed
class QuotaModel with _$QuotaModel {
  const factory QuotaModel({
    @Default('') String planName,
    @Default('') String subscriptionStatus,
    DateTime? windowStart,
    DateTime? windowEnd,
    @Default(0) int promptQuotaLimit,
    @Default(0) int promptUsage,
    @Default(0) int promptRemaining,
    @Default(0) int postQuotaLimit,
    @Default(0) int postUsage,
    @Default(0) int postRemaining,
    @Default(0) int textContentCount,
    @Default(0) int imageContentCount,
    @Default(0) int videoContentCount,
  }) = _QuotaModel;

  factory QuotaModel.fromJson(Map<String, dynamic> json) => _$QuotaModelFromJson(json);
}
