namespace AISAM.Services.IServices;

public interface IAutomationGenerationService
{
    Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default);
}
