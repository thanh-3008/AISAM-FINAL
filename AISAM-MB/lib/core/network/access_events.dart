import 'package:flutter_riverpod/flutter_riverpod.dart';

// A denial clears protected UI independently of login/session state.
final accessRevisionProvider = StateProvider<int>((ref) => 0);
final accessDeniedProvider = StateProvider<bool>((ref) => false);
