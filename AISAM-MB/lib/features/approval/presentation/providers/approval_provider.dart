import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/approval_repository.dart';
import '../../../content/data/models/content_model.dart';
import 'package:flutter/foundation.dart';

part 'approval_provider.g.dart';

@riverpod
class ApprovalNotifier extends _$ApprovalNotifier {
  @override
  Future<List<ContentResponseModel>> build() async {
    return _fetchPendingApprovals();
  }

  Future<List<ContentResponseModel>> _fetchPendingApprovals() async {
    final repository = ref.read(approvalRepositoryProvider);
    return repository.getPendingApprovals(page: 1, pageSize: 100);
  }


  Future<bool> approveContent(String id) async {
    try {
      final repository = ref.read(approvalRepositoryProvider);
      await repository.approveContent(id);
      await refresh();
      return true;
    } catch (e) {
      debugPrint('Approve error: $e');
      return false;
    }
  }

  Future<bool> rejectContent(String id, {String? reason}) async {
    try {
      final repository = ref.read(approvalRepositoryProvider);
      await repository.rejectContent(id, reason: reason);
      await refresh();
      return true;
    } catch (e) {
      debugPrint('Reject error: $e');
      return false;
    }
  }

  Future<bool> undoContent(String id) async {
    try {
      final repository = ref.read(approvalRepositoryProvider);
      await repository.undoContent(id);
      await refresh();
      return true;
    } catch (e) {
      debugPrint('Undo error: $e');
      return false;
    }
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchPendingApprovals());
    ref.read(historyApprovalNotifierProvider.notifier).refresh();
  }
}

@riverpod
class HistoryApprovalNotifier extends _$HistoryApprovalNotifier {
  @override
  Future<List<ContentResponseModel>> build() async {
    return _fetchHistoryApprovals();
  }

  Future<List<ContentResponseModel>> _fetchHistoryApprovals() async {
    final repository = ref.read(approvalRepositoryProvider);
    return repository.getHistoryApprovals(page: 1, pageSize: 50);
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchHistoryApprovals());
  }
}
