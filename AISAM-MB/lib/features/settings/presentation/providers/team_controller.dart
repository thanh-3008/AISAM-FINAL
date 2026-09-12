import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/team_model.dart';
import '../../data/repositories/team_repository.dart';
import '../../../workspace/presentation/providers/workspace_controller.dart';
import '../../../../core/errors/app_exception.dart';

// Provider for list of teams in the current workspace
final teamsControllerProvider =
    StateNotifierProvider.autoDispose<TeamsController, AsyncValue<List<TeamSummaryModel>>>((ref) {
  return TeamsController(ref);
});

class TeamsController extends StateNotifier<AsyncValue<List<TeamSummaryModel>>> {
  final Ref _ref;

  TeamsController(this._ref) : super(const AsyncValue.loading()) {
    _ref.watch(activeWorkspaceControllerProvider);
    fetchTeams();
  }

  Future<void> fetchTeams() async {
    try {
      state = const AsyncValue.loading();
      final repo = _ref.read(teamRepositoryProvider);
      final teams = await repo.getTeams();
      if (mounted) state = AsyncValue.data(teams);
    } catch (e, st) {
      if (mounted) state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> createTeam(String name, String? description) async {
    final repo = _ref.read(teamRepositoryProvider);
    await repo.createTeam(name: name, description: description);
    await fetchTeams();
  }

  Future<void> addMemberToTeam(String teamId, String userId, {String role = 'ContentCreator'}) async {
    final repo = _ref.read(teamRepositoryProvider);
    await repo.addTeamMember(teamId: teamId, userId: userId, role: role);
    _ref.invalidate(teamDetailControllerProvider(teamId));
    await fetchTeams();
  }

  Future<void> removeMemberFromTeam(String teamId, String userId) async {
    final repo = _ref.read(teamRepositoryProvider);
    await repo.removeTeamMember(teamId: teamId, userId: userId);
    _ref.invalidate(teamDetailControllerProvider(teamId));
    await fetchTeams();
  }

  Future<void> refresh() async {
    await fetchTeams();
  }
}

// Provider for detailed info of a single team (members & assigned brands)
final teamDetailControllerProvider = FutureProvider.autoDispose.family<TeamDetailModel, String>((ref, teamId) async {
  final repo = ref.read(teamRepositoryProvider);
  return repo.getTeamById(teamId);
});

// Provider for workspace credit wallet balance (shared pool)
final workspaceWalletControllerProvider =
    StateNotifierProvider.autoDispose<WorkspaceWalletController, AsyncValue<CreditWalletModel>>((ref) {
  return WorkspaceWalletController(ref);
});

class WorkspaceWalletController extends StateNotifier<AsyncValue<CreditWalletModel>> {
  final Ref _ref;

  WorkspaceWalletController(this._ref) : super(const AsyncValue.loading()) {
    _ref.watch(activeWorkspaceControllerProvider);
    fetchWallet();
  }

  Future<void> fetchWallet() async {
    try {
      state = const AsyncValue.loading();
      final repo = _ref.read(teamRepositoryProvider);
      final wallet = await repo.getWorkspaceWallet();
      if (mounted) state = AsyncValue.data(wallet);
    } catch (e, st) {
      if (mounted) state = AsyncValue.error(ExceptionHandler.handle(e), st);
    }
  }

  Future<void> refresh() async {
    await fetchWallet();
  }
}
