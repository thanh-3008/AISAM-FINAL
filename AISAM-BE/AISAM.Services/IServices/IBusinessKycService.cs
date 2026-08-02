using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices;

public interface IBusinessKycService
{
    Task<GenericResponse<BusinessKycVerificationResponse>> SubmitAsync(
        Guid userId,
        SubmitBusinessKycRequest request,
        CancellationToken cancellationToken = default);
}
