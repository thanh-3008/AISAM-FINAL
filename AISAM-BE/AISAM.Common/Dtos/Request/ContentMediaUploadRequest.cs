using Microsoft.AspNetCore.Http;

namespace AISAM.Common.Dtos.Request;

public sealed class ContentMediaUploadRequest
{
    public IFormFile? File { get; set; }
}
