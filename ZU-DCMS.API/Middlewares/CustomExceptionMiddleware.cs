using System.Net;
using System.Text.Json;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Exceptions;

namespace ZU_DCMS.API.Middlewares
{
    /// <summary>
    /// Global exception handler middleware
    /// Maps exceptions to HTTP responses in a unified ApiResponse format
    /// </summary>
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Execute next middleware in pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Handle any unhandled exception globally
                await HandleExceptionAsync(context, ex);

                //context.Response.StatusCode = 500;

                //await context.Response.WriteAsync(JsonSerializer.Serialize(new { type = ex.GetType().Name, message = ex.Message, stack = ex.StackTrace }));

                //return;
            }
        }

        /// <summary>
        /// Maps exceptions to proper HTTP responses
        /// </summary>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            ApiResponse<object> response;
            HttpStatusCode statusCode;

            switch (exception)
            {
                // ---------------- VALIDATION ERROR ----------------
                //case ValidationException validationException:
                //    statusCode = HttpStatusCode.BadRequest;

                //    response = ApiResponse<object>.Failure(
                //        new List<string> { "Validation Failed" },
                //        "Validation Error",
                //        validationException.Errors
                //    );
                //    break;

                // ---------------- NOT FOUND ----------------
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;

                    response = ApiResponse<object>.Failure(
                        new List<string> { notFoundException.Message },
                        "Resource Not Found"
                    );
                    break;

                // ---------------- UNAUTHORIZED ----------------
                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;

                    response = ApiResponse<object>.Failure(
                        new List<string> { unauthorizedAccessException.Message },
                        "Unauthorized Access"
                    );
                    break;

                // ---------------- FORBIDDEN ----------------
                case ForbiddenException forbiddenException:
                    statusCode = HttpStatusCode.Forbidden;

                    response = ApiResponse<object>.Failure(
                        new List<string> { forbiddenException.Message },
                        "Forbidden Access"
                    );
                    break;

                // ---------------- UNEXPECTED ERROR ----------------
                default:
                    statusCode = HttpStatusCode.InternalServerError;

                    response = ApiResponse<object>.Failure(
                        new List<string> { exception.Message },
                        "Internal Server Error"
                    );
                    break;
            }

            // Set status code
            context.Response.StatusCode = (int)statusCode;

            // Return JSON response
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}