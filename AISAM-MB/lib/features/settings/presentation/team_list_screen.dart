import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_loading_indicator.dart';
import 'providers/language_provider.dart';
import 'providers/team_controller.dart';
import '../data/models/team_model.dart';

const Color _bgColor = Color(0xFFF7F9FB);
const Color _primaryColor = Color(0xFF003EC7);
const Color _secondaryColor = Color(0xFF6B38D4);
const Color _surfaceContainerLow = Color(0xFFF2F4F6);
const Color _surfaceContainer = Color(0xFFECEEF0);
const Color _textMain = Color(0xFF0F172A);
const Color _textMuted = Color(0xFF64748B);
const Color _onSurfaceVariant = Color(0xFF434656);
const Color _borderColor = Color.fromRGBO(226, 232, 240, 0.8);

class TeamListScreen extends ConsumerStatefulWidget {
  const TeamListScreen({super.key});

  @override
  ConsumerState<TeamListScreen> createState() => _TeamListScreenState();
}

class _TeamListScreenState extends ConsumerState<TeamListScreen> {
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final langState = ref.watch(languageControllerProvider);
    final isEn = (langState.value ?? 'vi') == 'en';
    final teamsState = ref.watch(teamsControllerProvider);

    return Scaffold(
      backgroundColor: _bgColor,
      body: CustomScrollView(
        slivers: [
          // Glass App Bar
          SliverAppBar(
            pinned: true,
            expandedHeight: 64.0,
            backgroundColor: Colors.transparent,
            elevation: 0,
            flexibleSpace: ClipRRect(
              child: BackdropFilter(
                filter: ImageFilter.blur(sigmaX: 16.0, sigmaY: 16.0),
                child: Container(
                  decoration: BoxDecoration(
                    color: _bgColor.withValues(alpha: 0.8),
                    border: const Border(bottom: BorderSide(color: _borderColor)),
                  ),
                ),
              ),
            ),
            leading: IconButton(
              icon: const Icon(Icons.arrow_back, color: _textMain),
              onPressed: () {
                if (context.canPop()) {
                  context.pop();
                } else {
                  context.go('/settings');
                }
              },
            ),
            title: Text(
              isEn ? 'Teams & Groups' : 'Chọn Nhóm (Team)',
              style: const TextStyle(
                fontFamily: 'Plus Jakarta Sans',
                fontWeight: FontWeight.w700,
                fontSize: 20,
                color: _textMain,
              ),
            ),
            centerTitle: true,
            actions: [
              IconButton(
                tooltip: isEn ? 'Create Team' : 'Tạo nhóm mới',
                icon: const Icon(Icons.group_add_outlined, color: _primaryColor),
                onPressed: () => _showCreateTeamDialog(context, isEn),
              ),
              IconButton(
                tooltip: isEn ? 'Refresh' : 'Làm mới',
                icon: const Icon(Icons.refresh, color: _primaryColor),
                onPressed: () => ref.read(teamsControllerProvider.notifier).refresh(),
              ),
              const SizedBox(width: 8),
            ],
          ),

          // Search & All-members Card
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Search Bar
                  Container(
                    height: 46,
                    decoration: BoxDecoration(
                      color: _surfaceContainerLow,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: _borderColor),
                    ),
                    child: TextField(
                      controller: _searchController,
                      onChanged: (val) => setState(() => _searchQuery = val),
                      style: const TextStyle(fontFamily: 'Plus Jakarta Sans', fontSize: 14),
                      decoration: InputDecoration(
                        hintText: isEn ? 'Search teams...' : 'Tìm kiếm nhóm...',
                        hintStyle: const TextStyle(color: _textMuted, fontSize: 14),
                        prefixIcon: const Icon(Icons.search, color: _textMuted, size: 20),
                        border: InputBorder.none,
                        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Option 1: All Workspace Members Card
                  _buildAllMembersCard(context, isEn),
                  const SizedBox(height: 20),

                  // Section Title
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        isEn ? 'WORKSPACE TEAMS' : 'DANH SÁCH NHÓM TRONG WORKSPACE',
                        style: const TextStyle(
                          fontFamily: 'Plus Jakarta Sans',
                          fontWeight: FontWeight.w700,
                          fontSize: 12,
                          color: _textMuted,
                          letterSpacing: 1.0,
                        ),
                      ),
                      TextButton.icon(
                        style: TextButton.styleFrom(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                          minimumSize: Size.zero,
                          tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                        ),
                        icon: const Icon(Icons.add, size: 16, color: _primaryColor),
                        label: Text(
                          isEn ? 'New Team' : 'Tạo nhóm',
                          style: const TextStyle(
                            fontFamily: 'Plus Jakarta Sans',
                            fontSize: 12,
                            fontWeight: FontWeight.w700,
                            color: _primaryColor,
                          ),
                        ),
                        onPressed: () => _showCreateTeamDialog(context, isEn),
                      ),
                    ],
                  ),
                  const SizedBox(height: 10),
                ],
              ),
            ),
          ),

          // Team List
          teamsState.when(
            data: (teams) {
              if (teams.isEmpty) {
                return SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 40, horizontal: 24),
                    child: Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: [
                          Container(
                            width: 80,
                            height: 80,
                            decoration: BoxDecoration(
                              color: _primaryColor.withOpacity(0.08),
                              shape: BoxShape.circle,
                            ),
                            child: const Icon(
                              Icons.groups_outlined,
                              size: 44,
                              color: _primaryColor,
                            ),
                          ),
                          const SizedBox(height: 18),
                          Text(
                            isEn ? 'No teams created yet' : 'Chưa có nhóm nào được tạo',
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontSize: 18,
                              fontWeight: FontWeight.w700,
                              color: _textMain,
                            ),
                            textAlign: TextAlign.center,
                          ),
                          const SizedBox(height: 8),
                          Text(
                            isEn
                                ? 'Create your first team to organize members, assign credit quotas, and manage brands effectively.'
                                : 'Tạo nhóm đầu tiên để phân quyền thành viên, cấp hạn mức credit và quản lý các thương hiệu hiệu quả.',
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontSize: 13,
                              color: _textMuted,
                              height: 1.4,
                            ),
                            textAlign: TextAlign.center,
                          ),
                          const SizedBox(height: 24),
                          ElevatedButton.icon(
                            style: ElevatedButton.styleFrom(
                              backgroundColor: _primaryColor,
                              foregroundColor: Colors.white,
                              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                              elevation: 2,
                            ),
                            icon: const Icon(Icons.group_add, size: 20),
                            label: Text(
                              isEn ? 'Create First Team' : 'Tạo nhóm đầu tiên',
                              style: const TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                fontWeight: FontWeight.w700,
                                fontSize: 15,
                              ),
                            ),
                            onPressed: () => _showCreateTeamDialog(context, isEn),
                          ),
                        ],
                      ),
                    ),
                  ),
                );
              }

              final filtered = teams.where((t) =>
                  t.name.toLowerCase().contains(_searchQuery.toLowerCase()) ||
                  (t.description?.toLowerCase().contains(_searchQuery.toLowerCase()) ?? false)).toList();

              if (filtered.isEmpty) {
                return SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 40, horizontal: 16),
                    child: Center(
                      child: Column(
                        children: [
                          Icon(Icons.search_off_outlined, size: 48, color: Colors.grey.shade400),
                          const SizedBox(height: 12),
                          Text(
                            isEn ? 'No teams found matching search.' : 'Không tìm thấy nhóm phù hợp với từ khóa.',
                            style: const TextStyle(color: _textMuted, fontSize: 14),
                          ),
                        ],
                      ),
                    ),
                  ),
                );
              }

              return SliverPadding(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                sliver: SliverList(
                  delegate: SliverChildBuilderDelegate(
                    (context, index) {
                      final team = filtered[index];
                      return _buildTeamCard(context, team, index, isEn);
                    },
                    childCount: filtered.length,
                  ),
                ),
              );
            },
            loading: () => const SliverToBoxAdapter(
              child: SizedBox(
                height: 200,
                child: Center(child: AppLoadingIndicator()),
              ),
            ),
            error: (err, stack) {
              final errStr = err.toString().toLowerCase();
              if (errStr.contains('không tìm thấy') || errStr.contains('404')) {
                return SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 40, horizontal: 24),
                    child: Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: [
                          Container(
                            width: 80,
                            height: 80,
                            decoration: BoxDecoration(
                              color: _primaryColor.withOpacity(0.08),
                              shape: BoxShape.circle,
                            ),
                            child: const Icon(
                              Icons.groups_outlined,
                              size: 44,
                              color: _primaryColor,
                            ),
                          ),
                          const SizedBox(height: 18),
                          Text(
                            isEn ? 'No teams created yet' : 'Chưa có nhóm nào được tạo',
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontSize: 18,
                              fontWeight: FontWeight.w700,
                              color: _textMain,
                            ),
                            textAlign: TextAlign.center,
                          ),
                          const SizedBox(height: 8),
                          Text(
                            isEn
                                ? 'Create your first team to organize members, assign credit quotas, and manage brands effectively.'
                                : 'Tạo nhóm đầu tiên để phân quyền thành viên, cấp hạn mức credit và quản lý các thương hiệu hiệu quả.',
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontSize: 13,
                              color: _textMuted,
                              height: 1.4,
                            ),
                            textAlign: TextAlign.center,
                          ),
                          const SizedBox(height: 24),
                          ElevatedButton.icon(
                            style: ElevatedButton.styleFrom(
                              backgroundColor: _primaryColor,
                              foregroundColor: Colors.white,
                              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                              elevation: 2,
                            ),
                            icon: const Icon(Icons.group_add, size: 20),
                            label: Text(
                              isEn ? 'Create First Team' : 'Tạo nhóm đầu tiên',
                              style: const TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                fontWeight: FontWeight.w700,
                                fontSize: 15,
                              ),
                            ),
                            onPressed: () => _showCreateTeamDialog(context, isEn),
                          ),
                        ],
                      ),
                    ),
                  ),
                );
              }

              return SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.all(24.0),
                  child: Center(
                    child: Column(
                      children: [
                        Text(
                          isEn ? 'Error loading teams: $err' : 'Lỗi tải danh sách nhóm: $err',
                          textAlign: TextAlign.center,
                          style: const TextStyle(color: Colors.red),
                        ),
                        const SizedBox(height: 12),
                        ElevatedButton(
                          onPressed: () => ref.read(teamsControllerProvider.notifier).refresh(),
                          child: Text(isEn ? 'Retry' : 'Thử lại'),
                        ),
                      ],
                    ),
                  ),
                ),
              );
            },
          ),

          const SliverToBoxAdapter(child: SizedBox(height: 40)),
        ],
      ),
    );
  }

  Widget _buildAllMembersCard(BuildContext context, bool isEn) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(14),
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: _primaryColor.withOpacity(0.3)),
          boxShadow: [
            BoxShadow(
              color: _primaryColor.withOpacity(0.06),
              blurRadius: 12,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            onTap: () async {
              await context.push(
                '/settings/team/members?all=true',
                extra: {'isAll': true, 'teamName': isEn ? 'All Members' : 'Tất cả thành viên'},
              );
              ref.read(teamsControllerProvider.notifier).refresh();
            },
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Row(
                children: [
                  Container(
                    width: 48,
                    height: 48,
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [_primaryColor, _secondaryColor],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Icon(Icons.people_alt, color: Colors.white, size: 26),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          isEn ? 'All Workspace Members' : 'Tất cả thành viên Workspace',
                          style: const TextStyle(
                            fontFamily: 'Plus Jakarta Sans',
                            fontWeight: FontWeight.w700,
                            fontSize: 16,
                            color: _textMain,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          isEn
                              ? 'View all members, roles and shared credit quotas'
                              : 'Xem toàn bộ thành viên, vai trò và hạn mức credit',
                          style: const TextStyle(
                            fontFamily: 'Plus Jakarta Sans',
                            fontSize: 13,
                            color: _textMuted,
                          ),
                        ),
                      ],
                    ),
                  ),
                  const Icon(Icons.chevron_right, color: _primaryColor),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildTeamCard(BuildContext context, TeamSummaryModel team, int index, bool isEn) {
    final gradients = [
      const LinearGradient(colors: [Color(0xFF003EC7), Color(0xFF6B38D4)]),
      const LinearGradient(colors: [Color(0xFF005851), Color(0xFF003EC7)]),
      const LinearGradient(colors: [Color(0xFFEA580C), Color(0xFFDB2777)]),
      const LinearGradient(colors: [Color(0xFF6B38D4), Color(0xFFDB2777)]),
    ];
    final avatarGradient = gradients[index % gradients.length];

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: _borderColor),
        boxShadow: const [
          BoxShadow(
            color: Color.fromRGBO(0, 0, 0, 0.02),
            blurRadius: 8,
            offset: Offset(0, 2),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: BorderRadius.circular(14),
          onTap: () async {
            await context.push(
              '/settings/team/members?teamId=${team.id}',
              extra: {'teamId': team.id, 'teamName': team.name},
            );
            ref.read(teamsControllerProvider.notifier).refresh();
          },
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Row(
              children: [
                // Team Initial Avatar
                Container(
                  width: 48,
                  height: 48,
                  decoration: BoxDecoration(
                    gradient: avatarGradient,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  alignment: Alignment.center,
                  child: Text(
                    team.name.isNotEmpty ? team.name.substring(0, 1).toUpperCase() : 'T',
                    style: const TextStyle(
                      fontFamily: 'Plus Jakarta Sans',
                      fontWeight: FontWeight.w700,
                      fontSize: 20,
                      color: Colors.white,
                    ),
                  ),
                ),
                const SizedBox(width: 14),

                // Team Info
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Flexible(
                            child: Text(
                              team.name,
                              style: const TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                fontWeight: FontWeight.w700,
                                fontSize: 16,
                                color: _textMain,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                          const SizedBox(width: 8),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            decoration: BoxDecoration(
                              color: team.status.toLowerCase() == 'active'
                                  ? Colors.green.withOpacity(0.12)
                                  : Colors.orange.withOpacity(0.12),
                              borderRadius: BorderRadius.circular(9999),
                            ),
                            child: Text(
                              team.status.toUpperCase(),
                              style: TextStyle(
                                fontSize: 10,
                                fontWeight: FontWeight.bold,
                                color: team.status.toLowerCase() == 'active'
                                    ? Colors.green.shade700
                                    : Colors.orange.shade700,
                              ),
                            ),
                          ),
                        ],
                      ),
                      if (team.description != null && team.description!.isNotEmpty) ...[
                        const SizedBox(height: 2),
                        Text(
                          team.description!,
                          style: const TextStyle(
                            fontFamily: 'Plus Jakarta Sans',
                            fontSize: 13,
                            color: _textMuted,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ],
                      const SizedBox(height: 8),
                      // Badges for members & brands
                      Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                            decoration: BoxDecoration(
                              color: _surfaceContainer,
                              borderRadius: BorderRadius.circular(6),
                            ),
                            child: Row(
                              children: [
                                const Icon(Icons.person_outline, size: 14, color: _onSurfaceVariant),
                                const SizedBox(width: 4),
                                Text(
                                  '${team.memberCount} ${isEn ? 'members' : 'thành viên'}',
                                  style: const TextStyle(
                                    fontSize: 12,
                                    fontWeight: FontWeight.w600,
                                    color: _onSurfaceVariant,
                                  ),
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(width: 8),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                            decoration: BoxDecoration(
                              color: _surfaceContainer,
                              borderRadius: BorderRadius.circular(6),
                            ),
                            child: Row(
                              children: [
                                const Icon(Icons.branding_watermark_outlined, size: 14, color: _onSurfaceVariant),
                                const SizedBox(width: 4),
                                Text(
                                  '${team.brandCount} ${isEn ? 'brands' : 'nhãn hàng'}',
                                  style: const TextStyle(
                                    fontSize: 12,
                                    fontWeight: FontWeight.w600,
                                    color: _onSurfaceVariant,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 8),

                const Icon(Icons.chevron_right, color: _textMuted),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _showCreateTeamDialog(BuildContext context, bool isEn) async {
    final nameController = TextEditingController();
    final descController = TextEditingController();
    final formKey = GlobalKey<FormState>();
    bool isSubmitting = false;

    await showDialog(
      context: context,
      barrierDismissible: false,
      builder: (dialogContext) => StatefulBuilder(
        builder: (ctx, setDialogState) => AlertDialog(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          title: Row(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: _primaryColor.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: const Icon(Icons.group_add, color: _primaryColor, size: 22),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  isEn ? 'Create New Team' : 'Tạo nhóm mới',
                  style: const TextStyle(
                    fontFamily: 'Plus Jakarta Sans',
                    fontWeight: FontWeight.w700,
                    fontSize: 18,
                  ),
                ),
              ),
            ],
          ),
          content: Form(
            key: formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  isEn ? 'Team Name *' : 'Tên nhóm *',
                  style: const TextStyle(
                    fontWeight: FontWeight.w600,
                    fontSize: 13,
                    color: _textMain,
                  ),
                ),
                const SizedBox(height: 6),
                TextFormField(
                  controller: nameController,
                  autofocus: true,
                  decoration: InputDecoration(
                    hintText: isEn ? 'e.g. Marketing Team' : 'Ví dụ: Đội ngũ Marketing',
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                  validator: (val) {
                    if (val == null || val.trim().isEmpty) {
                      return isEn ? 'Please enter team name' : 'Vui lòng nhập tên nhóm';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                Text(
                  isEn ? 'Description (Optional)' : 'Mô tả (Không bắt buộc)',
                  style: const TextStyle(
                    fontWeight: FontWeight.w600,
                    fontSize: 13,
                    color: _textMain,
                  ),
                ),
                const SizedBox(height: 6),
                TextFormField(
                  controller: descController,
                  maxLines: 2,
                  decoration: InputDecoration(
                    hintText: isEn ? 'Brief description about the team' : 'Mô tả ngắn về mục đích của nhóm',
                    contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: isSubmitting ? null : () => Navigator.of(dialogContext).pop(),
              child: Text(isEn ? 'Cancel' : 'Hủy'),
            ),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: _primaryColor,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: isSubmitting
                  ? null
                  : () async {
                      if (!formKey.currentState!.validate()) return;
                      setDialogState(() => isSubmitting = true);
                      try {
                        await ref.read(teamsControllerProvider.notifier).createTeam(
                              nameController.text.trim(),
                              descController.text.trim(),
                            );
                        if (dialogContext.mounted) {
                          Navigator.of(dialogContext).pop();
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(
                                isEn ? 'Team created successfully!' : 'Đã tạo nhóm thành công!',
                              ),
                              backgroundColor: Colors.green.shade700,
                            ),
                          );
                        }
                      } catch (e) {
                        setDialogState(() => isSubmitting = false);
                        if (dialogContext.mounted) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(e.toString()),
                              backgroundColor: Colors.red.shade700,
                            ),
                          );
                        }
                      }
                    },
              child: isSubmitting
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : Text(isEn ? 'Create' : 'Tạo nhóm'),
            ),
          ],
        ),
      ),
    );
  }
}
