using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AISAM.API.Filters;

public sealed class WorkspaceHeaderOperationFilter : IOperationFilter
{
    private static readonly HashSet<string> ProtectedPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/ai",
        "/api/brands",
        "/api/content",
        "/api/content-schedules",
        "/api/dashboard",
        "/api/products",
        "/api/quota",
        "/api/workspace-members",
        "/api/workspace-invitations",
        "/api/workspace-dashboard",
        "/api/payment",
        "/api/posts",
        "/api/social",
        "/api/social-auth",
        "/api/conversations",
        "/api/notifications"
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = "/" + context.ApiDescription.RelativePath?.TrimEnd('/');
        if (!ProtectedPrefixes.Any(prefix => relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();
        if (operation.Parameters.Any(parameter =>
                parameter.In == ParameterLocation.Header &&
                string.Equals(parameter.Name, "X-Workspace-Id", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Workspace-Id",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
            Description = "Active workspace ID. Required for workspace-scoped endpoints."
        });
    }
}
