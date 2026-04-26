using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.Login;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.RegisterPatient;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.StaffLogin;
using ZU_DCMS.APPLICATION.DTOs.Auth;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.ForgotPhone;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.RefreshToken;

namespace ZU_DCMS.API.Endpoints.Auth
{
    /// <summary>
    /// Extension class to register Auth endpoints.
    /// </summary>
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/auth")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Authentication");

            // 1. Patient Register
            group.MapPost("/register", async ([FromServices] ISender sender, [FromBody] RegisterPatientCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<AuthDto>.Success(result.Value, "Patient registered successfully."))
                    : Results.BadRequest(ApiResponse<AuthDto>.Failure(result.Errors, "Registration failed."));
            })
            .AllowAnonymous()
            .WithName("RegisterPatient")
            .WithSummary("Registers a new patient into the system")
            .Produces<ApiResponse<AuthDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AuthDto>>(StatusCodes.Status400BadRequest);

            // 2. Default Login
            group.MapPost("/login", async ([FromServices] ISender sender, [FromBody] LoginCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<AuthDto>.Success(result.Value, "Login successful."))
                    : Results.BadRequest(ApiResponse<AuthDto>.Failure(result.Errors, "Login failed."));
            })
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Authenticates a patient or general user and returns a JWT token")
            .Produces<ApiResponse<AuthDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AuthDto>>(StatusCodes.Status400BadRequest);

            // 3. Staff Login
            group.MapPost("/staff-login", async ([FromServices] ISender sender, [FromBody] StaffLoginCommand command) =>
            {
                // This command processes Admin, Doctor, Dean, ViceDean, Professor logins
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<AuthDto>.Success(result.Value, "Staff login successful."))
                    : Results.BadRequest(ApiResponse<AuthDto>.Failure(result.Errors, "Staff login failed."));
            })
            .AllowAnonymous()
            .WithName("StaffLogin")
            .WithSummary("Authenticates staff members and returns access & refresh tokens")
            .Produces<ApiResponse<AuthDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AuthDto>>(StatusCodes.Status400BadRequest);

            // 4. Forgot Phone Password / Reset Flow
            group.MapGet("/forgot-phone", async ([FromServices] ISender sender, [FromQuery] string nationalId) =>
            {
                var command = new ForgotPhoneCommand(nationalId);
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<ForgotPhoneResponseDto>.Success(result.Value, "Phone reset request processed."))
                    : Results.BadRequest(ApiResponse<ForgotPhoneResponseDto>.Failure(result.Errors, "Phone reset request failed."));
            })
            .AllowAnonymous()
            .WithName("ForgotPhone")
            .WithSummary("Handles forgot phone requests via identity number")
            .Produces<ApiResponse<ForgotPhoneResponseDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ForgotPhoneResponseDto>>(StatusCodes.Status400BadRequest);

            // 5. Refresh Token
            group.MapPost("/refresh", async ([FromServices] ISender sender, [FromBody] RefreshTokenCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<AuthDto>.Success(result.Value, "Token refreshed."))
                    : Results.Unauthorized(); // 401 if refresh failed
            })
            .AllowAnonymous()
            .WithName("RefreshToken")
            .WithSummary("Refreshes the access token using a valid refresh token")
            .Produces<ApiResponse<AuthDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
