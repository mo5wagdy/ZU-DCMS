using System.Net;
using System.Text.Json;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.APPLICATION.Exceptions;

namespace ZU_DCMS.API.Middlewares
{
    /// <summary>
    /// Middleware to catch unhandled exceptions globally, map them to HTTP status codes,
    /// and return standardized API responses in a unified format.
    /// </summary>
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAppLogger<CustomExceptionMiddleware> _logger;

        public CustomExceptionMiddleware(RequestDelegate next, IAppLogger<CustomExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Proceed with the rest of the request pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception details using application logger
                _logger.LogError(ex.Message, ex);
                
                // Handle the exception and format the response to the user
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Maps common exceptions to domain-specific HTTP status codes and wraps them in a standard ApiResponse.
        /// </summary>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var response = ApiResponse<object>.Failure(exception.Message, "An internal server error occurred.");

            // Map custom exceptions to appropriate HTTP status codes and response structures
            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.Failure(validationException.Errors.ToList(), "Validation Error");
                    break;
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    response = ApiResponse<object>.Failure(notFoundException.Message, "Resource Not Found");
                    break;
                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.Failure(unauthorizedAccessException.Message, "Unauthorized Access");
                    break;
            }

            // Configure the HTTP response properties
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Serialize the generic application response object to camel case json
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
