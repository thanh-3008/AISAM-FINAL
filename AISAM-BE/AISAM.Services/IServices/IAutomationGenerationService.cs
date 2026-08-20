namespace AISAM.Services.IServices;

public interface IAutomationGenerationService
{
    Task<TimeSpan> ProcessNextAsync(CancellationToken cancellationToken = default);
}
