using System.Net;
using System.Text.Json;
using AISAM.Common;

namespace AISAM.API.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    _logger.LogInformation("Request was cancelled by the client.");
                }
                else
                {
                    _logger.LogError(ex, "An unhandled exception occurred");
                }

                if (!context.Response.HasStarted)
                {
                    await HandleExceptionAsync(context, ex);
                }
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            HttpStatusCode status;
            string message;
            string errorCode;

            switch (exception)
            {
                case AISAM.Data.MutationAuthorizationException:
                    status = HttpStatusCode.Forbidden;
                    message = "Permission changed or expired before the write.";
                    errorCode = "MUTATION_AUTHORIZATION_CHANGED";
                    break;
                case Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
                    status = HttpStatusCode.Conflict;
                    message = "Access or resource state changed. Reload before retrying.";
                    errorCode = "CONCURRENT_ACCESS_CHANGE";
                    break;
                case AISAM.Services.Exceptions.ResourceAccessDeniedException:
                    status = HttpStatusCode.Forbidden;
                    message = "The current permission does not allow this action.";
                    errorCode = "RESOURCE_ACCESS_DENIED";
                    break;
                case UnauthorizedAccessException:
                    status = HttpStatusCode.Unauthorized;
                    message = "Unauthorized";
                    errorCode = "UNAUTHORIZED";
                    break;
                case ArgumentException:
                case InvalidOperationException:
                    status = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    errorCode = "BAD_REQUEST";
                    break;
                case KeyNotFoundException:
                    status = HttpStatusCode.NotFound;
                    message = exception.Message;
                    errorCode = "NOT_FOUND";
                    break;
                case OperationCanceledException:
                    status = (HttpStatusCode)499; // Client Closed Request
                    message = "Request was cancelled";
                    errorCode = "REQUEST_CANCELLED";
                    break;
                default:
                    status = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred";
                    errorCode = "INTERNAL_SERVER_ERROR";
                    break;
            }

            context.Response.StatusCode = (int)status;

            var response = GenericResponse<object>.CreateError(
                message,
                status,
                errorCode);

            if (context.RequestServices.GetService(typeof(IWebHostEnvironment)) is IWebHostEnvironment env && env.IsDevelopment())
            {
                response.Error ??= new ErrorDetails();
                response.Error.StackTrace = exception.ToString();
                response.Error.ErrorMessage = exception.Message;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(response, options);
            return context.Response.WriteAsync(json);
        }
    }
}
