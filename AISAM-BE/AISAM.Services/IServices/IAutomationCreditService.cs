using AISAM.Common;
using AISAM.Data.Enumeration;

namespace AISAM.Services.IServices;

public interface IAutomationCreditService
{
    Task<GenericResponse<bool>> ReserveAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> SettleAsync(Guid itemId, Guid userId, CreditActionEnum action, int amount, int expectedItemUsedCredits, CancellationToken cancellationToken = default);
    Task ReleaseAsync(Guid planId, CancellationToken cancellationToken = default);
}
