using System.Net;
using System.Text.Json.Serialization;

namespace AISAM.Common
{
    public class GenericResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("error")]
        public ErrorDetails? Error { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static GenericResponse<T> CreateSuccess(T data, string message = "Operation successful")
        {
            return new GenericResponse<T>
            {
                Success = true,
                Message = message,
                StatusCode = (int)HttpStatusCode.OK,
                Data = data
            };
        }

        public static GenericResponse<T> CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string? errorCode = null)
        {
            return new GenericResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = (int)statusCode,
                Error = new ErrorDetails
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }
    }

    public class ErrorDetails
    {
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("stackTrace")]
        public string? StackTrace { get; set; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, List<string>>? ValidationErrors { get; set; }
    }

    public class GenericResponse : GenericResponse<object>
    {
        public static GenericResponse CreateSuccess(string message = "Operation successful")
        {
            return new GenericResponse
            {
                Success = true,
                Message = message,
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
