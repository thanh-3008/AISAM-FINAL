import 'package:freezed_annotation/freezed_annotation.dart';
import '../errors/app_exception.dart';

part 'base_state.freezed.dart';

@freezed
class BaseState<T> with _$BaseState<T> {
  const factory BaseState.initial() = _Initial<T>;
  const factory BaseState.loading() = _Loading<T>;
  const factory BaseState.data(T data) = _Data<T>;
  const factory BaseState.empty() = _Empty<T>;
  const factory BaseState.error(AppException error) = _Error<T>;
}
