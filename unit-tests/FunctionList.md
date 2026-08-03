# Function List - AISAM Unit Test

| Requirement Name | Function Code | Class Name | Function Name | Description | Test Class |
|-----------------|---------------|------------|---------------|-------------|------------|
| Sign up | AUTH-01 | AuthService | RegisterAsync | Register user with email/password, create workspace, send verification email | AuthRegistrationWorkspaceTests |
| Sign In (Email/Password) | AUTH-02 | AuthService | LoginAsync | Authenticate user by email/password, return JWT tokens, update LastLoginAt | Core business logic |
| Sign In With Google | AUTH-03 | AuthService | GoogleLoginAsync | Login via Google OAuth ID token, auto-create user if new | Core business logic |
| Token Management | AUTH-04 | AuthService | RefreshTokenAsync | Rotate refresh token, detect token reuse, revoke old sessions | Core business logic |
| Workspace Management | WS-01 | WorkspaceService | CreateAsync | Create workspace with type validation, 1 Personal per user, Business requires payment | WorkspaceServiceTests |
| Workspace Management | WS-02 | WorkspaceService | GetByUserIdAsync | Get all workspaces for user with lifecycle synchronization | WorkspaceServiceTests |
| Workspace Management | WS-03 | WorkspaceService | UpdateAsync | Update workspace name, Owner-only access, block read-only workspaces | WorkspaceServiceTests |
| Workspace Management | WS-04 | WorkspaceService | AdminSoftDeleteAsync | Admin-only soft delete, only EligibleForDeletion status | WorkspaceServiceTests |
| Manual Content Creation | CT-01 | ContentService | CreateAsync | Create content with brand/product validation, status validation | ContentServiceTests |
| Published Posts Management | CT-03 | ContentService | PublishAsync | Publish to social platform, validate tokens/quota/workspace status | ContentServicePublishTests |
| Payment & Subscription | PM-01 | PayOSPaymentService | CreateCheckoutAsync | Create PayOS checkout for subscription or credit pack purchase | PaymentServiceTests |
| Payment & Subscription | PM-02 | PayOSPaymentService | CreateBusinessWorkspaceCheckoutAsync | Create payment checkout for Business workspace creation | PaymentServiceTests |
| Payment & Subscription | PM-03 | PayOSPaymentService | HandleCallbackAsync | Process PayOS callback, validate signature before order code check | PaymentServiceTests |
| Payment & Subscription | PM-06 | PayOSPaymentService | HandleWebhookAsync | Process PayOS webhook payload, verify signature, update payment status | PaymentServiceTests |
| Payment & Subscription | PM-07 | PayOSPaymentService | SynchronizeBusinessWorkspaceCheckoutAsync | Sync Business workspace payment, activate workspace after payment | PaymentServiceTests |
| Credit, Quota & Wallet | CR-01 | CreditService | ConsumeCreditsAsync | Consume credits from wallet for AI action, record usage | CreditServiceTests |
| Credit, Quota & Wallet | CR-02 | CreditService | GrantSubscriptionCreditsAsync | Grant initial credits based on subscription plan | CreditServiceTests |
| Credit, Quota & Wallet | CR-03 | CreditService | EnsureCurrentFreeCreditsAsync | Reset personal free wallet at 7-day cycle | CreditServiceTests |
| Credit, Quota & Wallet | CR-04 | CreditService | RecordUsageAsync | Store credit usage metadata without prompt content | CreditServiceTests |
| Social Accounts | SC-01 | SocialService | GetAuthUrlAsync | Create OAuth state for active profile | SocialServiceTests |
| Social Accounts | SC-02 | SocialService | LinkAccountAsync | Link social account via OAuth callback, update existing token | SocialServiceTests |
| Social Accounts | SC-03 | SocialService | LinkAccountInWorkspaceAsync | Link account in workspace context, create with protected token | SocialServiceTests |
| Social Accounts | SC-04 | SocialService | LinkSelectedTargetsInWorkspaceAsync | Link pages/groups in workspace, create integrations | SocialServiceTests |
| Social Accounts | SC-05 | SocialService | LinkSelectedTargetsForAccountAsync | Link pages/groups with brand validation, create integration with protected token | SocialServiceTests |
| Social Accounts | SC-06 | SocialService | UnlinkAccountAsync | Soft delete account and all integrations | SocialServiceTests |
| Social Accounts | SC-07 | SocialService | UnlinkTargetAsync | Soft delete only requested integration | SocialServiceTests |
| Social Accounts | SC-08 | SocialService | GetWorkspaceAccountsAsync | List workspace accounts without decrypting stored tokens | SocialServiceTests |
| AI Content Automation | AT-01 | AutomationService | ImportCsvAsync | Import CSV file, parse rows, combine date+time columns | AutomationServiceTests |
| AI Content Automation | AT-02 | AutomationService | CreateAsync | Split one row into platform-specific items with stable keys | AutomationServiceTests |
| AI Content Automation | AT-03 | AutomationService | ConfirmAsync | Confirm automation plan, create and validate items | AutomationServiceTests |
| AI Content Automation | AT-04 | AutomationService | UpdateItemAsync | Revalidate invalid item before re-confirmation | AutomationServiceTests |
| Analytics & Dashboard | DB-01 | DashboardService | GetWorkspaceSummaryAsync | Dashboard KPI cards scoped to workspace | DashboardServiceTests |
| Analytics & Dashboard | DB-02 | DashboardService | GetSummaryAsync | Dashboard KPI cards scoped to profile | DashboardServiceTests |
| Notification Management | NT-01 | NotificationService | GetPagedAsync | Paginated notification list for active profile | NotificationServiceTests |
| Notification Management | NT-02 | NotificationService | MarkReadAsync | Mark single notification as read with profile ownership check | NotificationServiceTests |
| Notification Management | NT-03 | NotificationService | MarkAllReadAsync | Mark all notifications as read for profile | NotificationServiceTests |
| Notification Management | NT-04 | NotificationService | GetUnreadCountAsync | Get unread count for badge display per profile | NotificationServiceTests |
| AI Generate (AI Content Creation) | CV-01 | ConversationService | GetByIdAsync | Get conversation detail by ID with profile ownership check | ConversationServiceTests |
| AI Generate (AI Content Creation) | CV-02 | ConversationService | SoftDeleteAsync | Soft delete conversation with profile ownership check | ConversationServiceTests |
| AI Generate (AI Content Creation) | CV-03 | ConversationService | GetPagedAsync | Paginated conversation list for active profile | ConversationServiceTests |
| Published Posts Management | PS-01 | PostService | GetPagedAsync | Paginated post list with brand/status filters scoped to profile | PostServiceTests |
| Published Posts Management | PS-02 | PostService | GetByIdAsync | Get post by ID with profile ownership check | PostServiceTests |
| Published Posts Management | PS-03 | PostService | GetPagedByWorkspaceAsync | Paginated post list scoped to workspace | PostServiceTests |
| AI Generate (AI Content Creation) | AI-01 | AIService | GenerateDraftAsync | Generate AI draft with credit consumption and error handling | AIServiceTests |
| AI Generate (AI Content Creation) | AI-02 | AIService | ChatAsync | Chat with AI, save messages, consume credits, handle generation response | AIServiceTests |
| AI Generate (AI Content Creation) | AI-03 | AIService | ImproveAsync | Improve existing content with prompt quota check | AIServiceTests |
| AI Generate (AI Content Creation) | AI-04 | AIService | ApproveGenerationAsync | Approve AI generation, copy text, set PendingApproval status | AIServiceTests |
| AI Generate (AI Content Creation) | AI-05 | AIService | GetGenerationsAsync | Get generations for content with profile ownership validation | AIServiceTests |
| AI Generate (AI Content Creation) | AI-06 | AIService | ChatInWorkspaceAsync | Chat with AI in workspace context with credit consumption | AIServiceTests |
| Brand Kit | BR-01 | BrandService | CreateAsync | Create brand in workspace, validate name, check membership | BrandWorkspaceOwnershipTests |
| Brand Kit | BR-02 | BrandService | GetByIdAsync | Get brand by ID across workspace boundary | BrandWorkspaceOwnershipTests |
| Brand Kit | BR-03 | BrandService | GetPagedByWorkspaceIdAsync | Paginated brand list scoped to workspace | BrandWorkspaceOwnershipTests |
| Product Management | PR-01 | ProductService | CreateAsync | Create product with brand workspace validation | ProductWorkspaceOwnershipTests |
| Product Management | PR-02 | ProductService | GetPagedAsync | Paginated product list from active workspace brands | ProductWorkspaceOwnershipTests |
| Credit, Quota & Wallet | QT-01 | QuotaService | GetWorkspaceSummaryAsync | Get workspace quota summary from active workspace usage | QuotaServiceTests |
| Credit, Quota & Wallet | QT-02 | QuotaService | EnsureWorkspacePostQuotaAsync | Check workspace post quota before publishing | QuotaServiceTests |
| Credit, Quota & Wallet | QT-03 | QuotaService | EnsurePromptQuotaAsync | Check prompt/AI quota before generation | QuotaServiceTests |
| Credit, Quota & Wallet | QT-04 | QuotaService | EnsurePostQuotaAsync | Check post quota before publishing | QuotaServiceTests |
| Workspace Management | WM-01 | WorkspaceMemberService | GetMembersAsync | List members, allow active member access | WorkspaceMemberServiceTests |
| Workspace Management | WM-02 | WorkspaceMemberService | UpdateRoleAsync | Update member role, Owner-only for non-owner targets | WorkspaceMemberServiceTests |
| Workspace Management | WM-03 | WorkspaceMemberService | UpdateQuotaAsync | Assign monthly quota limit for Business Pro members | WorkspaceMemberServiceTests |
| Workspace Management | WM-04 | WorkspaceMemberService | RemoveAsync | Remove member, Owner-only for non-owner targets | WorkspaceMemberServiceTests |
| Workspace Management | WM-05 | WorkspaceMemberService | TransferOwnershipAsync | Transfer workspace ownership, only to Manager role | WorkspaceMemberServiceTests |
| Profile & User Management | FD-01 | ProfileService | GetProfileByIdAsync | Get profile with user ownership check | FoundationTests |
| Profile & User Management | FD-02 | ProfileService | CreateProfileAsync | Create profile, reject if avatar file upload not enabled | FoundationTests |
| Profile & User Management | FD-03 | ProfileService | UpdateProfileAsync | Update profile with ownership validation, reject avatar file upload | FoundationTests |
| Profile & User Management | FD-04 | ProfileService | DeleteProfileAsync | Soft delete profile with ownership validation | FoundationTests |
| Profile & User Management | FD-05 | ProfileService | RestoreProfileAsync | Restore soft-deleted profile with ownership validation | FoundationTests |
| Product Management | FD-06 | ProductService | CreateAsync | Create product with image file handling | FoundationTests |
| Product Management | FD-07 | ProductService | UpdateAsync | Update product with image file handling | FoundationTests |
| Email Service | FD-08 | EmailService | SendEmailAsync | Send email, return false when SMTP not configured | FoundationTests |
| Content Scheduling & Calendar | CS-01 | ContentScheduleService | CreateAsync | Create pending schedule for content and integration | ContentScheduleServiceTests |
| Content Scheduling & Calendar | CS-02 | ContentScheduleService | UpdateAsync | Update schedule, reject completed schedules | ContentScheduleServiceTests |
| Content Scheduling & Calendar | CS-03 | ContentScheduleService | GetUpcomingAsync | Get future schedules for profile | ContentScheduleServiceTests |
| Content Scheduling & Calendar | SP-01 | ScheduledPostingService | RunDueSchedulesAsync | Execute due schedules, handle publish success/failure/quota/expired workspace | ScheduledPostingServiceTests |
| AI Service Clients | GC-01 | GeminiTextClient | GenerateAsync | Generate text via Gemini API, validate API key, parse response | GeminiTextClientTests |
| AI Service Clients | VC-01 | DeApiVideoClient | TryExtractVideoUrl | Parse video URL from API response JSON | DeApiVideoClientTests |
| Ad Campaign Management | AC-01 | AdCampaignService | CreateAsync | Create ad campaign with targeting, budget validation, and objective | AdCampaignServiceTests |
| Ad Campaign Management | AC-02 | AdCampaignService | DeployAsync | Deploy campaign to Facebook, validate creative and state | AdCampaignServiceTests |
| Ad Campaign Management | AC-03 | AdCampaignService | SyncCampaignInsightsAsync | Sync campaign insights from Facebook | AdCampaignServiceTests |
| Social Accounts | TP-01 | SocialTokenProtector | Protect | Encrypt social token plaintext to ciphertext | SocialTokenProtectorTests |
| Social Accounts | TP-02 | SocialTokenProtector | Unprotect | Decrypt social token ciphertext back to plaintext | SocialTokenProtectorTests |
| Social Accounts | OA-01 | MemoryOAuthStateStore | CreateAsync | Store OAuth state with profile/provider/expiry | OAuthStateStoreTests |
| Social Accounts | OA-02 | MemoryOAuthStateStore | ConsumeAsync | Consume OAuth state once, validate profile and expiry | OAuthStateStoreTests |
| Workspace Management | WO-01 | Entity Model | WorkspaceOwnership | Verify WorkspaceId column NOT NULL on all ownership entities | RemainingWorkspaceOwnershipTests |
| Workspace Management | WO-02 | Repository queries | WorkspaceQueries | Verify workspace-scoped queries isolate data correctly | RemainingWorkspaceOwnershipTests |
