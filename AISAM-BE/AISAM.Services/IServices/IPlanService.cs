using AISAM.Common;
using AISAM.Common.Dtos.Admin;

namespace AISAM.Services.IServices;

public interface IPlanService
{
    Task<GenericResponse<List<AdminPlanDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPlanDto>> CreateAsync(AdminCreatePlanRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPlanDto>> UpdateAsync(Guid id, AdminUpdatePlanRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
