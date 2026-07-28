import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../workspace/presentation/providers/workspace_member_controller.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../workspace/data/models/workspace_model.dart';
// --- Colors from Tailwind HTML ---
const Color _bgColor = Color(0xFFF7F9FB);
const Color _primaryColor = Color(0xFF003EC7);
const Color _secondaryColor = Color(0xFF6B38D4);
const Color _surfaceContainerLow = Color(0xFFF2F4F6);
const Color _surfaceContainer = Color(0xFFECEEF0);
const Color _surfaceContainerHigh = Color(0xFFE6E8EA);
const Color _textMain = Color(0xFF0F172A);
const Color _textMuted = Color(0xFF64748B);
const Color _onSurfaceVariant = Color(0xFF434656);
const Color _borderColor = Color.fromRGBO(255, 255, 255, 0.4);
const Color _outlineVariant = Color(0xFFC3C5D9);
const Color _tertiaryFixedDim = Color(0xFF6BD8CB);
const Color _secondaryFixedDim = Color(0xFFD0BCFF);
const Color _orange400 = Color(0xFFFB923C);
const Color _red400 = Color(0xFFF87171);

class TeamSettingsScreen extends ConsumerStatefulWidget {
  const TeamSettingsScreen({super.key});

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

  void _showInviteDialog(BuildContext context) {
    final emailController = TextEditingController();
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Mời thành viên mới', style: TextStyle(fontFamily: 'Plus Jakarta Sans', fontWeight: FontWeight.bold)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Nhập email của người bạn muốn mời vào Workspace này.', style: TextStyle(fontFamily: 'Plus Jakarta Sans')),
            const SizedBox(height: 16),
            TextField(
              controller: emailController,
              decoration: const InputDecoration(
                labelText: 'Email',
                border: OutlineInputBorder(),
                prefixIcon: Icon(Icons.email_outlined),
              ),
              keyboardType: TextInputType.emailAddress,
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(content: Text('Đã gửi lời mời đến ${emailController.text}')),
              );
            },
            child: const Text('Gửi lời mời'),
          ),
        ],
      ),
    );
  }

  void _showMemberOptionsDialog(BuildContext context, String name, String currentRole) {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: Text(
                'Tùy chọn cho $name',
                style: const TextStyle(fontFamily: 'Plus Jakarta Sans', fontSize: 18, fontWeight: FontWeight.bold),
              ),
            ),
            const Divider(),
            ListTile(
              leading: const Icon(Icons.admin_panel_settings_outlined),
              title: const Text('Đổi Role', style: TextStyle(fontFamily: 'Plus Jakarta Sans')),
              trailing: const Icon(Icons.chevron_right),
              onTap: () {
                Navigator.of(context).pop();
              },
            ),
            if (currentRole != 'Owner')
              ListTile(
                leading: Icon(Icons.person_remove, color: Theme.of(context).colorScheme.error),
                title: Text('Xoá khỏi Workspace', style: TextStyle(color: Theme.of(context).colorScheme.error, fontFamily: 'Plus Jakarta Sans')),
                onTap: () {
                  Navigator.of(context).pop();
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(content: Text('Đã xoá $name khỏi Workspace')),
                  );
                },
              ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final memberState = ref.watch(workspaceMemberControllerProvider);

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
          onPressed: () => _showInviteDialog(context),
          style: ElevatedButton.styleFrom(
            backgroundColor: _primaryColor,
            foregroundColor: Colors.white,
            elevation: 0,
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(9999),
            ),
          ),
          icon: const Icon(Icons.add, size: 24),
          label: const Text(
            'Mời thành viên',
            style: TextStyle(
              fontFamily: 'Plus Jakarta Sans',
              fontWeight: FontWeight.w600,
              fontSize: 14,
              letterSpacing: 0.5,
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
                  ),
                ),
              ),
            ),
            leading: IconButton(
              icon: const Icon(Icons.arrow_back, color: _onSurfaceVariant),
              onPressed: () {
                if (context.canPop()) {
                  context.pop();
                }
              },
            ),
            title: const Text(
              'Thành viên Team',
              style: TextStyle(
                fontFamily: 'Plus Jakarta Sans',
                fontWeight: FontWeight.w700,
                fontSize: 24,
                color: _textMain,
              ),
            ),
            centerTitle: true,
            actions: [
              IconButton(
                icon: const Icon(Icons.person_add, color: _primaryColor),
                onPressed: () => _showInviteDialog(context),
              ),
              const SizedBox(width: 8),
            ],
          ),
          
          // Body content
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Search and Filter
                  Row(
                    children: [
                      Expanded(
                        child: Container(
                          height: 44,
                          decoration: BoxDecoration(
                            color: _surfaceContainerLow,
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: TextField(
                            controller: _searchController,
                            onChanged: (val) => setState(() => _searchQuery = val),
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontSize: 14,
                            ),
                            decoration: const InputDecoration(
                              hintText: 'Tìm kiếm thành viên...',
                              hintStyle: TextStyle(
                                color: _textMuted,
                                fontFamily: 'Plus Jakarta Sans',
                                fontSize: 14,
                              ),
                              prefixIcon: Icon(Icons.search, color: _textMuted, size: 20),
                              border: InputBorder.none,
                              contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: 8),
                      Container(
                        height: 44,
                        width: 44,
                        decoration: BoxDecoration(
                          color: _surfaceContainerLow,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: IconButton(
                          icon: const Icon(Icons.filter_list, color: _onSurfaceVariant, size: 20),
                          onPressed: () {
                            // Filter action
                          },
                        ),
                      ),
                    ],
                  ),
                  // List Header
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 4.0),
                    child: Text(
                      'Danh sách',
                      style: const TextStyle(
                        fontFamily: 'Plus Jakarta Sans',
                        fontWeight: FontWeight.w700,
                        fontSize: 14,
                        color: _textMuted,
                        letterSpacing: 1.0, // tracking-wider
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),
                  
                  // List content
                  memberState.when(
                    data: (membersList) {
                      final filteredMembers = membersList.where((m) => 
                        (m.fullName ?? m.email).toLowerCase().contains(_searchQuery.toLowerCase()) || 
                        m.email.toLowerCase().contains(_searchQuery.toLowerCase())
                      ).toList();

                      if (filteredMembers.isEmpty) {
                        return const Padding(
                          padding: EdgeInsets.symmetric(vertical: 32),
                          child: Center(
                            child: Text(
                              'Không có thành viên nào.',
                              style: TextStyle(
                                fontFamily: 'Plus Jakarta Sans',
                                color: _textMuted,
                              ),
                            ),
                          ),
                        );
                      }

                      return ListView.separated(
                        padding: EdgeInsets.zero,
                        physics: const NeverScrollableScrollPhysics(),
                        shrinkWrap: true,
                        itemCount: filteredMembers.length,
                        separatorBuilder: (context, index) => const SizedBox(height: 8),
                        itemBuilder: (context, index) {
                          final member = filteredMembers[index];
                          return _buildMemberTile(member, context);
                        },
                      );
                    },
                    loading: () => const SizedBox(
                      height: 200,
                      child: Center(child: AppLoadingIndicator()),
                    ),
                    error: (err, stack) => SizedBox(
                      height: 200,
                      child: Center(child: Text('Error: $err')),
                    ),
                  ),
                  const SizedBox(height: 80),
                  const SizedBox(height: 100), // FAB padding
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMemberTile(WorkspaceMemberResponseModel member, BuildContext context) {
    final String name = member.fullName ?? member.email;
    final String email = member.email;
    final String role = member.role == 1 ? 'Owner' : (member.role == 2 ? 'Manager' : 'Viewer');
    final String status = 'active'; // TODO: Update if status exists
    final String? avatarUrl = null;
    final String? initials = (member.fullName != null && member.fullName!.isNotEmpty) 
                                ? member.fullName!.substring(0, 1).toUpperCase() 
                                : member.email.substring(0, 1).toUpperCase();

    final isPending = status == 'pending';

    // Role badge colors
    Color roleBgColor;
    Color roleTextColor;
    if (role == 'Owner') {
      roleBgColor = _secondaryColor.withValues(alpha: 0.1);
      roleTextColor = _secondaryColor;
    } else if (role == 'Manager') {
      roleBgColor = _primaryColor.withValues(alpha: 0.1);
      roleTextColor = _primaryColor;
    } else { // Viewer
      roleBgColor = _surfaceContainer;
      roleTextColor = _onSurfaceVariant;
    }

    // Avatar configuration
    Gradient? avatarGradient;
    if (role == 'Owner') {
      avatarGradient = const LinearGradient(colors: [_primaryColor, _secondaryColor], begin: Alignment.topRight, end: Alignment.bottomLeft);
    } else if (role == 'Manager') {
      // Rotate gradients for managers for variety just like HTML
      avatarGradient = name.contains('Trần') 
          ? const LinearGradient(colors: [_secondaryFixedDim, _tertiaryFixedDim], begin: Alignment.topRight, end: Alignment.bottomLeft)
          : const LinearGradient(colors: [_orange400, _red400], begin: Alignment.topRight, end: Alignment.bottomLeft);
    }

    return ClipRRect(
      borderRadius: BorderRadius.circular(12),
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 8.0, sigmaY: 8.0),
        child: Container(
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: isPending ? 0.5 : 0.7),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(
              color: isPending ? _outlineVariant : _borderColor,
              style: BorderStyle.solid,
            ),
            boxShadow: const [
              BoxShadow(
                color: Color.fromRGBO(0, 0, 0, 0.02),
                blurRadius: 10,
                offset: Offset(0, 2),
              ),
            ],
          ),
          child: Material(
            color: Colors.transparent,
            child: InkWell(
              onTap: () {
                _showMemberOptionsDialog(context, name, role);
              },
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Row(
                  children: [
                    // Avatar
                    if (isPending)
                      Container(
                        width: 48,
                        height: 48,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          border: Border.all(color: _outlineVariant, width: 2), // Simulate dashed with border
                        ),
                        child: const Icon(Icons.hourglass_empty, color: _outlineVariant),
                      )
                    else
                      Container(
                        width: 48,
                        height: 48,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          gradient: avatarGradient,
                          color: avatarGradient == null ? _surfaceContainerHigh : null,
                        ),
                        padding: EdgeInsets.all(avatarGradient != null ? 2 : 0),
                        child: Container(
                          decoration: const BoxDecoration(
                            shape: BoxShape.circle,
                            color: Colors.white,
                          ),
                          child: ClipOval(
                            child: avatarUrl != null
                                ? Image.network(avatarUrl, fit: BoxFit.cover)
                                : Center(
                                    child: Text(
                                      initials ?? '?',
                                      style: const TextStyle(
                                        color: _primaryColor,
                                        fontWeight: FontWeight.bold,
                                        fontSize: 18,
                                      ),
                                    ),
                                  ),
                          ),
                        ),
                      ),
                    const SizedBox(width: 16),
                    
                    // Name & Email
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            name,
                            style: TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontWeight: FontWeight.w600,
                              fontSize: 16,
                              color: _textMain,
                              fontStyle: isPending ? FontStyle.italic : FontStyle.normal,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const SizedBox(height: 4),
                          Text(
                            email,
                            style: const TextStyle(
                              fontFamily: 'Plus Jakarta Sans',
                              fontSize: 14,
                              color: _textMuted,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ],
                      ),
                    ),
                    
                    // Role Badge
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                      decoration: BoxDecoration(
                        color: roleBgColor,
                        borderRadius: BorderRadius.circular(9999),
                      ),
                      child: Text(
                        role.toUpperCase(),
                        style: TextStyle(
                          fontFamily: 'Plus Jakarta Sans',
                          fontSize: 10,
                          fontWeight: FontWeight.w700,
                          color: roleTextColor,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

