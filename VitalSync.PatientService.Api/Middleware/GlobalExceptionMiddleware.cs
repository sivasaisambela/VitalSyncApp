using System.Net;
using System.Text.Json;

namespace VitalSync.PatientService.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass request to the next middleware in line
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            // Map custom application exception types to HTTP status codes

            var (statusCode, title) = exception switch
            {
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource Not Found"),
                InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Invalid Operation"),
                ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
                _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            context.Response.StatusCode = statusCode;

            // RFC 7807 Standardized Error Response Payload
            var errorResponse = new
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(errorResponse, options);
            return context.Response.WriteAsync(json);

        }
    }
}
