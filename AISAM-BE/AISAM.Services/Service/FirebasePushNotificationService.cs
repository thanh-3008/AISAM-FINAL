using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class FirebasePushNotificationService : IPushNotificationService
{
    private readonly IDeviceTokenRepository _deviceTokenRepository;
    private readonly ILogger<FirebasePushNotificationService> _logger;
    private readonly bool _isConfigured;
    private static readonly object _initLock = new();
    private static bool _initAttempted = false;

    public FirebasePushNotificationService(
        IDeviceTokenRepository deviceTokenRepository,
        IConfiguration configuration,
        ILogger<FirebasePushNotificationService> logger)
    {
        _deviceTokenRepository = deviceTokenRepository;
        _logger = logger;

        lock (_initLock)
        {
            if (!_initAttempted)
            {
                _initAttempted = true;
                InitializeFirebase(configuration);
            }
        }

        _isConfigured = FirebaseApp.DefaultInstance != null;
    }

    private void InitializeFirebase(IConfiguration configuration)
    {
        try
        {
            if (FirebaseApp.DefaultInstance != null)
            {
                return;
            }

            var credentialPath = configuration["Firebase:CredentialFilePath"]
                ?? configuration["FIREBASE_CREDENTIAL_PATH"]
                ?? Environment.GetEnvironmentVariable("FIREBASE_CREDENTIAL_PATH")
                ?? Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            var serviceAccountJson = configuration["Firebase:ServiceAccountJson"]
                ?? configuration["FIREBASE_SERVICE_ACCOUNT_JSON"]
                ?? Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");

            GoogleCredential? credential = null;

            if (!string.IsNullOrWhiteSpace(serviceAccountJson))
            {
                credential = GoogleCredential.FromJson(serviceAccountJson);
            }
            else if (!string.IsNullOrWhiteSpace(credentialPath) && File.Exists(credentialPath))
            {
                credential = GoogleCredential.FromFile(credentialPath);
            }

            if (credential != null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });
                _logger.LogInformation("FirebaseApp initialized successfully for push notifications.");
            }
            else
            {
                _logger.LogWarning("Firebase credentials not found. Push notification service will run in mock/graceful fallback mode.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize FirebaseApp. Push notifications will run in fallback mode.");
        }
    }

    public async Task SendNotificationAsync(
        Guid profileId,
        string title,
        string message,
        IDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        await SendNotificationToProfilesAsync(new[] { profileId }, title, message, data, cancellationToken);
    }

    public async Task SendNotificationToProfilesAsync(
        IEnumerable<Guid> profileIds,
        string title,
        string message,
        IDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        var targetProfileIds = profileIds.Distinct().ToList();
        if (targetProfileIds.Count == 0) return;

        var tokens = await _deviceTokenRepository.GetActiveTokensByProfileIdsAsync(targetProfileIds, cancellationToken);
        if (tokens.Count == 0)
        {
            _logger.LogDebug("No active device tokens found for target profiles ({Count} profiles).", targetProfileIds.Count);
            return;
        }

        if (!_isConfigured || FirebaseApp.DefaultInstance == null)
        {
            _logger.LogInformation("Push notification '{Title}' ({TokensCount} devices) skipped: Firebase credentials not configured.", title, tokens.Count);
            return;
        }

        var tokenStrings = tokens.Select(t => t.Token).Distinct().ToList();
        var payloadData = data != null
            ? new Dictionary<string, string>(data)
            : new Dictionary<string, string>();

        payloadData.TryAdd("click_action", "FLUTTER_NOTIFICATION_CLICK");

        // Process in batches of 500 (FCM Multicast limit)
        for (var i = 0; i < tokenStrings.Count; i += 500)
        {
            var batchTokens = tokenStrings.Skip(i).Take(500).ToList();

            var multicastMessage = new MulticastMessage
            {
                Tokens = batchTokens,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = message
                },
                Data = payloadData,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "aisam_high_importance",
                        Sound = "default",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                        Priority = NotificationPriority.HIGH
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        ContentAvailable = true
                    }
                }
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicastMessage, cancellationToken);

                _logger.LogInformation("Sent push notification '{Title}' to {Total} devices. Success: {Success}, Failure: {Failure}",
                    title, batchTokens.Count, response.SuccessCount, response.FailureCount);

                if (response.FailureCount > 0)
                {
                    var invalidTokens = new List<string>();
                    for (var r = 0; r < response.Responses.Count; r++)
                    {
                        var itemResponse = response.Responses[r];
                        if (!itemResponse.IsSuccess &&
                            itemResponse.Exception?.MessagingErrorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
                        {
                            invalidTokens.Add(batchTokens[r]);
                        }
                    }

                    if (invalidTokens.Count > 0)
                    {
                        await _deviceTokenRepository.DeactivateTokensAsync(invalidTokens, cancellationToken);
                        _logger.LogInformation("Deactivated {Count} stale or invalid device tokens.", invalidTokens.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send multicast push notification for title: {Title}", title);
            }
        }
    }
}
