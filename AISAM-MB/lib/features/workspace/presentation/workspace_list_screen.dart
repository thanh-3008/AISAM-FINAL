import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/shared/app_loading_indicator.dart';
import '../../../core/shared/empty_state_widget.dart';
import 'providers/workspace_controller.dart';
import '../data/models/workspace_model.dart';
import '../../../core/services/logger_service.dart';

// --- Colors from Tailwind HTML ---
const Color _bgColor = Color(0xFFF7F9FB);
const Color _primaryColor = Color(0xFF003EC7);
const Color _secondaryColor = Color(0xFF6B38D4);
const Color _surfaceContainerLow = Color(0xFFF2F4F6);
const Color _surfaceContainer = Color(0xFFECEEF0);
const Color _textMain = Color(0xFF0F172A);
const Color _textMuted = Color(0xFF64748B);
const Color _onSurfaceVariant = Color(0xFF434656);
const Color _adsOrange = Color(0xFFEA580C);
const Color _publishingPink = Color(0xFFDB2777);
const Color _tertiaryColor = Color(0xFF005851);
const Color _secondaryContainer = Color(0xFF8455EF);
const Color _onSecondaryContainer = Color(0xFFFFFBFF);
const Color _surfaceContainerHighest = Color(0xFFE0E3E5);
const Color _onSurface = Color(0xFF191C1E);
const Color _borderColor = Color.fromRGBO(226, 232, 240, 0.8);

class WorkspaceListScreen extends ConsumerStatefulWidget {
  const WorkspaceListScreen({super.key});

  @override
  ConsumerState<WorkspaceListScreen> createState() => _WorkspaceListScreenState();
}

class _WorkspaceListScreenState extends ConsumerState<WorkspaceListScreen> {
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final workspaceState = ref.watch(workspaceControllerProvider);

    return Scaffold(
      backgroundColor: _bgColor,
      floatingActionButton: FloatingActionButton(
        onPressed: () => context.push('/workspace/create'),
        backgroundColor: _primaryColor,
        elevation: 4,
        shape: const CircleBorder(),
        child: const Icon(Icons.add, color: Colors.white, size: 28),
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
                filter: ImageFilter.blur(sigmaX: 12.0, sigmaY: 12.0),
                child: Container(
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.7),
                    border: const Border(bottom: BorderSide(color: _borderColor)),
                  ),
                ),
              ),
            ),
            leading: IconButton(
              icon: const Icon(Icons.arrow_back, color: _onSurface),
              onPressed: () {
                if (context.canPop()) {
                  context.pop();
                } else {
                  context.go('/dashboard');
                }
              },
            ),
            title: const Text(
              'Select Workspace',
              style: TextStyle(
                fontFamily: 'Inter',
                fontWeight: FontWeight.w600,
                fontSize: 20,
                color: _textMain,
                letterSpacing: -0.01,
              ),
            ),
            centerTitle: true,
            actions: [
              IconButton(
                icon: const Icon(Icons.add, color: _primaryColor),
                onPressed: () => context.push('/workspace/create'),
              ),
            ],
          ),
          
          // Body content
          SliverToBoxAdapter(
            child: workspaceState.when(
              data: (workspaces) {
                // Filter workspaces
                final filteredWorkspaces = workspaces.where((w) => 
                  w.name.toLowerCase().contains(_searchQuery.toLowerCase())
                ).toList();

                return Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 24.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Search Bar
                      Container(
                        height: 48,
                        decoration: BoxDecoration(
                          color: _surfaceContainerLow,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: TextField(
                          controller: _searchController,
                          onChanged: (value) => setState(() => _searchQuery = value),
                          style: const TextStyle(
                            fontFamily: 'Inter',
                            fontSize: 16,
                            color: _textMain,
                          ),
                          decoration: const InputDecoration(
                            hintText: 'Search workspaces...',
                            hintStyle: TextStyle(
                              color: _textMuted,
                              fontFamily: 'Inter',
                              fontSize: 16,
                            ),
                            prefixIcon: Icon(Icons.search, color: _textMuted),
                            border: InputBorder.none,
                            contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                          ),
                        ),
                      ),
                      const SizedBox(height: 24),
                      
                      // Section Header
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          const Text(
                            'RECENT WORKSPACES',
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontSize: 12,
                              fontWeight: FontWeight.w700,
                              color: _textMuted,
                              letterSpacing: 0.03,
                            ),
                          ),
                          GestureDetector(
                            onTap: () {},
                            child: const Text(
                              'View All',
                              style: TextStyle(
                                fontFamily: 'Inter',
                                fontSize: 12,
                                fontWeight: FontWeight.w700,
                                color: _primaryColor,
                                letterSpacing: 0.03,
                              ),
                            ),
                          )
                        ],
                      ),
                      const SizedBox(height: 12),
                      
                      // List or Empty
                      if (workspaces.isEmpty)
                        const Padding(
                          padding: EdgeInsets.only(top: 24.0),
                          child: EmptyStateWidget(
                            title: 'No Workspaces Found',
                            message: 'You don\'t belong to any workspace yet.',
                            icon: Icons.business,
                          ),
                        )
                      else if (filteredWorkspaces.isEmpty)
                         const Padding(
                          padding: EdgeInsets.only(top: 24.0),
                          child: Center(
                            child: Text(
                              'No workspaces match your search.',
                              style: TextStyle(color: _textMuted, fontFamily: 'Inter'),
                            ),
                          ),
                        )
                      else
                        ListView.builder(
                          padding: EdgeInsets.zero,
                          physics: const NeverScrollableScrollPhysics(),
                          shrinkWrap: true,
                          itemCount: filteredWorkspaces.length,
                          itemBuilder: (context, index) {
                            final workspace = filteredWorkspaces[index];
                            return Padding(
                              padding: const EdgeInsets.only(bottom: 12.0),
                              child: _WorkspaceListItem(
                                workspace: workspace,
                                index: index,
                              ),
                            );
                          },
                        ),
                        

                      
                      // Footer
                      const Padding(
                        padding: EdgeInsets.symmetric(vertical: 24.0),
                        child: Center(
                          child: Column(
                            children: [
                              Text(
                                'Logged in as user',
                                style: TextStyle(
                                  fontFamily: 'Inter',
                                  fontSize: 12,
                                  fontWeight: FontWeight.w700,
                                  color: _textMuted,
                                ),
                              ),
                              SizedBox(height: 4),
                              Text(
                                'v2.4.0-stable',
                                style: TextStyle(
                                  fontFamily: 'Inter',
                                  fontSize: 12,
                                  fontWeight: FontWeight.w700,
                                  color: _textMuted,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 60),
                    ],
                  ),
                );
              },
              loading: () => const SizedBox(
                height: 400,
                child: Center(child: AppLoadingIndicator()),
              ),
              error: (error, stack) => SizedBox(
                height: 400,
                child: Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text('Error: $error', textAlign: TextAlign.center),
                      const SizedBox(height: 16),
                      ElevatedButton(
                        onPressed: () => ref.read(workspaceControllerProvider.notifier).refreshWorkspaces(),
                        child: const Text('Retry'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _WorkspaceListItem extends ConsumerWidget {
  final WorkspaceResponseModel workspace;
  final int index;

  const _WorkspaceListItem({required this.workspace, required this.index});

  String _getRoleName(int roleInt) {
    // Basic mapping assumption
    if (roleInt == 0) return 'Owner';
    if (roleInt == 1) return 'Admin';
    if (roleInt == 2) return 'Member';
    if (roleInt == 3) return 'Client';
    return 'Member';
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    String placeholderDesc = workspace.description ?? 'No description';
    if (placeholderDesc.isEmpty) {
      placeholderDesc = 'No description';
    }

    final role = _getRoleName(workspace.currentUserRole);
    Color roleBgColor = _surfaceContainerHighest;
    Color roleTextColor = _onSurface;
    if (role.toLowerCase() == 'admin' || role.toLowerCase() == 'owner') {
      roleBgColor = _secondaryContainer;
      roleTextColor = _onSecondaryContainer;
    }

    final gradients = [
      const LinearGradient(colors: [_primaryColor, _secondaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_primaryColor, _publishingPink], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_tertiaryColor, _primaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
      const LinearGradient(colors: [_adsOrange, _primaryColor], begin: Alignment.topLeft, end: Alignment.bottomRight),
    ];
    final avatarGradient = gradients[index % gradients.length];

    return ClipRRect(
      borderRadius: BorderRadius.circular(12),
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 12.0, sigmaY: 12.0),
        child: Container(
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: 0.7),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: _borderColor),
          ),
          child: Material(
            color: Colors.transparent,
            child: InkWell(
              onTap: () async {
                LoggerService.i('Selected workspace: ${workspace.id}');
                final success = await ref.read(workspaceControllerProvider.notifier).selectWorkspace(workspace.id);
                if (success && context.mounted) {
                  context.go('/dashboard');
                }
              },
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Row(
                  children: [
                    Container(
                      width: 48,
                      height: 48,
                      decoration: BoxDecoration(
                        gradient: avatarGradient,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      alignment: Alignment.center,
                      child: Text(
                        workspace.name.isNotEmpty ? workspace.name.substring(0, 1).toUpperCase() : 'W',
                        style: const TextStyle(
                          fontFamily: 'Inter',
                          fontWeight: FontWeight.w700,
                          fontSize: 20,
                          color: Colors.white,
                        ),
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              Flexible(
                                child: Text(
                                  workspace.name,
                                  style: const TextStyle(
                                    fontFamily: 'Inter',
                                    fontWeight: FontWeight.w600,
                                    fontSize: 16, // headline-sm
                                    color: _textMain,
                                  ),
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ),
                              const SizedBox(width: 8),
                              if (role.isNotEmpty)
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                                  decoration: BoxDecoration(
                                    color: roleBgColor,
                                    borderRadius: BorderRadius.circular(9999),
                                  ),
                                  child: Text(
                                    role.toUpperCase(),
                                    style: TextStyle(
                                      fontFamily: 'Inter',
                                      fontSize: 10,
                                      fontWeight: FontWeight.w700,
                                      color: roleTextColor,
                                    ),
                                  ),
                                ),
                            ],
                          ),
                          const SizedBox(height: 2),
                          Text(
                            placeholderDesc,
                            style: const TextStyle(
                              fontFamily: 'Inter',
                              fontSize: 14,
                              color: _textMuted,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(width: 8),
                    Row(
                      children: [
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                          decoration: BoxDecoration(
                            color: _surfaceContainer,
                            borderRadius: BorderRadius.circular(9999),
                          ),
                          child: const Text(
                            'Members',
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontSize: 12,
                              fontWeight: FontWeight.w700,
                              color: _onSurfaceVariant,
                            ),
                          ),
                        ),
                        const SizedBox(width: 8),
                        const Icon(
                          Icons.chevron_right,
                          color: _textMuted,
                        ),
                      ],
                    )
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

