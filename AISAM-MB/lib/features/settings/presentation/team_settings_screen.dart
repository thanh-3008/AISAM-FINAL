import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../workspace/presentation/providers/workspace_member_controller.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../workspace/data/models/workspace_model.dart';
import 'providers/language_provider.dart';
import 'providers/team_controller.dart';
import '../data/models/team_model.dart';
import '../data/repositories/team_repository.dart';

// --- Colors ---
const Color _bgColor = Color(0xFFF7F9FB);
const Color _primaryColor = Color(0xFF003EC7);
const Color _secondaryColor = Color(0xFF6B38D4);
const Color _surfaceContainerLow = Color(0xFFF2F4F6);
const Color _surfaceContainer = Color(0xFFECEEF0);
const Color _textMain = Color(0xFF0F172A);
const Color _textMuted = Color(0xFF64748B);
const Color _onSurfaceVariant = Color(0xFF434656);
const Color _borderColor = Color.fromRGBO(226, 232, 240, 0.8);
const Color _secondaryFixedDim = Color(0xFFD0BCFF);
const Color _tertiaryFixedDim = Color(0xFF6BD8CB);
const Color _orange400 = Color(0xFFFB923C);
const Color _red400 = Color(0xFFF87171);

class TeamSettingsScreen extends ConsumerStatefulWidget {
  final String? teamId;
  final String? teamName;
  final bool isAll;

  const TeamSettingsScreen({
    super.key,
    this.teamId,
    this.teamName,
    this.isAll = false,
  });

  @override
  ConsumerState<TeamSettingsScreen> createState() => _TeamSettingsScreenState();
}

class _TeamSettingsScreenState extends ConsumerState<TeamSettingsScreen> {
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  void _showAddMemberToTeamDialog(
    BuildContext context,
    String teamId,
    List<TeamMemberItemModel> currentTeamMembers,
    List<WorkspaceMemberResponseModel> wsMembers,
    bool isEn,
  ) {
    final currentMemberIds = currentTeamMembers.map((m) => m.userId).toSet();
    final availableMembers = wsMembers.where((wm) {
      return !currentMemberIds.contains(wm.userId) && !currentMemberIds.contains(wm.id);
    }).toList();

    if (availableMembers.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            isEn
                ? 'All workspace members are already in this team.'
                : 'Tất cả thành viên trong Workspace đã thuộc nhóm này.',
          ),
          backgroundColor: Colors.orange.shade700,
        ),
      );
      return;
    }

    String? selectedUserId = availableMembers.first.userId.isNotEmpty
        ? availableMembers.first.userId
        : availableMembers.first.id;
    String selectedRole = 'ContentCreator';
    bool isSubmitting = false;

    showDialog(
      context: context,
      builder: (dialogContext) => StatefulBuilder(
        builder: (context, setDialogState) {
          return AlertDialog(
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
            title: Row(
              children: [
                const Icon(Icons.person_add_alt_1, color: _primaryColor),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    isEn ? 'Add Member to Team' : 'Thêm thành viên vào nhóm',
                    style: const TextStyle(fontFamily: 'Plus Jakarta Sans', fontWeight: FontWeight.bold, fontSize: 18),
                  ),
                ),
              ],
            ),
            content: SingleChildScrollView(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    isEn ? 'Select Workspace Member' : 'Chọn thành viên từ Workspace',
                    style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13, color: _textMuted),
                  ),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<String>(
                    value: selectedUserId,
                    isExpanded: true,
                    decoration: InputDecoration(
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                    ),
                    items: availableMembers.map((wm) {
                      final uid = wm.userId.isNotEmpty ? wm.userId : wm.id;
                      final name = (wm.fullName != null && wm.fullName!.isNotEmpty) ? wm.fullName! : wm.email;
                      return DropdownMenuItem<String>(
                        value: uid,
                        child: Text(
                          '$name (${wm.email})',
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(fontSize: 13),
                        ),
                      );
                    }).toList(),
                    onChanged: (val) {
                      if (val != null) {
                        setDialogState(() => selectedUserId = val);
                      }
                    },
                  ),
                  const SizedBox(height: 16),
                  Text(
                    isEn ? 'Assign Role in Team' : 'Vai trò trong nhóm',
                    style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13, color: _textMuted),
                  ),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<String>(
                    value: selectedRole,
                    decoration: InputDecoration(
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                    ),
                    items: [
                      DropdownMenuItem(
                        value: 'ContentCreator',
                        child: Text(isEn ? 'Content Creator' : 'Sáng tạo nội dung (Content Creator)'),
                      ),
                      DropdownMenuItem(
                        value: 'Manager',
                        child: Text(isEn ? 'Manager' : 'Quản lý (Manager)'),
                      ),
                      DropdownMenuItem(
                        value: 'Viewer',
                        child: Text(isEn ? 'Viewer' : 'Người xem (Viewer)'),
                      ),
                    ],
                    onChanged: (val) {
                      if (val != null) {
                        setDialogState(() => selectedRole = val);
                      }
                    },
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
                onPressed: isSubmitting || selectedUserId == null
                    ? null
                    : () async {
                        setDialogState(() => isSubmitting = true);
                        try {
                          await ref.read(teamsControllerProvider.notifier).addMemberToTeam(
                                teamId,
                                selectedUserId!,
                                role: selectedRole,
                              );
                          if (dialogContext.mounted) {
                            Navigator.of(dialogContext).pop();
                          }
                          if (context.mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                content: Text(
                                  isEn ? 'Member added to team successfully' : 'Đã thêm thành viên vào nhóm thành công',
                                ),
                                backgroundColor: const Color(0xFF16A34A),
                              ),
                            );
                          }
                        } catch (e) {
                          setDialogState(() => isSubmitting = false);
                          if (context.mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                content: Text(e.toString()),
                                backgroundColor: Colors.red,
                              ),
                            );
                          }
                        }
                      },
                child: isSubmitting
                    ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                    : Text(isEn ? 'Add Member' : 'Thêm vào nhóm'),
              ),
            ],
          );
        },
      ),
    );
  }

  void _showRemoveMemberConfirmation(
    BuildContext context,
    String teamId,
    String userId,
    String memberName,
    bool isEn,
  ) {
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Row(
          children: [
            const Icon(Icons.warning_amber_rounded, color: Colors.red),
            const SizedBox(width: 8),
            Text(
              isEn ? 'Remove Member' : 'Xóa thành viên',
              style: const TextStyle(fontFamily: 'Plus Jakarta Sans', fontWeight: FontWeight.bold),
            ),
          ],
        ),
        content: Text(
          isEn
              ? 'Are you sure you want to remove $memberName from this team?'
              : 'Bạn có chắc chắn muốn xóa $memberName khỏi nhóm này không?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: Text(isEn ? 'Cancel' : 'Hủy'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.red,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
            ),
            onPressed: () async {
              Navigator.of(dialogContext).pop();
              try {
                await ref.read(teamsControllerProvider.notifier).removeMemberFromTeam(teamId, userId);
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text(
                        isEn
                            ? 'Member removed from team successfully'
                            : 'Đã xóa thành viên khỏi nhóm thành công',
                      ),
                      backgroundColor: const Color(0xFF16A34A),
                    ),
                  );
                }
              } catch (e) {
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text(e.toString()),
                      backgroundColor: Colors.red,
                    ),
                  );
                }
              }
            },
            child: Text(isEn ? 'Remove' : 'Xóa'),
          ),
        ],
      ),
    );
  }

  void _showInviteWorkspaceMemberDialog(BuildContext context, bool isEn) {
    final emailController = TextEditingController();
    int selectedRole = 3; // ContentCreator
    int selectedQuotaMode = 1; // SharedPool
    final limitController = TextEditingController();
    bool isSubmitting = false;

    showDialog(
      context: context,
      builder: (dialogContext) => StatefulBuilder(
        builder: (context, setDialogState) {
          return AlertDialog(
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
            title: Row(
              children: [
                const Icon(Icons.person_add, color: _primaryColor),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    isEn ? 'Invite Workspace Member' : 'Mời thành viên Workspace',
                    style: const TextStyle(fontFamily: 'Plus Jakarta Sans', fontWeight: FontWeight.bold, fontSize: 18),
                  ),
                ),
              ],
            ),
            content: SingleChildScrollView(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    isEn
                        ? 'Enter email to invite a new member to this workspace.'
                        : 'Nhập email để gửi lời mời tham gia workspace.',
                    style: const TextStyle(fontSize: 13, color: _textMuted),
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: emailController,
                    decoration: InputDecoration(
                      labelText: 'Email',
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      prefixIcon: const Icon(Icons.email_outlined),
                      hintText: isEn ? 'e.g. member@company.com' : 'vd: member@company.com',
                    ),
                    keyboardType: TextInputType.emailAddress,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    isEn ? 'Role' : 'Vai trò',
                    style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13, color: _textMuted),
                  ),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<int>(
                    value: selectedRole,
                    decoration: InputDecoration(
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                    ),
                    items: [
                      DropdownMenuItem(
                        value: 3,
                        child: Text(isEn ? 'Content Creator' : 'Sáng tạo nội dung (Content Creator)'),
                      ),
                      DropdownMenuItem(
                        value: 2,
                        child: Text(isEn ? 'Manager' : 'Quản lý (Manager)'),
                      ),
                      DropdownMenuItem(
                        value: 4,
                        child: Text(isEn ? 'Viewer' : 'Người xem (Viewer)'),
                      ),
                    ],
                    onChanged: (val) {
                      if (val != null) setDialogState(() => selectedRole = val);
                    },
                  ),
                  const SizedBox(height: 16),
                  Text(
                    isEn ? 'Credit Quota' : 'Chế độ Credit',
                    style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13, color: _textMuted),
                  ),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<int>(
                    value: selectedQuotaMode,
                    decoration: InputDecoration(
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                    ),
                    items: [
                      DropdownMenuItem(
                        value: 1,
                        child: Text(isEn ? 'Shared Pool (Use workspace balance)' : 'Dùng chung quỹ ví Workspace'),
                      ),
                      DropdownMenuItem(
                        value: 3,
                        child: Text(isEn ? 'Monthly Limit' : 'Hạn mức theo tháng'),
                      ),
                      DropdownMenuItem(
                        value: 2,
                        child: Text(isEn ? 'Lifetime Limit' : 'Hạn mức trọn đời'),
                      ),
                    ],
                    onChanged: (val) {
                      if (val != null) setDialogState(() => selectedQuotaMode = val);
                    },
                  ),
                  if (selectedQuotaMode != 1) ...[
                    const SizedBox(height: 12),
                    TextField(
                      controller: limitController,
                      keyboardType: TextInputType.number,
                      decoration: InputDecoration(
                        labelText: isEn ? 'Credit Limit' : 'Hạn mức credit',
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                        prefixIcon: const Icon(Icons.token_outlined),
                      ),
                    ),
                  ],
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
                        final email = emailController.text.trim();
                        if (email.isEmpty || !email.contains('@')) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(isEn ? 'Please enter a valid email' : 'Vui lòng nhập email hợp lệ'),
                              backgroundColor: Colors.red,
                            ),
                          );
                          return;
                        }
                        setDialogState(() => isSubmitting = true);
                        try {
                          final limit = selectedQuotaMode != 1 ? int.tryParse(limitController.text.trim()) : null;
                          await ref.read(teamRepositoryProvider).inviteWorkspaceMember(
                                email: email,
                                role: selectedRole,
                                quotaMode: selectedQuotaMode,
                                creditLimit: limit,
                              );
                          if (dialogContext.mounted) {
                            Navigator.of(dialogContext).pop();
                          }
                          ref.read(workspaceMemberControllerProvider.notifier).refreshMembers();
                          if (context.mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                content: Text(
                                  isEn
                                      ? 'Invitation sent successfully to $email'
                                      : 'Đã gửi lời mời thành công đến $email',
                                ),
                                backgroundColor: const Color(0xFF16A34A),
                              ),
                            );
                          }
                        } catch (e) {
                          setDialogState(() => isSubmitting = false);
                          if (context.mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                content: Text(e.toString()),
                                backgroundColor: Colors.red,
                              ),
                            );
                          }
                        }
                      },
                child: isSubmitting
                    ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                    : Text(isEn ? 'Send Invite' : 'Gửi lời mời'),
              ),
            ],
          );
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final langState = ref.watch(languageControllerProvider);
    final isEn = (langState.value ?? 'vi') == 'en';
    final walletState = ref.watch(workspaceWalletControllerProvider);
    final workspaceMembersState = ref.watch(workspaceMemberControllerProvider);
    final walletBalance = walletState.valueOrNull?.balance ?? 0;

    final isSpecificTeam = widget.teamId != null && !widget.isAll;
    final teamDetailAsync = isSpecificTeam
        ? ref.watch(teamDetailControllerProvider(widget.teamId!))
        : null;

    final displayName = widget.teamName ??
        (widget.isAll
            ? (isEn ? 'All Members' : 'Tất cả thành viên')
            : (isEn ? 'Team Members' : 'Thành viên Team'));

    return Scaffold(
      backgroundColor: _bgColor,
      floatingActionButton: Container(
        margin: const EdgeInsets.only(bottom: 16),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(9999),
          boxShadow: [
            BoxShadow(
              color: _primaryColor.withValues(alpha: 0.3),
              blurRadius: 24,
              offset: const Offset(0, 12),
            ),
          ],
        ),
        child: ElevatedButton.icon(
          onPressed: () {
            if (isSpecificTeam) {
              final currentTeam = teamDetailAsync?.valueOrNull;
              final wsMembers = workspaceMembersState.valueOrNull ?? [];
              if (currentTeam != null) {
                _showAddMemberToTeamDialog(
                  context,
                  widget.teamId!,
                  currentTeam.members,
                  wsMembers,
                  isEn,
                );
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(isEn ? 'Team details still loading...' : 'Dữ liệu nhóm đang tải...'),
                  ),
                );
              }
            } else {
              _showInviteWorkspaceMemberDialog(context, isEn);
            }
          },
          style: ElevatedButton.styleFrom(
            backgroundColor: _primaryColor,
            foregroundColor: Colors.white,
            elevation: 0,
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(9999)),
          ),
          icon: const Icon(Icons.add, size: 22),
          label: Text(
            isSpecificTeam
                ? (isEn ? 'Add Member' : 'Thêm thành viên')
                : (isEn ? 'Invite Member' : 'Mời thành viên'),
            style: const TextStyle(
              fontFamily: 'Plus Jakarta Sans',
              fontWeight: FontWeight.w600,
              fontSize: 14,
            ),
          ),
        ),
      ),
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
                  context.go('/settings/team');
                }
              },
            ),
            title: Text(
              displayName,
              style: const TextStyle(
                fontFamily: 'Plus Jakarta Sans',
                fontWeight: FontWeight.w700,
                fontSize: 20,
                color: _textMain,
              ),
            ),
            centerTitle: true,
            actions: [
              if (isSpecificTeam)
                IconButton(
                  tooltip: isEn ? 'Add Member' : 'Thêm thành viên',
                  icon: const Icon(Icons.person_add_alt_1, color: _primaryColor),
                  onPressed: () {
                    final currentTeam = teamDetailAsync?.valueOrNull;
                    final wsMembers = workspaceMembersState.valueOrNull ?? [];
                    if (currentTeam != null) {
                      _showAddMemberToTeamDialog(
                        context,
                        widget.teamId!,
                        currentTeam.members,
                        wsMembers,
                        isEn,
                      );
                    }
                  },
                ),
              IconButton(
                tooltip: isEn ? 'Refresh' : 'Làm mới',
                icon: const Icon(Icons.refresh, color: _primaryColor),
                onPressed: () {
                  ref.read(workspaceMemberControllerProvider.notifier).refreshMembers();
                  ref.read(workspaceWalletControllerProvider.notifier).refresh();
                  if (widget.teamId != null) {
                    ref.invalidate(teamDetailControllerProvider(widget.teamId!));
                  }
                },
              ),
              const SizedBox(width: 8),
            ],
          ),

          // Shared Pool Banner & Search
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Workspace Shared Credit Card
                  _buildWorkspaceCreditBanner(context, walletBalance, isEn),
                  const SizedBox(height: 16),

                  // Search input
                  Container(
                    height: 44,
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
                        hintText: isEn ? 'Search members...' : 'Tìm kiếm thành viên...',
                        hintStyle: const TextStyle(color: _textMuted, fontSize: 14),
                        prefixIcon: const Icon(Icons.search, color: _textMuted, size: 20),
                        border: InputBorder.none,
                        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Section Header
                  Text(
                    isEn ? 'TEAM MEMBERS & SHARED CREDITS' : 'THÀNH VIÊN VÀ CREDIT DÙNG CHUNG',
                    style: const TextStyle(
                      fontFamily: 'Plus Jakarta Sans',
                      fontWeight: FontWeight.w700,
                      fontSize: 12,
                      color: _textMuted,
                      letterSpacing: 1.0,
                    ),
                  ),
                  const SizedBox(height: 8),
                ],
              ),
            ),
          ),

          // Members List
          if (isSpecificTeam && teamDetailAsync != null)
            teamDetailAsync.when(
              data: (teamDetail) {
                return workspaceMembersState.when(
                  data: (wsMembers) {
                    // Match team members with workspace member credit info using dual-key lookup
                    final memberLookup = <String, WorkspaceMemberResponseModel>{};
                    for (var m in wsMembers) {
                      memberLookup[m.userId] = m;
                      memberLookup[m.id] = m;
                    }

                    // Strict sync: Only keep team members that are active in the workspace
                    final activeTeamMembers = teamDetail.members.where((tm) {
                      return memberLookup.containsKey(tm.userId);
                    }).toList();

                    final members = activeTeamMembers.where((m) =>
                        m.name.toLowerCase().contains(_searchQuery.toLowerCase()) ||
                        m.email.toLowerCase().contains(_searchQuery.toLowerCase())).toList();

                    if (members.isEmpty) {
                      return SliverToBoxAdapter(
                        child: Padding(
                          padding: const EdgeInsets.symmetric(vertical: 40, horizontal: 24),
                          child: Center(
                            child: Column(
                              children: [
                                Icon(Icons.people_outline, size: 48, color: Colors.grey.shade400),
                                const SizedBox(height: 12),
                                Text(
                                  isEn ? 'No members found in this team.' : 'Không có thành viên nào trong nhóm này.',
                                  style: const TextStyle(color: _textMuted),
                                ),
                                const SizedBox(height: 16),
                                ElevatedButton.icon(
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: _primaryColor,
                                    foregroundColor: Colors.white,
                                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                                  ),
                                  icon: const Icon(Icons.person_add, size: 18),
                                  label: Text(isEn ? 'Add Member to Team' : 'Thêm thành viên vào nhóm'),
                                  onPressed: () => _showAddMemberToTeamDialog(
                                    context,
                                    widget.teamId!,
                                    teamDetail.members,
                                    wsMembers,
                                    isEn,
                                  ),
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
                            final tm = members[index];
                            final wsMember = memberLookup[tm.userId];
                            return _buildMemberCard(
                              name: tm.name,
                              email: tm.email,
                              roleString: tm.role,
                              quotaMode: wsMember?.quotaMode ?? 1,
                              creditLimit: wsMember?.creditLimit,
                              creditUsed: wsMember?.creditUsed ?? 0,
                              walletBalance: walletBalance,
                              isEn: isEn,
                              onRemove: () => _showRemoveMemberConfirmation(
                                context,
                                widget.teamId!,
                                tm.userId,
                                tm.name,
                                isEn,
                              ),
                            );
                          },
                          childCount: members.length,
                        ),
                      ),
                    );
                  },
                  loading: () => const SliverToBoxAdapter(
                    child: SizedBox(height: 200, child: Center(child: AppLoadingIndicator())),
                  ),
                  error: (err, _) => SliverToBoxAdapter(
                    child: Center(child: Text('Error: $err')),
                  ),
                );
              },
              loading: () => const SliverToBoxAdapter(
                child: SizedBox(height: 200, child: Center(child: AppLoadingIndicator())),
              ),
              error: (err, _) => SliverToBoxAdapter(
                child: Center(child: Text('Error: $err')),
              ),
            )
          else
            workspaceMembersState.when(
              data: (membersList) {
                final filtered = membersList.where((m) =>
                    (m.fullName ?? m.email).toLowerCase().contains(_searchQuery.toLowerCase()) ||
                    m.email.toLowerCase().contains(_searchQuery.toLowerCase())).toList();

                if (filtered.isEmpty) {
                  return SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(vertical: 40),
                      child: Center(
                        child: Text(
                          isEn ? 'No members found.' : 'Không tìm thấy thành viên nào.',
                          style: const TextStyle(color: _textMuted),
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
                        final member = filtered[index];
                        final roleString = member.role == 1
                            ? 'Owner'
                            : (member.role == 2
                                ? 'Manager'
                                : (member.role == 3 ? 'Content Creator' : 'Viewer'));

                        return _buildMemberCard(
                          name: member.fullName ?? member.email,
                          email: member.email,
                          roleString: roleString,
                          quotaMode: member.quotaMode ?? 1,
                          creditLimit: member.creditLimit,
                          creditUsed: member.creditUsed ?? 0,
                          walletBalance: walletBalance,
                          isEn: isEn,
                        );
                      },
                      childCount: filtered.length,
                    ),
                  ),
                );
              },
              loading: () => const SliverToBoxAdapter(
                child: SizedBox(height: 200, child: Center(child: AppLoadingIndicator())),
              ),
              error: (err, _) => SliverToBoxAdapter(
                child: Center(child: Text('Error: $err')),
              ),
            ),

          const SliverToBoxAdapter(child: SizedBox(height: 100)),
        ],
      ),
    );
  }

  Widget _buildWorkspaceCreditBanner(BuildContext context, int walletBalance, bool isEn) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [_primaryColor.withOpacity(0.08), _secondaryColor.withOpacity(0.08)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: _primaryColor.withOpacity(0.2)),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: _primaryColor.withOpacity(0.15),
              borderRadius: BorderRadius.circular(10),
            ),
            child: const Icon(Icons.account_balance_wallet, color: _primaryColor, size: 26),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  isEn ? 'Workspace Shared Credit Pool' : 'Quỹ Credit dùng chung Workspace',
                  style: const TextStyle(
                    fontFamily: 'Plus Jakarta Sans',
                    fontWeight: FontWeight.w600,
                    fontSize: 13,
                    color: _textMuted,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  '$walletBalance Credits',
                  style: const TextStyle(
                    fontFamily: 'Plus Jakarta Sans',
                    fontWeight: FontWeight.w800,
                    fontSize: 20,
                    color: _primaryColor,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMemberCard({
    required String name,
    required String email,
    required String roleString,
    required int quotaMode,
    required int? creditLimit,
    required int creditUsed,
    required int walletBalance,
    required bool isEn,
    VoidCallback? onRemove,
  }) {
    // Role styling
    Color roleBgColor;
    Color roleTextColor;
    String roleLabel = roleString;
    if (roleString.toLowerCase() == 'owner') {
      roleBgColor = _secondaryColor.withOpacity(0.12);
      roleTextColor = _secondaryColor;
      roleLabel = isEn ? 'Owner' : 'Chủ sở hữu';
    } else if (roleString.toLowerCase() == 'manager') {
      roleBgColor = _primaryColor.withOpacity(0.12);
      roleTextColor = _primaryColor;
      roleLabel = isEn ? 'Manager' : 'Quản lý';
    } else if (roleString.toLowerCase().contains('creator')) {
      roleBgColor = _secondaryFixedDim.withOpacity(0.3);
      roleTextColor = const Color(0xFF5B21B6);
      roleLabel = isEn ? 'Content Creator' : 'Sáng tạo nội dung';
    } else {
      roleBgColor = _surfaceContainer;
      roleTextColor = _onSurfaceVariant;
      roleLabel = isEn ? 'Viewer' : 'Người xem';
    }

    final initial = name.isNotEmpty ? name.substring(0, 1).toUpperCase() : 'M';
    final isSharedPool = quotaMode == 1 || creditLimit == null || creditLimit == 0;

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
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Row 1: Avatar, Name, Email, Role, Remove Button
            Row(
              children: [
                CircleAvatar(
                  radius: 20,
                  backgroundColor: _primaryColor.withOpacity(0.12),
                  child: Text(
                    initial,
                    style: const TextStyle(
                      fontFamily: 'Plus Jakarta Sans',
                      fontWeight: FontWeight.w700,
                      color: _primaryColor,
                      fontSize: 16,
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        name,
                        style: const TextStyle(
                          fontFamily: 'Plus Jakarta Sans',
                          fontWeight: FontWeight.w700,
                          fontSize: 15,
                          color: _textMain,
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      Text(
                        email,
                        style: const TextStyle(
                          fontFamily: 'Plus Jakarta Sans',
                          fontSize: 12,
                          color: _textMuted,
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(
                    color: roleBgColor,
                    borderRadius: BorderRadius.circular(9999),
                  ),
                  child: Text(
                    roleLabel,
                    style: TextStyle(
                      fontFamily: 'Plus Jakarta Sans',
                      fontSize: 11,
                      fontWeight: FontWeight.w700,
                      color: roleTextColor,
                    ),
                  ),
                ),
                if (onRemove != null) ...[
                  const SizedBox(width: 8),
                  InkWell(
                    borderRadius: BorderRadius.circular(9999),
                    onTap: onRemove,
                    child: Container(
                      padding: const EdgeInsets.all(4),
                      decoration: BoxDecoration(
                        color: Colors.red.withOpacity(0.08),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(Icons.close, size: 16, color: Colors.red),
                    ),
                  ),
                ],
              ],
            ),

            const SizedBox(height: 12),
            const Divider(height: 1, color: Color(0xFFF1F5F9)),
            const SizedBox(height: 12),

            // Row 2: Shared Credit & Quota Details
            if (isSharedPool)
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                decoration: BoxDecoration(
                  color: const Color(0xFFF0FDF4), // light emerald
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: const Color(0xFFBBF7D0)),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.share, color: Color(0xFF16A34A), size: 18),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            isEn ? 'Credit Mode: Shared Pool' : 'Chế độ: Dùng chung ví Workspace',
                            style: const TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                              color: Color(0xFF15803D),
                            ),
                          ),
                          Text(
                            isEn
                                ? 'Remaining: $walletBalance credits'
                                : 'Số credit khả dụng: $walletBalance credits',
                            style: const TextStyle(
                              fontSize: 12,
                              color: Color(0xFF166534),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              )
            else ...[
              // Assigned Quota Mode
              Builder(builder: (context) {
                final limit = creditLimit ?? 0;
                final remaining = (limit - creditUsed).clamp(0, limit);
                final pct = limit > 0 ? (creditUsed / limit).clamp(0.0, 1.0) : 0.0;
                final modeTitle = quotaMode == 2
                    ? (isEn ? 'Lifetime Quota' : 'Hạn mức trọn đời')
                    : (isEn ? 'Monthly Quota' : 'Hạn mức tháng');

                return Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: const Color(0xFFFFF7ED), // light orange
                    borderRadius: BorderRadius.circular(10),
                    border: Border.all(color: const Color(0xFFFED7AA)),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            modeTitle,
                            style: const TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                              color: Color(0xFFC2410C),
                            ),
                          ),
                          Text(
                            isEn
                                ? 'Remaining: $remaining / $limit'
                                : 'Còn lại: $remaining / $limit credits',
                            style: const TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.w800,
                              color: Color(0xFFC2410C),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      LinearProgressIndicator(
                        value: pct,
                        backgroundColor: Colors.orange.shade100,
                        valueColor: AlwaysStoppedAnimation(
                          pct >= 0.9 ? Colors.red : Colors.orange.shade700,
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(
                        isEn ? 'Used: $creditUsed credits' : 'Đã dùng: $creditUsed credits',
                        style: TextStyle(fontSize: 11, color: Colors.orange.shade900),
                      ),
                    ],
                  ),
                );
              }),
            ],
          ],
        ),
      ),
    );
  }
}
