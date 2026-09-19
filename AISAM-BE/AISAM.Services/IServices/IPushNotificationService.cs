namespace AISAM.Services.IServices;

public interface IPushNotificationService
{
    Task SendNotificationAsync(
        Guid profileId,
        string title,
        string message,
        IDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);

    Task SendNotificationToProfilesAsync(
        IEnumerable<Guid> profileIds,
        string title,
        string message,
        IDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}
