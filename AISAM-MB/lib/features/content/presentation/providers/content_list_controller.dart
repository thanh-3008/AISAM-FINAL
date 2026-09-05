import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../../data/repositories/content_repository.dart';
import '../../data/models/content_model.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../core/network/access_events.dart';
import '../../../access/presentation/access_providers.dart';

part 'content_list_controller.g.dart';

@riverpod
class ContentListController extends _$ContentListController {
  int _page = 1;
  final int _pageSize = 10;
  bool _hasMore = true;
  int _generation = 0;

  @override
  AsyncValue<List<ContentResponseModel>> build() {
    ++_generation;
    ref.onDispose(() => ++_generation);
    ref.watch(accessContextProvider);
    if (ref.watch(accessDeniedProvider)) return const AsyncValue.data([]);
    ++_generation;
    _fetchContents(isRefresh: true);
    return const AsyncValue.loading();
  }

  Future<void> _fetchContents({bool isRefresh = false}) async {
    final generation = _generation;
    if (isRefresh) {
      _page = 1;
      _hasMore = true;
    } else if (!_hasMore) {
      return;
    }

    try {
      await ref.read(accessContextProvider.future);
      final repository = ref.read(contentRepositoryProvider);
      final newItems = await repository.getContents(pageNumber: _page, pageSize: _pageSize);
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      
      if (newItems.length < _pageSize) {
        _hasMore = false;
      }

      if (isRefresh) {
        state = AsyncValue.data(newItems);
      } else {
        state = AsyncValue.data([...state.value ?? [], ...newItems]);
      }
      _page++;
    } catch (e, st) {
      if (generation != _generation || ref.read(accessDeniedProvider)) return;
      if (isRefresh) {
        state = AsyncValue.error(ExceptionHandler.handle(e), st);
      } else {
        // Handle pagination error silently or set to state if needed
      }
    }
  }

  Future<void> refresh() => _fetchContents(isRefresh: true);
  Future<void> loadMore() => _fetchContents(isRefresh: false);
}
