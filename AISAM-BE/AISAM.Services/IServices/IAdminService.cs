using AISAM.Common;
using AISAM.Common.Dtos.Admin;

namespace AISAM.Services.IServices;

public interface IAdminService
{
    Task<GenericResponse<AdminDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default);
}
