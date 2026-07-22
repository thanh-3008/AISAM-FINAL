using AISAM.Common;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices;

public interface IProductImportService
{
    Task<GenericResponse<ProductUrlExtractResponseDto>> ExtractFromUrlAsync(string url, CancellationToken cancellationToken = default);
}
