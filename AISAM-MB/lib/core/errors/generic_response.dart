import 'package:freezed_annotation/freezed_annotation.dart';

part 'generic_response.g.dart';

@JsonSerializable(genericArgumentFactories: true)
class GenericResponse<T> {
  final bool success;
  final String? message;
  final int statusCode;
  final T? data;
  final ErrorDetails? error;

  GenericResponse({
    required this.success,
    this.message,
    required this.statusCode,
    this.data,
    this.error,
  });

  factory GenericResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Object? json) fromJsonT,
  ) =>
      _$GenericResponseFromJson(json, fromJsonT);

  Map<String, dynamic> toJson(Object? Function(T value) toJsonT) =>
      _$GenericResponseToJson(this, toJsonT);
}

@JsonSerializable()
class ErrorDetails {
  final String? errorCode;
  final String? errorMessage;

  ErrorDetails({this.errorCode, this.errorMessage});

  factory ErrorDetails.fromJson(Map<String, dynamic> json) =>
      _$ErrorDetailsFromJson(json);

  Map<String, dynamic> toJson() => _$ErrorDetailsToJson(this);
}
