import 'package:go_router/go_router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import '../features/auth/presentation/login_screen.dart';
import '../features/auth/presentation/register_screen.dart';
import '../features/auth/presentation/forgot_password_screen.dart';
import '../features/auth/presentation/verify_email_screen.dart';
import '../features/dashboard/presentation/dashboard_screen.dart';
import '../features/workspace/presentation/workspace_list_screen.dart';
import '../features/workspace/presentation/create_workspace_screen.dart';
import '../features/workspace/presentation/workspace_detail_screen.dart';
import '../features/profile/presentation/profile_list_screen.dart';
import '../features/profile/presentation/create_profile_screen.dart';
import '../features/profile/presentation/brand_list_screen.dart';
import '../features/profile/presentation/create_brand_screen.dart';
import '../features/profile/presentation/product_list_screen.dart';
import '../features/profile/presentation/create_product_screen.dart';
import '../features/content/presentation/content_list_screen.dart';
import '../features/content/presentation/content_detail_screen.dart';
import '../features/content/presentation/content_editor_screen.dart';
import '../features/content/presentation/ai_generate_screen.dart';
import '../features/content/presentation/screens/tiktok_video_review_screen.dart';
import '../features/calendar/presentation/screens/calendar_screen.dart';
import '../features/approval/presentation/screens/approval_list_screen.dart';
import '../features/chat/presentation/screens/conversation_list_screen.dart';
import '../features/chat/presentation/screens/chat_screen.dart';
import '../core/storage/secure_storage.dart';
import '../core/services/logger_service.dart';
import 'shell_screen.dart';
import '../features/settings/presentation/settings_screen.dart';
import '../features/settings/presentation/account_screen.dart';
import '../features/settings/presentation/screens/social_connections_screen.dart';
import '../features/settings/presentation/team_settings_screen.dart';
import '../features/settings/presentation/language_screen.dart';
import '../features/notifications/presentation/notifications_screen.dart';
import '../features/billing/presentation/billing_screen.dart';

part 'router.g.dart';

@riverpod
GoRouter router(RouterRef ref) {
  return GoRouter(
    initialLocation: '/login',
    redirect: (context, state) async {
      final storage = ref.read(secureStorageProvider);
      final token = await storage.getAccessToken();
      final workspaceId = await storage.getActiveWorkspaceId();
      
      final isAuthRoute = state.uri.path == '/login' || 
                          state.uri.path == '/register' ||
                          state.uri.path == '/forgot-password' ||
                          state.uri.path == '/verify-email';
      final isDashboardRoute = state.uri.toString().startsWith('/dashboard');
      final isProfileRoute = state.uri.toString().startsWith('/profiles') ||
                             state.uri.toString().startsWith('/brands') ||
                             state.uri.toString().startsWith('/products');
      final isContentRoute = state.uri.toString().startsWith('/content');
      final isCalendarRoute = state.uri.toString().startsWith('/calendar');
      final isApprovalRoute = state.uri.toString().startsWith('/approvals');
      final isChatRoute = state.uri.toString().startsWith('/chat');

      LoggerService.d('Router Redirect: path=${state.uri}, hasToken=${token != null}, hasWorkspace=${workspaceId != null}');

      // Auth Guard
      if (token == null && !isAuthRoute) {
        return '/login';
      }
      
      // Guest Guard
      if (token != null && isAuthRoute) {
        return workspaceId != null ? '/dashboard' : '/overview';
      }

      // Workspace Guard
      if (token != null && (isDashboardRoute || isProfileRoute || isContentRoute || isCalendarRoute || isApprovalRoute || isChatRoute || state.uri.toString().startsWith('/settings')) && workspaceId == null) {
        return '/overview';
      }
      
      return null;
    },
    routes: [
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginScreen(),
      ),
      GoRoute(
        path: '/register',
        builder: (context, state) => const RegisterScreen(),
      ),
      GoRoute(
        path: '/forgot-password',
        builder: (context, state) => const ForgotPasswordScreen(),
      ),
      GoRoute(
        path: '/verify-email',
        builder: (context, state) => VerifyEmailScreen(
          email: state.uri.queryParameters['email'],
        ),
      ),
      GoRoute(
        path: '/overview',
        builder: (context, state) => const WorkspaceListScreen(),
      ),
      GoRoute(
        path: '/workspace/create',
        builder: (context, state) => const CreateWorkspaceScreen(),
      ),
      GoRoute(
        path: '/workspace/:id',
        builder: (context, state) => WorkspaceDetailScreen(workspaceId: state.pathParameters['id']!),
      ),
      
      // Main App Shell (Bottom Navigation)
      StatefulShellRoute.indexedStack(
        builder: (context, state, navigationShell) {
          return ShellScreen(navigationShell: navigationShell);
        },
        branches: [
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/dashboard',
                builder: (context, state) => const DashboardScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/approvals',
                builder: (context, state) => const ApprovalListScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/calendar',
                builder: (context, state) => const CalendarScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/settings',
                builder: (context, state) => const SettingsScreen(),
              ),
            ],
          ),
        ],
      ),

      // Settings Sub-Routes
      GoRoute(
        path: '/settings/account',
        builder: (context, state) => const AccountScreen(),
      ),
      GoRoute(
        path: '/settings/notifications',
        builder: (context, state) => const NotificationsScreen(),
      ),
      GoRoute(
        path: '/settings/team',
        builder: (context, state) => const TeamSettingsScreen(),
      ),
      GoRoute(
        path: '/settings/social',
        builder: (context, state) => const SocialConnectionsScreen(),
      ),
      GoRoute(
        path: '/settings/language',
        builder: (context, state) => const LanguageScreen(),
      ),
      GoRoute(
        path: '/settings/billing',
        builder: (context, state) => const BillingScreen(),
      ),

      // Profile Routes
      GoRoute(
        path: '/profiles',
        builder: (context, state) => const ProfileListScreen(),
      ),
      GoRoute(
        path: '/profiles/create',
        builder: (context, state) => const CreateProfileScreen(),
      ),
      GoRoute(
        path: '/brands',
        builder: (context, state) => const BrandListScreen(),
      ),
      // Brand Routes
      GoRoute(
        path: '/brands/create',
        builder: (context, state) => const CreateBrandScreen(),
      ),
      GoRoute(
        path: '/brands/:id/products',
        builder: (context, state) => ProductListScreen(brandId: state.pathParameters['id']!),
      ),
      // Product Routes
      GoRoute(
        path: '/products/create',
        builder: (context, state) => CreateProductScreen(brandId: state.uri.queryParameters['brandId']!),
      ),
      // Content Routes
      GoRoute(
        path: '/content',
        builder: (context, state) => const ContentListScreen(),
      ),
      GoRoute(
        path: '/content/create',
        builder: (context, state) {
          final extra = state.extra as Map<String, dynamic>?;
          return ContentEditorScreen(
            prefillBrandId: extra?['brandId'] as String?,
            prefillTitle: extra?['title'] as String?,
            prefillContent: extra?['content'] as String?,
          );
        },
      ),
      GoRoute(
        path: '/content/generate-ai',
        builder: (context, state) => const AiGenerateScreen(),
      ),
      GoRoute(
        path: '/content/:id',
        builder: (context, state) => ContentDetailScreen(contentId: state.pathParameters['id']!),
      ),
      GoRoute(
        path: '/content/:id/tiktok-review',
        builder: (context, state) => TiktokVideoReviewScreen(contentId: state.pathParameters['id']!),
      ),
      GoRoute(
        path: '/content/:id/edit',
        builder: (context, state) => ContentEditorScreen(contentId: state.pathParameters['id']!),
      ),
      // Chat Routes
      GoRoute(
        path: '/chat',
        builder: (context, state) => const ConversationListScreen(),
      ),
      GoRoute(
        path: '/chat/new',
        builder: (context, state) => const ChatScreen(),
      ),
      GoRoute(
        path: '/chat/:id',
        builder: (context, state) => ChatScreen(conversationId: state.pathParameters['id']!),
      ),
    ],
  );
}

