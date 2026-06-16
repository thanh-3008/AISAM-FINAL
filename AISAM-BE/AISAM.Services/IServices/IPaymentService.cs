using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using Microsoft.AspNetCore.Http;

namespace AISAM.Services.IServices;

public interface IPaymentService
{
    Task<GenericResponse<PayOSCheckoutResponse>> CreateCheckoutAsync(Guid workspaceId, Guid userId, CreateCheckoutRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> SyncReturnAsync(Guid workspaceId, Guid userId, IQueryCollection query, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> HandleCallbackAsync(IQueryCollection query, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> HandleWebhookAsync(string rawPayload, CancellationToken cancellationToken = default);
    Task<GenericResponse<PagedResult<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<CurrentSubscriptionDto>> GetCurrentSubscriptionAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}
