using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.Login;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.RegisterPatient;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.StaffLogin;
using ZU_DCMS.APPLICATION.Features.Auth.Queries;

namespace ZU_DCMS.API.Endpoints.Auth
{
    /// <summary>
    /// Extension class to register Auth endpoints.
    /// </summary>
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/auth")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Authentication");

            // 1. Patient Register
            group.MapPost("/register", async (ISender sender, RegisterPatientCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Patient registered successfully."));
            })
            .AllowAnonymous()
            .WithName("RegisterPatient")
            .WithSummary("Registers a new patient into the system");

            // 2. Default Login
            group.MapPost("/login", async (ISender sender, LoginCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Login successful."));
            })
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Authenticates a patient or general user and returns a JWT token");

            // 3. Staff Login
            group.MapPost("/staff-login", async (ISender sender, StaffLoginCommand command) =>
            {
                // This command processes Admin, Doctor, Dean, ViceDean, Professor logins
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Staff login successful."));
            })
            .AllowAnonymous()
            .WithName("StaffLogin")
            .WithSummary("Authenticates staff members and returns access & refresh tokens");

            // 4. Forgot Phone Password / Reset Flow
            group.MapPost("/forgot-phone", async (ISender sender, ForgotPhoneQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Phone reset request processed."));
            })
            .AllowAnonymous()
            .WithName("ForgotPhone")
            .WithSummary("Handles forgot password requests via phone number");
        }
    }
}
