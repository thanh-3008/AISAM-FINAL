import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/ai_content_repository.dart';
import '../../data/models/ai_generation_request.dart';
import '../../data/models/ai_generation_response.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/state/base_state.dart';
import 'content_list_controller.dart';
import '../../../billing/presentation/providers/billing_controller.dart';
import '../../../dashboard/presentation/dashboard_controller.dart';

part 'ai_generation_controller.g.dart';

@riverpod
class AiGenerationController extends _$AiGenerationController {
  @override
  BaseState<AiGenerationResponseModel> build() {
    return const BaseState.initial();
  }

  Future<void> generateDraft(CreateDraftRequest request) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(aiContentRepositoryProvider);
      final response = await repository.generateDraft(request);
      ref.invalidate(billingControllerProvider);
      ref.invalidate(dashboardControllerProvider);
      state = BaseState.data(response);
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }

  Future<void> improveContent(String contentId, ImproveContentRequest request) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(aiContentRepositoryProvider);
      final response = await repository.improveContent(contentId, request);
      ref.invalidate(billingControllerProvider);
      ref.invalidate(dashboardControllerProvider);
      state = BaseState.data(response);
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }

  Future<void> approveGeneration(String aiGenerationId) async {
    state = const BaseState.loading();
    try {
      final repository = ref.read(aiContentRepositoryProvider);
      await repository.approveGeneration(aiGenerationId);
      // Trigger content refresh, billing quota refresh and dashboard summary refresh
      ref.read(contentListControllerProvider.notifier).refresh();
      ref.invalidate(billingControllerProvider);
      ref.invalidate(dashboardControllerProvider);
      state = const BaseState.initial();
    } catch (e) {
      state = BaseState.error(ExceptionHandler.handle(e));
    }
  }
}
