import 'package:freezed_annotation/freezed_annotation.dart';

part 'notification_filter_model.freezed.dart';

@freezed
class NotificationFilterModel with _$NotificationFilterModel {
  const factory NotificationFilterModel({
    int? type,
    DateTime? fromDate,
    DateTime? toDate,
  }) = _NotificationFilterModel;
}
