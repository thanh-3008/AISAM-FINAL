using Microsoft.AspNetCore.Http;

namespace AISAM.Services.IServices;

public interface IOriginResolver
{
    string ResolveOrigin(HttpRequest request);
    string ResolveOrigin(string? candidateOrigin);
    bool IsAllowedOrigin(string? origin);
}
