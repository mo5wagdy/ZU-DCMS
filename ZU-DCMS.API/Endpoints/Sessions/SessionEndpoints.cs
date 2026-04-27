using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.APPLICATION.DTOs.Session;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetSessionPatients;
using ZU_DCMS.APPLICATION.Features.Sessions.Commands.GenerateSessions;
using ZU_DCMS.APPLICATION.Features.Sessions.Queries.FindSession;
using ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetAvailableSlots;
using ZU_DCMS.APPLICATION.Features.Sessions.Queries.IsSessionAvailable;

namespace ZU_DCMS.API.Endpoints.Sessions
{
    /// <summary>
    /// Extension class to register Session and Clinic scheduling endpoints.
    /// </summary>
    public static class SessionEndpoints
    {
        public static void MapSessionEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/sessions")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Sessions");

            // 1. Check open slots (For Patients or Public booking access)
            group.MapGet("/available-slots", async ([FromServices] ISender sender, [AsParameters] GetAvailableSlotsQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<AvailableSlotDto>>.Success(result.Value, "Available slots retrieved."))
                    : Results.BadRequest(ApiResponse<List<AvailableSlotDto>>.Failure(result.Errors, "Failed to retrieve available slots."));
            })
            .RequireAuthorization("PublicViewPolicy") // Patient/Public roles
            .WithName("GetAvailableSlots")
            .WithSummary("Retrieves open clinic session slots within a timeframe")
            .Produces<ApiResponse<List<AvailableSlotDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<AvailableSlotDto>>>(StatusCodes.Status400BadRequest);

            // 2. Staff lookup session filters
            group.MapGet("/find", async ([FromServices] ISender sender, [AsParameters] FindSessionQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<SessionDto>.Success(result.Value, "Session records retrieved."))
                    : Results.NotFound(ApiResponse<SessionDto>.Failure(result.Errors, "Session not found."));
            })
            .RequireAuthorization("StaffViewPolicy") // Internal doctors, admins, receptionists
            .WithName("FindSession")
            .WithSummary("Searches for specific clinical sessions via complex filters")
            .Produces<ApiResponse<SessionDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SessionDto>>(StatusCodes.Status404NotFound);

            // 3. Logic check for session capacity/flags
            group.MapGet("/check-availability", async ([FromServices] ISender sender, [AsParameters] IsSessionAvailableQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<bool>.Success(result.Value, "Availability evaluated."))
                    : Results.BadRequest(ApiResponse<bool>.Failure(result.Errors, "Availability check failed."));
            })
            .RequireAuthorization("PublicViewPolicy")
            .WithName("IsSessionAvailable")
            .WithSummary("Verifies precisely if a specific session can support more bookings")
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

            // 4. Generator (Admin Only)
            group.MapPost("/generate", async ([FromServices] ISender sender, [FromBody] GenerateSessionsCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<SessionDto>>.Success(result.Value, "Sessions formally generated."))
                    : Results.BadRequest(ApiResponse<List<SessionDto>>.Failure(result.Errors, "Failed to generate sessions."));
            })
            .RequireAuthorization("AdminPolicy") // strictly Admin generator
            .WithName("GenerateSessions")
            .WithSummary("Bulk generates routine clinic sessions in advance for the layout")
            .Produces<ApiResponse<List<SessionDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<SessionDto>>>(StatusCodes.Status400BadRequest);


            // 5. Get Patients for session
            group.MapGet("/patients", async ([FromServices] ISender sender, [FromQuery] int SessionId, [FromQuery] string InternDcotorId) =>
            {
                var result = await sender.Send(new GetSessionPatientsQuery(SessionId, InternDcotorId));
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<PagedResult<BookingForDiagnosisDto>>.Success(result.Value, "Patients retriedved."))
                    : Results.BadRequest(ApiResponse<PagedResult<BookingForDiagnosisDto>>.Failure(result.Errors, "Failed to retrieve patients"));
            })
            .WithName("Get Session Patients")
            .WithSummary("Retrieves patients paginated")
            .Produces<ApiResponse<PagedResult<BookingForDiagnosisDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PagedResult<BookingForDiagnosisDto>>>(StatusCodes.Status400BadRequest);
        }
    }
}
