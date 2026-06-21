namespace AISAM.Common.Messages;

public static class MessageConstants
{
    public static class General
    {
        public const string OperationSuccessful = "Operation successful";
        public const string SystemError = "System error";
        public const string NotFound = "Not found.";
        public const string ValidationFailed = "Validation failed";
        public const string UnexpectedError = "An unexpected error occurred";
        public const string Unauthorized = "Unauthorized";
        public const string AuthenticationRequired = "Authentication is required.";
    }

    public static class Content
    {
        // Success
        public const string CreatedSuccess = "Content created successfully.";
        public const string RetrievedSuccess = "Content retrieved successfully.";
        public const string UpdatedSuccess = "Content updated successfully.";
        public const string DeletedSuccess = "Content deleted successfully.";
        public const string RestoredSuccess = "Content restored successfully.";
        public const string PublishedSuccess = "Content published successfully.";
        public const string ClonedSuccess = "Content cloned successfully.";
        public const string ListRetrievedSuccess = "Contents retrieved successfully.";

        // Error
        public const string NotFound = "Content not found.";
        public const string AlreadyPublished = "Content has already been published.";
        public const string MustBeApproved = "Content must be approved before publishing.";
        public const string CannotChangePublished = "Cannot change status of published content.";
        public const string UsePublishEndpoint = "Use the publish endpoint to publish content.";
        public const string NotDeleted = "Content is not deleted.";
        public const string InvalidStatusTransition = "Cannot transition content from {0} to {1}.";
        public const string InvalidSelectedStatus = "Only Draft or PendingApproval status can be selected when creating non-ad content.";
        public const string BrandNotFound = "Brand not found.";
        public const string ProductNotFound = "Product not found.";
        public const string ProductNotInBrand = "Product does not belong to the selected brand.";

        // Publishing errors
        public const string SocialIntegrationNotFoundOrInactive = "Social integration not found or inactive.";
        public const string PublishingProviderNotSupported = "Publishing provider is not supported.";
        public const string SocialAccountNotFound = "Social account not found.";
        public const string SocialAccountInactive = "Social account is inactive.";
        public const string SocialAccountTokenMissing = "Social account token is missing. Please reconnect the account.";
        public const string SocialAccountTokenExpired = "Social account token has expired. Please reconnect the account.";
        public const string IntegrationTokenMissing = "Integration access token is missing. Please reconnect the account.";
        public const string TokenDecryptionFailed = "Token decryption failed. Please reconnect the account.";
        public const string PublishingFailed = "Publishing failed.";
        public const string WorkspaceExpiredOrInactive = "Publishing is blocked because the workspace is expired or inactive.";
    }

    public static class Schedule
    {
        // Success
        public const string RetrievedSuccess = "Schedule retrieved successfully.";
        public const string CreatedSuccess = "Schedule created successfully.";
        public const string BulkCreated = "{0}/{1} schedules created.";
        public const string ZeroCreated = "0 schedules created";
        public const string BulkFailed = "Bulk schedule failed.";
        public const string SchedulesRetrieved = "Schedules retrieved successfully.";

        // Error
        public const string NotFound = "Schedule not found.";
        public const string ScheduledTimeInvalid = "Scheduled time is invalid.";
        public const string ScheduledTimeMustBeFuture = "Scheduled time must be in the future.";
        public const string AlreadyHasActiveSchedule = "Content already has an active schedule.";
        public const string CannotUpdateCompleted = "Completed schedules cannot be updated.";
        public const string SocialIntegrationNotFound = "Social integration not found.";
        public const string ContentNotFound = "Content not found.";

        // Worker
        public const string SchedulePublishSucceeded = "Scheduled publish succeeded";
        public const string SchedulePublishFailed = "Scheduled publish failed";
        public const string SchedulePublishWillRetry = "Scheduled publish will retry";
        public const string ContentPublishedSuccessfully = "Content {0} was published successfully.";
        public const string WorkspaceBlocked = "Scheduled publishing is blocked because the workspace is expired or inactive.";
        public const string WorkerIterationFailed = "Scheduled posting worker iteration failed.";
    }

    public static class Brand
    {
        // Success
        public const string CreatedSuccess = "Brand created successfully";
        public const string RetrievedSuccess = "Brand retrieved successfully";
        public const string UpdatedSuccess = "Brand updated successfully";
        public const string DeletedSuccess = "Brand deleted successfully";
        public const string RestoredSuccess = "Brand restored successfully";
        public const string ListRetrievedSuccess = "Brands retrieved successfully";

        // Error
        public const string NotFound = "Brand not found";
        public const string ProfileNotFound = "Profile not found";
        public const string ProfileAccessDenied = "You are not allowed to access this profile";
        public const string WorkspaceAccessDenied = "You are not allowed to access this workspace";
        public const string NotDeleted = "Brand is not deleted";
    }

    public static class Product
    {
        // Success
        public const string CreatedSuccess = "Product created successfully";
        public const string RetrievedSuccess = "Product retrieved successfully";
        public const string UpdatedSuccess = "Product updated successfully";
        public const string DeletedSuccess = "Product deleted successfully";
        public const string RestoredSuccess = "Product restored successfully";
        public const string ListRetrievedSuccess = "Products retrieved successfully";

        // Error
        public const string NotFound = "Product not found";
        public const string BrandNotFound = "Brand not found";
        public const string BrandAccessDenied = "You are not allowed to access this brand";
        public const string AccessDenied = "You are not allowed to access this product";
        public const string UpdateDenied = "You are not allowed to update this product";
        public const string DeleteDenied = "You are not allowed to delete this product";
        public const string RestoreDenied = "You are not allowed to restore this product";
        public const string NotDeleted = "Product is not deleted";
        public const string ImageUploadNotEnabled = "Product image upload is not enabled in the current MVP backend.";
    }

    public static class Profile
    {
        // Success
        public const string CreatedSuccess = "Profile created successfully";
        public const string RetrievedSuccess = "Profile retrieved successfully";
        public const string UpdatedSuccess = "Profile updated successfully";
        public const string DeletedSuccess = "Profile deleted successfully";
        public const string RestoredSuccess = "Profile restored successfully";
        public const string ListRetrievedSuccess = "Profiles retrieved successfully";

        // Error
        public const string NotFound = "Profile not found";
        public const string UserNotFound = "User not found";
        public const string ProfileAccessDenied = "You are not allowed to access this profile";
        public const string CreateAccessDenied = "You are not allowed to create profiles for another user";
        public const string AvatarUploadNotEnabled = "Avatar file upload is not enabled in the current MVP backend. Use AvatarUrl instead.";
        public const string InvalidProfileContext = "Invalid profile context.";
        public const string MissingProfileHeader = "Missing or invalid X-Profile-Id header.";
    }

    public static class Workspace
    {
        // Success
        public const string CreatedSuccess = "Workspace created successfully.";
        public const string RetrievedSuccess = "Workspace retrieved successfully.";
        public const string UpdatedSuccess = "Workspace updated successfully.";
        public const string DeletedSuccess = "Workspace soft deleted successfully.";
        public const string ListRetrievedSuccess = "Workspaces retrieved successfully.";

        // Error
        public const string NotFound = "Workspace not found.";
        public const string UserNotFound = "User not found.";
        public const string InvalidType = "Invalid workspace type.";
        public const string PersonalWorkspaceLimit = "Each account can only have one personal workspace.";
        public const string OwnerOnlyUpdate = "Only the workspace owner can update the workspace.";
        public const string ReadOnlyExpired = "Workspace is read-only while its subscription is expired.";
        public const string AdminOnlyDelete = "Only an administrator can delete a workspace.";
        public const string NotEligibleForDeletion = "Workspace is not eligible for deletion.";
        public const string MissingHeader = "Missing or invalid X-Workspace-Id header.";
        public const string NotAMember = "You are not a member of this workspace.";
        public const string ProfileNotInWorkspace = "Profile does not belong to active workspace.";

        // Invitation
        public const string InvitationRevoked = "Invitation revoked successfully.";
        public const string MemberRemoved = "Workspace member removed successfully.";
    }

    public static class Auth
    {
        public const string EmailAlreadyExists = "User with this email already exists";
        public const string InvalidCredentials = "Invalid email or password";
        public const string GoogleLoginNotConfigured = "Google login is not configured";
        public const string InvalidGoogleToken = "Invalid Google token";
        public const string GoogleEmailNotVerified = "Google email is not verified";
        public const string InvalidOrExpiredRefreshToken = "Invalid or expired refresh token";
        public const string UserNotFound = "User not found";
        public const string CurrentPasswordIncorrect = "Current password is incorrect";
        public const string InvalidOrExpiredResetToken = "Invalid or expired reset token";
        public const string RegistrationError = "An error occurred during registration";
        public const string LoginError = "An error occurred during login";
        public const string InvalidToken = "Invalid token";
    }

    public static class Social
    {
        public const string OAuthStateInvalid = "OAuth state is invalid or expired.";
        public const string AccountAlreadyLinked = "Social account is already linked to another workspace.";
        public const string WorkspaceContextRequired = "Workspace context is required.";
        public const string BrandNotFound = "Brand not found.";
        public const string TargetNotAvailable = "Selected target is not available for this account.";
        public const string AccountNotFound = "Social account not found.";
        public const string OnlyFacebookSupported = "Only Facebook is supported in Phase C.";
        public const string FacebookProviderNotRegistered = "Facebook provider is not registered.";
    }

    public static class Post
    {
        public const string RetrievedSuccess = "Post retrieved successfully.";
        public const string ListRetrievedSuccess = "Posts retrieved successfully.";
        public const string NotFound = "Post not found.";
    }

    public static class Notification
    {
        public const string RetrievedSuccess = "Notification retrieved successfully.";
        public const string ListRetrievedSuccess = "Notifications retrieved successfully.";
        public const string MarkedAsRead = "Notification marked as read.";
        public const string AllMarkedAsRead = "All notifications marked as read.";
        public const string UnreadCountRetrieved = "Unread notification count retrieved successfully.";
        public const string NotFound = "Notification not found.";
    }

    public static class Conversation
    {
        public const string RetrievedSuccess = "Conversation retrieved successfully.";
        public const string ListRetrievedSuccess = "Conversations retrieved successfully.";
        public const string DeletedSuccess = "Conversation deleted successfully.";
        public const string NotFound = "Conversation not found.";
    }

    public static class Credit
    {
        public const string NoCreditsForPlan = "No credits granted for the selected plan.";
        public const string InvalidCreditPack = "Credit pack amount is invalid.";
        public const string GrantedSuccess = "Subscription credits granted successfully.";
    }

    public static class Scheduler
    {
        public const string RunCompleted = "Scheduler run completed successfully.";
        public const string NotFound = "Not found.";
    }
}
