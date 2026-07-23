import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:internet_connection_checker/internet_connection_checker.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'logger_service.dart';

part 'connectivity_service.g.dart';

class ConnectivityService {
  final Connectivity _connectivity;
  final InternetConnectionChecker _internetChecker;

  ConnectivityService(this._connectivity, this._internetChecker);

  Future<bool> get isConnected async {
    final connectivityResult = await _connectivity.checkConnectivity();
    if (connectivityResult.contains(ConnectivityResult.none)) {
      LoggerService.w('No network connection available.');
      return false;
    }
    final hasInternet = await _internetChecker.hasConnection;
    if (!hasInternet) {
      LoggerService.w('Network connected but no internet access.');
    }
    return hasInternet;
  }

  Stream<bool> get onConnectivityChanged async* {
    await for (final result in _connectivity.onConnectivityChanged) {
      if (result.contains(ConnectivityResult.none)) {
        yield false;
      } else {
        yield await _internetChecker.hasConnection;
      }
    }
  }
}

@riverpod
ConnectivityService connectivityService(ConnectivityServiceRef ref) {
  return ConnectivityService(
    Connectivity(),
    InternetConnectionChecker.createInstance(),
  );
}

