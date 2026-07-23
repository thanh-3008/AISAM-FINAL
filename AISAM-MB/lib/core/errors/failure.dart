class Failure implements Exception {
  final String message;
  final String? code;
  final int? statusCode;

  const Failure(this.message, {this.code, this.statusCode});

  @override
  String toString() => message;
}
